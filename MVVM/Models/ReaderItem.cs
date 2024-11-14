using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;



namespace MauiRfidSample.MVVM.Models
{
	public class ReaderItem : INotifyPropertyChanged
	{
		public ReaderItem()
		{

		}
		private string _deviceNumber;
		private string _deviceModel;
		private string _deviceSerialNumber;
		private bool _isSelected;
        internal int Index;

        public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		public string DeviceNumber
		{
			get { return _deviceNumber; }
			set
			{
				_deviceNumber = value;
				OnPropertyChanged();
			}
		}

		public string DeviceModel
		{
			get { return _deviceModel; }
			set
			{
				_deviceModel = value;
				OnPropertyChanged();
			}
		}

		public string DeviceSerialNumber
		{
			get { return _deviceSerialNumber; }
			set
			{
				_deviceSerialNumber = value;
				OnPropertyChanged();
			}
		}

		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				_isSelected = value;
				OnPropertyChanged();
			}
		}
	}
}