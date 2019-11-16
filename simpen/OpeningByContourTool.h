#pragma once

#ifndef OPENING_BY_CONTOUR_TOOL_H
#define OPENING_BY_CONTOUR_TOOL_H

#include "OpeningTask.h"

#include <msinput.fdf>

#include <mdl.h>
#include <interface/MstnTool.h>

namespace Openings
{

class OpeningByContourTool : 
    public Bentley::Ustn::MstnElementSetTool
{
public:
    static OpeningByContourTool* instanceP;
    static OpeningTask prevTask;

	// Keyin ----------------------------------
	static void run(char *unparsedP);
	static void updatePreview(char *unparsedP);
	static void addToModel(char *unparsedP);
	//-----------------------------------------

private:

	bool isAddToModelProcessActive;
	void clear();

    bool OnPostLocate(HitPathCP path, char *cantAcceptReason) override;
    EditElemHandleP BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) override;
    void OnComplexDynamics(MstnButtonEventCP ev) override;
    StatusInt OnElementModify(EditElemHandleR elHandle) override;
    
    bool WantAdditionalLocate(MstnButtonEventCP ev) override;
    bool WantDynamics() override;
    bool WantAccuSnap() override;
    
    bool NeedAcceptPoint() override;

    void OnRestartCommand() override;
    void OnPostInstall() override;
    bool OnInstall() override;
    void OnCleanup() override;
    
};

}

#endif // !OPENING_BY_CONTOUR_TOOL_H