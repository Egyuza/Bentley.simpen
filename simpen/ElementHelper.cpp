#include "ElementHelper.h"
#include "FontManager.h"

#include <math.h>

#include <CatalogCollection.h>

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

#include <mstrnsnt.fdf>
#include <msvar.fdf>
#include <msrmatrx.fdf>


using Bentley::Ustn::Element::EditElemHandle;

bool operator ==(DPoint3d& pt0, DPoint3d& pt1) {
    return pt0.x == pt1.x && pt0.y == pt1.y && pt0.z == pt1.z;
}
bool operator !=(DPoint3d& pt0, DPoint3d& pt1) {
    return !(pt0 == pt1);
}

DPoint3d getMiddle(DPoint3d* targetP, DPoint3d* baseP)
{
	DPoint3d res;
	DVec3d vec;
	mdlVec_subtractDPoint3dDPoint3d(&vec, targetP, baseP);
	mdlVec_projectPoint(&res, baseP, &vec, 0.5);
	return res;
}

/*------------------------------------------------------------------------------
�������� ������� ����� �� ���� ������
----------------------------------------------------------------------------- */
bool createLine(EditElemHandleR eehOut, DPoint3d* points) {
    MSElement el;
    mdlLine_create(&el, NULL, points);
    return ToEditHandle(eehOut, el);
}

/*------------------------------------------------------------------------------
�������� �����������
----------------------------------------------------------------------------- */
bool createStringLine(EditElemHandleR eehOut, DPoint3d* points, int numVerts, 
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
�������� ���������� �������
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

/*------------------------------------------------------------------------------
�������� ����������
----------------------------------------------------------------------------- */
bool createCircle(EditElemHandleR eehOut,
    DPoint3dCR center, double radius, int fillMode) {
    MSElement el;

    //DPoint3d pts[3] = { center, center, center };
    //pts[0].x = center.x - radius;
    //pts[0].y = center.y;
    //pts[1].x = center.x;
    //pts[1].y = center.y + radius;
    //pts[2].x = center.x + radius;
    //pts[2].y = center.y;

    /* create the pushpin outline */
    //mdlCircle_createBy3Pts(&el, NULL, pts, fillMode);

    mdlEllipse_create(&el,
        NULL, &DPoint3d(center), radius, radius, NULL, fillMode);

    return ToEditHandle(eehOut, el);
}

bool createArcEllipse(EditElemHandleR eehOut, DPoint3dCR center,
	double radius1, double radius2, RotMatrix* rotP)
{
	MSElement el;
	//RotMatrix rotMatrix;
	// mdlRMatrix_normalize(&rotMatrix, &rot);
	mdlArc_create(&el,
		NULL, &DPoint3d(center), radius1, radius2, rotP, 0, fc_2pi);

	return ToEditHandle(eehOut, el);
}

/*------------------------------------------------------------------------------
�������� �������
----------------------------------------------------------------------------- */
bool createEllipse(EditElemHandleR eehOut, DPoint3dCR center, 
    double radius1, double radius2, RotMatrix* rotP, int fillMode) 
{
    MSElement el;
    //RotMatrix rotMatrix;
    // mdlRMatrix_normalize(&rotMatrix, &rot);
    mdlEllipse_create(&el,
        NULL, &DPoint3d(center), radius1, radius2, rotP, fillMode);

    return ToEditHandle(eehOut, el);
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

    mdlKISolid_endCurrTrans(); // todo ����� final?
    
    MSElement e_Cell;
    MSElementDescr* edpCell = NULL;
    mdlCell_create(&e_Cell, NULL, NULL, FALSE);
    mdlElmdscr_new(&edpCell, NULL, &e_Cell);

    // append element to cell
    if (edp != NULL)
        mdlElmdscr_appendDscr(edpCell, edp);

    return res == SUCCESS && ToEditHandle(eehOut, edpCell);
}

// ����������� � ������������� �������� ��� �� �������
bool ToEditHandle(EditElemHandleR eehOut, MSElement element) {
    MSElementDescrP  elDescrP;
    mdlElmdscr_new(&elDescrP, NULL, &element);
    elDescrP->h.dgnModelRef = ACTIVEMODEL;
    //the second parameter lets the destructor free the descriptor 
    //(way cool I don't have to remember to free this now).
    eehOut.SetElemDescr(elDescrP, true, false);
    return eehOut.IsValid();
}

// ����������� � ������������� �������� ��� �� �������
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
    // �������� � ������� �����
    mdlTMatrix_fromPointAndScale(&tran, &point, scale);
    // ���������������
    return SUCCESS == mdlElmdscr_transform(eeh.GetElemDescrP(), &tran);
}

