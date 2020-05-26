#include "ElementHelper.h"
#include "OpeningHelper.h"
#include "ui.h"
#include "ProgressBar.h"

#include <buildingeditelemhandle.h>
#include <CatalogCollection.h>
#include <elementref.h>
#include <interface\ElemHandle.h>
#include <ListModelHelper.h>
#include <msdisplaypath.h>

#include <ditemlib.fdf>
#include <leveltable.fdf>

#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <mdltfform.fdf>
#include <mdltfframe.fdf>
#include <mdltfmodelref.fdf>
#include <mdltfaform.fdf>

#include <mdltfelmdscr.fdf>
#include <mdltfperfo.fdf>
#include <mdltfprojection.fdf>
#include <mscnv.fdf>
#include <msdgnmodelref.fdf>
#include <msdialog.fdf>
#include <mselemen.fdf>
#include <mselmdsc.fdf>
#include <msinput.fdf>
#include <mslocate.fdf>
#include <msmisc.fdf>
#include <msoutput.fdf>
#include <msparse.fdf>
#include <msscancrit.fdf>
#include <msstate.fdf>
#include <mssystem.fdf>
#include <mstrnsnt.fdf>
#include <msundo.fdf>
#include <msvar.fdf>
#include <msvec.fdf>

#include <mscell.fdf>

#include <mskisolid.fdf>

#include <mstmatrx.fdf>
#include <mscurrtr.fdf>
#include <msrmatrx.fdf>

#include <map>
#include <msmodel.fdf>

using Bentley::Ustn::Element::EditElemHandle;

namespace Openings {

StatusInt computeAndDrawTransient(Opening& opening) {

    EditElemHandle bodyShape, perfoShape, crossFirst, crossSecond;
    
	LevelID activeLevelId;
	mdlLevel_getActive(&activeLevelId);
	MSWChar activeLevelName[MAX_LEVEL_NAME_LENGTH];
	mdlLevel_getName(activeLevelName, MAX_LEVEL_NAME_LENGTH, ACTIVEMODEL, activeLevelId);
	
    if (SUCCESS != computeElementsForOpening(
        bodyShape, perfoShape, crossFirst, crossSecond, opening, 
		activeLevelName, activeLevelName))
    {
        return ERROR;
    }

    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        bodyShape.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        crossFirst.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        crossSecond.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);

    return SUCCESS;
}

StatusInt computeAndAddToModel(Opening& opening,
	bool rewritePrevious, MSElementDescr* previousP) 
{
    EditElemHandle bodyShape, perfoShape, crossFirst, crossSecond;
		

	if (SUCCESS != computeElementsForOpening(
        bodyShape, perfoShape, crossFirst, crossSecond, opening,
		Opening::LEVEL_NAME, Opening::LEVEL_SYMBOL_NAME))
    {
        return ERROR;
    }

	const LevelID activeLevelId = tcb->activeLevel;
	const symbology activeSymbology = tcb->symbology;

	tcb->symbology = Opening::SYMBOLOGY_BY_LEVEL;

	if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, Opening::LEVEL_NAME))
	{
		const char* msg = "Не найден слой - <C-OPENING-BOUNDARY>";
		mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);        
	}

	// СОЗДАНИЕ:
    TFFrame* frameP = createPenetrFrame(bodyShape, perfoShape, 
		opening.direction, opening.getDistance(), 0.0, true,
        opening.getTask().isThroughHole);
    
	if (frameP == NULL) {
		mdlLevel_setActive(activeLevelId);
		tcb->symbology = activeSymbology;
		return ERROR;
	}

	{ // ПРОЕКЦИЯ:
		StatusInt status;
		status = appendToProjection(frameP, crossFirst.GetElemDescrCP());
		status = appendToProjection(frameP, crossSecond.GetElemDescrCP());

		mdlTFFrame_synchronize(frameP);
	}

    StatusInt resStatus = ERROR;
    // ДОБАВЛЕНИЕ В МОДЕЛЬ:
    {
        DgnModelRefP modelRefP = ACTIVEMODEL;
        UInt32 fpos = mdlElement_getFilePos(FILEPOS_NEXT_NEW_ELEMENT, &modelRefP);
        
		if (rewritePrevious && previousP && 
			previousP->h.dgnModelRef == modelRefP) 
		{
			// todo 
			//TFFrame* prevFrameP = NULL;
			//MSElementDescr* prevFrameDef = NULL;
			//TFFrameList* listP = mdlTFFrameList_constructFromElmdscr(previousP);
			//if (listP) {
			//	prevFrameP = mdlTFFrameList_getFrame(listP);
			//	if (prevFrameP) {
			//		prevFrameDef = mdlTFFrame_get3DElmdscr(prevFrameP);
			//	}
			//}
			
			UInt32 prevPos = elementRef_getFilePos(previousP->h.elementRef);
			//MSElementDescr* newEdP = mdlTFFrame_get3DElmdscr(frameP);		

			//resStatus = newEdP != NULL;
			//mdlElmdscr_rewriteByModelRef(newEdP, previousP, prevPos, modelRefP);

			mdlElmdscr_deleteByModelRef(previousP, prevPos, modelRefP, 1);
			resStatus = mdlTFModelRef_addFrame(modelRefP, frameP);
		}
		else {
			resStatus = mdlTFModelRef_addFrame(modelRefP, frameP);
		}

        if (resStatus == SUCCESS)
        {
            if (fpos) {
                // записываем свойства в созданный CompoundCell:
                ElementRef elemRef = mdlModelRef_getElementRef(modelRefP, fpos);
				
				Bentley::Building::Elements::
					BuildingEditElemHandle beeh(elemRef, modelRefP);
				
				MSWChar kksW[50];
				mdlCnv_convertMultibyteToUnicode(opening.getKKS(), -1, kksW, 50);

                //std::wstring kksValue(kksW);

                setDataGroupInstanceValue(beeh, Opening::CATALOG_TYPE_NAME, 
					Opening::CATALOG_INSTANCE_NAME, L"Opening/@PartCode", kksW);
					
				updateOpeningR(beeh.GetElemDescrP(), NULL);
				beeh.Rewrite();
            }

            if (opening.getTask().isRequiredRemoveContour && 
                opening.contourRef != NULL && !elementRef_isEOF(opening.contourRef))
            {  
                // TODO проверить на контур из референса

                // Удаляем контур:
                MSElementDescrP contourEdP = NULL;
                mdlElmdscr_getByElemRef(
                    &contourEdP, opening.contourRef, modelRefP, FALSE, 0);

                fpos = mdlElmdscr_getFilePos(contourEdP);
                mdlElmdscr_deleteByModelRef(contourEdP, fpos, modelRefP, TRUE);
            }

            mdlOutput_prompt("Проём успешно создан");
            resStatus = SUCCESS;
        }
    }
        
    tcb->symbology = activeSymbology;  // 1. ! до активации слоя
    mdlLevel_setActive(activeLevelId); // 2.

    return resStatus;
}

