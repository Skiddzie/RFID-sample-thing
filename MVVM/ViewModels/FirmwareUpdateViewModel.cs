using Android.Widget;
using ICSharpCode.SharpZipLib.Core;
using MauiRfidSample.MVVM.Models;

namespace MauiRfidSample.MVVM.ViewModels
{
    class FirmwareUpdateViewModel : BaseViewModel
    { 
     
        private ScannerModel scannerModel;
        private string _connectedDevice, _currentProgress, _currentPercentage, _scannerFWVersion, _mySelectedFile, _firmwareButton;

        public FirmwareUpdateViewModel()
        {
            scannerModel = ScannerModel.scannerModel;
            UpdateUI();
        }

        internal async void SelectUpdateFirmware()
        {
            if (MySelectedFile == null)
            {
                FileResult fileResult = await PickFirmwareFile();

                if (fileResult != null)
                {
                    byte[] buffer = new byte[4096];
                    var fileName = fileResult.FileName;
                    var stream = await fileResult.OpenReadAsync();
                    var cacheFilePath = Path.Combine(FileSystem.AppDataDirectory, "ZebraFirmware/", fileName);

                    if (File.Exists(cacheFilePath))
                    {
                        File.Delete(cacheFilePath);
                    }
                    string directoryName = Path.GetDirectoryName(cacheFilePath);

                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    using (FileStream streamWriter = File.Create(cacheFilePath))
                    {
                        StreamUtils.Copy(stream, streamWriter, buffer);
                        if (File.Exists(cacheFilePath))
                        {
                            directoryName = Path.GetDirectoryName(cacheFilePath);
                            MySelectedFile = cacheFilePath;
                            FirmwareButton = "Update Firmware";
                        }
                    }
                }
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _ = scannerModel.UpdateScanner(MySelectedFile);
                });
            }
        }

        async Task<FileResult> PickFirmwareFile()
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select firmware file"
                });
                return result;
            }
            catch (Exception ex)
            {

            }
            return null;
        }

        public string MySelectedFile { get => _mySelectedFile; set { _mySelectedFile = value; OnPropertyChanged(); } }
        public string ConnectedDevice { get => _connectedDevice; set { _connectedDevice = value; OnPropertyChanged(); } }
        public string CurrentProgress { get => _currentProgress; set { _currentProgress = value; OnPropertyChanged(); } }
        public string CurrentPercentage { get => _currentPercentage; set { _currentPercentage = value; OnPropertyChanged(); } }
        public string ScannerFWVersion { get => _scannerFWVersion; set { _scannerFWVersion = value; OnPropertyChanged(); } }
        public string FirmwareButton { get => _firmwareButton; set { _firmwareButton = value; OnPropertyChanged(); } }

        public override void ScannerConnectionEvent(string deviceName)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UpdateUI();
                if (deviceName.Equals(""))
                {
                    Toast.MakeText(Android.App.Application.Context, "Disconnected..!", ToastLength.Long).Show();
                    ScannerFWVersion = "";
                }
                else
                {
                    Toast.MakeText(Android.App.Application.Context, "Connected..!", ToastLength.Long).Show();
                }
            });
        }

        private void UpdateUI()
        {
            ConnectedDevice = scannerConnected ? devicName : "Not Connected";
            ScannerFWVersion = sFWVersion;
            FirmwareButton = "Select Firmware";
            MySelectedFile = null;
        }

        public override void CurrentProgressUpdate(int currentProgress)
        {
            if (currentProgress == 100)
            {
                CurrentPercentage = currentProgress + "%";
            }
            else
            {
                CurrentProgress = currentProgress < 10 ? "0.0" + currentProgress : "0." + currentProgress;
                CurrentPercentage = currentProgress + "%";
            }

        }

        public override void FWVersion(string scannerFWVersion)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                ScannerFWVersion = scannerFWVersion;
            });
        }

    }
}
