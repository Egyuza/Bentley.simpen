using System.Collections.Generic;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace Embedded.Penetrations.Shared
{
public interface IPenetrTask
{
    string Code {get;}

    string Name { get; }

    long FlangesType {get; }
    DiameterType DiameterType {get;}

    /// <summary> в см, кратно 5 мм </summary>
    int LengthCm {get; set;}
    BCOM.Point3d Location { get; }
    BCOM.Matrix3d Rotation {get; }

    BCOM.Point3d CorrectiveAngles { get; }

    int FlangesCount { get; }

    double FlangeWallOffset{ get; }

    BCOM.ModelReference ModelRef { get; }

     /// <summary>
    /// расположение фланца, если он один
    /// </summary>
    BCOM.Vector3d SingleFlangeSide { get; }

    bool IsSingleFlangeFirst { get; }

    Dictionary<Sp3dToDataGroupMapProperty, string> DataGroupPropsValues { get; set; }

    void prepairDataGroup();

    void scanInfo();
}
}
