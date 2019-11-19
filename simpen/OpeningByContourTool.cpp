#include "Opening.h"
#include "OpeningByContourTool.h"
#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "simpencmd.h"
#include "ui.h"

#include <elementref.h>
#include <buildingeditelemhandle.h>
#include <CatalogCollection.h>

#include <mdltfelmdscr.fdf>
#include <mdltfform.fdf>
#include <msdialog.fdf>
#include <mslocate.fdf>
#include <msmisc.fdf>
#include <msoutput.fdf>
#include <msselect.fdf>
#include <msstate.fdf>
#include <mstrnsnt.fdf>
#include <msvar.fdf>
#include <msview.fdf>
#include <mswindow.fdf>
#include <msundo.fdf>


namespace Openings
{

OpeningByContourTool* OpeningByContourTool::instanceP = NULL;
OpeningTask OpeningByContourTool::prevTask = OpeningTask();

void OpeningByContourTool::run(char *unparsedP)
{
    OpeningByContourTool::instanceP = new OpeningByContourTool();
    OpeningByContourTool::instanceP->InstallTool();
}

void OpeningByContourTool::updatePreview(char *unparsedP)
{
	if (prevTask == OpeningTask::getInstance()) {
		return; //  параметры построения не изменились
	}

	mdlTransient_free(&msTransientElmP, true);

	OpeningByContourTool* toolP = OpeningByContourTool::instanceP;
	if (!toolP || toolP->isAddToModelProcessActive) {
		return;
	}

    prevTask = OpeningTask::getInstance();	 

    if (!Opening::instance.isValid()) {
        UI::warning("Контур построения не определён");
        return;
    }

    if (SUCCESS != computeAndDrawTransient(Opening::instance)) {
		UI::warning("Указанный контур должен быть параллелен плоскости указанной стены/плиты");
        return;
    }

    if (Opening::instance.isValid()) {
		UI::setEnableAddToModel();
    }
}

void OpeningByContourTool::addToModel(char *unparsedP)
{
	OpeningByContourTool* toolP = OpeningByContourTool::instanceP;

	toolP->isAddToModelProcessActive = true;

	if (Opening::instance.isValid() && toolP &&
		Opening::instance.getTask().isReadyToPublish)
	{
		mdlUndo_startGroup();
		computeAndAddToModel(Opening::instance);
		mdlUndo_endGroup();
	}

	toolP->isAddToModelProcessActive = false;
	run(unparsedP);
}

// Фильтр элементов, которые будут доступны пользователю для выбора
bool OpeningByContourTool::OnPostLocate(HitPathCP path, char *cantAcceptReason) 
{
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

        if (tfType == TF_LINEAR_FORM_ELM || tfType == TF_SLAB_FORM_ELM ||
			tfType == TF_FREE_FORM_ELM) 
		{
            return true;
        }

		Bentley::Building::Elements::BuildingEditElemHandle beeh(elRef, ACTIVEMODEL);
		beeh.LoadDataGroupData();

		CCatalogCollection::CCollectionConst_iterator itr;
		for (itr = beeh.GetCatalogCollection().Begin(); itr != beeh.GetCatalogCollection().End(); itr++)
		{
			const std::wstring catalogInstanceName = itr->first;
			if (catalogInstanceName == L"ConcreteWalls" || 
				catalogInstanceName == L"ConcreteSlabs")
			{
				return true;
			}
		}
    }


    return false;
}

EditElemHandleP
    OpeningByContourTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
{
	instanceP = this; // НВС

    // Here we have both the new agenda entry and the current hit path if needed...
    EditElemHandleP eehP = __super::BuildLocateAgenda(path, ev);
    
    if (eehP->GetElementType() == SHAPE_ELM) {
        Opening::instance = Opening(eehP->GetElemDescrP());
        OpeningTask::getInstance().isContourSelected = true;       
    }
    else if (eehP->GetElementType() == CELL_HEADER_ELM) {
        // должна быть стена или плита

        int tfType = mdlTFElmdscr_getApplicationType(eehP->GetElemDescrP());

        OpeningTask::getInstance().tfFormRef = eehP->GetElemRef();
        OpeningTask::getInstance().isTFFormSelected = true;               
    }

    //if (FALSE == elementRef_isEOF(Opening::instance.contourRef)) {

    return eehP;
}

void OpeningByContourTool::OnComplexDynamics(MstnButtonEventCP ev) {
    // TODO ! строить заново только если обновились данные
    updatePreview(NULL);
}

StatusInt OpeningByContourTool::OnElementModify(EditElemHandleR eeh) {
    
    if (eeh.GetElementType() != SHAPE_ELM) {
        return ERROR;
    }
    
	UI::sendTaskDataSynch();

    if (Opening::instance.getTask().isReadyToPublish) {
        computeAndAddToModel(Opening::instance);
        return SUCCESS;
    }
    return ERROR;
}


bool OpeningByContourTool::WantAdditionalLocate(MstnButtonEventCP ev) {

    if (!OpeningTask::getInstance().isContourSelected) {
        UI::promptU("Выберите элемент типа <Shape>, который будет задавать контур проёма");
        return true;
    }
    else if (!OpeningTask::getInstance().isTFFormSelected) {
		UI::promptU("Выберите элемент типа <Wall> или <Slab>");
        return true;
    }
    
    BeginComplexDynamics();
    return false;
}

bool OpeningByContourTool::WantDynamics() {
    return false; // управляем вручную через BeginComplexDynamics();
}

void OpeningByContourTool::OnRestartCommand() {
    run(NULL);
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

void OpeningByContourTool::clear()
{
	Opening::instance = Opening();
	OpeningTask::getInstance().clear();
	prevTask = OpeningTask();
	isAddToModelProcessActive = false;
}


void OpeningByContourTool::OnPostInstall() {
    __super::OnPostInstall();

    instanceP = this;

    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlSelect_freeAll();
    
	clear();

	UI::promptU("Выберите элемент типа <Shape>, который будет задавать контур проёма");
}

bool OpeningByContourTool::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);   // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    return true;
}

void OpeningByContourTool::OnCleanup() {
    instanceP = NULL;
	UI::reload();
}

}