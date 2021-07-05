using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared;
using System.Linq;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using Bentley.MicroStation.XmlInstanceApi;
using System.Xml.Linq;

#elif CONNECT
using System.Linq;
using System.Xml.Linq;
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET.Elements;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.EC.Persistence.Query;
using Bentley.ECObjects.Schema;
using Bentley.ECObjects.Instance;
#endif


namespace Shared.Bentley
{

public class test
{
    public List<string> Values { get; set; } = new List<string>() {"1", "2", "3"};
    public Dictionary<string,string> ValueMap { get; set; } =
         new Dictionary<string, string>() { { "1", "First"}, { "2", "Second"},};
}


public static class ElementHelper
{
    public static XDocument getSp3dXDocument(Element element, 
        bool includeRelations = true)
    {
        var XmlDoc = XDocument.Parse("<Root></Root>");
        var xmlData = getSp3dXmlData(element, includeRelations);

        if (xmlData == null)
            return null;
        foreach (string xmlText in xmlData)
        {
            XmlDoc.Root.Add(XElement.Parse(xmlText));
        }
        return XmlDoc;
    }

    public static void Merge(this XDocument xDoc, 
        XDocument other, string keyTagName = "UID")
    {
        foreach(XElement targetNode in xDoc.Root.Elements())
        {
            var relTag = targetNode.GetChild(keyTagName);
            if (relTag == null)
                continue;

            string relTagValue = relTag.Value.Trim();

            XElement additionalNode = other?.Root.Elements().FirstOrDefault(x => 
                x.GetChild(keyTagName) != null &&
                x.GetChild(keyTagName).Value.Contains(relTagValue)
            );

            if (additionalNode != null)
            {
                foreach(XElement subNode in additionalNode.Elements().ToList())
                {
                    XElement matchNode = targetNode.GetChild(subNode.Name.LocalName);
                    if (matchNode != null)
                    {
                        // TODO расскоментировать, если верно:
                        // matchNode.ReplaceWith(node);
                    }
                    else
                    {
                        targetNode.Add(subNode);
                    }
                }
            }
        }
    }


#if V8i
    public static IEnumerable<string> getSp3dXmlData(Element element, bool includeRelations = true)
    {
        var summary = new HashSet<string>();
        var modelSchema = new XmlInstanceSchemaManager(element.ModelRef);
        
        XmlInstanceApi api = XmlInstanceApi.CreateApi(modelSchema);
        IList<string> instances = api.ReadInstances(element.ElementRef);

        foreach (string inst in instances)
        {
#if DEBUG
            //geSummaryXmlRecurse_(api, inst, ref summary, includeRelations);
            //continue;
#endif

            summary.Add(prepair_(inst));

            if (!includeRelations)
                continue;

            string instId = XmlInstanceApi.GetInstanceIdFromXmlInstance(inst);
            IList<string> relations = api.ReadRelationshipInstances(instId);

            foreach (string relation in relations)
            {
                string sourceId, targetId;
                XmlInstanceApi.GetInstanceIdsFromXmlRelationshipInstance(
                    relation, out sourceId, out targetId);

                string sourceInst = api.ReadInstance(sourceId);
                string targetInst = api.ReadInstance(targetId);

                foreach (string subInst in new string[] { sourceInst, targetInst })
                {
                    summary.Add(prepair_(subInst));
                }
            }
        }
        return summary;
    }

