namespace IronElisp
{
    public partial class Q
    {
        public static LispObject char_table_extra_slots;
    }

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
        /* Put MARKER back on the free list after using it temporarily.  */
        public static void free_marker (LispObject marker)
        {
            unchain_marker(XMARKER(marker));
        }

        public static Interval make_interval()
        {
            Interval r = new Interval();
            // intervals_consed++;
            return r;
        }

        public static LispObject make_string (byte[] contents, int nbytes)
        {
            LispObject val;
            int nchars = 0;
            int multibyte_nbytes = 0;

            parse_str_as_multibyte (contents, nbytes, ref nchars, ref multibyte_nbytes);
            if (nbytes == nchars || nbytes != multibyte_nbytes)
                /* CONTENTS contains no multibyte sequences or contains an invalid
                   multibyte sequence.  We must make unibyte string.  */
                val = make_unibyte_string (contents, nbytes);
            else
                val = make_multibyte_string (contents, nchars, nbytes);
            return val;
        }

        /* Make an unibyte string from LENGTH bytes at CONTENTS.  */
        public static LispObject make_unibyte_string(byte[] contents, int length)
        {
            return make_unibyte_string(contents, 0, length);
        }

        /* Make an unibyte string from LENGTH bytes at CONTENTS.  */
        public static LispObject make_unibyte_string(byte[] contents, int idx, int length)
        {
            LispObject val = make_uninit_string(length);
            XSTRING(val).bcopy(contents, idx, 0, length);
            STRING_SET_UNIBYTE(ref val);
            return val;
        }

        /* Make a multibyte string from NCHARS characters occupying NBYTES
           bytes at CONTENTS.  */
        public static LispObject make_multibyte_string(byte[] contents, int nchars, int nbytes)
        {
            LispObject val = make_uninit_multibyte_string(nchars, nbytes);
            XSTRING(val).bcopy(contents, nbytes);
            return val;
        }

        public static LispObject make_specified_string(byte[] contents, int nchars, int nbytes, bool multibyte)
        {
            return make_specified_string(contents, 0, nbytes, multibyte);
        }

        /* Make a string from NCHARS characters occupying NBYTES bytes at
           CONTENTS.  The argument MULTIBYTE controls whether to label the
           string as multibyte.  If NCHARS is negative, it counts the number of
           characters by itself.  */
        public static LispObject make_specified_string(byte[] contents, int idx, int nchars, int nbytes, bool multibyte)
        {
            LispObject val;

            if (nchars < 0)
            {
                if (multibyte)
                    nchars = multibyte_chars_in_text (contents, idx, nbytes);
                else
                    nchars = nbytes;
            }
            val = make_uninit_multibyte_string (nchars, nbytes);
            (val as LispString).bcopy(contents, idx, 0, nbytes);

            if (!multibyte)
                STRING_SET_UNIBYTE (ref val);
            return val;
        }

        /* Make a string from the data at STR, treating it as multibyte if the
           data warrants.  */
        public static LispObject build_string(string str)
        {
            return make_string(str);
        }

        /* Return an unibyte Lisp_String set up to hold LENGTH characters
           occupying LENGTH bytes.  */
        public static LispObject make_uninit_string(int length)
        {
            if (length == 0)
                return empty_unibyte_string;
            LispObject val = make_uninit_multibyte_string(length, length);
            STRING_SET_UNIBYTE(ref val);
            return val;
        }

        /* Return a multibyte Lisp_String set up to hold NCHARS characters
           which occupy NBYTES bytes.  */
        public static LispObject make_uninit_multibyte_string(int nchars, int nbytes)
        {
            if (nchars < 0)
            {
                abort();
            }

            if (nbytes == 0)
                return empty_multibyte_string;

            LispObject s = new LispString(nchars, nbytes);
            return s;
        }
        
