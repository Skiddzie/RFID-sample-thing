
using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
namespace MauiRfidSample.MVVM.ViewModels
{
    class ReaderWiFiViewModel : BaseViewModel
    {

        private List<WifiScanData> wifiScanDataList;

        private ObservableCollection<SavedProfiles> _savedProfiles;
        private ObservableCollection<AvilableProfiles> _avilableProfiles;
        private ObservableCollection<ENUM_WIFI_PROTOCOL_TYPE> _protocollist;
        private ObservableCollection<string> _cacertlist;
        private ObservableCollection<string> _clientcertlist;
        private ObservableCollection<string> _privatekeyList;
        private ENUM_WIFI_PROTOCOL_TYPE _protocolSelected;

        private bool _enableUI, _connectedLayout, _popupAddProfiles, _isVisibleEAPRow, _isVisibleCaCertRow, _isVisibleIdentityRow, _isVisibleAnIdentityRow, 
            _isVisibleClientCertRow, _isVisiblePrivateKeyRow, _isVisiblePasswordRow;
        private string _connectedProfile, _addSSID, _passwordEntry, _eAPSelected, _caCertSelected,
            _identityEntry, _clientCertSelected, _privateKeySelected, _anIdentityEntry;
        public ObservableCollection<SavedProfiles> SavedProfilesList { get => _savedProfiles; set { _savedProfiles = value; OnPropertyChanged(); } }
        public ObservableCollection<AvilableProfiles> AvilableProfilesList { get => _avilableProfiles; set { _avilableProfiles = value; OnPropertyChanged(); } }

        public ObservableCollection<ENUM_WIFI_PROTOCOL_TYPE> ProtocolList { get => _protocollist; set { _protocollist = value; OnPropertyChanged(); } }

        public ObservableCollection<string> CaCertList { get => _cacertlist; set { _cacertlist = value; OnPropertyChanged(); } }
        public ObservableCollection<string> ClientCertList { get => _clientcertlist; set { _clientcertlist = value; OnPropertyChanged(); } }
        public ObservableCollection<string> PrivateKeyList { get => _privatekeyList; set { _privatekeyList = value; OnPropertyChanged(); } }

        public bool EnableUI { get => _enableUI; set { _enableUI = value; OnPropertyChanged(); } }
        public bool ConnectedLayout { get => _connectedLayout; set { _connectedLayout = value; OnPropertyChanged(); } }

        public bool PopupAddProfiles { get => _popupAddProfiles; set { _popupAddProfiles = value; OnPropertyChanged(); } }
        public bool IsVisibleEAPRow { get => _isVisibleEAPRow; set { _isVisibleEAPRow = value; OnPropertyChanged(); } }
        public bool IsVisibleCaCertRow { get => _isVisibleCaCertRow; set { _isVisibleCaCertRow = value; OnPropertyChanged(); } }
        public bool IsVisibleIdentityRow { get => _isVisibleIdentityRow; set { _isVisibleIdentityRow = value; OnPropertyChanged(); } }
        public bool IsVisibleClientCertRow { get => _isVisibleClientCertRow; set { _isVisibleClientCertRow = value; OnPropertyChanged(); } }
        public bool IsVisiblePrivateKeyRow { get => _isVisiblePrivateKeyRow; set { _isVisiblePrivateKeyRow = value; OnPropertyChanged(); } }
        public bool IsVisiblePasswordRow { get => _isVisiblePasswordRow; set { _isVisiblePasswordRow = value; OnPropertyChanged(); } }
        public bool IsVisibleAnIdentityRow { get => _isVisibleAnIdentityRow; set { _isVisibleAnIdentityRow = value; OnPropertyChanged(); } }
        public string ConnectedProfile { get => _connectedProfile; set { _connectedProfile = value; OnPropertyChanged(); } }
        public string AddSSID { get => _addSSID; set { _addSSID = value; OnPropertyChanged(); } }
        public ENUM_WIFI_PROTOCOL_TYPE ProtocolSelected { get => _protocolSelected; set { _protocolSelected = value; OnPropertyChanged(); } }
        public string PasswordEntry { get => _passwordEntry; set { _passwordEntry = value; OnPropertyChanged(); } }
        public string EAPSelected { get => _eAPSelected; set { _eAPSelected = value; OnPropertyChanged(); } }
        public string CaCertSelected { get => _caCertSelected; set { _caCertSelected = value; OnPropertyChanged(); } }
        public string IdentityEntry { get => _identityEntry; set { _identityEntry = value; OnPropertyChanged(); } }
        public string ClientCertSelected { get => _clientCertSelected; set { _clientCertSelected = value; OnPropertyChanged(); } }
        public string PrivateKeySelected { get => _privateKeySelected; set { _privateKeySelected = value; OnPropertyChanged(); } }
        public string AnIdentityEntry { get => _anIdentityEntry; set { _anIdentityEntry = value; OnPropertyChanged(); } }

