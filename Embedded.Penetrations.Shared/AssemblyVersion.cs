using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Embedded.Penetrations.Shared
{
    class AssemblyVersion
    {
        private static Version version_;
        public static Version Version
        {
            get
            {
                if (version_ != null)
                    return version_;
                
                version_ = new Version(2, 27, 0, 0);

                Version dllVersion = 
                    Assembly.GetExecutingAssembly().GetName().Version;
                
                Debug.Assert(version_ == dllVersion, 
                    $"Версия dll='{dllVersion}' не совпадает " + 
                    $"с назначенной версией приложения='{version_}'");
                
                return version_;
            }
        }

        public static string VersionStr =>
            $"{Version.Major}.{Version.Minor}.{Version.Build}";
    }
}
