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
using System.Net.Http;
using System.Text.Json;

namespace AscentSolutions.ZebraRfid
{
    public class AscentWebService
    {
        // the Auth Token and Service URL returned by the authentication process
        private string _oauthToken = "";
        private string _serviceUrl = "";

        // create the Http Client used to authenticate
        private HttpClient _httpClient = new HttpClient(new AscentAndroidClientHandler());

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

        //this is a temporary method until i figure out how to migrate login.xaml.cs to use ascentwebservice
        public void SetOAuthToken(string accessToken, string instanceUrl)
        {
            _oauthToken = accessToken;
            _serviceUrl = instanceUrl;
            _Authenticated = true;
        }
        public string ReturnToken()
            { return _oauthToken; }
        public string ReturnUrl()
            { return _serviceUrl; }
        // authenticate a user
        public async Task<bool> AuthenticateWebserver(string code)
        {
            try
            {
                using var httpClient = new HttpClient();

                var requestBody = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", "3MVG9FINO1nsxRuCKdhiAIOm6bjbYgBzJOWu9V7zNWfXv.W7NNd6a5zOXrIN3gVQxpS48QA0Qo6zbweC4T8lH"),
                new KeyValuePair<string, string>("client_secret", "A72411D38F432952D201A224FB7C57C2BA7BE516278D87EB4FD7DC05F5C1AF65"),
                new KeyValuePair<string, string>("redirect_uri", "myapp://oauth/callback")
            });

                Trace.WriteLine("Exchanging authorization code for access token...");

                var response = await httpClient.PostAsync("https://login.salesforce.com/services/oauth2/token", requestBody);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    Trace.WriteLine($"Token Response: {responseContent}");

                    var tokenData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(responseContent);
                    string accessToken = tokenData.GetProperty("access_token").GetString();
                    string instanceUrl = tokenData.GetProperty("instance_url").GetString();

                    Trace.WriteLine($"Access Token: {accessToken}");
                    Trace.WriteLine($"Instance URL: {instanceUrl}");

                    await SecureStorage.SetAsync("AccessToken", accessToken);
                    await SecureStorage.SetAsync("InstanceUrl", instanceUrl);

                    SetOAuthToken(accessToken, instanceUrl);

                    Trace.WriteLine("secure storage done");

                    string accessTokenString = await SecureStorage.GetAsync("AccessToken");
                    Trace.WriteLine(accessTokenString);
                    _Authenticated = true;
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Shell.Current.GoToAsync(nameof(MainPage));
                    });
                    
                }
                else
                {
                    Trace.WriteLine($"Failed to get access token: {responseContent}");
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Error during token exchange: {ex.Message}");
                _Authenticated = false;
            }
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
            //_httpClient = new HttpClient(new AscentAndroidClientHandler());
            // if we are authenticated, ok to proceed
            if (_Authenticated)
            {
                // create the query URL
                string restQuery = _serviceUrl + strURL;

                // create the POST request
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, restQuery);
                Trace.WriteLine("http request");
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