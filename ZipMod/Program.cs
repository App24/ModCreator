using System.IO.Compression;

namespace ZipMod
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (!Utilities.ConfigExists())
            {
                Utilities.CreateConfig("mod.zip", "bin/Debug/{modNameDll}", "bin/Release/{modNameDll}", "../README.md", "info.json");
            }

            ZipFiles zipFiles = Utilities.GetZipFiles();

            List<string> files = new List<string>();

            foreach(string file in zipFiles.files)
            {
                if (!File.Exists(file)) continue;
                if (!files.Exists(f => Path.GetFileName(f) == Path.GetFileName(file)))
                {
                    files.Add(file);
                }
                else
                {
                    var creationTime = File.GetCreationTimeUtc(file);
                    string otherFile = files.Find(f => Path.GetFileName(f) == Path.GetFileName(file));
                    var otherCreationTime = File.GetCreationTimeUtc(otherFile);
                    if(creationTime > otherCreationTime)
                    {
                        files.Remove(otherFile);
                        files.Add(file);
                    }
                }
            }

            using(ZipArchive zip = ZipFile.Open(zipFiles.fileName, ZipArchiveMode.Create))
            {
                foreach(string file in files)
                {
                    if (File.Exists(file))
                        zip.CreateEntryFromFile(file, Path.GetFileName(file));
                }
            }
        }
    }
}