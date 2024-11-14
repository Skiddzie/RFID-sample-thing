using Android.Widget;
using Com.Zebra.Rfid.Api3;
using Java.Lang;
using MauiRfidSample.MVVM.Models;

namespace MauiRfidSample.MVVM.ViewModels
{
    public class ReadWriteOperationsModel : BaseViewModel
    {
        private static ReaderModel rfid = ReaderModel.readerModel;
        private string _accessData;
        private string _TagPattern;
        private string _Password;
        private string _Memorybank, _LockPrivilege;

        private int Count = 0;
        private int Offset = 2;
        private MEMORY_BANK MemoryBankParam = MEMORY_BANK.MemoryBankEpc;

        public ReadWriteOperationsModel()
        {
            Password = "0";
            Memorybank = "EPC";
            LockPrivilege = "Read and Write";
            AccessData = "";
            TagPattern = InventoryListModel.SelectedItem?.ToString() ?? "";
        }
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
                    Count = 0;
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

        public void AccessOperationsLockClicked()
        {
            string TagId = TagPattern;
            UpdateBank();
            LOCK_PRIVILEGE privilege = UpdateLockPrivilege();
            if (ValidateFields())
            {
                TagAccess tagAccess = new TagAccess();
                TagAccess.LockAccessParams lockAccessParams = new TagAccess.LockAccessParams(tagAccess);
                lockAccessParams.AccessPassword = (long)Long.Decode("0X" + Password);
                switch(Memorybank)
                {
                    case "EPC":
                        lockAccessParams.SetLockPrivilege(LOCK_DATA_FIELD.LockEpcMemory, privilege);
                        break;
                    case "TID":
                        lockAccessParams.SetLockPrivilege(LOCK_DATA_FIELD.LockTidMemory, privilege);
                        break;
                    case "USER":
                        lockAccessParams.SetLockPrivilege(LOCK_DATA_FIELD.LockUserMemory, privilege);
                        break;
                    case "ACCESS PASSWORD":
                        lockAccessParams.SetLockPrivilege(LOCK_DATA_FIELD.LockAccessPassword, privilege);
                        break;
                    case "KILL PASSWORD":
                        lockAccessParams.SetLockPrivilege(LOCK_DATA_FIELD.LockKillPassword, privilege);
                        break;
                }

                ThreadPool.QueueUserWorkItem(o =>
                {
                    try
                    {
                        rfid.rfidReader.Actions.TagAccess.LockWait(TagId, lockAccessParams, null, true);
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

        private LOCK_PRIVILEGE UpdateLockPrivilege()
        {
            LOCK_PRIVILEGE privilege = LOCK_PRIVILEGE.LockPrivilegeReadWrite;
            switch (LockPrivilege)
            {
                case "Read and Write":
                    privilege = LOCK_PRIVILEGE.LockPrivilegeReadWrite;
                    break;

                case "Permanent Lock":
                    privilege = LOCK_PRIVILEGE.LockPrivilegePermaLock;
                    break;
                case "Permanent Unlock":
                    privilege = LOCK_PRIVILEGE.LockPrivilegePermaUnlock;
                    break;
                case "Unlock":
                    privilege = LOCK_PRIVILEGE.LockPrivilegeUnlock;
                    break;

            }
            return privilege;
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
