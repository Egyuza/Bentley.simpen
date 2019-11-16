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
#define    CT_CMD			2
#define    CT_OPENINGS		3
#define    CT_LOCATE		4
#define    CT_UPDATE		5

#define    CT_FRAME         6


Table    CT_MAIN =
{ 
    { 1,  CT_CMD,    PLACEMENT,    REQ,        "SIMPEN" }, 
};

Table    CT_CMD =
{
    { 1,    CT_OPENINGS,    INHERIT,     NONE,     "OPENINGS" },
    { 2,    CT_FRAME,       INHERIT,     NONE,     "PLACE" },
    { 3,    CT_NONE,        INHERIT,     NONE,     "TASK" },
    { 4,    CT_NONE,        INHERIT,     NONE,     "PLACEPEN" },
    { 5,    CT_NONE,        INHERIT,     NONE,     "PLACEEMB" },
    { 6,    CT_NONE,        INHERIT,     NONE,     "PENPRIM" },
    { 7,    CT_NONE,        INHERIT,     NONE,     "REPORT"}, 
    { 8,    CT_NONE,        INHERIT,     NONE,     "PLACEOP" },
    { 9,    CT_NONE,        INHERIT,     NONE,     "ELEM" },
};


Table    CT_OPENINGS =
{
    { 1,    CT_LOCATE,		INHERIT,     NONE,		"LOCATE" },
    { 2,    CT_NONE,        INHERIT,     NONE,		"ADD" },
	{ 3,    CT_UPDATE,      INHERIT,     NONE,		"UPDATE" },
};

Table    CT_LOCATE =
{
    { 1,  CT_NONE,     INHERIT,     NONE,       "TASK" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "CONTOUR" },
};

Table    CT_UPDATE =
{
    { 1,  CT_NONE,     INHERIT,     NONE,       "ALL" },
    { 2,  CT_NONE,     INHERIT,     NONE,       "PREVIEW" },
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
};
