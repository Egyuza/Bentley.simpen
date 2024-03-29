﻿using System;
using System.Reflection;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

namespace simpen.ui
{

[Bentley.MicroStation.AddIn(        
    KeyinTree = "simpen.ui.commands.xml",
    MdlTaskID = "SIMPEN.UI")
]
internal sealed class Addin : Bentley.MicroStation.AddIn
{
    private static Addin instance_;
    
    public string Name { get { return "simpen.ui"; }}

    private const string mdlRefAppName = "simpen";
    
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
    
    private static TFCOM.TFApplication _tfApp;
    public static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    }

    /// <summary> AddIn class must override the virtual Run() method of the base Addin class </summary>
    protected override int Run( string[] commandLine )
    {
        App.CadInputQueue.SendCommand(string.Format("mdl load {0}", mdlRefAppName));
        return 0;
    }
    
    public void sendKeyin(string command, bool likeKeyboardCommand = false)
    {
        App.CadInputQueue.SendCommand(string.Format("mdl keyin {0} {0} {1}", 
            mdlRefAppName, command), likeKeyboardCommand);
    }
    
    public object getCExpressionValue(string CExpression)
    {
        object value = App.GetCExpressionValue(CExpression, mdlRefAppName);
        return App.GetCExpressionValue(CExpression, mdlRefAppName);
    }

    public void setCExpressionValue(string CExpression, object newValue)
    {
        if (newValue is bool)
            newValue = Convert.ToInt32(newValue);

        App.SetCExpressionValue(CExpression, newValue, mdlRefAppName);
    }

    public static string getVersion()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format("v{0}.{1}.{2}", 
            version.Major, version.Minor, version.Build);
    }

}

}
