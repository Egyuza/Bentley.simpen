#include "simpencmd.h"
#include "Opening.h"
#include "OpeningByTaskTool.h"
#include "OpeningHelper.h"
#include "ElementHelper.h"
#include "XmlAttributeHelper.h"
#include "CExpression.h"
#include "ui.h"

#include <buildingeditelemhandle.h>
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
#include <mskisolid.fdf>
#include <mslocate.fdf>
#include <msmisc.fdf>
#include <msoutput.fdf>
#include <msselect.fdf>
#include <msstate.fdf>
#include <mstmatrx.fdf>
#include <mstrnsnt.fdf>
#include <msundo.fdf>
#include <msvar.fdf>
#include <msvec.fdf>
#include <msview.fdf>
#include <mswindow.fdf>

#include <mscurrtr.fdf>


#include <set>
#include <vector>

namespace Openings
{

USING_NAMESPACE_BENTLEY_XMLINSTANCEAPI_NATIVE;

OpeningByTaskTool* OpeningByTaskTool::instanceP = NULL;
OpeningTask OpeningByTaskTool::prevTask = OpeningTask();

void OpeningByTaskTool::run(char *unparsedP)
{
    mdlSelect_freeAll();
    OpeningByTaskTool::instanceP = new OpeningByTaskTool();
    OpeningByTaskTool::instanceP->InstallTool();
}

void OpeningByTaskTool::updatePreview(char *unparsedP)
{
    if (prevTask == OpeningTask::getInstance()) {
        return; // параметры построения не изменились
    }
	
	mdlTransient_free(&msTransientElmP, true);

    OpeningByTaskTool* toolP = OpeningByTaskTool::instanceP;
	if (!toolP || toolP->isAddToModelProcessActive) {
		return;
	}

    OpeningByTaskTool::prevTask = OpeningTask::getInstance();

	{ // ДЛЯ ОТЛАДКИ:
		//EditElemHandle line;
		//сreateStringLine(line, toolP->taskBounds, 8);
		//msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
		//	line.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
	
	}

    if (!toolP->isValid) {
        // параметры проёма не определены
        return;
    }

    DPoint3d contourPts[4];
    { // восстановление контура, т.к. пользователь мог изменить его параметры
        mdlVec_normalize(&toolP->heightVec);
        mdlVec_normalize(&toolP->widthVec);
        mdlVec_normalize(&toolP->depthVec);

        double height = CExpr::convertToUOR(OpeningTask::getInstance().height);
        double width = CExpr::convertToUOR(OpeningTask::getInstance().width);
        double depth = CExpr::convertToUOR(OpeningTask::getInstance().depth);

        double halfHeight = height / 2;
        double halfWidth = width / 2;

        DPoint3d lhs, rhs;
        mdlVec_projectPoint(&lhs, &toolP->contourOrigin, &toolP->heightVec, halfHeight);
        mdlVec_projectPoint(&rhs, &toolP->contourOrigin, &toolP->heightVec, -halfHeight);

        mdlVec_projectPoint(&contourPts[0], &lhs, &toolP->widthVec, halfWidth);
        mdlVec_projectPoint(&contourPts[1], &rhs, &toolP->widthVec, halfWidth);
        mdlVec_projectPoint(&contourPts[2], &rhs, &toolP->widthVec, -halfWidth);
        mdlVec_projectPoint(&contourPts[3], &lhs, &toolP->widthVec, -halfWidth);
    }

    EditElemHandle contour;
    if (createShape(contour, contourPts, 4, false)) {
        msTransientElmP = mdlTransient_addElemDescr(msTransientElmP,
            contour.GetElemDescrCP(), true, 0xffff, DRAW_MODE_TempDraw, FALSE, FALSE, TRUE);
    }
    
    Opening::instance = Opening(contour.GetElemDescrP());

    if (SUCCESS != computeAndDrawTransient(Opening::instance)) {
        // todo сообщение об ошибке
        UI::prompt("Указанный контур должен быть параллелен плоскости указанной стены/плиты");
        return;
    }

    if (Opening::instance.isValid()) {
		UI::setEnableAddToModel();
    }
}

void OpeningByTaskTool::addToModel(char *unparsedP)
{
	OpeningByTaskTool* toolP = OpeningByTaskTool::instanceP;
	toolP->isAddToModelProcessActive = true;

	UI::sendTaskDataSynch();

    if (Opening::instance.isValid() && toolP &&
        Opening::instance.getTask().isReadyToPublish) 
    {
		mdlUndo_startGroup();
		{
			MSElementDescrP taskEdP = NULL;
			mdlElmdscr_getByElemRef(
				&taskEdP, toolP->taskRef, ACTIVEMODEL, FALSE, 0);

			if (taskEdP && taskEdP->h.dgnModelRef != ACTIVEMODEL) {
				taskEdP = NULL;
			}

			computeAndAddToModel(
				Opening::instance, toolP->isTaskIsOpening, taskEdP);
		
			{ // удаление smartSolid
				if (toolP->isTaskIsSmartSolid && taskEdP)
				{
					int fpos = mdlElmdscr_getFilePos(taskEdP);
					mdlElmdscr_deleteByModelRef(taskEdP, fpos, ACTIVEMODEL, TRUE);
				}
			}
		}
		mdlUndo_endGroup();
    }

	toolP->isAddToModelProcessActive = false;
	run(NULL);
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
    
	// 2-ая для бывших дверных проёмов - <SmartSolid>
	if (TF_EBREP_ELM == mdlTFElmdscr_getApplicationType(eeh.GetElemDescrP())) {
		return true;
	}

	// 3-я проверка для самих проёмов
	if (isOpening(eeh.GetElemDescrP())) {
		// return true;
	}

    { 
	// 4-я проверка по атрибуту <P3DEquipment>
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
	clear();

    EditElemHandleP eehP = __super::BuildLocateAgenda(path, ev);

    if (!eehP->IsValid()) { // НВС
        return eehP;
    }

	taskRef = eehP->GetElemRef();
    OpeningTask::getInstance().isTaskSelected = true;

	// для бывших дверных проёмов - <SmartSolid>
	isTaskIsSmartSolid = 
		TF_EBREP_ELM == mdlTFElmdscr_getApplicationType(eehP->GetElemDescrP());
	isTaskIsOpening = isOpening(eehP->GetElemDescrP());

	MSElementDescrP boundsEdP = eehP->GetElemDescrP();

	{	// ОБРАБОТКА/КОРРЕКТИРОВКА объекта-задания из SPF:
		MSElementDescrP recEdP = eehP->GetElemDescrP();
		
		while (recEdP->el.ehdr.type == CELL_HEADER_ELM) {
			if (recEdP->h.firstElem->el.ehdr.type != CELL_HEADER_ELM) {
				// если первый дочерний уже не цел, то начинаем проверку

				// бывают задания, в которых присутствует лишняя 
				// сфера-поверхность, малая в сравнении с заданием, 
				// за счёт которой переопределяется интересующий проектировнщика
				// диапазон задания
				
				double diagonal = 0;

				MSElementDescrP curEdP = recEdP;
				while (curEdP != NULL && curEdP->el.ehdr.type == CELL_HEADER_ELM) {
					DPoint3d bnds[8];
					mdlCell_extract(NULL, bnds, NULL, NULL, NULL, 0, &curEdP->el);
					
					double subDiag = mdlVec_distance(&bnds[0], &bnds[6]);

					if (subDiag > diagonal) {
						diagonal = subDiag;
						boundsEdP = curEdP;
					}

					curEdP = curEdP->h.next;
				}
			}
			recEdP = recEdP->h.firstElem;
		}
	}

    { // Считывание геометрич. параметров задания:
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
            mdlElmdscr_transform(boundsEdP, &tran);
        }
		// вершины в общем случае по Range, но могут быть неточными...
        mdlCell_extract(&taskOrigin, taskBounds, NULL, NULL, NULL, 0, &boundsEdP->el);

		TFBrepList* brepListP = NULL;
		mdlTFBrepList_constructFromElmdscr(&brepListP, boundsEdP, ACTIVEMODEL);

		DPoint3d* verts = NULL;
		int vertsCount = 0;

		std::set<DPoint3d> vertsSet = std::set<DPoint3d>();
		std::vector<DPoint3d> vertsVec = std::vector<DPoint3d>();
		while (brepListP)
		{
			TFBrep* brepP = mdlTFBrepList_getBrep(brepListP);
			mdlTFBrep_getVertexLocations(brepP, &verts, &vertsCount);

			brepListP = mdlTFBrepList_getNext(brepListP);

			if (vertsCount == 24) {
				for (int i = 0; i < 24; ++i) {
					if (vertsSet.insert(verts[i]).second) {
						// сохраняем порядок точек
						vertsVec.push_back(verts[i]);
					}
				}
			}
		}
		if (brepListP) {
			mdlTFBrepList_free(&brepListP);
		}

		if (vertsCount == 8) {
			for (int i = 0; i < 8; ++i) {
				taskBounds[i] = verts[i];
			}
		}
		else if (vertsCount == 24) {
			if (vertsSet.size() == 8) { // корректируем вершины задания:
				for (int i = 0; i < 8; ++i) {
					taskBounds[i] = vertsVec[i];
				}
			}
		}
		
		if (false)
		{
			// ПРАВИЛЬНЫЙ пример преобразования KISolid координат:
			KIBODY* bodyP = NULL;
			Transform bodyTran;
			mdlKISolid_elementToBody2(&bodyP, &bodyTran, boundsEdP, ACTIVEMODEL, 1, FALSE);

			KIENTITY_LIST* listP = NULL;

			mdlKISolid_listCreate(&listP);
			mdlKISolid_getVertexList(listP, bodyP);
			int count = 0;
			mdlKISolid_listCount(&count, listP);

			DPoint3d bbounds[8];
			for (int i = 0; i < count && i < 8; i++)
			{
				KIVERTEX* vertexP = NULL;
				mdlKISolid_listNthEl(&vertexP, listP, i);
				DPoint3d pnt;
				mdlKISolid_vertexData(&pnt, vertexP);

				mdlKISolid_beginCurrTrans(ACTIVEMODEL);

				mdlTMatrix_transformPoint(&pnt, &bodyTran);
				mdlCurrTrans_transformPointArray(&pnt, &pnt, 1);

				bbounds[i] = pnt;

				mdlKISolid_endCurrTrans();
			}
			mdlKISolid_freeBody(bodyP);
		}
    }

// #endif // DEBUG

	//  СЧИТЫВАНИЕ KKS ИЗ ЗАДАНИЯ:a
	if (isTaskIsOpening) {
		// заданием выступает сам проём

		using namespace Bentley::Building::Elements;
		BuildingEditElemHandle beeh(eehP->GetElemDescrP(), false, true);
		beeh.LoadDataGroupData();

		CCatalogCollection::CCollectionConst_iterator itr;
		CCatalogCollection& collection = beeh.GetCatalogCollection();
		for (itr = collection.Begin(); itr != collection.End(); itr++)
		{
			const std::wstring catalogInstanceName = itr->first;
			if (catalogInstanceName == L"Opening") {
				std::wstring itemXPath = L"Opening/@PartCode";

				CCatalogSchemaItemT*    pSchemaItem = NULL;
				if ((pSchemaItem = collection.FindDataGroupSchemaItem(itemXPath))) {
					mdlCnv_convertUnicodeToMultibyte(
						&pSchemaItem->GetValue()[0], -1,
						OpeningTask::getInstance().kks, 200);
				}

				break;
			}
		}		
	}
	else {
		// технологическое задание
		bool status;
		XmlInstanceSchemaManager mgrR(eehP->GetModelRef());
		mgrR.ReadSchemas(status);
		if (status) {
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
	}
	
    ElementRef& formRef = OpeningTask::getInstance().tfFormRef;

    formRef = findIntersectedByTFType(boundsEdP,
        4, TF_LINEAR_FORM_ELM, TF_SLAB_FORM_ELM, TF_FREE_FORM_ELM, TF_ARC_FORM_ELM);

	std::wstring matchedInstName = L"";
	if (formRef == NULL) {
		std::wstring searchInstances[2] = { L"ConcreteWalls", L"ConcreteSlabs" };
		formRef = tfFindIntersectedByDGInstance(
			&boundsEdP->el, 2, searchInstances, matchedInstName);
	}

    if (formRef == NULL) {
        mdlOutput_messageCenter(MESSAGE_WARNING, 
            "Не найден объект <Wall> или <Slab>, внутри которого должен находиться объект тех. задания", "", FALSE);
        return eehP;
    }

    { // Определение плоскостей стены/плиты:
        EditElemHandle formEeh = EditElemHandle(formRef, ACTIVEMODEL);

        StatusInt status = SUCCESS;

		int formType = mdlTFElmdscr_getApplicationType(formEeh.GetElemDescrP());
		if (formType == TF_ARC_FORM_ELM) {
			// РАДИАЛЬНЫЕ СТЕНЫ

			status = ERROR;
			EditElemHandle formEeh = EditElemHandle(formRef, ACTIVEMODEL);

			TFBrepList* brepListP = NULL;
			mdlTFBrepList_constructFromElmdscr(&brepListP, formEeh.GetElemDescrP(), ACTIVEMODEL);

			TFBrepFaceList* faceListP = 
				mdlTFBrepList_getFacesByLabel(brepListP, FaceLabelEnum_Right);

			TFBrepList* brepIter = brepListP;
			while (brepIter) {
				MSElementDescr* faceP;
				mdlTFBrepFaceList_getElmdscr(&faceP, faceListP, 0);
				
				TFBrep* brep = mdlTFBrepList_getBrep(brepIter);

				double dist;
				for (int i = 0; i < 8; ++i) {
					dist = mdlTFBrep_findClosestPoint(brep, &taskBounds[i], &taskBounds[i]);
					dist = dist;
				}

				mdlVec_extractPolygonNormal(&planeFirst.normal, &planeFirst.origin,
					&taskBounds[0], 4);
				mdlVec_extractPolygonNormal(&planeSecond.normal, &planeSecond.origin,
					&taskBounds[4], 4);

				status = SUCCESS;

				// в случае когда поверхность стены не ломанная - след. будет NULL
				brepIter = mdlTFBrepList_getNext(brepIter);
			}
			if (faceListP) {
				mdlTFBrepFaceList_free(&faceListP);
			}

			mdlTFBrepList_free(&brepListP);
		}       
        else if (formType == TF_LINEAR_FORM_ELM || matchedInstName == L"ConcreteWalls" ||
			formType == TF_ARC_FORM_ELM) 
		{
            status = getFacePlaneByLabel(planeFirst,
                formEeh.GetElemDescrP(), FaceLabelEnum_Left);
            status += getFacePlaneByLabel(planeSecond,
                formEeh.GetElemDescrP(), FaceLabelEnum_Right);
        }
        else if (formType == TF_SLAB_FORM_ELM || formType == TF_FREE_FORM_ELM ||
			matchedInstName == L"ConcreteSlabs")
		{
            status = getFacePlaneByLabel(planeFirst,
                formEeh.GetElemDescrP(), FaceLabelEnum_Top);
            status += getFacePlaneByLabel(planeSecond,
                formEeh.GetElemDescrP(), FaceLabelEnum_Base);
        }
        else {
            return eehP;
        }


		if (status != SUCCESS) {
			if (formType == TF_SLAB_FORM_ELM || formType == TF_FREE_FORM_ELM) {
				UI::warning("Не удаётся получить проекцию тех. задания на найденный объект <Slab>");
			}
			else if (formType == TF_LINEAR_FORM_ELM) {
				UI::warning("Не удаётся получить проекцию тех. задания на найденный объект <Wall>");
			}
			else {
				UI::warning("Не удаётся получить проекцию тех. задания на найденный объект"); // НВС
			}
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
            else if (formType == TF_SLAB_FORM_ELM || formType == TF_FREE_FORM_ELM) {
                if ((int)projOrigin.z > (int)planeFirst.origin.z) {
                    buff = planeFirst;
                    planeFirst = planeSecond;
                    planeSecond = buff;
                }
            }
        }
    }        
    
    { // Определение параметров контура:
        DPoint3d* bnds = taskBounds;
		DPoint3d facetPoints[3][4];

		EditElemHandle formEeh = EditElemHandle(formRef, ACTIVEMODEL);
		int formType = mdlTFElmdscr_getApplicationType(formEeh.GetElemDescrP());
		
		if (formType == TF_ARC_FORM_ELM) {
			facetPoints[0][0] = bnds[0];
			facetPoints[0][1] = bnds[1];
			facetPoints[0][2] = bnds[2];
			facetPoints[0][3] = bnds[3];
			
			facetPoints[1][0] = bnds[0];
			facetPoints[1][1] = bnds[3];
			facetPoints[1][2] = bnds[4];
			facetPoints[1][3] = bnds[7];
			
			facetPoints[2][0] = bnds[3];
			facetPoints[2][1] = bnds[2];
			facetPoints[2][2] = bnds[5];
			facetPoints[2][3] = bnds[4];
		}
		else {
			//	{ bnds[0], bnds[1], bnds[7], bnds[4] },
			//	{ bnds[1], bnds[2], bnds[6], bnds[7] },
			//	{ bnds[4], bnds[7], bnds[6], bnds[5] },

			facetPoints[0][0] = bnds[0];
			facetPoints[0][1] = bnds[1];
			facetPoints[0][2] = bnds[7];
			facetPoints[0][3] = bnds[4];

			facetPoints[1][0] = bnds[1];
			facetPoints[1][1] = bnds[2];
			facetPoints[1][2] = bnds[6];
			facetPoints[1][3] = bnds[7];

			facetPoints[2][0] = bnds[4];
			facetPoints[2][1] = bnds[7];
			facetPoints[2][2] = bnds[6];
			facetPoints[2][3] = bnds[5];
		}
			
			

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
				// опорная точка Origin:
                mdlVec_extractPolygonNormal(NULL, &contourOrigin, pts, 4);
                mdlVec_projectPointToPlane(&contourOrigin, &contourOrigin,
                    &planeFirst.origin, &planeFirst.normal);

                { // Высота
                    OpeningTask::getInstance().height = CExpr::convertToMaster(
                        mdlVec_distance(&pts[3], &pts[0]));
					// вектор высоты:
                    mdlVec_subtractDPoint3dDPoint3d(&heightVec,
                        &pts[3], &pts[0]);
                }
                {// Ширина
                    OpeningTask::getInstance().width = CExpr::convertToMaster(
                        mdlVec_distance(&pts[1], &pts[0]));
					// вектор ширины:
                    mdlVec_subtractDPoint3dDPoint3d(&widthVec,
                        &pts[1], &pts[0]);
                }
                { // Глубина
                    DPoint3d projPoint;
                    mdlVec_projectPointToPlane(&projPoint, &contourOrigin,
                        &planeSecond.origin, &planeSecond.normal);

                    OpeningTask::getInstance().depth = CExpr::convertToMaster(
                        mdlVec_distance(&projPoint, &contourOrigin));
					// вектор глубины:
                    mdlVec_subtractDPoint3dDPoint3d(&depthVec,
                        &projPoint, &contourOrigin);
                }

                isValid = true;
				UI::readDataSynch();
				UI::setEnableAddToModel();

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

	addToModel(NULL);

    if (Opening::instance.getTask().isReadyToPublish) {
        computeAndAddToModel(Opening::instance);
        run(NULL);
    }

    return true;
}

void OpeningByTaskTool::OnRestartCommand() {
    run(NULL);
}

bool OpeningByTaskTool::OnResetButton(MstnButtonEventCP ev) {
    OnRestartCommand();
    return true;
}


bool OpeningByTaskTool::NeedAcceptPoint() {
    return true;
}

bool OpeningByTaskTool::WantAccuSnap() {
    return false;
}

void OpeningByTaskTool::clear() {
	instanceP = this; // НВС

	instanceP = this;
	Opening::instance = Opening();
	OpeningTask::getInstance().clear();

	isValid = false;
	taskRef = NULL;

	isTaskIsSmartSolid =
	isTaskIsOpening = false;
	isAddToModelProcessActive = false;

	// todo ? сброс занчений в форме пользователя ?
}


void OpeningByTaskTool::OnPostInstall() {
    __super::OnPostInstall();

	clear();

    mdlAccuDraw_setEnabledState(false); // Don't enable AccuDraw w/Dynamics...
    mdlLocate_setCursor();
    mdlLocate_allowLocked();

    mdlSelect_freeAll();
    prevTask = OpeningTask();
	


    UI::promptU("Выберите объект технологического задания для проёма");
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
	UI::reload();
}

}