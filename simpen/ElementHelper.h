#pragma once

#ifndef ELEMENT_HELPER_H
#define ELEMENT_HELPER_H

#include <mdl.h>

#include <tfform.h>
#include <tfframe.h>
#include <tfpoly.h>

#include <interface/ElemHandle.h>
#include <interface/ElemHandleGeometry.h>
#include <interface/element/DisplayHandler.h> 
#include <leveltable.h>
#include <buildingeditelemhandle.h>

#include <list>

bool operator ==(DPoint3d& pt0, DPoint3d& pt01);
bool operator !=(DPoint3d& pt0, DPoint3d& pt01);

// vector - нельзя, так как у EditElemHandle - приватный оператор "="
typedef class std::list<Bentley::Ustn::Element::EditElemHandle> ElementList;

DPoint3d getMiddle(DPoint3d* targetP, DPoint3d* baseP);

bool createLine(EditElemHandleR eehOut, DPoint3d* points);
bool createStringLine(EditElemHandleR eehOut, DPoint3d* points, int numVerts, const UInt32* weight = NULL);

bool createCircle(EditElemHandleR eehOut,
    DPoint3dCR center, double radius, int fillMode);

bool createArcEllipse(EditElemHandleR eehOut, DPoint3dCR center,
	double radius1, double radius2, RotMatrix* rotP);

bool createEllipse(EditElemHandleR eehOut, DPoint3dCR center,
    double radius1, double radius2, RotMatrix* rotP, int fillMode);

bool createShape(EditElemHandleR eehOut, DPoint3d* points, int numVerts, bool fillMode);
bool createBody(EditElemHandleR eehOut, EditElemHandleR shape, const DVec3d& vec,
    double height, double shell);

bool CreateCell(
    EditElemHandleR eehOut, MSWCharCP name, DPoint3dP origin, BoolInt pointCell);
bool AddChildToCell(EditElemHandleR cell, EditElemHandleR child);

bool ToEditHandle(EditElemHandleR eehOut, MSElement element);
bool ToEditHandle(EditElemHandleR eehOut, MSElementDescrP elemDescrP);

ElementRef findIntersectedByTFType(
    const MSElementDescrP edP, const int typesNum, int tfType, ...);

ElementRef tfFindIntersectedByDGInstance(const MSElementP elementP,
	const int typesCount, std::wstring dgInstNames[], std::wstring& outMatchedInstName);

bool planesAreMatch(const DPlane3d& first, const DPlane3d& second);
bool planesAreParallel(const DPlane3d& first, const DPlane3d& second);
double distanceToPlane(const DPoint3d& point, const DPlane3d& plane);
DVec3d computeVector(const DPoint3d& first, const DPoint3d& second);
DVec3d computeVectorToPlane(const DPoint3d& point, const DPlane3d& plane);

// Создание объекта прямоугольной проходки
TFFrame* createPenetrFrame(
    EditElemHandleR body, EditElemHandleR shapePerf,
    DVec3dR vec, double distance, double shell,
    bool isSweepBi, bool isPolicyThrough);

// Добавление геометрии, которая должна проецироваться на плоскость чертёжа
StatusInt appendToProjection(TFFrame* frameP, MSElementDescrCP edCP);

void createCross(EditElemHandleR outCross,
    const DPoint3d& centroid, const DPoint3d* vertices, int numVerts);

StatusInt getFacePlaneByLabel(
    DPlane3dR outPlane, MSElementDescrCP tfformEdP, FaceLabelEnum label);

bool setDataGroupInstanceValue(
	Bentley::Building::Elements::BuildingEditElemHandle& beeh,
	const std::wstring& catalogType, const std::wstring& catalogInstance,
	const std::wstring& itemXPath, const std::wstring& value);

#endif // !ELEMENT_HELPER_H