using Shared;
using Embedded.Openings.Shared;
using System;
using System.Windows.Forms;
using Embedded.Openings.Shared.ViewModels;

namespace Embedded.Openings
{
static class Keyins
{
    // загрузка: mdl load simpen.ui,,simpenDomain; simpen.ui form
    // выгрузка: clr unload domain simpenDomain
    public static void showForm(string unparsed)
    {
        OpeningsVM.getInstance(Addin.Instance, unparsed).showForm();
    }

}
}
