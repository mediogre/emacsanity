namespace IronElisp
{
    public partial class Q
    {
        public static LispObject inhibit_read_only;
    }

    public partial class L
    {
        /* Last buffer for which undo information was recorded.  */
        public static Buffer last_undo_buffer;

        /* Position of point last time we inserted a boundary.  */
        public static Buffer last_boundary_buffer;
        public static int last_boundary_position;

        /* The first time a command records something for undo.
           it also allocates the undo-boundary object
           which will be added to the list at the end of the command.
           This ensures we can't run out of space while trying to make
           an undo-boundary.  */
        public static LispObject pending_boundary;
        
        /* Nonzero means do not record point in record_point.  */
        public static bool undo_inhibit_record_point
        {
            get { return Defs.B[(int)Bools.undo_inhibit_record_point]; }
            set { Defs.B[(int)Bools.undo_inhibit_record_point] = value; }
        }

        /* Record point as it was at beginning of this command (if necessary)
           and prepare the undo info for recording a change.
           PT is the position of point that will naturally occur as a result of the
           undo record that will be added just after this command terminates.  */
        public static void record_point(int pt)
        {
            bool at_boundary;

            /* Don't record position of pt when undo_inhibit_record_point holds.  */
            if (undo_inhibit_record_point)
                return;

            /* Allocate a cons cell to be the undo boundary after this command.  */
            if (NILP(pending_boundary))
                pending_boundary = F.cons(Q.nil, Q.nil);

            if ((current_buffer != last_undo_buffer)
                /* Don't call Fundo_boundary for the first change.  Otherwise we
               risk overwriting last_boundary_position in Fundo_boundary with
               PT of the current buffer and as a consequence not insert an
               undo boundary because last_boundary_position will equal pt in
               the test at the end of the present function (Bug#731).  */
                && (MODIFF > SAVE_MODIFF()))
                F.undo_boundary();
            last_undo_buffer = current_buffer;

            if (CONSP(current_buffer.undo_list))
            {
                /* Set AT_BOUNDARY to 1 only when we have nothing other than
                   marker adjustment before undo boundary.  */

                LispObject tail = current_buffer.undo_list, elt;

                while (true)
                {
                    if (NILP(tail))
                        elt = Q.nil;
                    else
                        elt = XCAR(tail);
                    if (NILP(elt) || !(CONSP(elt) && MARKERP(XCAR(elt))))
                        break;
                    tail = XCDR(tail);
                }
                at_boundary = NILP(elt);
            }
            else
                at_boundary = true;

            if (MODIFF <= SAVE_MODIFF())
                record_first_change();

            /* If we are just after an undo boundary, and
               point wasn't at start of deleted range, record where it was.  */
            if (at_boundary
                && current_buffer == last_boundary_buffer
                && last_boundary_position != pt)
                current_buffer.undo_list
                  = F.cons(make_number(last_boundary_position), current_buffer.undo_list);
        }

        /* Record that a deletion is about to take place,
           of the characters in STRING, at location BEG.  */
        public static void record_delete(int beg, LispObject stringg)
        {
            LispObject sbeg;

            if (EQ(current_buffer.undo_list, Q.t))
                return;

            if (PT() == beg + SCHARS(stringg))
            {
                sbeg = XSETINT(-beg);
                record_point(PT());
            }
            else
            {
                sbeg = XSETINT(beg);
                record_point(beg);
            }

            current_buffer.undo_list = F.cons(F.cons(stringg, sbeg), current_buffer.undo_list);
        }

        /* Record the fact that MARKER is about to be adjusted by ADJUSTMENT.
           This is done only when a marker points within text being deleted,
           because that's the only case where an automatic marker adjustment
           won't be inverted automatically by undoing the buffer modification.  */
        public static void record_marker_adjustment (LispObject marker, int adjustment)
        {
            if (EQ(current_buffer.undo_list, Q.t))
                return;

            /* Allocate a cons cell to be the undo boundary after this command.  */
            if (NILP(pending_boundary))
                pending_boundary = F.cons(Q.nil, Q.nil);

            if (current_buffer != last_undo_buffer)
                F.undo_boundary();
            last_undo_buffer = current_buffer;

            current_buffer.undo_list = F.cons (F.cons (marker, make_number (adjustment)),
         current_buffer.undo_list);
        }

        /* Record that an unmodified buffer is about to be changed.
           Record the file modification date so that when undoing this entry
           we can tell whether it is obsolete because the file was saved again.  */
        public static void record_first_change()
        {
            LispObject high, low;
            Buffer base_buffer = current_buffer;

            if (EQ(current_buffer.undo_list, Q.t))
                return;

            if (current_buffer != last_undo_buffer)
                F.undo_boundary();
            last_undo_buffer = current_buffer;

            if (base_buffer.base_buffer != null)
                base_buffer = base_buffer.base_buffer;

            high = XSETINT((int) ((base_buffer.modtime >> 16) & 0xffff));
            low = XSETINT((int) (base_buffer.modtime & 0xffff));
            current_buffer.undo_list = F.cons(F.cons(Q.t, F.cons(high, low)), current_buffer.undo_list);
        }

        /* Record a change in property PROP (whose old value was VAL)
           for LENGTH characters starting at position BEG in BUFFER.  */
        public static void record_property_change(int beg, int length, LispObject prop, LispObject value, LispObject buffer)
        {
            LispObject lbeg, lend, entry;
            Buffer obuf = current_buffer, buf = XBUFFER(buffer);
            bool boundary = false;

            if (EQ(buf.undo_list, Q.t))
                return;

            /* Allocate a cons cell to be the undo boundary after this command.  */
            if (NILP(pending_boundary))
                pending_boundary = F.cons(Q.nil, Q.nil);

            if (buf != last_undo_buffer)
                boundary = true;
            last_undo_buffer = buf;

            /* Switch temporarily to the buffer that was changed.  */
            current_buffer = buf;

            if (boundary)
                F.undo_boundary();

            if (MODIFF <= SAVE_MODIFF())
                record_first_change();

            lbeg = XSETINT(beg);
            lend = XSETINT(beg + length);
            entry = F.cons(Q.nil, F.cons(prop, F.cons(value, F.cons(lbeg, lend))));
            current_buffer.undo_list = F.cons(entry, current_buffer.undo_list);

            current_buffer = obuf;
        }

        /* Record an insertion that just happened or is about to happen,
           for LENGTH characters at position BEG.
           (It is possible to record an insertion before or after the fact
           because we don't need to record the contents.)  */
        public static void record_insert(int beg, int length)
        {
            LispObject lbeg, lend;

            if (EQ(current_buffer.undo_list, Q.t))
                return;

            record_point(beg);

            /* If this is following another insertion and consecutive with it
               in the buffer, combine the two.  */
            if (CONSP(current_buffer.undo_list))
            {
                LispObject elt;
                elt = XCAR(current_buffer.undo_list);
                if (CONSP(elt) && INTEGERP(XCAR(elt)) && INTEGERP(XCDR(elt)) && XINT(XCDR(elt)) == beg)
                {
                    XSETCDR(elt, make_number(beg + length));
                    return;
                }
            }

            lbeg = XSETINT(beg);
            lend = XSETINT(beg + length);
            current_buffer.undo_list = F.cons(F.cons(lbeg, lend),
                                               current_buffer.undo_list);
        }

        /* Record that a replacement is about to take place,
           for LENGTH characters at location BEG.
           The replacement must not change the number of characters.  */
        public static void record_change (int beg, int length)
        {
            record_delete(beg, make_buffer_string(beg, beg + length, 1));
            record_insert(beg, length);
        }
    }

    public partial class F
    {
        public static LispObject undo_boundary()
        {
            LispObject tem;
            if (L.EQ(L.current_buffer.undo_list, Q.t))
                return Q.nil;
            tem = F.car(L.current_buffer.undo_list);
            if (!L.NILP(tem))
            {
                /* One way or another, cons nil onto the front of the undo list.  */
                if (!L.NILP(L.pending_boundary))
                {
                    /* If we have preallocated the cons cell to use here,
                       use that one.  */
                    L.XSETCDR(L.pending_boundary, L.current_buffer.undo_list);
                    L.current_buffer.undo_list = L.pending_boundary;
                    L.pending_boundary = Q.nil;
                }
                else
                    L.current_buffer.undo_list = F.cons(Q.nil, L.current_buffer.undo_list);
            }
            L.last_boundary_position = L.PT();
            L.last_boundary_buffer = L.current_buffer;
            return Q.nil;
        }
    }
}
