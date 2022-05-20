using Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace Embedded.Penetrations.Shared
{
public class PenetrInfo
{
    public readonly double pipeDiameterOutside;
    public readonly double pipeDiameterInside;
    public readonly double flangeDiameterOutside;
    public readonly double flangeDiameterInside;
    public readonly double flangeThick;
    public readonly string penCode;

    private PenetrInfo() {}

    public PenetrInfo(double pipeDiam, double pipeThick, 
        double flangeDiam, double flangeThick, string penCode)
    {
        this.pipeDiameterOutside = pipeDiam;
        this.pipeDiameterInside = pipeDiam - (2*pipeThick);

        this.flangeDiameterOutside = flangeDiam;
        this.flangeDiameterInside = this.pipeDiameterOutside;
        this.flangeThick = flangeThick;
        this.penCode = penCode;
    }

    public static long getFlangesType(string penCode)
    {
        if (penCode == "C") 
            return 5;

        try
        {
            return long.Parse(penCode);
        }
        catch (Exception ex)
        {
            Logger.Log.Error($"Не распознан тип проходки penCod='{penCode}'", ex);
            throw; 
        }
    }

    public static string getPenCode(long flangesType)
    {
        if (flangesType == 5) return "C";

        return flangesType.ToString();
    }
}
}
