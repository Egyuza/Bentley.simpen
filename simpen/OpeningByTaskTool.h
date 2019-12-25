#pragma once

#ifndef OPENING_BY_TASK_TOOL_H
#define OPENING_BY_TASK_TOOL_H

#include "OpeningTask.h"
#include "ElementHelper.h"

#include <msinput.fdf>

#include <mdl.h>
#include <interface/MstnTool.h>



namespace Openings
{

class OpeningByTaskTool :
    public Bentley::Ustn::MstnElementSetTool
{
public:
    static OpeningByTaskTool* instanceP;
    static OpeningTask prevTask;

    // Keyin ----------------------------------
    static void run(char *unparsedP);
    static void updatePreview(char *unparsedP);
    static void addToModel(char *unparsedP);
    //-----------------------------------------

private:
    DPoint3d taskOrigin;
    DPoint3d taskBounds[8];
	ElementRef taskRef;
	EditElemHandle contour;

    /* вершины объекта задания:

		вершины через mdlCell_extract:
          5______________6
         /|             /|
        4______________7 |
        | 3 - - - - - -|-2
        |/             |/
        0______________1


		вершины через mdlTFBrep_getVertexLocations:
		  5______________4
		 /|             /|
		2______________3 |
		| 6 - - - - - -|-7
		|/             |/
		1______________0
    */

    bool isValid;
	//ElementRef smartSolidTask;
	bool isTaskIsSmartSolid;
	bool isTaskIsOpening;

    DPlane3d planeFirst;
    DPlane3d planeSecond;

    DPoint3d contourOrigin;
    DVec3d heightVec;
    DVec3d widthVec;
    DVec3d depthVec;

	bool isAddToModelProcessActive;

	void clear();

    bool OnPostLocate(HitPathCP path, char *cantAcceptReason) override;
    EditElemHandleP BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) override;
    void OnComplexDynamics(MstnButtonEventCP ev) override;
    StatusInt OnElementModify(EditElemHandleR elHandle) override;
    bool OnDataButton(MstnButtonEventCP ev) override;

    bool WantAccuSnap() override;
    bool NeedAcceptPoint() override;

    void OnRestartCommand() override;
    bool OnResetButton(MstnButtonEventCP ev) override;
    void OnPostInstall() override;
    bool OnInstall() override;
    void OnCleanup() override;
};

}

#endif // !OPENING_BY_TASK_TOOL_H