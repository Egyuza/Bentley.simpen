////using Bentley.MicroStation.XmlInstanceApi;
//using System;
//using System.Xml;
//using System.Xml.Linq;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;

//using Bentley.DgnPlatformNET.Elements;

//using BCOM = Bentley.Interop.MicroStationDGN;
//using Bentley.DgnPlatformNET.DgnEC;
//using System.Linq;
//using Bentley.EC.Persistence.Query;
//using Bentley.EC.Persistence;
//using Bentley.ECObjects.Schema;
//using Bentley.ECSystem.Session;
//using ECSysRepo = Bentley.ECSystem.Repository;
//using Bentley.MstnPlatformNET;
//using Bentley.ECObjects.Instance;

//using Shared.sp3d;

//namespace simpen_cn
//{
//class ElementHelper
//{
//    public static BCOM.LineElement getElementRangeBox(BCOM.Element el)
//    {
//        BCOM.View view = ViewHelper.getActiveView();

//        BCOM.Point3d[] verts = new BCOM.Point3d[16];
//        verts[0] = el.Range.Low;
//        verts[1] = verts[0];
//        verts[1].X = el.Range.High.X;
//        verts[2] = verts[1];
//        verts[2].Y = el.Range.High.Y;
//        verts[3] = verts[2];
//        verts[3].X = el.Range.Low.X;
//        verts[4] =
//        verts[5] = verts[0];
//        verts[5].Z = el.Range.High.Z;
//        verts[6] = verts[5];
//        verts[6].Y = el.Range.High.Y;
//        verts[7] = verts[6];
//        verts[7].X = el.Range.High.X;
//        verts[8] = verts[7];
//        verts[8].Y = el.Range.Low.Y;
//        verts[9] = verts[5];
//        verts[10] = verts[6];
//        verts[11] = verts[3];
//        verts[12] = verts[2];
//        verts[13] = verts[7];
//        verts[14] = verts[8];
//        verts[15] = verts[1];

//        return Addin.App.CreateLineElement1(null, verts);
//    }

//    private static string getXmlFormECInstance(IECInstance ecInst)
//    {
//        string nameSpace = ecInst.ClassDefinition.Schema.NamespacePrefix;
//        XElement el = new XElement(XName.Get(ecInst.ClassDefinition.Name,
//            nameSpace));

//        foreach (var prop in ecInst.ClassDefinition)
//        {
//            var propValue = ecInst.FindPropertyValue(prop.Name, false, false,false, true);

//            if (propValue != null)
//            {
//                XElement subEl = new XElement(XName.Get(propValue?.AccessString, nameSpace));
//                subEl.Value = propValue.XmlStringValue;
//                el.Add(subEl);
//            }
//        }
//        return el.ToString();
//    }

//    public static bool isElementSp3dTask(Element element, out Sp3dTask task)
//    {
//        P3DHangerPipeSupport pipe = null;
//        P3DHangerStdComponent component = null;
//        task = null;

//        var manager = DgnECManager.Manager;
//        manager.ActivateDgnECEvents();

//        using (DgnECInstanceCollection ecInstances =
//            manager.GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses))
//        {
//            foreach (IDgnECInstance inst in ecInstances)
//            {
//                if (inst.ClassDefinition.Name != "P3DHangerStdComponent")
//                {
//                    continue;
//                }

//                //! если не удалить xmlns, то получим ошибку
//                // ~ "not absolut xmlns path"
//                string xmlData = Regex.Replace(getXmlFormECInstance(inst),
//                    " xmlns=\"[^\"]+\"", "");
//                component = XmlSerializer.FromXml<P3DHangerStdComponent>(xmlData);

//                inst.SelectClause = inst.SelectClause ?? new SelectCriteria();
//                inst.SelectClause.SelectAllProperties = true;
//                inst.SelectClause.SelectDistinctValues = true;
//                DgnSelectAllRelationshipsAccessor.SetIn(inst.SelectClause, true);

//                foreach (IECRelationshipInstance relInst in inst.GetRelationshipInstances())
//                {
//                    var refInst = (IDgnECInstance)relInst.Source;

