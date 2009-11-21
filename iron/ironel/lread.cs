namespace IronElisp
{
    public class InsaneStream // : System.IO.Stream
    {
        public InsaneStream(System.IO.Stream stream)
        {
            stream_ = stream;
        }

        public int ReadByte()
        {
            if (unget_stack_.Count > 0)
            {
                return unget_stack_.Pop();
            }

            return stream_.ReadByte();
        }

        public long Position
        {
            get
            {
                return stream_.Position + unget_stack_.Count;
            }
        }

        public void UngetByte(byte b)
        {
            unget_stack_.Push(b);
        }

        private System.IO.Stream stream_;
        private System.Collections.Generic.Stack<byte> unget_stack_ = new System.Collections.Generic.Stack<byte>();
    }

    public partial class Q
    {
        public static LispObject backquote, comma, comma_at, comma_dot, function;
        public static LispObject read_char, get_file_char;

        /* Used instead of Qget_file_char while loading *.elc files compiled
           by Emacs 21 or older.  */
        public static LispObject get_emacs_mule_file_char;
    }

    public partial class L
    {
        public static LispObject initial_obarray;

        /* non-zero if inside `load' */
        public static bool load_in_progress
        {
            get { return Defs.B[(int)Bools.load_in_progress]; }
            set { Defs.B[(int)Bools.load_in_progress] = value; }
        }

        /* Nonzero means load should forcibly load all dynamic doc strings.  */
        public static bool load_force_doc_strings
        {
            get { return Defs.B[(int)Bools.load_force_doc_strings]; }
            set { Defs.B[(int)Bools.load_force_doc_strings] = value; }
        }

        /* Nonzero means read should convert strings to unibyte.  */
        public static bool load_convert_to_unibyte
        {
            get { return Defs.B[(int)Bools.load_convert_to_unibyte]; }
            set { Defs.B[(int)Bools.load_convert_to_unibyte] = value; }
        }

        /* Non-zero means load dangerous compiled Lisp files.  */
        public static bool load_dangerous_libraries
        {
            get { return Defs.B[(int)Bools.load_dangerous_libraries]; }
            set { Defs.B[(int)Bools.load_dangerous_libraries] = value; }
        }

        public static int read_buffer_size;
        public static byte[] read_buffer;

        /* When READCHARFUN is Qget_file_char, Qget_emacs_mule_file_char,
           Qlambda, or a cons, we use this to keep an unread character because
           a file stream can't handle multibyte-char unreading.  The value -1
           means that there's no unread character. */
        public static int unread_char;

        public const int OBARRAY_SIZE = 1511;
        public static void init_obarray()
        {
            LispObject oblength = XSETINT (OBARRAY_SIZE);

            Q.nil = F.make_symbol(make_string("nil"));
            
            V.obarray = F.make_vector (oblength, make_number (0));
            initial_obarray = V.obarray; 

            XSYMBOL(Q.nil).Interned = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            XSYMBOL(Q.nil).Constant = true;
            XSYMBOL(Q.nil).Value = Q.nil;
            XSYMBOL(Q.nil).Plist = Q.nil;

            int hash = hash_string (System.Text.Encoding.UTF8.GetBytes("nil"), 3);
            hash %= OBARRAY_SIZE;
            XVECTOR (V.obarray)[hash] = Q.nil; 

            Q.unbound = F.make_symbol(make_string("unbound"));
            XSYMBOL(Q.nil).Function = Q.unbound;
            XSYMBOL(Q.unbound).Value = Q.unbound;
            XSYMBOL(Q.unbound).Function = Q.unbound;

            Q.t = intern("t");
            XSYMBOL(Q.t).Value = Q.t;
            XSYMBOL(Q.t).Constant = true;


            Q.variable_documentation = intern("variable-documentation");

            read_buffer_size = 100 + MAX_MULTIBYTE_LENGTH;
            read_buffer = new byte[read_buffer_size];
        }

         /* oblookup stores the bucket number here, for the sake of Funintern.  */
         public static int oblookup_last_bucket_number;
        /* Return the symbol in OBARRAY whose names matches the string
           of SIZE characters (SIZE_BYTE bytes) at PTR.
           If there is no such symbol in OBARRAY, return nil.

           Also store the bucket number in oblookup_last_bucket_number.  */
        public static LispObject oblookup (LispObject obarray, byte[] ptr, int size, int size_byte)
        {
            int hash;
            int obsize;
            LispObject tail;
            LispObject bucket;

            if (!VECTORP (obarray)
                || (obsize = XVECTOR (obarray).Size) == 0)
            {
                obarray = check_obarray (obarray);
                obsize = XVECTOR (obarray).Size;
            }

            hash = hash_string (ptr, size_byte) % obsize;
            bucket = XVECTOR (obarray)[hash];
            oblookup_last_bucket_number = hash;
            if (EQ (bucket, make_number (0)))
            {
            }
            else if (!SYMBOLP (bucket))
                error ("Bad data in guts of obarray"); /* Like CADR error message */
            else
                for (tail = bucket; ; )
                {
                    if (SBYTES (SYMBOL_NAME (tail)) == size_byte &&
                        SCHARS(SYMBOL_NAME(tail)) == size &&
                        ! XSTRING(SYMBOL_NAME(tail)).bcmp(ptr))
                        return tail;
                    else if (XSYMBOL (tail).next == null)
                        break;

                    tail = XSYMBOL (tail).next; 
                }
            return XSETINT(hash);
        }

        public static int hash_string(byte[] ptr, int len)
        {
            int p = 0;
            int end = len;
            byte c;
            int hash = 0;

            while (p != end)
            {
                c = ptr[p++];
                if (c >= 0140) c -= 40;
                hash = ((hash << 3) + (hash >> 28) + c);
            }
            return (int) (hash & 07777777777);
        }

        /* Get an error if OBARRAY is not an obarray.
           If it is one, return it.  */
        public static LispObject check_obarray(LispObject obarray)
        {
            if (!VECTORP (obarray) || XVECTOR (obarray).Size == 0)
            {
                /* If Vobarray is now invalid, force it to be valid.  */
                if (EQ (V.obarray, obarray))
                    V.obarray = initial_obarray;

                wrong_type_argument (Q.vectorp, obarray);
            }
            return obarray;
        }

        public static LispObject intern(string str)
        {
            return intern(System.Text.Encoding.UTF8.GetBytes(str));
        }

        /* Intern the C string STR: return a symbol with that name,
           interned in the current obarray.  */
        public static LispObject intern(byte[] str)
        {
            int len = str.Length;

            LispObject obarray = V.obarray;
            if (!VECTORP (obarray) || XVECTOR (obarray).Size == 0)
                obarray = check_obarray (obarray);
            LispObject tem = oblookup (obarray, str, len, len);
            if (SYMBOLP (tem))
                return tem;
            return F.intern (make_string (str, len), obarray);
        }

        public static LispSubr defsubr(string name, subr0 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr1 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr2 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr3 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr4 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr5 fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, max_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        public static LispSubr defsubr(string name, subr_many fn, int min_args, int max_args, string int_spec, string doc_string)
        {
            LispObject sym = intern(name);
            LispSubr subr = new LispSubr(name, fn, min_args, int_spec, doc_string);

            XSYMBOL(sym).Function = subr;
            return subr;
        }

        /* Define an "integer variable"; a symbol whose value is forwarded
           to a C variable of type int.  Sample call:
           DEFVAR_INT ("emacs-priority", &emacs_priority, "Documentation");  */
        public static void defvar_int(string namestring, Ints address, string doc)
        {
            LispObject sym, val;
            sym = intern(namestring);
            val = new LispIntFwd(Defs.Instance, (int) address);
            SET_SYMBOL_VALUE(sym, val);

            F.put(sym, Q.variable_documentation, make_string(doc));
        }

        /* Similar but define a variable whose value is t if address contains 1,
           nil if address contains 0.  */
        public static void defvar_bool(string namestring, Bools address, string doc)
        {
            LispObject sym, val;
            sym = intern(namestring);
            val = new LispBoolFwd(Defs.Instance, (int)address);

            SET_SYMBOL_VALUE(sym, val);
            F.put(sym, Q.variable_documentation, make_string(doc));
            V.byte_boolean_vars = F.cons(sym, V.byte_boolean_vars);
        }

        /* Similar but define a variable whose value is the Lisp Object stored
           at address.  Two versions: with and without gc-marking of the C
           variable.  The nopro version is used when that variable will be
           gc-marked for some other reason, since marking the same slot twice
           can cause trouble with strings.  */
        public static void defvar_lisp(string namestring, Indexable<LispObject> t, int o, string doc)
        {
            LispObject sym = intern(namestring);
            LispObject val = new LispObjFwd(t, o);

            SET_SYMBOL_VALUE(sym, val);
            F.put(sym, Q.variable_documentation, make_string(doc));
        }

        public static void defvar_lisp(string namestring, Objects address, string doc)
        {
            defvar_lisp(namestring, Defs.Instance, (int)address, doc);
        }

        public static void syms_of_lread()
        {
            defsubr("intern", F.intern, 1, 2, null,
                    @" Return the canonical symbol whose name is STRING.
If there is none, one is created by this function and returned.
A second optional argument specifies the obarray to use;
it defaults to the value of `obarray'.");
        }
    }

    public partial class V
    {
        public static LispObject current_load_list
        {
            get { return Defs.O[(int)Objects.current_load_list]; }
            set { Defs.O[(int)Objects.current_load_list] = value; }
        }

        public static LispObject standard_input
        {
            get { return Defs.O[(int)Objects.standard_input]; }
            set { Defs.O[(int)Objects.standard_input] = value; }
        }

        public static LispObject read_with_symbol_positions
        {
            get { return Defs.O[(int)Objects.read_with_symbol_positions]; }
            set { Defs.O[(int)Objects.read_with_symbol_positions] = value; }
        }

        public static LispObject read_symbol_positions_list
        {
            get { return Defs.O[(int)Objects.read_symbol_positions_list]; }
            set { Defs.O[(int)Objects.read_symbol_positions_list] = value; }
        }

        public static LispObject load_file_name
        {
            get { return Defs.O[(int)Objects.load_file_name]; }
            set { Defs.O[(int)Objects.load_file_name] = value; }
        }         

        public static LispObject obarray
        {
            get { return Defs.O[(int)Objects.obarray]; }
            set { Defs.O[(int)Objects.obarray] = value; }
        }

        public static LispObject byte_boolean_vars
        {
            get { return Defs.O[(int)Objects.byte_boolean_vars]; }
            set { Defs.O[(int)Objects.byte_boolean_vars] = value; }
        }

        public static LispObject old_style_backquotes
        {
            get { return Defs.O[(int)Objects.old_style_backquotes]; }
            set { Defs.O[(int)Objects.old_style_backquotes] = value; }
        }
    }

    public partial class F
    {
        public static LispObject load(LispObject file, LispObject noerror, LispObject nomessage, LispObject nosuffix, LispObject must_suffix)
        {
            // COMEBACK_WHEN_READY
            /*
            register FILE *stream;
            register int fd = -1;
            int count = SPECPDL_INDEX ();
            Lisp_Object found, efound, hist_file_name;
            // 1 means we printed the ".el is newer" message.  
            int newer = 0;
            // 1 means we are loading a compiled file.  
            int compiled = 0;
            Lisp_Object handler;
            int safe_p = 1;
            char *fmode = "r";
            Lisp_Object tmp[2];
            int version;

            fmode = "rt";

            CHECK_STRING (file);

            // If file name is magic, call the handler.  
            // This shouldn't be necessary any more now that `openp' handles it right.
            // handler = Ffind_file_name_handler (file, Qload);
            // if (!NILP (handler))
            // return call5 (handler, Qload, file, noerror, nomessage, nosuffix); 

            // Do this after the handler to avoid
            //    the need to gcpro noerror, nomessage and nosuffix.
            //    (Below here, we care only whether they are nil or not.)
            //    The presence of this call is the result of a historical accident:
            //    it used to be in every file-operation and when it got removed
            //    everywhere, it accidentally stayed here.  Since then, enough people
            //    supposedly have things like (load "$PROJECT/foo.el") in their .emacs
            //    that it seemed risky to remove.  
            if (! NILP (noerror))
            {
                file = internal_condition_case_1 (Fsubstitute_in_file_name, file,
                                                  Qt, load_error_handler);
                if (NILP (file))
                    return Qnil;
            }
            else
                file = Fsubstitute_in_file_name (file);


            // Avoid weird lossage with null string as arg,
            // since it would try to load a directory as a Lisp file 
            if (SCHARS (file) > 0)
            {
                int size = SBYTES (file);

                found = Qnil;

                if (! NILP (must_suffix))
                {
                    // Don't insist on adding a suffix if FILE already ends with one.  
                    if (size > 3
                        && !strcmp (SDATA (file) + size - 3, ".el"))
                        must_suffix = Qnil;
                    else if (size > 4
                             && !strcmp (SDATA (file) + size - 4, ".elc"))
                        must_suffix = Qnil;
                    // Don't insist on adding a suffix
                    // if the argument includes a directory name.  
                    else if (! NILP (Ffile_name_directory (file)))
                        must_suffix = Qnil;
                }

                fd = openp (Vload_path, file,
                            (!NILP (nosuffix) ? Qnil
                             : !NILP (must_suffix) ? Fget_load_suffixes ()
                             : Fappend (2, (tmp[0] = Fget_load_suffixes (),
                                            tmp[1] = Vload_file_rep_suffixes,
                                            tmp))),
                            &found, Qnil);
            }

            if (fd == -1)
            {
                if (NILP (noerror))
                    xsignal2 (Qfile_error, build_string ("Cannot open load file"), file);
                return Qnil;
            }

            // Tell startup.el whether or not we found the user's init file.
            if (EQ (Qt, Vuser_init_file))
                Vuser_init_file = found;

            // If FD is -2, that means openp found a magic file. 
            if (fd == -2)
            {
                if (NILP (Fequal (found, file)))
                    // If FOUND is a different file name from FILE,
                    //    find its handler even if we have already inhibited
                    //    the `load' operation on FILE.
                    handler = Ffind_file_name_handler (found, Qt);
                else
                    handler = Ffind_file_name_handler (found, Qload);
                if (! NILP (handler))
                    return call5 (handler, Qload, found, noerror, nomessage, Qt);
            }

            //  Check if we're stuck in a recursive load cycle.

            //    2000-09-21: It's not possible to just check for the file loaded
            //    being a member of Vloads_in_progress.  This fails because of the
            //    way the byte compiler currently works; `provide's are not
            //    evaluated, see font-lock.el/jit-lock.el as an example.  This
            //    leads to a certain amount of ``normal'' recursion.

            //    Also, just loading a file recursively is not always an error in
            //    the general case; the second load may do something different. 
            {
                int count = 0;
                Lisp_Object tem;
                for (tem = Vloads_in_progress; CONSP (tem); tem = XCDR (tem))
                    if (!NILP (Fequal (found, XCAR (tem))) && (++count > 3))
                    {
                        if (fd >= 0)
                            emacs_close (fd);
                        signal_error ("Recursive load", Fcons (found, Vloads_in_progress));
                    }
                record_unwind_protect (record_load_unwind, Vloads_in_progress);
                Vloads_in_progress = Fcons (found, Vloads_in_progress);
            }

            // Get the name for load-history.
            hist_file_name = (! NILP (Vpurify_flag)
                              ? Fconcat (2, (tmp[0] = Ffile_name_directory (file),
                                             tmp[1] = Ffile_name_nondirectory (found),
                                             tmp))
                              : found) ;

            version = -1;

            // Check for the presence of old-style quotes and warn about them. 
            specbind (Qold_style_backquotes, Qnil);
            record_unwind_protect (load_warn_old_style_backquotes, file);

            if (!bcmp (SDATA (found) + SBYTES (found) - 4,
                       ".elc", 4)
                || (version = safe_to_load_p (fd)) > 0)
                // Load .elc files directly, but not when they are
                //   remote and have no handler! 
            {
                if (fd != -2)
                {
                    struct stat s1, s2;
                    int result;

                    if (version < 0
                        && ! (version = safe_to_load_p (fd)))
                    {
                        safe_p = 0;
                        if (!load_dangerous_libraries)
                        {
                            if (fd >= 0)
                                emacs_close (fd);
                            error ("File `%s' was not compiled in Emacs",
                                   SDATA (found));
                        }
                        else if (!NILP (nomessage))
                            message_with_string ("File `%s' not compiled in Emacs", found, 1);
                    }

                    compiled = 1;

                    efound = ENCODE_FILE (found);

                    fmode = "rb";

                    stat ((char *)SDATA (efound), &s1);
                    SSET (efound, SBYTES (efound) - 1, 0);
                    result = stat ((char *)SDATA (efound), &s2);
                    SSET (efound, SBYTES (efound) - 1, 'c');

                    if (result >= 0 && (unsigned) s1.st_mtime < (unsigned) s2.st_mtime)
                    {
                        // Make the progress messages mention that source is newer.
                        newer = 1;

                        // If we won't print another message, mention this anyway.
                        if (!NILP (nomessage))
                        {
                            Lisp_Object msg_file;
                            msg_file = Fsubstring (found, make_number (0), make_number (-1));
                            message_with_string ("Source file `%s' newer than byte-compiled file",
                                                 msg_file, 1);
                        }
                    }
                }
            }
            else
            {
                // We are loading a source file (*.el). 
                if (!NILP (Vload_source_file_function))
                {
                    Lisp_Object val;

                    if (fd >= 0)
                        emacs_close (fd);
                    val = call4 (Vload_source_file_function, found, hist_file_name,
                                 NILP (noerror) ? Qnil : Qt,
                                 NILP (nomessage) ? Qnil : Qt);
                    return unbind_to (count, val);
                }
            }

            // #ifdef WINDOWSNT
            emacs_close (fd);
            efound = ENCODE_FILE (found);
            stream = fopen ((char *) SDATA (efound), fmode);
            // #else  / not WINDOWSNT 
            // stream = fdopen (fd, fmode);
            // #endif
            if (stream == 0)
            {
                emacs_close (fd);
                error ("Failure to create stdio stream for %s", SDATA (file));
            }

            if (! NILP (Vpurify_flag))
                Vpreloaded_file_list = Fcons (file, Vpreloaded_file_list);

            if (NILP (nomessage))
            {
                if (!safe_p)
                    message_with_string ("Loading %s (compiled; note unsafe, not compiled in Emacs)...",
                                         file, 1);
                else if (!compiled)
                    message_with_string ("Loading %s (source)...", file, 1);
                else if (newer)
                    message_with_string ("Loading %s (compiled; note, source file is newer)...",
                                         file, 1);
                else // The typical case; compiled file newer than source file. 
                    message_with_string ("Loading %s...", file, 1);
            }

            record_unwind_protect (load_unwind, make_save_value (stream, 0));
            record_unwind_protect (load_descriptor_unwind, load_descriptor_list);
            specbind (Qload_file_name, found);
            specbind (Qinhibit_file_name_operation, Qnil);
            load_descriptor_list
                = Fcons (make_number (fileno (stream)), load_descriptor_list);
            load_in_progress++;
            if (! version || version >= 22)
                readevalloop (Qget_file_char, stream, hist_file_name,
                              Feval, 0, Qnil, Qnil, Qnil, Qnil);
            else
            {
                // We can't handle a file which was compiled with
                // byte-compile-dynamic by older version of Emacs.
                specbind (Qload_force_doc_strings, Qt);
                readevalloop (Qget_emacs_mule_file_char, stream, hist_file_name, Feval,
                              0, Qnil, Qnil, Qnil, Qnil);
            }
            unbind_to (count, Qnil);

            // Run any eval-after-load forms for this file
            if (NILP (Vpurify_flag)
                && (!NILP (Ffboundp (Qdo_after_load_evaluation))))
                call1 (Qdo_after_load_evaluation, hist_file_name) ;

            xfree (saved_doc_string);
            saved_doc_string = 0;
            saved_doc_string_size = 0;

            xfree (prev_saved_doc_string);
            prev_saved_doc_string = 0;
            prev_saved_doc_string_size = 0;

            if (!noninteractive && NILP (nomessage))
            {
                if (!safe_p)
                    message_with_string ("Loading %s (compiled; note unsafe, not compiled in Emacs)...done",
                                         file, 1);
                else if (!compiled)
                    message_with_string ("Loading %s (source)...done", file, 1);
                else if (newer)
                    message_with_string ("Loading %s (compiled; note, source file is newer)...done",
                                         file, 1);
                else // The typical case; compiled file newer than source file.
                    message_with_string ("Loading %s...done", file, 1);
            }

            if (!NILP (Fequal (build_string ("obsolete"),
                               Ffile_name_nondirectory
                               (Fdirectory_file_name (Ffile_name_directory (found))))))
                message_with_string ("Package %s is obsolete", file, 1);

            return Qt;
            */
            return Q.nil; 
        }

        public static LispObject read(LispObject stream)
        {
            if (L.NILP(stream))
                stream = V.standard_input;
            if (L.EQ(stream, Q.t))
                stream = Q.read_char;
            if (L.EQ(stream, Q.read_char))
                return F.read_minibuffer(L.build_string("Lisp expression: "), Q.nil);

            return L.read_internal_start(stream, Q.nil, Q.nil);
        }        

        public static LispObject intern(LispObject str, LispObject obarray)
        {
            LispObject tem, sym;

            if (L.NILP (obarray)) obarray = V.obarray;
            obarray = L.check_obarray (obarray);

            L.CHECK_STRING(str);

            tem = L.oblookup (obarray, L.SDATA (str),
                              L.SCHARS(str),
                              L.SBYTES(str));
            if (!L.INTEGERP (tem))
                return tem;

            sym = F.make_symbol (str);

            if (L.EQ (obarray, L.initial_obarray))
                L.XSYMBOL(sym).Interned = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            else
                L.XSYMBOL(sym).Interned = LispSymbol.symbol_interned.SYMBOL_INTERNED;

            if ((L.SREF (str, 0) == ':') && L.EQ (obarray, L.initial_obarray))
            {
                L.XSYMBOL(sym).Constant = true;
                L.XSYMBOL(sym).Value = sym;
            }

            LispObject ptr = L.XVECTOR (obarray)[L.XINT (tem)];
            if (L.SYMBOLP (ptr))
                L.XSYMBOL (sym).next = L.XSYMBOL (ptr);
            else
                L.XSYMBOL (sym).next = null;

            L.XVECTOR (obarray)[L.XINT (tem)] = sym;
            return sym;
        }

        public static LispObject intern_soft (LispObject name, LispObject obarray)
        {
            LispObject tem, str;

            if (L.NILP (obarray)) obarray = V.obarray;
            obarray = L.check_obarray (obarray);

            if (!L.SYMBOLP (name))
            {
                L.CHECK_STRING (name);
                str = name;
            }
            else
                str = L.SYMBOL_NAME (name);

            tem = L.oblookup (obarray, L.SDATA (str), L.SCHARS (str), L.SBYTES (str));
            if (L.INTEGERP (tem) || (L.SYMBOLP (name) && !L.EQ (name, tem)))
                return Q.nil;
            else
                return tem;
        }

        public static LispObject unintern(LispObject name, LispObject obarray)
        {
            LispObject str, tem;
            int hash;

            if (L.NILP (obarray)) obarray = V.obarray;
            obarray = L.check_obarray (obarray);

            if (L.SYMBOLP (name))
                str = L.SYMBOL_NAME (name);
            else
            {
                L.CHECK_STRING (name);
                str = name;
            }

            tem = L.oblookup (obarray, L.SDATA (str),
                            L.SCHARS (str),
                            L.SBYTES (str));
            if (L.INTEGERP (tem))
                return Q.nil;
            /* If arg was a symbol, don't delete anything but that symbol itself.  */
            if (L.SYMBOLP (name) && !L.EQ (name, tem))
                return Q.nil;

            L.XSYMBOL (tem).Interned = LispSymbol.symbol_interned.SYMBOL_UNINTERNED;
            L.XSYMBOL (tem).Constant = false;
            L.XSYMBOL (tem).IsIndirectVariable = false;

            hash = L.oblookup_last_bucket_number;

            if (L.EQ (L.XVECTOR (obarray)[hash], tem))
            {
                if (L.XSYMBOL (tem).next != null)
                    L.XVECTOR (obarray)[hash] = L.XSYMBOL (tem).next;
                else
                    L.XVECTOR (obarray)[hash] = L.make_number (0);
            }
            else
            {
                LispObject tail, following;

                for (tail = L.XVECTOR (obarray)[hash];
                     L.XSYMBOL (tail).next != null;
                     tail = following)
                {
                    following = L.XSYMBOL (tail).next;
                    if (L.EQ (following, tem))
                    {
                        L.XSYMBOL (tail).next = L.XSYMBOL (following).next;
                        break;
                    }
                }
            }

            return Q.t;
        }
    }

    public partial class L
    {
        /* The association list of objects read with the #n=object form.
           Each member of the list has the form (n . object), and is used to
           look up the object for the corresponding #n# construct.
           It must be set to nil before all top-level calls to read0.  */
        public static LispObject read_objects;

        /* Nonzero means READCHAR should read bytes one by one (not character)
           when READCHARFUN is Qget_file_char or Qget_emacs_mule_file_char.
           This is set to 1 by read1 temporarily while handling #@NUMBER.  */
        public static bool load_each_byte;

        /* File for get_file_char to read from.  Use by load.  */
        public static InsaneStream instream;

        /* For use within read-from-string (this reader is non-reentrant!!)  */
        public static int read_from_string_index;
        public static int read_from_string_index_byte;
        public static int read_from_string_limit;

        /* Number of characters read in the current call to Fread or
           Fread_from_string. */
        public static int readchar_count;

        /* This contains the last string skipped with #@.  */
        public static byte[] saved_doc_string;
        /* Length of buffer allocated in saved_doc_string.  */
        public static int saved_doc_string_size;
        /* Length of actual data in saved_doc_string.  */
        public static int saved_doc_string_length;
        /* This is the file position that string came from.  */
        public static long saved_doc_string_position;

        /* This contains the previous string skipped with #@.
           We copy it from saved_doc_string when a new string
           is put in saved_doc_string.  */
        public static byte[] prev_saved_doc_string;
        /* Length of buffer allocated in prev_saved_doc_string.  */
        public static int prev_saved_doc_string_size;
        /* Length of actual data in prev_saved_doc_string.  */
        public static int prev_saved_doc_string_length;
        /* This is the file position that string came from.  */
        public static long prev_saved_doc_string_position;

        /* Nonzero means inside a new-style backquote
           with no surrounding parentheses.
           Fread initializes this to zero, so we need not specbind it
           or worry about what happens to it when there is an error.  */
        public static int new_backquote_flag;

        /* Function to set up the global context we need in toplevel read
           calls. */
        public static LispObject read_internal_start (LispObject stream, LispObject start, LispObject end)
        {
            LispObject retval;

            readchar_count = 0;
            new_backquote_flag = 0;
            read_objects = Q.nil;
            if (EQ (V.read_with_symbol_positions, Q.t)
                || EQ (V.read_with_symbol_positions, stream))
                V.read_symbol_positions_list = Q.nil;

            if (STRINGP (stream)
                || ((CONSP (stream) && STRINGP (XCAR (stream)))))
            {
                int startval, endval;
                LispObject stringg;

                if (STRINGP (stream))
                    stringg = stream;
                else
                    stringg = XCAR (stream);

                if (NILP (end))
                    endval = SCHARS (stringg);
                else
                {
                    CHECK_NUMBER (end);
                    endval = XINT (end);
                    if (endval < 0 || endval > SCHARS (stringg))
                        args_out_of_range (stringg, end);
                }

                if (NILP (start))
                    startval = 0;
                else
                {
                    CHECK_NUMBER (start);
                    startval = XINT (start);
                    if (startval < 0 || startval > endval)
                        args_out_of_range (stringg, start);
                }
                read_from_string_index = startval;
                read_from_string_index_byte = string_char_to_byte (stringg, startval);
                read_from_string_limit = endval;
            }

            retval = read0 (stream);
            if (EQ (V.read_with_symbol_positions, Q.t)
                || EQ (V.read_with_symbol_positions, stream))
                V.read_symbol_positions_list = F.nreverse (V.read_symbol_positions_list);
            return retval;
        }

        /* Signal Qinvalid_read_syntax error. */
        public static void invalid_syntax (string s)
        {
            xsignal1(Q.invalid_read_syntax, make_string(s));
        }

        /* Use this for recursive reads, in contexts where internal tokens
           are not allowed. */
        public static LispObject read0(LispObject readcharfun)
        {
            LispObject val;
            int c;

            val = read1(readcharfun, out c, false);
            if (c == 0)
                return val;

            xsignal1(Q.invalid_read_syntax,
                     F.make_string(make_number(1), make_number(c)));
            return Q.nil;
        }

        /* Signal an `end-of-file' error, if possible with file name
           information.  */
        public static void end_of_file_error()
        {
            if (STRINGP(V.load_file_name))
                xsignal1(Q.end_of_file, V.load_file_name);

            xsignal0(Q.end_of_file);
        }

        /* Read a \-escape sequence, assuming we already read the `\'.
           If the escape sequence forces unibyte, return eight-bit char.  */
        public static int read_escape(LispObject readcharfun, bool stringp)
        {
            int c = readchar(readcharfun);
            /* \u allows up to four hex digits, \U up to eight. Default to the
               behavior for \u, and change this value in the case that \U is seen. */
            int unicode_hex_count = 4;

            switch (c)
            {
                case -1:
                    end_of_file_error();
                    return 0;
                case 'a':
                    return 7;
                case 'b':
                    return '\b';
                case 'd':
                    return 0177;
                case 'e':
                    return 033;
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                case 'v':
                    return '\v';
                case '\n':
                    return -1;
                case ' ':
                    if (stringp)
                        return -1;
                    return ' ';

                case 'M':
                    c = readchar(readcharfun);
                    if (c != '-')
                        error("Invalid escape character syntax");
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    return c | (int) Modifiers.meta_modifier;

                case 'S':
                    c = readchar(readcharfun);
                    if (c != '-')
                        error("Invalid escape character syntax");
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    return c | (int) Modifiers.shift_modifier;

                case 'H':
                    c = readchar(readcharfun);
                    if (c != '-')
                        error("Invalid escape character syntax");
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    return c | (int) Modifiers.hyper_modifier;

                case 'A':
                    c = readchar(readcharfun);
                    if (c != '-')
                        error("Invalid escape character syntax");
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    return c | (int) Modifiers.alt_modifier;

                case 's':
                    c = readchar(readcharfun);
                    if (stringp || c != '-')
                    {
                        unreadchar(readcharfun, c);
                        return ' ';
                    }
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    return c | (int) Modifiers.super_modifier;

                case 'C':
                    c = readchar(readcharfun);
                    if (c != '-')
                        error("Invalid escape character syntax");
                    return 0;
                case '^':
                    c = readchar(readcharfun);
                    if (c == '\\')
                        c = read_escape(readcharfun, false);
                    if ((c & ~CHAR_MODIFIER_MASK) == '?')
                        return (int) (0177 | (c & CHAR_MODIFIER_MASK));
                    else if (!SINGLE_BYTE_CHAR_P((uint) (c & ~CHAR_MODIFIER_MASK)))
                        return c | (int) Modifiers.ctrl_modifier;
                    /* ASCII control chars are made from letters (both cases),
                   as well as the non-letters within 0100...0137.  */
                    else if ((c & 0137) >= 0101 && (c & 0137) <= 0132)
                        return (c & (037 | ~0177));
                    else if ((c & 0177) >= 0100 && (c & 0177) <= 0137)
                        return (c & (037 | ~0177));
                    else
                        return c | (int) Modifiers.ctrl_modifier;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                    /* An octal escape, as in ANSI C.  */
                    {
                        int i = c - '0';
                        int count = 0;
                        while (++count < 3)
                        {
                            if ((c = readchar(readcharfun)) >= '0' && c <= '7')
                            {
                                i *= 8;
                                i += c - '0';
                            }
                            else
                            {
                                unreadchar(readcharfun, c);
                                break;
                            }
                        }

                        if (i >= 0x80 && i < 0x100)
                            i = (int) BYTE8_TO_CHAR((byte) i);
                        return i;
                    }

                case 'x':
                    /* A hex escape, as in ANSI C.  */
                    {
                        int i = 0;
                        int count = 0;
                        while (true)
                        {
                            c = readchar(readcharfun);
                            if (c >= '0' && c <= '9')
                            {
                                i *= 16;
                                i += c - '0';
                            }
                            else if ((c >= 'a' && c <= 'f')
                                 || (c >= 'A' && c <= 'F'))
                            {
                                i *= 16;
                                if (c >= 'a' && c <= 'f')
                                    i += c - 'a' + 10;
                                else
                                    i += c - 'A' + 10;
                            }
                            else
                            {
                                unreadchar(readcharfun, c);
                                break;
                            }
                            count++;
                        }

                        if (count < 3 && i >= 0x80)
                            return (int) BYTE8_TO_CHAR((byte) i);
                        return i;
                    }

                case 'U':
                    /* Post-Unicode-2.0: Up to eight hex chars.  */
                    unicode_hex_count = 8;
                    goto case 'u';
                case 'u':

                    /* A Unicode escape. We only permit them in strings and characters,
                   not arbitrarily in the source code, as in some other languages.  */
                    {
                        int i = 0;
                        int count = 0;

                        while (++count <= unicode_hex_count)
                        {
                            c = readchar(readcharfun);
                            /* isdigit and isalpha may be locale-specific, which we don't
                               want. */
                            if (c >= '0' && c <= '9') i = (i << 4) + (c - '0');
                            else if (c >= 'a' && c <= 'f') i = (i << 4) + (c - 'a') + 10;
                            else if (c >= 'A' && c <= 'F') i = (i << 4) + (c - 'A') + 10;
                            else
                            {
                                error("Non-hex digit used for Unicode escape");
                                break;
                            }
                        }
                        if (i > 0x10FFFF)
                            error("Non-Unicode character: 0x%x", i);
                        return i;
                    }

                default:
                    return c;
            }
        }

        /* Read an integer in radix RADIX using READCHARFUN to read
           characters.  RADIX must be in the interval [2..36]; if it isn't, a
           read error is signaled .  Value is the integer read.  Signals an
           error if encountering invalid read syntax or if RADIX is out of
           range.  */
        public static LispObject read_integer(LispObject readcharfun, int radix)
        {
            int ndigits = 0;
            bool invalid_p;
            int c, sign = 0;
            int number = 0;

            if (radix < 2 || radix > 36)
                invalid_p = true;
            else
            {
                number = ndigits = 0;
                invalid_p = false;
                sign = 1;

                c = readchar(readcharfun);
                if (c == '-')
                {
                    c = readchar(readcharfun);
                    sign = -1;
                }
                else if (c == '+')
                    c = readchar(readcharfun);

                while (c >= 0)
                {
                    int digit;

                    if (c >= '0' && c <= '9')
                        digit = c - '0';
                    else if (c >= 'a' && c <= 'z')
                        digit = c - 'a' + 10;
                    else if (c >= 'A' && c <= 'Z')
                        digit = c - 'A' + 10;
                    else
                    {
                        unreadchar(readcharfun, c);
                        break;
                    }

                    if (digit < 0 || digit >= radix)
                        invalid_p = true;

                    number = radix * number + digit;
                    ++ndigits;
                    c = readchar(readcharfun);
                }
            }

            if (ndigits == 0 || invalid_p)
            {
                string buf = "integer, radix " + radix.ToString();
                invalid_syntax(buf);
            }

            return make_number(sign * number);
        }

        /* If the next token is ')' or ']' or '.', we store that character
           in *PCH and the return value is not interesting.  Else, we store
           zero in *PCH and we read and return one lisp object.

           FIRST_IN_LIST is nonzero if this is the first element of a list.  */
        public static LispObject read1 (LispObject readcharfun, out int pch, bool first_in_list)
        {
            int c;
            bool uninterned_symbol = false;
            bool multibyte;

            pch = 0;
            load_each_byte = true;

            retry:

            c = readchar (readcharfun, out multibyte); 
            if (c < 0)
                end_of_file_error ();

            switch (c)
            {
            case '(':
                return read_list (0, readcharfun);

            case '[':
                return read_vector (readcharfun, false);

            case ')':
            case ']':
                {
                    pch = c;
                    return Q.nil;
                }

            case '#':
                c = readchar(readcharfun);
#if COMEBACK_LATER
                if (c == '^')
                {
                    c = readchar(readcharfun);
                    if (c == '[')
                    {
                        LispObject tmp = read_vector (readcharfun, 0);
                        if (XVECTOR (tmp).Size < CHAR_TABLE_STANDARD_SLOTS)
                            error ("Invalid size char-table");
                        XSETPVECTYPE (XVECTOR (tmp), PVEC_CHAR_TABLE);
                        return tmp;
                    }
                    else if (c == '^')
                    {
                        c = readchar(readcharfun);
                        if (c == '[')
                        {
                            LispObject tmp;
                            int depth, size;

                            tmp = read_vector (readcharfun, 0);
                            if (!INTEGERP (AREF (tmp, 0)))
                                error ("Invalid depth in char-table");
                            depth = XINT (AREF (tmp, 0));
                            if (depth < 1 || depth > 3)
                                error ("Invalid depth in char-table");
                            size = XVECTOR (tmp)->size - 2;
                            if (chartab_size [depth] != size)
                                error ("Invalid size char-table");
                            XSETPVECTYPE (XVECTOR (tmp), PVEC_SUB_CHAR_TABLE);
                            return tmp;
                        }
                        invalid_syntax ("#^^", 3);
                    }
                    invalid_syntax ("#^", 2);
                }

                if (c == '&')
                {
                    LispObject length;
                    length = read1 (readcharfun, out pch, first_in_list);
                    c = readchar(readcharfun);
                    if (c == '"')
                    {
                        LispObject tmp, val;
                        int size_in_chars
                            = ((XINT (length) + BOOL_VECTOR_BITS_PER_CHAR - 1)
                               / BOOL_VECTOR_BITS_PER_CHAR);

                        unreadchar (readcharfun, c);
                        tmp = read1 (readcharfun, out pch, first_in_list);
                        if (STRING_MULTIBYTE (tmp)
                            || (size_in_chars != SCHARS (tmp)
                                /* We used to print 1 char too many
                                   when the number of bits was a multiple of 8.
                                   Accept such input in case it came from an old
                                   version.  */
                                && ! (XINT (length)
                                      == (SCHARS (tmp) - 1) * BOOL_VECTOR_BITS_PER_CHAR)))
                            invalid_syntax ("#&...", 5);

                        val = F.make_bool_vector (length, Q.nil);
                        bcopy (SDATA (tmp), XBOOL_VECTOR (val)->data,
                               size_in_chars);
                        /* Clear the extraneous bits in the last byte.  */
                        if (XINT (length) != size_in_chars * BOOL_VECTOR_BITS_PER_CHAR)
                            XBOOL_VECTOR (val)->data[size_in_chars - 1]
                                &= (1 << (XINT (length) % BOOL_VECTOR_BITS_PER_CHAR)) - 1;
                        return val;
                    }
                    invalid_syntax ("#&...", 5);
                }
#endif
                if (c == '[')
                {
                    /* Accept compiled functions at read-time so that we don't have to
                       build them using function calls.  */
                    LispObject tmp = read_vector (readcharfun, true);
                    return F.make_byte_code (XVECTOR (tmp).Size,
                                             XVECTOR(tmp).Contents);
                }
                if (c == '(')
                {
                    LispObject tmp;
                    int ch;

                    /* Read the string itself.  */
                    tmp = read1 (readcharfun, out ch, false);
                    if (ch != 0 || !STRINGP (tmp))
                        invalid_syntax ("#");
                    /* Read the intervals and their properties.  */
                    while (true)
                    {
                        LispObject beg, end, plist;

                        beg = read1 (readcharfun, out ch, false);
                        end = plist = Q.nil;
                        if (ch == ')')
                            break;
                        if (ch == 0)
                            end = read1 (readcharfun, out ch, false);
                        if (ch == 0)
                            plist = read1 (readcharfun, out ch, false);
                        if (ch != 0)
                            invalid_syntax ("Invalid string property list");
                        F.set_text_properties (beg, end, plist, tmp);
                    }
                    return tmp;
                }

                /* #@NUMBER is used to skip NUMBER following characters.
                   That's used in .elc files to skip over doc strings
                   and function definitions.  */
                if (c == '@')
                {
                    int i, nskip = 0;

                    load_each_byte = true;
                    /* Read a decimal integer.  */
                    while ((c = readchar(readcharfun)) >= 0
                           && c >= '0' && c <= '9')
                    {
                        nskip *= 10;
                        nskip += c - '0';
                    }
                    if (c >= 0)
                        unreadchar(readcharfun, c);

                    if (load_force_doc_strings
                        && (EQ (readcharfun, Q.get_file_char)
                            || EQ (readcharfun, Q.get_emacs_mule_file_char)))
                    {
                        /* If we are supposed to force doc strings into core right now,
                           record the last string that we skipped,
                           and record where in the file it comes from.  */

                        /* But first exchange saved_doc_string
                           with prev_saved_doc_string, so we save two strings.  */
                        {
                            byte[] temp = saved_doc_string;
                            int temp_size = saved_doc_string_size;
                            long temp_pos = saved_doc_string_position;
                            int temp_len = saved_doc_string_length;

                            saved_doc_string = prev_saved_doc_string;
                            saved_doc_string_size = prev_saved_doc_string_size;
                            saved_doc_string_position = prev_saved_doc_string_position;
                            saved_doc_string_length = prev_saved_doc_string_length;

                            prev_saved_doc_string = temp;
                            prev_saved_doc_string_size = temp_size;
                            prev_saved_doc_string_position = temp_pos;
                            prev_saved_doc_string_length = temp_len;
                        }

                        if (saved_doc_string_size == 0)
                        {
                            saved_doc_string_size = nskip + 100;
                            saved_doc_string = new byte[saved_doc_string_size];
                        }
                        if (nskip > saved_doc_string_size)
                        {
                            saved_doc_string_size = nskip + 100;
                            System.Array.Resize(ref saved_doc_string, saved_doc_string_size);
                        }

                        saved_doc_string_position = instream.Position;

                        /* Copy that many characters into saved_doc_string.  */
                        for (i = 0; i < nskip && c >= 0; i++)
                        {
                            c = readchar(readcharfun);
                            saved_doc_string[i] = (byte)c;
                        }

                        saved_doc_string_length = i;
                    }
                    else
                    {
                        /* Skip that many characters.  */
                        for (i = 0; i < nskip && c >= 0; i++)
                            c = readchar(readcharfun);
                    }

                    load_each_byte = false;
                    goto retry;
                }
                if (c == '!')
                {
                    /* #! appears at the beginning of an executable file.
                       Skip the first line.  */
                    while (c != '\n' && c >= 0)
                        c = readchar(readcharfun);
                    goto retry;
                }
                if (c == '$')
                    return V.load_file_name;
                if (c == '\'')
                    return F.cons (Q.function, F.cons (read0 (readcharfun), Q.nil));
                /* #:foo is the uninterned symbol named foo.  */
                if (c == ':')
                {
                    uninterned_symbol = true;
                    c = readchar(readcharfun);
                    goto default_label;
                }
                /* Reader forms that can reuse previously read objects.  */
                if (c >= '0' && c <= '9')
                {
                    int n = 0;
                    LispObject tem;

                    /* Read a non-negative integer.  */
                    while (c >= '0' && c <= '9')
                    {
                        n *= 10;
                        n += c - '0';
                        c = readchar(readcharfun);
                    }
                    /* #n=object returns object, but associates it with n for #n#.  */
                    if (c == '=')
                    {
                        /* Make a placeholder for #n# to use temporarily */
                        LispObject placeholder;
                        LispObject cell;

                        placeholder = F.cons(Q.nil, Q.nil);
                        cell = F.cons (make_number (n), placeholder);
                        read_objects = F.cons (cell, read_objects);

                        /* Read the object itself. */
                        tem = read0 (readcharfun);

                        /* Now put it everywhere the placeholder was... */
                        substitute_object_in_subtree (tem, placeholder);

                        /* ...and #n# will use the real value from now on.  */
                        F.setcdr (cell, tem);

                        return tem;
                    }
                    /* #n# returns a previously read object.  */
                    if (c == '#')
                    {
                        tem = F.assq (make_number (n), read_objects);
                        if (CONSP (tem))
                            return XCDR (tem);
                        /* Fall through to error message.  */
                    }
                    else if (c == 'r' ||  c == 'R')
                        return read_integer (readcharfun, n);

                    /* Fall through to error message.  */
                }
                else if (c == 'x' || c == 'X')
                    return read_integer (readcharfun, 16);
                else if (c == 'o' || c == 'O')
                    return read_integer (readcharfun, 8);
                else if (c == 'b' || c == 'B')
                    return read_integer (readcharfun, 2);

                unreadchar (readcharfun, c);
                invalid_syntax ("#");
                return Q.nil;

            case ';':
                while ((c = readchar(readcharfun)) >= 0 && c != '\n');
                goto retry;

            case '\'':
                {
                    return F.cons (Q.quote, F.cons (read0 (readcharfun), Q.nil));
                }

            case '`':
                if (first_in_list)
                {
                    V.old_style_backquotes = Q.t;
                    goto default_label;
                }
                else
                {
                    LispObject value;

                    new_backquote_flag++;
                    value = read0 (readcharfun);
                    new_backquote_flag--;

                    return F.cons (Q.backquote, F.cons (value, Q.nil));
                }

            case ',':
                if (new_backquote_flag != 0)
                {
                    LispObject comma_type = Q.nil;
                    LispObject value;
                    int ch = readchar (readcharfun);

                    if (ch == '@')
                        comma_type = Q.comma_at;
                    else if (ch == '.')
                        comma_type = Q.comma_dot;
                    else
                    {
                        if (ch >= 0) unreadchar (readcharfun, ch);
                        comma_type = Q.comma;
                    }

                    new_backquote_flag--;
                    value = read0 (readcharfun);
                    new_backquote_flag++;
                    return F.cons (comma_type, F.cons (value, Q.nil));
                }
                else
                {
                    V.old_style_backquotes = Q.t;
                    goto default_label;
                }

            case '?':
                {
                    uint modifiers;
                    int next_char;
                    bool ok;

                    c = readchar(readcharfun);
                    if (c < 0)
                        end_of_file_error ();

                    /* Accept `single space' syntax like (list ? x) where the
                       whitespace character is SPC or TAB.
                       Other literal whitespace like NL, CR, and FF are not accepted,
                       as there are well-established escape sequences for these.  */
                    if (c == ' ' || c == '\t')
                        return make_number (c);

                    if (c == '\\')
                        c = read_escape (readcharfun, false);
                    modifiers = ((uint) c) & CHAR_MODIFIER_MASK;
                    c = (int) (((uint) c) & ~CHAR_MODIFIER_MASK);
                    if (CHAR_BYTE8_P ((uint) c))
                        c = (int) CHAR_TO_BYTE8 ((uint) c);
                    c = (int) ((uint) c | modifiers);

                    next_char = readchar(readcharfun);
                    if (next_char == '.')
                    {
                        /* Only a dotted-pair dot is valid after a char constant.  */
                        int next_next_char = readchar(readcharfun);
                        unreadchar (readcharfun, next_next_char);

                        ok = (next_next_char <= 040
                              || (next_next_char < 0200
                                  && (System.Array.Exists(new byte[] { (byte)'"', (byte)'\'', (byte)';', (byte)'(', (byte)'[', (byte)'#', (byte)'?' }, (x) => x == next_next_char)
                                      || (!first_in_list && next_next_char == '`')
                                      || (new_backquote_flag != 0 && next_next_char == ','))));
                    }
                    else
                    {
                        ok = (next_char <= 040
                              || (next_char < 0200
                              && (System.Array.Exists(new byte[] {(byte)'"', (byte)'\'', (byte)';', (byte)'(', (byte)')', (byte)'[', (byte)']', (byte)'#', (byte)'?'}, (x) => x == next_char)
                                      || (!first_in_list && next_char == '`')
                                      || (new_backquote_flag != 0 && next_char == ','))));
                    }
                    unreadchar (readcharfun, next_char);
                    if (ok)
                        return make_number (c);

                    invalid_syntax ("?");
                }
                return Q.nil;
            case '"':
                {
                    int p = 0;
                    int end = read_buffer_size;
                    // int c;
                    /* Nonzero if we saw an escape sequence specifying
                       a multibyte character.  */
                    bool force_multibyte = false;
                    /* Nonzero if we saw an escape sequence specifying
                       a single-byte character.  */
                    bool force_singlebyte = false;
                    int cancel = 0;
                    int nchars = 0;

                    while ((c = readchar(readcharfun)) >= 0
                           && c != '\"')
                    {
                        if (end - p < MAX_MULTIBYTE_LENGTH)
                        {
                            System.Array.Resize(ref read_buffer, read_buffer_size *= 2);
                            end = read_buffer_size;
                        }

                        if (c == '\\')
                        {
                            uint modifiers;

                            c = read_escape (readcharfun, true);

                            /* C is -1 if \ newline has just been seen */
                            if (c == -1)
                            {
                                if (p == 0)
                                    cancel = 1;
                                continue;
                            }

                            modifiers = ((uint)c) & CHAR_MODIFIER_MASK;
                            c = (int) (((uint)c) & ~CHAR_MODIFIER_MASK);

                            if (CHAR_BYTE8_P ((uint) c))
                                force_singlebyte = true;
                            else if (! ASCII_CHAR_P ((uint) c))
                                force_multibyte = true;
                            else		/* i.e. ASCII_CHAR_P (c) */
                            {
                                /* Allow `\C- ' and `\C-?'.  */
                                if (modifiers == CHAR_CTL)
                                {
                                    if (c == ' ')
                                    {
                                        c = 0;
                                        modifiers = 0;
                                    }
                                    else if (c == '?')
                                    {
                                        c = 127;
                                        modifiers = 0;
                                    }
                                }
                                if ((modifiers & CHAR_SHIFT) != 0)
                                {
                                    /* Shift modifier is valid only with [A-Za-z].  */
                                    if (c >= 'A' && c <= 'Z')
                                        modifiers &= ~CHAR_SHIFT;
                                    else if (c >= 'a' && c <= 'z')
                                    {
                                        c -= ('a' - 'A');
                                        modifiers &= ~CHAR_SHIFT;
                                    }
                                }

                                if ((modifiers & CHAR_META) != 0)
                                {
                                    /* Move the meta bit to the right place for a
                                       string.  */
                                    modifiers &= ~CHAR_META;
                                    c = (int) BYTE8_TO_CHAR ((byte)(c | 0x80));
                                    force_singlebyte = true;
                                }
                            }

                            /* Any modifiers remaining are invalid.  */
                            if (modifiers != 0)
                                error ("Invalid modifier in string");
                            p += CHAR_STRING ((uint) c, read_buffer, p);
                        }
                        else
                        {
                            p += CHAR_STRING ((uint) c, read_buffer, p);
                            if (CHAR_BYTE8_P ((uint) c))
                                force_singlebyte = true;
                            else if (! ASCII_CHAR_P ((uint) c))
                                force_multibyte = true;
                        }
                        nchars++;
                    }

                    if (c < 0)
                        end_of_file_error ();

#if PURITY_SUPPORT
                    /* If purifying, and string starts with \ newline,
                       return zero instead.  This is for doc strings
                       that we are really going to find in etc/DOC.nn.nn  */
                    if (!NILP (Vpurify_flag) && NILP (Vdoc_file_name) && cancel)
                        return make_number (0);
#endif

                    if (force_multibyte)
                    {
                        /* READ_BUFFER already contains valid multibyte forms.  */
                    }
                    else if (force_singlebyte)
                    {
                        nchars = str_as_unibyte (read_buffer, p);
                        p = nchars;
                    }
                    else
                    {
                        /* Otherwise, READ_BUFFER contains only ASCII.  */
                    }

                    /* We want readchar_count to be the number of characters, not
                       bytes.  Hence we adjust for multibyte characters in the
                       string.  ... But it doesn't seem to be necessary, because
                       READCHAR *does* read multibyte characters from buffers. */
                    /* readchar_count -= (p - read_buffer) - nchars; */
                    return make_specified_string (read_buffer, nchars, p,
                                                  (force_multibyte
                                                   || (p != nchars)));
                }

            case '.':
                {
                    int next_char = readchar (readcharfun);
                    unreadchar(readcharfun, next_char);

                    if (next_char <= 040
                        || (next_char < 0200
                            && (System.Array.Exists (new byte [] {(byte)'"', (byte)'\'', (byte)';', (byte)'(', (byte)'[', (byte) '#', (byte)'?'}, (x) => x == next_char)
                                || (!first_in_list && next_char == '`')
                                || (new_backquote_flag != 0 && next_char == ','))))
                    {
                        pch = c;
                        return Q.nil;
                    }

                    /* Otherwise, we fall through!  Note that the atom-reading loop
                       below will now loop at least once, assuring that we will not
                       try to UNREAD two characters in a row.  */
                }
                goto default;
            default:
                default_label:
                if (c <= 040) goto retry;
                if (c == 0x8a0) /* NBSP */
                    goto retry;
                {
                    int p = 0;
                    bool quoted = false;

                    {
                        int end = read_buffer_size;

                        while (c > 040
                               && c != 0x8a0 /* NBSP */
                               && (c >= 0200
                                   || (!System.Array.Exists (new byte[] {(byte)'"', (byte)'\'', (byte) ';', (byte) '(', (byte)')', (byte) '[', (byte) ']', (byte) '#'}, (x) => x == c)
                                       && !(!first_in_list && c == '`')
                                       && !(new_backquote_flag != 0 && c == ','))))
                        {
                            if (end - p < MAX_MULTIBYTE_LENGTH)
                            {
                                System.Array.Resize(ref read_buffer, read_buffer_size *= 2);
                                end = read_buffer_size;
                            }

                            if (c == '\\')
                            {
                                c = readchar (readcharfun);
                                if (c == -1)
                                    end_of_file_error ();
                                quoted = true;
                            }

                            if (multibyte)
                                p += CHAR_STRING ((uint) c, read_buffer, p);
                            else
                                read_buffer[p++] = (byte) c;
                            c = readchar (readcharfun);
                        }

                        if (p == end)
                        {
                            int offset = p;
                            System.Array.Resize(ref read_buffer, read_buffer_size *= 2);
                            p = offset;
                            end = read_buffer_size;
                        }
                        read_buffer[p] = 0;
                        if (c >= 0)
                            unreadchar (readcharfun, c);
                    }

                    if (!quoted && !uninterned_symbol)
                    {
                        LispObject val;
                        int p1 = 0;
                        if (read_buffer[p1] == '+' || read_buffer[p1] == '-') p1++;
                        /* Is it an integer? */
                        if (p1 != p)
                        {
                            while (p1 != p && (c = read_buffer[p1]) >= '0' && c <= '9') p1++;
                            /* Integers can have trailing decimal points.  */
                            if (p1 > 0 && p1 < p && read_buffer[p1] == '.') p1++;
                            if (p1 == p)
                                /* It is an integer. */
                            {
                                if (read_buffer[p1 - 1] == '.')
                                    read_buffer[p1 - 1] = 0;

                                val = XSETINT(System.Int32.Parse(System.Text.Encoding.UTF8.GetString(read_buffer, 0, p1)));
                                return val;
                            }
                        }
                        if (isfloat_string (read_buffer))
                        {
                            /* Compute NaN and infinities using 0.0 in a variable,
                               to cope with compilers that think they are smarter
                               than we are.  */
                            double zero = 0.0;

                            double value;

                            /* Negate the value ourselves.  This treats 0, NaNs,
                               and infinity properly on IEEE floating point hosts,
                               and works around a common bug where atof ("-0.0")
                               drops the sign.  */
                            int negative = read_buffer[0] == '-' ? 1 : 0;

                            /* The only way p[-1] can be 'F' or 'N', after isfloat_string
                               returns 1, is if the input ends in e+INF or e+NaN.  */
                            switch (read_buffer[p - 1])
                            {
                            case (byte) 'F':
                                value = 1.0 / zero;
                                break;
                            case (byte) 'N':
                                value = zero / zero;

                                /* If that made a "negative" NaN, negate it.  */
#if COMEBACK_LATER
                                {
                                    int i;
                                    union { double d; char c[sizeof (double)]; } u_data, u_minus_zero;

                                    u_data.d = value;
                                    u_minus_zero.d = - 0.0;
                                    for (i = 0; i < sizeof (double); i++)
                                        if (u_data.c[i] & u_minus_zero.c[i])
                                        {
                                            value = - value;
                                            break;
                                        }
                                }
#endif
                                /* Now VALUE is a positive NaN.  */
                                break;
                            default:
                                value = System.Double.Parse(System.Text.Encoding.UTF8.GetString(read_buffer, negative, p - negative));
                                break;
                            }

                            return make_float (negative != 0 ? - value : value);
                        }
                    }
                    {
                        LispObject name, result;
                        int nbytes = p;
                        int nchars
                            = (multibyte ? multibyte_chars_in_text (read_buffer, 0, nbytes)
                               : nbytes);

                        name = make_specified_string(read_buffer, nchars, nbytes, multibyte);
                        result = (uninterned_symbol ? F.make_symbol (name)
                                  : F.intern (name, Q.nil));

                        if (EQ (V.read_with_symbol_positions, Q.t)
                            || EQ (V.read_with_symbol_positions, readcharfun))
                            V.read_symbol_positions_list =
                                /* Kind of a hack; this will probably fail if characters
                                   in the symbol name were escaped.  Not really a big
                                   deal, though.  */
                                F.cons (F.cons (result,
                                              make_number (readchar_count
                                                           - XINT (F.length (F.symbol_name (result))))),
                                       V.read_symbol_positions_list);
                        return result;
                    }
                }
            }
        }

        /* List of nodes we've seen during substitute_object_in_subtree. */
        public static LispObject seen_list;

        public static void substitute_object_in_subtree(LispObject obj, LispObject placeholder)
        {
            LispObject check_object;

            /* We haven't seen any objects when we start. */
            seen_list = Q.nil;

            /* Make all the substitutions. */
            check_object = substitute_object_recurse(obj, placeholder, obj);

            /* Clear seen_list because we're done with it. */
            seen_list = Q.nil;

            /* The returned object here is expected to always eq the
               original. */
            if (!EQ(check_object, obj))
                error("Unexpected mutation error in reader");
        }

        public static LispObject substitute_object_recurse(LispObject obj, LispObject placeholder, LispObject subtree)
        {
            throw new System.Exception("comeback");
#if COMEBACK_LATER
  /* If we find the placeholder, return the target object. */
  if (EQ (placeholder, subtree))
    return object;

  /* If we've been to this node before, don't explore it again. */
  if (!EQ (Qnil, Fmemq (subtree, seen_list)))
    return subtree;

  /* If this node can be the entry point to a cycle, remember that
     we've seen it.  It can only be such an entry point if it was made
     by #n=, which means that we can find it as a value in
     read_objects.  */
  if (!EQ (Qnil, Frassq (subtree, read_objects)))
    seen_list = Fcons (subtree, seen_list);

  /* Recurse according to subtree's type.
     Every branch must return a Lisp_Object.  */
  switch (XTYPE (subtree))
    {
    case Lisp_Vectorlike:
      {
	int i, length = 0;
	if (BOOL_VECTOR_P (subtree))
	  return subtree;		/* No sub-objects anyway.  */
	else if (CHAR_TABLE_P (subtree) || SUB_CHAR_TABLE_P (subtree)
		 || COMPILEDP (subtree))
	  length = ASIZE (subtree) & PSEUDOVECTOR_SIZE_MASK;
	else if (VECTORP (subtree))
	  length = ASIZE (subtree);
	else
	  /* An unknown pseudovector may contain non-Lisp fields, so we
	     can't just blindly traverse all its fields.  We used to call
	     `Flength' which signaled `sequencep', so I just preserved this
	     behavior.  */
	  wrong_type_argument (Qsequencep, subtree);

	for (i = 0; i < length; i++)
	  SUBSTITUTE (AREF (subtree, i),
		      ASET (subtree, i, true_value));
	return subtree;
      }

    case Lisp_Cons:
      {
	SUBSTITUTE (XCAR (subtree),
		    XSETCAR (subtree, true_value));
	SUBSTITUTE (XCDR (subtree),
		    XSETCDR (subtree, true_value));
	return subtree;
      }

    case Lisp_String:
      {
	/* Check for text properties in each interval.
	   substitute_in_interval contains part of the logic. */

	INTERVAL    root_interval = STRING_INTERVALS (subtree);
	Lisp_Object arg           = Fcons (object, placeholder);

	traverse_intervals_noorder (root_interval,
				    &substitute_in_interval, arg);

	return subtree;
      }

      /* Other types don't recurse any further. */
    default:
      return subtree;
    }
#endif
        }
        
        public const int LEAD_INT = 1;
        public const int DOT_CHAR = 2;
        public const int TRAIL_INT = 4;
        public const int E_CHAR = 8;
        public const int EXP_INT = 16;

        public static bool isfloat_string(byte[] bytes)
        {
            int state;

            int start = 0;
            int cp = 0;
            state = 0;

            if (bytes[cp] == '+' || bytes[cp] == '-')
                cp++;

            if (bytes[cp] >= '0' && bytes[cp] <= '9')
            {
                state |= LEAD_INT;
                while (bytes[cp] >= '0' && bytes[cp] <= '9')
                    cp++;
            }

            if (bytes[cp] == '.')
            {
                state |= DOT_CHAR;
                cp++;
            }

            if (bytes[cp] >= '0' && bytes[cp] <= '9')
            {
                state |= TRAIL_INT;
                while (bytes[cp] >= '0' && bytes[cp] <= '9')
                    cp++;
            }

            if (bytes[cp] == 'e' || bytes[cp] == 'E')
            {
                state |= E_CHAR;
                cp++;
                if (bytes[cp] == '+' || bytes[cp] == '-')
                    cp++;
            }

            if (bytes[cp] >= '0' && bytes[cp] <= '9')
            {
                state |= EXP_INT;
                while (bytes[cp] >= '0' && bytes[cp] <= '9')
                    cp++;
            }
            else if (cp == start)
            {
            }
            else if (bytes[cp-1] == '+' && bytes[cp] == 'I' && bytes[cp+1] == 'N' && bytes[cp+2] == 'F')
            {
                state |= EXP_INT;
                cp += 3;
            }
            else if (bytes[cp-1] == '+' && bytes[cp] == 'N' && bytes[cp+1] == 'a' && bytes[cp+2] == 'N')
            {
                state |= EXP_INT;
                cp += 3;
            }

            return (((bytes[cp] == 0) || (bytes[cp] == ' ') || (bytes[cp] == '\t') || (bytes[cp] == '\n') || (bytes[cp] == '\r') || (bytes[cp] == '\f'))
      && (state == (LEAD_INT | DOT_CHAR | TRAIL_INT)
          || state == (DOT_CHAR | TRAIL_INT)
          || state == (LEAD_INT | E_CHAR | EXP_INT)
          || state == (LEAD_INT | DOT_CHAR | TRAIL_INT | E_CHAR | EXP_INT)
          || state == (DOT_CHAR | TRAIL_INT | E_CHAR | EXP_INT)));
        }

        public static LispObject read_vector (LispObject readcharfun, bool bytecodeflag)
        {
            int i;
            int size;
            LispObject tem, item, vector;
            LispObject len;

            tem = read_list (1, readcharfun);
            len = F.length (tem);
            vector = F.make_vector (len, Q.nil);

            size = XVECTOR (vector).Size;
            LispVector ptr = XVECTOR (vector);
            for (i = 0; i < size; i++)
            {
                item = F.car (tem);
                /* If `load-force-doc-strings' is t when reading a lazily-loaded
                   bytecode object, the docstring containing the bytecode and
                   constants values must be treated as unibyte and passed to
                   Fread, to get the actual bytecode string and constants vector.  */
                if (bytecodeflag && load_force_doc_strings)
                {
                    if (i == LispCompiled.COMPILED_BYTECODE)
                    {
                        if (!STRINGP (item))
                            error ("Invalid byte code");

                        /* Delay handling the bytecode slot until we know whether
                           it is lazily-loaded (we can tell by whether the
                           constants slot is nil).  */
                        ptr[LispCompiled.COMPILED_CONSTANTS] = item;
                        item = Q.nil;
                    }
                    else if (i == LispCompiled.COMPILED_CONSTANTS)
                    {
                        LispObject bytestr = ptr[LispCompiled.COMPILED_CONSTANTS];

                        if (NILP (item))
                        {
                            /* Coerce string to unibyte (like string-as-unibyte,
                               but without generating extra garbage and
                               guaranteeing no change in the contents).  */
                            STRING_SET_CHARS (bytestr, SBYTES (bytestr));
                            STRING_SET_UNIBYTE (ref bytestr);

                            item = F.read (F.cons (bytestr, readcharfun));
                            if (!CONSP (item))
                                error ("Invalid byte code");

                            bytestr = XCAR (item);
                            item = XCDR (item);
                        }

                        /* Now handle the bytecode slot.  */
                        ptr[LispCompiled.COMPILED_BYTECODE] = bytestr;
                    }
                    else if (i == LispCompiled.COMPILED_DOC_STRING
                             && STRINGP (item)
                             && ! STRING_MULTIBYTE (item))
                    {
                        if (EQ (readcharfun, Q.get_emacs_mule_file_char))
                            item = F.decode_coding_string (item, Q.emacs_mule, Q.nil, Q.nil);
                        else
                            item = F.string_as_multibyte (item);
                    }
                }
                ptr[i] = item;
                tem = F.cdr (tem);
            }
            return vector;
        }
        
        /* FLAG = 1 means check for ] to terminate rather than ) and .
           FLAG = -1 means check for starting with defun
           and make structure pure.  */
        public static LispObject read_list (int flag, LispObject readcharfun)
        {
            /* -1 means check next element for defun,
               0 means don't check,
               1 means already checked and found defun. */
            int defunflag = flag < 0 ? -1 : 0;
            LispObject val, tail;
            LispObject elt, tem;
            /* 0 is the normal case.
               1 means this list is a doc reference; replace it with the number 0.
               2 means this list is a doc reference; replace it with the doc string.  */
            int doc_reference = 0;

            /* Initialize this to 1 if we are reading a list.  */
            bool first_in_list = flag <= 0;

            val = Q.nil;
            tail = Q.nil;

            while (true)
            {
                int ch;
                elt = read1 (readcharfun, out ch, first_in_list);

                first_in_list = false;

                /* While building, if the list starts with #$, treat it specially.  */
                if (EQ (elt, V.load_file_name)
                    && ! NILP (elt)
                    && !NILP (V.purify_flag))
                {
                    if (NILP (V.doc_file_name))
                        /* We have not yet called Snarf-documentation, so assume
                           this file is described in the DOC-MM.NN file
                           and Snarf-documentation will fill in the right value later.
                           For now, replace the whole list with 0.  */
                        doc_reference = 1;
                    else
                        /* We have already called Snarf-documentation, so make a relative
                           file name for this file, so it can be found properly
                           in the installed Lisp directory.
                           We don't use Fexpand_file_name because that would make
                           the directory absolute now.  */
                        elt = concat2 (build_string ("../lisp/"),
                                       F.file_name_nondirectory (elt));
                }
                else if (EQ (elt, V.load_file_name)
                         && ! NILP (elt)
                         && load_force_doc_strings)
                    doc_reference = 2;

                if (ch != 0)
                {
                    if (flag > 0)
                    {
                        if (ch == ']')
                            return val;
                        invalid_syntax (") or . in a vector");
                    }
                    if (ch == ')')
                        return val;
                    if (ch == '.')
                    {
                        if (!NILP (tail))
                            XSETCDR (tail, read0 (readcharfun));
                        else
                            val = read0 (readcharfun);
                        read1 (readcharfun, out ch, false);
                        if (ch == ')')
                        {
                            if (doc_reference == 1)
                                return make_number (0);
                            if (doc_reference == 2)
                            {
                                /* Get a doc string from the file we are loading.
                                   If it's in saved_doc_string, get it from there.

                                   Here, we don't know if the string is a
                                   bytecode string or a doc string.  As a
                                   bytecode string must be unibyte, we always
                                   return a unibyte string.  If it is actually a
                                   doc string, caller must make it
                                   multibyte.  */

                                int pos = XINT (XCDR (val));
                                /* Position is negative for user variables.  */
                                if (pos < 0) pos = -pos;
                                if (pos >= saved_doc_string_position
                                    && pos < (saved_doc_string_position
                                              + saved_doc_string_length))
                                {
                                    int start = pos - (int) saved_doc_string_position;
                                    int from, to;

                                    /* Process quoting with ^A,
                                       and find the end of the string,
                                       which is marked with ^_ (037).  */
                                    for (from = start, to = start;
                                         saved_doc_string[from] != 037;)
                                    {
                                        int c = saved_doc_string[from++];
                                        if (c == 1)
                                        {
                                            c = saved_doc_string[from++];
                                            if (c == 1)
                                                saved_doc_string[to++] = (byte) c;
                                            else if (c == '0')
                                                saved_doc_string[to++] = 0;
                                            else if (c == '_')
                                                saved_doc_string[to++] = 037;
                                        }
                                        else
                                            saved_doc_string[to++] = (byte) c;
                                    }

                                    return make_unibyte_string (saved_doc_string, start,
                                                                to - start);
                                }
                                /* Look in prev_saved_doc_string the same way.  */
                                else if (pos >= prev_saved_doc_string_position
                                         && pos < (prev_saved_doc_string_position
                                                   + prev_saved_doc_string_length))
                                {
                                    int start = pos - (int) prev_saved_doc_string_position;
                                    int from, to;

                                    /* Process quoting with ^A,
                                       and find the end of the string,
                                       which is marked with ^_ (037).  */
                                    for (from = start, to = start;
                                         prev_saved_doc_string[from] != 037;)
                                    {
                                        int c = prev_saved_doc_string[from++];
                                        if (c == 1)
                                        {
                                            c = prev_saved_doc_string[from++];
                                            if (c == 1)
                                                prev_saved_doc_string[to++] = (byte) c;
                                            else if (c == '0')
                                                prev_saved_doc_string[to++] = 0;
                                            else if (c == '_')
                                                prev_saved_doc_string[to++] = 037;
                                        }
                                        else
                                            prev_saved_doc_string[to++] = (byte) c;
                                    }

                                    return make_unibyte_string (prev_saved_doc_string,
                                                                start,
                                                                to - start);
                                }
                                else
                                    return get_doc_string (val, 1, 0);
                            }

                            return val;
                        }
                        invalid_syntax (". in wrong context");
                    }
                    invalid_syntax ("] in a list");
                }
                tem = F.cons (elt, Q.nil);
                if (!NILP (tail))
                    XSETCDR (tail, tem);
                else
                    val = tem;
                tail = tem;
                if (defunflag < 0)
                    defunflag = EQ (elt, Q.defun) ? 1 : 0;
//                else if (defunflag > 0)
//                    read_pure = 1;
            }
        }

        public static int readchar (LispObject readcharfun)
        {
            bool dummy;
            return readchar (readcharfun, out dummy); 
        }

        public delegate int readbyte_func(int i, LispObject a1);
        public static int readchar (LispObject readcharfun, out bool multibyte)
        {
            LispObject tem;
            int c = 0;

            readbyte_func readbyte;
            
            byte[] buf = new byte[MAX_MULTIBYTE_LENGTH];
            
            int i, len;
            bool emacs_mule_encoding = false;

            multibyte = false;

            readchar_count++;

#if COMEBACK_LATER
            if (BUFFERP (readcharfun))
            {
                buffer inbuffer = XBUFFER (readcharfun);

                int pt_byte = BUF_PT_BYTE (inbuffer);

                if (pt_byte >= BUF_ZV_BYTE (inbuffer))
                    return -1;

                if (! NILP (inbuffer.enable_multibyte_characters))
                {
                    /* Fetch the character code from the buffer.  */
                    unsigned char *p = BUF_BYTE_ADDRESS (inbuffer, pt_byte);
                    BUF_INC_POS (inbuffer, pt_byte);
                    c = STRING_CHAR (p, pt_byte - orig_pt_byte);
                    multibyte = 1;
                }
                else
                {
                    c = BUF_FETCH_BYTE (inbuffer, pt_byte);
                    if (! ASCII_BYTE_P (c))
                        c = BYTE8_TO_CHAR (c);
                    pt_byte++;
                }
                SET_BUF_PT_BOTH (inbuffer, BUF_PT (inbuffer) + 1, pt_byte);

                return c;
            }
            
            if (MARKERP (readcharfun))
            {
                buffer inbuffer = XMARKER (readcharfun).buffer;

                int bytepos = marker_byte_position (readcharfun);

                if (bytepos >= BUF_ZV_BYTE (inbuffer))
                    return -1;

                if (! NILP (inbuffer.enable_multibyte_characters))
                {
                    /* Fetch the character code from the buffer.  */
                    unsigned char *p = BUF_BYTE_ADDRESS (inbuffer, bytepos);
                    BUF_INC_POS (inbuffer, bytepos);
                    c = STRING_CHAR (p, bytepos - orig_bytepos);
                    multibyte = 1;
                }
                else
                {
                    c = BUF_FETCH_BYTE (inbuffer, bytepos);
                    if (! ASCII_BYTE_P (c))
                        c = BYTE8_TO_CHAR (c);
                    bytepos++;
                }

                XMARKER (readcharfun).bytepos = bytepos;
                XMARKER (readcharfun).charpos++;

                return c;
            }
#endif
            if (EQ (readcharfun, Q.lambda))
            {
                readbyte = readbyte_for_lambda;
                goto read_multibyte;
            }

            if (EQ (readcharfun, Q.get_file_char))
            {
                readbyte = readbyte_from_file;
                goto read_multibyte;
            }

            if (STRINGP (readcharfun))
            {
                if (read_from_string_index >= read_from_string_limit)
                    c = -1;
                else if (STRING_MULTIBYTE (readcharfun))
                {
                    multibyte = true;
                    FETCH_STRING_CHAR_ADVANCE_NO_CHECK (ref c, readcharfun,
                                                        ref read_from_string_index,
                                                        ref read_from_string_index_byte);
                }
                else
                {
                    c = SREF (readcharfun, read_from_string_index_byte);
                    read_from_string_index++;
                    read_from_string_index_byte++;
                }
                return c;
            }

            if (CONSP (readcharfun))
            {
                /* This is the case that read_vector is reading from a unibyte
                   string that contains a byte sequence previously skipped
                   because of #@NUMBER.  The car part of readcharfun is that
                   string, and the cdr part is a value of readcharfun given to
                   read_vector.  */
                readbyte = readbyte_from_string;
                if (EQ (XCDR (readcharfun), Q.get_emacs_mule_file_char))
                    emacs_mule_encoding = true;
                goto read_multibyte;
            }

            if (EQ (readcharfun, Q.get_emacs_mule_file_char))
            {
                readbyte = readbyte_from_file;
                emacs_mule_encoding = true;
                goto read_multibyte;
            }

            tem = call0 (readcharfun);

            if (NILP (tem))
                return -1;
            return XINT (tem);

            read_multibyte:
            if (unread_char >= 0)
            {
                c = unread_char;
                unread_char = -1;
                return c;
            }
            c = readbyte (-1, readcharfun);
            if (c < 0 || load_each_byte)
                return c;

            multibyte = true;

            if (ASCII_BYTE_P ((byte) c))
                return c;
            if (emacs_mule_encoding)
                return read_emacs_mule_char (c, readbyte, readcharfun);
            i = 0;
            buf[i++] = (byte) c;
            len = BYTES_BY_CHAR_HEAD ((uint) c);
            while (i < len)
            {
                c = readbyte (-1, readcharfun);
                if (c < 0 || ! TRAILING_CODE_P ((byte) c))
                {
                    while (--i > 1)
                        readbyte (buf[i], readcharfun);
                    return (int) BYTE8_TO_CHAR (buf[0]);
                }
                buf[i++] = (byte) c;
            }
            return (int) STRING_CHAR (buf, 0, i);
        }



        /* Unread the character C in the way appropriate for the stream READCHARFUN.
           If the stream is a user function, call it with the char as argument.  */
        public static void unreadchar(LispObject readcharfun, int c)
        {
            readchar_count--;
            if (c == -1)
            {
                /* Don't back up the pointer if we're unreading the end-of-input mark,
                   since readchar didn't advance it when we read it.  */
            }
#if COMEBACK_LATER
  else if (BUFFERP (readcharfun))
    {
      struct buffer *b = XBUFFER (readcharfun);
      int bytepos = BUF_PT_BYTE (b);

      BUF_PT (b)--;
      if (! NILP (b->enable_multibyte_characters))
	BUF_DEC_POS (b, bytepos);
      else
	bytepos--;

      BUF_PT_BYTE (b) = bytepos;
    }
  else if (MARKERP (readcharfun))
    {
      struct buffer *b = XMARKER (readcharfun)->buffer;
      int bytepos = XMARKER (readcharfun)->bytepos;

      XMARKER (readcharfun)->charpos--;
      if (! NILP (b->enable_multibyte_characters))
	BUF_DEC_POS (b, bytepos);
      else
	bytepos--;

      XMARKER (readcharfun)->bytepos = bytepos;
    }
#endif
            else if (STRINGP(readcharfun))
            {
                read_from_string_index--;
                read_from_string_index_byte = string_char_to_byte(readcharfun, read_from_string_index);
            }
            else if (CONSP(readcharfun))
            {
                unread_char = c;
            }
            else if (EQ(readcharfun, Q.lambda))
            {
                unread_char = c;
            }
            else if (EQ(readcharfun, Q.get_file_char) ||
                     EQ(readcharfun, Q.get_emacs_mule_file_char))
            {
                if (load_each_byte)
                {
                    instream.UngetByte((byte)c);
                }
                else
                    unread_char = c;
            }
            else
            {
                call1(readcharfun, make_number(c));
            }
        }

        /* readchar in lread.c calls back here to fetch the next byte.
           If UNREADFLAG is 1, we unread a byte.  */
        public static int read_bytecode_char (bool unreadflag)
        {
#if COMEBACK_LATER
            if (unreadflag)
            {
                read_bytecode_pointer--;
                return 0;
            }
            return *read_bytecode_pointer++;
#endif
            throw new System.Exception("Come back");
        }

        public static int readbyte_for_lambda(int c, LispObject readcharfun)
        {
            return read_bytecode_char(c >= 0);
        }

        public static int readbyte_from_file(int c, LispObject readcharfun)
        {
            if (c >= 0)
            {
                instream.UngetByte((byte)c);
                return 0;
            }

            c = instream.ReadByte();

#if EINTR
  /* Interrupted reads have been observed while reading over the network */
  while (c == EOF && ferror (instream) && errno == EINTR)
    {
      QUIT;
      clearerr (instream);
      c = getc (instream);
    }
#endif
            return c;
        }

        public static int readbyte_from_string(int c, LispObject readcharfun)
        {
            LispObject str = XCAR(readcharfun);

            if (c >= 0)
            {
                read_from_string_index--;
                read_from_string_index_byte = string_char_to_byte(str, read_from_string_index);
            }

            if (read_from_string_index >= read_from_string_limit)
                c = -1;
            else
                FETCH_STRING_CHAR_ADVANCE(ref c, str,
                               ref read_from_string_index,
                               ref read_from_string_index_byte);
            return c;
        }

        /* Read one non-ASCII character from INSTREAM.  The character is
           encoded in `emacs-mule' and the first byte is already read in
           C.  */
        public static int read_emacs_mule_char (int c, readbyte_func readbyte, LispObject readcharfun)
        {
            /* Emacs-mule coding uses at most 4-byte for one character.  */
            byte[] buf = new byte[4];
            int len = emacs_mule_bytes[c];
            charset charsett;
            int i;
            uint code;

            if (len == 1)
                /* C is not a valid leading-code of `emacs-mule'.  */
                return (int) BYTE8_TO_CHAR ((byte) c);

            i = 0;
            buf[i++] = (byte) c;
            while (i < len)
            {
                c = readbyte (-1, readcharfun);
                if (c < 0xA0)
                {
                    while (--i > 1)
                        readbyte (buf[i], readcharfun);
                    return (int) BYTE8_TO_CHAR (buf[0]);
                }
                buf[i++] = (byte) c;
            }

            if (len == 2)
            {
                charsett = emacs_mule_charset[buf[0]];
                code = (uint) (buf[1] & 0x7F);
            }
            else if (len == 3)
            {
                if (buf[0] == EMACS_MULE_LEADING_CODE_PRIVATE_11
                    || buf[0] == EMACS_MULE_LEADING_CODE_PRIVATE_12)
                {
                    charsett = emacs_mule_charset[buf[1]];
                    code = (uint) (buf[2] & 0x7F);
                }
                else
                {
                    charsett = emacs_mule_charset[buf[0]];
                    code = (uint) (((buf[1] << 8) | buf[2]) & 0x7F7F);
                }
            }
            else
            {
                charsett = emacs_mule_charset[buf[1]];
                code = (uint) (((buf[2] << 8) | buf[3]) & 0x7F7F);
            }
            c = DECODE_CHAR (charsett, code);
            if (c < 0)
                F.signal (Q.invalid_read_syntax,
                         F.cons (build_string ("invalid multibyte form"), Q.nil));
            return c;
        }
    }
}