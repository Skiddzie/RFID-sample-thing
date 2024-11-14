using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using System.Reflection;



namespace MauiRfidSample.MVVM.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	
	public partial class About : ContentPage
	{
		private static ReaderModel rfidModel = ReaderModel.readerModel;
		public About()
		{
			InitializeComponent();
			BindingContext = this;
			Title = "About";
		}

		public string version
		{
			get
			{
				//Assembly executingAssembly = Assembly.GetExecutingAssembly();
				return AppInfo.Current.VersionString;
			}
		}

		public string sdkversion
		{
			get
			{
				try
				{
					if (ReaderModel.readerModel.isConnected == true)
						return rfidModel.rfidReader.VersionInfo().Version;
					else
						return "";
				}
				catch (InvalidUsageException ex)
				{
				}
				return "";
			}
		}

		public string reader
		{
			get
			{
				try
				{
					if( ReaderModel.readerModel.isConnected == true){
						var deviceVersionInfo = new Android.Runtime.JavaDictionary<string, string>();
						rfidModel.rfidReader.Config.GetDeviceVersionInfo(deviceVersionInfo);
						if (deviceVersionInfo.ContainsKey("GENX_DEVICE"))
						{
							return deviceVersionInfo["GENX_DEVICE"]; //RFID_DEVICE
						}else if (deviceVersionInfo.ContainsKey("CRIMAN_DEVICE"))
                        {
							return deviceVersionInfo["CRIMAN_DEVICE"]; //RFID_DEVICE
						}
					}
					return "";
				}
				catch (OperationFailureException ex)
				{
				}
				return "";
			}
		}

		public string radio
		{
			get
			{
				try
				{
					if (ReaderModel.readerModel.isConnected == true)
					{
						var deviceVersionInfo = new Android.Runtime.JavaDictionary<string, string>();
						rfidModel.rfidReader.Config.GetDeviceVersionInfo(deviceVersionInfo);
						if (deviceVersionInfo.ContainsKey(Constants.Nge))
						{
							return deviceVersionInfo[Constants.Nge]; //NGE
						}
					}
					return "";
				}
				catch (OperationFailureException ex)
				{
				}
				return "";
			}
		}
	}
}