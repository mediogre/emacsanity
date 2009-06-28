namespace IronElisp
{
    public class Interval
    {
        public uint total_length;
        public uint position;

        public Interval left;
        public Interval right;

        public Interval up_interval;
        public LispObject up_object;

        public bool is_up_object;

        bool write_protect;
        bool visible;
        bool front_sticky;
        bool rear_sticky;

        public LispObject plist;

        public Interval()
        {
            total_length = 0;
            position = 0;
            left = null;
            right = null;

            ParentInterval = null;

            write_protect = false;
            visible = false;
            front_sticky = false;
            rear_sticky = false;

            plist = Q.nil;
        }

        public Interval ParentInterval
        {
            get
            {
                if (!is_up_object)
                {
                    return up_interval;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                is_up_object = false;
                up_interval = value;
            }
        }

        public LispObject ParentObject
        {
            get
            {
                if (is_up_object)
                {
                    return up_object;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                is_up_object = true;
                up_object = value;
            }
        }
    }

    public partial class L
    {
        public static bool NULL_INTERVAL_P(Interval i)
        {
            return i == null;
        }

        /* True if this interval has no right child. */
        public static bool NULL_RIGHT_CHILD(Interval i)
        {
            return i.right == null;
        }

        /* True if this interval has no left child. */
        public static bool NULL_LEFT_CHILD(Interval i)
        {
            return i.left == null;
        }

        /* True if this interval has no parent. */
        public static bool NULL_PARENT(Interval i)
        {
            return (i.is_up_object || i.ParentInterval == null);
        }

        /* True if this interval is the left child of some other interval. */
        public static bool AM_LEFT_CHILD(Interval i)
        {
            return (!NULL_PARENT(i) && i.ParentInterval.left == i);
        }

        /* True if this interval is the right child of some other interval. */
        public static bool AM_RIGHT_CHILD(Interval i)
        {
            return (!NULL_PARENT(i) && i.ParentInterval.right == i);
        }

        /* True if this interval has no parent and is therefore the root. */
        public static bool ROOT_INTERVAL_P(Interval i)
        {
            return NULL_PARENT(i);
        }

        /* The total size of all text represented by this interval and all its
           children in the tree.   This is zero if the interval is null. */
        public static uint TOTAL_LENGTH(Interval i)
        {
            return (i == null ? 0 : i.total_length);
        }

        /* The size of text represented by this interval alone. */
        public static uint LENGTH(Interval i)
        {
            if (i == null)
                return 0;

            return TOTAL_LENGTH(i) - TOTAL_LENGTH(i.right) - TOTAL_LENGTH(i.left);
        }

        /* The position of the character just past the end of I.  Note that
           the position cache i->position must be valid for this to work. */
        public static uint INTERVAL_LAST_POS(Interval i)
        {
            return i.position + LENGTH(i);
        }

        /* The total size of the left subtree of this interval. */
        public static uint LEFT_TOTAL_LENGTH(Interval i)
        {
            return (i.left != null ? i.left.total_length : 0);
        }

        /* The total size of the right subtree of this interval. */
        public static uint RIGHT_TOTAL_LENGTH(Interval i)
        {
            return (i.right != null ? i.right.total_length : 0);
        }

        /* Make the parent of D be whatever the parent of S is, regardless of
           type.  This is used when balancing an interval tree.  */
        public static void COPY_INTERVAL_PARENT(Interval d, Interval s)
        {
            d.up_interval = s.up_interval;
            d.up_object = s.up_object;

            d.is_up_object = s.is_up_object;
        }

        /* Abort if interval I's size is negative.  */
        public static void CHECK_TOTAL_LENGTH(Interval i)
        {
            if ((int)i.total_length < 0) abort();
        }

        /* True if this is a default interval, which is the same as being null
           or having no properties. */
        public static bool DEFAULT_INTERVAL_P(Interval i)
        {
            return (NULL_INTERVAL_P(i) || EQ(i.plist, Q.nil));
        }

        /* Assuming that a left child exists, perform the following operation:

             A		  B
            / \		 / \
           B       =>       A
          / \		   / \
             c		  c
        */

        public static Interval rotate_right(Interval interval)
        {
            Interval i;
            Interval B = interval.left;
            uint old_total = interval.total_length;

            /* Deal with any Parent of A;  make it point to B.  */
            if (!ROOT_INTERVAL_P(interval))
            {
                if (AM_LEFT_CHILD(interval))
                    interval.ParentInterval.left = B;
                else
                    interval.ParentInterval.right = B;
            }
            COPY_INTERVAL_PARENT(B, interval);

            /* Make B the parent of A */
            i = B.right;
            B.right = interval;
            interval.ParentInterval = B;

            /* Make A point to c */
            interval.left = i;
            if (!NULL_INTERVAL_P(i))
                i.ParentInterval = interval;

            /* A's total length is decreased by the length of B and its left child.  */
            interval.total_length -= B.total_length - LEFT_TOTAL_LENGTH(interval);
            CHECK_TOTAL_LENGTH(interval);

            /* B must have the same total length of A.  */
            B.total_length = old_total;
            CHECK_TOTAL_LENGTH(B);

            return B;
        }

        /* Assuming that a right child exists, perform the following operation:

            A               B
           / \	           / \
              B	   =>     A
             / \         / \
            c               c
        */
        public static Interval rotate_left(Interval interval)
        {
            Interval i;
            Interval B = interval.right;
            uint old_total = interval.total_length;

            /* Deal with any parent of A;  make it point to B.  */
            if (!ROOT_INTERVAL_P(interval))
            {
                if (AM_LEFT_CHILD(interval))
                    interval.ParentInterval.left = B;
                else
                    interval.ParentInterval.right = B;
            }
            COPY_INTERVAL_PARENT(B, interval);

            /* Make B the parent of A */
            i = B.left;
            B.left = interval;
            interval.ParentInterval = B;

            /* Make A point to c */
            interval.right = i;
            if (!NULL_INTERVAL_P(i))
                i.ParentInterval = interval;

            /* A's total length is decreased by the length of B and its right child.  */
            interval.total_length -= B.total_length - RIGHT_TOTAL_LENGTH(interval);
            CHECK_TOTAL_LENGTH(interval);

            /* B must have the same total length of A.  */
            B.total_length = old_total;
            CHECK_TOTAL_LENGTH(B);

            return B;
        }

        /* Balance an interval tree with the assumption that the subtrees
           themselves are already balanced.  */
        public static Interval balance_an_interval(Interval i)
        {
            int old_diff, new_diff;

            while (true)
            {
                old_diff = (int) LEFT_TOTAL_LENGTH(i) - (int) RIGHT_TOTAL_LENGTH(i);
                if (old_diff > 0)
                {
                    /* Since the left child is longer, there must be one.  */
                    new_diff = (int) i.total_length - (int) i.left.total_length + (int) RIGHT_TOTAL_LENGTH(i.left) - (int) LEFT_TOTAL_LENGTH(i.left);
                    if (System.Math.Abs(new_diff) >= old_diff)
                        break;
                    i = rotate_right(i);
                    balance_an_interval(i.right);
                }
                else if (old_diff < 0)
                {
                    /* Since the right child is longer, there must be one.  */
                    new_diff = (int) i.total_length - (int) i.right.total_length + (int) LEFT_TOTAL_LENGTH(i.right) - (int) RIGHT_TOTAL_LENGTH(i.right);
                    if (System.Math.Abs(new_diff) >= -old_diff)
                        break;
                    i = rotate_left(i);
                    balance_an_interval(i.left);
                }
                else
                    break;
            }
            return i;
        }

        /* Balance INTERVAL, potentially stuffing it back into its parent
           Lisp Object.  */
        public static Interval balance_possible_root_interval(Interval interval)
        {
            LispObject parent = null;
            bool have_parent = false;

            if (!interval.is_up_object && interval.ParentInterval == null)
                return interval;

            if (interval.is_up_object)
            {
                have_parent = true;
                parent = interval.ParentObject;
            }
            interval = balance_an_interval(interval);

            if (have_parent)
            {
                if (BUFFERP(parent))
                    XBUFFER(parent).text.intervals = interval;
                else if (STRINGP(parent))
                    XSTRING(parent).intervals = interval;
            }

            return interval;
        }

        /* Find the interval containing text position POSITION in the text
           represented by the interval tree TREE.  POSITION is a buffer
           position (starting from 1) or a string index (starting from 0).
           If POSITION is at the end of the buffer or string,
           return the interval containing the last character.

           The `position' field, which is a cache of an interval's position,
           is updated in the interval found.  Other functions (e.g., next_interval)
           will update this cache based on the result of find_interval.  */

        public static Interval find_interval(Interval tree, int position)
        {
            /* The distance from the left edge of the subtree at TREE
                              to POSITION.  */
            int relative_position;

            if (NULL_INTERVAL_P(tree))
                return null;

            relative_position = position;
            if (tree.is_up_object)
            {
                LispObject parent = tree.ParentObject;
                if (BUFFERP(parent))
                    relative_position -= BUF_BEG(XBUFFER(parent));
            }

            if (relative_position > TOTAL_LENGTH(tree))
                abort();			/* Paranoia */

            tree = balance_possible_root_interval(tree);

            while (true)
            {
                if (relative_position < LEFT_TOTAL_LENGTH(tree))
                {
                    tree = tree.left;
                }
                else if (!NULL_RIGHT_CHILD(tree)
                     && relative_position >= (TOTAL_LENGTH(tree)
                              - RIGHT_TOTAL_LENGTH(tree)))
                {
                    relative_position -= (int) (TOTAL_LENGTH(tree) - RIGHT_TOTAL_LENGTH(tree));
                    tree = tree.right;
                }
                else
                {
                    tree.position
                      = (uint) System.Math.Abs(position - relative_position /* left edge of *tree.  */
                         + (int) LEFT_TOTAL_LENGTH(tree)); /* left edge of this interval.  */

                    return tree;
                }
            }
        }

        /* Return 1 if the two intervals have the same properties,
           0 otherwise.  */
        public static bool intervals_equal(Interval i0, Interval i1)
        {
            LispObject i0_cdr, i0_sym;
            LispObject i1_cdr, i1_val;

            if (DEFAULT_INTERVAL_P(i0) && DEFAULT_INTERVAL_P(i1))
                return true;

            if (DEFAULT_INTERVAL_P(i0) || DEFAULT_INTERVAL_P(i1))
                return false;

            i0_cdr = i0.plist;
            i1_cdr = i1.plist;
            while (CONSP(i0_cdr) && CONSP(i1_cdr))
            {
                i0_sym = XCAR(i0_cdr);
                i0_cdr = XCDR(i0_cdr);
                if (!CONSP(i0_cdr))
                    return false;		/* abort (); */
                i1_val = i1.plist;
                while (CONSP(i1_val) && !EQ(XCAR(i1_val), i0_sym))
                {
                    i1_val = XCDR(i1_val);
                    if (!CONSP(i1_val))
                        return false;		/* abort (); */
                    i1_val = XCDR(i1_val);
                }

                /* i0 has something i1 doesn't.  */
                if (EQ(i1_val, Q.nil))
                    return false;

                /* i0 and i1 both have sym, but it has different values in each.  */
                if (!CONSP(i1_val))
                    return false;
                else
                {
                    i1_val = XCDR(i1_val);
                    if (!CONSP(i1_val) || !EQ(XCAR(i1_val), XCAR(i0_cdr)))
                        return false;
                }

                i0_cdr = XCDR(i0_cdr);

                i1_cdr = XCDR(i1_cdr);
                if (!CONSP(i1_cdr))
                    return false;		/* abort (); */
                i1_cdr = XCDR(i1_cdr);
            }

            /* Lengths of the two plists were equal.  */
            return (NILP(i0_cdr) && NILP(i1_cdr));
        }

        /* Find the succeeding interval (lexicographically) to INTERVAL.
           Sets the `position' field based on that of INTERVAL (see
           find_interval).  */
        public static Interval next_interval (Interval interval)
        {
            Interval i = interval;
            uint next_position;

            if (NULL_INTERVAL_P(i))
                return null;

            next_position = interval.position + LENGTH(interval);

            if (!NULL_RIGHT_CHILD(i))
            {
                i = i.right;
                while (!NULL_LEFT_CHILD(i))
                    i = i.left;

                i.position = next_position;
                return i;
            }

            while (!NULL_PARENT(i))
            {
                if (AM_LEFT_CHILD(i))
                {
                    i = i.ParentInterval;
                    i.position = next_position;
                    return i;
                }

                i = i.ParentInterval;
            }

            return null;
        }

        /* Return 1 if strings S1 and S2 have identical properties; 0 otherwise.
           Assume they have identical characters.  */
        public static bool compare_string_intervals(LispObject s1, LispObject s2)
        {
            Interval i1, i2;
            int pos = 0;
            int end = SCHARS(s1);

            i1 = find_interval(STRING_INTERVALS(s1), 0);
            i2 = find_interval(STRING_INTERVALS(s2), 0);

            while (pos < end)
            {
                /* Determine how far we can go before we reach the end of I1 or I2.  */
                int len1 = (i1 != null ? (int)INTERVAL_LAST_POS(i1) : end) - pos;
                int len2 = (i2 != null ? (int)INTERVAL_LAST_POS(i2) : end) - pos;
                int distance = System.Math.Min(len1, len2);

                /* If we ever find a mismatch between the strings,
               they differ.  */
                if (!intervals_equal(i1, i2))
                    return false;

                /* Advance POS till the end of the shorter interval,
               and advance one or both interval pointers for the new position.  */
                pos += distance;
                if (len1 == distance)
                    i1 = next_interval(i1);
                if (len2 == distance)
                    i2 = next_interval(i2);
            }
            return true;
        }
    }
}