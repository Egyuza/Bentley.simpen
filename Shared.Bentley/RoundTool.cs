using System;
using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif


namespace Shared.Bentley
{
static class RoundTool
{
    ///// <summary>
    ///// Функция кратного округления из оригинального simpen Л.Вибе
    ///// </summary>
    //public static long roundExt_Old(
    //    double val, int digs = -1, double snap = 5, int shft = 0)
    //{
    //    UOR uor = new UOR(App.ActiveModelReference);
    //    snap /= uor.activeSubPerMaster;

    //    double dv;
    //    dv = val * Math.Pow(snap, digs);
    //    dv = Math.Floor(dv + 0.55555555555555 - (0.111111111111111 * shft));
    //    dv = dv / Math.Pow(snap, digs);
    //    return Convert.ToInt64(dv);
    //}

    //public static BCOM.Point3d roundExt_Old(
    //    BCOM.Point3d pt, int digs = -1, double snap = 5, int shft = 0)
    //{        
    //    UOR uor = new UOR(App.ActiveModelReference);
    //    snap /= uor.activeSubPerMaster;

    //    BCOM.Point3d res;
    //    res.X = roundExt_Old(pt.X, digs, snap, shft);
    //    res.Y = roundExt_Old(pt.Y, digs, snap, shft);
    //    res.Z = roundExt_Old(pt.Z, digs, snap, shft);
    //    return res;
    //}

    public static double roundExt(double val, double snap)
    {
        return Convert.ToInt64(roundEx_(val, snap));
    }

    public static BCOM.Point3d roundExt(BCOM.Point3d pt, double snap)
    {        
        BCOM.Point3d res;
        res.X = roundEx_(pt.X, snap);
        res.Y = roundEx_(pt.Y, snap);
        res.Z = roundEx_(pt.Z, snap);
        return res;
    }

    private static double roundEx_(double value, double step)
    {
       if (value >= 0)
            return Math.Floor((value + step / 2) / step) * step;
        else
           return Math.Ceiling((value - step / 2) / step) * step;
    }
    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
