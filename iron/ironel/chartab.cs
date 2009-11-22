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
    }
}