namespace IronElisp
{
    class Alloc
    {
        public static Interval make_interval()
        {
            Interval r = new Interval();
            // intervals_consed++;
            return r;
        }

        public static LispObject make_string(string data)
        {
            // TODO: multi-byte stuff?
            return new LispString(data);
        }

        public static LispObject make_pure_string(string data)
        {
            return new LispString(string.Intern(data));
        }
    }

    partial class F
    {
        public static LispSymbol make_symbol(LispObject name)
        {
            LispSymbol p = new LispSymbol(name as LispString);
            // symbols_consed++;
            return p;
        }
    }
}