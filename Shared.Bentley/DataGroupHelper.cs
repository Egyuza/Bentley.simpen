using System;
using System.Collections.Generic;
using System.Text;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;

using BCOM = Bentley.Interop.MicroStationDGN;
using TFCOM = Bentley.Interop.TFCom;

#if V8i
using Bentley.Internal.MicroStation.Elements;

#elif CONNECT
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Building.DataGroupSystem;
#endif


namespace Shared.Bentley
{
public class DataGroupHelper
{
    public static bool IsElementOfCatalogType(BCOM.Element comElement, 
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

        BCOM.Application app;

            
        return false;
    }


    public static object GetDataGroupPropertyValue(BCOM.Element bcomElement,
        string propertyPath, string catalogType = null)
    {
        Element element = ElementHelper.getElement(bcomElement);
        if (element == null)
            return null;

        //var schemas = DataGroupDocument.Instance.CatalogSchemas.Schemas; // НВС для позгрузки

        using (var catalogEditHandle = new CatalogEditHandle(element, true, true))
        {
            foreach (DataGroupProperty property in catalogEditHandle.GetProperties())
            {
                if (property.Xpath.Equals(propertyPath)) 
                {
                    return property.Value;
                }
            }
        }
        return null;
    }
}
}
