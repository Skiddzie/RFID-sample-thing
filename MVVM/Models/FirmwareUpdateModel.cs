using Com.Zebra.Scannercontrol;
using Java.Lang;
using System.Xml;
using Exception = Java.Lang.Exception;

namespace MauiRfidSample.MVVM.Mode
{
    class FirmwareUpdateModel: Java.Lang.Object, IDcsSdkApiDelegate
    {
        private static FirmwareUpdateModel _fwUpdateModel;
        private int scannerId;

        //string inXml;
        private const string FIRMWARE_FOLDER_PATH = "ZebraFirmware/";
        private const string OUTPUT_FOLDER_PATH = "ZebraOutput/";
        private string filePathOfFirmware = "";
        private string outputFolderPath = "";
        private string[] filesInFirmware;
        private static SDKHandler sdkHandler;
        public static List<DCSScannerInfo> scannerList = new List<DCSScannerInfo>();

        internal delegate void ConnectionHandler(string deviceName);
        internal delegate void CurrentProgressUpdate(int currentProgress);
        internal delegate void AvilableScannerList(List<DCSScannerInfo> scannerList);
        internal delegate void FirmwareFilesInFolder(string fileList);
        internal delegate void ScannerFWVersion(string scannerFWVersion);

        internal event ConnectionHandler ScannerConnectionEvent;
        internal event CurrentProgressUpdate CurrentProgress;
        internal event AvilableScannerList ScannerList;
        internal event FirmwareFilesInFolder FileListFromFolder;
        internal event ScannerFWVersion FWVersion;

        public static FirmwareUpdateModel fwUpdateModel
        {
            get
            {
                if (_fwUpdateModel == null)
                    _fwUpdateModel = new FirmwareUpdateModel();
                return _fwUpdateModel;
            }
        }

        private FirmwareUpdateModel()
        {
            
        }

        /// <summary>
        /// Setup the SDK handler
        /// </summary>
        public void setupSDKHandler()
        {
            sdkHandler = new SDKHandler(Android.App.Application.Context);
            //For cdc device
            DCSSDKDefs.DCSSDK_RESULT result = sdkHandler.DcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DcssdkOpmodeUsbCdc);

            //For bluetooth device
            DCSSDKDefs.DCSSDK_RESULT btResult = sdkHandler.DcssdkSetOperationalMode(DCSSDKDefs.DCSSDK_MODE.DcssdkOpmodeBtNormal);
          
            sdkHandler.DcssdkSetDelegate(this);

            int notifications_mask = 0;
            // We would like to subscribe to all scanner available/not-available events
            notifications_mask |= DCSSDKDefs.DCSSDK_EVENT.DcssdkEventScannerAppearance.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventScannerDisappearance.Value;


            // We would like to subscribe to all scanner connection events
            notifications_mask |= DCSSDKDefs.DCSSDK_EVENT.DcssdkEventBarcode.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventBarcode.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventSessionEstablishment.Value | DCSSDKDefs.DCSSDK_EVENT.DcssdkEventSessionTermination.Value;


