using Shared.Bentley;
using System;
using Bentley.Interop.MicroStationDGN;

namespace Embedded.Penetrations.Shared
{
public class PenetrUserTask : PenetrTaskBase
{
    public PenetrUserTask() : base()
    {
        
    }

    public override Point3d CorrectiveAngles => IsManualRotateMode 
        ? App.Point3dFromXYZ(App.Radians(userAngleX), 
            App.Radians(userAngleY), App.Radians(userAngleZ)) 
        : App.Point3dZero();
    
    public double userAngleX { get; set; }
    public double userAngleY { get; set; }
    public double userAngleZ { get; set; }

    public bool IsManualRotateMode { get; set; }

    public bool IsAutoLength { get; set; }

    public override UOR UOR => UOR.getForActiveModel();

}
}
