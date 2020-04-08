using Shared;

namespace simpen.ui
{
namespace Keyins
{
    static class Openings
    {
        static OpeningForm form;

        public static bool DEBUG_MODE { get; private set; }

        // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
        // выгрузка: clr unload domain simpenDomain
        public static void showForm(string unparsed)
        {
            DEBUG_MODE =
                unparsed == null ? false : unparsed.ToUpper().Equals("DEBUG");

            if(form == null || form.IsDisposed)
                form = new OpeningForm();

            WindowHelper.show(form, Addin.Instance.Name + "::" + "Opening");
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

    static class Penetrations
    {
        static PenetrForm form;

        public static bool DEBUG_MODE { get; private set; }

        // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
        // выгрузка: clr unload domain simpenDomain
        public static void showForm(string unparsed)
        {
            DEBUG_MODE =
                unparsed == null ? false : unparsed.ToUpper().Equals("DEBUG");

            if (ViewHelper.getActiveView() == null) 
                return;

            if (form == null || form.IsDisposed)
                form = new PenetrForm();

            WindowHelper.show(form, Addin.Instance.Name + "::" + "Penetr");
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
