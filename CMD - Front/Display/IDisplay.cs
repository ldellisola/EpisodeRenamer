using System;
using System.Collections.Generic;
using System.Text;

namespace EpisodeRenamer.FrontEnd
{
    interface IDisplay
    {
        string getShowName();

        string getSeason();

        string getPathToFiles();

        string getFileExtension();

        int getIndentedShow(Object ob);

        bool warnUser(string warning);

        void log(string text);

        void showFilesToRename(List<Episode>episodes);

        void ShowErrors(string[] errors);
    }
}
