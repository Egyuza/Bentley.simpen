using System;
using System.Reflection;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace simpen.ui
{

[Bentley.MicroStation.AddIn(        
    KeyinTree = "simpen.ui.commands.xml",
    MdlTaskID = "SIMPEN.UI")
]
internal sealed class Addin : Bentley.MicroStation.AddIn
{
    private static Addin instance_;
    
    private const string mdlAppName = "simpen";
    
    /// <summary> Private constructor required for all AddIn classes derived from
    /// Bentley.MicroStation.AddIn. </summary>
    private Addin( IntPtr mdlDesc ) : base(mdlDesc)
    {
        instance_ = this;      
    }
    
    public static Addin Instance
    {
       get { return instance_; }
    }
    
    public static BCOM.Application App
    {
        get { return Bentley.MicroStation.InteropServices.Utilities.ComApp; }
    }
    
    /// <summary> AddIn class must override the virtual Run() method of the base Addin class </summary>
    protected override int Run( string[] commandLine )
    {
        App.CadInputQueue.SendCommand(string.Format("mdl load {0}", mdlAppName));
        return 0;
    }
    
    public void sendKeyin(string command, bool likeKeyboardCommand = false)
    {
        App.CadInputQueue.SendCommand(string.Format("mdl keyin {0} {0} {1}", 
            mdlAppName, command), likeKeyboardCommand);
    }
    
    public object getCExpressionValue(string CExpression)
    {
        object value = App.GetCExpressionValue(CExpression, mdlAppName);
        return App.GetCExpressionValue(CExpression, mdlAppName);
    }

    public void setCExpressionValue(string CExpression, object newValue)
    {
        if (newValue is bool)
            newValue = Convert.ToInt32(newValue);

        App.SetCExpressionValue(CExpression, newValue, mdlAppName);
    }

    public static string getVersion()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format("v{0}.{1}.{2}", 
            version.Major, version.Minor, version.Build);
    }

}

}
