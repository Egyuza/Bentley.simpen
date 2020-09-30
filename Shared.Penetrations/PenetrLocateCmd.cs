using System;
using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using Shared.Bentley;
using System.Collections.Generic;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
using Bentley.Internal.MicroStation.Elements;
#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared
{
public class PenetrLocateCmd : BCOM.ILocateCommandEvents
{   
    internal static void StartCommand()
    {
        app_.CommandState.StartLocate(new PenetrLocateCmd());
    }

    public PenetrLocateCmd()
    {
        
    }

    public void Start()
    {
        BCOM.LocateCriteria lc = app_.CommandState.CreateLocateCriteria(false);
        lc.ExcludeAllTypes();
        lc.IncludeType(BCOM.MsdElementType.CellHeader);        
        app_.CommandState.SetLocateCriteria(lc);

        app_.CommandState.EnableAccuSnap();
        app_.CommandState.ElementDisplayEnabled = true;
        
        //app_.CommandState.StartDynamics();

        //ensureLocateEnabled = true;

        //// TODO
        //app_.SetCExpressionValue("userPrefsP->smartGeomFlags.locateSurfaces", 4);
    }

    public void Cleanup()
    {
        app_.CommandState.StopDynamics();
    }

    public void DataPoint(ref BCOM.Point3d Point, BCOM.View View)
    {
        //throw new NotImplementedException();
    }

    public void Dynamics(
        ref BCOM.Point3d Point, BCOM.View View, BCOM.MsdDrawingMode DrawMode)
    {
        
    }

    public void Accept(BCOM.Element Element, ref BCOM.Point3d Point, BCOM.View View)
    {
        View.Redraw();
    }

    public void LocateFailed()
    {
        
    }

    public void LocateFilter(BCOM.Element element, ref BCOM.Point3d point, ref bool accepted)
    {
        accepted = false;
        if (element != null && element.IsPenetrationCell())
        {
            accepted = true;            
            startLeaderPrimitive(element);
        }
    }

    public void LocateReset()
    {
        
    }

    private void startLeaderPrimitive(BCOM.Element bcomElement)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return;
        
        var schemas = DataGroupDocument.Instance.CatalogSchemas.Schemas;

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            string code = "### code";
            string name = "### name";

            foreach (DataGroupProperty property in catalogEditHandle.GetProperties())
            {
                if (property?.Xpath == "EmbPart/@PartCode") 
                    code = property.Value.ToString();
                else if (property?.Xpath == "EmbPart/@CatalogName")
                    name = property.Value.ToString();
            }

            PenetrLeaderPrimitiveCmd.StartCommand(new PenetrLeaderInfo() {
                TextLines = new List<string>() {code, name}
            });

            // TODO решить проблему вылета при команде Modify DataGroup Instance
        }
    }

    private static BCOM.Application app_ => BMI.Utilities.ComApp;
    private long locatedElementId_;
}
}
