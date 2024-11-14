using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Timers;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class InventoryListModel : BaseViewModel
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

        public InventoryListModel()
        {
            // collection
            if (_allItems == null)
                _allItems = new ObservableCollection<TagItem>();
            // UI for hint
            updateHints();
        }

        public ObservableCollection<TagItem> AllItems { get => _allItems; set => _allItems = value; }

        public TagItem MySelectedItem { get => _mySelectedItem; set => _mySelectedItem = value; }

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

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(1000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            updateCounts();
        }

        public override void ReaderConnectionEvent(bool connection)
        {
            base.ReaderConnectionEvent(connection);
            updateHints();
            aTimer?.Stop();
            aTimer?.Dispose();
        }

        private void updateHints()
        {
            if (_allItems.Count == 0)
            {
                _listAvailable = false;
                readerConnection = isConnected ? "Connected" : "Not connected";
                if(isConnected)
                {
                    readerStatus = rfidModel.isBatchMode ? "Inventory is running in batch mode" : "Press and hold the trigger for tag reading";
                }
            }
            else
                _listAvailable = true;
        }
    }
}