#include "RectPenLocate.h"
#include "RectPen.h"
#include "RectPenDraw.h"
#include "Opening.h"


#include <stdio.h>
#include <string.h>

#include <WString.h>
#include <malloc.h>
#include <mselemen.fdf>
#include <mselmdsc.fdf>
#include <mslinkge.fdf>
#include <msscancrit.fdf>
#include <mstagdat.fdf>
#include <mselems.h>
#include <mscell.fdf>
#include <leveltable.fdf>
#include <mslstyle.fdf>
#include <msstrlst.h>
#include <mscnv.fdf>
#include <msdgnobj.fdf>
#include <msmodel.fdf>
#include <msview.fdf>
#include <msviewinfo.fdf>
#include <msvar.fdf>
#include <dlmsys.fdf>
#include <msdialog.fdf>

#include <msrmgr.h>
#include <mssystem.fdf>
#include <msparse.fdf>
#include <RefCounted.h>
#include <toolsubs.h>

#include <elementref.h>
#include <msdependency.fdf>
#include <msassoc.fdf>
#include <msmisc.fdf>
#include <mslocate.fdf>
#include <msstate.fdf>
#include <msoutput.fdf>
#include <mstypes.h>
#include <mstmatrx.fdf>

#include <tfform.h>
#include <mdltfform.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <msvec.fdf>
#include <tfpoly.h>
#include <mstrnsnt.fdf>
#include <mdlxmltools.fdf>

#include <msvba.fdf>

using Bentley::Ustn::Element::EditElemHandle;
using namespace Openings;

USING_NAMESPACE_BENTLEY_XMLINSTANCEAPI_NATIVE;

RectPenLocate* RectPenLocate::instanceP = NULL;


void cmdLocateRect(char *unparsedP)
{
    //mdlLocate_normal();
    //mdlState_startModifyCommand(
    //    cmdLocateRect, locateTaskPointAccept, 0,
    //    locateTaskPointShow, 0, 0, 0, FALSE, 0
    //);
    //mdlLocate_setFunction(LOCATE_POSTLOCATE, singlelocate_ElmFilter);
    //mdlLocate_init();
    ////mdlLocate_allowLocked();  

    isByContour = false;

    rectTask = RectPenTask::getEmpty();

    //rectHeight =
    //rectWidth =
    //rectDepth =
    //rectThickness =
    //rectFlanHeight =
    //rectFlanThickness = 0.;

    memset(rectKKS, 0, sizeof(rectKKS));
    memset(rectDescription, 0, sizeof(rectDescription));
    
    RectPenLocate::instanceP = new RectPenLocate();
    RectPenLocate::instanceP->InstallTool();
}

void constructByTask() {
    rectPen = RectPen(rectTask);
    if (rectPen.isValid()) {

        rectPen.addToModel();

        cmdLocateRect(NULL);
    }
    rectPen = RectPen();
}


RectPenLocate::RectPenLocate()
    : MstnElementSetTool()
{
    rectPen = RectPen();
}

/*---------------------------------------------------------------------------------**//**
* @bsimethod
+---------------+---------------+---------------+---------------+---------------+------*/
bool RectPenLocate::OnPostLocate(HitPathCP path, char *cantAcceptReason) {
    if (!__super::OnPostLocate(path, cantAcceptReason))
        return false;

    //mdlDisplayPath_setCursorElemIndex((DisplayPathP)path, 1);

    // Only allow elements of our type to be located...
    EditElemHandle eh(mdlDisplayPath_getElem((DisplayPathP)path, 0),
        mdlDisplayPath_getPathRoot((DisplayPathP)path));

    // TODO добавить проверку по свойству 'Catalog part number'

    if (eh.GetElementType() == CELL_HEADER_ELM) {
        return true;
    }
    return false;
}

