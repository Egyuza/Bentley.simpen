/*----------------------------------------------------------------------+
|									|
|   Include Files   							|
|									|
+----------------------------------------------------------------------*/
#include <rscdefs.h>
#include <cmdclass.h>


#define	CT_NONE		 0
#define CT_PENETR	 1
#define CT_CMDS		2
#define CT_PLACE		3
#define CT_PLDROP		4



Table CT_PENETR =
{
    {  1, CT_CMDS, PARAMETERS, CMDSTR(1), "PEN"},
};


Table CT_CMDS =
{
    //{  1, CT_NONE, INHERIT, NONE, "DIALOG"},
    //{  2, CT_NONE, INHERIT, NONE, "ABOUT"},
    {  3, CT_PLACE, INHERIT, NONE, "PLACE"},
    //{  3, CT_PLACE, INHERIT, NONE, "PLDROP"},
};


Table CT_PLACE =
{
    {  1, CT_NONE, INHERIT, NONE, "PX"},
    {  2, CT_NONE, INHERIT, NONE, "PY"},
    {  3, CT_NONE, INHERIT, NONE, "PZ"},
    {  4, CT_NONE, INHERIT, NONE, "NX"},
    {  5, CT_NONE, INHERIT, NONE, "NY"},
    {  6, CT_NONE, INHERIT, NONE, "NZ"},
    //{  7, CT_NONE, INHERIT, NONE, "VIEW"},
};
