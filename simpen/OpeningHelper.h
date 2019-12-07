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

StatusInt computeAndDrawTransient(Opening& opening);
StatusInt computeAndAddToModel(Opening& opening, 
	bool rewritePrevious = false, MSElementDescr* previousP = NULL);

// TODO назвать получше
StatusInt findDirectionVector(DVec3d& outDirectionVec, double& distance,
    const DPlane3d& contourPlane, const TFFormRecipeList* wallP);

StatusInt findDirVecFromArcWall(DVec3d& outDirectionVec, double& distance,
	const DPlane3d& contourPlane, const DPoint3d contourVertex,
	const ElementRef wallRef, DgnModelRefP modelRefP);


StatusInt computeElementsForOpening(EditElemHandleR outBodyShape,
    EditElemHandleR outPerfoShape, EditElemHandleR outCrossFirst,
    EditElemHandleR outCrossSecond, Opening& opening,
	MSWCharCP boundaryLevel, MSWCharCP symbolLevel);

void LocateFunc_providePathDescription
(
	DisplayPathP    path,           /* => display path */
	MSWCharP        description,    /* <=> description */
	MSWCharCP       refStr          /* => Ref string */
);

bool isOpening(MSElementDescr* edP);

void cmdUpdateAll(char *unparsedP);

bool updateOpeningR(MSElementDescrP edP);

int scanOpenings(
	MSElementDescr* edP,
	void*  param,
	ScanCriteria *pScanCriteria
);

}

#endif // !OPENING_HELPER_H