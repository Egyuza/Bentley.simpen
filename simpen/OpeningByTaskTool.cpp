#include "simpencmd.h"
#include "Opening.h"
#include "OpeningByTaskTool.h"
#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "XmlAttributeHelper.h"

#include <elementref.h>
#include <IModel\xmlinstanceapi.h>
#include <IModel\xmlinstanceidcache.h>
#include <IModel\xmlinstanceschemamanager.h>

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
#include <mdlxmltools.fdf>

namespace Openings
{

USING_NAMESPACE_BENTLEY_XMLINSTANCEAPI_NATIVE;

OpeningByTaskTool* OpeningByTaskTool::instanceP = NULL;
OpeningTask OpeningByTaskTool::userTask = OpeningTask();

void OpeningByTaskTool::runTool(char *unparsedP)
{
    OpeningByTaskTool::instanceP = new OpeningByTaskTool();
    OpeningByTaskTool::instanceP->InstallTool();
}

void OpeningByTaskTool::updatePreview(char *unparsedP)
{
    if (OpeningByTaskTool::instanceP != NULL &&
        OpeningByTaskTool::userTask == OpeningTask::getInstance())
    {
        return; //  параметры построения не изменились
    }

    OpeningByTaskTool::userTask = OpeningTask::getInstance();

    mdlTransient_free(&msTransientElmP, true);

    if (OpeningByTaskTool::instanceP == NULL) {
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

void OpeningByTaskTool::addToModel(char *unparsedP)
{
    if (Opening::instance.isValid() && OpeningByTaskTool::instanceP) {
        computeAndAddToModel(Opening::instance);

        runTool(NULL);
    }
}

// Фильтр элементов, которые будут доступны пользователю для выбора
bool OpeningByTaskTool::OnPostLocate(HitPathCP path, char *cantAcceptReason) {
    if (!__super::OnPostLocate(path, cantAcceptReason)) {
        return false;
    }

    // TODO добавить проверку по свойству 'Catalog part number'

    ElementRef elRef = mdlDisplayPath_getElem((DisplayPathP)path, 0);
    EditElemHandle eeh = EditElemHandle(elRef, 
        mdlDisplayPath_getPathRoot((DisplayPathP)path));
    

    // 1-ая проверка на CELL
    if (eeh.GetElementType() != CELL_HEADER_ELM) {
        return false;
    }

    bool bStatus;

    XmlInstanceStatus stt;
    stt.status = LICENSE_FAILED;
    XmlInstanceSchemaManager mgrR(eeh.GetModelRef());
    mgrR.ReadSchemas(bStatus);

    XmlInstanceApi xapiR = XmlInstanceApi::CreateApi(stt, mgrR);
    StringListHandle slhR = xapiR.ReadInstances(stt, eeh.GetElemRef());

    int res = 0;
    for (int i = 0; i < slhR.GetCount(); i++)
    {
        Bentley::WString strInst = slhR.GetString(i);

        XmlNodeRef node = NULL;
        if (XmlAttributeHelper::findNodeFromInstance(
            &node, strInst, L"P3DEquipment")) // 2-ая проверка по атрибуту
        {
            return true; 
        }
    }

    return false;
}

EditElemHandleP
    OpeningByTaskTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
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

void OpeningByTaskTool::OnComplexDynamics(MstnButtonEventCP ev) {
    // TODO ! строить заново только если обновились данные
    cmdUpdatePreview(NULL);
}

StatusInt OpeningByTaskTool::OnElementModify(EditElemHandleR eeh) {

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

bool OpeningByTaskTool::OnDataButton(MstnButtonEventCP ev) {
    // если задание не выбрано:
    return __super::OnDataButton(ev);

    // иначе определить что возвращать true или false
}


void OpeningByTaskTool::OnRestartCommand() {
    runTool(NULL);
}

bool OpeningByTaskTool::NeedAcceptPoint() {
    return true;
}

bool OpeningByTaskTool::WantAccuSnap()
{
    return false;
}

//bool ContourOpeningTool::NeedPointForDynamics()
//{
//    return true;
//}

void OpeningByTaskTool::OnPostInstall() {
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

bool OpeningByTaskTool::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);   // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    return true;
}

void OpeningByTaskTool::OnCleanup() {
    //userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    instanceP = NULL;
    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}

}