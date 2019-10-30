#include "RectPen.h"
#include "ElementHelper.h"

#include <tfform.h>
#include <mdltfform.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <msvec.fdf>
#include <tfpoly.h>

#include <mselmdsc.fdf>
#include <mselemen.fdf>
#include <msmisc.fdf>

#include <mdltfframe.fdf>

#include <mdltfwstring.fdf>
#include <mdltfperfo.fdf>
#include <mdltfmodelref.fdf>
#include <mdltfprojection.fdf>
#include <ditemlib.fdf>
#include <mscexpr.fdf>
#include <msdgnmodelref.fdf>
#include <mscnv.fdf>
#include <mscell.fdf>
#include <msinput.fdf>
#include <msrmatrx.fdf>
#include <mstmatrx.fdf>
#include <msscancrit.fdf>
#include <msundo.fdf>
#include <leveltable.fdf>
#include <msvar.fdf>

using Bentley::Ustn::Element::EditElemHandle;

extern char rectKKS[50] = {};
extern char rectDescription[50] = {};

extern bool isByContour = false;
extern bool isSweepBi = true;
extern bool isPolicyThrough = false;

extern double rectContourDistance = 0;
extern std::vector<DPoint3d> contourPoints = std::vector<DPoint3d>();

extern RectPenTask rectTask = RectPenTask::getEmpty();
extern RectPen rectPen = RectPen();

void publishRectVariables(SymbolSet* symSetP) {
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        "wallMounted", &rectTask.isWallMounted);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE), 
        "rectHeight", &rectTask.height);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectWidth", &rectTask.width);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectDepth", &rectTask.depth);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectThickness", &rectTask.thickness);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectFlanHeight", &rectTask.flanHeight);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectFlanThickness", &rectTask.flanWidth);

    mdlDialog_publishBasicArray(symSetP, mdlCExpression_getType(TYPECODE_CHAR), 
        "rectKKS", &rectKKS, sizeof(rectKKS));
    mdlDialog_publishBasicArray(symSetP, mdlCExpression_getType(TYPECODE_CHAR),
        "rectDescription", &rectDescription, sizeof(rectDescription));

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "rectContourDistance", &rectContourDistance);

    //mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
    //    "isSweepBi", &isSweepBi);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        "isPolicyThrough", &isPolicyThrough);
}

double getCExprVal(double cexpr) {
    double res = 0;
    mdlCnv_masterToUOR(&res, cexpr, ACTIVEMODEL);
    return res;
}
void setCExprVal(double* cexpr, double value) {
    mdlCnv_UORToMaster(cexpr, value, ACTIVEMODEL);
}

StatusInt RectPenTask::updateByCell(MSElementCP elP) {

    int count = 0;
    MSElementDescrP edP;
    mdlElmdscr_new(&edP, NULL, elP);

    MSElementDescrP currDescrP;
    MSElementDescrP prevDescrP;

    for (currDescrP = edP->h.firstElem;
        currDescrP->h.next != NULL;
        currDescrP = currDescrP->h.next, count++) 
    {        
        prevDescrP = currDescrP->h.previous;
    }
    
    if (count > 0) {
        ;
    }

    int res = mdlCell_extract(&origin, bounds, NULL, NULL, NULL, 0, elP);
    return res;

    DPoint3d pt1, pt2;

    pt1.x = (bounds[0].x + bounds[7].x) / 2;
    pt1.y = (bounds[0].y + bounds[7].y) / 2;
    pt1.z = (bounds[0].z + bounds[7].z) / 2;

    pt2.x = (bounds[3].x + bounds[6].x) / 2;
    pt2.y = (bounds[3].y + bounds[6].y) / 2;
    pt2.z = (bounds[3].z + bounds[6].z) / 2;

    //this->origin = pt1;
    // todo поиск стены

    mdlVec_subtractDPoint3dDPoint3d(&direction, &pt2, &pt1);
}