StatusInt computeElementsForOpening(EditElemHandleR outBodyShape,
    EditElemHandleR outPerfoShape, EditElemHandleR outCrossFirst,
    EditElemHandleR outCrossSecond, Opening& opening, 
	MSWCharCP boundaryLevel, MSWCharCP symbolLevel)
{
	MSElementDescrP edP = NULL;
	mdlElmdscr_getByElemRef(&edP,
		opening.contourRef, opening.contourModelRef, FALSE, 0);
	EditElemHandle contour;
	if (edP == NULL && opening.contourPoints.size() > 2) {
		if (createShape(contour,
				&opening.contourPoints[0], opening.contourPoints.size(), false))
		{
			edP = contour.GetElemDescrP();
		}
	}
	if (edP == NULL) {
		return FALSE;
	}

	int elemType = edP->el.hdr.ehdr.type;

	LevelID lvlId;
	mdlLevel_getActive(&lvlId);
	const LevelID activeLevelId = lvlId;
	const int COLOR_ACTIVE = tcb->symbology.color;

	tcb->symbology.color = COLOR_BYLEVEL;
	if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, boundaryLevel))
	{   // mdlLevel_setActiveByName - позволяет принудительно добавить 
		// библиотечный Level-стиль в модель
		//char* msg = "Не найден слой требуемый для создания проёма - <C-OPENING-BOUNDARY>";
		//mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
	}

	EditElemHandle formEeh = 
		EditElemHandle(OpeningTask::getInstance().tfFormRef, ACTIVEMODEL);
	int formType = mdlTFElmdscr_getApplicationType(formEeh.GetElemDescrP());
	
    DPlane3d contourPlane;
    mdlElmdscr_extractNormal(
        &contourPlane.normal, &contourPlane.origin, edP, NULL);
    
    double shift;
    DPoint3d pt1, pt2;
    mdlMinDist_betweenElms(&pt1, &pt2, &shift, 
        edP, formEeh.GetElemDescrP(), &contourPlane.origin, fc_epsilon);
    
    if (shift != 0.0 && elemType == SHAPE_ELM ) {

        //mdlVec_projectPoint(&contourPlane.origin, &contourPlane.origin, &shiftVec, 1.0);
        DPoint3d centroid;
        mdlMeasure_linearProperties(NULL, &centroid, NULL, NULL, NULL, NULL,
            NULL, NULL, edP, fc_epsilon);

        DVec3d shiftVec;
        //DPoint3d perpenPoint;
        //RotMatrix rotation;
        //mdlRMatrix_getIdentity(&rotation);
        //int stat = mdlProject_perpendicular(&perpenPoint, NULL, &shiftVec, formEeh.GetElemDescrP(), ACTIVEMODEL, &centroid, &rotation, fc_epsilon);

        mdlVec_subtractDPoint3dDPoint3d(&shiftVec, &pt2, &pt1);

        Transform tMatrix;
        mdlTMatrix_getIdentity(&tMatrix);
        mdlTMatrix_translate(&tMatrix, &tMatrix, shiftVec.x, shiftVec.y, shiftVec.z);
        mdlElmdscr_transform(edP, &tMatrix);

        mdlElmdscr_extractNormal(
            &contourPlane.normal, &contourPlane.origin, edP, NULL);

        //EditElemHandle line;
        //DPoint3d pts[] = { pt1, pt2 };
        //сreateLine(line, pts);

        //msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        //	line.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    }

    {
         // todo проецирование контура на ближайшую грань!!!
    
    }


    Transform elTran;
    mdlElmdscr_orientationExt(&elTran, edP, edP->h.dgnModelRef);

    
    double distance = .0;
    {   // обновление и корректировка
        DVec3d directVector;
		StatusInt orientStatus = ERROR;

		if (formType == TF_ARC_FORM_ELM) {
            // todo для круга
			orientStatus = findDirVecFromArcWall(directVector, distance, contourPlane,
				opening.contourPoints[0], OpeningTask::getInstance().tfFormRef,
				ACTIVEMODEL);
		}
		else {
            TFFormRecipeList* wall = NULL;
            if (BSISUCCESS != mdlTFFormRecipeList_constructFromElmdscr(
                &wall, formEeh.GetElemDescrP())) {
                tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
                mdlLevel_setActive(activeLevelId); // 2.
                return ERROR;
            }

			orientStatus =
				findDirectionVector(directVector, distance, contourPlane, wall);
		}

        if (SUCCESS != orientStatus)
		{
			tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
			mdlLevel_setActive(activeLevelId); // 2.
			return ERROR;
        }

        // если оставить знак, то направление инвертируется, 
        // т.к. его уже задаёт вектор
        distance = abs(distance);

        if (!opening.getTask().isThroughHole) {
            distance = opening.getDistance();
        }
        else {
            opening.setDistance(distance);
        }
        opening.direction = directVector;
        //mdlVec_scaleToLength(&opening.direction, &opening.direction, 
        //    distance != 0.0 ? distance : 1.0);
    }

    EditElemHandle linearContour;
    if (elemType == SHAPE_ELM) { // todo можно объединить с CMPLX_SHAPE_ELM
        createStringLine(linearContour,
            &opening.contourPoints[0], opening.contourPoints.size());

        // перфоратор должен быть с заливкой
        createShape(outPerfoShape,
            &opening.contourPoints[0], opening.contourPoints.size(), true);
    }
    else if (elemType == ELLIPSE_ELM) {
        RotMatrix rot;
		DPoint3d center;
		double primary, secondary;
		mdlArc_extract(NULL, NULL, NULL, &primary, &secondary, &rot, &center,
			&edP->el);

		createArcEllipse(linearContour, center, primary, secondary, &rot);

        // перфоратор должен быть с заливкой
        createEllipse(outPerfoShape, center, primary, secondary, &rot, true);
    }
    else if (elemType == CMPLX_SHAPE_ELM) {
		MSElementDescrP tempEdp = NULL;
		mdlElmdscr_duplicate(&tempEdp, edP);
		mdlElmdscr_addFill(&tempEdp);
		ToEditHandle(outPerfoShape, tempEdp);

		// разрываем, чтобы получить далее полую поверхность
		mdlElmdscr_open(&tempEdp, edP, opening.contourModelRef);
		ToEditHandle(linearContour, tempEdp);
    }

    if (!createBody(outBodyShape, linearContour, opening.direction, distance, 0.0))
	{
		tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
		mdlLevel_setActive(activeLevelId); // 2.
        return ERROR;
    }

	if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, symbolLevel))
	{	// todo
		// char* msg = "Не найден слой требуемый для создания проёма - <C-OPENING-BOUNDARY>";
		// mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
	}

    // ПЕРЕКРЕСТИЯ:
	if (elemType == SHAPE_ELM) {
		//NULL, NULL, contourEdP, fc_epsilon);
		DPoint3d centroid;
		mdlMeasure_linearProperties(NULL, &centroid, NULL, NULL, NULL, NULL,
			NULL, NULL, linearContour.GetElemDescrP(), fc_epsilon);

		int numVerts = opening.contourPoints.size();
		DPoint3d* vertices = &opening.contourPoints[0];

		createCross(outCrossFirst, centroid, &opening.contourPoints[0],
			numVerts);

		// 2-ое перекрестие получаем проецируя 1-ое на противоположную грань
		DVec3d projVec = opening.direction;
		mdlVec_scaleToLength(&projVec, &projVec, distance);
		mdlVec_projectPoint(&centroid, &centroid, &projVec, 1.0);
		for (int i = 0; i < numVerts; ++i) {
			mdlVec_projectPoint(&vertices[i], &vertices[i], &projVec, 1.0);
		}
		createCross(outCrossSecond, centroid, vertices, numVerts);
	}
	else if (elemType == CMPLX_SHAPE_ELM) {
		DVec3d normal;
		EditElemHandle tempContour;
		MSElementDescrP tempEdp = NULL;
		mdlElmdscr_duplicate(&tempEdp, linearContour.GetElemDescrCP());
		ToEditHandle(tempContour, tempEdp);

		mdlElmdscr_extractNormal(&normal, NULL, tempContour.GetElemDescrP(), NULL);
		
		RotMatrix rot;
		mdlRMatrix_fromNormalVector(&rot, &normal);
		RotMatrix inverseRot;
		mdlRMatrix_getInverse(&inverseRot, &rot);

		Transform tran;
		mdlTMatrix_fromRMatrix(&tran, &rot);
		mdlElmdscr_transform(tempContour.GetElemDescrP(), &tran);

		DPoint3d ptsRng[2];
		mdlElmdscr_computeRange(&ptsRng[0], &ptsRng[1], tempContour.GetElemDescrP(), NULL);
		createLine(outCrossFirst, ptsRng);

		{			
			mdlElmdscr_duplicate(&tempEdp, outCrossFirst.GetElemDescrCP());

			DPoint3d middle = getMiddle(&ptsRng[1], &ptsRng[0]);
			DVec3d nrm;
			mdlElmdscr_extractNormal(&nrm, NULL, tempContour.GetElemDescrP(), NULL);

			mdlTMatrix_fromRotationAroundPointAndVector(&tran,
				&middle, &nrm, fc_pi / 2);
			mdlElmdscr_transform(tempEdp, &tran);

			ToEditHandle(outCrossSecond, tempEdp);
		}
		
		{ // подрезка
			RotMatrix identityRot;
			mdlRMatrix_getIdentity(&identityRot);
			DPoint3d intersects[10];
			int count = mdlIntersect_allBetweenElms(intersects, NULL, 10,
				outCrossFirst.GetElemDescrP(), tempContour.GetElemDescrP(),
				NULL, fc_epsilon);

			createStringLine(outCrossFirst, intersects, count);

			count = mdlIntersect_allBetweenElms(intersects, NULL, 10,
				outCrossSecond.GetElemDescrP(), tempContour.GetElemDescrP(),
				NULL, fc_epsilon);

			createStringLine(outCrossSecond, intersects, count);
		}

		mdlTMatrix_fromRMatrix(&tran, &inverseRot);
		mdlElmdscr_transform(tempContour.GetElemDescrP(), &tran);
		mdlElmdscr_transform(outCrossFirst.GetElemDescrP(), &tran);
		mdlElmdscr_transform(outCrossSecond.GetElemDescrP(), &tran);

		std::vector<DPoint3d> crossVerts = std::vector<DPoint3d>();

		DPoint3d verts[10];
		int num;
		mdlLinear_extract(verts, &num, 
			outCrossFirst.GetElementCP(), outCrossFirst.GetModelRef());
		for (int i = 0; i < num; ++i) {
			crossVerts.push_back(verts[i]);
		}
		mdlLinear_extract(verts, &num,
			outCrossSecond.GetElementCP(), outCrossSecond.GetModelRef());
		for (int i = 0; i < num; ++i) {
			crossVerts.push_back(verts[i]);
		}

		mdlElmdscr_computeRange(&ptsRng[0], &ptsRng[1], tempContour.GetElemDescrP(), NULL);
		createLine(outCrossFirst, ptsRng);

		// 1-ое
		DPoint3d center = getMiddle(&ptsRng[0], &ptsRng[1]);
		createCross(outCrossFirst, center, &crossVerts[0], crossVerts.size());

		// 2-ое
		MSElementDescrP secondEdP = NULL;
		mdlElmdscr_duplicate(&secondEdP, outCrossFirst.GetElemDescrCP());
		
		DVec3d projVec = opening.direction;
		mdlVec_scaleToLength(&projVec, &projVec, distance);
		DPoint3d secondCenter;
		mdlVec_projectPoint(&secondCenter, &center, &projVec, 1.0);

		mdlTMatrix_getIdentity(&tran);
		mdlVec_subtractPoint(&secondCenter, &secondCenter, &center);
		mdlTMatrix_setTranslation(&tran, &secondCenter);
		mdlElmdscr_transform(secondEdP, &tran);

		ToEditHandle(outCrossSecond, secondEdP);
	}
	else if (elemType == ELLIPSE_ELM) {
		// 
		RotMatrix rot;
		double r1, r2;
		DPoint3d center;
		
		mdlArc_extract(NULL, NULL, NULL, &r1, &r2, &rot, &center, 
			linearContour.GetElementP());
		
		DVec3d normal;
		mdlElmdscr_extractNormal(&normal, NULL, linearContour.GetElemDescrP(), NULL);

		DPoint3d pts[5];
		double k = r2/pow(2.0, 0.5); //r1 * fc_pi / 4;
		/*pts[0] = pts[1] = ;*/
		mdlVec_zero(&pts[0]);
		mdlVec_zero(&pts[1]);
		pts[0].x = pts[0].y = k;
		pts[1].x = pts[1].y = -k;
		mdlVec_zero(&pts[2]);
		pts[3] = pts[0];
		pts[3].y = -k;
		pts[4] = pts[1];
		pts[4].y = +k;

		createStringLine(outCrossFirst, pts, 5);

		Transform tran;
		mdlTMatrix_getIdentity(&tran);
		DVec3d zAxis = {0, 0, 1};		
		mdlRMatrix_fromVectorToVector(&rot, &zAxis, &normal);
		mdlTMatrix_fromRMatrix(&tran, &rot);

		mdlElmdscr_transform(outCrossFirst.GetElemDescrP(), &tran);

		mdlTMatrix_getIdentity(&tran);
		mdlTMatrix_setTranslation(&tran, &center);
		mdlElmdscr_transform(outCrossFirst.GetElemDescrP(), &tran);

		// 2-ое
		MSElementDescrP secondEdP = NULL;
		mdlElmdscr_duplicate(&secondEdP, outCrossFirst.GetElemDescrCP());
		
		DVec3d projVec = opening.direction;
		mdlVec_scaleToLength(&projVec, &projVec, distance);
		DPoint3d secondCenter;
		mdlVec_projectPoint(&secondCenter, &center, &projVec, 1.0);

		mdlTMatrix_getIdentity(&tran);
		mdlVec_subtractPoint(&secondCenter, &secondCenter, &center);
		mdlTMatrix_setTranslation(&tran, &secondCenter);
		mdlElmdscr_transform(secondEdP, &tran);

		ToEditHandle(outCrossSecond, secondEdP);
	}
    
	tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
	mdlLevel_setActive(activeLevelId); // 2.

    return SUCCESS;
}

