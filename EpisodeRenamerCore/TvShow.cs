using System.Net;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;




namespace EpisodeRenamer
{
    public class Episode
    {

        public uint season { get; set; }
        public uint number { get; set; }
        public uint TVDBID { get; set; }
        public int MazeID { get; set; }
        public string title { get; set; }
        public string seriesName { get; set; }
        public string previousFileName { get; set; }
        public string newFileName { get; set; }
        public string extension { get; set; }
        public string previousPath { get; set; }
        public string newPath { get; set; }






    }



    public class TvShow
    {
       
       
        public string Name { get; set; }
        public string EpidguidesMap { get; set; }
        public uint TVDBID { get; set; }

    }

    public class FileUtils
    {
        public static void RenameFile(string oldName, string newName)
        {

            FileInfo prevDir = new FileInfo(oldName);
            FileInfo newDir = new FileInfo(newName);

            if(!newDir.Directory.Exists)
            {
                newDir.Directory.Create();
            }

            File.Move(oldName, newName);


        }
        public FileUtils() { }
        public FileUtils(string information)
        {
            this.information = information;
        }

        public void download(string url, string fileName)
        {
            WebClient client = new WebClient();



            client.DownloadFile(url, fileName);
            

            

        }

        public string Information
        {
            get
            {
                return information;
            }
            set
            {
                information = value;
            }
        }

        public void downloadAsString(string url)
        {
            WebClient client = new WebClient();
            //client.DownloadFile(url, fileName);
            Stream data = client.OpenRead(url);
            StreamReader reader = new StreamReader(data);
            this.information = reader.ReadToEnd();
        }

        public void load(string path)
        {
            try {
                var tempFile = System.IO.File.OpenText(path);

                this.information = tempFile.ReadToEnd();

                tempFile.Close();
            }
            catch( Exception e)
            {
                this.information = "";
            }
        }

        public bool WriteComment(string whereToWrite, string commentChars, string comment)
        {
            try
            {
                int index = information.IndexOf(whereToWrite);

                var parts = comment.Split('\n');

                string[] FormattedComment = new string[parts.Length];

                for (int i = 0; i < parts.Length; i++)
                {
                    FormattedComment[i] = commentChars + parts[i];
                }

                comment = "";

                foreach (var part in FormattedComment)
                {
                    comment += ("\n"+part);
                }

                information = information.Insert(index, comment+"\n");

                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void save(string fileName)
        {
            System.IO.File.WriteAllText(@fileName, information);
        }

        protected string information;
    }

    class CsvParser
    {
        static public void clearFile( ref FileUtils file)
        {
            string temp = file.Information;

            temp = temp.Substring(temp.IndexOf("<pre>")+7);

            temp = temp.Substring(0, temp.IndexOf("</pre>")-6);

            file.Information = temp;

        }
        
        static public TvShow FillTVShowInformation(string[] line)
        {
            var temp = new TvShow();
            temp.Name = line[0];
            temp.EpidguidesMap = line[3];

            return temp;
        }
        static public List<TvShow> getAllPossibleTVShows(string showName, FileUtils file)
        {
            List<TvShow> AllShows = new List<TvShow>();
            var reader = new StringReader(file.Information);
            CsvHelper.CsvParser parser = new CsvHelper.CsvParser(reader);

            string[] line;
           
            string[] header = parser.Read();
            while ( (line = parser.Read()) != null)
            {
                if( line[0].ToLower() == showName.ToLower())        // Verifico el nombre
                {
                    AllShows.Add(FillTVShowInformation(line));
                }
            }

            if( AllShows.Count == 1)
            {
                return AllShows;
            }

            parser.Dispose();
            reader = new StringReader(file.Information);
            parser = new CsvHelper.CsvParser(reader);

            while ((line = parser.Read()) != null)
            {
                if (line[0].ToLower().Contains(showName.ToLower()))        // Me fijo si hay parte de un nombre aca y lo agrego a la lsita
                {
                    AllShows.Add(FillTVShowInformation(line));
                }
            }
            return AllShows;
        }

        //static public void getAllEpisodesFromSeason(TvShow show,FileUtils file,int season)
        //{
        //    var reader = new StringReader(file.Information);
        //    CsvHelper.CsvParser parser = new CsvHelper.CsvParser(reader);

        //    string[] line;

        //    string[] header = parser.Read();
        //    while ((line = parser.Read()) != null)
        //    {
        //        int ss = -1;
        //        if(Int32.TryParse(line[1], out ss))
        //            if (season == ss)
        //            {
        //                Episode temp = new Episode();

        //                //temp.number = line[2];
        //                //temp.season = line[1];
        //                temp.title = line[4];
        //                show.EpisodeList.Add(temp);
        //            }
        //    }
        //}

        static public void getEpisode(FileUtils file, ref Episode ep)
        {
            var reader = new StringReader(file.Information);
            CsvHelper.CsvParser parser = new CsvHelper.CsvParser(reader);

            string[] line;
            int season = -1;
            int episodeNumber = -1;
            string[] header = parser.Read();
            while ((line = parser.Read()) != null)
            {

                if(Int32.TryParse(line[1], out season) && Int32.TryParse(line[2], out episodeNumber))

                    if (ep.season == season && ep.number == episodeNumber)
                    {
                        ep.title = line[4];
                    }
            }
        }
    }
}
