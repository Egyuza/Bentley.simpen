using System;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

using Shared.Bentley;
using Shared.Bentley.sp3d;

namespace Embedded.Penetrations.Shared
{
public class PenetrTaskFlange
{    
#if V8i
    public int elemRef {get; private set; }
    public int modelRef {get; private set; }
#endif
#if CONNECT
    public long elemRef {get; private set; }
    public long modelRef {get; private set; }
#endif

    public long elemId {get; private set; }
    
    public int Diametr { get; set; }

    //public ulong ElementId {get; private set; }
    public string Code {get; private set; }
    public string Oid {get; private set; }

    public BCOM.Point3d Location {get; private set; }

    public string ErrorText { get; private set; }

    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }

    private PenetrTaskFlange(BCOM.Element element, Sp3dTask_Old task)
    {
        init(element, task);
    }

    private void init(BCOM.Element element, Sp3dTask_Old task)
    {
        elemRef = element.MdlElementRef();
        modelRef = element.ModelReference.MdlModelRefP();
        elemId = element.ID;
        Oid = task.pipe.Oid;

        if (task.pipe == null || task.component == null)
        {
            ErrorText = "Не удалось прочитать данные задания";
            return;
        }

        Code = task.pipe.Name;

        BCOM.Point3d pt = new BCOM.Point3d() ;
        pt.X = task.pipe.LocationX;
        pt.Y = task.pipe.LocationY;
        pt.Z = task.pipe.LocationZ;
        Location = pt;

        // разбор типоразмера:
        try
        {
            string[] parameters = task.pipe.Description.TrimStart('T').Split('-');
            Diametr = int.Parse(parameters[1]);
        }
        catch (Exception)
        {
            ErrorText = string.Format("Не удалость разобрать типоразмер \"{0}\"",
                task.pipe.Description);
        }
    }

    public static bool getFromElement(BCOM.Element element, out PenetrTaskFlange penTask)
    {
        Sp3dTask_Old task = null;
        penTask = null;        

        if (!ElementHelper.isElementSp3dTask_Old(
            ElementHelper.getElement(element), out task) || !(task.isFlange()))
        {
            return false;
        }

        penTask = new PenetrTaskFlange(element, task);
        return true;        
    }


    public void setLocation(ref BCOM.Point3d origin)
    {
        Location = origin;
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
