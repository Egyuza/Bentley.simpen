#pragma once

#ifndef RECTPEN_DRAW_H
#define RECTPEN_DRAW_H

#include <vector>

#include <mdl.h>
#include <msinputq.h>
#include <interface/MstnTool.h>
#include <MicroStationAPI.h>

#include "ElementHelper.h"

//USING_NAMESPACE_BENTLEY;
//USING_NAMESPACE_BENTLEY_USTN;
//USING_NAMESPACE_BENTLEY_USTN_ELEMENT;

enum STEP {
    STEP_START,
    STEP_SHAPE_POINT_NEXT,
    STEP_DEST_POINT
};
STEP operator ++(STEP& step);
STEP operator --(STEP& step);

void cmdDrawRect(char *unparsedP);
void createTransient(EditElemHandleR eeh);
void constructByContour();
bool construct(DVec3d ppVec, double distance);


class RectPenDraw : public Bentley::Ustn::MstnElementSetTool {
public:
    RectPenDraw();
    

private:
    STEP step_; // текущий шаг построения
    //std::vector<DPoint3d> inputs_;
    //EditElemHandle shape_;

    // MstnElementSetTool: ---------------------------------------------------------
private:
    bool WantAccuSnap() override { return true; }
    // Can check GetHitSource to detect EditAction
    bool NeedPointForDynamics() { return SOURCE_Pick == GetElemSource(); }

    void OnComplexDynamics(MstnButtonEventCP ev) override;

    bool OnResetButton(MstnButtonEventCP ev) override;
    bool OnDataButton(MstnButtonEventCP ev) override;

    StatusInt OnElementModify(Bentley::Ustn::Element::EditElemHandle& elHandle) override;
    //bool OnModifierKeyTransition(bool wentDown, int key) override;

    void OnRestartCommand() override;
    void OnPostInstall() override;
    bool OnInstall() override;
    void OnCleanup() override;

};

#endif // !RECTPEN_DRAW_H