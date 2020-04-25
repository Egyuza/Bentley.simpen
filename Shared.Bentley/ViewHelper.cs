using System;
using System.Collections.Generic;

using Bentley.Interop.MicroStationDGN;
using System.Runtime.InteropServices;

#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#endif

#if CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared.Bentley
{
static class ViewHelper
{
    public static IEnumerable<View> getOpenViews()
    {
        var res = new List<View>();
        foreach (View view in App.ActiveDesignFile.Views)
        {
            if (view.IsOpen)
            {
                res.Add(view);
            }
        }
        return res;
    }

    public static View getActiveView()
    {
        foreach (View view in App.ActiveDesignFile.Views)
        {
            if (view.IsOpen && view.IsSelected)
            {
                return view;
            }
        }
        return null;
    }

    public static int getActiveViewIndex()
    {
        View activeView = getActiveView();
        return activeView != null ? activeView.Index : -1;
    }

    [DllImport("stdmdlbltin.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern int mdlView_fitViewToRange(
        IntPtr minPointP,
        IntPtr maxPointP,
        IntPtr optionsP,
        int viewIndex
    ); 

    [StructLayout(LayoutKind.Explicit)]
    struct FitViewOptions 
    {
         [FieldOffset(1)]
        public int forceActiveZToCenter;
         [FieldOffset(1)]
        public int expandClippingPlanes;
         [FieldOffset(1)]
        public int disableCenterCamera;
         [FieldOffset(1)]
        public int ignoreTransients; 
         [FieldOffset(1)]
        public int dontIncludeParentsOfNestedRefs;  
         [FieldOffset(1)]
        public int ignoreCallouts;
         [FieldOffset(10)]
        public int optionPadding;  
         [FieldOffset(16)]
        public int optionPadding2;
    };

    public static void zoomToElement(Element element)
    {
        View view = getActiveView();

        if (view == null || !element.IsGraphical) {
            return;
        }

        {
            // изучить функцию mdlView_fitViewToRange
            // https://communities.bentley.com/products/programming/microstation_programming/f/archived-microstation-v8-2004-edition-programming-forum/74744/how-to-display-an-element-at-the-center-of-the-view/201337#201337

            //FitViewOptions options = new FitViewOptions();
            //options.expandClippingPlanes = 1;
            //options.forceActiveZToCenter = 0;
            //options.optionPadding = 0;
            //options.optionPadding2 = 0;

            //IntPtr minPointP = Marshal.AllocHGlobal(Marshal.SizeOf(el.Range.Low));
            //IntPtr maxPointP = Marshal.AllocHGlobal(Marshal.SizeOf(el.Range.High));
            //IntPtr optionsP = Marshal.AllocHGlobal(Marshal.SizeOf(options));

            //Marshal.StructureToPtr(el.Range.Low, minPointP, false);
            //Marshal.StructureToPtr(el.Range.High, maxPointP, false);
            //Marshal.StructureToPtr(options, optionsP, false);
                
            //int status = 
            //    mdlView_fitViewToRange(minPointP, maxPointP, optionsP, view.Index);

            //view.Redraw();        
            //return;
        }
        
        const double zoom = 4;
        
        Point3d extent = 
            App.Point3dSubtract(element.Range.High, element.Range.Low);
        
        extent = App.Point3dScale(extent, zoom);
        //view.set_Origin(Addin.App.Point3dSubtract(el.Range.Low,
        //    Addin.App.Point3dScale(extent, 0.5)));

        Point3d pOrigin;

        //pOrigin = Addin.App.Point3dSubtract(el.Range.Low,
        //    Addin.App.Point3dScale(extent, 0.5));

        pOrigin.X = (element.Range.Low.X + element.Range.High.X) / 2;
        pOrigin.Y = (element.Range.Low.Y + element.Range.High.Y) / 2;
        pOrigin.Z = (element.Range.Low.Z + element.Range.High.Z) / 2;

        view.set_Origin(pOrigin);
        element.IsHighlighted = true;

        view.set_Extents(extent);
        view.ZoomAboutPoint(ref pOrigin, 1.0);

        view.Redraw();
    }
    private static Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
