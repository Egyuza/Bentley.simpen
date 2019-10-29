#pragma once

#ifndef RECTPEN_H
#define RECTPEN_H

#include <vector>

#include <mdl.h>
#include <interface/ElemHandle.h>
#include <tfframe.h>

#include <msinputq.h>
#include <interface/MstnTool.h>
#include <cexpr.h>

struct RectPenTask {
    bool isValid;
    DPoint3d origin;
    DVec3d direction;
    DPoint3d bounds[8];

    double height;
    double width;

    double depth;
    double thickness;

    double flanHeight;
    double flanWidth;

    bool isWallMounted;
    bool isEmpty;

    /*
    �������:

          5______________6
         /|             /|
        4______________7 |
        | 3 - - - - - -|-2
        |/             |/
        0______________1

    */

    // todo ��������� ���������� � cpp

    StatusInt updateByCell(MSElementCP elP);
    void update(RectPenTask& other);

    static RectPenTask getEmpty();

private:
    RectPenTask() {};
};

class RectPen {
public:
    RectPen();
    RectPen(RectPenTask& task);

    bool isValid();

    void redraw(Bentley::Ustn::RedrawElems& redrawTool);
    bool addToModel();
    bool createPenetr(EditElemHandleR eeh,
        EditElemHandleR crossFirst = Bentley::Ustn::Element::EditElemHandle(),
        EditElemHandleR  crossSecond = Bentley::Ustn::Element::EditElemHandle());

    MSElementDescr* penEdP;
    

    bool getDataByPointAndVector(DPoint3dR point, DVec3dR vec,
        /*out*/ EditElemHandleR shapeBody, /*out*/ EditElemHandleR shapePerf,
        /*out*/ EditElemHandleR crossFirst, /*out*/ EditElemHandleR crossSecond,
        /*out*/ double* distanceP);

    DVec3d wallNrms_[2];

private:
    RectPenTask task_;
    bool isWallFound_;
    int dataIndex_;
    
    // ������� ����� ��� ������ �������: front, left, top
    DPoint3d facetsPoints[3][4];
    DVec3d vectors[3];
    double distances[3];    


    
    // ��������� ������ �� 3-� ��������� ��������� ���������� ��������,
    // � ������ ����� �� ���������� ���������� ������������ �����
    bool getDataByIndex(/*in*/int index /*�� 0 �� 2*/, /*in*/ double shell,
        /*out*/ EditElemHandleR shapeBody, /*out*/ EditElemHandleR shapePerf,
        /*out*/ EditElemHandleR crossFirst, /*out*/ EditElemHandleR crossSecond,
        /*out*/ DVec3dR vec,
        /*out*/ double* distanceP);



    // ����� ������� '�����' - FreeForm, 
    // � ������� ����������� ����������� ������� �� ��������
    TFFormRecipeList* findWallByTask();

    // ��������� �������� ����� (Left, Right ������������)
    bool getWallNormals(TFFormRecipeList* wall/*in*/,
        DVec3d* nrm1/*out*/, DVec3d* nrm2/*out*/,
        MSElementDescr* face1, MSElementDescr* face2);



    //bool createPenetrFrame(EditElemHandleR eeh,
    //    EditElemHandleR shapeBody, EditElemHandleR shapePerf, 
    //    EditElemHandleR crossFirst, EditElemHandleR crossSecond,
    //    DVec3dR vec, double height, double shell);
};

extern RectPenTask rectTask;
extern RectPen rectPen;

// CExpressions ----------------------------
// TODO ����� ��� cexpression-���������� ��������� � ��������� RectTask
//extern double rectHeight;
//extern double rectWidth;
//extern double rectDepth;
//extern double rectThickness;
extern char rectKKS[50];
extern char rectDescription[50];

extern bool isByContour;
extern bool isSweepBi;
extern bool isPolicyThrough;

extern double rectContourDistance;
extern std::vector<DPoint3d> contourPoints;

//extern double rectFlanHeight;
//extern double rectFlanThickness;

void publishRectVariables(SymbolSet* symSetP);
double getCExprVal(double cexpr);
void setCExprVal(double* cexpr, double value);

#endif // !RECTPEN_H