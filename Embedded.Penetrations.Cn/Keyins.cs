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
        try
        {
            PenetrationVM.getInstance(
                Addin.Instance, new KeyinOptions(unparsed)).showForm();
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
            ex.ShowMessageBox();
        }
    }

    public static void convertTagsToDataGroup(string unparsed)
    {
        try
        {
            TagsToDataGroupConverter.Run();
        }
        catch (Exception ex)
        {
            Logger.Log.Error(ex);
            ex.ShowMessageBox();
        }
    }
}
}
