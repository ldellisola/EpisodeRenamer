using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using EpisodeRenamer.Core;

namespace GUI___EpisodeRenamer
{
    static class Program
    {

        public static Core API;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            API = new Core();

            var err = API.Initialize();

            if (err.Length != 0)
            {
                throw new Exception("API did not Initialized. {numb}".Replace("{numb}", err.Length.ToString()));
            }
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }





}
