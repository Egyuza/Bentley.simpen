#include "Opening.h"

#include <cexpr.h>
#include <interface/ElemHandle.h>
#include <elementref.h>

#include <ditemlib.fdf>
#include <mscexpr.fdf>
#include <mscnv.fdf>
#include <msvec.fdf>
#include <string>

using Bentley::Ustn::Element::EditElemHandle;

namespace Openings {

Opening Opening::instance = Opening();

const std::wstring Opening::CATALOG_TYPE_NAME = L"Opening";
const std::wstring Opening::CATALOG_INSTANCE_NAME = L"Opening";

Opening::Opening() {
    userDistance = 0.0;
    userDistance = 0.0;
    isReadyToPublish = false;
    isThroughHole = true;
    isRequiredRemoveContour = true;
    strcpy(kks, "");
}

const bool Opening::isValid()
{
    return
        elementRef_isEOF(contourRef) == FALSE &&
        elementRef_getElemType(contourRef) == SHAPE_ELM &&
        !isZero(direction);
}

const void Opening::publishCExpressions(SymbolSet* symSetP) {
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        "openingDistance", &instance.userDistance);
    
    mdlDialog_publishBasicArray(symSetP, mdlCExpression_getType(TYPECODE_CHAR),
        "openingKKS", &instance.kks, sizeof(instance.kks));
    
    // - откл. ez 2019-10-23
    //mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT), 
    //    "openingIsSweepBi", &instance.isSweepBi);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        "openingIsThroughHole", &instance.isThroughHole);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        "openingIsReadyToPublish", &instance.isReadyToPublish);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        "openingIsRequiredRemoveContour", &instance.isRequiredRemoveContour);
}

const bool isZero(DVec3dR vec)
{
    DVec3d zero;
    mdlVec_zero(&zero);

    return TRUE == mdlVec_equal(&vec, &zero);
}

double convertFromCExprVal(double cexpr) {
    double res = 0;
    mdlCnv_masterToUOR(&res, cexpr, ACTIVEMODEL);
    return res;
}
void convertToCExprVal(double* cexpr, double value) {
    mdlCnv_UORToMaster(cexpr, value, ACTIVEMODEL);
}

}