StatusInt findDirVecFromArcWall(DVec3d& outDirVec, double& outDistance,
	const DPlane3d& contourPlane, const DPoint3d contourVertex, 
	const ElementRef wallRef, DgnModelRefP modelRefP)
{
	outDistance = 0.0;
	mdlVec_zero(&outDirVec);

	EditElemHandle wallEeh = EditElemHandle(wallRef, modelRefP);


	int formType = mdlTFElmdscr_getApplicationType(wallEeh.GetElemDescrP());
	if (formType != TF_ARC_FORM_ELM) {
		return ERROR;
	}

	StatusInt status = SUCCESS;
	TFBrepList* brepListP = NULL;

	TFFormRecipeList* wallP = NULL;
	status += mdlTFFormRecipeList_constructFromElmdscr(&wallP, wallEeh.GetElemDescrP());
	status += mdlTFFormRecipeList_getBrepList(wallP, &brepListP, 0, 0, 0);
	
	TFFormRecipeArc* arcP = mdlTFFormRecipeList_getFormRecipe(wallP);
	mdlTFFormRecipeArc_getThickness(arcP, &outDistance);

	TFBrepFaceList* leftFacesP = 
		mdlTFBrepList_getFacesByLabel(brepListP, FaceLabelEnum_Left);
	MSElementDescrP leftEdP;
	status += mdlTFBrepFaceList_getElmdscr(&leftEdP, leftFacesP, NULL);

	TFBrepFaceList* rightFacesP =
		mdlTFBrepList_getFacesByLabel(brepListP, FaceLabelEnum_Right);
	MSElementDescrP rightEdP;
	status += mdlTFBrepFaceList_getElmdscr(&rightEdP, rightFacesP, NULL);

	TFBrepFaceList* baseFacesP =
		mdlTFBrepList_getFacesByLabel(brepListP, FaceLabelEnum_Base);
	MSElementDescrP baseEdP;
	status += mdlTFBrepFaceList_getElmdscr(&baseEdP, baseFacesP, NULL);

	// расстояние между левой и провой поверхностями стены:

	if (SUCCESS == status) {
		// проецируем всё на плоскость базовой поверхности стены, 
		// там вычисляем необходимое расстояние для проёма:

		DPlane3d basePlane;
		mdlElmdscr_extractNormal(&basePlane.normal, &basePlane.origin, baseEdP, NULL);
		
		DPoint3d arcCenter;
		double arcSweep, arcRadius;
		double arcThickness = outDistance;
		RotMatrix rot;
		mdlTFFormRecipeArc_extractBaseParams(arcP, 
			&arcCenter, &arcSweep, &arcRadius, &rot);

		DPoint3d projCenter;
		mdlVec_projectPointToPlane(&projCenter,
			&arcCenter, &basePlane.origin, &basePlane.normal);

		DPoint3d projStart = contourVertex;
		mdlVec_projectPointToPlane(&projStart,
			&projStart, &basePlane.origin, &basePlane.normal);

		if (std::abs(mdlVec_distance(&projStart, &projCenter) - arcRadius) < 0.1)
		{
			arcRadius -= arcThickness;
		}

		DPoint3d projEndEx;
		mdlVec_projectPoint(&projEndEx,
			&projStart, &contourPlane.normal, 1.25 * outDistance);
		mdlVec_projectPointToPlane(&projEndEx,
			&projEndEx, &basePlane.origin, &basePlane.normal);

		// чтобы точно получить пересечение, увеличиваем в обе стороны
		DPoint3d projStartEx;
		mdlVec_projectPoint(&projStartEx,
			&projStart, &contourPlane.normal, -1.25 * outDistance);

		EditElemHandle line;
		DPoint3d pts[] = { projStartEx, projEndEx };
		createLine(line, pts);

		MSElement arc;
		mdlArc_create(&arc,
			NULL, &projCenter, arcRadius, arcRadius, &rot, 0, arcSweep);

		EditElemHandle arcEeh;
		ToEditHandle(arcEeh, arc);

		//msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
		//	arcEeh.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);

		DPoint3d intersectsLine[3] = { DPoint3d(), DPoint3d(), DPoint3d() };
		DPoint3d intersectsArc[3] = { DPoint3d(), DPoint3d(), DPoint3d() };
		int count =
			mdlIntersect_allBetweenElms(intersectsLine, intersectsArc, 5,
				line.GetElemDescrP(), arcEeh.GetElemDescrP(), NULL, 0.1);

		count = count;

		if (count == 1) {
			mdlVec_subtractDPoint3dDPoint3d(&outDirVec, &intersectsArc[0], &projStart);
			outDistance = mdlVec_distance(&intersectsArc[0], &projStart);
		}

		//pts[0] = projStart;
		//pts[1] = intersectsArc[0];
		//сreateLine(line, pts);

		//msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
		//	line.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);

	}

	if (leftFacesP) {
		mdlTFBrepFaceList_free(&leftFacesP);
	}

	if (brepListP) {
		mdlTFBrepList_free(&brepListP);
	}

	return status;
}


