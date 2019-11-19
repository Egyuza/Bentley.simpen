using Bentley.Building.DataGroupSystem;
using Bentley.Building.DataGroupSystem.Serialization;
using Bentley.Ustn;
using System;

namespace simpen.ui
{

namespace Keyins
{
    static class Openings
    {
        static OpeningForm form;
    
        // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
        // выгрузка: clr unload domain simpenDomain
        public static void showForm(string unparsed)
        {
            if(form == null || form.IsDisposed)        
                form = new OpeningForm();
        
            WindowHelper.show(form);
            form.runLocatingTool();
        }
    
        public static void readData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            form.readTaskData();
        }
    
        public static void reload(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            form.reload();
        }

        public static void enableAddToModel(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            form?.enableAddToModel();
        }

        public static void sendTaskData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            form?.sendTaskData();
        }

        public static unsafe void setDgData(string unparsed)
        {
            uint result = 0;
            string[] strArray = unparsed.Split(' ');
            if (strArray.Length == 0)
                return;

            uint.TryParse(strArray[0], out result);
            if (result == 0u)
                return;

            DgnModelRef* dgnModelRefPtr = Bentley.Internal.MicroStation.ModelReference.Active.DgnModelRefPtr;
            Bentley.Internal.MicroStation.Elements.Element element = 
                Bentley.Internal.MicroStation.Elements.Element.FromFilePosition(result, dgnModelRefPtr);

            if (element == null)
                return;

            CatalogEditHandle catalogEditHandle = new CatalogEditHandle(element, true, true);
            if (catalogEditHandle == null || catalogEditHandle.CatalogInstanceName != null)
                return;
            catalogEditHandle.InsertDataGroupCatalogInstance("Opening", "Opening");
            catalogEditHandle.UpdateInstanceDataDefaults();
            foreach (DataGroupProperty property in catalogEditHandle.Properties)
            {
                if (property != null)
                {
                    if (strArray.Length > 1 && property.Xpath == "Opening/@PartCode")
                        catalogEditHandle.SetValue(property, strArray[1]);
                    //if (strArray.Length > 2 && property.Xpath == "EmbPart/@PartCode")
                    //    catalogEditHandle.SetValue(property, strArray[2]);
                }
            }
            catalogEditHandle.Rewrite(0);
            ((IDisposable)catalogEditHandle)?.Dispose();
        }
    }

    static class Penetrations
    {
        static PenetrForm form;

        // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
        // выгрузка: clr unload domain simpenDomain
        public static void showForm(string unparsed)
        {
            if (form == null || form.IsDisposed)
                form = new PenetrForm();

            WindowHelper.show(form);
            form.runLocatingTool();
        }

        public static void readData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            form.readTaskData();
        }

        public static void reload(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            form.reload();
        }

        public static void enableAddToModel(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            form?.enableAddToModel();
        }

        public static void sendTaskData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            form?.sendTaskData();
        }
    }
}
}
