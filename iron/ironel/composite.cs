namespace IronElisp
{
    /* Methods to display a sequence of components of a composition.  */
    public enum composition_method
    {
        /* Compose relatively without alternate characters.  */
        COMPOSITION_RELATIVE,
        /* Compose by specified composition rules.  This is not used in
           Emacs 21 but we need it to decode files saved in the older
           versions of Emacs.  */
        COMPOSITION_WITH_RULE,
        /* Compose relatively with alternate characters.  */
        COMPOSITION_WITH_ALTCHARS,
        /* Compose by specified composition rules with alternate characters.  */
        COMPOSITION_WITH_RULE_ALTCHARS,
        /* This is not a method.  */
        COMPOSITION_NO
    }

    /* Data structure that records information about a composition
           currently used in some buffers or strings.

           When a composition is assigned an ID number (by
           get_composition_id), this structure is allocated for the
           composition and linked in composition_table[ID].

           Identical compositions appearing at different places have the same
           ID, and thus share the same instance of this structure.  */
    public class composition
    {
        /* Number of glyphs of the composition components.  */
        public uint glyph_len;

        /* Width, ascent, and descent pixels of the composition.  */
        public short pixel_width, ascent, descent;

        public short lbearing, rbearing;

        /* How many columns the overall glyphs occupy on the screen.  This
           gives an approximate value for column calculation in
           Fcurrent_column, and etc.  */
        public ushort width;

        /* Method of the composition.  */
        public composition_method method;

        /* Index to the composition hash table.  */
        public int hash_index;

        /* For which font we have calculated the remaining members.  The
           actual type is device dependent.  */
        public System.IntPtr font;

        /* Pointer to an array of x-offset and y-offset (by pixels) of
           glyphs.  This points to a sufficient memory space (sizeof (int) *
           glyph_len * 2) that is allocated when the composition is
           registered in composition_table.  X-offset and Y-offset of Nth
           glyph are (2N)th and (2N+1)th elements respectively.  */
        public short[] offsets;
    }

    /* Vector size of Lispy glyph.  */
    public enum lglyph_indices
    {
        LGLYPH_IX_FROM, LGLYPH_IX_TO, LGLYPH_IX_CHAR, LGLYPH_IX_CODE,
        LGLYPH_IX_WIDTH, LGLYPH_IX_LBEARING, LGLYPH_IX_RBEARING,
        LGLYPH_IX_ASCENT, LGLYPH_IX_DESCENT, LGLYPH_IX_ADJUSTMENT,
        /* Not an index.  */
        LGLYPH_SIZE
    }

    public partial class V
    {
        public static LispObject auto_composition_function
        {
            get { return Defs.O[(int)Objects.auto_composition_function]; }
            set { Defs.O[(int)Objects.auto_composition_function] = value; }
        }

        public static LispObject composition_function_table
        {
            get { return Defs.O[(int)Objects.composition_function_table]; }
            set { Defs.O[(int)Objects.composition_function_table] = value; }
        }
    }

    public partial class Q
    {
        /* Emacs uses special text property `composition' to support character
           composition.  A sequence of characters that have the same (i.e. eq)
           `composition' property value is treated as a single composite
           sequence (we call it just `composition' here after).  Characters in
           a composition are all composed somehow on the screen.

           The property value has this form when the composition is made:
            ((LENGTH . COMPONENTS) . MODIFICATION-FUNC)
           then turns to this form:
            (COMPOSITION-ID . (LENGTH COMPONENTS-VEC . MODIFICATION-FUNC))
           when the composition is registered in composition_hash_table and
           composition_table.  These rather peculiar structures were designed
           to make it easy to distinguish them quickly (we can do that by
           checking only the first element) and to extract LENGTH (from the
           former form) and COMPOSITION-ID (from the latter form).

           We register a composition when it is displayed, or when the width
           is required (for instance, to calculate columns).

           LENGTH -- Length of the composition.  This information is used to
            check the validity of the composition.

           COMPONENTS --  Character, string, vector, list, or nil.

            If it is nil, characters in the text are composed relatively
            according to their metrics in font glyphs.

            If it is a character or a string, the character or characters
            in the string are composed relatively.

            If it is a vector or list of integers, the element is a
            character or an encoded composition rule.  The characters are
            composed according to the rules.  (2N)th elements are
            characters to be composed and (2N+1)th elements are
            composition rules to tell how to compose (2N+2)th element with
            the previously composed 2N glyphs.

           COMPONENTS-VEC -- Vector of integers.  In relative composition, the
            elements are characters to be composed.  In rule-base
            composition, the elements are characters or encoded
            composition rules.

           MODIFICATION-FUNC -- If non nil, it is a function to call when the
            composition gets invalid after a modification in a buffer.  If
            it is nil, a function in `composition-function-table' of the
            first character in the sequence is called.

           COMPOSITION-ID --Identification number of the composition.  It is
            used as an index to composition_table for the composition.

           When Emacs has to display a composition or has to know its
           displaying width, the function get_composition_id is called.  It
           returns COMPOSITION-ID so that the caller can access the
           information about the composition through composition_table.  If a
           COMPOSITION-ID has not yet been assigned to the composition,
           get_composition_id checks the validity of `composition' property,
           and, if valid, assigns a new ID, registers the information in
           composition_hash_table and composition_table, and changes the form
           of the property value.  If the property is invalid, return -1
           without changing the property value.

           We use two tables to keep information about composition;
           composition_hash_table and composition_table.

           The former is a hash table in which keys are COMPONENTS-VECs and
           values are the corresponding COMPOSITION-IDs.  This hash table is
           weak, but as each key (COMPONENTS-VEC) is also kept as a value of the
           `composition' property, it won't be collected as garbage until all
           bits of text that have the same COMPONENTS-VEC are deleted.

           The latter is a table of pointers to `struct composition' indexed
           by COMPOSITION-ID.  This structure keeps the other information (see
           composite.h).

           In general, a text property holds information about individual
           characters.  But, a `composition' property holds information about
           a sequence of characters (in this sense, it is like the `intangible'
           property).  That means that we should not share the property value
           in adjacent compositions -- we can't distinguish them if they have the
           same property.  So, after any changes, we call
           `update_compositions' and change a property of one of adjacent
           compositions to a copy of it.  This function also runs a proper
           composition modification function to make a composition that gets
           invalid by the change valid again.

           As the value of the `composition' property holds information about a
           specific range of text, the value gets invalid if we change the
           text in the range.  We treat the `composition' property as always
           rear-nonsticky (currently by setting default-text-properties to
           (rear-nonsticky (composition))) and we never make properties of
           adjacent compositions identical.  Thus, any such changes make the
           range just shorter.  So, we can check the validity of the `composition'
           property by comparing LENGTH information with the actual length of
           the composition.

        */

        public static LispObject composition;
        public static LispObject auto_composed;
    }

    public partial class L
    {
        /* Maximum number of compoments a single composition can have.  */
        public const int MAX_COMPOSITION_COMPONENTS = 16;

        /* Mask bits for CHECK_MASK arg to update_compositions.
           For a change in the region FROM and TO, check compositions ... */
        public const int CHECK_HEAD = 1;	/* adjacent to FROM */
        public const int CHECK_TAIL = 2;	/* adjacent to TO */
        public const int CHECK_INSIDE = 4;	/* between FROM and TO */
        public const int CHECK_BORDER = (CHECK_HEAD | CHECK_TAIL);
        public const int CHECK_ALL = (CHECK_BORDER | CHECK_INSIDE);

        /* Return COMPOSITION-ID of a composition at buffer position
           CHARPOS/BYTEPOS and length NCHARS.  The `composition' property of
           the sequence is PROP.  STRING, if non-nil, is a string that
           contains the composition instead of the current buffer.

           If the composition is invalid, return -1.  */
        public static int get_composition_id(int charpos, int bytepos, int nchars, LispObject prop, LispObject stringg)
        {
            LispObject id, length, components, key;
            LispObject[] key_contents;
            uint glyph_len;

            LispHashTable hash_table = XHASH_TABLE(composition_hash_table);
            int hash_index;
            uint hash_code = 0;
            composition cmp;
            int i, ch = 0;

            /* PROP should be
              Form-A: ((LENGTH . COMPONENTS) . MODIFICATION-FUNC)
               or
              Form-B: (COMPOSITION-ID . (LENGTH COMPONENTS-VEC . MODIFICATION-FUNC))
            */
            if (nchars == 0 || !CONSP(prop))
                goto invalid_composition;

            id = XCAR(prop);
            if (INTEGERP(id))
            {
                /* PROP should be Form-B.  */
                if (XINT(id) < 0 || XINT(id) >= n_compositions)
                    goto invalid_composition;
                return XINT(id);
            }

            /* PROP should be Form-A.
               Thus, ID should be (LENGTH . COMPONENTS).  */
            if (!CONSP(id))
                goto invalid_composition;
            length = XCAR(id);
            if (!INTEGERP(length) || XINT(length) != nchars)
                goto invalid_composition;

            components = XCDR(id);

            /* Check if the same composition has already been registered or not
               by consulting composition_hash_table.  The key for this table is
               COMPONENTS (converted to a vector COMPONENTS-VEC) or, if it is
               nil, vector of characters in the composition range.  */
            if (INTEGERP(components))
                key = F.make_vector(make_number(1), components);
            else if (STRINGP(components) || CONSP(components))
                key = F.vconcat(1, components);
            else if (VECTORP(components))
                key = components;
            else if (NILP(components))
            {
                key = F.make_vector(make_number(nchars), Q.nil);
                if (STRINGP(stringg))
                    for (i = 0; i < nchars; i++)
                    {
                        FETCH_STRING_CHAR_ADVANCE(ref ch, stringg, ref charpos, ref bytepos);
                        XVECTOR(key)[i] = make_number(ch);
                    }
                else
                    for (i = 0; i < nchars; i++)
                    {
                        FETCH_CHAR_ADVANCE(ref ch, ref charpos, ref bytepos);
                        XVECTOR(key)[i] = make_number(ch);
                    }
            }
            else
                goto invalid_composition;

            hash_index = hash_lookup(hash_table, key, ref hash_code);
            if (hash_index >= 0)
            {
                /* We have already registered the same composition.  Change PROP
               from Form-A above to Form-B while replacing COMPONENTS with
               COMPONENTS-VEC stored in the hash table.  We can directly
               modify the cons cell of PROP because it is not shared.  */
                key = HASH_KEY(hash_table, hash_index);
                id = HASH_VALUE(hash_table, hash_index);
                XSETCAR(prop, id);
                XSETCDR(prop, F.cons(make_number(nchars), F.cons(key, XCDR(prop))));
                return XINT(id);
            }

            /* This composition is a new one.  We must register it.  */

            /* Check if we have sufficient memory to store this information.  */
            if (composition_table_size == 0)
            {
                composition_table_size = 256;
                composition_table = new composition[composition_table_size];
            }
            else if (composition_table_size <= n_compositions)
            {
                composition_table_size += 256;
                System.Array.Resize(ref composition_table, composition_table_size);
            }

            key_contents = XVECTOR(key).Contents;

            /* Check if the contents of COMPONENTS are valid if COMPONENTS is a
               vector or a list.  It should be a sequence of:
              char1 rule1 char2 rule2 char3 ...    ruleN charN+1  */

            if (VECTORP(components)
                && ASIZE(components) >= 2
                && VECTORP(AREF(components, 0)))
            {
                /* COMPONENTS is a glyph-string.  */
                int len = ASIZE(key);

                for (i = 1; i < len; i++)
                    if (!VECTORP(AREF(key, i)))
                        goto invalid_composition;
            }
            else if (VECTORP(components) || CONSP(components))
            {
                int len = XVECTOR(key).Size;

                /* The number of elements should be odd.  */
                if ((len % 2) == 0)
                    goto invalid_composition;
                /* All elements should be integers (character or encoded
                   composition rule).  */
                for (i = 0; i < len; i++)
                {
                    if (!INTEGERP(key_contents[i]))
                        goto invalid_composition;
                }
            }

            /* Change PROP from Form-A above to Form-B.  We can directly modify
               the cons cell of PROP because it is not shared.  */
            id = make_number(n_compositions);
            XSETCAR(prop, id);
            XSETCDR(prop, F.cons(make_number(nchars), F.cons(key, XCDR(prop))));

            /* Register the composition in composition_hash_table.  */
            hash_index = hash_put(hash_table, key, id, hash_code);

            /* Register the composition in composition_table.  */
            cmp = new composition();

            cmp.method = (NILP(components)
                   ? composition_method.COMPOSITION_RELATIVE
                   : ((INTEGERP(components) || STRINGP(components))
                      ? composition_method.COMPOSITION_WITH_ALTCHARS
                      : composition_method.COMPOSITION_WITH_RULE_ALTCHARS));
            cmp.hash_index = hash_index;
            glyph_len = (uint) (cmp.method == composition_method.COMPOSITION_WITH_RULE_ALTCHARS
                     ? (XVECTOR(key).Size + 1) / 2
                     : XVECTOR(key).Size);
            cmp.glyph_len = glyph_len;
            cmp.offsets = new short[glyph_len * 2];
            cmp.font = new System.IntPtr();

            if (cmp.method != composition_method.COMPOSITION_WITH_RULE_ALTCHARS)
            {
                /* Relative composition.  */
                cmp.width = 0;
                for (i = 0; i < glyph_len; i++)
                {
                    ushort this_width;
                    ch = XINT(key_contents[i]);
                    this_width = (ushort) (ch == '\t' ? 1 : CHAR_WIDTH((uint) ch));
                    if (cmp.width < this_width)
                        cmp.width = this_width;
                }
            }
            else
            {
                /* Rule-base composition.  */
                float leftmost = 0.0f, rightmost;

                ch = XINT(key_contents[0]);
                rightmost = ch != '\t' ? CHAR_WIDTH((uint) ch) : 1;

                for (i = 1; i < glyph_len; i += 2)
                {
                    int rule, gref = 0, nref = 0, xoff = 0, yoff = 0;
                    int this_width;
                    float this_left;

                    rule = XINT(key_contents[i]);
                    ch = XINT(key_contents[i + 1]);
                    this_width = ch != '\t' ? CHAR_WIDTH((uint) ch) : 1;

                    /* A composition rule is specified by an integer value
                       that encodes global and new reference points (GREF and
                       NREF).  GREF and NREF are specified by numbers as
                       below:
                      0---1---2 -- ascent
                      |       |
                      |       |
                      |       |
                      9--10--11 -- center
                      |       |
                       ---3---4---5--- baseline
                      |       |
                      6---7---8 -- descent
                    */
                    COMPOSITION_DECODE_RULE(ref rule, ref gref, ref nref, ref xoff, ref yoff);
                    this_left = (leftmost
                             + (gref % 3) * (rightmost - leftmost) / 2.0f
                             - (nref % 3) * this_width / 2.0f);

                    if (this_left < leftmost)
                        leftmost = this_left;
                    if (this_left + this_width > rightmost)
                        rightmost = this_left + this_width;
                }

                cmp.width = (ushort) (rightmost - leftmost);
                if (cmp.width < (rightmost - leftmost))
                    /* To get a ceiling integer value.  */
                    cmp.width++;
            }

            composition_table[n_compositions] = cmp;

            return n_compositions++;

        invalid_composition:
            /* Would it be better to remove this `composition' property?  */
            return -1;
        }

        /* Find a static composition at or nearest to position POS of OBJECT
           (buffer or string).

           OBJECT defaults to the current buffer.  If there's a composition at
           POS, set *START and *END to the start and end of the sequence,
           *PROP to the `composition' property, and return 1.

           If there's no composition at POS and LIMIT is negative, return 0.

           Otherwise, search for a composition forward (LIMIT > POS) or
           backward (LIMIT < POS).  In this case, LIMIT bounds the search.

           If a composition is found, set *START, *END, and *PROP as above,
           and return 1, else return 0.

           This doesn't check the validity of composition.  */
        public static bool find_composition(int pos, int limit, ref int start, ref int end, ref LispObject prop, LispObject obj)
        {
            LispObject val;

            if (get_property_and_range(pos, Q.composition, ref prop, ref start, ref end, obj))
                return true;

            if (limit < 0 || limit == pos)
                return false;

            if (limit > pos)		/* search forward */
            {
                val = F.next_single_property_change(make_number(pos), Q.composition,
                                obj, make_number(limit));
                pos = XINT(val);
                if (pos == limit)
                    return false;
            }
            else				/* search backward */
            {
                if (get_property_and_range(pos - 1, Q.composition, ref prop, ref start, ref end,
                            obj))
                    return true;
                val = F.previous_single_property_change(make_number(pos), Q.composition,
                                    obj, make_number(limit));
                pos = XINT(val);
                if (pos == limit)
                    return false;
                pos--;
            }
            get_property_and_range(pos, Q.composition, ref prop, ref start, ref end, obj);
            return true;
        }

        /* Return 1 if the composition is already registered.  */
        public static bool COMPOSITION_REGISTERD_P(LispObject prop)
        {
            return INTEGERP(XCAR(prop));
        }

        /* Return ID number of the already registered composition.  */
        public static int COMPOSITION_ID(LispObject prop)
        {
            return XINT(XCAR(prop));
        }

        /* Decode encoded composition rule RULE_CODE into GREF (global
           reference point code), NREF (new reference point code), XOFF
           (horizontal offset) YOFF (vertical offset).  Don't check RULE_CODE,
           always set GREF and NREF to valid values.  By side effect,
           RULE_CODE is modified.  */
        public static void COMPOSITION_DECODE_RULE(ref int rule_code, ref int gref, ref int nref, ref int xoff, ref int yoff)
        {
            xoff = (rule_code) >> 16;
            yoff = ((rule_code) >> 8) & 0xFF;
            rule_code &= 0xFF;
            gref = (rule_code) / 12;
            if (gref > 12) gref = 11;
            nref = (rule_code) % 12;
        }

        /* Return length of the composition.  */
        public static int COMPOSITION_LENGTH(LispObject prop)	
        {
            return (COMPOSITION_REGISTERD_P(prop)
                    ? XINT(XCAR(XCDR(prop)))
                    : XINT(XCAR(XCAR(prop))));
        }

        /* Return modification function of the composition.  */
        public static LispObject COMPOSITION_MODIFICATION_FUNC(LispObject prop)
        {
            return (COMPOSITION_REGISTERD_P(prop)
             ? XCDR(XCDR(XCDR(prop)))
             : CONSP(prop) ? XCDR(prop) : Q.nil);
        }

        /* Temporary variable used in macros COMPOSITION_XXX.  */
        public static LispObject composition_temp;

        /* Number of compositions currently made. */
        public static int n_compositions;

        /* Return 1 if the composition is valid.  It is valid if length of
           the composition equals to (END - START).  */
        public static bool COMPOSITION_VALID_P(int start, int end, LispObject prop)			
        {
            if (! CONSP (prop))
                return false; 
                    
            if (COMPOSITION_REGISTERD_P (prop))
            {
                if (! (COMPOSITION_ID (prop) >= 0 &&
                       COMPOSITION_ID (prop) <= n_compositions &&
                       CONSP (XCDR (prop))))
                    return false; 
            }
            else
            {
                composition_temp = XCAR (prop);	  
                if (! CONSP (composition_temp))
                    return false;
                
                composition_temp = XCDR (composition_temp);
                if (! (NILP (composition_temp) ||
                       STRINGP (composition_temp) ||
                       VECTORP (composition_temp) ||
                       INTEGERP (composition_temp) ||
                       CONSP (composition_temp)))
                    return false; 
            }
                
            return  (end - start) == COMPOSITION_LENGTH (prop);
        }

        /* Run a proper function to adjust the composition sitting between
           FROM and TO with property PROP.  */
        public static void run_composition_function(int from, int to, LispObject prop)
        {
            LispObject func;
            int start = 0, end = 0;

            func = COMPOSITION_MODIFICATION_FUNC(prop);
            /* If an invalid composition precedes or follows, try to make them
               valid too.  */
            if (from > BEGV()
                && find_composition(from - 1, -1, ref start, ref end, ref prop, Q.nil)
                && !COMPOSITION_VALID_P(start, end, prop))
                from = start;
            if (to < ZV
                && find_composition(to, -1, ref start, ref end, ref prop, Q.nil)
                && !COMPOSITION_VALID_P(start, end, prop))
                to = end;
            if (!NILP(F.fboundp(func)))
                call2(func, make_number(from), make_number(to));
        }

        /* Make invalid compositions adjacent to or inside FROM and TO valid.
           CHECK_MASK is bitwise `or' of mask bits defined by macros
           CHECK_XXX (see the comment in composite.h).

           It also resets the text-property `auto-composed' to a proper region
           so that automatic character composition works correctly later while
           displaying the region.

           This function is called when a buffer text is changed.  If the
           change is deletion, FROM == TO.  Otherwise, FROM < TO.  */
        public static void update_compositions(int from, int to, int check_mask)
        {
            LispObject prop = null;
            int start = 0, end = 0;
            /* The beginning and end of the region to set the property
               `auto-composed' to nil.  */
            int min_pos = from, max_pos = to;

            if (inhibit_modification_hooks)
                return;

            /* If FROM and TO are not in a valid range, do nothing.  */
            if (!(BEGV() <= from && from <= to && to <= ZV))
                return;

            if ((check_mask & CHECK_HEAD) != 0)
            {
                /* FROM should be at composition boundary.  But, insertion or
               deletion will make two compositions adjacent and
               indistinguishable when they have same (eq) property.  To
               avoid it, in such a case, we change the property of the
               latter to the copy of it.  */
                if (from > BEGV()
      && find_composition(from - 1, -1, ref start, ref end, ref prop, Q.nil)
      && COMPOSITION_VALID_P(start, end, prop))
                {
                    min_pos = start;
                    if (end > to)
                        max_pos = end;
                    if (from < end)
                        F.put_text_property(make_number(from), make_number(end),
                Q.composition,
                F.cons(XCAR(prop), XCDR(prop)), Q.nil);
                    run_composition_function(start, end, prop);
                    from = end;
                }
                else if (from < ZV
           && find_composition(from, -1, ref start, ref from, ref prop, Q.nil)
           && COMPOSITION_VALID_P(start, from, prop))
                {
                    if (from > to)
                        max_pos = from;
                    run_composition_function(start, from, prop);
                }
            }

            if ((check_mask & CHECK_INSIDE) != 0)
            {
                /* In this case, we are sure that (check & CHECK_TAIL) is also
                   nonzero.  Thus, here we should check only compositions before
                   (to - 1).  */
                while (from < to - 1
         && find_composition(from, to, ref start, ref from, ref prop, Q.nil)
         && COMPOSITION_VALID_P(start, from, prop)
         && from < to - 1)
                    run_composition_function(start, from, prop);
            }

            if ((check_mask & CHECK_TAIL) != 0)
            {
                if (from < to
      && find_composition(to - 1, -1, ref start, ref end, ref prop, Q.nil)
      && COMPOSITION_VALID_P(start, end, prop))
                {
                    /* TO should be also at composition boundary.  But,
                       insertion or deletion will make two compositions adjacent
                       and indistinguishable when they have same (eq) property.
                       To avoid it, in such a case, we change the property of
                       the former to the copy of it.  */
                    if (to < end)
                    {
                        F.put_text_property(make_number(start), make_number(to),
                                Q.composition,
                                F.cons(XCAR(prop), XCDR(prop)), Q.nil);
                        max_pos = end;
                    }
                    run_composition_function(start, end, prop);
                }
                else if (to < ZV
           && find_composition(to, -1, ref start, ref end, ref prop, Q.nil)
           && COMPOSITION_VALID_P(start, end, prop))
                {
                    run_composition_function(start, end, prop);
                    max_pos = end;
                }
            }
            if (min_pos < max_pos)
            {
                int count = SPECPDL_INDEX();

                specbind(Q.inhibit_read_only, Q.t);
                specbind(Q.inhibit_modification_hooks, Q.t);
                specbind(Q.inhibit_point_motion_hooks, Q.t);
                F.remove_list_of_text_properties(make_number(min_pos),
                                 make_number(max_pos),
                                 F.cons(Q.auto_composed, Q.nil), Q.nil);
                unbind_to(count, Q.nil);
            }
        }

        /* Modify composition property values in LIST destructively.  LIST is
       a list as returned from text_property_list.  Change values to the
       top-level copies of them so that none of them are `eq'.  */
        public static void make_composition_value_copy(LispObject list)
        {
            LispObject plist, val;

            for (; CONSP(list); list = XCDR(list))
            {
                plist = XCAR(XCDR(XCDR(XCAR(list))));
                while (CONSP(plist) && CONSP(XCDR(plist)))
                {
                    if (EQ(XCAR(plist), Q.composition))
                    {
                        val = XCAR(XCDR(plist));
                        if (CONSP(val))
                        {
                            XSETCAR(XCDR(plist), F.cons(XCAR(val), XCDR(val)));
                        }
                    }
                    plist = XCDR(XCDR(plist));
                }
            }
        }

        /* Table of pointers to the structure `composition' indexed by
           COMPOSITION-ID.  This structure is for storing information about
           each composition except for COMPONENTS-VEC.  */
        public static composition[] composition_table;

        /* The current size of `composition_table'.  */
        public static int composition_table_size;

        /* Hash table for compositions.  The key is COMPONENTS-VEC of
           `composition' property.  The value is the corresponding
           COMPOSITION-ID.  */
        public static LispObject composition_hash_table;

        /* Return the Nth glyph of composition specified by CMP.  CMP is a
           pointer to `struct composition'. */
        public static int COMPOSITION_GLYPH(composition cmp, int n)
        {
            return XINT(XVECTOR(XVECTOR(XHASH_TABLE(composition_hash_table).key_and_value)[
                     cmp.hash_index * 2])[cmp.method == composition_method.COMPOSITION_WITH_RULE_ALTCHARS
                    ? n * 2 : n]);
        }

        /* Check if the character at CHARPOS (and BYTEPOS) is composed
           (possibly with the following characters) on window W.  ENDPOS limits
           characters to be composed.  FACE, in non-NULL, is a base face of
           the character.  If STRING is not nil, it is a string containing the
           character to check, and CHARPOS and BYTEPOS are indices in the
           string.  In that case, FACE must not be NULL.

           If the character is composed, setup members of CMP_IT (id, nglyphs,
           and from), and return 1.  Otherwise, update CMP_IT->stop_pos, and
           return 0.  */
        public static bool composition_reseat_it(composition_it cmp_it, int charpos, int bytepos, int endpos, Window w, face face, LispObject stringg)
        {
            if (cmp_it.ch == -2)
            {
                composition_compute_stop_pos(cmp_it, charpos, bytepos, endpos, stringg);
                if (cmp_it.ch == -2)
                    return false;
            }

            if (cmp_it.ch < 0)
            {
                /* We are looking at a static composition.  */
                int start = 0, end = 0;
                LispObject prop = null;

                find_composition(charpos, -1, ref start, ref end, ref prop, stringg);
                cmp_it.id = get_composition_id(charpos, bytepos, end - start,
                                 prop, stringg);
                if (cmp_it.id < 0)
                    goto no_composition;
                cmp_it.nchars = end - start;
                cmp_it.nglyphs = (int) composition_table[cmp_it.id].glyph_len;
            }
            else if (w != null)
            {
                LispObject val, elt;
                int i;

                val = CHAR_TABLE_REF(V.composition_function_table, (uint) cmp_it.ch);
                for (; CONSP(val); val = XCDR(val))
                {
                    elt = XCAR(val);
                    if (cmp_it.lookback == XINT(AREF(elt, 1)))
                        break;
                }
                if (NILP(val))
                    goto no_composition;

                val = autocmp_chars(val, charpos, bytepos, endpos, w, face, stringg);
                if (!composition_gstring_p(val))
                    goto no_composition;
                if (NILP(LGSTRING_ID(val)))
                    val = composition_gstring_put_cache(val, -1);
                cmp_it.id = XINT(LGSTRING_ID(val));
                for (i = 0; i < LGSTRING_GLYPH_LEN(val); i++)
                    if (NILP(LGSTRING_GLYPH(val, i)))
                        break;
                cmp_it.nglyphs = i;
            }
            else
                goto no_composition;
            cmp_it.from = 0;
            return true;

        no_composition:
            charpos++;
            if (STRINGP(stringg))
                bytepos += MULTIBYTE_LENGTH_NO_CHECK(SDATA(stringg), bytepos);
            else
                INC_POS(ref bytepos);
            composition_compute_stop_pos(cmp_it, charpos, bytepos, endpos, stringg);
            return false;
        }

        /* Try to compose the characters at CHARPOS according to CFT_ELEMENT
           which is an element of composition-function-table (which see).
           LIMIT limits the characters to compose.  STRING, if not nil, is a
           target string.  WIN is a window where the characters are being
           displayed.  */
        public static LispObject autocmp_chars(LispObject cft_element, int charpos, int bytepos, int limit, Window win, face face, LispObject stringg)
        {
            int count = SPECPDL_INDEX();
            Frame f = XFRAME(win.frame);
            LispObject pos = make_number(charpos);
            int pt = PT(), pt_byte = PT_BYTE();
            int lookback;

            record_unwind_save_match_data();
            for (lookback = -1; CONSP(cft_element); cft_element = XCDR(cft_element))
            {
                LispObject elt = XCAR(cft_element);
                LispObject re;
                LispObject font_object = Q.nil, gstring;
                int len, to;

                if (!VECTORP(elt) || ASIZE(elt) != 3)
                    continue;
                if (lookback < 0)
                {
                    lookback = XINT(AREF(elt, 1));
                    if (limit > charpos + MAX_COMPOSITION_COMPONENTS)
                        limit = charpos + MAX_COMPOSITION_COMPONENTS;
                }
                else if (lookback != XINT(AREF(elt, 1)))
                    break;
                re = AREF(elt, 0);
                if (NILP(re))
                    len = 1;
                else if ((len = fast_looking_at(re, charpos, bytepos, limit, -1, stringg))
                     > 0)
                {
                    if (NILP(stringg))
                        len = BYTE_TO_CHAR(bytepos + len) - charpos;
                    else
                        len = string_byte_to_char(stringg, bytepos + len) - charpos;
                }
                if (len > 0)
                {
                    limit = to = charpos + len;
// COMEBACK WHEN READY
#if HAVE_WINDOW_SYSTEM
	  if (FRAME_WINDOW_P (f))
	    {
	      font_object = font_range (charpos, &to, win, face, stringg);
	      if (! FONT_OBJECT_P (font_object)
		  || (! NILP (re)
		      && to < limit
		      && (fast_looking_at (re, charpos, bytepos, to, -1, stringg) <= 0)))
		{
		  if (NILP (string))
		    TEMP_SET_PT_BOTH (pt, pt_byte);
		  return unbind_to (count, Q.nil);
		}
	    }
	  else
#endif	// not HAVE_WINDOW_SYSTEM
                    font_object = win.frame;
                    gstring = F.composition_get_gstring(pos, make_number(to),
                                        font_object, stringg);
                    if (NILP(LGSTRING_ID(gstring)))
                    {
                        LispObject[] args = new LispObject[6];

                        args[0] = V.auto_composition_function;
                        args[1] = AREF(elt, 2);
                        args[2] = pos;
                        args[3] = make_number(to);
                        args[4] = font_object;
                        args[5] = stringg;
                        gstring = safe_call(6, args);
                    }
                    if (NILP(stringg))
                        TEMP_SET_PT_BOTH(pt, pt_byte);
                    return unbind_to(count, gstring);
                }
            }
            if (NILP(stringg))
                TEMP_SET_PT_BOTH(pt, pt_byte);
            return unbind_to(count, Q.nil);
        }

        public static int composition_update_it(composition_it cmp_it, int charpos, int bytepos, LispObject stringg)
        {
            int i, c = 0;

            if (cmp_it.ch < 0)
            {
                composition cmp = composition_table[cmp_it.id];

                cmp_it.to = cmp_it.nglyphs;
                if (cmp_it.nglyphs == 0)
                    c = -1;
                else
                {
                    for (i = 0; i < cmp.glyph_len; i++)
                        if ((c = COMPOSITION_GLYPH(cmp, i)) != '\t')
                            break;
                    if (c == '\t')
                        c = ' ';
                }
                cmp_it.width = cmp.width;
            }
            else
            {
                LispObject gstring = composition_gstring_from_id(cmp_it.id);

                if (cmp_it.nglyphs == 0)
                {
                    c = -1;
                    cmp_it.nchars = LGSTRING_CHAR_LEN(gstring);
                    cmp_it.width = 0;
                }
                else
                {
                    LispObject glyph = LGSTRING_GLYPH(gstring, cmp_it.from);
                    int from = LGLYPH_FROM(glyph);

                    c = XINT(LGSTRING_CHAR(gstring, from));
                    cmp_it.nchars = LGLYPH_TO(glyph) - from + 1;
                    cmp_it.width = (LGLYPH_WIDTH(glyph) > 0
                             ? CHAR_WIDTH((uint) LGLYPH_CHAR(glyph)) : 0);
                    for (cmp_it.to = cmp_it.from + 1; cmp_it.to < cmp_it.nglyphs;
                         cmp_it.to++)
                    {
                        glyph = LGSTRING_GLYPH(gstring, cmp_it.to);
                        if (LGLYPH_FROM(glyph) != from)
                            break;
                        if (LGLYPH_WIDTH(glyph) > 0)
                            cmp_it.width += CHAR_WIDTH((uint) LGLYPH_CHAR(glyph));
                    }
                }
            }

            charpos += cmp_it.nchars;
            if (STRINGP(stringg))
                cmp_it.nbytes = string_char_to_byte(stringg, charpos) - bytepos;
            else
                cmp_it.nbytes = CHAR_TO_BYTE(charpos) - bytepos;
            return c;
        }

        /* Update cmp_it->stop_pos to the next position after CHARPOS (and
           BYTEPOS) where character composition may happen.  If BYTEPOS is
           negative, compoute it.  If it is a static composition, set
           cmp_it->ch to -1.  Otherwise, set cmp_it->ch to the character that
           triggers a automatic composition.  */
        public static void composition_compute_stop_pos(composition_it cmp_it, int charpos, int bytepos, int endpos, LispObject stringg)
        {
            int start = 0, end = 0, c = 0;
            LispObject prop = null, val;
            /* This is from forward_to_next_line_start in xdisp.c.  */
            const int MAX_NEWLINE_DISTANCE = 500;

            if (endpos > charpos + MAX_NEWLINE_DISTANCE)
                endpos = charpos + MAX_NEWLINE_DISTANCE;
            cmp_it.stop_pos = endpos;
            cmp_it.id = -1;
            cmp_it.ch = -2;
            if (find_composition(charpos, endpos, ref start, ref end, ref prop, stringg)
                && COMPOSITION_VALID_P(start, end, prop))
            {
                cmp_it.stop_pos = endpos = start;
                cmp_it.ch = -1;
            }
            if (NILP(stringg) && PT() > charpos && PT() < endpos)
                cmp_it.stop_pos = PT();
            if (NILP(current_buffer.enable_multibyte_characters)
                || !FUNCTIONP(V.auto_composition_function))
                return;
            if (bytepos < 0)
            {
                if (STRINGP(stringg))
                    bytepos = string_char_to_byte(stringg, charpos);
                else
                    bytepos = CHAR_TO_BYTE(charpos);
            }

            start = charpos;
            while (charpos < endpos)
            {
                if (STRINGP(stringg))
                    FETCH_STRING_CHAR_ADVANCE(ref c, stringg, ref charpos, ref bytepos);
                else
                    FETCH_CHAR_ADVANCE(ref c, ref charpos, ref bytepos);
                if (c == '\n')
                {
                    cmp_it.ch = -2;
                    break;
                }
                val = CHAR_TABLE_REF(V.composition_function_table, (uint) c);
                if (!NILP(val))
                {
                    LispObject elt = null;

                    for (; CONSP(val); val = XCDR(val))
                    {
                        elt = XCAR(val);
                        if (VECTORP(elt) && ASIZE(elt) == 3 && NATNUMP(AREF(elt, 1))
                        && charpos - 1 - XINT(AREF(elt, 1)) >= start)
                            break;
                    }
                    if (CONSP(val))
                    {
                        cmp_it.lookback = XINT(AREF(elt, 1));
                        cmp_it.stop_pos = charpos - 1 - cmp_it.lookback;
                        cmp_it.ch = c;
                        return;
                    }
                }
            }
            cmp_it.stop_pos = charpos;
        }

        /* Lisp glyph-string handlers */

        /* Hash table for automatic composition.  The key is a header of a
           lgstring (Lispy glyph-string), and the value is a body of a
           lgstring.  */
        public static LispObject gstring_hash_table;

        /* Macros for lispy glyph-string.  This is completely different from
           struct glyph_string.  */
        public static LispObject LGSTRING_HEADER(LispObject lgs)
        {
            return AREF(lgs, 0);
        }

        public static void LGSTRING_SET_HEADER(LispObject lgs, LispObject header)
        {
            ASET(lgs, 0, header);
        }

        public static LispObject LGSTRING_FONT(LispObject lgs)
        {
            return AREF(LGSTRING_HEADER(lgs), 0);
        }

        public static LispObject LGSTRING_CHAR(LispObject lgs, int i)
        {
            return AREF(LGSTRING_HEADER(lgs), (i) + 1);
        }
        public static int LGSTRING_CHAR_LEN(LispObject lgs)
        {
            return (ASIZE(LGSTRING_HEADER(lgs)) - 1);
        }
        public static void LGSTRING_SET_FONT(LispObject lgs, LispObject val)
        {
            ASET(LGSTRING_HEADER(lgs), 0, (val));
        }

        public static void LGSTRING_SET_CHAR(LispObject lgs, int i, LispObject c)
        {
            ASET(LGSTRING_HEADER(lgs), (i) + 1, (c));
        }

        public static LispObject LGSTRING_ID(LispObject lgs)
        {
            return AREF(lgs, 1);
        }

        public static void LGSTRING_SET_ID(LispObject lgs, LispObject id)
        {
            ASET(lgs, 1, id);
        }

        public static int LGSTRING_GLYPH_LEN(LispObject lgs)
        {
            return (ASIZE((lgs)) - 2);
        }
        public static LispObject LGSTRING_GLYPH(LispObject lgs, int idx)
        {
            return AREF((lgs), (idx) + 2);
        }
        public static void LGSTRING_SET_GLYPH(LispObject lgs, int idx, LispObject val)
        {
            ASET((lgs), (idx) + 2, (val));
        }

        public static LispObject composition_gstring_put_cache(LispObject gstring, int len)
        {
            LispHashTable h = XHASH_TABLE(gstring_hash_table);
            uint hash;
            LispObject header, copy;
            int i;

            header = LGSTRING_HEADER(gstring);
            hash = h.hashfn(h, header);
            if (len < 0)
            {
                len = LGSTRING_GLYPH_LEN(gstring);
                for (i = 0; i < len; i++)
                    if (NILP(LGSTRING_GLYPH(gstring, i)))
                        break;
                len = i;
            }

            copy = F.make_vector(make_number(len + 2), Q.nil);
            LGSTRING_SET_HEADER(copy, F.copy_sequence(header));
            for (i = 0; i < len; i++)
                LGSTRING_SET_GLYPH(copy, i, F.copy_sequence(LGSTRING_GLYPH(gstring, i)));
            i = hash_put(h, LGSTRING_HEADER(copy), copy, hash);
            LGSTRING_SET_ID(copy, make_number(i));
            return copy;
        }

        public static LispObject composition_gstring_from_id(int id)
        {
            LispHashTable h = XHASH_TABLE(gstring_hash_table);

            return HASH_VALUE(h, id);
        }

        public static bool composition_gstring_p(LispObject gstring)
        {
            LispObject header;
            int i;

            if (!VECTORP(gstring) || ASIZE(gstring) < 2)
                return false;
            header = LGSTRING_HEADER(gstring);
            if (!VECTORP(header) || ASIZE(header) < 2)
                return false;
            if (!NILP(LGSTRING_FONT(gstring))
                && (!FONT_OBJECT_P(LGSTRING_FONT(gstring))
                && !CODING_SYSTEM_P(LGSTRING_FONT(gstring))))
                return false;
            for (i = 1; i < ASIZE(LGSTRING_HEADER(gstring)); i++)
                if (!NATNUMP(AREF(LGSTRING_HEADER(gstring), i)))
                    return false;
            if (!NILP(LGSTRING_ID(gstring)) && !NATNUMP(LGSTRING_ID(gstring)))
                return false;
            for (i = 0; i < LGSTRING_GLYPH_LEN(gstring); i++)
            {
                LispObject glyph = LGSTRING_GLYPH(gstring, i);
                if (NILP(glyph))
                    break;
                if (!VECTORP(glyph) || ASIZE(glyph) != (int) lglyph_indices.LGLYPH_SIZE)
                    return false;
            }

            return true;
        }

        public static int LGLYPH_FROM(LispObject g)
        {
            return XINT(AREF((g), (int) lglyph_indices.LGLYPH_IX_FROM));
        }
        public static int LGLYPH_TO(LispObject g)
        {
            return XINT(AREF((g), (int) lglyph_indices.LGLYPH_IX_TO));
        }
        public static int LGLYPH_CHAR(LispObject g)
        {
            return XINT(AREF((g), (int) lglyph_indices.LGLYPH_IX_CHAR));
        }
        public static int LGLYPH_WIDTH(LispObject g)
        {
            return XINT(AREF((g), (int) lglyph_indices.LGLYPH_IX_WIDTH));
        }
    }

    public partial class F
    {
        public static LispObject composition_get_gstring(LispObject from, LispObject to, LispObject font_object, LispObject stringg)
        {
            return Q.nil;
            // COMEBACK WHEN READY
            /*
              LispObject gstring, header;
              int frompos, topos;

              L.CHECK_NATNUM (from);
              L.CHECK_NATNUM (to);
              if (L.XINT (to) > L.XINT (from) + L.MAX_COMPOSITION_COMPONENTS)
                to = L.make_number (L.XINT (from) + L.MAX_COMPOSITION_COMPONENTS);
              if (! L.FONT_OBJECT_P (font_object))
                {
                  coding_system coding;
                  terminal terminal = get_terminal (font_object, 1);

                  coding = ((TERMINAL_TERMINAL_CODING (terminal).common_flags
                     & CODING_REQUIRE_ENCODING_MASK)
                    ? TERMINAL_TERMINAL_CODING (terminal) : &safe_terminal_coding);
                  font_object = CODING_ID_NAME (coding->id);
                }

              header = fill_gstring_header (Qnil, from, to, font_object, stringg);
              gstring = gstring_lookup_cache (header);
              if (! NILP (gstring))
                return gstring;

              frompos = XINT (from);
              topos = XINT (to);
              if (LGSTRING_GLYPH_LEN (gstring_work) < topos - frompos)
                gstring_work = Fmake_vector (make_number (topos - frompos + 2), Qnil);
              LGSTRING_SET_HEADER (gstring_work, header);
              LGSTRING_SET_ID (gstring_work, Qnil);
              fill_gstring_body (gstring_work);
              return gstring_work;*/
        }
    }
}