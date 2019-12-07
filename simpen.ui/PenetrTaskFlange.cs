using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;


using Bentley.Internal.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using Bentley.MicroStation.XmlInstanceApi;
using BCOM = Bentley.Interop.MicroStationDGN;

namespace simpen.ui
{
class PenetrTaskFlange
{    
    public IntPtr elemRef {get; private set; }
    public long elemId {get; private set; }
    public IntPtr modelRef {get; private set; }
    
    public int Diametr { get; set; }

    //public ulong ElementId {get; private set; }
    public string Code {get; private set; }
    public string Oid {get; private set; }

    public BCOM.Point3d Location {get; private set; }

    public string ErrorText { get; private set; }

    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }


    private PenetrTaskFlange(Element element, sp3d.Sp3dTask task)
    {
        elemRef = element.ElementRef;
        modelRef = element.ModelRef;
        elemId = element.ElementID;
        Oid = task.pipe.Oid;

        if (task.pipe == null || task.component == null)
        {
            ErrorText = "Не удалось прочитать данные задания";
            return;
        }

        Code = task.pipe.Name;

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

    public static bool getFromElement(Element element, out PenetrTaskFlange penTask)
    {
        sp3d.Sp3dTask task = null;
        penTask = null;

        if (!ElementHelper.isElementSp3dTask(
                element.ElementID, element.ModelRef, out task) || 
            !(task.isFlange()))
        {
            return false;
        }

        penTask = new PenetrTaskFlange(element, task);
        return true;        
    }
}
}
