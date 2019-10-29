#include    <mdl.h>	
#include    <mssystem.fdf>
#include    <msselect.fdf>
#include    <msstate.fdf>
#include    <msmisc.fdf>
#include    <msinput.fdf>
#include    <mskisolid.fdf>
#include    <mscnv.fdf>
#include    <mscurrtr.fdf>
#include    <mstmatrx.fdf>
#include    <msrmatrx.fdf>
#include    <mselmdsc.fdf>
#include    <mselemen.fdf>
#include    <msparse.fdf>
#include    <mscell.fdf>
#include    <msoutput.fdf>
#include    <msacs.fdf>
#include    <msvba.fdf>
#include    <mscexpr.fdf>
#include    <ditemlib.fdf>
#include    <msvec.fdf>
#include    <mdllib.fdf>
#include    <string.h>
#include    <elementref.h>

#include "pencmd.h"

// for manual placing
#define MODE_PEN      1
#define MODE_OPENING  2
#define MODE_RECT     3

//  for placing by message
#define TYPE_ROUND    1
#define TYPE_RECT     2
#define TYPE_OPENING  3
#define TYPE_CELL     4
#define TYPE_INDENT   5

#define MAX_PARAMS	  22

// double and float comparison
#define DBL_EPSILON     2.2204460492503131e-016 /* smallest such that 1.0+DBL_EPSILON != 1.0 */
#define FLT_EPSILON     1.192092896e-07        /* smallest such that 1.0+FLT_EPSILON != 1.0 */
#define EQ(x,v) (((v - FLT_EPSILON) < x) && (x <( v + FLT_EPSILON)))


int mdlGlobalError;


int values[3]; // penetration params
double penparams[MAX_PARAMS];

ULong lvlF = 0;
ULong lvlP = 0;

int iDirection = 0;
int iMode = 0;
int iFlanges = 0;
int iByMessage = 0;
int iPenType = 0;
int iBlick = 0;
int iJust = 1;

double dFlanDiam = 0.;
double dPipeDiam = 0.;
double dPipeThick = 0.;

double dRectWidth = 0.;
double dRectHeight = 0.;
double dRectThick = 0.;
double dFlanWidth = 0.;

double dFlanThick = 0.;
double dWallThick = 0.;


MSElementDescr *pen = NULL; 
MSElementDescr *penBuf = NULL; 

MSElementUnion el;

MSElementUnion elBlick[3];


///////////////////////////////
Private int getParamsFromString(
char* string, 
double* values, 
int count
)
{
	
	char* s = NULL;
	char* ss = NULL;
	char* s0 = NULL;
	int i;
	int ret = 0;

	if (*string == 0) return 0; // no value

	s0 = (char*)malloc(strlen(string)+1);

	if (s0 == NULL) return 0;

	strncpy(s0, string, strlen(string)+1);

	ss = s0;


	for(i = 0; i < MAX_PARAMS; i++)
	{
		s = ss;

		ss = strchr(s, ';');

		if (ss != NULL) *ss = 0;

		if (i == 0) ret = atoi(s); // возвращаемое количество фланцев
		else if (i == 18) iPenType = atoi(s); // тип проходки
		else values[i] = atof(s); // значение параметра

		ss++;
	}

	free(s0);


	return ret;

}


///////////////////////
Private int getValuesFromString(
char* string, 
int* values, 
int count
)
{
	
	char* s = NULL;
	char* ss = NULL;
	char* s0 = NULL;
	char* s1 = NULL;
	int i;

	char iDelim;


	if (*string == 0) return 0; // no value

	// check and set delimeter
	s1 = strchr(string, '-');
	if (s1 == NULL)
	{
		s1 = strchr(string, 'x');
		if (s1 == NULL) 
			return 0; 
		else 
		{
			iDelim = 'x';
		}
	}
	else
	{
		iDelim = '-';
	}

	// allocate
	s0 = (char*)malloc(strlen(string)+1);
	if (s0 == NULL) return 0;

	// copy to new allocated
	strncpy(s0, string, strlen(string)+1);

	ss = s0;


	for(i = 0; i < count; i++)
	{
		s = ss;

		ss = strchr(s, iDelim);
		if (ss == NULL) 
		{
			values[i] = atoi(s);
			if (i == 2) break;
			break;
		}
		else
		{
			*ss = 0;
			values[i] = atoi(s);
			if (i == 2) break;
			ss++;
		}
	}

	free(s0);

	return i+1;

}

