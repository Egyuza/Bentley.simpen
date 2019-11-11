#include "simpencmd.h"
#include "Opening.h"
#include "OpeningByTaskTool.h"
#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "XmlAttributeHelper.h"
#include "CExpression.h"

#include <elementref.h>
#include <IModel\xmlinstanceapi.h>
#include <IModel\xmlinstanceidcache.h>
#include <IModel\xmlinstanceschemamanager.h>
#include <tfpoly.h>

#include <mdllib.fdf>
#include <mdltfbrep.fdf>
#include <mdltfbrepf.fdf>
#include <mdltfelmdscr.fdf>
#include <mdltfform.fdf>
#include <mdlxmltools.fdf>
#include <mscell.fdf>
#include <mscnv.fdf>
#include <msdgnmodelref.fdf>
#include <msdialog.fdf>
#include <mselmdsc.fdf>
#include <mslocate.fdf>
#include <msmisc.fdf>
#include <msoutput.fdf>
#include <msselect.fdf>
#include <msstate.fdf>
#include <mstmatrx.fdf>
#include <mstrnsnt.fdf>
#include <msvar.fdf>
#include <msvec.fdf>
#include <msview.fdf>
#include <mswindow.fdf>


namespace Openings
{

USING_NAMESPACE_BENTLEY_XMLINSTANCEAPI_NATIVE;

OpeningByTaskTool* OpeningByTaskTool::instanceP = NULL;
OpeningTask OpeningByTaskTool::prevTask = OpeningTask();

void OpeningByTaskTool::runTool(char *unparsedP)
{
    mdlSelect_freeAll();
    OpeningByTaskTool::instanceP = new OpeningByTaskTool();
    OpeningByTaskTool::instanceP->InstallTool();
}

void OpeningByTaskTool::updatePreview(char *unparsedP)
{
    OpeningByTaskTool* tool = OpeningByTaskTool::instanceP;

    if (tool != NULL &&
        OpeningByTaskTool::prevTask == OpeningTask::getInstance())
    {
        return; // параметры построения не изменились
    }
    OpeningByTaskTool::prevTask = OpeningTask::getInstance();

    mdlTransient_free(&msTransientElmP, true);

    if (tool == NULL) {
        return; // НВС
    }

    if (!tool->isValid) {
        // параметры проёма не определены
        return;
    }


    DPoint3d contourPts[4];
    { // восстановление контура, т.к. пользователь мог изменить его параметры
        mdlVec_normalize(&tool->heightVec);
        mdlVec_normalize(&tool->widthVec);
        mdlVec_normalize(&tool->depthVec);

        double height = CExpr::convertToUOR(OpeningTask::getInstance().height);
        double width = CExpr::convertToUOR(OpeningTask::getInstance().width);
        double depth = CExpr::convertToUOR(OpeningTask::getInstance().depth);

        double halfHeight = height / 2;
        double halfWidth = width / 2;

        //DVec3d heightVecNeg, widthVecNeg;
        //mdlVec_negate(&heightVecNeg, &tool->heightVec);
        //mdlVec_negate(&heightVecNeg, &tool->widthVec);

        DPoint3d lhs, rhs;
        mdlVec_projectPoint(&lhs, &tool->contourOrigin, &tool->heightVec, halfHeight);
        mdlVec_projectPoint(&rhs, &tool->contourOrigin, &tool->heightVec, -halfHeight);

        mdlVec_projectPoint(&contourPts[0], &lhs, &tool->widthVec, halfWidth);
        mdlVec_projectPoint(&contourPts[1], &rhs, &tool->widthVec, halfWidth);
        mdlVec_projectPoint(&contourPts[2], &rhs, &tool->widthVec, -halfWidth);
        mdlVec_projectPoint(&contourPts[3], &lhs, &tool->widthVec, -halfWidth);
    }

    EditElemHandle contour;
    if (createShape(contour, contourPts, 4, false)) {
        msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
            contour.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    }
    
    Opening::instance = Opening(contour.GetElemDescrP());
    // Opening::instance.setDistance(OpeningTask::getInstance().depth);


    //if (!Opening::instance.isValid()) {
    //    mdlOutput_prompt("Элемент типа <Shape> (контур построения) не определён");
    //    return;
    //}

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
    if (Opening::instance.isValid() && OpeningByTaskTool::instanceP && 
        Opening::instance.getTask().isReadyToPublish) 
    {
        computeAndAddToModel(Opening::instance);
        runTool(NULL);
    }
}

// Фильтр элементов, которые будут доступны пользователю для выбора
bool OpeningByTaskTool::OnPostLocate(HitPathCP path, char *cantAcceptReason) {
    if (!__super::OnPostLocate(path, cantAcceptReason)) {
        return false;
    }

    ElementRef elRef = mdlDisplayPath_getElem((DisplayPathP)path, 0);
    EditElemHandle eeh = EditElemHandle(elRef, 
        mdlDisplayPath_getPathRoot((DisplayPathP)path));    

    // 1-ая проверка на CELL
    if (eeh.GetElementType() != CELL_HEADER_ELM) {
        return false;
    }
    
    { // 2-ая проверка по атрибуту <P3DEquipment>
        bool status;
        XmlInstanceSchemaManager mgrR(eeh.GetModelRef());
        mgrR.ReadSchemas(status);
        if (!status) {
            return false;
        }

        XmlInstanceStatus stt;
        stt.status = LICENSE_FAILED;
        XmlInstanceApi xapiR = XmlInstanceApi::CreateApi(stt, mgrR);
        StringListHandle slhR = xapiR.ReadInstances(stt, eeh.GetElemRef());

        int res = 0;
        for (int i = 0; i < slhR.GetCount(); i++)
        {
            Bentley::WString strInst = slhR.GetString(i);
                   
            XmlNodeRef node = NULL;
            if (XmlAttributeHelper::findNodeFromInstance(
                &node, strInst, L"P3DEquipment"))
            {
                return true;
            }
        }
    }

    return false;
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

EditElemHandleP
OpeningByTaskTool::BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev)
{
    isValid = false;
    OpeningTask::getInstance().tfFormRef = NULL;

    EditElemHandleP eehP = __super::BuildLocateAgenda(path, ev);

    if (!eehP->IsValid()) { // НВС
        return eehP;
    }

    OpeningTask::getInstance().isTaskSelected = true;

    { // Считывание параметров задания:
        DgnModelRefP cellModelP = eehP->GetModelRef();
        if (mdlModelRef_isReference(cellModelP)) {
            // если элемент в рефе, то трансформируем его под активную модель
            // т.к. его гометрические характеристики возвращаются
            // относительно собственной системы координат
            double scale;
            mdlModelRef_getUorScaleBetweenModels(&scale, cellModelP, ACTIVEMODEL);
            scale += 0.001;

            Transform tran;
            mdlTMatrix_referenceToMaster(&tran, cellModelP);
            mdlElmdscr_transform(eehP->GetElemDescrP(), &tran);
        }
        mdlCell_extract(&taskOrigin, taskBounds, NULL, NULL, NULL, 0, eehP->GetElementP());
    }

    ElementRef& formRef = OpeningTask::getInstance().tfFormRef;

    formRef = findIntersectedTFFormWithElement(eehP->GetElementP(),
        2, TF_LINEAR_FORM_ELM, TF_SLAB_FORM_ELM);

    if (formRef == NULL) {
        mdlOutput_messageCenter(MESSAGE_WARNING, 
            "Не найден объект <Wall> или <Slab>, внутри которого должен находиться объект тех. задания", "", FALSE);
        return eehP;
    }

    { // Определение плоскостей стены/плиты:
        EditElemHandle formEeh = EditElemHandle(formRef, ACTIVEMODEL);

        StatusInt status = SUCCESS;                
        int formType = mdlTFElmdscr_getApplicationType(formEeh.GetElemDescrP());
        if (formType == TF_LINEAR_FORM_ELM) {
            status = getFacePlaneByLabel(planeFirst,
                formEeh.GetElemDescrP(), FaceLabelEnum_Left);
            status += getFacePlaneByLabel(planeSecond,
                formEeh.GetElemDescrP(), FaceLabelEnum_Right);
        }
        else if (formType == TF_SLAB_FORM_ELM) {
            status = getFacePlaneByLabel(planeFirst,
                formEeh.GetElemDescrP(), FaceLabelEnum_Top);
            status += getFacePlaneByLabel(planeSecond,
                formEeh.GetElemDescrP(), FaceLabelEnum_Base);
        }
        else {
            return eehP;
        }

        { // корректировка плоскостей относительно пользователя
            DPlane3d buff;
            DPoint3d projOrigin;
            mdlVec_projectPointToPlane(&projOrigin, &planeFirst.origin, 
                &planeSecond.origin, &planeSecond.normal);

            if (formType == TF_LINEAR_FORM_ELM) {
                if ((int)projOrigin.y < (int)planeFirst.origin.y ||
                    (int)projOrigin.x >(int)planeFirst.origin.x)
                {
                    buff = planeFirst;
                    planeFirst = planeSecond;
                    planeSecond = buff;
                }
            }
            else if (formType == TF_SLAB_FORM_ELM) {
                if ((int)projOrigin.z > (int)planeFirst.origin.z) {
                    buff = planeFirst;
                    planeFirst = planeSecond;
                    planeSecond = buff;
                }
            }
        }

        if (status != SUCCESS) {
            mdlOutput_messageCenter(MESSAGE_WARNING,
                "Не удаётся получить проекцию тех. задания на найденный объект Wall/Slab", "", FALSE);
            formRef = NULL; // сброс
            return eehP;
        }
    }        
    
    { // Определение параметров контура:
        DPoint3d* bnds = taskBounds;
        DPoint3d facetPoints[3][4] = {
            { bnds[0], bnds[1], bnds[7], bnds[4] },
            { bnds[1], bnds[2], bnds[6], bnds[7] },
            { bnds[4], bnds[7], bnds[6], bnds[5] },
        };

        for (int i = 0; i < 3; ++i) {
            DPlane3d facet;
            mdlVec_extractPolygonNormal(&facet.normal, &facet.origin, 
                facetPoints[i], 4);

            if (planesAreParallel(facet, planeFirst)) {
                DPoint3d pts[4];
                for (int j = 0; j < 4; ++j) {
                    mdlVec_projectPointToPlane(&pts[j],
                        &facetPoints[i][j], &facet.origin, &facet.normal);
                }

                mdlVec_extractPolygonNormal(NULL, &contourOrigin, pts, 4);
                mdlVec_projectPointToPlane(&contourOrigin, &contourOrigin,
                    &planeFirst.origin, &planeFirst.normal);

                { // Высота
                    OpeningTask::getInstance().height = CExpr::convertToMaster(
                        mdlVec_distance(&pts[3], &pts[0]));

                    mdlVec_subtractDPoint3dDPoint3d(&heightVec,
                        &pts[3], &pts[0]);
                }
                {// Ширина
                    OpeningTask::getInstance().width = CExpr::convertToMaster(
                        mdlVec_distance(&pts[1], &pts[0]));

                    mdlVec_subtractDPoint3dDPoint3d(&widthVec,
                        &pts[1], &pts[0]);
                }
                { // Глубина
                    DPoint3d projPoint;
                    mdlVec_projectPointToPlane(&projPoint, &contourOrigin,
                        &planeSecond.origin, &planeSecond.normal);

                    OpeningTask::getInstance().depth = CExpr::convertToMaster(
                        mdlVec_distance(&projPoint, &contourOrigin));

                    mdlVec_subtractDPoint3dDPoint3d(&depthVec,
                        &projPoint, &contourOrigin);
                }

                { // Считывание KKS
                    bool status;
                    XmlInstanceSchemaManager mgrR(eehP->GetModelRef());
                    mgrR.ReadSchemas(status);
                    if (!status) {
                        return false;
                    }

                    XmlInstanceStatus stt;
                    stt.status = LICENSE_FAILED;
                    XmlInstanceApi xapiR = XmlInstanceApi::CreateApi(stt, mgrR);
                    StringListHandle slhR = xapiR.ReadInstances(stt, eehP->GetElemRef());

                    int res = 0;
                    for (int i = 0; i < slhR.GetCount(); i++)
                    {
                        Bentley::WString strInst = slhR.GetString(i);

                        XmlNodeRef node = NULL;
                        if (XmlAttributeHelper::findNodeFromInstance(
                            &node, strInst, L"P3DEquipment"))
                        {
                            MSWChar value[50];
                            int maxchars = 50;
                            XmlNodeRef child = NULL;

                            if (XmlAttributeHelper::findChildNode(&child, node, L"Name") &&
                                XmlAttributeHelper::getNodeValue(value, &maxchars, child))
                            {
                                mdlCnv_convertUnicodeToMultibyte(value, -1, 
                                    OpeningTask::getInstance().kks, 200);
                            }
                            mdlXMLDomNode_free(node);
                           
                        }
                    }
                }

                mdlInput_sendSynchronizedKeyin("mdl keyin simpen.ui simpen.ui readData", 0, 0, 0);
                mdlInput_sendSynchronizedKeyin("mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);

                isValid = true;
                break;
            }
        }
    }

    return eehP;
}

void OpeningByTaskTool::OnComplexDynamics(MstnButtonEventCP ev) {
    updatePreview(NULL);
}

bool OpeningByTaskTool::OnDataButton(MstnButtonEventCP ev) {
    if (!OpeningTask::getInstance().isTaskSelected) {        
        return __super::OnDataButton(ev); // -> вызов BuildLocateAgenda
    }

    mdlInput_sendSynchronizedKeyin(
        "mdl keyin simpen.ui simpen.ui sendTaskData", 0, 0, 0);

    if (Opening::instance.getTask().isReadyToPublish) {
        computeAndAddToModel(Opening::instance);
        runTool(NULL);
    }

    return true;
}

void OpeningByTaskTool::OnRestartCommand() {
    runTool(NULL);
}

bool OpeningByTaskTool::OnResetButton(MstnButtonEventCP ev) {
    OnRestartCommand();
    return true;
}


bool OpeningByTaskTool::NeedAcceptPoint() {
    return true;
}

bool OpeningByTaskTool::WantAccuSnap()
{
    return false;
}

void OpeningByTaskTool::OnPostInstall() {
    __super::OnPostInstall();

    instanceP = this;
    Opening::instance = Opening();
    OpeningTask::getInstance().isTaskSelected = false;

    isValid = false;

    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlSelect_freeAll();
    prevTask = OpeningTask();

    mdlOutput_promptU("Выбирите объект технологического задания для проёма");
}

bool OpeningByTaskTool::OnInstall() {
    if (!__super::OnInstall())
        return false;

    SetCmdNumber(0);   // For toolsettings/undo string...
    SetCmdName(0, 0);  // For command prompt...

    return true;
}

StatusInt OpeningByTaskTool::OnElementModify(EditElemHandleR eeh) {
    return ERROR; // реализация через OnDataButton
}

void OpeningByTaskTool::OnCleanup() {
    instanceP = NULL;
    mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
}

}