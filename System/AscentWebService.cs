using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Maui.Networking;

namespace MauiRfidSample
{
    public class AscentWebService
    {
        // the Auth Token and Service URL returned by the authentication process
        private string _oauthToken = "";
        private string _serviceUrl = "";

        // create the Http Client used to authenticate
        private HttpClient _httpClient = null;

        // are we authenticated?
        private bool _Authenticated = false;

        // The time last authenticated
        // and the cached credentials
        private DateTime _LastAuthenticated = DateTime.MinValue;
        private string _LastUsername = "";
        private bool _LastMode = false;

        // the error object returned from API calls
        public class ErrorObject
        {
            [JsonProperty("errorMsg")]
            public string ErrorMsg { get; set; }

            [JsonProperty("error")]
            public string Error { get; set; }
        }

        // authenticate a user
        public async Task<bool> Authenticate(string strUsername, string strPassword, bool bTestMode)
        {
            try
            {
                // assume not authenticated
                _Authenticated = false;

                // check for cached authentication
                TimeSpan ts = DateTime.Now - _LastAuthenticated;
                if ((_LastUsername == strUsername) && (_LastMode == bTestMode) && (ts.TotalMinutes < 15))
                {
                    // ok to use the cached credentials
                    RFIDCommission.WriteToCommissionLog("Using Cached Credentials to call Webservice");
                    _Authenticated = true;
                }
                else
                {
                    // set OAuth key and secret variables
                    // Test parameters
                    string sfdcURLTest = "https://test.salesforce.com/services/oauth2/token";
                    string sfdcConsumerKeyTest = "3MVG98EE59.VIHmwphpbDAEk38AnqFl1sXHLwkRnnhYZnV._NL85V4v1tMDtXxYnwTC3CKJ1uAVBPvHwDgdFX";
                    string sfdcConsumerSecretTest = "5110DB370BBF351D0B4911988A787F4B9BA8D838D851AE5A288CF6805106CF67";

                    // production parameters
                    string sfdcURLLive = "https://login.salesforce.com/services/oauth2/token";
                    string sfdcConsumerKeyLive = "3MVG9szVa2RxsqBb_CerPQPGN3RBxmfoiWHtsTf1lMIHFGCZXjznSmU1DXPsHW_tiVe6M6pS1KypLJvJWTjau";
                    string sfdcConsumerSecretLive = "3982959255080993874";

                    // production parameters
                    //string sfdcURLLive = "https://cah01--ConsPOC.cs11.my.salesforce.com/services/oauth2/token";
                    //string sfdcConsumerKeyLive = "3MVG9GiqKapCZBwFfhPpgaILvLYltH_EpFg4e4tk3b4BBb7XCidzQGWzvCuuS.OrGo0BODYWGU.zcPy44dG6J";
                    //string sfdcConsumerSecretLive = "7617848440587041666";

                    // create content object to post as form content
                    HttpContent content = new FormUrlEncodedContent(new System.Collections.Generic.Dictionary<string, string>
                    {
                        {"grant_type","password"},
                        {"client_id",bTestMode ? sfdcConsumerKeyTest : sfdcConsumerKeyLive},
                        {"client_secret",bTestMode ? sfdcConsumerSecretTest : sfdcConsumerSecretLive},
                        {"username",strUsername},
                        {"password",strPassword}
                    });

                    RFIDCommission.WriteToCommissionLog(string.Format("client_id:{0}, client_secret:{1},username:{2}, password:{3}", bTestMode ? sfdcConsumerKeyTest : sfdcConsumerKeyLive, bTestMode ? sfdcConsumerSecretTest : sfdcConsumerSecretLive, strUsername, strUsername));
                    // the URL to use
                    string strURL = bTestMode ? sfdcURLTest : sfdcURLLive;

                    // SalesForce requires TLS 1.2 now
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                    // wait for connectivity, or timeout
                    bool bConnected = await WaitForConnectivity();
                    RFIDCommission.WriteToCommissionLog("WaitForConnectivity() : " + bConnected.ToString());

                    // Post the authentication request                   
                    _httpClient = new HttpClient(new AscentAndroidClientHandler());
                    HttpResponseMessage message = await _httpClient.PostAsync(strURL, content);

                    // wait for the response
                    string responseString = await message.Content.ReadAsStringAsync();
                    RFIDCommission.WriteToCommissionLog(responseString);

                    // parse the results
                    JObject obj = JObject.Parse(responseString);
                    _oauthToken = (string)obj["access_token"];
                    _serviceUrl = (string)obj["instance_url"];

                    // if we got a token and url, we are authenticated
                    if ((_oauthToken != null) && (_serviceUrl != null))
                    {
                        // authenticated!
                        _Authenticated = true;

                        // save the cached values
                        _LastAuthenticated = DateTime.Now;
                        _LastMode = bTestMode;
                        _LastUsername = strUsername;
                    }
                }
            }
            catch (Exception ex)
            {
                // log it
                RFIDCommission.WriteToCommissionLog("ERR: " + ex.Message);
                Trace.WriteLine(ex);
            }

            // if not authenticated, make sure the cache is cleared
            if (!_Authenticated) ClearAuthenticationCache();

            // authenticated
            RFIDCommission.WriteToCommissionLog("_Authenticated: " + _Authenticated);
            return _Authenticated;
        }

