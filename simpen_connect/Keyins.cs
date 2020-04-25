using Bentley.Building.DataGroupSystem;
using Bentley.Building.DataGroupSystem.Serialization;
using System;

using Shared;

namespace simpen_cn
{

namespace Keyins
{
    static class Penetrations
    {
        static PenetrForm form;

        // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
        // выгрузка: clr unload domain simpenDomain
        public static void showForm(string unparsed)
        {
            //if (ViewHelper.getActiveView() == null) 
            //    return;
            
            //if (form == null || form.IsDisposed)
            //    form = new PenetrForm();

            //WindowHelper.show(form, Addin.Instance.Name);


            
        }

        public static void readData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            //form.readTaskData();
        }

        public static void reload(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;

            //form.reload();
        }

        public static void enableAddToModel(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            //form?.enableAddToModel();
        }

        public static void sendTaskData(string unparsed)
        {
            if (form == null || form.IsDisposed)
                return;
            //form?.sendTaskData();
        }
    }
}
}
