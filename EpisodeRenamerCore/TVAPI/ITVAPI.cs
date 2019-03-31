using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace EpisodeRenamer.TVAPI
{
    public enum type
    {
        EpiGuides,
        TVDB
    }

    public interface ITVAPI
    {
        string[] init(object obj);

        bool GetEpisode(ref Episode ep);

    }

    public class StaticTVAPI
    {

        public static bool ClearCache()
        {
            throw new Exception("Abstract Inteface Not Implemented");
        }
    }

    static class NameHelper
    {
        public static void GenerateNewName(ref Episode ep, Configuration.Configuration config)
        {
            ep.newFileName = config.NameFormat.Replace("{{Show}}", ep.seriesName).Replace("{{Title}}", ep.title).Replace("{{Season}}", ep.season.ToString("00")).Replace("{{Episode}}", ep.number.ToString("00"));

            ep.newFileName = Regex.Replace(ep.newFileName, "[><:\"/\\|?*]", "");
        }


        public static void GenerateNewPath (ref Episode ep, Configuration.Configuration config, System.IO.DirectoryInfo mainDir)
        {
            if(config.DirectoryFormat == null)
            {
                ep.newPath = ep.previousPath;
            }
            else
            {
                ep.newPath = mainDir.FullName+"\\" + config.DirectoryFormat.Replace("{{Show}}", ep.seriesName).Replace("{{Title}}", ep.title).Replace("{{Season}}", ep.season.ToString("00")).Replace("{{Episode}}", ep.number.ToString("00"));
            }

            ep.newPath = Regex.Replace(ep.newPath, "[><\"|?*]", "");
        }
    }
}
