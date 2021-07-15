using System;
using System.Collections.Generic;
using System.Threading;

namespace codeGen
{
    public class CpuState
    {
        public uint codePointer = 0;
        public uint OutpA;
        public uint OutpB;
        public uint OutpC;
        public uint OutpD;
        //public uint vaddr;//vregAddr -?
        public uint vRegReadAddr;
        public uint vRegReadCount;
        public string[] Code;
        public bool z;
        public bool s;
        public Func<int, byte> SdReader;
        public Action<int, uint> OutPortChanged;

        public void Reset()
        {
            Profiler.Clear();
            InstrCounter = 0;
            ClockCounter = 0;
            codePointer = 0;
            vRegReadAddr = 0;
            ram1 = new uint[1024];
            currentBus = 0;
            paused = false;
            OutpA = 0;
            z = false;
            reg1 = 0;
            reg2 = 0;
        }
        public uint reg1;
        public uint reg1f;
        public uint reg3;
        public uint accum;
        public uint vaddres;
        public uint reg2;
        public uint[] ram1 = new uint[1024];//ram1:varram
        public byte[] ram2 = new byte[512];//ram0:ram1p
        public UInt16[] sdram = new UInt16[1024 * 1024 * 4];//

        public Action<uint, uint> ExternalWrite;

        int currentBus = 0;
        public bool paused = false;
        public uint prevCodePointer;
        public List<uint> cpHistory = new List<uint>();

