#pragma once

#ifndef CONTOUR_OPENING_TOOL_H
#define CONTOUR_OPENING_TOOL_H

#include "OpeningTask.h"

#include <msinput.fdf>

#include <mdl.h>
#include <interface/MstnTool.h>

namespace Openings
{

void cmdLocateContour(char *unparsedP);
void cmdAddToModel(char *unparsedP);
void cmdUpdatePreview(char *unparsedP);

class ContourOpeningTool : 
    public Bentley::Ustn::MstnElementSetTool
{
public:
    static ContourOpeningTool* instanceP;
    static OpeningTask cachedTask;

private:
    // unsigned int flagLocateSurfaces;

    bool OnPostLocate(HitPathCP path, char *cantAcceptReason) override;
    EditElemHandleP BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) override;
    void OnComplexDynamics(MstnButtonEventCP ev) override;
    StatusInt OnElementModify(EditElemHandleR elHandle) override;
    
    bool WantAccuSnap() override;
    bool NeedPointForDynamics() override; // Can check GetHitSource to detect EditAction
    bool NeedAcceptPoint() override;

    void OnRestartCommand() override;
    void OnPostInstall() override;
    bool OnInstall() override;
    void OnCleanup() override;    
};

}

#endif // !CONTOUR_OPENING_TOOL_H