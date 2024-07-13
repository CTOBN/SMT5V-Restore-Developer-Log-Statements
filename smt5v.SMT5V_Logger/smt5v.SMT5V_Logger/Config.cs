using smt5v.SMT5V_Logger.Template.Configuration;
using System.ComponentModel;

namespace smt5v.SMT5V_Logger.Configuration
{
    public class Config : Configurable<Config>
    {
        [DisplayName("Write to Console")]
        [Description("Write a copy of the log output to the Reloaded II console.")]
        [DefaultValue(true)]
        public bool WriteToConsole { get; set; } = true;

        [DisplayName("Write to File")]
        [Description("Write a copy of the log output to file")]
        [DefaultValue(true)]
        public bool WriteToFile { get; set; } = true;

        [DisplayName("File to Write")]
        [Description("Name of the file the log will be written to if Write to File is on")]
        [DefaultValue("SMT_Log")]
        public string LoggingFilepath { get; set; } = "SMT_Log";

        /// <summary>
        /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
        /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
        /// </summary>
        public class ConfiguratorMixin : ConfiguratorMixinBase
        {
            // 
        }
    }
}
