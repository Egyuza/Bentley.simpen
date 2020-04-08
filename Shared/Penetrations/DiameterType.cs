using System;

namespace Shared.Penetrations
{
public class DiameterType : IComparable<DiameterType>
{
    public long number { get; set; }
    private float diameter;
    private float thickness;

    public string typeSize
    {
        get { return string.Format("{0}x{1}", diameter, thickness); }
    }

    public DiameterType(long number)
    {
        this.number = number;
        diameter =
        thickness = 0;
    }

    public DiameterType(long number, float diameter, float thickness)
    {
        this.number = number;
        this.diameter = diameter;
        this.thickness = thickness;
    }

    public int CompareTo(DiameterType other)
    {
        return number.CompareTo(other.number);
    }

    public override bool Equals(object obj)
    {
        if ((obj as DiameterType) != null)
            return this.CompareTo((DiameterType)obj) == 0;
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return (int)number;
    }

    public override string ToString()
    {
        return string.Format("{0} ({1})", number, typeSize);
    }

    public static DiameterType Parse(string text)
    {
        long number = long.Parse(text.Split(' ')[0]);
        text = text.Split(' ')[1].TrimStart('(').TrimEnd(')');

        float diameter = float.Parse(text.Split('x')[0]);
        float thickness = float.Parse(text.Split('x')[1]);

        return new DiameterType(number, diameter, thickness);
    }
}

}
