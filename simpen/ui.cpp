#include "ui.h"

#include <msinput.fdf>
#include <msoutput.fdf>

namespace Openings
{

namespace UI
{
	const char* keinPref = "mdl keyin simpen.ui simpen.ui opening";

	void sendTaskDataSynch() {
		char cmd[256];
		sprintf(cmd, "%s sendTaskData", keinPref);
		mdlInput_sendSynchronizedKeyin(cmd, 0, 0, 0);
	}

	void readDataSynch() {
		char cmd[256];
		sprintf(cmd, "%s readData", keinPref);
		mdlInput_sendSynchronizedKeyin(cmd, 0, 0, 0);
	}

	void reload() {
		char cmd[256];
		sprintf(cmd, "%s reload", keinPref);
		mdlInput_sendSynchronizedKeyin(cmd, 0, 0, 0);
	}

	void setEnableAddToModel() {
		char cmd[256];
		sprintf(cmd, "%s enableAddToModel", keinPref);
		mdlInput_sendSynchronizedKeyin(cmd, 0, 0, 0);
	}

	void setDGDataSynch_KKS(int filePos, const char* kks) {
		char cmd[256];
		sprintf(cmd, "%s setdgdata  %u %s", keinPref, filePos, kks);
		mdlInput_sendSynchronizedKeyin(cmd, 0, 0, 0);
	}

	void prompt(char* pMessage) {
		mdlOutput_prompt(pMessage);
	}

	void promptU(char* pMessage) {
		mdlOutput_promptU(pMessage);
	}

	void warning(const char* pMessage, bool openDialog) {
		mdlOutput_messageCenter(MESSAGE_WARNING, pMessage, "", openDialog);
	}

	void error(const char* pMessage, bool openDialog) {
		mdlOutput_messageCenter(MESSAGE_ERROR, pMessage, "", openDialog);
	}
}

}
