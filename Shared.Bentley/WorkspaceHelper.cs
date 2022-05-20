using System;
using System.Collections.Generic;
using System.Text;

using BCOM = Bentley.Interop.MicroStationDGN;
#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif

namespace Shared.Bentley
{
public static class WorkspaceHelper
{
    public static string GetConfigVariable(string variableName)
    {
        if (!App.ActiveWorkspace.IsConfigurationVariableDefined(variableName))
        {
            throw new Exception(
                $"Не определена конфигурационная переменная '{variableName}'");
        }
         
        return App.ActiveWorkspace.ConfigurationVariableValue(variableName);
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }
}
}
