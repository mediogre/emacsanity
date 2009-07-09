namespace IronElisp
{
    public partial class V
    {
        public static LispObject features
        {
            get { return Defs.O[(int)Objects.features]; }
            set { Defs.O[(int)Objects.features] = value; }
        }
    }

    public partial class L
    {
        /* Like Fassq but never report an error and do not allow quits.
           Use only on lists known never to be circular.  */
        public static LispObject assq_no_quit(LispObject key, LispObject list)
        {
            while (CONSP(list)
               && (!CONSP(XCAR(list))
                   || !EQ(XCAR(XCAR(list)), key)))
                list = XCDR(list);

            return CAR_SAFE(list);
        }

        /* DEPTH is current depth of recursion.  Signal an error if it
           gets too deep.
           PROPS, if non-nil, means compare string text properties too.  */
        public static bool internal_equal (LispObject o1, LispObject o2, int depth, bool props)
        {
            if (depth > 200)
                error ("Stack overflow in equal");

            tail_recurse:
            QUIT ();

            if (EQ (o1, o2))
                return true;

            if (XTYPE (o1) != XTYPE (o2))
                return false;

            if (o1 is LispCons)
            {
                if (!internal_equal (XCAR (o1), XCAR (o2), depth + 1, props))
                    return false;
                o1 = XCDR (o1);
                o2 = XCDR (o2);
                goto tail_recurse;
            }

            if (o1 is LispString)
            {
                if (SCHARS (o1) != SCHARS (o2))
                    return false;
                // COMEBACK_WHEN_READY!!!
                // if (SBYTES (o1) != SBYTES (o2))
                // return 0;
                if (SDATA (o1) != SDATA (o2))
                    return false;
                if (props && !compare_string_intervals (o1, o2))
                    return false;
                return true;
            }

            if (MARKERP (o1))
            {
                return (XMARKER (o1).buffer == XMARKER (o2).buffer
                        && (XMARKER (o1).buffer == null
                                || XMARKER (o1).bytepos == XMARKER (o2).bytepos));
            }

            if (OVERLAYP (o1))
            {
                if (!internal_equal (OVERLAY_START (o1), OVERLAY_START (o2),
                                     depth + 1, props)
                    || !internal_equal (OVERLAY_END (o1), OVERLAY_END (o2),
                                        depth + 1, props))
                    return false;
                o1 = XOVERLAY (o1).plist;
                o2 = XOVERLAY (o2).plist;
                goto tail_recurse;
            }

            // COMEBACK_WHEN_READY!!!
            /*
            switch (XTYPE (o1))
            {
            case Lisp_Float:
                {
                    double d1, d2;

                    d1 = extract_float (o1);
                    d2 = extract_float (o2);
                    // If d is a NaN, then d != d. Two NaNs should be `equal' even
                    // though they are not =. 
                    return d1 == d2 || (d1 != d1 && d2 != d2);
                }

            case Lisp_Vectorlike:
                {
                    register int i;
                    EMACS_INT size = ASIZE (o1);
                    // Pseudovectors have the type encoded in the size field, so this test
                    // actually checks that the objects have the same type as well as the
                    // same size.  
                    if (ASIZE (o2) != size)
                        return 0;
                    // Boolvectors are compared much like strings.  
                    if (BOOL_VECTOR_P (o1))
                    {
                        int size_in_chars
                            = ((XBOOL_VECTOR (o1)->size + BOOL_VECTOR_BITS_PER_CHAR - 1)
                               / BOOL_VECTOR_BITS_PER_CHAR);

                        if (XBOOL_VECTOR (o1)->size != XBOOL_VECTOR (o2)->size)
                            return 0;
                        if (bcmp (XBOOL_VECTOR (o1)->data, XBOOL_VECTOR (o2)->data,
                                  size_in_chars))
                            return 0;
                        return 1;
                    }
                    if (WINDOW_CONFIGURATIONP (o1))
                        return compare_window_configurations (o1, o2, 0);

                    // Aside from them, only true vectors, char-tables, compiled
                    //   functions, and fonts (font-spec, font-entity, font-ojbect)
                    //   are sensible to compare, so eliminate the others now.  
                    if (size & PSEUDOVECTOR_FLAG)
                    {
                        if (!(size & (PVEC_COMPILED
                                      | PVEC_CHAR_TABLE | PVEC_SUB_CHAR_TABLE | PVEC_FONT)))
                            return 0;
                        size &= PSEUDOVECTOR_SIZE_MASK;
                    }
                    for (i = 0; i < size; i++)
                    {
                        Lisp_Object v1, v2;
                        v1 = AREF (o1, i);
                        v2 = AREF (o2, i);
                        if (!internal_equal (v1, v2, depth + 1, props))
                            return 0;
                    }
                    return 1;
                }
                break;
            }
            */

            return false;
        }
    }

    public partial class F
    {
        public static LispObject equal (LispObject o1, LispObject o2)
        {
            return L.internal_equal (o1, o2, 0, false) ? Q.t : Q.nil;
        }
        
        public static LispObject string_equal(LispObject s1, LispObject s2)
        {
            if (L.SYMBOLP(s1))
                s1 = L.SYMBOL_NAME(s1);
            if (L.SYMBOLP(s2))
                s2 = L.SYMBOL_NAME(s2);
            L.CHECK_STRING(s1);
            L.CHECK_STRING(s2);

            if (L.SCHARS(s1) != L.SCHARS(s2)
                // COMEBACK_WHEN_READY
                // || SBYTES(s1) != SBYTES(s2)
                 || L.SDATA(s1) != L.SDATA(s2)
               )
            {
                return Q.nil;
            }
            return Q.t;
        }

        public static LispObject memq(LispObject elt, LispObject list)
        {
            while (true)
            {
                if (!L.CONSP(list) || L.EQ(L.XCAR(list), elt))
                    break;

                list = L.XCDR(list);
                if (!L.CONSP(list) || L.EQ(L.XCAR(list), elt))
                    break;

                list = L.XCDR(list);
                if (!L.CONSP(list) || L.EQ(L.XCAR(list), elt))
                    break;

                list = L.XCDR(list);
                L.QUIT();
            }

            L.CHECK_LIST(list);
            return list;
        }

        public static LispObject assq(LispObject key, LispObject list)
        {
            while (true)
            {
                if (!L.CONSP(list)
                || (L.CONSP(L.XCAR(list))
                    && L.EQ(L.XCAR(L.XCAR(list)), key)))
                    break;

                list = L.XCDR(list);
                if (!L.CONSP(list)
                || (L.CONSP(L.XCAR(list))
                    && L.EQ(L.XCAR(L.XCAR(list)), key)))
                    break;

                list = L.XCDR(list);
                if (!L.CONSP(list)
                || (L.CONSP(L.XCAR(list))
                    && L.EQ(L.XCAR(L.XCAR(list)), key)))
                    break;

                list = L.XCDR(list);
                L.QUIT();
            }

            return L.CAR(list);
        }

        /* This does not check for quits.  That is safe since it must terminate.  */
        public static LispObject plist_get(LispObject plist, LispObject prop)
        {
            LispObject tail, halftail;

            /* halftail is used to detect circular lists.  */
            tail = halftail = plist;
            while (L.CONSP(tail) && L.CONSP(L.XCDR(tail)))
            {
                if (L.EQ(prop, L.XCAR(tail)))
                    return L.XCAR(L.XCDR(tail));

                tail = L.XCDR(L.XCDR(tail));
                halftail = L.XCDR(halftail);
                if (L.EQ(tail, halftail))
                    break;
            }

            return Q.nil;
        }

        public static LispObject get(LispObject symbol, LispObject propname)
        {
            L.CHECK_SYMBOL(symbol);
            return plist_get(L.XSYMBOL(symbol).plist, propname);
        }

        public static LispObject plist_put(LispObject plist, LispObject prop, LispObject val)
        {
            LispObject tail, prev;
            LispObject newcell;
            prev = Q.nil;
            for (tail = plist; L.CONSP(tail) && L.CONSP(L.XCDR(tail));
                 tail = L.XCDR(L.XCDR(tail)))
            {
                if (L.EQ(prop, L.XCAR(tail)))
                {
                    F.setcar(L.XCDR(tail), val);
                    return plist;
                }

                prev = tail;
                L.QUIT();
            }
            newcell = F.cons(prop, F.cons(val, L.NILP(prev) ? plist : L.XCDR(L.XCDR(prev))));
            if (L.NILP(prev))
                return newcell;
            else
                F.setcdr(L.XCDR(prev), newcell);
            return plist;
        }

        public static LispObject put(LispObject symbol, LispObject propname, LispObject value)
        {
            L.CHECK_SYMBOL(symbol);
            L.XSYMBOL(symbol).plist = F.plist_put (L.XSYMBOL (symbol).plist, propname, value);
            return value;
        }

        public static LispObject length (LispObject sequence)
        {
            LispObject val;

            if (L.STRINGP (sequence))
                val = L.make_number(L.SCHARS (sequence));
            else if (L.VECTORP (sequence))
                val = L.make_number(L.ASIZE(sequence));
            else if (L.CHAR_TABLE_P (sequence))
                val = L.make_number(L.MAX_CHAR);
            else if (L.BOOL_VECTOR_P (sequence))
                val = L.make_number(L.XBOOL_VECTOR(sequence).size);
            else if (L.COMPILEDP (sequence))
                val = L.make_number((sequence as LispCompiled).Size);
            else if (L.CONSP (sequence))
            {
                int i = 0;
                while (L.CONSP (sequence))
                {
                    sequence = L.XCDR (sequence);
                    ++i;
                    
                    if (!L.CONSP (sequence))
                        break;

                    sequence = L.XCDR (sequence);
                    ++i;
                    L.QUIT();
                }

                L.CHECK_LIST_END(sequence, sequence);

                val = L.make_number (i);
            }
            else if (L.NILP(sequence))
                val = L.make_number(0);
            else
            {
                L.wrong_type_argument(Q.sequencep, sequence);
                return Q.nil;
            }

            return val;
        }

        public static LispObject nthcdr(LispObject n, LispObject list)
        {
            int i, num;
            L.CHECK_NUMBER(n);
            num = L.XINT(n);
            for (i = 0; i < num && !L.NILP(list); i++)
            {
                L.QUIT();
                L.CHECK_LIST_CONS(list, list);
                list = L.XCDR(list);
            }
            return list;
        }

        public static LispObject nth(LispObject n, LispObject list)
        {
            return F.car(F.nthcdr(n, list));
        }
    }
}