using Shared;
using Shared.Bentley;
using System;
using System.Collections.Generic;
using System.Text;

#if V8i
using Bentley.Internal.MicroStation.Elements;

#elif CONNECT
using Bentley.DgnPlatformNET.Elements;
#endif


namespace Embedded.Penetrations.Shared.sp3d
{
static class Sp3dExtensions
{

#if V8i

    public static bool isElementSp3dTask_Old(this Element element, out Sp3dTask_Old task)
    {
        P3DHangerPipeSupport pipe = null;
        P3DHangerStdComponent component = null;
        P3DEquipment equipment = null;
        task = null;

        //string xmlSummary = string.Empty;
        var xmlSumBilder = new System.Text.StringBuilder();
        
        if (element == null)
            return false;
        
        IEnumerable<string> summaryXmlData = ElementHelper.getSp3dXmlData(element);
        foreach( string xmlData in summaryXmlData)
        {
            if (xmlData.StartsWith("<P3DEquipment"))
            {                
                equipment = XmlSerializerEx.FromXml<P3DEquipment>(xmlData);
            }
            else if (xmlData.StartsWith("<P3DHangerPipeSupport"))
            {
                pipe = XmlSerializerEx.FromXml<P3DHangerPipeSupport>(xmlData);
            }
            else if (xmlData.StartsWith("<P3DHangerStdComponent"))
            {        
                component = XmlSerializerEx.FromXml<P3DHangerStdComponent>(xmlData);
            }
        }

        if (equipment != null)
        {
            task = new Sp3dTask_Old(equipment, summaryXmlData);
        }
        else if (pipe != null && component != null)
        {
            task = new Sp3dTask_Old(pipe, component, summaryXmlData);
        }

        return task != null;
    }

#elif CONNECT

    public static bool isElementSp3dTask_Old(this Element element, out Sp3dTask_Old task)
    {
        P3DHangerPipeSupport pipe = null;
        P3DHangerStdComponent component = null;
        P3DEquipment equipment = null;
        task = null;

        //string xmlSummary = string.Empty;
        var xmlSumBilder = new System.Text.StringBuilder();
        
        if (element == null)
            return false;
        
        IEnumerable<string> summaryXmlData = ElementHelper.getSp3dXmlData(element);
        foreach( string xmlData in summaryXmlData)
        {
            if (xmlData.StartsWith("<P3DEquipment"))
            {                
                equipment = XmlSerializerEx.FromXml<P3DEquipment>(xmlData);
            }
            else if (xmlData.StartsWith("<P3DHangerPipeSupport"))
            {
                pipe = XmlSerializerEx.FromXml<P3DHangerPipeSupport>(xmlData);
            }
            else if (xmlData.StartsWith("<P3DHangerStdComponent"))
            {        
                component = XmlSerializerEx.FromXml<P3DHangerStdComponent>(xmlData);
            }
        }

        if (equipment != null)
        {
            task = new Sp3dTask_Old(equipment, summaryXmlData);
        }
        else if (pipe != null && component != null)
        {
            task = new Sp3dTask_Old(pipe, component, summaryXmlData);
        }

        return task != null;
    }

#endif
}
}
