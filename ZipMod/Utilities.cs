using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZipMod
{
    public static class Utilities
    {
        const string CONFIG = "zipFiles.json";

        public static void CreateConfig(string fileName, params string[] files)
        {
            ZipFiles zipFiles = new ZipFiles();
            zipFiles.files = files;
            zipFiles.fileName = fileName;

            File.WriteAllText(CONFIG, JsonConvert.SerializeObject(zipFiles, Formatting.Indented));
        }

        internal static bool ConfigExists()
        {
            return File.Exists(CONFIG);
        }

        internal static ZipFiles GetZipFiles()
        {
            if (!ConfigExists())
            {
                return new ZipFiles();
            }

            ZipFiles zipFiles = JsonConvert.DeserializeObject<ZipFiles>(File.ReadAllText(CONFIG));
            return zipFiles;
        }
    }

    struct ZipFiles
    {
        public string fileName;
        public string[] files;
    }
}
