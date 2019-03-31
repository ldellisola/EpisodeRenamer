using System;
using System.Collections.Generic;
using System.Text;

namespace EpisodeRenamer.EpisodeParser
{
    public enum Type
    {
        Sonarr
    }

    public interface IEpisodeParser
    {
        string[] init(object obj);

        Episode parseFile(string path);

        //Episode parseName(string name);


    }
}
