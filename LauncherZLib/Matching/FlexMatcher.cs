using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LauncherZLib.Matching
{
    public class FlexMatcher
    {

        private readonly FlexLexicon _lexicon = new FlexLexicon();

        public FlexMatcher()
        {
            
        }

        public FlexLexicon Lexicon { get { return _lexicon; } }

        public FlexResult Match(string str, string[] keywords)
        {
            bool exactOnly = keywords.Length > 1;

        }
        

    }
}
