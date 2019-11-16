#include "ElementHelper.h"
#include "FontManager.h"

#include <math.h>

#include <mselmdsc.fdf>
#include <mselemen.fdf>
#include <mselementtemplate.fdf>
#include <leveltable.fdf>
#include <mskisolid.fdf>
#include <mscurrtr.fdf>
#include <mscell.fdf>
#include <mstmatrx.fdf>
#include <msscancrit.fdf>
#include <msvec.fdf>
#include <msinput.fdf>
#include <mscnv.fdf>

#include <mdltfform.fdf>
#include <mdltfframe.fdf>
#include <mdltfprojection.fdf>
#include <mdltfperfo.fdf>
#include <mdltfwstring.fdf>
#include <mdltfmodelref.fdf>
#include <mdltfelmdscr.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>


using Bentley::Ustn::Element::EditElemHandle;

bool operator ==(DPoint3d& pt0, DPoint3d& pt1) {
    return pt0.x == pt1.x && pt0.y == pt1.y && pt0.z == pt1.z;
}
bool operator !=(DPoint3d& pt0, DPoint3d& pt1) {
    return !(pt0 == pt1);
}

/*------------------------------------------------------------------------------
Создание простой линии по двум точкам
----------------------------------------------------------------------------- */
bool сreateLine(EditElemHandleR eehOut, DPoint3d* points) {
    MSElement el;
    mdlLine_create(&el, NULL, points);
    return ToEditHandle(eehOut, el);
}

/*------------------------------------------------------------------------------
Создание мультилинии
----------------------------------------------------------------------------- */
bool сreateStringLine(EditElemHandleR eehOut, DPoint3d* points, int numVerts, 
    const UInt32* weight) 
{
    MSElement el;
    mdlLineString_create(&el, NULL, points, numVerts);    

    bool res = ToEditHandle(eehOut, el);

    if (weight != NULL) {
        mdlElmdscr_setSymbology(eehOut.GetElemDescrP(), 0, 0, (UInt32*)weight, 0);
    } 

    return res;
}

/*------------------------------------------------------------------------------
Создание замкнутого контура
----------------------------------------------------------------------------- */
bool createShape(
    EditElemHandleR eehOut, DPoint3d* points, int numVerts, bool fillMode) 
{
    if (numVerts < 3) {
        return false;
    }

    MSElement el;
    if (SUCCESS == mdlShape_create(&el, NULL, points, numVerts, fillMode)) {
        return ToEditHandle(eehOut, el);
    }
    return false;
}

bool createBody(EditElemHandleR eehOut, EditElemHandleR shape, const DVec3d& vec, 
    double distance, double shell) 
{
    mdlKISolid_beginCurrTrans(MASTERFILE);      

    mdlCurrTrans_invScaleDoubleArray(&distance, &distance, 1);
    mdlCurrTrans_invScaleDoubleArray(&shell, &shell, 1);
    
    KIBODY  *kb_shape = NULL;
    MSElementDescr* edp = NULL;
    mdlElmdscr_duplicate(&edp, shape.GetElemDescrCP());
    
    StatusInt res = mdlKISolid_elementToBody(&kb_shape, edp, MASTERFILE);

    res = mdlKISolid_sweepBodyVector(kb_shape, &(DPoint3d)vec, distance, shell, 0.0);

    res = mdlKISolid_bodyToElement(&edp, kb_shape, FALSE, -1, NULL, MASTERFILE);

    mdlKISolid_freeBody(kb_shape);

    mdlKISolid_endCurrTrans(); // todo через final?
    
    MSElement e_Cell;
    MSElementDescr* edpCell = NULL;
    mdlCell_create(&e_Cell, NULL, NULL, FALSE);
    mdlElmdscr_new(&edpCell, NULL, &e_Cell);

    // append element to cell
    if (edp != NULL)
        mdlElmdscr_appendDscr(edpCell, edp);

    return res == SUCCESS && ToEditHandle(eehOut, edpCell);
}

