using System;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared.Bentley
{
public abstract class BentleyInteropBase
{
    protected static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

    private static TFCOM.TFApplication _tfApp;
    protected static TFCOM.TFApplication AppTF
    {
        get
        {
            return _tfApp ?? 
                (_tfApp = new TFCOM.TFApplicationList().TFApplication);
        }
    }

    protected readonly static BCOM.Point3d XAxis = App.Point3dFromXYZ(1, 0, 0);
    protected readonly static BCOM.Point3d YAxis = App.Point3dFromXYZ(0, 1, 0);
    protected readonly static BCOM.Point3d ZAxis = App.Point3dFromXYZ(0, 0, 1);
}
}
