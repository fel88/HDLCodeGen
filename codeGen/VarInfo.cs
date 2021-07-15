
namespace codeGen
{
    public class VarInfo
    {
        public string Name;
        public VarType Type;
        public int Address;
        public bool IsArray;
        public int ArraySize;
        public override string ToString()
        {
            return $"var: {Name}";
        }
    }
}