//////////////////////////////////
Private MSElementDescr* makeTube2(
double dInsideRadius,
double dOutsideRadius,
double dHeight,
double dOffset,
UInt32 iLevID
)
{

		DPoint3d       base;
		DPoint3d       vec;
		DPoint3d       nrm;
		RotMatrix rm;
		Transform tm;
		double shell;

		KIBODY  *kb_shape  = NULL;

		MSElementDescr* edp = NULL;

		int ret;

		memset(&base, 0, sizeof(DPoint3d));
		memset(&vec, 0, sizeof(DPoint3d));
		memset(&nrm, 0, sizeof(DPoint3d));

		shell = dOutsideRadius-dInsideRadius;


		base.z =  dOffset;
		//base.z = (iDirection / abs(iDirection)) * dOffset;
		vec.z = 1.;


		if (abs(iDirection) == 1) 
		{
			nrm.x = (iDirection / abs(iDirection)) * 1.;
		}
		else if (abs(iDirection) == 2) 
		{
			nrm.y = (iDirection / abs(iDirection)) * 1.;
		}
		else
		{
			nrm.z = (iDirection / abs(iDirection)) * 1.;
		}


		mdlRMatrix_fromNormalVector(&rm, &nrm);
		mdlRMatrix_getInverse(&rm, &rm);
		mdlTMatrix_fromRMatrix(&tm, &rm);

		mdlEllipse_create (&el, NULL, &base, dOutsideRadius, dOutsideRadius, NULL, 0);
		mdlElmdscr_new (&edp, NULL, &el);
		//mdlElmdscr_appendElement (edp, &el);

		mdlKISolid_beginCurrTrans (MASTERFILE);

		mdlCurrTrans_invScaleDoubleArray  (&dHeight, &dHeight, 1);
		mdlCurrTrans_invScaleDoubleArray  (&shell, &shell, 1);

		ret = mdlKISolid_elementToBody(&kb_shape, edp, MASTERFILE);

		ret = mdlKISolid_sweepBodyVector(kb_shape, &vec, dHeight, -shell, 0.);

		ret = mdlKISolid_bodyToElement (&edp, kb_shape, TRUE, -1, NULL, MASTERFILE);

		if (iLevID) mdlElmdscr_setProperties(edp, &iLevID, 0, 0, 0, 0, 0, 0, 0);

		mdlElmdscr_transform(edp, &tm);

		mdlKISolid_freeBody (kb_shape);

		mdlKISolid_endCurrTrans ();

		return edp;


}

//////////////////////////////////
Private MSElementDescr* makeTube( // NOT USING
double dInsideRadius,
double dOutsideRadius,
double dHeight,
double dOffset
)
{

		MSElement       cone1;
		MSElement       cone2;

		
		KIBODY          *bodyP1         = NULL;
		KIBODY          *bodyP2         = NULL; 
		
		KIENTITY_LIST   *list1  = NULL; /* Cutting List (holds BodyP2) */
		KIENTITY_LIST   *list2  = NULL; /* List of bodies after cut ) */
		
		DPoint3d       base;
		DPoint3d       top;   
		
		MSElementDescr *edp1 = NULL; 
		MSElementDescr *edp2 = NULL; 
		MSElementDescr *edp3 = NULL; 
		
		memset(&base, 0, sizeof(DPoint3d));
		memset(&top, 0, sizeof(DPoint3d));
		
		//printf("%f %f %f %f\n", dInsideRadius, dOutsideRadius, dHeight, dOffset);

		if (abs(iDirection) == 1) 
		{
			base.x = (iDirection / abs(iDirection)) * dOffset; // (iDirection / iDirection) returns 1 or -1
			top.x = (iDirection / abs(iDirection)) * (dOffset + dHeight);
			//printf("%i  %f  %f\n", (iDirection / abs(iDirection)), base.x, top.x);
		}
		else if (abs(iDirection) == 2) 
		{
			base.y = (iDirection / abs(iDirection)) * dOffset;
			top.y = (iDirection / abs(iDirection)) * (dOffset + dHeight);
			//printf("%i  %f  %f\n", (iDirection / abs(iDirection)), base.y, top.y);
		}
		else
		{
			base.z = (iDirection / abs(iDirection)) * dOffset;
			top.z = (iDirection / abs(iDirection)) * (dOffset + dHeight);
			//printf("%i  %f  %f\n", (iDirection / abs(iDirection)), base.z, top.z);
		}

		//printf("%f %f %f %f\n", dInsideRadius, dOutsideRadius, dHeight, dOffset);
		
		mdlCone_createRightCylinder(&cone1, NULL, dOutsideRadius, &base, &top);
		mdlCone_createRightCylinder(&cone2, NULL, dInsideRadius, &base, &top);
		
		//mdlElmdscr_fromCone(&edp1, &cone1, NULL);
		//mdlElmdscr_fromCone(&edp2, &cone2, NULL);
		
		mdlElmdscr_new (&edp1, NULL, &cone1);
		mdlElmdscr_new (&edp2, NULL, &cone2);
		
		
		/* Convert solid 1 element to a KIBODY */
		mdlKISolid_elementToBody (&bodyP1, edp1, MASTERFILE);
		mdlKISolid_elementToBody (&bodyP2, edp2, MASTERFILE);
		
		/* Create a blank KIENTITY_LIST */
		mdlKISolid_listCreate (&list1  );
		mdlKISolid_listCreate (&list2  );
		
		
		/* Add the cutting KIBODY to KIENTITY_LIST */
		mdlKISolid_listAdd (list2  , bodyP2);
		
		mdlKISolid_booleanDisjoint (list1, bodyP1, list2 ,
		               MODELER_BOOLEAN_difference); 
		
		
		
		//mdlElmdscr_add(edp1);
		//mdlElement_add(&cone);
		
		
		/* convert the new body back to an element */
		mdlKISolid_bodyToElement (&edp3, bodyP1, TRUE, -1, 
		                    NULL, MASTERFILE);
		
		/* Display and add the new solid */
		//mdlElmdscr_display (edp3, MASTERFILE, NORMALDRAW);
		//mdlElmdscr_add(edp3);
		
		mdlElmdscr_freeAll (&edp1);
		mdlElmdscr_freeAll (&edp2);
		//mdlElmdscr_freeAll (&edp3);
		
		return edp3;

}


