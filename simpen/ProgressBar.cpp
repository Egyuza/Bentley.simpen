#include "ProgressBar.h"

namespace ProgressBar {

using namespace Constants;

DialogBox* open(char* title) {
	return mdlDialog_completionBarOpen(title);
}

void update(DialogBox* dialogP, char* text, int percent) {
	mdlDialog_completionBarUpdate(dialogP, text, percent);
}

void close(DialogBox* dialogP) {
	mdlDialog_completionBarClose(dialogP);
}

void openStatus(char* text) {
	DialogBox* dialogP = mdlDialog_find(DIALOGID_CommandStatus, 0);
	mdlDialog_completionBarUpdate(dialogP, text, 0);

	mdlDialog_hookDialogSendUserMsg(dialogP, CMPLBARID_ResetCompletionBar, 0);
	mdlDialog_hookDialogSendUserMsg(dialogP, GENERICID_CompletionBar, 0);
}
void updateStatus(char* text, int percent) {
	DialogBox* dialogP = mdlDialog_find(DIALOGID_CommandStatus, 0);
	mdlDialog_completionBarUpdate(dialogP, text, percent);
	mdlDialog_hookDialogSendUserMsg(dialogP, CMPLBARID_DisplayMessage, 0);
	//mdlDialog_displayCompletionBarMessage(dialogP, text);
}

void closeStatus() {
	DialogBox* dialogP = mdlDialog_find(DIALOGID_CommandStatus, 0);
	mdlDialog_completionBarUpdate(dialogP, "Завершено", 100);
	mdlDialog_completionBarClose(dialogP);
	mdlDialog_hookDialogSendUserMsg(dialogP, -GENERICID_CompletionBar, 0);
}

}
