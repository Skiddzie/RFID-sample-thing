using Com.Zebra.Rfid.Api3;
using Com.Zebra.Scannercontrol;
using MauiRfidSample.MVVM.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;
		public static ReaderModel rfidModel = ReaderModel.readerModel;
		private static ScannerModel scannerModel = ScannerModel.scannerModel;

		public BaseViewModel()
		{

		}

		public virtual void HHTriggerEvent(bool pressed)
		{

		}

		public virtual void TagReadEvent(TagData[] tags)
		{

		}

		public virtual void StatusEvent(IEvents.StatusEventData statusEvent)
		{

		}

        public virtual void ReaderConnectionEvent(bool connection)
        {
            isConnected = connection;
        }

        public virtual void ReaderAppearanceEvent(bool appeared)
        {

        }


        internal void UpdateIn()
		{
			rfidModel.TagRead += TagReadEvent;
			rfidModel.TriggerEvent += HHTriggerEvent;
			rfidModel.StatusEvent += StatusEvent;
            rfidModel.ReaderConnectionEvent += ReaderConnectionEvent;
            rfidModel.ReaderAppearanceEvent += ReaderAppearanceEvent;
        }

		internal void UpdateOut()
		{
			rfidModel.TagRead -= TagReadEvent;
			rfidModel.TriggerEvent -= HHTriggerEvent;
			rfidModel.StatusEvent -= StatusEvent;
            rfidModel.ReaderConnectionEvent -= ReaderConnectionEvent;
            rfidModel.ReaderAppearanceEvent -= ReaderAppearanceEvent;
        }

        public bool isConnected { get => rfidModel.isConnected; set => OnPropertyChanged(); }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}


		public virtual void ScannerConnectionEvent(string deviceName)
		{

		}

		public virtual void CurrentProgressUpdate(int currentProgress)
		{

		}

		public virtual void FWVersion(string scannerFWVersion)
		{

		}

		internal void UpdateScannerIn()
		{
			scannerModel.ScannerConnectionEvent += ScannerConnectionEvent;
			scannerModel.CurrentProgress += CurrentProgressUpdate;
			scannerModel.FWVersion += FWVersion;
		}

		internal void UpdateScannerOut()
		{
			scannerModel.ScannerConnectionEvent -= ScannerConnectionEvent;
			scannerModel.CurrentProgress -= CurrentProgressUpdate;
			scannerModel.FWVersion -= FWVersion;
		}
		public bool scannerConnected { get => scannerModel.IsConnected; set => OnPropertyChanged(); }
		public string devicName { get => scannerModel.DeviceName; set => OnPropertyChanged(); }
		public string sFWVersion { get => scannerModel.getFWVersion; set => OnPropertyChanged(); }


		public virtual void BarcodeEvent(string barcode, string barcodeType)
		{

		}

		internal void UpdateBarcodeIn()
		{
			scannerModel.BarcodeEvent += BarcodeEvent;
			scannerModel.ScannerConnectionEvent += ScannerConnectionEvent;
		}

		internal void UpdateBarcodeOut()
		{
			scannerModel.BarcodeEvent -= BarcodeEvent;
			scannerModel.ScannerConnectionEvent -= ScannerConnectionEvent;
		}

		internal void UpdateReaderWiFiEventsIn()
		{
			rfidModel.WiFiNotificationEvent += WiFiNotificationEvent;
			rfidModel.WiFiScanResultsEvent += WiFiScanResultsEvent;
		}

		internal void UpdateReaderWiFiEventsOut()
		{
			rfidModel.WiFiNotificationEvent -= WiFiNotificationEvent;
			rfidModel.WiFiScanResultsEvent -= WiFiScanResultsEvent;
		}

		public virtual void WiFiNotificationEvent(string scanStatus)
		{

		}

		public virtual void WiFiScanResultsEvent(WifiScanData data)
		{

		}
	}
}
