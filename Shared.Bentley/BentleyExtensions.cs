using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
using Bentley.Internal.MicroStation.Elements;
#endif

#if CONNECT
using Bentley.DgnPlatformNET;
using Bentley.GeometryNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
#endif

namespace Shared.Bentley
{
public static class BentleyExtensions
{

    public static BCOM.Element createPointElement(this BCOM.Application app)
    {
        var zero = app.Point3dZero();
        return app.CreateLineElement2(null, zero, zero);
    }


#if V8i
    public static bool IsAttachmentOf(this BCOM.ModelReference model, 
        BCOM.ModelReference owner)
    {
        return model.AsAttachment(owner) != null;
    }

    public static BCOM.Attachment AsAttachment(this BCOM.ModelReference model, 
        BCOM.ModelReference owner = null)
    {
        owner = owner ?? App.ActiveModelReference;

        foreach (BCOM.Attachment attach in owner.Attachments)
        {
            // равенство работает только для версии V8i
            if (attach.MdlModelRefP() == model.MdlModelRefP())
                return attach;
        }
        return null;
    }

#elif CONNECT
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

    public static DgnModelRef AsDgnModelRef(this BCOM.ModelReference model)
    {
        return DgnModel.GetModelRef((IntPtr)model.MdlModelRefP());
    }
        

    public static bool IsDgnAttachmentOf(this DgnModel model, DgnModel owner)
    {
        return model.AsDgnAttachmentOf(owner) != null;
    }

    public static BCOM.Attachment AsAttachment(this BCOM.ModelReference model, 
        BCOM.ModelReference owner = null)
    {
        owner = owner ?? App.ActiveModelReference;

        foreach (BCOM.Attachment attach in owner.Attachments)
        {
            //if (attach.IsMissingFile || attach.IsMissingModel)
            //    continue;
            try 
            {
                // attach.DesignFile в некоторых случаях выбрасывает ошибку
                // attach.IsMissingFile - не помогает
                if (attach.DesignFile.MdlModelRefP() == model.MdlModelRefP())
                {
                    return attach;
                }
            }
            catch (Exception) {}
        }
        return null;
    }

    public static bool IsAttachmentOf(this BCOM.ModelReference model,
        BCOM.ModelReference owner)
    {
        return model.AsDgnModelRef().AsDgnModel().IsDgnAttachmentOf(
            owner.AsDgnModelRef().AsDgnModel());
    }

    public static DMatrix3d ToDMatrix3d(this BCOM.Matrix3d rotation)
    {
        return DMatrix3d.FromRows(rotation.RowX.ToDVector(), 
            rotation.RowY.ToDVector(), rotation.RowZ.ToDVector());
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

    public static BCOM.CellElement AsCellElementCOM(this Element element)
    {
        long id;
        #if CONNECT
            id = element.ElementId;
        #elif V8i
            id = element.ElementID;
        #endif

        return App.ActiveModelReference.GetElementByID(id).AsCellElement();
    }

    public static bool IsCompundCell(this BCOM.Element element)
    {
        TFCOM.TFElementList tfList = AppTF.CreateTFElement();
        tfList.InitFromElement(element);

        if (tfList.AsTFElement == null)
            return false;

        int tfType = tfList.AsTFElement.GetApplicationType();

        return tfList.AsTFElement.GetIsCompoundCellType();
    }

    public static IEnumerable<T> getSubElementsRecurse<T>(
        this BCOM.CellElement cell) where T : BCOM.Element
    {
        var res =  new List<T>();
        getSubElements_(cell, res);
        return res;
    }
    private static void getSubElements_<T>(
        BCOM.CellElement cell, List<T> coll) where T : BCOM.Element
    {
        BCOM.ElementEnumerator iter = cell.GetSubElements();
        while (iter.MoveNext())
        {
            if (iter.Current is T)
            {
                coll.Add((T)iter.Current);
            }
            else if (iter.Current.IsCellElement())
            {
                getSubElements_<T>(iter.Current.AsCellElement(), coll);
            }
        }
    }

    /// <summary> Оповещение - вывод текста ошибки </summary>
    public static void Alert(this Exception ex) // TODO   
    {
        #if CONNECT
            // MessageCenter.Instance.ShowErrorMessage(ex.Message, ex.StackTrace, true);
        #else  
            // TODO          
        #endif
        MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        
    }
    /// <summary> Оповещение - вывод текста ошибки </summary>
    public static void AlertIfDebug(this Exception ex)
    {
        #if DEBUG
            Alert(ex);
        #endif
    }

    public static void AlertIfDebug(string errMessage, string details = null)
    {
        #if DEBUG
            MessageCenter.AddMessage(
                errMessage, details, BCOM.MsdMessageCenterPriority.Error, true);
        #endif
    }

    /// <summary> Оповещение - вывод текста ошибки </summary>
    public static void Alert(string text)
    {
        // todo !  в MessageCenter?
        #if CONNECT
            NotificationManager.OpenMessageBox(
                NotificationManager.MessageBoxType.Ok, text, 
                NotificationManager.MessageBoxIconType.Critical);
        #elif V8i
            MessageBox.Show(text, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
        #endif
    }

    private static BCOM.MessageCenter MessageCenter => App.MessageCenter;

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

    private static TFCOM.TFApplication _tfApp;
    private static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    } 
}
}
