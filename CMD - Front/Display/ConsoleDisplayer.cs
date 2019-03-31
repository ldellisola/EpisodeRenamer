using System;
using System.Collections.Generic;
using System.Text;

namespace EpisodeRenamer.FrontEnd

{
    class ConsoleDisplayer : IDisplay
    {
        public string getFileExtension()
        {
            Console.Clear();
            Console.Write("Please tell me the extension of your files: ");
            return Console.ReadLine();
        }

        public void log(string text)
        {
            Console.WriteLine(text);
        }

        public int getIndentedShow(object ob)
        {
            
            List<TvShow> shows = ob as List<TvShow>;

            int index = 0;
            bool decided = false;

            while (!decided)
            {
                Console.Clear();
                printAllShows(shows, index);

                switch (Console.ReadKey().Key)
                {
                    case ConsoleKey.UpArrow:
                        if (index == 0)
                            index = shows.Count - 1;
                        else
                            index--;
                        break;
                    case ConsoleKey.DownArrow:
                        if (index == shows.Count - 1)
                            index = 0;
                        else
                            index++;
                        break;
                    case ConsoleKey.Spacebar:
                        decided = true;
                        break;
                }
                System.Threading.Thread.Sleep(80);
            }
            return index;

        }

        private void printAllShows(List<TvShow> shows, int index)
        {
            Console.WriteLine("Press the 'space' key to select a show:");
            for (int i = 0; i < shows.Count; i++)
            {
                if (i == index)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(shows[i].Name);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(shows[i].Name);
                }
            }
        }

        public string getPathToFiles()
        {
            Console.Clear();
            Console.Write("Please tell me the path where the files are stored: ");
            return Console.ReadLine();
        }

        public string getSeason()
        {
            Console.Clear();
            Console.Write("Please tell me which season you are trying to rename: ");
            return Console.ReadLine();
        }

        public string getShowName()
        {
            Console.Clear();
            Console.Write("Please tell me the name of the TV show you are trying to rename: ");
            return Console.ReadLine();
        }

        public bool warnUser(string warning)
        {
            //Console.Clear();
            Console.BackgroundColor = ConsoleColor.Yellow;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(warning);
            Console.WriteLine("To continue press the 'space' key");
            if (Console.ReadKey().Key == ConsoleKey.Spacebar)
                return true;
            else
                return false;
        }

        public void showFilesToRename(List<Episode>  episodes)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;
            foreach(var ep in episodes)
            {
                if (ep.previousFileName != "" && ep.newFileName != "" && ep.newFileName != ep.previousFileName) 
                    Console.WriteLine("{0} --> {1}", ep.previousFileName, ep.newFileName);
            }
        }

        public void ShowErrors(string[] errors)
        {
            Console.WriteLine("We have encountered the following errors:");
            foreach (string err in errors)
                Console.WriteLine(err);
        }
    }
}
