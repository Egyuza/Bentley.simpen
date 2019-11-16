#include "ui.h"

#include <msinput.fdf>
#include <msoutput.fdf>

namespace Openings
{

namespace UI
{
	void sendTaskDataSynch() {
		mdlInput_sendSynchronizedKeyin(
			"mdl keyin simpen.ui simpen.ui sendTaskData", 0, 0, 0);
	}

	void readDataSynch() {
		mdlInput_sendSynchronizedKeyin(
			"mdl keyin simpen.ui simpen.ui readData", 0, 0, 0);
	}

	void reload() {
		mdlInput_sendKeyin("mdl keyin simpen.ui simpen.ui reload", 0, 0, 0);
	}

	void setEnableAddToModel() {
		mdlInput_sendKeyin(
			"mdl keyin simpen.ui simpen.ui enableAddToModel", 0, 0, 0);
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
