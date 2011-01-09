namespace IronElisp
{
    public partial class L
    {
        /* Record one cached position found recently by
           buf_charpos_to_bytepos or buf_bytepos_to_charpos.  */
        public static int cached_charpos;
        public static int cached_bytepos;
        public static Buffer cached_buffer;
        public static int cached_modiff;

        /* Nonzero means enable debugging checks on byte/char correspondences.  */

        public static bool byte_debug_flag;

        /* Converting between character positions and byte positions.  */

        /* There are several places in the buffer where we know
           the correspondence: BEG, BEGV, PT, GPT, ZV and Z,
           and everywhere there is a marker.  So we find the one of these places
           that is closest to the specified position, and scan from there.  */

        /* charpos_to_bytepos returns the byte position corresponding to CHARPOS.  */

        /* This macro is a subroutine of charpos_to_bytepos.
           Note that it is desirable that BYTEPOS is not evaluated
           except when we really want its value.  */
        public static int CONSIDER_1(int CHARPOS, int BYTEPOS, Buffer b, int charpos,
                                     ref int best_above, ref int best_above_byte,
                                     ref int best_below, ref int best_below_byte,
                                     out bool did_return)
        {
            int this_charpos = (CHARPOS);
            bool changed = false;
            did_return = false;

            if (this_charpos == charpos)
            {
                int value = (BYTEPOS);
                if (byte_debug_flag)
                    byte_char_debug_check(b, charpos, value);
                did_return = true;
                return value;
            }
            else if (this_charpos > charpos)
            {
                if (this_charpos < best_above)
                {
                    best_above = this_charpos;
                    best_above_byte = (BYTEPOS);
                    changed = true;
                }
            }
            else if (this_charpos > best_below)
            {
                best_below = this_charpos;
                best_below_byte = (BYTEPOS);
                changed = true;
            }

            if (changed)
            {
                if (best_above - best_below == best_above_byte - best_below_byte)
                {
                    int value = best_below_byte + (charpos - best_below);
                    if (byte_debug_flag)
                        byte_char_debug_check(b, charpos, value);
                    did_return = true;
                    return value;
                }
            }
            return 0;
        }

        public static void byte_char_debug_check (Buffer b, int charpos, int bytepos)
        {
            int nchars = 0;

            if (bytepos > BUF_GPT_BYTE(b))
            {
                nchars = multibyte_chars_in_text(BUF_BEG_ADDR(b), 0,
                              BUF_GPT_BYTE(b) - BUF_BEG_BYTE(b));
                nchars += multibyte_chars_in_text(BUF_BEG_ADDR(b), BUF_GAP_END_ADDR(b),
                               bytepos - BUF_GPT_BYTE(b));
            }
            else
                nchars = multibyte_chars_in_text(BUF_BEG_ADDR(b), 0,
                                  bytepos - BUF_BEG_BYTE(b));

            if (charpos - 1 != nchars)
                abort();
        }

        public static int charpos_to_bytepos (int charpos)
        {
            return buf_charpos_to_bytepos(current_buffer, charpos);
        }

        public static int buf_charpos_to_bytepos(Buffer b, int charpos)
        {
            LispMarker tail;
            int best_above, best_above_byte;
            int best_below, best_below_byte;

            if (charpos < BUF_BEG(b) || charpos > BUF_Z(b))
                abort();

            best_above = BUF_Z(b);
            best_above_byte = BUF_Z_BYTE(b);

            /* If this buffer has as many characters as bytes,
               each character must be one byte.
               This takes care of the case where enable-multibyte-characters is nil.  */
            if (best_above == best_above_byte)
                return charpos;

            best_below = BEG;
            best_below_byte = BEG_BYTE();

            /* We find in best_above and best_above_byte
               the closest known point above CHARPOS,
               and in best_below and best_below_byte
               the closest known point below CHARPOS,

               If at any point we can tell that the space between those
               two best approximations is all single-byte,
               we interpolate the result immediately.  */
            bool did_return;
            int value = CONSIDER_1(BUF_PT(b), BUF_PT_BYTE(b),
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
            if (did_return) return value;
            value = CONSIDER_1(BUF_GPT(b), BUF_GPT_BYTE(b),
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
            if (did_return) return value;
            value = CONSIDER_1(BUF_BEGV(b), BUF_BEGV_BYTE(b),
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
            if (did_return) return value;
            value = CONSIDER_1(BUF_ZV(b), BUF_ZV_BYTE(b),
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
            if (did_return) return value;

            if (b == cached_buffer && BUF_MODIFF(b) == cached_modiff)
            {
                value = CONSIDER_1(cached_charpos, cached_bytepos,
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
                if (did_return) return value;
            }

            for (tail = BUF_MARKERS(b); tail != null; tail = tail.next)
            {
                value = CONSIDER_1(tail.charpos, tail.bytepos,
                       b, charpos, ref best_above, ref best_above_byte,
                       ref best_below, ref best_below_byte,
                       out did_return);
                if (did_return) return value;

                /* If we are down to a range of 50 chars,
               don't bother checking any other markers;
               scan the intervening chars directly now.  */
                if (best_above - best_below < 50)
                    break;
            }

            /* We get here if we did not exactly hit one of the known places.
               We have one known above and one known below.
               Scan, counting characters, from whichever one is closer.  */

            if (charpos - best_below < best_above - charpos)
            {
                bool record = charpos - best_below > 5000;

                while (best_below != charpos)
                {
                    best_below++;
                    BUF_INC_POS(b, ref best_below_byte);
                }

                /* If this position is quite far from the nearest known position,
               cache the correspondence by creating a marker here.
               It will last until the next GC.  */
                if (record)
                {
                    LispObject marker, buffer;
                    marker = F.make_marker();
                    buffer = b;
                    set_marker_both(marker, buffer, best_below, best_below_byte);
                }

                if (byte_debug_flag)
                    byte_char_debug_check(b, charpos, best_below_byte);

                cached_buffer = b;
                cached_modiff = BUF_MODIFF(b);
                cached_charpos = best_below;
                cached_bytepos = best_below_byte;

                return best_below_byte;
            }
            else
            {
                bool record = best_above - charpos > 5000;

                while (best_above != charpos)
                {
                    best_above--;
                    BUF_DEC_POS(b, ref best_above_byte);
                }

                /* If this position is quite far from the nearest known position,
               cache the correspondence by creating a marker here.
               It will last until the next GC.  */
                if (record)
                {
                    LispObject marker, buffer;
                    marker = F.make_marker();
                    buffer = b;
                    set_marker_both(marker, buffer, best_above, best_above_byte);
                }

                if (byte_debug_flag)
                    byte_char_debug_check(b, charpos, best_above_byte);

                cached_buffer = b;
                cached_modiff = BUF_MODIFF(b);
                cached_charpos = best_above;
                cached_bytepos = best_above_byte;

                return best_above_byte;
            }
        }

        /* bytepos_to_charpos returns the char position corresponding to BYTEPOS.  */

        /* This macro is a subroutine of bytepos_to_charpos.
           It is used when BYTEPOS is actually the byte position.  */
        public static int CONSIDER_2(int BYTEPOS, int CHARPOS, Buffer b, int bytepos,
                                     ref int best_above, ref int best_above_byte,
                                     ref int best_below, ref int best_below_byte,
                                     out bool did_return)
        {
            did_return = false; 
            int this_bytepos = (BYTEPOS);
            bool changed = false;							
									
            if (this_bytepos == bytepos)						
            {									
                int value = (CHARPOS);						
                if (byte_debug_flag)						
                    byte_char_debug_check (b, value, bytepos);

                did_return = true; 
                return value;							
            }									
            else if (this_bytepos > bytepos)					
            {									
                if (this_bytepos < best_above_byte)				
                {								
                    best_above = (CHARPOS);					
                    best_above_byte = this_bytepos;				
                    changed = true;							
                }								
            }									
            else if (this_bytepos > best_below_byte)				
            {									
                best_below = (CHARPOS);						
                best_below_byte = this_bytepos;					
                changed = true;							
            }									
									
            if (changed)								
            {									
                if (best_above - best_below == best_above_byte - best_below_byte)	
                {								
                    int value = best_below + (bytepos - best_below_byte);		
                    if (byte_debug_flag)						
                        byte_char_debug_check (b, value, bytepos);

                    did_return = true; 
                    return value;							
                }								
            }
            return 0; 
        }

        public static int bytepos_to_charpos (int bytepos)
        {
            return buf_bytepos_to_charpos(current_buffer, bytepos);
        }

        public static int buf_bytepos_to_charpos(Buffer b, int bytepos)
        {
            LispMarker tail;
            int best_above, best_above_byte;
            int best_below, best_below_byte;

            if (bytepos < BUF_BEG_BYTE(b) || bytepos > BUF_Z_BYTE(b))
                abort();

            best_above = BUF_Z(b);
            best_above_byte = BUF_Z_BYTE(b);

            /* If this buffer has as many characters as bytes,
               each character must be one byte.
               This takes care of the case where enable-multibyte-characters is nil.  */
            if (best_above == best_above_byte)
                return bytepos;

            best_below = BEG;
            best_below_byte = BEG_BYTE();

            bool did_return;
            int value = CONSIDER_2(BUF_PT_BYTE(b), BUF_PT(b), b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
            if (did_return) return value;
            value = CONSIDER_2(BUF_GPT_BYTE(b), BUF_GPT(b), b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
            if (did_return) return value;
            value = CONSIDER_2(BUF_BEGV_BYTE(b), BUF_BEGV(b), b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
            if (did_return) return value;
            value = CONSIDER_2(BUF_ZV_BYTE(b), BUF_ZV(b), b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
            if (did_return) return value;

            if (b == cached_buffer && BUF_MODIFF(b) == cached_modiff)
            {
                value = CONSIDER_2(cached_bytepos, cached_charpos, b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
                if (did_return) return value;
            }

            for (tail = BUF_MARKERS(b); tail != null; tail = tail.next)
            {
                value = CONSIDER_2(tail.bytepos, tail.charpos, b, bytepos, ref best_above, ref best_above_byte, ref best_below, ref best_below_byte, out did_return);
                if (did_return) return value;

                /* If we are down to a range of 50 chars,
               don't bother checking any other markers;
               scan the intervening chars directly now.  */
                if (best_above - best_below < 50)
                    break;
            }

            /* We get here if we did not exactly hit one of the known places.
               We have one known above and one known below.
               Scan, counting characters, from whichever one is closer.  */

            if (bytepos - best_below_byte < best_above_byte - bytepos)
            {
                bool record = bytepos - best_below_byte > 5000;

                while (best_below_byte < bytepos)
                {
                    best_below++;
                    BUF_INC_POS(b, ref best_below_byte);
                }

                /* If this position is quite far from the nearest known position,
               cache the correspondence by creating a marker here.
               It will last until the next GC.
               But don't do it if BUF_MARKERS is nil;
               that is a signal from Fset_buffer_multibyte.  */
                if (record && BUF_MARKERS(b) != null)
                {
                    LispObject marker, buffer;
                    marker = F.make_marker();
                    buffer = b;
                    set_marker_both(marker, buffer, best_below, best_below_byte);
                }

                if (byte_debug_flag)
                    byte_char_debug_check(b, best_below, bytepos);

                cached_buffer = b;
                cached_modiff = BUF_MODIFF(b);
                cached_charpos = best_below;
                cached_bytepos = best_below_byte;

                return best_below;
            }
            else
            {
                bool record = best_above_byte - bytepos > 5000;

                while (best_above_byte > bytepos)
                {
                    best_above--;
                    BUF_DEC_POS(b, ref best_above_byte);
                }

                /* If this position is quite far from the nearest known position,
               cache the correspondence by creating a marker here.
               It will last until the next GC.
               But don't do it if BUF_MARKERS is nil;
               that is a signal from Fset_buffer_multibyte.  */
                if (record && BUF_MARKERS(b) != null)
                {
                    LispObject marker, buffer;
                    marker = F.make_marker();
                    buffer = b;
                    set_marker_both(marker, buffer, best_above, best_above_byte);
                }

                if (byte_debug_flag)
                    byte_char_debug_check(b, best_above, bytepos);

                cached_buffer = b;
                cached_modiff = BUF_MODIFF(b);
                cached_charpos = best_above;
                cached_bytepos = best_above_byte;

                return best_above;
            }
        }

        /* Set the position of MARKER, specifying both the
           character position and the corresponding byte position.  */
        public static LispObject set_marker_both (LispObject marker, LispObject buffer, int charpos, int bytepos)
        {
            Buffer b;
            LispMarker m;

            CHECK_MARKER (marker);
            m = XMARKER (marker);

            if (NILP (buffer))
                b = current_buffer;
            else
            {
                CHECK_BUFFER (buffer);
                b = XBUFFER (buffer);
                /* If buffer is dead, set marker to point nowhere.  */
                if (EQ (b.name, Q.nil))
                {
                    unchain_marker (m);
                    return marker;
                }
            }

            /* In a single-byte buffer, the two positions must be equal.  */
            if (BUF_Z (b) == BUF_Z_BYTE (b)
                && charpos != bytepos)
                abort ();
            /* Every character is at least one byte.  */
            if (charpos > bytepos)
                abort ();

            m.bytepos = bytepos;
            m.charpos = charpos;

            if (m.buffer != b)
            {
                unchain_marker (m);
                m.buffer = b;
                m.next = b.BUF_MARKERS;
                b.BUF_MARKERS = m;
            }

            return marker;
        }

        /* Remove MARKER from the chain of whatever buffer it is in.
           Leave it "in no buffer".

           This is called during garbage collection,
           so we must be careful to ignore and preserve mark bits,
           including those in chain fields of markers.  */
        public static void unchain_marker (LispMarker marker)
        {
            LispMarker tail, prev, next;
            Buffer b;

            b = marker.buffer;
            if (b == null)
                return;

            if (EQ (b.name, Q.nil))
                abort ();

            marker.buffer = null;

            tail = b.BUF_MARKERS;
            prev = null;
            while (tail != null)
            {
                next = tail.next;

                if (marker == tail)
                {
                    if (prev == null)
                    {
                        b.BUF_MARKERS = next;
                        /* Deleting first marker from the buffer's chain.  Crash
                           if new first marker in chain does not say it belongs
                           to the same buffer, or at least that they have the same
                           base buffer.  */
                        if (next != null && b.text != next.buffer.text)
                            abort ();
                    }
                    else
                        prev.next = next;
                    /* We have removed the marker from the chain;
                       no need to scan the rest of the chain.  */
                    return;
                }
                else
                    prev = tail;
                tail = next;
            }

            /* Marker was not in its chain.  */
            abort ();
        }
        
        /* Return the char position of marker MARKER, as a C integer.  */
        public static int marker_position (LispObject marker)
        {
            LispMarker m = XMARKER(marker);
            Buffer buf = m.buffer;

            if (buf == null)
                error("Marker does not point anywhere");

            return m.charpos;
        }

        /* Return the byte position of marker MARKER, as a C integer.  */
        public static int marker_byte_position(LispObject marker)
        {
            LispMarker m = XMARKER(marker);
            Buffer buf = m.buffer;
            int i = m.bytepos;

            if (buf == null)
                error("Marker does not point anywhere");

            if (i < BUF_BEG_BYTE(buf) || i > BUF_Z_BYTE(buf))
                abort();

            return i;
        }

/* This version of Fset_marker won't let the position
   be outside the visible part.  */
        public static LispObject set_marker_restricted (LispObject marker, LispObject pos, LispObject buffer)
        {
            int charno, bytepos;
            Buffer b;
            LispMarker m;

            CHECK_MARKER(marker);
            m = XMARKER(marker);

            /* If position is nil or a marker that points nowhere,
               make this marker point nowhere.  */
            if (NILP(pos) || (MARKERP(pos) && XMARKER(pos).buffer == null))
            {
                unchain_marker(m);
                return marker;
            }

            if (NILP(buffer))
                b = current_buffer;
            else
            {
                CHECK_BUFFER(buffer);
                b = XBUFFER(buffer);
                /* If buffer is dead, set marker to point nowhere.  */
                if (EQ(b.name, Q.nil))
                {
                    unchain_marker(m);
                    return marker;
                }
            }

            /* Optimize the special case where we are copying the position
               of an existing marker, and MARKER is already in the same buffer.  */
            if (MARKERP(pos) && b == XMARKER(pos).buffer && b == m.buffer)
            {
                m.bytepos = XMARKER(pos).bytepos;
                m.charpos = XMARKER(pos).charpos;
                return marker;
            }

            CHECK_NUMBER_COERCE_MARKER(ref pos);

            charno = XINT(pos);

            if (charno < BUF_BEGV(b))
                charno = BUF_BEGV(b);
            if (charno > BUF_ZV(b))
                charno = BUF_ZV(b);

            bytepos = buf_charpos_to_bytepos(b, charno);

            /* Every character is at least one byte.  */
            if (charno > bytepos)
                abort();

            m.bytepos = bytepos;
            m.charpos = charno;

            if (m.buffer != b)
            {
                unchain_marker(m);
                m.buffer = b;
                m.next = BUF_MARKERS(b);
                BUF_MARKERS(b, m);
            }

            return marker;
        }
    }

    public partial class F
    {
        public static LispObject copy_marker(LispObject marker, LispObject type)
        {
            LispObject neww;

            L.CHECK_TYPE(L.INTEGERP(marker) || L.MARKERP(marker), Q.integer_or_marker_p, marker);

            neww = F.make_marker();
            F.set_marker(neww, marker,
                     (L.MARKERP(marker) ? F.marker_buffer(marker) : Q.nil));
            L.XMARKER(neww).insertion_type = !L.NILP(type);
            return neww;
        }

        /* Operations on markers. */
        public static LispObject marker_buffer(LispObject marker)
        {
            LispObject buf;
            L.CHECK_MARKER(marker);
            if (L.XMARKER(marker).buffer != null)
            {
                buf = L.XMARKER(marker).buffer;
                /* If the buffer is dead, we're in trouble: the buffer pointer here
               does not preserve the buffer from being GC'd (it's weak), so
               markers have to be unlinked from their buffer as soon as the buffer
               is killed.  */
                // eassert(!NILP(XBUFFER(buf)->name));
                return buf;
            }
            return Q.nil;
        }

        public static LispObject marker_position(LispObject marker)
        {
            L.CHECK_MARKER(marker);
            if (L.XMARKER(marker).buffer != null)
                return L.make_number(L.XMARKER(marker).charpos);

            return Q.nil;
        }

        public static LispObject set_marker(LispObject marker, LispObject position, LispObject buffer)
        {
            int charno, bytepos;
            Buffer b;
            LispMarker m;

            L.CHECK_MARKER(marker);
            m = L.XMARKER(marker);

            /* If position is nil or a marker that points nowhere,
               make this marker point nowhere.  */
            if (L.NILP(position)
                || (L.MARKERP(position) && L.XMARKER(position).buffer == null))
            {
                L.unchain_marker(m);
                return marker;
            }

            if (L.NILP(buffer))
                b = L.current_buffer;
            else
            {
                L.CHECK_BUFFER(buffer);
                b = L.XBUFFER(buffer);
                /* If buffer is dead, set marker to point nowhere.  */
                if (L.EQ(b.name, Q.nil))
                {
                    L.unchain_marker(m);
                    return marker;
                }
            }

            /* Optimize the special case where we are copying the position
               of an existing marker, and MARKER is already in the same buffer.  */
            if (L.MARKERP(position) && b == L.XMARKER(position).buffer
                && b == m.buffer)
            {
                m.bytepos = L.XMARKER(position).bytepos;
                m.charpos = L.XMARKER(position).charpos;
                return marker;
            }

            L.CHECK_NUMBER_COERCE_MARKER(ref position);

            charno = L.XINT(position);

            if (charno < L.BUF_BEG(b))
                charno = L.BUF_BEG(b);
            if (charno > L.BUF_Z(b))
                charno = L.BUF_Z(b);

            bytepos = L.buf_charpos_to_bytepos(b, charno);

            /* Every character is at least one byte.  */
            if (charno > bytepos)
                L.abort();

            m.bytepos = bytepos;
            m.charpos = charno;

            if (m.buffer != b)
            {
                L.unchain_marker(m);
                m.buffer = b;
                m.next = L.BUF_MARKERS(b);
                b.text.markers = m;
            }

            return marker;
        }
    }
}