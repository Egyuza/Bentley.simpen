using System;
using System.Collections.Generic;
using System.Text;

namespace Embedded.Openings.Shared
{
public class OpeningTask
{
    public double Height {get; set;}
    public double Width {get; set;}
    public double Depth {get; set;}

    public string Code {get; set;}

    public IntPtr OwnerFormRef {get; private set;}

    public Dictionary<string, string> Properties {get; set;}
}
}
