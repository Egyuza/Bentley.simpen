#pragma once

#ifndef OPENING_HELPER_H
#define OPENING_HELPER_H

#include "Opening.h"
#include "ElementHelper.h"

#include <tfform.h>
#include <interface\ElemHandle.h>

namespace Openings {

using Bentley::Ustn::Element::EditElemHandle;

struct OpeningCache
{
    Opening task;
    EditElemHandle bodyP;
    EditElemHandle crossFirstP;
    EditElemHandle crossSecondP;
};

void cmdUpdateAll(char *unparsedP);

StatusInt computeAndDrawTransient(Opening& opening);
StatusInt computeAndAddToModel(Opening& opening);

// TODO назвать получше
StatusInt findDirectionVector(DVec3d& outDirectionVec, double& distance,
    const DPlane3d& contourPlane, const TFFormRecipeList* wallP);

StatusInt computeElementsForOpening(EditElemHandleR outBodyShape,
    EditElemHandleR outPerfoShape, EditElemHandleR outCrossFirst,
    EditElemHandleR outCrossSecond, Opening& opening);

}

#endif // !OPENING_HELPER_H