﻿using System;
using System.Collections.Generic;
using System.Reflection;
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

    public static double getDistanceToPlane(this BCOM.Point3d point, BCOM.Plane3d plane)
    {
        BCOM.Point3d projPoint = App.Point3dProjectToPlane3d(point, plane);
        return App.Point3dDistance(point, projPoint);
    }

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

    private static string toStringPointDelimeter(double value) => 
        value.ToString().Replace(',','.');

    public static string ToStringEx(this BCOM.Point3d point)
    {
        return $"{toStringPointDelimeter(point.X)}, {toStringPointDelimeter(point.Y)}, {toStringPointDelimeter(point.Z)}";
    }

    public static bool EqualsPoint(this BCOM.Point3d lhs, BCOM.Point3d rhs)
    {
        return lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z;
    }

    public static void SetByLevel(this BCOM.Settings settings, BCOM.Level level)
    {
        settings.Level = level;
        settings.LineStyle = level.ElementLineStyle;
        settings.LineWeight = level.ElementLineWeight;
        settings.Color = level.ElementColor;
    }

    public static IEnumerable<BCOM.Element> GetProjectionElements(
        this TFCOM.TFFrame frame)
    {
        var elements = new List<BCOM.Element>();

        var projList = frame.GetProjectionList();
        while (projList != null)
        {
            BCOM.Element element;
            projList.AsTFProjection.GetElement(out element);
            if (element != null)
            {
                elements.Add(element);
            }
            projList = projList.GetNext();
        }

        return elements;
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

    public static BCOM.Element ToElementCOM(this Element element)
    {
        long id;
        BCOM.ModelReference modelRef;

        #if CONNECT
            id = element.ElementId;
            modelRef = App.MdlGetModelReferenceFromModelRefP(
                element.GetNativeDgnModelRef());
        #elif V8i
            id = element.ElementID;
            modelRef = App.MdlGetModelReferenceFromModelRefP(
                (int)element.ModelRef);
        #endif

        return modelRef.GetElementByID(id);
    }

    public static BCOM.ModelReference MdlGetModelReferenceFromModelRefP(this BCOM.Application app, IntPtr nativeModelRefP)
    {
        return app.MdlGetModelReferenceFromModelRefP((long)nativeModelRefP);
    }

    public static BCOM.CellElement ToCellElementCOM(this Element element)
    {
        return element.ToElementCOM().AsCellElement();
    }

    public static Element ToElement(this BCOM.Element bcomElement)
    {
    #if CONNECT        
        return Element.GetFromElementRefAndModelRef(
            (IntPtr)bcomElement.MdlElementRef(), (IntPtr)bcomElement.ModelReference.MdlModelRefP());
    #elif V8i
        return Element.ElementFactory(
            (IntPtr)bcomElement.MdlElementRef(), 
            (IntPtr)bcomElement.ModelReference.MdlModelRefP()
        );
    #endif

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
        AddToMessageCenter(ex, true);
    }

    public static void AddToMessageCenter(this Exception ex, bool openAlertDialog = false) // TODO   
    {
        string progName = Assembly.GetExecutingAssembly().GetName().Name;
        MessageCenter.AddMessage($"{progName}: {ex.Message}", ex.StackTrace, BCOM.MsdMessageCenterPriority.Error, openAlertDialog);
    }

    public static void AddInfoToMessageCenter(string text, bool openAlertDialog = false) // TODO   
    {
        string progName = Assembly.GetExecutingAssembly().GetName().Name;
        MessageCenter.AddMessage($"{progName}:\n{text}", text, BCOM.MsdMessageCenterPriority.Info, openAlertDialog);
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

    public static double getHeight(this BCOM.Range3d range)
    {
        return (range.High.Y - range.Low.Y);
    }

    public static double getWidth(this BCOM.Range3d range)
    {
        return (range.High.X - range.Low.X);
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