void RectPenTask::update(RectPenTask& other) {
    isValid = other.isValid;
    origin = other.origin;
    direction = other.direction;

    for (int i = 0; i < 8; ++i) {
        bounds[i] = other.bounds[i];
    }

    height = other.height;
    width = other.width;
    depth = other.depth;
    thickness = other.thickness;
    flanHeight = other.flanHeight;
    flanWidth = other.flanWidth;

    isWallMounted = other.isWallMounted;
    isEmpty = other.isEmpty;
}

RectPenTask RectPenTask::getEmpty() {
    RectPenTask task;
    task.isValid = false;
    task.origin = DPoint3d();
    task.direction = DVec3d();

    memset(task.bounds, 0, 8 * sizeof(DPoint3d));

    task.height =
        task.width =
        task.depth =
        task.thickness =
        task.flanHeight =
        task.flanWidth = 0.;

    task.isWallMounted = true;

    task.isEmpty = true;
    return task;
}

bool RectPen::isValid() {
    return task_.isValid;
}

RectPen::RectPen()
    : task_(RectPenTask::getEmpty()), isWallFound_(false), dataIndex_(0) {
}

RectPen::RectPen(RectPenTask& task) 
    : task_(task), isWallFound_(false), dataIndex_(0)
{
    
    // todo mdlIntersect_closestBetweenElms - проверка пересечения со стеной

    if (!task.isValid)
        return;
    
    DPoint3d* bnds = task.bounds;
    MSElementDescr* face1 = NULL;
    MSElementDescr* face2 = NULL;

    { // Векторы
        mdlVec_subtractDPoint3dDPoint3d(&vectors[0], &bnds[5], &bnds[4]);
        mdlVec_subtractDPoint3dDPoint3d(&vectors[1], &bnds[4], &bnds[7]);
        mdlVec_subtractDPoint3dDPoint3d(&vectors[2], &bnds[1], &bnds[7]);
    }

    { // Грани
        DPoint3d points[3][4] = {
            { bnds[0], bnds[1], bnds[7], bnds[4] },
            { bnds[1], bnds[2], bnds[6], bnds[7] },
            { bnds[4], bnds[7], bnds[6], bnds[5] },
        };
        for (int i = 0; i < 3; ++i) {
            for (int j = 0; j < 4; ++j) {
               facetsPoints[i][j] = points[i][j];
            }
        }
    }
    
    { // Расстояния
        distances[0] = mdlVec_distance(&bnds[4], &bnds[5]);
        distances[1] = mdlVec_distance(&bnds[7], &bnds[4]);
        distances[2] = mdlVec_distance(&bnds[7], &bnds[1]);        
    }
    

    { // Поиск стены, определение её параметров
        // TODO добавить определение точки проекции на стену, понадобится при 
        // отсутствии графического задания на проходку
        TFFormRecipeList* wallP = findWallByTask();
        if (wallP != NULL) {
            if (getWallNormals(wallP, &wallNrms_[0], &wallNrms_[1], face1, face2)) {
                isWallFound_ = true;
            }
        }
        if (wallP != NULL) { // НВС
            mdlTFFormRecipeList_free(&wallP);
        }
    }

    if (isWallFound_) { 
    // Определение индекса данных
        EditElemHandle shapes[3];
        for (int i = 0; i < 3; ++i) {
            createShape(shapes[i], facetsPoints[i], 4, false);

            DVec3d normal;
            mdlElmdscr_extractNormal(
                &normal, NULL, shapes[i].GetElemDescrCP(), NULL);

            if (mdlVec_areParallel(&normal, &wallNrms_[0]) ||
                mdlVec_areParallel(&normal, &wallNrms_[1])) 
            {
                // нормаль контура совпадает с нормалью стены - значит,
                // контур будет задавать перфоратор в CompoundCell

                dataIndex_ = i;

                // параметры задания;
                DPoint3d* pts = facetsPoints[i];
                
                task.origin.x = (pts[0].x + pts[2].x) / 2;
                task.origin.y = (pts[0].y + pts[2].y) / 2;
                task.origin.z = (pts[0].z + pts[2].z) / 2;
                               

                if (mdlVec_angleBetweenVectors(&normal, &wallNrms_[0]) < 1) // НВС погрешность 1 град.
                    task.direction = wallNrms_[0];
                else if (mdlVec_angleBetweenVectors(&normal, &wallNrms_[1]) < 1)
                    task.direction = wallNrms_[1];
                
                mdlVec_negate(&task.direction, &task.direction);

                if (task.isEmpty) {
                    setCExprVal(&task.width, mdlVec_distance(&pts[0], &pts[1]));
                    setCExprVal(&task.height, mdlVec_distance(&pts[0], &pts[3]));
                    setCExprVal(&task.depth, distances[i]);
                    task.isEmpty = false;
                }

                break;
            }
        }
    }

    task_ = task;
}


