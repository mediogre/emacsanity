namespace IronElisp
{
    public partial class L
    {
        public static LispHash initial_obarray;

        public static void init_obarray()
        {
            initial_obarray = V.obarray = new LispHash();

            Q.nil = F.make_symbol(make_string("nil"));
            Q.nil.Interned = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            Q.nil.Constant = true;
            Q.nil.Value = Q.nil;
            Q.nil.Function = Q.unbound;
            Q.nil.Plist = Q.nil;

            V.obarray["nil"] = Q.nil;

            Q.unbound = F.make_symbol(make_string("unbound"));
            Q.unbound.Value = Q.unbound;
            Q.unbound.Function = Q.unbound;

            Q.t = intern("t");
            Q.t.Value = Q.t;
            Q.t.Constant = true;

            Q.variable_documentation = intern("variable-documentation");

            // init read buffer?
        }

        static LispSymbol oblookup(LispHash obarray, string str)
        {
            return obarray[str];
        }

        public static LispSymbol intern(string str)
        {
            LispHash obarray = V.obarray;
            obarray = check_obarray(obarray);

            LispSymbol tem = oblookup(obarray, str);

            if (tem != null)
                return tem;

            return F.intern(make_string(str), obarray);
        }

        public static LispHash check_obarray(LispObject obarray)
        {
            /* need to check if V.obarray is invalid and set it ot initial obarray
                        if (h.Size == 0)
                        {
                            if (ReferenceEquals(h, V.obarray))
                            {
                                V.obarray = initial_obarray;
                            }
                        }
            */

            return obarray as LispHash;
        }

        public static void defsubr(string name, subr2 fn, int min_args, string int_spec, string doc_string)
        {
            LispSymbol sym = intern(name);
            sym.Function   = new LispSubr(name, fn, min_args, 2, int_spec, doc_string);
        }

        public static void syms_of_lread()
        {
            defsubr("intern", F.intern, 1, null,
@"Return the canonical symbol whose name is STRING.
If there is none, one is created by this function and returned.
A second optional argument specifies the obarray to use;
it defaults to the value of `obarray'.");
        }
    }


    public partial class V
    {
        public static LispHash obarray;
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
        
        public static LispSymbol intern(LispObject str, LispObject obarray)
        {
            if (obarray == null)
            {
                obarray = V.obarray;
            }

            LispHash hash = L.check_obarray(obarray);
            LispString s  = str as LispString;

            LispSymbol sym = hash[s.SData];
            if (sym != null)
            {
                return sym;
            }

            LispSymbol.symbol_interned intern_type = LispSymbol.symbol_interned.SYMBOL_INTERNED;
            if (ReferenceEquals(hash, L.initial_obarray))
            {
                intern_type = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            }

            sym = F.make_symbol(s);
            sym.Interned = intern_type;

            if (ReferenceEquals(hash, L.initial_obarray) && s.SData.Length > 0 && s.SData[0] == ':')
            {
                sym.Constant = true;
                sym.Value = sym;
            }

            hash[s.SData] = sym;
            return sym;
        }
    }

    public partial class L
    {
        public static void defvar_lisp(string namestring, Indexable<LispObject> t, int o)
        {
            LispObject sym = intern(namestring);
            LispObject val = new LispObjFwd(t, o);

            SET_SYMBOL_VALUE(sym, val);
        }
    }
}