        // wait for connectivity
        public async Task<bool> WaitForConnectivity()
        {
            bool bConnected = false;

            await Task.Run(() =>
            {
                DateTime dtStart = DateTime.Now;

                do
                {
                    // save into the database
                    bool bConnected = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                    // if not connected, delay and try again 
                    if (!bConnected)
                    {
                        // check the timeout
                        TimeSpan ts = DateTime.Now - dtStart;
                        if (ts.TotalSeconds > 10)
                        {
                            // timed out
                            break;
                        }

                        // short delay
                        Thread.Sleep(500);
                    }
                } while (!bConnected);
            });

            // done
            return bConnected;
        }

        // clear the cached authentication
        public void ClearAuthenticationCache()
        {
            // clear everything
            _LastAuthenticated = DateTime.MinValue;
            _LastUsername = "";
            _LastMode = false;
        }

        public async Task<T> QueryURL<T>(string strURL)
        {
            T oResult = default(T);

            // if we are authenticated, ok to proceed
            if (_Authenticated)
            {
                // create the query URL
                string restQuery = _serviceUrl + strURL;

                // create the request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, restQuery);

                //add token to header
                request.Headers.Add("Authorization", "Bearer " + _oauthToken);

                //return JSON to the caller
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //call endpoint async
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // get the result as JSON
                string result = await response.Content.ReadAsStringAsync();

                // unescape and remove leading/trailing double quotes
                string intermediate = GlobalHelpers.UnescapeString(result);
                string conv = GlobalHelpers.RemoveLeadingTrailingDoubleQuote(intermediate);

                // check the response state
                // 200 or 201 are OK, eveything else is a failure
                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    // attempt to deserialize the object
                    oResult = JsonConvert.DeserializeObject<T>(conv);
                }
                else
                {
                    // if the deserialize failed, try to deserialize as the error object
                    ErrorObject err = JsonConvert.DeserializeObject<ErrorObject>(conv);
                    throw new Exception(err.ErrorMsg);
                }
            }

            // done
            return oResult;
        }

