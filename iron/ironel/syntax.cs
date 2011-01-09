namespace IronElisp
{
    public enum syntaxcode
    {
        Swhitespace, /* for a whitespace character */
        Spunct,	 /* for random punctuation characters */
        Sword,	 /* for a word constituent */
        Ssymbol,	 /* symbol constituent but not word constituent */
        Sopen,	 /* for a beginning delimiter */
        Sclose,      /* for an ending delimiter */
        Squote,	 /* for a prefix character like Lisp ' */
        Sstring,	 /* for a string-grouping character like Lisp " */
        Smath,	 /* for delimiters like $ in Tex.  */
        Sescape,	 /* for a character that begins a C-style escape */
        Scharquote,  /* for a character that quotes the following character */
        Scomment,    /* for a comment-starting character */
        Sendcomment, /* for a comment-ending character */
        Sinherit,    /* use the standard syntax table for this character */
        Scomment_fence, /* Starts/ends comment which is delimited on the
		       other side by any char with the same syntaxcode.  */
        Sstring_fence,  /* Starts/ends string which is delimited on the
		       other side by any char with the same syntaxcode.  */
        Smax	 /* Upper bound on codes that are meaningful */
    }

    public class gl_state_s
    {
        public LispObject obj;			/* The object we are scanning. */
        public int start;				/* Where to stop. */
        public int stop;				/* Where to stop. */
        public int use_global;			/* Whether to use global_code
					   or c_s_t. */
        public LispObject global_code;		/* Syntax code of current char. */
        public LispObject current_syntax_table;	/* Syntax table for current pos. */
        public LispObject old_prop;			/* Syntax-table prop at prev pos. */
        public int b_property;			/* First index where c_s_t is valid. */
        public int e_property;			/* First index where c_s_t is
					   not valid. */
        public Interval forward_i;			/* Where to start lookup on forward */
        public Interval backward_i;			/* or backward movement.  The
					   data in c_s_t is valid
					   between these intervals,
					   and possibly at the
					   intervals too, depending
					   on: */
        /* Offset for positions specified to UPDATE_SYNTAX_TABLE.  */
        public int offset;
    }

    public partial class Q
    {
        public static LispObject syntax_table_p, syntax_table, scan_error;
    }

    public partial class V
    {
        /* Char-table of functions that find the next or previous word
           boundary.  */
        public static LispObject find_word_boundary_function_table
        {
            get { return Defs.O[(int)Objects.find_word_boundary_function_table]; }
            set { Defs.O[(int)Objects.find_word_boundary_function_table] = value; }
        }
    }

    public partial class L
    {
        public static gl_state_s gl_state = new gl_state_s();

        public static bool words_include_escapes
        {
            get { return Defs.B[(int)Bools.words_include_escapes]; }
            set { Defs.B[(int)Bools.words_include_escapes] = value; }
        }

        public static bool parse_sexp_lookup_properties
        {
            get { return Defs.B[(int)Bools.parse_sexp_lookup_properties]; }
            set { Defs.B[(int)Bools.parse_sexp_lookup_properties] = value; }
        }

        /* Convert a letter which signifies a syntax code
         into the code it signifies.
         This is used by modify-syntax-entry, and other things.  */
        public static byte[] syntax_spec_code = {
   255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, 255,
    (byte) syntaxcode.Swhitespace, (byte) syntaxcode.Scomment_fence, (byte) syntaxcode.Sstring, 255,
        (byte) syntaxcode.Smath, 255, 255, (byte) syntaxcode.Squote,
    (byte) syntaxcode.Sopen, (byte) syntaxcode.Sclose, 255, 255,
	255, (byte) syntaxcode.Swhitespace, (byte) syntaxcode.Spunct, (byte) syntaxcode.Scharquote,
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255,
	(byte) syntaxcode.Scomment, 255, (byte) syntaxcode.Sendcomment, 255,
    (byte) syntaxcode.Sinherit, 255, 255, 255, 255, 255, 255, 255,   /* @, A ... */
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, (byte) syntaxcode.Sword,
    255, 255, 255, 255, (byte) syntaxcode.Sescape, 255, 255, (byte) syntaxcode.Ssymbol,
    255, 255, 255, 255, 255, 255, 255, 255,   /* `, a, ... */
    255, 255, 255, 255, 255, 255, 255, 255,
    255, 255, 255, 255, 255, 255, 255, (byte) syntaxcode.Sword,
    255, 255, 255, 255, (byte) syntaxcode.Sstring_fence, 255, 255, 255
        };

        /* Indexed by syntax code, give the letter that describes it.  */
        public static char[] syntax_code_spec = new char[] {
    ' ', '.', 'w', '_', '(', ')', '\'', '\"', '$', '\\', '/', '<', '>', '@',
    '!', '|'
  };

        /* Convert the byte offset BYTEPOS into a character position,
           for the object recorded in gl_state with SETUP_SYNTAX_TABLE_FOR_OBJECT.

           The value is meant for use in the UPDATE_SYNTAX_TABLE... macros.
           These macros do nothing when parse_sexp_lookup_properties is 0,
           so we return 0 in that case, for speed.  */
        public static int SYNTAX_TABLE_BYTE_TO_CHAR(int bytepos)
        {
            if (!parse_sexp_lookup_properties)
                return 0;

            if (STRINGP(gl_state.obj))
                return string_byte_to_char(gl_state.obj, bytepos);

            if (BUFFERP(gl_state.obj))
                return buf_bytepos_to_charpos(XBUFFER(gl_state.obj),
                             (bytepos) + BUF_BEGV_BYTE(XBUFFER(gl_state.obj)) - 1) - BUF_BEGV(XBUFFER(gl_state.obj)) + 1;
            if (NILP(gl_state.obj))
                return BYTE_TO_CHAR(bytepos + BEGV_BYTE() - 1) - BEGV() + 1;

            return bytepos;
        }

        /* Make syntax table state (gl_state) good for CHARPOS, assuming it is
           currently good for a position before CHARPOS.  */
        public static bool UPDATE_SYNTAX_TABLE_FORWARD(int charpos)
        {
            if (parse_sexp_lookup_properties && (charpos) >= gl_state.e_property)
            {
                update_syntax_table(charpos + gl_state.offset, 1, 0, gl_state.obj);
                return true;
            }
            else
                return false;
        }

        /* Make syntax table state (gl_state) good for CHARPOS, assuming it is
           currently good for a position after CHARPOS.  */
        public static bool UPDATE_SYNTAX_TABLE_BACKWARD(int charpos)
        {
            if (parse_sexp_lookup_properties && (charpos) < gl_state.b_property)
            {
                update_syntax_table(charpos + gl_state.offset, -1, 0, gl_state.obj);
                return true;
            }
            else
                return false;
        }

        /* Make syntax table good for CHARPOS.  */
        public static bool UPDATE_SYNTAX_TABLE(int charpos)
        {
            if (parse_sexp_lookup_properties && charpos < gl_state.b_property)
            {
                update_syntax_table(charpos + gl_state.offset, -1, 0, gl_state.obj);
                return true;
            }
            else if (parse_sexp_lookup_properties && charpos >= gl_state.e_property)
            {
                update_syntax_table(charpos + gl_state.offset, 1, 0, gl_state.obj);
                return true;
            }
            else
                return false;
        }

        /* This macro should be called with FROM at the start of forward
           search, or after the last position of the backward search.  It
           makes sure that the first char is picked up with correct table, so
           one does not need to call UPDATE_SYNTAX_TABLE immediately after the
           call.
           Sign of COUNT gives the direction of the search.
         */
        public static void SETUP_SYNTAX_TABLE(int FROM, int COUNT)
        {
            gl_state.b_property = BEGV();
            gl_state.e_property = ZV + 1;
            gl_state.obj = Q.nil;
            gl_state.use_global = 0;
            gl_state.offset = 0;
            gl_state.current_syntax_table = current_buffer.syntax_table;
            if (parse_sexp_lookup_properties)
                if ((COUNT) > 0 || (FROM) > BEGV())
                    update_syntax_table((COUNT) > 0 ? (FROM) : (FROM) - 1, (COUNT),
                             1, Q.nil);
        }

        /* Same as above, but in OBJECT.  If OBJECT is nil, use current buffer.
           If it is t, ignore properties altogether.

           This is meant for regex.c to use.  For buffers, regex.c passes arguments
           to the UPDATE_SYNTAX_TABLE macros which are relative to BEGV.
           So if it is a buffer, we set the offset field to BEGV.  */
        public static void SETUP_SYNTAX_TABLE_FOR_OBJECT(LispObject OBJECT, int FROM, int COUNT)
        {
            gl_state.obj = (OBJECT);
            if (BUFFERP(gl_state.obj))
            {
                Buffer buf = XBUFFER(gl_state.obj);
                gl_state.b_property = 1;
                gl_state.e_property = BUF_ZV(buf) - BUF_BEGV(buf) + 1;
                gl_state.offset = BUF_BEGV(buf) - 1;
            }
            else if (NILP(gl_state.obj))
            {
                gl_state.b_property = 1;
                gl_state.e_property = ZV - BEGV() + 1;
                gl_state.offset = BEGV() - 1;
            }
            else if (EQ(gl_state.obj, Q.t))
            {
                gl_state.b_property = 0;
                gl_state.e_property = 1500000000;
                gl_state.offset = 0;
            }
            else
            {
                gl_state.b_property = 0;
                gl_state.e_property = 1 + SCHARS(gl_state.obj);
                gl_state.offset = 0;
            }
            gl_state.use_global = 0;
            gl_state.current_syntax_table = current_buffer.syntax_table;
            if (parse_sexp_lookup_properties)
                update_syntax_table(((FROM) + gl_state.offset
                          + (COUNT > 0 ? 0 : -1)),
                         COUNT, 1, gl_state.obj);
        }

        public static LispObject CURRENT_SYNTAX_TABLE()
        {
            return current_buffer.syntax_table;
        }

        public static LispObject SYNTAX_ENTRY_INT(uint c)
        {
            return CHAR_TABLE_REF(CURRENT_SYNTAX_TABLE(), c);
        }

        public static LispObject SYNTAX_ENTRY(uint c)
        {
            return SYNTAX_ENTRY_INT(c);
        }

        /* Used as a temporary in SYNTAX_ENTRY and other macros in syntax.h,
           if not compiled with GCC.  No need to mark it, since it is used
           only very temporarily.  */
        public static LispObject syntax_temp;

        /* A syntax table is a chartable whose elements are cons cells
           (CODE+FLAGS . MATCHING-CHAR).  MATCHING-CHAR can be nil if the char
           is not a kind of parenthesis.

           The low 8 bits of CODE+FLAGS is a code, as follows:  */
        public static syntaxcode SYNTAX(uint c)
        {
            syntax_temp = SYNTAX_ENTRY(c);
            if (CONSP(syntax_temp))
            {
                return (syntaxcode)(XINT(XCAR(syntax_temp)) & 0xff);
            }
            else
            {
                return syntaxcode.Swhitespace;
            }
        }

        public static int SYNTAX_WITH_FLAGS(uint c)
        {
            syntax_temp = SYNTAX_ENTRY((c));
            if (CONSP(syntax_temp))
            {
                return XINT(XCAR(syntax_temp));
            }
            else
            {
                return (int) syntaxcode.Swhitespace;
            }
        }

        public static LispObject SYNTAX_MATCH(uint c)
        {
            syntax_temp = SYNTAX_ENTRY((c));
            if (CONSP(syntax_temp))
            {
                return XCDR(syntax_temp);
            }
            else
            {
                return Q.nil;
            }
        }

        /* Then there are seven single-bit flags that have the following meanings:
          1. This character is the first of a two-character comment-start sequence.
          2. This character is the second of a two-character comment-start sequence.
          3. This character is the first of a two-character comment-end sequence.
          4. This character is the second of a two-character comment-end sequence.
          5. This character is a prefix, for backward-prefix-chars.
          6. see below
          7. This character is part of a nestable comment sequence.
          Note that any two-character sequence whose first character has flag 1
          and whose second character has flag 2 will be interpreted as a comment start.

          bit 6 is used to discriminate between two different comment styles.
          Languages such as C++ allow two orthogonal syntax start/end pairs
          and bit 6 is used to determine whether a comment-end or Scommentend
          ends style a or b.  Comment start sequences can start style a or b.
          Style a is always the default.
          */

        /* These macros extract a particular flag for a given character.  */
        public static bool SYNTAX_COMSTART_FIRST(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 16) & 1) != 0;
        }

        public static bool SYNTAX_COMSTART_SECOND(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 17) & 1) != 0;
        }

        public static bool SYNTAX_COMEND_FIRST(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 18) & 1) != 0;
        }

        public static bool SYNTAX_COMEND_SECOND(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 19) & 1) != 0;
        }

        public static bool SYNTAX_PREFIX(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 20) & 1) != 0;
        }

        public static bool SYNTAX_COMMENT_STYLE(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 21) & 1) != 0;
        }

        public static bool SYNTAX_COMMENT_NESTED(uint c)
        {
            return ((SYNTAX_WITH_FLAGS(c) >> 22) & 1) != 0;
        }

        /* Return the position across COUNT words from FROM.
           If that many words cannot be found before the end of the buffer, return 0.
           COUNT negative means scan backward and stop at word beginning.  */
        public static int scan_words(int from, int count)
        {
            int beg = BEGV();
            int end = ZV;
            int from_byte = CHAR_TO_BYTE(from);
            syntaxcode code;
            uint ch0, ch1;
            LispObject func, script, pos;

            immediate_quit = 1;
            QUIT();

            SETUP_SYNTAX_TABLE(from, count);

            while (count > 0)
            {
                while (true)
                {
                    if (from == end)
                    {
                        immediate_quit = 0;
                        return 0;
                    }
                    UPDATE_SYNTAX_TABLE_FORWARD(from);
                    ch0 = FETCH_CHAR_AS_MULTIBYTE(from_byte);
                    code = SYNTAX(ch0);
                    INC_BOTH(ref from, ref from_byte);
                    if (words_include_escapes
                        && (code == syntaxcode.Sescape || code == syntaxcode.Scharquote))
                        break;
                    if (code == syntaxcode.Sword)
                        break;
                }
                /* Now CH0 is a character which begins a word and FROM is the
                   position of the next character.  */
                func = CHAR_TABLE_REF(V.find_word_boundary_function_table, ch0);
                if (!NILP(F.fboundp(func)))
                {
                    pos = call2(func, make_number(from - 1), make_number(end));
                    if (INTEGERP(pos) && XINT(pos) > from)
                    {
                        from = XINT(pos);
                        from_byte = CHAR_TO_BYTE(from);
                    }
                }
                else
                {
                    script = CHAR_TABLE_REF(V.char_script_table, ch0);
                    while (true)
                    {
                        if (from == end) break;
                        UPDATE_SYNTAX_TABLE_FORWARD(from);
                        ch1 = FETCH_CHAR_AS_MULTIBYTE(from_byte);
                        code = SYNTAX(ch1);
                        if ((code != syntaxcode.Sword
                         && (!words_include_escapes
                             || (code != syntaxcode.Sescape && code != syntaxcode.Scharquote)))
                        || word_boundary_p(ch0, ch1))
                            break;
                        INC_BOTH(ref from, ref from_byte);
                        ch0 = ch1;
                    }
                }
                count--;
            }
            while (count < 0)
            {
                while (true)
                {
                    if (from == beg)
                    {
                        immediate_quit = 0;
                        return 0;
                    }
                    DEC_BOTH(ref from, ref from_byte);
                    UPDATE_SYNTAX_TABLE_BACKWARD(from);
                    ch1 = FETCH_CHAR_AS_MULTIBYTE(from_byte);
                    code = SYNTAX(ch1);
                    if (words_include_escapes
                        && (code == syntaxcode.Sescape || code == syntaxcode.Scharquote))
                        break;
                    if (code == syntaxcode.Sword)
                        break;
                }
                /* Now CH1 is a character which ends a word and FROM is the
                   position of it.  */
                func = CHAR_TABLE_REF(V.find_word_boundary_function_table, ch1);
                if (!NILP(F.fboundp(func)))
                {
                    pos = call2(func, make_number(from), make_number(beg));
                    if (INTEGERP(pos) && XINT(pos) < from)
                    {
                        from = XINT(pos);
                        from_byte = CHAR_TO_BYTE(from);
                    }
                }
                else
                {
                    script = CHAR_TABLE_REF(V.char_script_table, ch1);
                    while (true)
                    {
                        if (from == beg)
                            break;
                        DEC_BOTH(ref from, ref from_byte);
                        UPDATE_SYNTAX_TABLE_BACKWARD(from);
                        ch0 = FETCH_CHAR_AS_MULTIBYTE(from_byte);
                        code = SYNTAX(ch0);
                        if ((code != syntaxcode.Sword
                         && (!words_include_escapes
                             || (code != syntaxcode.Sescape && code != syntaxcode.Scharquote)))
                        || word_boundary_p(ch0, ch1))
                        {
                            INC_BOTH(ref from, ref from_byte);
                            break;
                        }
                        ch1 = ch0;
                    }
                }
                count++;
            }

            immediate_quit = 0;

            return from;
        }

        public static LispObject skip_chars (bool forwardp, LispObject stringg, LispObject lim, int handle_iso_classes)
        {
            uint c;
            byte[] fastmap = new byte[256];
            /* Store the ranges of non-ASCII characters.  */
            int[] char_ranges = null;
            int n_char_ranges = 0;
            int negate = 0;
            int i, i_byte;
            /* Set to 1 if the current buffer is multibyte and the region
               contains non-ASCII chars.  */
            bool multibyte;
            /* Set to 1 if STRING is multibyte and it contains non-ASCII
               chars.  */
            bool string_multibyte;
            int size_byte;
            byte[] str;
            int len = 0;
            LispObject iso_classes;

            CHECK_STRING(stringg);
            iso_classes = Q.nil;

            if (NILP(lim))
                lim = XSETINT(forwardp ? ZV : BEGV());
            else
                CHECK_NUMBER_COERCE_MARKER(ref lim);

            /* In any case, don't allow scan outside bounds of buffer.  */
            if (XINT(lim) > ZV)
                lim = XSETINT(ZV);
            if (XINT(lim) < BEGV())
                lim = XSETINT(BEGV());

            multibyte = (!NILP(current_buffer.enable_multibyte_characters)
                     && (XINT(lim) - PT() != CHAR_TO_BYTE(XINT(lim)) - PT_BYTE()));
            string_multibyte = SBYTES(stringg) > SCHARS(stringg);

            for (int x = 0; x < 256; x++)
            {
                fastmap[x] = 0;
            }

            str = SDATA(stringg);
            size_byte = SBYTES(stringg);

            i_byte = 0;
            if (i_byte < size_byte
                && SREF(stringg, 0) == '^')
            {
                negate = 1; i_byte++;
            }

            /* Find the characters specified and set their elements of fastmap.
               Handle backslashes and ranges specially.

               If STRING contains non-ASCII characters, setup char_ranges for
               them and use fastmap only for their leading codes.  */

            if (!string_multibyte)
            {
                int string_has_eight_bit = 0;

                /* At first setup fastmap.  */
                while (i_byte < size_byte)
                {
                    c = str[i_byte++];

                    if (handle_iso_classes != 0 && c == '['
                        && i_byte < size_byte
                        && str[i_byte] == ':')
                    {
                        int class_beg = i_byte + 1;
                        int class_end = class_beg;
                        int class_limit = size_byte - 2;
                        /* Leave room for the null.  */
                        byte[] class_name = new byte[CHAR_CLASS_MAX_LENGTH + 1];
                        re_wctype_t cc;

                        if (class_limit - class_beg > CHAR_CLASS_MAX_LENGTH)
                            class_limit = class_beg + CHAR_CLASS_MAX_LENGTH;

                        while (class_end < class_limit
                           && str[class_end] >= 'a' && str[class_end] <= 'z')
                            class_end++;

                        if (class_end == class_beg
                        || str[class_end] != ':' || str[class_end + 1] != ']')
                            goto not_a_class_name;

                        System.Array.Copy(str, class_beg, class_name, 0, class_end - class_beg);
                        class_name[class_end - class_beg] = 0;

                        cc = re_wctype(class_name);
                        if (cc == 0)
                            error("Invalid ISO C character class");

                        iso_classes = F.cons(make_number((int)cc), iso_classes);

                        i_byte = class_end + 2;
                        continue;
                    }

                not_a_class_name:
                    if (c == '\\')
                    {
                        if (i_byte == size_byte)
                            break;

                        c = str[i_byte++];
                    }
                    /* Treat `-' as range character only if another character
                       follows.  */
                    if (i_byte + 1 < size_byte
                        && str[i_byte] == '-')
                    {
                        uint c2;

                        /* Skip over the dash.  */
                        i_byte++;

                        /* Get the end of the range.  */
                        c2 = str[i_byte++];
                        if (c2 == '\\'
                        && i_byte < size_byte)
                            c2 = str[i_byte++];

                        if (c <= c2)
                        {
                            while (c <= c2)
                                fastmap[c++] = 1;
                            if (!ASCII_CHAR_P(c2))
                                string_has_eight_bit = 1;
                        }
                    }
                    else
                    {
                        fastmap[c] = 1;
                        if (!ASCII_CHAR_P(c))
                            string_has_eight_bit = 1;
                    }
                }

                /* If the current range is multibyte and STRING contains
               eight-bit chars, arrange fastmap and setup char_ranges for
               the corresponding multibyte chars.  */
                if (multibyte && string_has_eight_bit != 0)
                {
                    byte[] fastmap2 = new byte[256];
                    int range_start_byte, range_start_char;

                    System.Array.Copy(fastmap2, 128, fastmap, 128, 128);
                    for (int x = 128; x < 256; x++)
                        fastmap[x] = 0;

                    /* We are sure that this loop stops.  */
                    for (i = 128; fastmap2[i] == 0; i++)
                    {
                    }
                    c = unibyte_char_to_multibyte((uint)i);
                    fastmap[CHAR_LEADING_CODE(c)] = 1;
                    range_start_byte = i;
                    range_start_char = (int) c;
                    char_ranges = new int[128 * 2];
                    for (i = 129; i < 256; i++)
                    {
                        c = unibyte_char_to_multibyte((uint)i);
                        fastmap[CHAR_LEADING_CODE(c)] = 1;
                        if (i - range_start_byte != c - range_start_char)
                        {
                            char_ranges[n_char_ranges++] = range_start_char;
                            char_ranges[n_char_ranges++] = ((i - 1 - range_start_byte)
                                            + range_start_char);
                            range_start_byte = i;
                            range_start_char = (int) c;
                        }
                    }
                    char_ranges[n_char_ranges++] = range_start_char;
                    char_ranges[n_char_ranges++] = ((i - 1 - range_start_byte)
                                    + range_start_char);
                }
            }
            else				/* STRING is multibyte */
            {
                char_ranges = new int[SCHARS(stringg) * 2];

                while (i_byte < size_byte)
                {
                    byte leading_code;

                    leading_code = str[i_byte];
                    c = STRING_CHAR_AND_LENGTH(str, i_byte, size_byte - i_byte, ref len);
                    i_byte += len;

                    if (handle_iso_classes != 0 && c == '['
                        && i_byte < size_byte
                        && STRING_CHAR(str, i_byte, size_byte - i_byte) == ':')
                    {
                        int class_beg = i_byte + 1;
                        int class_end = class_beg;
                        int class_limit = size_byte - 2;
                        /* Leave room for the null.	 */
                        byte[] class_name = new byte[CHAR_CLASS_MAX_LENGTH + 1];
                        re_wctype_t cc;

                        if (class_limit - class_beg > CHAR_CLASS_MAX_LENGTH)
                            class_limit = class_beg + CHAR_CLASS_MAX_LENGTH;

                        while (class_end < class_limit
                           && str[class_end] >= 'a' && str[class_end] <= 'z')
                            class_end++;

                        if (class_end == class_beg
                        || str[class_end] != ':' || str[class_end + 1] != ']')
                            goto not_a_class_name_multibyte;

                        System.Array.Copy(str, class_beg, class_name, 0, class_end - class_beg);
                        class_name[class_end - class_beg] = 0;

                        cc = re_wctype(class_name);
                        if (cc == 0)
                            error("Invalid ISO C character class");

                        iso_classes = F.cons(make_number((int)cc), iso_classes);

                        i_byte = class_end + 2;
                        continue;
                    }

                not_a_class_name_multibyte:
                    if (c == '\\')
                    {
                        if (i_byte == size_byte)
                            break;

                        leading_code = str[i_byte];
                        c = STRING_CHAR_AND_LENGTH(str, i_byte,
                                    size_byte - i_byte, ref len);
                        i_byte += len;
                    }
                    /* Treat `-' as range character only if another character
                       follows.  */
                    if (i_byte + 1 < size_byte
                        && str[i_byte] == '-')
                    {
                        uint c2;
                        byte leading_code2;

                        /* Skip over the dash.  */
                        i_byte++;

                        /* Get the end of the range.  */
                        leading_code2 = str[i_byte];
                        c2 = STRING_CHAR_AND_LENGTH(str, i_byte,
                                     size_byte - i_byte, ref len);
                        i_byte += len;

                        if (c2 == '\\'
                        && i_byte < size_byte)
                        {
                            leading_code2 = str[i_byte];
                            c2 = STRING_CHAR_AND_LENGTH(str, i_byte, size_byte - i_byte, ref len);
                            i_byte += len;
                        }

                        if (c > c2)
                            continue;
                        if (ASCII_CHAR_P(c))
                        {
                            while (c <= c2 && c < 0x80)
                                fastmap[c++] = 1;
                            leading_code = CHAR_LEADING_CODE(c);
                        }
                        if (!ASCII_CHAR_P(c))
                        {
                            while (leading_code <= leading_code2)
                                fastmap[leading_code++] = 1;
                            if (c <= c2)
                            {
                                char_ranges[n_char_ranges++] = (int)c;
                                char_ranges[n_char_ranges++] = (int)c2;
                            }
                        }
                    }
                    else
                    {
                        if (ASCII_CHAR_P(c))
                            fastmap[c] = 1;
                        else
                        {
                            fastmap[leading_code] = 1;
                            char_ranges[n_char_ranges++] = (int)c;
                            char_ranges[n_char_ranges++] = (int)c;
                        }
                    }
                }

                /* If the current range is unibyte and STRING contains non-ASCII
               chars, arrange fastmap for the corresponding unibyte
               chars.  */

                if (!multibyte && n_char_ranges > 0)
                {
                    for (int x = 128; x < 256; x++)
                        fastmap[x] = 0;

                    for (i = 0; i < n_char_ranges; i += 2)
                    {
                        int c1 = char_ranges[i];
                        int c2 = char_ranges[i + 1];

                        for (; c1 <= c2; c1++)
                        {
                            int b = CHAR_TO_BYTE_SAFE((uint) c1);
                            if (b >= 0)
                                fastmap[b] = 1;
                        }
                    }
                }
            }

            /* If ^ was the first character, complement the fastmap.  */
            if (negate != 0)
            {
                if (!multibyte)
                    for (i = 0; i < 256; i++)
                        fastmap[i] ^= 1;
                else
                {
                    for (i = 0; i < 128; i++)
                        fastmap[i] ^= 1;
                    /* All non-ASCII chars possibly match.  */
                    for (; i < 256; i++)
                        fastmap[i] = 1;
                }
            }

            {
                int start_point = PT();
                int pos = PT();
                int pos_byte = PT_BYTE();
                PtrEmulator<byte> p = PT_ADDR(), endp, stop;

                if (forwardp)
                {
                    endp = (XINT(lim) == GPT) ? GPT_ADDR() : CHAR_POS_ADDR(XINT(lim));
                    stop = (pos < GPT && GPT < XINT(lim)) ? GPT_ADDR() : endp;
                }
                else
                {
                    endp = CHAR_POS_ADDR(XINT(lim));
                    stop = (pos >= GPT && GPT > XINT(lim)) ? GAP_END_ADDR() : endp;
                }

                immediate_quit = 1;
                if (forwardp)
                {
                    if (multibyte)
                        while (true)
                        {
                            int nbytes = 0;

                            if (p >= stop)
                            {
                                if (p >= endp)
                                    break;
                                p = GAP_END_ADDR();
                                stop = endp;
                            }
                            c = STRING_CHAR_AND_LENGTH(p.Collection, p.Index, MAX_MULTIBYTE_LENGTH, ref nbytes);
                            if (!NILP(iso_classes) && in_classes(c, iso_classes))
                            {
                                if (negate != 0)
                                    break;
                                else
                                    goto fwd_ok;
                            }

                            if (fastmap[p.Value] == 0)
                                break;
                            if (!ASCII_CHAR_P(c))
                            {
                                /* As we are looking at a multibyte character, we
                                   must look up the character in the table
                                   CHAR_RANGES.  If there's no data in the table,
                                   that character is not what we want to skip.  */

                                /* The following code do the right thing even if
                                   n_char_ranges is zero (i.e. no data in
                                   CHAR_RANGES).  */
                                for (i = 0; i < n_char_ranges; i += 2)
                                    if (c >= char_ranges[i] && c <= char_ranges[i + 1])
                                        break;
                                if ((negate ^ (i < n_char_ranges ? 1 : 0)) == 0)
                                    break;
                            }
                        fwd_ok:
                            p += nbytes;
                            pos++;
                            pos_byte += nbytes;
                        }
                    else
                        while (true)
                        {
                            if (p >= stop)
                            {
                                if (p >= endp)
                                    break;
                                p = GAP_END_ADDR();
                                stop = endp;
                            }

                            if (!NILP(iso_classes) && in_classes(p.Value, iso_classes))
                            {
                                if (negate != 0)
                                    break;
                                else
                                    goto fwd_unibyte_ok;
                            }

                            if (fastmap[p.Value] == 0)
                                break;
                        fwd_unibyte_ok:
                            p++;
                            pos++;
                            pos_byte++;
                        }
                }
                else
                {
                    if (multibyte)
                        while (true)
                        {
                            PtrEmulator<byte> prev_p;

                            if (p <= stop)
                            {
                                if (p <= endp)
                                    break;
                                p = GPT_ADDR();
                                stop = endp;
                            }
                            prev_p = p;
                            while (--p >= stop && !CHAR_HEAD_P(p.Value)) ;
                            c = STRING_CHAR(p.Collection, p.Index, MAX_MULTIBYTE_LENGTH);

                            if (!NILP(iso_classes) && in_classes(c, iso_classes))
                            {
                                if (negate != 0)
                                    break;
                                else
                                    goto back_ok;
                            }

                            if (fastmap[p.Value] == 0)
                                break;
                            if (!ASCII_CHAR_P(c))
                            {
                                /* See the comment in the previous similar code.  */
                                for (i = 0; i < n_char_ranges; i += 2)
                                    if (c >= char_ranges[i] && c <= char_ranges[i + 1])
                                        break;
                                if ((negate ^ (i < n_char_ranges ? 1 : 0)) == 0)
                                    break;
                            }
                        back_ok:
                            pos--;
                            pos_byte -= prev_p - p;
                        }
                    else
                        while (true)
                        {
                            if (p <= stop)
                            {
                                if (p <= endp)
                                    break;
                                p = GPT_ADDR();
                                stop = endp;
                            }

                            if (!NILP(iso_classes) && in_classes(p[-1], iso_classes))
                            {
                                if (negate != 0)
                                    break;
                                else
                                    goto back_unibyte_ok;
                            }

                            if (fastmap[p[-1]] == 0)
                                break;
                        back_unibyte_ok:
                            p--;
                            pos--;
                            pos_byte--;
                        }
                }

                SET_PT_BOTH(pos, pos_byte);
                immediate_quit = 0;

                return make_number(PT() - start_point);
            }
        }

        /* Return 1 if character C belongs to one of the ISO classes
           in the list ISO_CLASSES.  Each class is represented by an
           integer which is its type according to re_wctype.  */
        public static bool in_classes (uint c, LispObject iso_classes)
        {
            bool fits_class = false;

            while (!NILP(iso_classes))
            {
                LispObject elt;
                elt = XCAR(iso_classes);
                iso_classes = XCDR(iso_classes);

                if (re_iswctype(c, (re_wctype_t) XINT(elt)))
                    fits_class = true;
            }

            return fits_class;
        }

        // 1 + max-number of intervals to scan to property-change.
        public const int INTERVALS_AT_ONCE = 10;

        /* Update gl_state to an appropriate interval which contains CHARPOS.  The
           sign of COUNT give the relative position of CHARPOS wrt the previously
           valid interval.  If INIT, only [be]_property fields of gl_state are
           valid at start, the rest is filled basing on OBJECT.

           `gl_state.*_i' are the intervals, and CHARPOS is further in the search
           direction than the intervals - or in an interval.  We update the
           current syntax-table basing on the property of this interval, and
           update the interval to start further than CHARPOS - or be
           NULL_INTERVAL.  We also update lim_property to be the next value of
           charpos to call this subroutine again - or be before/after the
           start/end of OBJECT.  */
        public static void update_syntax_table(int charpos, int count, int init, LispObject obj)
        {
            LispObject tmp_table;
            int cnt = 0;
            bool invalidate = true;
            Interval i;

            if (init != 0)
            {
                gl_state.old_prop = Q.nil;
                gl_state.start = gl_state.b_property;
                gl_state.stop = gl_state.e_property;
                i = interval_of(charpos, obj);
                gl_state.backward_i = gl_state.forward_i = i;
                invalidate = false;
                if (NULL_INTERVAL_P(i))
                    return;
                /* interval_of updates only ->position of the return value, so
               update the parents manually to speed up update_interval.  */
                while (!NULL_PARENT(i))
                {
                    if (AM_RIGHT_CHILD(i))
                        i.ParentInterval.position = i.position
                          - LEFT_TOTAL_LENGTH(i) + TOTAL_LENGTH(i) /* right end */
                          - TOTAL_LENGTH(i.ParentInterval)
                          + LEFT_TOTAL_LENGTH(i.ParentInterval);
                    else
                        i.ParentInterval.position = i.position - LEFT_TOTAL_LENGTH(i)
                          + TOTAL_LENGTH(i);
                    i = i.ParentInterval;
                }
                i = gl_state.forward_i;
                gl_state.b_property = (int) i.position - gl_state.offset;
                gl_state.e_property = (int) INTERVAL_LAST_POS(i) - gl_state.offset;
                goto update;
            }
            i = count > 0 ? gl_state.forward_i : gl_state.backward_i;

            /* We are guaranteed to be called with CHARPOS either in i,
               or further off.  */
            if (NULL_INTERVAL_P(i))
                error("Error in syntax_table logic for to-the-end intervals");
            else if (charpos < i.position)		/* Move left.  */
            {
                if (count > 0)
                    error("Error in syntax_table logic for intervals <-");
                /* Update the interval.  */
                i = update_interval(i, charpos);
                if (INTERVAL_LAST_POS(i) != gl_state.b_property)
                {
                    invalidate = false;
                    gl_state.forward_i = i;
                    gl_state.e_property = (int) INTERVAL_LAST_POS(i) - gl_state.offset;
                }
            }
            else if (charpos >= INTERVAL_LAST_POS(i)) /* Move right.  */
            {
                if (count < 0)
                    error("Error in syntax_table logic for intervals ->");
                /* Update the interval.  */
                i = update_interval(i, charpos);
                if (i.position != gl_state.e_property)
                {
                    invalidate = false;
                    gl_state.backward_i = i;
                    gl_state.b_property = (int) i.position - gl_state.offset;
                }
            }

        update:
            tmp_table = textget(i.plist, Q.syntax_table);

            if (invalidate)
                invalidate = !EQ(tmp_table, gl_state.old_prop); /* Need to invalidate? */

            if (invalidate)		/* Did not get to adjacent interval.  */
            {				/* with the same table => */
                /* invalidate the old range.  */
                if (count > 0)
                {
                    gl_state.backward_i = i;
                    gl_state.b_property = (int) i.position - gl_state.offset;
                }
                else
                {
                    gl_state.forward_i = i;
                    gl_state.e_property = (int) INTERVAL_LAST_POS(i) - gl_state.offset;
                }
            }

            if (!EQ(tmp_table, gl_state.old_prop))
            {
                gl_state.current_syntax_table = tmp_table;
                gl_state.old_prop = tmp_table;
                if (EQ(F.syntax_table_p(tmp_table), Q.t))
                {
                    gl_state.use_global = 0;
                }
                else if (CONSP(tmp_table))
                {
                    gl_state.use_global = 1;
                    gl_state.global_code = tmp_table;
                }
                else
                {
                    gl_state.use_global = 0;
                    gl_state.current_syntax_table = current_buffer.syntax_table;
                }
            }

            while (!NULL_INTERVAL_P(i))
            {
                if (cnt != 0 && !EQ(tmp_table, textget(i.plist, Q.syntax_table)))
                {
                    if (count > 0)
                    {
                        gl_state.e_property = (int) i.position - gl_state.offset;
                        gl_state.forward_i = i;
                    }
                    else
                    {
                        gl_state.b_property
                      = (int)(i.position + LENGTH(i)) - gl_state.offset;
                        gl_state.backward_i = i;
                    }
                    return;
                }
                else if (cnt == INTERVALS_AT_ONCE)
                {
                    if (count > 0)
                    {
                        gl_state.e_property
                      = (int)(i.position + LENGTH(i)) - gl_state.offset
                            /* e_property at EOB is not set to ZV but to ZV+1, so that
                               we can do INC(from);UPDATE_SYNTAX_TABLE_FORWARD without
                               having to check eob between the two.  */
                      + (NULL_INTERVAL_P(next_interval(i)) ? 1 : 0);
                        gl_state.forward_i = i;
                    }
                    else
                    {
                        gl_state.b_property = (int) i.position - gl_state.offset;
                        gl_state.backward_i = i;
                    }
                    return;
                }
                cnt++;
                i = count > 0 ? next_interval(i) : previous_interval(i);
            }
            // eassert(NULL_INTERVAL_P(i)); /* This property goes to the end.  */
            if (count > 0)
                gl_state.e_property = gl_state.stop;
            else
                gl_state.b_property = gl_state.start;
        }
    }

    public partial class F
    {
        public static LispObject forward_word(LispObject arg)
        {
            LispObject tmp;
            int orig_val, val;

            if (L.NILP(arg))
                arg = L.XSETINT(1);
            else
                L.CHECK_NUMBER(arg);

            val = orig_val = L.scan_words(L.PT(), L.XINT(arg));
            if (orig_val == 0)
                val = L.XINT(arg) > 0 ? L.ZV : L.BEGV();

            /* Avoid jumping out of an input field.  */
            tmp = F.constrain_to_field(L.make_number(val), L.make_number(L.PT()),
                 Q.t, Q.nil, Q.nil);
            val = L.XINT(tmp);

            L.SET_PT(val);
            return val == orig_val ? Q.t : Q.nil;
        }

        public static LispObject skip_chars_forward(LispObject stringg, LispObject lim)
        {
            return L.skip_chars(true, stringg, lim, 1);
        }

        public static LispObject skip_chars_backward(LispObject stringg, LispObject lim)
        {
            return L.skip_chars(false, stringg, lim, 1);
        }

        public static LispObject syntax_table_p(LispObject obj)
        {
            if (L.CHAR_TABLE_P(obj)
                && L.EQ(L.XCHAR_TABLE(obj).purpose, Q.syntax_table))
                return Q.t;
            return Q.nil;
        }
    }
}