using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Shared.Bentley
{
static class RoundTool
{
    /// <summary>
    /// Функция кратного округления из оригинального simpen Л.Вибе
    /// </summary>
    public static long roundExt(
        double val, int digs = -1, double snap = 5, int shft = 0)
    {
        double dv;
        dv = val * Math.Pow(snap, digs);
        dv = Math.Floor(dv + 0.55555555555555 - (0.111111111111111 * shft));
        dv = dv / Math.Pow(snap, digs);
        return Convert.ToInt64(dv);
    }

    public static BCOM.Point3d roundExt(
        BCOM.Point3d pt, int digs = -1, double snap = 5, int shft = 0)
    {
        BCOM.Point3d res;
        res.X = roundExt(pt.X, digs, snap, shft);
        res.Y = roundExt(pt.Y, digs, snap, shft);
        res.Z = roundExt(pt.Z, digs, snap, shft);
        return res;
    }
}
}