StatusInt findDirectionVector(DVec3d& outDirectionVec, double& distance,
    const DPlane3d& contourPlane, const TFFormRecipeList* wallP)
{
    distance = 0.0;
    mdlVec_zero(&outDirectionVec);

    TFBrepList* brepListP = NULL;
    {
        TFFormRecipe* fP = 
            mdlTFFormRecipeList_getFormRecipe((TFFormRecipeList*)wallP);

        if (SUCCESS != mdlTFFormRecipe_getBrepList(fP, &brepListP, 0, 0, 0)) {
            return ERROR;
        }
    }

    // пробегаемся по всем граням:
    for (int i = FaceLabelEnum_None; i < FaceLabelEnum_Hidden; ++i)
    {
        DPlane3d facePlane; // плоскость грани
        {
            TFBrepFaceList* faceListP = mdlTFBrepList_getFacesByLabel(
                brepListP, static_cast<FaceLabelEnum>(i));

            MSElementDescr* faceP = NULL;
            mdlTFBrepFaceList_getElmdscr(&faceP, faceListP, 0);
        
            if (SUCCESS != mdlElmdscr_extractNormal(&facePlane.normal,
                &facePlane.origin, faceP, NULL))
            {
                continue;
            }
        }

        if (!planesAreParallel(facePlane, contourPlane)) {
            continue;
        }

    //// Ищем поверхность стены, в плоскости которой лежит указанный контур
        
        // TODO !!!!!!!!!! mdlVec_linePlaneIntersectParameter

        if (planesAreParallel(contourPlane, facePlane)) {

            bool isForwardFound = false;
           
            for (int j = FaceLabelEnum_None; j < FaceLabelEnum_Hidden; ++j)
            {
                if (i == j) {
                    continue;
                }

                TFBrepFaceList* faceFwdListP =
                    mdlTFBrepList_getFacesByLabel(brepListP, static_cast<FaceLabelEnum>(j));

                TFBrepFaceList* nextFaceListP = faceFwdListP;

                while (nextFaceListP != NULL) {
                    
                    MSElementDescr* faceForwardP = NULL;
                    mdlTFBrepFaceList_getElmdscr(&faceForwardP, nextFaceListP, 0);
                    
                    if (faceForwardP != NULL) {
                        DPlane3d forwardPlane;
                        mdlElmdscr_extractNormal(
                            &forwardPlane.normal, &forwardPlane.origin, faceForwardP, NULL);

                        // считаем, что противоположная стена должна быть параллельна
                        // плоскости контура:
                        if (planesAreParallel(contourPlane, forwardPlane)) {
                            
                            double dist1 = distanceToPlane(contourPlane.origin, facePlane);
                            double dist2 = distanceToPlane(contourPlane.origin, forwardPlane);

                            std::vector<DPoint3d>& contourPointsRef =
                                Opening::instance.contourPoints;

                            if (dist2 > dist1) {
                                distance = dist2;
                                outDirectionVec = computeVectorToPlane(contourPlane.origin, forwardPlane);
                            
                                std::vector<DPoint3d> copy = 
                                    std::vector<DPoint3d>(contourPointsRef);
                                contourPointsRef.clear();

                                for (size_t k = 0; k < copy.size(); ++k) {
                                    DPoint3d& pt = copy[k];
                                    mdlVec_projectPointToPlane(
                                        &pt, &pt, &facePlane.origin, &facePlane.normal);

                                    contourPointsRef.push_back(pt);
                                }
                            }
                            else {
                                distance = dist1;
                                outDirectionVec = computeVectorToPlane(contourPlane.origin, facePlane);
                            
                                std::vector<DPoint3d> copy =
                                    std::vector<DPoint3d>(contourPointsRef);
                                contourPointsRef.clear();

                                for (size_t k = 0; k < copy.size(); ++k) {
                                    DPoint3d& pt = copy[k];
                                    mdlVec_projectPointToPlane(
                                        &pt, &pt, &forwardPlane.origin, &forwardPlane.normal);

                                    contourPointsRef.push_back(pt);
                                }
                            }

                            isForwardFound = true;
                            break;
                        }
                    }

                    nextFaceListP = mdlTFBrepFaceList_getNext(nextFaceListP);
                }

                if (isForwardFound) {
                    break;
                }
            }

			mdlVec_normalize(&outDirectionVec);

            if (!isForwardFound) {
                distance = 0.0;
                outDirectionVec = contourPlane.normal;
            }
            
            return SUCCESS;
        }

    }

    return ERROR;
}

