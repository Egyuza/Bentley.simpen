using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared.Bentley
{
static class BentleyExtensions
{

#if V8i

#endif

#if CONNECT
    public static DPoint3d ToDPoint(this BCOM.Point3d pt)
    {
        return DPoint3d.FromXYZ(pt.X, pt.Y, pt.Z);
    }
    public static DVector3d ToDVector(this BCOM.Point3d pt)
    {
        return DVector3d.FromXYZ(pt.X, pt.Y, pt.Z);
    }

    public static DgnAttachment AsDgnAttachmentOf(this DgnModel model, DgnModel owner)
    {
        if (owner == null || model == owner)
            return null;

        foreach (DgnAttachment attach in owner.GetDgnAttachments())
        {
            if (attach.GetDgnModel() == model)
                return attach;
        }
        return null;
    }

    public static bool IsDgnAttachmentOf(this DgnModel model, DgnModel owner)
    {
        return model.AsDgnAttachmentOf(owner) != null;
    }
#endif

    public static string ToStringEx(this BCOM.Point3d point)
    {
        return string.Format("{0}, {1}, {2}", 
        Math.Round(point.X, 0), Math.Round(point.Y, 0), Math.Round(point.Z, 0));
    }

    public static BCOM.Point3d shift(this BCOM.Point3d p3d, 
        double dX = 0.0, double dY = 0.0, double dZ = 0.0)
    {
        BCOM.Point3d pt = App.Point3dZero();
        pt.X = p3d.X + dX;
        pt.Y = p3d.Y + dY;
        pt.Z = p3d.Z + dZ;
        return pt;
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
