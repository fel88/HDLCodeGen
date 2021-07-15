using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace codeGen
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }
        public static CpuState CurrentCpu;
        public static TextDisplay CurrentTdisp;

        private void screenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentCpu == null)
            {
                MessageBox.Show("cpu null");
                return;
            }
            screen sc = new screen();
            sc.MdiParent = this;
            sc.Init(CurrentCpu);
            sc.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (CurrentCpu == null)
            {
                MessageBox.Show("cpu null");
                return;
            }
            Scheme sc = new Scheme();
            sc.MdiParent = this;
            sc.Init(CurrentCpu);
            sc.Show();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Form2 f = new Form2();
            f.MdiParent = this;
            f.Show();
        }
    }
}
