using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using Shared.Bentley;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Openings.Shared
{
static class OpeningHelper
{
    public static bool getFromElement(Element element, out OpeningTask task)
    {
        task = null;
        XDocument xDoc = ElementHelper.getSp3dXDocument(element);
        var equipmentEl = xDoc.Root.Elements().FirstOrDefault(x => 
                x.Name.LocalName.Equals("P3DEquipment"));

        if (equipmentEl == null)
            return false;

        BCOM.Element bcomEl = element.AsElementCOM();

        var tfEl = AppTF.CreateTFElement();
        tfEl.InitFromElement(bcomEl);

        var tfElType = (TFCOM.TFdElementType)tfEl.AsTFElement.GetApplicationType();
        //if (tfElType != TFCOM.TFdElementType.tfdElementTypeEBrep)
        //{
        //    return false;
        //}

        BCOM.CellElement cell = element.AsCellElementCOM();
       // BCOM.Point3d[] verts = cell.AsSmartSolidElement.GetVertices();
        ;

        var brep = AppTF.CreateTFBrep();
        brep.InitFromElement(bcomEl, App.ActiveModelReference);

        BCOM.Point3d[] verts;
        brep.GetVertexLocations(out verts);

        if (verts.Count() > 8)
        {
            // встречалиь случаи с количеством вершин 24 шт.
            verts = new HashSet<BCOM.Point3d>(verts).Take(8).ToArray();
        }

        // todo пересечение со стеной/плитой/аркой;
        // todo определение граней параллельных поверхности стены/плиты/арки;

        //BCOM.Point3d[] closest = new BCOM.Point3d[verts.Count()];
        //for (int i = 0; i < verts.Count(); ++i)
        //{
        //    brep.FindClosestPoint(out closest[i], verts[i]);
        //}

        return true;

        return false;
    }

    static BCOM.Application App
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
