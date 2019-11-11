#pragma once

#ifndef OPENING_TASK_H
#define OPENING_TASK_H

//#include <mdl.h>
#include <elementref.h>
#include <cexpr.h>

namespace Openings {

struct OpeningTask {
    // �������������� �������� ���������� �������������� �� UOR ������!
    // -----------
    double height;
    double width;
    double depth;
    // -----------
    char kks[50];

    bool isThroughHole;
    bool isRequiredRemoveContour;
    bool isReadyToPublish;

    ElementRef tfFormRef;
    bool isContourSelected;
    bool isTFFormSelected;
    bool isTaskSelected;

    OpeningTask();
    
    static OpeningTask& getInstance();
    static const void publishCExpressions(SymbolSet* symSetP);
    
    bool operator ==(OpeningTask other);
    bool operator !=(OpeningTask other);

    

private:
    static OpeningTask instance_;
};

double convertFromCExprVal(double cexpr);
void convertToCExprVal(double* cexpr, double value);

}

#endif // !OPENING_TASK_H