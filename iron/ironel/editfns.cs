namespace IronElisp
{
    public partial class Q
    {
        public static LispObject buffer_access_fontify_functions;
        /* Symbol for the text property used to mark fields.  */
        public static LispObject field;
        /* A special value for Qfield properties.  */
        public static LispObject boundary;
    }

    public partial class V
    {
            

        public static LispObject buffer_access_fontify_functions
        {
            get { return Defs.O[(int)Objects.buffer_access_fontify_functions]; }
            set { Defs.O[(int)Objects.buffer_access_fontify_functions] = value; }
        }

        public static LispObject buffer_access_fontified_property
        {
            get { return Defs.O[(int)Objects.buffer_access_fontified_property]; }
            set { Defs.O[(int)Objects.buffer_access_fontified_property] = value; }
        }

        /* Non-nil means don't stop at field boundary in text motion commands.  */
        public static LispObject inhibit_field_text_motion
        {
            get { return Defs.O[(int)Objects.inhibit_field_text_motion]; }
            set { Defs.O[(int)Objects.inhibit_field_text_motion] = value; }
        }
    }

    public partial class F
    {
        public static LispObject insert_char(LispObject character, LispObject count, LispObject inherit)
        {
            byte[] stringg;
            int strlen;
            int i, n;
            int len;
            byte[] str = new byte[L.MAX_MULTIBYTE_LENGTH];

            L.CHECK_NUMBER(character);
            L.CHECK_NUMBER(count);

            if (!L.NILP(L.current_buffer.enable_multibyte_characters))
                len = L.CHAR_STRING((uint) L.XINT(character), str);
            else
            {
                str[0] = (byte) L.XINT(character);
                len = 1;
            }
            n = L.XINT(count) * len;
            if (n <= 0)
                return Q.nil;
            strlen = System.Math.Min(n, 256 * len);
            stringg = new byte[strlen];
            for (i = 0; i < strlen; i++)
                stringg[i] = str[i % len];
            while (n >= strlen)
            {
                L.QUIT();
                if (!L.NILP(inherit))
                    L.insert_and_inherit(stringg, strlen);
                else
                    L.insert(stringg, strlen);
                n -= strlen;
            }
            if (n > 0)
            {
                if (!L.NILP(inherit))
                    L.insert_and_inherit(stringg, n);
                else
                    L.insert(stringg, n);
            }
            return Q.nil;
        }

        public static LispObject current_time()
        {
            int t = L.EMACS_GET_TIME();
            return L.list3(L.make_number((L.EMACS_SECS(t) >> 16) & 0xffff),
                           L.make_number((L.EMACS_SECS(t) >> 0) & 0xffff),
                           L.make_number(L.EMACS_USECS(t)));
        }

        public static LispObject point_marker()
        {
            return L.buildmark(L.PT(), L.PT_BYTE());
        }

        public static LispObject forward_line(LispObject n)
        {
            int opoint = L.PT(), opoint_byte = L.PT_BYTE();
            int pos, pos_byte;
            int count, shortage;

            if (L.NILP(n))
                count = 1;
            else
            {
                L.CHECK_NUMBER(n);
                count = L.XINT(n);
            }

            if (count <= 0)
                shortage = L.scan_newline(L.PT(), L.PT_BYTE(), L.BEGV(), L.BEGV_BYTE(), count - 1, true);
            else
                shortage = L.scan_newline(L.PT(), L.PT_BYTE(), L.ZV, L.ZV_BYTE, count, true);

            /* Since scan_newline does TEMP_SET_PT_BOTH,
               and we want to set PT "for real",
               go back to the old point and then come back here.  */
            pos = L.PT();
            pos_byte = L.PT_BYTE();
            L.TEMP_SET_PT_BOTH(opoint, opoint_byte);
            L.SET_PT_BOTH(pos, pos_byte);

            if (shortage > 0
      && (count <= 0
      || (L.ZV > L.BEGV()
          && L.PT() != opoint
          && (L.FETCH_BYTE(L.PT_BYTE() - 1) != '\n'))))
                shortage--;

            return L.make_number(count <= 0 ? -shortage : shortage);
        }
        public static LispObject beginning_of_line(LispObject n)
        {
            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            L.SET_PT(L.XINT(F.line_beginning_position(n)));

            return Q.nil;
        }

        public static LispObject end_of_line(LispObject n)
        {
            int newpos;

            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            while (true)
            {
                newpos = L.XINT(F.line_end_position(n));
                L.SET_PT(newpos);

                if (L.PT() > newpos
                && L.FETCH_CHAR(L.PT() - 1) == '\n')
                {
                    /* If we skipped over a newline that follows
                       an invisible intangible run,
                       move back to the last tangible position
                       within the line.  */

                    L.SET_PT(L.PT() - 1);
                    break;
                }
                else if (L.PT() > newpos && L.PT() < L.ZV
                     && L.FETCH_CHAR(L.PT()) != '\n')
                    /* If we skipped something intangible
                       and now we're not really at eol,
                       keep going.  */
                    n = L.make_number(1);
                else
                    break;
            }

            return Q.nil;
        }

        public static LispObject field_beginning(LispObject pos, LispObject escape_from_edge, LispObject limit)
        {
            int beg = 0;
            int dummy = 0;
            L.find_field(pos, escape_from_edge, limit, true, ref beg, Q.nil, false, ref dummy);
            return L.make_number(beg);
        }

        public static LispObject field_end(LispObject pos, LispObject escape_from_edge, LispObject limit)
        {
            int end = 0;
            int dummy = 0;
            L.find_field(pos, escape_from_edge, Q.nil, false, ref dummy, limit, true, ref end);
            return L.make_number(end);
        }

        public static LispObject constrain_to_field(LispObject new_pos, LispObject old_pos, LispObject escape_from_edge, LispObject only_in_line, LispObject inhibit_capture_property)
        {
            /* If non-zero, then the original point, before re-positioning.  */
            int orig_point = 0;
            bool fwd;
            LispObject prev_old, prev_new;

            if (L.NILP(new_pos))
            /* Use the current point, and afterwards, set it.  */
            {
                orig_point = L.PT();
                new_pos = L.XSETINT(L.PT());
            }

            L.CHECK_NUMBER_COERCE_MARKER(ref new_pos);
            L.CHECK_NUMBER_COERCE_MARKER(ref old_pos);

            fwd = (L.XINT(new_pos) > L.XINT(old_pos));

            prev_old = L.make_number(L.XINT(old_pos) - 1);
            prev_new = L.make_number(L.XINT(new_pos) - 1);

            if (L.NILP(V.inhibit_field_text_motion)
                && !L.EQ(new_pos, old_pos)
                && (!L.NILP(F.get_char_property(new_pos, Q.field, Q.nil))
                    || !L.NILP(F.get_char_property(old_pos, Q.field, Q.nil))
                /* To recognize field boundaries, we must also look at the
                   previous positions; we could use `get_pos_property'
                   instead, but in itself that would fail inside non-sticky
                   fields (like comint prompts).  */
                    || (L.XINT(new_pos) > L.BEGV()
                        && !L.NILP(F.get_char_property(prev_new, Q.field, Q.nil)))
                    || (L.XINT(old_pos) > L.BEGV()
                        && !L.NILP(F.get_char_property(prev_old, Q.field, Q.nil))))
                && (L.NILP(inhibit_capture_property)
                /* Field boundaries are again a problem; but now we must
                   decide the case exactly, so we need to call
                   `get_pos_property' as well.  */
                    || (L.NILP(L.get_pos_property(old_pos, inhibit_capture_property, Q.nil))
                        && (L.XINT(old_pos) <= L.BEGV()
                            || L.NILP(F.get_char_property(old_pos, inhibit_capture_property, Q.nil))
                            || L.NILP(F.get_char_property(prev_old, inhibit_capture_property, Q.nil))))))
            /* It is possible that NEW_POS is not within the same field as
               OLD_POS; try to move NEW_POS so that it is.  */
            {
                int shortage = 0;
                LispObject field_bound;

                if (fwd)
                    field_bound = F.field_end(old_pos, escape_from_edge, new_pos);
                else
                    field_bound = F.field_beginning(old_pos, escape_from_edge, new_pos);

                if (/* See if ESCAPE_FROM_EDGE caused FIELD_BOUND to jump to the
             other side of NEW_POS, which would mean that NEW_POS is
             already acceptable, and it's not necessary to constrain it
             to FIELD_BOUND.  */
                ((L.XINT(field_bound) < L.XINT(new_pos)) ? fwd : !fwd))
                /* NEW_POS should be constrained, but only if either
                   ONLY_IN_LINE is nil (in which case any constraint is OK),
                   or NEW_POS and FIELD_BOUND are on the same line (in which
                   case the constraint is OK even if ONLY_IN_LINE is non-nil). */
                {
                    if (L.NILP(only_in_line))
                        new_pos = field_bound;
                    else
                    {
                        /* This is the ONLY_IN_LINE case, check that NEW_POS and
                       FIELD_BOUND are on the same line by seeing whether
                       there's an intervening newline or not.  */
                        L.scan_buffer('\n', L.XINT(new_pos), L.XINT(field_bound),
                                 fwd ? -1 : 1, true, ref shortage, true);
                        if (shortage != 0)
                            /* Constrain NEW_POS to FIELD_BOUND.  */
                            new_pos = field_bound;
                    }
                }

                if (orig_point != 0 && L.XINT(new_pos) != orig_point)
                    /* The NEW_POS argument was originally nil, so automatically set PT. */
                    L.SET_PT(L.XINT(new_pos));
            }

            return new_pos;
        }

        public static LispObject line_beginning_position(LispObject n)
        {
            int orig, orig_byte, end;
            int count = L.SPECPDL_INDEX();
            L.specbind(Q.inhibit_point_motion_hooks, Q.t);

            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            orig = L.PT();
            orig_byte = L.PT_BYTE();
            F.forward_line(L.make_number(L.XINT(n) - 1));
            end = L.PT();

            L.SET_PT_BOTH(orig, orig_byte);

            L.unbind_to(count, Q.nil);

            /* Return END constrained to the current input field.  */
            return F.constrain_to_field(L.make_number(end), L.make_number(orig),
                  L.XINT(n) != 1 ? Q.t : Q.nil,
                  Q.t, Q.nil);
        }

        public static LispObject line_end_position(LispObject n)
        {
            int end_pos;
            int orig = L.PT();

            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            end_pos = L.find_before_next_newline(orig, 0, L.XINT(n) - (L.XINT(n) <= 0 ? 1 : 0));

            /* Return END_POS constrained to the current input field.  */
            return F.constrain_to_field(L.make_number(end_pos), L.make_number(orig),
			      Q.nil, Q.t, Q.nil);
        }

        /* Callers passing one argument to Finsert need not gcpro the
           argument "array", since the only element of the array will
           not be used after calling insert or insert_from_string, so
           we don't care if it gets trashed.  */
        public static LispObject insert(int nargs, params LispObject[] args)
        {
            L.general_insert_function(L.insert, L.insert_from_string, 0, nargs, args);
            return Q.nil;
        }

        public static LispObject buffer_substring(LispObject start, LispObject end)
        {
            int b, e;

            L.validate_region(ref start, ref end);
            b = L.XINT(start);
            e = L.XINT(end);

            return L.make_buffer_string(b, e, 1);
        }

        public static LispObject following_char()
        {
            LispObject temp;
            if (L.PT() >= L.ZV)
                temp = L.XSETINT(0);
            else
                temp = L.XSETINT((int) L.FETCH_CHAR(L.PT_BYTE()));
            return temp;
        }

        public static LispObject previous_char()
        {
            LispObject temp;
            if (L.PT() <= L.BEGV())
                temp = L.XSETINT(0);
            else if (!L.NILP(L.current_buffer.enable_multibyte_characters))
            {
                int pos = L.PT_BYTE();
                L.DEC_POS(ref pos);
                temp = L.XSETINT((int) L.FETCH_CHAR(pos));
            }
            else
                temp = L.XSETINT(L.FETCH_BYTE(L.PT_BYTE() - 1));
            return temp;
        }

        public static LispObject bobp()
        {
            if (L.PT() == L.BEGV())
                return Q.t;
            return Q.nil;
        }

        public static LispObject eobp()
        {
            if (L.PT() == L.ZV)
                return Q.t;
            return Q.nil;
        }

        public static LispObject bolp()
        {
            if (L.PT() == L.BEGV() || L.FETCH_BYTE(L.PT_BYTE() - 1) == '\n')
                return Q.t;
            return Q.nil;
        }

        public static LispObject eolp()
        {
            if (L.PT() == L.ZV || L.FETCH_BYTE(L.PT_BYTE()) == '\n')
                return Q.t;
            return Q.nil;
        }

        public static LispObject char_after(LispObject pos)
        {
            int pos_byte;

            if (L.NILP(pos))
            {
                pos_byte = L.PT_BYTE();
                pos = L.XSETINT(L.PT());
            }

            if (L.MARKERP(pos))
            {
                pos_byte = L.marker_byte_position(pos);
                if (pos_byte < L.BEGV_BYTE() || pos_byte >= L.ZV_BYTE)
                    return Q.nil;
            }
            else
            {
                L.CHECK_NUMBER_COERCE_MARKER(ref pos);
                if (L.XINT(pos) < L.BEGV() || L.XINT(pos) >= L.ZV)
                    return Q.nil;

                pos_byte = L.CHAR_TO_BYTE(L.XINT(pos));
            }

            return L.make_number((int) L.FETCH_CHAR(pos_byte));
        }

        public static LispObject goto_char(LispObject position)
        {
            int pos;

            if (L.MARKERP(position) && L.current_buffer == L.XMARKER(position).buffer)
            {
                pos = L.marker_position(position);
                if (pos < L.BEGV())
                    L.SET_PT_BOTH(L.BEGV(), L.BEGV_BYTE());
                else if (pos > L.ZV)
                    L.SET_PT_BOTH(L.ZV, L.ZV_BYTE);
                else
                    L.SET_PT_BOTH(pos, L.marker_byte_position(position));

                return position;
            }

            L.CHECK_NUMBER_COERCE_MARKER(ref position);

            pos = L.clip_to_bounds(L.BEGV(), L.XINT(position), L.ZV);
            L.SET_PT(pos);
            return position;
        }

        public static LispObject delete_region(LispObject start, LispObject end)
        {
            L.validate_region(ref start, ref end);
            L.del_range(L.XINT(start), L.XINT(end));
            return Q.nil;
        }

        public static LispObject widen()
        {
            if (L.BEG != L.BEGV() || L.Z != L.ZV)
                L.current_buffer.clip_changed = 1;
            L.current_buffer.begv = L.BEG;
            L.current_buffer.begv_byte = L.BEG_BYTE();
            L.SET_BUF_ZV_BOTH(L.current_buffer, L.Z, L.Z_BYTE);
            /* Changing the buffer bounds invalidates any recorded current column.  */
            L.invalidate_current_column();
            return Q.nil;
        }

        public static LispObject narrow_to_region(LispObject start, LispObject end)
        {
            L.CHECK_NUMBER_COERCE_MARKER(ref start);
            L.CHECK_NUMBER_COERCE_MARKER(ref end);

            if (L.XINT(start) > L.XINT(end))
            {
                LispObject tem;
                tem = start; start = end; end = tem;
            }

            if (!(L.BEG <= L.XINT(start) && L.XINT(start) <= L.XINT(end) && L.XINT(end) <= L.Z))
                L.args_out_of_range(start, end);

            if (L.BEGV() != L.XINT(start) || L.ZV != L.XINT(end))
                L.current_buffer.clip_changed = 1;

            L.SET_BUF_BEGV(L.current_buffer, L.XINT(start));
            L.SET_BUF_ZV(L.current_buffer, L.XINT(end));
            if (L.PT() < L.XINT(start))
                L.SET_PT(L.XINT(start));
            if (L.PT() > L.XINT(end))
                L.SET_PT(L.XINT(end));
            /* Changing the buffer bounds invalidates any recorded current column.  */
            L.invalidate_current_column();
            return Q.nil;
        }
    }

    public partial class L
    {
        public static int EMACS_SECS(int time)
        {
		    return time;
        }

        public static int EMACS_USECS(int time)
        {
            return 0;
        }

        public static int EMACS_GET_TIME()
        {
            System.DateTime epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            return (int)(System.DateTime.UtcNow - epoch).TotalSeconds;
        }

        public static LispObject save_excursion_save()
        {
            bool visible = (XBUFFER(XWINDOW(selected_window).buffer) == current_buffer);

            return F.cons(F.point_marker(),
                  F.cons(F.copy_marker(current_buffer.mark, Q.nil),
                         F.cons(visible ? Q.t : Q.nil,
                            F.cons(current_buffer.mark_active,
                               selected_window))));
        }

        public static LispObject save_excursion_restore(LispObject info)
        {
            LispObject tem, tem1, omark, nmark;
            bool visible_p;

            tem = F.marker_buffer(XCAR(info));
            /* If buffer being returned to is now deleted, avoid error */
            /* Otherwise could get error here while unwinding to top level
               and crash */
            /* In that case, Fmarker_buffer returns nil now.  */
            if (NILP(tem))
                return Q.nil;

            omark = nmark = Q.nil;

            F.set_buffer(tem);

            /* Point marker.  */
            tem = XCAR(info);
            F.goto_char(tem);
            unchain_marker(XMARKER(tem));

            /* Mark marker.  */
            info = XCDR(info);
            tem = XCAR(info);
            omark = F.marker_position(current_buffer.mark);
            F.set_marker(current_buffer.mark, tem, F.current_buffer());
            nmark = F.marker_position(tem);
            unchain_marker(XMARKER(tem));

            /* visible */
            info = XCDR(info);
            visible_p = !NILP(XCAR(info));

            /* Mark active */
            info = XCDR(info);
            tem = XCAR(info);
            tem1 = current_buffer.mark_active;
            current_buffer.mark_active = tem;

            if (!NILP(V.run_hooks))
            {
                /* If mark is active now, and either was not active
               or was at a different place, run the activate hook.  */
                if (!NILP(current_buffer.mark_active))
                {
                    if (!EQ(omark, nmark))
                        call1(V.run_hooks, intern("activate-mark-hook"));
                }
                /* If mark has ceased to be active, run deactivate hook.  */
                else if (!NILP(tem1))
                    call1(V.run_hooks, intern("deactivate-mark-hook"));
            }

            /* If buffer was visible in a window, and a different window was
               selected, and the old selected window is still showing this
               buffer, restore point in that window.  */
            tem = XCDR(info);
            if (visible_p && !EQ(tem, selected_window))
            {
                tem1 = XWINDOW(tem).buffer;
                if (/* Window is live...  */
                 BUFFERP(tem1)
                    /* ...and it shows the current buffer.  */
                 && XBUFFER(tem1) == current_buffer)
                    F.set_window_point(tem, make_number(PT()));
            }

            return Q.nil;
        }

        public static LispObject save_restriction_save()
        {
            if (BEGV() == BEG && ZV == Z)
                /* The common case that the buffer isn't narrowed.
                   We return just the buffer object, which save_restriction_restore
                   recognizes as meaning `no restriction'.  */
                return F.current_buffer();
            else
            /* We have to save a restriction, so return a pair of markers, one
               for the beginning and one for the end.  */
            {
                LispObject beg, end;

                beg = buildmark(BEGV(), BEGV_BYTE());
                end = buildmark(ZV, ZV_BYTE);

                /* END must move forward if text is inserted at its exact location.  */
                XMARKER(end).insertion_type = true;

                return F.cons(beg, end);
            }
        }

        public static LispObject save_restriction_restore(LispObject data)
        {
            if (CONSP(data))
            /* A pair of marks bounding a saved restriction.  */
            {
                LispMarker beg = XMARKER(XCAR(data));
                LispMarker end = XMARKER(XCDR(data));
                Buffer buf = beg.buffer; /* END should have the same buffer. */

                if (buf != null /* Verify marker still points to a buffer.  */
                && (beg.charpos != BUF_BEGV(buf) || end.charpos != BUF_ZV(buf)))
                /* The restriction has changed from the saved one, so restore
                   the saved restriction.  */
                {
                    int pt = BUF_PT(buf);

                    SET_BUF_BEGV_BOTH(buf, beg.charpos, beg.bytepos);
                    SET_BUF_ZV_BOTH(buf, end.charpos, end.bytepos);

                    if (pt < beg.charpos || pt > end.charpos)
                        /* The point is outside the new visible range, move it inside. */
                        SET_BUF_PT_BOTH(buf,
                                 clip_to_bounds(beg.charpos, pt, end.charpos),
                                 clip_to_bounds(beg.bytepos, BUF_PT_BYTE(buf),
                                         end.bytepos));

                    buf.clip_changed = 1; /* Remember that the narrowing changed. */
                }
            }
            else
            /* A buffer, which means that there was no old restriction.  */
            {
                Buffer buf = XBUFFER(data);

                if (buf != null/* Verify marker still points to a buffer.  */
                && (BUF_BEGV(buf) != BUF_BEG(buf) || BUF_ZV(buf) != BUF_Z(buf)))
                /* The buffer has been narrowed, get rid of the narrowing.  */
                {
                    SET_BUF_BEGV_BOTH(buf, BUF_BEG(buf), BUF_BEG_BYTE(buf));
                    SET_BUF_ZV_BOTH(buf, BUF_Z(buf), BUF_Z_BYTE(buf));

                    buf.clip_changed = 1; /* Remember that the narrowing changed. */
                }
            }

            return Q.nil;
        }

        public static LispObject buildmark (int charpos, int bytepos)
        {
            LispObject mark = F.make_marker();
            set_marker_both(mark, Q.nil, charpos, bytepos);
            return mark;
        }

        /* Find all the overlays in the current buffer that touch position POS.
           Return the number found, and store them in a vector in VEC
           of length LEN.  */
        public static int overlays_around(int pos, LispObject[] vec, int len)
        {
            LispObject overlay, start, end;
            LispOverlay tail;
            int startpos, endpos;
            int idx = 0;

            for (tail = current_buffer.overlays_before; tail != null; tail = tail.next)
            {
                overlay = tail;

                end = OVERLAY_END(overlay);
                endpos = OVERLAY_POSITION(end);
                if (endpos < pos)
                    break;
                start = OVERLAY_START(overlay);
                startpos = OVERLAY_POSITION(start);
                if (startpos <= pos)
                {
                    if (idx < len)
                        vec[idx] = overlay;
                    /* Keep counting overlays even if we can't return them all.  */
                    idx++;
                }
            }

            for (tail = current_buffer.overlays_after; tail != null; tail = tail.next)
            {
                overlay = tail;

                start = OVERLAY_START(overlay);
                startpos = OVERLAY_POSITION(start);
                if (pos < startpos)
                    break;
                end = OVERLAY_END(overlay);
                endpos = OVERLAY_POSITION(end);
                if (pos <= endpos)
                {
                    if (idx < len)
                        vec[idx] = overlay;
                    idx++;
                }
            }

            return idx;
        }

        /* Return the value of property PROP, in OBJECT at POSITION.
           It's the value of PROP that a char inserted at POSITION would get.
           OBJECT is optional and defaults to the current buffer.
           If OBJECT is a buffer, then overlay properties are considered as well as
           text properties.
           If OBJECT is a window, then that window's buffer is used, but
           window-specific overlays are considered only if they are associated
           with OBJECT. */
        public static LispObject get_pos_property(LispObject position, LispObject prop, LispObject obj)
        {
            CHECK_NUMBER_COERCE_MARKER(ref position);

            if (NILP(obj))
                obj = current_buffer;
            else if (WINDOWP(obj))
                obj = XWINDOW(obj).buffer;

            if (!BUFFERP(obj))
                /* pos-property only makes sense in buffers right now, since strings
                   have no overlays and no notion of insertion for which stickiness
                   could be obeyed.  */
                return F.get_text_property(position, prop, obj);
            else
            {
                int posn = XINT(position);
                int noverlays;
                LispObject[] overlay_vec;
                LispObject tem;
                Buffer obuf = current_buffer;

                set_buffer_temp(XBUFFER(obj));

                /* First try with room for 40 overlays.  */
                noverlays = 40;
                overlay_vec = new LispObject[noverlays];
                noverlays = overlays_around(posn, overlay_vec, noverlays);

                /* If there are more than 40,
               make enough space for all, and try again.  */
                if (noverlays > 40)
                {
                    overlay_vec = new LispObject[noverlays];
                    noverlays = overlays_around(posn, overlay_vec, noverlays);
                }
                noverlays = sort_overlays(overlay_vec, noverlays, null);

                set_buffer_temp(obuf);

                /* Now check the overlays in order of decreasing priority.  */
                while (--noverlays >= 0)
                {
                    LispObject ol = overlay_vec[noverlays];
                    tem = F.overlay_get(ol, prop);
                    if (!NILP(tem))
                    {
                        /* Check the overlay is indeed active at point.  */
                        LispObject start = OVERLAY_START(ol), finish = OVERLAY_END(ol);
                        if ((OVERLAY_POSITION(start) == posn
                         && XMARKER(start).insertion_type == true)
                        || (OVERLAY_POSITION(finish) == posn
                            && XMARKER(finish).insertion_type == false))
                        {
                            /* The overlay will not cover a char inserted at point.  */
                        }
                        else
                        {
                            return tem;
                        }
                    }
                }

                { /* Now check the text properties.  */
                    int stickiness = text_property_stickiness(prop, position, obj);
                    if (stickiness > 0)
                        return F.get_text_property(position, prop, obj);
                    else if (stickiness < 0
                         && XINT(position) > BUF_BEGV(XBUFFER(obj)))
                        return F.get_text_property(make_number(XINT(position) - 1),
                                       prop, obj);
                    else
                        return Q.nil;
                }
            }
        }

        public delegate void insert_func(byte[] str, int x);
        public delegate void insert_from_string_func(LispObject a, int b, int c, int d, int e, int f);

        /* Insert NARGS Lisp objects in the array ARGS by calling INSERT_FUNC
           (if a type of object is Lisp_Int) or INSERT_FROM_STRING_FUNC (if a
           type of object is Lisp_String).  INHERIT is passed to
           INSERT_FROM_STRING_FUNC as the last argument.  */
        public static void general_insert_function(insert_func insert_func,
                                            insert_from_string_func insert_from_string_func,
                                            int inherit, int nargs, params LispObject[] args)
        {
            int argnum;
            LispObject val;

            for (argnum = 0; argnum < nargs; argnum++)
            {
                val = args[argnum];
                if (CHARACTERP(val))
                {
                    byte[] str = new byte[MAX_MULTIBYTE_LENGTH];
                    int len;

                    if (!NILP(current_buffer.enable_multibyte_characters))
                        len = CHAR_STRING((uint)XINT(val), str);
                    else
                    {
                        str[0] = (ASCII_CHAR_P((uint)XINT(val))
                          ? (byte)XINT(val)
                          : (byte)multibyte_char_to_unibyte((uint)XINT(val), Q.nil));
                        len = 1;
                    }
                    insert_func(str, len);
                }
                else if (STRINGP(val))
                {
                    insert_from_string_func(val, 0, 0,
                                  SCHARS(val),
                                  SBYTES(val),
                                  inherit);
                }
                else
                    wrong_type_argument(Q.char_or_string_p, val);
            }
        }

        public static int clip_to_bounds (int lower, int num, int upper)
        {
            if (num < lower)
                return lower;
            else if (num > upper)
                return upper;
            else
                return num;
        }

        /* Making strings from buffer contents.  */

        /* Return a Lisp_String containing the text of the current buffer from
           START to END.  If text properties are in use and the current buffer
           has properties in the range specified, the resulting string will also
           have them, if PROPS is nonzero.

           We don't want to use plain old make_string here, because it calls
           make_uninit_string, which can cause the buffer arena to be
           compacted.  make_string has no way of knowing that the data has
           been moved, and thus copies the wrong data into the string.  This
           doesn't effect most of the other users of make_string, so it should
           be left as is.  But we should use this function when conjuring
           buffer substrings.  */
        public static LispObject make_buffer_string (int start, int end, int props)
        {
            int start_byte = CHAR_TO_BYTE(start);
            int end_byte = CHAR_TO_BYTE(end);

            return make_buffer_string_both(start, start_byte, end, end_byte, props);
        }

        /* Return a Lisp_String containing the text of the current buffer from
           START / START_BYTE to END / END_BYTE.

           If text properties are in use and the current buffer
           has properties in the range specified, the resulting string will also
           have them, if PROPS is nonzero.

           We don't want to use plain old make_string here, because it calls
           make_uninit_string, which can cause the buffer arena to be
           compacted.  make_string has no way of knowing that the data has
           been moved, and thus copies the wrong data into the string.  This
           doesn't effect most of the other users of make_string, so it should
           be left as is.  But we should use this function when conjuring
           buffer substrings.  */
        public static LispObject make_buffer_string_both(int start, int start_byte, int end, int end_byte, int props)
        {
            LispObject result, tem, tem1;

            if (start < GPT && GPT < end)
                move_gap(start);

            if (!NILP(current_buffer.enable_multibyte_characters))
                result = make_uninit_multibyte_string(end - start, end_byte - start_byte);
            else
                result = make_uninit_string(end - start);
            XSTRING(result).bcopy(BYTE_POS_ADDR(start_byte), end_byte - start_byte);

            /* If desired, update and copy the text properties.  */
            if (props != 0)
            {
                update_buffer_properties(start, end);

                tem = F.next_property_change(make_number(start), Q.nil, make_number(end));
                tem1 = F.text_properties_at(make_number(start), Q.nil);

                if (XINT(tem) != end || !NILP(tem1))
                    copy_intervals_to_string(result, current_buffer, start,
                                  end - start);
            }

            return result;
        }

        /* Call Vbuffer_access_fontify_functions for the range START ... END
           in the current buffer, if necessary.  */
        public static void update_buffer_properties(int start, int end)
        {
            /* If this buffer has some access functions,
               call them, specifying the range of the buffer being accessed.  */
            if (!NILP(V.buffer_access_fontify_functions))
            {
                LispObject[] args = new LispObject[3];
                LispObject tem;

                args[0] = Q.buffer_access_fontify_functions;
                args[1] = XSETINT(start);
                args[2] = XSETINT(end);

                /* But don't call them if we can tell that the work
               has already been done.  */
                if (!NILP(V.buffer_access_fontified_property))
                {
                    tem = F.text_property_any(args[1], args[2],
                                  V.buffer_access_fontified_property,
                                  Q.nil, Q.nil);
                    if (!NILP(tem))
                        F.run_hook_with_args(3, args);
                }
                else
                    F.run_hook_with_args(3, args);
            }
        }

        /* Find the field surrounding POS in *BEG and *END.  If POS is nil,
   the value of point is used instead.  If BEG or END is null,
   means don't store the beginning or end of the field.

   BEG_LIMIT and END_LIMIT serve to limit the ranged of the returned
   results; they do not effect boundary behavior.

   If MERGE_AT_BOUNDARY is nonzero, then if POS is at the very first
   position of a field, then the beginning of the previous field is
   returned instead of the beginning of POS's field (since the end of a
   field is actually also the beginning of the next input field, this
   behavior is sometimes useful).  Additionally in the MERGE_AT_BOUNDARY
   true case, if two fields are separated by a field with the special
   value `boundary', and POS lies within it, then the two separated
   fields are considered to be adjacent, and POS between them, when
   finding the beginning and ending of the "merged" field.

   Either BEG or END may be 0, in which case the corresponding value
   is not stored.  */
        public static void find_field(LispObject pos, LispObject merge_at_boundary, LispObject beg_limit,
                                      bool has_beg, ref int beg, LispObject end_limit, bool has_end, ref int end)
        {
            /* Fields right before and after the point.  */
            LispObject before_field, after_field;
            /* 1 if POS counts as the start of a field.  */
            int at_field_start = 0;
            /* 1 if POS counts as the end of a field.  */
            int at_field_end = 0;

            if (NILP(pos))
                pos = XSETINT(PT());
            else
                CHECK_NUMBER_COERCE_MARKER(ref pos);

            LispObject dummy = null;

            after_field
              = get_char_property_and_overlay(pos, Q.field, Q.nil, ref dummy);
            before_field
              = (XINT(pos) > BEGV()
                 ? get_char_property_and_overlay(make_number(XINT(pos) - 1),
                              Q.field, Q.nil, ref dummy)
                /* Using nil here would be a more obvious choice, but it would
                   fail when the buffer starts with a non-sticky field.  */
                 : after_field);

            /* See if we need to handle the case where MERGE_AT_BOUNDARY is nil
               and POS is at beginning of a field, which can also be interpreted
               as the end of the previous field.  Note that the case where if
               MERGE_AT_BOUNDARY is non-nil (see function comment) is actually the
               more natural one; then we avoid treating the beginning of a field
               specially.  */
            if (NILP(merge_at_boundary))
            {
                LispObject field = get_pos_property(pos, Q.field, Q.nil);
                if (!EQ(field, after_field))
                    at_field_end = 1;
                if (!EQ(field, before_field))
                    at_field_start = 1;
                if (NILP(field) && at_field_start != 0 && at_field_end != 0)
                    /* If an inserted char would have a nil field while the surrounding
                       text is non-nil, we're probably not looking at a
                       zero-length field, but instead at a non-nil field that's
                       not intended for editing (such as comint's prompts).  */
                    at_field_end = at_field_start = 0;
            }

            /* Note about special `boundary' fields:

               Consider the case where the point (`.') is between the fields `x' and `y':

              xxxx.yyyy

               In this situation, if merge_at_boundary is true, we consider the
               `x' and `y' fields as forming one big merged field, and so the end
               of the field is the end of `y'.

               However, if `x' and `y' are separated by a special `boundary' field
               (a field with a `field' char-property of 'boundary), then we ignore
               this special field when merging adjacent fields.  Here's the same
               situation, but with a `boundary' field between the `x' and `y' fields:

              xxx.BBBByyyy

               Here, if point is at the end of `x', the beginning of `y', or
               anywhere in-between (within the `boundary' field), we merge all
               three fields and consider the beginning as being the beginning of
               the `x' field, and the end as being the end of the `y' field.  */

            if (has_beg)
            {
                if (at_field_start != 0)
                    /* POS is at the edge of a field, and we should consider it as
                       the beginning of the following field.  */
                    beg = XINT(pos);
                else
                /* Find the previous field boundary.  */
                {
                    LispObject p = pos;
                    if (!NILP(merge_at_boundary) && EQ(before_field, Q.boundary))
                        /* Skip a `boundary' field.  */
                        p = F.previous_single_char_property_change(p, Q.field, Q.nil,
                                               beg_limit);

                    p = F.previous_single_char_property_change(p, Q.field, Q.nil,
                                           beg_limit);
                    beg = NILP(p) ? BEGV() : XINT(p);
                }
            }

            if (has_end)
            {
                if (at_field_end != 0)
                    /* POS is at the edge of a field, and we should consider it as
                       the end of the previous field.  */
                    end = XINT(pos);
                else
                /* Find the next field boundary.  */
                {
                    if (!NILP(merge_at_boundary) && EQ(after_field, Q.boundary))
                        /* Skip a `boundary' field.  */
                        pos = F.next_single_char_property_change(pos, Q.field, Q.nil,
                                             end_limit);

                    pos = F.next_single_char_property_change(pos, Q.field, Q.nil,
                                         end_limit);
                    end = NILP(pos) ? ZV : XINT(pos);
                }
            }
        }
    }
}