#pragma once

#ifndef RECTPEN_LOCATE_H
#define RECTPEN_LOCATE_H

#include "RectPen.h"

#include <mdl.h>
#include <msinputq.h>
#include <interface/MstnTool.h>

#include <IModel/xmlinstanceapi.h> 
#include <IModel/xmlinstanceschemamanager.h> 
#include <mdlxmltools.h>
//#include <MicroStationAPI.h>

extern RectPen rectPen;

// CMD_SIMPEN_LOCATE_RECT
void cmdLocateRect(char *unparsedP); 
void constructByTask();

class RectPenLocate : public Bentley::Ustn::MstnElementSetTool {
public:
    RectPenLocate::RectPenLocate();
    static RectPenLocate* instanceP;
private:
    
    static MSElementP eP;

    // unsigned int flagLocateSurfaces;
        
    bool OnPostLocate(HitPathCP path, char *cantAcceptReason) override;
    EditElemHandleP BuildLocateAgenda(HitPathCP path, MstnButtonEventCP ev) override;
    void OnComplexDynamics(MstnButtonEventCP ev) override;
    StatusInt OnElementModify(EditElemHandleR elHandle) override;
    
    bool WantAccuSnap() override;
    bool NeedPointForDynamics() override;
    bool NeedAcceptPoint() override;
    
    void OnRestartCommand() override;
    void OnPostInstall() override;
    bool OnInstall() override;
    void OnCleanup() override;
};

#endif // !RECTPEN_LOCATE_H