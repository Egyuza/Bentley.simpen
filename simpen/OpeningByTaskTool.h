#pragma once

#ifndef OPENING_BY_TASK_TOOL_H
#define OPENING_BY_TASK_TOOL_H

#include "OpeningTask.h"

#include <msinput.fdf>

#include <mdl.h>
#include <interface/MstnTool.h>

namespace Openings
{

    void cmdLocateTechTask(char *unparsedP);
    void cmdAddToModel(char *unparsedP);
    void cmdUpdatePreview(char *unparsedP);

    class OpeningByTaskTool :
        public Bentley::Ustn::MstnElementSetTool
    {
    public:
        static OpeningByTaskTool* instanceP;
        static OpeningTask userTask;

        // Keyin ------------------------------------
        static void runTool(char *unparsedP);
        static void updatePreview(char *unparsedP);
        static void addToModel(char *unparsedP);
        //-------------------------------------------

    private:
        // unsigned int flagLocateSurfaces;

        bool OnPostLocate(HitPathCP path, char *cantAcceptReason) override;
        EditElemHandleP BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) override;
        void OnComplexDynamics(MstnButtonEventCP ev) override;
        StatusInt OnElementModify(EditElemHandleR elHandle) override;
        bool OnDataButton(MstnButtonEventCP ev) override;

        //bool WantAdditionalLocate(MstnButtonEventCP ev) override;
        //bool WantDynamics() override;
        bool WantAccuSnap() override;

        bool NeedAcceptPoint() override;

        void OnRestartCommand() override;
        void OnPostInstall() override;
        bool OnInstall() override;
        void OnCleanup() override;
    };
}

#endif // !OPENING_BY_TASK_TOOL_H