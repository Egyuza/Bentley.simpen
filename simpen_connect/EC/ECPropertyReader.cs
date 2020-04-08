using System;
using Bentley.DgnPlatformNET;
using Bentley.DgnPlatformNET.Elements;
using Bentley.ECObjects.Schema;
using Bentley.DgnPlatformNET.DgnEC;
using Bentley.ECObjects.Instance;
using System.Linq;

namespace simpen_cn
{
/// <summary>
/// Класс - средство записи EC-свойств в элемент
/// </summary>
public class ECPropertyReader
{
    private readonly Element element;
    private readonly IDgnECInstance ecInst;
    
    private ECPropertyReader(Element element, string className)
    {
        if (element == null)
            throw new ArgumentNullException("element");
        
        // !проверка, что элемент добавлен в модель
        if (element.DgnModel == null || element.ElementId == null)
            throw new ArgumentException(string.Format(
                "Couldn't read EC-properties from non-model element"));
        
        this.element = element;

        using (DgnECInstanceCollection ecInstances = DgnECManager.Manager.
            GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses))
        {
            ecInst = ecInstances.FirstOrDefault(x =>
                x.ClassDefinition.Name == className); // EnumString.ToString(instType));
        }
    }

    ///// <summary>
    /////  Получение экземпляра средства записи EC-свойств в dgn-элемент.
    /////  Может быть NULL.
    ///// </summary>
    //public static ECPropertyReader TryGet(
    //    Element element, ECClassTypeEnum instType)
    //{       
    //    var reader = new ECPropertyReader(element, instType);        
    //    return reader.ecInst != null ? reader : null;
    //}

    public IECPropertyValue Get(string propName)
    {
        IECPropertyValue propVal = ecInst.GetPropertyValue(propName);

        if (propVal == null || propVal.IsNull)
        {
            //throw new SPDSException(string.Format(
            //    "Element doesn't have property", propName));
            return null;
        }
        return propVal;        
    }

    public static void Test(Element element)
    {
        if (element == null)
            return;
        
        // !проверка, что элемент добавлен в модель
        if (element.DgnModel == null || element.ElementId == null)
            throw new ArgumentException(string.Format(
                "Couldn't read EC-properties from non-model element"));
        



        TextNodeElement txtEl = element as TextNodeElement;
        if (txtEl != null)
        {
            foreach (var id in txtEl.GetTextPartIds(new TextQueryOptions()))
            {
                var txtPart = txtEl.GetTextPart(id);
                TextBlockProperties props = txtPart.GetProperties();
                //txtPart.GetRunPropertiesForAdd()
            }
        }

       
        DgnECInstanceCollection ecInstances = DgnECManager.Manager.
            GetElementProperties(element, ECQueryProcessFlags.SearchAllClasses);

        //foreach(var linkId in element.GetLinkageIds())
        //{
        //    var link = element.GetLinkage(linkId);
        //    link = link;

        //    for (int i = 0; i < link.Size; ++i)
        //    {
        //        try
        //        {
        //            string str = link.ReadString();
        //        }
        //        catch (Exception)
        //        {                    
        //        }
        //    }

        //}
        

        System.Text.StringBuilder bldr = new System.Text.StringBuilder();

        foreach (IDgnECInstance inst in ecInstances)
        {
            bldr.AppendLine(string.Format("Inst: <{0}> ID=<{1}>",
                inst.ClassDefinition.Name, inst.InstanceId));
            foreach (IECPropertyValue propVal in inst)
            {
                if (propVal.Property.Name == "Links")
                {                    
                }

                if (propVal.Type is ECArrayType)
                {
                    try
                    {
                        foreach (var item in propVal as ECDArrayValue)
                        {
                            
                        }
                    }
                    catch (Exception)
                    {
                                        
                    }
                }


                try
                {
                    object value = "ERROR";
                    propVal.TryGetNativeValue(out value);                        

                     bldr.AppendLine(string.Format(
                        "\tProp: <{0}>, Type=<{1}>, StringValue=<{2}>", 
                        propVal.Property.Name, propVal.Type, value.ToString()));
                }
                catch (Exception)
                {
                    
                }

            }
            bldr.AppendLine();
        }
        
    }


}
}
