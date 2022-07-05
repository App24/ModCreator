using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModCreator
{
    internal static class Utilities
    {
        public const string GAME_CONFIG_FILE = "gameConfig.json";
        public const string CONFIG_FILE = "config.json";
        private static Dictionary<string, Func<ModInfo, Config, GameConfig, string>> fileReplaceables = new Dictionary<string, Func<ModInfo, Config, GameConfig, string>>()
        {
            {"{gameName}", (_, _, gameConfig) => gameConfig.gameName },
            {"{modName}", (modInfo, _, _) => modInfo.name },
            {"{description}", (modInfo, _, _) => modInfo.description },
            {"{modNameDll}", (modInfo, _, _) => modInfo.name+".dll" },
            {"{solutionGuid}", (modInfo,_, _) => modInfo.solutionGuid },
            {"{projectGuid}", (modInfo,_, _) => modInfo.projectGuid },
            {"{uniqueGuid}", (_,_, _) => Guid.NewGuid().ToString().ToUpper() },
            {"{author}", (_,config, _) => config.author },
            {"{installDir}", (_,config, _) => config.installDir },
            {"{modDir}", (modInfo,_, _) => Path.Combine("Mods", modInfo.name) },
            {"{fullModDir}", (modInfo,config, _) => Path.Combine(config.installDir, "Mods", modInfo.name) },
            {"{fullModFile}", (modInfo,config, _) => Path.Combine(config.installDir, "Mods", modInfo.name, modInfo.name)+".dll" },
            { "{requirements}", (modInfo, _, _)=>{
                string text="";

                if(modInfo.dependancies.Count > 0)
                {
                    text=$",{Environment.NewLine}\"Requirements\":[";
                    text+=string.Join(Environment.NewLine, modInfo.dependancies.Select(d=>$"\"{d}\""));
                    text+="]";
                }

                return text;
            }},
            {"{csFiles}", (modInfo,_,_)=>
            {
                string text="";

                List<string> allFiles=GetAllFiles(Path.Combine(modInfo.name, modInfo.name));

                foreach(string file in allFiles)
                {
                    if (file.EndsWith(".cs"))
                    {
                        string fileName;
                        {
                            List<string> parts=file.Split(Path.DirectorySeparatorChar).ToList();
                            parts.RemoveRange(0, 2);
                            fileName=string.Join(Path.DirectorySeparatorChar, parts);
                        }
                        text+=$@"<Compile Include=""{fileName}"" />
";
                    }
                }

                return text;
            } },
            {"{dependancies}", (modInfo, config, gameInfo) =>
            {
                string text="";

                gameInfo.defaultDependancies.ForEach(dependancy =>
                {
                    string dllName=dependancy.Replace("\\", "/").Split("/").Last();
                    string path=dependancy.Replace('/', '\\').Replace('\\', Path.DirectorySeparatorChar);
                    text+=$@"<Reference Include=""{dllName}"">
  <SpecificVersion>False</SpecificVersion>
  <HintPath>{Path.Combine(config.installDir, $"{gameInfo.gameName}_Data", "Managed", path)}.dll</HintPath>
</Reference>
";
                });

                modInfo.dependancies.ForEach(dependancy =>
                {
                    text+=$@"<Reference Include=""{dependancy}"">
  <SpecificVersion>False</SpecificVersion>
  <HintPath>{Path.Combine(config.installDir, "Mods", dependancy, dependancy)}.dll</HintPath>
</Reference>
";
                });

                return text;
            } }
        };

        public static Config GetConfig(GameConfig gameConfig)
        {
            if (!File.Exists(CONFIG_FILE))
            {
                CreateConfig(gameConfig);
            }

            try
            {
                Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(CONFIG_FILE));
                return config;
            }
            catch
            {
                File.Delete(CONFIG_FILE);
                return GetConfig(gameConfig);
            }
        }

        public static void CreateConfig(GameConfig gameConfig)
        {
            Console.WriteLine("First time running the mod creator, please fill in the following:");
            Console.WriteLine("To keep default values, press Enter");
            string installDir;
            do
            {
                installDir = GetInput($"Please enter {gameConfig.gameName} installation directory ({gameConfig.defaultInstallDir}): ", true);
            } while (installDir != "" && !CheckGameModdable(installDir, gameConfig.gameName));
            if (installDir == "")
                installDir = gameConfig.defaultInstallDir;
            string author = GetInput("Please enter your name: ");
            Config config = new Config();
            config.installDir = installDir;
            config.author = author;
            File.WriteAllText(CONFIG_FILE, JsonConvert.SerializeObject(config, Formatting.Indented));
        }

        public static GameConfig GetGameConfig()
        {
            if (!File.Exists(GAME_CONFIG_FILE))
            {
                CreateGameConfig();
            }

            try
            {
                GameConfig gameConfig = JsonConvert.DeserializeObject<GameConfig>(File.ReadAllText(GAME_CONFIG_FILE));
                return gameConfig;
            }
            catch
            {
                File.Delete(GAME_CONFIG_FILE);
                return GetGameConfig();
            }
        }

        public static void CreateGameConfig()
        {
            Console.WriteLine("There is no game information, please complete the following:");
            string gameName = GetInput("Please enter name of the game: ");
            string defaultInstallDir;
            do
            {
                defaultInstallDir = GetInput("Please enter default installation directory: ");
            } while (!CheckGameInstallation(defaultInstallDir, gameName));
            List<string> dependancies = new List<string>();
            while (true)
            {
                string dependancy = GetInput("Please enter a default dependancy, relative path to Managed folder, do not include .dll: (Press enter to finish) ", true);
                if (dependancy == "")
                    break;
                dependancies.Add(dependancy);
            }
            GameConfig gameConfig = new GameConfig();
            gameConfig.gameName = gameName;
            gameConfig.defaultInstallDir = defaultInstallDir;
            gameConfig.defaultDependancies = dependancies;
            File.WriteAllText(GAME_CONFIG_FILE, JsonConvert.SerializeObject(gameConfig, Formatting.Indented));
        }

        public static string GetInput(string prompt, bool allowEmpty = false)
        {
            string text;
            do
            {
                Console.Write(prompt);
                text = Console.ReadLine();
            } while (text == "" && !allowEmpty);
            return text;
        }

        public static bool GetConfirmation(string prompt)
        {
            string input = GetInput($"{prompt} (y/n) ").ToLower();
            return input == "y";
        }

        private static bool CheckGameInstallation(string installDir, string gameName)
        {
            if (!Directory.Exists(Path.Combine(installDir, $"{gameName}_Data", "Managed")))
            {
                Console.WriteLine("Invalid Game Folder! Please enter valid game folder.");
                return false;
            }
            return true;
        }

        private static bool CheckGameModdable(string installDir, string gameName)
        {
            if (!CheckGameInstallation(installDir, gameName)) return false;
            if (!Directory.Exists(Path.Combine(installDir, $"{gameName}_Data", "Managed", "UnityModManager")))
            {
                Console.WriteLine($"{gameName} is not moddable! Please enter moddable game.");
                return false;
            }
            return true;
        }

        public static bool GetModInfo(ref ModInfo modInfo)
        {
            string name;
            do
            {
                name = GetInput("Please enter name of mod: ");
            } while (name == "template");
            if (Directory.Exists(name))
            {
                if (!GetConfirmation($"Mod \"{name}\" already exists. Delete it?"))
                {
                    return false;
                }
                Directory.Delete(name, true);
            }
            string description = GetInput("Enter mod description: ", true);
            List<string> dependancies = new List<string>();
            while (true)
            {
                string dependancy = GetInput("Please enter a mod dependancy, do not include .dll: (Press enter to finish) ", true);
                if (dependancy == "")
                    break;
                dependancies.Add(dependancy);
            }
            modInfo.name = name;
            modInfo.description = description;
            modInfo.solutionGuid = Guid.NewGuid().ToString().ToUpper();
            modInfo.projectGuid = Guid.NewGuid().ToString().ToUpper();
            modInfo.dependancies = dependancies;
            return true;
        }

        public static void MoveTemplate(ModInfo modInfo, Config config, GameConfig gameConfig)
        {
            CopyDirectory("template", modInfo.name, true);
            foreach (string directory in GetAllDirectories(modInfo.name))
            {
                Directory.Move(directory, directory.Replace("{modName}", modInfo.name));
            }

            foreach (string file in GetAllFiles(modInfo.name))
            {
                string newPath = file.Replace("{modName}", modInfo.name);
                File.Move(file, newPath);
                string filePath;
                {
                    List<string> parts = newPath.Split(Path.DirectorySeparatorChar).ToList();
                    parts.RemoveAt(0);
                    filePath = string.Join(Path.DirectorySeparatorChar, parts);
                }
                Console.WriteLine($"Copied file: {filePath}");
            }

            foreach (string file in GetAllFiles(modInfo.name))
            {
                if (file.EndsWith(".exe")) continue;
                string filePath;
                {
                    List<string> parts = file.Split(Path.DirectorySeparatorChar).ToList();
                    parts.RemoveAt(0);
                    filePath = string.Join(Path.DirectorySeparatorChar, parts);
                }
                ReplaceData(file, modInfo, config, gameConfig);
                Console.WriteLine($"Modified file: {filePath}");
            }
        }

        private static void ReplaceData(string path, ModInfo modInfo, Config config, GameConfig gameConfig)
        {
            string content = File.ReadAllText(path);

            foreach (var data in fileReplaceables)
            {
                content = content.Replace(data.Key, data.Value(modInfo, config, gameConfig));
            }

            File.WriteAllText(path, content);
        }

        private static List<string> GetAllDirectories(string path)
        {
            List<string> dirs = new List<string>();

            string[] subDirectories = Directory.GetDirectories(path);

            dirs.AddRange(subDirectories);

            foreach (string subDirectory in subDirectories)
                dirs.AddRange(GetAllDirectories(subDirectory));

            return dirs;
        }

        private static List<string> GetAllFiles(string path)
        {
            List<string> files = new List<string>();

            files.AddRange(Directory.GetFiles(path));

            string[] subDirectories = Directory.GetDirectories(path);

            foreach (string subDirectory in subDirectories)
                files.AddRange(GetAllFiles(subDirectory));

            return files;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
