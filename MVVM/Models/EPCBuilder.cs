using Android.Widget;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AscentSolutions.ZebraRfid.MVVM.Models
{
    class EPCBuilder
    {

        public int BuildEPC(string FilterEPC, string PartitionEPC, string PrefixEPC, string ReferenceEPC, string SerialEPC, string AccessData)
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
        public string EPCDelivery(string FilterEPC, string PartitionEPC, string PrefixEPC, string ReferenceEPC, string SerialEPC, string data)
        {
            if(BuildEPC(FilterEPC, PartitionEPC, PrefixEPC, ReferenceEPC, SerialEPC, data) != 0)
            {
                return data;
            }
            else
            {
                return "1";
            }
            
        }
        private void ShowAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
            });
        }
    }
}
