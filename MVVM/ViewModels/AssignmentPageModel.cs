using Android.Widget;
using Com.Zebra.Rfid.Api3;
using Java.Lang;
using MauiRfidSample.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Input;
using Xamarin.Google.Crypto.Tink.Prf;

using static Com.Zebra.Rfid.Api3.Antennas;
using Exception = System.Exception;
using Object = System.Object;
using String = System.String;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class AssignmentPageModel : BaseViewModel
    {
        private static ObservableCollection<TagItem> _allItems;
        private static TagItem _mySelectedItem;
        private static Dictionary<String, int> tagListDict = new Dictionary<string, int>();
        private DateTime startime;
        private int totalTagCount = 0;
        private static string _uniquetags, _totaltags, _totaltime;
        private string _connectionStatus, _readerStatus;
        private System.Timers.Timer aTimer;
        private bool _listAvailable;

        //copied from ReadWriteOperationsModel
        private string _accessData;
        private string _TagPattern;
        private string _Password;
        private string _Memorybank, _LockPrivilege;

        private int Count = 0;
        private int Offset = 2;
        private MEMORY_BANK MemoryBankParam = MEMORY_BANK.MemoryBankEpc;

        private static ReaderModel rfid = ReaderModel.readerModel;

        public List<string> MemoryBanks { get; } = new List<string> { "EPC", "TID", "USER", "ACCESS PASSWORD", "KILL PASSWORD" };
        //

        private string _powerLevelInput;
        public string PowerLevelInput
        {
            get => _powerLevelInput;
            set
            {
                if (_powerLevelInput != value)
                {
                    _powerLevelInput = value;
                    OnPropertyChanged();
                }
            }
        }

        //
        public string AccessData
        {
            get { return _accessData; }
            set { _accessData = value; OnPropertyChanged(); }
        }

        public string TagPattern
        {
            get { return _TagPattern; }
            set { _TagPattern = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get { return _Password; }
            set { _Password = value; OnPropertyChanged(); }
        }

        public string Memorybank
        {
            get { return _Memorybank; }
            set { _Memorybank = value; OnPropertyChanged(); }
        }

        public string LockPrivilege
        {
            get { return _LockPrivilege; }
            set { _LockPrivilege = value; OnPropertyChanged(); }
        }
        //

        public ICommand SetPowerCommand { get; }
        public ICommand ReadCommand { get; }
        public ICommand WriteCommand { get; }


        public AssignmentPageModel()
        {

            if (_allItems == null)
                _allItems = new ObservableCollection<TagItem>();

            PowerLevelInput = "270";


            SetPowerCommand = new Command(SetPower);

            if (!rfidModel.isConnected)
            {
                rfidModel.Setup();
            }
            updateHints();

            Password = "0";
            Memorybank = "EPC";
            LockPrivilege = "Read and Write";
            AccessData = "";
            TagPattern = SelectedItem?.ToString() ?? "";

            ReadCommand = new Command(() => AccessOperationsReadClicked());

            WriteCommand = new Command(() => AccessOperationsWriteClicked());
        }

        private void UpdateBank()
        {
            switch (Memorybank)
            {
                case "EPC":
                    Count = 0;
                    Offset = 2;
                    MemoryBankParam = MEMORY_BANK.MemoryBankEpc;
                    break;
                case "TID":
                    Count = 0;
                    Offset = 0;
                    MemoryBankParam = MEMORY_BANK.MemoryBankTid;
                    break;
                case "USER":
                    Count = 0; //originally 2
                    Offset = 0;
                    MemoryBankParam = MEMORY_BANK.MemoryBankUser;
                    break;
                case "ACCESS PASSWORD":
                    Count = 2;
                    Offset = 2;
                    MemoryBankParam = MEMORY_BANK.MemoryBankReserved;
                    break;
                case "KILL PASSWORD":
                    Count = 2;
                    Offset = 0;
                    MemoryBankParam = MEMORY_BANK.MemoryBankReserved;
                    break;
            }
        }

        public void AccessOperationsReadClicked()
        {
            string TagId = TagPattern;
            UpdateBank();
            if (ValidateFields())
            {

                TagAccess tagAccess = new TagAccess();
                TagAccess.ReadAccessParams readAccessParams = new TagAccess.ReadAccessParams(tagAccess);

                readAccessParams.AccessPassword = (long)Long.Decode("0X" + Password);
                readAccessParams.Count = Count;
                readAccessParams.MemoryBank = MemoryBankParam;
                readAccessParams.Offset = Offset;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        TagData tagData = rfid.rfidReader.Actions.TagAccess.ReadWait(TagId, readAccessParams, null, false);
                        AccessData = tagData.MemoryBankData?.ToString();
                        ShowAlert(tagData.OpStatus.ToString());
                    }
                    catch (InvalidUsageException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                    catch (OperationFailureException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                });
            }

        }

        public void AccessOperationsWriteClicked()
        {
            string TagId = TagPattern;
            UpdateBank();
            if (ValidateFields())
            {
                TagAccess tagAccess = new TagAccess();
                TagAccess.WriteAccessParams writeAccessParams = new TagAccess.WriteAccessParams(tagAccess);
                writeAccessParams.AccessPassword = (long)Long.Decode("0X" + Password);
                writeAccessParams.MemoryBank = MemoryBankParam;
                writeAccessParams.Offset = Offset;
                writeAccessParams.SetWriteData(AccessData);
                writeAccessParams.WriteDataLength = AccessData.Length / 4;

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        rfid.rfidReader.Actions.TagAccess.WriteWait(TagId, writeAccessParams, null, null, true, false);
                        ShowAlert("Write Success");
                    }
                    catch (InvalidUsageException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                    catch (OperationFailureException e)
                    {
                        e.PrintStackTrace();
                        ShowAlert(e);
                    }
                });
            }
        }

        private async void SetPower()
        {
            try
            {
                if (rfidModel.rfidReader == null || !rfidModel.rfidReader.IsConnected)
                {
                    Console.WriteLine("RFID reader is not connected.");
                    return;
                }
                AntennaRfConfig antennaRfConfig = rfidModel.rfidReader.Config.Antennas.GetAntennaRfConfig(1);

                antennaRfConfig.TransmitPowerIndex = int.Parse(PowerLevelInput);

                rfidModel.rfidReader.Config.Antennas.SetAntennaRfConfig(1, antennaRfConfig);

                Console.WriteLine($"Transmit power set to level {PowerLevelInput}.");
            }
            catch (OperationFailureException ex)
            {
                Console.WriteLine("Failed to set transmit power: " + ex.StatusDescription);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error setting transmit power: " + ex.Message);
            }
        }

        public override void ReaderConnectionEvent(bool connection)
        {
            base.ReaderConnectionEvent(connection);
            if (connection)
            {
                SetPower();
            }
            updateHints();
            aTimer?.Stop();
            aTimer?.Dispose();
        }

        public ObservableCollection<TagItem> AllItems { get => _allItems; set => _allItems = value; }

        public TagItem MySelectedItem
        {
            get => _mySelectedItem;
            set
            {
                if (_mySelectedItem != value)
                {
                    _mySelectedItem = value;
                    OnPropertyChanged();
                    // Update TagPattern when selection changes
                    TagPattern = _mySelectedItem?.InvID;
                }
            }
        }
        public static String SelectedItem
        {
            get { return _mySelectedItem?.InvID; }
        }

        public string UniqueTags { get => _uniquetags; set { _uniquetags = value; OnPropertyChanged(); } }
        public string TotalTags { get => _totaltags; set { _totaltags = value; OnPropertyChanged(); } }
        public string TotalTime { get => _totaltime; set { _totaltime = value; OnPropertyChanged(); } }
        public string readerConnection { get => _connectionStatus; set { _connectionStatus = value; OnPropertyChanged(); } }
        public bool listAvailable { get => _listAvailable; set { _listAvailable = value; OnPropertyChanged(); } }
        public bool hintAvailable { get => !_listAvailable; set { OnPropertyChanged(); } }
        public string readerStatus { get => _readerStatus; set { _readerStatus = value; OnPropertyChanged(); } }

        private Object tagreadlock = new object();

        // Tag event
        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void TagReadEvent(TagData[] aryTags)
        {
            lock (tagreadlock)
            {
                for (int index = 0; index < aryTags.Length; index++)
                {
                    Console.WriteLine("Tag ID " + aryTags[index].TagID);

                    String tagID = aryTags[index].TagID;
                    if (tagID != null)
                    {
                        if (tagListDict.ContainsKey(tagID))
                        {
                            tagListDict[tagID] = tagListDict[tagID] + aryTags[index].TagSeenCount;
                            UpdateCount(tagID, tagListDict[tagID], aryTags[index].PeakRSSI);
                        }
                        else
                        {
                            tagListDict.Add(tagID, aryTags[index].TagSeenCount);
                            UpdateList(tagID, aryTags[index].TagSeenCount, aryTags[index].PeakRSSI);
                        }
                    }
                    totalTagCount += aryTags[index].TagSeenCount;
                    updateCounts();
                    if (aryTags[index].OpCode == ACCESS_OPERATION_CODE.AccessOperationRead &&
                        aryTags[index].OpStatus == ACCESS_OPERATION_STATUS.AccessSuccess)
                    {
                        if (aryTags[index].MemoryBankData.Length > 0)
                        {
                            Console.WriteLine(" Mem Bank Data " + aryTags[index].MemoryBankData);
                        }
                    }
                }
            }
        }

        private void UpdateList(String tag, int count, short rssi)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                _allItems.Add(new TagItem { InvID = tag, TagCount = count, RSSI = rssi });
            });
        }

        private void UpdateCount(String tag, int count, short rssi)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var found = _allItems.FirstOrDefault(x => x.InvID == tag);
                if (found != null)
                {
                    found.TagCount = count;
                    found.RSSI = rssi;
                }
            });
        }


        public override void HHTriggerEvent(bool pressed)
        {
            if (pressed)
            {
                PerformInventory();
                listAvailable = true;
                hintAvailable = false;
            }
            else
            {
                StopInventory();
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void StopInventory()
        {
            rfidModel.StopInventory();
            aTimer?.Stop();
            aTimer?.Dispose();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void PerformInventory()
        {
            Device.BeginInvokeOnMainThread(() => { tagListDict.Clear(); _allItems.Clear(); });
            totalTagCount = 0;
            startime = DateTime.Now;
            SetTimer();
            rfidModel.PerformInventory();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void StatusEvent(IEvents.StatusEventData statusEvent)
        {
            if (statusEvent.StatusEventType == STATUS_EVENT_TYPE.InventoryStartEvent)
            {
                //startime = DateTime.Now;
            }
            if (statusEvent.StatusEventType == STATUS_EVENT_TYPE.InventoryStopEvent)
            {
                updateCounts();
                int total = 0;
                foreach (var entry in tagListDict)
                    total += entry.Value;
                Console.WriteLine("Unique tags " + tagListDict.Count + " Total tags" + total);
            }
        }

        private void updateCounts()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                UniqueTags = tagListDict.Count.ToString();
                TotalTags = totalTagCount.ToString();
                TimeSpan span = (DateTime.Now - startime);
                TotalTime = span.ToString("hh\\:mm\\:ss");

            });
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            updateCounts();
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }



        private void updateHints()
        {
            if (_allItems.Count == 0)
            {
                _listAvailable = false;
                readerConnection = isConnected ? "Connected" : "Not connected";
                if (isConnected)
                {
                    readerStatus = rfidModel.isBatchMode ? "Inventory is running in batch mode" : "Press and hold the trigger for tag reading";
                }
            }
            else
                _listAvailable = true;
        }

        private void ShowAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
            });
        }


        private void ShowAlert(OperationFailureException e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, e.VendorMessage, ToastLength.Short).Show();
            });
        }

        private void ShowAlert(InvalidUsageException e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, e.Info, ToastLength.Short).Show();
            });
        }

        private bool ValidateFields()
        {
            if (isConnected)
            {
                try
                {
                    long pw = (long)Long.Decode("0X" + Password);
                    return true;
                }
                catch (NumberFormatException nfe)
                {
                    nfe.PrintStackTrace();
                    Android.Widget.Toast.MakeText(Android.App.Application.Context, "Password field is invalid !", ToastLength.Long).Show();
                }
            }
            else
                Android.Widget.Toast.MakeText(Android.App.Application.Context, "Reader is not connected", ToastLength.Long).Show();
            return false;
        }
    }
}