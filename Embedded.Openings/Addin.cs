using System;
using System.Reflection;

namespace Embedded.Openings
{

[Bentley.MicroStation.AddIn(        
    KeyinTree = "Embedded.Openings.commands.xml",
    MdlTaskID = "Embedded.Openings")
]
internal sealed class Addin : Bentley.MicroStation.AddIn
{
    private static Addin instance_;
       
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

    /// <summary> AddIn class must override the virtual Run() method of the base Addin class </summary>
    protected override int Run( string[] commandLine )
    {
        return 0;
    }

    public static string getVersion()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        return string.Format("v{0}.{1}.{2}", 
            version.Major, version.Minor, version.Build);
    }
}

}
