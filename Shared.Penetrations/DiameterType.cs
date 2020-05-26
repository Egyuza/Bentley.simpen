using System;

namespace Embedded.Penetrations.Shared
{
public class DiameterType : IComparable<DiameterType>
{
    public long Number { get; private set; }

    /// <summary> "Типоразмер" проходки </summary>
    public string TypeSize => string.Format("{0}x{1}", diameter_, thickness_);

    public DiameterType(long number)
    {
        this.Number = number;
        diameter_ =
        thickness_ = 0;
    }

    public DiameterType(long number, float diameter, float thickness)
    {
        this.Number = number;
        this.diameter_ = diameter;
        this.thickness_ = thickness;
    }

    public int CompareTo(DiameterType other) => Number.CompareTo(other.Number);
    public override int GetHashCode() => (int)Number;

    public override bool Equals(object obj)
    {
        if ((obj as DiameterType) != null)
            return this.CompareTo((DiameterType)obj) == 0;
        return base.Equals(obj);
    }

    public bool Equals(DiameterType obj)
    {
        return this.CompareTo(obj) == 0;
    }

    public override string ToString() => 
        string.Format("{0} ({1})", Number, TypeSize);

    public static DiameterType Parse(string text)
    {
        long number = long.Parse(text.Split(' ')[0]);
        text = text.Split(' ')[1].TrimStart('(').TrimEnd(')');

        float diameter = float.Parse(text.Split('x')[0]);
        float thickness = float.Parse(text.Split('x')[1]);

        return new DiameterType(number, diameter, thickness);
    }

    private float diameter_;
    private float thickness_;
}
}
