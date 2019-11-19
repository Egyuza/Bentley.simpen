/*
	ekzamiralov:
	Код получен от Леонида Вибе и максимально сохранён в первозданном виде.
	Изначально назывался "simpen.cpp" и был основным файлом библиотеки.
*/


#include "pipepen.h"
#include "simpencmd.h"
#include "ElementHelper.h"

/*----------------------------------------------------------------------+
|                                                                       |
|   Include Files                                                       |
|                                                                       |
+----------------------------------------------------------------------*/
#include <string.h>

#include <msrmgr.h>

#include <buildingeditelemhandle.h>
#include <CatalogCollection.h>
#include <ListModelHelper.h>
#include <interface/ElemHandle.h>
#include <interface/ILocate.h>

#include <elementref.h>

#include <mssystem.fdf>
#include <msparse.fdf>
#include <msstate.fdf>
#include <msoutput.fdf>
#include <msdialog.fdf>
#include <mscexpr.fdf>
#include <ditemlib.fdf>
#include <msrmatrx.fdf>
#include <mstmatrx.fdf>
#include <msvec.fdf>
#include <msmisc.fdf>
#include <mselmdsc.fdf>
#include <mselemen.fdf>
#include <mslocate.fdf>
#include <mscurrtr.fdf>
#include <msmodel.fdf>
#include <msselect.fdf>
#include <mdlxmltools.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <mstxtfil.h>

#include <mdltfmodelref.fdf>
#include <mdltfframe.fdf>
#include <mdltfwstring.fdf>
#include <mdltfprojection.fdf>
#include <mdltfcelllib.fdf>
#include <mdltfform.fdf>
#include <mdltflform.fdf>
#include <mdltffrform.fdf>
#include <mdltfperfo.fdf>
#include <mdltfpoly.fdf>
#include <mdltfglobal.fdf>
#include <mdltfpartref.fdf>
#include <msinput.fdf>
#include <msscancrit.fdf>
#include <msvar.fdf>

#include <mskisolid.fdf>
#include <mscnv.fdf>
#include <mscell.fdf>
#include <mdllib.fdf>
#include <msvba.fdf>
#include <mstrnsnt.fdf>
#include <msdgncache.fdf>
#include <msdgnmodelref.fdf>
#include <changetrack.fdf>

#include <tfpoly.h>

//#include <XAttributeIter.h>
#include <ElementGraphics.h>
#include <IModel/xmlinstanceapi.h> 
#include <IModel/xmlinstanceschemamanager.h> 

/*----------------------------------------------------------------------+
|                                    |
|   Global variables                        |
|                                    |
+----------------------------------------------------------------------*/
FrameDataGUI g_frameDataGUI;

/*----------------------------------------------------------------------+
|                                    |
|   Global variables                        |
|                                    |
+----------------------------------------------------------------------*/
/*----------------------------------------------------------------------+
|                                    |
|   External variables                            |
|                                    |
+----------------------------------------------------------------------*/
/*======================================================================+
|                                    |
|   Utility Routines                        |
|                                    |
+======================================================================*/

//double dFlangeOutDiam = 0.;
//double dFlangeInnDiam = 0.;
//double dFlangeWidth = 0.;
//
//double dPenOutDiam = 0.;
//double dPenInnDiam = 0.;
//double dPenLength = 0.;
//
//int iFlangeQty = 0;

// for manual placing
#define MODE_PEN      1
#define MODE_OPENING  2
#define MODE_RECT     3

//int iCfgVar_ArrOpen = -1;
//int iCfgVar_ArrSize = -1;
//int iCfgVar_DotFilled = -1;
//int iCfgVar_DotSize = -1;

DVec3d ptsShape[MAX_VERTICES];

double penparams[22];

ULong lvlF = 0;
ULong lvlP = 0;

//int iDirection = 0;
int iMode = 0;
int iFlanges = 0;
int iByMessage = 0;
int iPenType = 0;
int iBlick = 0;
int iJust = 1;

long coord[3];
long icoord[3];

double dFlanDiam = 0.;
double dPipeDiam = 0.;
double dPipeThick = 0.;

double dRectWidth = 0.;
double dRectHeight = 0.;
double dRectThick = 0.;
double dFlanWidth = 0.;

double dFlanThick = 0.;
double dWallThick = 0.;

int iAC = 0;
int iACStep = 0;

char s[10000];
char ss[10000];
char sss[10000];

DVec3d pZ;
DVec3d pZero;
DVec3d pZneg;

unsigned int flagLocateSurfaces = 100;

//extern TransDescrP msTransientElmP = NULL;

MSElementUnion el;
MSElementUnion elTmp;
MSElementUnion elPerf;
MSElementUnion elBlick[3];


MSElementDescr *edpPart = NULL;
MSElementDescr *edpPenPerf = NULL;
//MSElementDescr *edpPenPerf[2] = {NULL,NULL}; 
MSElementDescr *edpPenProj[2] = { NULL,NULL };
MSElementDescr *edpPartBuf = NULL;
MSElementDescr *edpLine = NULL;

Transform tmForPen;

double dPerfLenUors = 0.;

int values[3]; // penetration params

               // double and float comparison
#define DBL_EPSILON     2.2204460492503131e-016 /* smallest such that 1.0+DBL_EPSILON != 1.0 */
#define FLT_EPSILON     1.192092896e-07        /* smallest such that 1.0+FLT_EPSILON != 1.0 */
#define EQ(x,v) (((v - FLT_EPSILON) < x) && (x <( v + FLT_EPSILON)))

int  PartsReport(char* unparsedP);

char sRepExcl[5000];
char sLocFilter[5000];


char sLoc[500];

penprop pp;

void startPenPlaceCmd(
    char    *unparsedP
);

void startEmbPlaceCmd(
    char    *unparsedP
);

