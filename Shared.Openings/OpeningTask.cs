using System;
using System.Collections.Generic;
using System.Text;

namespace Embedded.Openings.Shared
{
public class OpeningTask
{
    public double Height {get; set;}
    public double Width {get; set;}
    public double Depth {get; set;}

    public string Code {get; set;}

    public IntPtr OwnerFormRef {get; private set;}

    public Dictionary<string, string> Properties {get; set;}
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