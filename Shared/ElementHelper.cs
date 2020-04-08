using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using Bentley.MicroStation.XmlInstanceApi;
#endif

#if CONNECT
using System.Linq;
using System.Xml.Linq;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET.Elements;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.EC.Persistence.Query;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.Instance;
#endif

using Shared.sp3d;

namespace Shared
{
class ElementHelper
{
#if V8i
    public static bool isElementSp3dTask(Element element, out Sp3dTask task)
    {
        P3DHangerPipeSupport pipe = null;
        P3DHangerStdComponent component = null;
        P3DEquipment equipment = null;
        task = null;
        
        if (element == null)
            return false;

        XmlInstanceSchemaManager modelSchema =
            new XmlInstanceSchemaManager(element.ModelRef);
        
        XmlInstanceApi api = XmlInstanceApi.CreateApi(modelSchema);
        IList<string> instances = api.ReadInstances(element.ElementRef);

        foreach (string inst in instances)
        {
            if (inst.StartsWith("<P3DEquipment"))
            {
                string xmlData = Regex.Replace(inst, " xmlns=\"[^\"]+\"", "");  
                equipment = XmlSerializer.FromXml<P3DEquipment>(xmlData);
                break;
            }

            string instId = XmlInstanceApi.GetInstanceIdFromXmlInstance(inst);
            IList<string> relations = api.ReadRelationshipInstances(instId);

            foreach (string relation in relations)
            {
                string sourceId, targetId;
                XmlInstanceApi.GetInstanceIdsFromXmlRelationshipInstance(
                    relation, out sourceId, out targetId);

                string sourceInst = api.ReadInstance(sourceId);
                string targetInst = api.ReadInstance(targetId);
                
                foreach (string subInst in new string[] {sourceInst, targetInst})
                {
                    //! если не удалить xmlns, то получим ошибку                        
                    // ~ "not absolut xmlns path"
                    string xmlData =
                        Regex.Replace(subInst, " xmlns=\"[^\"]+\"", "");                    

                    try
                    {
                        if (xmlData.StartsWith("<P3DHangerPipeSupport"))
                        {
                            pipe = XmlSerializer.FromXml
                                <P3DHangerPipeSupport>(xmlData);
                        }
                        else if (xmlData.StartsWith("<P3DHangerStdComponent"))
                        {        
                            component = XmlSerializer.FromXml
                                <P3DHangerStdComponent>(xmlData);
                        }
                    }
                    catch (Exception)
                    {
                        throw; // todo обработать
                    }
                }

            }
        }   
        
        if (equipment != null)
        {
            task = new Sp3dTask(equipment);
        }
        else if (pipe != null && component != null)
        {
            task = new Sp3dTask(pipe, component);
        }

        return task != null;
    }

    public static void extractFromElement(Element element, out long id,
        out IntPtr elemRef, out IntPtr modelRef)
    {
        id = element.ElementID;
        elemRef = element.ElementRef;
        modelRef = element.ModelRef;
    }

    public static Element getElement(IntPtr elemRef, IntPtr modelRef)
    {
        return Element.ElementFactory(elemRef, modelRef);
    }

    public static Element getElement(BCOM.Element bcomElement)
    {
         return Element.ElementFactory((IntPtr)bcomElement.MdlElementRef(), 
        (IntPtr)bcomElement.ModelReference.MdlModelRefP());
    }

    public static Element getElement(AddIn.SelectionChangedEventArgs eventArgs)
    {
        return getElement(getElementCOM(eventArgs));
    }

    public static BCOM.Element getElementCOM(Element element)
    {
        var modelRef = App.MdlGetModelReferenceFromModelRefP((int)element.ModelRef);
        return modelRef.GetElementByID(element.ElementID);
    }

    public static BCOM.Element getElementCOM(IntPtr elemRef, IntPtr modelRef)
    {
        return getElementCOM(getElement(elemRef, modelRef));
    }

