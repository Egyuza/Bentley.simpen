using System;
using System.Collections.Generic;
using System.Text;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;
using System.Linq;

#if V8i
using Bentley.Internal.MicroStation.Elements;

#elif CONNECT
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
using Bentley.DgnPlatformNET.Elements;
#endif


namespace Shared.Bentley
{
public static class DataGroupHelper
{
    public static bool IsElementOfCatalogType(this BCOM.Element comElement, 
        string catalogTypeName)
    {
        Element element = ElementHelper.getElement(comElement);
        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle != null && 
                catalogTypeName.Equals(catalogEditHandle.CatalogTypeName, 
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }


    public static object GetDataGroupPropertyValue(this BCOM.Element bcomElement,
        string propertyPath, string catalogType = null)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return null;

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            foreach (DataGroupProperty property in catalogEditHandle.GetProperties())
            {
                if (catalogType != null && 
                    !catalogType.Equals(catalogEditHandle.CatalogTypeName))
                {
                    continue;
                }

                if (property.Xpath.Equals(propertyPath)) 
                {
                    return property.Value;
                }
            }
        }
        return null;
    }

    public static bool SetDataGroupPropertyValue(this BCOM.Element bcomElement,
        string catalogName, string instanceName, 
        string propXpath, string propName, object value, 
        bool readOnly = false, bool visible = true)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return false;

        //var schemas = DataGroupDocument.Instance.CatalogSchemas.Schemas; // НВС для подгрузки схем

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            if (catalogEditHandle == null || 
                (catalogEditHandle.CatalogInstanceName != null &&
                    catalogEditHandle.CatalogInstanceName != instanceName))
            {
                return false;
            }

            if (!(catalogEditHandle.HasDataGroupData() &&
                catalogEditHandle.CatalogTypeName.Equals(catalogName) &&
                catalogEditHandle.CatalogInstanceName.Equals(instanceName)))
            {
                catalogEditHandle.InsertDataGroupCatalogInstance(catalogName, instanceName);
                catalogEditHandle.UpdateInstanceDataDefaults();
            }

            DataGroupProperty prop = catalogEditHandle.Properties
                .FirstOrDefault(x => x.Xpath.Equals(propXpath));
            
            if (prop == null)
            {
                prop = new DataGroupProperty(propName, value, readOnly, visible) {
                    Xpath = propXpath
                };
                catalogEditHandle.Properties.Add(prop);
            }
            else
            {
                catalogEditHandle.SetValue(prop, value.ToString());
            }
            
            int res = catalogEditHandle.Rewrite((int)BCOM.MsdDrawingMode.Normal);

            return res == 0;

            // TODO решить проблему вылета при команде Modify DataGroup Instance
        }


        return true;
    }
}
}