//////////////////////////////////
Private MSElementDescr* makePlate(
double dHeight,
double dWidth,
double dLength,
double dWallWidth,
long iThickness // if 0 then without boolian disjoint								  
)
{

		KIBODY          *cuboid         = NULL;
		MSElementDescr *edP = NULL; 
		Transform tm;
		DPoint3d p3d_orient;
		DPoint3d p3d_orientInner;
		DPoint3d p3d_partOffset;
		double dThickness;

		dThickness = (double)iThickness;

        /* Begin current translation */
        mdlKISolid_beginCurrTrans (MASTERFILE);

        /* Convert master units to UORs */
        mdlCnv_masterToUOR (&dHeight, dHeight, MASTERFILE);
        mdlCnv_masterToUOR (&dWidth, dWidth, MASTERFILE);
        mdlCnv_masterToUOR (&dLength, dLength, MASTERFILE);
        mdlCnv_masterToUOR (&dThickness, dThickness, MASTERFILE);
        mdlCnv_masterToUOR (&dWallWidth, dWallWidth, MASTERFILE);

        /* Convert current units to Parasolid units */
        mdlCurrTrans_invScaleDoubleArray (&dHeight, &dHeight, 1);
        mdlCurrTrans_invScaleDoubleArray (&dWidth, &dWidth, 1);
        mdlCurrTrans_invScaleDoubleArray (&dLength, &dLength, 1);
        mdlCurrTrans_invScaleDoubleArray (&dThickness, &dThickness, 1);
        mdlCurrTrans_invScaleDoubleArray (&dWallWidth, &dWallWidth, 1);


		p3d_partOffset.x = 0.;
		p3d_partOffset.y = 0.;
		p3d_partOffset.z = 0.;

		if (abs(iDirection) == 1) 
		{
			p3d_orient.x = dWidth;
			p3d_orient.y = dLength;
			p3d_orient.z = dHeight;

			p3d_orientInner.x = dWidth - dThickness * 2;
			p3d_orientInner.y = dLength;
			p3d_orientInner.z = dHeight - dThickness * 2;

			p3d_partOffset.y = dLength / 2. + dWallWidth - dLength;
		}
		else if (abs(iDirection) == 2) 
		{
			p3d_orient.x = dLength;
			p3d_orient.y = dHeight;
			p3d_orient.z = dWidth;

			p3d_orientInner.x = dLength;
			p3d_orientInner.y = dHeight - dThickness * 2;
			p3d_orientInner.z = dWidth - dThickness * 2;

			p3d_partOffset.x = dLength / 2. + dWallWidth - dLength;
		}
		else
		{
			p3d_orient.x = dHeight;
			p3d_orient.y = dWidth;
			p3d_orient.z = dLength;

			p3d_orientInner.x = dHeight - dThickness * 2;
			p3d_orientInner.y = dWidth - dThickness * 2;
			p3d_orientInner.z = dLength;

			p3d_partOffset.z = dLength / 2. + dWallWidth - dLength;
		}


		/* Make our tick mark */
		mdlKISolid_makeCuboid (&cuboid, 
								p3d_orient.x, 
								p3d_orient.y, 
								p3d_orient.z
								);



		if (iThickness > 0)
		{
			int ret = 0;

			KIBODY          *cuboidIn         = NULL;

			KIENTITY_LIST   *list1  = NULL; /* Cutting List (holds BodyP2) */
			KIENTITY_LIST   *list2  = NULL; /* List of bodies after cut ) */

			ret = mdlKISolid_makeCuboid (&cuboidIn, 
									p3d_orientInner.x, 
									p3d_orientInner.y, 
									p3d_orientInner.z
									);
			/* Create a blank KIENTITY_LIST */
			mdlKISolid_listCreate (&list1  );
			mdlKISolid_listCreate (&list2  );
			
			
			/* Add the cutting KIBODY to KIENTITY_LIST */
			mdlKISolid_listAdd (list2  , cuboidIn);
			
			ret = mdlKISolid_booleanDisjoint (list1, cuboid, list2 ,
						MODELER_BOOLEAN_difference); 

		}






		/* Convert the body to an element */
		mdlKISolid_bodyToElement (&edP, cuboid, TRUE, -1, NULL, MASTERFILE);


		mdlTMatrix_getIdentity(&tm);

		mdlTMatrix_setTranslation(&tm, &p3d_partOffset);

		mdlElmdscr_transform(edP, &tm);


		/* End current translation */
		mdlKISolid_endCurrTrans ();
		/* Free memory */
		mdlKISolid_freeBody (cuboid);


		return edP;

}


