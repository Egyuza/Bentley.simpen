using BCOM = Bentley.Interop.MicroStationDGN;

namespace simpen.ui
{
static class Extensions
{
    public static string ToStringEx(this BCOM.Point3d point)
    {
        return string.Format("{0}, {1}, {2}", point.X, point.Y, point.Z);
    }
}
}
