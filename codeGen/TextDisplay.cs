using System.Drawing;

namespace codeGen
{
    public class TextDisplay
    {
        public byte[] Fb0 = new byte[4096];
        public byte[] Fb1 = new byte[4096];
        public int CurrentFb;

        public Bitmap Draw()
        {
            Bitmap bmp = new Bitmap(800, 600);
            return bmp;
        }

        internal void WriteByte(uint a, uint v)
        {
            if (CurrentFb == 0)
            {
                Fb0[a] = (byte)(v);
            }
            else
            {
                Fb1[a] = (byte)v;
            }
        }
    }
}
