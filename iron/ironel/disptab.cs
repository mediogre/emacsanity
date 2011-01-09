namespace IronElisp
{
    public partial class Q
    {
        /* This is the `purpose' slot of a display table.  */
        public static LispObject display_table;
    }

    public partial class L
    {
        public const int DISP_TABLE_EXTRA_SLOTS = 6;

        /* Access the slots of a display-table, according to their purpose.  */
        public static bool DISP_TABLE_P(LispObject obj)						    
        {
            return (CHAR_TABLE_P(obj) && EQ(XCHAR_TABLE(obj).purpose, Q.display_table) &&
 CHAR_TABLE_EXTRA_SLOTS(XCHAR_TABLE(obj)) == DISP_TABLE_EXTRA_SLOTS);
        }

        public static LispObject DISP_CHAR_VECTOR(LispCharTable dp, int c)
        {
            return (ASCII_CHAR_P((uint)c)
   ? (NILP((dp).ascii)
      ? (dp).defalt
      : (SUB_CHAR_TABLE_P((dp).ascii)
     ? XSUB_CHAR_TABLE((dp).ascii)[c]
     : (dp).ascii))
   : disp_char_vector((dp), (c)));
        }
    }
}