        public ReaderWiFiViewModel()
        {
            _savedProfiles = new ObservableCollection<SavedProfiles>();
            _avilableProfiles = new ObservableCollection<AvilableProfiles>();
            _protocollist = new ObservableCollection<ENUM_WIFI_PROTOCOL_TYPE>();
            _cacertlist = new ObservableCollection<string>();
            _clientcertlist = new ObservableCollection<string>();
            _privatekeyList = new ObservableCollection<string>();
            wifiScanDataList = new List<WifiScanData> { new WifiScanData() };

            //string status = rfidModel.GetWiFiStatus();
            //if (!status.Equals("ENABLED"))
            //{
            //    rfidModel.enableWiFi();
            //}

            RefreshSavedProfiles();
            wifiScanDataList.Clear();
            bool result = rfidModel.ScanWiFi();
            if (result)
            {

            }
            ConnectedLayout = false;
            resetAddPopup();

        }

        internal void SelectedAvilableProfile(ItemTappedEventArgs e)
        {
            var ssid = (e.Item as AvilableProfiles).AvilableProfileSSID;
            //PasswordDialogAsync(ssid);
            PopupAddProfiles = true;
            foreach(WifiScanData data in wifiScanDataList)
            {
                if(data.Getssid() == ssid)
                {
                    AddSSID = ssid;
                    ProtocolList.Clear();
                    foreach (ENUM_WIFI_PROTOCOL_TYPE protocol in data.Protocol)
                    {
                        ProtocolList.Add(protocol);
                    }
                    IList<string> certList = rfidModel.WifiGetCertificates();
                    CaCertList.Clear();
                    ClientCertList.Clear();
                    PrivateKeyList.Clear();
                    foreach (string cert in certList)
                    {
                        CaCertList.Add(cert);
                        ClientCertList.Add(cert);
                        PrivateKeyList.Add(cert);
                    }


                }
            }


        }

        internal void SelectedSavedProfile(ItemTappedEventArgs e)
        {
            var ssid = (e.Item as SavedProfiles).SavedProfileSSID;
            ConnectDeleteDialogAsync(ssid);
        }

        internal void Refresh()
        {
            wifiScanDataList.Clear();
            rfidModel.ScanWiFi();
        }

        private async void DisconnectPopup()
        {
            string action = await Application.Current.MainPage.DisplayActionSheet("Do you want to Disconnect?", "yes", "no");
            if (action != null && action.Equals("yes"))
            {
                bool result = rfidModel.WiFiDisconnect();
                if (result)
                {
                   
                }
            }
            RefreshSavedProfiles();
        }

        private async void ConnectDeleteDialogAsync(string ssid)
        {
            string action = await Application.Current.MainPage.DisplayActionSheet(ssid, "Share Access", "Delete Profile", "Share WiFi Access with connected reader");
            if (action != null && action.Equals("Share Access"))
            {
                bool result = rfidModel.WiFiConnect(ssid);
                if (result)
                {
                    EnableUI = false;
                    // RefreshSavedProfiles();
                }
            }
            else if (action != null && action.Equals("Delete Profile"))
            {
                bool result = rfidModel.DeleteWiFiProfile(ssid);
                if (result)
                {
                    RefreshSavedProfiles();
                }
            }
        }

        private void RefreshSavedProfiles()
        {
            ConnectedLayout = false;
            SavedProfilesList.Clear();
            IList<WifiProfile> wifiProfiles = rfidModel.GetSavedWiFiProfiles();
            if (wifiProfiles != null && wifiProfiles.Count > 0)
            {
                foreach (WifiProfile profile in wifiProfiles)
                {
                    if (profile.Getstate != null && profile.Getstate().Equals(ENUM_WIFI_STATE.StateConnected))
                    {
                        ConnectedProfile = profile.Getssid();
                        ConnectedLayout = true;
                    }
                    else
                    {
                        SavedProfilesList.Add(new SavedProfiles { SavedProfileSSID = profile.Getssid() });
                    }
                }
            }
        }

