namespace IronElisp
{
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
    }

    public partial class L
    {
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
    }
}