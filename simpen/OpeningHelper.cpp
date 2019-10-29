#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "ContourOpeningTool.h"

#include <interface\ElemHandle.h>
#include <tfpoly.h>
#include <elementref.h>

#include <buildingeditelemhandle.h>
#include <CatalogCollection.h>
#include <ListModelHelper.h>

#include <mdltfform.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <msoutput.fdf>

#include <mstrnsnt.fdf>
#include <msvar.fdf>
#include <msmisc.fdf>
#include <msvec.fdf>

#include <msdgnmodelref.fdf>
#include <mdltfmodelref.fdf>
#include <msinput.fdf>

#include  <mdltfframe.fdf>


#include <ditemlib.fdf>
#include <msdialog.fdf>
#include <mssystem.fdf>
#include <msparse.fdf>
#include <mslocate.fdf>
#include <msstate.fdf>

#include <mselemen.fdf>
#include <msundo.fdf>
#include <leveltable.fdf>
#include <msscancrit.fdf>


using Bentley::Ustn::Element::EditElemHandle;

namespace Openings {

StatusInt computeAndDrawTransient(Opening& opening) {

    //if (!opening.isValid()) {
    //    return ERROR;
    //}

    EditElemHandle bodyShape, perfoShape, crossFirst, crossSecond;
    
    if (SUCCESS != computeElementsForOpening(
        bodyShape, perfoShape, crossFirst, crossSecond, opening)) 
    {
        return ERROR;
    }

    //createPenetrFrame(bodyShape, perfoShape, crossFirst, crossSecond, 
    //    opening.direction, opening.distance, 0.0, opening.isSweepBi, 
    //    opening.isThroughHole);
    
    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        bodyShape.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        crossFirst.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        crossSecond.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);

    //char msg[50];
    //sprintf(msg, "Расстояние до противоположной стены = %.3f", distance);
    //mdlOutput_prompt(msg);

    return SUCCESS;
}

StatusInt computeAndAddToModel(Opening& opening) {

    //if (opening.isValid()) {
    //    return ERROR;
    //}
   
    EditElemHandle bodyShape, perfoShape, crossFirst, crossSecond;

    LevelID activeLaevelId;
    mdlLevel_getActive(&activeLaevelId);    
    const int COLOR_CURRENT = tcb->symbology.color;

    LevelID openingLevelId = getLevelIdByName(L"C-OPENING-BOUNDARY");
    if (openingLevelId != LEVEL_NULL_ID) {
        mdlLevel_setActive(openingLevelId);
        tcb->symbology.color = COLOR_BYLEVEL;
    }
    else {
        char* msg = "Не найден слой требуемый для создания проёма - <C-OPENING-BOUNDARY>";
        mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);
    }

    if (SUCCESS != computeElementsForOpening(
        bodyShape, perfoShape, crossFirst, crossSecond, opening))
    {
        // todo избавиться от дублирования
        mdlLevel_setActive(activeLaevelId);
        tcb->symbology.color = COLOR_CURRENT;
        return ERROR;
    }

    double distance = opening.isThroughHole ? 
        opening.distance : convertFromCExprVal(opening.userDistance);

    {   // Определение слоёв перекрестий
        LevelID levelId = getLevelIdByName(L"C-OPENING-SYMBOL");
        if (levelId != LEVEL_NULL_ID) {
            crossFirst.GetElementP()->ehdr.level = levelId;
            crossSecond.GetElementP()->ehdr.level = levelId;
        }
        else {
            char* msg = "Не найден слой требуемый для перекрестий проёма - <C-OPENING-SYMBOL>";
            mdlOutput_messageCenter(MESSAGE_WARNING, msg, msg, FALSE);            
        }
    }

    TFFrame* frameP = createPenetrFrame(bodyShape, perfoShape, crossFirst, crossSecond,
        opening.direction, distance, 0.0, true,
        opening.isThroughHole);
    
    if (frameP == NULL) {
        mdlLevel_setActive(activeLaevelId);
        tcb->symbology.color = COLOR_CURRENT;
        return ERROR;
    }

    StatusInt resStatus = ERROR;

    mdlUndo_startGroup(); 
    {
        DgnModelRefP modelRefP = ACTIVEMODEL;
        UInt32 fpos = mdlElement_getFilePos(FILEPOS_NEXT_NEW_ELEMENT, &modelRefP);
        
        if (SUCCESS == mdlTFModelRef_addFrame(ACTIVEMODEL, frameP))
        {

            if (fpos) {
                // todo научиться это делать в mdl:
                //ElementRef elemRef = mdlModelRef_getElementRef(ACTIVEMODEL, fpos);
                //std::wstring kksValue(&opening.kks[0], &opening.kks[50]);
                //setDataGroupInstanceValue(elemRef, kksValue);
                // записываем свойства в созданный CompoundCell

                char buf[256];
                sprintf(buf, "mdl keyin simpen.ui simpen.ui setdgdata %u %s", // sprintf(buf, "mdl keyin aepsim aepsim setdgdata %u %s"
                    fpos, opening.kks);
                mdlInput_sendSynchronizedKeyin((MSCharCP)buf, 0, 0, 0);
            }


            if (opening.isRequiredRemoveContour) {  
                // Удаляем контур:
                MSElementDescrP contourEdP = NULL;
                mdlElmdscr_getByElemRef(&contourEdP, opening.contourRef, ACTIVEMODEL, FALSE, 0);

                fpos = mdlElmdscr_getFilePos(contourEdP);
                mdlElmdscr_deleteByModelRef(contourEdP, fpos, ACTIVEMODEL, TRUE);
            }

            mdlOutput_prompt("Проём успешно создан");
            resStatus = SUCCESS;
        }
    }
    mdlUndo_endGroup();
    
    tcb->symbology.color = COLOR_CURRENT;  // 1. ! до активации слоя
    mdlLevel_setActive(activeLaevelId); // 2.

    return resStatus;
}


