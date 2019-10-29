#pragma once

#ifndef PIPEPEN_H
#define PIPEPEN_H

#include "simpen.h"

#include <mdl.h>
#include <mdlxmltools.fdf>
#include <mssystem.fdf>
#include <mslocate.fdf>

#include <dlogitem.h>
#include <changetrack.fdf>

#include <tfprojection.h>
#include <tfform.h>
#include <tfframe.h>
#include <tfperfo.h>

extern DVec3d pZ;
extern DVec3d pZero;
extern DVec3d pZneg;

extern ULong lvlF;
extern ULong lvlP;

//int iDirection;
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

typedef struct PenProp {
    char sType[500];
    char sName[500];
    char sCode[500];
    //char sSign[2][100];
    int iFlan;
    int iDiam;
    int iWall;
    DVec3d pLoc;
    RotMatrix rm;
    UInt32 fpos;
    char sPath[500];
    char sUser[50];
    char sDate[50];
} penprop;

extern penprop pp;

extern FrameDataGUI g_frameDataGUI;

void startPenPlaceCmd(
    char    *unparsedP
);

void cmdTaskPen(char * unparsedP);

void cmdElem(char * unparsedP);

void cmdMakeOpening(char * unparsedP);

void cmdMakePenetr(char * unparsedP);

void cmdMakeEmbPlate(char * unparsedP);

int processInstance(Bentley::WString strInst);

int    locateTaskPointShow(DVec3d *ptP, int view);

int eleFunc(MSElementUnion * element, void * params, DgnModelRefP modelRef, MSElementDescr * elmDscrP, MSElementDescr ** newDscrPP, ModifyElementSource elemSource);

int scanPenPrimary(MSElementDescr * edDstP, void * param, ScanCriteria * pScanCriteria);

void cmdScanPenPrimary(char * unparsedP);

MSElementDescr * makeTube2(double dInsideRadius, double dOutsideRadius, double dHeight, double dOffset, UInt32 iLevID, bool bMakePerf);

MSElementDescr * makePlate2(double dHeight, double dWidth, double dLength, double dThickness, double dOffset, UInt32 iLevID, int bBlick);

MSElementDescr * makePlate(double dHeight, double dWidth, double dLength, double dWallWidth, long iThickness);

int makeOpening();

int makeRectPenetr();

int makePenetr();

void restartDefault();

int drawPart(Dpoint3d * pt, int view);

void placeEmbPlate(Dpoint3d * pt, int view);

void placePenetr(Dpoint3d * pt, int view);


void callbackDgnFileChanged(
    MSElementDescr * newDescr, 
    MSElementDescr * oldDescr, 
    ChangeTrackInfo * info, 
    BoolInt * cantBeUndoneFlag
);

ElmDscrToFile_Status callbackElmdDscrToFile(
    ElmDscrToFile_Actions action,
    DgnModelRefP modelRef, 
    UInt32 filePos, 
    MSElementDescr * newEdP, 
    MSElementDescr * oldEdP, 
    MSElementDescr ** replacementEdPP
);

void startEmbPlaceCmd(
    char    *unparsedP
);

int  PartsReport(char* unparsedP);

LocateFilterStatus  callbackLocateFilter(
    LOCATE_Action       action,
    MSElement*       pElement,
    DgnModelRefP       modelRef,
    UInt32       filePos,
    DVec3d*       pPoint,
    int       viewNumber,
    HitPathP       hitPath,
    char*       rejectReason
);


void hook_txCellName(
    DialogItemMessage* dimP /* => a ptr to a dialog item message */
);

long roundExt(double val, int digs, double snap, int shft);

int char_replace(char * str, char ch1, char ch2);

int userFuncNodeListIMod(XmlNodeRef node, void * sValueVoid);

DialogBox* findToolBox();

void syncToolbox();

int elemXInfo(ElementRef eref, DgnModelRefP mrP);

int locateTaskPointAccept(DVec3d * ptP, int view);

void locateTaskPointReset();

TFFormRecipeFreeList* formRecipeFreeList_create
(
    DPoint3d* pBasePoints,  /* => */
    int       nBasePoints,  /* => */
    double    height,       /* => */
    DVec3d*   pSweepDir     /* => */
);

StatusInt createFrameIngredients
(
    MSElementDescr**   pp3DEd,              /* <= */
    TFProjectionList** ppProjectionNode,    /* <= */
    TFPerforatorList** ppPerforatorNode,    /* <= */
    TFFormRecipeList** ppFormNode           /* <= */
);

TFFrameList* createFrameNode(void);
void placeFrameOrphan_dataPoint
(
    Dpoint3d *pPoint,   /* => */
    int       view      /* => */
);
void placeFrameOrphan_start(char* arg);
void placeFrameLibrary_cleanupFunc
(
    void
);
StatusInt getTransform
(
    Transform        *pTMatrix, /* <= */
    MSElementDescr   *pEd,      /* => */
    DPoint3d     *pPoint    /* => */
);
void placeFrameLibrary_acceptElement
(
    Dpoint3d *pPoint,   /* => */
    int       view      /* => */
);
void placeFrameLibrary_start(char* arg);

#endif // !PIPEPEN_H