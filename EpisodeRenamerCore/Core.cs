using System;
using System.Collections.Generic;
using EpisodeRenamer.EpisodeParser;
using EpisodeRenamer.TVAPI;
using System.Linq;
using Newtonsoft.Json;
using System.IO;

namespace EpisodeRenamer.Core
{
    public class Core
    {
        
        private  IEpisodeParser parser;
        private  ITVAPI tvApi;
        public Configuration.Configuration config { get; set; }
        public  static System.IO.DirectoryInfo FilesDirectory { get; set; }

        static public bool TrySonarrParser(string host, int port , string key)
        {
            IEpisodeParser tryParser = new SonarrParser();
            Configuration.Configuration tryConfig = new Configuration.Configuration();

            tryConfig.server = host;
            tryConfig.port = port;
            tryConfig.SonarrKey = key;

            return tryParser.init(tryConfig).Length == 0;

        }

        static public bool ClearCacheTVAPI(TVAPI.type api)
        {
            try
            {
                switch (api)
                {
                    case type.EpiGuides:
                        EpiGuidesAPI.ClearCache();
                        break;
                    case type.TVDB:
                        TVDBAPI.ClearCache();
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string[] Initialize()
        {
            List<string> errors = new List<string>();

            parser = null;
            tvApi = null;


            config =  new Configuration.Configuration();

            

            switch (config.TVAPI)
            {
                case type.EpiGuides:
                    tvApi = new TVAPI.EpiGuidesAPI();
                    break;
                case type.TVDB:
                    tvApi = new TVAPI.TVDBAPI();
                    break;
            }

            switch (config.EpisodeParser)
            {
                case EpisodeParser.Type.Sonarr:
                    parser = new SonarrParser();
                    break;
            }

            var err = tvApi.init(config);

            if (err.Length != 0)
            {
                errors.AddRange(err);
                errors.Add("TV API could not be initialized. Exiting");
            }

            err = parser.init(config);

            if (err.Length != 0)
            {
                errors.AddRange(err);
                errors.Add("Episode parser could not be initialized. Exiting");
            }



            return errors.ToArray();
        }

        public string [] SetFilesPath(string path)
        {
            List<string> errors = new List<string>();

            try
            {
                FilesDirectory = new System.IO.DirectoryInfo(path);


                if (!FilesDirectory.Exists)
                    errors.Add("The Directory doesn't exist");
            }
            catch (Exception e)
            {
                errors.Add(e.Message);
            }
            

            return errors.ToArray();
        }

        public List<Episode> GetEpisodes()
        {
            List<Episode> episodes = new List<Episode>();

            episodes.AddRange(LoadEpisodeFromDirectory(FilesDirectory));

            return episodes;
        }

        private List<Episode> LoadEpisodeFromDirectory(System.IO.DirectoryInfo dir)
        {
            List<Episode> episodes = new List<Episode>();

            foreach (var ext in config.FileExtensions)
            {
                foreach (var file in dir.GetFiles("*." + ext,SearchOption.AllDirectories).Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden)))
                {
                    Episode ep = parser.parseFile(file.FullName);

                    if (tvApi.GetEpisode(ref ep))
                        episodes.Add(ep);

                }
            }

            return episodes;
        }

        public void RenameSingleEpisode( Episode ep)
        {

            FileUtils.RenameFile(ep.previousPath + ep.previousFileName + ep.extension, ep.newPath + ep.newFileName + ep.extension);

        }

        public void RenameMultipleEpisodes(List<Episode> episodes)
        {
            foreach (var ep in episodes)
                RenameSingleEpisode(ep);
                    
        }

        public string[] LoadConfigurationFile(string pathToFile)
        {
            List<string> errors = new List<string>();

            FileInfo newConfig = new FileInfo(pathToFile);
            
           

            if (!newConfig.Exists)
            {
                errors.Add("Selected file is does not exist.");

                return errors.ToArray();
            }

            if(newConfig.Extension != "json")
            {
                errors.Add("Selected file is not valid. Please select a JSON file.");

                return errors.ToArray();
            }

            try
            {
                Configuration.Configuration newConfigObj = JsonConvert.DeserializeObject<Configuration.Configuration>(System.IO.File.ReadAllText(pathToFile));


                if(newConfigObj == null)
                {
                    errors.Add("Selected file is not valid.");

                    return errors.ToArray();
                }

                File.Copy(pathToFile, Configuration.Constants.ConfigFilePath, true);

                this.config = new Configuration.Configuration("load");

            }
            catch(Exception ex)
            {
                errors.Add("Selected file is not valid.");

                return errors.ToArray();
            }


            



            return errors.ToArray();
        }

        public string[] DownloadConfigurationFile(string pathToDir)
        {

            List<string> errors = new List<string>();

            try
            {

                DirectoryInfo copyDir = new DirectoryInfo(pathToDir);

                if (!copyDir.Exists)
                {
                    errors.Add("The selected Directory does not exist");
                }
                else
                {
                    try
                    {
                        File.Copy(Configuration.Constants.ConfigFilePath, pathToDir + "/Config.json", true);
                    }
                    catch
                    {
                        errors.Add("There was an error copying the file");
                    }
                }
            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
            }

            return errors.ToArray();


            

        }

    }


    



}
