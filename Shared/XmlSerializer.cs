using System;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;

namespace Shared
{
/// <summary> Класс, предоставляющий упрощённые методы для сериализации и
/// десериализации классов в Xml формат </summary>
public static class XmlSerializerEx
{
    private static XmlSerializerNamespaces _namespaces;
    public static XmlSerializerNamespaces Namespaces
    {
        get
        {
            _namespaces = _namespaces ?? new XmlSerializerNamespaces();
            return _namespaces;
        }
        set { _namespaces = value; }
    }
    
    /// <summary> Десериализация из файла Xml без генерации ошибки </summary>
    /// <exception cref=
    /// "Возвращает Null в случае возникновения внутренних исключений">
    /// </exception>
    public static T TryFromXmlFile<T>(string path)
    {
        try
        {
            return FromXmlFile<T>(path);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary> Десериализация из файла Xml </summary>
    /// <exception cref="System.InvalidOperationException"></exception>
    public static T FromXmlFile<T>(string path)
    {
        T obj;
        using (FileStream fs =
            new FileStream(path, FileMode.Open, access: FileAccess.Read))
        {
            var formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
            obj = (T)formatter.Deserialize(fs);
        }
        return obj;
    }

    /// <summary> Десериализация из Xml-строки без генерации ошибки, в случае 
    /// неуспеха возвращается null </summary>
    public static T TryFromXmlEx<T>(string xml)
    {
        try
        {
            return FromXml<T>(xml);
        }
        catch (Exception)
        {
            return default(T);
        }
    }

    /// <summary> Десериализация из Xml-строки без генерации ошибки, в случае 
    /// неуспеха возвращается null </summary>
    public static bool TryFromXml<T>(string xml, out T obj)
    {
        try
        {
            obj = FromXml<T>(xml);
            return true;
        }
        catch (Exception)
        {
            obj = default(T);
            return false;
        }
    }

    /// <summary> Десериализация из Xml-строки </summary>
    /// <exception cref="System.InvalidOperationException"></exception>
    public static T FromXml<T>(string xml)
    {
        T obj;
        using (TextReader txtReader = new StringReader(xml))
        {
            var formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
            try
            {
                obj = (T)formatter.Deserialize(txtReader);
            }
            catch (Exception)
            {
                // todo ! обработка ошибок чтения xml
                throw;
                 //new Exception(
                 //   "Не удалость десериализовать класс <{0}>", typeof(T));
            }
        }
        return obj;
    }

    /// <summary> Сериализация в Xml-строку без генерации ошибки, в случае 
    /// неуспеха возвращается null </summary>
    public static string TryToXml<T>(T obj)
    {
        try
        {
            return ToXml(obj);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary> Сериализация в Xml-строку </summary>
    public static string ToXml<T>(T obj)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        TextWriter txtWriter =
            new StringWriter(sb, CultureInfo.InvariantCulture);

        var formatter = new System.Xml.Serialization.XmlSerializer(typeof(T));
        try
        {
            formatter.Serialize(txtWriter, obj, Namespaces);
        }
        catch (Exception)
        {
            throw;
            //new SPDSException(
            //    "Не удалость сериализовать класс <{0}>", typeof(T));
        }
        return sb.ToString();
    }
}
}