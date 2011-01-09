namespace IronElisp
{
    public partial class V
    {
        public static LispObject default_text_properties
        {
            get { return Defs.O[(int)Objects.default_text_properties]; }
            set { Defs.O[(int)Objects.default_text_properties] = value; }
        }

        public static LispObject char_property_alias_alist
        {
            get { return Defs.O[(int)Objects.char_property_alias_alist]; }
            set { Defs.O[(int)Objects.char_property_alias_alist] = value; }
        }

        public static LispObject inhibit_point_motion_hooks
        {
            get { return Defs.O[(int)Objects.inhibit_point_motion_hooks]; }
            set { Defs.O[(int)Objects.inhibit_point_motion_hooks] = value; }
        }

        public static LispObject text_property_default_nonsticky
        {
            get { return Defs.O[(int)Objects.text_property_default_nonsticky]; }
            set { Defs.O[(int)Objects.text_property_default_nonsticky] = value; }
        }
    }

    public partial class F
    {
        public static LispObject next_single_char_property_change(LispObject position, LispObject prop, LispObject obj, LispObject limit)
        {
            if (L.STRINGP(obj))
            {
                position = F.next_single_property_change(position, prop, obj, limit);
                if (L.NILP(position))
                {
                    if (L.NILP(limit))
                        position = L.make_number(L.SCHARS(obj));
                    else
                    {
                        L.CHECK_NUMBER(limit);
                        position = limit;
                    }
                }
            }
            else
            {
                LispObject initial_value, value;
                int count = L.SPECPDL_INDEX();

                if (!L.NILP(obj))
                    L.CHECK_BUFFER(obj);

                if (L.BUFFERP(obj) && L.current_buffer != L.XBUFFER(obj))
                {
                    L.record_unwind_protect(F.set_buffer, F.current_buffer());
                    F.set_buffer(obj);
                }

                L.CHECK_NUMBER_COERCE_MARKER(ref position);

                initial_value = F.get_char_property(position, prop, obj);

                if (L.NILP(limit))
                    limit = L.XSETINT(L.ZV);
                else
                    L.CHECK_NUMBER_COERCE_MARKER(ref limit);

                if (L.XINT(position) >= L.XINT(limit))
                {
                    position = limit;
                    if (L.XINT(position) > L.ZV)
                        position = L.XSETINT(L.ZV);
                }
                else
                    while (true)
                    {
                        position = F.next_char_property_change(position, limit);
                        if (L.XINT(position) >= L.XINT(limit))
                        {
                            position = limit;
                            break;
                        }

                        value = F.get_char_property(position, prop, obj);
                        if (!L.EQ(value, initial_value))
                            break;
                    }

                L.unbind_to(count, Q.nil);
            }

            return position;
        }

        public static LispObject previous_single_char_property_change(LispObject position, LispObject prop, LispObject obj, LispObject limit)
        {
            if (L.STRINGP(obj))
            {
                position = F.previous_single_property_change(position, prop, obj, limit);
                if (L.NILP(position))
                {
                    if (L.NILP(limit))
                        position = L.make_number(0);
                    else
                    {
                        L.CHECK_NUMBER(limit);
                        position = limit;
                    }
                }
            }
            else
            {
                int count = L.SPECPDL_INDEX();

                if (!L.NILP(obj))
                    L.CHECK_BUFFER(obj);

                if (L.BUFFERP(obj) && L.current_buffer != L.XBUFFER(obj))
                {
                    L.record_unwind_protect(F.set_buffer, F.current_buffer());
                    F.set_buffer(obj);
                }

                L.CHECK_NUMBER_COERCE_MARKER(ref position);

                if (L.NILP(limit))
                    limit = L.XSETINT(L.BEGV());
                else
                    L.CHECK_NUMBER_COERCE_MARKER(ref limit);

                if (L.XINT(position) <= L.XINT(limit))
                {
                    position = limit;
                    if (L.XINT(position) < L.BEGV())
                        position = L.XSETINT(L.BEGV());
                }
                else
                {
                    LispObject initial_value
                      = F.get_char_property(L.make_number(L.XINT(position) - 1),
                                prop, obj);

                    while (true)
                    {
                        position = F.previous_char_property_change(position, limit);

                        if (L.XINT(position) <= L.XINT(limit))
                        {
                            position = limit;
                            break;
                        }
                        else
                        {
                            LispObject value
                              = F.get_char_property(L.make_number(L.XINT(position) - 1),
                                        prop, obj);

                            if (!L.EQ(value, initial_value))
                                break;
                        }
                    }
                }

                L.unbind_to(count, Q.nil);
            }

            return position;
        }

        public static LispObject remove_list_of_text_properties(LispObject start, LispObject end, LispObject list_of_properties, LispObject obj)
        {
            Interval i, unchanged;
            int s, len;
            bool modified = false;
            LispObject properties;
            properties = list_of_properties;

            if (L.NILP(obj))
                obj = L.current_buffer;

            i = L.validate_interval_range(obj, ref start, ref end, L.soft);
            if (L.NULL_INTERVAL_P(i))
                return Q.nil;

            s = L.XINT(start);
            len = L.XINT(end) - s;

            if (i.position != s)
            {
                /* No properties on this first interval -- return if
                   it covers the entire region.  */
                if (!L.interval_has_some_properties_list(properties, i))
                {
                    int got = (int)(L.LENGTH(i) - (s - i.position));
                    if (got >= len)
                        return Q.nil;
                    len -= got;
                    i = L.next_interval(i);
                }
                /* Split away the beginning of this interval; what we don't
               want to modify.  */
                else
                {
                    unchanged = i;
                    i = L.split_interval_right(unchanged, s - (int) unchanged.position);
                    L.copy_properties(unchanged, i);
                }
            }

            /* We are at the beginning of an interval, with len to scan.
               The flag `modified' records if changes have been made.
               When object is a buffer, we must call modify_region before changes are
               made and signal_after_change when we are done.
               We call modify_region before calling remove_properties if modified == 0,
               and we call signal_after_change before returning if modified != 0. */
            for (; ; )
            {
                if (i == null)
                    L.abort();

                if (L.LENGTH(i) >= len)
                {
                    if (!L.interval_has_some_properties_list(properties, i))
                        if (modified)
                        {
                            if (L.BUFFERP(obj))
                                L.signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                             L.XINT(end) - L.XINT(start));
                            return Q.t;
                        }
                        else
                            return Q.nil;

                    if (L.LENGTH(i) == len)
                    {
                        if (!modified && L.BUFFERP(obj))
                            L.modify_region(L.XBUFFER(obj), L.XINT(start), L.XINT(end), true);
                        L.remove_properties(Q.nil, properties, i, obj);
                        if (L.BUFFERP(obj))
                            L.signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                         L.XINT(end) - L.XINT(start));
                        return Q.t;
                    }

                    /* i has the properties, and goes past the change limit */
                    unchanged = i;
                    i = L.split_interval_left(i, len);
                    L.copy_properties(unchanged, i);
                    if (!modified && L.BUFFERP(obj))
                        L.modify_region(L.XBUFFER(obj), L.XINT(start), L.XINT(end), true);
                    L.remove_properties(Q.nil, properties, i, obj);
                    if (L.BUFFERP(obj))
                        L.signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                 L.XINT(end) - L.XINT(start));
                    return Q.t;
                }

                if (L.interval_has_some_properties_list(properties, i))
                {
                    if (!modified && L.BUFFERP(obj))
                        L.modify_region(L.XBUFFER(obj), L.XINT(start), L.XINT(end), true);
                    L.remove_properties(Q.nil, properties, i, obj);
                    modified = true;
                }
                len -= (int) L.LENGTH(i);
                i = L.next_interval(i);
            }
        }

        /* Callers note, this can GC when OBJECT is a buffer (or nil).  */
        public static LispObject put_text_property(LispObject start, LispObject end, LispObject property, LispObject value, LispObject obj)
        {
            F.add_text_properties(start, end,
                      F.cons(property, F.cons(value, Q.nil)),
                      obj);
            return Q.nil;
        }

        public static LispObject previous_single_property_change(LispObject position, LispObject prop, LispObject obj, LispObject limit)
        {
            Interval i, previous;
            LispObject here_val;

            if (L.NILP(obj))
                obj = L.current_buffer;

            if (!L.NILP(limit))
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);

            i = L.validate_interval_range(obj, ref position, ref position, L.soft);

            /* Start with the interval containing the char before point.  */
            if (!L.NULL_INTERVAL_P(i) && i.position == L.XINT(position))
                i = L.previous_interval(i);

            if (L.NULL_INTERVAL_P(i))
                return limit;

            here_val = L.textget(i.plist, prop);
            previous = L.previous_interval(i);
            while (!L.NULL_INTERVAL_P(previous)
               && L.EQ(here_val, L.textget(previous.plist, prop))
               && (L.NILP(limit)
                   || (previous.position + L.LENGTH(previous) > L.XINT(limit))))
                previous = L.previous_interval(previous);

            if (L.NULL_INTERVAL_P(previous)
                || (previous.position + L.LENGTH(previous)
                <= (L.INTEGERP(limit)
                    ? L.XINT(limit)
                    : (L.STRINGP(obj) ? 0 : L.BUF_BEGV(L.XBUFFER(obj))))))
                return limit;
            else
                return L.make_number((int)(previous.position + L.LENGTH(previous)));
        }

        public static LispObject next_single_property_change(LispObject position, LispObject prop, LispObject obj, LispObject limit)
        {
            Interval i, next;
            LispObject here_val;

            if (L.NILP(obj))
                obj = L.current_buffer;

            if (!L.NILP(limit))
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);

            i = L.validate_interval_range(obj, ref position, ref position, L.soft);
            if (L.NULL_INTERVAL_P(i))
                return limit;

            here_val = L.textget(i.plist, prop);
            next = L.next_interval(i);
            while (!L.NULL_INTERVAL_P(next)
               && L.EQ(here_val, L.textget(next.plist, prop))
               && (L.NILP(limit) || next.position < L.XINT(limit)))
                next = L.next_interval(next);

            if (L.NULL_INTERVAL_P(next)
                || (next.position
                >= (L.INTEGERP(limit)
                    ? L.XINT(limit)
                    : (L.STRINGP(obj)
                   ? L.SCHARS(obj)
                   : L.BUF_ZV(L.XBUFFER(obj))))))
                return limit;
            else
                return L.make_number((int) next.position);
        }
            
        public static LispObject text_property_any(LispObject start, LispObject end, LispObject property, LispObject value, LispObject obj)
        {
            Interval i;
            int e, pos;

            if (L.NILP(obj))
                obj = L.current_buffer;
            i = L.validate_interval_range(obj, ref start, ref end, L.soft);
            if (L.NULL_INTERVAL_P(i))
                return (!L.NILP(value) || L.EQ(start, end) ? Q.nil : start);
            e = L.XINT(end);

            while (!L.NULL_INTERVAL_P(i))
            {
                if (i.position >= e)
                    break;
                if (L.EQ(L.textget(i.plist, property), value))
                {
                    pos = (int) i.position;
                    if (pos < L.XINT(start))
                        pos = L.XINT(start);
                    return L.make_number(pos);
                }
                i = L.next_interval(i);
            }
            return Q.nil;
        }

        public static LispObject text_properties_at(LispObject position, LispObject obj)
        {
            Interval i;

            if (L.NILP(obj))
                obj = L.current_buffer;

            i = L.validate_interval_range(obj, ref position, ref position, L.soft);
            if (L.NULL_INTERVAL_P(i))
                return Q.nil;
            /* If POSITION is at the end of the interval,
               it means it's the end of OBJECT.
               There are no properties at the very end,
               since no character follows.  */
            if (L.XINT(position) == L.LENGTH(i) + i.position)
                return Q.nil;

            return i.plist;
        }

        public static LispObject get_text_property(LispObject position, LispObject prop, LispObject obj)
        {
            return L.textget(F.text_properties_at(position, obj), prop);
        }

        public static LispObject get_char_property(LispObject position, LispObject prop, LispObject obj)
        {
            LispObject dummy = null;
            return L.get_char_property_and_overlay(position, prop, obj, ref dummy);
        }

        public static LispObject get_char_property_and_overlay(LispObject position, LispObject prop, LispObject obj)
        {
            LispObject overlay = null;
            LispObject val = L.get_char_property_and_overlay(position, prop, obj, ref overlay);
            return F.cons(val, overlay);
        }

        public static LispObject next_char_property_change(LispObject position, LispObject limit)
        {
            LispObject temp;

            temp = F.next_overlay_change(position);
            if (!L.NILP(limit))
            {
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);
                if (L.XINT(limit) < L.XINT(temp))
                    temp = limit;
            }
            return F.next_property_change(position, Q.nil, temp);
        }

        public static LispObject previous_char_property_change(LispObject position, LispObject limit)
        {
            LispObject temp;

            temp = F.previous_overlay_change(position);
            if (!L.NILP(limit))
            {
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);
                if (L.XINT(limit) > L.XINT(temp))
                    temp = limit;
            }
            return F.previous_property_change(position, Q.nil, temp);
        }

        public static LispObject next_property_change(LispObject position, LispObject obj, LispObject limit)
        {
            Interval i, next;

            if (L.NILP(obj))
                obj = L.current_buffer;

            if (!L.NILP(limit) && !L.EQ(limit, Q.t))
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);

            i = L.validate_interval_range(obj, ref position, ref position, L.soft);

            /* If LIMIT is t, return start of next interval--don't
               bother checking further intervals.  */
            if (L.EQ(limit, Q.t))
            {
                if (L.NULL_INTERVAL_P(i))
                    next = i;
                else
                    next = L.next_interval(i);

                if (L.NULL_INTERVAL_P(next))
                {
                    position = L.XSETINT((L.STRINGP(obj)
                                ? L.SCHARS(obj)
                                : L.BUF_ZV(L.XBUFFER(obj))));
                }
                else
                {
                    position = L.XSETINT((int)next.position);
                }
                return position;
            }

            if (L.NULL_INTERVAL_P(i))
                return limit;

            next = L.next_interval(i);

            while (!L.NULL_INTERVAL_P(next) && L.intervals_equal(i, next)
               && (L.NILP(limit) || next.position < L.XINT(limit)))
                next = L.next_interval(next);

            if (L.NULL_INTERVAL_P(next)
                || (next.position
                >= (L.INTEGERP(limit)
                    ? L.XINT(limit)
                    : (L.STRINGP(obj)
                   ? L.SCHARS(obj)
                   : L.BUF_ZV(L.XBUFFER(obj))))))
                return limit;
            else
                return L.make_number((int) next.position);
        }

        public static LispObject previous_property_change(LispObject position, LispObject obj, LispObject limit)
        {
            Interval i, previous;

            if (L.NILP(obj))
                obj = L.current_buffer;

            if (!L.NILP(limit))
                L.CHECK_NUMBER_COERCE_MARKER(ref limit);

            i = L.validate_interval_range(obj, ref position, ref position, L.soft);
            if (L.NULL_INTERVAL_P(i))
                return limit;

            /* Start with the interval containing the char before point.  */
            if (i.position == L.XINT(position))
                i = L.previous_interval(i);

            previous = L.previous_interval(i);
            while (!L.NULL_INTERVAL_P(previous) && L.intervals_equal(previous, i)
               && (L.NILP(limit)
                   || (previous.position + L.LENGTH(previous) > L.XINT(limit))))
                previous = L.previous_interval(previous);

            if (L.NULL_INTERVAL_P(previous)
                || (previous.position + L.LENGTH(previous)
                <= (L.INTEGERP(limit)
                    ? L.XINT(limit)
                    : (L.STRINGP(obj) ? 0 : L.BUF_BEGV(L.XBUFFER(obj))))))
                return limit;
            else
                return L.make_number((int)(previous.position + L.LENGTH(previous)));
        }

        public static LispObject set_text_properties(LispObject start, LispObject end, LispObject properties, LispObject obj)
        {
            return L.set_text_properties(start, end, properties, obj, Q.t);
        }

        /* Callers note, this can GC when OBJECT is a buffer (or nil).  */
        public static LispObject add_text_properties(LispObject start, LispObject end, LispObject properties, LispObject obj)
        {
            throw new System.Exception();
#if COMEBACK_LATER
            Interval i, unchanged;
            int s, len, modified = 0;

            properties = validate_plist(properties);
            if (L.NILP(properties))
                return Q.nil;

            if (L.NILP(obj))
                L.XSETBUFFER(obj, current_buffer);

            i = validate_interval_range(obj, &start, &end, hard);
            if (L.NULL_INTERVAL_P(i))
                return Q.nil;

            s = L.XINT(start);
            len = L.XINT(end) - s;

            /* If we're not starting on an interval boundary, we have to
              split this interval.  */
            if (i.position != s)
            {
                /* If this interval already has the properties, we can
                   skip it.  */
                if (interval_has_all_properties(properties, i))
                {
                    int got = (L.LENGTH(i) - (s - i.position));
                    if (got >= len)
                        return (Q.nil);
                    len -= got;
                    i = next_interval(i);
                }
                else
                {
                    unchanged = i;
                    i = split_interval_right(unchanged, s - unchanged.position);
                    copy_properties(unchanged, i);
                }
            }

            if (BUFFERP(obj))
                modify_region(XBUFFER(obj), L.XINT(start), L.XINT(end), 1);

            /* We are at the beginning of interval I, with LEN chars to scan.  */
            for (; ; )
            {
                if (i == 0)
                    abort();

                if (L.LENGTH(i) >= len)
                {
                    if (interval_has_all_properties(properties, i))
                    {
                        if (BUFFERP(obj))
                            signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                         L.XINT(end) - L.XINT(start));

                        return modified ? Q.t : Q.nil;
                    }

                    if (L.LENGTH(i) == len)
                    {
                        add_properties(properties, i, obj);
                        if (BUFFERP(obj))
                            signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                         L.XINT(end) - L.XINT(start));
                        return Q.t;
                    }

                    /* i doesn't have the properties, and goes past the change limit */
                    unchanged = i;
                    i = split_interval_left(unchanged, len);
                    copy_properties(unchanged, i);
                    add_properties(properties, i, obj);
                    if (BUFFERP(obj))
                        signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                 L.XINT(end) - L.XINT(start));
                    return Q.t;
                }

                len -= L.LENGTH(i);
                modified += add_properties(properties, i, obj);
                i = next_interval(i);
            }
