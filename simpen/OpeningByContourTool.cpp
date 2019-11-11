#include "Opening.h"
#include "OpeningByContourTool.h"
#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "simpencmd.h"

#include <elementref.h>

#include <msvar.fdf>
#include <msstate.fdf>
#include <mslocate.fdf>
#include <msoutput.fdf>
#include <mdltfform.fdf>
#include <mstrnsnt.fdf>
#include <msdialog.fdf>
#include <msselect.fdf>
#include <msmisc.fdf>
#include <mdltfelmdscr.fdf>
#include <msview.fdf>
#include <mswindow.fdf>


namespace Openings
{

OpeningByContourTool* OpeningByContourTool::instanceP = NULL;
OpeningTask OpeningByContourTool::userTask = OpeningTask();

void cmdLocateContour(char *unparsedP)
{
    OpeningByContourTool::instanceP = new OpeningByContourTool();
    OpeningByContourTool::instanceP->InstallTool();
}

void cmdUpdatePreview(char *unparsedP)
{
    if (OpeningByContourTool::instanceP != NULL && 
        OpeningByContourTool::userTask == OpeningTask::getInstance())
    {
        return; //  параметры построения не изменились
    }

    OpeningByContourTool::userTask = OpeningTask::getInstance();

    mdlTransient_free(&msTransientElmP, true);

    if (OpeningByContourTool::instanceP == NULL) {
        return;
    }

    if (!Opening::instance.isValid()) { // elementRef_isEOF(Opening::instance.contourRef)) {
        mdlOutput_prompt("Элемент типа <Shape> (контур построения) не определён");
        return;
    }

    //MSElementDescrP edP = NULL;
    //mdlElmdscr_getByElemRef(&edP,
    //    Opening::instance.contourRef, ACTIVEMODEL, FALSE, 0);

    //if (edP == NULL || edP->el.ehdr.type != SHAPE_ELM) {
    //    mdlOutput_prompt("Элемент типа <Shape> (контур построения) не определён");
    //    return;
    //}

    //mdlOutput_prompt("Выделен контур для создания проёма");

    if (SUCCESS != computeAndDrawTransient(Opening::instance)) {
        // todo сообщение об ошибке
        mdlOutput_prompt(
            "Указанный контур должен быть параллелен плоскости указанной стены/плиты");
        return;
    }

    if (Opening::instance.isValid()) {
        mdlInput_sendKeyin(
            "mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);
    }
}

void cmdAddToModel(char *unparsedP)
{
    if (Opening::instance.isValid() && OpeningByContourTool::instanceP) {
        computeAndAddToModel(Opening::instance);
        cmdLocateContour(NULL);
    }
}

// Фильтр элементов, которые будут доступны пользователю для выбора
bool OpeningByContourTool::OnPostLocate(HitPathCP path, char *cantAcceptReason) {
    if (!__super::OnPostLocate(path, cantAcceptReason)) {
        return false;
    }

    ElementRef elRef = mdlDisplayPath_getElem((DisplayPathP)path, 0);    
    EditElemHandle eeh = EditElemHandle(elRef, ACTIVEMODEL);
    
    if (!OpeningTask::getInstance().isContourSelected &&
        eeh.GetElementType() == SHAPE_ELM) 
    {
        return true;
    }
    else if (OpeningTask::getInstance().isContourSelected &&
        eeh.GetElementType() == CELL_HEADER_ELM) 
    {
        // должна быть стена или плита
        int tfType = mdlTFElmdscr_getApplicationType(eeh.GetElemDescrP());

        if (tfType == TF_LINEAR_FORM_ELM || tfType == TF_SLAB_FORM_ELM) {
            return true;
        }
    }
    return false;
}

EditElemHandleP
    OpeningByContourTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
{
    // Here we have both the new agenda entry and the current hit path if needed...
    EditElemHandleP eehP = __super::BuildLocateAgenda(path, ev);
    
    if (eehP->GetElementType() == SHAPE_ELM) {
        Opening::instance = Opening(eehP->GetElemDescrP());
        OpeningTask::getInstance().isContourSelected = true;       
    }
    else if (eehP->GetElementType() == CELL_HEADER_ELM) {
        // должна быть стена или плита

        int tfType = mdlTFElmdscr_getApplicationType(eehP->GetElemDescrP());

        if (tfType == TF_LINEAR_FORM_ELM || tfType == TF_SLAB_FORM_ELM) {
            OpeningTask::getInstance().tfFormRef = eehP->GetElemRef();
            OpeningTask::getInstance().isTFFormSelected = true;
        }        
    }

    //if (FALSE == elementRef_isEOF(Opening::instance.contourRef)) {

    return eehP;
}

void OpeningByContourTool::OnComplexDynamics(MstnButtonEventCP ev) {
    // TODO ! строить заново только если обновились данные
    cmdUpdatePreview(NULL);
}

StatusInt OpeningByContourTool::OnElementModify(EditElemHandleR eeh) {
    
    if (eeh.GetElementType() != SHAPE_ELM) {
        return ERROR;
    }
    
    mdlInput_sendSynchronizedKeyin(
        "mdl keyin simpen.ui simpen.ui sendTaskData", 0, 0, 0);

    if (Opening::instance.getTask().isReadyToPublish) {
        computeAndAddToModel(Opening::instance);
        return SUCCESS;
    }
    return ERROR;
}


bool OpeningByContourTool::WantAdditionalLocate(MstnButtonEventCP ev) {

    if (!OpeningTask::getInstance().isContourSelected) {
        mdlOutput_promptU("Выбирите элемент типа <Shape>, который будет задавать контур проёма");
        return true;
    }
    else if (!OpeningTask::getInstance().isTFFormSelected) {
        mdlOutput_promptU("Выбирите элемент типа <Wall> или <Slab>");
        return true;
    }
    
    BeginComplexDynamics();
    return false;
}

bool OpeningByContourTool::WantDynamics() {
    return false; // управляем вручную через BeginComplexDynamics();
}

void OpeningByContourTool::OnRestartCommand() {
    cmdLocateContour(NULL);
}

bool OpeningByContourTool::NeedAcceptPoint() {
    return true;
}

bool OpeningByContourTool::WantAccuSnap()
{
    return false;
}

//bool ContourOpeningTool::NeedPointForDynamics()
//{
//    return true;
//}

void OpeningByContourTool::OnPostInstall() {
    __super::OnPostInstall();

    instanceP = this;
    Opening::instance = Opening();
    OpeningTask::getInstance().isContourSelected =
    OpeningTask::getInstance().isTFFormSelected = false;

    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlSelect_freeAll();
    userTask = OpeningTask();

    mdlOutput_promptU("Выбирите элемент типа <Shape>, который будет задавать контур проёма");
}

bool OpeningByContourTool::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);   // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    return true;
}

void OpeningByContourTool::OnCleanup() {
    //userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    instanceP = NULL;
    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}


}