    private static void geSummaryXmlRecurse_(XmlInstanceApi api, string instance, ref HashSet<string> summary, bool includeRelations = true)
    {
        summary.Add(prepair_(instance));

        if (!includeRelations)
            return;

        string instId = XmlInstanceApi.GetInstanceIdFromXmlInstance(instance);
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
                string xmlData = prepair_(subInst);
                if (!summary.Contains(xmlData))
                {
                    summary.Add(xmlData);
                    geSummaryXmlRecurse_(api, subInst, ref summary, includeRelations);
                }
            }
        }
    }

    private static string prepair_(string xmlInstance)
    {
        //! если не удалить xmlns, то получим ошибку                        
        // ~ "not absolut xmlns path"
        return Regex.Replace(xmlInstance, " xmlns=\"[^\"]+\"", "");
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
                try
                {
                    subEl.Value = propValue.XmlStringValue;
                }
                catch (Exception) {}
                el.Add(subEl);
            }
        }
        return el.ToString();
    }

    public static IEnumerable<string> getSp3dXmlData(Element element, bool includeRelations = true)
    {
        var summary = new HashSet<string>();

        var manager = DgnECManager.Manager;
        manager.ActivateDgnECEvents();

        using (DgnECInstanceCollection ecInstances =
            manager.GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses))
        {
            foreach (IDgnECInstance inst in ecInstances)
            {
                //! если не удалить xmlns, то получим ошибку
                // ~ "not absolut xmlns path"
                string xmlData = Regex.Replace(getXmlFormECInstance(inst),
                    " xmlns=\"[^\"]+\"", "");
                summary.Add(xmlData);

                if (!includeRelations)
                    continue;

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
                            summary.Add(xmlData); 
                        }
                    }
                }

            }
        }

        return summary;
    }


    


    //public static bool isElementSp3dTask(Element element, out Sp3dTask_Old task)
    //{
    //    P3DHangerPipeSupport pipe = null;
    //    P3DHangerStdComponent component = null;
    //    task = null;

    //    var manager = DgnECManager.Manager;
    //    manager.ActivateDgnECEvents();

    //    using (DgnECInstanceCollection ecInstances =
    //        manager.GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses))
    //    {
    //        foreach (IDgnECInstance inst in ecInstances)
    //        {
    //            if (inst.ClassDefinition.Name != "P3DHangerStdComponent")
    //            {
    //                continue;
    //            }

    //            //! если не удалить xmlns, то получим ошибку
    //            // ~ "not absolut xmlns path"
    //            string xmlData = Regex.Replace(getXmlFormECInstance(inst),
    //                " xmlns=\"[^\"]+\"", "");
    //            component = XmlSerializerEx.FromXml<P3DHangerStdComponent>(xmlData);

    //            inst.SelectClause = inst.SelectClause ?? new SelectCriteria();
    //            inst.SelectClause.SelectAllProperties = true;
    //            inst.SelectClause.SelectDistinctValues = true;
    //            DgnSelectAllRelationshipsAccessor.SetIn(inst.SelectClause, true);

    //            foreach (IECRelationshipInstance relInst in inst.GetRelationshipInstances())
    //            {
    //                var refInst = (IDgnECInstance)relInst.Source;

    //                if (relInst.Source.ClassDefinition.Name == "P3DHangerPipeSupport")
    //                {
    //                    FindInstancesScopeOption option = new FindInstancesScopeOption(DgnECHostType.Element);                          
    //                    FindInstancesScope scope = 
    //                        FindInstancesScope.CreateScope(refInst.Element, option);
                        
    //                    IECSchema schema = relInst.Source.ClassDefinition.Schema;

    //                    var query = QueryHelper.CreateQuery(schema, true, 
    //                        relInst.Source.ClassDefinition.Name);
    //                    query.SelectClause.SelectAllProperties = true;
    //                    query.SelectClause.SelectDistinctValues = true;
                        
    //                    var findInsts = manager.FindInstances(scope, query);
    //                    if (findInsts.Count() > 0)
    //                    {
    //                        //! если не удалить xmlns, то получим ошибку
    //                        // ~ "not absolut xmlns path"
    //                        xmlData = Regex.Replace(
    //                            getXmlFormECInstance(findInsts.First()), 
    //                            " xmlns=\"[^\"]+\"", "");
    //                        pipe = XmlSerializerEx.FromXml<P3DHangerPipeSupport>(xmlData);
    //                    }
    //                }
    //            }
    //        }
    //    }         

    //    if (pipe != null && component != null)
    //    {
    //        task = new Sp3dTask_Old(pipe, component);
    //    }

    //    return task != null;
    //}


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

    public static BCOM.Element getElementCOM(this Element element)
    {
        var modelRef = App.MdlGetModelReferenceFromModelRefP(
            (long)element.GetNativeDgnModelRef());
        return modelRef.GetElementByID(element.ElementId);
    }
