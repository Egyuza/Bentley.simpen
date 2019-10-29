#include "RectPenDraw.h"
#include "RectPenLocate.h"
#include "RectPen.h"

#include <mselmdsc.fdf>
#include <mdltfframe.fdf>
#include <mdltfwstring.fdf>
#include <mdltfperfo.fdf>
#include <mdltfmodelref.fdf>

#include <msvba.fdf>

using Bentley::Ustn::Element::EditElemHandle;

void cmdDrawRect(char *unparsedP) {
    //mdlLocate_normal();
    //mdlState_startModifyCommand(
    //    cmdLocateRect, locateTaskPointAccept, 0,
    //    locateTaskPointShow, 0, 0, 0, FALSE, 0
    //);
    //mdlLocate_setFunction(LOCATE_POSTLOCATE, singlelocate_ElmFilter);
    //mdlLocate_init();
    ////mdlLocate_allowLocked();  

    isByContour = true;

    RectPenDraw *pTool = new RectPenDraw();
    pTool->InstallTool();
}


void constructByContour() {
    EditElemHandle shape;
    createShape(shape, &contourPoints[0], contourPoints.size(), false);

    DVec3d normal;
    mdlElmdscr_extractNormal(&normal, NULL, shape.GetElemDescrCP(), NULL);

    construct(normal, getCExprVal(rectContourDistance));
}

bool construct(DVec3d ppVec, double distance) {
    EditElemHandle shape;
    createShape(shape, &contourPoints[0], contourPoints.size(), false);

    EditElemHandle conture;
    сreateStringLine(conture, &contourPoints[0], contourPoints.size());

    // todo shell
    double shell = 0.0;

    //EditElemHandle body;
    //if (!createBody(body, shape, ppVec, distance, shell))
    //    return false;

    EditElemHandle crossFirst, crossSecond;

    TFFrame* frameP = createPenetrFrame(conture, shape,
        conture, conture, ppVec, distance, shell, isSweepBi, isPolicyThrough);

    if (frameP != NULL) {

        UInt32 fpos = mdlModelRef_getEof(ACTIVEMODEL);

        if (SUCCESS == mdlTFModelRef_addFrame(ACTIVEMODEL, frameP)) {
            mdlTFModelRef_updateAutoOpeningsByFrame(
                ACTIVEMODEL, frameP, 1, 0, FramePerforationPolicyEnum_None);

            if (fpos && (strlen(rectKKS) > 0 || strlen(rectDescription) > 0)) {
                // записываем свойства в созданный CompoundCell
                // todo научиться это делать в mdl
                char buf[1000];
                sprintf(buf, "mdl keyin simpen.ui simpen.ui setdgdata %u %s",
                    fpos, rectKKS);
                mdlInput_sendKeyin((MSCharCP)buf, 0, 0, 0);
            }
            return true;
        }
    }

    return false;
}

void createTransient(EditElemHandleR eeh) {
    // Use general purpose tool transient (automatically cleared when tool exits).
    // mdlTransient_free(&msTransientElmP, true);

    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
        eeh.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);

}

RectPenDraw::RectPenDraw()
    : step_(STEP_START) 
{
    contourPoints = std::vector<DPoint3d>();
}

/*------------------------------------------------------------------------------
Реализация динамики процесса построения
----------------------------------------------------------------------------- */
void RectPenDraw::OnComplexDynamics(MstnButtonEventCP ev) {

    Bentley::Ustn::RedrawElems redrawTool;
    redrawTool.SetDrawMode(DRAW_MODE_TempDraw);
    redrawTool.SetDrawPurpose(DRAW_PURPOSE_Dynamics);
    redrawTool.SetViews(0xffff);

    if (rectTask.isValid) {
        EditElemHandle line;
        сreateStringLine(line, rectTask.bounds, 8);
        redrawTool.DoRedraw(line);
    }

    switch (step_) {
    case STEP_START:
        break;
    case STEP_SHAPE_POINT_NEXT:
    {
        //RedrawElems redrawTool;
        //redrawTool.SetDrawMode(DRAW_MODE_TempDraw);
        //redrawTool.SetDrawPurpose(DRAW_PURPOSE_Dynamics);
        //redrawTool.SetViews(0xffff);

        std::vector<DPoint3d> points(contourPoints.begin(), contourPoints.end());
        points.push_back(*ev->GetPoint());
        points.push_back(points[0]);

        EditElemHandle shape;
        if (createShape(shape, &points[0], points.size(), true)) {
            // контур выреза
            redrawTool.DoRedraw(shape);
        }
        else {
            // рисуем линию незамкнутого контура

            EditElemHandle line;
            сreateStringLine(line, &contourPoints[0], contourPoints.size());
            redrawTool.DoRedraw(line);
        }
    } break;
    case STEP_DEST_POINT:
        break;
    default:
        break;
    }
}