void RectPen::redraw(Bentley::Ustn::RedrawElems& redrawTool)
{
    if (!isValid())
        return;
      
    EditElemHandle shapeBody;
    EditElemHandle shapePerf;
    EditElemHandle crossFirst;
    EditElemHandle crossSecond;
    DVec3d vec;
    double distance;

    getDataByIndex(dataIndex_, getCExprVal(task_.thickness),
        shapeBody, shapePerf, crossFirst, crossSecond, vec, &distance); // todo shell

    //// рисуем тело
    //EditElemHandle pen;
    //if (createPenetrFrame(pen, shapeBody, shapePerf, vec, distance, shell)) {
    //    redrawTool.DoRedraw(pen);
    //}

    TFFrame* pFrame = createPenetrFrame(shapeBody, shapePerf, crossFirst, crossSecond,
        vec, distance, getCExprVal(task_.thickness), isSweepBi, isPolicyThrough);

    if (pFrame != NULL) {
        EditElemHandle eeh;
        ToEditHandle(eeh, mdlTFFrame_get3DElmdscr(pFrame));

        redrawTool.DoRedraw(eeh);
    }
    
    //if (createBody(body, shapeBody, vec, distance, shell)) {
    //    //mdlDynamic_setElmDescr(body.GetElemDescrP());               
    //}

}

bool RectPen::addToModel() {
    if (!isValid())
        return false;

    EditElemHandle shapeBody;
    EditElemHandle shapePerf;
    EditElemHandle crossFirst;
    EditElemHandle crossSecond;

    double distance;

    task_.update(rectTask);
   
    LevelID activeLaevelId;
    mdlLevel_getActive(&activeLaevelId);

    const int COLOR_CURRENT = tcb->symbology.color;

    LevelID openingLevelId = getLevelIdByName(L"C-OPENING-BOUNDARY");
    if (openingLevelId != LEVEL_NULL_ID) {
        mdlLevel_setActive(openingLevelId);
        tcb->symbology.color = COLOR_BYLEVEL;
    }    

    bool res = 
        rectPen.getDataByPointAndVector(task_.origin, task_.direction,
            shapeBody, shapePerf, crossFirst, crossSecond, &distance);

    TFFrame* frameP = createPenetrFrame(shapeBody, shapePerf, crossFirst, crossSecond,
        task_.direction, getCExprVal(task_.depth), getCExprVal(task_.thickness), isSweepBi, isPolicyThrough);

    {   // Определение слоёв перекрестий
        LevelID levelId = getLevelIdByName(L"C-OPENING-SYMBOL");
        if (levelId != LEVEL_NULL_ID) {
            crossFirst.GetElementP()->ehdr.level = levelId;
            crossSecond.GetElementP()->ehdr.level = levelId;
        }
    }

    if (frameP != NULL) {

        mdlUndo_startGroup();

        DgnModelRefP modelRefP = ACTIVEMODEL;
        UInt32 fpos = 
            mdlElement_getFilePos(FILEPOS_NEXT_NEW_ELEMENT, &modelRefP);

        if (SUCCESS == mdlTFModelRef_addFrame(ACTIVEMODEL, frameP)) {
            // todo ! выяснить окончательно от чего зависят настройки перфоратора

            if (fpos) {
                // записываем свойства в созданный CompoundCell
                // todo научиться это делать в mdl
                char buf[256];
                sprintf(buf, "mdl keyin simpen.ui simpen.ui setdgdata %u %s", 
                    fpos, rectKKS);
                mdlInput_sendSynchronizedKeyin((MSCharCP)buf, 0, 0, 0);
            }
            res = true;
        }
        mdlUndo_endGroup();
    }
    tcb->symbology.color = COLOR_CURRENT; // 1. ! до активации слоя
    mdlLevel_setActive(activeLaevelId); // 2.

    return res;
}

