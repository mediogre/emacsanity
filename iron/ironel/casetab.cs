namespace IronElisp
{
    public partial class Q
    {
        public static LispObject case_table_p, case_table;
    }

    public partial class V
    {
        public static LispObject ascii_downcase_table;
        public static LispObject ascii_upcase_table;
        public static LispObject ascii_canon_table, ascii_eqv_table;
    }

    public partial class F
    {
        public static LispObject set_case_table(LispObject table)
        {
            return L.set_case_table(table, false);
        }

        public static LispObject set_standard_case_table(LispObject table)
        {
            return L.set_case_table(table, true);
        }

        public static LispObject case_table_p(LispObject obj)
        {
            LispObject up, canon, eqv;

            if (!L.CHAR_TABLE_P(obj))
                return Q.nil;
            if (!L.EQ(L.XCHAR_TABLE(obj).purpose, Q.case_table))
                return Q.nil;

            up = L.XCHAR_TABLE(obj).extras(0);
            canon = L.XCHAR_TABLE(obj).extras(1);
            eqv = L.XCHAR_TABLE(obj).extras(2);

            return ((L.NILP(up) || L.CHAR_TABLE_P(up))
                && ((L.NILP(canon) && L.NILP(eqv))
                    || (L.CHAR_TABLE_P(canon)
                    && (L.NILP(eqv) || L.CHAR_TABLE_P(eqv))))
                ? Q.t : Q.nil);
        }
    }

    public partial class L
    {
        public static LispObject check_case_table(LispObject obj)
        {
            CHECK_TYPE(!NILP(F.case_table_p(obj)), Q.case_table_p, obj);
            return (obj);
        }

        public static LispObject set_case_table(LispObject table, bool standard)
        {
            LispObject up, canon, eqv;

            check_case_table(table);

            up = XCHAR_TABLE(table).extras(0);
            canon = XCHAR_TABLE(table).extras(1);
            eqv = XCHAR_TABLE(table).extras(2);

            if (NILP(up))
            {
                up = F.make_char_table(Q.case_table, Q.nil);
                map_char_table(set_identity, Q.nil, table, up);
                map_char_table(shuffle, Q.nil, table, up);
                XCHAR_TABLE(table).set_extras(0, up);
            }

            if (NILP(canon))
            {
                canon = F.make_char_table(Q.case_table, Q.nil);
                XCHAR_TABLE(table).set_extras(1, canon);
                map_char_table(set_canon, Q.nil, table, table);
            }

            if (NILP(eqv))
            {
                eqv = F.make_char_table(Q.case_table, Q.nil);
                map_char_table(set_identity, Q.nil, canon, eqv);
                map_char_table(shuffle, Q.nil, canon, eqv);
                XCHAR_TABLE(table).set_extras(2, eqv);
            }

            /* This is so set_image_of_range_1 in regex.c can find the EQV table.  */
            XCHAR_TABLE(canon).set_extras(2, eqv);

            if (standard)
            {
                V.ascii_downcase_table = table;
                V.ascii_upcase_table = up;
                V.ascii_canon_table = canon;
                V.ascii_eqv_table = eqv;
            }
            else
            {
                current_buffer.downcase_table = table;
                current_buffer.upcase_table = up;
                current_buffer.case_canon_table = canon;
                current_buffer.case_eqv_table = eqv;
            }

            return table;
        }

        /* The following functions are called in map_char_table.  */

        /* Set CANON char-table element for characters in RANGE to a
           translated ELT by UP and DOWN char-tables.  This is done only when
           ELT is a character.  The char-tables CANON, UP, and DOWN are in
           CASE_TABLE.  */
        public static LispObject set_canon(LispObject case_table, LispObject range, LispObject elt)
        {
            LispObject up = XCHAR_TABLE(case_table).extras(0);
            LispObject canon = XCHAR_TABLE(case_table).extras(1);

            if (NATNUMP(elt))
                F.set_char_table_range(canon, range, F.aref(case_table, F.aref(up, elt)));

            return Q.nil;
        }

        /* Set elements of char-table TABLE for C to C itself.  C may be a
           cons specifying a character range.  In that case, set characters in
           that range to themselves.  This is done only when ELT is a
           character.  This is called in map_char_table.  */
        public static LispObject set_identity(LispObject table, LispObject c, LispObject elt)
        {
            if (NATNUMP(elt))
            {
                int from, to;

                if (CONSP(c))
                {
                    from = XINT(XCAR(c));
                    to = XINT(XCDR(c));
                }
                else
                    from = to = XINT(c);
                for (; from <= to; from++)
                    CHAR_TABLE_SET(table, from, make_number(from));
            }
            return Q.nil;
        }

        /* Permute the elements of TABLE (which is initially an identity
           mapping) so that it has one cycle for each equivalence class
           induced by the translation table on which map_char_table is
           operated.  */
        public static LispObject shuffle(LispObject table, LispObject c, LispObject elt)
        {
            if (NATNUMP(elt))
            {
                LispObject tem = F.aref(table, elt);
                int from, to;

                if (CONSP(c))
                {
                    from = XINT(XCAR(c));
                    to = XINT(XCDR(c));
                }
                else
                    from = to = XINT(c);

                for (; from <= to; from++)
                    if (from != XINT(elt))
                    {
                        F.aset(table, elt, make_number(from));
                        F.aset(table, make_number(from), tem);
                    }
            }

            return Q.nil;
        }
    }
}