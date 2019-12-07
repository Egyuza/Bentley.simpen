using System;
using System.Collections.Generic;

using Bentley.Internal.MicroStation.Elements;
using BCOM = Bentley.Interop.MicroStationDGN;
using Bentley.Geometry;
using System.Runtime.InteropServices;

namespace simpen.ui
{
static class ViewHelper
{
    public static BCOM.View getActiveView()
    {
        foreach (BCOM.View view in Addin.App.ActiveDesignFile.Views)
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
        foreach (BCOM.View view in Addin.App.ActiveDesignFile.Views)
        {
            if (view.IsOpen && view.IsSelected)
            {
                return view.Index;
            }
        }
        return -1;
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

    public static void zoomToElement(long elemId, int modelRefP)
    {
        BCOM.ModelReference modelRef =
            Addin.App.MdlGetModelReferenceFromModelRefP(modelRefP);

        BCOM.Element el = modelRef.GetElementByID(elemId);
        BCOM.View view = getActiveView();

        if (view == null || !el.IsGraphical) {
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
        
        BCOM.Point3d extent = 
            Addin.App.Point3dSubtract(el.Range.High, el.Range.Low);
        
        extent = Addin.App.Point3dScale(extent, zoom);
        //view.set_Origin(Addin.App.Point3dSubtract(el.Range.Low,
        //    Addin.App.Point3dScale(extent, 0.5)));

        BCOM.Point3d pOrigin;

        //pOrigin = Addin.App.Point3dSubtract(el.Range.Low,
        //    Addin.App.Point3dScale(extent, 0.5));

        pOrigin.X = (el.Range.Low.X + el.Range.High.X) / 2;
        pOrigin.Y = (el.Range.Low.Y + el.Range.High.Y) / 2;
        pOrigin.Z = (el.Range.Low.Z + el.Range.High.Z) / 2;

        view.set_Origin(pOrigin);
        el.IsHighlighted = true;

        view.set_Extents(extent);
        view.ZoomAboutPoint(ref pOrigin, 1.0);

        view.Redraw();
    }

}
}
