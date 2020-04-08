using System;
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
public class AddinException : Exception
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

    public AddinException(string message)
        : base(message)
    {
    }

    public AddinException(string format, params object[] args)
        : base(String.Format(format, args))
    {
    }

    public AddinException(Exception ex, string message)
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
        #if CONNECT
            MessageCenter.Instance.ShowErrorMessage(ex.Message, ex.StackTrace, true);
        #else            
        #endif
    }

    public static void AlertIfDebug(string errMessage)
    {
        #if CONNECT
            MessageCenter.Instance.ShowErrorMessage(errMessage, "", true);
        #else            
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
        #endif
    }
}
}