bool RectPen::getDataByIndex(int index, double shell, 
    EditElemHandleR shapeBody, EditElemHandleR shapePerf, 
    EditElemHandleR crossFirst, EditElemHandleR crossSecond,
    DVec3dR vec, double* distanceP)
{
    if (!task_.isValid || index > 2)
        return false;
    
    vec = vectors[index];
    *distanceP = distances[index];

    bool res = false;

    DPoint3d* pts = facetsPoints[index];

    if (shell == 0.0) {
        DPoint3d linePoints[5] = { pts[0], pts[1], pts[2], pts[3], pts[0] };
        res = сreateStringLine(shapeBody, linePoints, 5);
    }
    else {
        res = createShape(shapeBody, pts, 4, false);
    }
    
    // получаем точки центрального сечения графического задания:
    DPoint3d centerPoints[4];
    for (int j = 0; j < 4; ++j) {
        mdlVec_projectPoint(&centerPoints[j], &pts[j], &vectors[index], 0.5);
    }
    
    UInt32 wgt = 0;
    if (res = res && createShape(shapePerf, centerPoints, 4, false)) {
        mdlElement_setSymbology(shapePerf.GetElementP(), 0, &wgt, 0);
    }
    
    if (res) {
        DPoint3d crossPnts[8] = {
            pts[0], pts[1], pts[2], pts[3],
            pts[0], pts[2], pts[1], pts[3]
        };

        сreateStringLine(crossFirst, crossPnts, 8);

        // проецируем на противоположную грань
        for (int i = 0; i < 8; ++i) {
            mdlVec_projectPoint(&crossPnts[i], &crossPnts[i], &vectors[index], 1.0);
        }
        сreateStringLine(crossSecond, crossPnts, 8);

        mdlElement_setSymbology(crossFirst.GetElementP(), 0, &wgt, 0);
        mdlElement_setSymbology(crossSecond.GetElementP(), 0, &wgt, 0);
    }

    return res;
}


void createCross(EditElemHandleR crossOut, DPoint3d* pts /* 4 точки*/) {

    DPoint3d crossPnts[8] = {
        pts[0], pts[1], pts[2], pts[3],
        pts[0], pts[2], pts[1], pts[3]
    };

    UInt32 weight = 0; 
    сreateStringLine(crossOut, crossPnts, 8, &weight);
}

