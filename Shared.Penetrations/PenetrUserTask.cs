using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

namespace Embedded.Penetrations.Shared
{
public class PenetrUserTask : BentleyInteropBase
{
    public string KKS {get; set;}
    public long FlangesType {get; set;}
    public DiameterType DiameterType {get; set;}
    public int LengthCm {get; set;} // в см, кратно 5 мм
    public BCOM.Point3d Location {get; set;}

    public BCOM.Matrix3d Rotation {get; set;} = App.Matrix3dZero();

    public int FlangesCount => FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0;
}
}
