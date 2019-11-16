#include "ElementHelper.h"
#include "OpeningHelper.h"

#include <buildingeditelemhandle.h>
#include <CatalogCollection.h>
#include <elementref.h>
#include <interface\ElemHandle.h>
#include <ListModelHelper.h>

#include <ditemlib.fdf>
#include <leveltable.fdf>

#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <mdltfform.fdf>
#include <mdltfframe.fdf>
#include <mdltfmodelref.fdf>

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
#include <mdltfprojection.fdf>

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

StatusInt computeAndAddToModel(Opening& opening) {

    EditElemHandle bodyShape, perfoShape, crossFirst, crossSecond;
		
	if (SUCCESS != computeElementsForOpening(
        bodyShape, perfoShape, crossFirst, crossSecond, opening,
		Opening::LEVEL_NAME, Opening::LEVEL_SYMBOL_NAME))
    {
        return ERROR;
    }

	LevelID lvlId;
    mdlLevel_getActive(&lvlId);
	const LevelID activeLevelId = lvlId;
    const int COLOR_ACTIVE = tcb->symbology.color;

	tcb->symbology.color = COLOR_BYLEVEL;
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
		tcb->symbology.color = COLOR_ACTIVE;
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
        
        if (SUCCESS == mdlTFModelRef_addFrame(ACTIVEMODEL, frameP))
        {
            if (fpos) {
                // TODO научиться это делать в mdl:

                //ElementRef elemRef = mdlModelRef_getElementRef(ACTIVEMODEL, fpos);
                //std::wstring kksValue(&opening.kks[0], &opening.kks[50]);
                //setDataGroupInstanceValue(elemRef, kksValue);
                // записываем свойства в созданный CompoundCell

                char buf[256];
                sprintf(buf, "mdl keyin simpen.ui simpen.ui setdgdata %u %s", // sprintf(buf, "mdl keyin aepsim aepsim setdgdata %u %s"
                    fpos, opening.getKKS());
                mdlInput_sendSynchronizedKeyin((MSCharCP)buf, 0, 0, 0);
            }

            if (opening.getTask().isRequiredRemoveContour && 
                !elementRef_isEOF(opening.contourRef)) 
            {  
                // todo проверить на контур из референса

                // Удаляем контур:
                MSElementDescrP contourEdP = NULL;
                mdlElmdscr_getByElemRef(
                    &contourEdP, opening.contourRef, ACTIVEMODEL, FALSE, 0);

                fpos = mdlElmdscr_getFilePos(contourEdP);
                mdlElmdscr_deleteByModelRef(contourEdP, fpos, ACTIVEMODEL, TRUE);
            }

            mdlOutput_prompt("Проём успешно создан");
            resStatus = SUCCESS;
        }
    }
        
    tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
    mdlLevel_setActive(activeLevelId); // 2.

    return resStatus;
}


StatusInt computeElementsForOpening(EditElemHandleR outBodyShape,
    EditElemHandleR outPerfoShape, EditElemHandleR outCrossFirst,
    EditElemHandleR outCrossSecond, Opening& opening, 
	MSWCharCP boundaryLevel, MSWCharCP symbolLevel)
{
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

    EditElemHandle contour;
    createShape(contour, &opening.contourPoints[0], opening.contourPoints.size(), false);

   // int tfTypes[2] = { TF_LINEAR_FORM_ELM, TF_SLAB_FORM_ELM };
    TFFormRecipeList* wall = NULL;
        // findIntersectedTFFormWithElement(contour.GetElementP(), tfTypes, 2);

    if (!wall) {
        EditElemHandle eeh = 
            EditElemHandle(OpeningTask::getInstance().tfFormRef, ACTIVEMODEL);

        if (BSISUCCESS != mdlTFFormRecipeList_constructFromElmdscr(
            &wall, eeh.GetElemDescrP())) 
        {
			tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
			mdlLevel_setActive(activeLevelId); // 2.
            return ERROR;
        }
    }

    DPlane3d contourPlane;
    mdlElmdscr_extractNormal(
        &contourPlane.normal, &contourPlane.origin, contour.GetElemDescrP(), NULL);
    
    double distance = 0.0;
    {   // обновление и корректировка
        DVec3d directVector;
        if (SUCCESS != 
			findDirectionVector(directVector, distance, contourPlane, wall))
		{
			tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
			mdlLevel_setActive(activeLevelId); // 2.
			return ERROR;
        }

        if (!opening.getTask().isThroughHole) {
            distance = opening.getDistance();
            // directVector = contourPlane.normal;
        }
        else {
            opening.setDistance(distance);
        }
        opening.direction = directVector;
        mdlVec_scaleToLength(&opening.direction, &opening.direction, 
            distance != 0.0 ? distance : 1.0);
        // если оставить знак, то направление инвертируется, т.к. его уже задаёт вектор
        distance = abs(distance);
    }

    createShape(contour, &opening.contourPoints[0], opening.contourPoints.size(), false);

    DPoint3d vertices[256];
    int numVerts;
    mdlLinear_extract(vertices, &numVerts, contour.GetElementP(), ACTIVEMODEL);
    //mdlLinear_extract(vertices, &numVerts, &contourEdP->el, ACTIVEMODEL);

    EditElemHandle shape;
    сreateStringLine(shape, vertices, numVerts);
    createShape(outPerfoShape, vertices, numVerts, true);

    if (!createBody(outBodyShape, shape, opening.direction, distance, 0.0)) 
	{
		tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
		mdlLevel_setActive(activeLevelId); // 2.
        return ERROR;
    }

	if (SUCCESS != mdlLevel_setActiveByName(LEVEL_NULL_ID, symbolLevel))
	{  
		//char* msg = "Не найден слой требуемый для создания проёма - <C-OPENING-BOUNDARY>";
		//mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
	}

    {   // Перекрестия:
        DPoint3d centroid;
        mdlMeasure_linearProperties(NULL, &centroid, NULL, NULL, NULL, NULL,
            NULL, NULL, contour.GetElemDescrP(), fc_epsilon);
        //NULL, NULL, contourEdP, fc_epsilon);

        createCross(outCrossFirst, centroid, vertices, numVerts);        

        // 2-ое перекрестие получаем проецируя 1-ое на противоположную грань
        DVec3d projVec = opening.direction;
        mdlVec_scaleToLength(&projVec, &projVec, distance);
        mdlVec_projectPoint(&centroid, &centroid, &projVec, 1.0);
        for (int i = 0; i < numVerts; ++i) {
            mdlVec_projectPoint(&vertices[i], &vertices[i], &projVec, 1.0);
        }
        createCross(outCrossSecond, centroid, vertices, numVerts);
    }
    
	tcb->symbology.color = COLOR_ACTIVE;  // 1. ! до активации слоя
	mdlLevel_setActive(activeLevelId); // 2.

    return SUCCESS;
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

            if (!isForwardFound) {
                distance = 0.0;
                outDirectionVec = contourPlane.normal;
            }
            
            return SUCCESS;
        }

    }

    return ERROR;
}

