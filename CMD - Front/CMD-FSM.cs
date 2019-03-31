using System;
using System.Collections.Generic;
using System.Linq;
using EpisodeRenamer.Configuration;
using EpisodeRenamer.FrontEnd;
using EpisodeRenamer.Core;
using GenericFSM;
using Utils;
using EpisodeRenamer;

using EpisodeRenamer.EpisodeParser;
using System.Text.RegularExpressions;

namespace FSM
{
    enum UserModeStates { Init, Exit,Select, Error, Configuration, Run};

    class UserModeFSM :GenericFSM.FSM<UserModeStates>
    {
        Core API;
        IDisplay display;

        public UserModeFSM(Core API, IDisplay display) : base(UserModeStates.Init) {
            this.API = API;
            this.display = display;
        }



        void InitState()
        {
            Debug.log("Initializing UserModeFSM.");
            Transition(UserModeStates.Select);
        }

        void ErrorState()
        {

            display.ShowErrors(errors.ToArray());
            
            errors.Clear();

            Console.WriteLine("Press any key to go back");
            Console.ReadKey();

            Transition(prevState);
        }



        void SelectState()
        {
            Console.Clear();

            Console.WriteLine("Welcome to EpisodeRenamer, What do you want to do?");
            Console.WriteLine("1\t- Rename some files!");
            Console.WriteLine("2\t- Mess with config files");
            Console.WriteLine("Esc\t- Exit");


            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    Transition(UserModeStates.Run);
                    break;
                case ConsoleKey.D2:
                    Transition(UserModeStates.Configuration);
                    break;
                case ConsoleKey.Escape:
                    Transition(UserModeStates.Exit);
                    break;
                default:
                    errors.Add("Invalid key");
                    Transition(UserModeStates.Error);
                    break;
            }

        }

        void ConfigurationState()
        {
            ConfigurationFSM configFSM = new ConfigurationFSM(API, display);
            configFSM.Transition(ConfigurationStates.Select);

            while(!configFSM.hasToLeave)
                configFSM.StateDo();

            API.Initialize();
            Transition(UserModeStates.Select);


        }

        void RunState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");
            Console.WriteLine("Please enter the path to the files:");

            string input = "";
            char c;
            
            while((c = Console.ReadKey().KeyChar) != '\n')
            {
                if((int)c == 27)
                {
                    Transition(UserModeStates.Select);
                    return;
                }
                input += c;
            }


            errors = API.SetFilesPath(input).ToList();

