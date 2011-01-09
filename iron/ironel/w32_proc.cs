namespace IronElisp
{
    public partial class V
    {
        public static LispObject w32_downcase_file_names
        {
            get { return Defs.O[(int)Objects.w32_downcase_file_names]; }
            set { Defs.O[(int)Objects.w32_downcase_file_names] = value; }
        }
    }
}