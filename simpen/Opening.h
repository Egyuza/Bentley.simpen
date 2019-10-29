#pragma once

#ifndef OPENING_H
#define OPENING_H

#include <mdl.h>
#include <cexpr.h>
#include <string>

namespace Openings {

struct Opening {
    DPoint3d origin;
    DVec3d direction;
    double userDistance; // todo значение необходимо конвертировать по UOR модели!
    double distance;
    // EditElemHandleP contourP;
    ElementRef contourRef;
    char kks[50];

    bool isThroughHole;
    //bool isSweepBi; // - откл. ez 2019-10-23
    bool isRequiredRemoveContour;
    bool isReadyToPublish;

    static const std::wstring CATALOG_TYPE_NAME;
    static const std::wstring CATALOG_INSTANCE_NAME;
        
    const bool isValid();

    static const void publishCExpressions(SymbolSet* symSetP);

    static Opening instance;

    Opening();
};

const bool isZero(DVec3dR vec);

double convertFromCExprVal(double cexpr);
void convertToCExprVal(double* cexpr, double value);
bool setDataGroupInstanceValue(const ElementRef& elemRef, const std::wstring& value);

}

#endif // !OPENING_H