//////////////////////////////////
Private MSElementDescr* makePlate2(
double dHeight,
double dWidth,
double dLength,
double dThickness,
double dOffset,
ULong iLevID,
int bBlick
)
{

		DPoint3d       base;
		DPoint3d       vec;
		DPoint3d       nrm;
		RotMatrix rm;
		Transform tm;
		double shell = dThickness;
		DPoint3d psh[5];
		DPoint3d pshb[5];
		double dw = 0.;
		double dh = 0.;

		KIBODY  *kb_shape  = NULL;

		MSElementDescr* edp = NULL;

		int ret;

		memset(&base, 0, sizeof(DPoint3d));
		memset(&vec, 0, sizeof(DPoint3d));
		memset(&nrm, 0, sizeof(DPoint3d));


		if (EQ(dHeight, 0.)) return NULL;
		if (EQ(dWidth, 0.)) return NULL;
		if (EQ(dLength, 0.)) return NULL;


		psh[0].z = psh[1].z = psh[2].z = psh[3].z = psh[4].z = dOffset;



		if (iJust == 1)
		{
			dw = 0.;
			dh = 0.;
		}
		else if (iJust == 2)
		{
			dw = 0.;
			dh = dHeight / 2.;
		}
		else if (iJust == 3)
		{
			dw = 0.;
			dh = dHeight;
		}
		else if (iJust == 4)
		{
			dw = dWidth / 2.;
			dh = 0.;
		}
		else if (iJust == 5)
		{
			dw = dWidth / 2.;
			dh = dHeight / 2.;
		}
		else if (iJust == 6)
		{
			dw = dWidth / 2.;
			dh = dHeight;
		}
		else if (iJust == 7)
		{
			dw = dWidth;
			dh = 0.;
		}
		else if (iJust == 8)
		{
			dw = dWidth;
			dh = dHeight / 2.;
		}
		else if (iJust == 9)
		{
			dw = dWidth;
			dh = dHeight;
		}


		psh[0].x = psh[4].x = dw - dWidth;
		psh[0].y = psh[4].y = dh - dHeight;

		psh[1].x = dw;
		psh[1].y = dh - dHeight;

		psh[2].x = dw;
		psh[2].y = dh;

		psh[3].x = dw - dWidth;
		psh[3].y = dh;

/*
		psh[0].x = psh[4].x = -dWidth / 2.;
		psh[0].y = psh[4].y = -dHeight / 2.;

		psh[1].x = -dWidth / 2.;
		psh[1].y = dHeight / 2.;

		psh[2].x = dWidth / 2.;
		psh[2].y = dHeight / 2.;

		psh[3].x = dWidth / 2.;
		psh[3].y = -dHeight / 2.;


*/


		vec.z = 1.;


		if (abs(iDirection) == 1) 
		{
			nrm.x = (iDirection / abs(iDirection)) * 1.;
		}
		else if (abs(iDirection) == 2) 
		{
			nrm.y = (iDirection / abs(iDirection)) * 1.;
		}
		else
		{
			nrm.z = (iDirection / abs(iDirection)) * 1.;
		}


		mdlRMatrix_fromNormalVector(&rm, &nrm);
		mdlRMatrix_getInverse(&rm, &rm);
		mdlTMatrix_fromRMatrix(&tm, &rm);


		mdlShape_create(&el, NULL, psh, 5, 0);

		if (bBlick)
		{
			UInt32       c = 0;
			UInt32       w = 0;
			Int32       s = 0; 
			Transform tmm = tm;
			DPoint3d pvec = vec;

			memcpy(pshb, psh, sizeof(psh));

			pshb[0] = pshb[2];
			pshb[0].x -= mdlCnv_masterUnitsToUors(abs(iBlick));
			pshb[0].y -= mdlCnv_masterUnitsToUors(abs(iBlick));

			pshb[4] = pshb[0];

			mdlShape_create(&elBlick[0], NULL, pshb, 5, -1);
			mdlElement_setSymbology(&elBlick[0], &c, &w, &s);
			mdlElement_transform(&elBlick[0], &elBlick[0], &tm);

			mdlRMatrix_multiplyPoint (&pvec, &rm);
			mdlVec_scaleToLengthInPlace(&pvec, dLength);
			mdlTMatrix_setTranslation (&tmm, &pvec);

			mdlShape_create(&elBlick[1], NULL, pshb, 5, -1);
			mdlElement_setSymbology(&elBlick[1], &c, &w, &s);
			mdlElement_transform(&elBlick[1], &elBlick[1], &tmm);

			memcpy(pshb, psh, sizeof(psh));
			pshb[1] = pshb[3];
			pshb[1].x += mdlCnv_masterUnitsToUors(abs(iBlick));
			pshb[1].y -= mdlCnv_masterUnitsToUors(abs(iBlick));

			mdlShape_create(&elBlick[2], NULL, pshb, 5, -1);
			mdlElement_setSymbology(&elBlick[2], &c, &w, &s);
			mdlElement_transform(&elBlick[2], &elBlick[2], &tmm);
		}

		//mdlEllipse_create (&el, NULL, &base, dOutsideRadius, dOutsideRadius, NULL, 0);
		mdlElmdscr_new (&edp, NULL, &el);
		//mdlElmdscr_appendElement (edp, &el);

		mdlKISolid_beginCurrTrans (MASTERFILE);

		mdlCurrTrans_invScaleDoubleArray  (&dLength, &dLength, 1);
		mdlCurrTrans_invScaleDoubleArray  (&shell, &shell, 1);

		ret = mdlKISolid_elementToBody(&kb_shape, edp, MASTERFILE);

		ret = mdlKISolid_sweepBodyVector(kb_shape, &vec, dLength, shell, 0.);

		ret = mdlKISolid_bodyToElement (&edp, kb_shape, TRUE, -1, NULL, MASTERFILE);

		if (iLevID) mdlElmdscr_setProperties(edp, &iLevID, 0, 0, 0, 0, 0, 0, 0);

		mdlElmdscr_transform(edp, &tm);

		mdlKISolid_freeBody (kb_shape);

		mdlKISolid_endCurrTrans ();

		return edp;


}



//////////////////////////
Private int makeOpening(
)
{


	MSElement e_Cell;
	MSElementDescr *edp1 = NULL; 

	edp1 = makePlate(
		penparams[1], // высота
		penparams[3], // ширина
		penparams[5], // длина
		0., // толщина стенки не нужна
		(long)penparams[2]  // толщина стенок проходки
		);

 	mdlCell_create(&e_Cell, NULL, NULL, FALSE);
	mdlElmdscr_new (&pen, NULL, &e_Cell); 
	
	// append element to cell
	if (edp1 != NULL) 
		mdlElmdscr_appendDscr(pen, edp1);	

	return SUCCESS;
}