bool RectPen::getDataByPointAndVector(DPoint3dR point, DVec3dR vec,
    /*out*/ EditElemHandleR shapeBody, /*out*/ EditElemHandleR shapePerf,
    /*out*/ EditElemHandleR crossFirst, /*out*/ EditElemHandleR crossSecond,
    /*out*/ double* distanceP)
{

    // в нулевых координатах за ось проходки принимаем ось Y

    RotMatrix rot;
    DVec3d yVec;
    yVec.y = 1;
    yVec.x = yVec.z = 0;

    mdlRMatrix_fromVectorToVector(&rot, &yVec, &vec);

    DPoint3d points[5];
    memset(points, 0, sizeof(DPoint3d));

    double height = getCExprVal(task_.height);
    double width = getCExprVal(task_.width);
    double depth = getCExprVal(task_.depth);

    points[0].x = points[3].x = - width/2;
    points[1].x = points[2].x = width/2;
    points[0].z = points[1].z = - height/2;
    points[2].z = points[3].z = height/2;
    points[4] = points[0];
    points[0].y = points[1].y = points[2].y = points[3].y = points[4].y;
    
    // проходка
    EditElemHandle shape;
    сreateStringLine(shape, points, 5);
    createShape(shapePerf, points, 5, true);
    createBody(shapeBody, shape, yVec, depth, 0.0);
    
    // кресты
    {
        createCross(crossFirst, points);    

        DPoint3d crossPts[4];
        for (int i = 0; i < 4; ++i) {
            crossPts[i] = points[i];
            crossPts[i].y = depth;
        }
        createCross(crossSecond, crossPts);
    }
    
    Transform tran;
    {
        //mdlTMatrix_getIdentity(&tran);
        mdlTMatrix_rotateByRMatrix(&tran, NULL, &rot);
        mdlTMatrix_setTranslation(&tran, &point);
    }

    mdlElmdscr_transform(shapeBody.GetElemDescrP(), &tran);
    mdlElmdscr_transform(shapePerf.GetElemDescrP(), &tran);
    mdlElmdscr_transform(crossFirst.GetElemDescrP(), &tran);
    mdlElmdscr_transform(crossSecond.GetElemDescrP(), &tran);
    
    return true;
}

// TODO разобраться как вернуть в функции указатель на найденую стенку, пока не используется
int scanWall(
    MSElementDescr  *edP,
    void*  param,
    ScanCriteria    *pScanCriteria
) {

   // TFFormRecipeList* wallP = (TFFormRecipeList*)param;

    TFFormRecipeList* flP = NULL;
    if (mdlTFFormRecipeList_constructFromElmdscr(&flP, edP) == BSISUCCESS) {
        TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(flP);
        int type = mdlTFFormRecipe_getType(fP);

        if (type == TF_LINEAR_FORM_ELM) {
            param = &(*flP);
            return 1;
        }
    }
    return 0;
}

TFFormRecipeList* RectPen::findWallByTask()
{
    if (!task_.isValid)
        return NULL;
    
    TFFormRecipeList* wallP = NULL;

    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status;
    
    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();

    // status = mdlScanCriteria_setReturnType(scP, MSSCANCRIT_ITERATE_ELMDSCR, FALSE, TRUE);       
    // todo через callback status = mdlScanCriteria_setElmDscrCallback(scP, (PFScanElemDscrCallback)scanWall, (void*)wallP);
    
    status = mdlScanCriteria_setElementTypeTest(scP, typeMask, sizeof(typeMask));
    // status = mdlScanCriteria_setCellNameTest(scP, L"Opening");
    status = mdlScanCriteria_setModel(scP, ACTIVEMODEL);
    

    { // ограничение по диапазоную точек:
            DPoint3d start = task_.bounds[0];
            DPoint3d end = task_.bounds[6];
            double delta = (abs(start.y - end.y)*0.10);
            end.y += delta;
            start.y += delta;

            DVector3d range = { start, end }; //  { task_.bounds[0] , task_.bounds[6] };
        
        ScanRange scanRng;
        mdlCnv_dRangeToScanRange(&scanRng, &range);
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


            for (int i = 0; i < scanWords && elemAddr[i] < eofPos; ++i) {
                MSElementDescr* edP = NULL;
                if (mdlElmdscr_read(&edP, elemAddr[i], 0, FALSE, &realPos) != 0) {

                    TFFormRecipeList* flP = NULL;
                    if (mdlTFFormRecipeList_constructFromElmdscr(&flP, edP) == BSISUCCESS) {
                        TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(flP);
                        int type = mdlTFFormRecipe_getType(fP);

                        if (type == TF_LINEAR_FORM_ELM || type == TF_SLAB_FORM_ELM) {
                            wallP = flP;
                            break;
                        }
                    }
                }
            }
        } while (status == BUFF_FULL);
    }


   // status = mdlScanCriteria_scan(scP, NULL, NULL, NULL);
    status = mdlScanCriteria_free(scP);

    return wallP;
}