        public static LispObject make_string(string str)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(str);
            return make_string(data, data.Length);
        }

        public static LispObject make_pure_string(string data)
        {
            return make_string(data);
        }

        static void init_strings()
        {
            empty_unibyte_string = make_pure_string(""); // , 0, 0, 0);
            empty_multibyte_string = make_pure_string(""); //, 0, 0, 1);
        }

        public static void init_alloc_once()
        {
            init_strings();

            byte_stack_list = null;
        }

        public static void init_alloc()
        {
            byte_stack_list = null;
        }

        public static void alloc_buffer_text(Buffer b, int nbytes)
        {
            b.text.beg = new byte[nbytes];
        }
    }

    public partial class F
    {
        public static LispObject vector(int nargs, int from, params LispObject[] args)
        {
            LispObject len, val;
            int index;
            LispVector p;

            len = L.XSETINT(nargs);
            val = F.make_vector(len, Q.nil);
            p = L.XVECTOR(val);
            nargs += from;
            for (index = from; index < nargs; index++)
                p[index] = args[index];
            return val;
        }

        public static LispObject vector(int nargs, params LispObject[] args)
        {
            LispObject len, val;
            int index;
            LispVector p;

            len = L.XSETINT(nargs);
            val = F.make_vector(len, Q.nil);
            p = L.XVECTOR(val);
            for (index = 0; index < nargs; index++)
                p[index] = args[index];
            return val;
        }

        public static LispObject make_marker()
        {
            LispMarker p = new LispMarker();
            p.buffer = null;
            p.bytepos = 0;
            p.charpos = 0;
            p.next = null;
            p.insertion_type = false;
            return p;
        }

        public static LispObject make_list(LispObject length, LispObject init)
        {
            LispObject val;
            int size;

            L.CHECK_NATNUM(length);
            size = L.XINT(length);

            val = Q.nil;
            while (size > 0)
            {
                val = F.cons(init, val);
                --size;

                if (size > 0)
                {
                    val = F.cons(init, val);
                    --size;

                    if (size > 0)
                    {
                        val = F.cons(init, val);
                        --size;

                        if (size > 0)
                        {
                            val = F.cons(init, val);
                            --size;

                            if (size > 0)
                            {
                                val = F.cons(init, val);
                                --size;
                            }
                        }
                    }
                }

                L.QUIT();
            }

            return val;
        }

        public static LispObject make_vector(LispObject length, LispObject init)
        {
            L.CHECK_NATNUM(length);
            int sizei = L.XINT(length);

            LispVector p = new LispVector(sizei);
            for (int index = 0; index < sizei; index++)
                p[index] = init;

            return p;
        }

        public static LispObject make_compiled_vector(LispObject length, LispObject init)
        {
            L.CHECK_NATNUM(length);
            int sizei = L.XINT(length);

            LispCompiled p = new LispCompiled(sizei);
            for (int index = 0; index < sizei; index++)
                p[index] = init;

            return p;
        }

        public static LispObject make_byte_code (int nargs, params LispObject[] args)
        {
            LispObject len, val;
            int index;

            len = L.XSETINT(nargs);
            val = F.make_compiled_vector(len, Q.nil);

            if (L.STRINGP (args[1]) && L.STRING_MULTIBYTE (args[1]))
                /* BYTECODE-STRING must have been produced by Emacs 20.2 or the
                   earlier because they produced a raw 8-bit string for byte-code
                   and now such a byte-code string is loaded as multibyte while
                   raw 8-bit characters converted to multibyte form.  Thus, now we
                   must convert them back to the original unibyte form.  */
                args[1] = F.string_as_unibyte (args[1]);

            LispVector p = L.XVECTOR(val);
            for (index = 0; index < nargs; index++)
            {
                p[index] = args[index];
            }

            return val;
        }

        public static LispObject make_bool_vector(LispObject length, LispObject init)
        {
            L.CHECK_NATNUM (length);
            byte real_init = (byte) (L.NILP(init) ? 0 : -1);
            return new LispBoolVector(L.XINT(length), real_init);
        }

        public static LispObject make_string (LispObject length, LispObject init)
        {
            LispObject val;
            int p, end;
            uint c;
            int nbytes;

            L.CHECK_NATNUM (length);
            L.CHECK_NUMBER (init);

            c = (uint) L.XINT (init);
            if (L.ASCII_CHAR_P (c))
            {
                nbytes = L.XINT (length);
                val = L.make_uninit_string (nbytes);
                p = 0; 
                end = L.SCHARS (val);
                byte[] d = L.SDATA (val); 
                while (p != end)
                    d[p++] = (byte) c;
            }
            else
            {
                byte[] str = new byte[L.MAX_MULTIBYTE_LENGTH];
                int len = L.CHAR_STRING (c, str);

                nbytes = len * L.XINT (length);
                val = L.make_uninit_multibyte_string (L.XINT (length), nbytes);
                p = 0;
                end = nbytes;
                while (p != end)
                {
                    L.XSTRING(val).bcopy(str, p, len);
                    p += len;
                }
            }

            return val;
        }
        
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

        public static LispFloat make_float(double x)
        {
            return new LispFloat(x);
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
        public static LispObject list(int nargs, params LispObject[] args)
        {
            LispObject val = Q.nil;

            while (nargs > 0)
            {
                nargs--;
                val = F.cons(args[nargs], val);
            }
            return val;
        }

        public static LispObject list_starting(int nargs, int start_idx, params LispObject[] args)
        {
            LispObject val = Q.nil;

            nargs += start_idx;
            while (nargs > 0)
            {
                nargs--;
                val = F.cons(args[nargs], val);
            }
            return val;
        }
    }
}