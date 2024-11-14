using Com.Zebra.Rfid.Api3;
using MauiRfidSample.MVVM.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Timers;

namespace MauiRfidSample.MVVM.ViewModels
{
    class RelativeDistanceViewModel : BaseViewModel
    {
        public ObservableCollection<TagItem> Tags { get; set; }
        private Object tagreadlock = new object();
        private static Dictionary<String, int> tagListDict = new Dictionary<string, int>();
        private bool _listAvailable;
        private string _connectionStatus;
        public string readerConnection { get => _connectionStatus; set { _connectionStatus = value; OnPropertyChanged(); } }
        public bool listAvailable { get => _listAvailable; set { _listAvailable = value; OnPropertyChanged(); } }
        public bool hintAvailable { get => !_listAvailable; set { OnPropertyChanged(); } }
        private System.Timers.Timer aTimer;

        public RelativeDistanceViewModel()
        {
            if (Tags == null)
                Tags = new ObservableCollection<TagItem>();
            updateHints();
        }

        public override void TagReadEvent(TagData[] aryTags)
        {
            lock (tagreadlock)
            {
                for (int index = 0; index < aryTags.Length; index++)
                {
                    //Console.WriteLine("Tag ID " + aryTags[index].TagID);

                    String tagID = aryTags[index].TagID;
                    if (tagID != null)
                    {
                        if (tagListDict.ContainsKey(tagID))
                        {
                            tagListDict[tagID] = tagListDict[tagID] + aryTags[index].TagSeenCount;
                        }
                        else
                        {
                            tagListDict.Add(tagID, aryTags[index].TagSeenCount);
                            Tags.Add(new TagItem { InvID = tagID, RSSI = 0, RelativeDistance = 0, TagCount = aryTags[index].TagSeenCount });
                        }
                        UpdateDistance(tagID, tagListDict[tagID], aryTags[index].PeakRSSI);
                    }
                }
            }
        }

        private void UpdateDistance(string tagID, int count, short rssi)
        {
            var found = Tags.FirstOrDefault(x => x.InvID == tagID);
            if (found != null)
            {
                found.TagCount = count;
                // normalize
                int nr = rssi;
                if (rssi < -72)
                    nr = -72;
                if (rssi > -22)
                    nr = -22;
                found.RelativeDistance = (nr + 72) * 2;
                //Console.WriteLine("Tag ID " + found.InvID + " " + found.TagCount + " " + rssi + " " + found.RelativeDistance);
            }
        }

        public override void HHTriggerEvent(bool pressed)
        {
            if (pressed)
            {
                Tags.Clear();
                tagListDict.Clear();
                SetSingulation(true);
                bool bret = rfidModel.PerformInventory();
                if (bret)
                {
                    SetTimer();
                    listAvailable = true;
                    hintAvailable = false;
                }
            }
            else
            {
                rfidModel.StopInventory();
                aTimer?.Stop();
                aTimer?.Dispose();
                SetSingulation(false);
            }
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
            if (Tags.Count == 0)
            {
                _listAvailable = false;
                readerConnection = isConnected ? "Connected" : "Not connected";
            }
            else
                _listAvailable = true;
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(2000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            updateCounts();
        }

        private void updateCounts()
        {
            lock (tagreadlock)
            {
                foreach (var t in Tags)
                {
                    // no read for second 
                    if (t.TagCount == 0)
                        t.RelativeDistance = 0;
                    t.TagCount = 0;
                    tagListDict[t.InvID] = 0;
                }
            }
        }

        private void SetSingulation(bool set)
        {
            try
            {
                var singulation = rfidModel.rfidReader.Config.Antennas.GetSingulationControl(1);
                singulation.Session = SESSION.SessionS0;
                singulation.Action.SetPerformStateAwareSingulationAction(false);
                if (set)
                {
                    singulation.Action.InventoryState = INVENTORY_STATE.InventoryStateAbFlip;
                }
                else
                {
                    singulation.Action.InventoryState = INVENTORY_STATE.InventoryStateA;
                }
                rfidModel.rfidReader.Config.Antennas.SetSingulationControl(1, singulation);
            }
            catch(OperationFailureException opf)
            {
                opf.PrintStackTrace();
            }
        }
    }
}
