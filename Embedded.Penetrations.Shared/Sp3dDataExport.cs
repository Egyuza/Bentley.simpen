using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using System.Linq;
using Shared;
using System.Xml.Linq;
using System.IO;

namespace Embedded.Penetrations.Shared
{
class Sp3dDataExport : BentleyInteropBase
{
    public static void ExportToCsv()
    {
        BCOM.ModelReference model = App.ActiveModelReference;

        BCOM.ElementScanCriteria criteria = new BCOM.ElementScanCriteriaClass();
        criteria.ExcludeAllTypes();
        criteria.ExcludeNonGraphical();
        criteria.IncludeType(BCOM.MsdElementType.CellHeader);

        BCOM.ElementEnumerator iter = App.ActiveModelReference.Scan(criteria);

        var builder = new StringBuilder();
        string dltr = ";";

        while (iter.MoveNext())
        {        
            BCOM.Element element = iter.Current;
            XDocument xDoc = ElementHelper.getSp3dXDocument(element.ToElement());

            var dgPropColl = new Dictionary<Sp3dToDataGroupMapProperty, string>();
            Sp3dToDGMapping.Instance.LoadValuesFromXDoc(xDoc, dgPropColl, true);

            if (builder.Length == 0)
            {   // строка заголовков:
                builder.Append("ElementId");
                foreach (var pair in dgPropColl)
                {
                    builder.Append(dltr + pair.Key.TargetName);
                }
                builder.AppendLine();
            }
            
            builder.Append(element.ID);
            foreach (var pair in dgPropColl)
            {
                builder.Append(dltr + pair.Value);
            }
            builder.AppendLine();
        }

        string path = Path.ChangeExtension(App.ActiveDesignFile.FullName, ".csv");
        File.WriteAllText(path, builder.ToString());        
        
        if (File.Exists(path))
        {
            System.Diagnostics.Process.Start(path);
            App.MessageCenter.AddMessage($"SUCCESS: экспорт '{path}'", "",
                BCOM.MsdMessageCenterPriority.None, false);
        }
        else
        {
            App.MessageCenter.AddMessage($"FAILED: экспорт '{path}'", "", 
                BCOM.MsdMessageCenterPriority.Warning, false);
        }
    }
}
}
