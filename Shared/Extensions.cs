﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;

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

    public static bool ContainsAny<T>(this ICollection<T> coll, params T[] items)
    {
        foreach(T item in items)
        {
            if (coll.Contains(item))
                return true;
        }
        return false;
    }
    public static bool ContainsAny(this string text, params string[] items)
    {
        foreach(string item in items)
        {
            if (text.Contains(item))
                return true;
        }
        return false;
    }

    public static XElement getChildByRegexPath(this XElement source, string path, 
        out string propName)
    {
        propName = null;

        XElement curElement = source;
        string[] spath = path.Split('/');
        for(int i = 0; i < spath.Length; ++i)
        {
            var regex = new Regex($"^{spath[i]}$");
            var remCur = curElement;
            foreach(XElement subEl in curElement.Nodes())
            {
                if (regex.IsMatch(subEl.Name.LocalName))
                {
                    if (i == spath.Length - 1)
                    {
                        propName = subEl.Name.LocalName;
                        return subEl;
                    }
                    
                    curElement = subEl;
                    break;
                }
            }
            if (remCur == curElement)
                break;
        }

        return null;
    }

}
}
