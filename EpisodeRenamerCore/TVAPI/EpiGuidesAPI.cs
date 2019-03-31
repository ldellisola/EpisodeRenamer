using System;
using System.Collections.Generic;
using System.IO;
using EpisodeRenamer.Configuration;

namespace EpisodeRenamer.TVAPI
{
    class EpiGuidesAPI : StaticTVAPI,ITVAPI
    {
        public EpiGuidesAPI() { }

        public new static bool ClearCache()
        {
            try
            {
                if (System.IO.Directory.Exists(Constants.EpGuidesFolder))
                {
                    System.IO.Directory.Delete(Constants.EpGuidesFolder,true);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }


        public bool GetEpisode(ref Episode ep)
        {
            FileUtils tvFile = new FileUtils();

            if (File.Exists(Constants.EpGuidesFolder + "\\" + ep.seriesName + ".csv"))
            {

                tvFile.load(Constants.EpGuidesFolder + "\\" + ep.seriesName + ".csv");

                CsvParser.getEpisode(tvFile, ref ep);
            }
            else
            {
                var shows = CsvParser.getAllPossibleTVShows(ep.seriesName, index);

                if (shows.Count != 0)
                {

                    tvFile.download(TvShowURL + shows[0].EpidguidesMap, Constants.EpiguidesTvShows + "\\" + ep.seriesName + ".csv");
                    tvFile.load(Constants.EpiguidesTvShows + "\\" + ep.seriesName + ".csv");
                    CsvParser.clearFile(ref tvFile);
                    tvFile.save(Constants.EpiguidesTvShows + "\\" + ep.seriesName + ".csv");

                    CsvParser.getEpisode(tvFile, ref ep);
                }
                else
                {
                    return false;
                }

            }

            NameHelper.GenerateNewName(ref ep, config);
            NameHelper.GenerateNewPath(ref ep, config, Core.Core.FilesDirectory);

            return true;
        }



        public string[] init(object obj)
        {
            List<string> errors = new List<string>();
            try
            {
                config = obj as Configuration.Configuration;

                if (!System.IO.Directory.Exists(Constants.EpGuidesFolder))
                {
                    System.IO.Directory.CreateDirectory(Constants.EpGuidesFolder);
                }

                index = new FileUtils();

                if (!System.IO.File.Exists(Constants.EpGuidesFolder + "\\" + indexFilePath))
                {
                    index.download(indexURL, Constants.EpGuidesFolder + "\\" + indexFilePath);
                }

                index.load(Constants.EpGuidesFolder + "\\" + indexFilePath);


            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
            }

            return errors.ToArray();
        }


        private Configuration.Configuration config;
        private FileUtils index;
        private const string indexFilePath = @"EpiguidesIndex.csv";

        private const string indexURL = "http://epguides.com/common/allshows.txt";
        private const string TvShowURL = "http://epguides.com/common/exportToCSVmaze.asp?maze=";
    }
}
