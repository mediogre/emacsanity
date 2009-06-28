namespace IronElisp
{
    public partial class V
    {
        public static LispObject print_level
        {
            get { return Defs.O[(int)Objects.print_level]; }
            set { Defs.O[(int)Objects.print_level] = value; }
        }

        public static LispObject standard_output
        {
            get { return Defs.O[(int)Objects.standard_output]; }
            set { Defs.O[(int)Objects.standard_output] = value; }
        }
    }

    public partial class F
    {
        public static LispObject prin1(LispObject obj, LispObject printcharfun)
        {
            // COMEBACK_WHEN_READY !!!
            /*
              PRINTDECLARE;

              if (NILP (printcharfun))
                printcharfun = Vstandard_output;
              PRINTPREPARE;
              print (obj, printcharfun, 1);
              PRINTFINISH;
            */
            return obj;
        }

        public static LispObject error_message_string(LispObject obj)
        {
            // COMEBACK_WHEN_READY
            return Q.nil;
        }
    }

    public partial class L
    {
        /* Used from outside of print.c to print a block of SIZE
           single-byte chars at DATA on the default output stream.
           Do not use this on the contents of a Lisp string.  */

        public static void write_string(string data, int size)
        {
            // COMEBACK_WHEN_READY !!!
            /*
              PRINTDECLARE;
              Lisp_Object printcharfun;

              printcharfun = Vstandard_output;

              PRINTPREPARE;
              strout (data, size, size, printcharfun, 0);
              PRINTFINISH;
            */
        }

        public static void temp_output_buffer_setup (string bufname)
        {
            // COMEBACK_WHEN_READY !!!
            /*
            int count = SPECPDL_INDEX ();
            Buffer old = current_buffer;

            record_unwind_protect (set_buffer_if_live, F.current_buffer ());

            F.set_buffer (F.get_buffer_create (make_string (bufname)));
            
            F.kill_all_local_variables ();
            delete_all_overlays (current_buffer);

            current_buffer.directory = old.directory;
            current_buffer.read_only = Q.nil;
            current_buffer.filename = Q.nil;
            current_buffer.undo_list = Q.t;

            current_buffer.enable_multibyte_characters = buffer_defaults.enable_multibyte_characters;
            specbind (Q.inhibit_read_only, Q.t);
            specbind (Q.inhibit_modification_hooks, Q.t);
            F.erase_buffer ();

            LispObject buf = current_buffer;

            F.run_hooks (1, Q.temp_buffer_setup_hook);

            unbind_to (count, Q.nil);

            specbind (Q.standard_output, buf);
            */
        }

        public static void temp_output_buffer_show (LispObject buf)
        {
            // COMEBACK_WHEN_READY !!!
            /*
            register struct buffer *old = current_buffer;
            register Lisp_Object window;
            register struct window *w;

            XBUFFER (buf)->directory = current_buffer->directory;

            Fset_buffer (buf);
            BUF_SAVE_MODIFF (XBUFFER (buf)) = MODIFF;
            BEGV = BEG;
            ZV = Z;
            SET_PT (BEG);

            set_buffer_internal (old);

            if (!NILP (Vtemp_buffer_show_function))
                call1 (Vtemp_buffer_show_function, buf);
            else
            {
                window = display_buffer (buf, Qnil, Qnil);

                if (!EQ (XWINDOW (window)->frame, selected_frame))
                    Fmake_frame_visible (WINDOW_FRAME (XWINDOW (window)));
                Vminibuf_scroll_window = window;
                w = XWINDOW (window);
                XSETFASTINT (w->hscroll, 0);
                XSETFASTINT (w->min_hscroll, 0);
                set_marker_restricted_both (w->start, buf, BEG, BEG);
                set_marker_restricted_both (w->pointm, buf, BEG, BEG);

                // Run temp-buffer-show-hook, with the chosen window selected
                // and its buffer current. 

                if (!NILP (Vrun_hooks)
                    && !NILP (Fboundp (Qtemp_buffer_show_hook))
                    && !NILP (Fsymbol_value (Qtemp_buffer_show_hook)))
                {
                    int count = SPECPDL_INDEX ();
                    Lisp_Object prev_window, prev_buffer;
                    prev_window = selected_window;
                    XSETBUFFER (prev_buffer, old);

                    // Select the window that was chosen, for running the hook.
                    // Note: Both Fselect_window and select_window_norecord may
                    // set-buffer to the buffer displayed in the window,
                    // so we need to save the current buffer.  --stef 
                    record_unwind_protect (Fset_buffer, prev_buffer);
                    record_unwind_protect (select_window_norecord, prev_window);
                    Fselect_window (window, Qt);
                    Fset_buffer (w->buffer);
                    call1 (Vrun_hooks, Qtemp_buffer_show_hook);
                    unbind_to (count, Qnil);
                }
            }
            */
        }

        public static LispObject internal_with_output_to_temp_buffer(string bufname, subr0 function)
        {
            int count = SPECPDL_INDEX();
            LispObject buf, val;

            record_unwind_protect(F.set_buffer, F.current_buffer());
            temp_output_buffer_setup(bufname);
            buf = V.standard_output;

            val = function();

            temp_output_buffer_show(buf);

            return unbind_to(count, val);
        }
        
        public static LispObject internal_with_output_to_temp_buffer(string bufname, subr1 function, LispObject args)
        {
            int count = SPECPDL_INDEX();
            LispObject buf, val;

            record_unwind_protect(F.set_buffer, F.current_buffer());
            temp_output_buffer_setup(bufname);
            buf = V.standard_output;

            val = function(args);

            temp_output_buffer_show(buf);

            return unbind_to(count, val);
        }
    }
}