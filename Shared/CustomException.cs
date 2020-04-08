using System;
using System.Windows.Forms;
using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET;
using Bentley.MstnPlatformNET;
#endif

namespace Shared
{
public class CustomException : Exception
{
    private Exception _inner = null;

    public override string Message
    {
        get
        {
            string msg = base.Message;
            if (_inner != null)
            {
                msg += string.Format(" (InnerMessage: {0})", 
                    _inner.Message);
            }
            return msg;
        }
    }

    public CustomException(string message)
        : base(message)
    {
    }

    public CustomException(string format, params object[] args)
        : base(String.Format(format, args))
    {
    }

    public CustomException(Exception ex, string message)
        : base(message)
    {
        _inner = ex;
    }

    /// <summary> Оповещение - вывод текста ошибки </summary>
    public void Alert()
    {
        AlertIfDebug(this);
    }
    /// <summary> Оповещение - вывод текста ошибки </summary>
    public static void AlertIfDebug(Exception ex)
    {
        #if DEBUG
            #if CONNECT
                MessageCenter.Instance.ShowErrorMessage(ex.Message, ex.StackTrace, true);
            #else            
            #endif
        #endif
    }

    public static void AlertIfDebug(string errMessage, string details = null)
    {
        #if DEBUG
            #if CONNECT
                MessageCenter.Instance.ShowErrorMessage(errMessage, "", true);
            #elif V8i    
                App.MessageCenter.AddMessage(errMessage, details, 
                BCOM.MsdMessageCenterPriority.Error, true);
            #endif
        #endif
    }

    /// <summary> Оповещение - вывод текста ошибки </summary>
    public static void Alert(string text)
    {
        // todo !  в MessageCenter?
        #if CONNECT
            NotificationManager.OpenMessageBox(
                NotificationManager.MessageBoxType.Ok, text, 
                NotificationManager.MessageBoxIconType.Critical);
        #elif V8i
            MessageBox.Show(text, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        #endif
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