#endif
        }
    }

    public partial class L
    {
        /* For any members of PLIST, or LIST,
           which are properties of I, remove them from I's plist.
           (If PLIST is non-nil, use that, otherwise use LIST.)
           OBJECT is the string or buffer containing I.  */
        public static int remove_properties(LispObject plist, LispObject list, Interval i, LispObject obj)
        {
            LispObject tail1, tail2, sym, current_plist;
            int changed = 0;

            /* Nonzero means tail1 is a plist, otherwise it is a list.  */
            bool use_plist;

            current_plist = i.plist;

            if (!NILP(plist))
            {
                tail1 = plist;
                use_plist = true;
            }
            else
            {
                tail1 = list;
                use_plist = false;
            }

            /* Go through each element of LIST or PLIST.  */
            while (CONSP(tail1))
            {
                sym = XCAR(tail1);

                /* First, remove the symbol if it's at the head of the list */
                while (CONSP(current_plist) && EQ(sym, XCAR(current_plist)))
                {
                    if (BUFFERP(obj))
                        record_property_change((int) i.position, (int) LENGTH(i),
                                    sym, XCAR(XCDR(current_plist)),
                                    obj);

                    current_plist = XCDR(XCDR(current_plist));
                    changed++;
                }

                /* Go through I's plist, looking for SYM.  */
                tail2 = current_plist;
                while (!NILP(tail2))
                {
                    LispObject tthis;
                    tthis = XCDR(XCDR(tail2));
                    if (CONSP(tthis) && EQ(sym, XCAR(tthis)))
                    {
                        if (BUFFERP(obj))
                            record_property_change((int) i.position, (int) LENGTH(i),
                                        sym, XCAR(XCDR(tthis)), obj);

                        F.setcdr(XCDR(tail2), XCDR(XCDR(tthis)));
                        changed++;
                    }
                    tail2 = tthis;
                }

                /* Advance thru TAIL1 one way or the other.  */
                tail1 = XCDR(tail1);
                if (use_plist && CONSP(tail1))
                    tail1 = XCDR(tail1);
            }

            if (changed != 0)
                i.plist = current_plist;
            return changed;
        }

        /* Return nonzero if the plist of interval I has any of the
           property names in LIST, regardless of their values.  */
        public static bool interval_has_some_properties_list(LispObject list, Interval i)
        {
            LispObject tail1, tail2, sym;

            /* Go through each element of LIST.  */
            for (tail1 = list; CONSP(tail1); tail1 = XCDR(tail1))
            {
                sym = F.car(tail1);

                /* Go through i's plist, looking for tail1 */
                for (tail2 = i.plist; CONSP(tail2); tail2 = XCDR(XCDR(tail2)))
                    if (EQ(sym, XCAR(tail2)))
                        return true;
            }

            return false;
        }

        /* Signal a `text-read-only' error.  This function makes it easier
           to capture that error in GDB by putting a breakpoint on it.  */
        public static void text_read_only (LispObject propval)
        {
            if (STRINGP(propval))
                xsignal1(Q.text_read_only, propval);

            xsignal0(Q.text_read_only);
        }

        /* Extract the interval at the position pointed to by BEGIN from
           OBJECT, a string or buffer.  Additionally, check that the positions
           pointed to by BEGIN and END are within the bounds of OBJECT, and
           reverse them if *BEGIN is greater than *END.  The objects pointed
           to by BEGIN and END may be integers or markers; if the latter, they
           are coerced to integers.

           When OBJECT is a string, we increment *BEGIN and *END
           to make them origin-one.

           Note that buffer points don't correspond to interval indices.
           For example, point-max is 1 greater than the index of the last
           character.  This difference is handled in the caller, which uses
           the validated points to determine a length, and operates on that.
           Exceptions are Ftext_properties_at, Fnext_property_change, and
           Fprevious_property_change which call this function with BEGIN == END.
           Handle this case specially.

           If FORCE is soft (0), it's OK to return NULL_INTERVAL.  Otherwise,
           create an interval tree for OBJECT if one doesn't exist, provided
           the object actually contains text.  In the current design, if there
           is no text, there can be no text properties.  */
        public const int soft = 0;
        public const int hard = 1;

        public static Interval validate_interval_range(LispObject obj, ref LispObject begin, ref LispObject end, int force)
        {
            Interval i;
            int searchpos;

            CHECK_STRING_OR_BUFFER(obj);
            CHECK_NUMBER_COERCE_MARKER(ref begin);
            CHECK_NUMBER_COERCE_MARKER(ref end);

            /* If we are asked for a point, but from a subr which operates
               on a range, then return nothing.  */
            if (EQ(begin, end) && begin != end)
                return NULL_INTERVAL;

            if (XINT(begin) > XINT(end))
            {
                LispObject n;
                n = begin;
                begin = end;
                end = n;
            }

            if (BUFFERP(obj))
            {
                Buffer b = XBUFFER(obj);

                if (!(BUF_BEGV(b) <= XINT(begin) && XINT(begin) <= XINT(end)
                  && XINT(end) <= BUF_ZV(b)))
                    args_out_of_range(begin, end);
                i = BUF_INTERVALS(b);

                /* If there's no text, there are no properties.  */
                if (BUF_BEGV(b) == BUF_ZV(b))
                    return NULL_INTERVAL;

                searchpos = XINT(begin);
            }
            else
            {
                int len = SCHARS(obj);

                if (!(0 <= XINT(begin) && XINT(begin) <= XINT(end)
                   && XINT(end) <= len))
                    args_out_of_range(begin, end);
                begin = XSETINT(XINT(begin));
                if (begin != end)
                    end = XSETINT(XINT(end));
                i = STRING_INTERVALS(obj);

                if (len == 0)
                    return NULL_INTERVAL;

                searchpos = XINT(begin);
            }

            if (NULL_INTERVAL_P(i))
                return (force != 0 ? create_root_interval(obj) : i);

            return find_interval(i, searchpos);
        }

        /* Return the value of char's property PROP, in OBJECT at POSITION.
           OBJECT is optional and defaults to the current buffer.
           If OVERLAY is non-0, then in the case that the returned property is from
           an overlay, the overlay found is returned in *OVERLAY, otherwise nil is
           returned in *OVERLAY.
           If POSITION is at the end of OBJECT, the value is nil.
           If OBJECT is a buffer, then overlay properties are considered as well as
           text properties.
           If OBJECT is a window, then that window's buffer is used, but
           window-specific overlays are considered only if they are associated
           with OBJECT. */
        public static LispObject get_char_property_and_overlay(LispObject position, LispObject prop, LispObject obj, ref LispObject overlay)
        {
            Window w = null;

            CHECK_NUMBER_COERCE_MARKER(ref position);

            if (NILP(obj))
                obj = current_buffer;

            if (WINDOWP(obj))
            {
                w = XWINDOW(obj);
                obj = w.buffer;
            }
            if (BUFFERP(obj))
            {
                int noverlays = 0;
                LispObject[] overlay_vec = null;
                Buffer obuf = current_buffer;

                if (XINT(position) < BUF_BEGV(XBUFFER(obj)) || XINT(position) > BUF_ZV(XBUFFER(obj)))
                    xsignal1(Q.args_out_of_range, position);

                set_buffer_temp(XBUFFER(obj));

                int next_dummy = 0;
                GET_OVERLAYS_AT(XINT(position), ref overlay_vec, ref noverlays, ref next_dummy, 0);
                noverlays = sort_overlays(overlay_vec, noverlays, w);

                set_buffer_temp(obuf);

                /* Now check the overlays in order of decreasing priority.  */
                while (--noverlays >= 0)
                {
                    LispObject tem = F.overlay_get(overlay_vec[noverlays], prop);
                    if (!NILP(tem))
                    {
                        /* Return the overlay we got the property from.  */
                        overlay = overlay_vec[noverlays];
                        return tem;
                    }
                }
            }

            /* Indicate that the return value is not from an overlay.  */
            overlay = Q.nil;

            /* Not a buffer, or no appropriate overlay, so fall through to the
               simpler case.  */
            return F.get_text_property(position, prop, obj);
        }
        /* Replace properties of text from START to END with new list of
           properties PROPERTIES.  OBJECT is the buffer or string containing
           the text.  OBJECT nil means use the current buffer.
           SIGNAL_AFTER_CHANGE_P nil means don't signal after changes.  Value
           is nil if the function _detected_ that it did not replace any
           properties, non-nil otherwise.  */
        public static LispObject set_text_properties(LispObject start, LispObject end, LispObject properties,
                                                     LispObject obj, LispObject signal_after_change_p)
        {
            throw new System.Exception("Come back");
#if COMEBACK_LATER
  register INTERVAL i;
  Lisp_Object ostart, oend;

  ostart = start;
  oend = end;

  properties = validate_plist (properties);

  if (NILP (obj))
    XSETBUFFER (obj, current_buffer);

  /* If we want no properties for a whole string,
     get rid of its intervals.  */
  if (NILP (properties) && STRINGP (obj)
      && XFASTINT (start) == 0
      && XFASTINT (end) == SCHARS (obj))
    {
      if (! STRING_INTERVALS (obj))
	return Qnil;

      STRING_SET_INTERVALS (obj, NULL_INTERVAL);
      return Qt;
    }

  i = validate_interval_range (obj, &start, &end, soft);

  if (NULL_INTERVAL_P (i))
    {
      /* If buffer has no properties, and we want none, return now.  */
      if (NILP (properties))
	return Qnil;

      /* Restore the original START and END values
	 because validate_interval_range increments them for strings.  */
      start = ostart;
      end = oend;

      i = validate_interval_range (obj, &start, &end, hard);
      /* This can return if start == end.  */
      if (NULL_INTERVAL_P (i))
	return Qnil;
    }

  if (BUFFERP (obj))
    modify_region (XBUFFER (obj), XINT (start), XINT (end), 1);

  set_text_properties_1 (start, end, properties, obj, i);

  if (BUFFERP (obj) && !NILP (signal_after_change_p))
    signal_after_change (XINT (start), XINT (end) - XINT (start),
			 XINT (end) - XINT (start));
  return Qt;
#endif
        }

/* Return the direction from which the text-property PROP would be
   inherited by any new text inserted at POS: 1 if it would be
   inherited from the char after POS, -1 if it would be inherited from
   the char before POS, and 0 if from neither.
   BUFFER can be either a buffer or nil (meaning current buffer).  */
        public static int text_property_stickiness(LispObject prop, LispObject pos, LispObject buffer)
        {
            LispObject prev_pos, front_sticky;
            bool is_rear_sticky = true, is_front_sticky = false; /* defaults */

            if (NILP(buffer))
                buffer = current_buffer;

            if (XINT(pos) > BUF_BEGV(XBUFFER(buffer)))
            /* Consider previous character.  */
            {
                LispObject rear_non_sticky;

                prev_pos = make_number(XINT(pos) - 1);
                rear_non_sticky = F.get_text_property(prev_pos, Q.rear_nonsticky, buffer);

                if (!NILP(CONSP(rear_non_sticky)
                   ? F.memq(prop, rear_non_sticky)
                   : rear_non_sticky))
                    /* PROP is rear-non-sticky.  */
                    is_rear_sticky = false;
            }
            else
                return 0;

            /* Consider following character.  */
            /* This signals an arg-out-of-range error if pos is outside the
               buffer's accessible range.  */
            front_sticky = F.get_text_property(pos, Q.front_sticky, buffer);

            if (EQ(front_sticky, Q.t)
                || (CONSP(front_sticky)
                && !NILP(F.memq(prop, front_sticky))))
                /* PROP is inherited from after.  */
                is_front_sticky = true;

            /* Simple cases, where the properties are consistent.  */
            if (is_rear_sticky && !is_front_sticky)
                return -1;
            else if (!is_rear_sticky && is_front_sticky)
                return 1;
            else if (!is_rear_sticky && !is_front_sticky)
                return 0;

            /* The stickiness properties are inconsistent, so we have to
               disambiguate.  Basically, rear-sticky wins, _except_ if the
               property that would be inherited has a value of nil, in which case
               front-sticky wins.  */
            if (XINT(pos) == BUF_BEGV(XBUFFER(buffer))
                || NILP(F.get_text_property(prev_pos, prop, buffer)))
                return 1;
            else
                return -1;
        }

        /* Return a list representing the text properties of OBJECT between
           START and END.  if PROP is non-nil, report only on that property.
           Each result list element has the form (S E PLIST), where S and E
           are positions in OBJECT and PLIST is a property list containing the
           text properties of OBJECT between S and E.  Value is nil if OBJECT
           doesn't contain text properties between START and END.  */

        public static LispObject text_property_list(LispObject obj, LispObject start, LispObject end, LispObject prop)
        {
            throw new System.Exception();
#if COMEBACK_LATER
  struct interval *i;
  Lisp_Object result;

  result = Qnil;

  i = validate_interval_range (object, &start, &end, soft);
  if (!NULL_INTERVAL_P (i))
    {
      int s = XINT (start);
      int e = XINT (end);

      while (s < e)
	{
	  int interval_end, len;
	  Lisp_Object plist;

	  interval_end = i->position + LENGTH (i);
	  if (interval_end > e)
	    interval_end = e;
	  len = interval_end - s;

	  plist = i->plist;

	  if (!NILP (prop))
	    for (; CONSP (plist); plist = Fcdr (XCDR (plist)))
	      if (EQ (XCAR (plist), prop))
		{
		  plist = Fcons (prop, Fcons (Fcar (XCDR (plist)), Qnil));
		  break;
		}

	  if (!NILP (plist))
	    result = Fcons (Fcons (make_number (s),
				   Fcons (make_number (s + len),
					  Fcons (plist, Qnil))),
			    result);

	  i = next_interval (i);
	  if (NULL_INTERVAL_P (i))
	    break;
	  s = i->position;
	}
    }

  return result;
#endif
        }

        /* Add text properties to OBJECT from LIST.  LIST is a list of triples
           (START END PLIST), where START and END are positions and PLIST is a
           property list containing the text properties to add.  Adjust START
           and END positions by DELTA before adding properties.  Value is
           non-zero if OBJECT was modified.  */
        public static bool add_text_properties_from_list(LispObject obj, LispObject list, LispObject delta)
        {
            bool modified_p = false;

            for (; CONSP(list); list = XCDR(list))
            {
                LispObject item, start, end, plist, tem;

                item = XCAR(list);
                start = make_number(XINT(XCAR(item)) + XINT(delta));
                end = make_number(XINT(XCAR(XCDR(item))) + XINT(delta));
                plist = XCAR(XCDR(XCDR(item)));

                tem = F.add_text_properties(start, end, plist, obj);
                if (!NILP(tem))
                    modified_p = true;
            }

            return modified_p;
        }

        /* verify_interval_modification saves insertion hooks here
           to be run later by report_interval_modification.  */
        public static LispObject interval_insert_behind_hooks;
        public static LispObject interval_insert_in_front_hooks;

        /* Check for read-only intervals between character positions START ... END,
           in BUF, and signal an error if we find one.

           Then check for any modification hooks in the range.
           Create a list of all these hooks in lexicographic order,
           eliminating consecutive extra copies of the same hook.  Then call
           those hooks in order, with START and END - 1 as arguments.  */
        public static void verify_interval_modification(Buffer buf, int start, int end)
        {
            Interval intervals = BUF_INTERVALS(buf);
            Interval i;
            LispObject hooks;
            LispObject prev_mod_hooks;
            LispObject mod_hooks;

            hooks = Q.nil;
            prev_mod_hooks = Q.nil;
            mod_hooks = Q.nil;

            interval_insert_behind_hooks = Q.nil;
            interval_insert_in_front_hooks = Q.nil;

            if (NULL_INTERVAL_P(intervals))
                return;

            if (start > end)
            {
                int temp = start;
                start = end;
                end = temp;
            }

            /* For an insert operation, check the two chars around the position.  */
            if (start == end)
            {
                Interval prev = null;
                LispObject before, after;

                /* Set I to the interval containing the char after START,
               and PREV to the interval containing the char before START.
               Either one may be null.  They may be equal.  */
                i = find_interval(intervals, start);

                if (start == BUF_BEGV(buf))
                    prev = null;
                else if (i.position == start)
                    prev = previous_interval(i);
                else if (i.position < start)
                    prev = i;
                if (start == BUF_ZV(buf))
                    i = null;

                /* If Vinhibit_read_only is set and is not a list, we can
               skip the read_only checks.  */
                if (NILP(V.inhibit_read_only) || CONSP(V.inhibit_read_only))
                {
                    /* If I and PREV differ we need to check for the read-only
                       property together with its stickiness.  If either I or
                       PREV are 0, this check is all we need.
                       We have to take special care, since read-only may be
                       indirectly defined via the category property.  */
                    if (i != prev)
                    {
                        if (!NULL_INTERVAL_P(i))
                        {
                            after = textget(i.plist, Q.read_only);

                            /* If interval I is read-only and read-only is
                               front-sticky, inhibit insertion.
                               Check for read-only as well as category.  */
                            if (!NILP(after)
                                && NILP(F.memq(after, V.inhibit_read_only)))
                            {
                                LispObject tem;

                                tem = textget(i.plist, Q.front_sticky);
                                if (TMEM(Q.read_only, tem)
                                || (NILP(F.plist_get(i.plist, Q.read_only))
                                    && TMEM(Q.category, tem)))
                                    text_read_only(after);
                            }
                        }

                        if (!NULL_INTERVAL_P(prev))
                        {
                            before = textget(prev.plist, Q.read_only);

                            /* If interval PREV is read-only and read-only isn't
                               rear-nonsticky, inhibit insertion.
                               Check for read-only as well as category.  */
                            if (!NILP(before)
                                && NILP(F.memq(before, V.inhibit_read_only)))
                            {
                                LispObject tem;

                                tem = textget(prev.plist, Q.rear_nonsticky);
                                if (!TMEM(Q.read_only, tem)
                                && (!NILP(F.plist_get(prev.plist, Q.read_only))
                                    || !TMEM(Q.category, tem)))
                                    text_read_only(before);
                            }
                        }
                    }
                    else if (!NULL_INTERVAL_P(i))
                    {
                        after = textget(i.plist, Q.read_only);

                        /* If interval I is read-only and read-only is
                       front-sticky, inhibit insertion.
                       Check for read-only as well as category.  */
                        if (!NILP(after) && NILP(F.memq(after, V.inhibit_read_only)))
                        {
                            LispObject tem;

                            tem = textget(i.plist, Q.front_sticky);
                            if (TMEM(Q.read_only, tem)
                                || (NILP(F.plist_get(i.plist, Q.read_only))
                                && TMEM(Q.category, tem)))
                                text_read_only(after);

                            tem = textget(prev.plist, Q.rear_nonsticky);
                            if (!TMEM(Q.read_only, tem)
                                && (!NILP(F.plist_get(prev.plist, Q.read_only))
                                || !TMEM(Q.category, tem)))
                                text_read_only(after);
                        }
                    }
                }

                /* Run both insert hooks (just once if they're the same).  */
                if (!NULL_INTERVAL_P(prev))
                    interval_insert_behind_hooks
                      = textget(prev.plist, Q.insert_behind_hooks);
                if (!NULL_INTERVAL_P(i))
                    interval_insert_in_front_hooks
                      = textget(i.plist, Q.insert_in_front_hooks);
            }
            else
            {
                /* Loop over intervals on or next to START...END,
               collecting their hooks.  */

                i = find_interval(intervals, start);
                do
                {
                    if (!INTERVAL_WRITABLE_P(i))
                        text_read_only(textget(i.plist, Q.read_only));

                    if (!inhibit_modification_hooks)
                    {
                        mod_hooks = textget(i.plist, Q.modification_hooks);
                        if (!NILP(mod_hooks) && !EQ(mod_hooks, prev_mod_hooks))
                        {
                            hooks = F.cons(mod_hooks, hooks);
                            prev_mod_hooks = mod_hooks;
                        }
                    }

                    i = next_interval(i);
                }
                /* Keep going thru the interval containing the char before END.  */
                while (!NULL_INTERVAL_P(i) && i.position < end);

                if (!inhibit_modification_hooks)
                {
                    hooks = F.nreverse(hooks);
                    while (!EQ(hooks, Q.nil))
                    {
                        call_mod_hooks(F.car(hooks), make_number(start),
                                make_number(end));
                        hooks = F.cdr(hooks);
                    }
                }
            }
        }

        /* Run the interval hooks for an insertion on character range START ... END.
           verify_interval_modification chose which hooks to run;
           this function is called after the insertion happens
           so it can indicate the range of inserted text.  */
        public static void report_interval_modification (LispObject start, LispObject end)
        {
            if (!NILP(interval_insert_behind_hooks))
                call_mod_hooks(interval_insert_behind_hooks, start, end);
            if (!NILP(interval_insert_in_front_hooks) &&
                !EQ(interval_insert_in_front_hooks, interval_insert_behind_hooks))
                call_mod_hooks(interval_insert_in_front_hooks, start, end);
        }

        /* Call the modification hook functions in LIST, each with START and END.  */
        public static void call_mod_hooks (LispObject list, LispObject start, LispObject end)
        {
            while (!NILP(list))
            {
                call2(F.car(list), start, end);
                list = F.cdr(list);
            }
        }

        /* Replace properties of text from START to END with new list of
           properties PROPERTIES.  BUFFER is the buffer containing
           the text.  This does not obey any hooks.
           You can provide the interval that START is located in as I,
           or pass NULL for I and this function will find it.
           START and END can be in any order.  */
        public static void set_text_properties_1(LispObject start, LispObject end, LispObject properties, LispObject buffer, Interval i)
        {
            Interval prev_changed = NULL_INTERVAL;
            int s, len;
            Interval unchanged;

            s = XINT(start);
            len = XINT(end) - s;
            if (len == 0)
                return;
            if (len < 0)
            {
                s = s + len;
                len = -len;
            }

            if (i == null)
                i = find_interval(BUF_INTERVALS(XBUFFER(buffer)), s);

            if (i.position != s)
            {
                unchanged = i;
                i = split_interval_right(unchanged, s - (int) unchanged.position);

                if (LENGTH(i) > len)
                {
                    copy_properties(unchanged, i);
                    i = split_interval_left(i, len);
                    set_properties(properties, i, buffer);
                    return;
                }

                set_properties(properties, i, buffer);

                if (LENGTH(i) == len)
                    return;

                prev_changed = i;
                len -= (int)LENGTH(i);
                i = next_interval(i);
            }

            /* We are starting at the beginning of an interval, I */
            while (len > 0)
            {
                if (i == null)
                    abort();

                if (LENGTH(i) >= len)
                {
                    if (LENGTH(i) > len)
                        i = split_interval_left(i, len);

                    /* We have to call set_properties even if we are going to
                       merge the intervals, so as to make the undo records
                       and cause redisplay to happen.  */
                    set_properties(properties, i, buffer);
                    if (!NULL_INTERVAL_P(prev_changed))
                        merge_interval_left(i);
                    return;
                }

                len -= (int) LENGTH(i);

                /* We have to call set_properties even if we are going to
               merge the intervals, so as to make the undo records
               and cause redisplay to happen.  */
                set_properties(properties, i, buffer);
                if (NULL_INTERVAL_P(prev_changed))
                    prev_changed = i;
                else
                    prev_changed = i = merge_interval_left(i);

                i = next_interval(i);
            }
        }

        /* If o1 is a cons whose cdr is a cons, return non-zero and set o2 to
           the o1's cdr.  Otherwise, return zero.  This is handy for
           traversing plists.  */
        public static bool PLIST_ELT_P(LispObject o1, ref LispObject o2)
        {
            if (!CONSP(o1))
                return false;
            o2 = XCDR(o1);
            return CONSP(o2);
        }

        /* Changing the plists of individual intervals.  */

        /* Return the value of PROP in property-list PLIST, or Qunbound if it
           has none.  */
        public static LispObject property_value (LispObject plist, LispObject prop)
        {
            LispObject value = null;

            while (PLIST_ELT_P(plist, ref value))
                if (EQ(XCAR(plist), prop))
                    return XCAR(value);
                else
                    plist = XCDR(value);

            return Q.unbound;
        }

        /* Set the properties of INTERVAL to PROPERTIES,
           and record undo info for the previous values.
           OBJECT is the string or buffer that INTERVAL belongs to.  */
        public static void set_properties(LispObject properties, Interval interval, LispObject obj)
        {
            LispObject sym, value = null;

            if (BUFFERP(obj))
            {
                /* For each property in the old plist which is missing from PROPERTIES,
               or has a different value in PROPERTIES, make an undo record.  */
                for (sym = interval.plist;
                 PLIST_ELT_P(sym, ref value);
                 sym = XCDR(value))
                    if (!EQ(property_value(properties, XCAR(sym)),
                          XCAR(value)))
                    {
                        record_property_change((int) interval.position, (int) LENGTH(interval),
                                    XCAR(sym), XCAR(value),
                                    obj);
                    }

                /* For each new property that has no value at all in the old plist,
               make an undo record binding it to nil, so it will be removed.  */
                for (sym = properties;
                 PLIST_ELT_P(sym, ref value);
                 sym = XCDR(value))
                    if (EQ(property_value(interval.plist, XCAR(sym)), Q.unbound))
                    {
                        record_property_change((int) interval.position, (int) LENGTH(interval),
                                    XCAR(sym), Q.nil,
                                    obj);
                    }
            }

            /* Store new properties.  */
            interval.plist = F.copy_sequence(properties);
        }

        /* I don't think this is the right interface to export; how often do you
           want to do something like this, other than when you're copying objects
           around?

           I think it would be better to have a pair of functions, one which
           returns the text properties of a region as a list of ranges and
           plists, and another which applies such a list to another object.  */

        /* Add properties from SRC to SRC of SRC, starting at POS in DEST.
           SRC and DEST may each refer to strings or buffers.
           Optional sixth argument PROP causes only that property to be copied.
           Properties are copied to DEST as if by `add-text-properties'.
           Return t if any property value actually changed, nil otherwise.  */

        /* Note this can GC when DEST is a buffer.  */
        public static LispObject copy_text_properties (LispObject start, LispObject end, LispObject src, LispObject pos, LispObject dest, LispObject prop)
        {
            Interval i;
            LispObject res;
            LispObject stuff;
            LispObject plist;
            int s, e, e2, p, len;
            int modified = 0;

            i = validate_interval_range(src, ref start, ref end, soft);
            if (NULL_INTERVAL_P(i))
                return Q.nil;

            CHECK_NUMBER_COERCE_MARKER(ref pos);
            {
                LispObject dest_start, dest_end;

                dest_start = pos;
                dest_end = XSETINT( XINT(dest_start) + (XINT(end) - XINT(start)));
                /* Apply this to a copy of pos; it will try to increment its arguments,
                   which we don't want.  */
                validate_interval_range(dest, ref dest_start, ref dest_end, soft);
            }

            s = XINT(start);
            e = XINT(end);
            p = XINT(pos);

            stuff = Q.nil;

            while (s < e)
            {
                e2 = (int) (i.position + LENGTH(i));
                if (e2 > e)
                    e2 = e;
                len = e2 - s;

                plist = i.plist;
                if (!NILP(prop))
                    while (!NILP(plist))
                    {
                        if (EQ(F.car(plist), prop))
                        {
                            plist = F.cons(prop, F.cons(F.car(F.cdr(plist)), Q.nil));
                            break;
                        }
                        plist = F.cdr(F.cdr(plist));
                    }
                if (!NILP(plist))
                {
                    /* Must defer modifications to the interval tree in case src
                       and dest refer to the same string or buffer.  */
                    stuff = F.cons(F.cons(make_number(p),
                F.cons(make_number(p + len),
                       F.cons(plist, Q.nil))),
            stuff);
                }

                i = next_interval(i);
                if (NULL_INTERVAL_P(i))
                    break;

                p += len;
                s = (int) i.position;
            }

            while (!NILP(stuff))
            {
                res = F.car(stuff);
                res = F.add_text_properties(F.car(res), F.car(F.cdr(res)),
                  F.car(F.cdr(F.cdr(res))), dest);
                if (!NILP(res))
                    modified++;
                stuff = F.cdr(stuff);
            }

            return modified != 0 ? Q.t : Q.nil;
        }

/* Returns the interval of POSITION in OBJECT.
   POSITION is BEG-based.  */
        public static Interval interval_of(int position, LispObject obj)
        {
            Interval i;
            int beg, end;

            if (NILP(obj))
                obj = current_buffer;
            else if (EQ(obj, Q.t))
                return NULL_INTERVAL;

            CHECK_STRING_OR_BUFFER(obj);

            if (BUFFERP(obj))
            {
                Buffer b = XBUFFER(obj);

                beg = BUF_BEGV(b);
                end = BUF_ZV(b);
                i = BUF_INTERVALS(b);
            }
            else
            {
                beg = 0;
                end = SCHARS(obj);
                i = STRING_INTERVALS(obj);
            }

            if (!(beg <= position && position <= end))
                args_out_of_range(make_number(position), make_number(position));
            if (beg == end || NULL_INTERVAL_P(i))
                return NULL_INTERVAL;

            return find_interval(i, position);
        }
    }
}