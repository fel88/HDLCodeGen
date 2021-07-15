using System;
using System.Linq;

namespace codeGen
{
    public class CodeBlock
    {
        public string[] Code
        {
            get
            {
                if (Text == null) return null;
                return Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }
        }
        public string Text;
        public string Source;
        public CodeBlock Goto;
        public string GotoLabel;
        public GotoModeEnum GotoMode = GotoModeEnum.None;
        public GotoJumpMode JumpMode;
    }
}
