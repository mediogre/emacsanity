namespace IronElisp
{
    public partial class L
    {
        /* 64/16/32/128 */

        /* Number of elements in Nth level char-table.  */
        public static int[] chartab_size = new int [] { (1 << LispCharTable.CHARTAB_SIZE_BITS_0),
    (1 << LispCharTable.CHARTAB_SIZE_BITS_1),
    (1 << LispCharTable.CHARTAB_SIZE_BITS_2),
    (1 << LispCharTable.CHARTAB_SIZE_BITS_3) };

        /* Number of characters each element of Nth level char-table
           covers.  */
        public static int[] chartab_chars = new int[]{
    (1 << (LispCharTable.CHARTAB_SIZE_BITS_1 + LispCharTable.CHARTAB_SIZE_BITS_2 + LispCharTable.CHARTAB_SIZE_BITS_3)),
    (1 << (LispCharTable.CHARTAB_SIZE_BITS_2 + LispCharTable.CHARTAB_SIZE_BITS_3)),
    (1 << LispCharTable.CHARTAB_SIZE_BITS_3),
    1 };

        /* Number of characters (in bits) each element of Nth level char-table
           covers.  */
        public static int[] chartab_bits =
  { (LispCharTable.CHARTAB_SIZE_BITS_1 + LispCharTable.CHARTAB_SIZE_BITS_2 + LispCharTable.CHARTAB_SIZE_BITS_3),
    (LispCharTable.CHARTAB_SIZE_BITS_2 + LispCharTable.CHARTAB_SIZE_BITS_3),
    LispCharTable.CHARTAB_SIZE_BITS_3,
    0 };

        public static int CHARTAB_IDX(int c, int depth, int min_char)
        {
            return (((c) - (min_char)) >> chartab_bits[(depth)]);
        }

        public static LispObject sub_char_table_ref(LispObject table, int c)
        {
            LispSubCharTable tbl = XSUB_CHAR_TABLE(table);
            int depth = XINT(tbl.depth);
            int min_char = XINT(tbl.min_char);
            LispObject val = tbl.contents(CHARTAB_IDX(c, depth, min_char));
            if (SUB_CHAR_TABLE_P(val))
                val = sub_char_table_ref(val, c);
            return val;
        }

        public static LispObject char_table_ref(LispObject table, int c)
        {
            LispCharTable tbl = XCHAR_TABLE(table);
            LispObject val;

            if (ASCII_CHAR_P((uint) c))
            {
                val = tbl.ascii;
                if (SUB_CHAR_TABLE_P(val))
                    val = XSUB_CHAR_TABLE(val).contents(c);
            }
            else
            {
                val = tbl.contents(CHARTAB_IDX(c, 0, 0));
                if (SUB_CHAR_TABLE_P(val))
                    val = sub_char_table_ref(val, c);
            }
            if (NILP(val))
            {
                val = tbl.defalt;
                if (NILP(val) && CHAR_TABLE_P(tbl.parent))
                    val = char_table_ref(tbl.parent, c);
            }
            return val;
        }

        /* Map C_FUNCTION or FUNCTION over TABLE (top or sub char-table),
           calling it for each character or group of characters that share a
           value.  RANGE is a cons (FROM . TO) specifying the range of target
           characters, VAL is a value of FROM in TABLE, DEFAULT_VAL is the
           default value of the char-table, PARENT is the parent of the
           char-table.

           ARG is passed to C_FUNCTION when that is called.

           It returns the value of last character covered by TABLE (not the
           value inheritted from the parent), and by side-effect, the car part
           of RANGE is updated to the minimum character C where C and all the
           following characters in TABLE have the same value.  */
        public static LispObject map_sub_char_table(subr3 c_function, LispObject function, LispObject table, LispObject arg, LispObject val, LispObject range,
            LispObject default_val, LispObject parent)
        {
            /* Pointer to the elements of TABLE. */
            ICharTable contents;
            /* Depth of TABLE.  */
            int depth;
            /* Minimum and maxinum characters covered by TABLE. */
            int min_char, max_char;
            /* Number of characters covered by one element of TABLE.  */
            int chars_in_block;
            int from = XINT(XCAR(range)), to = XINT(XCDR(range));
            int i, c;

            if (SUB_CHAR_TABLE_P(table))
            {
                LispSubCharTable tbl = XSUB_CHAR_TABLE(table);

                depth = XINT(tbl.depth);
                contents = tbl;
                min_char = XINT(tbl.min_char);
                max_char = min_char + chartab_chars[depth - 1] - 1;
            }
            else
            {
                depth = 0;
                contents = XCHAR_TABLE(table);
                min_char = 0;
                max_char = (int) MAX_CHAR;
            }
            chars_in_block = chartab_chars[depth];

            if (to < max_char)
                max_char = to;
            /* Set I to the index of the first element to check.  */
            if (from <= min_char)
                i = 0;
            else
                i = (from - min_char) / chars_in_block;
            for (c = min_char + chars_in_block * i; c <= max_char;
                 i++, c += chars_in_block)
            {
                LispObject thiss = contents.contents(i);
                int nextc = c + chars_in_block;

                if (SUB_CHAR_TABLE_P(thiss))
                {
                    if (to >= nextc)
                        XSETCDR(range, make_number(nextc - 1));
                    val = map_sub_char_table(c_function, function, thiss, arg,
                                  val, range, default_val, parent);
                }
                else
                {
                    if (NILP(thiss))
                        thiss = default_val;
                    if (!EQ(val, thiss))
                    {
                        bool different_value = true;

                        if (NILP(val))
                        {
                            if (!NILP(parent))
                            {
                                LispObject temp = XCHAR_TABLE(parent).parent;

                                /* This is to get a value of FROM in PARENT
                               without checking the parent of PARENT.  */
                                XCHAR_TABLE(parent).parent = Q.nil;
                                val = CHAR_TABLE_REF(parent, (uint)from);
                                XCHAR_TABLE(parent).parent = temp;
                                XSETCDR(range, make_number(c - 1));
                                val = map_sub_char_table(c_function, function,
                                          parent, arg, val, range,
                                          XCHAR_TABLE(parent).defalt,
                                          XCHAR_TABLE(parent).parent);
                                if (EQ(val, thiss))
                                    different_value = false;
                            }
                        }
                        if (!NILP(val) && different_value)
                        {
                            XSETCDR(range, make_number(c - 1));
                            if (EQ(XCAR(range), XCDR(range)))
                            {
                                if (c_function != null)
                                    c_function(arg, XCAR(range), val);
                                else
                                    call2(function, XCAR(range), val);
                            }
                            else
                            {
                                if (c_function != null)
                                    c_function(arg, range, val);
                                else
                                    call2(function, range, val);
                            }
                        }
                        val = thiss;
                        from = c;
                        XSETCAR(range, make_number(c));
                    }
                }
                XSETCDR(range, make_number(to));
            }
            return val;
        }

        /* Map C_FUNCTION or FUNCTION over TABLE, calling it for each
           character or group of characters that share a value.

           ARG is passed to C_FUNCTION when that is called.  */
        public static void map_char_table(subr3 c_function, LispObject function, LispObject table, LispObject arg)
        {
            LispObject range, val;

            range = F.cons(make_number(0), make_number((int) MAX_CHAR));
            val = XCHAR_TABLE(table).ascii;
            if (SUB_CHAR_TABLE_P(val))
                val = XSUB_CHAR_TABLE(val).contents(0);
            val = map_sub_char_table(c_function, function, table, arg, val, range,
                          XCHAR_TABLE(table).defalt,
                          XCHAR_TABLE(table).parent);
            /* If VAL is nil and TABLE has a parent, we must consult the parent
               recursively.  */
            while (NILP(val) && !NILP(XCHAR_TABLE(table).parent))
            {
                LispObject parent = XCHAR_TABLE(table).parent;
                LispObject temp = XCHAR_TABLE(parent).parent;
                int from = XINT(XCAR(range));

                /* This is to get a value of FROM in PARENT without checking the
               parent of PARENT.  */
                XCHAR_TABLE(parent).parent = Q.nil;
                val = CHAR_TABLE_REF(parent, (uint) from);
                XCHAR_TABLE(parent).parent = temp;
                val = map_sub_char_table(c_function, function, parent, arg, val, range,
                          XCHAR_TABLE(parent).defalt,
                          XCHAR_TABLE(parent).parent);
                table = parent;
            }

            if (!NILP(val))
            {
                if (EQ(XCAR(range), XCDR(range)))
                {
                    if (c_function != null)
                        c_function(arg, XCAR(range), val);
                    else
                        call2(function, XCAR(range), val);
                }
                else
                {
                    if (c_function != null)
                        c_function(arg, range, val);
                    else
                        call2(function, range, val);
                }
            }

        }

        public static void sub_char_table_set(LispObject table, int c, LispObject val)
        {
            LispSubCharTable tbl = XSUB_CHAR_TABLE(table);
            int depth = XINT((tbl).depth);
            int min_char = XINT((tbl).min_char);
            int i = CHARTAB_IDX(c, depth, min_char);
            LispObject sub;

            if (depth == 3)
                tbl.set_contents(i, val);
            else
            {
                sub = tbl.contents(i);
                if (!SUB_CHAR_TABLE_P(sub))
                {
                    sub = make_sub_char_table(depth + 1,
                                   min_char + i * chartab_chars[depth], sub);
                    tbl.set_contents(i, sub);
                }
                sub_char_table_set(sub, c, val);
            }
        }

        public static LispObject char_table_set(LispObject table, int c, LispObject val)
        {
            LispCharTable tbl = XCHAR_TABLE(table);

            if (ASCII_CHAR_P((uint)c)
                && SUB_CHAR_TABLE_P(tbl.ascii))
            {
                XSUB_CHAR_TABLE(tbl.ascii).set_contents(c, val);
            }
            else
            {
                int i = CHARTAB_IDX(c, 0, 0);
                LispObject sub;

                sub = tbl.contents(i);
                if (!SUB_CHAR_TABLE_P(sub))
                {
                    sub = make_sub_char_table(1, i * chartab_chars[0], sub);
                    tbl.set_contents(i, sub);
                }
                sub_char_table_set(sub, c, val);
                if (ASCII_CHAR_P((uint)c))
                    tbl.ascii = char_table_ascii(table);
            }
            return val;
        }

        public static void sub_char_table_set_range(ref LispObject table, int depth, int min_char, int from, int to, LispObject val)
        {
            int max_char = min_char + chartab_chars[depth] - 1;

            if (depth == 3 || (from <= min_char && to >= max_char))
                table = val;
            else
            {
                int i, j;

                depth++;
                if (!SUB_CHAR_TABLE_P(table))
                    table = make_sub_char_table(depth, min_char, table);
                if (from < min_char)
                    from = min_char;
                if (to > max_char)
                    to = max_char;
                i = CHARTAB_IDX(from, depth, min_char);
                j = CHARTAB_IDX(to, depth, min_char);
                min_char += chartab_chars[depth] * i;
                for (; i <= j; i++, min_char += chartab_chars[depth])
                {
                    LispObject tmp = XSUB_CHAR_TABLE(table).contents(i);
                    sub_char_table_set_range(ref tmp,
                                  depth, min_char, from, to, val);
                    XSUB_CHAR_TABLE(table).set_contents(i, tmp);
                }
            }
        }

        public static LispObject char_table_set_range(LispObject table, int from, int to, LispObject val)
        {
            LispCharTable tbl = XCHAR_TABLE(table);
            int i, min_char;

            if (from == to)
                char_table_set(table, from, val);
            else
            {
                for (i = CHARTAB_IDX(from, 0, 0), min_char = i * chartab_chars[0];
                 min_char <= to;
                 i++, min_char += chartab_chars[0])
                {
                    LispObject tmp = tbl.contents(i);
                    sub_char_table_set_range(ref tmp, 0, min_char, from, to, val);
                    tbl.set_contents(i, tmp);
                }
                if (ASCII_CHAR_P((uint)from))
                    tbl.ascii = char_table_ascii(table);
            }
            return val;
        }

        public static LispObject make_sub_char_table(int depth, int min_char, LispObject defalt)
        {
            LispObject table;
            int size = LispSubCharTable.VECSIZE - 1 + chartab_size[depth];

            table = F.make_vector(make_number(size), defalt);
            table = new LispSubCharTable(XVECTOR(table));
            XSUB_CHAR_TABLE(table).depth = make_number(depth);
            XSUB_CHAR_TABLE(table).min_char = make_number(min_char);

            return table;
        }

        public static LispObject char_table_ascii(LispObject table)
        {
            LispObject sub;

            sub = XCHAR_TABLE(table).contents(0);
            if (!SUB_CHAR_TABLE_P(sub))
                return sub;
            sub = XSUB_CHAR_TABLE(sub).contents(0);
            if (!SUB_CHAR_TABLE_P(sub))
                return sub;
            return XSUB_CHAR_TABLE(sub).contents(0);
        }

        public static LispObject copy_sub_char_table(LispObject table)
        {
            LispObject copy;
            int depth = XINT(XSUB_CHAR_TABLE(table).depth);
            int min_char = XINT(XSUB_CHAR_TABLE(table).min_char);
            LispObject val;
            int i;

            copy = make_sub_char_table(depth, min_char, Q.nil);
            /* Recursively copy any sub char-tables.  */
            for (i = 0; i < chartab_size[depth]; i++)
            {
                val = XSUB_CHAR_TABLE(table).contents(i);
                if (SUB_CHAR_TABLE_P(val))
                    XSUB_CHAR_TABLE(copy).set_contents(i, copy_sub_char_table(val));
                else
                    XSUB_CHAR_TABLE(copy).set_contents(i, val);
            }

            return copy;
        }

        public static LispObject copy_char_table(LispObject table)
        {
            LispObject copy;
            int size = XCHAR_TABLE(table).Size;
            int i;

            copy = F.make_vector(make_number(size), Q.nil);
            copy = new LispCharTable(XVECTOR(copy));
            XCHAR_TABLE(copy).defalt = XCHAR_TABLE(table).defalt;
            XCHAR_TABLE(copy).parent = XCHAR_TABLE(table).parent;
            XCHAR_TABLE(copy).purpose = XCHAR_TABLE(table).purpose;
            for (i = 0; i < chartab_size[0]; i++)
                XCHAR_TABLE(copy).set_contents(i,
                  (SUB_CHAR_TABLE_P(XCHAR_TABLE(table).contents(i))
                 ? copy_sub_char_table(XCHAR_TABLE(table).contents(i))
                 : XCHAR_TABLE(table).contents(i)));
            XCHAR_TABLE(copy).ascii = char_table_ascii(copy);
            size -= LispCharTable.VECSIZE - 1;
            for (i = 0; i < size; i++)
                XCHAR_TABLE(copy).set_extras(i, XCHAR_TABLE(table).extras(i));

            return copy;
        }

        /* Look up the element in TABLE at index CH, and return it as an
           integer.  If the element is not a character, return CH itself.  */
        public static int char_table_translate (LispObject table, int ch)
        {
            LispObject value;
            value = F.aref(table, make_number(ch));
            if (!CHARACTERP(value))
                return ch;
            return XINT(value);
        }
    }

    public partial class F
    {
        public static LispObject set_char_table_range(LispObject char_table, LispObject range, LispObject value)
        {
            L.CHECK_CHAR_TABLE(char_table);
            if (L.EQ(range, Q.t))
            {
                int i;

                L.XCHAR_TABLE(char_table).ascii = value;
                for (i = 0; i < L.chartab_size[0]; i++)
                    L.XCHAR_TABLE(char_table).set_contents(i, value);
            }
            else if (L.EQ(range, Q.nil))
                L.XCHAR_TABLE(char_table).defalt = value;
            else if (L.INTEGERP(range))
                L.char_table_set(char_table, L.XINT(range), value);
            else if (L.CONSP(range))
            {
                L.CHECK_CHARACTER_CAR(range);
                L.CHECK_CHARACTER_CDR(range);
                L.char_table_set_range(char_table,
                          L.XINT(L.XCAR(range)), L.XINT(L.XCDR(range)), value);
            }
            else
                L.error("Invalid RANGE argument to `set-char-table-range'");

            return value;
        }

        public static LispObject make_char_table(LispObject purpose, LispObject init)
        {
            LispObject vector;
            LispObject n;
            int n_extras;
            int size;

            L.CHECK_SYMBOL(purpose);
            n = F.get(purpose, Q.char_table_extra_slots);
            if (L.NILP(n))
                n_extras = 0;
            else
            {
                L.CHECK_NATNUM(n);
                n_extras = L.XINT(n);
                if (n_extras > 10)
                    L.args_out_of_range(n, Q.nil);
            }

            size = LispCharTable.VECSIZE - 1 + n_extras;
            vector = F.make_vector(L.make_number(size), init);

            vector = new LispCharTable(L.XVECTOR(vector));
            L.XCHAR_TABLE(vector).parent = Q.nil;
            L.XCHAR_TABLE(vector).purpose = purpose;
            return vector;
        }
    }
}