EditElemHandleP RectPenLocate::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) {

    DisplayPathP dispPathP = (DisplayPathP)path;

    // чтобы в OnElementModify передать cell-элемент:
    mdlDisplayPath_setCursorElemIndex(dispPathP, 0);

    // Here we have both the new agenda entry and the current hit path if needed...
    EditElemHandleP elHandle = __super::BuildLocateAgenda(path, ev);


    if (elHandle != NULL) {
        TFFormRecipeList* flP = NULL;
        if (BSISUCCESS == mdlTFFormRecipeList_constructFromElmdscr(&flP,
            elHandle->GetElemDescrP())) {
            //writeToLog("  mdlTFFormRecipeList_constructFromElmdscr SUCCESS", prm->fname, timer);
            TFFormRecipe* fP = mdlTFFormRecipeList_getFormRecipe(flP);
            int type = mdlTFFormRecipe_getType(fP);

            UInt32 pos = mdlElmdscr_getFilePos(elHandle->GetElemDescrCP());
        }
    }

    DgnModelRefP cellModelP = elHandle->GetModelRef();

    MSElementDescr* edP = NULL;
    //mdlElmdscr_duplicate(&edP, elHandle->GetElemDescrP());

    edP = elHandle->GetElemDescrP();

    if (mdlModelRef_isReference(cellModelP)) {
        // если элемент в рефе, то трансформируем его под активную модель
        // т.к. его гометрические характеристики возвращаются
        // относительно собственной системы координат
        double scale;
        mdlModelRef_getUorScaleBetweenModels(&scale, cellModelP, ACTIVEMODEL);
        scale += 0.001;

        Transform tran;
        mdlTMatrix_referenceToMaster(&tran, cellModelP);
        mdlElmdscr_transform(edP, &tran);
    }
    
    // TODO ! НУЖНА ПРОВЕРКА НА ТО, ЧТО ЭЛЕМЕНТ - ЭТО ЗАДАНИЕ НА ПРОХОДКУ

    // обработка объекта-задания из SPF:
    {
        MSElementDescrP recEdP = edP;
        while (recEdP->el.ehdr.type == CELL_HEADER_ELM) {
            if (recEdP->h.firstElem->el.ehdr.type != CELL_HEADER_ELM) { 
                // если первый дочерний уже не цел, то начинаем проверку

                // бывают задания, в которых присутствует лишняя сфера-поверхность,
                // за счёт которой переопределяется интересующий проектировнщика
                // диапазон задания

                DPoint3d headBounds[8];
                mdlCell_extract(NULL, headBounds, NULL, NULL, NULL, 0, &edP->el);

                MSElementDescrP curEdP = recEdP;
                while (curEdP != NULL && curEdP->el.ehdr.type == CELL_HEADER_ELM) {
                    DPoint3d bounds[8];
                    mdlCell_extract(NULL, bounds, NULL, NULL, NULL, 0, &curEdP->el);

                    int matchCount = 0;
                    for (int i = 0; i < 8; ++i) {
                        for (int j = 0; j < 8; ++j) {
                            if (mdlVec_equalTolerance(&bounds[i], &headBounds[j], 0.1)) {
                                ++matchCount;
                            }
                        }
                    }
                    if (matchCount >= 4) {
                        edP = curEdP;
                        break;
                    }
                    curEdP = curEdP->h.next;
                }
            }
            recEdP = recEdP->h.firstElem;
        }
    }

    RotMatrix rot;
    DPoint3d scale;
    if (SUCCESS == mdlCell_extract(&rectTask.origin, rectTask.bounds, &rot,
        &scale, NULL, 0, &edP->el))
    {
        bool bStatus;

        XmlInstanceStatus stt;
        stt.status = LICENSE_FAILED;
        XmlInstanceSchemaManager mgrR(elHandle->GetModelRef());
        mgrR.ReadSchemas(bStatus);

        XmlInstanceApi xapiR = XmlInstanceApi::CreateApi(stt, mgrR);
        StringListHandle slhR = xapiR.ReadInstances(stt, elHandle->GetElemRef());


        int res = 0;
        for (int i = 0; i < slhR.GetCount(); i++)
        {
            Bentley::WString strInst = slhR.GetString(i);

            XmlNodeRef node = NULL;
            if (findNodeFromInstance(&node, strInst, L"P3DEquipment"))
            {
                MSWChar value[50];
                int maxchars = 50;
                XmlNodeRef child = NULL;

                if (findChildNode(&child, node, L"Name") &&
                    getNodeValue(value, &maxchars, child))
                {
                    mdlCnv_convertUnicodeToMultibyte(value, -1, rectKKS, 200);                    
                }
                if (findChildNode(&child, node, L"Description") &&
                    getNodeValue(value, &maxchars, child)) 
                {
                    mdlCnv_convertUnicodeToMultibyte(value, -1, rectDescription, 200);
                }
                mdlXMLDomNode_free(node);
            }            
        }

        rectTask.isValid = true;
        rectPen = RectPen(rectTask);
        
        if (rectPen.isValid()) {
            mdlInput_sendSynchronizedKeyin("mdl keyin simpen.ui simpen.ui readData", 0, 0, 0);
            mdlInput_sendSynchronizedKeyin("mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);
        }
    }

    // mdlElmdscr_freeAll(&edP);

    return  elHandle;
}

bool getNodeValue(MSWChar* value, int* pMaxChars, XmlNodeRef node) {

    XmlNodeListRef nodeListRef;
    mdlXMLDomNode_getChildNodes(&nodeListRef, node);
        
    int numChildren = mdlXMLDomNodeList_getNumChildren(nodeListRef);
    bool res = false;
    for (int i = 0; i < numChildren; ++i) {
        XmlNodeRef child = NULL;
        mdlXMLDomNodeList_getChild(&child, nodeListRef, i);

        long type;
        mdlXMLDomNode_getNodeType(&type, child);

        if (type == 3) {
            mdlXMLDomNode_getValue(value, pMaxChars, child);
            res = true;
        }
        mdlXMLDomNode_free(child);
    }
    return res;
}


bool findChildNode(XmlNodeRef* childP, XmlNodeRef node, const MSWChar* childName) {

    if (node == NULL)
        return false;

    bool res = false;

    if (mdlXMLDomNode_hasChildNodes(node)) {
        XmlNodeListRef nodeListRef;
        mdlXMLDomNode_getChildNodes(&nodeListRef, node);

        XmlNodeRef child;
        int numChildren = mdlXMLDomNodeList_getNumChildren(nodeListRef);

        for (int i = 0; i < numChildren; ++i) {
            mdlXMLDomNodeList_getChild(&child, nodeListRef, i);

            int numn = 50;
            MSWChar wNodeName[50];
            mdlXMLDomNode_getName(wNodeName, &numn, child);

            if (wcscmp(wNodeName, childName) == 0) {
                mdlXMLDomNode_cloneNode(childP, child, true);
                res = true;
            }
            mdlXMLDomNode_free(child);
            if (res)
                break;
        }
        mdlXMLDomNodeList_free(nodeListRef);
    }
    return res;
}


bool findNodeFromInstance(XmlNodeRef* node, Bentley::WString strInst, const MSWChar* nodeName)
{
    XmlDomRef pDomRef;
    XmlNodeRef pNodeRef;
    if (SUCCESS != mdlXMLDom_createFromText(&pDomRef, 0, strInst.GetMSWCharCP())) 
        return false;    

    mdlXMLDom_getRootNode(&pNodeRef, pDomRef);    

    bool res = findChildNode(node, pNodeRef, nodeName);
        
    mdlXMLDom_free(pDomRef);
    return res;
}


int testInstance(Bentley::WString strInst) 
{
    char sValue[50000];
    mdlCnv_convertUnicodeToMultibyte(strInst.GetMSWCharCP(), -1, sValue, 200);

    mdlOutput_messageCenterW(MESSAGE_WARNING, strInst.GetMSWCharCP(), strInst.GetMSWCharCP(), true);

    return 0;
}

/*
<P3DEquipment instanceID="DGNEC::15de0c0000::ECXA::1" xmlns="SP3DReview.04.02">
    <ConstructionStatus>2</ConstructionStatus>
    <ConstructionStatus2>2</ConstructionStatus2>
    <Dry_Installed_Weight>0</Dry_Installed_Weight>
    <Wet_Operating_Weight>0</Wet_Operating_Weight>
    <Global_Dry_Installed_CoG_X>48.25</Global_Dry_Installed_CoG_X>
    <Global_Dry_Installed_CoG_Y>67.4</Global_Dry_Installed_CoG_Y>
    <Global_Dry_Installed_CoG_Z>-8.8</Global_Dry_Installed_CoG_Z>
    <Global_Wet_Operating_CoG_X>48.25</Global_Wet_Operating_CoG_X>
    <Global_Wet_Operating_CoG_Y>67.4</Global_Wet_Operating_CoG_Y>
    <Global_Wet_Operating_CoG_Z>-8.8</Global_Wet_Operating_CoG_Z>
    <EqType0>2780</EqType0>
    <FabricationType>7</FabricationType>
    <FabricationRequirement>10</FabricationRequirement>
    <LocationX>48.6</LocationX>
    <LocationY>67.8</LocationY>
    <LocationZ>-8.8</LocationZ>
    <MTO_ReportingRequirements>5</MTO_ReportingRequirements>
    <MTO_ReportingType>5</MTO_ReportingType>
    <SP3D_DateCreated>636717472120000000</SP3D_DateCreated>
    <SP3D_DateLastModified>636717472120000000</SP3D_DateLastModified>
    <SP3D_ApprovalStatus>1</SP3D_ApprovalStatus>
    <SP3D_ApprovalReason>1</SP3D_ApprovalReason>
    <SP3D_PermissionGroup>128</SP3D_PermissionGroup>
    <OrientationMatrix_x0>1</OrientationMatrix_x0>
    <OrientationMatrix_x1>1.68051667607498E-18</OrientationMatrix_x1>
    <OrientationMatrix_x2>0</OrientationMatrix_x2>
    <OrientationMatrix_y0>0</OrientationMatrix_y0>
    <OrientationMatrix_y1>7.09223972875472E-17</OrientationMatrix_y1>
    <OrientationMatrix_y2>-1</OrientationMatrix_y2>
    <OrientationMatrix_z0>0</OrientationMatrix_z0>
    <OrientationMatrix_z1>1</OrientationMatrix_z1>
    <OrientationMatrix_z2>6.12323399573677E-17</OrientationMatrix_z2>
    <Range1X>48.2490005493164</Range1X>
    <Range1Y>67.3990020751953</Range1Y>
    <Range1Z>-8.80099964141846</Range1Z>
    <Range2X>48.951000213623</Range2X>
    <Range2Y>67.8010025024414</Range2Y>
    <Range2Z>-8.59899997711182</Range2Z>
    <Oid>00004e2e-0000-0000-481f-217a8f5b8c04</Oid>
    <UID> @a=0027!!20014##327737544678645576</UID>
    <Name>ElectricalParPenetration-1-4175</Name>
    <CatalogPartNumber>ElectricalParPenetration</CatalogPartNumber>
    <ShortMaterialDescription>ГЉГ ГЎГҐГ«ГјГ­Г Гї ГЇГ°Г®ГµГ®Г¤ГЄГ  Г®ГЈГ­ГҐГ§Г Г№ГЁГІГ­Г Гї / Fireproofing electrical penetration</ShortMaterialDescription>
    <SP3D_SystemPath>HnhNPP\Task\02_Penetration Area\10UKA\Electrical\10UKA99</SP3D_SystemPath>
    <SP3D_UserCreated>SP\EATokareva</SP3D_UserCreated>
    <SP3D_UserLastModified>SP\EATokareva</SP3D_UserLastModified>
</P3DEquipment>

*/


void RectPenLocate::OnComplexDynamics(MstnButtonEventCP ev) {
    //RedrawElems redrawTool;
    //redrawTool.SetDrawMode(DRAW_MODE_TempDraw);
    //redrawTool.SetDrawPurpose(DRAW_PURPOSE_Dynamics);
    //redrawTool.SetViews(0xffff);

    // TODO не удаётся отобразить в динамике объект проходки,
    // поэтому размещаём как transient объект

    mdlTransient_free(&msTransientElmP, true);

    rectPen = RectPen(rectTask);
    
    //if (!rectPen.isValid())
    //    return;

    EditElemHandle shapebody;
    EditElemHandle shapePerf;
    EditElemHandle crossFirst, crossSecond;

    double dist;
    bool res = rectPen.getDataByPointAndVector(rectTask.origin, rectTask.direction,
        shapebody, shapePerf, crossFirst, crossSecond, &dist);

    if (res) {
        msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
            shapebody.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
        msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
            crossFirst.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
        msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
            crossSecond.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    }
    
    //if (rectPen.createPenetr(pen, crossFirst, crossSecond))
    //{       
    //    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
    //        pen.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    //    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
    //        crossFirst.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    //    msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
    //        crossSecond.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    //}
}

//bool RectPenLocate::OnDataButton(MstnButtonEventCP ev) {
//    DisplayPathP dispPathP = mdlLocate_getCurrPath();
//    BuildLocateAgenda((HitPathP)dispPathP, ev);
//
//    return rectPen.addToModel();
//}

StatusInt RectPenLocate::OnElementModify(EditElemHandleR eeh) {
    if (OpeningTask::getInstance().isReadyToPublish) {
        rectPen.addToModel();
        return SUCCESS;
    }
    return ERROR;
}

void RectPenLocate::OnRestartCommand() {
    cmdLocateRect(NULL);
}

bool RectPenLocate::NeedAcceptPoint() {
    return true;
}

bool RectPenLocate::WantAccuSnap() 
{ 
    return false; 
}
bool RectPenLocate::NeedPointForDynamics() 
{ 
    return false; 
}


void RectPenLocate::OnPostInstall() {
    __super::OnPostInstall();
    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlOutput_prompt("Выбирите объект-задание, которое задаст параметры проёма");

    mdlSelect_freeAll();

    //flagLocateSurfaces = userPrefsP->smartGeomFlags.locateSurfaces;
    //userPrefsP->smartGeomFlags.locateSurfaces = 4;
}

bool RectPenLocate::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);      // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    //mdlState_startModifyCommand()

    return true;
}

void RectPenLocate::OnCleanup() {
    //userPrefsP->smartGeomFlags.locateSurfaces = flagLocateSurfaces;
    instanceP = NULL;
    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}