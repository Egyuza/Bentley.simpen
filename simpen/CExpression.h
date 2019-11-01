#pragma once

#ifndef CEXPRESSION_H
#define CEXPRESSION_H

namespace CExpr
{

namespace Opening
{
    const static char* HEIGHT = "openingHeight";
    const static char* WIDTH = "openingWidth";
    const static char* DISTANCE = "openingDistance";
    const static char* KKS = "openingKKS";
    const static char* IS_THROUGH_HOLE = "openingIsThroughHole";
    const static char* IS_REQUIRED_REMOVE_CONTOUR = "openingIsRequiredRemoveContour";
    const static char* IS_READY_TO_PUBLISH = "openingIsReadyToPublish";
}

double convertToUOR(double cexpr);
double convertToMaster(double value);

}

#endif // !CEXPRESSION_H
