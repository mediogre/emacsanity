namespace IronElisp
{
    public partial class Q
    {
        public static LispObject category_table, categoryp, categorysetp, category_table_p;
    }

    public partial class V
    {
        /* Variables to determine word boundary.  */
        public static LispObject word_separating_categories
        {
            get { return Defs.O[(int)Objects.word_separating_categories]; }
            set { Defs.O[(int)Objects.word_separating_categories] = value; }
        }

        public static LispObject word_combining_categories
        {
            get { return Defs.O[(int)Objects.word_combining_categories]; }
            set { Defs.O[(int)Objects.word_combining_categories] = value; }
        }
    }

    public partial class L
    {
        /* We introduce here three types of object: category, category set,
           and category table.

           A category is like syntax but differs in the following points:

           o A category is represented by a mnemonic character of the range
           ` '(32)..`~'(126) (printable ASCII characters).

           o A category is not exclusive, i.e. a character has multiple
           categories (category set).  Of course, there's a case that a
           category set is empty, i.e. the character has no category.

           o In addition to the predefined categories, a user can define new
           categories.  Total number of categories is limited to 95.

           A category set is a set of categories represented by Lisp
           bool-vector of length 128 (only elements of 31th through 126th
           are used).

           A category table is like syntax-table, represented by a Lisp
           char-table.  The contents are category sets or nil.  It has two
           extra slots, for a vector of doc string of each category and a
           version number.

           The first extra slot is a vector of doc strings of categories, the
           length is 95.  The Nth element corresponding to the category N+32.

           The second extra slot is a version number of the category table.
           But, for the moment, we are not using this slot.  */
        public static bool CATEGORYP(LispObject x)
        {
            return (INTEGERP(x) && XINT(x) >= 0x20 && XINT(x) <= 0x7E);
        }

        public static void CHECK_CATEGORY(LispObject x) 
        {
            CHECK_TYPE(CATEGORYP(x), Q.categoryp, x);
        }

        public static LispBoolVector XCATEGORY_SET(LispObject x)
        {
            return XBOOL_VECTOR(x);
        }

        public static bool CATEGORY_SET_P(LispObject x) 
        {
            return (BOOL_VECTOR_P(x) && XBOOL_VECTOR(x).Size == 128);
        }

        /* Return a new empty category set.  */
        public static LispObject MAKE_CATEGORY_SET()
        {
            return F.make_bool_vector(make_number(128), Q.nil);
        }

        /* Make CATEGORY_SET includes (if VAL is t) or excludes (if VAL is
           nil) CATEGORY.  */
        public static void SET_CATEGORY_SET(LispObject category_set, LispObject category, LispObject val) 
        {
            set_category_set(category_set, category, val);
        }

        public static void CHECK_CATEGORY_SET(LispObject x) 
        {
            CHECK_TYPE(CATEGORY_SET_P(x), Q.categorysetp, x);
        }

        /* Return 1 if CATEGORY_SET contains CATEGORY, else return 0.
           The faster version of `!NILP (Faref (category_set, category))'.  */
        public static bool CATEGORY_MEMBER(int category, LispObject category_set)		 		
        {
            return (XCATEGORY_SET(category_set)[(category) / 8]
   & (byte)(1 << ((category) % 8))) != 0;
        }

        /* Return the category set of character C in the current category table.  */
        public static LispObject CATEGORY_SET(uint c)
        {
            return char_category_set(c);
        }

        /* Return 1 if there is a word boundary between two word-constituent
           characters C1 and C2 if they appear in this order, else return 0.
           There is no word boundary between two word-constituent ASCII and
           Latin-1 characters.  */
        public static bool WORD_BOUNDARY_P(uint c1, uint c2)
        {
            return (!(SINGLE_BYTE_CHAR_P(c1) && SINGLE_BYTE_CHAR_P(c2)) && word_boundary_p(c1, c2));
        }

        /* Return 1 if there is a word boundary between two word-constituent
           characters C1 and C2 if they appear in this order, else return 0.
           Use the macro WORD_BOUNDARY_P instead of calling this function
           directly.  */
        public static bool word_boundary_p(uint c1, uint c2)
        {
            LispObject category_set1, category_set2;
            LispObject tail;
            bool default_result;

            if (EQ(CHAR_TABLE_REF(V.char_script_table, c1),
                CHAR_TABLE_REF(V.char_script_table, c2)))
            {
                tail = V.word_separating_categories;
                default_result = false;
            }
            else
            {
                tail = V.word_combining_categories;
                default_result = true;
            }

            category_set1 = CATEGORY_SET(c1);
            if (NILP(category_set1))
                return default_result;
            category_set2 = CATEGORY_SET(c2);
            if (NILP(category_set2))
                return default_result;

            for (; CONSP(tail); tail = XCDR(tail))
            {
                LispObject elt = XCAR(tail);

                if (CONSP(elt)
                && (NILP(XCAR(elt))
                    || (CATEGORYP(XCAR(elt))
                    && CATEGORY_MEMBER(XINT(XCAR(elt)), category_set1)))
                && (NILP(XCDR(elt))
                    || (CATEGORYP(XCDR(elt))
                    && CATEGORY_MEMBER(XINT(XCDR(elt)), category_set2))))
                    return !default_result;
            }
            return default_result;
        }

        public static void set_category_set(LispObject category_set, LispObject category, LispObject val)
        {
            int idx = XINT(category) / 8;
            byte bits = (byte)(1 << (XINT(category) % 8));

            if (NILP(val))
                XCATEGORY_SET(category_set)[idx] &= (byte)~bits;
            else
                XCATEGORY_SET(category_set)[idx] |= bits;
        }

        public static LispObject char_category_set (uint c)
        {
            return CHAR_TABLE_REF(current_buffer.category_table, c);
        }

        /* Temporary internal variable used in macro CHAR_HAS_CATEGORY.  */
        public static LispObject _temp_category_set;

        /* Return 1 if category set of CH contains CATEGORY, elt return 0.  */
        public static bool CHAR_HAS_CATEGORY(int ch, int category)	
        {
            _temp_category_set = CATEGORY_SET((uint) ch);
            return CATEGORY_MEMBER(category, _temp_category_set);
        }
    }
}