namespace IronElisp
{
    public class byte_stack
    {
        public int pc;
        public LispObject top, bottom;
        public LispObject byte_string;
        public int byte_string_start;
        public LispObject constants;

        public byte_stack next;
    }

    public partial class L
    {
        public static byte_stack byte_stack_list;
    }

    public partial class F
    {
        public static LispObject byte_code(LispObject bytestr, LispObject vector, LispObject maxdepth)
        {
            // COMEBACK_WHEN_READY!!! (when normal eval is working)
            return Q.nil;
        }
    }

    public partial class Q
    {
        public static LispObject bytecode;
    }
}
