using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Shared
{
static class Extensions
{
    public static void ShowMessage(this Exception ex, string text = null)
    {
        text = text == null ? ex.Message : text + $"\nException: {ex.Message}";
        MessageBox.Show(text, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static double ToDouble(this float value)
    {
        return floatToDouble(value);
    }

    public static double floatToDouble(float value)
    {
        // для точного перевода в double, 
        // в противном случае, например,
        // число 168,3 может получить хвост => 168,300000305175781
        return (double)(decimal)value;
    }

    public static bool TryRemove<TKey, TValue>(
        this IDictionary<TKey,TValue> dictionary, TKey key)
    {
        if (dictionary.ContainsKey(key))
        {
            dictionary.Remove(key);
            return true;
        }
        return false;
    }

    public static string ToStringEx<T>(this IList<T> list)
    {
        if (list == null)
            return "null";

        var builder = new StringBuilder("{");
        foreach (T item in list)
        {
            builder.Append($"'{item.ToString()}', ");
        }
        builder.Append("}");
        return builder.ToString();
    }

    public static bool IsMatch(this string text, string regexp)
    {
        var reg = new Regex(regexp);
        return reg.IsMatch(text);
    }

}
}
