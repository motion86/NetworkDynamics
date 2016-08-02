using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace NetworkDynamics
{
    public partial class ChartLabels : Form
    {
        public static List<int> labels { private set; get; }
        public static List<int> title { private set; get; }

        private static ChartLabels Instance = new ChartLabels();

        private ChartLabels()
        {
            InitializeComponent();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            labels = new List<int>();
            foreach (CheckBox cb in gb1.Controls)
                if (cb.Checked)
                    labels.Add(Int32.Parse(cb.Tag.ToString()));
            labels.Sort();

            title = new List<int>();
            foreach (CheckBox cb in gb2.Controls)
                if (cb.Checked)
                    title.Add(Int32.Parse(cb.Tag.ToString()));
            title.Sort();

            Hide();
        }

        public static void ShowForm(System.Drawing.Point coords)
        {
            Instance.Location = coords;
            Instance.btnDone.Location = new System.Drawing.Point(24, 234);
            Instance.Show();
        }

        public static void EnableControl(string name, bool enable, int gb)
        {
            if (name.Equals("cbS"))
                name = "cbK";
            if (name.Equals("cbtS"))
                name = "cbtK";
            try
            {
                if (gb == 1) Instance.gb1.Controls[name].Enabled = enable;
                if (gb == 2) Instance.gb2.Controls[name].Enabled = enable;
            }
            catch { }
        }

        public static void CheckControl(string name, bool check, int gb)
        {
            if (name.Equals("cbS"))
                name = "cbK";
            if (name.Equals("cbtS"))
                name = "cbtK";
            try
            {
                if (gb == 1) ((CheckBox)Instance.gb1.Controls[name]).Checked = check;
                if (gb == 2) ((CheckBox)Instance.gb2.Controls[name]).Checked = check;
            }
            catch { }
        }
    }
}
