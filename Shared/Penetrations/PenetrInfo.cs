using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Penetrations
{
public class PenetrInfo
{
    public readonly double pipeDiameterOutside;
    public readonly double pipeDiameterInside;
    public readonly double flangeDiameterOutside;
    public readonly double flangeDiameterInside;
    public readonly double flangeThick;

    private PenetrInfo() {}

    public PenetrInfo(double pipeDiam, double pipeThick, 
        double flangeDiam, double flangeThick)
    {
        this.pipeDiameterOutside = pipeDiam;
        this.pipeDiameterInside = pipeDiam - (2*pipeThick);

        this.flangeDiameterOutside = flangeDiam;
        this.flangeDiameterInside = this.pipeDiameterOutside;
        this.flangeThick = flangeThick;
    }
}
}
