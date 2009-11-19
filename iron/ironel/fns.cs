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
        public static LispObject concat2(LispObject s1, LispObject s2)
        {
            return concat(2, new LispObject[] { s1, s2 }, typeof(LispString), false);
        }

        /* This structure holds information of an argument of `concat' that is
           a string and has text properties to be copied.  */
        class textprop_rec
        {
            public int argnum;			/* refer to ARGS (arguments of `concat') */
            public int from;			/* refer to ARGS[argnum] (argument string) */
            public int to;			/* refer to VAL (the target string) */
        }

        public static LispObject concat (int nargs, LispObject[] args, System.Type target_type, bool last_special)
        {
            LispObject val;
            LispObject tail;
            LispObject thiss;
            int toindex;
            int toindex_byte = 0;
            int result_len;
            int result_len_byte;
            int argnum;
            LispObject last_tail;
            LispObject prev;
            bool some_multibyte;

            /* When we make a multibyte string, we can't copy text properties
               while concatinating each string because the length of resulting
               string can't be decided until we finish the whole concatination.
               So, we record strings that have text properties to be copied
               here, and copy the text properties after the concatination.  */
             textprop_rec[]  textprops = null;
            /* Number of elments in textprops.  */
            int num_textprops = 0;
            // USE_SAFE_ALLOCA;

            tail = Q.nil;

            /* In append, the last arg isn't treated like the others */
            if (last_special && nargs > 0)
            {
                nargs--;
                last_tail = args[nargs];
            }
            else
                last_tail = Q.nil;

            /* Check each argument.  */
            for (argnum = 0; argnum < nargs; argnum++)
            {
                thiss = args[argnum];
                if (!(CONSP (thiss) || NILP (thiss) || VECTORP (thiss) || STRINGP (thiss)
                      || COMPILEDP (thiss) || BOOL_VECTOR_P (thiss)))
                    wrong_type_argument (Q.sequencep, thiss);
            }

            /* Compute total length in chars of arguments in RESULT_LEN.
               If desired output is a string, also compute length in bytes
               in RESULT_LEN_BYTE, and determine in SOME_MULTIBYTE
               whether the result should be a multibyte string.  */
            result_len_byte = 0;
            result_len = 0;
            some_multibyte = false;
            for (argnum = 0; argnum < nargs; argnum++)
            {
                int len;
                thiss = args[argnum];
                len = XINT (F.length (thiss));
                if (target_type == typeof(LispString))
                {
                    /* We must count the number of bytes needed in the string
                       as well as the number of characters.  */
                    int i;
                    LispObject ch;
                    int this_len_byte;

                    if (VECTORP (thiss))
                        for (i = 0; i < len; i++)
                        {
                            ch = AREF (thiss, i);
                            CHECK_CHARACTER (ch);
                            this_len_byte = CHAR_BYTES ((uint) XINT (ch));
                            result_len_byte += this_len_byte;
                            if (! ASCII_CHAR_P ((uint) XINT (ch)) && ! CHAR_BYTE8_P ((uint) XINT (ch)))
                                some_multibyte = true;
                        }
                    else if (BOOL_VECTOR_P (thiss) && XBOOL_VECTOR (thiss).Size > 0)
                        wrong_type_argument (Q.integerp, F.aref (thiss, make_number (0)));
                    else if (CONSP (thiss))
                        for (; CONSP (thiss); thiss = XCDR (thiss))
                        {
                            ch = XCAR (thiss);
                            CHECK_CHARACTER (ch);
                            this_len_byte = CHAR_BYTES ((uint) XINT (ch));
                            result_len_byte += this_len_byte;
                            if (! ASCII_CHAR_P ((uint) XINT (ch)) && ! CHAR_BYTE8_P ((uint) XINT (ch)))
                                some_multibyte = true;
                        }
                    else if (STRINGP (thiss))
                    {
                        if (STRING_MULTIBYTE (thiss))
                        {
                            some_multibyte = true;
                            result_len_byte += SBYTES (thiss);
                        }
                        else
                            result_len_byte += count_size_as_multibyte (SDATA (thiss),
                                                                        SCHARS (thiss));
                    }
                }

                result_len += len;
                if (result_len < 0)
                    error ("String overflow");
            }

            if (! some_multibyte)
                result_len_byte = result_len;

            /* Create the output object.  */
            if (target_type == typeof(LispCons))
            {
                val = F.make_list (make_number (result_len), Q.nil);
            }
                // FIXME: we are not using LispVector to piggyback other vector-like things
                // FIXME: so we'll need to make_xxx accordingly
            else if (target_type == typeof(LispVector))
            {
                val = F.make_vector (make_number (result_len), Q.nil);
            }
            else if (some_multibyte)
            {
                val = make_uninit_multibyte_string (result_len, result_len_byte);
            }
            else
            {
                val = make_uninit_string (result_len);
            }

            /* In `append', if all but last arg are nil, return last arg.  */
            if (target_type == typeof(LispCons) && EQ (val, Q.nil))
                return last_tail;

            /* Copy the contents of the args into the result.  */
            if (CONSP (val))
            {
                tail = val;
                toindex = -1; /* -1 in toindex is flag we are making a list */
            }
            else
            {
                toindex = 0;
                toindex_byte = 0;
            }

            prev = Q.nil;
            if (STRINGP (val))
                textprops = new textprop_rec[nargs];

            for (argnum = 0; argnum < nargs; argnum++)
            {
                LispObject thislen;
                int thisleni = 0;
                uint thisindex = 0;
                uint thisindex_byte = 0;

                thiss = args[argnum];
                if (!CONSP (thiss))
                {
                    thislen = F.length (thiss);
                    thisleni = XINT (thislen);
                }

                /* Between strings of the same kind, copy fast.  */
                if (STRINGP (thiss) && STRINGP (val)
                    && STRING_MULTIBYTE (thiss) == some_multibyte)
                {
                    int thislen_byte = SBYTES (thiss);

                    XSTRING(val).bcopy(SDATA(thiss), toindex_byte, SBYTES(thiss));
                    // bcopy(SDATA(thiss), SDATA(val) + toindex_byte, SBYTES(thiss));
                    if (! NULL_INTERVAL_P (STRING_INTERVALS (thiss)))
                    {
                        textprops[num_textprops].argnum = argnum;
                        textprops[num_textprops].from = 0;
                        textprops[num_textprops++].to = toindex;
                    }
                    toindex_byte += thislen_byte;
                    toindex += thisleni;
                    STRING_SET_CHARS (val, SCHARS (val));
                }
                /* Copy a single-byte string to a multibyte string.  */
                else if (STRINGP (thiss) && STRINGP (val))
                {
                    if (! NULL_INTERVAL_P (STRING_INTERVALS (thiss)))
                    {
                        textprops[num_textprops].argnum = argnum;
                        textprops[num_textprops].from = 0;
                        textprops[num_textprops++].to = toindex;
                    }
                    toindex_byte += copy_text (SDATA (thiss),
                                               SDATA (val), toindex_byte,
                                               SCHARS (thiss), false, true);
                    toindex += thisleni;
                }
                else
                    /* Copy element by element.  */
                    while (true)
                    {
                        LispObject elt;

                        /* Fetch next element of `this' arg into `elt', or break if
                           `this' is exhausted. */
                        if (NILP (thiss)) break;
                        if (CONSP (thiss))
                        {
                            elt = XCAR (thiss);
                            thiss = XCDR (thiss);
                        }
                        else if (thisindex >= thisleni)
                            break;
                        else if (STRINGP (thiss))
                        {
                            int c = 0;
                            if (STRING_MULTIBYTE (thiss))
                            {
                                int d1 = (int) thisindex;
                                int d2 = (int) thisindex_byte;
                                FETCH_STRING_CHAR_ADVANCE_NO_CHECK (ref c, thiss,
                                                                    ref d1,
                                                                    ref d2);
                                thisindex = (uint) d1;
                                thisindex_byte = (uint) d2;

                                elt = XSETINT (c);
                            }
                            else
                            {
                                elt = XSETINT (SREF (thiss, (int) thisindex)); thisindex++;
                                if (some_multibyte
                                    && XINT (elt) >= 0200
                                    && XINT (elt) < 0400)
                                {
                                    c = (int) unibyte_char_to_multibyte ((uint) XINT (elt));
                                    elt = XSETINT (c);
                                }
                            }
                        }
                        else if (BOOL_VECTOR_P (thiss))
                        {
                            int bte;
                            bte = XBOOL_VECTOR (thiss)[(int) (thisindex / LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR)];
                            if ((bte & (1 << (int)(thisindex % LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR))) != 0)
                                elt = Q.t;
                            else
                                elt = Q.nil;
                            thisindex++;
                        }
                        else
                        {
                            elt = AREF (thiss, (int) thisindex);
                            thisindex++;
                        }

                        /* Store this element into the result.  */
                        if (toindex < 0)
                        {
                            XSETCAR (tail, elt);
                            prev = tail;
                            tail = XCDR (tail);
                        }
                        else if (VECTORP (val))
                        {
                            ASET (val, toindex, elt);
                            toindex++;
                        }
                        else
                        {
                            CHECK_NUMBER (elt);
                            if (some_multibyte)
                                toindex_byte += CHAR_STRING((uint) XINT(elt),
                                                             SDATA(val), toindex_byte);
                            else
                            {
                                SSET(val, (int)toindex_byte, (byte) XINT(elt));
                                toindex_byte++;
                            }
                            toindex++;
                        }
                    }
            }
            if (!NILP (prev))
                XSETCDR (prev, last_tail);

            if (num_textprops > 0)
            {
                LispObject props;
                int last_to_end = -1;

                for (argnum = 0; argnum < num_textprops; argnum++)
                {
                    thiss = args[textprops[argnum].argnum];
                    props = text_property_list (thiss,
                                                make_number (0),
                                                make_number (SCHARS (thiss)),
                                                Q.nil);
                    /* If successive arguments have properites, be sure that the
                       value of `composition' property be the copy.  */
                    if (last_to_end == textprops[argnum].to)
                        make_composition_value_copy (props);
                    add_text_properties_from_list (val, props,
                                                   make_number (textprops[argnum].to));
                    last_to_end = textprops[argnum].to + SCHARS (thiss);
                }
            }

            return val;
        }
        
        public static LispObject string_char_byte_cache_string;
        public static int string_char_byte_cache_charpos;
        public static int string_char_byte_cache_bytepos;

        public static void clear_string_char_byte_cache()
        {
            string_char_byte_cache_string = Q.nil;
        }

        /* Return the byte index corresponding to CHAR_INDEX in STRING.  */
        public static int string_char_to_byte (LispObject stringg, int char_index)
        {
            int i_byte;
            int best_below, best_below_byte;
            int best_above, best_above_byte;

            best_below = best_below_byte = 0;
            best_above = SCHARS (stringg);
            best_above_byte = SBYTES (stringg);
            if (best_above == best_above_byte)
                return char_index;

            if (EQ (stringg, string_char_byte_cache_string))
            {
                if (string_char_byte_cache_charpos < char_index)
                {
                    best_below = string_char_byte_cache_charpos;
                    best_below_byte = string_char_byte_cache_bytepos;
                }
                else
                {
                    best_above = string_char_byte_cache_charpos;
                    best_above_byte = string_char_byte_cache_bytepos;
                }
            }

            if (char_index - best_below < best_above - char_index)
            {
                int p = best_below_byte;

                while (best_below < char_index)
                {
                    p += BYTES_BY_CHAR_HEAD (SDATA(stringg)[p]);
                    best_below++;
                }
                i_byte = p;
            }
            else
            {
                int p = best_above_byte;

                while (best_above > char_index)
                {
                    p--;
                    while (!CHAR_HEAD_P (SDATA (stringg)[p])) p--;
                    best_above--;
                }
                i_byte = p;
            }

            string_char_byte_cache_bytepos = i_byte;
            string_char_byte_cache_charpos = char_index;
            string_char_byte_cache_string = stringg;

            return i_byte;
        }

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
                if (SBYTES(o1) != SBYTES(o2))
                    return false;
                if (! XSTRING(o1).bcmp(SDATA(o2)))
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

        public static int hash_lookup(LispHash h, LispObject key)
        {
            uint stupid = 0;
            return hash_lookup(h, key, ref stupid);
        }

        /* Lookup KEY in hash table H.  If HASH is non-null, return in *HASH
           the hash code of KEY.  Value is the index of the entry in H
           matching KEY, or -1 if not found.  */
        public static int hash_lookup (LispHash h, LispObject key, ref uint hash)
        {
#if COMEBACK_LATER
  unsigned hash_code;
  int start_of_bucket;
  Lisp_Object idx;

  hash_code = h->hashfn (h, key);
  if (hash)
    *hash = hash_code;

  start_of_bucket = hash_code % ASIZE (h->index);
  idx = HASH_INDEX (h, start_of_bucket);

  while (!NILP (idx))
    {
      int i = XFASTINT (idx);
      if (EQ (key, HASH_KEY (h, i))
	  || (h->cmpfn
	      && h->cmpfn (h, key, hash_code,
			   HASH_KEY (h, i), XUINT (HASH_HASH (h, i)))))
	break;
      idx = HASH_NEXT (h, i);
    }

  return NILP (idx) ? -1 : XFASTINT (idx);
#endif
            throw new System.Exception("No hash_lookup");
        }
    }

    public partial class F
    {
        public static LispObject string_as_unibyte (LispObject stringg)
        {
            L.CHECK_STRING (stringg);

            if (L.STRING_MULTIBYTE (stringg))
            {
                int bytes = L.SBYTES (stringg);
                byte[] str = new byte[bytes];

                System.Array.Copy(L.SDATA(stringg), str, bytes);
                bytes = L.str_as_unibyte (str, bytes);
                stringg = L.make_unibyte_string (str, bytes);
            }
            return stringg;
        }

        public static LispObject string_as_multibyte(LispObject stringg)
        {
            L.CHECK_STRING(stringg);

            if (!L.STRING_MULTIBYTE(stringg))
            {
                LispObject new_string;
                int nchars = 0, nbytes = 0;

                L.parse_str_as_multibyte(L.SDATA(stringg),
                                         L.SBYTES(stringg),
                                         ref nchars, ref nbytes);
                new_string = L.make_uninit_multibyte_string(nchars, nbytes);
                L.XSTRING(new_string).bcopy(L.SDATA(stringg), L.SBYTES(stringg));
                if (nbytes != L.SBYTES(stringg))
                {
                    int dummy = 0;
                    L.str_as_multibyte(L.SDATA(new_string), nbytes,
                                       L.SBYTES(stringg), ref dummy);
                }
                stringg = new_string;
                L.STRING_SET_INTERVALS(stringg, L.NULL_INTERVAL);
            }
            return stringg;
        }

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
                val = L.make_number((int) L.MAX_CHAR);
            else if (L.BOOL_VECTOR_P (sequence))
                val = L.make_number(L.XBOOL_VECTOR(sequence).Size);
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

        public static LispObject nreverse(LispObject list)
        {
            LispObject prev, tail, next;

            if (L.NILP(list)) return list;
            prev = Q.nil;
            tail = list;
            while (!L.NILP(tail))
            {
                L.QUIT();
                L.CHECK_LIST_CONS(tail, list);
                next = L.XCDR(tail);
                F.setcdr(tail, prev);
                prev = tail;
                tail = next;
            }
            return prev;
        }
    }
}