int modifyFunc(
	MSElementUnion      *element,     // <=> element to be modified
	void                *params,      // => user parameter
	DgnModelRefP        modelRef,     // => model to hold current elem
	MSElementDescr      *elmDscrP,    // => element descr for elem
	MSElementDescr      **newDscrPP,  // <= if replacing entire descr
	ModifyElementSource elemSource   // => The source for the element.
) {
	//eleFunc return value Meaning 
	//MODIFY_STATUS_NOCHANGE The element was not changed. 
	//MODIFY_STATUS_REPLACE Replace original element with the new element. 
	//MODIFY_STATUS_DELETE Delete the original element. 
	//MODIFY_STATUS_ABORT Stop processing component elements. MODIFY_STATUS_ABORT can be used with any of the other values. 
	//MODIFY_STATUS_FAIL An error occurred during element modification. Ignore all changes previously made. 
	//MODIFY_STATUS_REPLACEDSCR Replace the current element descriptor with a new element descriptor. This is used to replace one or more elements with a (possibly) different number of elements. 
	//MODIFY_STATUS_ERROR MODIFY_STATUS_FAIL | MODIFY_STATUS_ABORT 

	//int cl = PRIMARY_CLASS;

	//mdlElement_setProperties(element, 0, 0, &cl, 0, 0, 0, 0, 0);

	return MODIFY_STATUS_REPLACE;
}

