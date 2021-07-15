using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace codeGen
{
    public partial class Scheme : Form
    {
        public Scheme()
        {
            InitializeComponent();
            bmp = new Bitmap(2000, 1500);
            gr = Graphics.FromImage(bmp);
        }
        Bitmap bmp;
        Graphics gr;
        List<Block> blocks = new List<Block>();
        private void timer1_Tick(object sender, EventArgs e)
        {
            gr.Clear(Color.White);
            foreach (var item in blocks)
            {
                item.Draw(gr);
            }
            pictureBox1.Image = bmp;

        }

        internal void Init(CpuState currentCpu)
        {            
            blocks.Add(new Block() { Rect = new Rectangle(100, 100, 200, 300),Name="cpu:0" });
            blocks.Add(new Block() { Rect = new Rectangle(500, 150, 50, 80),Name="hex-indicator:0" });
            blocks.Add(new Block() { Rect = new Rectangle(500, 300, 50, 70),Name="leds:0" });
            blocks.Add(new Block() { Rect = new Rectangle(500, 400, 50, 80),Name="text/vga:0" });
        }
    }

    public class Block
    {
        public string Name;
        public Rectangle Rect;
        public List<BlockPin> Inputs = new List<BlockPin>();
        public List<BlockPin> Outputs = new List<BlockPin>();
        public void Draw(Graphics gr)
        {
            gr.DrawRectangle(Pens.Black, Rect);
            gr.DrawString(Name, SystemFonts.DefaultFont, Brushes.Black, Rect.Location);
        }

    }
    public class BlockPin
    {
        public string Name;
        public int Width;
    }
}
