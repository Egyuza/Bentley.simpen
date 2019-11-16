#pragma once

#ifndef OPENINGS_UI_H
#define OPENINGS_UI_H

namespace Openings
{

namespace UI
{
	void sendTaskDataSynch();
	void readDataSynch();
	void reload();

	void setEnableAddToModel();

	void prompt(char* pMessage);
	void promptU(char* pMessage);
	void warning(const char* pMessage, bool openDialog = false);
	void error(const char* pMessage, bool openDialog = false);
}

}

#endif // OPENINGS_UI_H