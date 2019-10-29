#pragma once

#ifndef ELEMENT_HELPER_H
#define ELEMENT_HELPER_H

#include <mdl.h>
#include <mselmdsc.fdf>

#include <tfform.h>
#include <tfframe.h>


//#include    <stdio.h>
//#include    <string.h>
//#include    <malloc.h>
//#include    <mselemen.fdf>
//#include    <mslinkge.fdf>
//#include    <msscancrit.fdf>
//#include    <mstagdat.fdf>
//#include    <mselems.h>
//#include    <mscell.fdf>
//#include    <leveltable.fdf>
//#include    <mslstyle.fdf>
//#include    <msstrlst.h>
//#include    <mscnv.fdf>
//#include    <msdgnobj.fdf>
//#include    <msmodel.fdf>
//#include    <msview.fdf>
//#include    <msviewinfo.fdf>
//#include    <msvar.fdf>
//#include    <dlmsys.fdf>
//#include    <msdialog.fdf>

//#include    <msrmgr.h>
//#include    <mssystem.fdf>
//#include    <msparse.fdf>



//#include <toolsubs.h>
//
//#include <elementref.h>
//#include <msdependency.fdf>
//#include <msassoc.fdf>
//#include <msmisc.fdf>
//#include <mslocate.fdf>
//#include <msstate.fdf>
//#include <msoutput.fdf>
//#include <mstypes.h>
//#include <MicroStationAPI.h>
//#include <mstmatrx.fdf>
//#include <accudraw.h>
//

#include <interface/ElemHandle.h>
#include <interface/ElemHandleGeometry.h>
#include <interface/element/DisplayHandler.h> 
#include <leveltable.h>

#include <list>

bool operator ==(DPoint3d& pt0, DPoint3d& pt01);
bool operator !=(DPoint3d& pt0, DPoint3d& pt01);

// vector - нельзя, так как у EditElemHandle - приватный оператор "="
typedef class std::list<Bentley::Ustn::Element::EditElemHandle> ElementList;

bool сreateLine(EditElemHandleR eehOut, DPoint3d* points);
bool сreateStringLine(EditElemHandleR eehOut, DPoint3d* points, int numVerts, const UInt32* weight = NULL);

bool createShape(EditElemHandleR eehOut, DPoint3d* points, int numVerts, bool fillMode);
bool createBody(EditElemHandleR eehOut, EditElemHandleR shape, const DVec3d& vec,
    double height, double shell);

bool CreateCell(
    EditElemHandleR eehOut, MSWCharCP name, DPoint3dP origin, BoolInt pointCell);
bool AddChildToCell(EditElemHandleR cell, EditElemHandleR child);

bool ToEditHandle(EditElemHandleR eehOut, MSElement element);
bool ToEditHandle(EditElemHandleR eehOut, MSElementDescrP elemDescrP);

TFFormRecipeList* findIntersectedTfFormWithElement(
    const MSElementP elementP, int* tfTypes, int tfTypeNum);

bool planesAreMatch(const DPlane3d& first, const DPlane3d& second);
bool planesAreParallel(const DPlane3d& first, const DPlane3d& second);
double distanceToPlane(const DPoint3d& point, const DPlane3d& plane);
DVec3d computeVector(const DPoint3d& first, const DPoint3d& second);
DVec3d computeVectorToPlane(const DPoint3d& point, const DPlane3d& plane);

LevelID getLevelIdByName(MSWCharCP name);

// Создание объекта прямоугольной проходки
TFFrame* createPenetrFrame(
    EditElemHandleR shapeBody, EditElemHandleR shapePerf,
    EditElemHandleR crossFirst, EditElemHandleR crossSecond,
    DVec3dR vec, double distance, double shell,
    bool isSweepBi, bool isPolicyThrough);

void createCross(EditElemHandleR outCross,
    const DPoint3d& centroid, const DPoint3d* vertices, int numVerts);

#endif // !ELEMENT_HELPER_H