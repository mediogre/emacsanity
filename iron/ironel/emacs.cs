namespace IronElisp
{
    public partial class L
    {
        /* Empty lisp strings.  To avoid having to build any others.  */
        public static LispObject empty_unibyte_string;
        public static LispObject empty_multibyte_string;

        /* Set nonzero after Emacs has started up the first time.
          Prevents reinitialization of the Lisp world and keymaps
          on subsequent starts.  */
        public static bool initialized;

        /* If non-zero, a filter or a sentinel is running.  Tested to save the match
           data on the first attempt to change it inside asynchronous code.  */
        public static bool running_asynch_code;

        // Nonzero means running Emacs without interactive terminal.
        public static bool noninteractive;

        /* Value of Lisp variable `noninteractive'.
           Normally same as C variable `noninteractive'
           but nothing terrible happens if user sets this one.  */
        public static bool noninteractive1;


        // this was main in C
        public static void MainInit(string[] argv)
        {
#if COMEBACK_LATER
            int do_initial_setlocale;
            int skip_args = 0;

            int no_loadup = 0;
            char *junk = 0;
            char *dname_arg = 0;

            sort_args (argc, argv);
            argc = 0;
            while (argv[argc]) argc++;

            if (argmatch (argv, argc, "-version", "--version", 3, NULL, &skip_args)
                /* We don't know the version number unless this is a dumped Emacs.
                   So ignore --version otherwise.  */
                && initialized)
            {
                Lisp_Object tem, tem2;
                tem = Fsymbol_value (intern ("emacs-version"));
                tem2 = Fsymbol_value (intern ("emacs-copyright"));
                if (!STRINGP (tem))
                {
                    fprintf (stderr, "Invalid value of `emacs-version'\n");
                    exit (1);
                }
                if (!STRINGP (tem2))
                {
                    fprintf (stderr, "Invalid value of `emacs-copyright'\n");
                    exit (1);
                }
                else
                {
                    printf ("GNU Emacs %s\n", SDATA (tem));
                    printf ("%s\n", SDATA(tem2));
                    printf ("GNU Emacs comes with ABSOLUTELY NO WARRANTY.\n");
                    printf ("You may redistribute copies of Emacs\n");
                    printf ("under the terms of the GNU General Public License.\n");
                    printf ("For more information about these matters, ");
                    printf ("see the file named COPYING.\n");
                    exit (0);
                }
            }

            clearerr (stdin);

            /* We do all file input/output as binary files.  When we need to translate
               newlines, we do that manually.  */
            _fmode = O_BINARY;

            /* Skip initial setlocale if LC_ALL is "C", as it's not needed in that case.
               The build procedure uses this while dumping, to ensure that the
               dumped Emacs does not have its system locale tables initialized,
               as that might cause screwups when the dumped Emacs starts up.  */
            {
                char *lc_all = getenv ("LC_ALL");
                do_initial_setlocale = ! lc_all || strcmp (lc_all, "C");
            }

            /* Set locale now, so that initial error messages are localized properly.
               fixup_locale must wait until later, since it builds strings.  */
            if (do_initial_setlocale)
                setlocale (LC_ALL, "");

            inhibit_window_system = 0;

            /* Handle the -batch switch, which means don't do interactive display.  */
            noninteractive = 0;
            if (argmatch (argv, argc, "-batch", "--batch", 5, NULL, &skip_args))
            {
                noninteractive = 1;
                Vundo_outer_limit = Qnil;
            }
            if (argmatch (argv, argc, "-script", "--script", 3, &junk, &skip_args))
            {
                noninteractive = 1;	/* Set batch mode.  */
                /* Convert --script to -scriptload, un-skip it, and sort again
                   so that it will be handled in proper sequence.  */
                /* FIXME broken for --script=FILE - is that supposed to work?  */
                argv[skip_args - 1] = "-scriptload";
                skip_args -= 2;
                sort_args (argc, argv);
            }

            /* Handle the --help option, which gives a usage message.  */
            if (argmatch (argv, argc, "-help", "--help", 3, NULL, &skip_args))
            {
                printf (USAGE1, argv[0], USAGE2);
                printf (USAGE3);
                printf (USAGE4, bug_reporting_address ());
                exit (0);
            }

            if (argmatch (argv, argc, "-daemon", "--daemon", 5, NULL, &skip_args)
                || argmatch (argv, argc, "-daemon", "--daemon", 5, &dname_arg, &skip_args))
            {
                fprintf (stderr, "This platform does not support the -daemon flag.\n");
                exit (1);
            }

            init_signals ();

            /* Don't catch SIGHUP if dumping.  */
            if (1
                )
            {
                /* In --batch mode, don't catch SIGHUP if already ignored.
                   That makes nohup work.  */
                if (! noninteractive
                    || signal (SIGHUP, SIG_IGN) != SIG_IGN)
                    signal (SIGHUP, fatal_error_signal);
            }

            if (
                1
                )
            {
                /* Don't catch these signals in batch mode if dumping.
                   On some machines, this sets static data that would make
                   signal fail to work right when the dumped Emacs is run.  */
                signal (SIGQUIT, fatal_error_signal);
                signal (SIGILL, fatal_error_signal);
                signal (SIGTRAP, fatal_error_signal);
// #ifdef SIGABRT
                signal (SIGABRT, fatal_error_signal);
// #endif
                signal (SIGFPE, fatal_error_signal);
                signal (SIGSEGV, fatal_error_signal);
                signal (SIGTERM, fatal_error_signal);
            }
#endif
            noninteractive1 = noninteractive;

            /* Perform basic initializations (not merely interning symbols).  */

            if (!initialized)
            {
                init_alloc_once ();
                init_obarray ();
                init_eval_once ();
#if COMEBACK_LATER
                init_character_once ();
                init_charset_once ();
                init_coding_once ();
                init_syntax_once ();	/* Create standard syntax table.  */
                init_category_once ();	/* Create standard category table.  */
                /* Must be done before init_buffer.  */
                init_casetab_once ();
                init_buffer_once ();	/* Create buffer table and some buffers.  */
                init_minibuf_once ();	/* Create list of minibuffers.  */
				/* Must precede init_window_once.  */

                /* Call syms_of_xfaces before init_window_once because that
                   function creates Vterminal_frame.  Termcap frames now use
                   faces, and the face implementation uses some symbols as
                   face names.  */
                syms_of_xfaces ();
                /* XXX syms_of_keyboard uses some symbols in keymap.c.  It would
                   be better to arrange things not to have this dependency.  */
                syms_of_keymap ();
                /* Call syms_of_keyboard before init_window_once because
                   keyboard sets up symbols that include some face names that
                   the X support will want to use.  This can happen when
                   CANNOT_DUMP is defined.  */
                syms_of_keyboard ();

                /* Called before syms_of_fileio, because it sets up Qerror_condition.  */
                syms_of_data ();
                syms_of_fileio ();
                /* Before syms_of_coding to initialize Vgc_cons_threshold.  */
                syms_of_alloc ();
                /* Before syms_of_coding because it initializes Qcharsetp.  */
                syms_of_charset ();
                /* Before init_window_once, because it sets up the
                   Vcoding_system_hash_table.  */
                syms_of_coding ();	/* This should be after syms_of_fileio.  */

                init_window_once ();	/* Init the window system.  */
                init_fileio_once ();	/* Must precede any path manipulation.  */
// #ifdef HAVE_WINDOW_SYSTEM
                init_fringe_once ();	/* Swap bitmaps if necessary. */
// #endif /* HAVE_WINDOW_SYSTEM */
#endif
            }

            init_alloc ();

#if COMEBACK_LATER
            if (do_initial_setlocale)
            {
                fixup_locale ();
                Vsystem_messages_locale = Vprevious_system_messages_locale;
                Vsystem_time_locale = Vprevious_system_time_locale;
            }
#endif
            init_eval ();
            init_data ();

            running_asynch_code = false;

#if COMEBACK_LATER
            /* Handle --unibyte and the EMACS_UNIBYTE envvar,
               but not while dumping.  */
            if (1)
            {
                int inhibit_unibyte = 0;

                /* --multibyte overrides EMACS_UNIBYTE.  */
                if (argmatch (argv, argc, "-no-unibyte", "--no-unibyte", 4, NULL, &skip_args)
                    || argmatch (argv, argc, "-multibyte", "--multibyte", 4, NULL, &skip_args)
                    /* Ignore EMACS_UNIBYTE before dumping.  */
                    || (!initialized && noninteractive))
                    inhibit_unibyte = 1;

                /* --unibyte requests that we set up to do everything with single-byte
                   buffers and strings.  We need to handle this before calling
                   init_lread, init_editfns and other places that generate Lisp strings
                   from text in the environment.  */
                /* Actually this shouldn't be needed as of 20.4 in a generally
                   unibyte environment.  As handa says, environment values
                   aren't now decoded; also existing buffers are now made
                   unibyte during startup if .emacs sets unibyte.  Tested with
                   8-bit data in environment variables and /etc/passwd, setting
                   unibyte and Latin-1 in .emacs. -- Dave Love  */
                if (argmatch (argv, argc, "-unibyte", "--unibyte", 4, NULL, &skip_args)
                    || argmatch (argv, argc, "-no-multibyte", "--no-multibyte", 4, NULL, &skip_args)
                    || (getenv ("EMACS_UNIBYTE") && !inhibit_unibyte))
                {
                    Lisp_Object old_log_max;
                    Lisp_Object symbol, tail;

                    symbol = intern ("default-enable-multibyte-characters");
                    Fset (symbol, Qnil);

                    if (initialized)
                    {
                        /* Erase pre-dump messages in *Messages* now so no abort.  */
                        old_log_max = Vmessage_log_max;
                        XSETFASTINT (Vmessage_log_max, 0);
                        message_dolog ("", 0, 1, 0);
                        Vmessage_log_max = old_log_max;
                    }

                    for (tail = Vbuffer_alist; CONSP (tail);
                         tail = XCDR (tail))
                    {
                        Lisp_Object buffer;

                        buffer = Fcdr (XCAR (tail));
                        /* Make a multibyte buffer unibyte.  */
                        if (BUF_Z_BYTE (XBUFFER (buffer)) > BUF_Z (XBUFFER (buffer)))
                        {
                            struct buffer *current = current_buffer;

                            set_buffer_temp (XBUFFER (buffer));
                            Fset_buffer_multibyte (Qnil);
                            set_buffer_temp (current);
                        }
                    }
                }
            }

            bool no_loadup = argmatch (argv, argc, "-nl", "--no-loadup", 6, NULL, &skip_args);
#endif
            /* argmatch must not be used after here,
               except when bulding temacs
               because the -d argument has not been skipped in skip_args.  */
#if COMEBACK_LATER
// #ifdef WINDOWSNT
            globals_of_w32 ();
            /* Initialize environment from registry settings.  */
            init_environment (argv);
            init_ntproc ();	/* must precede init_editfns.  */
// #endif

            /* egetenv is a pretty low-level facility, which may get called in
               many circumstances; it seems flimsy to put off initializing it
               until calling init_callproc.  */
            set_initial_environment ();

            init_buffer ();	/* Init default directory of main buffer.  */

            init_callproc_1 ();	/* Must precede init_cmdargs and init_sys_modes.  */
            init_cmdargs (argc, argv, skip_args);	/* Must precede init_lread.  */

            if (initialized)
            {
                /* Erase any pre-dump messages in the message log, to avoid confusion.  */
                LispObject old_log_max = V.message_log_max;
                V.message_log_max = make_number(0);
                message_dolog("", 0, 1, 0);
                V.message_log_max = old_log_max;
            }

            init_callproc ();	/* Must follow init_cmdargs but not init_sys_modes.  */
            init_lread ();

            /* Intern the names of all standard functions and variables;
               define standard keys.  */

            if (!initialized)
            {
                /* The basic levels of Lisp must come first.  */
                /* And data must come first of all
                   for the sake of symbols like error-message.  */
                syms_of_data ();
                syms_of_chartab ();
                syms_of_lread ();
                syms_of_print ();
                syms_of_eval ();
                syms_of_fns ();
                syms_of_floatfns ();

                syms_of_buffer ();
                syms_of_bytecode ();
                syms_of_callint ();
                syms_of_casefiddle ();
                syms_of_casetab ();
                syms_of_callproc ();
                syms_of_category ();
                syms_of_ccl ();
                syms_of_character ();
                syms_of_cmds ();
// #ifndef NO_DIR_LIBRARY
                syms_of_dired ();
// #endif /* not NO_DIR_LIBRARY */
                syms_of_display ();
                syms_of_doc ();
                syms_of_editfns ();
                syms_of_emacs ();

                syms_of_indent ();
                syms_of_insdel ();
                /* syms_of_keymap (); */
                syms_of_macros ();
                syms_of_marker ();
                syms_of_minibuf ();
                syms_of_process ();
                syms_of_search ();
                syms_of_frame ();
                syms_of_syntax ();
                syms_of_terminal ();
                syms_of_term ();

                syms_of_undo ();
// #ifdef HAVE_SOUND
                syms_of_sound ();
// #endif
                syms_of_textprop ();
                syms_of_composite ();
// #ifdef WINDOWSNT
                syms_of_ntproc ();
// #endif /* WINDOWSNT */
                syms_of_window ();
                syms_of_xdisp ();
                syms_of_font ();
// #ifdef HAVE_WINDOW_SYSTEM
                syms_of_fringe ();
                syms_of_image ();
// #endif /* HAVE_WINDOW_SYSTEM */

                syms_of_menu ();

// #ifdef HAVE_NTGUI
                syms_of_w32term ();
                syms_of_w32fns ();
                syms_of_w32select ();
                syms_of_w32menu ();
                syms_of_fontset ();
// #endif /* HAVE_NTGUI */

// #ifdef SYMS_SYSTEM
                syms_of_ntterm (); //SYMS_SYSTEM;
// #endif

                keys_of_casefiddle ();
                keys_of_cmds ();
                keys_of_buffer ();
                keys_of_keyboard ();
                keys_of_keymap ();
                keys_of_window ();
            }

                /* Initialization that must be done even if the global variable
                   initialized is non zero.  */
// #ifdef HAVE_NTGUI
                globals_of_w32fns ();
                globals_of_w32menu ();
                globals_of_w32select ();
// #endif  /* HAVE_NTGUI */

            init_charset ();

            init_editfns (); /* init_process uses Voperating_system_release. */
            init_process (); /* init_display uses add_keyboard_wait_descriptor. */
            init_keyboard ();	/* This too must precede init_sys_modes.  */
            if (!noninteractive)
                init_display ();	/* Determine terminal type.  Calls init_sys_modes.  */
            init_fns ();
            init_xdisp ();

// #ifdef HAVE_WINDOW_SYSTEM
            init_fringe ();
            init_image ();
// #endif /* HAVE_WINDOW_SYSTEM */
            init_macros ();
            init_floatfns ();
// #ifdef HAVE_SOUND
            init_sound ();
// #endif
            init_window ();

            if (!initialized)
            {
                string file;
                /* Handle -l loadup, args passed by Makefile.  */
                if (argmatch (argv, argc, "-l", "--load", 3, ref file, &skip_args))
                    V.top_level = F.cons (intern ("load"),
                                        F.cons (build_string (file), Q.nil));
                /* Unless next switch is -nl, load "loadup.el" first thing.  */
                if (! no_loadup)
                    V.top_level = F.cons (intern ("load"),
                                        F.cons (build_string ("loadup.el"), Q.nil));
            }

            initialized = true;

            /* Enter editor command loop.  This never returns.  */
            F.recursive_edit ();
#endif
        }
    }
}