using System;
using System.Collections.Generic;
using System.Text;

namespace Embedded.Penetrations.Shared
{
    class AssemblyVersion
    {
        private static Version version_;
        public static Version Version => 
            version_ = version_ ?? new Version(2, 9, 1);
        public static string VersionStr =>
            $"{Version.Major}.{Version.Minor}.{Version.Build}";
    }
}