//////////////////////////
Private int makeRectPenetr(
)
{

	MSElement e_Cell;
	MSElementDescr *edp1 = NULL; 
	MSElementDescr *edp2 = NULL; 
	MSElementDescr *edp3 = NULL; 

	double wgt[2];
	double hgt[2];
	double len[2];
	double thk[2];
	double ofs[3];


	//if (iFlanges == 2)
	{

		mdlCnv_masterToUOR (&hgt[0], (dRectHeight + dRectThick * 2.), MASTERFILE); 
		mdlCnv_masterToUOR (&hgt[1], (dRectHeight), MASTERFILE); 

		mdlCnv_masterToUOR (&wgt[0], (dRectWidth + dRectThick * 2.), MASTERFILE); 
		mdlCnv_masterToUOR (&wgt[1], (dRectWidth), MASTERFILE); 

		mdlCnv_masterToUOR (&len[0], dFlanThick, MASTERFILE); 
		mdlCnv_masterToUOR (&len[1], (dWallThick - dFlanThick), MASTERFILE); 

		mdlCnv_masterToUOR (&thk[0], dFlanWidth, MASTERFILE); 
		mdlCnv_masterToUOR (&thk[1], dRectThick, MASTERFILE); 

		mdlCnv_masterToUOR (&ofs[0], 0., MASTERFILE); 
		mdlCnv_masterToUOR (&ofs[1], dFlanThick / 2., MASTERFILE); 
		mdlCnv_masterToUOR (&ofs[2], (dWallThick - dFlanThick), MASTERFILE); 

		//mdlSystem_enterDebug();

		edp1 = makePlate2(hgt[0], wgt[0], len[0], thk[0], ofs[0], lvlF, FALSE);
		edp2 = makePlate2(hgt[1], wgt[1], len[1], thk[1], ofs[1], lvlP, TRUE);
		edp3 = makePlate2(hgt[0], wgt[0], len[0], thk[0], ofs[2], lvlF, FALSE);

	}

 	mdlCell_create(&e_Cell, NULL, NULL, FALSE);
	mdlElmdscr_new (&pen, NULL, &e_Cell); 
	
	// append element to cell
	if (edp1 != NULL) 
	{
		mdlElmdscr_appendDscr(pen, edp1);
	}


	//{
	//	DVec3d pvec;
	//	Transform tm, tmi;
	//	mdlElmdscr_extractCellTMatrix(&tm, &tmi, &pen->el, MASTERFILE);
	//	mdlTMatrix_getMatrixRow(&pvec, &tm, 2);
	//	printf("%f %f %f\n", pvec.x, pvec.y, pvec.z);
	//}



	if (edp2 != NULL) 
	{
		mdlElmdscr_appendDscr(pen, edp2);	
		if (iBlick) mdlElmdscr_appendElement(pen, &elBlick[0]);
		if (iBlick > 0) mdlElmdscr_appendElement(pen, &elBlick[1]);
		if (iBlick < 0) mdlElmdscr_appendElement(pen, &elBlick[2]);
	}


	if (edp3 != NULL) 
	{
		mdlElmdscr_appendDscr(pen, edp3);	
	}


	return SUCCESS;


}

