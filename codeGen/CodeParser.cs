using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace codeGen
{
    public class CodeParser
    {        
        public static CodeBlock ParseReadSd(VarInfo[] vars, string[] tkns)
        {
            if (tkns[0] == "readSd")//read sector from sd card
            {
                CodeBlock cbb = new CodeBlock();
                StringBuilder sbb = new StringBuilder();


                int addr = 0;
                if (vars.Any(z => z.Name == tkns[2]))
                {
                    var vr1 = vars.First(z => z.Name == tkns[2]);
                    //read var here
                    //vr1.Address
                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append(WriteReg1g(vr1.Address));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data   


                    sbb.Append("A400" + Environment.NewLine);//copy reg1 to vaddr
                    sbb.Append("A101" + Environment.NewLine);//read sector

                    cbb.Text = sbb.ToString();
                    return cbb;

                }
                if (tkns[2].Contains("0x"))
                {
                    addr = Convert.ToInt32(tkns[2], 16);
                }
                else
                {
                    addr = int.Parse(tkns[2]);
                }
                sbb.Append(WriteReg1g(addr));

                sbb.Append("A400" + Environment.NewLine);//copy reg1 to vaddr
                sbb.Append("A101" + Environment.NewLine);//read sector

                cbb.Text = sbb.ToString();
                return cbb;

            }
            return null;
        }

        public static CodeBlock ParsePortOutput(VarInfo[] vars, string[] tkns)
        {

            CodeBlock cbb = new CodeBlock();
            StringBuilder sbb = new StringBuilder();

            //out port write variable
            if (!(tkns[0] == "out" && tkns.Contains("["))) return null;


            StringBuilder sb2 = new StringBuilder();
            var paddr = int.Parse(tkns[2]);

            var tl = tkns.LastOrDefault(u => vars.Any(uu => uu.Name == u));

            if (tl != null)
            {
                var vr = vars.FirstOrDefault(z => z.Name == tl);
                var vaddr = vr.Address;
                sb2.Append("3DD1" + Environment.NewLine);//switch bus
                sb2.Append(WriteReg1g(vaddr));
                sb2.Append("E000" + Environment.NewLine);//write addr
                sb2.Append("A103" + Environment.NewLine);//read data                        
            }
            else
            {
                int val = 0;
                if (tkns[5].Contains("0x"))
                {
                    val = Convert.ToInt32(tkns[5], 16);
                }
                else
                {
                    val = int.Parse(tkns[5]);
                }
                sb2.Append(WriteReg1g(val));
            }
            string port = "A";
            if (paddr == 1)
            {
                port = "B";
            }
            if (paddr == 2)
            {
                port = "C";
            }
            if (paddr == 3)
            {
                port = "D";
            }
            sb2.Append("800" + port + Environment.NewLine);//write port A ,B,C                       


            cbb.Text = sb2.ToString();


            return cbb;


        }

        public static CodeBlock ParseSdramAssign(VarInfo[] vars, string[] tkns)
        {


            //static sdram assign 
            if (!(tkns.First() == "sdram" && tkns[1] == "[")) return null;


            CodeBlock cbb = new CodeBlock();
            StringBuilder sbb = new StringBuilder();



            var tl = vars.Any(uu => uu.Name == tkns[5]);
            var tlcnt = tkns.Count(u => vars.Any(uu => uu.Name == u));
            //if (tlcnt == 2)
            {
                /*
                                         * todo: vram[var]=int
                                         *       vram[var]=var
                                         */
            }
            //  else
            {

                if (tl)//vram[int]=var
                {

                    var ind1 = tkns.ToList().IndexOf(tkns.First(z => z == "="));
                    var vr = vars.FirstOrDefault(z => z.Name == tkns[5]);
                    var vaddr = vr.Address;
                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append(WriteReg1g(vaddr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data   

                    //
                    sbb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1

                    //
                    if (vars.Any(z => z.Name == tkns[2]))
                    {
                        //read var here
                        var vr2 = vars.LastOrDefault(z => z.Name == tkns[2]);
                        var vaddr2 = vr2.Address;
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vaddr2));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data   
                    }
                    else
                    {


                        var addr = 0;
                        if (tkns[2].Contains("0x"))
                        {
                            addr = Convert.ToInt32(tkns[2], 16);
                        }
                        else
                        {
                            addr = int.Parse(tkns[2]);
                        }
                        sbb.Append(WriteReg1g(addr));

                    }



                    sbb.Append("AA20" + Environment.NewLine);//write addr

                    sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                    sbb.Append("A107" + Environment.NewLine);//write data
                }
                else//vram[int]=int
                {
                    if (vars.Any(z => z.Name == tkns[2]))
                    {
                        //read var here
                        var vr2 = vars.LastOrDefault(z => z.Name == tkns[2]);
                        var vaddr2 = vr2.Address;
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vaddr2));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data   
                    }
                    else
                    {
                        var addr = int.Parse(tkns[2]);
                        sbb.Append(WriteReg1g(addr));
                    }

                    var val = 0;
                    if (tkns[5].Contains("0x"))
                    {
                        val = Convert.ToInt32(tkns[5], 16);
                    }
                    else
                    {
                        val = int.Parse(tkns[5]);
                    }


                    //WriteReg1(addr);

                    sbb.Append("AA20" + Environment.NewLine);//write addr

                    sbb.Append(WriteReg1g(val));
                    sbb.Append("A107" + Environment.NewLine);//write data



                }

            }
            cbb.Text = sbb.ToString();
            return cbb;





        }
        public static string WriteReg1g(int addr)
        {
            StringBuilder sb = new StringBuilder();
            if (addr <= 255)
            {
                sb.Append("1C" + addr.ToString("X2") + Environment.NewLine);
            }
            else
            {
                bool first = true;
                for (int z = 3; z >= 0; z--)
                {
                    var aa = (addr & (0xFF << z * 8)) >> (z * 8);
                    if (aa == 0 && first) continue;

                    sb.Append((first ? "1C" : "AC") + aa.ToString("X2") + Environment.NewLine);
                    first = false;

                }
            }
            return sb.ToString();
        }
        public static CodeBlock ParseSumUp(VarInfo[] vars, string[] tkns)
        {

            if (!(vars.Any(z => z.Name == tkns.First()) && (tkns[1] == "-=" || tkns[1] == "+=")))
                return null;


            CodeBlock cbb = new CodeBlock();
            StringBuilder sbb = new StringBuilder();


            var vr = vars.First(z => z.Name == tkns.First());
            var addr = vr.Address;
            if (vr.Type == VarType.floatT)
            {
                sbb.Append("3DD1" + Environment.NewLine);//switch bus
                sbb.Append(WriteReg1g(addr));
                sbb.Append("E000" + Environment.NewLine);//write addr
                sbb.Append("A103" + Environment.NewLine);//read data

                sbb.Append("AA00" + Environment.NewLine);

                if (vars.Any(z => z.Name == tkns.Last(u => u != ";")))
                {
                    var vr2 = vars.First(z => z.Name == tkns.Last(u => u != ";"));
                    var addr2 = vr2.Address;
                    sbb.Append(WriteReg1g(addr2));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data
                }
                else
                {
                    var val = float.Parse(tkns.Last(z => z != ";").Replace("f", ""), CultureInfo.InvariantCulture);
                    sbb.Append(WriteReg1g(val));
                }

                sbb.Append("D802" + Environment.NewLine);//fpu add
                sbb.Append("AA00" + Environment.NewLine);

                //write back

                sbb.Append(WriteReg1g(addr));
                sbb.Append("E000" + Environment.NewLine);//write addr
                sbb.Append("AA10" + Environment.NewLine);//restore
                sbb.Append("A104" + Environment.NewLine);//write back
            }
            else
            {

                sbb.Append("3DD1" + Environment.NewLine);//switch bus
                sbb.Append(WriteReg1g(addr));
                sbb.Append("E000" + Environment.NewLine);//write addr
                sbb.Append("A103" + Environment.NewLine);//read data
                sbb.Append("A600" + Environment.NewLine);//reset accum
                sbb.Append("A700" + Environment.NewLine);//add reg1 to accum

                if (vars.Any(z => z.Name == tkns.Last(u => u != ";")))
                {
                    var vr2 = vars.First(z => z.Name == tkns.Last(u => u != ";"));
                    var addr2 = vr2.Address;
                    sbb.Append(WriteReg1g(addr2));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data
                }
                else
                {
                    var tkn = tkns.Last(z => z != ";");
                    int val = 0;
                    if (tkn.Contains("0x"))
                    {
                        val = Convert.ToInt32(tkn, 16);
                    }
                    else
                    {
                        val = int.Parse(tkn);
                    }
                    sbb.Append(WriteReg1g(val));
                }

                if (tkns[1] == "+=")
                {
                    sbb.Append("A700" + Environment.NewLine);//add reg1 to accum
                }
                else
                {
                    sbb.Append("A710" + Environment.NewLine);//sub reg1 to accum
                }

                //write back

                sbb.Append(WriteReg1g(addr));
                sbb.Append("E000" + Environment.NewLine);//write addr
                sbb.Append("AE00" + Environment.NewLine);//copy reg1 <-accum
                sbb.Append("A104" + Environment.NewLine);//write back
            }

            cbb.Text = sbb.ToString();
            return cbb;
        }

        public static string WriteReg1g(float addr)
        {
            StringBuilder sb = new StringBuilder();
            var bts = BitConverter.GetBytes(addr);
            for (int z = 3; z >= 0; z--)
            {
                var aa = bts[z];
                sb.Append("AC" + aa.ToString("X2") + Environment.NewLine);

            }
            return sb.ToString();
        }

        public static CodeBlock ParseCall(VarInfo[] vars, string[] tkns)
        {
            if (tkns[0] != "call") return null;//call label with ret

            throw new NotImplementedException();
        }

        public static CodeBlock ParseIncrement(VarInfo[] vars, string[] tkns)
        {
            if (vars.Any(z => z.Name == tkns.First()) && (tkns[1] == "--" || tkns[1] == "++")) //decrement
            {
                StringBuilder sb2 = new StringBuilder();
                var vr = vars.First(z => z.Name == tkns.First());
                var addr = vr.Address;
                //1. read var
                //2. decrement reg1
                //3. write back
                sb2.Append("3DD1" + Environment.NewLine);//switch bus
                sb2.Append(WriteReg1g(addr));
                sb2.Append("E000" + Environment.NewLine);//write addr
                sb2.Append("A103" + Environment.NewLine);//read data
                if (tkns[1] == "--")
                {
                    sb2.Append("EB00" + Environment.NewLine);//decrement
                }
                else if (tkns[1] == "++")
                {
                    sb2.Append("EA00" + Environment.NewLine);//increment
                }

                sb2.Append("A104" + Environment.NewLine);//write back

                CodeBlock cb = new CodeBlock();
                cb.Text = sb2.ToString();
                return cb;
            }
            return null;
        }
        private static string ParseAssign(string[] tkns, List<VarInfo> vars)
        {
            StringBuilder sb = new StringBuilder();
            var vr = vars.First(z => z.Name == tkns.First());
            var addr = vr.Address;
            if (tkns.Any(z => z == "timer"))
            {
                sb.Append("3DD1" + Environment.NewLine);//switch bus
                sb.Append(WriteReg1g(addr));

                sb.Append("E000" + Environment.NewLine);//write addr

                sb.Append("FC10" + Environment.NewLine);//fetch timer

                sb.Append("A104" + Environment.NewLine);//read data   
                return sb.ToString();
            }

            if (tkns.Any(z => z == "ram"))//var=ram[addr]
            {

                if (vars.Any(z => z.Name == tkns[4]))
                {
                    //read var here
                    var vr2 = vars.LastOrDefault(z => z.Name == tkns[4]);
                    var vaddr = vr2.Address;
                    sb.Append("3DD1" + Environment.NewLine);//switch bus
                    sb.Append(WriteReg1g(vaddr));
                    sb.Append("E000" + Environment.NewLine);//write addr
                    sb.Append("A103" + Environment.NewLine);//read data   
                    //sb.Append("A113" + Environment.NewLine);//read data   
                }
                else
                {
                    int raddr = 0;
                    if (tkns[4].Contains("0x"))
                    {
                        raddr = Convert.ToInt32(tkns[4], 16);
                    }
                    else
                    {
                        raddr = int.Parse(tkns[4]);
                    }
                    sb.Append(WriteReg1g(raddr));
                }
                sb.Append("E000" + Environment.NewLine);//set vregaddr to read = reg1
                sb.Append("A901" + Environment.NewLine);// set vRegReadCount
                sb.Append("A102" + Environment.NewLine);// read register
                sb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1

                sb.Append("3DD1" + Environment.NewLine);//switch bus
                sb.Append(WriteReg1g(addr));

                sb.Append("E000" + Environment.NewLine);//write addr
                sb.Append("AA10" + Environment.NewLine);//store: reg1<-reg2                        

                sb.Append("A104" + Environment.NewLine);//write data
            }
            else if (tkns.Any(z => z == "sdram"))//var=sdram[addr]
            {
                if (vars.Any(z => z.Name == tkns[4]))
                {
                    //read var here
                    var vr2 = vars.LastOrDefault(z => z.Name == tkns[4]);
                    var vaddr = vr2.Address;
                    sb.Append("3DD1" + Environment.NewLine);//switch bus
                    sb.Append(WriteReg1g(vaddr));
                    sb.Append("E000" + Environment.NewLine);//write addr
                    sb.Append("A103" + Environment.NewLine);//read data   
                }
                else
                {
                    var raddr = int.Parse(tkns[4]);
                    sb.Append(WriteReg1g(raddr));
                }

                sb.Append("AA20" + Environment.NewLine);//set vregaddr to read = reg1                        

                sb.Append("A108" + Environment.NewLine);// read sdram
                sb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1

                sb.Append("3DD1" + Environment.NewLine);//switch bus
                sb.Append(WriteReg1g(addr));

                sb.Append("E000" + Environment.NewLine);//write addr
                sb.Append("AA10" + Environment.NewLine);//store: reg1<-reg2                        

                sb.Append("A104" + Environment.NewLine);//write data
            }
            else
            {


                if (vars.Any(z => z.Name == tkns[2]))
                {
                    var vr2 = vars.First(z => z.Name == tkns[2]);
                    //read var here

                    var vaddr = vr2.Address;
                    sb.Append("3DD1" + Environment.NewLine);//switch bus
                    sb.Append(WriteReg1g(vaddr));
                    sb.Append("E000" + Environment.NewLine);//write addr
                    sb.Append("A103" + Environment.NewLine);//read data   
                    if (vr.Type == VarType.floatT && vr2.Type == VarType.intT) //cast
                    {
                        sb.Append("D801" + Environment.NewLine);//cast int to float
                    }
                    if (vr.Type == VarType.intT && vr2.Type == VarType.floatT) //cast
                    {
                        sb.Append("D805" + Environment.NewLine);//cast int to float
                    }
                    sb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1
                    sb.Append("3DD1" + Environment.NewLine);//switch bus
                    sb.Append(WriteReg1g(addr));
                    sb.Append("E000" + Environment.NewLine);//write addr
                    sb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                    sb.Append("A104" + Environment.NewLine);//write data

                }
                else
                {
                    sb.Append("3DD1" + Environment.NewLine);//switch bus
                    sb.Append(WriteReg1g(addr));
                    sb.Append("E000" + Environment.NewLine);//write addr

                    if (tkns[2].EndsWith("f") && !tkns[2].StartsWith("0x"))
                    {
                        var val = float.Parse(tkns[2].Replace("f", ""), CultureInfo.InvariantCulture);
                        sb.Append(WriteReg1g(val));
                    }
                    else
                    {
                        if (tkns[2] == "regs.cp")
                        {
                            sb.Append("AD07" + Environment.NewLine);//reg1<-code_pointer
                        }
                        else
                        {
                            int val = 0;
                            if (tkns[2].Contains("0x"))
                            {
                                val = Convert.ToInt32(tkns[2], 16);
                            }
                            else
                            {
                                val = int.Parse(tkns[2]);
                            }
                            sb.Append(WriteReg1g(val));
                        }
                    }
                    sb.Append("A104" + Environment.NewLine);//write data

                }

            }
            return sb.ToString();
        }

        public static CodeBlock[] ParseExression(string[] tkns, int j, List<VarInfo> vars)
        {
            List<CodeBlock> bret = new List<CodeBlock>();

            int stackCnt = 0;
            List<string> accum = new List<string>();
            for (int i = j; i < tkns.Length; i++)
            {
                if (tkns[i] == "{")
                {
                    stackCnt++;
                    continue;
                }
                if (tkns[i] == "}")
                {
                    stackCnt--;
                    if (stackCnt == 0) break;
                    continue;
                }
                accum.Add(tkns[i]);

            }
            List<string> accum2 = new List<string>();
            for (int i = 0; i < accum.Count; i++)
            {
                if (accum[i] == ";")
                {

                    if (vars.Any(z => z.Name == accum2.First()) && accum2[1] == "=") //var assign
                    {
                        CodeBlock cb = new CodeBlock();
                        cb.Text = ParseAssign(accum2.ToArray(), vars);
                        cb.Source = string.Join("", accum2);

                        bret.Add(cb);

                    }
                    else if (accum2.Contains("goto"))
                    {

                        CodeBlock cb = new CodeBlock();
                        var aa = tkns.Last(z => z != ";" && char.IsLetter(z[0]));
                        cb.GotoLabel = aa;
                        cb.Source = string.Join("", accum2);
                        cb.GotoMode = GotoModeEnum.Label;
                        //blocks.Add(cb);
                        bret.Add(cb);
                    }
                    else
                    {
                        var temp = ParseIncrement(vars.ToArray(), accum2.ToArray());
                        if (temp != null)
                        {
                            temp.Source = string.Join("", accum2); ;
                            bret.Add(temp);
                        }
                        temp = CodeParser.ParseReadSd(vars.ToArray(), accum2.ToArray());
                        if (temp != null)
                        {
                            temp.Source = string.Join("", accum2); ;
                            bret.Add(temp);
                        }
                        temp = ParseSumUp(vars.ToArray(), accum2.ToArray());
                        if (temp != null)
                        {
                            temp.Source = string.Join("", accum2); ;
                            bret.Add(temp);
                        }
                        temp = ParseSdramAssign(vars.ToArray(), accum2.ToArray());
                        if (temp != null)
                        {
                            temp.Source = string.Join("", accum2); ;
                            bret.Add(temp);
                        }
                        temp = ParsePortOutput(vars.ToArray(), accum2.ToArray());
                        if (temp != null)
                        {
                            temp.Source = string.Join("", accum2); ;
                            bret.Add(temp);
                        }
                    }
                    accum2.Clear();
                    continue;
                }
                accum2.Add(accum[i]);

            }

            return bret.ToArray();
        }

        public static string[] Tokenize(string input)
        {
            List<string> ret = new List<string>();
            StringBuilder sb = new StringBuilder();
            bool insideString = false;
            List<Predicate<char>[]> pools = new List<Predicate<char>[]>();
            pools.Add(new Predicate<char>[] { char.IsLetterOrDigit, (x) => x == '_' || x == '.' });
            pools.Add(new Predicate<char>[] { (x) => x == '+' || x == '-' || x == '=' || x == '!' || x == '>' || x == '<' || x == '^' || x == '&' || x == '|' || x == '*' || x == '/' });

            pools.Add(new Predicate<char>[] { (x) => x == '<' || x == '>' });
            pools.Add(new Predicate<char>[] { char.IsWhiteSpace });
            for (int i = 0; i < input.Length; i++)
            {
                if (sb.Length == 0)
                {
                    sb.Append(input[i]);
                    continue;
                }
                if (input[i] == '\"')
                {
                    if (insideString)
                    {
                        insideString = false;

                        sb.Append(input[i]);
                        ret.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                    if (sb.Length > 0)
                    {
                        ret.Add(sb.ToString());
                        sb.Clear();
                    }
                    insideString = true;
                }
                if (insideString)
                {
                    sb.Append(input[i]);
                    continue;
                }

                bool good = pools.Any(z => new char[] { sb[sb.Length - 1], input[i] }.All(t => z.Any(y => y(t))));
                if (sb[sb.Length - 1] == '=' && input[i] != '=')
                {
                    good = false;
                }
                if (sb[sb.Length - 1] == '-' && char.IsDigit(input[i]))
                {
                    good = true;
                }
                if (good)
                {

                    sb.Append(input[i]);
                }
                else
                {
                    ret.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(input[i]);
                }
            }
            if (sb.Length > 0)
            {
                ret.Add(sb.ToString());
            }
            ret = ret.Where(z => !z.All(char.IsWhiteSpace)).ToList();
            return ret.ToArray();
        }

        public static string[] GetWriteReg1(int addr, bool alwaysFullAddr = false)
        {
            List<string> ret = new List<string>();
            if (addr < 255 && !alwaysFullAddr)
            {
                ret.Add("1C" + addr.ToString("X2"));

            }
            else
            {
                if (alwaysFullAddr)
                {
                    for (int z = 3; z >= 0; z--)
                    {
                        var aa = (addr & (0xFF << z * 8)) >> (z * 8);
                        ret.Add("AC" + aa.ToString("X2"));
                    }
                }
                else
                {
                    bool first = true;
                    for (int z = 3; z >= 0; z--)
                    {
                        var aa = (addr & (0xFF << z * 8)) >> (z * 8);
                        //ret.Add("AC" + aa.ToString("X2"));
                        if (aa == 0 && first) continue;
                        ret.Add((first ? "1C" : "AC") + aa.ToString("X2"));
                        first = false;
                    }
                }
            }
            return ret.ToArray();
        }
        public static IfBlockData ParseIfExpr(VarInfo[] vars, string[] tkns)
        {
            List<CodeBlock> ret = new List<CodeBlock>();
            IfBlockData data = new IfBlockData();
            data.Exit = new CodeBlock();
            data.Code = new CodeBlock();
            var op = "!=";
            List<string> accum = new List<string>();
            bool ifIns = false;
            for (int j = 0; j < tkns.Length; j++)
            {
                if (tkns[j] == "(")
                {
                    ifIns = true;
                    continue;
                }
                if (tkns[j] == ")") break;
                if (ifIns)
                    accum.Add(tkns[j]);
            }
            var source1 = $"if({string.Join("", accum)})";

            if (accum.Contains("||") || accum.Contains("&&"))
            {
                if (accum.Count(z => z == "||" || z == "&&") > 1)
                {
                    throw new NotImplementedException("too complex if condition not supported");
                }
                List<string> accum2 = new List<string>();
                List<string[]> conds = new List<string[]>();
                for (int i = 0; i < accum.Count; i++)
                {

                    if (accum[i] == "&&" || accum[i] == "||")
                    {
                        conds.Add(accum2.ToArray());
                        accum2.Clear();
                        continue;
                    }
                    accum2.Add(accum[i]);

                }
                if (accum2.Any())
                {
                    conds.Add(accum2.ToArray());
                    accum2.Clear();
                }
                foreach (var cnd in conds)
                {
                    CodeBlock cbb = new CodeBlock();
                    StringBuilder sbb = new StringBuilder();

                    var vr2 = vars.First(z => z.Name == cnd.Last(u => vars.Any(uu => uu.Name == u)));

                    var vaddr2 = vr2.Address;

                    string operand2 = cnd[2];

                    int val = 0;
                    if (operand2.Contains("0x"))
                    {
                        val = Convert.ToInt32(operand2, 16);
                    }
                    else
                    {
                        val = int.Parse(operand2);
                    }


                    sbb.Append(WriteReg1g(val));
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store
                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append(WriteReg1g(vaddr2));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data


                    sbb.Append("AD99" + Environment.NewLine);//compare           
                    //check operand here
                    if (cnd[1] == ">=")
                    {
                        //sbb.Append("AD02" + Environment.NewLine);// JL
                        //cbb.JumpMode = GotoJumpMode.JL;
                    }
                    if (cnd[1] == "<=")
                    {
                        //sbb.Append("AD02" + Environment.NewLine);// JL
                        //cbb.JumpMode = GotoJumpMode.JL;
                    }

                    cbb.Source = $"{source1} ({string.Join("", cnd)})";
                    ret.Add(cbb);
                    cbb.Text = sbb.ToString();
                }
                ret[0].Goto = ret[1];
                if (accum.Contains("&&"))//and
                {
                    switch (conds[0][1])
                    {
                        case ">=":
                            ret[0].JumpMode = GotoJumpMode.JL;
                            ret[0].Goto = data.Exit;
                            break;
                        case ">":
                            ret[0].JumpMode = GotoJumpMode.JLE;
                            ret[0].Goto = data.Exit;
                            break;
                        case "<":
                            ret[0].JumpMode = GotoJumpMode.JGE;
                            ret[0].Goto = data.Exit;
                            break;
                        case "<=":
                            ret[0].JumpMode = GotoJumpMode.JG;
                            ret[0].Goto = data.Exit;
                            break;
                        case "!=":
                            ret[0].JumpMode = GotoJumpMode.JE;
                            ret[0].Goto = data.Exit;
                            break;
                        case "==":
                            ret[0].JumpMode = GotoJumpMode.JNE;
                            ret[0].Goto = data.Exit;
                            break;
                    }
                    switch (conds[1][1])
                    {
                        case ">=":
                            ret[1].JumpMode = GotoJumpMode.JL;
                            ret[1].Goto = data.Exit;
                            break;
                        case ">":
                            ret[1].JumpMode = GotoJumpMode.JLE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "<=":
                            ret[1].JumpMode = GotoJumpMode.JG;
                            ret[1].Goto = data.Exit;
                            break;
                        case "<":
                            ret[1].JumpMode = GotoJumpMode.JGE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "!=":
                            ret[1].JumpMode = GotoJumpMode.JE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "==":
                            ret[1].JumpMode = GotoJumpMode.JNE;
                            ret[1].Goto = data.Exit;
                            break;
                    }


                }
                else
                { // or 
                    switch (conds[0][1])
                    {
                        case ">=":
                            ret[0].JumpMode = GotoJumpMode.JGE;
                            ret[0].Goto = data.Code;
                            break;
                        case ">":
                            ret[0].JumpMode = GotoJumpMode.JG;
                            ret[0].Goto = data.Code;
                            break;
                        case "<=":
                            ret[0].JumpMode = GotoJumpMode.JLE;
                            ret[0].Goto = data.Code;
                            break;
                        case "<":
                            ret[0].JumpMode = GotoJumpMode.JL;
                            ret[0].Goto = data.Code;
                            break;
                        case "!=":
                            ret[0].JumpMode = GotoJumpMode.JE;
                            ret[0].Goto = data.Exit;
                            break;
                        case "==":
                            ret[0].JumpMode = GotoJumpMode.JNE;
                            ret[0].Goto = data.Exit;
                            break;
                    }
                    switch (conds[1][1])
                    {
                        case ">=":
                            ret[1].JumpMode = GotoJumpMode.JL;
                            ret[1].Goto = data.Exit;
                            break;
                        case ">":
                            ret[1].JumpMode = GotoJumpMode.JLE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "<=":
                            ret[1].JumpMode = GotoJumpMode.JG;
                            ret[1].Goto = data.Exit;
                            break;
                        case "<":
                            ret[1].JumpMode = GotoJumpMode.JGE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "!=":
                            ret[1].JumpMode = GotoJumpMode.JE;
                            ret[1].Goto = data.Exit;
                            break;
                        case "==":
                            ret[1].JumpMode = GotoJumpMode.JNE;
                            ret[1].Goto = data.Exit;
                            break;
                    }
                }

            }
            else
            {
                var vr = vars.First(z => z.Name == accum[0]);

                var vaddr = vr.Address;
                if (accum.Contains("["))
                {
                    var offset = accum[accum.IndexOf("[") + 1];
                    var offs = int.Parse(offset);
                    vaddr += offs;
                }
                op = accum.First(z => z == "==" || z == "!=" || z == ">" || z == ">=" || z == "<" || z == "<=");
                //op = tkns[3];

                string operand2 = tkns[4];
                operand2 = tkns[tkns.ToList().IndexOf(op) + 1];
                if (vars.Any(z => z.Name == operand2))
                {
                    var vr2 = vars.First(z => z.Name == operand2);
                    CodeBlock cbb = new CodeBlock();

                    StringBuilder sbb = new StringBuilder();

                    sbb.Append("3DD1" + Environment.NewLine);//switch bus


                    sbb.Append(WriteReg1g(vr2.Address));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store

                    sbb.Append(WriteReg1g(vaddr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data

                    sbb.Append("AD99" + Environment.NewLine);//compare                        
                    cbb.Text = sbb.ToString();
                    cbb.Source = source1;

                    switch (op)
                    {
                        case ">=":
                            cbb.JumpMode = GotoJumpMode.JL;
                            break;
                        case ">":
                            cbb.JumpMode = GotoJumpMode.JLE;
                            break;
                        case "<":
                            cbb.JumpMode = GotoJumpMode.JGE;
                            break;
                        case "<-":
                            cbb.JumpMode = GotoJumpMode.JG;
                            break;
                        case "!=":
                            cbb.JumpMode = GotoJumpMode.JE;
                            break;
                        case "==":
                            cbb.JumpMode = GotoJumpMode.JNE;
                            break;
                    }
                    cbb.Goto = data.Exit;
                    ret.Add(cbb);


                }
                else
                {
                    int val = 0;
                    if (operand2.Contains("0x"))
                    {
                        val = Convert.ToInt32(operand2, 16);
                    }
                    else
                    {
                        val = int.Parse(operand2);
                    }


                    if (val == 0 && op == "!=")
                    {
                        CodeBlock cbb = new CodeBlock();

                        StringBuilder sbb = new StringBuilder();
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vaddr));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data
                        sbb.Append("3DC0" + Environment.NewLine);//check reg1==zero
                        cbb.Text = sbb.ToString();
                        cbb.Source = source1;
                        cbb.JumpMode = GotoJumpMode.JE;
                        cbb.Goto = data.Exit;
                        ret.Add(cbb);

                    }
                    else
                    {
                        CodeBlock cbb = new CodeBlock();

                        StringBuilder sbb = new StringBuilder();

                        sbb.Append("3DD1" + Environment.NewLine);//switch bus

                        sbb.Append(WriteReg1g(val));
                        sbb.Append("AA00" + Environment.NewLine);//reg2 store

                        sbb.Append(WriteReg1g(vaddr));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data
                        sbb.Append("AD99" + Environment.NewLine);//compare                        
                        cbb.Text = sbb.ToString();
                        cbb.Source = source1;

                        switch (op)
                        {
                            case ">=":
                                cbb.JumpMode = GotoJumpMode.JL;
                                break;
                            case ">":
                                cbb.JumpMode = GotoJumpMode.JLE;
                                break;
                            case "<":
                                cbb.JumpMode = GotoJumpMode.JGE;
                                break;
                            case "<=":
                                cbb.JumpMode = GotoJumpMode.JG;
                                break;
                            case "!=":
                                cbb.JumpMode = GotoJumpMode.JE;
                                break;
                            case "==":
                                cbb.JumpMode = GotoJumpMode.JNE;
                                break;
                        }
                        cbb.Goto = data.Exit;
                        ret.Add(cbb);


                    }
                }
            }

            data.Header = ret.ToArray();

            return data;

        }
        public static int GetOffset(VarInfo[] vars)
        {
            int offset = 0;
            foreach (var item in vars)
            {
                if (item.IsArray)
                {
                    if (item.Type == VarType.byteT)//packed
                    {
                        offset += item.ArraySize / 4 + 1;
                    }
                    else
                    {
                        offset += item.ArraySize;
                    }
                    continue;
                }
                offset += 1;
            }
            return offset;
        }

        public string WriteReg1(float addr)
        {
            var bts = BitConverter.GetBytes(addr);
            StringBuilder sb = new StringBuilder();
            for (int z = 3; z >= 0; z--)
            {
                var aa = bts[z];
                sb.Append("AC" + aa.ToString("X2") + Environment.NewLine);
            }
            return sb.ToString();
        }

        public class ParseContext
        {
            public List<VarInfo> Vars = new List<VarInfo>();
            public List<CodeBlock> CodeBlocks = new List<CodeBlock>();
            public List<string> Code = new List<string>();
        }

        public static ParseContext Generate(string[] lines, string fontKeys)
        {
            ParseContext pctx = new ParseContext();

            List<string> ret = new List<string>();
            //Dictionary<string, int> labels = new Dictionary<string, int>();

            List<CodeBlock> blocks = new List<CodeBlock>();
            var vars = new List<VarInfo>();
            //bool ifInside = false;

            List<int> tofix = new List<int>();
            List<string> tofixs = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {

                var line = lines[i];
                var tkns = CodeParser.Tokenize(line);
                if (tkns.Count() == 0) continue;
                if (line.Trim().StartsWith("//")) continue;
                
                if (line.EndsWith(":"))
                {                    
                    LabelBlock cb = new LabelBlock();
                    cb.Label = tkns[0];
                    blocks.Add(cb);
                    continue;
                }
                if (tkns.First() == "goto")
                {
                    CodeBlock cb = new CodeBlock();
                    if (vars.Any(z => z.Name == tkns[1]))
                    {
                        var vr = vars.First(z => z.Name == tkns[1]);
                        StringBuilder sbb = new StringBuilder();
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vr.Address));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data

                        sbb.Append("B800" + Environment.NewLine);


                        cb.Text = sbb.ToString();
                        cb.Source = line;
                    }
                    else
                    {
                        var aa = tkns.Last(z => z != ";" && char.IsLetter(z[0]));
                        cb.GotoLabel = aa;
                        cb.Source = line;
                        cb.GotoMode = GotoModeEnum.Label;
                    }
                    
                    blocks.Add(cb);
                    continue;
                }
                if (tkns.First() == "byte") //byte var init
                {
                    if (tkns.Contains("["))//array
                    {
                        for (int j = 0; j < tkns.Length; j++)
                        {
                            if (tkns[j] == "[")
                            {
                                var i1 = int.Parse(tkns[j + 1]);
                                vars.Add(new VarInfo()
                                {
                                    Name = tkns[1],
                                    Type = VarType.byteT,
                                    Address = GetOffset(vars.ToArray()),
                                    IsArray = true,
                                    ArraySize = i1
                                });
                            }
                        }

                        continue;
                    }
                    vars.Add(new VarInfo()
                    {
                        Name = tkns[1],
                        Type = VarType.byteT,
                        Address = GetOffset(vars.ToArray())
                    });
                    continue;
                }
                if (tkns.First() == "int") //var init
                {
                    if (tkns.Contains("["))//array
                    {
                        for (int j = 0; j < tkns.Length; j++)
                        {
                            if (tkns[j] == "[")
                            {
                                var i1 = int.Parse(tkns[j + 1]);
                                vars.Add(new VarInfo()
                                {
                                    Name = tkns[1],
                                    Type = VarType.intT,
                                    Address = GetOffset(vars.ToArray()),
                                    IsArray = true,
                                    ArraySize = i1
                                });
                            }
                        }

                        continue;
                    }
                    if (vars.Any(z => z.Name == tkns[1]))
                    {
                        throw new Exception("duplicate variable: " + tkns[1]);
                    }
                    vars.Add(new VarInfo()
                    {
                        Name = tkns[1],
                        Type = VarType.intT,
                        Address = GetOffset(vars.ToArray())
                    });
                    continue;
                }
                if (tkns.First() == "float") //var init
                {
                    vars.Add(new VarInfo()
                    {
                        Name = tkns[1],
                        Type = VarType.floatT,
                        Address = GetOffset(vars.ToArray())
                    });
                    continue;
                }
                if (tkns.Length > 2 && tkns[2] == "fifoRead")
                {
                    CodeBlock cbb = new CodeBlock();
                    cbb.Source = line;
                    StringBuilder sbb = new StringBuilder();
                    var vr = vars.First(z => z.Name == tkns.First());
                    var addr = vr.Address;
                    sbb.Append("A10A" + Environment.NewLine);//fifo read
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store

                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append(WriteReg1g(addr));
                    sbb.Append("E000" + Environment.NewLine);//write addr                    
                    sbb.Append("AA10" + Environment.NewLine);//reg2 restore
                    sbb.Append("A104" + Environment.NewLine);//write back

                    cbb.Text = sbb.ToString();
                    blocks.Add(cbb);
                    continue;
                }
                if (vars.Any(z => z.Name == tkns.First()) && (tkns[1] == "^=")) //xor
                {

                    var vr = vars.First(z => z.Name == tkns.First());
                    var addr = vr.Address;


                    ret.Add("3DD1" + Environment.NewLine);//switch bus
                    ret.Add(WriteReg1g(addr));
                    ret.Add("E000" + Environment.NewLine);//write addr
                    ret.Add("A103" + Environment.NewLine);//read data
                    ret.Add("AA00" + Environment.NewLine);//reg2 store



                    if (vars.Any(z => z.Name == tkns.Last(u => u != ";")))
                    {
                        var vr2 = vars.First(z => z.Name == tkns.Last(u => u != ";"));
                        var addr2 = vr2.Address;
                        ret.Add(WriteReg1g(addr2));
                        ret.Add("E000" + Environment.NewLine);//write addr
                        ret.Add("A103" + Environment.NewLine);//read data
                    }
                    else
                    {
                        int val = int.Parse(tkns.Last(z => z != ";"));
                        ret.Add(WriteReg1g(val));
                    }
                    ret.Add("3DE4" + Environment.NewLine);//xor                     
                    ret.Add("AA00" + Environment.NewLine);//reg2 store

                    //write back

                    ret.Add(WriteReg1g(addr));
                    ret.Add("E000" + Environment.NewLine);//write addr
                    ret.Add("AA10" + Environment.NewLine);//reg2 store

                    ret.Add("A104" + Environment.NewLine);//write back

                }
                if (vars.Any(z => z.Name == tkns.First()) && (tkns[1] == "&=" || tkns[1] == "|=")) //and , or
                {
                    CodeBlock cbb = new CodeBlock();
                    StringBuilder sbb = new StringBuilder();

                    var vr = vars.First(z => z.Name == tkns.First());
                    var addr = vr.Address;


                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    sbb.Append(WriteReg1g(addr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store



                    if (vars.Any(z => z.Name == tkns.Last(u => u != ";")))
                    {
                        var vr2 = vars.First(z => z.Name == tkns.Last(u => u != ";"));
                        var addr2 = vr2.Address;
                        sbb.Append(WriteReg1g(addr2));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data
                    }
                    else
                    {
                        var ttt = tkns.Last(z => z != ";");

                        int val = 0;
                        if (ttt.Contains("0x"))
                        {
                            val = Convert.ToInt32(ttt, 16);
                        }
                        else
                        {
                            val = int.Parse(ttt);
                        }

                        sbb.Append(WriteReg1g(val));
                    }
                    if (tkns[1] == "&=")
                    {
                        sbb.Append("3DA0" + Environment.NewLine);//and                     
                    }
                    else
                    {
                        sbb.Append("3DB0" + Environment.NewLine);//or                     

                    }
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store

                    //write back

                    sbb.Append(WriteReg1g(addr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("AA10" + Environment.NewLine);//reg2 store

                    sbb.Append("A104" + Environment.NewLine);//write back

                    cbb.Source = line;
                    cbb.Text = sbb.ToString();
                    blocks.Add(cbb);

                }
                if (vars.Any(z => z.Name == tkns.First()) && (tkns[1] == "*=" || tkns[1] == "/=")) //mul, div
                {

                    CodeBlock cbb = new CodeBlock();
                    StringBuilder sbb = new StringBuilder();

                    var vr = vars.First(z => z.Name == tkns.First());
                    var addr = vr.Address;
                    sbb.Append("3DD1" + Environment.NewLine);//switch bus
                    bool isFloat = false;

                    if (vars.Any(z => z.Name == tkns.Last(u => u != ";")))
                    {
                        var vr2 = vars.First(z => z.Name == tkns.Last(u => u != ";"));
                        var addr2 = vr2.Address;
                        sbb.Append(WriteReg1g(addr2));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data
                        if (vr2.Type == VarType.floatT)
                        {
                            isFloat = true;
                        }
                    }
                    else
                    {
                        var ll = tkns.Last(z => z != ";");
                        if (ll.EndsWith("f"))
                        {
                            isFloat = true;
                            var val = float.Parse(ll.Replace("f", ""), CultureInfo.InvariantCulture);
                            sbb.Append(WriteReg1g(val));
                        }
                        else
                        {
                            //int val = int.Parse(ll);
                            int val = 0;
                            if (ll.Contains("0x"))
                            {
                                val = Convert.ToInt32(ll, 16);
                            }
                            else
                            {
                                val = int.Parse(ll);
                            }
                            sbb.Append(WriteReg1g(val));
                        }

                    }
                    sbb.Append("AA00" + Environment.NewLine);//reg2 store

                    sbb.Append(WriteReg1g(addr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("A103" + Environment.NewLine);//read data




                    if (isFloat)
                    {
                        sbb.Append("D803" + Environment.NewLine);//mul float                    
                    }
                    else
                    {
                        if (tkns[1] == "*=")
                        {
                            sbb.Append("AB00" + Environment.NewLine);//mul int
                        }
                        if (tkns[1] == "/=")
                        {
                            sbb.Append("AB01" + Environment.NewLine);//div int
                        }

                    }

                    sbb.Append("AA00" + Environment.NewLine);//reg2 store

                    //write back

                    sbb.Append(WriteReg1g(addr));
                    sbb.Append("E000" + Environment.NewLine);//write addr
                    sbb.Append("AA10" + Environment.NewLine);//reg2 store

                    sbb.Append("A104" + Environment.NewLine);//write back

                    cbb.Text = sbb.ToString();
                    cbb.Source = line;
                    blocks.Add(cbb);
                }


                

                var temp = ParseIncrement(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                temp = ParseReadSd(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                temp = ParseCall(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                temp = ParseSumUp(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                temp = ParseSdramAssign(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                temp = ParsePortOutput(vars.ToArray(), tkns);
                if (temp != null)
                {
                    temp.Source = line;
                    blocks.Add(temp);
                    ret.Add(temp.Text);
                    continue;
                }
                
                if (vars.Any(z => z.Name == tkns.First()) && (tkns[1] == ">>" || tkns[1] == "<<")) //bit shift
                {
                    StringBuilder sb2 = new StringBuilder();

                    var vr = vars.First(z => z.Name == tkns.First());
                    var addr = vr.Address;
                    //detect count and make loop
                    sb2.Append("3DD1" + Environment.NewLine);//switch bus
                    sb2.Append(WriteReg1g(addr));
                    sb2.Append("E000" + Environment.NewLine);//write addr
                    sb2.Append("A103" + Environment.NewLine);//read data
                    int count = int.Parse(tkns[2]);
                    if (count == 16)
                    {
                        if (tkns[1] == ">>")
                        {
                            sb2.Append("3DEA" + Environment.NewLine);//right shift
                        }
                        else if (tkns[1] == "<<")
                        {
                            sb2.Append("3DEB" + Environment.NewLine);//left shift
                        }
                    }
                    else
                    {
                        for (int j = 0; j < count; j++)
                        {
                            if (tkns[1] == ">>")
                            {
                                sb2.Append("3DE1" + Environment.NewLine);//right shift
                            }
                            else if (tkns[1] == "<<")
                            {
                                sb2.Append("3DE0" + Environment.NewLine);//left shift
                            }
                        }
                    }


                    sb2.Append("A104" + Environment.NewLine);//write back

                    CodeBlock cb = new CodeBlock();
                    cb.Source = line;
                    cb.Text = sb2.ToString();
                    blocks.Add(cb);
                }

                if (tkns.Length > 3 && vars.Any(z => z.Name == tkns.First()) && vars.Any(z => z.Name == tkns[2]) && tkns[3] == "[" && tkns[1] == "=") //var=arr[index] assign 
                {
                    var vr = vars.First(z => z.Name == tkns.First());
                    if (vars.Any(z => z.Name == tkns[4]))
                    {
                        var vr2 = vars.First(z => z.Name == tkns[4]);//index
                        var vr3 = vars.Last(z => z.Name == tkns[2]);
                        if (vr.Type == VarType.byteT) throw new NotImplementedException();
                        StringBuilder sbb = new StringBuilder();

                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vr2.Address));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data                                                                
                        sbb.AppendLine("A600");
                        sbb.AppendLine("A700");//save shift to accum
                        sbb.Append(WriteReg1g(vr3.Address));
                        sbb.AppendLine("A700");

                        sbb.Append("AE00" + Environment.NewLine);//copy reg1 <-accum
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data                                                                
                        sbb.Append("AA00" + Environment.NewLine);//copy reg2 <- reg1
                        sbb.Append(WriteReg1g(vr.Address));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                        sbb.Append("A104" + Environment.NewLine);//write data
                        CodeBlock cbb3 = new CodeBlock();
                        cbb3.Text = sbb.ToString();
                        blocks.Add(cbb3);
                        cbb3.Source = line;
                        continue;
                    }
                    else
                    {

                        int val = 0;
                        if (tkns[4].Contains("0x"))
                        {
                            val = Convert.ToInt32(tkns[4], 16);
                        }
                        else
                        {
                            val = int.Parse(tkns[4]);
                        }
                        var vr3 = vars.Last(z => z.Name == tkns[2]);
                        if (vr.Type == VarType.byteT) throw new NotImplementedException();
                        StringBuilder sbb = new StringBuilder();

                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(val));
                        sbb.AppendLine("A600");
                        sbb.AppendLine("A700");//save shift to accum
                        sbb.Append(WriteReg1g(vr3.Address));
                        sbb.AppendLine("A700");

                        sbb.Append("AE00" + Environment.NewLine);//copy reg1 <-accum
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data                                                                
                        sbb.Append("AA00" + Environment.NewLine);//copy reg2 <- reg1
                        sbb.Append(WriteReg1g(vr.Address));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                        sbb.Append("A104" + Environment.NewLine);//write data
                        CodeBlock cbb3 = new CodeBlock();
                        cbb3.Text = sbb.ToString();
                        blocks.Add(cbb3);
                        cbb3.Source = line;
                        continue;
                    }

                }

                if (vars.Any(z => z.Name == tkns.First()) && tkns[1] == "[" && tkns[4] == "=") //arr[index]=val assign 
                {
                    int offset = 0;
                    //var val = int.Parse(tkns[5]);
                    var vr = vars.First(z => z.Name == tkns.First());
                    if (vr.Type == VarType.byteT) throw new NotImplementedException();
                    StringBuilder sbb = new StringBuilder();


                    if (vars.Any(z => z.Name == tkns[2]))
                    {
                        if (vars.Any(z => z.Name == tkns[5]))
                        {
                            var vr2 = vars.First(z => z.Name == tkns[5]);
                            var vr3 = vars.First(z => z.Name == tkns[2]);
                            //read var here


                            sbb.Append("3DD1" + Environment.NewLine);//switch bus
                            sbb.Append(WriteReg1g(vr3.Address));
                            sbb.Append("E000" + Environment.NewLine);//write addr
                            sbb.Append("A103" + Environment.NewLine);//read data   
                            //sum offset+addr.
                            //"a600"
                            //"a700"
                            sbb.AppendLine("A600");
                            sbb.AppendLine("A700");
                            sbb.Append(WriteReg1g(vr.Address));
                            sbb.AppendLine("A700");
                            sbb.Append("AE00" + Environment.NewLine);//copy reg1 <-accum
                            sbb.Append("AA20" + Environment.NewLine);//copy reg1 <-accum

                            var vaddr = vr2.Address;

                            sbb.Append(WriteReg1g(vaddr));
                            sbb.Append("E000" + Environment.NewLine);//write addr
                            sbb.Append("A103" + Environment.NewLine);//read data   
                            if (vr.Type == VarType.floatT && vr2.Type == VarType.intT) //cast
                            {
                                sbb.Append("D801" + Environment.NewLine);//cast int to float
                            }
                            if (vr.Type == VarType.intT && vr2.Type == VarType.floatT) //cast
                            {
                                sbb.Append("D805" + Environment.NewLine);//cast int to float
                            }
                            sbb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1


                            sbb.Append("AA30" + Environment.NewLine);//copy reg1 <-reg3                            
                            sbb.Append("E000" + Environment.NewLine);//write addr
                            sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                            sbb.Append("A104" + Environment.NewLine);//write data

                        }
                        else
                        {

                        }
                        CodeBlock cbb3 = new CodeBlock();
                        cbb3.Text = sbb.ToString();
                        blocks.Add(cbb3);
                        cbb3.Source = line;
                        continue;
                    }
                    offset = int.Parse(tkns[2]);
                    var addr = vr.Address + offset;
                    if (vars.Any(z => z.Name == tkns[5]))
                    {
                        var vr2 = vars.First(z => z.Name == tkns[5]);
                        //read var here

                        var vaddr = vr2.Address;
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(vaddr));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("A103" + Environment.NewLine);//read data   
                        if (vr.Type == VarType.floatT && vr2.Type == VarType.intT) //cast
                        {
                            sbb.Append("D801" + Environment.NewLine);//cast int to float
                        }
                        if (vr.Type == VarType.intT && vr2.Type == VarType.floatT) //cast
                        {
                            sbb.Append("D805" + Environment.NewLine);//cast int to float
                        }
                        sbb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(addr));
                        sbb.Append("E000" + Environment.NewLine);//write addr
                        sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                        sbb.Append("A104" + Environment.NewLine);//write data

                    }
                    else
                    {
                        sbb.Append("3DD1" + Environment.NewLine);//switch bus
                        sbb.Append(WriteReg1g(addr));
                        sbb.Append("E000" + Environment.NewLine);//write addr

                        if (tkns[5].EndsWith("f"))
                        {
                            var val = float.Parse(tkns[5].Replace("f", ""), CultureInfo.InvariantCulture);
                            sbb.Append(WriteReg1g(val));
                        }
                        else
                        {
                            int val = 0;
                            if (tkns[5].Contains("0x"))
                            {
                                val = Convert.ToInt32(tkns[5], 16);
                            }
                            else
                            {
                                val = int.Parse(tkns[5]);
                            }
                            sbb.Append(WriteReg1g(val));
                        }
                        sbb.Append("A104" + Environment.NewLine);//write data

                    }
                    CodeBlock cbb = new CodeBlock();
                    cbb.Text = sbb.ToString();
                    cbb.Source = line;
                    blocks.Add(cbb);
                }

                if (vars.Any(z => z.Name == tkns.First()) && tkns[1] == "=") //var assign
                {
                    CodeBlock cb = new CodeBlock();
                    cb.Source = line;
                    blocks.Add(cb);
                    cb.Text = ParseAssign(tkns, vars);
                    ret.Add(cb.Text);

                    continue;
                }


                if (tkns.Contains("printf"))
                {
                    bool f = false;

                    var jn = tkns.First(z => z.StartsWith("\""));
                    CodeBlock cbb = new CodeBlock();
                    int addr = 0;
                    for (int j = 1; j < jn.Length - 1; j++)
                    {
                        var ind = fontKeys.IndexOf(jn[j]);
                        cbb.Text += $"vram[{addr}]={ind};" + Environment.NewLine;
                        addr++;
                    }

                    blocks.Add(cbb);

                }
                if (tkns.Contains("prints"))//print sdram
                {
                    bool f = false;

                    var jn = tkns.First(z => z.StartsWith("\""));
                    CodeBlock cbb = new CodeBlock();
                    int addr = 0;
                    if (tkns.Any(z => z == ","))
                    {
                        var fr = tkns.First(z => z == ",");
                        var offset = tkns[tkns.ToList().IndexOf(fr) + 1];

                        if (offset.Contains("0x"))
                        {
                            addr = Convert.ToInt32(offset, 16);
                        }
                        else
                        {
                            addr = int.Parse(offset);
                        }
                    }
                    for (int j = 1; j < jn.Length - 1; j++)
                    {
                        var ind = fontKeys.IndexOf(jn[j]);
                        cbb.Text += $"sdram[{addr}]={ind};" + Environment.NewLine;
                        addr++;
                    }
                    foreach (var zz in cbb.Code)
                    {
                        var tkns1 = Tokenize(zz);
                        var sd = ParseSdramAssign(vars.ToArray(), tkns1);
                        sd.Source = zz;
                        blocks.Add(sd);
                    }
                }
       

                if (tkns.Contains("vram") && tkns.Contains("["))//static vram assign 
                {


                    CodeBlock cbb = new CodeBlock();
                    cbb.Source = line;

                    StringBuilder sbb = new StringBuilder();
                    var tl = vars.Any(uu => uu.Name == tkns[5]);
                    var tlcnt = tkns.Count(u => vars.Any(uu => uu.Name == u));
                    //if (tlcnt == 2)
                    {
                        /*
                                                 * todo: vram[var]=int
                                                 *       vram[var]=var
                                                 */
                    }
                    //  else
                    {

                        if (tl)//vram[int]=var
                        {

                            var ind1 = tkns.ToList().IndexOf(tkns.First(z => z == "="));
                            var vr = vars.FirstOrDefault(z => z.Name == tkns[5]);
                            var vaddr = vr.Address;
                            sbb.Append("3DD1" + Environment.NewLine);//switch bus
                            sbb.Append(WriteReg1g(vaddr));
                            sbb.Append("E000" + Environment.NewLine);//write addr
                            sbb.Append("A103" + Environment.NewLine);//read data   

                            //
                            sbb.Append("AA00" + Environment.NewLine);//store: reg2<-reg1

                            //
                            if (vars.Any(z => z.Name == tkns[2]))
                            {
                                //read var here
                                var vr2 = vars.LastOrDefault(z => z.Name == tkns[2]);
                                var vaddr2 = vr2.Address;
                                sbb.Append("3DD1" + Environment.NewLine);//switch bus
                                sbb.Append(WriteReg1g(vaddr2));
                                sbb.Append("E000" + Environment.NewLine);//write addr
                                sbb.Append("A103" + Environment.NewLine);//read data   
                            }
                            else
                            {
                                var addr = int.Parse(tkns[2]);
                                sbb.Append(WriteReg1g(addr));
                            }

                            //sbb.Append("3DD2" + Environment.NewLine);//switch bus

                            sbb.Append("E000" + Environment.NewLine);//write addr

                            sbb.Append("AA10" + Environment.NewLine);//restore: reg1<-reg2
                            sbb.Append("A114" + Environment.NewLine);//write data



                        }
                        else//vram[int]=int
                        {
                            if (vars.Any(z => z.Name == tkns[2]))
                            {
                                //read var here
                                var vr2 = vars.LastOrDefault(z => z.Name == tkns[2]);
                                var vaddr2 = vr2.Address;
                                sbb.Append("3DD1" + Environment.NewLine);//switch bus
                                sbb.Append(WriteReg1g(vaddr2));
                                sbb.Append("E000" + Environment.NewLine);//write addr
                                sbb.Append("A103" + Environment.NewLine);//read data   
                            }
                            else
                            {
                                var addr = int.Parse(tkns[2]);
                                sbb.Append(WriteReg1g(addr));
                            }

                            var val = 0;
                            if (tkns[5].Contains("0x"))
                            {
                                val = Convert.ToInt32(tkns[5], 16);
                            }
                            else
                            {
                                val = int.Parse(tkns[5]);
                            }

                            //sbb.Append("3DD2" + Environment.NewLine);//switch bus
                            //WriteReg1(addr);

                            sbb.Append("E000" + Environment.NewLine);//write addr
                            sbb.Append(WriteReg1g(val));
                            sbb.Append("A114" + Environment.NewLine);//write data


                        }
                    }
                    blocks.Add(cbb);
                    cbb.Text = sbb.ToString();
                    continue;

                }
                if (tkns.Contains("if"))//if check
                {
                    var data = ParseIfExpr(vars.ToArray(), tkns);

                    

                    blocks.AddRange(data.Header);
                    blocks.Add(data.Code);

                    var top = data.Header.Last();
                    CodeBlock[] first = null;
                    CodeBlock after = new CodeBlock();
                    if (tkns.Contains("{"))
                    {

                        var fr = tkns.First(z => z == "{");
                        int ind1 = 0;
                        for (int j = 0; j < tkns.Length; j++)
                        {
                            if (tkns[j] == "{")
                            {

                                first = ParseExression(tkns, j, vars);
                                //top.Goto = first.Last();
                                //top.GotoMode = GotoModeEnum.BlockAfter;
                                blocks.AddRange(first);
                               

                                ind1 = j;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int j = 0; j < tkns.Length; j++)
                        {
                            if (tkns[j] == ")")
                            {
                                first = ParseExression(tkns, j + 1, vars); ;
                                //top.Goto = first.Last();
                                //top.GotoMode = GotoModeEnum.BlockAfter;
                                blocks.AddRange(first);
                                //richTextBox2.AppendText(first.Text);
                                break;
                            }
                        }
                    }

                    if (tkns.Contains("else"))
                    {
                        first.Last().Goto = after;
                        bool isElse = false;
                        for (int j = 0; j < tkns.Length; j++)
                        {
                            if (tkns[j] == "else")
                            {
                                isElse = true;
                                continue;
                            }
                            if (isElse)
                            {
                                if (tkns[j] == "{")
                                {
                                    var cb = ParseExression(tkns, j, vars);
                                    data.Header.Last().Goto = cb.First();
                                    blocks.AddRange(cb);

                                    break;
                                }
                            }
                        }
                        blocks.Add(after);
                    }
                    blocks.Add(data.Exit);
                }
                if (tkns.Contains("resetTimer"))//reset timer
                {
                    CodeBlock cb = new CodeBlock();
                    StringBuilder sb2 = new StringBuilder();

                    sb2.AppendLine("FC20");
                    cb.Text = sb2.ToString();
                    cb.Source = line;
                    blocks.Add(cb);
                }
            }

            CodeBlock cbb2 = new CodeBlock();
            cbb2.Source = "(NOP)";
            cbb2.Text = "9000" + Environment.NewLine;
            blocks.Add(cbb2);

        
            //bind labels
            for (int i = 0; i < blocks.Count; i++)
            {
                CodeBlock item = blocks[i];
                //if (item.GotoLabel != null && item.Goto == null)
                {
                    switch (item.GotoMode)
                    {
                        case GotoModeEnum.Label:
                            item.Goto = blocks.First(z => z is LabelBlock lb && lb.Label == item.GotoLabel);
                            break;
                        case GotoModeEnum.BlockAfter:
                            item.Goto = blocks[blocks.IndexOf(item.Goto) + 1];
                            break;
                    }
                }
            }

            pctx.CodeBlocks = blocks;
            pctx.Vars = vars;
            pctx.Code = ret;
            return pctx;
        }

    }
    public enum GotoJumpMode
    {
        Jump, JE, JNE, JLE, JGE, JG, JL
    }
    public enum GotoModeEnum
    {
        None, Label, Block, BlockAfter
    }

    public enum VarType
    {
        intT,
        floatT,
        byteT
    }
}