std::string updatedInfo;
std::map<DgnModelRefP, int> updateRequiringModels;

int updatedElementsCount;

bool updateOpeningR(MSElementDescrP edP, bool* requiredP)
{
	bool res = false;

	if (edP->el.ehdr.type == CELL_HEADER_ELM) {

		MSElementDescrP childP = edP->h.firstElem;
		while (childP)
		{
			res |= updateOpeningR(childP, requiredP);
			childP = childP->h.next;
		}
		return res;
	}

	DgnModelRefP modelRefP = edP->h.dgnModelRef;
	

	const LevelID activeLevelId = tcb->activeLevel;
	const Symbology activeSymbology = tcb->symbology;

	LevelID levelId;
	bool isElementMatched = false;

	if (edP->el.ehdr.type == CMPLX_SHAPE_ELM /*BOUNDARY*/ || 
		edP->el.ehdr.type == LINE_ELM /*BOUNDARY*/) {
		MSWCharCP leveName = Opening::LEVEL_NAME;
		if (SUCCESS != mdlLevel_getIdFromName(&levelId,
			modelRefP, LEVEL_NULL_ID, leveName))
		{
			if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, leveName))
			{
				char msg[256];
				sprintf(msg, "Не найден слой - <%s>", leveName);
				mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
			}
			else {
				mdlLevel_getIdFromName(&levelId,
					modelRefP, LEVEL_NULL_ID, leveName);
			}
		}
		isElementMatched = true;
	}
	else if (edP->el.ehdr.type == LINE_STRING_ELM /*CROSSES*/ ||
			edP->el.ehdr.type == SHAPE_ELM /*PERFORATOR*/) 
	{
		MSWCharCP leveName = Opening::LEVEL_SYMBOL_NAME;
		if (SUCCESS != mdlLevel_getIdFromName(&levelId,
			modelRefP, LEVEL_NULL_ID, leveName))
		{
			if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, leveName))
			{
				char msg[256];
				sprintf(msg, "Не найден слой - <%s>", leveName);
				mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
			}
			else {
				mdlLevel_getIdFromName(&levelId,
					modelRefP, LEVEL_NULL_ID, leveName);
			}
		}
		isElementMatched = true;
	}

	if (isElementMatched) {		
		if (edP->el.ehdr.level != levelId ||
			edP->el.hdr.dhdr.symb != Opening::SYMBOLOGY_BY_LEVEL) 
		{
			if (mdlModelRef_isReadOnly(modelRefP)) {
				updateRequiringModels[modelRefP]++;

				if (requiredP != NULL) {
					*requiredP = true;
				}
			}
			else {
				edP->el.hdr.dhdr.symb = Opening::SYMBOLOGY_BY_LEVEL;
				edP->el.ehdr.level = levelId;
				res = true;
			}
		}
	}

	// recover:
	if (tcb->activeLevel != activeLevelId) {
		tcb->activeLevel = activeLevelId;
	}
	if (tcb->symbology != activeSymbology) {
		tcb->symbology = activeSymbology;
	}

	return res;
}


