#include "CExpression.h"

#include <mscnv.fdf>

namespace CExpr
{

double convertToUOR(double cexpr) {
    double res = 0;
    mdlCnv_masterToUOR(&res, cexpr, ACTIVEMODEL);
    return res;
}
double convertToMaster(double value) {
    double res = 0;
    mdlCnv_UORToMaster(&res, value, ACTIVEMODEL);
    return res;
}

}