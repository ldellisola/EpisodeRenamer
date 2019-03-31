using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SonarrSharp;
using EpisodeRenamer.TVAPI;
using EpisodeRenamer.EpisodeParser;

namespace EpisodeRenamer
{
    public class SonarrParser : IEpisodeParser
    {
        string[] IEpisodeParser.init( object obj)
        {
            List<string> errors = new List<string>();
            config = obj as Configuration.Configuration;

            if (config == null)
            {
                errors.Add("Argument is not a config type");
            }
            else if (config.port == 0 || config.server == "" || config.SonarrKey == "" || config.SonarrKey.Length != 32)
            {
                errors.Add("Sonarr properties are not correct. Please correct them.");
            }
            else
            {
                try
                {
                    client = new SonarrClient(config.server, config.port, config.SonarrKey);
                    parser = new SonarrSharp.Endpoints.Parse.Parse(client);
                    
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                    errors.Add("Couldn't connect with Sonarr. Please make sure that Sonarr es running and that your Key is correct.");
                }
            }

            return errors.ToArray();

        }

        Episode IEpisodeParser.parseFile(string path)
        {
            var async = parser.ParsePath(path);

            async.Wait();

            var result = async.Result;

            try
            {
                Episode ep = new Episode();

                ep.season = (uint)result.ParsedEpisodeInfo.SeasonNumber;
                ep.number = (uint)result.ParsedEpisodeInfo.EpisodeNumbers[0];
                ep.seriesName = result.ParsedEpisodeInfo.SeriesTitleInfo.Title;

                ep.previousFileName = Path.GetFileNameWithoutExtension(path);
                ep.previousPath = path.Replace(Path.GetFileName(path), "");
                ep.extension = Path.GetExtension(path);

                return ep;
            }
            catch(Exception ex)
            {
                return null;
            }

        }






        private SonarrClient client;
        private Configuration.Configuration config;
        private SonarrSharp.Endpoints.Parse.Parse parser;
    }
}
