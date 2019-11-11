#include "Opening.h"
#include "OpeningTask.h"
#include "CExpression.h"

#include <set>
#include <algorithm>

//#include <interface/ElemHandle.h>
#include <elementref.h>

#include <msvec.fdf>
#include <msmisc.fdf>
#include <mselmdsc.fdf>


using Bentley::Ustn::Element::EditElemHandle;

bool operator ==(DPoint3d lhs, DPoint3d rhs) {
    return TRUE == mdlVec_equal(&lhs, &rhs);
};

bool operator !=(DPoint3d lhs, DPoint3d rhs) {
    return !(lhs == rhs);
};

bool operator <(DPoint3d lhs, DPoint3d rhs) {
    if (lhs.x != rhs.x) {
        return lhs.x < rhs.x;
    }
    if (lhs.y != rhs.y) {
        return lhs.y < rhs.y;
    }
    return lhs.z < rhs.z;
};

bool operator >(DPoint3d lhs, DPoint3d rhs) {
    return lhs != rhs && !(lhs < rhs);
};

namespace Openings {

Opening Opening::instance = Opening();

const std::wstring Opening::CATALOG_TYPE_NAME = L"Opening";
const std::wstring Opening::CATALOG_INSTANCE_NAME = L"Opening";
MSWCharCP Opening::LEVEL_NAME = L"C-OPENING-BOUNDARY";
MSWCharCP Opening::LEVEL_SYMBOL_NAME = L"C-OPENING-SYMBOL";

Opening::Opening() {
    origin =
    direction = DVec3d();
    contourRef = NULL;
    contourPoints = std::vector<DPoint3d>();
    contourPoints.reserve(5);
}

Opening::Opening(MSElementDescrP shapeEdP) {
    Opening();

    if (shapeEdP->el.ehdr.type != SHAPE_ELM) {
        return;
    }

    contourRef = shapeEdP->h.elementRef;

    DPoint3d points[MAX_VERTICES];
    int numVerts;
    if (SUCCESS == mdlLinear_extract(points, &numVerts, &shapeEdP->el, ACTIVEMODEL))
    {
        for (int i = 0; i < numVerts; ++i) {
            contourPoints.push_back(points[i]);
        }
    }
    mdlElmdscr_extractNormal(&direction, &origin, shapeEdP, NULL);
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
        contourPoints.size() == other.contourPoints.size() &&        
        std::equal(contourPoints.begin(), contourPoints.end(), 
            other.contourPoints.begin()) &&
        getTask().isThroughHole == other.getTask().isThroughHole;
}

bool Opening::operator !=(Opening other) {
    return !(*this == other);
}

const bool Opening::isValid()
{
    std::set<DPoint3d> points =
        std::set<DPoint3d>(contourPoints.begin(), contourPoints.end());

    return
        points.size() > 2 && !isZero(direction);
}

const bool isZero(DVec3dR vec)
{
    DVec3d zero;
    mdlVec_zero(&zero);

    return TRUE == mdlVec_equal(&vec, &zero);
}

}
