using CommandLine;
using CommandLine.Text;

namespace ModCreator
{
    internal class Program
    {
        public class Options
        {
            [Option("name", Required =true, HelpText ="Name of the mod")]
            public string Name { get; set; }

            [Option(shortName: 'f', longName:"force", Default =false, Required =false, HelpText ="Delete previous mod if it exists")]
            public bool Force { get; set; }

            [Usage()]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    return new List<Example>()
                    {
                        new Example("Create a mod", new Options{Name="modName"})
                    };
                }
            }
        }

        private static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                if (!File.Exists(Utilities.GAME_CONFIG_FILE))
                {
                    Console.WriteLine($"You are missing {Utilities.GAME_CONFIG_FILE}. Please complete the following.");
                    Utilities.CreateGameConfig();
                }
                GameConfig gameConfig = Utilities.GetGameConfig();
                if (!File.Exists(Utilities.CONFIG_FILE))
                {
                    Console.WriteLine($"You are missing {Utilities.CONFIG_FILE}. Please complete the following.");
                    Utilities.CreateConfig(gameConfig);
                }
                Config config = Utilities.GetConfig(gameConfig);
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o => RunOptions(o, config, gameConfig));
            }
            else
            {
                GameConfig gameConfig = Utilities.GetGameConfig();
                Config config = Utilities.GetConfig(gameConfig);
                ModInfo modInfo = new ModInfo();
                if (!Utilities.GetModInfo(ref modInfo))
                {
                    return;
                }
                Utilities.MoveTemplate(modInfo, config, gameConfig);
            }
        }

        static void RunOptions(Options options, Config config, GameConfig gameConfig)
        {
            string modName = options.Name;
            if (modName == "template")
            {
                Console.WriteLine("You cannot name a mod that!");
                return;
            }
            bool force = options.Force;
            if(Directory.Exists(modName) && !force)
            {
                Console.WriteLine("Mod already exists");
                return;
            }
            if (Directory.Exists(modName) && force)
            {
                Directory.Delete(modName, true);
            }
            List<string> dependancies = new List<string>();
            ModInfo modInfo = new ModInfo();
            modInfo.name = modName;
            modInfo.solutionGuid = Guid.NewGuid().ToString().ToUpper();
            modInfo.projectGuid = Guid.NewGuid().ToString().ToUpper();
            modInfo.dependancies = dependancies;
            Utilities.MoveTemplate(modInfo, config, gameConfig);
        }
    }
}