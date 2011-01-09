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

        public bool write_protect;
        public bool visible;
        public bool front_sticky;
        public bool rear_sticky;

        public LispObject plist;

        public Interval(Interval i)
        {
            total_length = i.total_length;
            position = i.position;

            left = i.left;
            right = i.right;

            up_interval = i.up_interval;
            up_object = i.up_object;

            is_up_object = i.is_up_object;

            write_protect = i.write_protect;
            visible = i.visible;
            front_sticky = i.front_sticky;
            rear_sticky = i.rear_sticky;

            plist = i.plist;
        }

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

    public partial class Q
    {
        /* Types of hooks.  */
        public static LispObject mouse_left;
        public static LispObject mouse_entered;
        public static LispObject point_left;
        public static LispObject point_entered;
        public static LispObject category;
        public static LispObject local_map;

        /* Visual properties text (including strings) may have.  */
        public static LispObject foreground, background, font, underline, stipple;
        public static LispObject invisible, read_only, intangible, mouse_face;
        public static LispObject minibuffer_prompt;

        /* Sticky properties */
        public static LispObject front_sticky, rear_nonsticky;
    }

    public partial class L
    {
        public static Interval NULL_INTERVAL = null;
        public static Interval INTERVAL_DEFAULT = NULL_INTERVAL;

        /* Test for membership, allowing for t (actually any non-cons) to mean the
           universal set.  */
        public static bool TMEM(LispObject sym, LispObject set)
        {
            return (CONSP(set) ? !NILP(F.memq(sym, set)) : !NILP(set));
        }
        /* Create the root interval of some object, a buffer or string.  */
        public static Interval create_root_interval(LispObject parent)
        {
            Interval neww;

            neww = make_interval();

            if (BUFFERP(parent))
            {
                neww.total_length = (uint) (BUF_Z(XBUFFER(parent)) - BUF_BEG(XBUFFER(parent)));
                CHECK_TOTAL_LENGTH(neww);
                XBUFFER(parent).text.intervals = neww;
                neww.position = (uint) BEG;
            }
            else if (STRINGP(parent))
            {
                neww.total_length = (uint) SCHARS(parent);
                CHECK_TOTAL_LENGTH(neww);
                STRING_SET_INTERVALS(parent, neww);
                neww.position = 0;
            }

            neww.ParentObject = parent;
            return neww;
        }

        /* If PROP is the `invisible' property of a character,
           this is 1 if the character should be treated as invisible,
           and 2 if it is invisible but with an ellipsis.  */
        public static int TEXT_PROP_MEANS_INVISIBLE(LispObject prop)
        {
            if (EQ(current_buffer.invisibility_spec, Q.t))
            {
                return (!NILP(prop) ? 1 : 0);
            }
            else
            {
             return invisible_p(prop, current_buffer.invisibility_spec);
            }
        }

        /* Is this interval visible?  Replace later with cache access */
        public static bool INTERVAL_VISIBLE_P(Interval i)
        {
            return (!NULL_INTERVAL_P(i) && NILP(textget(i.plist, Q.invisible)));
        }

        /* Is this interval writable?  Replace later with cache access */
        public static bool INTERVAL_WRITABLE_P(Interval i)
        {
            return (!NULL_INTERVAL_P(i)
             && (NILP(textget(i.plist, Q.read_only))
                 || ((CONSP(V.inhibit_read_only)
                  ? !NILP(F.memq(textget(i.plist, Q.read_only),
                          V.inhibit_read_only))
                  : !NILP(V.inhibit_read_only)))));
        }

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

        /* True if this interval has no children. */
        public static bool LEAF_INTERVAL_P(Interval i)
        {
            return (i.left == NULL_INTERVAL && i.right == NULL_INTERVAL);
        }

        /* True if this interval has no parent and is therefore the root. */
        public static bool ROOT_INTERVAL_P(Interval i)
        {
            return NULL_PARENT(i);
        }

        /* True if this interval is the only interval in the interval tree. */
        public static bool ONLY_INTERVAL_P(Interval i)
        {
            return (ROOT_INTERVAL_P(i) && LEAF_INTERVAL_P(i));
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

        public static void GET_INTERVAL_OBJECT(ref LispObject d, Interval s)
        {
            //(eassert((s)->up_obj == 1),
            d = s.ParentObject;
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

        /* Find the preceding interval (lexicographically) to INTERVAL.
           Sets the `position' field based on that of INTERVAL (see
           find_interval).  */
        public static Interval previous_interval(Interval interval)
        {
            Interval i;

            if (NULL_INTERVAL_P(interval))
                return NULL_INTERVAL;

            if (!NULL_LEFT_CHILD(interval))
            {
                i = interval.left;
                while (!NULL_RIGHT_CHILD(i))
                    i = i.right;

                i.position = interval.position - LENGTH(i);
                return i;
            }

            i = interval;
            while (!NULL_PARENT(i))
            {
                if (AM_RIGHT_CHILD(i))
                {
                    i = i.ParentInterval;

                    i.position = interval.position - LENGTH(i);
                    return i;
                }
                i = i.ParentInterval;
            }

            return NULL_INTERVAL;
        }

        /* Find the interval containing POS given some non-NULL INTERVAL
           in the same tree.  Note that we need to update interval->position
           if we go down the tree.
           To speed up the process, we assume that the ->position of
           I and all its parents is already uptodate.  */
        public static Interval update_interval(Interval i, int pos)
        {
            if (NULL_INTERVAL_P(i))
                return NULL_INTERVAL;

            while (true)
            {
                if (pos < i.position)
                {
                    /* Move left. */
                    if (pos >= i.position - TOTAL_LENGTH(i.left))
                    {
                        i.left.position = i.position - TOTAL_LENGTH(i.left)
                      + LEFT_TOTAL_LENGTH(i.left);
                        i = i.left;		/* Move to the left child */
                    }
                    else if (NULL_PARENT(i))
                        error("Point before start of properties");
                    else
                        i = i.ParentInterval;
                    continue;
                }
                else if (pos >= INTERVAL_LAST_POS(i))
                {
                    /* Move right. */
                    if (pos < INTERVAL_LAST_POS(i) + TOTAL_LENGTH(i.right))
                    {
                        i.right.position = INTERVAL_LAST_POS(i)
                          + LEFT_TOTAL_LENGTH(i.right);
                        i = i.right;		/* Move to the right child */
                    }
                    else if (NULL_PARENT(i))
                        error("Point %d after end of properties", pos);
                    else
                        i = i.ParentInterval;
                    continue;
                }
                else
                    return i;
            }
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

        /* Set point in BUFFER to CHARPOS.  If the target position is
           before an intangible character, move to an ok place.  */

        public static void set_point(int charpos)
        {
            set_point_both(charpos, buf_charpos_to_bytepos(current_buffer, charpos));
        }

        /* If there's an invisible character at position POS + TEST_OFFS in the
           current buffer, and the invisible property has a `stickiness' such that
           inserting a character at position POS would inherit the property it,
           return POS + ADJ, otherwise return POS.  If TEST_INTANG is non-zero,
           then intangibility is required as well as invisibleness.

           TEST_OFFS should be either 0 or -1, and ADJ should be either 1 or -1.

           Note that `stickiness' is determined by overlay marker insertion types,
           if the invisible property comes from an overlay.  */
        public static int adjust_for_invis_intang(int pos, int test_offs, int adj, int test_intang)
        {
            LispObject invis_propval, invis_overlay = null;
            LispObject test_pos;

            if ((adj < 0 && pos + adj < BEGV()) || (adj > 0 && pos + adj > ZV))
                /* POS + ADJ would be beyond the buffer bounds, so do no adjustment.  */
                return pos;

            test_pos = make_number(pos + test_offs);

            invis_propval
              = get_char_property_and_overlay(test_pos, Q.invisible, Q.nil,
                               ref invis_overlay);

            if ((test_intang == 0
                 || !NILP(F.get_char_property(test_pos, Q.intangible, Q.nil)))
                && TEXT_PROP_MEANS_INVISIBLE(invis_propval) != 0
                /* This next test is true if the invisible property has a stickiness
               such that an insertion at POS would inherit it.  */
                && (NILP(invis_overlay)
                /* Invisible property is from a text-property.  */
                ? (text_property_stickiness(Q.invisible, make_number(pos), Q.nil)
                   == (test_offs == 0 ? 1 : -1))
                /* Invisible property is from an overlay.  */
                : (test_offs == 0
                   ? XMARKER(OVERLAY_START(invis_overlay)).insertion_type == false
                   : XMARKER(OVERLAY_END(invis_overlay)).insertion_type == true)))
                pos += adj;

            return pos;
        }

        /* Set point in BUFFER to CHARPOS, which corresponds to byte
           position BYTEPOS.  If the target position is
           before an intangible character, move to an ok place.  */

        public static void set_point_both(int charpos, int bytepos)
        {
            Interval to, from, toprev, fromprev;
            int buffer_point;
            int old_position = PT();
            /* This ensures that we move forward past intangible text when the
               initial position is the same as the destination, in the rare
               instances where this is important, e.g. in line-move-finish
               (simple.el).  */
            bool backwards = (charpos < old_position ? true : false);
            bool have_overlays;
            int original_position;

            current_buffer.point_before_scroll = Q.nil;

            if (charpos == PT())
                return;

            /* In a single-byte buffer, the two positions must be equal.  */
            //eassert (ZV != ZV_BYTE || charpos == bytepos);

            /* Check this now, before checking if the buffer has any intervals.
               That way, we can catch conditions which break this sanity check
               whether or not there are intervals in the buffer.  */
            //eassert (charpos <= ZV && charpos >= BEGV);

            have_overlays = (current_buffer.overlays_before != null || current_buffer.overlays_after != null);

            /* If we have no text properties and overlays,
               then we can do it quickly.  */
            if (NULL_INTERVAL_P(BUF_INTERVALS(current_buffer)) && !have_overlays)
            {
                temp_set_point_both(current_buffer, charpos, bytepos);
                return;
            }

            /* Set TO to the interval containing the char after CHARPOS,
               and TOPREV to the interval containing the char before CHARPOS.
               Either one may be null.  They may be equal.  */
            to = find_interval(BUF_INTERVALS(current_buffer), charpos);
            if (charpos == BEGV())
                toprev = null;
            else if (to != null && to.position == charpos)
                toprev = previous_interval(to);
            else
                toprev = to;

            buffer_point = (PT() == ZV ? ZV - 1 : PT());

            /* Set FROM to the interval containing the char after PT,
               and FROMPREV to the interval containing the char before PT.
               Either one may be null.  They may be equal.  */
            /* We could cache this and save time.  */
            from = find_interval(BUF_INTERVALS(current_buffer), buffer_point);
            if (buffer_point == BEGV())
                fromprev = null;
            else if (from != null && from.position == PT())
                fromprev = previous_interval(from);
            else if (buffer_point != PT())
            {
                fromprev = from;
                from = null;
            }
            else
                fromprev = from;

            /* Moving within an interval.  */
            if (to == from && toprev == fromprev && INTERVAL_VISIBLE_P(to)
                && !have_overlays)
            {
                temp_set_point_both(current_buffer, charpos, bytepos);
                return;
            }

            original_position = charpos;

            /* If the new position is between two intangible characters
               with the same intangible property value,
               move forward or backward until a change in that property.  */
            if (NILP(V.inhibit_point_motion_hooks)
                && ((!NULL_INTERVAL_P(to) && !NULL_INTERVAL_P(toprev))
                || have_overlays)
                /* Intangibility never stops us from positioning at the beginning
               or end of the buffer, so don't bother checking in that case.  */
                && charpos != BEGV() && charpos != ZV)
            {
                LispObject pos;
                LispObject intangible_propval;

                if (backwards)
                {
                    /* If the preceding character is both intangible and invisible,
                       and the invisible property is `rear-sticky', perturb it so
                       that the search starts one character earlier -- this ensures
                       that point can never move to the end of an invisible/
                       intangible/rear-sticky region.  */
                    charpos = adjust_for_invis_intang(charpos, -1, -1, 1);

                    pos = XSETINT(charpos);

                    /* If following char is intangible,
                       skip back over all chars with matching intangible property.  */

                    intangible_propval = F.get_char_property(pos, Q.intangible, Q.nil);

                    if (!NILP(intangible_propval))
                    {
                        while (XINT(pos) > BEGV()
                           && EQ(F.get_char_property(make_number(XINT(pos) - 1),
                                      Q.intangible, Q.nil),
                              intangible_propval))
                            pos = F.previous_char_property_change(pos, Q.nil);

                        /* Set CHARPOS from POS, and if the final intangible character
                       that we skipped over is also invisible, and the invisible
                       property is `front-sticky', perturb it to be one character
                       earlier -- this ensures that point can never move to the
                       beginning of an invisible/intangible/front-sticky region.  */
                        charpos = adjust_for_invis_intang(XINT(pos), 0, -1, 0);
                    }
                }
                else
                {
                    /* If the following character is both intangible and invisible,
                       and the invisible property is `front-sticky', perturb it so
                       that the search starts one character later -- this ensures
                       that point can never move to the beginning of an
                       invisible/intangible/front-sticky region.  */
                    charpos = adjust_for_invis_intang(charpos, 0, 1, 1);

                    pos = XSETINT(charpos);

                    /* If preceding char is intangible,
                       skip forward over all chars with matching intangible property.  */

                    intangible_propval = F.get_char_property(make_number(charpos - 1),
                                         Q.intangible, Q.nil);

                    if (!NILP(intangible_propval))
                    {
                        while (XINT(pos) < ZV
                           && EQ(F.get_char_property(pos, Q.intangible, Q.nil),
                              intangible_propval))
                            pos = F.next_char_property_change(pos, Q.nil);

                        /* Set CHARPOS from POS, and if the final intangible character
                       that we skipped over is also invisible, and the invisible
                       property is `rear-sticky', perturb it to be one character
                       later -- this ensures that point can never move to the
                       end of an invisible/intangible/rear-sticky region.  */
                        charpos = adjust_for_invis_intang(XINT(pos), -1, 1, 0);
                    }
                }

                bytepos = buf_charpos_to_bytepos(current_buffer, charpos);
            }

            if (charpos != original_position)
            {
                /* Set TO to the interval containing the char after CHARPOS,
               and TOPREV to the interval containing the char before CHARPOS.
               Either one may be null.  They may be equal.  */
                to = find_interval(BUF_INTERVALS(current_buffer), charpos);
                if (charpos == BEGV())
                    toprev = null;
                else if (to != null && to.position == charpos)
                    toprev = previous_interval(to);
                else
                    toprev = to;
            }

            /* Here TO is the interval after the stopping point
               and TOPREV is the interval before the stopping point.
               One or the other may be null.  */

            temp_set_point_both(current_buffer, charpos, bytepos);

            /* We run point-left and point-entered hooks here, if the
               two intervals are not equivalent.  These hooks take
               (old_point, new_point) as arguments.  */
            if (NILP(V.inhibit_point_motion_hooks)
                && (!intervals_equal(from, to)
                || !intervals_equal(fromprev, toprev)))
            {
                LispObject leave_after, leave_before, enter_after, enter_before;

                if (fromprev != null)
                    leave_before = textget(fromprev.plist, Q.point_left);
                else
                    leave_before = Q.nil;

                if (from != null)
                    leave_after = textget(from.plist, Q.point_left);
                else
                    leave_after = Q.nil;

                if (toprev != null)
                    enter_before = textget(toprev.plist, Q.point_entered);
                else
                    enter_before = Q.nil;

                if (to != null)
                    enter_after = textget(to.plist, Q.point_entered);
                else
                    enter_after = Q.nil;

                if (!EQ(leave_before, enter_before) && !NILP(leave_before))
                    call2(leave_before, make_number(old_position),
                           make_number(charpos));
                if (!EQ(leave_after, enter_after) && !NILP(leave_after))
                    call2(leave_after, make_number(old_position),
                           make_number(charpos));

                if (!EQ(enter_before, leave_before) && !NILP(enter_before))
                    call2(enter_before, make_number(old_position),
                           make_number(charpos));
                if (!EQ(enter_after, leave_after) && !NILP(enter_after))
                    call2(enter_after, make_number(old_position),
                           make_number(charpos));
            }
        }

        /* Get the value of property PROP from PLIST,
           which is the plist of an interval.
           We check for direct properties, for categories with property PROP,
           and for PROP appearing on the default-text-properties list.  */
        public static LispObject textget(LispObject plist, LispObject prop)
        {
            return lookup_char_property(plist, prop, true);
        }

        public static LispObject lookup_char_property(LispObject plist, LispObject prop, bool textprop)
        {
            LispObject tail, fallback = Q.nil;

            for (tail = plist; CONSP(tail); tail = F.cdr(XCDR(tail)))
            {
                LispObject tem;
                tem = XCAR(tail);
                if (EQ(prop, tem))
                    return F.car(XCDR(tail));
                if (EQ(tem, Q.category))
                {
                    tem = F.car(XCDR(tail));
                    if (SYMBOLP(tem))
                        fallback = F.get(tem, prop);
                }
            }

            if (!NILP(fallback))
                return fallback;
            /* Check for alternative properties */
            tail = F.assq(prop, V.char_property_alias_alist);
            if (!NILP(tail))
            {
                tail = XCDR(tail);
                for (; NILP(fallback) && CONSP(tail); tail = XCDR(tail))
                    fallback = F.plist_get(plist, XCAR(tail));
            }

            if (textprop && NILP(fallback) && CONSP(V.default_text_properties))
                fallback = F.plist_get(V.default_text_properties, prop);
            return fallback;
        }

        /* Set point "temporarily", without checking any text properties.  */
        public static void temp_set_point(Buffer buffer, int charpos)
        {
            temp_set_point_both(buffer, charpos,
                         buf_charpos_to_bytepos(buffer, charpos));
        }

        /* Set point in BUFFER "temporarily" to CHARPOS, which corresponds to
           byte position BYTEPOS.  */
        public static void temp_set_point_both(Buffer buffer, int charpos, int bytepos)
        {
            /* In a single-byte buffer, the two positions must be equal.  */
            if (BUF_ZV(buffer) == BUF_ZV_BYTE(buffer) && charpos != bytepos)
                abort();

            if (charpos > bytepos)
                abort();

            if (charpos > BUF_ZV(buffer) || charpos < BUF_BEGV(buffer))
                abort();

            buffer.pt_byte = bytepos;
            buffer.pt = charpos;
        }

        /* Find the interval in TREE corresponding to the relative position
           FROM and delete as much as possible of AMOUNT from that interval.
           Return the amount actually deleted, and if the interval was
           zeroed-out, delete that interval node from the tree.

           Note that FROM is actually origin zero, aka relative to the
           leftmost edge of tree.  This is appropriate since we call ourselves
           recursively on subtrees.

           Do this by recursing down TREE to the interval in question, and
           deleting the appropriate amount of text.  */
        public static int interval_deletion_adjustment(Interval tree, int from, int amount)
        {
            int relative_position = from;

            if (NULL_INTERVAL_P(tree))
                return 0;

            /* Left branch */
            if (relative_position < LEFT_TOTAL_LENGTH(tree))
            {
                int subtract = interval_deletion_adjustment(tree.left,
                                     relative_position,
                                     amount);
                tree.total_length -= (uint)subtract;
                CHECK_TOTAL_LENGTH(tree);
                return subtract;
            }
            /* Right branch */
            else if (relative_position >= (TOTAL_LENGTH(tree)
                           - RIGHT_TOTAL_LENGTH(tree)))
            {
                int subtract;

                relative_position -= (int) (tree.total_length - RIGHT_TOTAL_LENGTH(tree));
                subtract = interval_deletion_adjustment(tree.right,
                                     relative_position,
                                     amount);
                tree.total_length -= (uint) subtract;
                CHECK_TOTAL_LENGTH(tree);
                return subtract;
            }
            /* Here -- this node.  */
            else
            {
                /* How much can we delete from this interval?  */
                int my_amount = (((int)tree.total_length - (int)RIGHT_TOTAL_LENGTH(tree)) - relative_position);

                if (amount > my_amount)
                    amount = my_amount;

                tree.total_length -= (uint)amount;
                CHECK_TOTAL_LENGTH(tree);
                if (LENGTH(tree) == 0)
                    delete_interval(tree);

                return amount;
            }

            /* Never reach here.  */
        }

        /* Effect the adjustments necessary to the interval tree of BUFFER to
           correspond to the deletion of LENGTH characters from that buffer
           text.  The deletion is effected at position START (which is a
           buffer position, i.e. origin 1).  */
        public static void adjust_intervals_for_deletion(Buffer buffer, int start, int length)
        {
            int left_to_delete = length;
            Interval tree = BUF_INTERVALS(buffer);
            LispObject parent = null;
            int offset;

            GET_INTERVAL_OBJECT(ref parent, tree);
            offset = (BUFFERP(parent) ? BUF_BEG(XBUFFER(parent)) : 0);

            if (NULL_INTERVAL_P(tree))
                return;

            if (start > offset + TOTAL_LENGTH(tree)
                || start + length > offset + TOTAL_LENGTH(tree))
                abort();

            if (length == TOTAL_LENGTH(tree))
            {
                buffer.text.intervals = NULL_INTERVAL;
                return;
            }

            if (ONLY_INTERVAL_P(tree))
            {
                tree.total_length -= (uint) length;
                CHECK_TOTAL_LENGTH(tree);
                return;
            }

            if (start > offset + TOTAL_LENGTH(tree))
                start = offset + (int) TOTAL_LENGTH(tree);
            while (left_to_delete > 0)
            {
                left_to_delete -= interval_deletion_adjustment(tree, start - offset,
                                        left_to_delete);
                tree = BUF_INTERVALS(buffer);
                if (left_to_delete == tree.total_length)
                {
                    buffer.text.intervals = NULL_INTERVAL;
                    return;
                }
            }
        }

        /* Make the adjustments necessary to the interval tree of BUFFER to
           represent an addition or deletion of LENGTH characters starting
           at position START.  Addition or deletion is indicated by the sign
           of LENGTH.  */
        public static void offset_intervals (Buffer buffer, int start, int length)
        {
            if (NULL_INTERVAL_P(BUF_INTERVALS(buffer)) || length == 0)
                return;

            if (length > 0)
                adjust_intervals_for_insertion(BUF_INTERVALS(buffer), start, length);
            else
                adjust_intervals_for_deletion(buffer, start, -length);
        }

        /* Effect an adjustment corresponding to the addition of LENGTH characters
           of text.  Do this by finding the interval containing POSITION in the
           interval tree TREE, and then adjusting all of its ancestors by adding
           LENGTH to them.

           If POSITION is the first character of an interval, meaning that point
           is actually between the two intervals, make the new text belong to
           the interval which is "sticky".

           If both intervals are "sticky", then make them belong to the left-most
           interval.  Another possibility would be to create a new interval for
           this text, and make it have the merged properties of both ends.  */
        public static Interval adjust_intervals_for_insertion(Interval tree, int position, int length)
        {
            Interval i;
            Interval temp;
            int eobp = 0;
            LispObject parent = null;
            int offset;

            if (TOTAL_LENGTH(tree) == 0)	/* Paranoia */
                abort();

            GET_INTERVAL_OBJECT(ref parent, tree);
            offset = (BUFFERP(parent) ? BUF_BEG(XBUFFER(parent)) : 0);

            /* If inserting at point-max of a buffer, that position will be out
               of range.  Remember that buffer positions are 1-based.  */
            if (position >= TOTAL_LENGTH(tree) + offset)
            {
                position = (int) TOTAL_LENGTH(tree) + offset;
                eobp = 1;
            }

            i = find_interval(tree, position);

            /* If in middle of an interval which is not sticky either way,
               we must not just give its properties to the insertion.
               So split this interval at the insertion point.

               Originally, the if condition here was this:
              (! (position == i->position || eobp)
               && END_NONSTICKY_P (i)
               && FRONT_NONSTICKY_P (i))
               But, these macros are now unreliable because of introduction of
               Vtext_property_default_nonsticky.  So, we always check properties
               one by one if POSITION is in middle of an interval.  */
            if (!(position == i.position || eobp != 0))
            {
                LispObject tail;
                LispObject front, rear;

                tail = i.plist;

                /* Properties font-sticky and rear-nonsticky override
                   Vtext_property_default_nonsticky.  So, if they are t, we can
                   skip one by one checking of properties.  */
                rear = textget(i.plist, Q.rear_nonsticky);
                if (!CONSP(rear) && !NILP(rear))
                {
                    /* All properties are nonsticky.  We split the interval.  */
                    goto check_done;
                }
                front = textget(i.plist, Q.front_sticky);
                if (!CONSP(front) && !NILP(front))
                {
                    /* All properties are sticky.  We don't split the interval.  */
                    tail = Q.nil;
                    goto check_done;
                }

                /* Does any actual property pose an actual problem?  We break
                   the loop if we find a nonsticky property.  */
                for (; CONSP(tail); tail = F.cdr(XCDR(tail)))
                {
                    LispObject prop, tmp;
                    prop = XCAR(tail);

                    /* Is this particular property front-sticky?  */
                    if (CONSP(front) && !NILP(F.memq(prop, front)))
                        continue;

                    /* Is this particular property rear-nonsticky?  */
                    if (CONSP(rear) && !NILP(F.memq(prop, rear)))
                        break;

                    /* Is this particular property recorded as sticky or
                           nonsticky in Vtext_property_default_nonsticky?  */
                    tmp = F.assq(prop, V.text_property_default_nonsticky);
                    if (CONSP(tmp))
                    {
                        if (NILP(tmp))
                            continue;
                        break;
                    }

                    /* By default, a text property is rear-sticky, thus we
                       continue the loop.  */
                }

            check_done:
                /* If any property is a real problem, split the interval.  */
                if (!NILP(tail))
                {
                    temp = split_interval_right(i, position - (int) i.position);
                    copy_properties(i, temp);
                    i = temp;
                }
            }

            /* If we are positioned between intervals, check the stickiness of
               both of them.  We have to do this too, if we are at BEG or Z.  */
            if (position == i.position || eobp != 0)
            {
                Interval prev;

                if (position == BEG)
                    prev = null;
                else if (eobp != 0)
                {
                    prev = i;
                    i = null;
                }
                else
                    prev = previous_interval(i);

                /* Even if we are positioned between intervals, we default
               to the left one if it exists.  We extend it now and split
               off a part later, if stickiness demands it.  */
                for (temp = prev != null ? prev : i; temp != null; temp = temp.ParentInterval)
                {
                    temp.total_length += (uint) length;
                    CHECK_TOTAL_LENGTH(temp);
                    temp = balance_possible_root_interval(temp);
                }

                /* If at least one interval has sticky properties,
               we check the stickiness property by property.

               Originally, the if condition here was this:
                  (END_NONSTICKY_P (prev) || FRONT_STICKY_P (i))
               But, these macros are now unreliable because of introduction
               of Vtext_property_default_nonsticky.  So, we always have to
               check stickiness of properties one by one.  If cache of
               stickiness is implemented in the future, we may be able to
               use those macros again.  */
                if (true)
                {
                    LispObject pleft, pright;
                    Interval newi = new Interval();

                    pleft = NULL_INTERVAL_P(prev) ? Q.nil : prev.plist;
                    pright = NULL_INTERVAL_P(i) ? Q.nil : i.plist;
                    newi.plist = merge_properties_sticky(pleft, pright);

                    if (prev == null) /* i.e. position == BEG */
                    {
                        if (!intervals_equal(i, newi))
                        {
                            i = split_interval_left(i, length);
                            i.plist = newi.plist;
                        }
                    }
                    else if (!intervals_equal(prev, newi))
                    {
                        prev = split_interval_right(prev,
                                     position - (int) prev.position);
                        prev.plist = newi.plist;
                        if (!NULL_INTERVAL_P(i)
                        && intervals_equal(prev, i))
                            merge_interval_right(prev);
                    }

                    /* We will need to update the cache here later.  */
                }
/*                else if (prev == null && !NILP(i.plist))
                {
                    // Just split off a new interval at the left.
                    //   Since I wasn't front-sticky, the empty plist is ok.  
                    i = split_interval_left(i, length);
                }
*/
            }

            /* Otherwise just extend the interval.  */
            else
            {
                for (temp = i; temp != null; temp = temp.ParentInterval)
                {
                    temp.total_length += (uint) length;
                    CHECK_TOTAL_LENGTH(temp);
                    temp = balance_possible_root_interval(temp);
                }
            }

            return tree;
        }

        /* Split INTERVAL into two pieces, starting the second piece at
           character position OFFSET (counting from 0), relative to INTERVAL.
           INTERVAL becomes the left-hand piece, and the right-hand piece
           (second, lexicographically) is returned.

           The size and position fields of the two intervals are set based upon
           those of the original interval.  The property list of the new interval
           is reset, thus it is up to the caller to do the right thing with the
           result.

           Note that this does not change the position of INTERVAL;  if it is a root,
           it is still a root after this operation.  */
        public static Interval split_interval_right(Interval interval, int offset)
        {
            Interval neww = make_interval();
            uint position = interval.position;
            uint new_length = LENGTH(interval) - (uint)offset;

            neww.position = position + (uint) offset;
            neww.ParentInterval = interval;

            if (NULL_RIGHT_CHILD(interval))
            {
                interval.right = neww;
                neww.total_length = new_length;
                CHECK_TOTAL_LENGTH(neww);
            }
            else
            {
                /* Insert the new node between INTERVAL and its right child.  */
                neww.right = interval.right;
                interval.right.ParentInterval = neww;
                interval.right = neww;
                neww.total_length = new_length + neww.right.total_length;
                CHECK_TOTAL_LENGTH(neww);
                balance_an_interval(neww);
            }

            balance_possible_root_interval(interval);

            return neww;
        }

        /* Split INTERVAL into two pieces, starting the second piece at
           character position OFFSET (counting from 0), relative to INTERVAL.
           INTERVAL becomes the right-hand piece, and the left-hand piece
           (first, lexicographically) is returned.

           The size and position fields of the two intervals are set based upon
           those of the original interval.  The property list of the new interval
           is reset, thus it is up to the caller to do the right thing with the
           result.

           Note that this does not change the position of INTERVAL;  if it is a root,
           it is still a root after this operation.  */
        public static Interval split_interval_left(Interval interval, int offset)
        {
            Interval neww = make_interval();
            int new_length = offset;

            neww.position = interval.position;
            interval.position = interval.position + (uint) offset;
            neww.ParentInterval = interval;

            if (NULL_LEFT_CHILD(interval))
            {
                interval.left = neww;
                neww.total_length = (uint) new_length;
                CHECK_TOTAL_LENGTH(neww);
            }
            else
            {
                /* Insert the new node between INTERVAL and its left child.  */
                neww.left = interval.left;
                neww.left.ParentInterval = neww;
                interval.left = neww;
                neww.total_length = (uint) new_length + neww.left.total_length;
                CHECK_TOTAL_LENGTH(neww);
                balance_an_interval(neww);
            }

            balance_possible_root_interval(interval);

            return neww;
        }

        /* Copy the cached property values of interval FROM to interval TO. */
        public static void COPY_INTERVAL_CACHE(Interval from, Interval to)
        {
            to.write_protect = from.write_protect;
            to.visible = from.visible;
            to.front_sticky = from.front_sticky;
            to.rear_sticky = from.rear_sticky;
        }

        /* Make the interval TARGET have exactly the properties of SOURCE */
        public static void copy_properties (Interval source, Interval target)
        {
            if (DEFAULT_INTERVAL_P(source) && DEFAULT_INTERVAL_P(target))
                return;

            COPY_INTERVAL_CACHE(source, target);
            target.plist = F.copy_sequence(source.plist);
        }

        /* Merge interval I with its lexicographic successor. The resulting
           interval is returned, and has the properties of the original
           successor.  The properties of I are lost.  I is removed from the
           interval tree.

           IMPORTANT:
           The caller must verify that this is not the last (rightmost)
           interval.  */
        public static Interval merge_interval_right(Interval i)
        {
            uint absorb = LENGTH(i);
            Interval successor;

            /* Zero out this interval.  */
            i.total_length -= absorb;
            CHECK_TOTAL_LENGTH(i);

            /* Find the succeeding interval.  */
            if (!NULL_RIGHT_CHILD(i))      /* It's below us.  Add absorb
				      as we descend.  */
            {
                successor = i.right;
                while (!NULL_LEFT_CHILD(successor))
                {
                    successor.total_length += absorb;
                    CHECK_TOTAL_LENGTH(successor);
                    successor = successor.left;
                }

                successor.total_length += absorb;
                CHECK_TOTAL_LENGTH(successor);
                delete_interval(i);
                return successor;
            }

            successor = i;
            while (!NULL_PARENT(successor))	   /* It's above us.  Subtract as
					      we ascend.  */
            {
                if (AM_LEFT_CHILD(successor))
                {
                    successor = successor.ParentInterval;
                    delete_interval(i);
                    return successor;
                }

                successor = successor.ParentInterval;
                successor.total_length -= absorb;
                CHECK_TOTAL_LENGTH(successor);
            }

            /* This must be the rightmost or last interval and cannot
               be merged right.  The caller should have known.  */
            abort();
            return NULL_INTERVAL;
        }

        /* Any property might be front-sticky on the left, rear-sticky on the left,
           front-sticky on the right, or rear-sticky on the right; the 16 combinations
           can be arranged in a matrix with rows denoting the left conditions and
           columns denoting the right conditions:
              _  __  _
        _     FR FR FR FR
        FR__   0  1  2  3
         _FR   4  5  6  7
        FR     8  9  A  B
          FR   C  D  E  F

           left-props  = '(front-sticky (p8 p9 pa pb pc pd pe pf)
                   rear-nonsticky (p4 p5 p6 p7 p8 p9 pa pb)
                   p0 L p1 L p2 L p3 L p4 L p5 L p6 L p7 L
                   p8 L p9 L pa L pb L pc L pd L pe L pf L)
           right-props = '(front-sticky (p2 p3 p6 p7 pa pb pe pf)
                   rear-nonsticky (p1 p2 p5 p6 p9 pa pd pe)
                   p0 R p1 R p2 R p3 R p4 R p5 R p6 R p7 R
                   p8 R p9 R pa R pb R pc R pd R pe R pf R)

           We inherit from whoever has a sticky side facing us.  If both sides
           do (cases 2, 3, E, and F), then we inherit from whichever side has a
           non-nil value for the current property.  If both sides do, then we take
           from the left.

           When we inherit a property, we get its stickiness as well as its value.
           So, when we merge the above two lists, we expect to get this:

           result      = '(front-sticky (p6 p7 pa pb pc pd pe pf)
                   rear-nonsticky (p6 pa)
                   p0 L p1 L p2 L p3 L p6 R p7 R
                   pa R pb R pc L pd L pe L pf L)

           The optimizable special cases are:
               left rear-nonsticky = nil, right front-sticky = nil (inherit left)
               left rear-nonsticky = t,   right front-sticky = t   (inherit right)
               left rear-nonsticky = t,   right front-sticky = nil (inherit none)
        */
        public static LispObject merge_properties_sticky(LispObject pleft, LispObject pright)
        {
            LispObject props, front, rear;
            LispObject lfront, lrear, rfront, rrear;
            LispObject tail1, tail2, sym, lval, rval, cat;
            bool use_left, use_right;
            bool lpresent;

            props = Q.nil;
            front = Q.nil;
            rear = Q.nil;
            lfront = textget(pleft, Q.front_sticky);
            lrear = textget(pleft, Q.rear_nonsticky);
            rfront = textget(pright, Q.front_sticky);
            rrear = textget(pright, Q.rear_nonsticky);

            /* Go through each element of PRIGHT.  */
            for (tail1 = pright; CONSP(tail1); tail1 = F.cdr(XCDR(tail1)))
            {
                LispObject tmp;

                sym = XCAR(tail1);

                /* Sticky properties get special treatment.  */
                if (EQ(sym, Q.rear_nonsticky) || EQ(sym, Q.front_sticky))
                    continue;

                rval = F.car(XCDR(tail1));
                for (tail2 = pleft; CONSP(tail2); tail2 = F.cdr(XCDR(tail2)))
                    if (EQ(sym, XCAR(tail2)))
                        break;

                /* Indicate whether the property is explicitly defined on the left.
               (We know it is defined explicitly on the right
               because otherwise we don't get here.)  */
                lpresent = !NILP(tail2);
                lval = (NILP(tail2) ? Q.nil : F.car(F.cdr(tail2)));

                /* Even if lrear or rfront say nothing about the stickiness of
               SYM, Vtext_property_default_nonsticky may give default
               stickiness to SYM.  */
                tmp = F.assq(sym, V.text_property_default_nonsticky);
                use_left = (lpresent
                    && !(TMEM(sym, lrear)
                      || (CONSP(tmp) && !NILP(XCDR(tmp)))));
                use_right = (TMEM(sym, rfront)
                     || (CONSP(tmp) && NILP(XCDR(tmp))));
                if (use_left && use_right)
                {
                    if (NILP(lval))
                        use_left = false;
                    else if (NILP(rval))
                        use_right = false;
                }
                if (use_left)
                {
                    /* We build props as (value sym ...) rather than (sym value ...)
                       because we plan to nreverse it when we're done.  */
                    props = F.cons(lval, F.cons(sym, props));
                    if (TMEM(sym, lfront))
                        front = F.cons(sym, front);
                    if (TMEM(sym, lrear))
                        rear = F.cons(sym, rear);
                }
                else if (use_right)
                {
                    props = F.cons(rval, F.cons(sym, props));
                    if (TMEM(sym, rfront))
                        front = F.cons(sym, front);
                    if (TMEM(sym, rrear))
                        rear = F.cons(sym, rear);
                }
            }

            /* Now go through each element of PLEFT.  */
            for (tail2 = pleft; CONSP(tail2); tail2 = F.cdr(XCDR(tail2)))
            {
                LispObject tmp;

                sym = XCAR(tail2);

                /* Sticky properties get special treatment.  */
                if (EQ(sym, Q.rear_nonsticky) || EQ(sym, Q.front_sticky))
                    continue;

                /* If sym is in PRIGHT, we've already considered it.  */
                for (tail1 = pright; CONSP(tail1); tail1 = F.cdr(XCDR(tail1)))
                    if (EQ(sym, XCAR(tail1)))
                        break;
                if (!NILP(tail1))
                    continue;

                lval = F.car(XCDR(tail2));

                /* Even if lrear or rfront say nothing about the stickiness of
               SYM, Vtext_property_default_nonsticky may give default
               stickiness to SYM.  */
                tmp = F.assq(sym, V.text_property_default_nonsticky);

                /* Since rval is known to be nil in this loop, the test simplifies.  */
                if (!(TMEM(sym, lrear) || (CONSP(tmp) && !NILP(XCDR(tmp)))))
                {
                    props = F.cons(lval, F.cons(sym, props));
                    if (TMEM(sym, lfront))
                        front = F.cons(sym, front);
                }
                else if (TMEM(sym, rfront) || (CONSP(tmp) && NILP(XCDR(tmp))))
                {
                    /* The value is nil, but we still inherit the stickiness
                       from the right.  */
                    front = F.cons(sym, front);
                    if (TMEM(sym, rrear))
                        rear = F.cons(sym, rear);
                }
            }
            props = F.nreverse(props);
            if (!NILP(rear))
                props = F.cons(Q.rear_nonsticky, F.cons(F.nreverse(rear), props));

            cat = textget(props, Q.category);
            if (!NILP(front)
                &&
                /* If we have inherited a front-stick category property that is t,
               we don't need to set up a detailed one.  */
                !(!NILP(cat) && SYMBOLP(cat)
               && EQ(F.get(cat, Q.front_sticky), Q.t)))
                props = F.cons(Q.front_sticky, F.cons(F.nreverse(front), props));
            return props;
        }

        /* Delete a node I from its interval tree by merging its subtrees
           into one subtree which is then returned.  Caller is responsible for
           storing the resulting subtree into its parent.  */
        public static Interval delete_node(Interval i)
        {
            Interval migrate, thiss;
            uint migrate_amt;

            if (NULL_INTERVAL_P(i.left))
                return i.right;
            if (NULL_INTERVAL_P(i.right))
                return i.left;

            migrate = i.left;
            migrate_amt = i.left.total_length;
            thiss = i.right;
            thiss.total_length += migrate_amt;
            while (!NULL_INTERVAL_P(thiss.left))
            {
                thiss = thiss.left;
                thiss.total_length += migrate_amt;
            }
            CHECK_TOTAL_LENGTH(thiss);
            thiss.left = migrate;
            migrate.ParentInterval = thiss;

            return i.right;
        }

        /* Delete interval I from its tree by calling `delete_node'
           and properly connecting the resultant subtree.

           I is presumed to be empty; that is, no adjustments are made
           for the length of I.  */
        public static void delete_interval(Interval i)
        {
            Interval parent;
            uint amt = LENGTH(i);

            if (amt > 0)			/* Only used on zero-length intervals now.  */
                abort();

            if (ROOT_INTERVAL_P(i))
            {
                LispObject owner = null;
                GET_INTERVAL_OBJECT(ref owner, i);
                parent = delete_node(i);
                if (!NULL_INTERVAL_P(parent))
                    parent.ParentObject = owner;

                if (BUFFERP(owner))
                    XBUFFER(owner).text.intervals = parent;
                else if (STRINGP(owner))
                    STRING_SET_INTERVALS(owner, parent);
                else
                    abort();

                return;
            }

            parent = i.ParentInterval;
            if (AM_LEFT_CHILD(i))
            {
                parent.left = delete_node(i);
                if (!NULL_INTERVAL_P(parent.left))
                    parent.left.ParentInterval = parent;
            }
            else
            {
                parent.right = delete_node(i);
                if (!NULL_INTERVAL_P(parent.right))
                    parent.right.ParentInterval = parent;
            }
        }

        /* Produce an interval tree reflecting the intervals in
           TREE from START to START + LENGTH.
           The new interval tree has no parent and has a starting-position of 0.  */
        public static Interval copy_intervals(Interval tree, int start, int length)
        {
            Interval i, neww, t;
            int got, prevlen;

            if (NULL_INTERVAL_P(tree) || length <= 0)
                return NULL_INTERVAL;

            i = find_interval(tree, start);
            if (NULL_INTERVAL_P(i) || LENGTH(i) == 0)
                abort();

            /* If there is only one interval and it's the default, return nil.  */
            if ((start - i.position + 1 + length) < LENGTH(i)
                && DEFAULT_INTERVAL_P(i))
                return NULL_INTERVAL;

            neww = make_interval();
            neww.position = 0;
            got = (int) (LENGTH(i) - ((uint) start - i.position));
            neww.total_length = (uint) length;
            CHECK_TOTAL_LENGTH(neww);
            copy_properties(i, neww);

            t = neww;
            prevlen = got;
            while (got < length)
            {
                i = next_interval(i);
                t = split_interval_right(t, prevlen);
                copy_properties(i, t);
                prevlen = (int) LENGTH(i);
                got += prevlen;
            }

            return balance_an_interval(neww);
        }

        /* Give STRING the properties of BUFFER from POSITION to LENGTH.  */
        public static void copy_intervals_to_string(LispObject stringg, Buffer buffer, int position, int length)
        {
            Interval interval_copy = copy_intervals(BUF_INTERVALS(buffer),
                                 position, length);
            if (NULL_INTERVAL_P(interval_copy))
                return;

            interval_copy.ParentObject = stringg;
            STRING_SET_INTERVALS(stringg, interval_copy);
        }

        /* Move point to POSITION, unless POSITION is inside an intangible
           segment that reaches all the way to point.  */
        public static void move_if_not_intangible(int position)
        {
            LispObject pos;
            LispObject intangible_propval;

            pos = make_number(position);

            if (!NILP(V.inhibit_point_motion_hooks))
                /* If intangible is inhibited, always move point to POSITION.  */
                ;
            else if (PT() < position && XINT(pos) < ZV)
            {
                /* We want to move forward, so check the text before POSITION.  */

                intangible_propval = F.get_char_property(pos,
                                     Q.intangible, Q.nil);

                /* If following char is intangible,
               skip back over all chars with matching intangible property.  */
                if (!NILP(intangible_propval))
                    while (XINT(pos) > BEGV()
                           && EQ(F.get_char_property(make_number(XINT(pos) - 1),
                                      Q.intangible, Q.nil),
                              intangible_propval))
                        pos = F.previous_char_property_change(pos, Q.nil);
            }
            else if (XINT(pos) > BEGV())
            {
                /* We want to move backward, so check the text after POSITION.  */

                intangible_propval = F.get_char_property(make_number(XINT(pos) - 1),
                                     Q.intangible, Q.nil);

                /* If following char is intangible,
               skip forward over all chars with matching intangible property.  */
                if (!NILP(intangible_propval))
                    while (XINT(pos) < ZV
                           && EQ(F.get_char_property(pos, Q.intangible, Q.nil),
                              intangible_propval))
                        pos = F.next_char_property_change(pos, Q.nil);

            }
            else if (position < BEGV())
                position = BEGV();
            else if (position > ZV)
                position = ZV;

            /* If the whole stretch between PT and POSITION isn't intangible,
               try moving to POSITION (which means we actually move farther
               if POSITION is inside of intangible text).  */

            if (XINT(pos) != PT())
                SET_PT(position);
        }

        /* If text at position POS has property PROP, set *VAL to the property
           value, *START and *END to the beginning and end of a region that
           has the same property, and return 1.  Otherwise return 0.

           OBJECT is the string or buffer to look for the property in;
           nil means the current buffer. */
        public static bool get_property_and_range(int pos, LispObject prop, ref LispObject val, ref int start, ref int end, LispObject obj)
        {
            Interval i, prev, next;

            if (NILP(obj))
                i = find_interval(BUF_INTERVALS(current_buffer), pos);
            else if (BUFFERP(obj))
                i = find_interval(BUF_INTERVALS(XBUFFER(obj)), pos);
            else if (STRINGP(obj))
                i = find_interval(STRING_INTERVALS(obj), pos);
            else
            {
                abort();
                return false;
            }

            if (NULL_INTERVAL_P(i) || (i.position + LENGTH(i) <= pos))
                return false;
            val = textget(i.plist, prop);
            if (NILP(val))
                return false;

            next = i;			/* remember it in advance */
            prev = previous_interval(i);
            while (!NULL_INTERVAL_P(prev) && EQ(val, textget(prev.plist, prop)))
            {
                i = prev;
                prev = previous_interval(prev);
            }
            start = (int) i.position;

            next = next_interval(i);
            while (!NULL_INTERVAL_P(next) && EQ(val, textget(next.plist, prop)))
            {
                i = next;
                next = next_interval(next);
            }
            end = (int) (i.position + LENGTH(i));

            return true;
        }

        /* Insert the intervals of SOURCE into BUFFER at POSITION.
           LENGTH is the length of the text in SOURCE.

           The `position' field of the SOURCE intervals is assumed to be
           consistent with its parent; therefore, SOURCE must be an
           interval tree made with copy_interval or must be the whole
           tree of a buffer or a string.

           This is used in insdel.c when inserting Lisp_Strings into the
           buffer.  The text corresponding to SOURCE is already in the buffer
           when this is called.  The intervals of new tree are a copy of those
           belonging to the string being inserted; intervals are never
           shared.

           If the inserted text had no intervals associated, and we don't
           want to inherit the surrounding text's properties, this function
           simply returns -- offset_intervals should handle placing the
           text in the correct interval, depending on the sticky bits.

           If the inserted text had properties (intervals), then there are two
           cases -- either insertion happened in the middle of some interval,
           or between two intervals.

           If the text goes into the middle of an interval, then new
           intervals are created in the middle with only the properties of
           the new text, *unless* the macro MERGE_INSERTIONS is true, in
           which case the new text has the union of its properties and those
           of the text into which it was inserted.

           If the text goes between two intervals, then if neither interval
           had its appropriate sticky property set (front_sticky, rear_sticky),
           the new text has only its properties.  If one of the sticky properties
           is set, then the new text "sticks" to that region and its properties
           depend on merging as above.  If both the preceding and succeeding
           intervals to the new text are "sticky", then the new text retains
           only its properties, as if neither sticky property were set.  Perhaps
           we should consider merging all three sets of properties onto the new
           text...  */
        public static void graft_intervals_into_buffer(Interval source, int position, int length, Buffer buffer, int inherit)
        {
            Interval under, over, thiss, prev;
            Interval tree;
            int over_used;

            tree = BUF_INTERVALS(buffer);

            /* If the new text has no properties, then with inheritance it
               becomes part of whatever interval it was inserted into.
               To prevent inheritance, we must clear out the properties
               of the newly inserted text.  */
            if (NULL_INTERVAL_P(source))
            {
                LispObject buf;
                if (inherit == 0 && !NULL_INTERVAL_P(tree) && length > 0)
                {
                    buf = buffer;
                    set_text_properties_1(make_number(position),
                               make_number(position + length),
                               Q.nil, buf, null);
                }
                if (!NULL_INTERVAL_P(BUF_INTERVALS(buffer)))
                    /* Shouldn't be necessary.  -stef  */
                    buffer.text.intervals = balance_an_interval(BUF_INTERVALS(buffer));
                return;
            }

            if (NULL_INTERVAL_P(tree))
            {
                /* The inserted text constitutes the whole buffer, so
               simply copy over the interval structure.  */
                if ((BUF_Z(buffer) - BUF_BEG(buffer)) == TOTAL_LENGTH(source))
                {
                    LispObject buf;
                    buf = buffer;
                    buffer.text.intervals = reproduce_tree_obj(source, buf);
                    buffer.text.intervals.position = (uint)BEG;

                    buffer.text.intervals.is_up_object = true;

                    /* Explicitly free the old tree here?  */

                    return;
                }

                /* Create an interval tree in which to place a copy
               of the intervals of the inserted string.  */
                {
                    LispObject buf;
                    buf = buffer;
                    tree = create_root_interval(buf);
                }
            }
            else if (TOTAL_LENGTH(tree) == TOTAL_LENGTH(source))
            /* If the buffer contains only the new string, but
               there was already some interval tree there, then it may be
               some zero length intervals.  Eventually, do something clever
               about inserting properly.  For now, just waste the old intervals.  */
            {
                buffer.text.intervals = reproduce_tree(source, tree.ParentInterval);
                buffer.text.intervals.position = (uint) BEG;
                buffer.text.intervals.is_up_object = true;
                /* Explicitly free the old tree here.  */

                return;
            }
            /* Paranoia -- the text has already been added, so this buffer
               should be of non-zero length.  */
            else if (TOTAL_LENGTH(tree) == 0)
                abort();

            thiss = under = find_interval(tree, position);
            if (NULL_INTERVAL_P(under))	/* Paranoia */
                abort();
            over = find_interval(source, interval_start_pos(source));

            /* Here for insertion in the middle of an interval.
               Split off an equivalent interval to the right,
               then don't bother with it any more.  */

            if (position > under.position)
            {
                Interval end_unchanged = split_interval_left(thiss, position - (int) under.position);
                copy_properties(under, end_unchanged);
                under.position = (uint) position;
            }
            else
            {
                /* This call may have some effect because previous_interval may
                   update `position' fields of intervals.  Thus, don't ignore it
                   for the moment.  Someone please tell me the truth (K.Handa).  */
                prev = previous_interval(under);
            }

            /* Insertion is now at beginning of UNDER.  */

            /* The inserted text "sticks" to the interval `under',
               which means it gets those properties.
               The properties of under are the result of
               adjust_intervals_for_insertion, so stickiness has
               already been taken care of.  */

            /* OVER is the interval we are copying from next.
               OVER_USED says how many characters' worth of OVER
               have already been copied into target intervals.
               UNDER is the next interval in the target.  */
            over_used = 0;
            while (!NULL_INTERVAL_P(over))
            {
                /* If UNDER is longer than OVER, split it.  */
                if (LENGTH(over) - over_used < LENGTH(under))
                {
                    thiss = split_interval_left(under, (int) LENGTH(over) - over_used);
                    copy_properties(under, thiss);
                }
                else
                    thiss = under;

                /* THIS is now the interval to copy or merge into.
               OVER covers all of it.  */
                if (inherit != 0)
                    merge_properties(over, thiss);
                else
                    copy_properties(over, thiss);

                /* If THIS and OVER end at the same place,
               advance OVER to a new source interval.  */
                if (LENGTH(thiss) == LENGTH(over) - over_used)
                {
                    over = next_interval(over);
                    over_used = 0;
                }
                else
                    /* Otherwise just record that more of OVER has been used.  */
                    over_used += (int) LENGTH(thiss);

                /* Always advance to a new target interval.  */
                under = next_interval(thiss);
            }

            if (!NULL_INTERVAL_P(BUF_INTERVALS(buffer)))
                buffer.text.intervals = balance_an_interval(BUF_INTERVALS(buffer));
            return;
        }

        /* Make an exact copy of interval tree SOURCE which descends from
           PARENT.  This is done by recursing through SOURCE, copying
           the current interval and its properties, and then adjusting
           the pointers of the copy.  */
        public static Interval reproduce_tree (Interval source, Interval parent)
        {
            Interval t = new Interval(source);
            copy_properties(source, t);
            t.ParentInterval = parent;

            if (!NULL_LEFT_CHILD(source))
                t.left = reproduce_tree(source.left, t);
            if (!NULL_RIGHT_CHILD(source))
                t.right = reproduce_tree(source.right, t);

            return t;
        }

        public static Interval reproduce_tree_obj(Interval source, LispObject parent)
        {
            Interval t = new Interval(source);

            copy_properties(source, t);
            t.ParentObject = parent;
            if (!NULL_LEFT_CHILD(source))
                t.left = reproduce_tree(source.left, t);
            if (!NULL_RIGHT_CHILD(source))
                t.right = reproduce_tree(source.right, t);

            return t;
        }

        /* Return the proper position for the first character
           described by the interval tree SOURCE.
           This is 1 if the parent is a buffer,
           0 if the parent is a string or if there is no parent.

           Don't use this function on an interval which is the child
           of another interval!  */
        public static int interval_start_pos (Interval source)
        {
            LispObject parent;

            if (NULL_INTERVAL_P(source))
                return 0;

            if (! source.is_up_object)
                return 0;

            parent = source.ParentObject;

            if (BUFFERP(parent))
                return BUF_BEG(XBUFFER(parent));
            return 0;
        }

        /* Copy only the set bits of FROM's cache. */
        public static void MERGE_INTERVAL_CACHE(Interval from, Interval to)
        {
            if (from.write_protect) to.write_protect = true;
            if (from.visible) to.visible = true;
            if (from.front_sticky) to.front_sticky = true;
            if (from.rear_sticky) to.rear_sticky = true;
        }

        /* Merge the properties of interval SOURCE into the properties
           of interval TARGET.  That is to say, each property in SOURCE
           is added to TARGET if TARGET has no such property as yet.  */
        public static void merge_properties(Interval source, Interval target)
        {
            LispObject o, sym, val;

            if (DEFAULT_INTERVAL_P(source) && DEFAULT_INTERVAL_P(target))
                return;

            MERGE_INTERVAL_CACHE(source, target);

            o = source.plist;
            while (CONSP(o))
            {
                sym = XCAR(o);
                o = XCDR(o);
                CHECK_CONS(o);

                val = target.plist;
                while (CONSP(val) && !EQ(XCAR(val), sym))
                {
                    val = XCDR(val);
                    if (!CONSP(val))
                        break;
                    val = XCDR(val);
                }

                if (NILP(val))
                {
                    val = XCAR(o);
                    target.plist = F.cons(sym, F.cons(val, target.plist));
                }
                o = XCDR(o);
            }
        }

        /* Merge interval I with its lexicographic predecessor. The resulting
   interval is returned, and has the properties of the original predecessor.
   The properties of I are lost.  Interval node I is removed from the tree.

   IMPORTANT:
   The caller must verify that this is not the first (leftmost) interval.  */
        public static Interval merge_interval_left(Interval i)
        {
            uint absorb = LENGTH(i);
            Interval predecessor;

            /* Zero out this interval.  */
            i.total_length -= absorb;
            CHECK_TOTAL_LENGTH(i);

            /* Find the preceding interval.  */
            if (!NULL_LEFT_CHILD(i))	/* It's below us. Go down,
				   adding ABSORB as we go.  */
            {
                predecessor = i.left;
                while (!NULL_RIGHT_CHILD(predecessor))
                {
                    predecessor.total_length += absorb;
                    CHECK_TOTAL_LENGTH(predecessor);
                    predecessor = predecessor.right;
                }

                predecessor.total_length += absorb;
                CHECK_TOTAL_LENGTH(predecessor);
                delete_interval(i);
                return predecessor;
            }

            predecessor = i;
            while (!NULL_PARENT(predecessor))	/* It's above us.  Go up,
				   subtracting ABSORB.  */
            {
                if (AM_RIGHT_CHILD(predecessor))
                {
                    predecessor = predecessor.ParentInterval;
                    delete_interval(i);
                    return predecessor;
                }

                predecessor = predecessor.ParentInterval;
                predecessor.total_length -= absorb;
                CHECK_TOTAL_LENGTH(predecessor);
            }

            /* This must be the leftmost or first interval and cannot
               be merged left.  The caller should have known.  */
            abort();
            return null;
        }
    }
}