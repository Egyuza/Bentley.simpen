#pragma once

#include "Opening.h"

#ifndef OPENING_RECT_H
#define OPENING_RECT_H

namespace Openings {
    
class OpeningRect: Opening
{
    double height;
    double width;

    DVec3d heightVec;
    DVec3d widthVec;

    
    
    OpeningRect();

};

}


#endif // !OPENING_RECT_H