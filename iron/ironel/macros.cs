namespace IronElisp
{
    public partial class V
    {
        public static LispObject executing_kbd_macro
        {
            get { return Defs.O[(int)Objects.executing_kbd_macro]; }
            set { Defs.O[(int)Objects.executing_kbd_macro] = value; }
        }
    }
}
