using Shared;
using Embedded.Penetrations.Shared;
using Embedded.Penetrations.Shared.Mapping;
using System;
using System.Windows.Forms;
using Shared.Bentley;

namespace Embedded.Penetrations
{
static class Keyins
{
    public static void showForm(string unparsed)
    {
        PenetrationVM.getInstance(
            Addin.Instance, new KeyinOptions(unparsed)).showForm();
    }

    public static void drawLeader(string unparsed)
    {
        PenetrLocateCmd.StartCommand();
    }

    public static void convertTagsToDataGroup(string unparsed)
    {
        try
        {
            TagsToDataGroupConverter.Run();
        }
        catch (Exception ex)
        {
            ex.Alert();
        }
    }

    public static void exportSp3dDataToCsv(string unparsed)
    {
        try
        {
            Sp3dDataExport.ExportToCsv();
        }
        catch (Exception ex)
        {
            ex.Alert();
        }
    }

    public static void Test(string unparsed)
    {
        WindowHelper.show(new UI.PenFormTelerik());
    }

}
}
