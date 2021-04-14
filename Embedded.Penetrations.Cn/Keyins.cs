using Shared;
using Embedded.Penetrations.Shared;
using Embedded.Penetrations.Shared.Mapping;
using System;
using System.Windows.Forms;

namespace Embedded.Penetrations.Cn
{
static class Keyins
{
    // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
    // выгрузка: clr unload domain simpenDomain
    public static void showForm(string unparsed)
    {
        //WindowHelper.show(new PenetrForm(), "pen_id");
        PenetrationVM.getInstance(Addin.Instance, unparsed).showForm();
    }

    public static void convertTagsToDataGroup(string unparsed)
    {
        try
        {
            TagsToDataGroupConverter.Run();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
}
