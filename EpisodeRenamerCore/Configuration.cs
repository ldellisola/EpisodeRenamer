using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace EpisodeRenamer.Configuration
{




    public class Configuration {


        public string server { get; set; }
        public int port { get; set; }
        public string SonarrKey { get; set; }
        public string TVDBKey { get; set; }
        public List<string> FileExtensions { get; set; }
        public string NameFormat { get; set; }
        public string DirectoryFormat { get; set; }
        public TVAPI.type TVAPI { get; set; }
        public EpisodeParser.Type EpisodeParser { get; set; }

        


        public Configuration(string mode = "load")
        {

            if (mode == "load")
            {
                if (System.IO.File.Exists(Constants.ConfigFilePath))
                {
                    try
                    {
                        LoadConfigurationFile();
                    }
                    catch (Exception ex)
                    {}
                }
                else
                {
                    var config = new Configuration("create");
                    config.GenerateFile(Constants.ConfigFilePath);

                    LoadConfigurationFile();

                }
            }


            if (mode == "create")
            {
                server = "localhost";
                port = 8989;
                SonarrKey = "e5d889b0f2454cdfa9e44e76f0d82f7e";
                FileExtensions = new List<string>();
                FileExtensions.Add("avi");
                FileExtensions.Add("mkv");
                FileExtensions.Add("mp4");
                FileExtensions.Add("ogg");
                FileExtensions.Add("mov");
                FileExtensions.Add("3gp");
                TVDBKey = "CR3J2WD5DEJCGRY5";

                NameFormat = "S{{Season}}E{{Episode}} - {{Title}}";
                DirectoryFormat = "{{Show}}/Season {{Season}}/";
            }
            


            



            if (!System.IO.Directory.Exists(Constants.tempFolder))
            {
                System.IO.Directory.CreateDirectory(Constants.tempFolder);
            }

            if (!System.IO.Directory.Exists(Constants.EpiguidesTvShows))
            {
                System.IO.Directory.CreateDirectory(Constants.EpiguidesTvShows);
            }



            if (!System.IO.Directory.Exists(Constants.TVDBFolder))
            {
                System.IO.Directory.CreateDirectory(Constants.TVDBFolder);
            }


            
        }



        private void LoadConfigurationFile()
        {
            var a = System.IO.Directory.GetCurrentDirectory();
            string text = System.IO.File.ReadAllText(Constants.ConfigFilePath);

            var config = JsonConvert.DeserializeObject<Configuration>(text);

            this.DirectoryFormat = config.DirectoryFormat;
            this.EpisodeParser = config.EpisodeParser;
            this.FileExtensions = config.FileExtensions;
            this.NameFormat = config.NameFormat;
            this.port = config.port;
            this.server = config.server;
            this.SonarrKey = config.SonarrKey;
            this.TVAPI = config.TVAPI;
            this.TVDBKey = config.TVDBKey;
        }



        public void GenerateFile(string path)
        {
            string json = JsonConvert.SerializeObject(this);

            FileUtils configFile = new FileUtils(json);


            configFile.WriteComment("\"SonarrKey\"", "//", "Download Sonarr and use your key to enable filename parsing");
            configFile.WriteComment("\"DirectoryFormat\"", "//", "Leave blank if you want your files to stay in the same directory\n use the tags {{Show}},{{Season}},{{Episode}} and {{Title}} to indicate the directory");
            configFile.WriteComment("\"NameFormat\"", "//", "Use the tags {{Show}},{{Season}},{{Episode}} and {{Title}} to indicate the file naming schema");
            configFile.WriteComment("\"EpisodeParser\"", "//", "Indicate which API you are going to use for parsing the name of the files\n 0 - Sonarr");
            configFile.WriteComment("\"FileExtensions\"", "//", "It indicates the files extensions that the program will target");
            configFile.WriteComment("\"port\"", "//", "Its the port for Sonarr");
            configFile.WriteComment("\"server\"", "//", "Its the server for Sonarr");
            configFile.WriteComment("\"TVAPI\"", "//", "Indicate which API are you going to use for getting the name of the episodes\n 0 - Epiguides | Its faster but might not be accurate\n 1 - TVBD | Its slower but its as accurate as it gets");
            configFile.WriteComment("\"TVDBKey\"", "//", "Its the key to interact with TVDB server");
            configFile.save(path);
        }



    }

    static class Constants {


        public static string PhysicalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/EpisodeRenamer"; //new System.IO.FileInfo( new Uri(System.Reflection.Assembly.GetEntryAssembly().GetName().CodeBase).AbsolutePath.Replace("%20"," ")).Directory.FullName; //@"C:\Users\luckd\source\repos\EpisodeRenamer\EpisodeRenamer\bin\Debug\netcoreapp2.1";
        public static string tempFolder = PhysicalPath + "\\TemporalFiles";

        public static string EpGuidesFolder = tempFolder +"\\Epguides";
        public static string EpiguidesTvShows = EpGuidesFolder + "\\TvShows";

        public static string TVDBFolder = tempFolder +"\\TVDB";
        public static string ConfigFilePath = PhysicalPath + @"/Config.json";
    }
}
