using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LauncherZLib.API;
using Newtonsoft.Json;

namespace CorePlugins.AppLauncher
{
    public class AppManifestManager
    {
        private ILogger _logger;
        private AppManifest _manifest;
        private List<string> _searchPath;

        public AppManifestManager(ILogger logger)
        {
            _logger = logger;
        }

        public void AddSearchPath(string path)
        {
            
        }

        public void ScheduleUpdateManifest()
        {
            
        }

        public void SaveManifestToFile(string path)
        {
            try
            {
                var serializer = new JsonSerializer();
                using (var sw = new StreamWriter(path))
                {
                    serializer.Serialize(sw, _manifest);
                    sw.Flush();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Failed to save application manifest to {0}", path));
            }
        }

        public void LoadManifestFromFile(string path)
        {
            try
            {

            }
            catch (Exception ex)
            {
                
            }
        }

        private void DoUpdateManifest()
        {
            
        }

    }
}
