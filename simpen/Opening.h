#pragma once

#ifndef OPENING_H
#define OPENING_H

#include <string>
#include <vector>

#include <mdl.h>
#include "OpeningTask.h"

namespace Openings {

struct Opening {
    static const std::wstring CATALOG_TYPE_NAME;
    static const std::wstring CATALOG_INSTANCE_NAME;

    DPoint3d origin;
    DVec3d direction;

    ElementRef contourRef;
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

    bool operator ==(Opening other);
    bool operator !=(Opening other);
};

const bool isZero(DVec3dR vec);

double convertFromCExprVal(double cexpr);
void convertToCExprVal(double* cexpr, double value);
bool setDataGroupInstanceValue(const ElementRef& elemRef, const std::wstring& value);

}

#endif // !OPENING_H