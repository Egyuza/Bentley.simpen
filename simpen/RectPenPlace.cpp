#include "RectPenPlace.h"
#include "RectPenLocate.h"

#include <mselmdsc.fdf>
#include <mdltfframe.fdf>
#include <mdltfwstring.fdf>
#include <mdltfperfo.fdf>
#include <mdltfmodelref.fdf>

USING_NAMESPACE_BENTLEY;
USING_NAMESPACE_BENTLEY_USTN;
USING_NAMESPACE_BENTLEY_USTN_ELEMENT;

void cmdPlaceRect(char *unparsedP) {
    RectPenPlace *pTool = new RectPenPlace();
    pTool->InstallTool();
}

RectPenPlace::RectPenPlace()
{
}

/*------------------------------------------------------------------------------
Реализация динамики процесса построения
----------------------------------------------------------------------------- */
void RectPenPlace::OnComplexDynamics(MstnButtonEventCP ev) {
    RedrawElems redrawTool;
    redrawTool.SetDrawMode(DRAW_MODE_TempDraw);
    redrawTool.SetDrawPurpose(DRAW_PURPOSE_Dynamics);
    redrawTool.SetViews(0xffff);
}

/*------------------------------------------------------------------------------
Ввод точки построения пользователем - ЛКМ
----------------------------------------------------------------------------- */
bool RectPenPlace::OnDataButton(MstnButtonEventCP ev) {
    
    return true;
}

/*------------------------------------------------------------------------------
----------------------------------------------------------------------------- */
StatusInt RectPenPlace::OnElementModify(EditElemHandleR elHandle) {
    return ERROR;
}

/*------------------------------------------------------------------------------
Событе нажатия ПКМ
----------------------------------------------------------------------------- */
bool RectPenPlace::OnResetButton(MstnButtonEventCP ev) {
    
    OnReinitialize();
    
    return true;
}

/*------------------------------------------------------------------------------
Перезапуск тула
----------------------------------------------------------------------------- */
void RectPenPlace::OnRestartCommand() {
   
}

/*------------------------------------------------------------------------------
after the tool is installed
----------------------------------------------------------------------------- */
void RectPenPlace::OnPostInstall() {

    mdlAccuSnap_enableSnap(TRUE);
    
    __super::OnPostInstall();
}

/*------------------------------------------------------------------------------
the tool is being installed are there any tasks to do?
----------------------------------------------------------------------------- */
bool RectPenPlace::OnInstall() {
    return __super::OnInstall();
}