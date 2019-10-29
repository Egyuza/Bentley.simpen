/*--------------------------------------------------------------------------------------+
|
|    $RCSfile: simpen.r,v $
|   $Revision: 1.2 $
|       $Date: 2005/08/17 12:27:59 $
|
|  $Copyright: (c) 2005 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
/*----------------------------------------------------------------------+
|                                    |
|   Include Files                               |
|                                    |
+----------------------------------------------------------------------*/
#include <dlogbox.h>
#include <dlogids.h>
#include <cmdlist.h>

#include <keys.h>

#include "simpen.h"
#include "simpencmd.h"

#include "simpentxt.h"

#define X1    1.0
#define Y1    6

/*-----------------------------------------------------------------------
 Setup for native code only MDL app
-----------------------------------------------------------------------*/
#define  DLLAPP_PRIMARY     1

DllMdlApp   DLLAPP_PRIMARY =
    {
    "SIMPEN", "simpen"
    }

DItem_IconCmdRsc ICMDID_placeFrame =
{
    NOHELP, OHELPTASKIDCMD, 0,
    CMD_SIMPEN_PLACE, OTASKID, "", "",
    {
        /* Tool Settings in cmdItemListRsc */
    }
}

extendedAttributes
{{
   {EXTATTR_FLYTEXT, FLYOVER_PlaceFrame},
   {EXTATTR_BALLOON, BALLOON_PlaceFrame},
}};

CmdItemListRsc CMD_SIMPEN_PLACE =
    {{
    {{X1,  Y1,    X1*75,   0}, Text, TEXTID_cellName, ON, 0, "", ""},
    }};

CmdItemListRsc CMD_SIMPEN_PLACEPEN =
    {{
{{0.00*XC,0.5*YC,0,0}, Text, 82, ON, 0, "", ""},
{{0.00*XC,2.0*YC,0,0}, Text, 81, ON, 0, "", ""},
{{0.00*XC,3.5*YC,0,0}, Text, 71, ON, 0, "", ""},
{{10.0*XC,3.5*YC,0,0},        ToggleButton, 71, ON, 0, "", ""},
{{0.00*XC,5.0*YC,0,0}, Text, 72, ON, 0, "", ""},
{{10.0*XC,5.0*YC,0,0},        ToggleButton, 72, ON, 0, "", ""},
{{0.00*XC,6.5*YC,0,0}, Text, 73, ON, 0, "", ""},
{{10.0*XC,6.5*YC,0,0},        ToggleButton, 73, ON, 0, "", ""},
    }};

CmdItemListRsc CMD_SIMPEN_PLACEEMB =
    {{
{{0.00*XC,0.5*YC,0,0}, Text, 82, ON, 0, "", ""},
{{0.00*XC,2.0*YC,0,0}, Text, 81, ON, 0, "", ""},
{{0.00*XC,3.5*YC,0,0}, Text, 71, ON, 0, "", ""},
{{10.0*XC,3.5*YC,0,0},        ToggleButton, 71, ON, 0, "", ""},
{{0.00*XC,5.0*YC,0,0}, Text, 72, ON, 0, "", ""},
{{10.0*XC,5.0*YC,0,0},        ToggleButton, 72, ON, 0, "", ""},
{{0.00*XC,6.5*YC,0,0}, Text, 73, ON, 0, "", ""},
{{10.0*XC,6.5*YC,0,0},        ToggleButton, 73, ON, 0, "", ""},
    }};
    
CmdItemListRsc CMD_SIMPEN_TASK =
    {{
{{0.00*XC,0.5*YC,0,0}, Text, 82, ON, 0, "", ""},
{{0.00*XC,2.0*YC,0,0}, Text, 81, ON, 0, "", ""},
{{0.00*XC,3.5*YC,0,0}, Text, 71, ON, 0, "", ""},
{{10.0*XC,3.5*YC,0,0},        ToggleButton, 71, ON, 0, "", ""},
{{0.00*XC,5.0*YC,0,0}, Text, 72, ON, 0, "", ""},
{{10.0*XC,5.0*YC,0,0},        ToggleButton, 72, ON, 0, "", ""},
{{0.00*XC,6.5*YC,0,0}, Text, 73, ON, 0, "", ""},
{{10.0*XC,6.5*YC,0,0},        ToggleButton, 73, ON, 0, "", ""},
    }};
    

