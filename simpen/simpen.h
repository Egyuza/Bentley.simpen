#pragma once

#ifndef SIMPEN_H
#define SIMPEN_H

#include <msdefs.h>

#define DEBUG

/*----------------------------------------------------------------------+
|                                    |
|   Local defines                            |
|                                    |
+----------------------------------------------------------------------*/

/*----------------------------------------------------------------------+
|                                    |
|   Message list IDs                            |
|                                    |
+----------------------------------------------------------------------*/
#define    MESSAGELISTID_Commands              0
#define    MESSAGELISTID_Prompts               1
#define    MESSAGELISTID_Msgs                  2

/*----------------------------------------------------------------------+
|                                    |
|   Prompt IDs - used in the Message list definition for command prompts|
|                                    |
+----------------------------------------------------------------------*/
#define    PROMPTID_IdentifyLinearForm         1
#define PROMPTID_EnterDataPoint             2
/*----------------------------------------------------------------------+
|                                    |
|   Command IDs - used in the Message list definition for command names    |
|                                    |
+----------------------------------------------------------------------*/
#define    COMMANDID_PlaceFrameLibrary        1
#define    COMMANDID_PlaceFrameOrphan        2

/*----------------------------------------------------------------------+
|                                    |
|   Message IDs - used in the Message list definition for messages    |
|                                    |
+----------------------------------------------------------------------*/
#define    MSGID_LoadCmdTbl                1

/*----------------------------------------------------------------------+
|                                    |
|   Dialog Item Hook ID's                              |
|                                    |
+----------------------------------------------------------------------*/
#define HOOKID_txCellName               1

/*----------------------------------------------------------------------+
|                                    |
|   Icon Command Items                                |
|                                    |
+----------------------------------------------------------------------*/
#define ICMDID_placeFrame               1

/*----------------------------------------------------------------------+
|                                    |
|   Text Items                                   |
|                                    |
+----------------------------------------------------------------------*/
#define TEXTID_cellName              1
/*----------------------------------------------------------------------+
|                                    |
|   Other Items                             |
|                                    |
+----------------------------------------------------------------------*/

#define OPTIONBUTTONID_NoteStyle            1


#define COMBO_Levels                1
#define COMBO_PartSignUp            2
#define COMBO_PartSignDn            3

/*----------------------------------------------------------------------+
|                                    |
|   Local type definitions                        |
|                                    |
+----------------------------------------------------------------------*/
typedef struct framedatagui {
    char     cellName[MAX_CELLNAME_BYTES];
} FrameDataGUI;

#endif // !SIMPEN_H
