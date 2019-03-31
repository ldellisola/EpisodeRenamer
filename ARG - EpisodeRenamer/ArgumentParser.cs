using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace ARG___EpisodeRenamer
{
    public class ArgumentParser
    {
        public void start(string[] arr)
        {
        }
    }

    class Options
    {
        [Option('p',"-path",Default = "",HelpText ="Path to the file",Required =true)]
        public string path { get; set; }

    }
}
