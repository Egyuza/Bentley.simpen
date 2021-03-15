using Embedded.Penetrations.Shared;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Shared.Bentley.sp3d
{
class Sp3dTask_Old
{
    public P3DHangerPipeSupport pipe { get; private set; }
    public P3DHangerStdComponent component { get; private set; }
    public P3DEquipment equipment { get; private set; }

    //public string XmlSummary { get; private set; }

    public XDocument XmlDoc { get; private set; }

    public Sp3dTask_Old(P3DHangerPipeSupport pipe, P3DHangerStdComponent component, IEnumerable<string> xmlData)
    {
        this.pipe = pipe;
        this.component = component;
        readXmlDoc(xmlData);
    }

    public Sp3dTask_Old(IEnumerable<string> xmlData)
    {
        readXmlDoc(xmlData);
    }

    public Sp3dTask_Old(P3DEquipment equipment, IEnumerable<string> xmlData)
    {
        this.pipe = null;
        this.component = null;
        this.equipment = equipment;
        readXmlDoc(xmlData);
    }

    private void readXmlDoc(IEnumerable<string> xmlData)
    {
        XmlDoc = XDocument.Parse("<Root></Root>");
        if (xmlData == null)
            return;
        foreach (string xmlText in xmlData)
        {
            XmlDoc.Root.Add(XElement.Parse(xmlText));
        }


    }

    public bool isFlange()
    {
        return component?.Description == "PenFlange";
    }
    public bool isEquipment()
    {
        return equipment == null ? false :
            equipment.CatalogPartNumber.StartsWith("PenRound-");
    }
    public bool isPipe()
    {
        return component?.Description == "PenPipe";
    }
    public bool isPipeOld()
    {
        return component?.Description == "PntrtPlate-d";
    }

}
}
