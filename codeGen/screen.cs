using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace codeGen
{
    public partial class screen : Form
    {
        public screen()
        {
            InitializeComponent();
            bmp = new Bitmap(800, 600);
            gr = Graphics.FromImage(bmp);

            //load default font
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
  .Single(str => str.Contains("font1.dat"));

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                var bts = memoryStream.ToArray();
                int cntr = 0;
                for (int i = 0; i < bts.Length; i += 2)
                {
                    ushort uuu = (ushort)(bts[i] | (bts[i + 1] << 8));

                    font[cntr++] = uuu;

                }
            }


        }
        Bitmap bmp;
        Graphics gr;
        CpuState cpu;
        public void Init(CpuState cpu)
        {
            this.cpu = cpu;

        }

      
        Thread th;
        private void button3_Click(object sender, EventArgs e)
        {
            th = new Thread(() =>
           {
               while (true)
               {
                   if (cpu == null) return;
                   gr.Clear(Color.Black);
                   mode = (cpu.OutpD & 0x3) == 2;

                   if (mode || forceMode)
                   {
                       int x = 0;
                       int y = 0;
                       for (int i = 0; i < 800 * 600; i++)
                       {
                           var r = (cpu.sdram[i] & 0xf) * 16;
                           var g = ((cpu.sdram[i] & 0xf0) >> 4) * 16;
                           var b = ((cpu.sdram[i] & 0xf00) >> 8) * 16;

                           bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                           x++;
                           if (x == 800) { x = 0; y++; }
                       }
                   }
                   else
                   {
                       int cntr = 0;
                       for (int x = 0; x < 800; x++)
                       {
                           for (int y = 0; y < 600; y++)
                           {
                               var ind = (mdi.CurrentTdisp.CurrentFb + 1) % 2;
                               byte[] ram = mdi.CurrentTdisp.Fb0;
                               if (ind == 1)
                               {
                                   ram = mdi.CurrentTdisp.Fb1;
                               }


                               var symbX = x / 16;
                               var symbY = y / 16;
                               var sAddr = symbX + symbY * 50;
                               var t = ram[sAddr];

                               var shift = y % 16;
                               var ind1 = (t * 16 + shift);
                               var bt = font[ind1];

                               var symbShift = x % 16;
                               var bit = bt & (1 << symbShift);
                               if (bit > 0)
                               {
                                   bmp.SetPixel(x, y, Color.White);
                               }
                               else
                               {
                                   bmp.SetPixel(x, y, Color.Black);
                               }
                           }
                       }
                   }
                   pictureBox1.Invoke((Action)(() =>
                   {
                       if (pictureBox1.Image != null)
                       {
                           pictureBox1.Image.Dispose();
                       }
                       pictureBox1.Image = bmp.Clone() as Bitmap;
                   }));


               }
           });
            th.IsBackground = true;
            th.Start();

        }

        bool mode = false;
        bool forceMode = false;
        private void button2_Click(object sender, EventArgs e)
        {
            mode = true;
            Text = "VGA display";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            mode = false;
            Text = "Text display";

        }

        ushort[] font = new ushort[4096];
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bts = File.ReadAllBytes(ofd.FileName);

            int cntr = 0;
            for (int i = 0; i < bts.Length; i += 2)
            {
                ushort uuu = (ushort)(bts[i] | (bts[i + 1] << 8));

                font[cntr++] = uuu;

            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 120; i++)
            {
                mdi.CurrentTdisp.Fb0[i] = (byte)i;
                mdi.CurrentTdisp.Fb1[i] = (byte)i;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0xE0);
            cpu.fifoq.Enqueue(0x72);

            cpu.fifoq.Enqueue(0xE0);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x72);
        }

        private void button7_Click(object sender, EventArgs e)
        {

            cpu.fifoq.Enqueue(0xE0);
            cpu.fifoq.Enqueue(0x75);

            cpu.fifoq.Enqueue(0xE0);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x75);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x5A);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x5A);
        }

        private void screen_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (th != null)
            {
                th.Abort();
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            for (int i = 0; i < 100; i++)
            {
                listView1.Items.Add(new ListViewItem(new string[] { i.ToString("X2"), cpu.sdram[i].ToString("X2") }) { Tag = i });
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            for (int i = 0; i < 100; i++)
            {
                listView1.Items.Add(new ListViewItem(new string[] { i.ToString("X2"), mdi.CurrentTdisp.Fb0[i].ToString("X2") }) { Tag = i });
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            for (int i = 0; i < 100; i++)
            {
                listView1.Items.Add(new ListViewItem(new string[] { i.ToString("X2"), mdi.CurrentTdisp.Fb1[i].ToString("X2") }) { Tag = i });
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;

            for (int i = 0; i < bmp.Width; i++)
            {
                if (i > 800) continue;
                for (int j = 0; j < bmp.Height; j++)
                {
                    if (j > 600) break;
                    int pos = i + j * 800;
                    var px = bmp.GetPixel(i, j);
                    var r = px.R / 16;
                    var g = px.G / 16;
                    var b = px.B / 16;
                    cpu.sdram[pos] = (ushort)(r | (g << 4) | (b << 8));
                }
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            forceMode = !forceMode;

            button13.BackColor = forceMode ? Color.Red : Color.Yellow;
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (tableLayoutPanel1.ColumnStyles[1].Width != 0)
            {
                tableLayoutPanel1.ColumnStyles[1].Width = 0;
            }
            else
            {
                tableLayoutPanel1.ColumnStyles[1].Width = 250;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            lock (cpu.Profiler)
            {
                foreach (var item in cpu.Profiler.OrderByDescending(z=>z.Value))
                {
                    listView1.Items.Add(new ListViewItem(new string[] { item.Key, item.Value.ToString("N0") }) { Tag = item });
                }
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            cpu.ProfilerEnabled = checkBox2.Checked;
        }

        private void button16_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x1D);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x1D);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x1C);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x1C);
        }

        private void button18_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x16);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x16);
        }

        private void button19_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x66);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x66);
        }

        private void button20_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x29);
            cpu.fifoq.Enqueue(0xF0);
            cpu.fifoq.Enqueue(0x29);
        }
    }
}
