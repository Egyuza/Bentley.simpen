using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Openings
{
public class OpeningTask
{
    public double Height {get; set;}
    public double Width {get; set;}
    public double Depth {get; set;}

    public string Code {get; set;}

    public IntPtr OwnerFormRef {get; private set;}
}
}
