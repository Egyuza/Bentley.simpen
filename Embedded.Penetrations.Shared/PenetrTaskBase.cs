﻿using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Embedded.Penetrations.Shared
{
public abstract class PenetrTaskBase : BentleyInteropBase, IPenetrTask
{
    public string Code {get; set;}

    public string Name => $"T{PenCode}-{DiameterType.Number}-{LengthCm}";

    public string PenCode { get; set; }
    public long FlangesType
    {
        get { return PenetrInfo.getFlangesType(PenCode); }
        set { PenCode = PenetrInfo.getPenCode(value); }
    }

    public DiameterType DiameterType {get; set;}
    public int LengthCm {get; set;} // в см, кратно 5 мм


    private BCOM.Point3d location_;
    public BCOM.Point3d Location
    {
        get { return location_; } 
        set { location_ = RoundTool.roundExt(value, /* 5 мм */ 5 / UOR.activeSubPerMaster); }
    }

    public BCOM.Point3d?[] RefPoints {get; private set;}

    public BCOM.Matrix3d Rotation {get; set;}

    public int FlangesCount => 
        FlangesType == 1 ? 1 : FlangesType == 2 ? 2 : 0;

    /// <summary>
    /// смещение плоскости фланца относительно плоскости стены,
    /// для лучшей видимости фланца
    /// отступ 0.5 мм
    /// </summary>
    public double FlangeWallOffset => 
        1.0 / UOR.activeSubPerMaster; // TODO можно ли сделать мешьше - 0.02

    public BCOM.ModelReference modelRef {get; protected set;}
    public BCOM.Vector3d singleFlangeSide {get; protected set;}

    public abstract UOR UOR {get;}

    public virtual BCOM.Point3d CorrectiveAngles {get;}
    public BCOM.ModelReference ModelRef {get; }
    public BCOM.Vector3d SingleFlangeSide { get; }

    public bool IsSingleFlangeFirst => App.Vector3dEqualTolerance(
        SingleFlangeSide, App.Vector3dFromXYZ(0, 0, -1), 0.1);


    public Dictionary<Sp3dToDataGroupMapProperty, string> DataGroupPropsValues { get; set; }

    public virtual void scanInfo()
    {
        // TODO?
    }

    public void prepairDataGroup()
    {
        DataGroupPropsValues = DataGroupPropsValues ?? new Dictionary<Sp3dToDataGroupMapProperty, string>();
        DataGroupPropsValues.Clear();
        foreach (var propMap in Sp3dToDGMapping.Instance.Items)
        {  
            if (propMap.TargetXPath == null)
                continue;
           
            switch (propMap.Key)
            {
            case "Code": DataGroupPropsValues[propMap] = propMap.getMapValue(Code); break;
            case "Name": DataGroupPropsValues[propMap] = propMap.getMapValue(Name); break;
            }
        }
    }

    public PenetrTaskBase()
    {
        Rotation = App.Matrix3dIdentity();
        CorrectiveAngles = App.Point3dZero();
        ModelRef = App.ActiveModelReference;

        // базовая ориентация при построении
        // фланец совпадает с Точкой установки
        SingleFlangeSide = App.Vector3dFromXYZ(0, 0, -1);
    }

    public override string ToString() => Name;

    public static BCOM.Level LevelMain => ElementHelper.GetOrCreateLevel(PenConfigVariables.Level.Value);
    public static BCOM.Level LevelSymb => ElementHelper.GetOrCreateLevel(PenConfigVariables.ProjectionLevel.Value);
    public static BCOM.Level LevelFlangeSymb => ElementHelper.GetOrCreateLevel(PenConfigVariables.ProjectionFlangeLevel.Value);
    public static BCOM.Level LevelRefPoint => ElementHelper.GetOrCreateLevel(PenConfigVariables.ProjectionPointLevel.Value);

}
}
