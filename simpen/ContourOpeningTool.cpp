#include "Opening.h"
#include "ContourOpeningTool.h"
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

ContourOpeningTool* ContourOpeningTool::instanceP = NULL;
OpeningTask ContourOpeningTool::userTask = OpeningTask();

void cmdLocateContour(char *unparsedP)
{
    ContourOpeningTool::instanceP = new ContourOpeningTool();
    ContourOpeningTool::instanceP->InstallTool();
}

void cmdUpdatePreview(char *unparsedP)
{
    if (ContourOpeningTool::instanceP != NULL && 
        ContourOpeningTool::userTask == OpeningTask::getInstance())
    {
        return; //  параметры построения не изменились
    }

    ContourOpeningTool::userTask = OpeningTask::getInstance();

    mdlTransient_free(&msTransientElmP, true);

    if (ContourOpeningTool::instanceP == NULL) {
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
    if (Opening::instance.isValid() && ContourOpeningTool::instanceP) {
        computeAndAddToModel(Opening::instance);

        cmdLocateContour(NULL);
    }
}

// Фильтр элементов, которые будут доступны пользователю для выбора
bool ContourOpeningTool::OnPostLocate(HitPathCP path, char *cantAcceptReason) {
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
    ContourOpeningTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
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

void ContourOpeningTool::OnComplexDynamics(MstnButtonEventCP ev) {
    // TODO ! строить заново только если обновились данные
    cmdUpdatePreview(NULL);
}

StatusInt ContourOpeningTool::OnElementModify(EditElemHandleR eeh) {
    
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


bool ContourOpeningTool::WantAdditionalLocate(MstnButtonEventCP ev) {

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

bool ContourOpeningTool::WantDynamics() {
    return false; // управляем вручную через BeginComplexDynamics();
}

void ContourOpeningTool::OnRestartCommand() {
    cmdLocateContour(NULL);
}

bool ContourOpeningTool::NeedAcceptPoint() {
    return true;
}

bool ContourOpeningTool::WantAccuSnap()
{
    return false;
}

//bool ContourOpeningTool::NeedPointForDynamics()
//{
//    return true;
//}

void ContourOpeningTool::OnPostInstall() {
    __super::OnPostInstall();

    instanceP = this;
    Opening::instance = Opening();
    OpeningTask::getInstance().isContourSelected =
    OpeningTask::getInstance().isTFFormSelected = false;

    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    // mdlLocate_allowLocked();

    mdlSelect_freeAll();
    userTask = OpeningTask();

    mdlOutput_promptU("Выбирите элемент типа <Shape>, который будет задавать контур проёма");
}

bool ContourOpeningTool::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);   // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    return true;
}

void ContourOpeningTool::OnCleanup() {
    //userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    instanceP = NULL;
    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}


}