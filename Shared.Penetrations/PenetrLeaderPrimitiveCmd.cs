using System;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Shared.Bentley;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared
{
public class PenetrLeaderPrimitiveCmd : BCOM.IPrimitiveCommandEvents
{   

    [DllImport("ustation.dll")]
    public static extern int mdlCell_setIsAnnotation(int msElementDescr, int isAnnotation); 

    [DllImport("ustation.dll")]
    public static extern int mdlCell_setAnnotationScale(int msElementDescr, double scale);    


    //public static extern unsafe int mdlCell_setIsAnnotation([In] msElementDescr* obj0, [In] int obj1);


    internal static void StartCommand(PenetrLeaderInfo leaderInfo)
    {
        app_.CommandState.StartPrimitive(new PenetrLeaderPrimitiveCmd(leaderInfo));
    }

    public PenetrLeaderPrimitiveCmd(PenetrLeaderInfo leaderInfo)
    {
        leaderInfo_ = leaderInfo;
    }

    public void Start()
    {
        pointIndex_ = 0;
        uor_ = new UOR(app_.ActiveModelReference);

        BCOM.LocateCriteria lc = app_.CommandState.CreateLocateCriteria(false);
        lc.ExcludeAllTypes();
        lc.IncludeOnlySolid();
        lc.IncludeType(BCOM.MsdElementType.CellHeader);        
        app_.CommandState.SetLocateCriteria(lc);

        app_.CommandState.EnableAccuSnap();
        app_.CommandState.ElementDisplayEnabled = true;
        app_.CommandState.StartDynamics();

        // TODO
        //app_.SetCExpressionValue("userPrefsP->smartGeomFlags.locateSurfaces", 4);
    }

    public void Cleanup()
    {
        app_.CommandState.StopDynamics();
    }

    public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
    {
        points_[pointIndex_++] = app_.Point3dFromXY(Point.X, Point.Y);

        if (pointIndex_ > 2)
        {
            // TODO в модель
            BCOM.Element leader = createLeader();

            leader.ScaleUniform(leader.AsCellElement().Origin, 
                1/ElementHelper.getActiveAnnotationScale());

            // ! перед добавлением в модель
            mdlCell_setIsAnnotation((int)leader.MdlElementDescrP(), 1); 
            //leader.AsCellElement().ScaleUniform();
           

            app_.ActiveModelReference.AddElement(leader);

            var propHand = app_.CreatePropertyHandler(leader);
            propHand.GetAccessStrings();
            propHand.SelectByAccessString("AnnotationPurpose");
            if ((bool)propHand.GetValue())
            {
                propHand.SelectByAccessString("IsAnnotation");
                propHand.SetValue(true);
            }            

            leader.Rewrite();
            app_.ActiveModelReference.PropagateAnnotationScale();
            
            //var res = mdlCell_setAnnotationScale(leader.MdlElementDescrP(), ElementHelper.getActiveAnnotationScale());        

            PenetrLocateCmd.StartCommand();
        }
    }

    public void Dynamics(
        ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
    {
        points_[pointIndex_] = Point;
        createLeader()?.Redraw(DrawMode);
    }

    public void Keyin(string Keyin)
    {
        //throw new NotImplementedException();
    }

    public void Reset()
    {
        pointIndex_ = 0;
        //throw new NotImplementedException();
    }

    private BCOM.Element createLeader()
    {
        List<BCOM.Element> elements = new List<BCOM.Element>();

        if (pointIndex_ < 1)
        {
            points_[1] = points_[2] = points_[0];
        }
        else if (pointIndex_ < 2)
        {
            points_[2] = points_[1];
        }
        else
        {
            points_[2] = app_.Point3dFromXY(points_[2].X, points_[1].Y);
        }

        int kX = (points_[2].X - points_[1].X) >= 0 ? 1 : -1;
        
        BCOM.TextElement textUpper = app_.CreateTextElement1(
            null, leaderInfo_.TextLines[0], app_.Point3dZero(), app_.Matrix3dZero());
        textUpper.Redraw(BCOM.MsdDrawingMode.Temporary);
        textUpper.set_Origin(points_[2].shift(dY: textUpper.Range.getHeight() * 0.75));
        
        BCOM.TextElement textLower = app_.CreateTextElement1(
            null, leaderInfo_.TextLines[1], app_.Point3dZero(), app_.Matrix3dZero());
        textLower.Redraw(BCOM.MsdDrawingMode.Temporary);
        textLower.set_Origin(points_[2].shift(dY: -textUpper.Range.getHeight() * 0.25));

        double maxwidth = 
            Math.Max(textUpper.Range.getWidth(), textLower.Range.getWidth());
        if (kX < 0)
        {
            textUpper.set_Origin(textUpper.get_Origin().shift(-maxwidth));
            textLower.set_Origin(textLower.get_Origin().shift(-maxwidth));
        }

        points_[3] = app_.Point3dFromXY(points_[2].X + 
            kX * (maxwidth + leaderInfo_.GapTextAfter), points_[2].Y);

        BCOM.Element line = app_.CreateLineElement1(null, points_);
        elements.Add(line);

        elements.Add(textUpper);
        elements.Add(textLower);

        return app_.CreateCellElement1(
            "PenetrLeader", elements.ToArray(), points_[0]);
    }

    private PenetrLeaderInfo leaderInfo_;
    private int pointIndex_;
    private BCOM.Point3d[] points_ = new BCOM.Point3d[4];
    private UOR uor_;

    private static BCOM.Application app_ => BMI.Utilities.ComApp;



}

}
