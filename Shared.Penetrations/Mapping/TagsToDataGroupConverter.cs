﻿using Bentley.Interop.MicroStationDGN;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
using System.Linq;

#if V8i
using Bentley.Internal.MicroStation.Elements;
using BMI = Bentley.MicroStation.InteropServices;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
using Bentley.MstnPlatformNET;
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Embedded.Penetrations.Shared.Mapping
{
public class TagsToDataGroupConverter : BentleyInteropBase
{
    public static void Run() 
    {
        ElementScanCriteria scanCriteria = new ElementScanCriteriaClass();
        scanCriteria.ExcludeAllTypes();
        scanCriteria.ExcludeNonGraphical();
        scanCriteria.IncludeType(MsdElementType.CellHeader);

        scanRecurse(App.ActiveModelReference, scanCriteria);
    }

    private static void scanRecurse(ModelReference model, ElementScanCriteria criteria)
    {
        ElementEnumerator iter = App.ActiveModelReference.Scan(criteria);

        iter.Reset();        
        while (iter.MoveNext())
        {
            TagToDataGroupMapProperty mapProp;
            if (!IsElementMatchesMapping(iter.Current, out mapProp))
                continue;            
            
            bool res = DataGroupHelper.SetDataGroupPropertyValue(iter.Current, 
                mapProp.DataGroupCatalogType, mapProp.DataGroupInstance, 
                mapProp.DataGroupXPath, mapProp.TagName, mapProp.Value);

            ;    

            //setMapTagOnElement(iter.Current, mapProperty);
        }

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

    private static bool IsElementMatchesMapping(BCOM.Element comElement, out 
        TagToDataGroupMapProperty mapProperty)
    {
        mapProperty = null;
        foreach (TagToDataGroupMapProperty item in TagsToDataGroupMapping.Instance.Items)
        {
            TagElement tag = comElement.GetTags().FirstOrDefault(x => 
                x.TagSetName.Equals(item.TagSetName) && x.TagDefinitionName.Equals(item.TagName));
            
            item.Value = tag.Value;
            mapProperty = item;
            return true;        

            //if (DataGroupHelper.IsElementOfCatalogType(comElement, item.DataGroupCatalogType))
            //{
            //    mapProperty = item;
            //    return true;
            //}
        }
        return false;
    }

    //private static void setMapTagOnElement(BCOM.Element element,
    //    TagToDataGroupMapProperty mapProperty)
    //{
    //    object propValue = DataGroupHelper.GetDataGroupPropertyValue(
    //            element, mapProperty.DataGroupXPath);

    //    MsdTagType tagType = 
    //        (MsdTagType)Enum.Parse(typeof(MsdTagType), mapProperty.TagType);

    //    dynamic typedValue;
    //    switch (tagType)
    //    {
    //        case MsdTagType.Character:
    //            typedValue = propValue.ToString(); break;
    //        case MsdTagType.Double:
    //            typedValue = (double)propValue; break;
    //        case MsdTagType.ShortInteger:
    //            typedValue = (int)propValue; break;
    //        case MsdTagType.LongInteger:
    //            typedValue = (long)propValue; break;
    //        case MsdTagType.Binary:
    //            typedValue = (Byte[])propValue; break;
    //        default:
    //            typedValue = propValue; break;
    //    }

    //    ElementHelper.setTagOnElement(element, mapProperty.TagSetName,
    //        mapProperty.TagName, propValue, tagType);
    //}


    protected static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
