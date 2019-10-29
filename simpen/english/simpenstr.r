/*--------------------------------------------------------------------------------------+
|
|    $RCSfile: simpenstr.r,v $
|   $Revision: 1.2 $
|       $Date: 2005/08/17 12:25:52 $
|
|  $Copyright: (c) 2005 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
/*----------------------------------------------------------------------+
|                                    |
|   Include Files                               |
|                                    |
+----------------------------------------------------------------------*/
#include <rscdefs.h>

#include "simpen.h"

MessageList MESSAGELISTID_Commands =
{
    {
    {COMMANDID_PlaceFrameLibrary,        "Place Frame from Library"},
    {COMMANDID_PlaceFrameOrphan,        "Place Frame Orphan"},
    }
};

MessageList MESSAGELISTID_Prompts =
{
    {
    {PROMPTID_IdentifyLinearForm,     "Identify linear form"},
    {PROMPTID_EnterDataPoint,         "Enter data point"},
    }
};

MessageList MESSAGELISTID_Msgs =
{
    {
    {MSGID_LoadCmdTbl,         "Unable to load command table"},
    }
};

