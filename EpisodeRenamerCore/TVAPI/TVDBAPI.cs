using System;
using System.Collections.Generic;
using System.Text;
using TVDBSharp;
using TVDBSharp.Models.Requests;
using System.Linq;
using Newtonsoft.Json;

namespace EpisodeRenamer.TVAPI
{
    public class TVDBAPI : StaticTVAPI, ITVAPI
    {

        public TVDBAPI() { }

        public new static bool ClearCache()
        {
            try
            {
                if (System.IO.Directory.Exists(Configuration.Constants.TVDBFolder))
                {
                    System.IO.Directory.Delete(Configuration.Constants.TVDBFolder, true);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool OverrideSeriedID(string SeriesName, uint SeriesID)
        {
            try
            {
                var dict = LoadDictionary();

                if (dict.ContainsKey(SeriesName))
                {
                    dict[SeriesName].TVDBID = SeriesID;
                }
                else
                {
                    var tempTV = new TvShow();
                    tempTV.Name = SeriesName;
                    tempTV.TVDBID = SeriesID;

                    dict.Add(SeriesName, tempTV);
                }

                SaveDictionary(dict);
                return true;
            }
            catch
            {
                return false;
            }


        }

        public bool GetEpisode(ref Episode ep)
        {
            if(DateTime.Now> token.Expires)
            {
                var tokenReq = client.Authentication.RefreshToken();
                tokenReq.Wait();
                token = tokenReq.Result;
            }

            try
            {

                var StoreShow = new TvShow();

                if (DicSeriesID.ContainsKey(ep.seriesName))
                {
                    StoreShow = DicSeriesID[ep.seriesName];
                }
                else
                {
                    var SeriesReq = client.Series.FindSeries(ep.seriesName);
                    SeriesReq.Wait();
                    var SeriesList = SeriesReq.Result;

                    if (SeriesList.Count > 0)
                    {
                        var show = SeriesList.FirstOrDefault();
                        
                        StoreShow.Name = show.SeriesName;
                        StoreShow.TVDBID = show.ID;

                        if (!show.Aliases.Contains(ep.seriesName) && !DicSeriesID.ContainsKey(ep.seriesName))
                        {
                            DicSeriesID.Add(ep.seriesName, StoreShow);
                        }
                        
                        foreach(var temp in show.Aliases)
                        {
                            if(!DicSeriesID.ContainsKey(temp))
                                DicSeriesID.Add(temp, StoreShow);
                        }

                        SaveDictionary(DicSeriesID);

                    }
                }
                
                    var EpReq = client.Episodes.GetEpisode(StoreShow.TVDBID, ep.season, ep.number);
                    EpReq.Wait();

                    var episode = EpReq.Result;

                    ep.TVDBID = episode.ID;
                    ep.title = episode.Title;
                    ep.seriesName = StoreShow.Name;

                    NameHelper.GenerateNewName(ref ep, config);
                    NameHelper.GenerateNewPath(ref ep, config, Core.Core.FilesDirectory);


                    return true;
                
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void SaveDictionary(Dictionary<string, TvShow> dict)
        {
            try
            {
                string json = JsonConvert.SerializeObject(dict);

                new FileUtils(json).save(Configuration.Constants.TVDBFolder + "//" + DictionaryDirectory);
            }
            catch
            {

            }
        }

        public string[] init(object obj)
        {
            List<string> errors = new List<string>();
            try
            {
                config = (Configuration.Configuration)obj;

                client = new TVDBManager();
                var req = client.Authentication.GetJwtToken(new CredentialPacket(config.TVDBKey));
                req.Wait();

                token = req.Result;

                client.Authenticate(token.Token);

                DicSeriesID = LoadDictionary();


                return errors.ToArray();

            }
            catch(Exception ex)
            {
                errors.Add(ex.Message);
                return errors.ToArray();
            }
           
        }

        private static Dictionary<string, TvShow> LoadDictionary()
        {
            var TempDic = new Dictionary<string, TvShow>();

            if (System.IO.File.Exists(Configuration.Constants.TVDBFolder + "\\" + DictionaryDirectory))
            {
                string text = System.IO.File.ReadAllText(Configuration.Constants.TVDBFolder + "//" + DictionaryDirectory);

                TempDic = JsonConvert.DeserializeObject<Dictionary<string, TvShow>>(text);
            }
            else
            {
                TempDic = new Dictionary<string, TvShow>();
            }

            return TempDic;
        }

        private Dictionary<string, TvShow> DicSeriesID ;
        private TVDBManager client;
        TVDBSharp.Models.TVDBJwtToken token;
        Configuration.Configuration config;

        private const string DictionaryDirectory = @"TVDBDic.json";
    }
}
