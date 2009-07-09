namespace IronElisp
{
    public partial class L
    {
        /* Nonzero if input is coming from the keyboard */
        public static bool INTERACTIVE()
        {
            return (NILP(V.executing_kbd_macro) && !noninteractive);
        }
    }
}