//

bool setDataGroupInstanceValue(const ElementRef& elemRef, const std::wstring& value) {
    if (NULL == elemRef) {
        return false;
    }

    /*!
    To iterate through all the CCatalogInstances in the collection see the following:
    \code
    CCatalogCollection::CCollectionConst_iterator itr;
    for (itr = GetCatalogCollection().Begin(); itr != GetCatalogCollection().End(); itr++)
    CCatalogInstanceT const& pCatalogInstance = *itr->second;
    \endcode
    */

    Bentley::Building::Elements::BuildingEditElemHandle beeh(elemRef, ACTIVEMODEL);
    beeh.LoadDataGroupData();
    beeh.LoadDataGroupFromDisk();

    CCatalogCollection& collection = beeh.GetCatalogCollection();

    collection.SetIsValid(true);
    if (false) {

        collection.InsertDataGroupCatalogInstance(L"Opening", L"Opening");

        beeh.GetCatalogCollection().UpdateInstanceDataDefaults(
            Opening::CATALOG_INSTANCE_NAME);

        std::wstring const      itemXPath = L"Opening/Opening/@PartCode";
        //std::wstring const      itemValue = L"Wood";
        CCatalogSchemaItemT*    pSchemaItem = NULL;
        if (NULL != (pSchemaItem = beeh.GetCatalogCollection().FindDataGroupSchemaItem(itemXPath)))
            pSchemaItem->SetValue(value);

        return SUCCESS == beeh.Rewrite();
    }

    return false;
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

int scanOpenings(
    MSElementDescr  *edDstP,
    void*  param,
    ScanCriteria    *pScanCriteria
) {

    TFFrameList* flP = mdlTFFrameList_constructFromElmdscr(edDstP);
    if (flP) {
        mdlTFFrameList_free(&flP);

        //ret = mdlModify_elementDescr2(&edDstP, ACTIVEMODEL, MODIFY_REQUEST_NOHEADERS, eleFunc, 0, 0);
        mdlModify_elementSingle(ACTIVEMODEL, mdlElmdscr_getFilePos(edDstP),
            MODIFY_REQUEST_NOHEADERS, MODIFY_ORIG, modifyFunc, 0, 0);
    }

    return 0;
}

// todo Обновление всех проёмов модели
void cmdUpdateAll(char *unparsedP)
{
    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status;

    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();
    status = mdlScanCriteria_setReturnType(scP, MSSCANCRIT_ITERATE_ELMDSCR, FALSE, TRUE);
    status = mdlScanCriteria_setElmDscrCallback(scP, (PFScanElemDscrCallback)scanOpenings, 0);
    status = mdlScanCriteria_setElementTypeTest(scP, typeMask, sizeof(typeMask));
    status = mdlScanCriteria_setCellNameTest(scP, L"Opening");
    status = mdlScanCriteria_setModel(scP, ACTIVEMODEL);
    status = mdlScanCriteria_scan(scP, NULL, NULL, NULL);
    status = mdlScanCriteria_free(scP);
}

}