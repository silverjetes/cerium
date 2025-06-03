using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cerium
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string usr = Environment.UserName;
            label1.Text = "You only have one shot, " + usr;
            pictureBox1.Image = SystemIcons.Information.ToBitmap();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