//////////////////////////
Private int makePenetr(
)
{

	MSElement e_Cell;
	MSElementDescr *edp1 = NULL; 
	MSElementDescr *edp2 = NULL; 
	MSElementDescr *edp3 = NULL; 

	double dPipeDiameter;
	double dPipeThickness;
	double dFlangeThickness;
	double dFlangeWidth;

	double dPipeInsideDiameter;
	double dPipeOutsideDiameter;
	double dPipeLength;
	double dPipeOffset;

	double dFlangeInsideDiameter;
	double dFlangeOutsideDiameter;
	double dFlangeLength;
	double dFlangeOffset;


	if (iByMessage == TRUE)
	{
		dPipeDiameter = penparams[1];
		dPipeThickness = penparams[2];
		dFlangeThickness = penparams[3];
		dFlangeWidth = penparams[4];
		dWallThick = penparams[5];
	}
	else
	{
		dPipeDiameter = dPipeDiam;
		dPipeThickness = dPipeThick;
		dFlangeWidth = dFlanDiam;
		dFlangeThickness = dFlanThick;
	}


	mdlCnv_masterToUOR (&dPipeInsideDiameter, (dPipeDiameter - dPipeThickness * 2), MASTERFILE); 
	mdlCnv_masterToUOR (&dPipeOutsideDiameter, (dPipeDiameter), MASTERFILE); 
	
	mdlCnv_masterToUOR (&dFlangeInsideDiameter, (dPipeDiameter), MASTERFILE); 
	mdlCnv_masterToUOR (&dFlangeOutsideDiameter, (dFlangeWidth), MASTERFILE); 


	if (iFlanges == 2)
	{
		mdlCnv_masterToUOR (&dPipeLength, (dWallThick - dFlangeThickness), MASTERFILE); 
		mdlCnv_masterToUOR (&dPipeOffset, (dFlangeThickness / 2.), MASTERFILE); 

		mdlCnv_masterToUOR (&dFlangeLength, (dFlangeThickness), MASTERFILE); 
		mdlCnv_masterToUOR (&dFlangeOffset, (dWallThick - dFlangeThickness), MASTERFILE); 

		edp1 = makeTube2(
			dPipeInsideDiameter/2., 
			dPipeOutsideDiameter/2., 
			dPipeLength,
			dPipeOffset,
			lvlP
			);

		edp2 = makeTube2(
			dFlangeInsideDiameter/2., 
			dFlangeOutsideDiameter/2., 
			dFlangeLength,
			0.,
			lvlF
			);

		edp3 = makeTube2(
			dFlangeInsideDiameter/2., 
			dFlangeOutsideDiameter/2., 
			dFlangeLength,
			dFlangeOffset,
			lvlF
			);
	}
	else if (iFlanges == 1)
	{
		mdlCnv_masterToUOR (&dPipeLength, (dWallThick - dFlangeThickness / 2.), MASTERFILE); 
		mdlCnv_masterToUOR (&dPipeOffset, (dFlangeThickness / 2.), MASTERFILE); 

		mdlCnv_masterToUOR (&dFlangeLength, (dFlangeThickness), MASTERFILE); 
		mdlCnv_masterToUOR (&dFlangeOffset, 0., MASTERFILE); 

		edp1 = makeTube2(
			dPipeInsideDiameter/2., 
			dPipeOutsideDiameter/2., 
			dPipeLength,
			dPipeOffset,
			lvlP
			);

		edp2 = makeTube2(
			dFlangeInsideDiameter/2., 
			dFlangeOutsideDiameter/2., 
			dFlangeLength,
			0.,
			lvlF
			);

		if (iPenType == TYPE_INDENT) 	
			edp3 = makePlate(
							penparams[19], // ширина
							penparams[20], // высота
							penparams[21], // глубина
							penparams[5], // толщина стенки
							0  // толщина стенок не нужна
							);



	}
	else
	{
		mdlCnv_masterToUOR (&dPipeLength, dWallThick, MASTERFILE); 
		mdlCnv_masterToUOR (&dPipeOffset, 0., MASTERFILE); 
		
		mdlCnv_masterToUOR (&dFlangeLength, 0., MASTERFILE); 
		mdlCnv_masterToUOR (&dFlangeOffset, 0., MASTERFILE); 
		
		edp1 = makeTube2(
			dPipeInsideDiameter/2., 
			dPipeOutsideDiameter/2., 
			dPipeLength,
			0.,
			lvlP
			);
	}

 	mdlCell_create(&e_Cell, NULL, NULL, FALSE);
	mdlElmdscr_new (&pen, NULL, &e_Cell); 
	
	// append element to cell
	if (edp1 != NULL) 
		mdlElmdscr_appendDscr(pen, edp1);	


	if (edp2 != NULL) 
		mdlElmdscr_appendDscr(pen, edp2);	


	if (edp3 != NULL) 
		mdlElmdscr_appendDscr(pen, edp3);	



	return SUCCESS;
}





/////////////////////////////
Private void restartDefault()
{
	if (pen != NULL)
	{
		mdlElmdscr_freeAll (&pen);
		pen = NULL;
	}

	mdlState_startDefaultCommand();
}


//////////////////////////////
Private int	drawPenetr
(
Dpoint3d    *pt,
int	    view
)
{


	Transform	tMatrix;
    RotMatrix	rMatrix;

    /* get the inverse of the rotation matrix for the given view */

	//if (iDirection == 4)
		mdlRMatrix_fromView (&rMatrix, view, TRUE);
	//else
	//	mdlRMatrix_fromAngle (&rMatrix, 0.);


    mdlRMatrix_invert (&rMatrix, &rMatrix);
    
    /* convert to a transformation matrix and scale the columns */
    mdlTMatrix_fromRMatrix (&tMatrix, &rMatrix);

	mdlTMatrix_setTranslation (&tMatrix, pt);
    mdlElement_transform (dgnBuf, dgnBuf, &tMatrix);


	return SUCCESS;
}

//////////////////////////////
int elemFunc(
MSElement       *element,    // => element to act upon 
void            *params,     // => passed from original call 
int             operation,   // => why you were called 
ULong           offset,      // => offset from header 
MSElementDescr  *elemDscrP   // => element descr 
)
{
	//mdlElmdscr_show(elemDscrP, "  ");
	//mdlElement_add(element);

	int     elementType;
	int     isComplexHeader;


	elementType = mdlElement_getType(element);
	isComplexHeader = mdlElement_isComplexHeader(element);

	if (elementType == 2 && isComplexHeader)
	{
		mdlElement_showInfo(element);
	}


	return SUCCESS;
}



/////////////////////////////
Private void placePenetr(
Dpoint3d    *pt,	    /* => first data point */
int	    view	    /* => view for same */
)
{
	ElementID eid;
	//char     strID [10];
	//char**    args;


	if (pen != NULL)
	{

		Transform	tMatrix;
		RotMatrix	rMatrix;
		MSElementDescr *penToAdd = NULL; 

		if (iByMessage)
		{
			DPoint3d p1;
			DPoint3d p2;
			DPoint3d p3;

			p1.x = penparams[6];
			p1.y = penparams[7];
			p1.z = penparams[8];
			p2.x = penparams[9];
			p2.y = penparams[10];
			p2.z = penparams[11];
			p3.x = penparams[12];
			p3.y = penparams[13];
			p3.z = penparams[14];

			mdlRMatrix_fromRowVectors(&rMatrix, &p1, &p2, &p3);

		}
		else
		{
			//if (iDirection == 4)
				mdlRMatrix_fromView (&rMatrix, view, TRUE);
			//else
				//mdlRMatrix_fromAngle (&rMatrix, 0.); // 3.1415926535897932384626433832795

			mdlRMatrix_invert (&rMatrix, &rMatrix);
		}

	    
		/* convert to a transformation matrix and scale the columns */
		mdlTMatrix_fromRMatrix (&tMatrix, &rMatrix);

		mdlTMatrix_setTranslation (&tMatrix, pt);

		mdlElmdscr_duplicate(&penToAdd, pen);
		mdlElmdscr_transform (penToAdd, &tMatrix);

		mdlElmdscr_display (penToAdd, MASTERFILE, NORMALDRAW);

		mdlElmdscr_add(penToAdd);

		eid = elementRef_getElemID(penToAdd->h.elementRef);
		sprintf (strID, "%I64d", eid);
		args[0] = strID;


		mdlElmdscr_freeAll(&penToAdd);
/*
		if (!iByMessage && (iMode == MODE_PEN || iMode == MODE_RECT)) 
		{
			char procname[50];

			if (iMode == MODE_PEN)
				strcpy(procname, "eventPenPlaced");
			else if (iMode == MODE_RECT)
				strcpy(procname, "eventRectPlaced");

			if (mdlVBA_runProcedure(NULL, 0, "so2", "mpen", procname, 1, args) != SUCCESS)
				mdlOutput_messageCenterW(MESSAGE_ERROR, L"Ошибка вызова процедуры VBA, теги не установлены", 
							L"Ошибка вызова процедуры VBA, теги не установлены", TRUE);
		}
*/

	}
}

