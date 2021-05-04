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
public class PenetrPrimitiveCmd : BentleyInteropBase, BCOM.IPrimitiveCommandEvents
{   
    internal static void StartCommand(SingleModel singleModel)
    {
        app_.CommandState.StartPrimitive(new PenetrPrimitiveCmd(singleModel));
    }

    internal static void StartDefaultCommand()
    {
        app_.CommandState.StartDefaultCommand();
    }

    public PenetrPrimitiveCmd(SingleModel singleModel)
    {
        this.singleModel_ = singleModel;
    }

    public void Start()
    {
        BCOM.LocateCriteria lc = app_.CommandState.CreateLocateCriteria(false);
        //lc.ExcludeAllTypes();
        //lc.IncludeType(BCOM.MsdElementType.Shape);
        //lc.IncludeType(BCOM.MsdElementType.ComplexShape);

        app_.CommandState.SetLocateCriteria(lc);

        app_.CommandState.EnableAccuSnap();
        app_.CommandState.ElementDisplayEnabled = true;
        app_.CommandState.StartDynamics();

        ensureLocateEnabledRequired_ = true;

        // позво
        app_.SetCExpressionValue("userPrefsP->smartGeomFlags.locateSurfaces", 4);
    }

    public void Cleanup()
    {
        app_.CommandState.StopDynamics();
    }

    public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
    {
        PenetrUserTask userTask;
        if (!processInput(out userTask, ref Point, View))
            return;   
        
        PenetrInfo penInfo = PenetrDataSource.Instance.getPenInfo(
            userTask.FlangesType, 
            userTask.DiameterType.Number);
                
        ElementHelper.RunByRecovertingSettings(() => { 
            PenetrHelper.addToModel(userTask, penInfo);
        });
    }

    public void Dynamics(ref BCOM.Point3d Point, BCOM.View View, 
        BCOM.MsdDrawingMode DrawMode)
    {
        PenetrUserTask userTask;
        if (!processInput(out userTask, ref Point, View))
            return;

        PenetrInfo penInfo = PenetrDataSource.Instance.getPenInfo(
            userTask.FlangesType, 
            userTask.DiameterType.Number);

        var frameList = PenetrHelper.createFrameList(
            userTask, penInfo, PenetrTaskBase.LevelMain);

        var el = frameList.AsTFFrame.Get3DElement();
        el.Redraw(DrawMode);
        
    }

    private bool processInput(out PenetrUserTask userTask, 
        ref BCOM.Point3d Point, BCOM.View View)
    {
        BCOM.Point3d point = Point;
        ensureLocateEnabled(ref point, View);

        userTask = singleModel_.UserTask;

        if (string.IsNullOrEmpty(userTask.Code))
            return false;

        BCOM.HitPath hitPath = App.CommandState.GetHitPath();
        if (!userTask.IsManualRotateMode && hitPath != null)
        {
            BCOM.Element firstHitElem = hitPath.GetElementAt(1);
            //int type1 = (int)firstHitElem.Type;

            if (firstHitElem.IsPlanarElement())
            {
                userTask.Rotation = App.Matrix3dFromRotationBetweenVectors(
                    ZAxis, firstHitElem.AsPlanarElement().Normal);
            }
            else 
            {
                // TODO обход по поверхностям
            }
        }

        if (userTask.IsAutoLength)
        {
            double thickness = 0.0;

            BCOM.Element locEl = app_.CommandState.LocateElement(ref Point, View, true);

            if (locEl != null && locEl.IsCellElement())
            {
                TFCOM.TFElementList tfList = AppTF.CreateTFElement();
                tfList.InitFromElement(locEl);

                int type = tfList.AsTFElement.GetApplicationType();

                if (tfList.AsTFElement.GetIsFormType())
                {
                    TFCOM._TFFormRecipeList recipeList;                    
                    tfList.AsTFElement.GetFormRecipeList(out recipeList);

                    if (type == (int)TFFormTypeEnum.TF_SLAB_FORM_ELM)
                    {
                        var slab = (TFCOM.TFFormRecipeSlabList)recipeList;

                        slab.AsTFFormRecipeSlab.GetThickness(out thickness);                
                    }
                    else if (type == (int)TFFormTypeEnum.TF_LINEAR_FORM_ELM)
                    {
                        var wall = (TFCOM.TFFormRecipeLinearList)recipeList;
                        wall.AsTFFormRecipeLinear.GetThickness(out thickness);                              
                    }                
                }
            }

            if (thickness > 0.0)
            {
                userTask.LengthCm = (int)Math.Ceiling(thickness) / 10;
            }
            else
            {
                userTask.LengthCm = 1;
            }
        }

        singleModel_.setLocation(point); // для обновления формы
        return true;
    }


    public bool getTFPlainFromBrepFace(out TFCOM.TFPlane tfPlain, TFCOM._TFFormRecipeList resipeList, TFCOM.TFdFaceLabel label)
    {        
        TFCOM.TFBrepList brepList;
        resipeList.GetBrepList(out brepList, false, false, false);

        TFCOM.TFBrepFaceList faceList = brepList.GetFacesByLabel(label);

        //BCOM.Point3d center;
        //faceList.AsTFBrepFace.GetCenter(out center);

        //BCOM.Element faceElem;
        //faceList.GetElement(out faceElem, App.Transform3dIdentity());
                
        //TFCOM.TFPlane tfPlain;
        return faceList.AsTFBrepFace.IsPlanar(out tfPlain);
        //{
        //    return true
        //}

        //if (!faceElem.IsPlanarElement())
        //{
        //    plane = new BCOM.Plane3d();
        //    return false;
        //}

        //var normal = faceElem.AsPlanarElement().Normal;

        //plane = new BCOM.Plane3d() {
        //    Normal = normal,
        //    Origin = center
        //}; 
        //return true;
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

    private bool ensureLocateEnabledRequired_ = true;

    private void ensureLocateEnabled(ref BCOM.Point3d Point, BCOM.View View)
    {
         if (ensureLocateEnabledRequired_)
        {   // ! unblock Locate
            ensureLocateEnabledRequired_ = false;
            app_.CommandState.StopDynamics();

            app_.CommandState.LocateElement(ref Point, View, true);
            app_.CommandState.GetHitPath();

            app_.CommandState.StartDynamics();
        }
    }


}

}