    public static BCOM.Element getElementCOM(AddIn.SelectionChangedEventArgs eventArgs)
    {
        var activeModel = App.MdlGetModelReferenceFromModelRefP(
        (int)eventArgs.ModelReference.DgnModelRefIntPtr);

        var cache = activeModel.ElementCacheContainingFilePosition(
            (int)eventArgs.FilePosition);

        int index = cache.IndexFromFilePosition((int)eventArgs.FilePosition);
        return cache.GetElement(index);
    }


#endif

#if CONNECT
    private static string getXmlFormECInstance(IECInstance ecInst)
    {
        string nameSpace = ecInst.ClassDefinition.Schema.NamespacePrefix;
        XElement el = new XElement(XName.Get(ecInst.ClassDefinition.Name,
            nameSpace));

        foreach (var prop in ecInst.ClassDefinition)
        {
            var propValue = ecInst.FindPropertyValue(prop.Name, false, false,false, true);

            if (propValue != null)
            {
                XElement subEl = new XElement(XName.Get(propValue?.AccessString, nameSpace));
                subEl.Value = propValue.XmlStringValue;
                el.Add(subEl);
            }
        }
        return el.ToString();
    }

    public static bool isElementSp3dTask(Element element, out Sp3dTask task)
    {
        P3DHangerPipeSupport pipe = null;
        P3DHangerStdComponent component = null;
        task = null;

        var manager = DgnECManager.Manager;
        manager.ActivateDgnECEvents();

        using (DgnECInstanceCollection ecInstances =
            manager.GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses))
        {
            foreach (IDgnECInstance inst in ecInstances)
            {
                if (inst.ClassDefinition.Name != "P3DHangerStdComponent")
                {
                    continue;
                }

                //! если не удалить xmlns, то получим ошибку
                // ~ "not absolut xmlns path"
                string xmlData = Regex.Replace(getXmlFormECInstance(inst),
                    " xmlns=\"[^\"]+\"", "");
                component = XmlSerializer.FromXml<P3DHangerStdComponent>(xmlData);

                inst.SelectClause = inst.SelectClause ?? new SelectCriteria();
                inst.SelectClause.SelectAllProperties = true;
                inst.SelectClause.SelectDistinctValues = true;
                DgnSelectAllRelationshipsAccessor.SetIn(inst.SelectClause, true);

                foreach (IECRelationshipInstance relInst in inst.GetRelationshipInstances())
                {
                    var refInst = (IDgnECInstance)relInst.Source;

                    if (relInst.Source.ClassDefinition.Name == "P3DHangerPipeSupport")
                    {
                        FindInstancesScopeOption option = new FindInstancesScopeOption(DgnECHostType.Element);                          
                        FindInstancesScope scope = 
                            FindInstancesScope.CreateScope(refInst.Element, option);
                        
                        IECSchema schema = relInst.Source.ClassDefinition.Schema;

                        var query = QueryHelper.CreateQuery(schema, true, 
                            relInst.Source.ClassDefinition.Name);
                        query.SelectClause.SelectAllProperties = true;
                        query.SelectClause.SelectDistinctValues = true;
                        
                        var findInsts = manager.FindInstances(scope, query);
                        if (findInsts.Count() > 0)
                        {
                            //! если не удалить xmlns, то получим ошибку
                            // ~ "not absolut xmlns path"
                            xmlData = Regex.Replace(
                                getXmlFormECInstance(findInsts.First()), 
                                " xmlns=\"[^\"]+\"", "");
                            pipe = XmlSerializer.FromXml<P3DHangerPipeSupport>(xmlData);
                        }
                    }
                }
            }
        }         

        if (pipe != null && component != null)
        {
            task = new Sp3dTask(pipe, component);
        }

        return task != null;
    }

    public static void extractFromElement(Element element, out long id,
        out IntPtr elemRef, out IntPtr modelRef)
    {
        id = element.ElementId;
        elemRef = element.GetNativeElementRef();
        modelRef = element.GetNativeDgnModelRef();
    }

    public static Element getElement(IntPtr elemRef, IntPtr modelRef)
    {
        return Element.GetFromElementRefAndModelRef(elemRef, modelRef);
    }

    public static Element getElement(BCOM.Element bcomElement)
    {
        return Element.GetFromElementRefAndModelRef((IntPtr)bcomElement.MdlElementRef(), 
            (IntPtr)bcomElement.ModelReference.MdlModelRefP());
    }

    public static BCOM.Element getElementCOM(IntPtr elemRef, IntPtr modelRef)
    {
        var elem = Element.GetFromElementRefAndModelRef(elemRef, modelRef);
        return getElementCOM(elem);
    }

    public static BCOM.Element getElementCOM(Element element)
    {
        var modelRef = App.MdlGetModelReferenceFromModelRefP(
            (long)element.GetNativeDgnModelRef());
        return modelRef.GetElementByID(element.ElementId);
    }
#endif

