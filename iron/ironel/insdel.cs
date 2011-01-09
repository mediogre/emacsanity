namespace IronElisp
{
    public partial class V
    {
        /* Non-nil means don't call the after-change-functions right away,
           just record an element in Vcombine_after_change_calls_list.  */
        public static LispObject combine_after_change_calls
        {
            get { return Defs.O[(int)Objects.combine_after_change_calls]; }
            set { Defs.O[(int)Objects.combine_after_change_calls] = value; }
        }
    }

    public partial class Q
    {
        public static LispObject inhibit_modification_hooks;
    }

    public partial class L
    {
        /* List of elements of the form (BEG-UNCHANGED END-UNCHANGED CHANGE-AMOUNT)
           describing changes which happened while combine_after_change_calls
           was nonzero.  We use this to decide how to call them
           once the deferral ends.

           In each element.
           BEG-UNCHANGED is the number of chars before the changed range.
           END-UNCHANGED is the number of chars after the changed range,
           and CHANGE-AMOUNT is the number of characters inserted by the change
           (negative for a deletion).  */
        public static LispObject combine_after_change_list;

        /* Buffer which combine_after_change_list is about.  */
        public static LispObject combine_after_change_buffer;

        /* Check all markers in the current buffer, looking for something invalid.  */
        public static bool check_markers_debug_flag
        {
            get { return Defs.B[(int)Bools.check_markers_debug_flag]; }
            set { Defs.B[(int)Bools.check_markers_debug_flag] = value; }
        }

        /* If nonzero, all modification hooks are suppressed.  */
        public static bool inhibit_modification_hooks
        {
            get { return Defs.B[(int)Bools.inhibit_modification_hooks]; }
            set { Defs.B[(int)Bools.inhibit_modification_hooks] = value; }
        }

        public static void CHECK_MARKERS()
        {
            if (check_markers_debug_flag)
                check_markers();
        }

        public static void check_markers()
        {
            LispMarker tail;
            bool multibyte = !NILP(current_buffer.enable_multibyte_characters);

            for (tail = BUF_MARKERS(current_buffer); tail != null; tail = tail.next)
            {
                if (tail.buffer.text != current_buffer.text)
                    abort();
                if (tail.charpos > Z)
                    abort();
                if (tail.bytepos > Z_BYTE)
                    abort();
                if (multibyte && !CHAR_HEAD_P(FETCH_BYTE(tail.bytepos)))
                    abort();
            }
        }

        /* Move gap to position CHARPOS.
           Note that this can quit!  */
        public static void move_gap(int charpos)
        {
            move_gap_both(charpos, charpos_to_bytepos(charpos));
        }

        /* Move gap to byte position BYTEPOS, which is also char position CHARPOS.
           Note that this can quit!  */
        public static void move_gap_both(int charpos, int bytepos)
        {
            if (bytepos < GPT_BYTE)
                gap_left(charpos, bytepos, false);
            else if (bytepos > GPT_BYTE)
                gap_right(charpos, bytepos);
        }

        /* Move the gap to a position less than the current GPT.
           BYTEPOS describes the new position as a byte position,
           and CHARPOS is the corresponding char position.
           If NEWGAP is nonzero, then don't update beg_unchanged and end_unchanged.  */
        public static void gap_left(int charpos, int bytepos, bool newgap)
        {
            PtrEmulator<byte> to, from;
            int i;
            int new_s1;

            if (!newgap)
                BUF_COMPUTE_UNCHANGED(current_buffer, charpos, GPT);

            i = GPT_BYTE;
            to = GAP_END_ADDR();
            from = GPT_ADDR();
            new_s1 = GPT_BYTE;

            /* Now copy the characters.  To move the gap down,
               copy characters up.  */

            while (true)
            {
                /* I gets number of characters left to copy.  */
                i = new_s1 - bytepos;
                if (i == 0)
                    break;
                /* If a quit is requested, stop copying now.
               Change BYTEPOS to be where we have actually moved the gap to.  */
                if (QUITP())
                {
                    bytepos = new_s1;
                    charpos = BYTE_TO_CHAR(bytepos);
                    break;
                }
                /* Move at most 32000 chars before checking again for a quit.  */
                if (i > 32000)
                    i = 32000;
#if GAP_USE_BCOPY
      if (i >= 128
	  /* bcopy is safe if the two areas of memory do not overlap
	     or on systems where bcopy is always safe for moving upward.  */
	  && (BCOPY_UPWARD_SAFE
	      || to - from >= 128))
	{
	  /* If overlap is not safe, avoid it by not moving too many
	     characters at once.  */
	  if (!BCOPY_UPWARD_SAFE && i > to - from)
	    i = to - from;
	  new_s1 -= i;
	  from -= i, to -= i;
	  bcopy (from, to, i);
	}
      else
#endif
                {
                    new_s1 -= i;
                    while (--i >= 0)
                    {
                        to--; from--;
                        to.Value = from.Value;
                    }
                }
            }

            /* Adjust markers, and buffer data structure, to put the gap at BYTEPOS.
               BYTEPOS is where the loop above stopped, which may be what was specified
               or may be where a quit was detected.  */
            adjust_markers_gap_motion(bytepos, GPT_BYTE, GAP_SIZE);
            current_buffer.text.gpt_byte = bytepos;
            current_buffer.text.gpt = charpos;
            if (bytepos < charpos)
                abort();
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> ptr = GPT_ADDR();
                ptr.Value = 0; /* Put an anchor.  */
            }
            QUIT();
        }

        /* Move the gap to a position greater than than the current GPT.
           BYTEPOS describes the new position as a byte position,
           and CHARPOS is the corresponding char position.  */
        public static void gap_right(int charpos, int bytepos)
        {
            PtrEmulator<byte> to, from;
            int i;
            int new_s1;

            BUF_COMPUTE_UNCHANGED(current_buffer, charpos, GPT);

            i = GPT_BYTE;
            from = GAP_END_ADDR();
            to = GPT_ADDR();
            new_s1 = GPT_BYTE;

            /* Now copy the characters.  To move the gap up,
               copy characters down.  */

            while (true)
            {
                /* I gets number of characters left to copy.  */
                i = bytepos - new_s1;
                if (i == 0)
                    break;
                /* If a quit is requested, stop copying now.
               Change BYTEPOS to be where we have actually moved the gap to.  */
                if (QUITP())
                {
                    bytepos = new_s1;
                    charpos = BYTE_TO_CHAR(bytepos);
                    break;
                }
                /* Move at most 32000 chars before checking again for a quit.  */
                if (i > 32000)
                    i = 32000;
#if GAP_USE_BCOPY
      if (i >= 128
	  /* bcopy is safe if the two areas of memory do not overlap
	     or on systems where bcopy is always safe for moving downward.  */
	  && (BCOPY_DOWNWARD_SAFE
	      || from - to >= 128))
	{
	  /* If overlap is not safe, avoid it by not moving too many
	     characters at once.  */
	  if (!BCOPY_DOWNWARD_SAFE && i > from - to)
	    i = from - to;
	  new_s1 += i;
	  bcopy (from, to, i);
	  from += i, to += i;
	}
      else
#endif
                {
                    new_s1 += i;
                    while (--i >= 0)
                    {
                        to.Value = from.Value;
                        to++; from++;
                    }
                }
            }

            adjust_markers_gap_motion(GPT_BYTE + GAP_SIZE, bytepos + GAP_SIZE,
                           -GAP_SIZE);
            current_buffer.text.gpt = charpos;
            current_buffer.text.gpt_byte = bytepos;
            if (bytepos < charpos)
                abort();
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> ptr = GPT_ADDR();
                ptr.Value = 0; /* Put an anchor.  */
            }
            QUIT();
        }

        /* Add AMOUNT to the byte position of every marker in the current buffer
           whose current byte position is between FROM (exclusive) and TO (inclusive).

           Also, any markers past the outside of that interval, in the direction
           of adjustment, are first moved back to the near end of the interval
           and then adjusted by AMOUNT.

           When the latter adjustment is done, if AMOUNT is negative,
           we record the adjustment for undo.  (This case happens only for
           deletion.)

           The markers' character positions are not altered,
           because gap motion does not affect character positions.  */

        public static int adjust_markers_test;

        public static void adjust_markers_gap_motion(int from, int to, int amount)
        {
            /* Now that a marker has a bytepos, not counting the gap,
               nothing needs to be done here.  */
#if NOT_IMPLEMENTED_IN_GNU0
  Lisp_Object marker;
  register struct Lisp_Marker *m;
  register EMACS_INT mpos;

  marker = BUF_MARKERS (current_buffer);

  while (!NILP (marker))
    {
      m = XMARKER (marker);
      mpos = m->bytepos;
      if (amount > 0)
	{
	  if (mpos > to && mpos < to + amount)
	    {
	      if (adjust_markers_test)
		abort ();
	      mpos = to + amount;
	    }
	}
      else
	{
	  /* Here's the case where a marker is inside text being deleted.
	     AMOUNT can be negative for gap motion, too,
	     but then this range contains no markers.  */
	  if (mpos > from + amount && mpos <= from)
	    {
	      if (adjust_markers_test)
		abort ();
	      mpos = from + amount;
	    }
	}
      if (mpos > from && mpos <= to)
	mpos += amount;
      m->bufpos = mpos;
      marker = m->chain;
    }
#endif
        }

        public static int copy_text(byte[] from_addr, PtrEmulator<byte> to,
                                     int nbytes, bool from_multibyte, bool to_multibyte)
        {
            return copy_text(from_addr, 0, to, nbytes, from_multibyte, to_multibyte);
        }

        /* Copy NBYTES bytes of text from FROM_ADDR to TO_ADDR.
           FROM_MULTIBYTE says whether the incoming text is multibyte.
           TO_MULTIBYTE says whether to store the text as multibyte.
           If FROM_MULTIBYTE != TO_MULTIBYTE, we convert.

           Return the number of bytes stored at TO_ADDR.  */
        public static int copy_text(byte[] from_addr, int from_index, PtrEmulator<byte> to,
                                     int nbytes, bool from_multibyte, bool to_multibyte)
        {
            if (from_multibyte == to_multibyte)
            {
                System.Array.Copy(from_addr, from_index, to.Collection, to.Index, nbytes);
                return nbytes;
            }
            else if (from_multibyte)
            {
                int nchars = 0;
                int bytes_left = nbytes;
                LispObject tbl = Q.nil;

                while (bytes_left > 0)
                {
                    int thislen = 0;
                    uint c = STRING_CHAR_AND_LENGTH(from_addr, from_index, bytes_left, ref thislen);
                    if (!ASCII_CHAR_P(c))
                        c &= 0xFF;
                    to.Value = (byte)c;
                    to++;
                    from_index += thislen;
                    bytes_left -= thislen;
                    nchars++;
                }
                return nchars;
            }
            else
            {
                PtrEmulator<byte> initial_to_addr = to;

                /* Convert single-byte to multibyte.  */
                while (nbytes > 0)
                {
                    uint c = from_addr[from_index++];

                    if (c >= 128)
                    {
                        c = unibyte_char_to_multibyte(c);
                        to += CHAR_STRING(c, to.Collection, to.Index);
                        nbytes--;
                    }
                    else
                    {
                        /* Special case for speed.  */
                        to.Value = (byte)c;
                        to++;
                        nbytes--;
                    }
                }
                return to - initial_to_addr;
            }
        }

        public static int count_size_as_multibyte(byte[] ptr, int nbytes)
        {
            return count_size_as_multibyte(ptr, 0, nbytes);
        }

        /* Return the number of bytes it would take
           to convert some single-byte text to multibyte.
           The single-byte text consists of NBYTES bytes at PTR.  */
        public static int count_size_as_multibyte(byte[] ptr, int idx, int nbytes)
        {
            int i;
            int outgoing_nbytes = 0;

            int p = idx;
            for (i = 0; i < nbytes; i++)
            {
                uint c = ptr[p++];

                if (c < 128)
                    outgoing_nbytes++;
                else
                {
                    c = unibyte_char_to_multibyte(c);
                    outgoing_nbytes += CHAR_BYTES(c);
                }
            }

            return outgoing_nbytes;
        }

        /* Check that it is okay to modify the buffer between START and END,
           which are char positions.

           Run the before-change-function, if any.  If intervals are in use,
           verify that the text to be modified is not read-only, and call
           any modification properties the text may have.

           If PRESERVE_PTR is nonzero, we relocate *PRESERVE_PTR
           by holding its value temporarily in a marker.  */
        public static void prepare_to_modify_buffer(int start, int end, bool preserve, ref int preserve_ptr)
        {
            Buffer base_buffer;

            if (!NILP(current_buffer.read_only))
                F.barf_if_buffer_read_only();

            /* Let redisplay consider other windows than selected_window
               if modifying another buffer.  */
            if (XBUFFER(XWINDOW(selected_window).buffer) != current_buffer)
                ++windows_or_buffers_changed;

            if (BUF_INTERVALS(current_buffer) != null)
            {
                if (preserve)
                {
                    LispObject preserve_marker;
                    preserve_marker = F.copy_marker(make_number(preserve_ptr), Q.nil);
                    verify_interval_modification(current_buffer, start, end);
                    preserve_ptr = marker_position(preserve_marker);
                    unchain_marker(XMARKER(preserve_marker));
                }
                else
                    verify_interval_modification(current_buffer, start, end);
            }

            /* For indirect buffers, use the base buffer to check clashes.  */
            if (current_buffer.base_buffer != null)
                base_buffer = current_buffer.base_buffer;
            else
                base_buffer = current_buffer;

#if CLASH_DETECTION
  if (!NILP (base_buffer->file_truename)
      /* Make binding buffer-file-name to nil effective.  */
      && !NILP (base_buffer->filename)
      && SAVE_MODIFF >= MODIFF)
    lock_file (base_buffer->file_truename);
#else
            /* At least warn if this file has changed on disk since it was visited.  */
            if (!NILP(base_buffer.filename)
                && SAVE_MODIFF() >= MODIFF
                && NILP(F.verify_visited_file_modtime(F.current_buffer()))
                && !NILP(F.file_exists_p(base_buffer.filename)))
                call1(intern("ask-user-about-supersession-threat"),
                   base_buffer.filename);
#endif // not CLASH_DETECTION

            signal_before_change(start, end, preserve, ref preserve_ptr);

            if (current_buffer.newline_cache != null)
                invalidate_region_cache(current_buffer,
                                         current_buffer.newline_cache,
                                         start - BEG, Z - end);
            if (current_buffer.width_run_cache != null)
                invalidate_region_cache(current_buffer,
                                         current_buffer.width_run_cache,
                                         start - BEG, Z - end);

            V.deactivate_mark = Q.t;
        }

        /* Delete characters in current buffer
           from FROM up to (but not including) TO.
           If TO comes before FROM, we delete nothing.  */
        public static void del_range(int from, int to)
        {
            del_range_1(from, to, true, false);
        }

        /* Like del_range; PREPARE says whether to call prepare_to_modify_buffer.
           RET_STRING says to return the deleted text. */
        public static LispObject del_range_1(int from, int to, bool prepare, bool ret_string)
        {
            int from_byte, to_byte;
            LispObject deletion;

            /* Make args be valid */
            if (from < BEGV())
                from = BEGV();
            if (to > ZV)
                to = ZV;

            if (to <= from)
                return Q.nil;

            if (prepare)
            {
                int range_length = to - from;
                prepare_to_modify_buffer(from, to, true, ref from);
                to = System.Math.Min(ZV, from + range_length);
            }

            from_byte = CHAR_TO_BYTE(from);
            to_byte = CHAR_TO_BYTE(to);

            deletion = del_range_2(from, from_byte, to, to_byte, ret_string);
            signal_after_change(from, to - from, 0);
            update_compositions(from, from, CHECK_HEAD);
            return deletion;
        }

        /* Adjust point for an insertion of NBYTES bytes, which are NCHARS characters.

           This is used only when the value of point changes due to an insert
           or delete; it does not represent a conceptual change in point as a
           marker.  In particular, point is not crossing any interval
           boundaries, so there's no need to use the usual SET_PT macro.  In
           fact it would be incorrect to do so, because either the old or the
           new value of point is out of sync with the current set of
           intervals.  */
        public static void adjust_point(int nchars, int nbytes)
        {
            current_buffer.pt += nchars;
            current_buffer.pt_byte += nbytes;

            /* In a single-byte buffer, the two positions must be equal.  */
            // eassert(PT_BYTE >= PT && PT_BYTE - PT <= ZV_BYTE - ZV);
        }

        /* Adjust markers for a replacement of a text at FROM (FROM_BYTE) of
           length OLD_CHARS (OLD_BYTES) to a new text of length NEW_CHARS
           (NEW_BYTES).  It is assumed that OLD_CHARS > 0, i.e., this is not
           an insertion.  */
        public static void adjust_markers_for_replace(int from, int from_byte,
                int old_chars, int old_bytes,
                int new_chars, int new_bytes)
        {
            LispMarker m;
            int prev_to_byte = from_byte + old_bytes;
            int diff_chars = new_chars - old_chars;
            int diff_bytes = new_bytes - old_bytes;

            for (m = BUF_MARKERS(current_buffer); m != null; m = m.next)
            {
                if (m.bytepos >= prev_to_byte)
                {
                    m.charpos += diff_chars;
                    m.bytepos += diff_bytes;
                }
                else if (m.bytepos > from_byte)
                {
                    m.charpos = from;
                    m.bytepos = from_byte;
                }
            }

            CHECK_MARKERS();
        }

        /* Adjust all markers for a deletion
           whose range in bytes is FROM_BYTE to TO_BYTE.
           The range in charpos is FROM to TO.

           This function assumes that the gap is adjacent to
           or inside of the range being deleted.  */
        public static void adjust_markers_for_delete(int from, int from_byte,
               int to, int to_byte)
        {
            LispObject marker;
            LispMarker m;
            int charpos;

            for (m = BUF_MARKERS(current_buffer); m != null; m = m.next)
            {
                charpos = m.charpos;

                if (charpos > Z)
                    abort();

                /* If the marker is after the deletion,
               relocate by number of chars / bytes deleted.  */
                if (charpos > to)
                {
                    m.charpos -= to - from;
                    m.bytepos -= to_byte - from_byte;
                }
                /* Here's the case where a marker is inside text being deleted.  */
                else if (charpos > from)
                {
                    if (!m.insertion_type)
                    { /* Normal markers will end up at the beginning of the
	       re-inserted text after undoing a deletion, and must be
	       adjusted to move them to the correct place.  */
                        marker = m;
                        record_marker_adjustment(marker, from - charpos);
                    }
                    else if (charpos < to)
                    { /* Before-insertion markers will automatically move forward
	       upon re-inserting the deleted text, so we have to arrange
	       for them to move backward to the correct position.  */
                        marker = m;
                        record_marker_adjustment(marker, charpos - to);
                    }
                    m.charpos = from;
                    m.bytepos = from_byte;
                }
                /* Here's the case where a before-insertion marker is immediately
               before the deleted region.  */
                else if (charpos == from && m.insertion_type)
                {
                    /* Undoing the change uses normal insertion, which will
                       incorrectly make MARKER move forward, so we arrange for it
                       to then move backward to the correct place at the beginning
                       of the deleted region.  */
                    marker = m;
                    record_marker_adjustment(marker, to - from);
                }
            }
        }

        /* Delete a range of text, specified both as character positions
           and byte positions.  FROM and TO are character positions,
           while FROM_BYTE and TO_BYTE are byte positions.
           If RET_STRING is true, the deleted area is returned as a string. */
        public static LispObject del_range_2(int from, int from_byte,
                                             int to, int to_byte, bool ret_string)
        {
            int nbytes_del, nchars_del;
            LispObject deletion;

            CHECK_MARKERS();

            nchars_del = to - from;
            nbytes_del = to_byte - from_byte;

            /* Make sure the gap is somewhere in or next to what we are deleting.  */
            if (from > GPT)
                gap_right(from, from_byte);
            if (to < GPT)
                gap_left(to, to_byte, false);

            if (ret_string || !EQ(current_buffer.undo_list, Q.t))
                deletion = make_buffer_string_both(from, from_byte, to, to_byte, 1);
            else
                deletion = Q.nil;

            /* Relocate all markers pointing into the new, larger gap
               to point at the end of the text before the gap.
               Do this before recording the deletion,
               so that undo handles this after reinserting the text.  */
            adjust_markers_for_delete(from, from_byte, to, to_byte);

            if (!EQ(current_buffer.undo_list, Q.t))
                record_delete(from, deletion);

            current_buffer.text.modiff++;
            current_buffer.text.chars_modiff = current_buffer.text.modiff;

            /* Relocate point as if it were a marker.  */
            if (from < PT())
                adjust_point(from - (PT() < to ? PT() : to),
                      from_byte - (PT_BYTE() < to_byte ? PT_BYTE() : to_byte));

            offset_intervals(current_buffer, from, -nchars_del);

            /* Adjust the overlay center as needed.  This must be done after
               adjusting the markers that bound the overlays.  */
            adjust_overlays_for_delete(from, nchars_del);

            current_buffer.text.gap_size += nbytes_del;
            current_buffer.zv_byte -= nbytes_del;
            current_buffer.text.z_byte -= nbytes_del;
            current_buffer.zv -= nchars_del;
            current_buffer.text.z -= nchars_del;
            current_buffer.text.gpt = from;
            current_buffer.text.gpt_byte = from_byte;
            if (GAP_SIZE > 0 && current_buffer.text.inhibit_shrinking == 0)
                /* Put an anchor, unless called from decode_coding_object which
                   needs to access the previous gap contents.  */
                current_buffer.text.beg[current_buffer.text.gpt_byte - BEG_BYTE()] = 0;

            if (GPT_BYTE < GPT)
                abort();

            if (GPT - BEG < BEG_UNCHANGED)
                current_buffer.text.beg_unchanged = GPT - BEG;
            if (Z - GPT < END_UNCHANGED)
                current_buffer.text.end_unchanged = Z - GPT;

            CHECK_MARKERS();

            evaporate_overlays(from);

            return deletion;
        }

        /* Set a variable to nil if an error occurred.
           Don't change the variable if there was no error.
           VAL is a cons-cell (VARIABLE . NO-ERROR-FLAG).
           VARIABLE is the variable to maybe set to nil.
           NO-ERROR-FLAG is nil if there was an error,
           anything else meaning no error (so this function does nothing).  */
        public static LispObject reset_var_on_error(LispObject val)
        {
            if (NILP(XCDR(val)))
                F.set(XCAR(val), Q.nil);
            return Q.nil;
        }

        /* These macros work with an argument named `preserve_ptr'
           and a local variable named `preserve_marker'.  */
        public static void PRESERVE_VALUE(bool preserve, int preserve_ptr, ref LispObject preserve_marker)
        {
            if (preserve && NILP(preserve_marker))
                preserve_marker = F.copy_marker(make_number(preserve_ptr), Q.nil);
        }

        public static void RESTORE_VALUE(ref int preserve_ptr, LispObject preserve_marker)
        {
            if (!NILP(preserve_marker))
            {
                preserve_ptr = marker_position(preserve_marker);
                unchain_marker(XMARKER(preserve_marker));
            }
        }

        public static void PRESERVE_START_END(ref LispObject start_marker, ref LispObject end_marker,
                                              LispObject start, LispObject end)
        {
            if (NILP(start_marker))
                start_marker = F.copy_marker(start, Q.nil);
            if (NILP(end_marker))
                end_marker = F.copy_marker(end, Q.nil);
        }

        public static LispObject FETCH_START(LispObject start_marker, LispObject start)
        {
            return (!NILP(start_marker) ? F.marker_position(start_marker) : start);
        }

        public static LispObject FETCH_END(LispObject end_marker, LispObject end)
        {
            return (!NILP(end_marker) ? F.marker_position(end_marker) : end);
        }

        /* Signal a change to the buffer immediately before it happens.
           START_INT and END_INT are the bounds of the text to be changed.

           If PRESERVE_PTR is nonzero, we relocate *PRESERVE_PTR
           by holding its value temporarily in a marker.  */
        public static void signal_before_change(int start_int, int end_int,
                                                bool preserve,
                                                ref int preserve_ptr)
        {
            LispObject start, end;
            LispObject start_marker, end_marker;
            LispObject preserve_marker;
            int count = SPECPDL_INDEX();

            if (inhibit_modification_hooks)
                return;

            start = make_number(start_int);
            end = make_number(end_int);
            preserve_marker = Q.nil;
            start_marker = Q.nil;
            end_marker = Q.nil;

            specbind(Q.inhibit_modification_hooks, Q.t);

            /* If buffer is unmodified, run a special hook for that case.  */
            if (SAVE_MODIFF() >= MODIFF
                && !NILP(V.first_change_hook)
                && !NILP(V.run_hooks))
            {
                PRESERVE_VALUE(preserve, preserve_ptr, ref preserve_marker);
                PRESERVE_START_END(ref start_marker, ref end_marker, start, end);
                call1(V.run_hooks, Q.first_change_hook);
            }

            /* Now run the before-change-functions if any.  */
            if (!NILP(V.before_change_functions))
            {
                LispObject[] args = new LispObject[3];
                LispObject rvoe_arg = F.cons(Q.before_change_functions, Q.nil);

                PRESERVE_VALUE(preserve, preserve_ptr, ref preserve_marker);
                PRESERVE_START_END(ref start_marker, ref end_marker, start, end);

                /* Mark before-change-functions to be reset to nil in case of error.  */
                record_unwind_protect(reset_var_on_error, rvoe_arg);

                /* Actually run the hook functions.  */
                args[0] = Q.before_change_functions;
                args[1] = FETCH_START(start_marker, start);
                args[2] = FETCH_END(end_marker, end);
                F.run_hook_with_args(3, args);

                /* There was no error: unarm the reset_on_error.  */
                XSETCDR(rvoe_arg, Q.t);
            }

            if (current_buffer.overlays_before != null || current_buffer.overlays_after != null)
            {
                PRESERVE_VALUE(preserve, preserve_ptr, ref preserve_marker);
                report_overlay_modification(FETCH_START(start_marker, start), FETCH_END(end_marker, end), false,
                             FETCH_START(start_marker, start), FETCH_END(end_marker, end), Q.nil);
            }

            if (!NILP(start_marker))
                free_marker(start_marker);
            if (!NILP(end_marker))
                free_marker(end_marker);
            RESTORE_VALUE(ref preserve_ptr, preserve_marker);

            unbind_to(count, Q.nil);
        }

        /* Signal a change immediately after it happens.
           CHARPOS is the character position of the start of the changed text.
           LENDEL is the number of characters of the text before the change.
           (Not the whole buffer; just the part that was changed.)
           LENINS is the number of characters in that part of the text
           after the change.  */
        public static void signal_after_change(int charpos, int lendel, int lenins)
        {
            int count = SPECPDL_INDEX();
            if (inhibit_modification_hooks)
                return;

            /* If we are deferring calls to the after-change functions
               and there are no before-change functions,
               just record the args that we were going to use.  */
            if (!NILP(V.combine_after_change_calls)
                && NILP(V.before_change_functions)
                && current_buffer.overlays_before == null
                && current_buffer.overlays_after == null)
            {
                LispObject elt;

                if (!NILP(combine_after_change_list)
                && current_buffer != XBUFFER(combine_after_change_buffer))
                    F.combine_after_change_execute();

                elt = F.cons(make_number(charpos - BEG),
                     F.cons(make_number(Z - (charpos - lendel + lenins)),
                        F.cons(make_number(lenins - lendel), Q.nil)));
                combine_after_change_list
              = F.cons(elt, combine_after_change_list);
                combine_after_change_buffer = F.current_buffer();

                return;
            }

            if (!NILP(combine_after_change_list))
                F.combine_after_change_execute();

            specbind(Q.inhibit_modification_hooks, Q.t);

            if (!NILP(V.after_change_functions))
            {
                LispObject[] args = new LispObject[4];
                LispObject rvoe_arg = F.cons(Q.after_change_functions, Q.nil);

                /* Mark after-change-functions to be reset to nil in case of error.  */
                record_unwind_protect(reset_var_on_error, rvoe_arg);

                /* Actually run the hook functions.  */
                args[0] = Q.after_change_functions;
                args[1] = XSETINT(charpos);
                args[2] = XSETINT(charpos + lenins);
                args[3] = XSETINT(lendel);
                F.run_hook_with_args(4, args);

                /* There was no error: unarm the reset_on_error.  */
                XSETCDR(rvoe_arg, Q.t);
            }

            if (current_buffer.overlays_before != null || current_buffer.overlays_after != null)
                report_overlay_modification(make_number(charpos),
                             make_number(charpos + lenins),
                             true,
                             make_number(charpos),
                             make_number(charpos + lenins),
                             make_number(lendel));

            /* After an insertion, call the text properties
               insert-behind-hooks or insert-in-front-hooks.  */
            if (lendel == 0)
                report_interval_modification(make_number(charpos),
                              make_number(charpos + lenins));

            unbind_to(count, Q.nil);
        }

        /* Call this if you're about to change the region of BUFFER from
           character positions START to END.  This checks the read-only
           properties of the region, calls the necessary modification hooks,
           and warns the next redisplay that it should pay attention to that
           area.

           If PRESERVE_CHARS_MODIFF is non-zero, do not update CHARS_MODIFF.
           Otherwise set CHARS_MODIFF to the new value of MODIFF.  */
        public static void modify_region(Buffer buffer, int start, int end, bool preserve_chars_modiff)
        {
            Buffer old_buffer = current_buffer;

            if (buffer != old_buffer)
                set_buffer_internal(buffer);

            int dummy = 0;
            prepare_to_modify_buffer(start, end, false, ref dummy);

            BUF_COMPUTE_UNCHANGED(buffer, start - 1, end);

            if (MODIFF <= SAVE_MODIFF())
                record_first_change();

            current_buffer.text.modiff++;

            if (!preserve_chars_modiff)
                current_buffer.text.chars_modiff = MODIFF;

            buffer.point_before_scroll = Q.nil;

            if (buffer != old_buffer)
                set_buffer_internal(old_buffer);
        }

        /* Make the gap NBYTES_ADDED bytes longer.  */
        public static void make_gap_larger(int nbytes_added)
        {
            LispObject tem;
            int real_gap_loc;
            int real_gap_loc_byte;
            int old_gap_size;

            /* If we have to get more space, get enough to last a while.  */
            nbytes_added += 2000;

            /* Don't allow a buffer size that won't fit in an int
               even if it will fit in a Lisp integer.
               That won't work because so many places use `int'.

               Make sure we don't introduce overflows in the calculation.  */

            if (Z_BYTE - BEG_BYTE() + GAP_SIZE
                >= ((1 << (31 - 1)) - 1 - nbytes_added))
                error("Buffer exceeds maximum size");

            enlarge_buffer_text(current_buffer, nbytes_added);

            /* Prevent quitting in move_gap.  */
            tem = V.inhibit_quit;
            V.inhibit_quit = Q.t;

            real_gap_loc = GPT;
            real_gap_loc_byte = GPT_BYTE;
            old_gap_size = GAP_SIZE;

            /* Call the newly allocated space a gap at the end of the whole space.  */
            current_buffer.text.gpt = Z + GAP_SIZE;
            current_buffer.text.gpt_byte = Z_BYTE + GAP_SIZE;
            current_buffer.text.gap_size = nbytes_added;

            /* Move the new gap down to be consecutive with the end of the old one.
               This adjusts the markers properly too.  */
            gap_left(real_gap_loc + old_gap_size, real_gap_loc_byte + old_gap_size, true);

            /* Now combine the two into one large gap.  */
            current_buffer.text.gap_size += old_gap_size;
            current_buffer.text.gpt = real_gap_loc;
            current_buffer.text.gpt_byte = real_gap_loc_byte;

            /* Put an anchor.  */
            // *(Z_ADDR) = 0;

            V.inhibit_quit = tem;
        }


        public static void make_gap(int nbytes_added)
        {
            if (nbytes_added >= 0)
                make_gap_larger(nbytes_added);
        }

        /* Insert a sequence of NCHARS chars which occupy NBYTES bytes
           starting at STRING.  INHERIT, PREPARE and BEFORE_MARKERS
           are the same as in insert_1.  */
        public static void insert_1_both(byte[] stringg,
           int nchars, int nbytes,
           int inherit, int prepare, int before_markers)
        {
            if (nchars == 0)
                return;

            if (NILP(current_buffer.enable_multibyte_characters))
                nchars = nbytes;

            if (prepare != 0)
            {
                /* Do this before moving and increasing the gap,
                   because the before-change hooks might move the gap
                   or make it smaller.  */
                int dummy = 0;
                prepare_to_modify_buffer(PT(), PT(), false, ref dummy);
            }

            if (PT() != GPT)
                move_gap_both(PT(), PT_BYTE());
            if (GAP_SIZE < nbytes)
                make_gap(nbytes - GAP_SIZE);

#if BYTE_COMBINING_DEBUG
  if (count_combining_before (string, nbytes, PT, PT_BYTE)
      || count_combining_after (string, nbytes, PT, PT_BYTE))
    abort ();
#endif

            /* Record deletion of the surrounding text that combines with
     the insertion.  This, together with recording the insertion,
     will add up to the right stuff in the undo list.  */
            record_insert(PT(), nchars);
            current_buffer.text.modiff++;
            current_buffer.text.chars_modiff = MODIFF;

            System.Array.Copy(stringg, 0, current_buffer.text.beg, current_buffer.text.gpt_byte - BEG_BYTE(), nbytes);

            current_buffer.text.gap_size -= nbytes;
            current_buffer.text.gpt += nchars;
            current_buffer.zv += nchars;
            current_buffer.text.z += nchars;
            current_buffer.text.gpt_byte += nbytes;
            current_buffer.zv_byte += nbytes;
            current_buffer.text.z_byte += nbytes;
            if (GAP_SIZE > 0)
            {
                // *(GPT_ADDR) = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            /* The insert may have been in the unchanged region, so check again. */
            if (Z - GPT < END_UNCHANGED)
                current_buffer.text.end_unchanged = Z - GPT;

            adjust_overlays_for_insert(PT(), nchars);
            adjust_markers_for_insert(PT(), PT_BYTE(),
                           PT() + nchars, PT_BYTE() + nbytes,
                           before_markers);

            if (BUF_INTERVALS(current_buffer) != null)
                offset_intervals(current_buffer, PT(), nchars);

            if (inherit == 0 && BUF_INTERVALS(current_buffer) != null)
                set_text_properties(make_number(PT()), make_number(PT() + nchars),
                         Q.nil, Q.nil, Q.nil);

            adjust_point(nchars, nbytes);

            CHECK_MARKERS();
        }

        /* Adjust markers for an insertion that stretches from FROM / FROM_BYTE
           to TO / TO_BYTE.  We have to relocate the charpos of every marker
           that points after the insertion (but not their bytepos).

           When a marker points at the insertion point,
           we advance it if either its insertion-type is t
           or BEFORE_MARKERS is true.  */
        public static void adjust_markers_for_insert(int from, int from_byte,
               int to, int to_byte, int before_markers)
        {
            LispMarker m;
            bool adjusted = false;
            int nchars = to - from;
            int nbytes = to_byte - from_byte;

            for (m = BUF_MARKERS(current_buffer); m != null; m = m.next)
            {
                //      eassert (m->bytepos >= m->charpos
                //	       && m->bytepos - m->charpos <= Z_BYTE - Z);

                if (m.bytepos == from_byte)
                {
                    if (m.insertion_type || before_markers != 0)
                    {
                        m.bytepos = to_byte;
                        m.charpos = to;
                        if (m.insertion_type)
                            adjusted = true;
                    }
                }
                else if (m.bytepos > from_byte)
                {
                    m.bytepos += nbytes;
                    m.charpos += nchars;
                }
            }

            /* Adjusting only markers whose insertion-type is t may result in
               - disordered start and end in overlays, and 
               - disordered overlays in the slot `overlays_before' of current_buffer.  */
            if (adjusted)
            {
                fix_start_end_in_overlays(from, to);
                fix_overlays_before(current_buffer, from, to);
            }
        }

        /* Insert a string of specified length before point.
           This function judges multibyteness based on
           enable_multibyte_characters in the current buffer;
           it never converts between single-byte and multibyte.

           DO NOT use this for the contents of a Lisp string or a Lisp buffer!
           prepare_to_modify_buffer could relocate the text.  */
        public static void insert(byte[] stringg, int nbytes)
        {
            if (nbytes > 0)
            {
                int len = chars_in_text(stringg, nbytes), opoint;
                insert_1_both(stringg, len, nbytes, 0, 1, 0);
                opoint = PT() - len;
                signal_after_change(opoint, 0, len);
                update_compositions(opoint, PT(), CHECK_BORDER);
            }
        }

        /* Likewise, but inherit text properties from neighboring characters.  */
        public static void insert_and_inherit(byte[] stringg, int nbytes)
        {
            if (nbytes > 0)
            {
                int len = chars_in_text(stringg, nbytes), opoint;
                insert_1_both(stringg, len, nbytes, 1, 1, 0);
                opoint = PT() - len;
                signal_after_change(opoint, 0, len);
                update_compositions(opoint, PT(), CHECK_BORDER);
            }
        }

        /* Insert the part of the text of STRING, a Lisp object assumed to be
           of type string, consisting of the LENGTH characters (LENGTH_BYTE bytes)
           starting at position POS / POS_BYTE.  If the text of STRING has properties,
           copy them into the buffer.

           It does not work to use `insert' for this, because a GC could happen
           before we bcopy the stuff into the buffer, and relocate the string
           without insert noticing.  */
        public static void insert_from_string(LispObject stringg, int pos, int pos_byte,
            int length, int length_byte, int inherit)
        {
            int opoint = PT();

            if (SCHARS(stringg) == 0)
                return;

            insert_from_string_1(stringg, pos, pos_byte, length, length_byte,
                      inherit, 0);
            signal_after_change(opoint, 0, PT() - opoint);
            update_compositions(opoint, PT(), CHECK_BORDER);
        }

        /* Subroutine of the insertion functions above.  */
        public static void insert_from_string_1(LispObject stringg, int pos, int pos_byte,
              int nchars, int nbytes,
              int inherit, int before_markers)
        {
            int outgoing_nbytes = nbytes;
            Interval intervals;

            /* Make OUTGOING_NBYTES describe the text
               as it will be inserted in this buffer.  */

            if (NILP(current_buffer.enable_multibyte_characters))
                outgoing_nbytes = nchars;
            else if (!STRING_MULTIBYTE(stringg))
                outgoing_nbytes
                  = count_size_as_multibyte(SDATA(stringg), pos_byte,
                             nbytes);

            /* Do this before moving and increasing the gap,
               because the before-change hooks might move the gap
               or make it smaller.  */
            int dummy = 0;
            prepare_to_modify_buffer(PT(), PT(), false, ref dummy);

            if (PT() != GPT)
                move_gap_both(PT(), PT_BYTE());
            if (GAP_SIZE < outgoing_nbytes)
                make_gap(outgoing_nbytes - GAP_SIZE);

            /* Copy the string text into the buffer, perhaps converting
               between single-byte and multibyte.  */
            copy_text(SDATA(stringg), pos_byte, GPT_ADDR(), nbytes,
                   STRING_MULTIBYTE(stringg),
                   !NILP(current_buffer.enable_multibyte_characters));

#if BYTE_COMBINING_DEBUG
  /* We have copied text into the gap, but we have not altered
     PT or PT_BYTE yet.  So we can pass PT and PT_BYTE
     to these functions and get the same results as we would
     have got earlier on.  Meanwhile, PT_ADDR does point to
     the text that has been stored by copy_text.  */
  if (count_combining_before (GPT_ADDR, outgoing_nbytes, PT, PT_BYTE)
      || count_combining_after (GPT_ADDR, outgoing_nbytes, PT, PT_BYTE))
    abort ();
#endif

            record_insert(PT(), nchars);
            current_buffer.text.modiff++;
            current_buffer.text.chars_modiff = MODIFF;

            current_buffer.text.gap_size -= outgoing_nbytes;
            current_buffer.text.gpt += nchars;
            current_buffer.zv += nchars;
            current_buffer.text.z += nchars;
            current_buffer.text.gpt_byte += outgoing_nbytes;
            current_buffer.zv_byte += outgoing_nbytes;
            current_buffer.text.z_byte += outgoing_nbytes;
            if (GAP_SIZE > 0)
            {
                // *(GPT_ADDR) = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            /* The insert may have been in the unchanged region, so check again. */
            if (Z - GPT < END_UNCHANGED)
                current_buffer.text.end_unchanged = Z - GPT;

            adjust_overlays_for_insert(PT(), nchars);
            adjust_markers_for_insert(PT(), PT_BYTE(), PT() + nchars,
                           PT_BYTE() + outgoing_nbytes,
                           before_markers);

            offset_intervals(current_buffer, PT(), nchars);

            intervals = STRING_INTERVALS(stringg);
            /* Get the intervals for the part of the string we are inserting.  */
            if (nbytes < SBYTES(stringg))
                intervals = copy_intervals(intervals, pos, nchars);

            /* Insert those intervals.  */
            graft_intervals_into_buffer(intervals, PT(), nchars,
                             current_buffer, inherit);

            adjust_point(nchars, outgoing_nbytes);

            CHECK_MARKERS();
        }

        /* Replace the text from character positions FROM to TO with NEW,
           If PREPARE is nonzero, call prepare_to_modify_buffer.
           If INHERIT, the newly inserted text should inherit text properties
           from the surrounding non-deleted text.  */

        /* Note that this does not yet handle markers quite right.
           Also it needs to record a single undo-entry that does a replacement
           rather than a separate delete and insert.
           That way, undo will also handle markers properly.

           But if MARKERS is 0, don't relocate markers.  */
        public static void replace_range(int from, int to, LispObject neww, bool prepare, int inherit, bool markers)
        {
            int inschars = SCHARS(neww);
            int insbytes = SBYTES(neww);
            int from_byte, to_byte;
            int nbytes_del, nchars_del;
            LispObject temp;
            Interval intervals;
            int outgoing_insbytes = insbytes;
            LispObject deletion;

            CHECK_MARKERS();

            deletion = Q.nil;

            if (prepare)
            {
                int range_length = to - from;
                prepare_to_modify_buffer(from, to, true, ref from);
                to = from + range_length;
            }

            /* Make args be valid */
            if (from < BEGV())
                from = BEGV();
            if (to > ZV)
                to = ZV;

            from_byte = CHAR_TO_BYTE(from);
            to_byte = CHAR_TO_BYTE(to);

            nchars_del = to - from;
            nbytes_del = to_byte - from_byte;

            if (nbytes_del <= 0 && insbytes == 0)
                return;

            /* Make OUTGOING_INSBYTES describe the text
               as it will be inserted in this buffer.  */

            if (NILP(current_buffer.enable_multibyte_characters))
                outgoing_insbytes = inschars;
            else if (!STRING_MULTIBYTE(neww))
                outgoing_insbytes
                  = count_size_as_multibyte(SDATA(neww), insbytes);

            /* Make sure point-max won't overflow after this insertion.  */
            temp = make_number(Z_BYTE - nbytes_del + insbytes);
            if (Z_BYTE - nbytes_del + insbytes != XINT(temp))
                error("Maximum buffer size exceeded");

            /* Make sure the gap is somewhere in or next to what we are deleting.  */
            if (from > GPT)
                gap_right(from, from_byte);
            if (to < GPT)
                gap_left(to, to_byte, false);

            /* Even if we don't record for undo, we must keep the original text
               because we may have to recover it because of inappropriate byte
               combining.  */
            if (!EQ(current_buffer.undo_list, Q.t))
                deletion = make_buffer_string_both(from, from_byte, to, to_byte, 1);

            GAP_SIZE += nbytes_del;
            ZV -= nchars_del;
            Z -= nchars_del;
            ZV_BYTE -= nbytes_del;
            Z_BYTE -= nbytes_del;
            GPT = from;
            GPT_BYTE = from_byte;
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> tmp = GPT_ADDR();
                tmp.Value = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            if (GPT - BEG < BEG_UNCHANGED)
                BEG_UNCHANGED = GPT - BEG;
            if (Z - GPT < END_UNCHANGED)
                END_UNCHANGED = Z - GPT;

            if (GAP_SIZE < insbytes)
                make_gap(insbytes - GAP_SIZE);

            /* Copy the string text into the buffer, perhaps converting
               between single-byte and multibyte.  */
            copy_text(SDATA(neww), GPT_ADDR(), insbytes,
                   STRING_MULTIBYTE(neww),
                   !NILP(current_buffer.enable_multibyte_characters));

#if BYTE_COMBINING_DEBUG
  /* We have copied text into the gap, but we have not marked
     it as part of the buffer.  So we can use the old FROM and FROM_BYTE
     here, for both the previous text and the following text.
     Meanwhile, GPT_ADDR does point to
     the text that has been stored by copy_text.  */
  if (count_combining_before (GPT_ADDR, outgoing_insbytes, from, from_byte)
      || count_combining_after (GPT_ADDR, outgoing_insbytes, from, from_byte))
    abort ();
#endif

            if (!EQ(current_buffer.undo_list, Q.t))
            {
                /* Record the insertion first, so that when we undo,
               the deletion will be undone first.  Thus, undo
               will insert before deleting, and thus will keep
               the markers before and after this text separate.  */
                record_insert(from + SCHARS(deletion), inschars);
                record_delete(from, deletion);
            }

            GAP_SIZE -= outgoing_insbytes;
            GPT += inschars;
            ZV += inschars;
            Z += inschars;
            GPT_BYTE += outgoing_insbytes;
            ZV_BYTE += outgoing_insbytes;
            Z_BYTE += outgoing_insbytes;
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> tmp = GPT_ADDR();
                tmp.Value = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            /* Adjust the overlay center as needed.  This must be done after
               adjusting the markers that bound the overlays.  */
            adjust_overlays_for_delete(from, nchars_del);
            adjust_overlays_for_insert(from, inschars);

            /* Adjust markers for the deletion and the insertion.  */
            if (markers)
                adjust_markers_for_replace(from, from_byte, nchars_del, nbytes_del,
                            inschars, outgoing_insbytes);

            offset_intervals(current_buffer, from, inschars - nchars_del);

            /* Get the intervals for the part of the string we are inserting--
               not including the combined-before bytes.  */
            intervals = STRING_INTERVALS(neww);
            /* Insert those intervals.  */
            graft_intervals_into_buffer(intervals, from, inschars,
                             current_buffer, inherit);

            /* Relocate point as if it were a marker.  */
            if (from < PT())
                adjust_point((from + inschars - (PT() < to ? PT() : to)),
                      (from_byte + outgoing_insbytes
                       - (PT_BYTE() < to_byte ? PT_BYTE() : to_byte)));

            if (outgoing_insbytes == 0)
                evaporate_overlays(from);

            CHECK_MARKERS();

            MODIFF++;
            CHARS_MODIFF = MODIFF;

            signal_after_change(from, nchars_del, GPT - from);
            update_compositions(from, GPT, CHECK_BORDER);
        }

        /* Replace the text from character positions FROM to TO with
           the text in INS of length INSCHARS.
           Keep the text properties that applied to the old characters
           (extending them to all the new chars if there are more new chars).

           Note that this does not yet handle markers quite right.

           If MARKERS is nonzero, relocate markers.

           Unlike most functions at this level, never call
           prepare_to_modify_buffer and never call signal_after_change.  */
        public static void replace_range_2(int from, int from_byte,
                                           int to, int to_byte,
                                           byte[] ins, int inschars, int insbytes,
                                           bool markers)
        {
            int nbytes_del, nchars_del;
            LispObject temp;

            CHECK_MARKERS();

            nchars_del = to - from;
            nbytes_del = to_byte - from_byte;

            if (nbytes_del <= 0 && insbytes == 0)
                return;

            /* Make sure point-max won't overflow after this insertion.  */
            temp = make_number(Z_BYTE - nbytes_del + insbytes);
            if (Z_BYTE - nbytes_del + insbytes != XINT(temp))
                error("Maximum buffer size exceeded");

            /* Make sure the gap is somewhere in or next to what we are deleting.  */
            if (from > GPT)
                gap_right(from, from_byte);
            if (to < GPT)
                gap_left(to, to_byte, false);

            GAP_SIZE += nbytes_del;
            ZV -= nchars_del;
            Z -= nchars_del;
            ZV_BYTE -= nbytes_del;
            Z_BYTE -= nbytes_del;
            GPT = from;
            GPT_BYTE = from_byte;
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> tmp = GPT_ADDR();
                tmp.Value = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            if (GPT - BEG < BEG_UNCHANGED)
                BEG_UNCHANGED = GPT - BEG;
            if (Z - GPT < END_UNCHANGED)
                END_UNCHANGED = Z - GPT;

            if (GAP_SIZE < insbytes)
                make_gap(insbytes - GAP_SIZE);

            /* Copy the replacement text into the buffer.  */
            PtrEmulator<byte>.bcopy(new PtrEmulator<byte>(ins), GPT_ADDR(), insbytes);

#if BYTE_COMBINING_DEBUG
  /* We have copied text into the gap, but we have not marked
     it as part of the buffer.  So we can use the old FROM and FROM_BYTE
     here, for both the previous text and the following text.
     Meanwhile, GPT_ADDR does point to
     the text that has been stored by copy_text.  */
  if (count_combining_before (GPT_ADDR, insbytes, from, from_byte)
      || count_combining_after (GPT_ADDR, insbytes, from, from_byte))
    abort ();
#endif

            GAP_SIZE -= insbytes;
            GPT += inschars;
            ZV += inschars;
            Z += inschars;
            GPT_BYTE += insbytes;
            ZV_BYTE += insbytes;
            Z_BYTE += insbytes;
            if (GAP_SIZE > 0)
            {
                PtrEmulator<byte> tmp = GPT_ADDR();
                tmp.Value = 0; /* Put an anchor.  */
            }

            if (GPT_BYTE < GPT)
                abort();

            /* Adjust the overlay center as needed.  This must be done after
               adjusting the markers that bound the overlays.  */
            if (nchars_del != inschars)
            {
                adjust_overlays_for_insert(from, inschars);
                adjust_overlays_for_delete(from + inschars, nchars_del);
            }

            /* Adjust markers for the deletion and the insertion.  */
            if (markers
                && !(nchars_del == 1 && inschars == 1 && nbytes_del == insbytes))
                adjust_markers_for_replace(from, from_byte, nchars_del, nbytes_del,
                            inschars, insbytes);

            offset_intervals(current_buffer, from, inschars - nchars_del);

            /* Relocate point as if it were a marker.  */
            if (from < PT() && (nchars_del != inschars || nbytes_del != insbytes))
            {
                if (PT() < to)
                    /* PT was within the deleted text.  Move it to FROM.  */
                    adjust_point(from - PT(), from_byte - PT_BYTE());
                else
                    adjust_point(inschars - nchars_del, insbytes - nbytes_del);
            }

            if (insbytes == 0)
                evaporate_overlays(from);

            CHECK_MARKERS();

            MODIFF++;
            CHARS_MODIFF = MODIFF;
        }
    }

    public partial class F
    {
        public static LispObject combine_after_change_execute_1 (LispObject val)
        {
            V.combine_after_change_calls = val;
            return val;
        }

        public static LispObject combine_after_change_execute()
        {
            int count = L.SPECPDL_INDEX();
            int beg, end, change;
            int begpos, endpos;
            LispObject tail;

            if (L.NILP(L.combine_after_change_list))
                return Q.nil;

            /* It is rare for combine_after_change_buffer to be invalid, but
               possible.  It can happen when combine-after-change-calls is
               non-nil, and insertion calls a file handler (e.g. through
               lock_file) which scribbles into a temp file -- cyd  */
            if (!L.BUFFERP(L.combine_after_change_buffer)
      || L.NILP(L.XBUFFER(L.combine_after_change_buffer).name))
            {
                L.combine_after_change_list = Q.nil;
                return Q.nil;
            }

            L.record_unwind_protect(F.set_buffer, F.current_buffer());

            F.set_buffer(L.combine_after_change_buffer);

            /* # chars unchanged at beginning of buffer.  */
            beg = L.Z - L.BEG;
            /* # chars unchanged at end of buffer.  */
            end = beg;
            /* Total amount of insertion (negative for deletion).  */
            change = 0;

            /* Scan the various individual changes,
               accumulating the range info in BEG, END and CHANGE.  */
            for (tail = L.combine_after_change_list; L.CONSP(tail);
                 tail = L.XCDR(tail))
            {
                LispObject elt;
                int thisbeg, thisend, thischange;

                /* Extract the info from the next element.  */
                elt = L.XCAR(tail);
                if (!L.CONSP(elt))
                    continue;
                thisbeg = L.XINT(L.XCAR(elt));

                elt = L.XCDR(elt);
                if (!L.CONSP(elt))
                    continue;
                thisend = L.XINT(L.XCAR(elt));

                elt = L.XCDR(elt);
                if (!L.CONSP(elt))
                    continue;
                thischange = L.XINT(L.XCAR(elt));

                /* Merge this range into the accumulated range.  */
                change += thischange;
                if (thisbeg < beg)
                    beg = thisbeg;
                if (thisend < end)
                    end = thisend;
            }

            /* Get the current start and end positions of the range
               that was changed.  */
            begpos = L.BEG + beg;
            endpos = L.Z - end;

            /* We are about to handle these, so discard them.  */
            L.combine_after_change_list = Q.nil;

            /* Now run the after-change functions for real.
               Turn off the flag that defers them.  */
            L.record_unwind_protect(F.combine_after_change_execute_1,
                       V.combine_after_change_calls);
            L.signal_after_change(begpos, endpos - begpos - change, endpos - begpos);
            L.update_compositions(begpos, endpos, L.CHECK_ALL);

            return L.unbind_to(count, Q.nil);
        }
    }
}