using EpisodeRenamer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GUI___EpisodeRenamer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnSavePref_Click(object sender, EventArgs e)
        {
            EpisodeRenamer.Configuration.Configuration config = new EpisodeRenamer.Configuration.Configuration();



        }
    }
}