bool bSetProp = false;

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             08/03
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
StatusInt frameSetPartData
(
    TFFrame* pThis /* <=> */
) {
    TFPartRefList* pPartRefNode = NULL;

    mdlTFGlobal_getActivePartRef(&pPartRefNode);
    mdlTFFrame_setPartRef(pThis, mdlTFPartRefList_getPartRef(pPartRefNode));
    mdlTFPartRefList_free(&pPartRefNode);

    return SUCCESS;
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             08/03
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
TFFormRecipeFreeList* formRecipeFreeList_create
(
    DPoint3d* pBasePoints,  /* => */
    int       nBasePoints,  /* => */
    double    height,       /* => */
    DVec3d*   pSweepDir     /* => */
) {
    TFFormRecipeFreeList* pRecipeNode = mdlTFFormRecipeFreeList_construct();
    TFFormRecipeFree*     pRecipe = mdlTFFormRecipeFreeList_getFormRecipeFree(pRecipeNode);
    TFPolyList*           pPolyNode = mdlTFPolyList_construct();
    TFPoly*               pPoly = mdlTFPolyList_getPoly(pPolyNode);

    mdlTFFormRecipeFree_setTopFixedHeight(pRecipe, height);
    mdlTFFormRecipeFree_setSweepDirection(pRecipe, pSweepDir);

    mdlTFPoly_addPointArray(pPoly, pBasePoints, nBasePoints);

    mdlTFFormRecipeFree_setBase(pRecipe, pPolyNode);
    mdlTFFormRecipe_initLocalCoords(pRecipe);

    return pRecipeNode;
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
StatusInt createFrameIngredients
(
    MSElementDescr**   pp3DEd,              /* <= */
    TFProjectionList** ppProjectionNode,    /* <= */
    TFPerforatorList** ppPerforatorNode,    /* <= */
    TFFormRecipeList** ppFormNode           /* <= */
)
{
    MSElementDescr* pBaseEd = NULL;
    MSElementDescr* pProjectionEd = NULL;
    MSElementDescr* pPerforatorEd = NULL;
    double          uorPerMast = mdlModelRef_getUorPerMaster(ACTIVEMODEL);
    double          height = 6.0 * uorPerMast;
    double          senseDist = 100 * uorPerMast;
    MSElementUnion  el;
    DPoint3d        points[5];
    DPoint3d        startPoint;
    DPoint3d        endPoint;
    DVec3d          sweepDir;

    if (!pp3DEd || !ppProjectionNode || !ppPerforatorNode || !ppFormNode) {
        return BSIERROR;
    }

    /* create a type 19 solid element for the 3D part of the frame */
    memset(points, 0, sizeof(points));
    points[1].x = points[2].x = 10.0 * uorPerMast;
    points[2].y = points[3].y = 5.0 * uorPerMast;
    mdlShape_create(&el, NULL, points, 5, 0);
    mdlElmdscr_new(&pBaseEd, NULL, &el);
    memset(&startPoint, 0, sizeof(startPoint));
    memset(&endPoint, 0, sizeof(endPoint));
    endPoint.z = 8.0 * uorPerMast;

    if (SUCCESS == mdlSurface_project(pp3DEd, pBaseEd, &startPoint, &endPoint, NULL)) {
        (*pp3DEd)->el.hdr.ehdr.type = SOLID_ELM;
    }

    mdlElmdscr_freeAll(&pBaseEd);

    /* create a TFProjectionNode from a shape. The TFProjection can be used as a piece of replacement
    geometry in a drawing that is generated by the Drawing Extraction Manager  */
    memset(points, 0, sizeof(points));
    points[1].x = points[2].x = 10.0 * uorPerMast;
    points[0].y = points[1].y = points[4].y = 7.0 * uorPerMast;
    points[2].y = points[3].y = 11.0 * uorPerMast;
    mdlShape_create(&el, NULL, points, 5, 0);
    mdlElmdscr_new(&pProjectionEd, NULL, &el);
    if (*ppProjectionNode = mdlTFProjectionList_construct()) {
        TFProjection* pProjection = mdlTFProjectionList_getProjection(*ppProjectionNode);
        mdlTFProjection_setEmbeddedElmdscr(pProjection, pProjectionEd, TRUE); // pProjectionEd is consumed, no need to free it
    }

    /* create a TFPerforatorNode. If a frame has a perforator, it is used to define the shape of the opening
    that is cut into walls that are within the sense distance of the perforator */
    memset(points, 0, sizeof(points));
    points[1].x = points[2].x = 10.0 * uorPerMast;
    points[2].z = points[3].z = 6.0 * uorPerMast;
    mdlShape_create(&el, NULL, points, 5, 0);
    mdlElmdscr_new(&pPerforatorEd, NULL, &el);
    mdlTFPerforatorList_constructFromElmdscr(ppPerforatorNode, pPerforatorEd, NULL, senseDist, NULL);
    mdlElmdscr_freeAll(&pPerforatorEd);

    /* create a free form */
    memset(points, 0, sizeof(points));
    points[1].x = points[2].x = 10.0 * uorPerMast;
    points[2].y = points[3].y = -5.0 * uorPerMast;
    memset(&sweepDir, 0, sizeof(sweepDir));
    sweepDir.z = 1.0;
    *ppFormNode = formRecipeFreeList_create(points, 4, height, &sweepDir);

    return SUCCESS;
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
TFFrameList* createFrameNode
(
    void
) {
    TFFrameList*      pFrameNode = NULL;
    MSElementDescr*   p3DEd = NULL;
    TFFormRecipeList* pFormNode = NULL;
    TFProjectionList* pProjectionNode = NULL;
    TFPerforatorList* pPerforatorNode = NULL;

    createFrameIngredients(&p3DEd, &pProjectionNode, &pPerforatorNode, &pFormNode);

    if (pFrameNode = mdlTFFrameList_construct()) {
        TFFrame*       pFrame = mdlTFFrameList_getFrame(pFrameNode);
        TFWStringList* pNameNodeW = mdlTFWStringList_constructFromCharString("MyFrame");

        mdlTFFrame_add3DElmdscr(pFrame, p3DEd); // p3DEd is consumed, no need to free it

        mdlTFFrame_setProjectionList(pFrame, pProjectionNode);    // pProjectionNode is consumed, no need to frre it

        mdlTFFrame_setPerforatorList(pFrame, &pPerforatorNode); // pPerforatorNode is consumed, no need to free it

        mdlTFFrame_setFormRecipeList(pFrame, pFormNode); // pFormNode is consumed, no need to free it

        mdlTFFrame_setName(pFrame, mdlTFWStringList_getWString(pNameNodeW));

        mdlTFWStringList_free(&pNameNodeW);

        frameSetPartData(pFrame);
    }

    return pFrameNode;
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void placeFrameOrphan_dataPoint
(
    Dpoint3d *pPoint,   /* => */
    int       view      /* => */
) {
    TFFrameList* pFrameNode = createFrameNode();
    TFFrame*     pFrame = mdlTFFrameList_getFrame(pFrameNode);
    Transform    trans;
    RotMatrix    viewMatrix;
    RotMatrix    viewMatrixI;

    mdlTMatrix_getIdentity(&trans);
    mdlTMatrix_setTranslation(&trans, pPoint);

    mdlRMatrix_fromView(&viewMatrix, view, TRUE);
    mdlRMatrix_transpose(&viewMatrixI, &viewMatrix);
    mdlTMatrix_rotateByRMatrix(&trans, &trans, &viewMatrixI);
    mdlTFFrame_transform(pFrame, &trans, TRUE);

    mdlTFFrame_synchronize(pFrame);

    mdlTFModelRef_addFrame(ACTIVEMODEL, pFrame);

    mdlTFFrameList_free(&pFrameNode);

    mdlState_restartCurrentCommand();
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void placeFrameOrphan_start(char* arg) {
    mdlState_startPrimitive(placeFrameOrphan_dataPoint, placeFrameOrphan_start, COMMANDID_PlaceFrameOrphan, PROMPTID_EnterDataPoint);
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void placeFrameLibrary_cleanupFunc
(
    void
) {
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
StatusInt getTransform
(
    Transform        *pTMatrix, /* <= */
    MSElementDescr   *pEd,      /* => */
    DPoint3d     *pPoint    /* => */
) {
    StatusInt         status = BSIERROR;
    TFFormRecipeList* pFormNode = NULL;

    if (SUCCESS == mdlTFFormRecipeList_constructFromElmdscr(&pFormNode, pEd)) {
        TFFormRecipe* pForm = mdlTFFormRecipeList_getFormRecipe(pFormNode);

        if (TF_LINEAR_FORM_ELM == mdlTFFormRecipe_getType(pForm)) {
            Dpoint3d  start;
            DPoint3d  end;
            DPoint3d  org1;
            Dpoint3d  xAxis;
            DPoint3d  point3;
            DPoint3d  p;
            DVec3d    sweepDir;
            DVec3d    vec1;
            DVec3d    vec3;
            DVec3d    vec4;
            DVec3d    vec5;
            RotMatrix rMatrix;
            double    dot;

            mdlTFFormRecipeLinear_getSweepDirection(pForm, &sweepDir);

            mdlTFFormRecipeLinear_getEndPoints(pForm, &start, &end);

            memset(&org1, 0, sizeof(DPoint3d));
            mdlVec_subtractPoint(&vec1, &end, &start);
            mdlVec_normalize(&vec1);
            mdlVec_normalize(&sweepDir);
            mdlVec_crossProduct(&vec3, &sweepDir, &vec1);
            mdlVec_addPoint(&xAxis, &org1, &vec1);
            mdlVec_addPoint(&point3, &org1, &vec3);
            mdlRMatrix_from3Points(&rMatrix, &org1, &xAxis, &point3);

            mdlVec_subtractPoint(&vec4, pPoint, &start);
            dot = mdlVec_dotProduct(&vec4, &vec1);
            mdlVec_scale(&vec5, &vec1, dot);
            mdlVec_addPoint(&p, &start, &vec5);

            mdlTMatrix_fromRMatrix(pTMatrix, &rMatrix);
            mdlTMatrix_setTranslation(pTMatrix, &p);

            status = SUCCESS;
        }
    }

    mdlTFFormRecipeList_free(&pFormNode);

    return status;
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void placeFrameLibrary_acceptElement
(
    Dpoint3d *pPoint,   /* => */
    int       view      /* => */
) {
    MSElementDescr* pEd = NULL;
    ULong        filePos = 0L;
    DgnModelRefP    modelRef = INVALID_MODELREF;

    filePos = mdlElement_getFilePos(FILEPOS_CURRENT, &modelRef);

    if (0L != mdlElmdscr_readToMaster(&pEd, filePos, modelRef, 0, NULL)) {
        DPoint3d  hitPoint;
        Transform tMatrix;

        mdlLocate_getHitPoint(&hitPoint);

        if (SUCCESS == getTransform(&tMatrix, pEd, pPoint)) {
            TFCellLibraryList *pCellLibNode = mdlTFCellLibraryList_construct();
            TFCellLibrary*     pCellLib = mdlTFCellLibraryList_getCellLibrary(pCellLibNode);
            TFFrameList*       pFrameNode = NULL;

            mdlTFCellLibrary_setName(pCellLib, L"08c_door");

            if (SUCCESS == mdlTFCellLibrary_getFrameNode(pCellLib, &pFrameNode, NULL, L"0810AA", TRUE, ACTIVEMODEL)) {
                TFFrame* pFrame = mdlTFFrameList_getFrame(pFrameNode);

                mdlTFFrame_transform(pFrame, &tMatrix, TRUE);

                //you may need to make perforators active for frames created before 8.5
                mdlTFFrame_setPerforatorsAreActive(pFrame, TRUE);

                // add frame to dgn
                mdlTFModelRef_addFrame(ACTIVEMODEL, pFrame);

                //create openings in form that are found within the sense distance of the frame's perforators
                mdlTFModelRef_updateAutoOpeningsByFrame(ACTIVEMODEL, pFrame, TRUE, FALSE, FramePerforationPolicyEnum_None);
            }

            mdlTFFrameList_free(&pFrameNode);
            mdlTFCellLibraryList_free(&pCellLibNode);
        }

        mdlElmdscr_freeAll(&pEd);
    }

    mdlState_restartCurrentCommand();
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void placeFrameLibrary_start(char* arg) {
    mdlState_startModifyCommand(placeFrameLibrary_start, placeFrameLibrary_acceptElement, NULL, NULL, NULL, COMMANDID_PlaceFrameLibrary, PROMPTID_IdentifyLinearForm, FALSE, 0);
    mdlLocate_init();
    mdlLocate_normal();

    mdlState_setFunction(STATE_COMMAND_CLEANUP, placeFrameLibrary_cleanupFunc);
}

/*---------------------------------------------------------------------------------**//**
                                                                                      * @bsimethod                                                    BSI             06/05
                                                                                      +---------------+---------------+---------------+---------------+---------------+------*/
void hook_txCellName
(
    DialogItemMessage* dimP /* => a ptr to a dialog item message */
) {
    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType) {
    case DITEM_MESSAGE_ALLCREATED:
        break;
    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}


///////////////////////////////////////////////////////////////
long roundExt(double val, int digs = -1, double snap = 5., int shft = 0) {

    double dv;

    dv = val * pow(snap, digs);

    dv = floor(dv + 0.55555555555555 - (0.111111111111111 * shft));

    dv = dv / pow(snap, digs);

    return (long)dv;
}

/////////////////////////////////////////////
int char_replace(char * str, char ch1, char ch2) {
    int changes = 0;
    while (*str != '\0') {
        if (*str == ch1) {
            *str = ch2;
            changes++;
        }
        str++;
    }
    return changes;
}


//////////////////////////////////
int userFuncNodeListIMod(XmlNodeRef node, void* sValueVoid) {


    int numn = 500;
    int numv = 500;
    MSWChar wNodeName[500];
    MSWChar wNodeValue[500];
    //MSWChar wAttrName[500];
    //MSWChar wAttrValue[500];
    XmlNodeListRef    pNodeListRef;
    //XmlNamedNodeMapRef       pNodeMapRef ;
    long ntype = 0;
    char* sValue = (char*)sValueVoid;


    mdlXMLDomNode_getName(wNodeName, &numn, node);
    mdlXMLDomNode_getValue(wNodeValue, &numv, node);
    mdlXMLDomNode_getNodeType(&ntype, node);

    //printf("   %i   name %S   value %S \n", ntype, wNodeName, wNodeValue);
    

    if (ntype == 1) // name
    {

        if (sValue == NULL && strlen(pp.sType) == 0) // root
        {
            mdlCnv_convertUnicodeToMultibyte(wNodeName, -1, pp.sType, 500);
        }

        if (wcscmp(wNodeName, L"Name") == 0) // wNodeValue пока пустое
        {
            sValue = pp.sCode;
        }

        if (wcscmp(wNodeName, L"Description") == 0) // wNodeValue пока пустое
        {
            sValue = pp.sName;
            /*
            char seps[]   = "-";
            char *token;

            if (strlen(sValue) > 0)
            {
            token = strtok( sValue, seps );
            if (token != NULL )
            {
            if (token[0] == 'T') token++;
            pp.iFlan = atoi(token);
            }

            token = strtok( NULL, seps );
            if (token != NULL ) pp.iDiam = atoi(token);

            token = strtok( NULL, seps );
            if (token != NULL ) pp.iWall = atoi(token);
            }
            */

        }

        if (wcscmp(wNodeName, L"SP3D_UserLastModified") == 0) {
            sValue = pp.sUser;
        }
        if (wcscmp(wNodeName, L"SP3D_DateLastModified") == 0) {
            sValue = pp.sDate;
        }

        if (wcscmp(wNodeName, L"SP3D_SystemPath") == 0) {
            sValue = pp.sPath;
        }

        if (wcsncmp(wNodeName, L"Location", 8) == 0) {
            sValue = sLoc;
        }

        if (wcsncmp(wNodeName, L"OrientationMatrix", 17) == 0) {
            sValue = sLoc;
        }

        //<LocationX>30.6</LocationX>
        //<LocationY>51.4515762067868</LocationY>
        //<LocationZ>4.24998820482663</LocationZ>


        /*
        if (mdlXMLDomElement_getAllAttributes (&pNodeMapRef, node) == SUCCESS)
        {
        int nnum = mdlXMLDomAttrList_getNumChildren(pNodeMapRef);

        for (int i = 0; i < nnum; i++)
        {
        int numn = 500;
        int numv = 500;
        XmlNodeRef n;

        if (mdlXMLDomAttrList_getChild (&n, pNodeMapRef, i) == SUCCESS)
        {
        mdlXMLDomNode_getName(wAttrName, &numn, n);
        mdlXMLDomNode_getValue(wAttrValue, &numv, n);

        printf("   attr %S = %S\n", wAttrName, wAttrValue);


        mdlXMLDomNode_free(n);
        }

        }

        mdlXMLDomAttrList_free (pNodeMapRef);
        }

        */

        if (mdlXMLDomNode_hasChildNodes(node)) {
            if (mdlXMLDomNode_getChildNodes(&pNodeListRef, node) == SUCCESS) {
                mdlXMLDomNodeList_traverse(pNodeListRef, 0, userFuncNodeListIMod, sValue);
                mdlXMLDomNodeList_free(pNodeListRef);
            }
        }

        if (wcscmp(wNodeName, L"LocationX") == 0) {
            if (strlen(sValue) > 0) {
                double xx = atof(sLoc);
                coord[0] = roundExt(xx * 1000.);
                icoord[0] = 1;
                pp.pLoc.x = mdlCnv_masterUnitsToUors(coord[0]);
            }
        }

        if (wcscmp(wNodeName, L"LocationY") == 0) {
            if (strlen(sValue) > 0) {
                double yy = atof(sLoc);
                coord[1] = roundExt(yy * 1000.);
                icoord[1] = 1;
                pp.pLoc.y = mdlCnv_masterUnitsToUors(coord[1]);
            }
        }

        if (wcscmp(wNodeName, L"LocationZ") == 0) {
            if (strlen(sValue) > 0) {
                double zz = atof(sLoc);
                coord[2] = roundExt(zz * 1000.);
                icoord[2] = 1;
                pp.pLoc.z = mdlCnv_masterUnitsToUors(coord[2]);
            }
        }

        if (wcscmp(wNodeName, L"OrientationMatrix_x0") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[0][0] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_y0") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[0][1] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_z0") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[0][2] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_x1") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[1][0] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_y1") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[1][1] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_z1") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[1][2] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_x2") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[2][0] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_y2") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[2][1] = roundExt(atof(sLoc), 5, 10., 0);
        }
        if (wcscmp(wNodeName, L"OrientationMatrix_z2") == 0) {
            if (strlen(sValue) > 0) pp.rm.form3d[2][2] = roundExt(atof(sLoc), 5, 10., 0);
        }

    }

    if (ntype == 3 && sValue) // value
    {
        mdlCnv_convertUnicodeToMultibyte(wNodeValue, -1, sValue, 200);
    }



    return SUCCESS;

}

/////////////////////////
DialogBox* findToolBox() {
    DialogBox *dbP;

    if (NULL != (dbP = mdlDialog_find(-79, NULL))) // DIALOGID_ToolSettings
        return dbP;
    else
        return NULL;
}

///////////////////
void syncToolbox() {
    DialogBox *dbP;

    if (NULL != (dbP = findToolBox())) {
        mdlDialog_itemsSynch(dbP);
    }
}


/////////////////////////////////
int elemXInfo(
    ElementRef  eref,
    DgnModelRefP mrP
) {
    // предполагается что схема уже загружена в ACTIVEMODEL
    //MDL LOAD bentley.ecxAttributesAddin.dll
    //ECX SCHEMA EXPORT
    //ECX SCHEMA import fullpath

    bool bStatus;
    Bentley::WString strInstId;
    MSElementDescr* edP = NULL;
    int ret = ERROR;

    Bentley::XMLInstanceAPI::Native::XmlInstanceStatus stt;


    int itype = elementRef_getElemType(eref);

    ElementID elid = elementRef_getElemID(eref);


    Bentley::XMLInstanceAPI::Native::XmlInstanceSchemaManager mgrR(mrP);

    mgrR.ReadSchemas(bStatus);

    Bentley::XMLInstanceAPI::Native::XmlInstanceApi xapiR = Bentley::XMLInstanceAPI::Native::XmlInstanceApi::CreateApi(stt, mgrR);
    
    stt.status = Bentley::XMLInstanceAPI::Native::LICENSE_FAILED;
    Bentley::XMLInstanceAPI::Native::StringListHandle slhR = xapiR.ReadInstances(stt, eref);





    for (int i = 0; i < slhR.GetCount(); i++) {
        Bentley::WString strInst = slhR.GetString(i);

        //printf("\n---------\n%S\n-----------", strInst.GetMSWCharCP());

        strInstId = xapiR.GetInstanceIdFromXmlInstance(stt, strInst);

        strInstId.ToChar(sss, 5000);

        Bentley::XMLInstanceAPI::Native::StringListHandle slrh = xapiR.ReadRelationshipInstances(stt, strInstId);


        ret = processInstance(strInst);

        if (strcmp(pp.sType, "P3DEquipment") == 0) break;


        for (int a = 0; a < slrh.GetCount(); a++) {

            Bentley::WString strRelInst = slrh.GetString(a);

            mdlCnv_convertUnicodeToMultibyte(strRelInst.GetMSWCharCP(), -1, s, 5000);

            //printf("\n=================================\n%s\n==================================",s);

            char* sNull;
            char* sSrcInstId = strstr(s, "sourceInstanceID=\"");

            if (sSrcInstId) {
                sSrcInstId = strstr(sSrcInstId, "\"");

                sSrcInstId++;

                sNull = strstr(sSrcInstId, "\"");
                if (sNull) *sNull = 0;

                Bentley::WString strInstToAdd;

                Bentley::WString strSrcInstId(sSrcInstId);
                strInstToAdd = xapiR.ReadInstance(strSrcInstId);
                strInstToAdd.ToChar(ss, 5000);

                //printf("\n+++++++++++++++\n%s\n+++++++++++++++++",ss);

                ret = processInstance(strInstToAdd);

                /*
                XmlDomRef pDomRef;
                XmlNodeRef pNodeRef;
                if (mdlXMLDom_createFromText (&pDomRef, 0, strInstToAdd.GetMSWCharCP()) == SUCCESS)
                {
                //if (mdlXMLDom_validate(pDomRef) == SUCCESS)
                {
                mdlXMLDom_getRootNode (&pNodeRef, pDomRef);

                int numn = 500;
                XmlNodeListRef    pNodeListRef ;
                long ntype = 0;


                //mdlXMLDomNode_getName(wNodeName, &numn, pNodeRef);
                //mdlXMLDomNode_getValue(wNodeValue, &numn, pNodeRef);

                if (mdlXMLDomNode_hasChildNodes(pNodeRef))
                {
                if (mdlXMLDomNode_getChildNodes(&pNodeListRef, pNodeRef) == SUCCESS)
                {
                mdlXMLDomNodeList_traverse(pNodeListRef, 0, userFuncNodeListIMod, (char*)0);
                mdlXMLDomNodeList_free (pNodeListRef);
                }

                ret = SUCCESS;
                }
                }
                }
                */
            }
            /*

            <P3DEquipment instanceID="DGNEC::15781b0000::ECXA::1" xmlns="SP3DReview.04.02">
            <ConstructionStatus>2</ConstructionStatus>
            <ConstructionStatus2>2</ConstructionStatus2>
            <EqType0>2780</EqType0>
            <FabricationType>7</FabricationType>
            <FabricationRequirement>10</FabricationRequirement>
            <LocationX>27.2</LocationX>
            <LocationY>53.5</LocationY>
            <LocationZ>3.7</LocationZ>
            <MTO_ReportingRequirements>5</MTO_ReportingRequirements>
            <MTO_ReportingType>5</MTO_ReportingType>
            <SP3D_DateCreated>636636916070000000</SP3D_DateCreated>
            <SP3D_DateLastModified>636728569000000000</SP3D_DateLastModified>
            <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
            <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
            <SP3D_PermissionGroup>129</SP3D_PermissionGroup>
            <OrientationMatrix_x0>-1</OrientationMatrix_x0>
            <OrientationMatrix_x1>4.61328024427608E-17</OrientationMatrix_x1>
            <OrientationMatrix_x2>0</OrientationMatrix_x2>
            <OrientationMatrix_y0>5.44811591673962E-17</OrientationMatrix_y0>
            <OrientationMatrix_y1>-1</OrientationMatrix_y1>
            <OrientationMatrix_y2>0</OrientationMatrix_y2>
            <OrientationMatrix_z0>-2.01119502996061E-17</OrientationMatrix_z0>
            <OrientationMatrix_z1>1.20820779596181E-17</OrientationMatrix_z1>
            <OrientationMatrix_z2>1</OrientationMatrix_z2>
            <Range1X>26.798999786377</Range1X>
            <Range1Y>53.2290000915527</Range1Y>
            <Range1Z>3.42899990081787</Range1Z>
            <Range2X>27.201000213623</Range2X>
            <Range2Y>53.7709999084473</Range2Y>
            <Range2Z>3.97099995613098</Range2Z>
            <Oid>00004e2e-0000-0000-8ef3-25a0135b5004</Oid>
            <UID> @a=0027!!20014##310848514137912206</UID>
            <Name>10KLE72BQ2667</Name>
            <Description>T1-9-40</Description>
            <CatalogPartNumber>PenRound-t1</CatalogPartNumber>
            <ShortMaterialDescription>HVAC Penetration</ShortMaterialDescription>
            <SP3D_SystemPath>HnhNPP\1\10KLE</SP3D_SystemPath>
            <SP3D_UserCreated>SP\AVSevostyanov</SP3D_UserCreated>
            <SP3D_UserLastModified>SP\a_terentiev</SP3D_UserLastModified>
            </P3DEquipment>

            636397648190000000-
            621355968000000000

            <P3DHangerPipeSupport instanceID="DGNEC::15f2400000::ECXA::1" xmlns="SP3DReview.04.02">
            <Global_Wet_Operating_CoG_X>30.6127</Global_Wet_Operating_CoG_X>
            <Global_Wet_Operating_CoG_Y>51.4230762067868</Global_Wet_Operating_CoG_Y>
            <Global_Wet_Operating_CoG_Z>4.11198820482663</Global_Wet_Operating_CoG_Z>
            <Wet_Operating_Weight>8.53822051392638E-05</Wet_Operating_Weight>
            <SP3D_DateCreated>636262987720000000</SP3D_DateCreated>
            <SP3D_DateLastModified>636397648190000000</SP3D_DateLastModified>
            <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
            <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
            <SP3D_PermissionGroup>244</SP3D_PermissionGroup>
            <OrientationMatrix_x0>0</OrientationMatrix_x0>
            <OrientationMatrix_x1>1</OrientationMatrix_x1>
            <OrientationMatrix_x2>0</OrientationMatrix_x2>
            <OrientationMatrix_y0>-1</OrientationMatrix_y0>
            <OrientationMatrix_y1>1.1817803680092E-17</OrientationMatrix_y1>
            <OrientationMatrix_y2>0</OrientationMatrix_y2>
            <OrientationMatrix_z0>0</OrientationMatrix_z0>
            <OrientationMatrix_z1>-7.31836466427715E-19</OrientationMatrix_z1>
            <OrientationMatrix_z2>1</OrientationMatrix_z2>
            <Range1X>30.5990009307861</Range1X>
            <Range1Y>51.2910766601563</Range1Y>
            <Range1Z>4.08948802947998</Range1Z>
            <Range2X>31.0009994506836</Range2X>
            <Range2Y>51.6120758056641</Range2Y>
            <Range2Z>4.41048812866211</Range2Z>
            <LocationX>30.6</LocationX>
            <LocationY>51.4515762067868</LocationY>
            <LocationZ>4.24998820482663</LocationZ>
            <Group_EmptyWeight>8.53822051392638E-05</Group_EmptyWeight>
            <Oid>00022303-0000-0000-4035-f94dda58bf04</Oid>
            <UID> @a=0028!!140035##342089791337739584</UID>
            <Name>10KBF&amp;&amp;BQ2007</Name>
            <Description>T2-6-40</Description>
            <SP3D_UserCreated>SP\m_genadenik</SP3D_UserCreated>
            <SP3D_UserLastModified>SP\a_terentiev</SP3D_UserLastModified>
            <SP3D_SystemPath>HnhNPP\1\10KBF\021-10UKA-99-0001-01-10KBF50-BL</SP3D_SystemPath>
            </P3DHangerPipeSupport>

            1   name P3DMemberSystemLinear   value
            1   name MemberPriority   value
            3   name #text   value 0
            1   name SP3D_DateCreated   value
            3   name #text   value 636439341870000000
            1   name SP3D_DateLastModified   value
            3   name #text   value 636440747740000000
            1   name SP3D_ApprovalStatus   value
            3   name #text   value 1
            1   name SP3D_ApprovalReason   value
            3   name #text   value 1
            1   name SP3D_PermissionGroup   value
            3   name #text   value 244
            1   name Range1X   value
            3   name #text   value -21.7709999084473
            1   name Range1Y   value
            3   name #text   value -17.9095001220703
            1   name Range1Z   value
            3   name #text   value -8.03849983215332
            1   name Range2X   value
            3   name #text   value -21.4290008544922
            1   name Range2Y   value
            3   name #text   value -17.8904991149902
            1   name Range2Z   value
            3   name #text   value -7.61149978637695
            1   name Group_EmptyWeight   value
            3   name #text   value 6.67168999525555
            1   name DVCS_Easting   value
            3   name #text   value -21.6
            1   name DVCS_Northing   value
            3   name #text   value -8.0375
            1   name DVCS_Elevation   value
            3   name #text   value -17.895
            1   name End2_LocationX   value
            3   name #text   value -21.6
            1   name End2_LocationY   value
            3   name #text   value -17.895
            1   name End2_LocationZ   value
            3   name #text   value -7.6125
            1   name Length   value
            3   name #text   value 0.425
            1   name Oid   value
            3   name #text   value 0003a98b-0000-0000-861a-1f40e7592f04
            1   name UID   value
            3   name #text   value  @a=0028!!240011##301558549805210246
            1   name Name   value
            3   name #text   value 10UJE99_NE0032V
            1   name SP3D_UserCreated   value
            3   name #text   value SP\d_lezin
            1   name SP3D_UserLastModified   value
            3   name #text   value SP\d_lezin
            1   name SP3D_SystemPath   value
            3   name #text   value HnhNPP\Task\10UJE\10UJE_Embadded plates\10UJE99

            */



        }
    }






    return ret;
}

/////////////////////////////////////////////
int    locateTaskPointAccept(
    DVec3d    *ptP,
    int        view) {

    return SUCCESS;
}

/////////////////////////////////////////////
void    locateTaskPointReset() {
    mdlState_startModifyCommand(locateTaskPointReset, locateTaskPointAccept, 0, locateTaskPointShow, 0, 0, 0, FALSE, 0);
    mdlLocate_init();
    mdlLocate_allowLocked();
}


/////////////////////////////////////////////
int    locateTaskPointShow(
    DVec3d    *ptP,
    int        view) {

    int ret = 0;
    DgnModelRefP curMrP = NULL;

    UInt32 fpos = mdlElement_getFilePos(FILEPOS_CURRENT, &curMrP);

    ElementRef eref = dgnCache_findElemByFilePos(mdlModelRef_getCache(curMrP), fpos, TRUE);

    memset(&pp, 0, sizeof(pp));

    if (eref) ret = elemXInfo(eref, curMrP);

    syncToolbox();

    if (strlen(pp.sName) > 0) mdlVBA_runProcedure(0, 0, "so2", "db", "mdlFoundTaskPen", 0, 0);

    return ret;


}


/////////////////////////////////
void cmdTaskPen(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_TASK
{


    if (mdlSystem_getCfgVar(sLocFilter, "SIMPEN_LOCATE_FILTER", 5000) != SUCCESS)
        memset(sLocFilter, 0, sizeof(sLocFilter));


    mdlLocate_clearHilited(TRUE);
    memset(&pp, 0, sizeof(pp));
    mdlState_startModifyCommand(locateTaskPointReset, locateTaskPointAccept, 0, locateTaskPointShow, 0, 0, 0, FALSE, 0);
    mdlLocate_init();
    mdlLocate_allowLocked();

}

/////////////////////////////////
void cmdElem(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_ELEM
{
    ElementID elid = 0;
    ElementRef eref;
    MSElementDescr* edP = NULL;
    DgnModelRefP mrP = ACTIVEMODEL;

    if (unparsedP && strlen(unparsedP) > 0) {
        if (sscanf(unparsedP, "%I64u", &elid) != 1) {
            return;
        }

        eref = dgnCache_findElemByID(mdlModelRef_getCache(ACTIVEMODEL), elid);

    }
    else {
        if (mdlSelect_getElement(0, &eref, &mrP) != SUCCESS) return;
    }

    if (eref) {
        mdlElmdscr_getByElemRef(&edP, eref, mrP, FALSE, 0);
    }
    else
        return;

    //==================================



    //elemXInfo(eref, mrP);


    //==================================

    if (eref) {
        Bentley::Building::Elements::BuildingEditElemHandle beeh(eref, ACTIVEMODEL);

        if (beeh.IsValid()) {

            beeh.GetCatalogCollection().InsertDataGroupCatalogInstance(L"EmbeddedPart", L"Embedded Part", L"APP");

            beeh.GetCatalogCollection().UpdateInstanceDataDefaults(L"EmbeddedPart");

            CCatalogSchemaItemT*    pSchemaItem = NULL;
            if (NULL != (pSchemaItem = beeh.GetCatalogCollection().FindDataGroupSchemaItem(L"EmbPart/@CatalogName")))
                pSchemaItem->SetValue(L"AAAAAA");

            if (beeh.Rewrite()) {
            }
        }
    }


    //==================================

    if (edP) mdlElmdscr_freeAll(&edP);



}
/*
/////////////////////////////////
void cmdMakePenetrPX(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_PX
{
iDirection = 1;
startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakePenetrPY(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_PY
{
iDirection = 2;
startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakePenetrPZ(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_PZ
{
iDirection = 3;
startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakePenetrNX(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_NX
{
iDirection = -1;
startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakePenetrNY(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_NY
{
iDirection = -2;
startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakePenetrNZ(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACE_NZ
{
iDirection = -3;
startPenPlaceCmd(unparsedP);
}
*/
/////////////////////////////////
void cmdMakePenetr(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACEPEN
{
    //iDirection = 0;

    mdlParams_setActive((void*)PRIMARY_CLASS, ACTIVEPARAM_CLASS);

    startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakeOpening(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACEOP
{
    //iDirection = 0;

    mdlParams_setActive((void*)PRIMARY_CLASS, ACTIVEPARAM_CLASS);

    //startPenPlaceCmd(unparsedP);
}

/////////////////////////////////
void cmdMakeEmbPlate(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PLACEEMB
{
    //iDirection = 0;

    mdlParams_setActive((void*)PRIMARY_CLASS, ACTIVEPARAM_CLASS);

    startEmbPlaceCmd(unparsedP);
}

//////////////////////
int eleFunc(
    MSElementUnion      *element,     // <=> element to be modified
    void                *params,      // => user parameter
    DgnModelRefP        modelRef,     // => model to hold current elem
    MSElementDescr      *elmDscrP,    // => element descr for elem
    MSElementDescr      **newDscrPP,  // <= if replacing entire descr
    ModifyElementSource elemSource   // => The source for the element.
) {
    //eleFunc return value Meaning 
    //MODIFY_STATUS_NOCHANGE The element was not changed. 
    //MODIFY_STATUS_REPLACE Replace original element with the new element. 
    //MODIFY_STATUS_DELETE Delete the original element. 
    //MODIFY_STATUS_ABORT Stop processing component elements. MODIFY_STATUS_ABORT can be used with any of the other values. 
    //MODIFY_STATUS_FAIL An error occurred during element modification. Ignore all changes previously made. 
    //MODIFY_STATUS_REPLACEDSCR Replace the current element descriptor with a new element descriptor. This is used to replace one or more elements with a (possibly) different number of elements. 
    //MODIFY_STATUS_ERROR MODIFY_STATUS_FAIL | MODIFY_STATUS_ABORT 

    int cl = PRIMARY_CLASS;

    mdlElement_setProperties(element, 0, 0, &cl, 0, 0, 0, 0, 0);

    return MODIFY_STATUS_REPLACE;
}

///////////////////////
int scanPenPrimary(
    MSElementDescr  *edDstP,
    void*  param,
    ScanCriteria    *pScanCriteria
) {

    TFFrameList* flP = mdlTFFrameList_constructFromElmdscr(edDstP);
    if (flP) {
        mdlTFFrameList_free(&flP);

        //ret = mdlModify_elementDescr2(&edDstP, ACTIVEMODEL, MODIFY_REQUEST_NOHEADERS, eleFunc, 0, 0);
        mdlModify_elementSingle(ACTIVEMODEL, mdlElmdscr_getFilePos(edDstP), MODIFY_REQUEST_NOHEADERS, MODIFY_ORIG, eleFunc, 0, 0);
    }



    return 0;
}

/////////////////////////////////
void cmdScanPenPrimary(
    char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_PENPRIM
{


    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status;



    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();
    status = mdlScanCriteria_setReturnType(scP, MSSCANCRIT_ITERATE_ELMDSCR, FALSE, TRUE);
    status = mdlScanCriteria_setElmDscrCallback(scP, (PFScanElemDscrCallback)scanPenPrimary, 0);
    status = mdlScanCriteria_setElementTypeTest(scP, typeMask, sizeof(typeMask));
    status = mdlScanCriteria_setCellNameTest(scP, L"EmbeddedPart");
    status = mdlScanCriteria_setModel(scP, ACTIVEMODEL);
    status = mdlScanCriteria_scan(scP, NULL, NULL, NULL);
    status = mdlScanCriteria_free(scP);


}

//////////////////////////////////
MSElementDescr* makeTube2(
    double dInsideRadius,
    double dOutsideRadius,
    double dHeight,
    double dOffset,
    UInt32 iLevID,
    bool bMakePerf
) {

    DPoint3d       base;
    DPoint3d       vec;
    DVec3d       nrm;
    RotMatrix rm;
    Transform tm;
    double shell;

    KIBODY  *kb_shape = NULL;

    MSElementDescr* edp = NULL;

    int ret;

    memset(&base, 0, sizeof(DPoint3d));
    memset(&vec, 0, sizeof(DPoint3d));
    memset(&nrm, 0, sizeof(DPoint3d));

    shell = dOutsideRadius - dInsideRadius;


    base.z = dOffset;
    //base.z = (iDirection / abs(iDirection)) * dOffset;
    vec.z = 1.;

    /*
    if (abs(iDirection) == 1)
    {
    nrm.x = (iDirection / abs(iDirection)) * 1.;
    }
    else if (abs(iDirection) == 2)
    {
    nrm.y = (iDirection / abs(iDirection)) * 1.;
    }
    else
    {
    nrm.z = (iDirection / abs(iDirection)) * 1.;
    }
    */

    mdlRMatrix_fromNormalVector(&rm, &nrm);
    mdlRMatrix_getInverse(&rm, &rm);
    mdlTMatrix_fromRMatrix(&tm, &rm);

    mdlEllipse_create(&el, NULL, &base, dOutsideRadius, dOutsideRadius, NULL, 0);
    mdlElmdscr_new(&edp, NULL, &el);
    //mdlElmdscr_appendElement (edp, &el);

    if (bMakePerf) {
        if (edpPenPerf) {
            mdlElmdscr_freeAll(&edpPenPerf);
            edpPenPerf = NULL;
        }

        mdlCnv_masterToUOR(&dPerfLenUors, dWallThick, MASTERFILE);



        DVec3d pcnt;
        mdlVec_zero(&pcnt); // !!!

        RotMatrix rm;
        mdlRMatrix_getIdentity(&rm);
        pcnt.z = 0.;
        mdlEllipse_create(&elPerf, 0, &pcnt, dOutsideRadius, dOutsideRadius, &rm, 0);
        mdlElmdscr_new(&edpPenPerf, NULL, &elPerf);
        //mdlElmdscr_setVisible(edpPenPerf, FALSE);




        if (edpPenProj[0]) {
            mdlElmdscr_freeAll(&edpPenProj[0]);
            edpPenProj[0] = NULL;
        }
        if (edpPenProj[1]) {
            mdlElmdscr_freeAll(&edpPenProj[1]);
            edpPenProj[1] = NULL;
        }

        DVec3d plin[8];
        UInt32 wgt = 0;
        memset(plin, 0, sizeof(plin));

        plin[1].x = dInsideRadius * 0.707;
        plin[1].y = dInsideRadius * 0.707;
        plin[3].x = dInsideRadius * 0.707;
        plin[3].y = -dInsideRadius * 0.707;
        plin[5].x = -dInsideRadius * 0.707;
        plin[5].y = dInsideRadius * 0.707;
        plin[7].x = -dInsideRadius * 0.707;
        plin[7].y = -dInsideRadius * 0.707;
        mdlLineString_create(&elPerf, 0, plin, 8);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_new(&edpPenProj[0], NULL, &elPerf);

        for (int i = 0; i < 8; i++) plin[i].z = dPerfLenUors;
        mdlLineString_create(&elPerf, 0, plin, 8);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_new(&edpPenProj[1], NULL, &elPerf);

        //mdlElmdscr_transform(edpPenProj[0], &tm);
        //mdlElmdscr_transform(edpPenProj[1], &tm);


    }

    // todo активировать, когда будет исправлено

    /// ===== доб 17/06/2019 ====
    /// ===== изм 21/06/2019 ====
    ///
    if (edpPenProj[0]) {
        DVec3d parc;
        UInt32 wgt = 2;
        mdlVec_zero(&parc);
        parc.z = dOffset;
        mdlArc_create(&elPerf, 0, &parc, dInsideRadius, dInsideRadius, 0, 0., fc_2pi);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_appendElement(edpPenProj[0], &elPerf);

        mdlArc_create(&elPerf, 0, &parc, dOutsideRadius, dOutsideRadius, 0, 0., fc_2pi);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_appendElement(edpPenProj[0], &elPerf);
    }

    if (edpPenProj[1]) {
        DVec3d parc;
        UInt32 wgt = 2;
        mdlVec_zero(&parc);
        parc.z = dOffset;
        mdlArc_create(&elPerf, 0, &parc, dInsideRadius, dInsideRadius, 0, 0., fc_2pi);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_appendElement(edpPenProj[1], &elPerf);

        mdlArc_create(&elPerf, 0, &parc, dOutsideRadius, dOutsideRadius, 0, 0., fc_2pi);
        mdlElement_setSymbology(&elPerf, 0, &wgt, 0);
        mdlElmdscr_appendElement(edpPenProj[1], &elPerf);
    }
    ///
    ///

    mdlKISolid_beginCurrTrans(MASTERFILE);

    mdlCurrTrans_invScaleDoubleArray(&dHeight, &dHeight, 1);
    mdlCurrTrans_invScaleDoubleArray(&shell, &shell, 1);

    ret = mdlKISolid_elementToBody(&kb_shape, edp, MASTERFILE);

    ret = mdlKISolid_sweepBodyVector(kb_shape, &vec, dHeight, -shell, 0.);

    ret = mdlKISolid_bodyToElement(&edp, kb_shape, TRUE, -1, NULL, MASTERFILE);

    if (iLevID) mdlElmdscr_setProperties(edp, &iLevID, 0, 0, 0, 0, 0, 0, 0);

    //mdlElmdscr_transform(edp, &tm);

    mdlKISolid_freeBody(kb_shape);

    mdlKISolid_endCurrTrans();

    return edp;


}



//////////////////////////////////
MSElementDescr* makePlate2(
    double dHeight,
    double dWidth,
    double dLength,
    double dThickness,
    double dOffset,
    UInt32 iLevID,
    int bBlick
) {

    DPoint3d       base;
    DPoint3d       vec;
    DVec3d       nrm;
    RotMatrix rm;
    Transform tm;
    double shell = dThickness;
    DPoint3d psh[5];
    //DPoint3d pshb[5];
    double dw = 0.;
    double dh = 0.;

    KIBODY  *kb_shape = NULL;

    MSElementDescr* edp = NULL;

    int ret;

    memset(&base, 0, sizeof(DPoint3d));
    memset(&vec, 0, sizeof(DPoint3d));
    memset(&nrm, 0, sizeof(DPoint3d));


    if (EQ(dHeight, 0.)) return NULL;
    if (EQ(dWidth, 0.)) return NULL;
    if (EQ(dLength, 0.)) return NULL;


    psh[0].z = psh[1].z = psh[2].z = psh[3].z = psh[4].z = dOffset;



    if (iJust == 1) {
        dw = 0.;
        dh = 0.;
    }
    else if (iJust == 2) {
        dw = 0.;
        dh = dHeight / 2.;
    }
    else if (iJust == 3) {
        dw = 0.;
        dh = dHeight;
    }
    else if (iJust == 4) {
        dw = dWidth / 2.;
        dh = 0.;
    }
    else if (iJust == 5) {
        dw = dWidth / 2.;
        dh = dHeight / 2.;
    }
    else if (iJust == 6) {
        dw = dWidth / 2.;
        dh = dHeight;
    }
    else if (iJust == 7) {
        dw = dWidth;
        dh = 0.;
    }
    else if (iJust == 8) {
        dw = dWidth;
        dh = dHeight / 2.;
    }
    else if (iJust == 9) {
        dw = dWidth;
        dh = dHeight;
    }


    psh[0].x = psh[4].x = dw - dWidth;
    psh[0].y = psh[4].y = dh - dHeight;

    psh[1].x = dw;
    psh[1].y = dh - dHeight;

    psh[2].x = dw;
    psh[2].y = dh;

    psh[3].x = dw - dWidth;
    psh[3].y = dh;

    /*
    psh[0].x = psh[4].x = -dWidth / 2.;
    psh[0].y = psh[4].y = -dHeight / 2.;

    psh[1].x = -dWidth / 2.;
    psh[1].y = dHeight / 2.;

    psh[2].x = dWidth / 2.;
    psh[2].y = dHeight / 2.;

    psh[3].x = dWidth / 2.;
    psh[3].y = -dHeight / 2.;


    */


    vec.z = 1.;

    /*
    if (abs(iDirection) == 1)
    {
    nrm.x = (iDirection / abs(iDirection)) * 1.;
    }
    else if (abs(iDirection) == 2)
    {
    nrm.y = (iDirection / abs(iDirection)) * 1.;
    }
    else
    {
    nrm.z = (iDirection / abs(iDirection)) * 1.;
    }
    */

    mdlRMatrix_fromNormalVector(&rm, &nrm);
    mdlRMatrix_getInverse(&rm, &rm);
    mdlTMatrix_fromRMatrix(&tm, &rm);


    mdlShape_create(&el, NULL, psh, 5, 0);
    /*
    if (bBlick)
    {
    UInt32       c = 0;
    UInt32       w = 0;
    Int32       s = 0;
    Transform tmm = tm;
    DPoint3d pvec = vec;

    memcpy(pshb, psh, sizeof(psh));

    pshb[0] = pshb[2];
    pshb[0].x -= mdlCnv_masterUnitsToUors(abs(iBlick));
    pshb[0].y -= mdlCnv_masterUnitsToUors(abs(iBlick));

    pshb[4] = pshb[0];

    mdlShape_create(&elBlick[0], NULL, pshb, 5, -1);
    mdlElement_setSymbology(&elBlick[0], &c, &w, &s);
    mdlElement_transform(&elBlick[0], &elBlick[0], &tm);

    mdlRMatrix_multiplyPoint (&pvec, &rm);
    mdlVec_scaleToLengthInPlace(&pvec, dLength);
    mdlTMatrix_setTranslation (&tmm, &pvec);

    mdlShape_create(&elBlick[1], NULL, pshb, 5, -1);
    mdlElement_setSymbology(&elBlick[1], &c, &w, &s);
    mdlElement_transform(&elBlick[1], &elBlick[1], &tmm);

    memcpy(pshb, psh, sizeof(psh));
    pshb[1] = pshb[3];
    pshb[1].x += mdlCnv_masterUnitsToUors(abs(iBlick));
    pshb[1].y -= mdlCnv_masterUnitsToUors(abs(iBlick));

    mdlShape_create(&elBlick[2], NULL, pshb, 5, -1);
    mdlElement_setSymbology(&elBlick[2], &c, &w, &s);
    mdlElement_transform(&elBlick[2], &elBlick[2], &tmm);
    }
    */
    //mdlEllipse_create (&el, NULL, &base, dOutsideRadius, dOutsideRadius, NULL, 0);
    mdlElmdscr_new(&edp, NULL, &el);
    //mdlElmdscr_appendElement (edp, &el);

    mdlKISolid_beginCurrTrans(MASTERFILE);

    mdlCurrTrans_invScaleDoubleArray(&dLength, &dLength, 1);
    mdlCurrTrans_invScaleDoubleArray(&shell, &shell, 1);

    ret = mdlKISolid_elementToBody(&kb_shape, edp, MASTERFILE);

    ret = mdlKISolid_sweepBodyVector(kb_shape, &vec, dLength, shell, 0.);

    ret = mdlKISolid_bodyToElement(&edp, kb_shape, TRUE, -1, NULL, MASTERFILE);

    if (iLevID) mdlElmdscr_setProperties(edp, &iLevID, 0, 0, 0, 0, 0, 0, 0);

    mdlElmdscr_transform(edp, &tm);

    mdlKISolid_freeBody(kb_shape);

    mdlKISolid_endCurrTrans();

    return edp;


}


//////////////////////////////////
MSElementDescr* makePlate(
    double dHeight,
    double dWidth,
    double dLength,
    double dWallWidth,
    long iThickness // if 0 then without boolian disjoint                                  
) {

    KIBODY          *cuboid = NULL;
    MSElementDescr *edP = NULL;
    Transform tm;
    DPoint3d p3d_orient;
    DPoint3d p3d_orientInner;
    DPoint3d p3d_partOffset;
    double dThickness;

    dThickness = (double)iThickness;

    /* Begin current translation */
    mdlKISolid_beginCurrTrans(MASTERFILE);

    /* Convert master units to UORs */
    mdlCnv_masterToUOR(&dHeight, dHeight, MASTERFILE);
    mdlCnv_masterToUOR(&dWidth, dWidth, MASTERFILE);
    mdlCnv_masterToUOR(&dLength, dLength, MASTERFILE);
    mdlCnv_masterToUOR(&dThickness, dThickness, MASTERFILE);
    mdlCnv_masterToUOR(&dWallWidth, dWallWidth, MASTERFILE);

    /* Convert current units to Parasolid units */
    mdlCurrTrans_invScaleDoubleArray(&dHeight, &dHeight, 1);
    mdlCurrTrans_invScaleDoubleArray(&dWidth, &dWidth, 1);
    mdlCurrTrans_invScaleDoubleArray(&dLength, &dLength, 1);
    mdlCurrTrans_invScaleDoubleArray(&dThickness, &dThickness, 1);
    mdlCurrTrans_invScaleDoubleArray(&dWallWidth, &dWallWidth, 1);


    p3d_partOffset.x = 0.;
    p3d_partOffset.y = 0.;
    p3d_partOffset.z = 0.;

    //if (abs(iDirection) == 1) 
    //{
    //    p3d_orient.x = dWidth;
    //    p3d_orient.y = dLength;
    //    p3d_orient.z = dHeight;

    //    p3d_orientInner.x = dWidth - dThickness * 2;
    //    p3d_orientInner.y = dLength;
    //    p3d_orientInner.z = dHeight - dThickness * 2;

    //    p3d_partOffset.y = dLength / 2. + dWallWidth - dLength;
    //}
    //else if (abs(iDirection) == 2) 
    //{
    //    p3d_orient.x = dLength;
    //    p3d_orient.y = dHeight;
    //    p3d_orient.z = dWidth;

    //    p3d_orientInner.x = dLength;
    //    p3d_orientInner.y = dHeight - dThickness * 2;
    //    p3d_orientInner.z = dWidth - dThickness * 2;

    //    p3d_partOffset.x = dLength / 2. + dWallWidth - dLength;
    //}
    //else
    {
        p3d_orient.x = dHeight;
        p3d_orient.y = dWidth;
        p3d_orient.z = dLength;

        p3d_orientInner.x = dHeight - dThickness * 2;
        p3d_orientInner.y = dWidth - dThickness * 2;
        p3d_orientInner.z = dLength;

        p3d_partOffset.z = dLength / 2. + dWallWidth - dLength;
    }


    /* Make our tick mark */
    mdlKISolid_makeCuboid(&cuboid,
        p3d_orient.x,
        p3d_orient.y,
        p3d_orient.z
    );



    if (iThickness > 0) {
        int ret = 0;

        KIBODY          *cuboidIn = NULL;

        KIENTITY_LIST   *list1 = NULL; /* Cutting List (holds BodyP2) */
        KIENTITY_LIST   *list2 = NULL; /* List of bodies after cut ) */

        ret = mdlKISolid_makeCuboid(&cuboidIn,
            p3d_orientInner.x,
            p3d_orientInner.y,
            p3d_orientInner.z
        );
        /* Create a blank KIENTITY_LIST */
        mdlKISolid_listCreate(&list1);
        mdlKISolid_listCreate(&list2);


        /* Add the cutting KIBODY to KIENTITY_LIST */
        mdlKISolid_listAdd(list2, cuboidIn);

        ret = mdlKISolid_booleanDisjoint(list1, cuboid, list2,
            MODELER_BOOLEAN_difference);

    }






    /* Convert the body to an element */
    mdlKISolid_bodyToElement(&edP, cuboid, TRUE, -1, NULL, MASTERFILE);


    mdlTMatrix_getIdentity(&tm);

    mdlTMatrix_setTranslation(&tm, &p3d_partOffset);

    mdlElmdscr_transform(edP, &tm);


    /* End current translation */
    mdlKISolid_endCurrTrans();
    /* Free memory */
    mdlKISolid_freeBody(cuboid);


    return edP;

}


//////////////////////////
int makeOpening(
) {


    MSElement e_Cell;
    MSElementDescr *edp1 = NULL;

    edp1 = makePlate(
        penparams[1], // высота
        penparams[3], // ширина
        penparams[5], // глубина
        0., // толщина стенки не нужна
        (long)penparams[2]  // толщина стенок проходки
    );

    mdlCell_create(&e_Cell, NULL, NULL, FALSE);
    mdlElmdscr_new(&edpPart, NULL, &e_Cell);

    // append element to cell
    if (edp1 != NULL)
        mdlElmdscr_appendDscr(edpPart, edp1);

    return SUCCESS;
}


//////////////////////////
int makeRectPenetr(
) {

    MSElement e_Cell;
    MSElementDescr *edp1 = NULL;
    MSElementDescr *edp2 = NULL;
    MSElementDescr *edp3 = NULL;

    double wgt[2];
    double hgt[2];
    double len[2];
    double thk[2];
    double ofs[3];


    //if (iFlanges == 2)
    {

        mdlCnv_masterToUOR(&hgt[0], (dRectHeight + dRectThick * 2.), MASTERFILE);
        mdlCnv_masterToUOR(&hgt[1], (dRectHeight), MASTERFILE);

        mdlCnv_masterToUOR(&wgt[0], (dRectWidth + dRectThick * 2.), MASTERFILE);
        mdlCnv_masterToUOR(&wgt[1], (dRectWidth), MASTERFILE);

        mdlCnv_masterToUOR(&len[0], dFlanThick, MASTERFILE);
        mdlCnv_masterToUOR(&len[1], (dWallThick - dFlanThick), MASTERFILE);

        mdlCnv_masterToUOR(&thk[0], dFlanWidth, MASTERFILE);
        mdlCnv_masterToUOR(&thk[1], dRectThick, MASTERFILE);

        mdlCnv_masterToUOR(&ofs[0], 0., MASTERFILE);
        mdlCnv_masterToUOR(&ofs[1], dFlanThick / 2., MASTERFILE);
        mdlCnv_masterToUOR(&ofs[2], (dWallThick - dFlanThick), MASTERFILE);

        //mdlSystem_enterDebug();

        edp1 = makePlate2(hgt[0], wgt[0], len[0], thk[0], ofs[0], lvlF, FALSE);
        edp2 = makePlate2(hgt[1], wgt[1], len[1], thk[1], ofs[1], lvlP, TRUE);
        edp3 = makePlate2(hgt[0], wgt[0], len[0], thk[0], ofs[2], lvlF, FALSE);

    }

    mdlCell_create(&e_Cell, NULL, NULL, FALSE);
    mdlElmdscr_new(&edpPart, NULL, &e_Cell);

    // append element to cell
    if (edp1 != NULL) {
        mdlElmdscr_appendDscr(edpPart, edp1);
    }


    //{
    //    DVec3d pvec;
    //    Transform tm, tmi;
    //    mdlElmdscr_extractCellTMatrix(&tm, &tmi, &edpPart->el, MASTERFILE);
    //    mdlTMatrix_getMatrixRow(&pvec, &tm, 2);
    //    printf("%f %f %f\n", pvec.x, pvec.y, pvec.z);
    //}



    if (edp2 != NULL) {
        mdlElmdscr_appendDscr(edpPart, edp2);
        if (iBlick) mdlElmdscr_appendElement(edpPart, &elBlick[0]);
        if (iBlick > 0) mdlElmdscr_appendElement(edpPart, &elBlick[1]);
        if (iBlick < 0) mdlElmdscr_appendElement(edpPart, &elBlick[2]);
    }


    if (edp3 != NULL) {
        mdlElmdscr_appendDscr(edpPart, edp3);
    }


    return SUCCESS;


}


//////////////////////////
int makePenetr(
) {

    MSElement e_Cell;
    MSElementDescr *edp1 = NULL;
    MSElementDescr *edp2 = NULL;
    MSElementDescr *edp3 = NULL;

    double dPipeDiameter;
    double dPipeThickness;
    double dFlangeThickness;
    double dFlangeWidth;

    double dPipeInsideDiameter;
    double dPipeOutsideDiameter;
    double dPipeLength;
    double dPipeOffset;

    double dFlangeInsideDiameter;
    double dFlangeOutsideDiameter;
    double dFlangeLength;
    double dFlangeOffset;



    dPipeDiameter = dPipeDiam;
    dPipeThickness = dPipeThick;
    dFlangeWidth = dFlanDiam;
    dFlangeThickness = dFlanThick;


    mdlCnv_masterToUOR(&dPipeInsideDiameter, (dPipeDiameter - dPipeThickness * 2), MASTERFILE);
    mdlCnv_masterToUOR(&dPipeOutsideDiameter, (dPipeDiameter), MASTERFILE);

    mdlCnv_masterToUOR(&dFlangeInsideDiameter, (dPipeDiameter), MASTERFILE);
    mdlCnv_masterToUOR(&dFlangeOutsideDiameter, (dFlangeWidth), MASTERFILE);


    if (iFlanges == 2) {
        mdlCnv_masterToUOR(&dPipeLength, (dWallThick - dFlangeThickness), MASTERFILE);
        mdlCnv_masterToUOR(&dPipeOffset, (dFlangeThickness / 2.), MASTERFILE);

        mdlCnv_masterToUOR(&dFlangeLength, (dFlangeThickness), MASTERFILE);
        mdlCnv_masterToUOR(&dFlangeOffset, (dWallThick - dFlangeThickness), MASTERFILE);

        edp1 = makeTube2(
            dPipeInsideDiameter / 2.,
            dPipeOutsideDiameter / 2.,
            dPipeLength,
            dPipeOffset,
            lvlP, true
        );

        edp2 = makeTube2(
            dFlangeInsideDiameter / 2.,
            dFlangeOutsideDiameter / 2.,
            dFlangeLength,
            0. - 100.,
            lvlF, false
        );

        edp3 = makeTube2(
            dFlangeInsideDiameter / 2.,
            dFlangeOutsideDiameter / 2.,
            dFlangeLength,
            dFlangeOffset + 100.,
            lvlF, false
        );
    }
    else if (iFlanges == 1) {
        mdlCnv_masterToUOR(&dPipeLength, (dWallThick - dFlangeThickness / 2.), MASTERFILE);
        mdlCnv_masterToUOR(&dPipeOffset, (dFlangeThickness / 2.), MASTERFILE);

        mdlCnv_masterToUOR(&dFlangeLength, (dFlangeThickness), MASTERFILE);
        mdlCnv_masterToUOR(&dFlangeOffset, 0., MASTERFILE);

        edp1 = makeTube2(
            dPipeInsideDiameter / 2.,
            dPipeOutsideDiameter / 2.,
            dPipeLength,
            dPipeOffset,
            lvlP, true
        );

        edp2 = makeTube2(
            dFlangeInsideDiameter / 2.,
            dFlangeOutsideDiameter / 2.,
            dFlangeLength,
            0. - 100.,
            lvlF, false
        );

    }
    else {
        mdlCnv_masterToUOR(&dPipeLength, dWallThick, MASTERFILE);
        mdlCnv_masterToUOR(&dPipeOffset, 0., MASTERFILE);

        mdlCnv_masterToUOR(&dFlangeLength, 0., MASTERFILE);
        mdlCnv_masterToUOR(&dFlangeOffset, 0., MASTERFILE);

        edp1 = makeTube2(
            dPipeInsideDiameter / 2.,
            dPipeOutsideDiameter / 2.,
            dPipeLength,
            0.,
            lvlP, true
        );
    }

    mdlCell_create(&e_Cell, NULL, NULL, FALSE);
    mdlElmdscr_new(&edpPart, NULL, &e_Cell);

    // append element to cell
    if (edp1 != NULL)
        mdlElmdscr_appendDscr(edpPart, edp1);


    if (edp2 != NULL)
        mdlElmdscr_appendDscr(edpPart, edp2);


    if (edp3 != NULL)
        mdlElmdscr_appendDscr(edpPart, edp3);


    return SUCCESS;
}



////////////////////////////////
void restartDefault() {

    if (edpPart != NULL) {
        mdlElmdscr_freeAll(&edpPart);
        edpPart = NULL;
    }

    if (edpPenPerf != NULL) {
        mdlElmdscr_freeAll(&edpPenPerf);
        edpPenPerf = NULL;
    }

    if (flagLocateSurfaces != 100) {
        //userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
        flagLocateSurfaces = 100;
    }

    mdlState_startDefaultCommand();
}

int scanIterate(
    MSElementDescr  *edDstP,
    UInt32*  fpos,
    ScanCriteria    *pScanCriteria
) {
    *fpos = mdlElmdscr_getFilePos(edDstP);

    return 1;
}

//////////////////////////////
int    drawPart(
    Dpoint3d    *pt,
    int        view
) {

    UInt32 fpos = 0;
    Dpoint3d* ptP = NULL;
    DVec3d* ptPerpP = NULL;
    Dpoint3d  ptProj;
    int a, b;

    //RotMatrix rm;
    //RotMatrix* rmP = NULL;
    /*
    ScanCriteria    *scP = NULL;
    UShort          typeMask[6];
    int status;
    ScanRange sr;


    sr.xhighlim = (int64)pt->x + 100;
    sr.yhighlim = (int64)pt->y + 100;
    sr.zhighlim = (int64)pt->z + 100;
    sr.xlowlim = (int64)pt->x - 100;
    sr.ylowlim = (int64)pt->y - 100;
    sr.zlowlim = (int64)pt->z - 100;


    memset (typeMask, 0, sizeof (typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    scP = mdlScanCriteria_create();
    status = mdlScanCriteria_setReturnType (scP, MSSCANCRIT_ITERATE_ELMDSCR, FALSE, TRUE);
    status = mdlScanCriteria_setElmDscrCallback (scP, (PFScanElemDscrCallback)scanIterate, &fpos);
    status = mdlScanCriteria_setElementTypeTest (scP, typeMask, sizeof (typeMask));
    status = mdlScanCriteria_setRangeTest(scP, &sr); // супер
    //status = mdlScanCriteria_setLocateTest(scP, FALSE);
    //status = mdlScanCriteria_setViewTest(scP, view, ACTIVEMODEL);
    status = mdlScanCriteria_setModel (scP, ACTIVEMODEL);
    status = mdlScanCriteria_scan (scP, NULL, NULL, NULL);
    status = mdlScanCriteria_free (scP);
    */




    fpos = mdlLocate_findElement(pt, view, FALSE, 0, FALSE);


    if (mdlLocate_getProjectedPoint(&ptProj, &a, &b) == SUCCESS) {
        memcpy(pt, &ptProj, sizeof(Dpoint3d));
    }




    if (fpos) {

        //printf("%u   \n", fpos);

        MSElementDescr* edP = NULL;

        if (mdlElmdscr_readToMaster(&edP, fpos, mdlLocate_getCurrModelRef(), 0, 0) != NULL) {
            TFFormRecipeList* flP = NULL;
            if (mdlTFFormRecipeList_constructFromElmdscr(&flP, edP) == BSISUCCESS) {
                //writeToLog("  mdlTFFormRecipeList_constructFromElmdscr SUCCESS", prm->fname, timer);
                TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(flP);
                int type = mdlTFFormRecipe_getType(fP);

                if (type == TF_LINEAR_FORM_ELM
                    //|| type == TF_SLAB_FORM_ELM
                    ) {
                    TFBrepList* blP;
                    TFFormRecipeLinear* linP = (TFFormRecipeLinear*)fP;

                    //TFFormRecipeLinearList* llinP = (TFFormRecipeLinearList*)flP;

                    TFFormRecipeList* pCurrFormRecipeNode = flP;

                    if (mdlTFFormRecipe_getBrepList(fP, &blP, 0, 0, 0) == BSISUCCESS) {
                        Dpoint3d pts[2];
                        Dpoint3d ptss[2];
                        //Dpoint3d perp[2];
                        double dist[2];
                        int num = 0;
                        DVec3d pnrm[2];

                        int stt[2];

                        MSElementDescr* edLftP = NULL;
                        MSElementDescr* edRgtP = NULL;

                        TFBrepFaceList* faceLftP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Left);
                        TFBrepFaceList* faceRgtP = mdlTFBrepList_getFacesByLabel(blP, FaceLabelEnum_Right);

                        if (faceLftP) {
                            mdlTFBrepFaceList_getElmdscr(&edLftP, faceLftP, 0);

                            if (edLftP) {
                                stt[0] = mdlElmdscr_extractNormal(&pnrm[0], &pts[0], edLftP, &pZ);

                                if (stt[0] == SUCCESS) {
                                    mdlVec_negateInPlace(&pnrm[0]);
                                    mdlVec_projectPointToPlane(&ptss[0], pt, &pts[0], &pnrm[0]);
                                    dist[0] = mdlVec_distance(&ptss[0], pt);
                                }

                                mdlElmdscr_freeAll(&edLftP);
                            }

                            mdlTFBrepFaceList_free(&faceLftP);
                        }

                        if (faceRgtP) {
                            mdlTFBrepFaceList_getElmdscr(&edRgtP, faceRgtP, 0);

                            if (edRgtP) {
                                stt[1] = mdlElmdscr_extractNormal(&pnrm[1], &pts[1], edRgtP, &pZ);

                                if (stt[1] == SUCCESS) {
                                    mdlVec_negateInPlace(&pnrm[1]);
                                    mdlVec_projectPointToPlane(&ptss[1], pt, &pts[1], &pnrm[1]);
                                    dist[1] = mdlVec_distance(&ptss[1], pt);
                                }

                                mdlElmdscr_freeAll(&edRgtP);
                            }

                            mdlTFBrepFaceList_free(&faceRgtP);
                        }

                        if (stt[0] == SUCCESS && stt[1] == SUCCESS) {
                            if (ptP == NULL) {
                                if (dist[0] < dist[1]) {
                                    ptP = &ptss[0];
                                    ptPerpP = &pnrm[0];
                                }
                                else {
                                    ptP = &ptss[1];
                                    ptPerpP = &pnrm[1];
                                }
                            }

                            //if (ptPerpP) printf("   %f  %f  %f      %f  %f       %f  %f  %f\n", ptPerpP->x, ptPerpP->y, ptPerpP->z, dist[0], dist[1], ptP->x, ptP->y, ptP->z);

                        }

                        mdlTFBrepList_free(&blP);


                    }

                    //int i = mdlTFFormRecipeLinear_getThickness(linP, &elemAttr.dWallWidth);
                    //if (i == SUCCESS)
                    //{
                    //    sprintf(sss, "  mdlTFFormRecipeLinear_getThickness SUCCESS = %.0f (%.0f)", elemAttr.dWallWidth, elemAttr.dWallWidth * 1e-2);
                    //    writeToLog(sss, prm->fname, timer);
                    //}
                }

                //if (type == TF_SLAB_FORM_ELM)
                //{
                //}


                mdlTFFormRecipeList_free(&flP);
            }
        }


    }


    if (ptP == NULL) ptP = pt;



    if (!icoord[0])
        coord[0] = roundExt(mdlCnv_uorsToMasterUnits(ptP->x), -1, 5.0, 0);

    if (!icoord[1])
        coord[1] = roundExt(mdlCnv_uorsToMasterUnits(ptP->y), -1, 5.0, 0);

    if (!icoord[2])
        coord[2] = roundExt(mdlCnv_uorsToMasterUnits(ptP->z), -1, 5.0, 0);

    pt->x = mdlCnv_masterUnitsToUors((double)coord[0]);
    pt->y = mdlCnv_masterUnitsToUors((double)coord[1]);
    pt->z = mdlCnv_masterUnitsToUors((double)coord[2]);

    ptP->x = mdlCnv_masterUnitsToUors((double)coord[0]);
    ptP->y = mdlCnv_masterUnitsToUors((double)coord[1]);
    ptP->z = mdlCnv_masterUnitsToUors((double)coord[2]);


    RotMatrix    rMatrix;



    if (mdlRMatrix_isOrthogonal(&pp.rm) && icoord[0] && icoord[1] && icoord[2]) {
        rMatrix = pp.rm;

        if (strcmp(pp.sType, "P3DEquipment") == 0)
            mdlRMatrix_rotate(&rMatrix, &rMatrix, 0., -fc_piover2, 0.);
        else
            mdlRMatrix_rotate(&rMatrix, &rMatrix, fc_piover2, 0., 0.);

        mdlRMatrix_getInverse(&rMatrix, &rMatrix);

        ptP->x = pp.pLoc.x;
        ptP->y = pp.pLoc.y;
        ptP->z = pp.pLoc.z;
    }
    else if (fpos && ptPerpP) {
        mdlRMatrix_fromNormalVector(&rMatrix, ptPerpP);
        mdlRMatrix_getInverse(&rMatrix, &rMatrix);
    }
    else {
        mdlRMatrix_fromView(&rMatrix, view, TRUE);
        mdlRMatrix_invert(&rMatrix, &rMatrix);
    }


    mdlTMatrix_fromRMatrix(&tmForPen, &rMatrix);
    mdlTMatrix_setTranslation(&tmForPen, ptP);
    mdlElmdscr_duplicate(&edpPartBuf, edpPart);
    mdlElmdscr_transform(edpPartBuf, &tmForPen);
    mdlDynamic_setElmDescr(edpPartBuf);
        
    syncToolbox();

    return SUCCESS;
}


/////////////////////////////
void placeEmbPlate(
    Dpoint3d    *pt,        /* => first data point */
    int        view        /* => view for same */
) {
    UInt32 wgt = 2;

    drawPart(pt, view); // tmForPen ready

    if (edpPart) {

        MSElementDescr *penToAdd = NULL;

        mdlElmdscr_duplicate(&penToAdd, edpPart);
        mdlElmdscr_setSymbology(penToAdd, 0, 0, &wgt, 0);

        mdlElmdscr_transform(penToAdd, &tmForPen);

        UInt32 fpos = mdlElmdscr_add(penToAdd);


        char buf[1000];
        //UInt32 fpos = mdlModelRef_getEof(ACTIVEMODEL);


        if (fpos && wcslen(tcb->activeCell) > 0 && strlen(pp.sCode) > 0) {
            char cellname[500];
            mdlCnv_convertUnicodeToMultibyte(tcb->activeCell, -1, cellname, 500);
            char_replace(cellname, ' ', '_');
            sprintf(buf, "mdl keyin aepsim aepsim setdgdata %u %s %s", fpos, cellname, pp.sCode);
            mdlInput_sendSynchronizedKeyin((MSCharCP)buf, 0, 0, 0);
        }


    }

    //if (flagLocateSurfaces != 100)
    //{
    //    userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    //    flagLocateSurfaces = 100;
    //}

}

/////////////////////////////
void placePenetr(
    Dpoint3d    *pt,        /* => first data point */
    int        view        /* => view for same */
) {
    //ElementID eid;
    //char     strID [10];
    //char*    args [1];

    UInt32 wgt = 2;

    drawPart(pt, view); // tmForPen ready

    if (edpPart) {

        MSElementDescr *penToAdd = NULL;
        MSElementDescr *penPerfToAdd = NULL;
        MSElementDescr *penProjToAdd[2] = { NULL,NULL };

        mdlElmdscr_duplicate(&penToAdd, edpPart);
        mdlElmdscr_setSymbology(penToAdd, 0, 0, &wgt, 0);
        //mdlElmdscr_transform (penToAdd, &tMatrix);

        if (edpPenPerf) mdlElmdscr_duplicate(&penPerfToAdd, edpPenPerf);
        //mdlElmdscr_transform (penPerfToAdd, &tMatrix);

        if (edpPenProj[0]) mdlElmdscr_duplicate(&penProjToAdd[0], edpPenProj[0]);
        if (edpPenProj[1]) mdlElmdscr_duplicate(&penProjToAdd[1], edpPenProj[1]);
        
        //=============================================

        {
            TFFrameList*      pFrameNode = NULL;
            //TFFormRecipeList* pFormNode       = NULL;
            //TFProjectionList* pProjectionNode = NULL;
            TFPerforatorList* pPerforatorNode = NULL;
            Transform tm;

            mdlTMatrix_getIdentity(&tm);

            

            if (pFrameNode = mdlTFFrameList_construct()) {
                TFFrame*       pFrame = mdlTFFrameList_getFrame(pFrameNode);

                TFWStringList* pNameNodeW = mdlTFWStringList_constructFromCharString("EmbeddedPart");

                mdlTFFrame_add3DElmdscr(pFrame, penToAdd); // p3DEd is consumed, no need to free it

                                                           //mdlTFFrame_setProjectionList (pFrame, pProjectionNode);    // pProjectionNode is consumed, no need to frre it



                if (edpPenPerf) {
                    mdlTFPerforatorList_constructFromElmdscr(&pPerforatorNode, penPerfToAdd, 0, 0., 0);
                    
                    // ez 2019-06-03 доб.
                    StatusInt sweepStat = mdlTFPerforatorList_setSweepMode(pPerforatorNode, PerforatorSweepModeEnum_Bi);

                    mdlTFFrame_setPerforatorList(pFrame, &pPerforatorNode); // pPerforatorNode is consumed, no need to free it

                    mdlTFFrame_setSenseDistance2(pFrame, dPerfLenUors + 1000.);


                    mdlElmdscr_freeAll(&penPerfToAdd);
                }

                if (penProjToAdd[0] && penProjToAdd[1]) {

                    TFProjectionList* pProjectionNode = NULL;
                    TFProjectionList* pProjectionNode2 = NULL;

                    if ((pProjectionNode = mdlTFProjectionList_construct()) &&
                        (pProjectionNode2 = mdlTFProjectionList_construct())) 
                    {
                        /// ===== доб 17/06/2019 ====
                        ///

                        Transform tm;
                        DPoint3d p;
                        mdlVec_zero(&p);
                        p.z = dPerfLenUors; //printf("%f\n", dPerfLenUors);

                        mdlTMatrix_getIdentity(&tm);
                        mdlTMatrix_rotateByAngles(&tm, &tm, fc_pi, 0., 0.);
                        mdlTMatrix_setTranslation(&tm, &p);
                        mdlTFProjectionList_transform(pProjectionNode, &tm);

                        ///
                        ///

                        mdlTFProjectionList_append(&pProjectionNode, pProjectionNode2);

                        TFProjection* pProjection = mdlTFProjectionList_getProjection(pProjectionNode);

                        mdlTFProjection_setEmbeddedElmdscr(pProjection, penProjToAdd[1], TRUE); /// ===== изм 17/06/2019 ====

                        TFProjectionList* pProjectionNode2 = mdlTFProjectionList_getNext(pProjectionNode);

                        TFProjection* pProjection2 = mdlTFProjectionList_getProjection(pProjectionNode2);

                        mdlTFProjection_setEmbeddedElmdscr(pProjection2, penProjToAdd[0], TRUE); /// ===== изм 17/06/2019 ====

                        mdlTFFrame_setProjectionList(pFrame, pProjectionNode);    // pProjectionNode is consumed, no need to frre it
                    }

                }

                //mdlTFFrame_setFormRecipeList (pFrame, pFormNode); // pFormNode is consumed, no need to free it
                //mdlTFFrame_setFormRecipeList2 (pFrame, 0);

                mdlTFFrame_setName(pFrame, mdlTFWStringList_getWString(pNameNodeW));

                mdlTFWStringList_free(&pNameNodeW);


                mdlTFFrame_transform(pFrame, &tmForPen, TRUE);

                mdlTFFrame_synchronize(pFrame);

                mdlTFFrame_setPerforatorsAreActive(pFrame, TRUE);



                //bSetProp = true;

				// ez 2019-11-19 доб.
				DgnModelRefP modelRefP = ACTIVEMODEL;
                UInt32 fpos = mdlElement_getFilePos(FILEPOS_NEXT_NEW_ELEMENT, &modelRefP);

                if (mdlTFModelRef_addFrame(ACTIVEMODEL, pFrame) == SUCCESS) {
                    mdlTFModelRef_updateAutoOpeningsByFrame(ACTIVEMODEL, pFrame, 1, 0, FramePerforationPolicyEnum_None);
                
					// TODO
					//if (fpos) {
					//	ElementRef elemRef = mdlModelRef_getElementRef(ACTIVEMODEL, fpos);
					//	std::wstring name(&pp.sName[0], &pp.sName[500]);
					//	std::wstring kks(&pp.sCode[0], &pp.sCode[500]);

					//	bool res;
					//	res = setDataGroupInstanceValue(elemRef, ACTIVEMODEL,
					//		L"EmbeddedPart", L"Embedded Part",
					//		L"EmbPart/@CatalogName", name);
					//	res = setDataGroupInstanceValue(elemRef, ACTIVEMODEL,
					//		L"EmbeddedPart", L"Embedded Part",
					//		L"EmbPart/@PartCode", kks);
					//}
				}

                mdlTFFrameList_free(&pFrameNode);

				// ez 2019-11-19 удалил:
                if (fpos && strlen(pp.sName) > 0 && strlen(pp.sCode) > 0) {
                    char buf[1000];
                    sprintf(buf, "mdl keyin aepsim aepsim setdgdata %u %s %s", fpos, pp.sName, pp.sCode);
                    mdlInput_sendSynchronizedKeyin((MSCharCP)buf, 0, 0, 0);
                }

                /*
                mdlSelect_freeAll();

                mdlSelect_addElement(fpos, ACTIVEMODEL, 0, 0);

                mdlInput_sendSynchronizedKeyin("dg add data;dx=", 0, 0, 0);

                mdlSelect_freeAll();



                ElementRef  elemRef = mdlModelRef_getElementRef(ACTIVEMODEL, fpos);

                if (elemRef)
                {



                Bentley::Building::Elements::BuildingEditElemHandle beeh(elemRef, ACTIVEMODEL);


                beeh.LoadDataGroupFromDisk();


                MSWChar wsName[500];
                MSWChar wsCode[500];
                mdlCnv_convertMultibyteToUnicode(pp.sName, -1, wsName, 500);
                mdlCnv_convertMultibyteToUnicode(pp.sCode, -1, wsCode, 500);

                CCatalogSchemaItemT*    pSchemaItemName = NULL;
                CCatalogSchemaItemT*    pSchemaItemCode = NULL;

                if (NULL != (pSchemaItemName = beeh.GetCatalogCollection().FindDataGroupSchemaItem(L"EmbPart/@CatalogName")))
                {
                if (pSchemaItemName->GetValue().empty())
                pSchemaItemName->SetValue(wsName);// to comply with ArchDoor schema: PAZ or BXF or RFA
                }

                if (NULL != (pSchemaItemCode = beeh.GetCatalogCollection().FindDataGroupSchemaItem(L"EmbPart/@PartCode")))
                {
                if (pSchemaItemCode->GetValue().empty())
                pSchemaItemCode->SetValue(wsCode);// to comply with ArchDoor schema: PAZ or BXF or RFA
                }


                beeh.Rewrite();


                }

                */






                /*

                ElementRef  elemRef  = dgnCache_findElemByFilePos(mdlModelRef_getCache(ACTIVEMODEL), fpos, TRUE);

                if (elemRef)
                {
                Bentley::Building::Elements::BuildingEditElemHandle beeh (elemRef, ACTIVEMODEL, false, false);

                if (beeh.IsValid())
                {
                //beeh.DeleteAllInstances();

                //beeh.LoadDataGroupFromDisk();


                Bentley::Building::CatalogInstance::CCatalogInstanceT* ci = new Bentley::Building::CatalogInstance::CCatalogInstanceT(L"EmbeddedPart", L"Embedded Part");

                Bentley::Building::CatalogCollection::CCatalogCollection cc = beeh.GetCatalogCollection();
                cc.InsertDataGroupCatalogInstance(*ci);
                //cc.UpdateInstanceDataDefaults (L"EmbeddedPart");



                Bentley::Building::CatalogCollection::CCatalogCollection::CCollection_iterator cci =
                beeh.GetCatalogCollection().InsertDataGroupCatalogInstance(*ci);

                beeh.

                beeh.GetCatalogCollection().UpdateInstanceDataDefaults (L"EmbeddedPart");

                CCatalogSchemaItemT*    pSchemaItem = NULL;
                if (NULL != (pSchemaItem = beeh.GetCatalogCollection().FindDataGroupSchemaItem (L"EmbPart/@CatalogName")))
                pSchemaItem->SetValue (L"AAAAAA");


                if (beeh.ReplaceInModel())
                {
                }
                }
                }
                */



                /*





                beeh.LoadDataGroupFromDisk();
                Bentley::Building::CatalogCollection::CCatalogCollection cc = beeh.GetCatalogCollection();


                cc.InsertDataGroupCatalogInstance(L"Embedded Part", L"Embedded Part");
                cc.UpdateInstanceDataDefaults (L"Embedded Part");

                if (cc.GetIsValid())
                {
                //beeh.GetCatalogCollection ().InsertDataGroupCatalogInstance (L"Embedded Part", L"Embedded Part");
                //beeh.GetCatalogCollection ().UpdateInstanceDataDefaults (L"Embedded Part");



                int res = beeh.Rewrite();
                }

                }
                //      Bentley::Building::CatalogCollection::CCatalogCollection* instance = new Bentley::Building::CatalogCollection::CCatalogCollection ();



                //Bentley::Building::XmlFragmentPropertyDeserialization  deserialize;
                //deserialize.Read (*instance, elemRef, ACTIVEMODEL, NULL, NULL);





                //beeh.LoadDataGroupData();
                //beeh.LoadDataGroupFromDisk();

                //Bentley::Building::CatalogCollection::CCatalogCollection cc = beeh.GetCatalogCollection();

                //if (cc.GetIsValid())
                //{

                //    beeh.GetCatalogCollection ().InsertDataGroupCatalogInstance (L"Embedded Part", L"Embedded Part");
                //    beeh.GetCatalogCollection ().UpdateInstanceDataDefaults (L"Embedded Part");

                //    CCatalogSchemaItemT*    pSchemaItem = NULL;
                //    if (NULL != (pSchemaItem = beeh.GetCatalogCollection ().FindDataGroupSchemaItem (L"EmbPart/@CatalogName")))
                //    pSchemaItem->SetValue (L"AAAAAA");

                //}
                //}
                }

                */






            }

        }



        //=============================================

        //mdlElmdscr_add(penToAdd);

        //mdlElmdscr_freeAll(&penToAdd);






        /*
        if (!iByMessage && (iMode == MODE_PEN || iMode == MODE_RECT))
        {
        char procname[50];

        if (iMode == MODE_PEN)
        strcpy(procname, "eventPenPlaced");
        else if (iMode == MODE_RECT)
        strcpy(procname, "eventRectPlaced");

        if (mdlVBA_runProcedure(NULL, 0, "so2", "mpen", procname, 1, args) != SUCCESS)
        mdlOutput_messageCenterW(MESSAGE_ERROR, L"Ошибка вызова процедуры VBA, теги не установлены",
        L"Ошибка вызова процедуры VBA, теги не установлены", TRUE);
        }
        */

    }


    //if (flagLocateSurfaces != 100)
    //{
    //    userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    //    flagLocateSurfaces = 100;
    //}

}

/*
///////////////////////
int getValuesFromString(
char* string,
int* values,
int count
)
{

char* s = NULL;
char* ss = NULL;
char* s0 = NULL;
char* s1 = NULL;
int i;

char iDelim;


if (*string == 0) return 0; // no value

// check and set delimeter
s1 = strchr(string, '-');
if (s1 == NULL)
{
s1 = strchr(string, 'x');
if (s1 == NULL)
return 0;
else
{
iDelim = 'x';
}
}
else
{
iDelim = '-';
}

// allocate
s0 = (char*)malloc(strlen(string)+1);
if (s0 == NULL) return 0;

// copy to new allocated
strncpy(s0, string, strlen(string)+1);

ss = s0;


for(i = 0; i < count; i++)
{
s = ss;

ss = strchr(s, iDelim);
if (ss == NULL)
{
values[i] = atoi(s);
if (i == 2) break;
break;
}
else
{
*ss = 0;
values[i] = atoi(s);
if (i == 2) break;
ss++;
}
}

free(s0);

return i+1;

}
*/

///////////////////////////
void startEmbPlaceCmd(
    char    *unparsedP
) {
    mdlAccuSnap_enableSnap(TRUE);

    flagLocateSurfaces = userPrefsP->smartGeomFlags.locateSurfaces;
    //userPrefsP->smartGeomFlags.locateSurfaces = 4; // Always locate interiors


    // unparsedP - имя cell

    MSWChar wsCellName[200];
    MSElementDescr* edP = NULL;
    int res;

    DgnFileObjP libP = NULL;

    //res = mdlCell_getLibraryObject (&libP, "FH1_EP_WELDA", 0);
    //if (res != SUCCESS) return;

    if (unparsedP && strlen(unparsedP) > 0) {
        mdlCnv_convertMultibyteToUnicode(unparsedP, -1, wsCellName, 200);
        wcscpy(tcb->activeCell, wsCellName);
    }
    else if (wcslen(tcb->activeCell) > 0) {
        wcscpy(wsCellName, tcb->activeCell);
    }
    else {
        mdlOutput_messageCenter(MESSAGE_ERROR, "Вы должны определить активное имя библиотеки", "", FALSE);
        return;
    }


    res = mdlCell_findCell(&libP, 0, wsCellName, 2);
    if (res != SUCCESS) return;

    if (edpPart != NULL) {
        mdlElmdscr_freeAll(&edpPart);
        edpPart = NULL;
    }

    RotMatrix rm;
    DVec3d p;

    p.x = 0.;
    p.y = -1.;
    p.z = 0.;

    mdlRMatrix_fromNormalVector(&rm, &p);

    res = mdlCell_getElmDscr(
        &edpPart,    //MSElementDescr      **cellDscrPP,       // <= ptr to ptr to element descr     
        NULL,    //MSElementDescr      **txtNodeDscrPP,    // <= ptr-ptr to empty txnode descr    
        NULL,    //Dpoint3d            *rOrigin,           // => origin of cell                   
        NULL,    //Dpoint3d            *scale,             // => scale factors                    
        FALSE,    //BoolInt             trueScale,          // => use cell and DGN units in scaling 
        &rm,    //RotMatrix           *rotMatrix,         // => rotation matrix for cell         
        NULL,    //short               *attributes,        // => attrb data to append to hdr      
        0,        //UInt32              ggroup,             // => graphic group number             
        0,        //int                 sharedFlag,         // => 0=no shared, 1=shared, 2=use tcb 
        TRUE,    //BoolInt             updateMasterFile,   // => If true, copy the necessary styles, etc. to display to master 
        wsCellName, //const MSWChar       *cellName,          // => name of cell                     
        libP);    //const DgnFileObjP   library             // => cell library    

    if (res != SUCCESS) return;


    if (edpPart) {
        mdlState_startPrimitive(placeEmbPlate, restartDefault, 0, 0);
        mdlElmdscr_duplicate(&edpPartBuf, edpPart);
        mdlDynamic_setElmDescr(edpPartBuf);
        mdlState_dynamicUpdate(drawPart, TRUE);
    }



}

///////////////////////////
void startPenPlaceCmd(
    char    *unparsedP
) {
    //printf("%i\n", iDirection);

    int aq = 0;

    values[0] = 0;
    values[1] = 0;
    values[2] = 0;

    mdlAccuSnap_enableSnap(TRUE);

    flagLocateSurfaces = userPrefsP->smartGeomFlags.locateSurfaces;
    //userPrefsP->smartGeomFlags.locateSurfaces = 4; // Always locate interiors

    //aq = getValuesFromString(unparsedP, values, 3);

    //mdlSystem_enterDebug();

    if (aq > 0 && aq != 3) {
        mdlOutput_messageCenter(MESSAGE_WARNING, "неверное количество аргументов", "неверное количество аргументов", FALSE);
        return;
    }

    iByMessage = 0;

    if (iMode == MODE_PEN) {
        if (aq == 0 || (aq == 3 && values[0] > 0 && values[0] < 4 && values[1] >= 0 && values[2] > 0)) {
            //===========
            makePenetr();
            //===========

            if (edpPart) {
                mdlState_startPrimitive(placePenetr, restartDefault, 0, 0);
                mdlElmdscr_duplicate(&edpPartBuf, edpPart);
                //mdlTransient_free(&msTransientElmP, true);
                //msTransientElmP = mdlTransient_addElemDescr (msTransientElmP, edpPartBuf, FALSE, 0x00ff, NORMALDRAW, 0, 0, TRUE);
                mdlDynamic_setElmDescr(edpPartBuf);
                mdlState_dynamicUpdate(drawPart, TRUE);
            }
        }
        else {
            mdlOutput_messageCenter(MESSAGE_WARNING, "неверный диапазон аргументов", "неверный диапазон аргументов", FALSE);
        }
    }
    else if (iMode == MODE_RECT) {

        //===============
        makeRectPenetr();
        //===============

        if (edpPart) {
            mdlState_startPrimitive(placePenetr, restartDefault, 0, 0);
            mdlElmdscr_duplicate(&edpPartBuf, edpPart);
            //mdlTransient_free(&msTransientElmP, true);
            //msTransientElmP = mdlTransient_addElemDescr (msTransientElmP, edpPartBuf, FALSE, 0x00ff, DRAW_MODE_Normal, 0, 0, TRUE);
            mdlDynamic_setElmDescr(edpPartBuf);
            mdlState_dynamicUpdate(drawPart, TRUE);

        }

    }
    else {
        penparams[1] = values[0];
        penparams[3] = values[1];
        penparams[5] = values[2];

        penparams[2] = 10.;

        //============
        makeOpening();
        //============

        if (edpPart) {
            mdlState_startPrimitive(placePenetr, restartDefault, 0, 0);
            mdlElmdscr_duplicate(&edpPartBuf, edpPart);
            //mdlTransient_free(&msTransientElmP, true);
            //msTransientElmP = mdlTransient_addElemDescr (msTransientElmP, edpPartBuf, FALSE, 0x00ff, NORMALDRAW, 0, 0, TRUE);
            mdlDynamic_setElmDescr(edpPartBuf);
            mdlState_dynamicUpdate(drawPart, TRUE);
        }
    }

}


////////////////////////////////////
void  callbackDgnFileChanged(
    MSElementDescr*       newDescr,
    MSElementDescr*       oldDescr,
    ChangeTrackInfo*       info,
    BoolInt*       cantBeUndoneFlag
) {

    if (newDescr && bSetProp) {
        bSetProp = false;

        ElementID elid = mdlElement_getID(&newDescr->el);
        UInt32 fp = mdlElmdscr_getFilePos(newDescr);

        if (fp) {

            MSElementDescr* edP = NULL;

            mdlElmdscr_read(&edP, fp, ACTIVEMODEL, 0, 0);

            if (edP) {
                mdlElmdscr_rewrite(edP, edP, fp);
            }

            mdlElmdscr_freeAll(&edP);
        }


        if (elid) {

            ElementRef  elemRef = dgnCache_findElemByID(mdlModelRef_getCache(ACTIVEMODEL), elid);

            if (elemRef) {
                //Bentley::Building::Elements::BuildingEditElemHandle beeh (elemRef, ACTIVEMODEL);

                //if (beeh.IsValid())
                //{


                //      Bentley::Building::CatalogCollection::CCatalogCollection* instance = new Bentley::Building::CatalogCollection::CCatalogCollection ();

                //Bentley::Building::CatalogInstance::CCatalogInstanceT* ci = new Bentley::Building::CatalogInstance::CCatalogInstanceT(L"Embedded Part", L"Embedded Part");


                //Bentley::Building::XmlFragmentPropertyDeserialization  deserialize;
                //deserialize.Read (*instance, elemRef, ACTIVEMODEL, NULL, NULL);



                //beeh.AppendDataGroupData(0, 0, (MSWChar*)ci->GetSchemaName().c_str());


                //beeh.LoadDataGroupData();
                //beeh.LoadDataGroupFromDisk();

                //Bentley::Building::CatalogCollection::CCatalogCollection cc = beeh.GetCatalogCollection();

                //if (cc.GetIsValid())
                //{

                //    beeh.GetCatalogCollection ().InsertDataGroupCatalogInstance (L"Embedded Part", L"Embedded Part");
                //    //beeh.GetCatalogCollection ().UpdateInstanceDataDefaults (L"Embedded Part");

                //    //CCatalogSchemaItemT*    pSchemaItem = NULL;
                //    //if (NULL != (pSchemaItem = beeh.GetCatalogCollection ().FindDataGroupSchemaItem (L"EmbPart/@CatalogName")))
                //    //    pSchemaItem->SetValue (L"PartName");

                //    //int res = beeh.Rewrite();
                //}
                //}
            }
        }
    }



}

///////////////////////////////////////
ElmDscrToFile_Status callbackElmdDscrToFile
(
    ElmDscrToFile_Actions       action,
    DgnModelRefP       modelRef,
    UInt32       filePos,
    MSElementDescr*       newEdP,
    MSElementDescr*       oldEdP,
    MSElementDescr**       replacementEdPP
) {
    ElmDscrToFile_Status ret = ELMDTF_STATUS_SUCCESS;


    return ret;

}

/*

///////////////////////
int    locateNotePoint(
DVec3d    *pt,
int        view)
{
UInt32 fp = 0;
DgnModelRefP mrP = NULL;
MSElementDescr* edp = NULL;

fp = mdlLocate_findElement (pt, view, 0, 0, FALSE);

mrP = mdlLocate_getCurrModelRef();

if (iACStep == 0 && fp)
{
mdlElmdscr_readToMaster(&edp, fp, mrP, 0, 0);
}

//...........


return SUCCESS;
}

/////////////////////////////////
void cmdPlaceNote(
char    *unparsedP
)
//cmdNumber   CMD_SIMPEN_NOTE
{

DialogBox* dbP;
char ls[10];

iAC = CMD_SIMPEN_NOTE;
iACStep = 0;


if (mdlSystem_getCfgVar (ls, "EMBDB_NOTE_ARROW_OPEN", 10) == SUCCESS)
iCfgVar_ArrOpen = atoi(ls);

if (mdlSystem_getCfgVar (ls, "EMBDB_NOTE_ARROW_SIZE", 10) == SUCCESS)
iCfgVar_ArrSize = atoi(ls);

if (mdlSystem_getCfgVar (ls, "EMBDB_NOTE_DOT_FILLED", 10) == SUCCESS)
iCfgVar_DotFilled = atoi(ls);

if (mdlSystem_getCfgVar (ls, "EMBDB_NOTE_DOT_SIZE", 10) == SUCCESS)
iCfgVar_DotSize = atoi(ls);


memset(pp.sSign, sizeof(pp.sSign));

mdlLocate_init ();

mdlLocate_allowLocked();

mdlState_startPrimitive(locateNotePoint, mdlState_startDefaultCommand, 0, 0);

mdlOutput_prompt("Укажите элемент или начальную точку выноски");

mdlLocate_setCursor();

}
*/


//////////////////////////////////////////////
int processInstance(Bentley::WString strInst) {

    int ret = ERROR;
    XmlDomRef pDomRef;
    XmlNodeRef pNodeRef;

    //MSWChar wNodeName[500];
    //MSWChar wNodeValue[500];
    //int numn = 500;

    if (mdlXMLDom_createFromText(&pDomRef, 0, strInst.GetMSWCharCP()) == SUCCESS) {
        //if (mdlXMLDom_validate(pDomRef) == SUCCESS)
        {
            mdlXMLDom_getRootNode(&pNodeRef, pDomRef);

            int numn = 500;
            XmlNodeListRef    pNodeListRef;
            long ntype = 0;

            //mdlXMLDomNode_getName(wNodeName, &numn, pNodeRef);
            //mdlXMLDomNode_getValue(wNodeValue, &numn, pNodeRef);
            //sprintf(s, "%S = %S", wNodeName, wNodeValue);

            if (mdlXMLDomNode_hasChildNodes(pNodeRef)) {
                if (mdlXMLDomNode_getChildNodes(&pNodeListRef, pNodeRef) == SUCCESS) {
                    mdlXMLDomNodeList_traverse(pNodeListRef, 0, userFuncNodeListIMod, (char*)0);
                    mdlXMLDomNodeList_free(pNodeListRef);
                }

                ret = SUCCESS;
            }
        }
    }

    char seps[] = "-";
    char *token;

    if (strlen(pp.sName) > 0) {
        strcpy(s, pp.sName);

        token = strtok(s, seps);
        if (token != NULL) {
            if (token[0] == 'T') token++;
            pp.iFlan = atoi(token);
        }

        token = strtok(NULL, seps);
        if (token != NULL) pp.iDiam = atoi(token);

        token = strtok(NULL, seps);
        if (token != NULL) pp.iWall = atoi(token);
    }



    return ret;

}



//////////////////////////
int scanPartsReport(
    ElementRef eref,
    FILE* f,
    ScanCriteria *scP
) {
    memset(&pp, 0, sizeof(pp));

    elemXInfo(eref, ACTIVEMODEL);

    ElementID elid = elementRef_getElemID(eref);

    char sName[500] = "";
    char sDescr[500] = "";
    char sUser[500] = "";
    char sPath[1000] = "";
    char sLocX[100] = "";
    char sLocY[100] = "";
    char sLocZ[100] = "";


    DPoint3d pMin;
    DPoint3d pMax;
    MSElementDescr* edP = NULL;
    mdlElmdscr_getByElemRef(&edP, eref, ACTIVEMODEL, FALSE, 0);
    DPoint3d p[2];
    if (edP) {
        mdlElmdscr_computeRange(&pMin, &pMax, edP, NULL);
        mdlCnv_UORToMaster(&p[0].x, pMin.x, ACTIVEMODEL);
        mdlCnv_UORToMaster(&p[0].y, pMin.y, ACTIVEMODEL);
        mdlCnv_UORToMaster(&p[0].z, pMin.z, ACTIVEMODEL);
        mdlCnv_UORToMaster(&p[1].x, pMax.x, ACTIVEMODEL);
        mdlCnv_UORToMaster(&p[1].y, pMax.y, ACTIVEMODEL);
        mdlCnv_UORToMaster(&p[1].z, pMax.z, ACTIVEMODEL);
        sprintf(sLocX, "%.3f", (p[0].x + p[1].x) / 2.);
        sprintf(sLocY, "%.3f", (p[0].y + p[1].y) / 2.);
        sprintf(sLocZ, "%.3f", (p[0].z + p[1].z) / 2.);
    }
    else {
        strcpy(sLocX, "error");
        strcpy(sLocY, "error");
        strcpy(sLocZ, "error");
    }





    //[73] = {sGroup=0x380197c4 "SP3DReview.04.02" sName=0x380199b8 "SP3D_UserLastModified" sValue=0x38019bac "SP\IBGankin" }
    //[74] = {sGroup=0x38019da0 "SP3DReview.04.02" sName=0x38019f94 "SP3D_SystemPath" sValue=0x3801a188 "HnhNPP\Task\10UJE\10UJE_Embadded plates\10UJE99" }


    //UInt64 i;
    //char sdt[100];
    //if (sscanf(pp.sDate, "%I64u", &i) == 1) 
    //{
    //    struct tm * timeinfo;
    //    i = i - 621355968000000000;
    //    time_t idt = i;
    //    timeinfo = gmtime(&idt);
    //    strftime(sdt,sizeof(sdt),"%d-%m-%Y %H:%M:%S",timeinfo);
    //}




    if (strstr(sRepExcl, pp.sType) == 0) {
        sprintf(sss, "%I64u;%s;%s;%s;%s,%s,%s;%s;%s",
            elid,
            pp.sType,
            pp.sName,
            pp.sCode,
            sLocX,
            sLocY,
            sLocZ,
            pp.sUser,
            //sdt,
            pp.sPath
        );

        mdlTextFile_putString(sss, f, TEXTFILE_DEFAULT);
    }






    return 0;
}


//////////////////////////
int  PartsReport(char* unparsedP) {

    ScanCriteria    *scP = NULL;
    int status;

    UShort          typeMask[6];
    memset(typeMask, 0, sizeof(typeMask));
    typeMask[0] = TMSK0_CELL_HEADER;

    char curfile[300];
    mdlModelRef_getFileName(ACTIVEMODEL, curfile, 300);
    strcat(curfile, ".parts.csv");


    if (mdlSystem_getCfgVar(sRepExcl, "SIMDGC_REPORT_EXCLUDE", 5000) != SUCCESS)
        memset(sRepExcl, 0, sizeof(sRepExcl));





    FILE* f = mdlTextFile_open(curfile, TEXTFILE_WRITE);

    if (f == NULL) return ERROR;

    scP = mdlScanCriteria_create();
    status = mdlScanCriteria_setReturnType(scP, MSSCANCRIT_ITERATE_ELMREF, FALSE, TRUE);
    status = mdlScanCriteria_setElemRefCallback(scP, (PFScanElemRefCallback)scanPartsReport, f);
    status = mdlScanCriteria_setElementTypeTest(scP, typeMask, sizeof(typeMask));
    //status = mdlScanCriteria_setDrawnElements(scP); // м.б. не только селл
    status = mdlScanCriteria_setModel(scP, ACTIVEMODEL);
    status = mdlScanCriteria_scan(scP, NULL, NULL, NULL);
    status = mdlScanCriteria_free(scP);

    mdlTextFile_close(f);

    sprintf(sss, "%%excel \"%s\"", curfile);
    //mdlCnv_convertMultibyteToUnicode(sss, 5000, ws, 5000);

    MSCharCP wscp = (MSCharCP)sss;

    mdlInput_sendSynchronizedKeyin(wscp, 0, 0, 0);

    return SUCCESS;

}

///////////////////////////////////////
LocateFilterStatus  callbackLocateFilter(
    LOCATE_Action       action,
    MSElement*       pElement,
    DgnModelRefP       modelRef,
    UInt32       filePos,
    DVec3d*       pPoint,
    int       viewNumber,
    HitPathP       hitPath,
    char*       rejectReason
)
{

    //GLOBAL_LOCATE_IDENTIFY       = 1 ,  
    //GLOBAL_LOCATE_SELECTIONSET       = 2 ,  
    //GLOBAL_LOCATE_FENCE       = 3 ,  
    //GLOBAL_LOCATE_FENCECLIP       = 4 ,  
    //GLOBAL_LOCATE_SNAP       = 5 ,  
    //GLOBAL_LOCATE_AUTOLOCATE       = 6 ,  


    LocateFilterStatus ret = LOCATE_FILTER_STATUS_Neutral;

    UInt32 cnum;


    if (strlen(sLocFilter) == 0) return LOCATE_FILTER_STATUS_Neutral;

    mdlInput_commandState1(NULL, &cnum, NULL, NULL);


    if (cnum != CMD_SIMPEN_TASK) return LOCATE_FILTER_STATUS_Neutral;


    MSElementDescr* edp = NULL;

    mdlElmdscr_read(&edp, filePos, modelRef, FALSE, 0);



    if (edp) {
        memset(&pp, 0, sizeof(pp));

        elemXInfo(edp->h.elementRef, modelRef);

        if (strstr(sLocFilter, pp.sType) == 0) // не нашел
        {
            ret = LOCATE_FILTER_STATUS_Reject; // игнор
        }

        mdlElmdscr_freeAll(&edp);


    }

    return ret;

}