        // post an object to the URL and get back a result
        public async Task<T> PostURL<T>(string strURL, object oObjectToPost)
        {
            T oResult = default(T);

            // if we are authenticated, ok to proceed
            if (_Authenticated)
            {
                // create the query URL
                string restQuery = _serviceUrl + strURL;

                // create the POST request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, restQuery);
                string strJSON = JsonConvert.SerializeObject(oObjectToPost,
                            Newtonsoft.Json.Formatting.None,
                            new JsonSerializerSettings
                            {
                                NullValueHandling = NullValueHandling.Ignore
                            });

                request.Content = new StringContent(strJSON, Encoding.UTF8, "application/json");

                // add token to header
                request.Headers.Add("Authorization", "Bearer " + _oauthToken);

                // return JSON to the caller
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // call endpoint async
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // get the result as JSON
                string result = await response.Content.ReadAsStringAsync();

                // unescape and remove leading/trailing double quotes
                string intermediate = GlobalHelpers.UnescapeString(result);
                string conv = GlobalHelpers.RemoveLeadingTrailingDoubleQuote(intermediate);

                // check the response state
                // 200 or 201 are OK, eveything else is a failure
                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    // attempt to deserialize the object
                    try
                    {
                        oResult = JsonConvert.DeserializeObject<T>(conv);
                    }
                    catch
                    {
                        oResult = default(T);
                    }
                }
                else
                {
                    // if the deserialize failed, try to deserialize as the error object
                    ErrorObject err = JsonConvert.DeserializeObject<ErrorObject>(conv);
                    throw new Exception(err.ErrorMsg);
                }
            }

            // done
            return oResult;
        }

        // put an object to the URL and get back a result
        public async Task<T> PutURL<T>(string strURL, object oObjectToPost)
        {
            T oResult = default(T);

            // if we are authenticated, ok to proceed
            if (_Authenticated)
            {
                // create the query URL
                string restQuery = _serviceUrl + strURL;

                // create the POST request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, restQuery);
                string strJSON = JsonConvert.SerializeObject(oObjectToPost);
                request.Content = new StringContent(strJSON, Encoding.UTF8, "application/json");

                // add token to header
                request.Headers.Add("Authorization", "Bearer " + _oauthToken);

                // return JSON to the caller
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // call endpoint async
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // get the result as JSON
                string result = await response.Content.ReadAsStringAsync();

                // unescape and remove leading/trailing double quotes
                string intermediate = GlobalHelpers.UnescapeString(result);
                string conv = GlobalHelpers.RemoveLeadingTrailingDoubleQuote(intermediate);

                // check the response state
                // 200 or 201 are OK, eveything else is a failure
                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    // attempt to deserialize the object
                    oResult = JsonConvert.DeserializeObject<T>(conv);
                }
                else
                {
                    // if the deserialize failed, try to deserialize as the error object
                    ErrorObject err = JsonConvert.DeserializeObject<ErrorObject>(conv);
                    throw new Exception(err.ErrorMsg);
                }
            }

            // done
            return oResult;
        }

        // delete an object to the URL and get back a result
        public async Task<T> DeleteURL<T>(string strURL)
        {
            T oResult = default(T);

            // if we are authenticated, ok to proceed
            if (_Authenticated)
            {
                // create the query URL
                string restQuery = _serviceUrl + strURL;

                // create the DELETE request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, restQuery);

                //add token to header
                request.Headers.Add("Authorization", "Bearer " + _oauthToken);

                //return JSON to the caller
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //call endpoint async
                HttpResponseMessage response = await _httpClient.SendAsync(request);

                // get the result as JSON
                string result = await response.Content.ReadAsStringAsync();

                // unescape and remove leading/trailing double quotes
                string intermediate = GlobalHelpers.UnescapeString(result);
                string conv = GlobalHelpers.RemoveLeadingTrailingDoubleQuote(intermediate);

                // check the response state
                // 200 or 201 are OK, eveything else is a failure
                if ((response.StatusCode == HttpStatusCode.OK) || (response.StatusCode == HttpStatusCode.Created))
                {
                    // attempt to deserialize the object
                    oResult = JsonConvert.DeserializeObject<T>(conv);
                }
                else
                {
                    // if the deserialize failed, try to deserialize as the error object
                    ErrorObject err = JsonConvert.DeserializeObject<ErrorObject>(conv);
                    throw new Exception(err.ErrorMsg);
                }
            }

            // done
            return oResult;
        }
    }
}