            // We would like to subscribe to all barcode events
            // subscribe to events set in notification mask
            sdkHandler.DcssdkSubsribeForEvents(notifications_mask);
            if (sdkHandler != null)
            {
                IList<DCSScannerInfo> availableScanners = sdkHandler.DcssdkGetAvailableScannersList();

                scannerList.Clear();
                if (availableScanners != null)
                {
                    foreach (DCSScannerInfo scanner in availableScanners)
                    {
                    
                        scannerList.Add(scanner);
                    }
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    ScannerList?.Invoke(scannerList);
                    if (GetFilePathOfFirmwareFile())
                    {
                        //if use dat file no needed to extract..
                        // ExtractTheFirmwareFile();

                    }
                });
            }
        }

        public async Task getScannerFirmwareVersion(int scannerID)
        {
            string inXML = "<inArgs><scannerID>" + scannerID + "</scannerID><cmdArgs><arg-xml><attrib_list>20012</attrib_list></arg-xml></cmdArgs></inArgs>";
            StringBuilder outXML = new StringBuilder();
            Task<bool> result = Task.Run(() => executeCommand(scannerID, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkRsmAttrGet, outXML, inXML));
            if (await result)
            {
                FWVersion?.Invoke(getSingleStringValue(outXML));
            }
        }

       

        private string getSingleStringValue(StringBuilder outXML)
        {
            try
            {
                XmlReader reader = XmlReader.Create(new StringReader((string)outXML));
                reader.ReadToFollowing("value");
                return reader.ReadElementContentAsString().Trim();
            }
            catch(XmlException e)
            {
                System.Console.WriteLine(e);
            }
            return "";
        }

        /// <summary>
        /// Connect to the scanner
        /// </summary>
        public void ConnectSanner(int scannerID)
        {
            try
            {
                sdkHandler.DcssdkEstablishCommunicationSession(scannerID);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
            }
        }

        /// <summary>
        /// Get the file path of firmware file in internal storage
        /// </summary>
        private bool GetFilePathOfFirmwareFile()
        {

            var directoryPathofFirmwarePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString(), FIRMWARE_FOLDER_PATH);

            outputFolderPath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).ToString(), OUTPUT_FOLDER_PATH);

            filesInFirmware = Directory.GetFiles(directoryPathofFirmwarePath);

            if (filesInFirmware.Length > 0)
            {
                foreach (string path in filesInFirmware)
                {              
                    FileListFromFolder?.Invoke(Path.GetFileName(path));
                }
                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Update the firmware
        /// </summary>
        public async Task UpdateScanner(string selectedFile)
        {
            try
            {
                foreach (string filepath in filesInFirmware)
                {
                    if (filepath.Contains(selectedFile))
                    {
                        filePathOfFirmware = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), FIRMWARE_FOLDER_PATH, filepath);
                        string inXml = "<inArgs><scannerID>" + scannerId + "</scannerID><cmdArgs><arg-string>" + filePathOfFirmware + "</arg-string></cmdArgs></inArgs>";

                        Task<bool> result = Task.Run(() => executeCommand(scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkUpdateFirmware, null, inXml));
                        if (await result)
                        {

                        }
                    }
                }

            }
            catch (Java.Lang.Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Scanner Error : " + e.ToString());
            }
        }

        /// <summary>
        /// Notification to inform that new Aux scanner has been appeared
        /// </summary>
        /// <param name="newTopology">Device tree that change has occurred</param>
        /// <param name="scanerInformation"Scanner Information></param>
        public void DcssdkEventAuxScannerAppeared(DCSScannerInfo newTopology, DCSScannerInfo scanerInformation)
        {
            System.Diagnostics.Debug.WriteLine("Scanner Aux data : " + scanerInformation.ScannerName);
        }

        /// <summary>
        /// The event responsible for capturing the barcode data.
        /// </summary>
        /// <param name="barcodeData">Barcode data</param>
        /// <param name="barcodeType">Barcode type of the scanned barcode. Values of bar code data types</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK</param>
        public void DcssdkEventBarcode(byte[] barcodeData, int barcodeType, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner Barcode data : " + barcodeData);
        }

        /// <summary>
        /// The event responsible for capturing the binary data.
        /// </summary>
        /// <param name="barcodeData">Object representing raw data of the received Intelligent Document Capture(IDC) data.</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK</param>
        public void DcssdkEventBinaryData(byte[] barcodeData, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner Barcode data : " + barcodeData);
        }

        /// <summary>
        /// The event responsible for capturing the scanner connection
        /// </summary>
        /// <param name="scannerInfo">Object representing an appeared active scanner.</param>
        public void DcssdkEventCommunicationSessionEstablished(DCSScannerInfo scannerInfo)
        {
            scannerId = scannerInfo.ScannerID;
            ScannerConnectionEvent?.Invoke(scannerInfo.ScannerName);
        }

        /// <summary>
        /// "Session Terminated" notification informs about disappearance of a particular active scanner
        /// </summary>
        /// <param name="connectedScanner">Unique identifier of a disappeared active scanner assigned by SDK</param>
        public void DcssdkEventCommunicationSessionTerminated(int connectedScanner)
        {
            scannerId = connectedScanner;
            ScannerConnectionEvent?.Invoke("");
        }

        /// <summary>
        /// The event responsible for capturing the firmware update data.
        /// </summary>
        /// <param name="firmwareUpdateEvent">Firmware update information</param>
        public void DcssdkEventFirmwareUpdate(FirmwareUpdateEvent firmwareUpdateEvent)
        {

            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfDlStart)
            {
                System.Diagnostics.Debug.WriteLine("ScannerControl Update Firmware Session Started! ");
            }

            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfDlProgress)
            {
                if (firmwareUpdateEvent.CurrentRecord % 100 == 0)
                {
                    CurrentProgress?.Invoke(firmwareUpdateEvent.CurrentRecord * 100 / firmwareUpdateEvent.MaxRecords);
                }
            }
            if (firmwareUpdateEvent.EventType == DCSSDKDefs.DCSSDK_FU_EVENT_TYPE.ScannerUfSessEnd)
            {
                CurrentProgress?.Invoke(100);
                try
                {
                    Java.Lang.Thread.Sleep(1000);
                }
                catch (InterruptedException e)
                {
                    e.PrintStackTrace();
                }
                Device.BeginInvokeOnMainThread(() =>
                {
                    _ = startNewFirmware();
                });
            }
        }

        /// <summary>
        /// Start the new firmware
        /// </summary>
        private async Task startNewFirmware()
        {

            string inXml = "<inArgs><scannerID>" + scannerId + "</scannerID></inArgs>";
            StringBuilder outXml = new StringBuilder();

            Task<bool> result = Task.Run(() => executeCommand(scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE.DcssdkStartNewFirmware, null, inXml));
            if (await result)
            {
              
            }
        }

        /// <summary>
        /// The event responsible for capturing the Image data.
        /// </summary>
        /// <param name="imageData">Object representing raw data of the received image.</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK.</param>
        public void DcssdkEventImage(byte[] imageData, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner event image data  : " + imageData);
        }

        /// <summary>
        /// Device Arrival" notification informs about appearance of a particular available scanner
        /// </summary>
        /// <param name="scannerInfo">Object representing an appeared available scanner.</param>
        public void DcssdkEventScannerAppeared(DCSScannerInfo scannerInfo)
        {
            System.Diagnostics.Debug.WriteLine("Scanner appeared : " + scannerInfo.ScannerName);
        }

        /// <summary>
        /// The event responsible for capturing the scanner disappearing.
        /// </summary>
        /// <param name="scannerId">Unique identifier of a disappeared available scanner assigned by SDK.</param>
        public void DcssdkEventScannerDisappeared(int scannerId)
        {
            ScannerConnectionEvent?.Invoke("");
        }

        /// <summary>
        ///  The event responsible for handling the video data.
        /// </summary>
        /// <param name="videoFrame">Object representing raw data of the received video frame</param>
        /// <param name="scannerId">Unique identifier of a particular active scanner assigned by SDK.</param>
        public void DcssdkEventVideo(byte[] videoFrame, int scannerId)
        {
            System.Diagnostics.Debug.WriteLine("Scanner video event : " + videoFrame);
        }

        private async Task<bool> executeCommand(int scannerId, DCSSDKDefs.DCSSDK_COMMAND_OPCODE opCode, StringBuilder outXml, string inXml)
        {
            if (sdkHandler != null)
            {
                if (outXml == null)
                {
                    outXml = new StringBuilder();
                }
                Task<DCSSDKDefs.DCSSDK_RESULT> result = Task.Run(() => sdkHandler.DcssdkExecuteCommandOpCodeInXMLForScanner(opCode, inXml, outXml, scannerId));
                if (await result == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultSuccess)
                    return true;
                else if (await result == DCSSDKDefs.DCSSDK_RESULT.DcssdkResultFailure)
                    return false;
            }
            return false;
        }
    }
}
