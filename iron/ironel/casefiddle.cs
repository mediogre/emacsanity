namespace IronElisp
{
    public enum case_action
    {
        CASE_UP, CASE_DOWN, CASE_CAPITALIZE, CASE_CAPITALIZE_UP
    }

    public partial class Q
    {
        public static LispObject identity;
    }

    public partial class L
    {
        public static LispObject casify_object(case_action flag, LispObject obj)
        {
            int c, c1;
            bool inword = flag == case_action.CASE_DOWN;

            /* If the case table is flagged as modified, rescan it.  */
            if (NILP(XCHAR_TABLE(current_buffer.downcase_table).extras(1)))
                F.set_case_table(current_buffer.downcase_table);

            if (INTEGERP(obj))
            {
                int flagbits = (int)(CHAR_ALT | CHAR_SUPER | CHAR_HYPER | CHAR_SHIFT | CHAR_CTL | CHAR_META);
                int flags = XINT(obj) & flagbits;
                bool multibyte = !NILP(current_buffer.enable_multibyte_characters);

                /* If the character has higher bits set
               above the flags, return it unchanged.
               It is not a real character.  */
                if ((uint)XINT(obj) > (uint)flagbits)
                    return obj;

                c1 = XINT(obj) & ~flagbits;
                /* FIXME: Even if enable-multibyte-characters is nil, we may
               manipulate multibyte chars.  This means we have a bug for latin-1
               chars since when we receive an int 128-255 we can't tell whether
               it's an eight-bit byte or a latin-1 char.  */
                if (c1 >= 256)
                    multibyte = true;
                if (!multibyte)
                    MAKE_CHAR_MULTIBYTE(ref c1);
                c = (int)DOWNCASE((uint)c1);
                if (inword)
                    obj = XSETINT(c | flags);
                else if (c == (XINT(obj) & ~flagbits))
                {
                    if (!inword)
                        c = (int)UPCASE1((uint)c1);
                    if (!multibyte)
                        MAKE_CHAR_UNIBYTE(ref c);
                    obj = XSETINT(c | flags);
                }
                return obj;
            }

            if (!STRINGP(obj))
            {
                wrong_type_argument(Q.char_or_string_p, obj);
                return null;
            }
            else if (!STRING_MULTIBYTE(obj))
            {
                int i;
                int size = SCHARS(obj);

                obj = F.copy_sequence(obj);
                for (i = 0; i < size; i++)
                {
                    c = SREF(obj, i);
                    MAKE_CHAR_MULTIBYTE(ref c);
                    c1 = c;
                    if (inword && flag != case_action.CASE_CAPITALIZE_UP)
                        c = (int)DOWNCASE((uint)c);
                    else if (!UPPERCASEP((uint)c)
                         && (!inword || flag != case_action.CASE_CAPITALIZE_UP))
                        c = (int)UPCASE1((uint)c1);
                    if ((int)flag >= (int)case_action.CASE_CAPITALIZE)
                        inword = (SYNTAX((uint)c) == syntaxcode.Sword);
                    if (c != c1)
                    {
                        MAKE_CHAR_UNIBYTE(ref c);
                        /* If the char can't be converted to a valid byte, just don't
                       change it.  */
                        if (c >= 0 && c < 256)
                            SSET(obj, i, (byte)c);
                    }
                }
                return obj;
            }
            else
            {
                int i, i_byte, size = SCHARS(obj);
                int len = 0;

                /* Over-allocate by 12%: this is a minor overhead, but should be
               sufficient in 99.999% of the cases to avoid a reallocation.  */
                int o_size = SBYTES(obj) + SBYTES(obj) / 8 + MAX_MULTIBYTE_LENGTH;
                byte[] dst = new byte[o_size];
                int o = 0;

                for (i = i_byte = 0; i < size; i++, i_byte += len)
                {
                    if (o + MAX_MULTIBYTE_LENGTH > o_size)
                    { /* Not enough space for the next char: grow the destination.  */
                        byte[] old_dst = dst;
                        o_size += o_size;	/* Probably overkill, but extremely rare.  */
                        dst = new byte[o_size];
                        System.Array.Copy(old_dst, dst, o);
                    }
                    c = (int)STRING_CHAR_AND_LENGTH(SDATA(obj), i_byte, 0, ref len);
                    if (inword && flag != case_action.CASE_CAPITALIZE_UP)
                        c = (int)DOWNCASE((uint)c);
                    else if (!UPPERCASEP((uint)c)
                         && (!inword || flag != case_action.CASE_CAPITALIZE_UP))
                        c = (int)UPCASE1((uint)c);
                    if ((int)flag >= (int)case_action.CASE_CAPITALIZE)
                        inword = (SYNTAX((uint)c) == syntaxcode.Sword);
                    o += CHAR_STRING((uint)c, dst, o);
                }
                // eassert (o - dst <= o_size);
                obj = make_multibyte_string(dst, size, o);
                return obj;
            }
        }

/* flag is CASE_UP, CASE_DOWN or CASE_CAPITALIZE or CASE_CAPITALIZE_UP.
   b and e specify range of buffer to operate on. */
        public static void casify_region(case_action flag, LispObject b, LispObject e)
        {
            int c;
            bool inword = flag == case_action.CASE_DOWN;
            bool multibyte = !NILP(current_buffer.enable_multibyte_characters);
            int start, end;
            int start_byte, end_byte;
            int first = -1, last = 0;	/* Position of first and last changes.  */
            int opoint = PT();
            int opoint_byte = PT_BYTE();

            if (EQ(b, e))
                /* Not modifying because nothing marked */
                return;

            /* If the case table is flagged as modified, rescan it.  */
            if (NILP(XCHAR_TABLE(current_buffer.downcase_table).extras(1)))
                F.set_case_table(current_buffer.downcase_table);

            validate_region(ref b, ref e);
            start = XINT(b);
            end = XINT(e);
            modify_region(current_buffer, start, end, false);
            record_change(start, end - start);
            start_byte = CHAR_TO_BYTE(start);
            end_byte = CHAR_TO_BYTE(end);

            while (start < end)
            {
                int c2, len;

                if (multibyte)
                {
                    c = (int) FETCH_MULTIBYTE_CHAR(start_byte);
                    len = CHAR_BYTES((uint) c);
                }
                else
                {
                    c = FETCH_BYTE(start_byte);
                    MAKE_CHAR_MULTIBYTE(ref c);
                    len = 1;
                }
                c2 = c;
                if (inword && flag != case_action.CASE_CAPITALIZE_UP)
                    c = (int) DOWNCASE((uint) c);
                else if (!UPPERCASEP((uint) c)
                     && (!inword || flag != case_action.CASE_CAPITALIZE_UP))
                    c = (int) UPCASE1((uint) c);
                if ((int)flag >= (int)case_action.CASE_CAPITALIZE)
                    inword = ((SYNTAX((uint) c) == syntaxcode.Sword) && (inword || !SYNTAX_PREFIX((uint) c)));
                if (c != c2)
                {
                    last = start;
                    if (first < 0)
                        first = start;

                    if (!multibyte)
                    {
                        MAKE_CHAR_UNIBYTE(ref c);
                        FETCH_BYTE(start_byte, c);
                    }
                    else if (ASCII_CHAR_P((uint) c2) && ASCII_CHAR_P((uint) c))
                        FETCH_BYTE(start_byte, c);
                    else
                    {
                        int tolen = CHAR_BYTES((uint) c);
                        int j;
                        byte[] str = new byte[MAX_MULTIBYTE_LENGTH];

                        CHAR_STRING((uint) c, str);
                        if (len == tolen)
                        {
                            /* Length is unchanged.  */
                            for (j = 0; j < len; ++j)
                                FETCH_BYTE(start_byte + j, str[j]);
                        }
                        else
                        {
                            /* Replace one character with the other,
                               keeping text properties the same.  */
                            replace_range_2(start, start_byte,
                                     start + 1, start_byte + len,
                                     str, 1, tolen,
                                     false);
                            len = tolen;
                        }
                    }
                }
                start++;
                start_byte += len;
            }

            if (PT() != opoint)
                TEMP_SET_PT_BOTH(opoint, opoint_byte);

            if (first >= 0)
            {
                signal_after_change(first, last + 1 - first, last + 1 - first);
                update_compositions(first, last + 1, CHECK_ALL);
            }
        }
    }

    public partial class F
    {
        public static LispObject upcase_region(LispObject beg, LispObject end)
        {
            L.casify_region(case_action.CASE_UP, beg, end);
            return Q.nil;
        }

        /* Like Fcapitalize_region but change only the initials.  */
        public static LispObject upcase_initials_region(LispObject beg, LispObject end)
        {
            L.casify_region(case_action.CASE_CAPITALIZE_UP, beg, end);
            return Q.nil;
        }

        public static LispObject upcase(LispObject obj)
        {
            return L.casify_object(case_action.CASE_UP, obj);
        }

        public static LispObject downcase(LispObject obj)
        {
            return L.casify_object(case_action.CASE_DOWN, obj);
        }

        public static LispObject capitalize(LispObject obj)
        {
            return L.casify_object(case_action.CASE_CAPITALIZE, obj);
        }

        /* Like Fcapitalize but change only the initials.  */
        public static LispObject upcase_initials(LispObject obj)
        {
            return L.casify_object(case_action.CASE_CAPITALIZE_UP, obj);
        }
    }
}