bool AddChildToCell(EditElemHandleR cell, EditElemHandleR child) {

    if (cell.GetElementType() != CELL_HEADER_ELM) {
        return false;
    }
    MSElementDescrP el_descrP = child.ExtractElemDescr();
    return SUCCESS == mdlElmdscr_appendDscr(cell.GetElemDescrP(), el_descrP);
}


bool getKISolidCenterPoint(DPoint3d* center, const MSElementDescrP elemdP) {
	bool res = false;

	mdlKISolid_beginCurrTrans(ACTIVEMODEL);

	KIBODY* bodyP = NULL;
	mdlKISolid_elementToBody(&bodyP, elemdP, ACTIVEMODEL);
	
	DPoint3d lo, hi;
	if (SUCCESS == mdlKISolid_getBodyBox(&lo, &hi, bodyP)) {
		DVec3d kiVec;
		mdlVec_subtractDPoint3dDPoint3d(&kiVec, &hi, &lo);
		mdlVec_projectPoint(center, &lo, &kiVec, 0.5);
		res = true;
	}
	mdlKISolid_freeBody(bodyP);
	mdlKISolid_endCurrTrans();
	return res;
}

int getIntersectPointsCount(const MSElementDescrP edP1, const MSElementDescrP edP2)
{
	int res = 0;

	KIBODY* pBody1 = NULL;
	KIBODY* pBody2 = NULL;
	Transform transform1, transform2, targetTransform, toolTransform;

	mdlKISolid_beginCurrTrans(ACTIVEMODEL);

	StatusInt ret1 =
		mdlKISolid_elementToBody2(&pBody1, &transform1, edP1, ACTIVEMODEL, 1L, FALSE);
	StatusInt ret2 =
		mdlKISolid_elementToBody2(&pBody2, &transform2, edP2, ACTIVEMODEL, 1L, FALSE);

	if (ret1 == SUCCESS && ret2 == SUCCESS)
	{
		if (mdlTMatrix_getInverse(&targetTransform, &transform1) == SUCCESS)
		{
			mdlTMatrix_multiply(&toolTransform, &targetTransform, &transform2);
			if (mdlKISolid_applyTransform(pBody2, &toolTransform) == SUCCESS)
			{
				int ret = mdlKISolid_intersect(pBody1, pBody2);
				if (ret != SUCCESS)
				{
					mdlKISolid_freeBody(pBody1);
					mdlKISolid_freeBody(pBody2);
				}
				else
				{
					KIENTITY_LIST* listP = NULL;
					mdlKISolid_listCreate(&listP);
					mdlKISolid_getVertexList(listP, pBody1);

					mdlKISolid_listCount(&res, listP);
					mdlKISolid_listDelete(&listP);

					mdlKISolid_freeBody(pBody1);
					// never free/or use pBody2 after success
				}
			}
		}
	}

	mdlKISolid_endCurrTrans();
	return res;
}

bool isKISolidPointInside(const DPoint3d& kipoint, const MSElementDescrP edP)
{
	DPoint3d point = kipoint;
	int inBody = POINT_UNKNOWN;

	mdlKISolid_beginCurrTrans(ACTIVEMODEL);

	KIBODY* bodyP = NULL;
	if (SUCCESS == mdlKISolid_elementToBody(&bodyP, edP, ACTIVEMODEL))
	{
		mdlKISolid_pointInBody(&inBody, &point, bodyP);
		mdlKISolid_freeBody(bodyP);
	}
	mdlKISolid_endCurrTrans();

	return inBody == POINT_INSIDE;
}

