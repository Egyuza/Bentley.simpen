using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;

namespace Embedded.Penetrations.UI
{
    public partial class PenFormTelerik : Form
    {
        public PenFormTelerik()
        {
#if V8i
            ThemeResolutionService.ApplicationThemeName = "Office2010Silver";
#else
            ThemeResolutionService.ApplicationThemeName = "Office2010Blue";
#endif
            InitializeComponent();
        }
    }
}
