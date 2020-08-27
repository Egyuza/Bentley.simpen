using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Layout;
using Hierarchy = log4net.Repository.Hierarchy;

using BCOM = Bentley.Interop.MicroStationDGN;
#if V8i
using BMI = Bentley.MicroStation.InteropServices;
#elif CONNECT
using BMI = Bentley.MstnPlatformNET.InteropServices;
#endif


namespace Shared
{
public static class Logger
{
    public static ILog Log => 
        log_ ?? (log_ = getLog_());

    private static bool isActive_;
    public static bool IsActive
    {
        get { return isActive_; }
        set
        {
            if (value == isActive_)
                return;    

            isActive_ = value;

            var logger = Log.Logger as Hierarchy.Logger;
            if (isActive_)
            {
                isActive_ = true;
                fileAppender_ = fileAppender_ ?? getFileAppender_();
                logger.AddAppender(fileAppender_);
            }
            else if (!isActive_)
            {
                isActive_ = false;
                logger.RemoveAppender(fileAppender_);
            }            
        }
    }

    private static ILog log_;
    private static ILog getLog_()
    {
        ILog log;        
        BasicConfigurator.Configure();

        log = LogManager.GetLogger(AppName);
        
        var logger = log.Logger as Hierarchy.Logger;

        logger.Level = Level.All;
        logger.Additivity = false;

        return log;
    }
    
    private static IAppender fileAppender_;
    private static IAppender getFileAppender_()
    {
        string folderPath = overrideLogFolder_ ?? getDefaultLogFolder_();        
        string filePath = Path.Combine(folderPath, AppName + ".log");

        PatternLayout layout = new PatternLayout(
            "[%date{dd.MM.yyyy HH:mm:ss}] [%level]" + " %message%newline%exception");

        layout.ActivateOptions();
        FileAppender fileAppender = new FileAppender
        {
            Threshold = Level.All,
            Layout = layout,
            File = filePath,
            Encoding = Encoding.UTF8,
            AppendToFile = true,
            ImmediateFlush = true,
        };
        fileAppender.ActivateOptions();
        return fileAppender;
    }

    private static string AppName => 
        Assembly.GetExecutingAssembly().GetName().Name;
    
    private static string overrideLogFolder_;

    private static string getDefaultLogFolder_() =>
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            Path.Combine("Bentley", "OIM"));

    public static void setLogFolder(string path)
    {
        overrideLogFolder_ = path;
    }

    private static BCOM.Application App
    {
        get { return BMI.Utilities.ComApp; }
    }

}
}
