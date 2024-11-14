using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.Content.PM;
using Android.Hardware.Usb;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using AndroidX.Core.Content;
using Com.Zebra.Rfid.Api3;

namespace MauiRfidSample
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {

        const string FIRMWARE_FOLDER = "/ZebraFirmware";
        const string OUTPUT_FOLDER = "/ZebraOutput";

        const int BLUETOOTH_PERMISSION_REQUEST_CODE = 1001;
        const int BLUETOOTH_ENABLE_REQUEST_CODE = 1002;
        private Action<bool> _onRequestPermissionsResult;
        private Action<bool> _onRequestBTEnable;


        /// <summary>
        /// Create directory for store the firmware plugin(Plugin should be ".SCNPLG")
        /// </summary>
        private void CreateDirectory()
        {
            CheckFileReadWritePermissions();

        }

        /// <summary>
        /// Check application permissions
        /// </summary>
        private void CheckFileReadWritePermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
            {
                return;
            }
            else
            {
                if (PackageManager.CheckPermission(Manifest.Permission.ReadExternalStorage, PackageName) != Permission.Granted
                    && PackageManager.CheckPermission(Manifest.Permission.WriteExternalStorage, PackageName) != Permission.Granted)
                {
                    var permissions = new string[] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
                    RequestPermissions(permissions, 2226);

                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            // Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (requestCode == 2226)
            {
                try
                {

                    var firmwareDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + FIRMWARE_FOLDER;
                    var outputDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads) + OUTPUT_FOLDER;
                    Directory.CreateDirectory(firmwareDirectory);
                    Directory.CreateDirectory(outputDirectory);

                    if (Directory.Exists(firmwareDirectory))
                    {
                        Console.WriteLine("That path firmwareDirectory  exists already.");

                    }

                }
                catch (Java.Lang.Exception e)
                {
                    Console.WriteLine("Sample app Exception " + e.Message);
                }
            }
            if (requestCode == BLUETOOTH_PERMISSION_REQUEST_CODE)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    if (_onRequestPermissionsResult != null)
                    {
                        _onRequestPermissionsResult(true);
                        _onRequestPermissionsResult = null;
                    }
                }
                else
                {
                    if (_onRequestPermissionsResult != null)
                    {
                        _onRequestPermissionsResult(false);
                        _onRequestPermissionsResult = null;
                    }
                }
            }
        }

        internal void CheckPermissions(Action<bool> onRequestPermissionsResult)
        {
            _onRequestPermissionsResult = onRequestPermissionsResult;
            RequestUSBPermission();
        }

        internal void CheckBTPermission()
        {
           

            if (Build.VERSION.SdkInt <= BuildVersionCodes.R)
            {
                if (_onRequestPermissionsResult != null)
                {
                    _onRequestPermissionsResult(true);
                    _onRequestPermissionsResult = null;
                }
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothConnect) == (int)Permission.Granted)
                {
                    if (_onRequestPermissionsResult != null)
                    {
                        _onRequestPermissionsResult(true);
                        _onRequestPermissionsResult = null;
                    }
                }
                else
                {
                    var permissions = new string[] { Manifest.Permission.BluetoothConnect, Manifest.Permission.BluetoothScan };
                    RequestPermissions(permissions, BLUETOOTH_PERMISSION_REQUEST_CODE);
                }
            }
        }

        internal void CheckBTEnable(Action<bool> onRequestBTEnable)
        {
            _onRequestBTEnable = onRequestBTEnable;
            Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
            StartActivityForResult(enableBtIntent, BLUETOOTH_ENABLE_REQUEST_CODE);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == BLUETOOTH_ENABLE_REQUEST_CODE)
            {
                if (resultCode == Result.Ok)
                {
                    if (_onRequestBTEnable != null)
                    {
                        _onRequestBTEnable(true);
                        _onRequestBTEnable = null;
                    }
                }
                else
                {
                    if (_onRequestBTEnable != null)
                    {
                        _onRequestBTEnable(false);
                        _onRequestBTEnable = null;
                    }
                }
            }
        }

        internal void RequestUSBPermission()
        {
            int vendorId = 0x05E0;
            int productId = 0x1701;
            UsbReceiver usbReceiver = new UsbReceiver();
            String ACTION_USB_PERMISSION = "com.zebra.rfid.app.USB_PERMISSION";
            UsbManager usbManager = (UsbManager)Android.App.Application.Context.GetSystemService(UsbService);
            if (usbManager != null)
            {
                if (usbManager.DeviceList.Count > 0)
                {
                    foreach (UsbDevice device in usbManager.DeviceList.Values)
                    {
                        if ((device.VendorId == vendorId) && (device.ProductId == productId))
                        {
                            if (!usbManager.HasPermission(device))
                            {
                                PendingIntent mPermissionIntent;
                                if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                                {
                                   mPermissionIntent = PendingIntent.GetBroadcast(this, 0, new Intent(ACTION_USB_PERMISSION), PendingIntentFlags.Mutable);
                                }
                                else
                                {
                                   mPermissionIntent = PendingIntent.GetBroadcast(this, 0, new Intent(ACTION_USB_PERMISSION), PendingIntentFlags.UpdateCurrent);
                                }
                                IntentFilter filter = new IntentFilter(ACTION_USB_PERMISSION);
                                Android.App.Application.Context.RegisterReceiver(usbReceiver, filter);
                                usbManager.RequestPermission(device, mPermissionIntent);
                            }
                            else
                            {
                                CheckBTPermission();
                            }

                        }

                    }
                }
                else
                {
                    CheckBTPermission();
                }
            }
            else
            {
                CheckBTPermission();
            }
        }

        [BroadcastReceiver(Name = "com.zebra.rfid.app.USB_PERMISSION")]
        public class UsbReceiver : BroadcastReceiver
        {
            public override void OnReceive(Context context, Intent intent)
            {
                MainActivity mainActivity = Platform.CurrentActivity as MainActivity;
                mainActivity.CheckBTPermission();
            }
        }


    }
}
