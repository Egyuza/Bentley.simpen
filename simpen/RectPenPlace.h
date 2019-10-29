#pragma once

#ifndef RECTPEN_PLACE_H
#define RECTPEN_PLACE_H

#include <vector>

#include <mdl.h>
#include <msinputq.h>
#include <interface/MstnTool.h>
#include <MicroStationAPI.h>

#include "ElementHelper.h"

void cmdPlaceRect(char *unparsedP);

class RectPenPlace : public Bentley::Ustn::MstnElementSetTool {
public:
    RectPenPlace();

private:

// MstnElementSetTool: ---------------------------------------------------------
private:
    bool WantAccuSnap() override { return true; }
    // Can check GetHitSource to detect EditAction
    bool NeedPointForDynamics() { return SOURCE_Pick == GetElemSource(); }

    void OnComplexDynamics(MstnButtonEventCP ev) override;

    bool OnResetButton(MstnButtonEventCP ev) override;
    bool OnDataButton(MstnButtonEventCP ev) override;

    StatusInt OnElementModify(EditElemHandleR elHandle) override;

    void OnRestartCommand() override;
    void OnPostInstall() override;
    bool OnInstall() override;

};

#endif // !RECTPEN_PLACE_H