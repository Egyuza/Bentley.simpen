using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using System.Linq;
using Shared;
using Bentley.Interop.MicroStationDGN;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared.Mapping
{
public class TagsToDataGroupConverter : BentleyInteropBase
{
    public static void Run() 
    {
        BCOM.ElementScanCriteria scanCriteria = new BCOM.ElementScanCriteriaClass();
        scanCriteria.ExcludeAllTypes();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeType(BCOM.MsdElementType.CellHeader);

        scanRecurse(App.ActiveModelReference, scanCriteria);
    }

    private static void scanRecurse(BCOM.ModelReference model, BCOM.ElementScanCriteria criteria)
    {
        BCOM.ElementEnumerator iter = App.ActiveModelReference.Scan(criteria);

        var errorColl = new Dictionary<BCOM.Element, List<TagToDataGroupMapProperty>>();
        var successList = new List<BCOM.Element>();
        int summaryCount = 0;

        iter.Reset();
        while (iter.MoveNext())
        {
            IEnumerable<TagToDataGroupMapProperty> mapTags;
            if (!ScanElementHasMappingTags(iter.Current, out mapTags))
                continue;            
            
            ++summaryCount;
            var skippedProps = new List<TagToDataGroupMapProperty>();

            foreach (TagToDataGroupMapProperty mapTag in mapTags)
            {
                bool res = DataGroupHelper.SetDataGroupPropertyValue(iter.Current, 
                    mapTag.DataGroupCatalogType, mapTag.DataGroupInstance, 
                    mapTag.DataGroupXPath, mapTag.TagName, mapTag.Value);

                if (!res)
                {
                    skippedProps.Add(mapTag);
                }
            }

            if (skippedProps.Count == 0)
            {
                successList.Add(iter.Current);
            }
            else
            {
                errorColl.Add(iter.Current, skippedProps);
            }
        }

        string brief = $"Команда 'Экспорт свойств из Tags в DataGroup', успешно: {successList.Count()}/{summaryCount}";

        var builder = new StringBuilder();

        builder.AppendLine($"*** С ошибками: {errorColl.Count()} из {summaryCount}");
        if (errorColl.Count() > 0)
        {
            foreach(var pair in errorColl)
            {
                var element = pair.Key;
                var errProps = pair.Value;

                builder.AppendLine(element.ID.ToString() + ":");
                foreach(TagToDataGroupMapProperty prop in errProps)
                {
                    builder.AppendLine("    -" + XmlSerializerEx.ToXml(prop));
                }
            }
        }
        builder.Append("\n\n");

        builder.AppendLine($"*** Успешно: {successList.Count()} из {summaryCount}");
        if (successList.Count() > 0)
        {
            foreach(var element in successList)
            {
                builder.AppendLine(element.ID.ToString());
            }
        }

    #if V8i
        App.MessageCenter.AddMessage(brief, builder.ToString(), BCOM.MsdMessageCenterPriority.Info, true);
    #elif CONNECT
        Bentley.MstnPlatformNET.MessageCenter.Instance.ShowMessage(MessageType.Info, brief, builder.ToString(), MessageAlert.Dialog);
    #endif

        // TODO ОБРАБОТКА РЕФЕРЕНСОВ
        //foreach (BCOM.Attachment attachment in model.Attachments)
        //{
        //    if (!attachment.IsActive || !attachment.IsMissingFile || !attachment.IsMissingModel)
        //        return;

        //    ModelReference modelRef = 
        //        App.MdlGetModelReferenceFromModelRefP(attachment.MdlModelRefP());
        //    scanRecurse(modelRef, criteria);
        //}
    }

    public static bool ScanElementHasMappingTags(BCOM.Element comElement, out 
        IEnumerable<TagToDataGroupMapProperty> mapTags)
    {
        var listProps = new List<TagToDataGroupMapProperty>();         
        foreach (TagToDataGroupMapProperty item in TagsToDataGroupMapping.Instance.Items)
        {
            BCOM.TagElement tag = comElement.GetTags().FirstOrDefault(x => 
                x.TagSetName.Equals(item.TagSetName) && x.TagDefinitionName.Equals(item.TagName));
            
            if (tag != null)
            {
                item.Value = tag.Value;
                listProps.Add(item);
            }
        }

        mapTags = listProps.Count != 0 ? listProps : null;
        return listProps.Count != 0;
    }

    public static bool ScanElementHasMappingDataGroupProps(
        BCOM.Element comElement, out IEnumerable<TagToDataGroupMapProperty> mapProperties)
    {
        var listProps = new List<TagToDataGroupMapProperty>();        
        foreach (TagToDataGroupMapProperty item in TagsToDataGroupMapping.Instance.Items)
        {
            object dgValue = comElement.GetDataGroupPropertyValue(
                item.DataGroupXPath, item.DataGroupCatalogType);

            if (dgValue != null)
            {
                item.Value = dgValue;
                listProps.Add(item);
            }
        }

        mapProperties = listProps.Count != 0 ? listProps : null;
        return listProps.Count != 0;
    }

    public static bool SetMapTagOnElement(BCOM.Element element,
        TagToDataGroupMapProperty mapProperty)
    {
        object propValue = DataGroupHelper.GetDataGroupPropertyValue(
                element, mapProperty.DataGroupXPath);

        if (propValue == null)
            return false;

        MsdTagType tagType;
        if (propValue is string)
            tagType = MsdTagType.Character;            
        else if (propValue is double)
            tagType = MsdTagType.Double;
        else if (propValue is int)
            tagType = MsdTagType.ShortInteger;
        else if (propValue is long)
            tagType = MsdTagType.LongInteger;
        else if (propValue is Byte[])
            tagType = MsdTagType.ShortInteger;
        else
            tagType = MsdTagType.Character;

        return ElementHelper.setTagOnElement(element, mapProperty.TagSetName,
            mapProperty.TagName, propValue, tagType);
    }

}
}