int scanOpenings(
	MSElementDescr* edP,
	void*  param,
	ScanCriteria *pScanCriteria
) {

	if (TF_COMPOUND_CELL_ELM != mdlTFElmdscr_getApplicationType(edP)) {
		return 0;
	}

	if (!isOpening(edP)) {
		return 0;
	}

	bool referenceRequired = false;
	bool isDirty = updateOpeningR(edP, &referenceRequired);

	{ // check Cell.Name:
		MSWChar name[MAX_CELLNAME_LENGTH];
		mdlCell_extractName(name, MAX_CELLNAME_LENGTH, &edP->el);

		if (wcscmp(name, &Opening::CELL_NAME[0]) != 0) {
			isDirty = true;
			mdlCell_setName(&edP->el, &Opening::CELL_NAME[0]);
		}
	}

	if (isDirty) {
		EditElemHandle eeh = EditElemHandle(edP, true, false);		
		if (!referenceRequired && SUCCESS == eeh.ReplaceInModel()) {
			++updatedElementsCount;

			char text[256];
			sprintf(text, "elemId = %u\n", edP->el.hdr.ehdr.uniqueId);
			updatedInfo.append(text);			
		}
	}

	return 0;
}

void scanThroughAllRefsRecurse(DgnModelRefP modelRef, ScanCriteria *scP,
	int* progressP, const int progressStep)
{
	mdlScanCriteria_setModel(scP, modelRef);
	mdlScanCriteria_scan(scP, NULL, NULL, NULL);

	ModelRefIteratorP iterator;
	mdlModelRefIterator_create(&iterator, modelRef,
		MRITERATE_PrimaryChildRefs, 0);

	DgnModelRefP refModelRef;
	while (NULL != (refModelRef = mdlModelRefIterator_getNext(iterator)))
	{
		*progressP += progressStep;
		char text[256];
		sprintf(text, "Обновление проёмов: %i%%", *progressP);
		ProgressBar::updateStatus(text, *progressP);

		scanThroughAllRefsRecurse(refModelRef, scP, progressP, progressStep);
	}
}

void countAllRefsRecurse(DgnModelRefP modelRef, int* countOutP)
{
	ModelRefIteratorP iterator;
	mdlModelRefIterator_create(&iterator, modelRef,
		MRITERATE_PrimaryChildRefs, 0);

	DgnModelRefP refModelRef;
	while (NULL != (refModelRef = mdlModelRefIterator_getNext(iterator)))
	{
		(*countOutP)++;
		countAllRefsRecurse(refModelRef, countOutP);
	}
}

// todo Обновление всех проёмов модели
void cmdUpdateAll(char *unparsedP)
{
    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status = 0;

    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();
	status += mdlScanCriteria_setReturnType(scP, MSSCANCRIT_ITERATE_ELMDSCR, FALSE, TRUE);
	status += mdlScanCriteria_setElmDscrCallback(scP, (PFScanElemDscrCallback)scanOpenings, 0);
    //status +=  mdlScanCriteria_setCellNameTest(scP, L"Opening");
	status += mdlScanCriteria_setDrawnElements(scP);
	status += mdlScanCriteria_addSingleElementTypeTest(scP, CELL_HEADER_ELM);
	status += mdlScanCriteria_setModel(scP, ACTIVEMODEL);

	if (status == SUCCESS) {
		mdlUndo_startGroup();

		updatedElementsCount = 0;
		updatedInfo = "";
		updateRequiringModels = std::map<DgnModelRefP, int>();

		{
			ProgressBar::openStatus("Обновление проёмов");
			int refsCount = 0;
			countAllRefsRecurse(ACTIVEMODEL, &refsCount);

			int step = refsCount == 0 ? 100 : 100/refsCount;
			int currentProgress = 0;
			scanThroughAllRefsRecurse(ACTIVEMODEL, scP, &currentProgress, step);

			ProgressBar::closeStatus();
		}

		int openingsReqCount = 0;
		if (updateRequiringModels.size() > 0) {
			updatedInfo.append("\n\nМодели требующие обновления:\n");
		}
		for (std::map<DgnModelRefP, int>::iterator iter = updateRequiringModels.begin(); 
			iter != updateRequiringModels.end(); iter++) 
		{
			std::pair<DgnModelRefP, int> pair = *iter;
			openingsReqCount += pair.second;

			char fileName[256];
			mdlModelRef_getFileName(pair.first, fileName, 256);

			MSWChar displayName[512];
			//mdlModelRef_getModelName(pair.first, displayName);
			mdlModelRef_getDisplayName(pair.first, displayName, 512, NULL);
			std::wstring ws(displayName);

			//updatedInfo.append(fileName);
			updatedInfo.append(ws.begin(), ws.end());
			updatedInfo.append("\n");
		}

		char text[256];
		sprintf(text, "Обновлено проёмов: %i\n\nТребуется обновить в ссылочных моделях: %i (моделей: %i) ", 
			updatedElementsCount, openingsReqCount, updateRequiringModels.size());
		updatedInfo.insert(0, text);

		mdlOutput_messageCenter(MESSAGE_INFO, text, &updatedInfo[0], 
			strcmp(unparsedP, "silent") == 0 ? FALSE : TRUE);

		mdlUndo_endGroup();
	}
    mdlScanCriteria_free(scP);
}



