using Bentley.Windowing;
using System.Windows.Forms;
using System.Collections.Generic;

using BCOM = Bentley.Interop.MicroStationDGN;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared
{
internal static class WindowHelper
{
    static Dictionary<Form, WindowContent> cache = new Dictionary<Form, WindowContent>();

    internal static void show(Form form, string id)
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
            m_window = winMngr.DockPanel(form, id,
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
        App.CommandState.StartDefaultCommand();
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
