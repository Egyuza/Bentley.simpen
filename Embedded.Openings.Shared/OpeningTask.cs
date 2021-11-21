using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Embedded.Openings.Shared
{
public class OpeningTask : BentleyInteropBase
{
    public double Height {get; set;}
    public double Width {get; set;}
    public double Depth {get; set;}

	public BCOM.Point3d Origin { get; set; }

	public BCOM.Point3d HeigthVec {get; set;}
	public BCOM.Point3d WidthVec {get; set;}
	public BCOM.Point3d DepthVec {get; set;}

    //public string Code {get; set;}

    public IntPtr OwnerFormRef {get; private set;}

    public Dictionary<Sp3dToDataGroupMapProperty, string> 
        DataGroupPropsValues { get; set; }

    public OpeningTask()
    {
        DataGroupPropsValues = new Dictionary<Sp3dToDataGroupMapProperty, string>();
    }

	public BCOM.ShapeElement GetContourShape(BCOM.MsdFillMode fillMode = BCOM.MsdFillMode.NotFilled)
    {
        var bounds = new BCOM.Point3d[4];

        bounds[0] = Origin.AddScaled(HeigthVec, -0.5 * Height)
            .AddScaled(WidthVec, -0.5 * Width);
        bounds[1] = Origin.AddScaled(HeigthVec, 0.5 * Height)
            .AddScaled(WidthVec, -0.5 * Width);
        bounds[2] = Origin.AddScaled(HeigthVec, 0.5 * Height)
            .AddScaled(WidthVec, 0.5 * Width);
        bounds[3] = Origin.AddScaled(HeigthVec, -0.5 * Height)
            .AddScaled(WidthVec, 0.5 * Width);

        var shape = App.CreateShapeElement1(null, bounds, fillMode);
        
        if (!App.Point3dEqualTolerance(
            shape.Normal.Normalize(), DepthVec.Normalize(), 0.00005))
        {
            shape.Reverse();
        }
        return shape;
    }
    public BCOM.LineElement GetLineContour()
    {
        var bounds = new BCOM.Point3d[5];

        bounds[0] = Origin.AddScaled(HeigthVec, -0.5 * Height)
            .AddScaled(WidthVec, -0.5 * Width);
        bounds[1] = Origin.AddScaled(HeigthVec, 0.5 * Height)
            .AddScaled(WidthVec, -0.5 * Width);
        bounds[2] = Origin.AddScaled(HeigthVec, 0.5 * Height)
            .AddScaled(WidthVec, 0.5 * Width);
        bounds[3] = Origin.AddScaled(HeigthVec, -0.5 * Height)
            .AddScaled(WidthVec, 0.5 * Width);
        bounds[4] = bounds[0];

        BCOM.LineElement line = App.CreateLineElement1(null, bounds);
        
        if (!App.Point3dEqualTolerance(
            line.Normal.Normalize(), DepthVec.Normalize(), 0.00005))
        {
            line.Reverse();
        }
        return line;
    }
}
}

    /* вершины объекта задания:

		вершины через mdlCell_extract (2 варианта, встречаются оба):
		
		  4______________5
		 /|             /|
		7______________6 |
	a)	| 3 - - - - - -|-2
		|/             |/
		0______________1

		  5______________6
		 /|             /|
		4______________7 |
	b)	| 3 - - - - - -|-2
		|/             |/
		0______________1


		вершины через mdlTFBrep_getVertexLocations:
		  5______________4
		 /|             /|
		2______________3 |
		| 6 - - - - - -|-7
		|/             |/
		1______________0
    */