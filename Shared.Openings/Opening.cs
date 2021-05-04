using Bentley.Building.DataGroupSystem;
using Bentley.Building.DataGroupSystem.Serialization;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using System.Linq;

#if V8i
using Bentley.MicroStation;
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;

#endif

namespace Embedded.Openings.Shared
{
public class Opening : BentleyInteropBase
{
    public OpeningTask Task { get; private set; }
    public TFCOM.TFFrameListClass FrameList { get; private set; }
    public BCOM.Element GetElement() => FrameList.AsTFFrame.Get3DElement();
    public IEnumerable <BCOM.Element> GetProjections() => FrameList.GetProjectionElements();

    private readonly BCOM.ShapeElement contour_;
    private bool IsPerforationAdded_;
    private bool IsProjectionAdded_;

    public Opening(OpeningTask task)
    {
        Task = task;
        FrameList = new TFCOM.TFFrameListClass();

        BCOM.Level level = ElementHelper.GetOrCreateLevel(LEVEL_NAME);

        contour_ = task.GetContourShape();

        BCOM.SmartSolidElement body =
            App.SmartSolid.ExtrudeClosedPlanarCurve(contour_, task.Depth, 0.0, false);
        body.Level = level;
        ElementHelper.setSymbologyByLevel(body);

        FrameList.Add3DElement(body);
        FrameList.AsTFFrame.SetName(CELL_NAME);
    }

    public void AddProjection()
    {
        if (IsProjectionAdded_)
            return;

        BCOM.LineElement contour = Task.GetLineContour();
        BCOM.LineElement cross = 
            ElementHelper.createCrossInContour(contour);

        BCOM.Level level = ElementHelper.GetOrCreateLevel(LEVEL_PROJECTION_NAME);

        FrameList.AddProjection(contour, "contour", level);
        FrameList.AddProjection(cross, "cross", level);

        // 2-ая проекция:
        var contour2 = (BCOM.LineElement)contour.Clone();
        var cross2 = (BCOM.LineElement)cross.Clone();

        contour2.Move(App.Point3dScale(Task.DepthVec, Task.Depth));
        cross2.Move(App.Point3dScale(Task.DepthVec, Task.Depth));

        FrameList.AddProjection(contour2, "contour", level);
        FrameList.AddProjection(cross2, "cross", level);

        IsProjectionAdded_ = true;
    }

    public void AddPerforation()
    {
        if (IsPerforationAdded_)
            return;

        TFCOM.TFPerforatorList perfoList = AppTF.CreateTFPerforator();

        BCOM.ShapeElement perfoContour = 
            Task.GetContourShape(BCOM.MsdFillMode.Filled);
        perfoList.InitFromElement(perfoContour, 
            Task.DepthVec, Task.Depth * 1.01, App.Transform3dIdentity());

        perfoList.SetSweepMode(
            TFCOM.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi);
        //perfoList.SetSenseDist(1.01 * length / 2);
        perfoList.SetPolicy(
            TFCOM.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist);
        perfoList.SetIsVisible(false);

        FrameList.AsTFFrame.SetPerforatorList(perfoList);
        FrameList.AsTFFrame.SetSenseDistance2(Task.Depth);        
        FrameList.AsTFFrame.SetPerforatorsAreActive(true);
        FrameList.Synchronize(string.Empty);
        
        FrameList.ApplyPerforatorInModel();

        IsPerforationAdded_ = true;
    }

    public bool SetDataGroupInstance()
    {
        BCOM.Element bcomElement;
        FrameList.GetElement(out bcomElement);
        Element element = bcomElement.ToElement();

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle == null || 
                catalogEditHandle.CatalogInstanceName != null)
            {
                return false;
            }

            catalogEditHandle.InsertDataGroupCatalogInstance("Opening", "Opening");
            catalogEditHandle.UpdateInstanceDataDefaults();            

            foreach (var pair in Task.DataGroupPropsValues)
            {
                Sp3dToDataGroupMapProperty mapProp = pair.Key;
                string value = pair.Value;

                DataGroupProperty prop = catalogEditHandle.GetProperties()
                    .FirstOrDefault(x => x.Xpath == mapProp.TargetXPath);

                if (prop == null)
                {
                    prop = new DataGroupProperty(
                        mapProp.TargetName, value, mapProp.ReadOnly, mapProp.Visible);
                    prop.Xpath = mapProp.TargetXPath;
                    catalogEditHandle.Properties.Add(prop);
                }
                catalogEditHandle.SetValue(prop, value);
            }

            int res = catalogEditHandle.Rewrite((int)BCOM.MsdDrawingMode.Normal);
            return res == 0;

            // TODO решить проблему вылета при команде Modify DataGroup Instance
        }
    }


    public bool AddToModel(bool recoverSettings = true, BCOM.ModelReference model = null)
    {
        model = model ?? App.ActiveModelReference;
        bool res = false;

        BCOM.Level level = ElementHelper.GetOrCreateLevel(LEVEL_NAME);
        App.ActiveSettings.SetByLevel(level);

        if (recoverSettings)
        {
            ElementHelper.RunByRecovertingSettings(() =>
            {
                res = addToModel_(model);
            });
        }
        else
        {
            res = addToModel_(model);
        }
        // DataGroup свойства:
        res &= SetDataGroupInstance();

        return res;
    }

    private bool addToModel_(BCOM.ModelReference model = null)
    {
        model = model ?? App.ActiveModelReference;

        BCOM.Element element;
        FrameList.GetElement(out element);

        if (element.ID != 0 && element.ModelReference == model)
            return true;

        FrameList.AddToModel(model);
        FrameList.GetElement(out element);

        bool res = element.ID != 0;
        return res;
    }


    


    public const string CELL_NAME = "Opening";
    public const string CATALOG_TYPE_NAME = "Opening";
    public const string CATALOG_INSTANCE_NAME = "Opening";
    public const string LEVEL_NAME = "C-OPENING-BOUNDARY";
    public const string LEVEL_PROJECTION_NAME = "C-OPENING-SYMBOL";

}
}
