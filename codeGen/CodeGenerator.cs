using System;
using System.Collections.Generic;
using System.Text;

namespace codeGen
{
    public class CodeGenerator
    {
        public static string GenerateHex(string[] code, int wordSize, int _memSize)
        {
            StringBuilder sb = new StringBuilder();
            List<byte> data = new List<byte>();
            foreach (var item in code)
            {
                for (int i = 0; i < item.Length; i += 2)
                {
                    var bb = Convert.ToByte(item.Substring(i, 2), 16);
                    data.Add(bb);
                }
            }
            int shift = 0;
            
            int memSize = _memSize * wordSize;
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
                    if (data.Count <= (i + j))
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
    }
}
