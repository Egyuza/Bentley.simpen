using System;
using Bentley.MstnPlatformNET.WPF;
using SD = System.Drawing;

namespace Embedded.Penetrations
{
class PenetrWindow : DockableWindow
{
    public static PenetrWindow Instance {get; private set;}

    public PenetrWindow()
    {
        var userControl = new PenetrView();
        this.Content = userControl;
       
        this.Title = "Проходки";
        this.Attach(Addin.Instance, "Embedded.PenetrWindow", new SD.Size(100, 100));
    }

    public static void Open(string unparsed)
    {
        if (null == Instance)
        {
            Instance = new PenetrWindow();
            Instance.Show ();
        }
    }

    public static void CloseWindow(string unparsed)
    {
        if (null != Instance)
        {
            Instance.Close ();
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed (e);
        this.Detach ();
        Instance = null;
    }
}
}
