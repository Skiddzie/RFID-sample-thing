using Android.Widget;
using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using static Android.Graphics.ColorSpace;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class ReaderListViewModel : BaseViewModel
    {
        public ObservableCollection<ReaderItem> _allItems;
        private ReaderItem _SelectedItem;
        private IList<ReaderDevice> readerlist;

        /// <summary>
        /// View model for reader list
        /// Creates observable collection for reader list
        /// Updates reader list
        /// </summary>
        public ReaderListViewModel()
        {
            if (_allItems == null)
                _allItems = new ObservableCollection<ReaderItem>();
            ThreadPool.QueueUserWorkItem(o =>
            {
                UpdateReaderList();
            });
        }

        /// <summary>
        /// Clear observable collection for reader list
        /// Updates reader list from available readers
        /// Update connection status and populate name and serial number fields
        /// </summary>
        private void UpdateReaderList()
        {
            SelectedReader = null;
            AllItems.Clear();
            readerlist = rfidModel.GetAvailableReaders();
            for (int i = 0; i < readerlist.Count; i++)
            {
                string model = "";
                string serialnumber = "";
                bool isConnected = false;
                if (rfidModel.rfidReader != null && rfidModel.rfidReader.HostName.Equals(readerlist[i].Name) && rfidModel.rfidReader.IsConnected)
                {
                    model = rfidModel.rfidReader.ReaderCapabilities.ModelName;
                    serialnumber = rfidModel.rfidReader.ReaderCapabilities.SerialNumber;
                    isConnected = true;
                }
                ReaderItem ritem = new ReaderItem
                {
                    DeviceNumber = readerlist[i].Name,
                    DeviceModel = model,
                    DeviceSerialNumber = serialnumber,
                    Index = i,
                    IsSelected = isConnected
                };
                AllItems.Add(ritem);
                if (isConnected)
                    SelectedReader = ritem;
            }
        }


        /// <summary>
        /// On tap of item from list connect if not connected, disconnect if already connected
        /// </summary>
        /// <param name="selectedItem"> tapped reader item </param>
        internal void ItemSelected(object selectedItem)
        {
            var ritem = ((ReaderItem)selectedItem);
            Console.WriteLine("ItemSelected " + ritem.DeviceNumber);
            int id = AllItems.IndexOf(ritem);

            // If same reader then disconnect on second tap
            if (ritem.IsSelected)
            {
                ritem.IsSelected = false;
                SelectedReader = null;
                rfidModel.Disconnect();
            }
            else
            {
                // if other reader is connected then disconnect it
                if (isConnected)
                {
                    ReaderItem ritem2 = null;
                    foreach (var item in AllItems)
                        if (item.DeviceNumber.Equals(rfidModel.rfidReader.HostName))
                            ritem2 = item;
                    ritem2.IsSelected = false;
                    SelectedReader = null;
                    rfidModel.Disconnect();
                }
                // if first tap or diffrent reader then connect
                rfidModel.ConnectReader(ritem.Index);
                SelectedReader = ritem;
            }
            //_allItems[id] = ritem;
        }


        /// <summary>
        /// Delegate subscribed for reader connection and disconnection event
        /// </summary>
        /// <param name="connection">whether connect or disconnected </param>
        public override void ReaderConnectionEvent(bool connection)
        {
            Console.WriteLine("ReaderConnectionEvent " + connection);

            base.ReaderConnectionEvent(connection);
            if (connection) {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Toast.MakeText(Android.App.Application.Context, "Connected to: "+ rfidModel.rfidReader.HostName, ToastLength.Short).Show();
                });
                AllItems.Clear();
                for (int i = 0; i < readerlist.Count; i++)
                {
                    string model = "";
                    string serialnumber = "";
                    bool isConnected = false;
                    if (rfidModel.rfidReader != null && rfidModel.rfidReader.HostName.Equals(readerlist[i].Name) && rfidModel.rfidReader.IsConnected)
                    {
                        model = rfidModel.rfidReader.ReaderCapabilities.ModelName;
                        serialnumber = rfidModel.rfidReader.ReaderCapabilities.SerialNumber;
                        isConnected = true;
                    }
                    ReaderItem ritem = new ReaderItem
                    {
                        DeviceNumber = readerlist[i].Name,
                        DeviceModel = model,
                        DeviceSerialNumber = serialnumber,
                        Index = i,
                        IsSelected = isConnected
                    };
                    AllItems.Add(ritem);
                    if (isConnected)
                        SelectedReader = ritem;
                }
            }
        
            if (SelectedReader != null)
            {
                SelectedReader.IsSelected = connection;
                if (connection)
                {
                   // SelectedReader.DeviceModel = rfidModel.rfidReader.ReaderCapabilities.ModelName;
                    //SelectedReader.DeviceSerialNumber = rfidModel.rfidReader.ReaderCapabilities.SerialNumber;
                }
                else
                {
                    if (SelectedReader.DeviceNumber.Equals(rfidModel.rfidReader.HostName))
                        ThreadPool.QueueUserWorkItem(o => rfidModel.Disconnect());
                    SelectedReader = null;
                }
            }
        }

        /// <summary>
        /// Delegate subscribed for reader connection and disconnection event
        /// </summary>
        /// <param name="appeared">Reader appeared/attached or disappeared/detach</param>
        public override void ReaderAppearanceEvent(bool appeared)
        {
            base.ReaderAppearanceEvent(appeared);
            ThreadPool.QueueUserWorkItem(o =>
            {
                UpdateReaderList();
                // if single reader and not connected then connect same
                if (AllItems.Count == 1 && !rfidModel.isConnected)
                    ItemSelected(AllItems[0]);
            });
        }

        public ObservableCollection<ReaderItem> AllItems
        {
            get { return _allItems; }
            set
            {
                _allItems = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ReaderItem SelectedReader
        {
            get { return _SelectedItem; }
            set
            {
                Console.WriteLine("SelectedReader " + value?.DeviceNumber);
                _SelectedItem = value;
                OnPropertyChanged();
            }
        }

    }
}