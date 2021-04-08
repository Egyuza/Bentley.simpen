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

        var formsIntersected = new List<TFCOM.TFElementList>();
        
        {  // Пересечения со стеной/плитой/аркой;
                foreach (BCOM.Element current in bcomEl.scanIntersectsInElementRange())
            {
                TFCOM.TFElementList tfList = AppTF.CreateTFElement();
                tfList.InitFromElement(current);

                if (tfList.AsTFElement == null)
                    continue;

                int tfType = tfList.AsTFElement.GetApplicationType();
                if (tfList.AsTFElement.GetIsFormType())
                {
                    if (isAvaliableTFFromType(tfType))
                    {
                        formsIntersected.Add(tfList);
                    }
                }
            }
        }
        
        BCOM.Plane3d planeFirst = new BCOM.Plane3d();
        BCOM.Plane3d planeSecond = new BCOM.Plane3d();

        TFFormTypeEnum formType = TFFormTypeEnum.UNDEFINED;

        bool planesAreFound = false;

        if (formsIntersected.Count > 0)
        {
            // todo определение граней параллельных поверхности стены/плиты/арки;

            var tfElement = formsIntersected.First().AsTFElement;
            BCOM.Element formElement;
            tfElement.GetElement(out formElement);
            
            formType = ElementHelper.ParseFormType(tfElement.GetApplicationType());

            switch (formType)
            {
            case TFFormTypeEnum.TF_LINEAR_FORM_ELM:
                planesAreFound = ElementHelper.getFacePlaneByLabel(out planeFirst, 
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelLeft);
                planesAreFound &= ElementHelper.getFacePlaneByLabel(out planeSecond, 
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelRight);
                break;
            case TFFormTypeEnum.TF_SLAB_FORM_ELM:
                planesAreFound =ElementHelper.getFacePlaneByLabel(out planeFirst, 
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelTop);
                planesAreFound &= ElementHelper.getFacePlaneByLabel(out planeSecond, 
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelBase);
                break;
            case TFFormTypeEnum.TF_ARC_FORM_ELM:
                // TODO РАДИАЛЬНЫЕ СТЕНЫ
                break;
            }
        }

        if (planesAreFound)
        { 
        // корректировка плоскостей относительно пользователя
            BCOM.Point3d projOrigin =
                App.Point3dProjectToPlane3d(ref planeFirst.Origin, ref planeSecond, null, false);

            switch (formType)
            {
            case TFFormTypeEnum.TF_LINEAR_FORM_ELM:
                if ((int)projOrigin.Y < (int)planeFirst.Origin.Y ||
                    (int)projOrigin.X >(int)planeFirst.Origin.X)
                {
                    BCOM.Plane3d buff = planeFirst;
                    planeFirst = planeSecond;
                    planeSecond = buff;
                }
                break;
            case TFFormTypeEnum.TF_SLAB_FORM_ELM:
            case TFFormTypeEnum.TF_FREE_FORM_ELM:
                if ((int)projOrigin.Z > (int)planeFirst.Origin.Z) {
                    BCOM.Plane3d buff = planeFirst;
                    planeFirst = planeSecond;
                    planeSecond = buff;
                }
                break;
            }
        }


        { // Определение параметров контура:
            BCOM.Point3d[] bnds  = verts;
            BCOM.Point3d[][] facetPoints = 
            {
                new BCOM.Point3d[4],
                new BCOM.Point3d[4],
                new BCOM.Point3d[4]
            };

		    //if (formType == TF_ARC_FORM_ELM) {
		    //	facetPoints[0][0] = bnds[0];
		    //	facetPoints[0][1] = bnds[1];
		    //	facetPoints[0][2] = bnds[2];
		    //	facetPoints[0][3] = bnds[3];
		    //	
		    //	facetPoints[1][0] = bnds[0];
		    //	facetPoints[1][1] = bnds[3];
		    //	facetPoints[1][2] = bnds[4];
		    //	facetPoints[1][3] = bnds[7];
		    //	
		    //	facetPoints[2][0] = bnds[3];
		    //	facetPoints[2][1] = bnds[2];
		    //	facetPoints[2][2] = bnds[5];
		    //	facetPoints[2][3] = bnds[4];
		    //}
		    //else {

		    for (int i = 0; i < 2; ++i) {

			    if (i == 0) {
				    if (formType == TFFormTypeEnum.TF_ARC_FORM_ELM) {
					    facetPoints[0][0] = bnds[0];
					    facetPoints[0][1] = bnds[1];
					    facetPoints[0][2] = bnds[2];
					    facetPoints[0][3] = bnds[3];
					
					    facetPoints[1][0] = bnds[0];
					    facetPoints[1][1] = bnds[3];
					    facetPoints[1][2] = bnds[4];
					    facetPoints[1][3] = bnds[7];
					
					    facetPoints[2][0] = bnds[3];
					    facetPoints[2][1] = bnds[2];
					    facetPoints[2][2] = bnds[5];
					    facetPoints[2][3] = bnds[4];
				    }
				    else {

					    facetPoints[0][0] = bnds[0];
					    facetPoints[0][1] = bnds[1];
					    facetPoints[0][2] = bnds[6];
					    facetPoints[0][3] = bnds[7];

					    facetPoints[1][0] = bnds[1];
					    facetPoints[1][1] = bnds[2];
					    facetPoints[1][2] = bnds[5];
					    facetPoints[1][3] = bnds[6];

					    facetPoints[2][0] = bnds[4];
					    facetPoints[2][1] = bnds[5];
					    facetPoints[2][2] = bnds[6];
					    facetPoints[2][3] = bnds[7];

				    }
			    }
			    else {
				    facetPoints[0][0] = bnds[0];
				    facetPoints[0][1] = bnds[1];
				    facetPoints[0][2] = bnds[7];
				    facetPoints[0][3] = bnds[4];

				    facetPoints[1][0] = bnds[1];
				    facetPoints[1][1] = bnds[2];
				    facetPoints[1][2] = bnds[6];
				    facetPoints[1][3] = bnds[7];

				    facetPoints[2][0] = bnds[4];
				    facetPoints[2][1] = bnds[7];
				    facetPoints[2][2] = bnds[6];
				    facetPoints[2][3] = bnds[5];
			    }

			    for (int j = 0; j < 3; ++j) {
				    BCOM.Plane3d facet = 
                        ElementHelper.GetPlane3DByPoints(facetPoints[j]);                   
				    if (ElementHelper.IsPlanesAreParallel(facet, planeFirst)) {
                        // TODO

					    //DPoint3d pts[4];
					    //for (int j = 0; j < 4; ++j) {
						   // mdlVec_projectPointToPlane(&pts[j],
							  //  &facetPoints[j][j], &facet.origin, &facet.normal);
					    //}
					    //// опорная точка Origin:
					    //mdlVec_extractPolygonNormal(NULL, &contourOrigin, pts, 4);
					    //mdlVec_projectPointToPlane(&contourOrigin, &contourOrigin,
						   // &planeFirst.origin, &planeFirst.normal);

					    //{ // Высота
						   // OpeningTask::getInstance().height = CExpr::convertToMaster(
							  //  mdlVec_distance(&pts[3], &pts[0]));
						   // // вектор высоты:
						   // mdlVec_subtractDPoint3dDPoint3d(&heightVec,
							  //  &pts[3], &pts[0]);
					    //}
					    //{// Ширина
						   // OpeningTask::getInstance().width = CExpr::convertToMaster(
							  //  mdlVec_distance(&pts[1], &pts[0]));
						   // // вектор ширины:
						   // mdlVec_subtractDPoint3dDPoint3d(&widthVec,
							  //  &pts[1], &pts[0]);
					    //}
					    //{ // Глубина
						   // DPoint3d projPoint;
						   // mdlVec_projectPointToPlane(&projPoint, &contourOrigin,
							  //  &planeSecond.origin, &planeSecond.normal);

						   // OpeningTask::getInstance().depth = CExpr::convertToMaster(
							  //  mdlVec_distance(&projPoint, &contourOrigin));
						   // // вектор глубины:
						   // mdlVec_subtractDPoint3dDPoint3d(&depthVec,
							  //  &projPoint, &contourOrigin);
					    //}

					    //isValid = true;
					    //UI::readDataSynch();
					    //UI::setEnableAddToModel();

					    break;
				    }
			    }

			    //if (isValid) {
				   // break;
			    //}
		    }

        }


        //BCOM.Point3d[] closest = new BCOM.Point3d[verts.Count()];
        //for (int i = 0; i < verts.Count(); ++i)
        //{
        //    brep.FindClosestPoint(out closest[i], verts[i]);
        //}

        return true;

        return false;
    }

    private static bool isAvaliableTFFromType(int tftype)
    {
        if (!Enum.IsDefined(typeof(TFFormTypeEnum), tftype))
            return false;

        switch ((TFFormTypeEnum)tftype)
        {
            case TFFormTypeEnum.TF_FREE_FORM_ELM:
            case TFFormTypeEnum.TF_LINEAR_FORM_ELM:
            case TFFormTypeEnum.TF_SLAB_FORM_ELM:
            case TFFormTypeEnum.TF_SMOOTH_FORM_ELM:
            case TFFormTypeEnum.TF_ARC_FORM_ELM:
                return true;
        }
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
