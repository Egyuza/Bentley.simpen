using System;
using System.Linq;
using Bentley.MstnPlatformNET;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using System.Reflection;

namespace Embedded.Penetrations
{
[AddInAttribute(MdlTaskID = "Embedded_Penetrations")]
public sealed class Addin : Bentley.MstnPlatformNET.AddIn
{
    public string Name { get { return "Embedded_Penetrations"; }}

    private static Addin instance_;
    public static Addin Instance
    {
       get { return instance_; }
    }

    protected override int Run(string[] commandLine)
    {
        instance_ = this;
        return 0;
    }

    public Addin(IntPtr mdlDesc)
        : base(mdlDesc)
    {
    }

    public static BCOM.Application App
    {
       get { return BMI.Utilities.ComApp; }

    }

    private static TFCOM.TFApplication _tfApp;
    public static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    }

    //public void sendKeyin(string command, bool likeKeyboardCommand = false)
    //{
    //    App.CadInputQueue.SendCommand(string.Format("mdl keyin {0} {0} {1}", 
    //        Name, command), likeKeyboardCommand);
    //}
    
    //public object getCExpressionValue(string CExpression)
    //{
    //    object value = App.GetCExpressionValue(CExpression, Name);
    //    return App.GetCExpressionValue(CExpression, Name);
    //}

    //public void setCExpressionValue(string CExpression, object newValue)
    //{
    //    if (newValue is bool)
    //        newValue = Convert.ToInt32(newValue);

    //    App.SetCExpressionValue(CExpression, newValue, Name);
    //}

    public static string getVersion()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format("v{0}.{1}.{2}", 
            version.Major, version.Minor, version.Build);
    }
}
}