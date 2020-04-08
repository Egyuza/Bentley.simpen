#pragma once

#ifndef PROGRESS_BAR_H
#define PROGRESS_BAR_H

#include <msdialog.fdf>


namespace ProgressBar {

namespace Constants
{
	const int DIALOGID_CompletionBar = (-106);
	const int BASEID_CompletionBar = (DIALOGID_CompletionBar * 100);
	
	const int CMPLBARID_UpdateCompletionBar = (BASEID_CompletionBar - 1);
	const int CMPLBARID_DisplayMessage = (BASEID_CompletionBar - 2);
	const int CMPLBARID_ResetCompletionBar = (BASEID_CompletionBar - 3);
	const int CMPLBARID_CloseCompletionBar = (BASEID_CompletionBar - 4);
	
	const int DIALOGID_CommandStatus = (-400);
	const int GENERICID_CompletionBar = (BASEID_CompletionBar - 1);
}


/* пример на vb6.0
Type CompletionBarInfo
	percentComplete As Long
	msgText As Long
End Type

Private Sub ResumeWindowsEvents()
	Dim iEvents As Integer
	iEvents = DoEvents()
End Sub

Sub Test_ProgressCommand()
	Dim lDialogBox As Long

	lDialogBox = mdlDialog_find(DIALOGID_CommandStatus, 0)

	Dim data As CompletionBarInfo
	Call mdlDialog_completionBarUpdate(lDialogBox, "", 0)
	data.msgText = 0
	data.percentComplete = 0
	Call mdlDialog_hookDialogSendUserMsg(lDialogBox, CMPLBARID_ResetCompletionBar, 0)
	Call mdlDialog_hookDialogSendUserMsg(lDialogBox, GENERICID_CompletionBar, 0)

	Call ResumeWindowsEvents
	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 1", 0)

	Call ResumeWindowsEvents
	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 2", 80)

	Call ResumeWindowsEvents
	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 3", 100)


	Call mdlDialog_hookDialogSendUserMsg(lDialogBox, -GENERICID_CompletionBar, 0)
	Call ResumeWindowsEvents
End Sub

Sub Test_ProgessDialog()
	Dim lDialogBox As Long

	lDialogBox = mdlDialog_completionBarOpen("My Progress")

	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 1", 0)
	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 1", 0)

	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 2", 80)
	Call mdlDialog_completionBarUpdate(lDialogBox, "Update 3", 100)

	Call mdlDialog_completionBarClose(lDialogBox)
End Sub
*/

DialogBox* open(char* title);
void update(DialogBox* dialogP, char* text, int percent);
void close(DialogBox* dialogP);

void openStatus(char* text);
void updateStatus(char* text, int percent);
void closeStatus();

}

#endif // !PROGRESS_BAR_H