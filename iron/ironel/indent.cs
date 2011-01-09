namespace IronElisp
{
    public partial class L
    {
        /* Cache of beginning of line found by the last call of
           current_column. */
        public static int current_column_bol_cache;

        /* These three values memorize the current column to avoid recalculation.  */

        /* Last value returned by current_column.
           Some things in set last_known_column_point to -1
           to mark the memorized value as invalid.  */
        public static double last_known_column;

        /* Value of point when current_column was called.  */
        public static int last_known_column_point;

        /* Value of MODIFF when current_column was called.  */
        public static int last_known_column_modified;

        /* Get the display table to use for the current buffer.  */
        public static LispCharTable buffer_display_table ()
        {
            LispObject thisbuf;

            thisbuf = current_buffer.display_table;
            if (DISP_TABLE_P(thisbuf))
                return XCHAR_TABLE(thisbuf);
            if (DISP_TABLE_P(V.standard_display_table))
                return XCHAR_TABLE(V.standard_display_table);
            return null;
        }

        /* Cancel any recorded value of the horizontal position.  */
        public static void invalidate_current_column()
        {
            last_known_column_point = 0;
        }

        /* Indentation can insert tabs if this is non-zero;
           otherwise always uses spaces.  */
        public static bool indent_tabs_mode
        {
            get { return Defs.B[(int)Bools.indent_tabs_mode]; }
            set { Defs.B[(int)Bools.indent_tabs_mode] = value; }
        }

        /* Check the presence of a display property and compute its width.
           If a property was found and its width was found as well, return
           its width (>= 0) and set the position of the end of the property
           in ENDPOS.
           Otherwise just return -1.  */
        public static int check_display_width(int pos, int col, ref int endpos)
        {
            LispObject val, overlay = null;

            if (CONSP(val = get_char_property_and_overlay
                   (make_number(pos), Q.display, Q.nil, ref overlay))
                && EQ(Q.space, XCAR(val)))
            { /* FIXME: Use calc_pixel_width_or_height, as in term.c.  */
                LispObject plist = XCDR(val), prop;
                int width = -1;

                prop = F.plist_get(plist, Q.Cwidth);
                if (NATNUMP(prop))
                    width = XINT(prop);
                else if (FLOATP(prop))
                    width = (int)(XFLOAT_DATA(prop) + 0.5);
                else
                {
                    prop = F.plist_get(plist, Q.Calign_to);
                    if (NATNUMP(prop))
                        width = XINT(prop) - col;
                    else if (FLOATP(prop))
                        width = (int)(XFLOAT_DATA(prop) + 0.5) - col;
                }

                if (width >= 0)
                {
                    int start = 0;
                    if (OVERLAYP(overlay))
                        endpos = OVERLAY_POSITION(OVERLAY_END(overlay));
                    else
                        get_property_and_range(pos, Q.display, ref val, ref start, ref endpos, Q.nil);
                    return width;
                }
            }
            return -1;
        }

        /* Scanning from the beginning of the current line, stop at the buffer
           position ENDPOS or at the column GOALCOL or at the end of line, whichever
           comes first.
           Return the resulting buffer position and column in ENDPOS and GOALCOL.
           PREVCOL gets set to the column of the previous position (it's always
           strictly smaller than the goal column).  */
        public static void scan_for_column(bool has_end, ref int endpos, bool has_goal, ref int goalcol, bool has_prev, ref int prevcol)
        {
            int tab_width = XINT(current_buffer.tab_width);
            bool ctl_arrow = !NILP(current_buffer.ctl_arrow);
            LispCharTable dp = buffer_display_table();
            bool multibyte = !NILP(current_buffer.enable_multibyte_characters);
            composition_it cmp_it = new composition_it();
            LispObject window;
            Window w;

            /* Start the scan at the beginning of this line with column number 0.  */
            int col = 0, prev_col = 0;
            int goal = has_goal ? goalcol : MOST_POSITIVE_FIXNUM;
            int end = has_end ? endpos : PT();
            int scan, scan_byte;
            int next_boundary;
            {
                int opoint = PT(), opoint_byte = PT_BYTE();
                scan_newline(PT(), PT_BYTE(), BEGV(), BEGV_BYTE(), -1, true);
                current_column_bol_cache = PT();
                scan = PT();
                scan_byte = PT_BYTE();
                SET_PT_BOTH(opoint, opoint_byte);
                next_boundary = scan;
            }

            window = F.get_buffer_window(F.current_buffer(), Q.nil);
            w = !NILP(window) ? XWINDOW(window) : null;

            if (tab_width <= 0 || tab_width > 1000) tab_width = 8;
            // bzero (&cmp_it, sizeof cmp_it);
            cmp_it.id = -1;
            composition_compute_stop_pos(cmp_it, scan, scan_byte, end, Q.nil);

            /* Scan forward to the target position.  */
            while (scan < end)
            {
                int c;

                /* Occasionally we may need to skip invisible text.  */
                while (scan == next_boundary)
                {
                    int old_scan = scan;
                    /* This updates NEXT_BOUNDARY to the next place
                       where we might need to skip more invisible text.  */
                    scan = skip_invisible(scan, ref next_boundary, end, Q.nil);
                    if (scan != old_scan)
                        scan_byte = CHAR_TO_BYTE(scan);
                    if (scan >= end)
                        goto endloop;
                }

                /* Test reaching the goal column.  We do this after skipping
               invisible characters, so that we put point before the
               character on which the cursor will appear.  */
                if (col >= goal)
                    break;
                prev_col = col;

                { /* Check display property.  */
                    int endx = 0;
                    int width = check_display_width(scan, col, ref endx);
                    if (width >= 0)
                    {
                        col += width;
                        if (endx > scan) /* Avoid infinite loops with 0-width overlays.  */
                        {
                            scan = endx; scan_byte = charpos_to_bytepos(scan);
                            continue;
                        }
                    }
                }

                /* Check composition sequence.  */
                if (cmp_it.id >= 0
                || (scan == cmp_it.stop_pos
                    && composition_reseat_it(cmp_it, scan, scan_byte, end,
                              w, null, Q.nil)))
                    composition_update_it(cmp_it, scan, scan_byte, Q.nil);
                if (cmp_it.id >= 0)
                {
                    scan += cmp_it.nchars;
                    scan_byte += cmp_it.nbytes;
                    if (scan <= end)
                        col += cmp_it.width;
                    if (cmp_it.to == cmp_it.nglyphs)
                    {
                        cmp_it.id = -1;
                        composition_compute_stop_pos(cmp_it, scan, scan_byte, end,
                                      Q.nil);
                    }
                    else
                        cmp_it.from = cmp_it.to;
                    continue;
                }

                c = FETCH_BYTE(scan_byte);

                /* See if there is a display table and it relates
               to this character.  */

                if (dp != null
                && !(multibyte && BASE_LEADING_CODE_P((byte) c))
                && VECTORP(DISP_CHAR_VECTOR(dp, c)))
                {
                    LispObject charvec;
                    int i, n;

                    /* This character is displayed using a vector of glyphs.
                       Update the column/position based on those glyphs.  */

                    charvec = DISP_CHAR_VECTOR(dp, c);
                    n = ASIZE(charvec);

                    for (i = 0; i < n; i++)
                    {
                        /* This should be handled the same as
                       next_element_from_display_vector does it.  */
                        LispObject entry = AREF(charvec, i);

                        if (GLYPH_CODE_P(entry)
                        && GLYPH_CODE_CHAR_VALID_P(entry))
                            c = GLYPH_CODE_CHAR(entry);
                        else
                            c = ' ';

                        if (c == '\n')
                            goto endloop;
                        if (c == '\r' && EQ(current_buffer.selective_display, Q.t))
                            goto endloop;
                        if (c == '\t')
                        {
                            col += tab_width;
                            col = col / tab_width * tab_width;
                        }
                        else
                            ++col;
                    }
                }
                else
                {
                    /* The display table doesn't affect this character;
                       it displays as itself.  */

                    if (c == '\n')
                        goto endloop;
                    if (c == '\r' && EQ(current_buffer.selective_display, Q.t))
                        goto endloop;
                    if (c == '\t')
                    {
                        col += tab_width;
                        col = col / tab_width * tab_width;
                    }
                    else if (multibyte && BASE_LEADING_CODE_P((byte) c))
                    {
                        /* Start of multi-byte form.  */
                        PtrEmulator<byte> ptr;
                        int bytes = 0, width = 0, wide_column = 0;

                        ptr = BYTE_POS_ADDR(scan_byte);
                        MULTIBYTE_BYTES_WIDTH(ptr.Index, dp, ref bytes, ref width, ref wide_column);
                        /* Subtract one to compensate for the increment
                       that is going to happen below.  */
                        scan_byte += bytes - 1;
                        col += width;
                    }
                    else if (ctl_arrow && (c < 32 || c == 127))
                        col += 2;
                    else if (c < 32 || c >= 127)
                        col += 4;
                    else
                        col++;
                }
                scan++;
                scan_byte++;

            }
        endloop:

            last_known_column = col;
            last_known_column_point = PT();
            last_known_column_modified = MODIFF;

            if (has_goal)
                goalcol = col;
            if (has_end)
                endpos = scan;
            if (has_prev)
                prevcol = prev_col;
        }

        /* Skip some invisible characters starting from POS.
           This includes characters invisible because of text properties
           and characters invisible because of overlays.

           If position POS is followed by invisible characters,
           skip some of them and return the position after them.
           Otherwise return POS itself.

           Set *NEXT_BOUNDARY_P to the next position at which
           it will be necessary to call this function again.

           Don't scan past TO, and don't set *NEXT_BOUNDARY_P
           to a value greater than TO.

           If WINDOW is non-nil, and this buffer is displayed in WINDOW,
           take account of overlays that apply only in WINDOW.

           We don't necessarily skip all the invisible characters after POS
           because that could take a long time.  We skip a reasonable number
           which can be skipped quickly.  If there might be more invisible
           characters immediately following, then *NEXT_BOUNDARY_P
           will equal the return value.  */
        public static int skip_invisible(int pos, ref int next_boundary_p, int to, LispObject window)
        {
            LispObject prop, position, overlay_limit, proplimit;
            LispObject buffer, tmp;
            int end;
            int inv_p;

            position = XSETINT(pos);
            buffer = current_buffer;

            /* Give faster response for overlay lookup near POS.  */
            recenter_overlay_lists(current_buffer, pos);

            /* We must not advance farther than the next overlay change.
               The overlay change might change the invisible property;
               or there might be overlay strings to be displayed there.  */
            overlay_limit = F.next_overlay_change(position);
            /* As for text properties, this gives a lower bound
               for where the invisible text property could change.  */
            proplimit = F.next_property_change(position, buffer, Q.t);
            if (XINT(overlay_limit) < XINT(proplimit))
                proplimit = overlay_limit;
            /* PROPLIMIT is now a lower bound for the next change
               in invisible status.  If that is plenty far away,
               use that lower bound.  */
            if (XINT(proplimit) > pos + 100 || XINT(proplimit) >= to)
                next_boundary_p = XINT(proplimit);
            /* Otherwise, scan for the next `invisible' property change.  */
            else
            {
                /* Don't scan terribly far.  */
                proplimit = XSETINT(System.Math.Min(pos + 100, to));
                /* No matter what. don't go past next overlay change.  */
                if (XINT(overlay_limit) < XINT(proplimit))
                    proplimit = overlay_limit;
                tmp = F.next_single_property_change(position, Q.invisible,
                                buffer, proplimit);
                end = XINT(tmp);
#if ZER0
      /* Don't put the boundary in the middle of multibyte form if
         there is no actual property change.  */
      if (end == pos + 100
	  && !NILP (current_buffer.enable_multibyte_characters)
	  && end < ZV())
	while (pos < end && !CHAR_HEAD_P (POS_ADDR (end)))
	  end--;
#endif
                next_boundary_p = end;
            }
            /* if the `invisible' property is set, we can skip to
               the next property change */
            prop = F.get_char_property(position, Q.invisible,
                           (!NILP(window)
                            && EQ(XWINDOW(window).buffer, buffer))
                           ? window : buffer);
            inv_p = TEXT_PROP_MEANS_INVISIBLE(prop);
            /* When counting columns (window == nil), don't skip over ellipsis text.  */
            if (NILP(window) ? inv_p == 1 : inv_p != 0)
                return next_boundary_p;
            return pos;
        }

        /* Set variables WIDTH and BYTES for a multibyte sequence starting at P.

           DP is a display table or NULL.

           This macro is used in current_column_1, Fmove_to_column, and
           compute_motion.  */
        public static void MULTIBYTE_BYTES_WIDTH(int p, LispCharTable dp, ref int bytes, ref int width, ref int wide_column)
        {
            uint c;

            wide_column = 0;
            c = STRING_CHAR_AND_LENGTH(BEG_ADDR(), p, MAX_MULTIBYTE_LENGTH, ref bytes);
            if (BYTES_BY_CHAR_HEAD(BEG_ADDR()[p]) != bytes)
                width = bytes * 4;
            else
            {
                if (dp != null && VECTORP(DISP_CHAR_VECTOR(dp, (int) c)))
                    width = XVECTOR(DISP_CHAR_VECTOR(dp, (int) c)).Size;
                else
                    width = CHAR_WIDTH(c);
                if (width > 1)
                    wide_column = width;
            }
        }

        /* Return the column number of position POS
           by scanning forward from the beginning of the line.
           This function handles characters that are invisible
           due to text properties or overlays.  */
        public static double current_column_1()
        {
            int col = MOST_POSITIVE_FIXNUM;
            int opoint = PT();

            int dummy = 0;
            scan_for_column(true, ref opoint, true, ref col, false, ref dummy);
            return col;
        }

        public static double current_column()
        {
            int col;
            PtrEmulator<byte> ptr, stop;
            bool tab_seen;
            int post_tab;
            int c;
            int tab_width = XINT(current_buffer.tab_width);
            bool ctl_arrow = !NILP(current_buffer.ctl_arrow);
            LispCharTable dp = buffer_display_table();

            if (PT() == last_known_column_point
                && MODIFF == last_known_column_modified)
                return last_known_column;

            /* If the buffer has overlays, text properties,
               or multibyte characters, use a more general algorithm.  */
            if (BUF_INTERVALS(current_buffer) != null
                || current_buffer.overlays_before != null
                || current_buffer.overlays_after != null
                || Z != Z_BYTE)
                return current_column_1();

            /* Scan backwards from point to the previous newline,
               counting width.  Tab characters are the only complicated case.  */

            /* Make a pointer for decrementing through the chars before point.  */
            ptr = BYTE_POS_ADDR(PT_BYTE() - 1) + 1;
            /* Make a pointer to where consecutive chars leave off,
               going backwards from point.  */
            if (PT() == BEGV())
                stop = ptr;
            else if (PT() <= GPT || BEGV() > GPT)
                stop = BEGV_ADDR();
            else
                stop = GAP_END_ADDR();

            if (tab_width <= 0 || tab_width > 1000)
                tab_width = 8;

            col = 0; tab_seen = false; post_tab = 0;

            while (true)
            {
                int i, n;
                LispObject charvec;

                if (ptr == stop)
                {
                    /* We stopped either for the beginning of the buffer
                       or for the gap.  */
                    if (ptr == BEGV_ADDR())
                        break;

                    /* It was the gap.  Jump back over it.  */
                    stop = BEGV_ADDR();
                    ptr = GPT_ADDR();

                    /* Check whether that brings us to beginning of buffer.  */
                    if (BEGV() >= GPT)
                        break;
                }

                --ptr;
                c = ptr.Value;

                if (dp != null && VECTORP(DISP_CHAR_VECTOR(dp, c)))
                {
                    charvec = DISP_CHAR_VECTOR(dp, c);
                    n = ASIZE(charvec);
                }
                else
                {
                    charvec = Q.nil;
                    n = 1;
                }

                for (i = n - 1; i >= 0; --i)
                {
                    if (VECTORP(charvec))
                    {
                        /* This should be handled the same as
                       next_element_from_display_vector does it.  */
                        LispObject entry = AREF(charvec, i);

                        if (GLYPH_CODE_P(entry)
                        && GLYPH_CODE_CHAR_VALID_P(entry))
                            c = GLYPH_CODE_CHAR(entry);
                        else
                            c = ' ';
                    }

                    if (c >= 32 && c < 127)
                        col++;
                    else if (c == '\n'
                         || (c == '\r'
                             && EQ(current_buffer.selective_display, Q.t)))
                    {
                        ptr++;
                        goto start_of_line_found;
                    }
                    else if (c == '\t')
                    {
                        if (tab_seen)
                            col = ((col + tab_width) / tab_width) * tab_width;

                        post_tab += col;
                        col = 0;
                        tab_seen = true;
                    }
                    else if (VECTORP(charvec))
                        /* With a display table entry, C is displayed as is, and
                           not displayed as \NNN or as ^N.  If C is a single-byte
                           character, it takes one column.  If C is multi-byte in
                           an unibyte buffer, it's translated to unibyte, so it
                           also takes one column.  */
                        ++col;
                    else
                        col += (ctl_arrow && c < 128) ? 2 : 4;
                }
            }

        start_of_line_found:

            if (tab_seen)
            {
                col = ((col + tab_width) / tab_width) * tab_width;
                col += post_tab;
            }

            if (ptr == BEGV_ADDR())
                current_column_bol_cache = BEGV();
            else
                current_column_bol_cache = BYTE_TO_CHAR(PTR_BYTE_POS(ptr));

            last_known_column = col;
            last_known_column_point = PT();
            last_known_column_modified = MODIFF;

            return col;
        }
    }

    public partial class F
    {
        public static LispObject indent_to(LispObject column, LispObject minimum)
        {
            int mincol;
            int fromcol;
            int tab_width = L.XINT(L.current_buffer.tab_width);

            L.CHECK_NUMBER(column);
            if (L.NILP(minimum))
                minimum = L.XSETINT(0);
            L.CHECK_NUMBER(minimum);

            fromcol = (int) L.current_column();
            mincol = fromcol + L.XINT(minimum);
            if (mincol < L.XINT(column)) mincol = L.XINT(column);

            if (fromcol == mincol)
                return L.make_number(mincol);

            if (tab_width <= 0 || tab_width > 1000) tab_width = 8;

            if (L.indent_tabs_mode)
            {
                LispObject n;
                n = L.XSETINT(mincol / tab_width - fromcol / tab_width);
                if (L.XINT(n) != 0)
                {
                    F.insert_char(L.make_number('\t'), n, Q.t);

                    fromcol = (mincol / tab_width) * tab_width;
                }
            }

            column = L.XSETINT(mincol - fromcol);
            F.insert_char(L.make_number(' '), column, Q.t);

            L.last_known_column = mincol;
            L.last_known_column_point = L.PT();
            L.last_known_column_modified = L.MODIFF;

            column = L.XSETINT(mincol);
            return column;
        }
    }
}