// Конвертация в редактируемый сылочный тип на элемент
bool ToEditHandle(EditElemHandleR eehOut, MSElement element) {
    MSElementDescrP  elDescrP;
    mdlElmdscr_new(&elDescrP, NULL, &element);
    elDescrP->h.dgnModelRef = ACTIVEMODEL;
    //the second parameter lets the destructor free the descriptor 
    //(way cool I don't have to remember to free this now).
    eehOut.SetElemDescr(elDescrP, true, false);
    return eehOut.IsValid();
}

// Конвертация в редактируемый сылочный тип на элемент
bool ToEditHandle(EditElemHandleR eehOut, MSElementDescrP elDescrP) {
    elDescrP->h.dgnModelRef = ACTIVEMODEL;
    //the second parameter lets the destructor free the descriptor 
    //(way cool I don't have to remember to free this now).
    eehOut.SetElemDescr(elDescrP, true, false);
    return eehOut.IsValid();
}

bool CreateCell(
    EditElemHandleR eehOut, MSWCharCP name, DPoint3dP origin, BoolInt pointCell) {
    MSElement cell;
    mdlCell_create(&cell, name, origin, pointCell);
    return ToEditHandle(eehOut, cell);
}


bool Scale(EditElemHandleR eeh, DPoint3dCR point, double scale) {
    Transform tran;
    mdlTMatrix_getIdentity(&tran);
    // привязка к базовой точке
    mdlTMatrix_fromPointAndScale(&tran, &point, scale);
    // масштабирование
    return SUCCESS == mdlElmdscr_transform(eeh.GetElemDescrP(), &tran);
}

bool AddChildToCell(EditElemHandleR cell, EditElemHandleR child) {

    if (cell.GetElementType() != CELL_HEADER_ELM) {
        return false;
    }
    MSElementDescrP el_descrP = child.ExtractElemDescr();
    return SUCCESS == mdlElmdscr_appendDscr(cell.GetElemDescrP(), el_descrP);
}

ElementRef findIntersectedTFFormWithElement(
    const MSElementP elementP, const int typesNum, int tfType, ...)
{
    ElementRef result = NULL;

    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status;

    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();

    status = mdlScanCriteria_setElementTypeTest(scP, typeMask, sizeof(typeMask));
    status = mdlScanCriteria_setModel(scP, ACTIVEMODEL);

    { // ограничение по диапазоную точек:
        ScanRange scanRng = elementP->hdr.dhdr.range;
        mdlScanCriteria_setRangeTest(scP, &scanRng);
    }

    { // todo от этого кода можно избавится, перенеся нагрузку на callback function
        mdlScanCriteria_setReturnType(scP, MSSCANCRIT_RETURN_FILEPOS, FALSE, TRUE);

        UInt32 elemAddr[10];
        UInt32 eofPos = mdlElement_getFilePos(FILEPOS_EOF, NULL);
        UInt32 filePos = 0;
        UInt32 realPos = 0;
        status = ERROR;
        do {
            int scanWords = sizeof(elemAddr) / sizeof(short);

            status = mdlScanCriteria_scan(scP, elemAddr, &scanWords, &filePos);
            
            for (int i = 0; i < scanWords && elemAddr[i] < eofPos; ++i)
            {
                MSElementDescr* edP = NULL;
                if (FILEPOS_EOF != 
                    mdlElmdscr_read(&edP, elemAddr[i], 0, FALSE, &realPos))
                {
                    bool isTypeMatched = false;

                    int elemTfType = mdlTFElmdscr_getApplicationType(edP);
                             
                    int* typeP = &tfType;
					int count = typesNum;
                    while (count--) {
                        if (elemTfType == *typeP) {
                            result = edP->h.elementRef;
                            isTypeMatched = true;
                            break;
                        }                        
                        ++typeP;
                    }

                    mdlElmdscr_freeAll(&edP);

                    if (isTypeMatched) {
                        break;
                    }
                }
            }
        } while (status == BUFF_FULL);
    }
    mdlScanCriteria_free(scP);

    return result;
}

