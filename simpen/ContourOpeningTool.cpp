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


namespace Openings
{
ContourOpeningTool* ContourOpeningTool::instanceP = NULL;

void cmdLocateContour(char *unparsedP)
{
    ContourOpeningTool::instanceP = new ContourOpeningTool();
    ContourOpeningTool::instanceP->InstallTool();
}

void cmdUpdatePreview(char *unparsedP)
{
    mdlTransient_free(&msTransientElmP, true);

    if (ContourOpeningTool::instanceP == NULL) {
        return;
    }

    if (elementRef_isEOF(Opening::instance.contourRef)) {
        mdlOutput_prompt("Элемент типа <Shape> (контур построения) не определён");
        return;
    }

    MSElementDescrP edP = NULL;
    mdlElmdscr_getByElemRef(&edP,
        Opening::instance.contourRef, ACTIVEMODEL, FALSE, 0);

    if (edP == NULL || edP->el.ehdr.type != SHAPE_ELM) {
        mdlOutput_prompt("Элемент типа <Shape> (контур построения) не определён");
        return;
    }

    //mdlOutput_prompt("Выделен контур для создания проёма");

    if (SUCCESS != computeAndDrawTransient(Opening::instance)) {
        // todo сообщение об ошибке
        mdlOutput_prompt(
            "Указанный контур не лежит в плоскости какой-либо стены или плиты");
        return;
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
    if (!__super::OnPostLocate(path, cantAcceptReason))
        return false;

    ElementRef elRef = mdlDisplayPath_getElem((DisplayPathP)path, 0);
    return elementRef_getElemType(elRef) == SHAPE_ELM;
}

EditElemHandleP
    ContourOpeningTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
{
    // Here we have both the new agenda entry and the current hit path if needed...
    Opening::instance.contourRef = mdlDisplayPath_getElem((DisplayPathP)path, 0);

    // todo setDataGroupInstanceValue(Opening::instance.contourRef, L"kksValue");

    if (FALSE == elementRef_isEOF(Opening::instance.contourRef)) {
        mdlInput_sendKeyin(
            "mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);
    }

    return __super::BuildLocateAgenda(path, ev);
}

void ContourOpeningTool::OnComplexDynamics(MstnButtonEventCP ev) {
    // TODO ! строить заново только если обновились данные
    cmdUpdatePreview(NULL);
}

StatusInt ContourOpeningTool::OnElementModify(EditElemHandleR eeh) {
    mdlInput_sendSynchronizedKeyin(
        "mdl keyin simpen.ui simpen.ui sendTaskData", 0, 0, 0);

    if (Opening::instance.isReadyToPublish) {
        computeAndAddToModel(Opening::instance);
        return SUCCESS;
    }
    return ERROR;
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

bool ContourOpeningTool::NeedPointForDynamics()
{
    return false;
}

void ContourOpeningTool::OnPostInstall() {
    __super::OnPostInstall();
    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlSelect_freeAll();

    mdlOutput_prompt("Выбирите элемент типа <Shape>, который будет задавать контур проёма");
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