///////////////////////////
Private void checkValues(
char	*unparsedP
)
{
	//printf("%i\n", iDirection);

	int aq = 0;

	values[0] = 0;
	values[1] = 0;
	values[2] = 0;


	aq = getValuesFromString(unparsedP, values, 3);

	//mdlSystem_enterDebug();

	if (aq > 0 && aq != 3) 
	{
		mdlOutput_messageCenter(MESSAGE_WARNING, "неверное количество аргументов", "неверное количество аргументов", FALSE);
		return;
	}

	iByMessage = 0;

	if (iMode == MODE_PEN)
	{
		if (aq == 0 || (aq == 3 && values[0] > 0 && values[0] < 4 && values[1] >= 0 && values[2] > 0))
		{
			makePenetr();

			if (pen) 
			{
				mdlState_startPrimitive (placePenetr, restartDefault, 0, 0);
				mdlElmdscr_duplicate (&penBuf, pen);
				mdlDynamic_setElmDescr (penBuf);
				mdlState_dynamicUpdate (drawPenetr, TRUE);
			}
		}
		else
		{
			mdlOutput_messageCenter(MESSAGE_WARNING, "неверный диапазон аргументов", "неверный диапазон аргументов", FALSE);
		}
	}
	else if (iMode == MODE_RECT)
	{

			makeRectPenetr();

			if (pen) 
			{
				mdlState_startPrimitive (placePenetr, restartDefault, 0, 0);
				mdlElmdscr_duplicate (&penBuf, pen);
				mdlDynamic_setElmDescr (penBuf);
				mdlState_dynamicUpdate (drawPenetr, TRUE);
			}

	}
	else
	{
		penparams[1] = values[0];
		penparams[3] = values[1];
		penparams[5] = values[2];

		penparams[2] = 10.;

		makeOpening();

		if (pen) 
		{
			mdlState_startPrimitive (placePenetr, restartDefault, 0, 0);
			mdlElmdscr_duplicate (&penBuf, pen);
			mdlDynamic_setElmDescr (penBuf);
			mdlState_dynamicUpdate (drawPenetr, TRUE);
		}
	}

}

///////////////////////////////////
//Public cmdName void cmdPenDialog(
//char	*unparsedP
//)
//cmdNumber   CMD_PEN_DIALOG
//{
//
//}