bool planesAreMatch(const DPlane3d& first, const DPlane3d& second) {

    if (!planesAreParallel(first, second)) {
        return false;
    }

    DPoint3d projFirstToSecondPoint;
    mdlVec_projectPointToPlane(&projFirstToSecondPoint, &(DPoint3d)first.origin,
        &(DPoint3d)second.origin, &(DVec3d)second.normal);

    // округляем расстояние до целого:
    double distance = 
        (double)(int)mdlVec_distance(&first.origin, &projFirstToSecondPoint);

    return distance == 0.0;
}

bool planesAreParallel(const DPlane3d& first, const DPlane3d& second) {

    DVec3d secondNegateNormal;
    mdlVec_negate(&secondNegateNormal, &second.normal);

    return (TRUE == mdlVec_areParallel(&first.normal, &second.normal) ||
        TRUE == mdlVec_areParallel(&first.normal, &secondNegateNormal));
}

double distanceToPlane(const DPoint3d& point, const DPlane3d& plane) {

    DPoint3d projPoint;    
    mdlVec_projectPointToPlane(&projPoint, &(DPoint3d)point, 
        &(DPoint3d)plane.origin, &(DVec3d)plane.normal);

    return mdlVec_distance(&point, &projPoint);
}

DVec3d computeVector(const DPoint3d& start, const DPoint3d& target) {

    DVec3d res;
    mdlVec_subtractDPoint3dDPoint3d(&res, &target, &start);
    return res;
}

DVec3d computeVectorToPlane(const DPoint3d& point, const DPlane3d& plane) {

    DPoint3d projPoint;
    mdlVec_projectPointToPlane(&projPoint, &(DPoint3d)point,
        &(DPoint3d)plane.origin, &(DVec3d)plane.normal);

    DVec3d res;
    mdlVec_subtractDPoint3dDPoint3d(&res, &projPoint, &point);

    return res;
}

//LevelID getLevelIdByName(MSWCharCP name)
//{
//    LevelID activeLevelId, levelId;
//    mdlLevel_getActive(&activeLevelId);
//
//    if (SUCCESS != mdlLevel_getIdFromName(&levelId, 
//        ACTIVEMODEL, LEVEL_NULL_ID, name))
//    {
//        mdlLevel_setActiveByName(LEVEL_NULL_ID, name);
//        if (SUCCESS != mdlLevel_getIdFromName(&levelId,
//            ACTIVEMODEL, LEVEL_NULL_ID, name)) {
//            return LEVEL_NULL_ID;
//        }
//        // возвращаем
//        mdlLevel_setActive(activeLevelId);
//    }
//    return levelId;
//}

TFFrame* createPenetrFrame(
    EditElemHandleR shapeBody, EditElemHandleR shapePerf,
    DVec3dR vec, double distance, double shell, 
    bool isSweepBi, bool isPolicyThrough)
{
    TFFrame* frameP = NULL;

    EditElemHandle body;
    if (!createBody(body, shapeBody, vec, distance, shell)) {
        return false;
    }

    MSElementDescr *penToAdd = NULL;

    mdlElmdscr_duplicate(&penToAdd, body.GetElemDescrCP());

    //UInt32 weight = 2;
    //mdlElmdscr_setSymbology(penToAdd, 0, 0, &weight, 0);

    TFFrameList*      pFrameNode = NULL;
    TFPerforatorList* perfoListP = NULL;

    Transform tm;
    mdlTMatrix_getIdentity(&tm);

    pFrameNode = mdlTFFrameList_construct();
    frameP = mdlTFFrameList_getFrame(pFrameNode);

    mdlTFFrame_add3DElmdscr(frameP, penToAdd); // p3DEd is consumed, no need to free it

    mdlTFFrame_setSenseDistance2(frameP, distance);
    
    { // ПЕРФОРАТОР
        mdlTFPerforatorList_constructFromElmdscr(&perfoListP, shapePerf.GetElemDescrCP(), 0, 0., 0);

        PerforatorSweepModeEnum sweep = isSweepBi ?
            PerforatorSweepModeEnum_Bi : PerforatorSweepModeEnum_Uni;

        PerforatorPolicyEnum policy = isPolicyThrough ?
            PerforatorPolicyEnum_ThroughHoleWithinSenseDist :
            PerforatorPolicyEnum_BlindHoleWithinSenseDist;

        mdlTFPerforatorList_setSenseDist(perfoListP,
            isPolicyThrough ? (1.01 * distance) : // НВС (может и не влияет...)
            distance);

        mdlTFPerforatorList_setSweepMode(perfoListP, sweep);
        mdlTFPerforatorList_setPolicy(perfoListP, policy);
      
        mdlTFFrame_setPerforatorList(frameP, &perfoListP); // pPerforatorList is consumed, no need to free it
        mdlTFPerforatorList_setIsActive(perfoListP, TRUE);
    }

    TFWStringList* pNameNodeW =
        mdlTFWStringList_constructFromCharString("Opening");
    mdlTFFrame_setName(frameP, mdlTFWStringList_getWString(pNameNodeW));
    mdlTFWStringList_free(&pNameNodeW);

    mdlTFFrame_synchronize(frameP);

    { // !!! без этого не работает:
      // create openings in form that are found within the sense distance 
      // of the frame's perforators
        mdlTFModelRef_updateAutoOpeningsByFrame(
            ACTIVEMODEL, frameP, 1, 0, FramePerforationPolicyEnum_None);
    }

    return frameP; // todo
}

