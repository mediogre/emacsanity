namespace IronElisp
{
    public partial class L
    {
        /* Nonzero if we should redraw the mode lines on the next redisplay.  */
        public static int update_mode_lines;

        /* Number of windows showing the buffer of the selected window (or
           another buffer with the same base buffer).  keyboard.c refers to
           this.  */
        public static int buffer_shared;

        /* Nonzero if window sizes or contents have changed since last
           redisplay that finished.  */
        public static int windows_or_buffers_changed;

        /* If >= 0, computed, exact values of mode-line and header-line height
           to use in the macros CURRENT_MODE_LINE_HEIGHT and
           CURRENT_HEADER_LINE_HEIGHT.  */
        public static int current_mode_line_height, current_header_line_height;

        // Non-zero while redisplay_internal is in progress.
        public static bool redisplaying_p;

        /* Non-zero means we're allowed to display a hourglass pointer.  */
        public static bool display_hourglass_p;

        /* Display a message M which contains a single %s
           which gets replaced with STRING.  */
        public static void message_with_string(string m, LispObject str, bool log)
        {
            // COMEBACK_WHEN_READY!!!
        }

        /* Return the pixel width of display area AREA of window W.  AREA < 0
           means return the total width of W, not including fringes to
           the left and right of the window.  */
        public static int window_box_width(Window w, int area)
        {
            int cols = XINT(w.total_cols);
            int pixels = 0;

            if (!w.pseudo_window_p)
            {
                cols -= WINDOW_SCROLL_BAR_COLS(w);

                if (area == (int)glyph_row_area.TEXT_AREA)
                {
                    if (INTEGERP(w.left_margin_cols))
                        cols -= XINT(w.left_margin_cols);
                    if (INTEGERP(w.right_margin_cols))
                        cols -= XINT(w.right_margin_cols);
                    pixels = -WINDOW_TOTAL_FRINGE_WIDTH(w);
                }
                else if (area == (int)glyph_row_area.LEFT_MARGIN_AREA)
                {
                    cols = (INTEGERP(w.left_margin_cols)
                         ? XINT(w.left_margin_cols) : 0);
                    pixels = 0;
                }
                else if (area == (int)glyph_row_area.RIGHT_MARGIN_AREA)
                {
                    cols = (INTEGERP(w.right_margin_cols)
                         ? XINT(w.right_margin_cols) : 0);
                    pixels = 0;
                }
            }

            return cols * WINDOW_FRAME_COLUMN_WIDTH(w) + pixels;
        }

        /* Return the window-relative coordinate of the left edge of display
           area AREA of window W.  AREA < 0 means return the left edge of the
           whole window, to the right of the left fringe of W.  */
        public static int window_box_left_offset(Window w, int area)
        {
            int x;

            if (w.pseudo_window_p)
                return 0;

            x = WINDOW_LEFT_SCROLL_BAR_AREA_WIDTH(w);

            if (area == (int)glyph_row_area.TEXT_AREA)
                x += (WINDOW_LEFT_FRINGE_WIDTH(w)
                  + window_box_width(w, (int)glyph_row_area.LEFT_MARGIN_AREA));
            else if (area == (int)glyph_row_area.RIGHT_MARGIN_AREA)
                x += (WINDOW_LEFT_FRINGE_WIDTH(w)
                  + window_box_width(w, (int)glyph_row_area.LEFT_MARGIN_AREA)
                  + window_box_width(w, (int)glyph_row_area.TEXT_AREA)
                  + (WINDOW_HAS_FRINGES_OUTSIDE_MARGINS(w)
                     ? 0
                     : WINDOW_RIGHT_FRINGE_WIDTH(w)));
            else if (area == (int)glyph_row_area.LEFT_MARGIN_AREA
                 && WINDOW_HAS_FRINGES_OUTSIDE_MARGINS(w))
                x += WINDOW_LEFT_FRINGE_WIDTH(w);

            return x;
        }

        /* Return the frame-relative coordinate of the left edge of display
           area AREA of window W.  AREA < 0 means return the left edge of the
           whole window, to the right of the left fringe of W.  */
        public static int window_box_left(Window w, int area)
        {
            Frame f = XFRAME(w.frame);
            int x;

            if (w.pseudo_window_p)
                return FRAME_INTERNAL_BORDER_WIDTH(f);

            x = (WINDOW_LEFT_EDGE_X(w)
                 + window_box_left_offset(w, area));

            return x;
        }

        /* EXPORT:
           Return an estimation of the pixel height of mode or top lines on
           frame F.  FACE_ID specifies what line's height to estimate.  */
        public static int estimate_mode_line_height(Frame f, face_id face_id)
        {
#if COMEBACK_LATER//#ifdef HAVE_WINDOW_SYSTEM
  if (FRAME_WINDOW_P (f))
    {
      int height = FONT_HEIGHT (FRAME_FONT (f));

      /* This function is called so early when Emacs starts that the face
	 cache and mode line face are not yet initialized.  */
      if (FRAME_FACE_CACHE (f))
	{
	  struct face *face = FACE_FROM_ID (f, face_id);
	  if (face)
	    {
	      if (face->font)
		height = FONT_HEIGHT (face->font);
	      if (face->box_line_width > 0)
		height += 2 * face->box_line_width;
	    }
	}

      return height;
    }
#endif

            return 1;
        }

        /* This is like a combination of memq and assq.  Return 1/2 if PROPVAL
           appears as an element of LIST or as the car of an element of LIST.
           If PROPVAL is a list, compare each element against LIST in that
           way, and return 1/2 if any element of PROPVAL is found in LIST.
           Otherwise return 0.  This function cannot quit.
           The return value is 2 if the text is invisible but with an ellipsis
           and 1 if it's invisible and without an ellipsis.  */
        public static int invisible_p(LispObject propval, LispObject list)
        {
            LispObject tail, proptail;

            for (tail = list; CONSP(tail); tail = XCDR(tail))
            {
                LispObject tem;
                tem = XCAR(tail);
                if (EQ(propval, tem))
                    return 1;
                if (CONSP(tem) && EQ(propval, XCAR(tem)))
                    return NILP(XCDR(tem)) ? 1 : 2;
            }

            if (CONSP(propval))
            {
                for (proptail = propval; CONSP(proptail); proptail = XCDR(proptail))
                {
                    LispObject propelt;
                    propelt = XCAR(proptail);
                    for (tail = list; CONSP(tail); tail = XCDR(tail))
                    {
                        LispObject tem;
                        tem = XCAR(tail);
                        if (EQ(propelt, tem))
                            return 1;
                        if (CONSP(tem) && EQ(propelt, XCAR(tem)))
                            return NILP(XCDR(tem)) ? 1 : 2;
                    }
                }
            }

            return 0;
        }

        /* Return value in display table DP (Lisp_Char_Table *) for character
           C.  Since a display table doesn't have any parent, we don't have to
           follow parent.  Do not call this function directly but use the
           macro DISP_CHAR_VECTOR.  */
        public static LispObject disp_char_vector(LispCharTable dp, int c)
        {
            LispObject val;

            if (ASCII_CHAR_P((uint)c))
            {
                val = dp.ascii;
                if (SUB_CHAR_TABLE_P(val))
                    val = XSUB_CHAR_TABLE(val)[c];
            }
            else
            {
                LispObject table;

                table = dp;
                val = char_table_ref(table, c);
            }
            if (NILP(val))
                val = dp.defalt;
            return val;
        }

        public static LispObject overlay_arrow_string_or_property(LispObject var)
        {
            LispObject val = F.get(var, Q.overlay_arrow_string);

            if (STRINGP(val))
                return val;

            return V.overlay_arrow_string;
        }

        /* Mark overlay arrows to be updated on next redisplay.  */
        public static void update_overlay_arrows(int up_to_date)
        {
            LispObject vlist;

            for (vlist = V.overlay_arrow_variable_list; CONSP(vlist); vlist = XCDR(vlist))
            {
                LispObject var = XCAR(vlist);

                if (!SYMBOLP(var))
                    continue;

                if (up_to_date > 0)
                {
                    LispObject val = find_symbol_value(var);
                    F.put(var, Q.last_arrow_position, COERCE_MARKER(val));
                    F.put(var, Q.last_arrow_string, overlay_arrow_string_or_property(var));
                }
                else if (up_to_date < 0
                     || !NILP(F.get(var, Q.last_arrow_position)))
                {
                    F.put(var, Q.last_arrow_position, Q.t);
                    F.put(var, Q.last_arrow_string, Q.t);
                }
            }
        }

        /* Value is the position described by X.  If X is a marker, value is
           the marker_position of X.  Otherwise, value is X.  */
        public static LispObject COERCE_MARKER(LispObject X)
        {
            return (MARKERP(X) ? F.marker_position(X) : X);
        }

        /* Mark the display of window W as accurate or inaccurate.  If
           ACCURATE_P is non-zero mark display of W as accurate.  If
           ACCURATE_P is zero, arrange for W to be redisplayed the next time
           redisplay_internal is called.  */
        public static void mark_window_display_accurate_1(Window w, int accurate_p)
        {
            if (BUFFERP(w.buffer))
            {
                Buffer b = XBUFFER(w.buffer);

                w.last_modified = make_number(accurate_p != 0 ? BUF_MODIFF(b) : 0);
                w.last_overlay_modified = make_number(accurate_p != 0 ? BUF_OVERLAY_MODIFF(b) : 0);
                w.last_had_star = BUF_MODIFF(b) > BUF_SAVE_MODIFF(b) ? Q.t : Q.nil;

                if (accurate_p != 0)
                {
                    b.clip_changed = 0;
                    b.prevent_redisplay_optimizations_p = false;

                    BUF_UNCHANGED_MODIFIED(b, BUF_MODIFF(b));
                    BUF_OVERLAY_UNCHANGED_MODIFIED(b, BUF_OVERLAY_MODIFF(b));
                    BUF_BEG_UNCHANGED(b, BUF_GPT(b) - BUF_BEG(b));
                    BUF_END_UNCHANGED(b, BUF_Z(b) - BUF_GPT(b));

                    w.current_matrix.buffer = b;
                    w.current_matrix.begv = BUF_BEGV(b);
                    w.current_matrix.zv = BUF_ZV(b);

                    w.last_cursor = w.cursor;
                    w.last_cursor_off_p = w.cursor_off_p;

                    if (w == XWINDOW(selected_window))
                        w.last_point = make_number(BUF_PT(b));
                    else
                        w.last_point = make_number(XMARKER(w.pointm).charpos);
                }
            }

            if (accurate_p != 0)
            {
                w.window_end_valid = w.buffer;
                w.update_mode_line = Q.nil;
            }
        }

        /* Mark the display of windows in the window tree rooted at WINDOW as
           accurate or inaccurate.  If ACCURATE_P is non-zero mark display of
           windows as accurate.  If ACCURATE_P is zero, arrange for windows to
           be redisplayed the next time redisplay_internal is called.  */
        public static void mark_window_display_accurate(LispObject window, int accurate_p)
        {
            Window w;

            for (; !NILP(window); window = w.next)
            {
                w = XWINDOW(window);
                mark_window_display_accurate_1(w, accurate_p);

                if (!NILP(w.vchild))
                    mark_window_display_accurate(w.vchild, accurate_p);
                if (!NILP(w.hchild))
                    mark_window_display_accurate(w.hchild, accurate_p);
            }

            if (accurate_p != 0)
            {
                update_overlay_arrows(1);
            }
            else
            {
                /* Force a thorough redisplay the next time by setting
               last_arrow_position and last_arrow_string to t, which is
               unequal to any useful value of Voverlay_arrow_...  */
                update_overlay_arrows(-1);
            }
        }

        /* Error handler for safe_eval and safe_call.  */
        public static LispObject safe_eval_handler (LispObject arg)
        {
            add_to_log("Error during redisplay: %s", arg, Q.nil);
            return Q.nil;
        }

        /* Evaluate SEXPR and return the result, or nil if something went
           wrong.  Prevent redisplay during the evaluation.  */

        /* Call function ARGS[0] with arguments ARGS[1] to ARGS[NARGS - 1].
           Return the result, or nil if something went wrong.  Prevent
           redisplay during the evaluation.  */
        public static LispObject safe_call (int nargs, params LispObject[] args)
        {
            LispObject val;

            if (V.inhibit_eval_during_redisplay)
                val = Q.nil;
            else
            {
                int count = SPECPDL_INDEX();
                specbind(Q.inhibit_redisplay, Q.t);
                /* Use Qt to ensure debugger does not run,
               so there is no possibility of wanting to redisplay.  */
                val = internal_condition_case_2(F.funcall, nargs, args, Q.t,
				       safe_eval_handler);
                val = unbind_to(count, val);
            }

            return val;
        }

        /* Add a message with format string FORMAT and arguments ARG1 and ARG2
           to *Messages*.  */
        public static void add_to_log(string format, LispObject arg1, LispObject arg2)
        {
            // COMEBACK WHEN READY
            /*  Lisp_Object args[3];
              Lisp_Object msg, fmt;
              char *buffer;
              int len;
              USE_SAFE_ALLOCA;

            //  /* Do nothing if called asynchronously.  Inserting text into
            //     a buffer may call after-change-functions and alike and
            //     that would means running Lisp asynchronously.  * /
              if (handling_signal)
                return;

              fmt = msg = Qnil;

              args[0] = fmt = build_string (format);
              args[1] = arg1;
              args[2] = arg2;
              msg = Fformat (3, args);

              len = SBYTES (msg) + 1;
              SAFE_ALLOCA (buffer, char *, len);
              bcopy (SDATA (msg), buffer, len);

              message_dolog (buffer, len - 1, 1, 0);
              SAFE_FREE ();
            */
        }
    }

    public partial class Q
    {
        // Non-nil means don't actually do any redisplay.
        public static LispObject inhibit_redisplay;

        public static LispObject risky_local_variable;
        public static LispObject inhibit_point_motion_hooks;

        /* Names of text properties relevant for redisplay.  */
        public static LispObject display;
        public static LispObject space, Calign_to;

        /* Alternative overlay-arrow-string and overlay-arrow-bitmap
           properties on a symbol in overlay-arrow-variable-list.  */
        public static LispObject overlay_arrow_string, overlay_arrow_bitmap;

        /* Values of those variables at last redisplay are stored as
           properties on `overlay-arrow-position' symbol.  However, if
           Voverlay_arrow_position is a marker, last-arrow-position is its
           numerical position.  */
        public static LispObject last_arrow_position, last_arrow_string;

        public static LispObject window_scroll_functions;
    }

    public partial class V
    {
        // Non-nil means don't actually do any redisplay.
        public static LispObject inhibit_redisplay;

        public static bool inhibit_eval_during_redisplay
        {
            get { return Defs.B[(int)Bools.inhibit_eval_during_redisplay]; }
            set { Defs.B[(int)Bools.inhibit_eval_during_redisplay] = value; }            
        }

        /* Non-nil means highlight trailing whitespace.  */
        public static LispObject show_trailing_whitespace
        {
            get { return Defs.O[(int)Objects.show_trailing_whitespace]; }
            set { Defs.O[(int)Objects.show_trailing_whitespace] = value; }
        }

        /* Number of lines to keep in the message log buffer.  t means
           infinite.  nil means don't log at all.  */
        public static LispObject message_log_max
        {
            get { return Defs.O[(int)Objects.message_log_max]; }
            set { Defs.O[(int)Objects.message_log_max] = value; }
        }

        /* String to display for the arrow.  Only used on terminal frames.  */
        public static LispObject overlay_arrow_string
        {
            get { return Defs.O[(int)Objects.overlay_arrow_string]; }
            set { Defs.O[(int)Objects.overlay_arrow_string] = value; }
        }

        /* List of variables (symbols) which hold markers for overlay arrows.
           The symbols on this list are examined during redisplay to determine
           where to display overlay arrows.  */
        public static LispObject overlay_arrow_variable_list
        {
            get { return Defs.O[(int)Objects.overlay_arrow_variable_list]; }
            set { Defs.O[(int)Objects.overlay_arrow_variable_list] = value; }
        }        

        public static LispObject window_scroll_functions
        {
            get { return Defs.O[(int)Objects.window_scroll_functions]; }
            set { Defs.O[(int)Objects.window_scroll_functions] = value; }
        }
    }
}