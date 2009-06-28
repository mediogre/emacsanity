namespace IronElisp
{
    public partial class V
    {
        public static LispObject gc_cons_percentage
        {
            get {return Defs.O[ (int) Objects.gc_cons_percentage]; }
            set {Defs.O[ (int) Objects.gc_cons_percentage] = value; }
        }

        public static LispObject purify_flag
        {
            get {return Defs.O[ (int) Objects.purify_flag]; }
            set {Defs.O[ (int) Objects.purify_flag] = value; }
        }
        	
        public static LispObject post_gc_hook
        {
            get {return Defs.O[ (int) Objects.post_gc_hook]; }
            set {Defs.O[ (int) Objects.post_gc_hook] = value; }
        }
        	
        public static LispObject memory_signal_data
        {
            get {return Defs.O[ (int) Objects.memory_signal_data]; }
            set {Defs.O[ (int) Objects.memory_signal_data] = value; }
        }
	
        public static LispObject memory_full
        {
            get {return Defs.O[ (int) Objects.memory_full]; }
            set {Defs.O[ (int) Objects.memory_full] = value; }
        }
        	
        public static LispObject gc_elapsed
        {
            get {return Defs.O[ (int) Objects.gc_elapsed]; }
            set {Defs.O[ (int) Objects.gc_elapsed] = value; }
        }        
    }
    
    public partial class L
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

    public partial class F
    {
        public static LispSymbol make_symbol(LispObject name)
        {
            LispSymbol p = new LispSymbol(name as LispString);
            // symbols_consed ++;
            return p;
        }
    }

    public partial class L
    {
        public static LispInt make_number(int x)
        {
            return new LispInt(x);
        }
    }

    public partial class F
    {
        public static LispCons cons(LispObject car, LispObject cdr)
        {
            LispCons r = new LispCons(car, cdr);
            // cons_cells_consed ++;
            return r;
        }
    }

    /* Make a list of 1, 2, 3, 4 or 5 specified objects.  */
    public partial class L
    {
        public static LispObject list1(LispObject arg1)
        {
            return F.cons(arg1, Q.nil);
        }
        public static LispObject list2(LispObject arg1, LispObject arg2)
        {
            return F.cons(arg1, F.cons(arg2, Q.nil));
        }

        public static LispObject list3(LispObject arg1, LispObject arg2, LispObject arg3)
        {
            return F.cons(arg1, F.cons(arg2, F.cons(arg3, Q.nil)));
        }

        public static LispObject list4(LispObject arg1, LispObject arg2, LispObject arg3, LispObject arg4)
        {
            return F.cons(arg1, F.cons(arg2, F.cons(arg3, F.cons(arg4, Q.nil))));
        }

        public static LispObject list5(LispObject arg1, LispObject arg2, LispObject arg3, LispObject arg4, LispObject arg5)
        {
            return F.cons(arg1, F.cons(arg2, F.cons(arg3, F.cons(arg4,
                                         F.cons(arg5, Q.nil)))));
        }
    }

    public partial class F
    {
        public static LispObject list(int nargs, LispObject[] args)
        {
            LispObject val = Q.nil;

            while (nargs > 0)
            {
                nargs--;
                val = F.cons(args[nargs], val);
            }
            return val;
        }
    }
}