bool isCenterIsInsideOfAnother(const MSElementDescrP centerOwnerEdP, const MSElementDescrP scanEdP)
{
	DPoint3d center;

	if (getKISolidCenterPoint(&center, centerOwnerEdP)) {

		return isKISolidPointInside(center, scanEdP);
	}
	return false;	
}

ElementRef findIntersectedByTFType(
    const MSElementDescrP elemdP, const int typesNum, int tfType, ...)
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

	DPoint3d centerKI;
	mdlVec_zero(&centerKI);
	getKISolidCenterPoint(&centerKI, elemdP);

	DPoint3d centerPoint;
    {	// ����������� ��������� ������:
        ScanRange scanRng = elemdP->el.hdr.dhdr.range;

		DVector3d range;
		mdlCnv_scanRangeToDRange(&range, &scanRng);

		DVec3d rngVec;
		mdlVec_subtractDPoint3dDPoint3d(&rngVec, &range.end, &range.org);

		DPoint3d middle;
		mdlVec_projectPoint(&middle, &range.org, &rngVec, 0.5);
		centerPoint = middle;		
		
		// ��������� � ����������� ��������� - ���� ��������� ��������
		range.org =
		range.end = middle;

		mdlCnv_dRangeToScanRange(&scanRng, &range);
        mdlScanCriteria_setRangeTest(scP, &scanRng);
    }

    { // todo �� ����� ���� ����� ����������, �������� �������� �� callback function
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
				ElementRef candidate = NULL;
				int candidateIntersects = 0;

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
							
							if (isCenterIsInsideOfAnother(elemdP, edP))
							{
								result = edP->h.elementRef;
								isTypeMatched = true;
								break;
							}
							
							int intersectPoints = 
								getIntersectPointsCount(elemdP, edP);

							if (intersectPoints > candidateIntersects) {
								candidateIntersects = intersectPoints;
								candidate = edP->h.elementRef;
							}						
                        }
                        ++typeP;
                    }

                    mdlElmdscr_freeAll(&edP);

                    if (isTypeMatched) {
                        break;
                    }
                }

				if (candidate != NULL && candidateIntersects > 0) {
					result = candidate;
				}
            }
        } while (status == BUFF_FULL);
    }
    mdlScanCriteria_free(scP);

    return result;
}