/*
CmdItemListRsc CMD_SIMPEN_NOTE =
    {{
{{5.0*XC,0.5*YC,0,0}, ComboBox, COMBO_PartSignUp, ON, 0, "", ""},
{{5.0*XC,2.0*YC,0,0}, ComboBox, COMBO_PartSignDn, ON, 0, "", ""},
{{5.0*XC,3.5*YC,0,0}, OptionButton, OPTIONBUTTONID_NoteStyle, ON, 0, "", ""},
//{{5.0*XC,5.0*YC,15.0*XC,0}, ComboBox, COMBO_Levels, ON, 0, "", ""}, // C-ANNO-PART
    }};
*/


#undef X1
#undef Y1

DItem_TextRsc TEXTID_cellName =
{
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKID_txCellName, NOARG,
    MAX_CELLNAME_BYTES-1, "%s", "%s", "", "", NOMASK, NOCONCAT,
    TXT_cellName,
    "g_frameDataGUI.cellName"
};




DItem_TextRsc 81 =
    {
        NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
        NOHOOK, NOARG,
        20, "%s", "%s", "", "", NOMASK, TEXT_ALWAYSBEVELED,
        "Code",
        "scode"
    };

DItem_TextRsc 82 =
    {
        NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
        NOHOOK, NOARG,
        20, "%s", "%s", "", "", NOMASK, TEXT_ALWAYSBEVELED,
        "Name",
        "sname"
    };


DItem_TextRsc 71 = 
    {
        NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
        NOHOOK, NOARG, 
        8, "%ld", "%ld", "", "", NOMASK, TEXT_ALWAYSBEVELED, 
        "Коорд X",
        "coordx"
    };
    
DItem_ToggleButtonRsc 71 =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP,
    NOHOOK, NOARG, NOMASK, NOINVERT,
    "",
    "icoordx"
    };
    

DItem_TextRsc 72 = 
    {
        NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
        NOHOOK, NOARG, 
        8, "%ld", "%ld", "", "", NOMASK, TEXT_ALWAYSBEVELED, 
        "Коорд Y",
        "coordy"
    };
    
DItem_ToggleButtonRsc 72 =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP,
    NOHOOK, NOARG, NOMASK, NOINVERT,
    "",
    "icoordy"
    };
    

DItem_TextRsc 73 = 
    {
        NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
        NOHOOK, NOARG, 
        8, "%ld", "%ld", "", "", NOMASK, TEXT_ALWAYSBEVELED, 
        "Коорд Z",
        "coordz"
    };
    
DItem_ToggleButtonRsc 73 =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP,
    NOHOOK, NOARG, NOMASK, NOINVERT,
    "",
    "icoordz"
    };
    

/*
 DItem_ComboBoxRsc COMBO_PartSignUp =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
    HOOKID_NOTECOMBO, NOARG,
    20, "", "", "", "", NOMASK,
    0, 5, 1, 0, 0, 
    COMBOATTR_AUTOADDNEWSTRINGS, // | COMBOATTR_SORT | COMBOATTR_LABELABOVE, 
    "вверху",
    "signup",
{
{20*XC, 20, ALIGN_LEFT, ""},
}
    };
    
 DItem_ComboBoxRsc COMBO_PartSignDn =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
    HOOKID_NOTECOMBO, NOARG,
    20, "", "", "", "", NOMASK,
    0, 5, 1, 0, 0, 
    COMBOATTR_AUTOADDNEWSTRINGS, // | COMBOATTR_SORT | COMBOATTR_LABELABOVE, 
    "внизу",
    "signdn",
{
{20*XC, 20, ALIGN_LEFT, ""},
}
    };
    
 DItem_ComboBoxRsc COMBO_Levels =
    {
    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, 
    HOOKID_LEVELCOMBO, NOARG,
    20, "", "", "", "", NOMASK,
    0, 5, 1, 0, 0, 
    COMBOATTR_READONLY, // | COMBOATTR_SORT | COMBOATTR_LABELABOVE, 
    "слой",
    "sNoteLevel",
{
{20*XC, 20, ALIGN_LEFT, ""},
}
    };
*/