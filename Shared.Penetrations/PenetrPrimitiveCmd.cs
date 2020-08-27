using System;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared.Bentley;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif



namespace Embedded.Penetrations.Shared
{
public class PenetrPrimitiveCmd : BCOM.IPrimitiveCommandEvents
{   
    internal static void StartCommand(SingleModel singleModel)
    {
        app_.CommandState.StartPrimitive(new PenetrPrimitiveCmd(singleModel));
    }

    public PenetrPrimitiveCmd(SingleModel singleModel)
    {
        this.singleModel_ = singleModel;
    }

    public void Start()
    {
        BCOM.LocateCriteria lc = app_.CommandState.CreateLocateCriteria(false);
        //lc.ExcludeAllTypes();
        //lc.IncludeType(BCOM.MsdElementType.CellHeader);        
        app_.CommandState.SetLocateCriteria(lc);

        app_.CommandState.EnableAccuSnap();
        app_.CommandState.ElementDisplayEnabled = true;
        app_.CommandState.StartDynamics();

        ensureLocateEnabled = true;

        // позво
        app_.SetCExpressionValue("userPrefsP->smartGeomFlags.locateSurfaces", 4);
    }

    public void Cleanup()
    {
        app_.CommandState.StopDynamics();
    }

    public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
    {
        //throw new NotImplementedException();
    }

    private bool ensureLocateEnabled = true;

    public void Dynamics(
        ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
    {
        if (ensureLocateEnabled)
        {   // ! unblock Locate
            ensureLocateEnabled = false;
            app_.CommandState.StopDynamics();

            app_.CommandState.LocateElement(ref Point, View, true);
            app_.CommandState.GetHitPath();

            app_.CommandState.StartDynamics();
        }

        var locEl = app_.CommandState.LocateElement(ref Point, View, true);
        var hitPath = app_.CommandState.GetHitPath();
        if (hitPath != null)
        {
            string kks = singleModel_.UserTask.KKS;
            ;
        }

        BCOM.Point3d hitPoint = Point; // app_.CommandState.GetHitPoint();

        singleModel_.UserTask.Location = Point;

        if (!hitPoint.EqualsPoint(app_.Point3dZero()))
        {
            PenetrInfo penInfo = PenetrDataSource.Instance.getPenInfo(
                singleModel_.UserTask.FlangesType, 
                singleModel_.UserTask.DiameterType.Number);

            var frame = PenetrHelper.createFrameList(
                singleModel_.UserTask, penInfo, PenetrVueTask.LevelMain);

            //BCOM.Element element = frame.Get3DElement();
            //element.Redraw(DrawMode);

            var circle = 
                ElementHelper.createCircle(penInfo.pipeDiameterOutside, Point);
            //circle.Redraw(DrawMode);

            BCOM.SmartSolidElement cylindrInside =
                app_.SmartSolid.CreateCylinder(null, 
                penInfo.pipeDiameterOutside / 2, 
                singleModel_.UserTask.LengthCm * 10);

            cylindrInside.Move(Point);

            TFCOM.TFFrameListClass frameList = new TFCOM.TFFrameListClass();
            frameList.Add3DElement(cylindrInside);

            var el = frameList.Get3DElement();
            el.Redraw(DrawMode);
        }
        if (locEl != null ||
            app_.CommandState.GetLocatedElement(false) != null)        
        {
            ;
        }
    }

    public void Keyin(string Keyin)
    {
        //throw new NotImplementedException();
    }

    public void Reset()
    {
        //throw new NotImplementedException();
    }

    private SingleModel singleModel_;
    private static BCOM.Application app_ => BMI.Utilities.ComApp;

}

}