/*
Старый способ

TFFormRecipeList* RectPen::findWallByTask()
{
    if (!task_.isValid)
        return NULL;

    TFFormRecipeList* wallP = NULL;

    ScanCriteria    *scP;
    scP = mdlScanCriteria_create();
    //    scanList.extendedType   = FILEPOS | EXTATTR;
    mdlScanCriteria_setReturnType(scP, MSSCANCRIT_RETURN_FILEPOS, FALSE, TRUE);
    mdlScanCriteria_setModel(scP, ACTIVEMODEL);

    DVector3d range = { task_.bounds[0] , task_.bounds[6] };
    ScanRange scanRng;
    mdlCnv_dRangeToScanRange(&scanRng, &range);
    mdlScanCriteria_setRangeTest(scP, &scanRng);   

    UInt32 elemAddr[10];
    UInt32 eofPos = mdlElement_getFilePos(FILEPOS_EOF, NULL);

    UInt32 filePos = 0;
    UInt32 realPos = 0;
    int status = ERROR;
    do {
        int scanWords = sizeof(elemAddr) / sizeof(short);

        status = mdlScanCriteria_scan(scP, elemAddr, &scanWords, &filePos);
        
        for (int i = 0; i < scanWords && elemAddr[i] < eofPos; ++i) {
            MSElementDescr* edP = NULL;
            if (mdlElmdscr_read(&edP, elemAddr[i], 0, FALSE, &realPos) != 0) {

                TFFormRecipeList* flP = NULL;
                if (mdlTFFormRecipeList_constructFromElmdscr(&flP, edP) == BSISUCCESS) {
                    TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(flP);
                    int type = mdlTFFormRecipe_getType(fP);

                    if (type == TF_LINEAR_FORM_ELM) {
                        wallP = flP;
                        break;
                    }
                }
            }
        }
    } while (status == BUFF_FULL);

    mdlScanCriteria_free(scP);

    return wallP;
}

*/