StatusInt appendToProjection(TFFrame* frameP, MSElementDescrCP edCP) {
	TFProjectionList* projListP = NULL;
	TFProjectionList* projNodeP = NULL;

	StatusInt res = ERROR;

	if (!(projListP = mdlTFFrame_getProjectionList(frameP))) {
		projListP = mdlTFProjectionList_construct();
		projNodeP = projListP;
	}
	else if (SUCCESS == 
		mdlTFProjectionList_append(&projListP, mdlTFProjectionList_construct())) 
	{
		projNodeP = mdlTFProjectionList_getNext(projListP);
	}

	if (projNodeP) {
		TFProjection* projP = mdlTFProjectionList_getProjection(projNodeP);

		MSElementDescrP edP = NULL;
		mdlElmdscr_duplicate(&edP, edCP);

		res = mdlTFProjection_setEmbeddedElmdscr(projP, edP, TRUE);
		if (res == SUCCESS) {
			res = mdlTFFrame_setProjectionList(frameP, projListP); 
			// projListP is consumed, no need to free it
		}
	}
	return res;
}

void createCross(EditElemHandleR outCross, 
    const DPoint3d& centroid, const DPoint3d* vertices, int numVerts) {
    
    const int size = 256; // max 128 вершины
    DPoint3d crossPnts[size];

    int j = -1;
    for (int i = 0; i < numVerts && j < size - 1; ++i) {
        crossPnts[++j] = centroid;
        crossPnts[++j] = vertices[i];
    }

    UInt32 weight = 0;
    сreateStringLine(outCross, crossPnts, j + 1, &weight);
}

StatusInt getFacePlaneByLabel(DPlane3dR outPlane, 
    MSElementDescrCP tfformEdP, FaceLabelEnum label)
{
    StatusInt status = ERROR;

    MSElementDescrP edP = NULL;
    mdlElmdscr_duplicate(&edP, tfformEdP);
    
    TFFormRecipeList* frListP = NULL;
    if (BSISUCCESS == mdlTFFormRecipeList_constructFromElmdscr(&frListP, edP)) {
        TFFormRecipe* frP = mdlTFFormRecipeList_getFormRecipe(frListP);

        TFBrepList* blistP = NULL;
        if (BSISUCCESS == mdlTFFormRecipe_getBrepList(frP, &blistP, 0, 0, 0)) {

            TFBrepFaceList* faceListP = mdlTFBrepList_getFacesByLabel(blistP, label);
            if (faceListP) {
                MSElementDescr* faceP;
                mdlTFBrepFaceList_getElmdscr(&faceP, faceListP, 0);

                if (faceP) {
                    status = mdlElmdscr_extractNormal(&outPlane.normal, 
                        &outPlane.origin, faceP, NULL);
                    mdlElmdscr_freeAll(&faceP);
                }

                mdlTFBrepFaceList_free(&faceListP);
            }
        }
        if (frListP) mdlTFFormRecipeList_free(&frListP);        
    }

    if (edP) mdlElmdscr_freeAll(&edP);    

    return status;
}