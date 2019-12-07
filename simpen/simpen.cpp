#include "simpen.h"
#include "simpencmd.h"

#include "pipepen.h" // старый код

#include "OpeningHelper.h"
#include "OpeningByContourTool.h"
#include "OpeningByTaskTool.h"

#include <msrmgr.h>

#include <msdialog.fdf>
#include <ditemlib.fdf>
#include <mscexpr.fdf>
#include <msparse.fdf>
#include <msstate.fdf>
#include <msvec.fdf>


USING_NAMESPACE_BENTLEY_USTN;
USING_NAMESPACE_BENTLEY_USTN_ELEMENT;

extern DVec3d pZ;
extern DVec3d pZero;
extern DVec3d pZneg;

extern ULong lvlF;
extern ULong lvlP;

extern int iMode;
extern int iFlanges;
extern int iByMessage;
extern int iPenType;
extern int iBlick;
extern int iJust;

extern long coord[];
extern long icoord[];

extern double dFlanDiam;
extern double dPipeDiam;
extern double dPipeThick;

extern double dRectWidth;
extern double dRectHeight;
extern double dRectThick;
extern double dFlanWidth;

extern double dFlanThick;
extern double dWallThick;


Private void initGlobals(void) 
{
    memset(&g_frameDataGUI, 0, sizeof(FrameDataGUI));
}

Private DialogHookInfo uHooks[] =
{
    { HOOKID_txCellName, (PFDialogHook)hook_txCellName },
};

namespace Openings
{
	void cmdAddToModel(char *unparsedP)
	{
		if (OpeningByContourTool::instanceP) {
			OpeningByContourTool::addToModel(unparsedP);
		}
		else if (OpeningByTaskTool::instanceP) {
			OpeningByTaskTool::addToModel(unparsedP);
		}
	}

	void cmdUpdatePreview(char *unparsedP)
	{
		if (OpeningByContourTool::instanceP) {
			OpeningByContourTool::updatePreview(unparsedP);
		}
		else if (OpeningByTaskTool::instanceP) {
			OpeningByTaskTool::updatePreview(unparsedP);
		}
	}
}

typedef void(*Command_Handler) (char*);

using namespace Openings;

Private MdlCommandNumber commandNumber[] =
{
	{ (Command_Handler)placeFrameLibrary_start,		CMD_SIMPEN_PLACE_LIBRARY },
	{ (Command_Handler)placeFrameOrphan_start,		CMD_SIMPEN_PLACE_ORPHAN },
	{ (Command_Handler)cmdMakePenetr,				CMD_SIMPEN_PLACEPEN },
	{ (Command_Handler)cmdScanPenPrimary,			CMD_SIMPEN_PENPRIM },
	{ (Command_Handler)cmdMakeEmbPlate,				CMD_SIMPEN_PLACEEMB },
	{ (Command_Handler)cmdElem,						CMD_SIMPEN_ELEM },
	{ (Command_Handler)cmdTaskPen,					CMD_SIMPEN_TASK },
	{ (Command_Handler)PartsReport,					CMD_SIMPEN_REPORT },
	{ (Command_Handler)cmdMakeOpening,				CMD_SIMPEN_PLACEOP },

    { (Command_Handler)OpeningByContourTool::run,	CMD_SIMPEN_OPENINGS_LOCATE_CONTOUR },
    { (Command_Handler)OpeningByTaskTool::run,		CMD_SIMPEN_OPENINGS_LOCATE_TASK },
    { (Command_Handler)cmdAddToModel,               CMD_SIMPEN_OPENINGS_ADD },
    { (Command_Handler)cmdUpdatePreview,            CMD_SIMPEN_OPENINGS_UPDATE_PREVIEW },
    { (Command_Handler)cmdUpdateAll,				CMD_SIMPEN_OPENINGS_UPDATE_ALL },
    0
};

//{(Command_Handler) cmdMakePenetrPX,            CMD_SIMPEN_PLACE_PX},
//{(Command_Handler) cmdMakePenetrPY,             CMD_SIMPEN_PLACE_PY},
//{(Command_Handler) cmdMakePenetrPZ,            CMD_SIMPEN_PLACE_PZ},
//{(Command_Handler) cmdMakePenetrNX,             CMD_SIMPEN_PLACE_NX},
//{(Command_Handler) cmdMakePenetrNY,            CMD_SIMPEN_PLACE_NY},
//{(Command_Handler) cmdMakePenetrNZ,             CMD_SIMPEN_PLACE_NZ},

