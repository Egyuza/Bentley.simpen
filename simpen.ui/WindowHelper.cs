using Bentley.Windowing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace simpen.ui
{
internal static class WindowHelper
{
    static Dictionary<Form, WindowContent> cache = new Dictionary<Form, WindowContent>();

    internal static void show(Form form)
    {
        WindowManager winMngr = // вычислит только если Addin загружено в DefaultDomain
            WindowManager.GetForMicroStation();
        
        if (winMngr == null)
        {
            form.Show();
            return;
        }

        WindowContent m_window = null;

        if (cache.ContainsKey(form))
        {
            m_window = cache[form];        
        }
        else
        {
            m_window = winMngr.DockPanel(form, "simpen.ui",
                form.Text, DockLocation.Floating); // здесь вызов Frm_Load  
            cache.Add(form, m_window);

            form.FormClosed += Form_FormClosed;  
        }
        FormWindowState state = form.WindowState;
        form.WindowState = state != FormWindowState.Normal ? FormWindowState.Normal : state;

        m_window.Show();
        m_window.FloatingHostForm?.Refresh();
        form.Refresh();
    }

    internal static void close(Form form)
    {
        if (cache.ContainsKey(form))
            cache.Remove(form);
    }


    private static void Form_FormClosed(object sender, FormClosedEventArgs e)
    {
        close((Form)sender);
        Addin.App.CommandState.StartDefaultCommand();
    }
}
}
