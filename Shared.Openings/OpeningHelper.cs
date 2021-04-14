using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Linq;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using Shared.Bentley;
using Shared;
using System.Data;

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
    private struct VertexInfo : IComparable<VertexInfo>
    {
        public double Distance;
        public BCOM.Point3d Point;

        public int CompareTo(VertexInfo other)
        {
            return this.Distance.CompareTo(other.Distance);
        }
    }

    private struct DimensionInfo : IComparable<DimensionInfo>
    {
        public double Length;
        public BCOM.Point3d Vector;

        public DimensionInfo(BCOM.Point3d p0, BCOM.Point3d p1)
        {
            Length = Math.Round(App.Point3dDistance(p0, p1), 5);
            //Vector = App.Vector3dSubtractPoint3dPoint3d(p1, p0);
            Vector = App.Point3dSubtract(p1, p0);
        }

        public int CompareTo(DimensionInfo other)
        {
            return this.Length.CompareTo(other.Length);
        }
    }

    public static bool getFromElement(Element element, out OpeningTask task)
    {
        task = new OpeningTask();
        XDocument xDoc = ElementHelper.getSp3dXDocument(element);

        string propName;
        var checkNode = xDoc.Root.getChildByRegexPath("P3DEquipment");

        if (checkNode == null)
            return false;

        // TODO
        task.Code = xDoc.Root.
            getChildByRegexPath("P3DEquipment/Name", out propName)?.Value;

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

        var brep = AppTF.CreateTFBrep();
        brep.InitFromElement(bcomEl, App.ActiveModelReference);

        BCOM.Point3d[] verts;
        brep.GetVertexLocations(out verts);

        if (verts == null || verts.Count() < 8)
            return false;

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

                if (isAvaliableTFFromType(tfType))
                {
                    //if (tfList.AsTFElement.GetIsFormType())
                    //{
                        formsIntersected.Add(tfList);
                    //}
                }
            }
        }
        
        var planeFirst = new BCOM.Plane3d();
        var planeSecond = new BCOM.Plane3d();

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
                planesAreFound = ElementHelper.getFacePlaneByLabel(out planeFirst,
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelTop);
                planesAreFound &= ElementHelper.getFacePlaneByLabel(out planeSecond,
                    formElement, TFCOM.TFdFaceLabel.tfdFaceLabelBase);
                break;
            case TFFormTypeEnum.TF_ARC_FORM_ELM:
                // TODO РАДИАЛЬНЫЕ СТЕНЫ
                break;
            case TFFormTypeEnum.TF_EBREP_ELM:
                
                TFCOM.TFBrepList brepList = AppTF.CreateTFBrep();
                brepList.InitFromElement(formElement, App.ActiveModelReference);

                planesAreFound = ElementHelper.getFacePlaneByLabel(out planeFirst,
                    brepList, TFCOM.TFdFaceLabel.tfdFaceLabelTop);

                var intersectedPlanes = new List<TFCOM.TFPlane>();

                TFCOM.TFPlane tfPlane;
                IEnumerable<TFCOM.TFBrepFaceListClass> faceLists = brepList.GetFacesEx();
                foreach(var faceList in faceLists)
                {
                    if (faceList.IsPlanarAndIntersectsElement(bcomEl, out tfPlane))
                    {
                        intersectedPlanes.Add(tfPlane);
                    }
                }

                if (intersectedPlanes.Count > 2)
                {
                    // TODO ??
                    /*
                        1. собрать коллекцию с суммой площадей параллельных поверхностей
                        2. у большей коллекции по суммарной площади вычислить нормаль
                        3. выбирать плоскости проекции задания в соответсвии с выбранной нормалью
                    */

                    var facesInfo = new List<FaceInfo>();
                    foreach(var faceList in faceLists)
                    {
                        TFCOM.TFPlane faceTFPlane;
                        if (faceList.IsPlanar(out faceTFPlane))
                        {
                            double area;
                            faceList.GetArea(out area, string.Empty);
                            BCOM.Plane3d plane3D = faceTFPlane.ToPlane3d();

                            FaceInfo faceInfo = facesInfo.FirstOrDefault(x =>
                                x.Normal.IsVectorParallelTo(plane3D.Normal));

                            if (faceInfo.Area == 0)
                            {
                                facesInfo.Add(new FaceInfo() { 
                                    Area = area, Normal = plane3D.Normal 
                                });
                            }
                            else
                            {
                                faceInfo.Area += area;
                            }
                        }
                    }
                    facesInfo.Sort((x, y) => -1 * x.Area.CompareTo(y.Area));

                    FaceInfo faceTarget = facesInfo[0];

                    intersectedPlanes.RemoveAll((x) => { 
                        BCOM.Point3d norm;
                        x.GetNormal(out norm);

                        return !norm.IsVectorParallelTo(faceTarget.Normal);
                    });
                    ;
                }   
                
                if (intersectedPlanes.Count == 2)
                {
                    // ! должно выполняться условие параллельности
                    planeFirst = intersectedPlanes[0].ToPlane3d();
                    planeSecond = intersectedPlanes[1].ToPlane3d();

                    planesAreFound = planeFirst.IsParallelTo(planeSecond);
                }

                {
                    /*
                     Если только одна поверхность:
                        1. через точку контура провести нормаль
                        2. найти поверхности, кот. пересекает нормаль                     
                    */
                }


                break;
            }
        }

        if (!planesAreFound)
            return false;

        { 
        // корректировка плоскостей относительно пользователя
            BCOM.Point3d projOrigin = 
                planeFirst.Origin.ProjectToPlane3d(planeSecond);

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

            var sorted = new List<VertexInfo>();
            foreach (BCOM.Point3d pt in verts)
            {
                var projPt = pt.ProjectToPlane3d(planeFirst);
                sorted.Add(new VertexInfo() { 
                    Distance = App.Point3dDistance(pt, projPt),
                    Point = projPt
                });
            }
            sorted.Sort();
            var firstFaceBounds = sorted.Take(4).Select(x => x.Point);

            var checkSet = new HashSet<BCOM.Point3d>(firstFaceBounds);
            if (checkSet.Count < 4)
                return false;

            sorted.Clear();
            foreach (BCOM.Point3d pt in verts)
            {
                var projPt = pt.ProjectToPlane3d(planeSecond);
                sorted.Add(new VertexInfo() { 
                    Distance = App.Point3dDistance(pt, projPt),
                    Point = projPt
                });
            }
            sorted.Sort();
            var secondFaceBounds = sorted.Take(4).Select(x => x.Point);
            checkSet = new HashSet<BCOM.Point3d>(secondFaceBounds);
            if (checkSet.Count < 4)
                return false;
               
            var pts = firstFaceBounds.ToArray();
                
            var dimensions = new List<DimensionInfo>() {
                new DimensionInfo(pts[0], pts[1]),
                new DimensionInfo(pts[0], pts[2]),
                new DimensionInfo(pts[0], pts[3]),
            };
            dimensions.Sort();
                
            if (planeFirst.IsParallelTo(App.GetPlane3dXY()))
            {
                // TOTO ширина и высота относительно ориентации
            }

            // опорная точка Origin:
            //contourOrigin
            BCOM.ShapeElement contour = App.CreateShapeElement1(null, pts);
            BCOM.Point3d projOriginFirst =
                contour.Centroid().ProjectToPlane3d(planeFirst);

            task.Origin = projOriginFirst;

            { // Высота
                task.Height = dimensions[0].Length;
                task.HeigthVec = dimensions[0].Vector;
			}
			{// Ширина
                task.Width = dimensions[1].Length;
                task.WidthVec = dimensions[1].Vector;
			}
			{ // Глубина
				BCOM.Point3d projOriginSecond = 
                    projOriginFirst.ProjectToPlane3d(planeSecond);
                task.Depth = Math.Round(
                    App.Point3dDistance(projOriginFirst, projOriginSecond), 5);
                task.DepthVec = 
                    App.Point3dSubtract(projOriginSecond, projOriginFirst);                
			}

            return true;


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

                bool isValid = false;                

                break;

			    //for (int j = 0; j < 3; ++j) {
       //             BCOM.Plane3d facet;
       //             if (!ElementHelper.GetPlane3DByPoints(out facet, facetPoints[j]))
       //                 continue;                   

				   // if (ElementHelper.IsPlanesAreParallel(facet, planeFirst)) {
       //                 // TODO

					  //  var pts =  facetPoints[j];
       //  //               new BCOM.Point3d[4];
					  //  //for (int k = 0; k < 4; ++k) {
       //  //                   pts[k] = facetPoints[j][k].ProjectToPlane3d(facet);
					  //  //}
					  //  // опорная точка Origin:
       //                 //contourOrigin
       //                 BCOM.ShapeElement contour = App.CreateShapeElement1(null, pts);
       //                 BCOM.Point3d projOriginFirst = 
       //                     contour.Centroid().ProjectToPlane3d(planeFirst);

					  //  { // Высота
       //                     task.Height = App.Point3dDistance(pts[3], pts[0]);
       //                     task.HeigthVec = 
       //                         App.Vector3dSubtractPoint3dPoint3d(pts[3], pts[0]);
					  //  }
					  //  {// Ширина
       //                     task.Width = App.Point3dDistance(pts[1], pts[0]);
       //                     task.WidthVec= 
       //                         App.Vector3dSubtractPoint3dPoint3d(pts[1], pts[0]);
					  //  }
					  //  { // Глубина
						 //  BCOM.Point3d projOriginSecond = 
       //                     projOriginFirst.ProjectToPlane3d(planeSecond);

       //                     task.Depth = App.Point3dDistance(
       //                         projOriginFirst, projOriginSecond);
       //                     task.DepthVec= 
       //                         App.Vector3dSubtractPoint3dPoint3d(
       //                             projOriginSecond, projOriginFirst);
					  //  }
       //                 isValid = true;
					  //  break;
				   // }
			    //}

			    //if (isValid)
				   // break;			    
		    }
        }

        //BCOM.Point3d[] closest = new BCOM.Point3d[verts.Count()];
        //for (int i = 0; i < verts.Count(); ++i)
        //{
        //    brep.FindClosestPoint(out closest[i], verts[i]);
        //}

        return true;
    }

    public struct FaceInfo
    {
        public BCOM.Point3d Normal;
        public double Area;
    }

    public static TFCOM.TFFrameListClass createFrameList(
        OpeningTask task, BCOM.Level level)
    {
        BCOM.Element shape = GetOpeningShape(task);

        BCOM.SmartSolidElement body = 
            App.SmartSolid.ExtrudeClosedPlanarCurve(shape, task.Depth, 0.0, true);

        var frameList = new TFCOM.TFFrameListClass();
        frameList.Add3DElement(body);
        return frameList;
    }


    public static bool  getTaskDataFromDataRow(out OpeningTask task, DataRow row)
    {
        try
        {
            task = new OpeningTask(){
                
            };
        }
        catch (Exception)
        {
            task = null;
        }

        return task != null;
    }

    public static BCOM.Element GetOpeningShape(OpeningTask task)
    {
        var bounds = new BCOM.Point3d[4];

        bounds[0] = task.Origin.AddScaled(task.HeigthVec, -0.5).AddScaled(task.WidthVec, -0.5);
        bounds[1] = task.Origin.AddScaled(task.HeigthVec, 0.5).AddScaled(task.WidthVec, -0.5);
        bounds[2] = task.Origin.AddScaled(task.HeigthVec, 0.5).AddScaled(task.WidthVec, 0.5);
        bounds[3] = task.Origin.AddScaled(task.HeigthVec, -0.5).AddScaled(task.WidthVec, 0.5);

        var shape = App.CreateShapeElement1(null, bounds);
        
        if (!App.Point3dEqualTolerance(
            shape.Normal.Normalize(), task.DepthVec.Normalize(), 0.00005))
        {
            shape.Reverse();
        }
        return shape;
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
            case TFFormTypeEnum.TF_EBREP_ELM:
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