ElementRef tfFindIntersectedByDGInstance(const MSElementP elementP, 
	const int typesCount, std::wstring dgInstNames[], std::wstring& outMatchedInstName)
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

	{ // ����������� �� ���������� �����:
		ScanRange scanRng = elementP->hdr.dhdr.range;
		mdlScanCriteria_setRangeTest(scP, &scanRng);
	}

	{ // todo �� ����� ���� ����� ���������, �������� �������� �� callback function
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


					Bentley::Building::Elements::
						BuildingEditElemHandle beeh(edP->h.elementRef, ACTIVEMODEL);
					beeh.LoadDataGroupData();

					CCatalogCollection::CCollectionConst_iterator itr;
					for (itr = beeh.GetCatalogCollection().Begin(); itr != beeh.GetCatalogCollection().End(); itr++)
					{
						const std::wstring catalogInstanceName = itr->first;						

						for (int ii = 0; ii < typesCount; ++ii) {
							if (catalogInstanceName == dgInstNames[ii]) {
								outMatchedInstName = dgInstNames[ii];
								result = edP->h.elementRef;
								isTypeMatched = true;
								break;
							}
						}
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

    // ��������� ���������� �� ������:
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

TFFrame* createPenetrFrame(
    EditElemHandleR body, EditElemHandleR shapePerf,
    DVec3dR vec, double distance, double shell, 
    bool isSweepBi, bool isPolicyThrough)
{
	if (!body.IsValid()) {
		return NULL;
	}

    TFFrame* frameP = NULL;

    MSElementDescr *penToAdd = NULL;
    mdlElmdscr_duplicate(&penToAdd, body.GetElemDescrCP());

    TFFrameList*      frameListP = NULL;
    TFPerforatorList* perfoListP = NULL;

    Transform tm;
    mdlTMatrix_getIdentity(&tm);

    frameListP = mdlTFFrameList_construct();
    frameP = mdlTFFrameList_getFrame(frameListP);

    mdlTFFrame_add3DElmdscr(frameP, penToAdd); // p3DEd is consumed, no need to free it

    mdlTFFrame_setSenseDistance2(frameP, distance);
    
    { // ����������
        mdlTFPerforatorList_constructFromElmdscr(&perfoListP, shapePerf.GetElemDescrCP(), 0, 0., 0);

        PerforatorSweepModeEnum sweep = isSweepBi ?
            PerforatorSweepModeEnum_Bi : PerforatorSweepModeEnum_Uni;

        PerforatorPolicyEnum policy = isPolicyThrough ?
            PerforatorPolicyEnum_ThroughHoleWithinSenseDist :
            PerforatorPolicyEnum_BlindHoleWithinSenseDist;

        mdlTFPerforatorList_setSenseDist(perfoListP,
            isPolicyThrough ? (1.01 * distance) : // ��� (����� � �� ������...)
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
	mdlTFFrameList_synchronize(frameListP);

    { // !!! ��� ����� �� ��������:
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
    
    const int size = 256; // max 128 �������
    DPoint3d crossPnts[size];

    int j = -1;
    for (int i = 0; i < numVerts && j < size - 1; ++i) {
        crossPnts[++j] = centroid;
        crossPnts[++j] = vertices[i];
    }

    UInt32 weight = 0;
    createStringLine(outCross, crossPnts, j + 1, &weight);
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


/*------------------------------------------------------------------------------
��������� �������� DataGroup ��������
----------------------------------------------------------------------------- */
bool setDataGroupInstanceValue(
	Bentley::Building::Elements::BuildingEditElemHandle& beeh,
	const std::wstring& catalogType, const std::wstring& catalogInstance,
	const std::wstring& itemXPath, const std::wstring& value)
{
	//if (elemRef == NULL || elementRef_isEOF(elemRef)) {
	//	return false;
	//}

	/*!
	To iterate through all the CCatalogInstances in the collection see the following:
	\code
	CCatalogCollection::CCollectionConst_iterator itr;
	for (itr = GetCatalogCollection().Begin(); itr != GetCatalogCollection().End(); itr++)
	CCatalogInstanceT const& pCatalogInstance = *itr->second;
	\endcode
	*/

	//Bentley::Building::Elements::BuildingEditElemHandle beeh(elemRef, modelRefP);
	beeh.LoadDataGroupData();

	CCatalogCollection& collection = beeh.GetCatalogCollection();

	bool matches = false;
	CCatalogCollection::CCollectionConst_iterator itr = NULL;
	for (itr = collection.Begin(); itr != collection.End(); itr++) {
		CCatalogInstanceT const& pCatalogInstance = *itr->second;

		if (pCatalogInstance.GetCatalogAppTypeName() == catalogType &&
			pCatalogInstance.GetCatalogInstanceName() == catalogInstance)
		{
			matches = true;
			break;
		}
	}

	if (!matches) {
		collection.InsertDataGroupCatalogInstance(catalogType, catalogInstance);
		collection.UpdateInstanceDataDefaults(catalogInstance);
	}

	bool res = false;

	CCatalogSchemaItemT*    pSchemaItem = NULL;
	if ((pSchemaItem = collection.FindDataGroupSchemaItem(itemXPath))) {
		res = pSchemaItem->SetValue(value);
	}
	return res; // SUCCESS == beeh.Rewrite();
}