void publishVarBase(void* setP, MdlTypecodes typeCode, char* varName, void* varP) // todo использовать
{
	mdlDialog_publishBasicVariable(setP, mdlCExpression_getType(typeCode), varName, varP);
}

extern "C" DLLEXPORT  int MdlMain (int argc, char *argv[]) 
{

#ifdef DEBUG
    mdlSystem_enterDebug();
#endif // DEBUG    

    SymbolSet*    pSet = NULL;
    RscFileHandle rscFileH = 0;
	
    initGlobals();

    if (SUCCESS != mdlResource_openFile(&rscFileH, NULL, RSC_READ)) {
        mdlSystem_exit(BSIERROR, 1);
    }

    mdlSystem_registerCommandNumbers(commandNumber);

    mdlParse_loadCommandTable(NULL);

    mdlState_registerStringIds(MESSAGELISTID_Commands, MESSAGELISTID_Prompts);

    mdlDialog_hookPublish(sizeof(uHooks) / sizeof(DialogHookInfo), uHooks);

    pSet = mdlCExpression_initializeSet(VISIBILITY_DIALOG_BOX | VISIBILITY_CALCULATOR, 0, TRUE);

    mdlDialog_publishComplexVariable(pSet, "framedatagui", "g_frameDataGUI", &g_frameDataGUI);
	
    mdlVec_fromXYZ(&pZ, 0., 0., 1.);
    mdlVec_fromXYZ(&pZero, 0., 0., 0.);
    mdlVec_fromXYZ(&pZneg, 0., 0., -1.);

    // прямоугольные проходки:
    Openings::OpeningTask::publishCExpressions(pSet);

    // round
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "fdiam", &dFlanDiam);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "pdiam", &dPipeDiam);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "pthick", &dPipeThick);

    // rect
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "rwidth", &dRectWidth);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "rheight", &dRectHeight);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "rthick", &dRectThick);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "fwidth", &dFlanWidth);

    // common
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "fthick", &dFlanThick);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_DOUBLE), "wthick", &dWallThick);

    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "just", &iJust);

    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "blick", &iBlick);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "mode", &iMode);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "fqty", &iFlanges);

    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "lvlF", &lvlF);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "lvlP", &lvlP);

    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "iflan", &pp.iFlan);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "idiam", &pp.iDiam);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "iwall", &pp.iWall);
    mdlDialog_publishBasicArray(pSet, mdlCExpression_getType(TYPECODE_CHAR), "scode", &pp.sCode, sizeof(pp.sCode));
    mdlDialog_publishBasicArray(pSet, mdlCExpression_getType(TYPECODE_CHAR), "sname", &pp.sName, sizeof(pp.sName));
    //mdlDialog_publishBasicArray (pSet, mdlCExpression_getType(TYPECODE_CHAR), "signup", &pp.sSign[0], sizeof(pp.sSign[0]));
    //mdlDialog_publishBasicArray (pSet, mdlCExpression_getType(TYPECODE_CHAR), "signdn", &pp.sSign[1], sizeof(pp.sSign[1]));


    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "coordx", &coord[0]);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "coordy", &coord[1]);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "coordz", &coord[2]);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "icoordx", &icoord[0]);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "icoordy", &icoord[1]);
    mdlDialog_publishBasicVariable(pSet, mdlCExpression_getType(TYPECODE_LONG), "icoordz", &icoord[2]);

    //mdlSystem_setFunction(SYSTEM_ELMDSCR_TO_FILE, callbackElmdDscrToFile);
    //mdlChangeTrack_setFunction (CHANGE_TRACK_FUNC_Changed, callbackDgnFileChanged);

	mdlLocate_setFunction(LOCATE_GLOBAL_PRELOCATE, callbackLocateFilter);
	
	mdlLocate_setFunction(LOCATE_PROVIDE_PATH_DESCRIPTION, 
		Openings::LocateFunc_providePathDescription);

    return SUCCESS;
}