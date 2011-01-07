using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using log4net.Appender;
using log4net.Layout;

namespace Flubu
{
    public class ConsoleExecutionEnvironment : TaskContextBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleExecutionEnvironment"/> class.
        /// </summary>
        /// <param name="scriptName">Name of the script.</param>
        /// <param name="logFileName">Name of the log file (including the path).</param>
        /// <param name="howManyOldLogsToKeep">The how many old log files the script should keep.</param>
        public ConsoleExecutionEnvironment (
            string scriptName, 
            string logFileName,
            int howManyOldLogsToKeep)
        {
            ScriptName = scriptName;

            RollingFileAppender appender = new RollingFileAppender ();
            PatternLayout layout = new PatternLayout ("%date %message%newline");
            layout.ActivateOptions ();
            appender.Layout = layout;
            appender.File = Path.Combine (Environment.CurrentDirectory, logFileName);
            appender.AppendToFile = false;
            appender.MaxSizeRollBackups = howManyOldLogsToKeep;
            appender.RollingStyle = RollingFileAppender.RollingMode.Once;
            appender.Name = "ScriptingAppender";
            appender.ActivateOptions ();

            log4net.Config.BasicConfigurator.Configure (appender);

            AddLogger(new Log4NetLogger(scriptName));
            AddLogger(new MulticoloredConsoleLogger(Console.Out));
        }

        public override string GetConfigSetting (string settingName)
        {
            if (settingName == null)
                throw new ArgumentNullException ("settingName");                
            
            if (false == configurationSettings.ContainsKey (settingName))
            {
                if (InteractiveMode)
                {
                    Console.Out.Write("Enter value for setting '{0}': ", settingName);
                    Console.Out.Flush();
                    string val = Console.In.ReadLine();
                    configurationSettings.Add(settingName, val);
                }
                else
                {
                    throw new ArgumentException(
                        String.Format(
                            CultureInfo.InvariantCulture,
                            "Configuration setting '{0}' has no value defined.",
                            settingName));
                }
            }

            return configurationSettings[settingName];
        }

        public override bool IsConfigSettingDefined(string settingName)
        {
            if (settingName == null)
                throw new ArgumentNullException("settingName");

            return configurationSettings.ContainsKey(settingName);
        }

        public override IEnumerable<KeyValuePair<string, string>> ListConfigSettings()
        {
            return configurationSettings;
        }

        public override void SetConfigSetting (string settingName, string settingValue)
        {
            if (settingName == null)
                throw new ArgumentNullException ("settingName");                
            
            configurationSettings[settingName] = settingValue;
        }

        public override string ReceiveInput (string prompt)
        {
            Console.Out.Write (prompt);
            return Console.In.ReadLine ();
        }

        private readonly Dictionary<string, string> configurationSettings = new Dictionary<string, string> ();
    }
}
