using System;
using Bentley.MstnPlatformNET;

namespace Embedded.Penetrations.Cn
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
        return 0;
    }

    public Addin(IntPtr mdlDesc)
        : base(mdlDesc)
    {
        instance_ = this;
    }   
}
}