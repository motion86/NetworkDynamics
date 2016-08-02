using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkDynamics
{
    public partial class LoadFile : Form
    {
        private MainForm app;
        List<string> files;

        public LoadFile(MainForm app, string fileExtension)
        {
            InitializeComponent();
            this.app = app;
            listFiles(fileExtension);
        }

        private void listFiles(string fileExtension)
        {
            files = FileIO.GetFiles(null, fileExtension, true);

            foreach (string f in files)
                lbFileList.Items.Add(FileIO.getFileNameFromPath(f));
        }

        private void lbFileList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (btnLoadFile.Enabled == false && lbFileList.SelectedItem != null)
                btnLoadFile.Enabled = true;
        }

        private void btnFileCancel_Click(object sender, EventArgs e)
        {
            app.uncheckLoadMatrix();
            this.Close();
        }

        private void btnLoadFile_Click(object sender, EventArgs e)
        {
            VertexData.ClearRank();
            app.cbNewState(true);
            app.genNet(files[lbFileList.SelectedIndex]);
            this.Close();
        }
    }
}
