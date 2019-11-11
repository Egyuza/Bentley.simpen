#include "simpen.h"
#include "simpencmd.h"

#include "pipepen.h"
#include "RectPenLocate.h"
#include "RectPenDraw.h"
#include "RectPenPlace.h"

#include "OpeningTask.h"
#include "OpeningHelper.h"
#include "OpeningByContourTool.h"
#include "OpeningByTaskTool.h"

#include <msdialog.fdf>

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


void cmdConstructRect(char *unparsedP) {

    constructByTask();
}

typedef void(*Command_Handler) (char*);

Private MdlCommandNumber commandNumber[] =
{
    { (Command_Handler)placeFrameLibrary_start, CMD_SIMPEN_PLACE_LIBRARY },
    { (Command_Handler)placeFrameOrphan_start,  CMD_SIMPEN_PLACE_ORPHAN },
    { (Command_Handler)cmdMakePenetr,           CMD_SIMPEN_PLACEPEN },
    { (Command_Handler)cmdScanPenPrimary,       CMD_SIMPEN_PENPRIM },
    { (Command_Handler)cmdMakeEmbPlate,         CMD_SIMPEN_PLACEEMB },
    { (Command_Handler)cmdElem,                 CMD_SIMPEN_ELEM },
    { (Command_Handler)cmdTaskPen,              CMD_SIMPEN_TASK },
    { (Command_Handler)PartsReport,             CMD_SIMPEN_REPORT },
    { (Command_Handler)cmdMakeOpening,          CMD_SIMPEN_PLACEOP },

    { (Command_Handler)cmdConstructRect,        CMD_SIMPEN_CONSTRUCT_RECT },
    { (Command_Handler)cmdPlaceRect,            CMD_SIMPEN_PLACE_RECT },
    { (Command_Handler)cmdDrawRect,             CMD_SIMPEN_DRAW_RECT },
       
    { (Command_Handler)
        Openings::cmdLocateContour,             CMD_SIMPEN_LOCATE_CONTOUR },
    { (Command_Handler)
        Openings::OpeningByTaskTool::runTool,   CMD_SIMPEN_LOCATE_TASK },
    { (Command_Handler)
        Openings::cmdAddToModel,                CMD_SIMPEN_CONSTRUCT_OPENING },
    { (Command_Handler)
        Openings::cmdUpdatePreview,             CMD_SIMPEN_UPDATE_PREVIEW_OPENING },
    { (Command_Handler)
    Openings::cmdUpdateAll,                 CMD_SIMPEN_UPDATE_ALL_OPENINGS },
    0
};


//{(Command_Handler) cmdMakePenetrPX,            CMD_SIMPEN_PLACE_PX},
//{(Command_Handler) cmdMakePenetrPY,             CMD_SIMPEN_PLACE_PY},
//{(Command_Handler) cmdMakePenetrPZ,            CMD_SIMPEN_PLACE_PZ},
//{(Command_Handler) cmdMakePenetrNX,             CMD_SIMPEN_PLACE_NX},
//{(Command_Handler) cmdMakePenetrNY,            CMD_SIMPEN_PLACE_NY},
//{(Command_Handler) cmdMakePenetrNZ,             CMD_SIMPEN_PLACE_NZ},

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
    publishRectVariables(pSet);

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

    return SUCCESS;


}


void publishVarBase(void* setP, MdlTypecodes typeCode, char* varName, void* varP) // todo использовать
{
    mdlDialog_publishBasicVariable(setP, mdlCExpression_getType(typeCode), varName, varP);
}