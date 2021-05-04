using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Shared;
using System.Linq;

namespace Shared.Bentley 
{
    [Serializable]
    public class Sp3dToDataGroupMapProperty
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public string TargetXPath { get; set; }
        [XmlAttribute]
        public string TargetName { get; set; }
        [XmlAttribute]
        public bool Visible { get; set; } = true;
        [XmlAttribute]
        public bool ReadOnly { get; set; }

        [XmlArray]
        public List<string> Sp3dXmlPaths { get; set; }
        
        [XmlArrayItem("Item")]
        public List<ValuesMapItem> ValuesMap { get; set; }

        public string getMapValue(string value)
        {
            if (ValuesMap == null || ValuesMap.Count == 0)
                return value;
            
            var mapValue = ValuesMap.FirstOrDefault(x => x.Key == value);
            return mapValue != null ? mapValue.Value : value;
        }
    }

    [Serializable]
    public class ValuesMapItem
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlAttribute]
        public string Value { get; set; }

    }
}