        public Semaphore fifosem = new Semaphore(0, 1);
        public Queue<uint> fifoq = new Queue<uint>();
        public int ClockCounter = 0;
        public int InstrCounter = 0;
        public bool ProfilerEnabled = false;
        public Dictionary<string, int> Profiler = new Dictionary<string, int>();
        public void Step()
        {
            InstrCounter++;
            prevCodePointer = codePointer;
            cpHistory.Add(codePointer);
            while (cpHistory.Count > 20)
            {
                cpHistory.RemoveAt(0);
            }
            
            if (codePointer >= Code.Length)
            {
                paused = true;
                return;
            }
       
            var code = Convert.ToUInt32("0x" + Code[codePointer], 16);
            if (ProfilerEnabled)
            {
                lock (Profiler)
                {
                    if (!Profiler.ContainsKey(Code[codePointer]))
                    {
                        Profiler.Add(Code[codePointer], 0);
                    }
                    Profiler[Code[codePointer]]++;
                }
            }
            var op1 = (code & 0xff00) >> 8;
            var oplow = (code & 0xff);
            
            bool processed = false;

            ClockCounter += 4;
            switch (op1)
            {
                case 0x80:
                    switch (oplow)
                    {
                        case 0x0A:
                            OutpA = reg1;
                            OutPortChanged?.Invoke(0, OutpA);
                            processed = true;

                            break;
                        case 0x0B:
                            OutpB = reg1;
                            OutPortChanged?.Invoke(1, OutpB);
                            processed = true;

                            break;
                        case 0x0C:
                            OutpC = reg1;
                            OutPortChanged?.Invoke(2, OutpC);
                            processed = true;

                            break;
                        case 0x0D:
                            OutpD = reg1;
                            OutPortChanged?.Invoke(3, OutpD);
                            processed = true;

                            break;
                    }
                    break;
                case 0x1C:
                    reg1 = Convert.ToByte(Code[codePointer].Substring(2), 16);
                    processed = true;
                    break;
                case 0xA4:
                    vaddres = reg1;
                    processed = true;
                    break;
                case 0xB0:
                    codePointer = Convert.ToByte(Code[codePointer].Substring(2), 16);
                    processed = true;
                    break;
                case 0xAC:
                    reg1 = reg1 << 8;
                    reg1 |= Convert.ToByte(Code[codePointer].Substring(2), 16);
                    processed = true;
                    break;
                case 0xB8:
                    codePointer = reg1;
                    return;
                case 0x90:
                    processed = true;
                    break;
                case 0xE0:
                    vRegReadAddr = reg1;
                    processed = true;
                    break;
                case 0xAD:

                    switch (oplow)
                    {
                        case 0x00:
                            reg1 = reg1 - reg2;
                            z = reg1 == 0;
                            processed = true;
                            break;
                        case 0x01:
                            if (!z && s)
                            {
                                codePointer = reg1;
                                return;
                            }
                            processed = true;
                            break;
                        case 0x02:
                            if (z || s)
                            {
                                codePointer = reg1;
                                return;
                            }
                            processed = true;
                            break;
                        case 0x03:
                            if (!z && !s)
                            {
                                codePointer = reg1;
                                return;
                            }
                            processed = true;
                            break;
                        case 0x04:
                            if (z || !s)
                            {
                                codePointer = reg1;
                                return;
                            }
                            processed = true;
                            break;
                        case 0x99:
                            if (reg1 == reg2)
                            {
                                z = true;
                                s = false;
                            }
                            else
                            {
                                z = false;
                                if (reg1 > reg2)
                                {
                                    s = true;
                                }
                                else
                                {
                                    s = false;
                                }
                            }

                            ClockCounter += 4;
                            processed = true;
                            break;
                    }

                    break;
                case 0xAA:

                    switch (oplow)
                    {
                        case 0x00:
                            reg2 = reg1;
                            processed = true;
                            break;
                        case 0x10:
                            reg1 = reg2;
                            processed = true;
                            break;
                        case 0x20:
                            reg3 = reg1;
                            processed = true;
                            break;
                        case 0x30:
                            reg1 = reg3;
                            processed = true;
                            break;
                    }
                    break;
                case 0xA6:
                    accum = 0;
                    processed = true;
                    break;
                case 0xA7:
                    switch (oplow)
                    {
                        case 0x00:
                            accum += reg1;
                            processed = true;

                            break;
                        case 0x10:
                            accum -= reg1;
                            processed = true;

                            break;
                    }
                    break;
                case 0xD8:
                    switch (oplow)
                    {
                        case 0x01:
                            float t = reg1;
                            processed = true;
                            reg1 = BitConverter.ToUInt32(BitConverter.GetBytes(t), 0);

                            break;
                        case 0x02:
                            {
                                var s1 = BitConverter.ToSingle(BitConverter.GetBytes(reg1), 0);
                                var s2 = BitConverter.ToSingle(BitConverter.GetBytes(reg2), 0);
                                reg1 = BitConverter.ToUInt32(BitConverter.GetBytes(s1 + s2), 0);
                                processed = true;
                            }
                            break;
                        case 0x03:
                            {
                                var s1 = BitConverter.ToSingle(BitConverter.GetBytes(reg1), 0);
                                var s2 = BitConverter.ToSingle(BitConverter.GetBytes(reg2), 0);
                                reg1 = BitConverter.ToUInt32(BitConverter.GetBytes(s1 * s2), 0);
                                processed = true;
                            }
                            break;
                        case 0x04:
                            {
                                var s1 = BitConverter.ToSingle(BitConverter.GetBytes(reg1), 0);
                                var s2 = BitConverter.ToSingle(BitConverter.GetBytes(reg2), 0);
                                reg1 = BitConverter.ToUInt32(BitConverter.GetBytes(s1 / s2), 0);
                                processed = true;
                            }
                            break;
                        case 0x05:

                            var s = BitConverter.ToSingle(BitConverter.GetBytes(reg1), 0);
                            if (s < 0) { this.s = true; }
                            reg1 = (uint)Math.Abs(s);

                            processed = true;

                            break;
                    }
                    break;
                case 0xAE:
                    reg1 = accum;
                    processed = true;
                    break;
                case 0xA9:
                    switch (oplow)
                    {
                        case 0x01:
                            vRegReadCount = Convert.ToByte(Code[codePointer].Substring(2), 16);
                            processed = true;
                            break;
                    }
                    break;
                case 0xA1:
                    switch (oplow)
                    {
                        case 0x01://read sector from sd card
                            {
                                for (int j = 0; j < 512; j++)
                                {
                                    ram2[j] = SdReader((int)vaddres + j);
                                }
                                processed = true;
                            }
                            break;
                        case 0x02://read register
                            {
                                reg1 = 0;
                                for (int j = 0; j < vRegReadCount; j++)
                                {
                                    reg1 <<= 8;
                                    var a1 = j + vRegReadAddr;

                                    reg1 |= ram2[a1 % ram2.Length];
                                }

                                processed = true;
                            }
                            break;
                        case 0x03:
                            //vRegReadAddr = reg1;
                            reg1 = ram1[vRegReadAddr];
                            processed = true;
                            break;
                        case 0x13:
                            vRegReadAddr = reg1;
                            reg1 = ram1[reg1];
                            processed = true;
                            break;
                        case 0x04:
                            
                            ram1[vRegReadAddr] = reg1;
                            

                            processed = true;
                            break;
                        case 0x14:                            
                                ExternalWrite?.Invoke(reg1, vRegReadAddr);                            

                            processed = true;
                            break;
                        case 0x07:
                            sdram[reg3] = (UInt16)reg1;
                            processed = true;
                            break;
                        case 0x08:
                            reg1 = sdram[reg3];
                            processed = true;
                            break;
                        case 0x0A://fifo read
                            if (fifoq.Count == 0)
                            {
                                if (ProfilerEnabled)
                                {
                                    Profiler[Code[codePointer]]--;
                                }
                                InstrCounter--; return;
                            }
                            ClockCounter += 10;
                            reg1 = fifoq.Dequeue();
                            processed = true;
                            break;
                    }
                    break;
                case 0xFC:
                    switch (oplow)
                    {
                        case 0x10:
                            ClockCounter = 0;
                            break;
                        case 0x20:
                            reg1 = (uint)ClockCounter;
                            break;
                    }
                    processed = true;
                    break;
                case 0x3D:
                    switch (oplow)
                    {
                        case 0xD1:
                            {
                                currentBus = 0;
                                processed = true;
                                break;
                            }
                        case 0xD2:
                            {
                                currentBus = 1;
                                processed = true;
                                break;
                            }

                        case 0xE0:
                            {
                                reg1 <<= 1;
                                processed = true;

                            }
                            break;
                        case 0xEB:
                            {
                                reg1 <<= 16;
                                processed = true;

                            }
                            break;
                        case 0xEA:
                            {
                                reg1 >>= 16;
                                processed = true;

                            }
                            break;
                        case 0xE1:
                            {
                                reg1 >>= 1;
                                processed = true;

                            }
                            break;
                        case 0xB0:
                            {
                                reg1 = reg1 | reg2;
                                processed = true;
                            }
                            break;
                        case 0xA0:
                            {
                                reg1 = reg1 & reg2;
                                processed = true;
                            }
                            break;

                        case 0xC0:
                            z = reg1 == 0;
                            processed = true;
                            break;

                    }
                    break;
                case 0xEB:

                    reg1--;
                    processed = true;
                    break;
                case 0xEA:

                    reg1++;
                    processed = true;
                    break;
                case 0xBA:
                    if (z)
                    {
                        codePointer = reg1;
                        return;
                    }
                    processed = true;
                    break;
                case 0xBC:
                    if (!z)
                    {
                        codePointer = reg1;
                        return;
                    }
                    processed = true;
                    break;
                case 0xAB://mul
                    switch (oplow)
                    {
                        case 0x00:
                            reg1 = reg1 * reg2;
                            processed = true;
                            break;
                        default:
                            reg1 = reg1 / reg2;
                            processed = true;
                            break;
                    }

                    break;
            }


            if (!processed)
            {
                throw new Exception("unknown opcode: " + Code[codePointer]);
            }
            codePointer++;
        }
    }
}
