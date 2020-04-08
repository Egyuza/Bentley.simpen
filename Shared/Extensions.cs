using System;

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

namespace Shared
{
static class Extensions
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

    public static void ShowMessage(this Exception ex)
    {
        MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static double ToDouble(this float value)
    {
        return floatToDouble(value);
    }

    public static double floatToDouble(float value)
    {
        // для точного перевода в double, 
        // в противном случае, например,
        // число 168,3 может получить хвост => 168,300000305175781
        return (double)(decimal)value;
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