StatusInt computeElementsForOpening(EditElemHandleR outBodyShape,
    EditElemHandleR outPerfoShape, EditElemHandleR outCrossFirst,
    EditElemHandleR outCrossSecond, Opening& opening) 
{
    int tfTypes[2] = { TF_LINEAR_FORM_ELM, TF_SLAB_FORM_ELM };

    MSElementDescrP contourEdP = NULL;
    mdlElmdscr_getByElemRef(&contourEdP, opening.contourRef, ACTIVEMODEL, FALSE, 0);

    TFFormRecipeList* wall =
        findIntersectedTfFormWithElement(&contourEdP->el, tfTypes, 2);

    if (!wall) {
        return ERROR;
    }

    DPlane3d contourPlane;
    mdlElmdscr_extractNormal(
        &contourPlane.normal, &contourPlane.origin, contourEdP, NULL);


    double distanceReal = 0.0;
    {   // обновление и корректировка
        DVec3d directVector;
        if (SUCCESS != findDirectionVector(directVector, distanceReal, contourPlane, wall)) {
            return ERROR;
        }

        if (!opening.isThroughHole) {
            distanceReal = convertFromCExprVal(opening.userDistance);
            // directVector = contourPlane.normal;
        }
        else {
            opening.distance = distanceReal;
        }
        opening.direction = directVector;
        mdlVec_scaleToLength(&opening.direction, &opening.direction, distanceReal);
        distanceReal = abs(distanceReal);
    }

    //EditElemHandle shapeEeh;
    //ToEditHandle(shapeEeh, contourEdP);

    DPoint3d vertices[256];
    int numVerts;
    mdlLinear_extract(vertices, &numVerts, &contourEdP->el, ACTIVEMODEL);

    EditElemHandle shape;
    сreateStringLine(shape, vertices, numVerts);
    createShape(outPerfoShape, vertices, numVerts, true);

    if (!createBody(outBodyShape, shape, opening.direction, distanceReal, 0.0)) {
        return ERROR;
    }

    {   // Перекрестия:
        DPoint3d centroid;
        mdlMeasure_linearProperties(NULL, &centroid, NULL, NULL, NULL, NULL,
            NULL, NULL, contourEdP, fc_epsilon);

        createCross(outCrossFirst, centroid, vertices, numVerts);        

        // 2-ое перекрестие получаем проецируя 1-ое на противоположную грань
        DVec3d projVec = opening.direction;
        mdlVec_scaleToLength(&projVec, &projVec, distanceReal);
        mdlVec_projectPoint(&centroid, &centroid, &projVec, 1.0);
        for (int i = 0; i < numVerts; ++i) {
            mdlVec_projectPoint(&vertices[i], &vertices[i], &projVec, 1.0);
        }
        createCross(outCrossSecond, centroid, vertices, numVerts);
    }



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


    //TFBrepFaceList*       mdlTFBrepFaceList_getNext
    //(
    //    TFBrepFaceList const*       pThis
    //);

    //const int size = 4;
    //std::pair<FaceLabelEnum, FaceLabelEnum> faceLabels[size] =
    //{
    //    std::pair<FaceLabelEnum, FaceLabelEnum>(FaceLabelEnum_Left, FaceLabelEnum_Right),
    //    std::pair<FaceLabelEnum, FaceLabelEnum>(FaceLabelEnum_Right, FaceLabelEnum_Left),
    //    std::pair<FaceLabelEnum, FaceLabelEnum>(FaceLabelEnum_Top, FaceLabelEnum_Base),
    //    std::pair<FaceLabelEnum, FaceLabelEnum>(FaceLabelEnum_Base, FaceLabelEnum_Top),
    //};

    for (int i = FaceLabelEnum_None; i < FaceLabelEnum_Hidden; ++i)
    {
        TFBrepFaceList* faceListP =
            mdlTFBrepList_getFacesByLabel(brepListP, static_cast<FaceLabelEnum>(i));
        
        MSElementDescr* faceP = NULL;
        mdlTFBrepFaceList_getElmdscr(&faceP, faceListP, 0);
                
    //}

    //// Ищем поверхность стены, в плоскости которой лежит указанный контур
    //for (int i = 0; i < size; ++i) {
    //    std::pair<FaceLabelEnum, FaceLabelEnum> faces = faceLabels[i];

    //    TFBrepFaceList* brepFace = 
    //        mdlTFBrepList_getFacesByLabel(brepListP, faces.first);        
    //   
    //    MSElementDescr* faceP = NULL;
    //    mdlTFBrepFaceList_getElmdscr(&faceP, brepFace, 0);


        DPlane3d facePlane;
        if (SUCCESS != mdlElmdscr_extractNormal(&facePlane.normal, 
            &facePlane.origin, faceP, NULL))
        {
            continue;
        }

        if (planesAreMatch(contourPlane, facePlane)) {

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
                            distance = distanceToPlane(contourPlane.origin, forwardPlane);
                            outDirectionVec = computeVectorToPlane(contourPlane.origin, forwardPlane);
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

// Обновление всех проёмов модели
void cmdUpdateAll(char *unparsedP)
{
    // return;

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