void LocateFunc_providePathDescription
(
	DisplayPathP    path,           /* => display path */
	MSWCharP        description,    /* <=> description */
	MSWCharCP       refStr          /* => Ref string */
)
{
	/* !!!
		Перезаписывает atfflyover tooltip 
		только если библиотека довавлена в автозагрузку, порядок загрузки также
		определяет приоритет перезаписи "description" среди прочих библиотек,
		кот. также могут переопределять эту функцию	
	*/

	ElementRef  elemRef;
	MSElement   el;
	int         elSize;

	// Get the element
	elemRef = mdlDisplayPath_getCursorElem(path);
	elSize = elementRef_getElement(elemRef, &el, sizeof el);

	DgnModelRefP modelRefP = mdlDisplayPath_getPathRoot(path);

	MSElementDescrP edP = NULL;
	mdlElmdscr_getByElemRef(&edP, elemRef, modelRefP, FALSE, 0);

	if (TF_COMPOUND_CELL_ELM != mdlTFElmdscr_getApplicationType(edP)) {
		return;
	}

	bool isOpeningElement = false;
	std::wstring kks = L"";

	{	// проверка на DataGroup каталог
		using namespace Bentley::Building::Elements;
		BuildingEditElemHandle beeh(elemRef, ACTIVEMODEL);
		beeh.LoadDataGroupData();

		CCatalogCollection::CCollectionConst_iterator itr;
		CCatalogCollection& collection = beeh.GetCatalogCollection();
		for (itr = collection.Begin(); itr != collection.End(); itr++)
		{
			const std::wstring catalogInstanceName = itr->first;
			if (catalogInstanceName == L"Opening") {
				isOpeningElement = true;

				std::wstring itemXPath = L"Opening/@PartCode";

				CCatalogSchemaItemT*    pSchemaItem = NULL;
				if ((pSchemaItem = collection.FindDataGroupSchemaItem(itemXPath))) {
					kks = pSchemaItem->GetValue();
				}

				break;
			}
		}
	}

	if (!isOpeningElement) {
		return;
	}

	TFFrameList* frameList = mdlTFFrameList_constructFromElmdscr(edP);

	if (!frameList) {
		return;
	}
	
	double width = 0;
	double height = 0;

	TFFrame* frameP = mdlTFFrameList_getFrame(frameList);

	TFPerforatorList* perfoListP = NULL;
	if (SUCCESS == mdlTFFrame_getPerforatorList(frameP, &perfoListP)) {
		TFPerforator* perfoP = mdlTFPerforatorList_getPerforator(perfoListP);
		if (perfoP) {

			TFBrepList* brepListP = NULL;
			mdlTFPerforator_getProfile2(perfoP, &brepListP);

			DPoint3d* points;
			int size = 0;
			if (brepListP) {
				mdlTFBrepList_getVertexLocations(brepListP, &points, &size);

				if (size == 4) {
					width = mdlVec_distance(&points[0], &points[1]);
					height = mdlVec_distance(&points[1], &points[2]);
				}
			}
			if (points) {
				delete points;
			}
		}
	}

	mdlTFPerforatorList_free(&perfoListP);
	mdlTFFrameList_free(&frameList);

	mdlCnv_UORToMaster(&width, width, modelRefP);
	mdlCnv_UORToMaster(&height, height, modelRefP);
	
	MSWChar tooltip[256];
	swprintf(tooltip, 256, 
		L"Opening\n%s\n%.0fx%.0f", &kks[0], width, height);

	wcscpy(description, tooltip);
	
	//mdlOutput_flyoverMsgU("My info!");

	// description = L"!MY INFO";
	//wcscat(description, L"\n Opening: KKS=\"\" WxH=\"100x400\"");

	// wcscat(description, L"\nMY INFO");

	return;
}

bool isOpening(MSElementDescr* edP)
{
	if (TF_COMPOUND_CELL_ELM != mdlTFElmdscr_getApplicationType(edP)) {
		return false;
	}

	bool res = false;
	{	// проверка на DataGroup каталог
		using namespace Bentley::Building::Elements;
		BuildingEditElemHandle beeh(edP, false, true);
		beeh.LoadDataGroupData();

		CCatalogCollection::CCollectionConst_iterator itr;
		CCatalogCollection& collection = beeh.GetCatalogCollection();
		for (itr = collection.Begin(); itr != collection.End(); itr++)
		{
			const std::wstring catalogInstanceName = itr->first;
			if (catalogInstanceName == L"Opening") {
				res = true;

				//std::wstring itemXPath = L"Opening/@PartCode";
				//char kks[256];
				//CCatalogSchemaItemT*    pSchemaItem = NULL;
				//if ((pSchemaItem = collection.FindDataGroupSchemaItem(itemXPath))) {
				//	mdlCnv_convertUnicodeToMultibyte(
				//		&pSchemaItem->GetValue()[0], -1, kks, 200);
				//}

				//if (strcmp(kks, "10UBA19_PN0020B") == 0) {
				//	kks[0] = kks[0];
				//}

				break;
			}
		}
	}

	return res;
}


}