/////////////////////////////////
Public cmdName void cmdMakePenetrPX(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_PX
{
	iDirection = 1;
	checkValues(unparsedP);
}

/////////////////////////////////
Public cmdName void cmdMakePenetrPY(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_PY
{
	iDirection = 2;
	checkValues(unparsedP);
}

/////////////////////////////////
Public cmdName void cmdMakePenetrPZ(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_PZ
{
	iDirection = 3;
	checkValues(unparsedP);
}

/////////////////////////////////
Public cmdName void cmdMakePenetrNX(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_NX
{
	iDirection = -1;
	checkValues(unparsedP);
}

/////////////////////////////////
Public cmdName void cmdMakePenetrNY(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_NY
{
	iDirection = -2;
	checkValues(unparsedP);
}

/////////////////////////////////
Public cmdName void cmdMakePenetrNZ(
char	*unparsedP
)
cmdNumber   CMD_PEN_PLACE_NZ
{
	iDirection = -3;
	checkValues(unparsedP);
}

///////////////////////////////////
//Public cmdName void cmdMakePenetrView(
//char	*unparsedP
//)
//cmdNumber   CMD_PEN_PLACE_VIEW
//{
//	iDirection = 4;
//	checkValues(unparsedP);
//}
//


/////////////////////////
void  userInput_receive ( 
Inputq_element  *queueElementP  
)
{
	if (queueElementP->hdr.source == 418)
	{
		// queueElementP->u.fill - присланный стринг
		// 2;273.0;7.0;6.0;440.0;300;-0.70711;0.70711;0.00000;0.00000;0.00000;1.00000;0.70711;0.70711;0.00000;3000;-2000;0
		// 0 - номер количества фланцев
		// 1 - диаметр трубы или ширина для прямоугольных
		// 2 - толщина трубы или толщина стенок для прямоугольных проходок
		// 3 - толщина фланца или высота для прямоугольных
		// 4 - диаметр фланца или _ для прямоугольных
		// 5 - толщина стены, где находится проходка
		// 6...14 - значений RotMatrix
		// 15...17 -  значения Point3d
		// 18 - тип проходки 
		// 19...21 -  параметры штрабы - ширина, высота, глубина

		//printf("%s\n", queueElementP->u.fill);
		//1;159.0;6.0;6.0;320.0;400;-0.20800;0.67900;0.70400;0.97800;0.14400;0.15000;0.00000;0.71900;-0.69500;72455;43144;115374;5;260.00000;260.00000;30.00000

		iFlanges = getParamsFromString(queueElementP->u.fill, penparams, MAX_PARAMS);

		iDirection = 3; // PZ

		iByMessage = 1;



		if (iPenType == TYPE_ROUND || iPenType == TYPE_INDENT) 
		{
			makePenetr();
		}
		else if (iPenType == TYPE_OPENING || iPenType == TYPE_RECT)
		{
			makeOpening();
		}
		else
		{
			printf("error - got iPenType = %i\n", iPenType);
		}

		if (pen) 
		{
			DPoint3d p;
			mdlCnv_masterToUOR (&(p.x), penparams[15], MASTERFILE);
			mdlCnv_masterToUOR (&(p.y), penparams[16], MASTERFILE);
			mdlCnv_masterToUOR (&(p.z), penparams[17], MASTERFILE);

			placePenetr(&p, 0);
		}

	}
}


//struct inputq_element
//    {
//    Inputq_header hdr;
//    union
//	{
//	Inputq_keyin	    keyin;	    /* keyed in (not command) */
//	Inputq_datapnt	    data;	    /* data point */
//	Inputq_command	    cmd;	    /* parsed command */
//	Inputq_tentpnt	    tent;	    /* tentative point */
//	Inputq_reset	    reset;	    /* reset */
//	Inputq_partial	    partial;	    /* incomplete keyin */
//	Inputq_unassignedcb cursbutn;	    /* unassigned cursor button */
//	Inputq_menumsg	    menumsg;	    /* menu message to be posted */
//	Inputq_submenu	    submenu;	    /* submenu to be displayed */
//	Inputq_menuwait	    menuwait;	    /* go back, get another input */
//	Inputq_contents	    contents;	    /* menu entry contents */
//	Inputq_null	    nullqelem;	    /* NULL event */
//	Inputq_nullcmd	    nullcmd;	    /* NULL command elem */
//	Inputq_rawButton    rawButton;	    /* raw button information */
//	Inputq_rawKeyStroke rawKeyStroke;   /* raw keystroke information */
//	Inputq_cookedKey    cookedKey;	    /* virtual keystoke information */
//	Inputq_rawIconEvent rawIconEvent;   /* raw Icon event information */
//	Inputq_timerEvent   timerEvent;	    /* timer event information */
//	Inputq_virtualEOQ   virtualEOQ;	    /* virtual end-of-queue */
//	Inputq_userWinClose userWinClose;   /* user closing window */
//	char fill[480];			    /* make sure it's big enough */
//	} u;
//    };

//typedef struct inputq_header
//    {
//    int     cmdtype;			/* type of input following */
//    int     bytes;			/* bytes in queue element */
//    int     source;			/* source of input (user, app, etc) */
//    int     uc_fno_value;		/* value to put in tcb->uc_fno */
//#if defined (macintosh)
//    ProcessID sourcepid;		/* source pid for queue element */
//#else
//    int	    sourcepid;			/* source pid for queue element */
//#endif
//    char    taskId[16];			/* destination child task */
//    int     unused;			/* NOTE: This is here to make size of Inputq_header a multiple of 8 bytes! */
//    } Inputq_header;


////////////////////////////////////////////////
// M A I N
////////////////////////////////////////////////
Public int main(
int   argc,	 
char *argv[]
)
{ 

	#if defined (MSVERSION) && (MSVERSION > 0x850) 
		SymbolSet*  setP;
	#else
		char	    *setP;
	#endif


    setP = mdlCExpression_initializeSet (VISIBILITY_CALCULATOR, 0, TRUE);


	mdlParse_loadCommandTable (NULL);

	// round
    mdlDialog_publishBasicVariable (setP, &doubleType, "fdiam", &dFlanDiam);
    mdlDialog_publishBasicVariable (setP, &doubleType, "pdiam", &dPipeDiam);
    mdlDialog_publishBasicVariable (setP, &doubleType, "pthick", &dPipeThick);

	// rect
    mdlDialog_publishBasicVariable (setP, &doubleType, "rwidth", &dRectWidth);
    mdlDialog_publishBasicVariable (setP, &doubleType, "rheight", &dRectHeight);
    mdlDialog_publishBasicVariable (setP, &doubleType, "rthick", &dRectThick);
    mdlDialog_publishBasicVariable (setP, &doubleType, "fwidth", &dFlanWidth);

	// common
    mdlDialog_publishBasicVariable (setP, &doubleType, "fthick", &dFlanThick);
    mdlDialog_publishBasicVariable (setP, &doubleType, "wthick", &dWallThick);

    mdlDialog_publishBasicVariable (setP, &longType, "just", &iJust);

    mdlDialog_publishBasicVariable (setP, &longType, "blick", &iBlick);
    mdlDialog_publishBasicVariable (setP, &longType, "mode", &iMode);
    mdlDialog_publishBasicVariable (setP, &longType, "fqty", &iFlanges);

    mdlDialog_publishBasicVariable (setP, &longType, "lvlF", &lvlF);
    mdlDialog_publishBasicVariable (setP, &longType, "lvlP", &lvlP);

	mdlInput_setFunction(INPUT_MESSAGE_RECEIVED, userInput_receive);


    return SUCCESS;


}



