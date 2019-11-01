#include "OpeningTask.h"
#include "CExpression.h"

#include <string.h>
#include <mscexpr.fdf>

#include <ditemlib.fdf>
#include <mscnv.fdf>

namespace Openings {

OpeningTask OpeningTask::instance_ = OpeningTask();

OpeningTask::OpeningTask() 
{
    // Значения по умолчанию:
    height =
    width =
    depth = 0.0;
    strcpy(kks, "");

    isThroughHole = true;
    isRequiredRemoveContour = true;
    isReadyToPublish = false;
}

OpeningTask& OpeningTask::getInstance() {
    return instance_;
}

bool OpeningTask::operator ==(OpeningTask other) {
    return 
        height == other.height &&
        width == other.width &&
        depth == other.depth &&
        strcmp(kks, other.kks) &&
        isThroughHole == other.isThroughHole &&
        isRequiredRemoveContour == other.isRequiredRemoveContour &&
        isReadyToPublish == other.isReadyToPublish;
}

bool OpeningTask::operator !=(OpeningTask other) {
    return !(*this == other);
}

const void OpeningTask::publishCExpressions(SymbolSet* symSetP) {

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        (char*)CExpr::Opening::HEIGHT, &instance_.height);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        (char*)CExpr::Opening::WIDTH, &instance_.width);
    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_DOUBLE),
        (char*)CExpr::Opening::DISTANCE, &instance_.depth);
    mdlDialog_publishBasicArray(symSetP, mdlCExpression_getType(TYPECODE_CHAR),
        (char*)CExpr::Opening::KKS, &instance_.kks, sizeof(instance_.kks));

    // - откл. ez 2019-10-23
    //mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT), 
    //    "openingIsSweepBi", &instance.isSweepBi);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        (char*)CExpr::Opening::IS_THROUGH_HOLE, &instance_.isThroughHole);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        (char*)CExpr::Opening::IS_REQUIRED_REMOVE_CONTOUR, &instance_.isRequiredRemoveContour);

    mdlDialog_publishBasicVariable(symSetP, mdlCExpression_getType(TYPECODE_INT),
        (char*)CExpr::Opening::IS_READY_TO_PUBLISH, &instance_.isReadyToPublish);
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