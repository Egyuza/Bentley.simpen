using Shared;
using Embedded.Penetrations.Shared;
using Embedded.Penetrations.Shared.Mapping;
using System;
using System.Windows.Forms;
using Shared.Bentley;
using System.Text;

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

    public static void ShowConfigVariablesList(string unparsed)
    {
        var builder = new StringBuilder("Список конфигурационных переменных:\n\n");

        var list = PenConfigVariables.GetVariables();
        list.Sort((x,y) => x.Name.CompareTo(y.Name));
        list.ForEach(x => builder.AppendLine($"{x.Name} = {x.TryGetValue()}\n"));

        BentleyExtensions.AddInfoToMessageCenter(builder.ToString(), true);
    }
}
}
