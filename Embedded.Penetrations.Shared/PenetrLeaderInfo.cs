using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;

namespace Embedded.Penetrations.Shared
{

public enum ArrowTypeEnum
{
    Arrow,
    Point
}

public class PenetrLeaderInfo : BentleyInteropBase
{
    public List<string> TextLines {get; set;}

    public double Arrow_dX {get; set;}
    public double Arrow_dY {get; set;}
    public ArrowTypeEnum ArrowType {get; set;}
    public double GapTextBefore {get; set;}
    public double GapTextAfter {get; set;}

    public PenetrLeaderInfo()
    {
        ArrowType = (ArrowTypeEnum)Enum.Parse(typeof(ArrowTypeEnum), 
            getCfgVarValueSting(LEADER_ARROW_TYPE, nameof(ArrowTypeEnum.Arrow)));

        Arrow_dX = Scaled(getCfgVarValueDouble(LEADER_ARROW_DX, 2.8));
        Arrow_dY = Scaled(getCfgVarValueDouble(LEADER_ARROW_DX, 2.8));
        GapTextBefore = Scaled(getCfgVarValueDouble(LEADER_TEXT_BEFORE, 2.5));
        GapTextAfter = Scaled(getCfgVarValueDouble(LEADER_TEXT_AFTER, 2.5));
    }

    /// <summary> 
    /// Результирующий масштабный коэффициент для перевода геометрии в 
    /// миллиметрах на модель чертежа
    /// </summary>
    /// UOR - Units of resolution - разрешение dgn-файла, на основе кот.
    /// расчитываются все координаты модели
    public double Scale
    {
        get
        {
            var x1 = ElementHelper.getActiveAnnotationScale();
            var x2 = App.ActiveModelReference.SubUnitsPerMasterUnit;
            
            return ElementHelper.getActiveAnnotationScale()/
                App.ActiveModelReference.SubUnitsPerMasterUnit;
        }
    }
    /// <summary>
    /// Получение действительного значения
    /// </summary>
    public double UnScaled(double value)
    {
        return value / Scale;
    }

    /// <summary>
    /// Приведение значения к текущему масштабу аннотаций модели
    /// </summary>
    public double Scaled(double value)
    {
        return value * Scale;
    }


    private string getCfgVarValueSting(string name, string defaultValue)
    {
        if (App.ActiveWorkspace.IsConfigurationVariableDefined(name))
        {
            return App.ActiveWorkspace.ConfigurationVariableValue(name);
        }
        return defaultValue;
    }

    private double getCfgVarValueDouble(string name, double defaultValue)
    {
        if (App.ActiveWorkspace.IsConfigurationVariableDefined(name))
        {
            return double.Parse(App.ActiveWorkspace.ConfigurationVariableValue(name));
        }
        return defaultValue;
    }

    //private T getCfgVarValue<T>(string name, T defaultValue)
    //{
    //    if (App.ActiveWorkspace.IsConfigurationVariableDefined(name))
    //    {
    //        dynamic valueStr = App.ActiveWorkspace.ConfigurationVariableValue(name);

    //        if (typeof(T) == typeof(string))
    //            return (T)valueStr;
    //        else if (typeof(T) == typeof(double))
    //            return (T)(double.Parse(valueStr));
    //    }
    //    return defaultValue;
    //}

    public static BCOM.Level Level => ElementHelper.GetOrCreateLevel("C-LEADER-EMB");

    public const string LEADER_ARROW_DX = "AEP_EMB_LEADER_ARROW_DX";
    public const string LEADER_ARROW_DY = "AEP_EMB_LEADER_ARROW_DY";
    public const string LEADER_ARROW_TYPE = "AEP_EMB_LEADER_ARROW_TYPE";
    public const string LEADER_TEXT_BEFORE = "AEP_EMB_LEADER_TEXT_BEFORE";
    public const string LEADER_TEXT_AFTER = "AEP_EMB_LEADER_TEXT_AFTER";
}
}
