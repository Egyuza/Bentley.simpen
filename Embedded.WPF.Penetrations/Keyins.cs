namespace Embedded.Penetrations
{

static class Keyins
{
    // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
    // выгрузка: clr unload domain simpenDomain
    public static void openForm(string unparsed)
    {    
        PenetrWindow.Open(unparsed);
    }
}
}
