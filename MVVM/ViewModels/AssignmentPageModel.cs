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

        private string _accessData;

        private string _filterEPC;
        private string _partitionEPC;
        private string _prefixEPC;
        private string _referenceEPC;
        private string _serialEPC;

        private string _TagPattern;
        private string _Password;
        private string _Memorybank, _LockPrivilege;

        private int Count = 0;
        private int Offset = 2;
        private MEMORY_BANK MemoryBankParam = MEMORY_BANK.MemoryBankEpc;

        private static ReaderModel rfid = ReaderModel.readerModel;

        public List<string> MemoryBanks { get; } = new List<string> { "EPC", "TID", "USER", "ACCESS PASSWORD", "KILL PASSWORD" };

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

        // if you end up using the other memory banks this has to be turned into several different variables
        public string AccessData
        {
            get { return _accessData; }
            set { _accessData = value; OnPropertyChanged(); }
        }
        //--------------------------------------------------------------------------------------------------
        public string FilterEPC
        {
            get { return _filterEPC; }
            set { _filterEPC = value; OnPropertyChanged(); }
        }
        public string PartitionEPC
        {
            get { return _partitionEPC; }
            set { _partitionEPC = value; OnPropertyChanged(); }
        }
        public string PrefixEPC
        {
            get { return _prefixEPC; }
            set { _prefixEPC = value; OnPropertyChanged(); }
        }
        public string ReferenceEPC
        {
            get { return _referenceEPC; }
            set { _referenceEPC = value; OnPropertyChanged(); }
        }
        public string SerialEPC
        {
            get { return _serialEPC; }
            set { _serialEPC = value; OnPropertyChanged(); }
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

        public ICommand SetPowerCommand { get; }
        public ICommand ReadCommand { get; }
        public ICommand WriteCommand { get; }


        public AssignmentPageModel()
        {

            if (_allItems == null)
                _allItems = new ObservableCollection<TagItem>();
            //wanna keep this low now incase this shit gives you turbo cancer
            PowerLevelInput = "50";


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

        public int BuildEPC()
        {
            try
            {
                if (!int.TryParse(FilterEPC, out int filterValue) || filterValue < 0 || filterValue > 7)
                {
                    ShowAlert("filter value must be a number between 0 and 7.");
                    return 1;
                }

                if (!int.TryParse(PartitionEPC, out int partitionValue) || partitionValue < 0 || partitionValue > 6)
                {
                    ShowAlert("partition value must be a number between 0 and 6.");
                    return 1;
                }


                int companyPrefixBits = 0;
                int itemReferenceBits = 0;
                int serialNumberBits = 38;

                int companyPrefixDigits = 0;
                int itemReferenceDigits = 0;

                

                switch (partitionValue)
                {
                    case 0:
                        companyPrefixBits = 40;
                        itemReferenceBits = 4;
                        companyPrefixDigits = 12;
                        itemReferenceDigits = 1;
                        break;
                    case 1:
                        companyPrefixBits = 37;
                        itemReferenceBits = 7;
                        companyPrefixDigits = 11;
                        itemReferenceDigits = 2;
                        break;
                    case 2:
                        companyPrefixBits = 34;
                        itemReferenceBits = 10;
                        companyPrefixDigits = 10;
                        itemReferenceDigits = 3;
                        break;
                    case 3:
                        companyPrefixBits = 30;
                        itemReferenceBits = 14;
                        companyPrefixDigits = 9;
                        itemReferenceDigits = 4;
                        break;
                    case 4:
                        companyPrefixBits = 27;
                        itemReferenceBits = 17;
                        companyPrefixDigits = 8;
                        itemReferenceDigits = 5;
                        break;
                    case 5:
                        companyPrefixBits = 24;
                        itemReferenceBits = 20;
                        companyPrefixDigits = 7;
                        itemReferenceDigits = 6;
                        break;
                    case 6:
                        companyPrefixBits = 20;
                        itemReferenceBits = 24;
                        companyPrefixDigits = 6;
                        itemReferenceDigits = 7;
                        break;
                    default:
                        ShowAlert("Invalid partition value.");
                        return 1;
                }

                if (!ulong.TryParse(PrefixEPC, out ulong companyPrefixNumber))
                {
                    ShowAlert($"Company Prefix must be a {companyPrefixDigits}-digit numeric string.");
                    return 1;
                }

                if (!ulong.TryParse(ReferenceEPC, out ulong itemReferenceNumber))
                {
                    ShowAlert($"Item Reference must be a {itemReferenceDigits}-digit numeric string.");
                    return 1;
                }


                ulong maxSerialNumber = (ulong)(System.Math.Pow(2, serialNumberBits) - 1);

                if (string.IsNullOrEmpty(SerialEPC) || !ulong.TryParse(SerialEPC, out ulong serialNumber) || serialNumber > maxSerialNumber)
                {
                    ShowAlert($"Serial Number must be a numeric string up to 12 digits (max {maxSerialNumber}).");
                    return 1;
                }

                string headerBin = Convert.ToString(48, 2).PadLeft(8, '0');
                string filterBin = Convert.ToString(filterValue, 2).PadLeft(3, '0');
                string partitionBin = Convert.ToString(partitionValue, 2).PadLeft(3, '0');

                string companyPrefixBin = Convert.ToString((long)companyPrefixNumber, 2).PadLeft(companyPrefixBits, '0');
                string itemReferenceBin = Convert.ToString((long)itemReferenceNumber, 2).PadLeft(itemReferenceBits, '0');
                string serialNumberBin = Convert.ToString((long)serialNumber, 2).PadLeft(serialNumberBits, '0');

                string epcBin = headerBin + filterBin + partitionBin + companyPrefixBin + itemReferenceBin + serialNumberBin;

                int totalBits = epcBin.Length;
                if (totalBits != 96)
                {
                    ShowAlert($"An error occurred during EPC construction. Expected 96 bits but got {totalBits} bits.");
                    return 1;
                }

                //convert back to hex. idk how this works
                //why is it not ToString() with base 16 the way binary would be with base 2?
                StringBuilder epcHexBuilder = new StringBuilder(24);
                for (int i = 0; i < 96; i += 4)
                {
                    string fourBits = epcBin.Substring(i, 4);
                    epcHexBuilder.Append(Convert.ToByte(fourBits, 2).ToString("X1"));
                }

                string epcHex = epcHexBuilder.ToString();

                //this variable is also being used to send to other memory banks, which causes issues
                AccessData = epcHex.ToUpper(); 

                //returning integers because checking for null values within the method itself seems to give null exceptions anyway
                //the issue only exists with prefix and reference, no clue why
                //there's probably a much more elegant way to do this, but this is what i found works, and it doesn't seem to cause any issues
                //now it's like a c program :)
                return 0;
            }
            catch (Exception ex)
            {
                ShowAlert("An error occurred: " + ex.Message);
                return 1;
            }
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
                //i forget why it was easier to check if they were null over here instead of within the method.
                //it was causing an issue though, please trust me
                if (PrefixEPC is null || ReferenceEPC is null)
                {
                    ShowAlert("Check Prefix and Reference");
                    return;
                }
                int builtEPC = BuildEPC();
                if (builtEPC == 1)
                {
                    return;
                }

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