    public static BCOM.LineElement createCrossRound(
        ref  BCOM.Point3d origin, double diameter)
    {
        BCOM.Point3d[] verts = { origin, origin, origin, origin, origin };
        double k = diameter/2 * Math.Cos(Math.PI/4);
        verts[0].X = -k;
        verts[0].Y = -k;
        verts[1].X = k;
        verts[1].Y = k;
        verts[2] = origin;
        verts[3] = verts[0];
        verts[3].Y *= -1;
        verts[4] = verts[1];
        verts[4].Y *= -1;
        return App.CreateLineElement1(null, verts);
    }

    public static BCOM.ArcElement createCircle(
        ref  BCOM.Point3d origin, double diameter)
    {
        var rot = App.Matrix3dIdentity();        
        //return App.CreateEllipseElement2(null, origin, 
        //    diameter/2, diameter/2, rot, BCOM.MsdFillMode.NotFilled);  
            
       return App.CreateArcElement2(null, origin, diameter/2, diameter/2, 
        rot, 0, Math.PI *2);
    }



    public static BCOM.LineElement getElementRangeBox(BCOM.Element el)
    {
        BCOM.View view = ViewHelper.getActiveView();

        BCOM.Point3d[] verts = new BCOM.Point3d[16];
        verts[0] = el.Range.Low;
        verts[1] = verts[0];
        verts[1].X = el.Range.High.X;
        verts[2] = verts[1];
        verts[2].Y = el.Range.High.Y;
        verts[3] = verts[2];
        verts[3].X = el.Range.Low.X;
        verts[4] =
        verts[5] = verts[0];
        verts[5].Z = el.Range.High.Z;
        verts[6] = verts[5];
        verts[6].Y = el.Range.High.Y;
        verts[7] = verts[6];
        verts[7].X = el.Range.High.X;
        verts[8] = verts[7];
        verts[8].Y = el.Range.Low.Y;
        verts[9] = verts[5];
        verts[10] = verts[6];
        verts[11] = verts[3];
        verts[12] = verts[2];
        verts[13] = verts[7];
        verts[14] = verts[8];
        verts[15] = verts[1];

        return App.CreateLineElement1(null, verts);
    }

    public static BCOM.Point3d getMiddlePoint(BCOM.Point3d first, BCOM.Point3d second)
    {
        BCOM.Vector3d vec = 
            App.Vector3dSubtractPoint3dPoint3d(second, first);

        return App.Point3dAddScaledVector3d(first, vec, 0.5);
    }

    public static double getHeight(BCOM.ConeElement cone)
    {
        return App.Point3dDistance(
            cone.get_BaseCenterPoint(), cone.get_TopCenterPoint());
    }

    public static BCOM.Point3d getCenter(BCOM.ConeElement cone)
    {
        return getMiddlePoint(
            cone.get_BaseCenterPoint(), cone.get_TopCenterPoint());
    }

    public static BCOM.Point3d getCenter(BCOM.Element element)
    {
        return getMiddlePoint(element.Range.Low,element.Range.High);
    }

    public static BCOM.Level getOrCreateLevel(string name)
    {
        BCOM.Levels levels = App.ActiveDesignFile.Levels;
        levels.IncludeHidden = true;

        try
        {
            return levels[name];
        }
        catch (Exception)
        {
            return App.ActiveDesignFile.AddNewLevel(name);
        }
    }

    //public static BCOM.Level getLevel(string name)
    //{
    //    BCOM.Levels levels = App.ActiveDesignFile.Levels;

    //    var activeModel =  App.ActiveModelReference;   
    //    activeModel.Levels.IncludeHidden = true;

    //    try
    //    {
    //        return activeModel.Levels[name];
    //    }
    //    catch (Exception) {} // не найден


    //    BCOM.Level activeLevel = App.ActiveSettings.Level;

    //    try
    //    {
    //        Addin.Instance.sendKeyin(
    //            string.Format("level set active {0}", name));

    //        activeModel.Levels.IncludeHidden = true;
    //        return activeModel.Levels[name];
    //    }
    //    catch (Exception) 
    //    {
    //        return null;
    //    }
    //    finally
    //    {
    //        App.ActiveSettings.Level = activeLevel;
    //    }
    //}

    //public static BCOM.Level getLevelOrActive(string name)
    //{
    //    return getLevel(name) ?? App.ActiveSettings.Level;
    //}

    public static void setSymbologyByLevel(BCOM.Element element)
    {
        element.LineStyle = App.ByLevelLineStyle();
        element.LineWeight = App.ByLevelLineWeight();
        element.Color = App.ByLevelColor();
    }
    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}