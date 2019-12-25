#pragma once

#ifndef OPENING_H
#define OPENING_H

#include <string>
#include <vector>

#include <mdl.h>
#include "OpeningTask.h"

bool operator == (const DPoint3d lhs, const DPoint3d rhs);
bool operator != (const DPoint3d lhs, const DPoint3d rhs);
bool operator < (const DPoint3d lhs, const DPoint3d rhs);
bool operator > (const DPoint3d lhs, const DPoint3d rhs);

bool operator == (const Symbology lhs, const Symbology rhs);
bool operator != (const Symbology lhs, const Symbology rhs);

namespace Openings {


struct Opening {
	static const std::wstring CELL_NAME;
    static const std::wstring CATALOG_TYPE_NAME;
    static const std::wstring CATALOG_INSTANCE_NAME;
    static const MSWCharCP LEVEL_NAME;
    static const MSWCharCP LEVEL_SYMBOL_NAME;
	static const Symbology SYMBOLOGY;

    DPoint3d origin;
    DVec3d direction;

    ElementRef contourRef;
    DgnModelRefP contourModelRef;

    std::vector<DPoint3d> contourPoints;

    static Opening instance;

    Opening();
    Opening(MSElementDescrP shapeEdP);

    double getDistance();
    void setDistance(double value);

    char* getKKS();
    void setKKS(const char* value);

    OpeningTask& getTask();
    const bool isValid();

    static bool isElemContourType(int type);

    bool operator ==(Opening other);
    bool operator !=(Opening other);
};

const bool isZero(DVec3dR vec);

double convertFromCExprVal(double cexpr);
void convertToCExprVal(double* cexpr, double value);

}

#endif // !OPENING_H