#endif

    public static BCOM.Point3d? GetPoint3d(string coords)
    {
        BCOM.Point3d pt;

        if (getPoint3d(coords, out pt))
            return pt;

        return null;
    }

    public static bool getPoint3d(string coords, out BCOM.Point3d pt)
    {
        pt = App.Point3dZero();

        if (string.IsNullOrEmpty(coords))
            return false;

        string[] sval = coords.Split(',');

        try
        {
            pt = App.Point3dFromXYZ(
                sval[0].ToDouble(), sval[1].ToDouble(), sval[2].ToDouble());
            return true;
        }
        catch (Exception) {}
        {       
        }

        return false;
    }

    public static BCOM.Element createPoint(BCOM.Point3d? origin = null)
    {
        var center = origin.HasValue ? origin.Value : App.Point3dZero();
        return App.CreateLineElement2(null, center, center);
    }

    public static BCOM.LineElement createCrossRound(
        double diameter, BCOM.Point3d? origin = null)
    {
        BCOM.Point3d center = 
            origin.HasValue ? origin.Value : App.Point3dZero();

        BCOM.Point3d[] verts = { center, center, center, center, center };
        double k = diameter/2 * Math.Cos(Math.PI/4);

        verts[0].X -= k;
        verts[0].Y -= k;
        verts[1].X += k;
        verts[1].Y += k;
        verts[2] = center;
        verts[3] = verts[0];
        verts[3].Y += 2*k;
        verts[4] = verts[1];
        verts[4].Y -= 2*k;

        return App.CreateLineElement1(null, verts);
    }

    public static BCOM.LineElement createCrossInContour(
        BCOM.VertexList contour)
    {
        BCOM.Point3d[] shapeVerts = contour.GetVertices();
        var shape = App.CreateShapeElement1(null, shapeVerts);
        BCOM.Point3d centroid = shape.Centroid();

        var points = new List<BCOM.Point3d>();

        foreach (BCOM.Point3d vert in shapeVerts)
        {
            points.Add(vert);
            points.Add(centroid);
        }

        return App.CreateLineElement1(null, points.ToArray());
    }

    public static BCOM.ArcElement createCircle(
        double diameter, BCOM.Point3d? origin = null)
    {
        BCOM.Point3d center = 
            origin.HasValue ? origin.Value : App.Point3dZero();

        var rot = App.Matrix3dIdentity();        
        //return App.CreateEllipseElement2(null, origin, 
        //    diameter/2, diameter/2, rot, BCOM.MsdFillMode.NotFilled);  
            
       return App.CreateArcElement2(null, center, diameter/2, diameter/2, 
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

    public static BCOM.Level GetOrCreateLevel(string name)
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

    public static BCOM.TextStyle getTextStyle(string name)
    {
        BCOM.TextStyles styles = App.ActiveDesignFile.TextStyles;
        
        try
        {
            return styles[name];
        }
        catch (Exception)
        {
            return App.ActiveSettings.TextStyle;
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


    /// <summary>
    /// Поиск пересечений с элементами по диапазону заданного элемента в
    /// в пространстве заданной модели
    /// </summary>
    public static IEnumerable<BCOM.Element> scanIntersectsInElementRange(
        this BCOM.Element element, BCOM.ModelReference model = null)
    {
        model = model ?? App.ActiveModelReference;

        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.Range3d scanRange = getElementScanRange(element, model);

        criteria.IncludeOnlyWithinRange(scanRange);
        return model.Scan(criteria).BuildArrayFromContents();
    }

    public static BCOM.Range3d getElementScanRange(
        BCOM.Element element, BCOM.ModelReference model = null)
    {
        BCOM.Range3d scanRange = element.Range;
        
#if CONNECT
        // корректировака для версии CONNECT
        if (element.ModelReference.IsAttachmentOf(model))
        {
            // здесь есть различия с V8i: // TODO проверить
            double k = model.UORsPerStorageUnit / 
                element.ModelReference.UORsPerStorageUnit;
            scanRange.High = App.Point3dScale(scanRange.High, k);            
            scanRange.Low = App.Point3dScale(scanRange.Low, k);
        }
#endif
        return scanRange;
    }

    public static void setSymbologyByLevel(BCOM.Element element) // legacy
    {
        bool dirty = false;
        setSymbologyByLevel(element, ref dirty);
    }

    public static void setSymbologyByLevel(BCOM.Element element, ref bool dirty)
    {
        if (element.LineStyle != App.ByLevelLineStyle())
        {
            dirty = true;
            element.LineStyle = App.ByLevelLineStyle();
        }
        if (element.LineWeight != App.ByLevelLineWeight())
        {
            dirty = true;
            element.LineWeight = App.ByLevelLineWeight();
        }
        if (element.Color != App.ByLevelColor())
        {
            dirty = true;
            element.Color = App.ByLevelColor();
        }        
    }

    public static double getActiveAnnotationScale()
    {
        return App.ActiveModelReference.GetSheetDefinition().AnnotationScaleFactor;
    }

    public static bool setTagOnElement<T>(BCOM.Element element, 
        string tagSetName, string tagName, T value, BCOM.MsdTagType type = BCOM.MsdTagType.Character)
    {
        BCOM.TagElement tag = null;
        BCOM.DesignFile dgnFile = element.ModelReference.DesignFile;

        BCOM.TagSet tagSet = getTagSetOrCreate(dgnFile, tagSetName);
        
        try { tag = element.GetTag(tagSet, tagName); }
        catch (Exception) {}
        
        if (tag == null) {
            BCOM.TagDefinition tagDef = getTagDefOrCreate(tagSet, tagName, type);
            // TODO tagDef.IsHidden  = ...
            tag = element.AddTag(tagDef);
        }

        if (tag != null)
        {
            tag.Value = value;
            tag.Rewrite();
            element.Rewrite();
            return true;
        }

        return false;
    }

    public static BCOM.TagSet getTagSetOrCreate(BCOM.DesignFile dgnFile, string tagSetName) {
        try
        {
            return dgnFile.TagSets[tagSetName];
        }
        catch (Exception)
        {
            return dgnFile.TagSets.Add(tagSetName);
        }
    }

    public static BCOM.TagDefinition getTagDefOrCreate(
        BCOM.TagSet tagSet, string tagName, BCOM.MsdTagType tagType) 
    {
        try
        {
            return tagSet.TagDefinitions[tagName];
        }
        catch (Exception)
        {
            return tagSet.TagDefinitions.Add(tagName, tagType);
        }
    }

    public static bool getFacePlaneByLabel(out BCOM.Plane3d plane,
        BCOM.Element element, TFCOM.TFdFaceLabel faceLabel)
    {
        TFCOM.TFBrepList brepList;

        var formRecipeList = new TFCOM.TFFormRecipeListClass();
        formRecipeList.InitFromElement(element);

        string options = string.Empty;
        formRecipeList.GetBrepList(out brepList, false, false, false, ref options);

        return getFacePlaneByLabel(out plane, brepList, faceLabel);
    }

    public static bool getFacePlaneByLabel(out BCOM.Plane3d plane,
        TFCOM.TFBrepList brepList, TFCOM.TFdFaceLabel faceLabel)
    {
        plane = new BCOM.Plane3d();
        TFCOM.TFPlane tfPlane;

        var faceList = 
            brepList.GetFacesByLabel(faceLabel) as TFCOM.TFBrepFaceListClass;
        if (faceList != null && faceList.IsPlanar(out tfPlane))
        {
            tfPlane.GetNormal(out plane.Normal);

            {   // origin:
                BCOM.Point3d[] verts;
                faceList.AsTFBrepFace.GetVertexLocations(out verts);
                BCOM.ShapeElement shape = App.CreateShapeElement1(null, verts);

                plane.Origin = shape.Centroid();
            }
            return true;            
        }
        return false;
    }

    public static BCOM.Plane3d ToPlane3d(this TFCOM.TFPlane tfPlane)
    {
        BCOM.Plane3d plane3D;
        BCOM.Point3d nor;
        tfPlane.GetVector(out plane3D.Origin, out nor);
        tfPlane.GetNormal(out plane3D.Normal);

        return plane3D;
    }

    public static IEnumerable<TFCOM.TFBrepFaceListClass> GetFacesEx(
        this TFCOM.TFBrepList brepList)
    {
        var res = new HashSet<TFCOM.TFBrepFaceListClass>();

        var faceList = brepList.GetFaces() as TFCOM.TFBrepFaceListClass;
        if (faceList.GetCount() == 0)
            return res;

        res.Add(faceList.AsTFBrepFace as TFCOM.TFBrepFaceListClass);
        while(null != (faceList = faceList.GetNext() as TFCOM.TFBrepFaceListClass))
        {
            res.Add(faceList.AsTFBrepFace as TFCOM.TFBrepFaceListClass);
        }
        return res;
    }

    public static bool IsVectorParallelTo(this BCOM.Point3d self, BCOM.Point3d other)
    {
        return App.Vector3dAreVectorsParallel(self.ToVector3d(), other.ToVector3d());
    }

    public static bool IsParallelTo(this BCOM.Vector3d self, BCOM.Vector3d other)
    {
        return App.Vector3dAreVectorsParallel(self, other);
    }

    public static bool IsPlanarAndIntersectsElement(
        this TFCOM.TFBrepFace face, BCOM.Element element, out TFCOM.TFPlane tfPlane)
    {
        if (face.IsPlanar(out tfPlane))
        {
            //faceList.AsTFBrepFace.GetLabel(out faceLabel);
            BCOM.Point3d[] faceVerts;
            face.GetVertexLocations(out faceVerts);
            var faceShape = App.CreateShapeElement1(null, faceVerts);

            BCOM.Range3d result = new BCOM.Range3d();
            if (App.Range3dIntersect2(ref result, element.Range, faceShape.Range))
            {
                return  true;
            }
        }
        return false;
    }

    public static TFFormTypeEnum ParseFormType(int formType)
    {
        try
        {
            return (TFFormTypeEnum)formType;
        }
        catch (Exception)
        {
            return TFFormTypeEnum.UNDEFINED;
        }
    }



    public static bool GetPlane3DByPoints(out BCOM.Plane3d plane, BCOM.Point3d[] polygon)
    {
        plane = new BCOM.Plane3d();
        BCOM.ShapeElement shape = App.CreateShapeElement1(null, polygon);
        if (shape.IsPlanar)
        {
            plane.Origin = shape.Centroid();
            plane.Normal = shape.Normal;
            return true;
        }        
        return false;
    }

    public static bool IsParallelTo(this BCOM.Plane3d self, BCOM.Plane3d plane) {

        return IsPlanesAreParallel(self, plane);
    }

    public static bool IsPlanesAreParallel(BCOM.Plane3d first, BCOM.Plane3d second) {

        BCOM.Point3d secondNegateNormal = App.Point3dNegate(ref second.Normal);

        return (App.Point3dAreVectorsParallel(first.Normal, second.Normal) ||
            App.Point3dAreVectorsParallel(first.Normal, secondNegateNormal));
    }

    public static BCOM.Point3d ProjectToPlane3d(this BCOM.Point3d pt,
        BCOM.Plane3d plane)
    {
        return App.Point3dProjectToPlane3d(pt, plane, null, false);
    }


    private static BCOM.Plane3d? planeXY_;
    private static BCOM.Plane3d? planeXZ_;
    private static BCOM.Plane3d? planeYZ_;
    public static BCOM.Plane3d GetPlane3dXY(this BCOM.Application app)
    {
        if (planeXY_ == null)
        {
            planeXY_ = new BCOM.Plane3d() {
                Origin = App.Point3dZero(),
                Normal = app.Point3dFromXYZ(0, 0, 1)
            };
        }
        return planeXY_.Value;
    }
    public static BCOM.Plane3d GetPlane3dXZ(this BCOM.Application app)
    {
        if (planeXZ_ == null)
        {
            planeXZ_ = new BCOM.Plane3d() {
                Origin = App.Point3dZero(),
                Normal = app.Point3dFromXYZ(0, 1, 0)
            };
        }
        return planeXZ_.Value;
    }
    public static BCOM.Plane3d GetPlane3dYZ(this BCOM.Application app)
    {
        if (planeYZ_ == null)
        {
            planeYZ_ = new BCOM.Plane3d() {
                Origin = App.Point3dZero(),
                Normal = app.Point3dFromXYZ(1, 0, 0)
            };
        }
        return planeYZ_.Value;
    }

    public static BCOM.Point3d AddScaled(this BCOM.Point3d pt, 
        BCOM.Point3d vector, double scale)
    {
        return App.Point3dAddScaled(pt, vector, scale);
    }

    public static BCOM.Point3d Normalize(this BCOM.Point3d pt)
    {
        return App.Point3dNormalize(pt);
    }
    public static BCOM.Vector3d Normalize(this BCOM.Vector3d vec)
    {
        return App.Vector3dNormalize(vec);
    }

    public static BCOM.Vector3d ToVector3d(this BCOM.Point3d pt)
    {
        return App.Vector3dFromPoint3d(pt);
    }

    public static BCOM.Point3d ToPoint3d(this BCOM.Vector3d vec)
    {
        return App.Point3dFromVector3d(vec);
    }

    public static void RunByRecovertingSettings (Action action, params object[] args)
    {
        var activeSets = App.ActiveSettings;

        BCOM.Level activeLevel = activeSets.Level;
        BCOM.LineStyle activeLineStyle = activeSets.LineStyle;
        int activeLineWeight = activeSets.LineWeight;
        int activeColor = activeSets.Color;

        try
        {
            action?.DynamicInvoke(args);
        }
        catch (Exception ex)
        {
            ex.ShowMessageBox();
        }
        finally
        {
            activeSets.Level = activeLevel;
            activeSets.LineStyle = activeLineStyle;
            activeSets.LineWeight = activeLineWeight;
            activeSets.Color = activeColor;
        }
    }

    public static void AddProjection(this TFCOM.TFFrameList frameList, 
        BCOM.Element element, string projectionName, BCOM.Level level)
    {
        TFCOM.TFProjectionList tfProjection = AppTF.CreateTFProjection((string)null);
        tfProjection.Init();
        element.Level = level;

        ElementHelper.setSymbologyByLevel(element);
        tfProjection.AsTFProjection.SetDefinitionName(projectionName);
        tfProjection.AsTFProjection.SetEmbeddedElement(element);
        tfProjection.AsTFProjection.SetIsDoubleSided(true);

        TFCOM.TFProjectionList projectionList = frameList.AsTFFrame.GetProjectionList();
        if (projectionList == null)
        {
            frameList.AsTFFrame.SetProjectionList(tfProjection);
        }
        else
        {
            projectionList.Append(tfProjection);
        }
    }

    /// <summary>
    /// без этого кода не срабатывает перфорация в стенке/плите
    /// судя по всему инициализирует обновление объектов, с которыми
    /// взаимодействует frame
    /// </summary>
    public static void ApplyPerforatorInModel(this TFCOM.TFFrameList frameList)
    {
        AppTF.ModelReferenceUpdateAutoOpeningsByFrame(
            App.ActiveModelReference, frameList.AsTFFrame, true, false, 
            TFCOM.TFdFramePerforationPolicy.tfdFramePerforationPolicyNone);
    }

    /// <summary>
    /// Форсируем обновление для уверенности в пробитии отверстия перфоратора в стене/плите/...
    /// </summary>
    public static void ForceRedrawPerforator(this TFCOM.TFFrameList frameList)
    {
        // ! 1.предварительная трансформация "на месте"
        var tranIdentity = App.Transform3dIdentity();
        string emptyOption = string.Empty;
        frameList.Transform(tranIdentity, true, emptyOption);
        // ! 2. обновление перфоратора
        frameList.ApplyPerforatorInModel();
        // ! 3. перезапись
        AppTF.ModelReferenceRewriteFrame(App.ActiveModelReference, frameList.AsTFFrame);
    }

    public static void AddToModel(this TFCOM.TFFrameList frameList, 
        BCOM.ModelReference model = null)
    {
        model = model ?? App.ActiveModelReference;
        AppTF.ModelReferenceAddFrameList(App.ActiveModelReference, frameList);
        frameList.ForceRedrawPerforator();
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

    private static TFCOM.TFApplication _tfApp;    
    static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    }



}
}