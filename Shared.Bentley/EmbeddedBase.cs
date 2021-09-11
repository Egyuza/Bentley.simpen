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

namespace Shared.Bentley
{
public abstract class EmbeddedBase : BentleyInteropBase
{
    public TFCOM.TFFrameListClass FrameList { get; private set; }
    public BCOM.Element GetElement() => FrameList.Get3DElement();
    public IEnumerable <BCOM.Element> GetProjections() => 
        FrameList?.GetProjectionElements();

    private bool IsPerforationAdded_;
    private bool IsProjectionAdded_;

    public abstract string CellName { get; }
    public abstract string LevelName { get; }
    public abstract string CatalogTypeName { get; }
    public abstract string CatalogInstanceName { get; }

    public abstract IEnumerable<BCOM.Element> GetBodyElements();
    public abstract IEnumerable<ProjectionInfo> GetProjectionInfoList();
    public abstract TFCOM.TFPerforatorList GetPerfoList();
    public abstract double GetPerfoSenseDistance();

    public abstract Dictionary<Sp3dToDataGroupMapProperty, string> 
        DataGroupPropsValues { get; }

    private bool IsInitialized_;

    public void Initialize()
    {
        if (IsInitialized_)
            return;

        BCOM.Level level = ElementHelper.GetOrCreateLevel(LevelName);

        FrameList = new TFCOM.TFFrameListClass();

        IEnumerable<BCOM.Element> bodyElements = GetBodyElements();
        foreach(BCOM.Element element in bodyElements)
        {
            element.Level = level;
            ElementHelper.setSymbologyByLevel(element);
            FrameList.Add3DElement(element);
        }

        FrameList.SetName(CellName);

        IsInitialized_ = true;
    }

    public void AddProjection()
    {
        if (IsProjectionAdded_)
            return;

        CatchExceptionsByMessageCenter(() =>{

            IEnumerable<ProjectionInfo> projInfoList = GetProjectionInfoList();

            foreach(ProjectionInfo projInfo in projInfoList)
            {
                BCOM.Level level = ElementHelper.GetOrCreateLevel(projInfo.LevelName);
                FrameList.AddProjection(
                    projInfo.Element, projInfo.ProjectionName, level);
            }

            IsProjectionAdded_ = true;
        });
    }

    public void AddPerforation()
    {
        if (IsPerforationAdded_)
            return;

        CatchExceptionsByMessageCenter(() =>{

            TFCOM.TFPerforatorList perfoList = GetPerfoList();

            perfoList.SetSweepMode(
                TFCOM.TFdPerforatorSweepMode.tfdPerforatorSweepModeBi);
            //perfoList.SetSenseDist(1.01 * length / 2);
            perfoList.SetPolicy(
                TFCOM.TFdPerforatorPolicy.tfdPerforatorPolicyThroughHoleWithinSenseDist);
            perfoList.SetIsVisible(false);

            FrameList.AsTFFrame.SetPerforatorList(perfoList);
            FrameList.AsTFFrame.SetSenseDistance2(GetPerfoSenseDistance());
            FrameList.AsTFFrame.SetPerforatorsAreActive(true);
            FrameList.Synchronize(string.Empty);

            FrameList.ApplyPerforatorInModel();

            IsPerforationAdded_ = true;
        });
    }

    public bool SetDataGroupInstance()
    {
        bool res = false;

        CatchExceptionsByMessageCenter(() =>{
            res = SetDataGroupInstance_();
        });

        return res;
    }

    private bool SetDataGroupInstance_()
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

            catalogEditHandle.InsertDataGroupCatalogInstance(
                CatalogTypeName, CatalogInstanceName);
            catalogEditHandle.UpdateInstanceDataDefaults();            

            foreach (var pair in DataGroupPropsValues)
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

        CatchExceptionsByMessageCenter(() =>{
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
            res &= SetDataGroupInstance_();
        });

        return res;
    }

    private bool addToModel_(BCOM.ModelReference model = null)
    {
        model = model ?? App.ActiveModelReference;

        BCOM.Level level = ElementHelper.GetOrCreateLevel(LevelName);
        App.ActiveSettings.SetByLevel(level);

        BCOM.Element element;
        FrameList.GetElement(out element);

        if (element.ID != 0 && element.ModelReference == model)
            return true;

        FrameList.AddToModel(model);
        FrameList.GetElement(out element);

        bool res = element.ID != 0;
        return res;
    }
}
}
