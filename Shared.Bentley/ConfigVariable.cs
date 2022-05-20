using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif


namespace Shared.Bentley
{
    public class ConfigVariable
    {
        public string Name { get; private set; }

        private string defaultValue_;

        public ConfigVariable(string name, string defaultValue = null)
        {
            Name = name;
            defaultValue_ = defaultValue;
        }

        public bool IsDefined => App.ActiveWorkspace.IsConfigurationVariableDefined(Name);

        public string Value 
        { 
            get
            {
                if (IsDefined)
                    return App.ActiveWorkspace.ConfigurationVariableValue(Name, true);

                if (defaultValue_ != null)
                    return defaultValue_;

                throw new Exception(
                    $"Не определена конфигурационная переменная '{Name}'");
            }
        }

        public string TryGetValue()
        {
            if (IsDefined)
                return App.ActiveWorkspace.ConfigurationVariableValue(Name, true);

            if (defaultValue_ != null)
                return defaultValue_;

            return string.Empty;
        }

        public string ValueNonExpand()
        {
            if (IsDefined)
                return App.ActiveWorkspace.ConfigurationVariableValue(Name, false);

            if (defaultValue_ != null)
                return defaultValue_;

            throw new Exception(
                $"Не определена конфигурационная переменная '{Name}'");
        }

        public override string ToString()
        {
            return Value;
        }

        private static BCOM.Application App
        {
            get { return BMI.Utilities.ComApp; }
        }
    }
}
