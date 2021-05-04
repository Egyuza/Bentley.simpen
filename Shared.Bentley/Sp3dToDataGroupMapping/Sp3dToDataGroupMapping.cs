using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Shared;

using BCOM = Bentley.Interop.MicroStationDGN;
using Shared.Bentley;
using System.Xml.Linq;
#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared.Bentley
{
[Serializable]
[XmlRoot("Sp3dToDataGroupMapping")]
public class Sp3dToDataGroupMapping
{
    [XmlArrayItem("Item")]
    public Sp3dToDataGroupMapProperty[] Items { get; set; }

    protected Sp3dToDataGroupMapping() {}

    protected static Sp3dToDataGroupMapping read(string mapFilePath) =>
        XmlSerializerEx.FromXmlFile<Sp3dToDataGroupMapping>(mapFilePath);

    protected static Sp3dToDataGroupMapping instance_;


    public void LoadValuesFromXDoc(XDocument xDoc,
        Dictionary<Sp3dToDataGroupMapProperty, string> targetColl)
    {
        targetColl = targetColl ?? new Dictionary<Sp3dToDataGroupMapProperty, string>();

        foreach (var propMap in Items)
        {  
            if (propMap.TargetXPath == null)
                continue;
            
            foreach (string path in propMap.Sp3dXmlPaths)
            {
                string propName;
                var xEl = xDoc.Root.GetChildByRegexPath(path, out propName);

                if (xEl != null)
                {
                    string value = propMap.getMapValue(xEl.Value);
                    targetColl.Add(propMap, value);
                }
            }
        }
    }
}
}
