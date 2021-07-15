using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;


namespace codeGen
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            updateCpuInfo();
            cpu.ExternalWrite = (v, a) =>
            {
                tdisp.WriteByte(a, v);
            };
            cpu.OutPortChanged = (ind, v) =>
            {
                if (ind == 2)
                {
                    tdisp.CurrentFb = v == 2 ? 1 : 0;
                }

            };
            mdi.CurrentCpu = cpu;
            mdi.CurrentTdisp = tdisp;
        }

        public VarInfo[] Generate()
        {
            richTextBox2.Clear();
            listView1.Items.Clear();
            var ret = CodeParser.Generate(richTextBox1.Lines, textBox5.Text);
            richTextBox2.Lines = ret.Code.ToArray();
            var blocks = ret.CodeBlocks;
            code = richTextBox2.Lines.ToArray();

            #region parse code from blocks
            StringBuilder sb = new StringBuilder();
            listView1.Items.Clear();
            int cntr = 0;

            if (checkBox1.Checked)
            {
                foreach (var item in blocks)
                {
                    if (item.Code == null) continue;
                    var ar = item.Code.ToArray();
                    ar = ar.Where(z => !z.StartsWith("3DD")).ToArray();

                    item.Text = string.Join(Environment.NewLine, ar) + Environment.NewLine;
                    item.Text = item.Text.Replace("E000" + Environment.NewLine + "A103", "A113");
                }
            }

            foreach (var item in blocks)
            {
                sb.Append(item.Text);
                if (item.Code != null)
                    foreach (var citem in item.Code)
                    {
                        listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), citem, item.Source }) { Tag = cntr });
                        cntr++;
                    }
                if (item.Goto != null)
                {
                    //insert goto code
                    //calc offset to required block
                    var ind1 = blocks.IndexOf(item.Goto);
                    int offset = 0;
                    for (int i = 0; i < ind1; i++)
                    {
                        if (blocks[i].Code != null)
                        {
                            offset += blocks[i].Code.Length;
                        }
                        if (blocks[i].Goto != null) { offset += 5; }

                    }
                    foreach (var item2 in CodeParser.GetWriteReg1(offset, true))
                    {
                        sb.Append(item2 + Environment.NewLine);
                        listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), item2, item.Source }) { });
                        cntr++;
                    }
                    switch (item.JumpMode)
                    {
                        case GotoJumpMode.Jump:
                            sb.Append("B800" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "B800 (jmp)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JE:
                            sb.Append("BA00" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "BA00 (je)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JNE:
                            sb.Append("BC00" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "BC00 (jne)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JL:
                            sb.Append("AD03" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "AD03 (jl)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JLE:
                            sb.Append("AD04" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "AD04 (jle)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JG:
                            sb.Append("AD01" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "AD01 (jg)", item.Source }) { });

                            cntr++;
                            break;
                        case GotoJumpMode.JGE:
                            sb.Append("AD02" + Environment.NewLine);
                            listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X"), "AD02 (jge)", item.Source }) { });

                            cntr++;
                            break;
                    }



                    //sb.Append("BA00" + Environment.NewLine);
                    // sb.Append("BC00" + Environment.NewLine);
                }
            }
            richTextBox2.Text = sb.ToString();
            if (checkBox2.Checked)
            {

                List<int[]> gotos = new List<int[]>();
                for (int i = 0; i < richTextBox2.Lines.Length; i++)
                {
                    var txt = richTextBox2.Lines[i];
                    if (txt.StartsWith("B") || txt.StartsWith("AD03")
                        || txt.StartsWith("AD04")
                        || txt.StartsWith("AD02")
                        || txt.StartsWith("AD01"))
                    {
                        List<string> accum = new List<string>();
                        for (int j = 0; j < 4; j++)
                        {
                            var txt1 = richTextBox2.Lines[i - 1 - j];
                            if (txt1.StartsWith("AC") || txt1.StartsWith("1C"))
                            {
                                accum.Add(txt1);
                                continue;
                            }
                            break;
                        }
                        //restore address here

                        if (accum.Any())
                        {
                            accum.Reverse();
                            var jn = string.Join("", accum.Select(z => z.Substring(2)));
                            var addr = Convert.ToInt32("0x" + jn, 16);
                            var aa = accum.Select(z => z.Substring(2)).ToArray();
                            int reduce = 0;
                            for (int ii = 0; ii < aa.Length; ii++)
                            {
                                if (aa[ii] != "00") break;
                                reduce++;
                            }
                            if (reduce == 4) reduce = 3;

                            gotos.Add(new int[] { i, addr, reduce });
                        }
                        else
                        {
                            throw new ArgumentException();
                        }
                    }
                }

                //fix only goto adresses
                int[] oldaddr = new int[gotos.Count];
                for (int jj = 0; jj < gotos.Count; jj++)
                {
                    oldaddr[jj] = gotos[jj][0];
                }
                for (int jj = 0; jj < gotos.Count; jj++)
                {
                    var pre = gotos.Where(u => u[1] > gotos[jj][0]).ToArray();
                    var post = gotos.Where(u => u[0] >= gotos[jj][0]).ToArray();
                    for (int t = 0; t < pre.Length; t++)
                    {
                        pre[t][1] -= gotos[jj][2];
                    }
                    for (int t = 0; t < post.Length; t++)
                    {
                        post[t][0] -= gotos[jj][2];
                    }
                    //gotos[jj][0] -= gotos[jj][2];
                }

                //fix all adresses inplace
                List<string> rr = new List<string>();

                cntr = 0;
                int oldlimit = 0;
                for (int ii = 0; ii < richTextBox2.Lines.Length; ii++)
                {

                    string item = richTextBox2.Lines[ii];
                    if (string.IsNullOrEmpty(item)) continue;
                    rr.Add(item + ";;" + listView1.Items[ii].SubItems[2].Text);


                    if (oldaddr.Any(z => z == ii) && cntr > oldlimit)
                    {
                        var ind = oldaddr.First(z => z == ii);
                        oldlimit = cntr;
                        var ll1 = oldaddr.ToList();
                        ind = oldaddr.ToList().IndexOf(ind);
                        var gt = gotos[ind];

                        List<string> accum = new List<string>();
                        for (int j = 0; j < 4; j++)
                        {
                            var txt1 = richTextBox2.Lines[ii - 1 - j];
                            if (txt1.StartsWith("AC") || txt1.StartsWith("1C"))
                            {
                                accum.Add(txt1);
                                continue;
                            }
                            break;
                        }
                        int reduce = 0;
                        accum.Reverse();

                        var aa = accum.Select(z => z.Substring(2)).ToArray();

                        
                        reduce = accum.Count;

                        var rrr = CodeParser.GetWriteReg1(gt[1]);
                        var spl = rr[rr.Count - 2].Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                        for (int i3 = 0; i3 < reduce; i3++)
                        {
                            rr.RemoveAt(rr.Count - 2);
                        }

                        foreach (var item2 in rrr)
                        {
                            rr.Insert(rr.Count - 1, item2 + ";;" + spl[1]);
                        }
                        cntr -= reduce;
                        cntr += rrr.Length;
                    }
                    cntr++;

                }

                richTextBox2.Lines = rr.Select(z => z.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries)[0]).ToArray();
                code = richTextBox2.Lines.ToArray();

                
                listView1.Items.Clear();
                cntr = 0;
                foreach (var item in rr)
                {
                    var spl = item.Split(new string[] { ";;" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    listView1.Items.Add(new ListViewItem(new string[] { cntr.ToString("X4"), spl[0], spl[1] }) { Tag = cntr });
                    cntr++;
                }

            }
            else
            {
                code = richTextBox2.Lines.ToArray();
            }

            #endregion

            return ret.Vars.ToArray();
        }


        string[] code;
        private void button1_Click(object sender, EventArgs e)
        {
            var vars = Generate();
            listView4.Items.Clear();
            foreach (var v in vars)
            {
                listView4.Items.Add(new ListViewItem(new string[] { v.Name, v.Address.ToString("X2"), "" }) { Tag = v });
            }
        }

        private void richTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                Generate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.hex|*.hex";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            if (!sfd.FileName.EndsWith("hex")) { MessageBox.Show("error extension"); return; }
            Generate();
            File.WriteAllText(sfd.FileName, CodeGenerator.GenerateHex(code, wordSize, memSize));
        }

        int memSize = 0x1000;
        private void button3_Click(object sender, EventArgs e)
        {
            Generate();
            richTextBox2.Text = CodeGenerator.GenerateHex(code, wordSize, memSize);
        }

        int lastSearchId = -1;
        private void button4_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < listView1.Items.Count; i++)
            {
                listView1.Items[i].Selected = false;
            }
            bool was = false;
            bool have = false;
            while (!was)
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    if (listView1.Items[i].SubItems[1].Text.Contains(textBox3.Text) || listView1.Items[i].SubItems[2].Text.Contains(textBox3.Text))
                    {
                        if (i > lastSearchId)
                        {
                            listView1.Items[i].Selected = true;
                            listView1.Items[i].EnsureVisible();
                            lastSearchId = i;
                            was = true;
                            break;
                        }
                        else
                        {
                            have = true;
                        }
                    }
                }
                if (!was && have)
                {
                    lastSearchId = -1;
                }
                if (!have)
                {
                    break;
                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.hex|*.hex";
            if (sfd.ShowDialog() != DialogResult.OK) return;
            if (!sfd.FileName.EndsWith("hex")) { MessageBox.Show("error extension"); return; }

            File.WriteAllText(sfd.FileName, CodeGenerator.GenerateHex(richTextBox2.Lines.ToArray(), wordSize, memSize));
        }

        private void ledBlinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("led.txt");            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            timer1.Enabled = !timer1.Enabled;
            if (timer1.Enabled)
            {
                toolStripButton1.Text = "stop emulation";
            }
            else
            {
                toolStripButton1.Text = "run emulation";
            }

        }

        public CpuState cpu = new CpuState();
        private void timer1_Tick(object sender, EventArgs e)
        {

            var code = richTextBox2.Lines.ToArray();
            cpu.Code = code.Where(z => z != string.Empty).ToArray();
            cpu.Step();

            DrawPeripheral(cpu);
            updateCpuInfo();

            if (listView1.Items.Count > cpu.prevCodePointer)
            {
                listView1.Items[(int)cpu.prevCodePointer].BackColor = Color.White;
            }
            if (listView1.Items.Count > cpu.codePointer)
                listView1.Items[(int)cpu.codePointer].BackColor = Color.LightBlue;

            if (bps.Contains(cpu.codePointer)) timer1.Enabled = false;
        }

        public void updateCpuInfo()
        {
            listView2.BeginUpdate();
            listView2.Items.Clear();
            listView2.Items.Add(new ListViewItem(new string[] { "reg1", cpu.reg1.ToString("X2"), cpu.reg1.ToString() }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "reg2", cpu.reg2.ToString("X2"), cpu.reg2.ToString() }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "vRegReadAddr", cpu.vRegReadAddr.ToString("X2"), cpu.vRegReadAddr.ToString() }) { });
            if (cpu.Code != null && cpu.codePointer < cpu.Code.Length)
                listView2.Items.Add(new ListViewItem(new string[] { "instr", cpu.Code[cpu.codePointer] }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "cp", cpu.codePointer.ToString("X2"), cpu.codePointer.ToString() }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "z", cpu.z + "" }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "s", cpu.s + "" }) { });
            listView2.Items.Add(new ListViewItem(new string[] { "intr.counter", cpu.InstrCounter.ToString("N0") + "" }) { });
            listView2.EndUpdate();
            listView3.Items.Clear();
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    switch (comboBox1.SelectedIndex)
                    {
                        case 0:
                            {
                                listView3.Items.Add(new ListViewItem(new string[] { (memOffset + i).ToString("X2"),
                        cpu.ram1[memOffset + i].ToString("X2"),
                        cpu.ram1[memOffset + i].ToString() ,
                        radioButton2.Checked?BitConverter.ToSingle(BitConverter.GetBytes( cpu.ram1[memOffset + i]<<16),0).ToString():
                        BitConverter.ToSingle(BitConverter.GetBytes( cpu.ram1[memOffset + i]),0).ToString()
                    }
                                )
                                { });
                            }
                            break;
                        case 1:
                            {
                                listView3.Items.Add(new ListViewItem(new string[] {
                        (memOffset + i).ToString("X2"),
                        cpu.ram2[memOffset + i].ToString("X2"),
                        cpu.ram2[memOffset + i].ToString()


                    })
                                { });
                            }
                            break;
                        case 2:
                            {
                                listView3.Items.Add(new ListViewItem(new string[] { (memOffset + i).ToString("X2"),
                        cpu.sdram[memOffset + i].ToString("X2"),
                        cpu.sdram[memOffset + i].ToString() ,
                          radioButton2.Checked?
                          BitConverter.ToSingle(BitConverter.GetBytes((uint)(cpu.sdram[memOffset + i]<<16)),0).ToString()
                          :
                        BitConverter.ToSingle(BitConverter.GetBytes(((uint)(cpu.sdram[memOffset + i+1])<<16)|cpu.sdram[memOffset + i]),0).ToString()                    }
                                )
                                { }); ;
                            }
                            break;
                    }


                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        int memOffset = 0;

        Bitmap ledBmp = null;
        Graphics ledGr;
        public void DrawPeripheral(CpuState cpu)
        {
            if (ledBmp == null)
            {
                ledBmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                ledGr = Graphics.FromImage(ledBmp);

            }
            ledGr.Clear(SystemColors.Control);
            for (int i = 0; i < 4; i++)
            {
                int ww = 10;
                int gap = 5;
                ledGr.FillEllipse(Brushes.Gray, i * (ww + gap), 5, ww, ww);
                if ((cpu.OutpA & (1 << i)) > 0)
                {
                    ledGr.FillEllipse(Brushes.LightGreen, i * (ww + gap), 5, ww, ww);

                }
                ledGr.DrawEllipse(Pens.Black, i * (ww + gap), 5, ww, ww);
            }
            label7.Text = "HEX: " + cpu.OutpB.ToString("X4");
            pictureBox1.Image = ledBmp;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var code = richTextBox2.Lines.ToArray();
            cpu.Code = code.Where(z => z != string.Empty).ToArray();
            cpu.Step();
            DrawPeripheral(cpu);
            updateCpuInfo();

            listView1.Items[(int)cpu.prevCodePointer].BackColor = Color.White;
            listView1.Items[(int)cpu.codePointer].BackColor = Color.LightBlue;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            cpu.Reset();
            DrawPeripheral(cpu);
            updateCpuInfo();
        }

        private void keybledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("keyb.txt");            
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var ind = textBox5.Text.IndexOf(textBox6.Text[0]);
            MessageBox.Show(ind + "; 0x" + ind.ToString("X2"));
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                memSize = (int)Convert.ToUInt32(textBox2.Text, 16);
            }
            catch { }
        }

        private void cliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("cli.txt");
        }

        private void readFileSdToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("readfile.txt");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            richTextBox1.Text = File.ReadAllText(ofd.FileName);
        }

        private void sdChecksumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("sdchecksum.txt");            
        }

        string sdpath = "";        
        FileStream sdstream;
        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            cpu.SdReader = (addr) =>
            {
                //add cache page here
                sdstream.Seek(addr, SeekOrigin.Begin);
                return (byte)sdstream.ReadByte();
            };
            sdpath = ofd.FileName;
            sdstream = File.OpenRead(sdpath);            
        }

        private void bookToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("book.txt");
        }

        private void imgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("bmp.txt");
        }

        TextDisplay tdisp = new TextDisplay();

        private void button8_Click(object sender, EventArgs e)
        {
            cpu.fifoq.Enqueue(0x5a);
            cpu.fifoq.Enqueue(0xf0);
            cpu.fifoq.Enqueue(0x5a);
            label9.Text = cpu.fifoq.Count + "";
        }

        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                memOffset = Convert.ToInt32(textBox4.Text, 16);
                updateCpuInfo();
            }
            catch  { }
        }
        Thread th;
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            if (th != null)
            {
                toolStripStatusLabel1.Text = "already runned";
                toolStripStatusLabel1.BackColor = Color.Yellow;
                return;
            }
            var code = richTextBox2.Lines.ToArray();
            cpu.Code = code.Where(z => z != string.Empty).ToArray();

            th = new Thread(() =>
           {
               //cpu.Reset();
               statusStrip1.Invoke((Action)(() =>
               {
                   toolStripStatusLabel1.Text = "cpu reseted..";
                   toolStripStatusLabel1.BackColor = Color.White;
                   toolStripStatusLabel1.ForeColor = Color.Black;

               }));
               while (true)
               {
                   cpu.Step();
                   if (cpu.paused)
                   {
                       statusStrip1.Invoke((Action)(() =>
                       {
                           toolStripStatusLabel1.Text = "cpu finished";
                           toolStripStatusLabel1.BackColor = Color.White;
                           toolStripStatusLabel1.ForeColor = Color.Blue;
                       }));

                       break;
                   }
                   if (bps.Contains(cpu.codePointer))
                   {
                       statusStrip1.Invoke((Action)(() =>
                       {
                           toolStripStatusLabel1.Text = "bp fired";
                           toolStripStatusLabel1.BackColor = Color.White;
                           toolStripStatusLabel1.ForeColor = Color.Blue;
                       }));

                       break;
                   }
               }
               th = null;


           });

            th.IsBackground = true;
            th.Start();
        }

        private void gotoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var addr = Convert.ToUInt32(listView1.SelectedItems[0].SubItems[0].Text, 16);
            cpu.codePointer = addr;
        }
        List<uint> bps = new List<uint>();

        private void bpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var addr = Convert.ToUInt32(listView1.SelectedItems[0].SubItems[0].Text, 16);
            if (bps.Contains(addr)) { listView1.SelectedItems[0].BackColor = Color.White; bps.Remove(addr); return; }
            bps.Add(addr);
            listView1.SelectedItems[0].BackColor = Color.LightYellow;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                int offset = 0;
                if (textBox7.Text.StartsWith("0x"))
                {
                    offset = Convert.ToInt32(textBox7.Text.Replace("0x", ""), 16);
                }
                else
                {
                    offset = int.Parse(textBox7.Text);
                }
                uint value = 0;
                if (textBox8.Text.StartsWith("0x"))
                {
                    value = Convert.ToUInt32(textBox8.Text.Replace("0x", ""), 16);
                }
                else
                {
                    value = uint.Parse(textBox8.Text);
                }

                switch (comboBox1.SelectedIndex)
                {
                    case 0:

                        cpu.ram1[offset] = value;
                        break;
                    case 1:
                        cpu.ram2[offset] = (byte)value;
                        break;
                    case 2:
                        cpu.sdram[offset] = (ushort)value;
                        break;
                }

                updateCpuInfo();
            }
            catch { }
        }

        private void sdramFillerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("sdramFilter.txt");
        }

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateCpuInfo();
        }

        private void grayscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("gray.txt");
        }

        private void img12bitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("img12bit.txt");
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateCpuInfo();
        }

        private void float1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("float1.txt");            
        }

        private void sdramImgSampleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("sdramImgSample.txt");            
        }

        private void dirTraverseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("dir.txt");
        }

        void loadSample(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
  .Single(str => str.Contains(name));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                richTextBox1.Text = result;
            }
        }
        private void complexConditionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("complexCond.txt");            
        }

        private void complex2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("complex2.txt");            
        }

        private void remToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadSample("rem.txt");            
        }

        private void label13_Click(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button12_Click(object sender, EventArgs e)
        {
            cpu.sdram = new ushort[cpu.sdram.Length];
            updateCpuInfo();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bts = File.ReadAllBytes(ofd.FileName);
            int offset = 0;
            if (!string.IsNullOrEmpty(textBox10.Text))
                if (textBox10.Text.Contains("0x"))
                {
                    offset = Convert.ToInt32(textBox10.Text, 16);
                }
                else
                {
                    offset = int.Parse(textBox10.Text);
                }
            int shift = offset;
            for (int i = 0; i < bts.Length; i += 2)
            {
                ushort data = (ushort)(bts[i] | (bts[i + 1] << 8));
                cpu.sdram[shift] = data;
                shift++;

            }
            updateCpuInfo();
        }

        private void converImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = assembly.GetManifestResourceNames()
  .Single(str => str.Contains("bf16.txt"));
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                richTextBox1.Text = result;
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bmp = Bitmap.FromFile(ofd.FileName) as Bitmap;
            int cntr = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    var p = bmp.GetPixel(i, j);
                    cpu.sdram[cntr++] = p.R;
                    cpu.sdram[cntr++] = p.G;
                    cpu.sdram[cntr++] = p.B;


                }
            }
            updateCpuInfo();

        }

        private void button15_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var bts = File.ReadAllBytes(ofd.FileName);

            int offset = 0;
            if (!string.IsNullOrEmpty(textBox10.Text))
            {

                if (textBox10.Text.Contains("0x"))
                {
                    offset = Convert.ToInt32(textBox10.Text, 16);
                }
                else
                {
                    offset = int.Parse(textBox10.Text);
                }
            }
            for (int i = 0; i < bts.Length; i += 4)
            {
                cpu.sdram[offset++] = (ushort)(bts[i + 2] | (bts[i + 3] << 8));
            }
            updateCpuInfo();

        }


        private void button16_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            List<byte> bts = new List<byte>();
            foreach (var item in cpu.sdram)
            {
                bts.AddRange(BitConverter.GetBytes(item));
            }

            File.WriteAllBytes(sfd.FileName, bts.ToArray());
        }

        private void button9_Click(object sender, EventArgs e)
        {

        }
        int wordSize = 2;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                wordSize = int.Parse(textBox1.Text);
            }
            catch { }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            File.WriteAllText(ofd.FileName, richTextBox1.Text);
        }
    }

}