//                    if (relInst.Source.ClassDefinition.Name == "P3DHangerPipeSupport")
//                    {
//                        FindInstancesScopeOption option = new FindInstancesScopeOption(DgnECHostType.Element);                          
//                        FindInstancesScope scope = 
//                            FindInstancesScope.CreateScope(refInst.Element, option);
                        
//                        IECSchema schema = relInst.Source.ClassDefinition.Schema;

//                        var query = QueryHelper.CreateQuery(schema, true, 
//                            relInst.Source.ClassDefinition.Name);
//                        query.SelectClause.SelectAllProperties = true;
//                        query.SelectClause.SelectDistinctValues = true;
                        
//                        var findInsts = manager.FindInstances(scope, query);
//                        if (findInsts.Count() > 0)
//                        {
//                            //! если не удалить xmlns, то получим ошибку
//                            // ~ "not absolut xmlns path"
//                            xmlData = Regex.Replace(
//                                getXmlFormECInstance(findInsts.First()), 
//                                " xmlns=\"[^\"]+\"", "");
//                            pipe = XmlSerializer.FromXml<P3DHangerPipeSupport>(xmlData);
//                        }
//                    }
//                }
//            }
//        }         

//        if (pipe != null && component != null)
//        {
//            task = new Sp3dTask(pipe, component);
//        }

//        return task != null;
//    }

//    public static BCOM.Point3d getMiddlePoint(BCOM.Point3d first, BCOM.Point3d second)
//    {
//        BCOM.Vector3d vec = 
//            Addin.App.Vector3dSubtractPoint3dPoint3d(second, first);

//        return Addin.App.Point3dAddScaledVector3d(first, vec, 0.5);
//    }

//    public static double getHeight(BCOM.ConeElement cone)
//    {
//        return Addin.App.Point3dDistance(
//            cone.get_BaseCenterPoint(), cone.get_TopCenterPoint());
//    }

//    public static BCOM.Point3d getCenter(BCOM.ConeElement cone)
//    {
//        return getMiddlePoint(
//            cone.get_BaseCenterPoint(), cone.get_TopCenterPoint());
//    }

//    public static BCOM.Point3d getCenter(BCOM.Element element)
//    {
//        return getMiddlePoint(element.Range.Low,element.Range.High);
//    }

//    public static BCOM.Level getOrCreateLevel(string name)
//    {
//        BCOM.Levels levels = Addin.App.ActiveDesignFile.Levels;
//        levels.IncludeHidden = true;

//        try
//        {
//            return levels[name];
//        }
//        catch (Exception) 
//        {
//            return Addin.App.ActiveDesignFile.AddNewLevel(name);
//        }
//    }

//    //public static BCOM.Level getLevel(string name)
//    //{
//    //    BCOM.Levels levels = Addin.App.ActiveDesignFile.Levels;

//    //    var activeModel =  Addin.App.ActiveModelReference;   
//    //    activeModel.Levels.IncludeHidden = true;

//    //    try
//    //    {
//    //        return activeModel.Levels[name];
//    //    }
//    //    catch (Exception) {} // не найден


//    //    BCOM.Level activeLevel = Addin.App.ActiveSettings.Level;

//    //    try
//    //    {
//    //        Addin.Instance.sendKeyin(
//    //            string.Format("level set active {0}", name));

//    //        activeModel.Levels.IncludeHidden = true;
//    //        return activeModel.Levels[name];
//    //    }
//    //    catch (Exception) 
//    //    {
//    //        return null;
//    //    }
//    //    finally
//    //    {
//    //        Addin.App.ActiveSettings.Level = activeLevel;
//    //    }
//    //}

//    //public static BCOM.Level getLevelOrActive(string name)
//    //{
//    //    return getLevel(name) ?? Addin.App.ActiveSettings.Level;
//    //}

//    public static void setSymbologyByLevel(BCOM.Element element)
//    {
//        element.LineStyle = Addin.App.ByLevelLineStyle();
//        element.LineWeight = Addin.App.ByLevelLineWeight();
//        element.Color = Addin.App.ByLevelColor();
//    }
//}
//}
