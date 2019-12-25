using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation;
using Bentley.MicroStation.XmlInstanceApi;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace simpen.ui
{
class ElementHelper
{
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

        return Addin.App.CreateLineElement1(null, verts);
    }

    public static bool isElementSp3dTask(long elementId, IntPtr modelRef,
        out sp3d.Sp3dTask task)
    {
        sp3d.P3DHangerPipeSupport pipe = null;
        sp3d.P3DHangerStdComponent component = null;
        task = null;

        Element element = Element.FromElementID((ulong)elementId, modelRef);
        
        if (element == null)
            return false;

        XmlInstanceSchemaManager modelSchema =
            new XmlInstanceSchemaManager(element.ModelRef);
        
        XmlInstanceApi api = XmlInstanceApi.CreateApi(modelSchema);
        IList<string> instances = api.ReadInstances(element.ElementRef);

        foreach (string inst in instances)
        {
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
                                <sp3d.P3DHangerPipeSupport>(xmlData);
                        }
                        else if (xmlData.StartsWith("<P3DHangerStdComponent"))
                        {        
                            component = XmlSerializer.FromXml
                                <sp3d.P3DHangerStdComponent>(xmlData);
                        }
                    }
                    catch (Exception)
                    {
                        throw; // todo обработать
                    }
                }

            }
        }   
        
        if (pipe != null && component != null)
        {
            task = new sp3d.Sp3dTask(pipe, component);
        }

        return task != null;
    }


    public static BCOM.Point3d getMiddlePoint(BCOM.Point3d first, BCOM.Point3d second)
    {
        BCOM.Vector3d vec = 
            Addin.App.Vector3dSubtractPoint3dPoint3d(second, first);

        return Addin.App.Point3dAddScaledVector3d(first, vec, 0.5);
    }

    public static double getHeight(BCOM.ConeElement cone)
    {
        return Addin.App.Point3dDistance(
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
}
}
