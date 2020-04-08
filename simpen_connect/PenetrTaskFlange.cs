//using System;
//using Bentley.Internal.MstnPlatformNET;
//using Bentley.DgnPlatformNET.Elements;
//using BCOM = Bentley.Interop.MicroStationDGN;

//using Shared.sp3d;

//namespace simpen_cn
//{
//class PenetrTaskFlange
//{    
//    public long elemRef {get; private set; }
//    public long elemId {get; private set; }
//    public long modelRef {get; private set; }
    
//    public int Diametr { get; set; }

//    //public ulong ElementId {get; private set; }
//    public string Code {get; private set; }
//    public string Oid {get; private set; }

//    public BCOM.Point3d Location {get; private set; }

//    public string ErrorText { get; private set; }

//    public bool isValid { get { return string.IsNullOrEmpty(ErrorText); } }


//    private PenetrTaskFlange(Element element, Sp3dTask task)
//    {
//        BCOM.Element bcomEl =
//        Addin.App.MdlGetModelReferenceFromModelRefP((long)element.GetNativeDgnModelRef())
//            .GetElementByID(element.ElementId);
//        init(bcomEl, task);
//    }

//    private PenetrTaskFlange(BCOM.Element element, Sp3dTask task)
//    {
//        init(element, task);         
//    }

//    private void init(BCOM.Element element, Sp3dTask task)
//    {
//        elemRef = element.MdlElementRef();
//        modelRef = element.ModelReference.MdlModelRefP();
//        elemId = element.ID;
//        Oid = task.pipe.Oid;

//        if (task.pipe == null || task.component == null)
//        {
//            ErrorText = "Не удалось прочитать данные задания";
//            return;
//        }

//        Code = task.pipe.Name;

//        BCOM.Point3d pt = new BCOM.Point3d() ;
//        pt.X = task.pipe.LocationX;
//        pt.Y = task.pipe.LocationY;
//        pt.Z = task.pipe.LocationZ;
//        Location = pt;

//        // разбор типоразмера:
//        try
//        {
//            string[] parameters = task.pipe.Description.TrimStart('T').Split('-');
//            Diametr = int.Parse(parameters[1]);
//        }
//        catch (Exception)
//        {
//            ErrorText = string.Format("Не удалость разобрать типоразмер \"{0}\"",
//                task.pipe.Description);
//        }            
//    }

//    public static bool getFromElement(Element element, out PenetrTaskFlange penTask)
//    {
//        Sp3dTask task = null;
//        penTask = null;

//        if (!ElementHelper.isElementSp3dTask(element, out task) || 
//            !(task.isFlange()))
//        {
//            return false;
//        }

//        penTask = new PenetrTaskFlange(element, task);
//        return true;        
//    }

//    public static bool getFromElement(BCOM.Element element, out PenetrTaskFlange penTask)
//    {
//        Sp3dTask task = null;
//        penTask = null;

//        Element elem = Element.GetFromElementRefAndModelRef(
//            (IntPtr)element.MdlElementRef(),
//            (IntPtr)element.ModelReference.MdlModelRefP());

//        if (!ElementHelper.isElementSp3dTask(elem, out task) || 
//            !(task.isFlange()))
//        {
//            return false;
//        }

//        penTask = new PenetrTaskFlange(element, task);
//        return true;        
//    }


//    public void setLocation(ref BCOM.Point3d origin)
//    {
//        Location = origin;
//    }

//}
//}
