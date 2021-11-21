using System;
using Bentley.MstnPlatformNET;

using shared = Shared;

namespace Embedded.Penetrations
{
[AddInAttribute(MdlTaskID = "Embedded.Penetrations.Cn")]
public sealed class Addin : Bentley.MstnPlatformNET.AddIn
{
    private static Addin instance_;
    public static Addin Instance
    {
       get { return instance_; }
    }

    protected override int Run(string[] commandLine)
    {
        shared.Logger.Log.Info("старт приложения");
        return 0;
    }

    public Addin(IntPtr mdlDesc)
        : base(mdlDesc)
    {
        instance_ = this;
    }   
}
}