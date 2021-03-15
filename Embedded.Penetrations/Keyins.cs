using Shared;
using Embedded.Penetrations.Shared;
using Embedded.Penetrations.Shared.Mapping;

namespace Embedded.Penetrations
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

    public static void drawLeader(string unparsed)
    {
        PenetrLocateCmd.StartCommand();
    }

    public static void convertDataGroupPropertiesToTags(string unparsed)
    {
        TagsToDataGroupConverter.Run();
    }
}
}
