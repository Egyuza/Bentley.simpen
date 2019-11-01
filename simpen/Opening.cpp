#include "Opening.h"
#include "OpeningTask.h"
#include "CExpression.h"

//#include <interface/ElemHandle.h>
#include <elementref.h>

#include <msvec.fdf>

using Bentley::Ustn::Element::EditElemHandle;

namespace Openings {

Opening Opening::instance = Opening();

const std::wstring Opening::CATALOG_TYPE_NAME = L"Opening";
const std::wstring Opening::CATALOG_INSTANCE_NAME = L"Opening";

Opening::Opening() {
    origin =
    direction = DPoint3d();
    contourRef = ElementRef();
}

OpeningTask& Opening::getTask() {
    return OpeningTask::getInstance();
}

double Opening::getDistance() {
    return  CExpr::convertToUOR(OpeningTask::getInstance().depth);
}
void Opening::setDistance(double value) {
    OpeningTask::getInstance().depth = CExpr::convertToMaster(value);
}

char* Opening::getKKS() {
    return OpeningTask::getInstance().kks;
}
void Opening::setKKS(const char* value) {
    strcpy(OpeningTask::getInstance().kks, value);
}

bool Opening::operator ==(Opening other) {
    return mdlVec_equal(&origin, &other.origin) &&
        mdlVec_equal(&direction, &other.direction) &&
        getDistance() == other.getDistance() &&
        contourRef == other.contourRef &&
        getTask().isThroughHole == other.getTask().isThroughHole;
}

bool Opening::operator !=(Opening other) {
    return !(*this == other);
}

const bool Opening::isValid()
{
    return
        elementRef_isEOF(contourRef) == FALSE &&
        elementRef_getElemType(contourRef) == SHAPE_ELM &&
        !isZero(direction);
}

const bool isZero(DVec3dR vec)
{
    DVec3d zero;
    mdlVec_zero(&zero);

    return TRUE == mdlVec_equal(&vec, &zero);
}

}
