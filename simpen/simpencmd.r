#pragma suppressREQCmds
/*--------------------------------------------------------------------------------------+
|
|    $RCSfile: simpencmd.r,v $
|   $Revision: 1.2 $
|       $Date: 2005/08/17 12:28:20 $
|
|  $Copyright: (c) 2005 Bentley Systems, Incorporated. All rights reserved. $
|
+--------------------------------------------------------------------------------------*/
#include <rscdefs.h>
#include <cmdclass.h>

/*----------------------------------------------------------------------+
|                                    |
|   Local Defines                            |
|                                    |
+----------------------------------------------------------------------*/
#define    CT_NONE          0
#define    CT_MAIN          1
#define    CT_PLACE         2
#define    CT_FRAME         3
#define    CT_LOCATE        4
#define    CT_CONSTRUCT     5
#define    CT_DRAW          6
#define    UPDATE        7


Table    CT_MAIN =
{ 
    { 1,  CT_PLACE,    PLACEMENT,    REQ,        "SIMPEN" }, 
};

Table    CT_PLACE =
{
    { 1,    CT_FRAME,       INHERIT,     NONE,     "PLACE" },
    { 3,    CT_NONE,        INHERIT,     NONE,     "TASK" },
    { 4,    CT_NONE,        INHERIT,     NONE,     "PLACEPEN" },
    { 5,    CT_NONE,        INHERIT,     NONE,     "PLACEEMB" },
    { 6,    CT_NONE,        INHERIT,     NONE,     "PENPRIM" },
    { 7,    CT_NONE,        INHERIT,     NONE,      "REPORT"}, 
    { 8,    CT_NONE,        INHERIT,     NONE,     "PLACEOP" },
    { 9,    CT_LOCATE,      INHERIT,     NONE,     "LOCATE" },
    { 2,    CT_NONE,        INHERIT,     NONE,     "ELEM" },
    { 10,   CT_DRAW,        INHERIT,     NONE,     "DRAW" },
    { 11,   CT_CONSTRUCT,   INHERIT,     NONE,     "CONSTRUCT" },
    { 12,   UPDATE,         INHERIT,     NONE,     "UPDATE" },
};

Table    CT_FRAME =
{
    { 1,  CT_NONE,     INHERIT,     DEF,         "Library" },
    { 2,  CT_NONE,     INHERIT,     NONE,         "Orphan" },
    { 3,  CT_NONE,     INHERIT,     NONE,         "PX" },
    { 4,  CT_NONE,     INHERIT,     NONE,         "PY" },
    { 5,  CT_NONE,     INHERIT,     NONE,         "PZ" },
    { 6,  CT_NONE,     INHERIT,     NONE,         "NX" },
    { 7,  CT_NONE,     INHERIT,     NONE,         "NY" },
    { 8,  CT_NONE,     INHERIT,     NONE,         "NZ" },
    { 9,  CT_NONE,     INHERIT,     NONE,         "RECT" },
};

Table    CT_LOCATE =
{
    { 1,  CT_NONE,     INHERIT,     DEF,        "PIPE" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "CONTOUR" },
    { 3,  CT_NONE,     INHERIT,     NONE,       "TASK" },
};

Table    CT_CONSTRUCT =
{
    { 1,  CT_NONE,     INHERIT,     DEF,        "PIPE" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "RECT" },
    { 3,  CT_NONE,     INHERIT,     NONE,       "OPENING" },
};

Table    CT_DRAW =
{
    { 1,  CT_NONE,     INHERIT,     DEF,        "PIPE" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "RECT" },
};

Table    UPDATE =
{
    { 1,  CT_NONE,     INHERIT,     NONE,       "ALL_OPENINGS" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "PREVIEW_OPENING" },
};