using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Shared
{
static class Extensions
{
    public static void ShowMessage(this Exception ex)
    {
        MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
}
}
