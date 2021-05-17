using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Shared
{

public enum BindinUpdateMode
{
    ControlOnly,
    SourceOnly,
    Both
}

static class WinFormExtensions
{
    public static void setBinding(this Form form, 
        string controlName, string controlPropertyName, 
        object dataSource, string dataSourceMember, 
        BindinUpdateMode bindingMode = BindinUpdateMode.Both)
    {
        try
        {
            setBinding(findControl(controlName, form), controlPropertyName,
                dataSource, dataSourceMember, bindingMode);
        }
        catch (Exception ex)
        {
            ex.ShowMessageBox();
        }
    }

    public static void setBinding(this Control control, string controlPropertyName,
        object dataSource, string dataSourceMember, 
        BindinUpdateMode bindingMode = BindinUpdateMode.Both)
    {       
        try
        {
            var binding = new Binding(
                controlPropertyName, dataSource, dataSourceMember);
             
            switch (bindingMode)
            {
            case BindinUpdateMode.ControlOnly:
                binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;
                binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;
                break;
            case BindinUpdateMode.SourceOnly:
                binding.ControlUpdateMode = ControlUpdateMode.Never;
                binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                break;
            default:
                binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
                binding.ControlUpdateMode = ControlUpdateMode.OnPropertyChanged;             
                break;
            }

            control.DataBindings.Add(binding);
        }
        catch (Exception ex)
        {
            ex.ShowMessageBox();
        }
    }

    public static Control findControl(string name, Control owner)
    {
        Control res = null;
        res = owner.Controls[name];
        
        if (res == null)
        {
            foreach (Control control in owner.Controls)
            {
                res = findControl(name, control);
                if (res != null) return res;
            }
        }
        return res;
    }
}
}
