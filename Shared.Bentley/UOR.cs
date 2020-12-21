using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Shared.Bentley
{
public class UOR : BentleyInteropBase
{
    private BCOM.ModelReference target_;

    public double perMaster => target_.UORsPerMasterUnit;
    public double subPerMaster => target_.SubUnitsPerMasterUnit;
    public double perStorage => target_.UORsPerStorageUnit;
    public double perSub => target_.UORsPerSubUnit;

    public double activePerMaster => App.ActiveModelReference.UORsPerMasterUnit;
    public double activeSubPerMaster => App.ActiveModelReference.SubUnitsPerMasterUnit;
    public double activePerStorage => App.ActiveModelReference.UORsPerStorageUnit;
    public double activePerSub => App.ActiveModelReference.UORsPerSubUnit;

    public UOR(BCOM.ModelReference target)
    {
        target_ = target;   
    }

    public double convertToMaster(double value) => value/activeSubPerMaster;

    public static UOR getForActiveModel()
    {
        return new UOR(App.ActiveModelReference);
    }
}
}
