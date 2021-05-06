using System;
using System.Collections.Generic;
using System.Text;
using BCOM = Bentley.Interop.MicroStationDGN;
using System.Linq;

namespace Shared.Bentley
{
public class ProjectionInfo
{
    public BCOM.Element Element { get; set; }
    public string ProjectionName { get; set; }
    public string LevelName { get; set; }
}
}