/*------------------------------------------------------------------------------
Ввод точки построения пользователем - ЛКМ
----------------------------------------------------------------------------- */
bool RectPenDraw::OnDataButton(MstnButtonEventCP ev) {

    switch (step_) {
    case STEP_START:
        BeginComplexDynamics();
    case STEP_SHAPE_POINT_NEXT:
        contourPoints.push_back(*ev->GetPoint());
        break;
    case STEP_DEST_POINT:
        break;
    default:
        break;
    }

    if (step_ == STEP_SHAPE_POINT_NEXT) {
        int size = contourPoints.size();
        if (size > 2 && (contourPoints[0] == contourPoints[size - 1])) {
            // если последняя точка совпадает с первой

            EditElemHandle shape;
            if (createShape(shape, &contourPoints[0], contourPoints.size(), true)) {
                // контур выреза
                createTransient(shape);
                // mdlVBA_runProcedure(0, 0, "so2", "penRect", "enableConstructButton", 0, 0);
            
                char buf[1000];
                sprintf(buf, "mdl keyin simpen.ui simpen.ui enableAddToModel");
                mdlInput_sendKeyin((MSCharCP)buf, 0, 0, 0);
            }
            ++step_;
        }
    }
    else if (step_ == STEP_DEST_POINT) {

        EditElemHandle shape;
        createShape(shape, &contourPoints[0], contourPoints.size(), false);

        DPoint3d ptProj;
        DVec3d ppVec;
        DPoint3d pt = *ev->GetPoint();
        mdlProject_perpendicular(&ptProj, NULL, &ppVec, shape.GetElemDescrP(),
            ACTIVEMODEL, &pt, NULL, 0.0);

        double distance = mdlVec_distance(&pt, &ptProj);

        construct(ppVec, distance);
    }
    else {
        // переход на след. шаг построения
        ++step_;
    }

    return true;
}




/*------------------------------------------------------------------------------
----------------------------------------------------------------------------- */
StatusInt RectPenDraw::OnElementModify(
    Bentley::Ustn::Element::EditElemHandle& elHandle) 
{
    return ERROR;
}

/*------------------------------------------------------------------------------
Событе нажатия ПКМ
----------------------------------------------------------------------------- */
bool RectPenDraw::OnResetButton(MstnButtonEventCP ev) {

    if (step_ == STEP_SHAPE_POINT_NEXT && contourPoints.size() > 2) {
        contourPoints.push_back(contourPoints[0]); // замыкаем контур

        EditElemHandle shape;
        if (createShape(shape, &contourPoints[0], contourPoints.size(), true)) {
            // контур выреза
            createTransient(shape);
            mdlInput_sendKeyin(
                "mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);
        }
        ++step_;
        return false;
    }
    
    OnReinitialize();    

    return true;
}

/*------------------------------------------------------------------------------
Перезапуск тула
----------------------------------------------------------------------------- */
void RectPenDraw::OnRestartCommand() {

}

/*------------------------------------------------------------------------------
after the tool is installed
----------------------------------------------------------------------------- */
void RectPenDraw::OnPostInstall() {

    //mdlAccuSnap_enableLocate(TRUE);
    mdlAccuSnap_enableSnap(TRUE);

    // mdlView_defaultCursor();
    //mdlLocate_setCursor();
    __super::OnPostInstall();
}

/*------------------------------------------------------------------------------
the tool is being installed are there any tasks to do?
----------------------------------------------------------------------------- */
bool RectPenDraw::OnInstall() {
    return __super::OnInstall();
}


void RectPenDraw::OnCleanup() {
    // mdlVBA_runProcedure(0, 0, "so2", "penRect", "reload", 0, 0);

    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}

STEP operator ++(STEP& step) {
    step = static_cast<STEP>(step + 1);
    return step;
}
STEP operator --(STEP& step) {
    step = static_cast<STEP>(step - 1);
    return step;
}