        public override void WiFiNotificationEvent(string scanStatus)
        {
            switch (scanStatus)
            {
                case "ScanStart":
                    AvilableProfilesList.Clear();
                    EnableUI = false;

                    break;
                case "Connect":
                    RefreshSavedProfiles();
                    EnableUI = true;
                    break;
                case "Disconnect":
                    //RefreshSavedProfiles();
                    break;
                case "ScanStop":
                    RefreshSavedProfiles();
                    EnableUI = true;
                    break;
                case "Operation Failed":
                    RefreshSavedProfiles();
                    EnableUI = true;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void WiFiScanResultsEvent(WifiScanData data)
        {
            AvilableProfilesList.Add(new AvilableProfiles { AvilableProfileSSID = data.Getssid() });
            wifiScanDataList.Add(data);
        }

        internal void ConnectedProfileClicked()
        {
            DisconnectPopup();
        }

        internal void AddProflie()
        {
            if (ProtocolSelected == null)
            {
               resetAddPopup();
                return;
            }

            WifiProfile wifiProfile = new WifiProfile();
            WifiSecureConfig wifiSecureConfig = new WifiSecureConfig();

            wifiProfile.Setssid(AddSSID);
            wifiProfile.Setprotocol(ProtocolSelected);
            wifiProfile.Setpassword(PasswordEntry);

            if (ProtocolSelected.ToString().Equals("No_Encryption")){
                wifiProfile.Setssid(AddSSID);
            }
            else if(ProtocolSelected.ToString().Contains("Personal"))
            {
                wifiProfile.Setssid(AddSSID);
                wifiProfile.Setprotocol((ENUM_WIFI_PROTOCOL_TYPE)ProtocolSelected);
                wifiProfile.Setpassword(PasswordEntry);
            }
            else if (ProtocolSelected.ToString().Contains("Enterprise"))
            {
                wifiProfile.Setssid(AddSSID);
                wifiProfile.Setprotocol((ENUM_WIFI_PROTOCOL_TYPE)ProtocolSelected);
                wifiProfile.Setpassword(PasswordEntry);
                wifiSecureConfig.Seteap(EAPSelected);
                wifiSecureConfig.Setcacert(CaCertSelected);
                wifiSecureConfig.Setidentity(IdentityEntry);

                if (EAPSelected.Equals("TLS"))
                {
                    wifiSecureConfig.Setclientcert(ClientCertSelected);
                    wifiSecureConfig.PrivateKey = PrivateKeySelected;
                    wifiSecureConfig.PrivatePassword = PasswordEntry;
                }
                else if(EAPSelected.Equals("TTLS") || EAPSelected.Equals("PEAP"))
                {
                    wifiSecureConfig.AnonymousIdentity = AnIdentityEntry;
                    wifiSecureConfig.Password = PasswordEntry;
                }

            }




            wifiProfile.Setconfig(wifiSecureConfig);
            bool result = rfidModel.AddWiFiProfile(wifiProfile);
            if (result)
            {
                RefreshSavedProfiles();
            }
            resetAddPopup();
        }

        internal void ClosePopup()
        {
            resetAddPopup();
        }

        private void resetAddPopup()
        {
            PopupAddProfiles = false;
            IsVisibleEAPRow = false;
            IsVisibleCaCertRow = false;
            IsVisibleIdentityRow = false;
            IsVisibleAnIdentityRow = false;
            IsVisibleClientCertRow = false;
            IsVisiblePrivateKeyRow = false;
            IsVisiblePasswordRow = false;
        }

        internal void ProtocolPickerSelectedIndexChanged(Object sender)
        {
            Picker picker = (Picker)sender;

            if (picker.SelectedIndex == -1)
            {
                //resetAddPopup();
            }
            else
            {
                string protocol = picker.Items[picker.SelectedIndex];
                if (protocol.Contains("Personal"))
                {
                    IsVisiblePasswordRow = true;
                }else if (protocol.Contains("Enterprise"))
                {
                    IsVisibleEAPRow = true;
                    IsVisibleCaCertRow = true;
                    IsVisibleIdentityRow = true;
                    IsVisiblePasswordRow = true;
                }
            }

        }

        internal void EAPPickerSelectedIndexChanged(object sender)
        {
            Picker picker = (Picker)sender;

            if (picker.SelectedIndex == -1)
            {
                //resetAddPopup();
            }
            else
            {
                string eap = picker.Items[picker.SelectedIndex];
                if (eap.Contains("TTLS") || eap.Contains("PEAP"))
                {
                    IsVisibleAnIdentityRow= true;
                    IsVisibleClientCertRow = false;
                    IsVisiblePrivateKeyRow = false;
                }
                else if (eap.Contains("TLS"))
                {
                    IsVisibleAnIdentityRow = false;
                    IsVisibleClientCertRow = true;
                    IsVisiblePrivateKeyRow= true;

                }
            }
        }

        public class SavedProfiles
        {
            public string SavedProfileSSID { get; set; }
        }

        public class AvilableProfiles
        {
            public string AvilableProfileSSID { get; set; }
        }

    }


}
