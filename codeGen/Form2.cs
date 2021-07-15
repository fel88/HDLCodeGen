using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace codeGen
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            textBox6.Text += " ";
            for (char i = 'а'; i <= 'я'; i++)
            {
                textBox6.Text += i;
            }
            for (char i = 'А'; i <= 'Я'; i++)
            {
                textBox6.Text += i;
            }
            for (char i = 'a'; i <= 'z'; i++)
            {
                textBox6.Text += i;
            }
            for (char i = 'A'; i <= 'Z'; i++)
            {
                textBox6.Text += i;
            }
            for (char i = '0'; i <= '9'; i++)
            {
                textBox6.Text += i;
            }
            textBox6.Text += "!@#$%^&*()-+.,/\\'\"><!\"№;%:?*[]{}`~_╝║д╣╕╖╢╡╜╛└┴├…°";
            // alt+188 ╝
            label1.Text = textBox6.Text.Length + "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            Bitmap bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;
            List<byte> bts = new List<byte>();
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    var px = bmp.GetPixel(i, j);
                    bts.Add(px.R);
                    bts.Add(px.G);
                    bts.Add(px.B);
                }
            }
            File.WriteAllBytes("outp.dat", bts.ToArray());
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var n = int.Parse(textBox1.Text);
            long sum = 0;
            for (int i = 0; i < n; i++)
            {
                sum ^= i;

            }
            textBox2.Text = sum + "";
            MessageBox.Show(sum + "");
        }

        public static string generateHex(byte[] data, int words, int wordSize = 2)
        {
            StringBuilder sb = new StringBuilder();

            int shift = 0;
            var uu = words;


            int memSize = uu * wordSize;
            int chunkSize = 2;
            for (int i = 0; i < memSize; i += chunkSize)
            {
                var cnt = memSize - i;
                if (cnt > chunkSize) { cnt = chunkSize; }
                sb.Append($":{cnt.ToString("X2")}{shift.ToString("X4")}00");
                byte crc = (byte)cnt;
                crc += (byte)(shift & (0xff));
                crc += (byte)((shift >> 8) & (0xff));
                for (int j = 0; j < cnt; j++)
                {
                    if (data.Length <= (i + j))
                    {
                        sb.Append("00");
                    }
                    else
                    {
                        sb.Append(data[i + j].ToString("X2"));
                        crc += data[i + j];
                    }
                }
                crc = (byte)(~crc);
                crc++;

                sb.AppendLine($"{crc.ToString("X2")}");
                shift += (cnt / wordSize);

            }
            sb.AppendLine(":00000001FF");
            return sb.ToString();
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        public static byte[] GetBytes(string bitString)
        {
            return Enumerable.Range(0, bitString.Length / 8).
                Select(pos =>
                {
                    var str = Reverse(bitString.Substring(pos * 8, 8));

                    return Convert.ToByte(str, 2);
                }

                ).ToArray();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            //Bitmap bmp = Clipboard.GetImage() as Bitmap;
            Bitmap bmp = new Bitmap(16, 16);

            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.DrawString(textBox4.Text, new Font(fontName, fontSize), Brushes.Black, -2, -2);
            pictureBox1.Image = bmp;
            StringBuilder bb = new StringBuilder();
            Bitmap outb = new Bitmap(16, 16);
            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {

                    var clr = bmp.GetPixel(i, j);
                    var g = (clr.R + clr.G + clr.B) / 3;
                    if (g < 128)
                    {
                        outb.SetPixel(i, j, Color.Black);
                        bb.Append("1");
                    }
                    else
                    {
                        outb.SetPixel(i, j, Color.White);
                        bb.Append("0");
                    }
                }
            }
            pictureBox2.Image = outb;
            var bts = GetBytes(bb.ToString());
            data = bts;
            textBox3.Text = string.Empty;
            textBox5.Text = string.Empty;

            for (int i = 0; i < bts.Length; i++)
            {
                textBox3.Text += bts[i].ToString("X2") + " ";
            }
            for (int i = 0; i < bts.Length; i++)
            {
                textBox5.Text += (char)bts[i];
            }
            richTextBox1.Clear();
            for (int i = 0; i < bb.Length; i += 16)
            {
                richTextBox1.AppendText(bb.ToString().Substring(i, 16) + Environment.NewLine);
            }
        }
        string fontName = "Consolas";
        int fontSize = 12;
        byte[] generateCharCode(char ch)
        {
            //Bitmap bmp = Clipboard.GetImage() as Bitmap;
            Bitmap bmp = new Bitmap(16, 16);

            var gr = Graphics.FromImage(bmp);
            gr.Clear(Color.White);
            gr.DrawString(ch + "", new Font(fontName, fontSize), Brushes.Black, -2, -2);

            StringBuilder bb = new StringBuilder();

            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {

                    var clr = bmp.GetPixel(i, j);
                    var g = (clr.R + clr.G + clr.B) / 3;
                    if (g < 128)
                    {

                        bb.Append("1");
                    }
                    else
                    {

                        bb.Append("0");
                    }
                }
            }

            var bts = GetBytes(bb.ToString());

            return bts;
        }
        byte[] data;
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, generateHex(data, 4096));
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            List<byte> data = new List<byte>();
            for (int i = 0; i < textBox6.Text.Length; i++)
            {
                data.AddRange(generateCharCode(textBox6.Text[i]));

            }
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, generateHex(data.ToArray(), 4096));
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox8.Text = string.Empty;
            List<byte> data = new List<byte>();
            for (int i = 0; i < richTextBox2.Text.Length; i++)
            {
                var ind = textBox6.Text.IndexOf(richTextBox2.Text[i]);

                data.Add((byte)ind);
            }
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, generateHex(data.ToArray(), 4096, 1));
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            fontSize = (int)numericUpDown1.Value;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bb = File.ReadAllBytes(ofd.FileName);
            ushort sum = 0;
            ushort sum2 = 0;
            for (int i = 0; i < bb.Length; i++)
            {
                sum2 += bb[i];
            }
            for (int i = 0; i < 64 * 512 * 10; i++)
            {
                sum += bb[i];
            }
            MessageBox.Show((sum).ToString("X2") + "; sum2: " + sum2.ToString("X2"));

        }

        private void button8_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;
            List<byte> ret = new List<byte>();

            for (int j = 0; j < bmp.Height; j++)
            {
                for (int i = 0; i < bmp.Width; i++)
                {
                    var px = bmp.GetPixel(i, j);
                    var r = (byte)(px.R / 16);
                    var g = (byte)(px.G / 16);
                    var b = (byte)(px.B / 16);
                    var res = (byte)(r | (g << 4));
                    ret.Add(res);
                    ret.Add(b);
                }
            }
            File.WriteAllBytes("bmp0.dat", ret.ToArray());
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button9_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bts = File.ReadAllBytes(ofd.FileName);
            Bitmap bmp = new Bitmap(800, 600);


            int pos = 0;
            for (int i = 0x36; i < bts.Length; i += 3)
            {
                int x = (pos) % 800;
                int y = (pos) / 800;
                bmp.SetPixel(x, y, Color.FromArgb(bts[i + 2], bts[i + 1], bts[i]));
                pos++;
            }
            pictureBox3.Image = bmp;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            List<byte> data = new List<byte>();
            for (int i = 0; i < textBox6.Text.Length; i++)
            {
                data.AddRange(generateCharCode(textBox6.Text[i]));

            }
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllBytes(sfd.FileName, (data.ToArray()));
            }
        }
    }
}