bool RectPen::getWallNormals(TFFormRecipeList* wallP, DVec3d* nrm1, DVec3d* nrm2,
    MSElementDescr* face1, MSElementDescr* face2)
{
    TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(wallP);

    TFBrepList* blP = NULL;
    TFFormRecipeLinear* linP = (TFFormRecipeLinear*)fP;

    //TFFormRecipeList* pCurrFormRecipeNode = wallP;
    if (mdlTFFormRecipe_getBrepList(fP, &blP, 0, 0, 0) == BSISUCCESS) {
        DPoint3d pts[2];
        int stt[2];

        //MSElementDescr* face1 = NULL;
        //MSElementDescr* face2 = NULL;

        // TODO доработать

        // настенная или напольная:
        FaceLabelEnum faceFirstLabel, faceSecondLabel;
        faceFirstLabel = task_.isWallMounted ? FaceLabelEnum_Left : FaceLabelEnum_Top;
        faceSecondLabel = task_.isWallMounted ? FaceLabelEnum_Right : FaceLabelEnum_Base;
        //FaceLabelEnum faceFirstLabel = task_.isWallMounted ? FaceLabelEnum_Left : FaceLabelEnum_Top;
        //FaceLabelEnum faceSecondLabel = task_.isWallMounted ? FaceLabelEnum_Right : FaceLabelEnum_Base;
        
        if (false) // TODO механизм автоориентирования ещё сырой
        {
            //Корректируем вычисляем грани, кот. образуют толщину стены или плиты:

            TFBrepFaceList* faceLeftP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Left);
            TFBrepFaceList* faceRightP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Right);
            TFBrepFaceList* faceTopP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Top);
            TFBrepFaceList* faceBaseP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Base);
            
            double distHor = 0.0, distVert = 0.0;
            if (faceLeftP && faceRightP) {
                MSElementDescr* faceFirst, *faceSecond;
                mdlTFBrepFaceList_getElmdscr(&faceFirst, faceLeftP, 0);
                mdlTFBrepFaceList_getElmdscr(&faceSecond, faceRightP, 0);

                mdlMinDist_betweenElms(NULL, NULL, &distHor, faceFirst, faceSecond, NULL, 0.0);
            }
            if (faceTopP && faceBaseP) {
                MSElementDescr* faceFirst, *faceSecond;
                mdlTFBrepFaceList_getElmdscr(&faceFirst, faceTopP, 0);
                mdlTFBrepFaceList_getElmdscr(&faceSecond, faceBaseP, 0);

                mdlMinDist_betweenElms(NULL, NULL, &distVert, faceFirst, faceSecond, NULL, 0.0);
            }

            if (distHor > 0.0 && distVert > 0) {
                faceFirstLabel = distHor <= distVert ? FaceLabelEnum_Left : FaceLabelEnum_Top;
                faceSecondLabel = distHor <= distVert ? FaceLabelEnum_Right : FaceLabelEnum_Base;
            }
            else {
                faceFirstLabel = task_.isWallMounted ? FaceLabelEnum_Left : FaceLabelEnum_Top;
                faceSecondLabel = task_.isWallMounted ? FaceLabelEnum_Right : FaceLabelEnum_Base;
            }
        }

        TFBrepFaceList* faceFirstP = mdlTFBrepList_getFacesByLabel(blP, faceFirstLabel);
        TFBrepFaceList* faceSecondP = mdlTFBrepList_getFacesByLabel(blP, faceSecondLabel);
        if (faceFirstP) {
            mdlTFBrepFaceList_getElmdscr(&face1, faceFirstP, 0);

            if (face1) {
                stt[0] = mdlElmdscr_extractNormal(nrm1, &pts[0], face1, NULL);
                mdlElmdscr_freeAll(&face1);
            }

            mdlTFBrepFaceList_free(&faceFirstP);
        }

        if (faceSecondP) {
            mdlTFBrepFaceList_getElmdscr(&face2, faceSecondP, 0);

            if (face2) {
                stt[1] = mdlElmdscr_extractNormal(nrm2, &pts[1], face2, NULL);
                mdlElmdscr_freeAll(&face2);
            }

            mdlTFBrepFaceList_free(&faceSecondP);
        }

        return stt[0] == SUCCESS && stt[1] == SUCCESS;
    }

    return false;
}



//bool RectPen::createPenetrFrame(EditElemHandleR eeh,
//    EditElemHandleR shapeBody, EditElemHandleR shapePerf, DVec3dR vec, double distance, double shell)
//{
//    TFFrame* pFrame = createPenetrFrame(shapeBody, shapePerf, vec, distance, shell);
//    if (pFrame != NULL) {
//        return ToEditHandle(eeh, mdlTFFrame_get3DElmdscr(pFrame));
//    }
//    return false;
//}


bool RectPen::createPenetr(EditElemHandleR eeh, 
    EditElemHandleR crossFirst, EditElemHandleR crossSecond) 
{
    EditElemHandle shapeBody;
    EditElemHandle shapePerf;
    //EditElemHandle crossFirst;
    //EditElemHandle crossSecond;
    DVec3d vec;
    double distance;

    getDataByIndex(dataIndex_, getCExprVal(task_.thickness),
        shapeBody, shapePerf, crossFirst, crossSecond, vec, &distance); // todo shell

    TFFrame* frameP = createPenetrFrame(shapeBody, shapePerf, crossFirst, crossSecond,
        vec, distance, getCExprVal(task_.thickness), isSweepBi, isPolicyThrough);

    if (frameP != NULL) {
        return ToEditHandle(eeh, mdlTFFrame_get3DElmdscr(frameP));
    }
    return false;
}