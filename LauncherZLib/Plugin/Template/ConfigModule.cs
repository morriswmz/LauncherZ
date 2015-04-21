using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.Utils;

namespace LauncherZLib.Plugin.Template
{
    public class ConfigModule<TC> where TC : class
    {

        protected ILogger Logger;
        protected Func<TC> DefaultConfigConstructor;

        public ConfigModule(ILogger logger, TC defaultConfig)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            if (defaultConfig == null)
                throw new ArgumentNullException("defaultConfig");
            Logger = logger;
            DefaultConfigConstructor = () => defaultConfig;
        } 

        public ConfigModule(ILogger logger, Func<TC> defaultConfigConstructor)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            Logger = logger;
            DefaultConfigConstructor = defaultConfigConstructor;
        }

        public TC Config { get; protected set; }



    }
}
