using System;
using System.Windows.Forms;


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
}
}