            if(errors.Count == 0)
            {

                var episodes = API.GetEpisodes();

                display.showFilesToRename(episodes);

                if (display.warnUser("Do you want to continue?"))
                    API.RenameMultipleEpisodes(episodes);

                Console.WriteLine("Would you like to continue renamig files? [y/n]");
                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.Y:
                        Transition(UserModeStates.Run);
                        break;
                    case ConsoleKey.N:
                        Transition(UserModeStates.Select);
                        break;
                    default:
                        errors.Add("Invalid key");
                        Transition(UserModeStates.Error);
                        break;
                }
            }
            else
            {
                Transition(UserModeStates.Error);
            }



        }

        void ExitState()
        {
            Leave();
        }


    }

    enum ConfigurationStates  { Error, Select, LoadFile, DownloadFile, SonarrProperties, TVAPIProperties, FilesProperties, Exit }

    class ConfigurationFSM : GenericFSM.FSM<ConfigurationStates>
    {
        Core API;
        IDisplay display;

        public ConfigurationFSM (Core api, IDisplay display)
            :base(ConfigurationStates.Select)
        {
            this.API = api;
            this.display = display;
        }

        void SelectState()
        {
            Console.Clear();

            Console.WriteLine("Modify config file:");
            Console.WriteLine("1\t- Load new config file");
            Console.WriteLine("2\t- Download current config file");
            Console.WriteLine("3\t- Modify Sonarr properties");
            Console.WriteLine("4\t- Modify TVAPI properties");
            Console.WriteLine("5\t- Modify directory and files properties");
            Console.WriteLine("Esc\t- Go back to the menu");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    Transition(ConfigurationStates.LoadFile);
                    break;
                case ConsoleKey.D2:
                    Transition(ConfigurationStates.DownloadFile);
                    break;
                case ConsoleKey.D3:
                    Transition(ConfigurationStates.SonarrProperties);
                    break;
                case ConsoleKey.D4:
                    Transition(ConfigurationStates.TVAPIProperties);
                    break;
                case ConsoleKey.D5:
                    Transition(ConfigurationStates.FilesProperties);
                    break;
                case ConsoleKey.Escape:
                    Transition(ConfigurationStates.Exit);
                    break;
                default:
                    errors.Add("Invalid key");
                    Transition(ConfigurationStates.Error);
                    break;
                    
            }
        }

        void FilesPropertiesState()
        {
            var fsm = new FilesFSM(API, display);

            fsm.Transition(FilesStates.Select);

            do
                fsm.StateDo();
            while (!fsm.hasToLeave);

            Transition(ConfigurationStates.Select);
        }

        void TVAPIPropertiesState()
        {
            var fsm = new TVAPIFSM(API, display);

            fsm.Transition(TVAPIStates.Select);

            do
                fsm.StateDo();
            while (!fsm.hasToLeave);

            Transition(ConfigurationStates.Select);
        }

        void SonarrPropertiesState()
        {
            SonarrFSM fsm = new SonarrFSM(API, display);

            fsm.Transition(SonarrStates.Select);

            do
                fsm.StateDo();
            while (!fsm.hasToLeave);

            Transition(ConfigurationStates.Select);
        }

        void DownloadFileState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");

            Console.WriteLine("Enter the path where you want to download the file.");

            string input = "";
            char c;

            while((c = Console.ReadKey().KeyChar) != '\n')
            {
                if(c == 27)
                {
                    Transition(ConfigurationStates.Exit);
                    return;
                }

                input += c;
            }


            errors = API.DownloadConfigurationFile(input).ToList();

            if (errors.Count != 0)
                Transition(ConfigurationStates.Error);
            else
                Transition(ConfigurationStates.Select);
        }

        void LoadFileState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");

            Console.WriteLine("Enter the path where the new config file is stored:");

            string input = "";
            char c;

            while ((c = Console.ReadKey().KeyChar) != '\n')
            {
                if (c == 27)
                {
                    Transition(ConfigurationStates.Exit);
                    return;
                }

                input += c;
            }

            errors = API.LoadConfigurationFile(input).ToList();

            if (errors.Count != 0)
                Transition(ConfigurationStates.Error);
            else
                Transition(ConfigurationStates.Select);
        }

        void ExitState()
        {
            Debug.log("Exiting Configuration Manager. Last event: " + prevState.ToString());
            Leave();
        }

        void ErrorTransition(ConfigurationStates state)
        {
            prevState = state;
        }

        void ErrorState()
        {
            display.ShowErrors(errors.ToArray());

            errors.Clear();

            Console.WriteLine("Press any key to go back");
            Console.ReadKey();

            Transition(prevState);
        }

    }

    enum SonarrStates { Error, Exit, Select, Host, Port, Key }

    class SonarrFSM : GenericFSM.FSM<SonarrStates>
    {
        Core API;
        IDisplay display;

        public SonarrFSM(Core api, IDisplay display)
            : base(SonarrStates.Select)
        {
            this.API = api;
            this.display = display;
        }

        void SelectState()
        {
            Console.Clear();
            Console.WriteLine("Set Sonarr properties:");
            Console.WriteLine("1\t- Set host");
            Console.WriteLine("2\t- Set port:");
            Console.WriteLine("3\t- Set Sonarr Key");
            Console.WriteLine("Esc\t- Go back");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    Transition(SonarrStates.Host);
                    break;
                case ConsoleKey.D2:
                    Transition(SonarrStates.Port);
                    break;
                case ConsoleKey.D3:
                    Transition(SonarrStates.Key);
                    break;
                case ConsoleKey.Escape:
                    Transition(SonarrStates.Exit);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(SonarrStates.Error);
                    break;
            }
        }

        void KeyState()
        {
            Console.Clear();
            Console.WriteLine("The current SonarrKey is: " + API.config.SonarrKey);
            Console.WriteLine("Type the new SonarrKey or leave it empty to exit");

            string input = Console.ReadLine();

            if (input.Length != 0)
            {
                if (!Core.TrySonarrParser(API.config.server, API.config.port,input))
                {
                    errors.Add("SonarrKey Invalid");
                    Transition(SonarrStates.Error);
                }
                else
                {
                    Transition(SonarrStates.Select);
                }
            }
            else
            {
                Transition(SonarrStates.Select);
            }
        }

        void PortState()
        {
            var prevPort = API.config.port;
            Console.Clear();
            Console.WriteLine("The current Port is: " + prevPort.ToString());
            Console.WriteLine("Type the new Port or leave it empty to exit");

            string input = Console.ReadLine();

            if (input.Length != 0)
            {
                int newPort;
                if (int.TryParse(input,out newPort) && !Core.TrySonarrParser(API.config.server, newPort, API.config.SonarrKey))
                {
                    errors.Add("Invalid Port");
                    Transition(SonarrStates.Error);
                }
                else
                {
                    Transition(SonarrStates.Select);
                }
            }
            else
            {
                Transition(SonarrStates.Select);
            }
        }

        void HostState()
        {
            string prevHost = API.config.server;
            Console.Clear();
            Console.WriteLine("The current Host is: " + prevHost);
            Console.WriteLine("Type the new Host or leave it empty to exit");

            string input = Console.ReadLine();

            if(input.Length != 0)
            {
                if (!Core.TrySonarrParser( input, API.config.port, API.config.SonarrKey))
                {
                    errors.Add("Invalid Host");
                    Transition(SonarrStates.Error);
                }
                else
                    Transition(SonarrStates.Select);
            }
            else
                Transition(SonarrStates.Select);
            
        }

        void ExitState()
        {
            Debug.log("Exiting Sonarr Manager. Last event: " + prevState.ToString());

            Leave();
        }

        void ErrorState()
        {
            display.ShowErrors(errors.ToArray());

            Console.WriteLine("Press any key to go back");
            Console.ReadKey();
            errors.Clear();

            Transition(prevState);
        }
    }

    enum TVAPIStates { Select,Error, Exit, Epiguides, TVDB, SetAPI}

    class TVAPIFSM : GenericFSM.FSM<TVAPIStates>
    {
        Core API;
        IDisplay display;

        public TVAPIFSM(Core api, IDisplay display)
            : base(TVAPIStates.Select)
        {
            this.API = api;
            this.display = display;
        }

        void SelectState()
        {
            Console.Clear();
            Console.WriteLine("Set TV API properties:");
            Console.WriteLine("1\t- Set TV API");
            Console.WriteLine("2\t- Go to Epiguides");
            Console.WriteLine("3\t- Go to TVDB");
            Console.WriteLine("Esc\t- Go back");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    Transition(TVAPIStates.SetAPI);
                    break;
                case ConsoleKey.D2:
                    Transition(TVAPIStates.Epiguides);
                    break;
                case ConsoleKey.D3:
                    Transition(TVAPIStates.TVDB);
                    break;
                case ConsoleKey.Escape:
                    Transition(TVAPIStates.Exit);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(TVAPIStates.Error);
                    break;
            }
        }

        void TVDBState()
        {
            Console.Clear();
            Console.WriteLine("TVDB API Configuration");
            Console.WriteLine("1\t- Clear cache");
            Console.WriteLine("2\t- Override Series ID");
            Console.WriteLine("Esc\t- Go back");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    if (Core.ClearCacheTVAPI(EpisodeRenamer.TVAPI.type.TVDB))
                        Transition(TVAPIStates.Select);
                    else
                    {
                        errors.Add("There was an error clearing the Cache");
                        Transition(TVAPIStates.Error);
                    }
                    break;
                case ConsoleKey.D2:

                    string seriesName = "";
                    uint SeriesID;

                    Console.Clear();
                    Console.WriteLine("Enter the name of the TV show:");

                    seriesName = Console.ReadLine();

                    if(seriesName == "")
                        errors.Add("The TV show Name is invalid");

                    Console.WriteLine("Enter the SeriesID from TVDB");

                    if(!uint.TryParse(Console.ReadLine(),out SeriesID))
                        errors.Add("The SeriesID must be a positive integer");


                    if (errors.Count == 0)
                    {
                        if (EpisodeRenamer.TVAPI.TVDBAPI.OverrideSeriedID(seriesName, SeriesID))
                            Transition(TVAPIStates.Select);
                        else
                        {
                            errors.Add("There was an error overriding the ID");
                            Transition(TVAPIStates.Error);
                        }
                    }
                    else
                        Transition(TVAPIStates.Error);


                    break;
                case ConsoleKey.Escape:
                    Transition(TVAPIStates.Exit);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(TVAPIStates.Error);
                    break;
            }

        }

        void EpiguidesState()
        {
            Console.Clear();
            Console.WriteLine("Epiguides API Configuration:");
            Console.WriteLine("1\t- Clear Cache");
            Console.WriteLine("Esc\t- Go back");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D1:
                    if (Core.ClearCacheTVAPI(EpisodeRenamer.TVAPI.type.EpiGuides))
                        Transition(TVAPIStates.Select);
                    else
                    {
                        errors.Add("There was an error clearing the Cache");
                        Transition(TVAPIStates.Error);
                    }
                    break;
                case ConsoleKey.Escape:
                    Transition(TVAPIStates.Exit);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(TVAPIStates.Error);
                    break;
            }
        }

        void SetAPIState()
        {
            Console.Clear();
            Console.WriteLine("Indicate which API are you going to use for getting the name of the episodes");
            Console.WriteLine("0 - Epiguides | Its faster but might not be accurate");
            Console.WriteLine("1 - TVBD | Its slower but its as accurate as it gets");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.D0:
                    API.config.TVAPI = EpisodeRenamer.TVAPI.type.EpiGuides;
                    Transition(TVAPIStates.Select);
                    break;
                case ConsoleKey.D1:
                    API.config.TVAPI = EpisodeRenamer.TVAPI.type.TVDB;
                    Transition(TVAPIStates.Select);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(TVAPIStates.Error);
                    break;
            }
        }

        

        void ExitState()
        {
            Debug.log("Exiting TV API Manager. Last event: " + prevState.ToString());

            Leave();
        }

        void ErrorState()
        {
            display.ShowErrors(errors.ToArray());

            Console.WriteLine("Press any key to go back");
            Console.ReadKey();
            errors.Clear();

            Transition(prevState);
        }
    }

    enum FilesStates { Select, Error, Exit, AddFileExt, DelFileExt, NameFormat, DisableDiretoryFormat ,SetDirectoryFormat }

    class FilesFSM : GenericFSM.FSM<FilesStates>
    {
        Core API;
        IDisplay display;

        public FilesFSM(Core api, IDisplay display)
            : base(FilesStates.Select)
        {
            this.API = api;
            this.display = display;
        }

        void SelectState()
        {
            Console.Clear();
            Console.WriteLine("Set Files and Directory properties:");
            Console.WriteLine("1\t- Add file extensions to our whitelist");
            Console.WriteLine("2\t- Remove file extensions to our whitelist");
            Console.WriteLine("3\t- Change the file's renaming style");
            Console.WriteLine("4\t- Disable automatic TV show maping");
            Console.WriteLine("5\t- Change the directory's renaming style and enable TV show maping");
            Console.WriteLine("Esc\t- Go back");
            var a = Console.ReadKey().Key;
            switch (a)
            {
                case ConsoleKey.D1:
                    Transition(FilesStates.AddFileExt);
                    break;
                case ConsoleKey.D2:
                    Transition(FilesStates.DelFileExt);
                    break;
                case ConsoleKey.D3:
                    Transition(FilesStates.NameFormat);
                    break;
                case ConsoleKey.D4:
                    Transition(FilesStates.DisableDiretoryFormat);
                    break;
                case ConsoleKey.D5:
                    Transition(FilesStates.SetDirectoryFormat);
                    break;
                case ConsoleKey.Escape:
                    Transition(FilesStates.Exit);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(FilesStates.Error);
                    break;
            }
        }

        void SetDirectoryFormatState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");
            Console.WriteLine("This will enable TV show maping and you will set the directory naming scheme");
            if (API.config.DirectoryFormat == "")
                Console.WriteLine("Currently TV show mapping is disabled");
            else
                Console.WriteLine("The current directory naming scheme is: " + API.config.DirectoryFormat);
            Console.WriteLine("Use the tags {{Show}},{{Season}},{{Episode}} and {{Title}} to indicate the directory naming scheme");

            string input = "";
            char c;

            while ((c = Console.ReadKey().KeyChar) != '\n')
            {
                if (c == 27)
                {
                    Transition(FilesStates.Select);
                    return;
                }
                input += c;
            }

            if (input.IndexOfAny(System.IO.Path.GetInvalidPathChars()) == -1 || input =="")
            {
                errors.Add("Invalid Directory naming scheme");
                Transition(FilesStates.Error);
            }
            else
            {
                API.config.DirectoryFormat = input;
                Transition(FilesStates.Select);
            }

        }

        void DisableDiretoryFormatState()
        {
            Console.Clear();
            Console.WriteLine("Disabling this will delete the current namign schema for directories.");
            Console.WriteLine("Are you sure you want to do this? [y/n]");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Y:
                    API.config.DirectoryFormat = "";
                    Transition(FilesStates.Select);
                    break;
                case ConsoleKey.N:
                    Transition(FilesStates.Select);
                    break;
                default:
                    errors.Add("Invalid Key");
                    Transition(FilesStates.Error);
                    break;
            }
        }

        void NameFormatState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");
            Console.WriteLine("The current file naming schema is:");
            Console.WriteLine(API.config.NameFormat);
            Console.WriteLine("Use the tags {{Show}},{{Season}},{{Episode}} and {{Title}} to indicate the file naming scheme");

            string input = "";
            char c;

            while((c = Console.ReadKey().KeyChar) != '\n')
            {
                if(c == 27)
                {
                    Transition(FilesStates.Select);
                    return;
                }
                input += c;
            }
            
            if(input.IndexOfAny(System.IO.Path.GetInvalidFileNameChars())!= -1 || input == "")
            {
                errors.Add("Invalid File file naming schema");
                Transition(FilesStates.Error);
            }
            else
            {
                API.config.NameFormat = input;
                Transition(FilesStates.Select);
            }

        }

        void DelFileExtState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");
            Console.WriteLine("We are currently looking for files with this format:");
            Console.WriteLine(API.config.FileExtensions);
            Console.WriteLine("Which file extension would you like to remove?");

            string input = "";
            char c;

            while ((c = Console.ReadKey().KeyChar) != '\n')
            {
                if (c == 27)
                {
                    Transition(FilesStates.Select);
                    return;
                }
                input += c;
            }



            if (API.config.FileExtensions.Contains(input))
                API.config.FileExtensions.Remove(input);

        }

        void AddFileExtState()
        {
            Console.Clear();
            Console.WriteLine("Press 'Esc' to go back");
            Console.WriteLine("We are currently looking for files with this format:");
            Console.WriteLine(API.config.FileExtensions);
            Console.WriteLine("Which file extension would you like to add? ");

            string input = "";
            char c;

            while ((c = Console.ReadKey().KeyChar) != '\n')
            {
                if (c == 27)
                {
                    Transition(FilesStates.Select);
                    return;
                }
                input += c;
            }


            if(new Regex("^[a-zA-Z0-9]*$").IsMatch(input) || input.Length > 4 || input == "")
            {
                errors.Add("Invalid file extension");
                Transition(FilesStates.Error);
            }
            else
            {
                API.config.FileExtensions.Add(input);
                Transition(FilesStates.Select);
            }


        }



        void ExitState()
        {
            Debug.log("Exiting TV API Manager. Last event: " + prevState.ToString());

            Leave();
        }

        void ErrorState()
        {
            display.ShowErrors(errors.ToArray());

            Console.WriteLine("Press any key to go back");
            Console.ReadKey();
            errors.Clear();

            Transition(prevState);
        }
    }
}
