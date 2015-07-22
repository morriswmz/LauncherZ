using System.Collections.Generic;

namespace LauncherZLib.Plugin.Modules.Routing
{
    public class RoutingParameterCollection
    {

        public static RoutingParameterCollection Empty = new RoutingParameterCollection();

        private string[] _unnamedParams;
        private Dictionary<string, string> _namedParams; 

        public string this[int index]
        {
            get { return _unnamedParams[index]; }    
        }

        public string this[string key]
        {
            get { return _namedParams[key]; }
        }

        public RoutingParameterCollection():this(new string[0], new Dictionary<string, string>(0))
        {
        }

        public RoutingParameterCollection(string[] unnamedParams, Dictionary<string, string> namedParams)
        {
            _unnamedParams = unnamedParams;
            _namedParams = namedParams;
        }

        public override string ToString()
        {
            if (_namedParams.Count == 0 && _unnamedParams.Length == 0)
            {
                return "{}";
            }
            if (_unnamedParams.Length == 0)
            {
                return string.Format("{{{0} named}}", _namedParams.Count);
            }
            if (_namedParams.Count == 0)
            {
                return string.Format("{{{0} unnamed}}", _unnamedParams.Length);
            }
            return string.Format("{{{0} named, {1} unnamed}}", _namedParams.Count, _unnamedParams.Length);
        }
    }
}
