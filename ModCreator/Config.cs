using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCreator
{
    internal struct Config
    {
        public string installDir;
        public string author;
    }

    internal struct GameConfig
    {
        public string gameName;
        public string defaultInstallDir;
        public List<string> defaultDependancies;
    }

    internal struct ModInfo
    {
        public string name;
        public string description;
        public string solutionGuid;
        public string projectGuid;
        public List<string> dependancies;
    }
}
