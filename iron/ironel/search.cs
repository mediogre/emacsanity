using System.Text.RegularExpressions;
namespace IronElisp
{
    public partial class Q
    {
        /* error condition signaled when regexp compile_pattern fails */
        public static LispObject invalid_regexp;

        /* Error condition used for failing searches */
        public static LispObject search_failed;
    }

    public partial class V
    {
        public static LispObject search_spaces_regexp
        {
            get { return Defs.O[(int)Objects.search_spaces_regexp]; }
            set { Defs.O[(int)Objects.search_spaces_regexp] = value; }
        }

        /* If non-nil, the match data will not be changed during call to
           searching or matching functions.  This variable is for internal use
           only.  */
        public static LispObject inhibit_changing_match_data
        {
            get { return Defs.O[(int)Objects.inhibit_changing_match_data]; }
            set { Defs.O[(int)Objects.inhibit_changing_match_data] = value; }
        }
    }

    public partial class L
    {
        public const int REGEXP_CACHE_SIZE = 20;

        /* If the regexp is non-nil, then the buffer contains the compiled form
           of that regexp, suitable for searching.  */
        public class regexp_cache
        {
            // public regexp_cache next;
            public LispObject regexp, whitespace_regexp;
            /* Syntax table for which the regexp applies.  We need this because
               of character classes.  If this is t, then the compiled pattern is valid
               for any syntax-table.  */
            public LispObject syntax_table;
            public re_pattern_buffer buf = new re_pattern_buffer();
            public byte[] fastmap = new byte[256];
            /* Nonzero means regexp was compiled to do full POSIX backtracking.  */
            public bool posix;
        }

        /* The instances of that struct.  */
        public static System.Collections.Generic.LinkedList<regexp_cache> searchbufs = new System.Collections.Generic.LinkedList<regexp_cache>();

        /* The head of the linked list; points to the most recently used buffer.  */
        // public static regexp_cache searchbuf_head;

        /* Every call to re_match, etc., must pass &search_regs as the regs
           argument unless you can show it is unnecessary (i.e., if re_match
           is certainly going to be called again before region-around-match
           can be called).

           Since the registers are now dynamically allocated, we need to make
           sure not to refer to the Nth register before checking that it has
           been allocated by checking search_regs.num_regs.

           The regex code keeps track of whether it has allocated the search
           buffer using bits in the re_pattern_buffer.  This means that whenever
           you compile a new pattern, it completely forgets whether it has
           allocated any registers, and will allocate new registers the next
           time you call a searching or matching function.  Therefore, we need
           to call re_set_registers after compiling a new pattern or after
           setting the match registers, so that the regex functions will be
           able to free or re-allocate it properly.  */
        public static re_registers search_regs = new re_registers();

        /* The buffer in which the last search was performed, or
           Qt if the last search was done in a string;
           Qnil if no searching has been done yet.  */
        public static LispObject last_thing_searched;

        public static LispObject match_limit(LispObject num, bool beginningp)
        {
            int n;

            CHECK_NUMBER(num);
            n = XINT(num);
            if (n < 0)
                args_out_of_range(num, make_number(0));
            if (search_regs.num_regs <= 0)
                error("No match data, because no search succeeded");
            if (n >= search_regs.num_regs
                || search_regs.start[n] < 0)
                return Q.nil;
            return (make_number((beginningp) ? search_regs.start[n]
                                      : search_regs.end[n]));
        }

        public static LispObject unwind_set_match_data(LispObject list)
        {
            /* It is NOT ALWAYS safe to free (evaporate) the markers immediately.  */
            return F.set_match_data(list, Q.t);
        }

        /* Called to unwind protect the match data.  */
        public static void record_unwind_save_match_data()
        {
            record_unwind_protect(unwind_set_match_data, F.match_data(Q.nil, Q.nil, Q.nil));
        }

        /* Match REGEXP against STRING, searching all of STRING,
           and return the index of the match, or negative on failure.
           This does not clobber the match data.  */
        public static int fast_string_match(LispObject regexp, LispObject str)
        {
            int val;
            re_pattern_buffer bufp;

            bufp = compile_pattern(regexp, null, Q.nil,
                        false, STRING_MULTIBYTE(str));
            immediate_quit = 1;
            re_match_object = str;

            val = re_search(bufp, new PtrEmulator<byte>(SDATA(str)),
                     SBYTES(str), 0,
                     SBYTES(str), null);
            immediate_quit = 0;
            return val;
        }

        /* The newline cache: remembering which sections of text have no newlines.  */

        /* If the user has requested newline caching, make sure it's on.
           Otherwise, make sure it's off.
           This is our cheezy way of associating an action with the change of
           state of a buffer-local variable.  */
        public static void newline_cache_on_off(Buffer buf)
        {
            if (NILP(buf.cache_long_line_scans))
            {
                /* It should be off.  */
                if (buf.newline_cache != null)
                {
                    free_region_cache(buf.newline_cache);
                    buf.newline_cache = null;
                }
            }
            else
            {
                /* It should be on.  */
                if (buf.newline_cache == null)
                    buf.newline_cache = new_region_cache();
            }
        }


        /* Search for COUNT instances of the character TARGET between START and END.

           If COUNT is positive, search forwards; END must be >= START.
           If COUNT is negative, search backwards for the -COUNTth instance;
              END must be <= START.
           If COUNT is zero, do anything you please; run rogue, for all I care.

           If END is zero, use BEGV or ZV instead, as appropriate for the
           direction indicated by COUNT.

           If we find COUNT instances, set *SHORTAGE to zero, and return the
           position past the COUNTth match.  Note that for reverse motion
           this is not the same as the usual convention for Emacs motion commands.

           If we don't find COUNT instances before reaching END, set *SHORTAGE
           to the number of TARGETs left unfound, and return END.

           If ALLOW_QUIT is non-zero, set immediate_quit.  That's good to do
           except when inside redisplay.  */
        public static int scan_buffer(int target, int start, int end, int count, bool have_shortage, ref int shortage, bool allow_quit)
        {
            region_cache newline_cache;
            int direction;

            if (count > 0)
            {
                direction = 1;
                if (end == 0) end = ZV;
            }
            else
            {
                direction = -1;
                if (end == 0) end = BEGV();
            }

            newline_cache_on_off(current_buffer);
            newline_cache = current_buffer.newline_cache;

            if (have_shortage)
                shortage = 0;

            immediate_quit = allow_quit ? 1 : 0;

            if (count > 0)
                while (start != end)
                {
                    /* Our innermost scanning loop is very simple; it doesn't know
                       about gaps, buffer ends, or the newline cache.  ceiling is
                       the position of the last character before the next such
                       obstacle --- the last character the dumb search loop should
                       examine.  */
                    int ceiling_byte = CHAR_TO_BYTE(end) - 1;
                    int start_byte = CHAR_TO_BYTE(start);
                    int tem;

                    /* If we're looking for a newline, consult the newline cache
                       to see where we can avoid some scanning.  */
                    if (target == '\n' && newline_cache != null)
                    {
                        int next_change = 0;
                        immediate_quit = 0;
                        while (region_cache_forward
                               (current_buffer, newline_cache, start_byte, true, ref next_change) != 0)
                            start_byte = next_change;
                        immediate_quit = allow_quit ? 1 : 0;

                        /* START should never be after END.  */
                        if (start_byte > ceiling_byte)
                            start_byte = ceiling_byte;

                        /* Now the text after start is an unknown region, and
                           next_change is the position of the next known region. */
                        ceiling_byte = System.Math.Min(next_change - 1, ceiling_byte);
                    }

                    /* The dumb loop can only scan text stored in contiguous
                       bytes. BUFFER_CEILING_OF returns the last character
                       position that is contiguous, so the ceiling is the
                       position after that.  */
                    tem = BUFFER_CEILING_OF(start_byte);
                    ceiling_byte = System.Math.Min(tem, ceiling_byte);

                    {
                        /* The termination address of the dumb loop.  */
                        PtrEmulator<byte> ceiling_addr = new PtrEmulator<byte>(BYTE_POS_ADDR(ceiling_byte), 1);
                        PtrEmulator<byte> cursor = BYTE_POS_ADDR(start_byte);
                        PtrEmulator<byte> bbase = cursor;

                        while (cursor < ceiling_addr)
                        {
                            PtrEmulator<byte> scan_start = cursor;

                            /* The dumb loop.  */
                            while (cursor.Value != target && ++cursor < ceiling_addr)
                                ;

                            /* If we're looking for newlines, cache the fact that
                               the region from start to cursor is free of them. */
                            if (target == '\n' && newline_cache != null)
                                know_region_cache(current_buffer, newline_cache,
                                                   start_byte + (scan_start - bbase),
                                                   start_byte + (cursor - bbase));

                            /* Did we find the target character?  */
                            if (cursor < ceiling_addr)
                            {
                                if (--count == 0)
                                {
                                    immediate_quit = 0;
                                    return BYTE_TO_CHAR(start_byte + (cursor - bbase) + 1);
                                }
                                cursor++;
                            }
                        }

                        start = BYTE_TO_CHAR(start_byte + (cursor - bbase));
                    }
                }
            else
                while (start > end)
                {
                    /* The last character to check before the next obstacle.  */
                    int ceiling_byte = CHAR_TO_BYTE(end);
                    int start_byte = CHAR_TO_BYTE(start);
                    int tem;

                    /* Consult the newline cache, if appropriate.  */
                    if (target == '\n' && newline_cache != null)
                    {
                        int next_change = 0;
                        immediate_quit = 0;
                        while (region_cache_backward
                               (current_buffer, newline_cache, start_byte, true, ref next_change) != 0)
                            start_byte = next_change;
                        immediate_quit = allow_quit ? 1 : 0;

                        /* Start should never be at or before end.  */
                        if (start_byte <= ceiling_byte)
                            start_byte = ceiling_byte + 1;

                        /* Now the text before start is an unknown region, and
                           next_change is the position of the next known region. */
                        ceiling_byte = System.Math.Max(next_change, ceiling_byte);
                    }

                    /* Stop scanning before the gap.  */
                    tem = BUFFER_FLOOR_OF(start_byte - 1);
                    ceiling_byte = System.Math.Max(tem, ceiling_byte);

                    {
                        /* The termination address of the dumb loop.  */
                        PtrEmulator<byte> ceiling_addr = BYTE_POS_ADDR(ceiling_byte);
                        PtrEmulator<byte> cursor = BYTE_POS_ADDR(start_byte - 1);
                        PtrEmulator<byte> bbase = cursor;

                        while (cursor >= ceiling_addr)
                        {
                            PtrEmulator<byte> scan_start = cursor;

                            while (cursor.Value != target && --cursor >= ceiling_addr)
                                ;

                            /* If we're looking for newlines, cache the fact that
                               the region from after the cursor to start is free of them.  */
                            if (target == '\n' && newline_cache != null)
                                know_region_cache(current_buffer, newline_cache,
                                                   start_byte + (cursor - bbase),
                                                   start_byte + (scan_start - bbase));

                            /* Did we find the target character?  */
                            if (cursor >= ceiling_addr)
                            {
                                if (++count >= 0)
                                {
                                    immediate_quit = 0;
                                    return BYTE_TO_CHAR(start_byte + (cursor - bbase));
                                }
                                cursor--;
                            }
                        }

                        start = BYTE_TO_CHAR(start_byte + (cursor - bbase));
                    }
                }

            immediate_quit = 0;
            if (have_shortage)
                shortage = count * direction;
            return start;
        }

        /* Search for COUNT instances of a line boundary, which means either a
           newline or (if selective display enabled) a carriage return.
           Start at START.  If COUNT is negative, search backwards.

           We report the resulting position by calling TEMP_SET_PT_BOTH.

           If we find COUNT instances. we position after (always after,
           even if scanning backwards) the COUNTth match, and return 0.

           If we don't find COUNT instances before reaching the end of the
           buffer (or the beginning, if scanning backwards), we return
           the number of line boundaries left unfound, and position at
           the limit we bumped up against.

           If ALLOW_QUIT is non-zero, set immediate_quit.  That's good to do
           except in special cases.  */
        public static int scan_newline(int start, int start_byte, int limit, int limit_byte, int count, bool allow_quit)
        {
            int direction = ((count > 0) ? 1 : -1);

            PtrEmulator<byte> cursor;
            PtrEmulator<byte> bbase;

            int ceiling;
            PtrEmulator<byte> ceiling_addr;

            int old_immediate_quit = immediate_quit;

            /* The code that follows is like scan_buffer
               but checks for either newline or carriage return.  */

            if (allow_quit)
                immediate_quit++;

            start_byte = CHAR_TO_BYTE(start);

            if (count > 0)
            {
                while (start_byte < limit_byte)
                {
                    ceiling = BUFFER_CEILING_OF(start_byte);
                    ceiling = System.Math.Min(limit_byte - 1, ceiling);
                    ceiling_addr = BYTE_POS_ADDR(ceiling) + 1;
                    bbase = (cursor = BYTE_POS_ADDR(start_byte));
                    while (true)
                    {
                        while (cursor.Value != '\n' && ++cursor != ceiling_addr)
                        {
                        }

                        if (cursor != ceiling_addr)
                        {
                            if (--count == 0)
                            {
                                immediate_quit = old_immediate_quit;
                                start_byte = start_byte + (cursor - bbase) + 1;
                                start = BYTE_TO_CHAR(start_byte);
                                TEMP_SET_PT_BOTH(start, start_byte);
                                return 0;
                            }
                            else
                                if (++cursor == ceiling_addr)
                                    break;
                        }
                        else
                            break;
                    }
                    start_byte += cursor - bbase;
                }
            }
            else
            {
                while (start_byte > limit_byte)
                {
                    ceiling = BUFFER_FLOOR_OF(start_byte - 1);
                    ceiling = System.Math.Max(limit_byte, ceiling);
                    ceiling_addr = BYTE_POS_ADDR(ceiling) - 1;
                    bbase = (cursor = BYTE_POS_ADDR(start_byte - 1) + 1);
                    while (true)
                    {
                        while (--cursor != ceiling_addr && cursor.Value != '\n')
                        {
                        }

                        if (cursor != ceiling_addr)
                        {
                            if (++count == 0)
                            {
                                immediate_quit = old_immediate_quit;
                                /* Return the position AFTER the match we found.  */
                                start_byte = start_byte + (cursor - bbase) + 1;
                                start = BYTE_TO_CHAR(start_byte);
                                TEMP_SET_PT_BOTH(start, start_byte);
                                return 0;
                            }
                        }
                        else
                            break;
                    }
                    /* Here we add 1 to compensate for the last decrement
                       of CURSOR, which took it past the valid range.  */
                    start_byte += cursor - bbase + 1;
                }
            }

            TEMP_SET_PT_BOTH(limit, limit_byte);
            immediate_quit = old_immediate_quit;

            return count * direction;
        }

        /* Like find_next_newline, but returns position before the newline,
           not after, and only search up to TO.  This isn't just
           find_next_newline (...)-1, because you might hit TO.  */
        public static int find_before_next_newline(int from, int to, int cnt)
        {
            int shortage = 0;
            int pos = scan_buffer('\n', from, to, cnt, true, ref shortage, true);

            if (shortage == 0)
                pos--;

            return pos;
        }

        /* Match REGEXP atainst the characters after POS to LIMIT, and return
           the number of matched characters.  If STRING is non-nil, match
           against the characters in it.  In that case, POS and LIMIT are
           indices into the string.  This function doesn't modify the match
           data.  */
        public static int fast_looking_at(LispObject regexp, int pos, int pos_byte, int limit, int limit_byte, LispObject stringg)
        {
            bool multibyte;
            re_pattern_buffer buf;
            PtrEmulator<byte> p1, p2;
            int s1, s2;
            int len;

            if (STRINGP(stringg))
            {
                if (pos_byte < 0)
                    pos_byte = string_char_to_byte(stringg, pos);
                if (limit_byte < 0)
                    limit_byte = string_char_to_byte(stringg, limit);

                p1 = new PtrEmulator<byte>();
                s1 = 0;
                p2 = new PtrEmulator<byte>(SDATA(stringg));
                s2 = SBYTES(stringg);
                re_match_object = stringg;
                multibyte = STRING_MULTIBYTE(stringg);
            }
            else
            {
                if (pos_byte < 0)
                    pos_byte = CHAR_TO_BYTE(pos);
                if (limit_byte < 0)
                    limit_byte = CHAR_TO_BYTE(limit);
                pos_byte -= BEGV_BYTE();
                limit_byte -= BEGV_BYTE();
                p1 = BEGV_ADDR();
                s1 = GPT_BYTE - BEGV_BYTE();
                p2 = GAP_END_ADDR();
                s2 = ZV_BYTE - GPT_BYTE;
                if (s1 < 0)
                {
                    p2 = new PtrEmulator<byte>(p1);
                    s2 = ZV_BYTE - BEGV_BYTE();
                    s1 = 0;
                }
                if (s2 < 0)
                {
                    s1 = ZV_BYTE - BEGV_BYTE();
                    s2 = 0;
                }
                re_match_object = Q.nil;
                multibyte = !NILP(current_buffer.enable_multibyte_characters);
            }

            buf = compile_pattern(regexp, null, Q.nil, false, multibyte);
            immediate_quit = 1;
            len = re_match_2(buf, p1, s1, p2, s2,
                      pos_byte, null, limit_byte);
            immediate_quit = 0;

            return len;
        }

        /* Compile a regexp if necessary, but first check to see if there's one in
           the cache.
           PATTERN is the pattern to compile.
           TRANSLATE is a translation table for ignoring case, or nil for none.
           REGP is the structure that says where to store the "register"
           values that will result from matching this pattern.
           If it is 0, we should compile the pattern not to record any
           subexpression bounds.
           POSIX is nonzero if we want full backtracking (POSIX style)
           for this pattern.  0 means backtrack only enough to get a valid match.  */
        public static re_pattern_buffer compile_pattern(LispObject pattern, re_registers regp, LispObject translate, bool posix, bool multibyte)
        {
            System.Collections.Generic.LinkedListNode<regexp_cache> cpp;
            regexp_cache cp = null;

            cpp = searchbufs.First;
            for (int i = 0; i < searchbufs.Count; i++)
            {
                cp = cpp.Value;
                /* Entries are initialized to nil, and may be set to nil by
               compile_pattern_1 if the pattern isn't valid.  Don't apply
               string accessors in those cases.  However, compile_pattern_1
               is only applied to the cache entry we pick here to reuse.  So
               nil should never appear before a non-nil entry.  */
                if (NILP(cp.regexp))
                {
                    // goto compile_it;
                    compile_pattern_1(cp, pattern, translate, regp, posix);
                    break;
                }

                if (SCHARS(cp.regexp) == SCHARS(pattern) && STRING_MULTIBYTE(cp.regexp) == STRING_MULTIBYTE(pattern) &&
                    !NILP(F.string_equal(cp.regexp, pattern)) &&
                    EQ(cp.buf.translate, (!NILP(translate) ? translate : make_number(0))) &&
                    cp.posix == posix &&
                    (EQ(cp.syntax_table, Q.t) || EQ(cp.syntax_table, current_buffer.syntax_table)) &&
                    !NILP(F.equal(cp.whitespace_regexp, V.search_spaces_regexp)) &&
                    cp.buf.charset_unibyte == charset_unibyte)
                    break;

                /* If we're at the end of the cache, compile into the nil cell
               we found, or the last (least recently used) cell with a
               string value.  */
                if (cpp.Next == null)
                {
                    // compile_it:
                    compile_pattern_1(cp, pattern, translate, regp, posix);
                    break;
                }
            }

            /* When we get here, cp (aka *cpp) contains the compiled pattern,
               either because we found it in the cache or because we just compiled it.
               Move it to the front of the queue to mark it as most recently used.  */
            searchbufs.Remove(cpp);
            searchbufs.AddFirst(cpp);

            /* Advise the searching functions about the space we have allocated
               for register data.  */
            if (regp != null)
                re_set_registers(cp.buf, regp, regp.num_regs, regp.start, regp.end);

            /* The compiled pattern can be used both for mulitbyte and unibyte
               target.  But, we have to tell which the pattern is used for. */
            cp.buf.target_multibyte = multibyte;

            return cp.buf;
        }

        public static void matcher_overflow()
        {
            error("Stack overflow in regexp matcher");
        }

        public static LispObject looking_at_1(LispObject stringg, bool posix)
        {
            LispObject val;
            PtrEmulator<byte> p1, p2;
            int s1, s2;
            int i;
            re_pattern_buffer bufp;

            if (running_asynch_code)
                save_search_regs();

            /* This is so set_image_of_range_1 in regex.c can find the EQV table.  */
            XCHAR_TABLE(current_buffer.case_canon_table).set_extras(2, current_buffer.case_eqv_table);

            CHECK_STRING(stringg);
            bufp = compile_pattern(stringg,
                        (NILP(V.inhibit_changing_match_data)
                         ? search_regs : null),
                        (!NILP(current_buffer.case_fold_search)
                         ? current_buffer.case_canon_table : Q.nil),
                        posix,
                        !NILP(current_buffer.enable_multibyte_characters));

            immediate_quit = 1;
            QUIT();			/* Do a pending quit right away, to avoid paradoxical behavior */

            /* Get pointers and sizes of the two strings
               that make up the visible portion of the buffer. */

            p1 = BEGV_ADDR();
            s1 = GPT_BYTE - BEGV_BYTE();
            p2 = GAP_END_ADDR();
            s2 = ZV_BYTE - GPT_BYTE;
            if (s1 < 0)
            {
                p2 = p1;
                s2 = ZV_BYTE - BEGV_BYTE();
                s1 = 0;
            }
            if (s2 < 0)
            {
                s1 = ZV_BYTE - BEGV_BYTE();
                s2 = 0;
            }

            re_match_object = Q.nil;

            i = re_match_2(bufp, p1, s1, p2, s2,
                    PT_BYTE() - BEGV_BYTE(),
                    (NILP(V.inhibit_changing_match_data)
                     ? search_regs : null),
                    ZV_BYTE - BEGV_BYTE());
            immediate_quit = 0;

            if (i == -2)
                matcher_overflow();

            val = (0 <= i ? Q.t : Q.nil);
            if (NILP(V.inhibit_changing_match_data) && i >= 0)
                for (i = 0; i < search_regs.num_regs; i++)
                    if (search_regs.start[i] >= 0)
                    {
                        search_regs.start[i]
                          = BYTE_TO_CHAR(search_regs.start[i] + BEGV_BYTE());
                        search_regs.end[i]
                          = BYTE_TO_CHAR(search_regs.end[i] + BEGV_BYTE());
                    }

            /* Set last_thing_searched only when match data is changed.  */
            if (NILP(V.inhibit_changing_match_data))
                last_thing_searched = current_buffer;

            return val;
        }

        public static void syms_of_search()
        {
            for (int i = 0; i < REGEXP_CACHE_SIZE; i++)
            {
                regexp_cache cp = new regexp_cache();
                cp.buf.allocated = 100;
                cp.buf.buffer = new byte[100];
                cp.buf.fastmap = cp.fastmap;
                // cp.fastmap.CopyTo(cp.buf.fastmap, 0);
                cp.regexp = Q.nil;
                cp.whitespace_regexp = Q.nil;
                cp.syntax_table = Q.nil;

                searchbufs.AddLast(cp);
            }

            Q.search_failed = intern("search-failed");
            Q.invalid_regexp = intern("invalid-regexp");

            F.put(Q.search_failed, Q.error_conditions,
              F.cons(Q.search_failed, F.cons(Q.error, Q.nil)));
            F.put(Q.search_failed, Q.error_message,
              L.build_string("Search failed"));

            F.put(Q.invalid_regexp, Q.error_conditions,
              F.cons(Q.invalid_regexp, F.cons(Q.error, Q.nil)));
            F.put(Q.invalid_regexp, Q.error_message,
              L.build_string("Invalid regexp"));

            last_thing_searched = Q.nil;
            saved_last_thing_searched = Q.nil;

            defvar_lisp("search-spaces-regexp", Objects.search_spaces_regexp,
              @" Regexp to substitute for bunches of spaces in regexp search.
Some commands use this for user-specified regexps.
Spaces that occur inside character classes or repetition operators
or other such regexp constructs are not replaced with this.
A value of nil (which is the normal value) means treat spaces literally.");
            V.search_spaces_regexp = Q.nil;

            defvar_lisp("inhibit-changing-match-data", Objects.inhibit_changing_match_data,
                @" Internal use only.
If non-nil, the primitive searching and matching functions
such as `looking-at', `string-match', `re-search-forward', etc.,
do not set the match data.  The proper way to use this variable
is to bind it with `let' around a small expression.");
            V.inhibit_changing_match_data = Q.nil;

            defsubr("looking-at", F.looking_at, 1, 1, null,
       @" Return t if text after point matches regular expression REGEXP.
This function modifies the match data that `match-beginning',
`match-end' and `match-data' access; save and restore the match
data if you want to preserve them.");

            defsubr("posix-looking-at", F.posix_looking_at, 1, 1, null,
        @" Return t if text after point matches regular expression REGEXP.
Find the longest match, in accord with Posix regular expression rules.
This function modifies the match data that `match-beginning',
`match-end' and `match-data' access; save and restore the match
data if you want to preserve them.");

            defsubr("string-match", F.string_match, 2, 3, null,
       @" Return index of start of first match for REGEXP in STRING, or nil.
Matching ignores case if `case-fold-search' is non-nil.
If third arg START is non-nil, start search at that index in STRING.
For index of first char beyond the match, do (match-end 0).
`match-end' and `match-beginning' also give indices of substrings
matched by parenthesis constructs in the pattern.

You can use the function `match-string' to extract the substrings
matched by the parenthesis constructions in REGEXP.");

            defsubr("posix-string-match", F.posix_string_match, 2, 3, null,
       @" Return index of start of first match for REGEXP in STRING, or nil.
Find the longest match, in accord with Posix regular expression rules.
Case is ignored if `case-fold-search' is non-nil in the current buffer.
If third arg START is non-nil, start search at that index in STRING.
For index of first char beyond the match, do (match-end 0).
`match-end' and `match-beginning' also give indices of substrings
matched by parenthesis constructs in the pattern.");

            defsubr("search-forward", F.search_forward, 1, 4, "MSearch: ",
       @" Search forward from point for STRING.
Set point to the end of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend after that position.  A value of nil is
  equivalent to (point-max).
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.

Search case-sensitivity is determined by the value of the variable
`case-fold-search', which see.

See also the functions `match-beginning', `match-end' and `replace-match'.");

            defsubr("search-backward", F.search_backward, 1, 4, "MSearch backward: ",
         @" Search backward from point for STRING.
Set point to the beginning of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend before that position.
Optional third argument, if t, means if fail just return nil (no error).
 If not nil and not t, position at limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.

Search case-sensitivity is determined by the value of the variable
`case-fold-search', which see.

See also the functions `match-beginning', `match-end' and `replace-match'.");

            defsubr("word-search-forward", F.word_search_forward, 1, 4, "sWord search: ",
       @" Search forward from point for STRING, ignoring differences in punctuation.
Set point to the end of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.");

            defsubr("word-search-backward", F.word_search_backward, 1, 4, "sWord search backward: ",
       @" Search backward from point for STRING, ignoring differences in punctuation.
Set point to the beginning of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend before that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.");

            defsubr("word-search-forward-lax", F.word_search_forward_lax, 1, 4, "sWord search: ",
       @" Search forward from point for STRING, ignoring differences in punctuation.
Set point to the end of the occurrence found, and return point.

Unlike `word-search-forward', the end of STRING need not match a word
boundary unless it ends in whitespace.

An optional second argument bounds the search; it is a buffer position.
The match found must not extend after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.");

            defsubr("word-search-backward-lax", F.word_search_backward_lax, 1, 4, "sWord search backward: ",
       @" Search backward from point for STRING, ignoring differences in punctuation.
Set point to the beginning of the occurrence found, and return point.

Unlike `word-search-backward', the end of STRING need not match a word
boundary unless it ends in whitespace.

An optional second argument bounds the search; it is a buffer position.
The match found must not extend before that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.");

            defsubr("re-search-forward", F.re_search_forward, 1, 4, "sRE search: ",
                   @" Search forward from point for regular expression REGEXP.
Set point to the end of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.
See also the functions `match-beginning', `match-end', `match-string',
and `replace-match'.");

            defsubr("re-search-backward", F.re_search_backward, 1, 4, "sRE search backward: ",
       @" Search backward from point for match for regular expression REGEXP.
Set point to the beginning of the match, and return point.
The match found is the one starting last in the buffer
and yet ending before the origin of the search.
An optional second argument bounds the search; it is a buffer position.
The match found must start at or after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.
See also the functions `match-beginning', `match-end', `match-string',
and `replace-match'.");

            defsubr("posix-search-forward", F.posix_search_forward, 1, 4, "sPosix search: ",
       @" Search forward from point for regular expression REGEXP.
Find the longest match in accord with Posix regular expression rules.
Set point to the end of the occurrence found, and return point.
An optional second argument bounds the search; it is a buffer position.
The match found must not extend after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.
See also the functions `match-beginning', `match-end', `match-string',
and `replace-match'.");

            defsubr("posix-search-backward", F.posix_search_backward, 1, 4, "sPosix search backward: ",
       @" Search backward from point for match for regular expression REGEXP.
Find the longest match in accord with Posix regular expression rules.
Set point to the beginning of the match, and return point.
The match found is the one starting last in the buffer
and yet ending before the origin of the search.
An optional second argument bounds the search; it is a buffer position.
The match found must start at or after that position.
Optional third argument, if t, means if fail just return nil (no error).
  If not nil and not t, move to limit of search and return nil.
Optional fourth argument is repeat count--search for successive occurrences.
See also the functions `match-beginning', `match-end', `match-string',
and `replace-match'.");

            defsubr("replace-match", F.replace_match, 1, 5, null,
       @" Replace text matched by last search with NEWTEXT.
Leave point at the end of the replacement text.

If second arg FIXEDCASE is non-nil, do not alter case of replacement text.
Otherwise maybe capitalize the whole text, or maybe just word initials,
based on the replaced text.
If the replaced text has only capital letters
and has at least one multiletter word, convert NEWTEXT to all caps.
Otherwise if all words are capitalized in the replaced text,
capitalize each word in NEWTEXT.

If third arg LITERAL is non-nil, insert NEWTEXT literally.
Otherwise treat `\\' as special:
  `\\&' in NEWTEXT means substitute original matched text.
  `\\N' means substitute what matched the Nth `\\(...\\)'.
       If Nth parens didn't match, substitute nothing.
  `\\\\' means insert one `\\'.
Case conversion does not apply to these substitutions.

FIXEDCASE and LITERAL are optional arguments.

The optional fourth argument STRING can be a string to modify.
This is meaningful when the previous match was done against STRING,
using `string-match'.  When used this way, `replace-match'
creates and returns a new string made by copying STRING and replacing
the part of STRING that was matched.

The optional fifth argument SUBEXP specifies a subexpression;
it says to replace just that subexpression with NEWTEXT,
rather than replacing the entire matched text.
This is, in a vague sense, the inverse of using `\\N' in NEWTEXT;
`\\N' copies subexp N into NEWTEXT, but using N as SUBEXP puts
NEWTEXT in place of subexp N.
This is useful only after a regular expression search or match,
since only regular expressions have distinguished subexpressions.");

            defsubr("match-beginning", F.match_beginning, 1, 1, null,
       @" Return position of start of text matched by last search.
SUBEXP, a number, specifies which parenthesized expression in the last
  regexp.
Value is nil if SUBEXPth pair didn't match, or there were less than
  SUBEXP pairs.
Zero means the entire text matched by the whole regexp or whole string.");

            defsubr("match-end", F.match_end, 1, 1, null,
       @" Return position of end of text matched by last search.
SUBEXP, a number, specifies which parenthesized expression in the last
  regexp.
Value is nil if SUBEXPth pair didn't match, or there were less than
  SUBEXP pairs.
Zero means the entire text matched by the whole regexp or whole string.");

            defsubr("match-data", F.match_data, 0, 3, null,
       @" Return a list containing all info on what the last search matched.
Element 2N is `(match-beginning N)'; element 2N + 1 is `(match-end N)'.
All the elements are markers or nil (nil if the Nth pair didn't match)
if the last match was on a buffer; integers or nil if a string was matched.
Use `store-match-data' to reinstate the data in this list.

If INTEGERS (the optional first argument) is non-nil, always use
integers \(rather than markers) to represent buffer positions.  In
this case, and if the last match was in a buffer, the buffer will get
stored as one additional element at the end of the list.

If REUSE is a list, reuse it as part of the value.  If REUSE is long
enough to hold all the values, and if INTEGERS is non-nil, no consing
is done.

If optional third arg RESEAT is non-nil, any previous markers on the
REUSE list will be modified to point to nowhere.

Return value is undefined if the last search failed.");

            /* We used to have an internal use variant of `reseat' described as:

                  If RESEAT is `evaporate', put the markers back on the free list
                  immediately.  No other references to the markers must exist in this
                  case, so it is used only internally on the unwind stack and
                  save-match-data from Lisp.

               But it was ill-conceived: those supposedly-internal markers get exposed via
               the undo-list, so freeing them here is unsafe.  */
            defsubr("set-match-data", F.set_match_data, 1, 2, null,
       @" Set internal data on last search match from elements of LIST.
LIST should have been created by calling `match-data' previously.

If optional arg RESEAT is non-nil, make markers on LIST point nowhere.");

            defsubr("regexp-quote", F.regexp_quote, 1, 1, null,
       @" Return a regexp string which matches exactly STRING and nothing else.");
        }

        /* Compile a regexp and signal a Lisp error if anything goes wrong.
           PATTERN is the pattern to compile.
           CP is the place to put the result.
           TRANSLATE is a translation table for ignoring case, or nil for none.
           REGP is the structure that says where to store the "register"
           values that will result from matching this pattern.
           If it is 0, we should compile the pattern not to record any
           subexpression bounds.
           POSIX is nonzero if we want full backtracking (POSIX style)
           for this pattern.  0 means backtrack only enough to get a valid match.

           The behavior also depends on Vsearch_spaces_regexp.  */
        public static void compile_pattern_1(regexp_cache cp, LispObject pattern, LispObject translate, re_registers regp, bool posix)
        {
            string val;
            ulong old;

            cp.regexp = Q.nil;
            cp.buf.translate = (!NILP(translate) ? translate : make_number(0));
            cp.posix = posix;
            cp.buf.multibyte = STRING_MULTIBYTE(pattern);
            cp.buf.charset_unibyte = charset_unibyte;
            if (STRINGP(V.search_spaces_regexp))
                cp.whitespace_regexp = V.search_spaces_regexp;
            else
                cp.whitespace_regexp = Q.nil;

            /* rms: I think BLOCK_INPUT is not needed here any more,
               because regex.c defines malloc to call xmalloc.
               Using BLOCK_INPUT here means the debugger won't run if an error occurs.
               So let's turn it off.  */
            /*  BLOCK_INPUT;  */
            old = re_set_syntax(RE_SYNTAX_EMACS
                         | (posix ? 0 : RE_NO_POSIX_BACKTRACKING));

            if (STRINGP(V.search_spaces_regexp))
                re_set_whitespace_regexp(SDATA(V.search_spaces_regexp));
            else
                re_set_whitespace_regexp(null);

            val = re_compile_pattern(SDATA(pattern), SBYTES(pattern), ref cp.buf);

            /* If the compiled pattern hard codes some of the contents of the
               syntax-table, it can only be reused with *this* syntax table.  */
            cp.syntax_table = cp.buf.used_syntax ? current_buffer.syntax_table : Q.t;

            re_set_whitespace_regexp(null);

            re_set_syntax(old);
            /* UNBLOCK_INPUT;  */
            if (val != null)
                xsignal1(Q.invalid_regexp, build_string(val));

            cp.regexp = F.copy_sequence(pattern);
        }

        /* If non-zero the match data have been saved in saved_search_regs
           during the execution of a sentinel or filter. */
        public static bool search_regs_saved;
        public static re_registers saved_search_regs;
        public static LispObject saved_last_thing_searched;

        /* Called from Flooking_at, Fstring_match, search_buffer, Fstore_match_data
           if asynchronous code (filter or sentinel) is running. */
        public static void save_search_regs()
        {
            if (!search_regs_saved)
            {
                saved_search_regs.num_regs = search_regs.num_regs;
                saved_search_regs.start = search_regs.start;
                saved_search_regs.end = search_regs.end;
                saved_last_thing_searched = last_thing_searched;
                last_thing_searched = Q.nil;
                search_regs.num_regs = 0;
                search_regs.start = null;
                search_regs.end = null;

                search_regs_saved = true;
            }
        }

        /* Called upon exit from filters and sentinels. */
        public static void restore_search_regs()
        {
            if (search_regs_saved)
            {
                if (search_regs.num_regs > 0)
                {
                    // xfree(search_regs.start);
                    // xfree(search_regs.end);
                }
                search_regs.num_regs = saved_search_regs.num_regs;
                search_regs.start = saved_search_regs.start;
                search_regs.end = saved_search_regs.end;
                last_thing_searched = saved_last_thing_searched;
                saved_last_thing_searched = Q.nil;
                search_regs_saved = false;
            }
        }

        public static LispObject string_match_1(LispObject regexp, LispObject stringg, LispObject start, bool posix)
        {
            int val;
            re_pattern_buffer bufp;
            int pos, pos_byte;
            int i;

            if (running_asynch_code)
                save_search_regs();

            CHECK_STRING(regexp);
            CHECK_STRING(stringg);

            if (NILP(start))
            {
                pos = 0;
                pos_byte = 0;
            }
            else
            {
                int len = SCHARS(stringg);

                CHECK_NUMBER(start);
                pos = XINT(start);
                if (pos < 0 && -pos <= len)
                    pos = len + pos;
                else if (0 > pos || pos > len)
                    args_out_of_range(stringg, start);
                pos_byte = string_char_to_byte(stringg, pos);
            }

            /* This is so set_image_of_range_1 in regex.c can find the EQV table.  */
            XCHAR_TABLE(current_buffer.case_canon_table).set_extras(2, current_buffer.case_eqv_table);

            bufp = compile_pattern(regexp,
                        (NILP(V.inhibit_changing_match_data)
                         ? search_regs : null),
                        (!NILP(current_buffer.case_fold_search)
                         ? current_buffer.case_canon_table : Q.nil),
                        posix,
                        STRING_MULTIBYTE(stringg));
            immediate_quit = 1;
            re_match_object = stringg;

            val = re_search(bufp, new PtrEmulator<byte>(SDATA(stringg)),
                     SBYTES(stringg), pos_byte,
                     SBYTES(stringg) - pos_byte,
                     (NILP(V.inhibit_changing_match_data)
                      ? search_regs : null));
            immediate_quit = 0;

            /* Set last_thing_searched only when match data is changed.  */
            if (NILP(V.inhibit_changing_match_data))
                last_thing_searched = Q.t;

            if (val == -2)
                matcher_overflow();
            if (val < 0) return Q.nil;

            if (NILP(V.inhibit_changing_match_data))
                for (i = 0; i < search_regs.num_regs; i++)
                    if (search_regs.start[i] >= 0)
                    {
                        search_regs.start[i]
                          = string_byte_to_char(stringg, search_regs.start[i]);
                        search_regs.end[i]
                          = string_byte_to_char(stringg, search_regs.end[i]);
                    }

            return make_number(string_byte_to_char(stringg, val));
        }

        /* Subroutines of Lisp buffer search functions. */
        public static LispObject search_command(LispObject stringg, LispObject bound, LispObject noerror, LispObject count, int direction, bool RE, bool posix)
        {
            int np;
            int lim, lim_byte;
            int n = direction;

            if (!NILP(count))
            {
                CHECK_NUMBER(count);
                n *= XINT(count);
            }

            CHECK_STRING(stringg);
            if (NILP(bound))
            {
                if (n > 0)
                {
                    lim = ZV; lim_byte = ZV_BYTE;
                }
                else
                {
                    lim = BEGV(); lim_byte = BEGV_BYTE();
                }
            }
            else
            {
                CHECK_NUMBER_COERCE_MARKER(ref bound);
                lim = XINT(bound);
                if (n > 0 ? lim < PT() : lim > PT())
                    error("Invalid search bound (wrong side of point)");
                if (lim > ZV)
                {
                    lim = ZV; lim_byte = ZV_BYTE;
                }
                else if (lim < BEGV())
                {
                    lim = BEGV(); lim_byte = BEGV_BYTE();
                }
                else
                    lim_byte = CHAR_TO_BYTE(lim);
            }

            /* This is so set_image_of_range_1 in regex.c can find the EQV table.  */
            XCHAR_TABLE(current_buffer.case_canon_table).set_extras(2, current_buffer.case_eqv_table);

            np = search_buffer(stringg, PT(), PT_BYTE(), lim, lim_byte, n, RE,
                        (!NILP(current_buffer.case_fold_search)
                         ? current_buffer.case_canon_table
                         : Q.nil),
                        (!NILP(current_buffer.case_fold_search)
                         ? current_buffer.case_eqv_table
                         : Q.nil),
                        posix);
            if (np <= 0)
            {
                if (NILP(noerror))
                    xsignal1(Q.search_failed, stringg);

                if (!EQ(noerror, Q.t))
                {
                    if (lim < BEGV() || lim > ZV)
                        abort();
                    SET_PT_BOTH(lim, lim_byte);
                    return Q.nil;
#if NOT_NOW 
/* This would be clean, but maybe programs depend on
	 a value of nil here.  */
	  np = lim;
#endif
                }
                else
                    return Q.nil;
            }

            if (np < BEGV() || np > ZV)
                abort();

            SET_PT(np);

            return make_number(np);
        }

        /* Return 1 if REGEXP it matches just one constant string.  */
        public static bool trivial_regexp_p(LispObject regexp)
        {
            int len = SBYTES(regexp);
            PtrEmulator<byte> s = new PtrEmulator<byte>(SDATA(regexp));
            while (--len >= 0)
            {
                switch (s.ValueAndInc())
                {
                    case (byte)'.':
                    case (byte)'*':
                    case (byte)'+':
                    case (byte)'?':
                    case (byte)'[':
                    case (byte)'^':
                    case (byte)'$':
                        return false;
                    case (byte)'\\':
                        if (--len < 0)
                            return false;
                        switch (s.ValueAndInc())
                        {
                            case (byte)'|':
                            case (byte)'(':
                            case (byte)')':
                            case (byte)'`':
                            case (byte)'\'':
                            case (byte)'b':
                            case (byte)'B':
                            case (byte)'<':
                            case (byte)'>':
                            case (byte)'w':
                            case (byte)'W':
                            case (byte)'s':
                            case (byte)'S':
                            case (byte)'=':
                            case (byte)'{':
                            case (byte)'}':
                            case (byte)'_':
                            case (byte)'c':
                            case (byte)'C':	/* for categoryspec and notcategoryspec */
                            case (byte)'1':
                            case (byte)'2':
                            case (byte)'3':
                            case (byte)'4':
                            case (byte)'5':
                            case (byte)'6':
                            case (byte)'7':
                            case (byte)'8':
                            case (byte)'9':
                                return false;
                        }
                        break;
                    default:
                        break;
                }
            }
            return true;
        }

        /* Search for the n'th occurrence of STRING in the current buffer,
           starting at position POS and stopping at position LIM,
           treating STRING as a literal string if RE is false or as
           a regular expression if RE is true.

           If N is positive, searching is forward and LIM must be greater than POS.
           If N is negative, searching is backward and LIM must be less than POS.

           Returns -x if x occurrences remain to be found (x > 0),
           or else the position at the beginning of the Nth occurrence
           (if searching backward) or the end (if searching forward).

           POSIX is nonzero if we want full backtracking (POSIX style)
           for this pattern.  0 means backtrack only enough to get a valid match.  */
        public static void TRANSLATE(ref int outt, LispObject trt, int d)
        {
            if (!NILP(trt))
            {
                LispObject temp;
                temp = F.aref(trt, make_number(d));
                if (INTEGERP(temp))
                    outt = XINT(temp);
                else
                    outt = d;
            }
            else
                outt = d;
        }

        /* Only used in search_buffer, to record the end position of the match
           when searching regexps and SEARCH_REGS should not be changed
           (i.e. Vinhibit_changing_match_data is non-nil).  */
        public static re_registers search_regs_1 = new re_registers();

        public static int search_buffer(LispObject stringg, int pos, int pos_byte, int lim, int lim_byte, int n,
           bool RE, LispObject trt, LispObject inverse_trt, bool posix)
        {
            int len = SCHARS(stringg);
            int len_byte = SBYTES(stringg);
            int i;

            if (running_asynch_code)
                save_search_regs();

            /* Searching 0 times means don't move.  */
            /* Null string is found at starting position.  */
            if (len == 0 || n == 0)
            {
                set_search_regs(pos_byte, 0);
                return pos;
            }

            if (RE && !(trivial_regexp_p(stringg) && NILP(V.search_spaces_regexp)))
            {
                PtrEmulator<byte> p1, p2;
                int s1, s2;
                re_pattern_buffer bufp;

                bufp = compile_pattern(stringg,
                            (NILP(V.inhibit_changing_match_data)
                             ? search_regs : search_regs_1),
                            trt, posix,
                            !NILP(current_buffer.enable_multibyte_characters));

                immediate_quit = 1;	/* Quit immediately if user types ^G,
				   because letting this function finish
				   can take too long. */
                QUIT();			/* Do a pending quit right away,
				   to avoid paradoxical behavior */
                /* Get pointers and sizes of the two strings
               that make up the visible portion of the buffer. */

                p1 = BEGV_ADDR();
                s1 = GPT_BYTE - BEGV_BYTE();
                p2 = GAP_END_ADDR();
                s2 = ZV_BYTE - GPT_BYTE;
                if (s1 < 0)
                {
                    p2 = p1;
                    s2 = ZV_BYTE - BEGV_BYTE();
                    s1 = 0;
                }
                if (s2 < 0)
                {
                    s1 = ZV_BYTE - BEGV_BYTE();
                    s2 = 0;
                }
                re_match_object = Q.nil;

                while (n < 0)
                {
                    int val;
                    val = re_search_2(bufp, p1, s1, p2, s2,
                               pos_byte - BEGV_BYTE(), lim_byte - pos_byte,
                               (NILP(V.inhibit_changing_match_data)
                                ? search_regs : search_regs_1),
                        /* Don't allow match past current point */
                               pos_byte - BEGV_BYTE());
                    if (val == -2)
                    {
                        matcher_overflow();
                    }
                    if (val >= 0)
                    {
                        if (NILP(V.inhibit_changing_match_data))
                        {
                            pos_byte = search_regs.start[0] + BEGV_BYTE();
                            for (i = 0; i < search_regs.num_regs; i++)
                                if (search_regs.start[i] >= 0)
                                {
                                    search_regs.start[i]
                                      = BYTE_TO_CHAR(search_regs.start[i] + BEGV_BYTE());
                                    search_regs.end[i]
                                      = BYTE_TO_CHAR(search_regs.end[i] + BEGV_BYTE());
                                }
                            last_thing_searched = current_buffer;
                            /* Set pos to the new position. */
                            pos = search_regs.start[0];
                        }
                        else
                        {
                            pos_byte = search_regs_1.start[0] + BEGV_BYTE();
                            /* Set pos to the new position.  */
                            pos = BYTE_TO_CHAR(search_regs_1.start[0] + BEGV_BYTE());
                        }
                    }
                    else
                    {
                        immediate_quit = 0;
                        return (n);
                    }
                    n++;
                }
                while (n > 0)
                {
                    int val;
                    val = re_search_2(bufp, p1, s1, p2, s2,
                               pos_byte - BEGV_BYTE(), lim_byte - pos_byte,
                               (NILP(V.inhibit_changing_match_data)
                                ? search_regs : search_regs_1),
                               lim_byte - BEGV_BYTE());
                    if (val == -2)
                    {
                        matcher_overflow();
                    }
                    if (val >= 0)
                    {
                        if (NILP(V.inhibit_changing_match_data))
                        {
                            pos_byte = search_regs.end[0] + BEGV_BYTE();
                            for (i = 0; i < search_regs.num_regs; i++)
                                if (search_regs.start[i] >= 0)
                                {
                                    search_regs.start[i]
                                      = BYTE_TO_CHAR(search_regs.start[i] + BEGV_BYTE());
                                    search_regs.end[i]
                                      = BYTE_TO_CHAR(search_regs.end[i] + BEGV_BYTE());
                                }
                            last_thing_searched = current_buffer;
                            pos = search_regs.end[0];
                        }
                        else
                        {
                            pos_byte = search_regs_1.end[0] + BEGV_BYTE();
                            pos = BYTE_TO_CHAR(search_regs_1.end[0] + BEGV_BYTE());
                        }
                    }
                    else
                    {
                        immediate_quit = 0;
                        return (0 - n);
                    }
                    n--;
                }
                immediate_quit = 0;
                return (pos);
            }
            else				/* non-RE case */
            {
                byte[] raw_pattern;
                PtrEmulator<byte> pat;
                int raw_pattern_size;
                int raw_pattern_size_byte;
                byte[] patbuf;
                bool multibyte = !NILP(current_buffer.enable_multibyte_characters);
                PtrEmulator<byte> base_pat;
                /* Set to positive if we find a non-ASCII char that need
               translation.  Otherwise set to zero later.  */
                int char_base = -1;
                bool boyer_moore_ok = true;

                /* MULTIBYTE says whether the text to be searched is multibyte.
               We must convert PATTERN to match that, or we will not really
               find things right.  */

                if (multibyte == STRING_MULTIBYTE(stringg))
                {
                    raw_pattern = SDATA(stringg);
                    raw_pattern_size = SCHARS(stringg);
                    raw_pattern_size_byte = SBYTES(stringg);
                }
                else if (multibyte)
                {
                    raw_pattern_size = SCHARS(stringg);
                    raw_pattern_size_byte
                      = count_size_as_multibyte(SDATA(stringg),
                                     raw_pattern_size);
                    raw_pattern = new byte[raw_pattern_size_byte]; // + 1);
                    copy_text(SDATA(stringg), new PtrEmulator<byte>(raw_pattern),
                           SCHARS(stringg), false, true);
                }
                else
                {
                    /* Converting multibyte to single-byte.

                       ??? Perhaps this conversion should be done in a special way
                       by subtracting nonascii-insert-offset from each non-ASCII char,
                       so that only the multibyte chars which really correspond to
                       the chosen single-byte character set can possibly match.  */
                    raw_pattern_size = SCHARS(stringg);
                    raw_pattern_size_byte = SCHARS(stringg);
                    raw_pattern = new byte[raw_pattern_size]; // + 1);
                    copy_text(SDATA(stringg), new PtrEmulator<byte>(raw_pattern),
                           SBYTES(stringg), true, false);
                }

                /* Copy and optionally translate the pattern.  */
                len = raw_pattern_size;
                len_byte = raw_pattern_size_byte;
                patbuf = new byte[len * MAX_MULTIBYTE_LENGTH];
                pat = new PtrEmulator<byte>(patbuf);
                base_pat = new PtrEmulator<byte>(raw_pattern);
                if (multibyte)
                {
                    /* Fill patbuf by translated characters in STRING while
                       checking if we can use boyer-moore search.  If TRT is
                       non-nil, we can use boyer-moore search only if TRT can be
                       represented by the byte array of 256 elements.  For that,
                       all non-ASCII case-equivalents of all case-senstive
                       characters in STRING must belong to the same charset and
                       row.  */

                    while (--len >= 0)
                    {
                        byte[] str_base = new byte[MAX_MULTIBYTE_LENGTH];
                        PtrEmulator<byte> str;
                        int c, translated = 0, inverse = 0;
                        int in_charlen = 0, charlen;

                        /* If we got here and the RE flag is set, it's because we're
                       dealing with a regexp known to be trivial, so the backslash
                       just quotes the next character.  */
                        if (RE && base_pat.Value == '\\')
                        {
                            len--;
                            raw_pattern_size--;
                            len_byte--;
                            base_pat++;
                        }

                        c = (int)STRING_CHAR_AND_LENGTH(base_pat, len_byte, ref in_charlen);

                        if (NILP(trt))
                        {
                            str = base_pat;
                            charlen = in_charlen;
                        }
                        else
                        {
                            /* Translate the character.  */
                            TRANSLATE(ref translated, trt, c);
                            charlen = CHAR_STRING((uint)translated, str_base);
                            str = new PtrEmulator<byte>(str_base);

                            /* Check if C has any other case-equivalents.  */
                            TRANSLATE(ref inverse, inverse_trt, c);
                            /* If so, check if we can use boyer-moore.  */
                            if (c != inverse && boyer_moore_ok)
                            {
                                /* Check if all equivalents belong to the same
                               group of characters.  Note that the check of C
                               itself is done by the last iteration.  */
                                int this_char_base = -1;

                                while (boyer_moore_ok)
                                {
                                    if (ASCII_BYTE_P((byte)inverse))
                                    {
                                        if (this_char_base > 0)
                                            boyer_moore_ok = false;
                                        else
                                            this_char_base = 0;
                                    }
                                    else if (CHAR_BYTE8_P((uint)inverse))
                                        /* Boyer-moore search can't handle a
                                           translation of an eight-bit
                                           character.  */
                                        boyer_moore_ok = false;
                                    else if (this_char_base < 0)
                                    {
                                        this_char_base = inverse & ~0x3F;
                                        if (char_base < 0)
                                            char_base = this_char_base;
                                        else if (this_char_base != char_base)
                                            boyer_moore_ok = false;
                                    }
                                    else if ((inverse & ~0x3F) != this_char_base)
                                        boyer_moore_ok = false;
                                    if (c == inverse)
                                        break;
                                    TRANSLATE(ref inverse, inverse_trt, inverse);
                                }
                            }
                        }

                        /* Store this character into the translated pattern.  */
                        PtrEmulator<byte>.bcopy(str, pat, charlen);
                        pat += charlen;
                        base_pat += in_charlen;
                        len_byte -= in_charlen;
                    }

                    /* If char_base is still negative we didn't find any translated
                       non-ASCII characters.  */
                    if (char_base < 0)
                        char_base = 0;
                }
                else
                {
                    /* Unibyte buffer.  */
                    char_base = 0;
                    while (--len >= 0)
                    {
                        int c, translated = 0;

                        /* If we got here and the RE flag is set, it's because we're
                       dealing with a regexp known to be trivial, so the backslash
                       just quotes the next character.  */
                        if (RE && base_pat.Value == '\\')
                        {
                            len--;
                            raw_pattern_size--;
                            base_pat++;
                        }
                        c = (int)base_pat.ValueAndInc();
                        TRANSLATE(ref translated, trt, c);
                        pat.Value = (byte)translated;
                        pat++;
                    }
                }

                len_byte = pat - patbuf;
                len = raw_pattern_size;
                pat = base_pat = new PtrEmulator<byte>(patbuf);

                if (boyer_moore_ok)
                    return boyer_moore(n, pat, len, len_byte, trt, inverse_trt,
                                pos, pos_byte, lim, lim_byte,
                                char_base);
                else
                    return simple_search(n, pat, len, len_byte, trt,
                                  pos, pos_byte, lim, lim_byte);
            }
        }

        /* Do a simple string search N times for the string PAT,
           whose length is LEN/LEN_BYTE,
           from buffer position POS/POS_BYTE until LIM/LIM_BYTE.
           TRT is the translation table.

           Return the character position where the match is found.
           Otherwise, if M matches remained to be found, return -M.

           This kind of search works regardless of what is in PAT and
           regardless of what is in TRT.  It is used in cases where
           boyer_moore cannot work.  */
        public static int simple_search(int n, PtrEmulator<byte> pat, int len, int len_byte,
                                        LispObject trt, int pos, int pos_byte, int lim, int lim_byte)
        {
            bool multibyte = !NILP(current_buffer.enable_multibyte_characters);
            bool forward = n > 0;
            /* Number of buffer bytes matched.  Note that this may be different
               from len_byte in a multibyte buffer.  */
            int match_byte = 0;

            if (lim > pos && multibyte)
                while (n > 0)
                {
                    while (true)
                    {
                        /* Try matching at position POS.  */
                        int this_pos = pos;
                        int this_pos_byte = pos_byte;
                        int this_len = len;
                        int this_len_byte = len_byte;
                        PtrEmulator<byte> p = new PtrEmulator<byte>(pat);
                        if (pos + len > lim || pos_byte + len_byte > lim_byte)
                            goto stop;

                        while (this_len > 0)
                        {
                            int charlen = 0, buf_charlen = 0;
                            int pat_ch, buf_ch;

                            pat_ch = (int)STRING_CHAR_AND_LENGTH(p, this_len_byte, ref charlen);
                            buf_ch = (int)STRING_CHAR_AND_LENGTH(BYTE_POS_ADDR(this_pos_byte),
                                             ZV_BYTE - this_pos_byte,
                                             ref buf_charlen);
                            TRANSLATE(ref buf_ch, trt, buf_ch);

                            if (buf_ch != pat_ch)
                                break;

                            this_len_byte -= charlen;
                            this_len--;
                            p += charlen;

                            this_pos_byte += buf_charlen;
                            this_pos++;
                        }

                        if (this_len == 0)
                        {
                            match_byte = this_pos_byte - pos_byte;
                            pos += len;
                            pos_byte += match_byte;
                            break;
                        }

                        INC_BOTH(ref pos, ref pos_byte);
                    }

                    n--;
                }
            else if (lim > pos)
                while (n > 0)
                {
                    while (true)
                    {
                        /* Try matching at position POS.  */
                        int this_pos = pos;
                        int this_len = len;
                        PtrEmulator<byte> p = new PtrEmulator<byte>(pat);

                        if (pos + len > lim)
                            goto stop;

                        while (this_len > 0)
                        {
                            int pat_ch = p.ValueAndInc();
                            int buf_ch = FETCH_BYTE(this_pos);
                            TRANSLATE(ref buf_ch, trt, buf_ch);

                            if (buf_ch != pat_ch)
                                break;

                            this_len--;
                            this_pos++;
                        }

                        if (this_len == 0)
                        {
                            match_byte = len;
                            pos += len;
                            break;
                        }

                        pos++;
                    }

                    n--;
                }
            /* Backwards search.  */
            else if (lim < pos && multibyte)
                while (n < 0)
                {
                    while (true)
                    {
                        /* Try matching at position POS.  */
                        int this_pos = pos - len;
                        int this_pos_byte;
                        int this_len = len;
                        int this_len_byte = len_byte;
                        PtrEmulator<byte> p = new PtrEmulator<byte>(pat);

                        if (this_pos < lim || (pos_byte - len_byte) < lim_byte)
                            goto stop;
                        this_pos_byte = CHAR_TO_BYTE(this_pos);
                        match_byte = pos_byte - this_pos_byte;

                        while (this_len > 0)
                        {
                            int charlen = 0, buf_charlen = 0;
                            int pat_ch, buf_ch;

                            pat_ch = (int)STRING_CHAR_AND_LENGTH(p, this_len_byte, ref charlen);
                            buf_ch = (int)STRING_CHAR_AND_LENGTH(BYTE_POS_ADDR(this_pos_byte),
                                             ZV_BYTE - this_pos_byte,
                                             ref buf_charlen);
                            TRANSLATE(ref buf_ch, trt, buf_ch);

                            if (buf_ch != pat_ch)
                                break;

                            this_len_byte -= charlen;
                            this_len--;
                            p += charlen;
                            this_pos_byte += buf_charlen;
                            this_pos++;
                        }

                        if (this_len == 0)
                        {
                            pos -= len;
                            pos_byte -= match_byte;
                            break;
                        }

                        DEC_BOTH(ref pos, ref pos_byte);
                    }

                    n++;
                }
            else if (lim < pos)
                while (n < 0)
                {
                    while (true)
                    {
                        /* Try matching at position POS.  */
                        int this_pos = pos - len;
                        int this_len = len;
                        PtrEmulator<byte> p = new PtrEmulator<byte>(pat);

                        if (this_pos < lim)
                            goto stop;

                        while (this_len > 0)
                        {
                            int pat_ch = p.ValueAndInc();
                            int buf_ch = FETCH_BYTE(this_pos);
                            TRANSLATE(ref buf_ch, trt, buf_ch);

                            if (buf_ch != pat_ch)
                                break;
                            this_len--;
                            this_pos++;
                        }

                        if (this_len == 0)
                        {
                            match_byte = len;
                            pos -= len;
                            break;
                        }

                        pos--;
                    }

                    n++;
                }

        stop:
            if (n == 0)
            {
                if (forward)
                    set_search_regs((multibyte ? pos_byte : pos) - match_byte, match_byte);
                else
                    set_search_regs(multibyte ? pos_byte : pos, match_byte);

                return pos;
            }
            else if (n > 0)
                return -n;
            else
                return n;
        }

        /* Do Boyer-Moore search N times for the string BASE_PAT,
           whose length is LEN/LEN_BYTE,
           from buffer position POS/POS_BYTE until LIM/LIM_BYTE.
           DIRECTION says which direction we search in.
           TRT and INVERSE_TRT are translation tables.
           Characters in PAT are already translated by TRT.

           This kind of search works if all the characters in BASE_PAT that
           have nontrivial translation are the same aside from the last byte.
           This makes it possible to translate just the last byte of a
           character, and do so after just a simple test of the context.
           CHAR_BASE is nonzero if there is such a non-ASCII character.

           If that criterion is not satisfied, do not call this function.  */
        public static int boyer_moore(int n, PtrEmulator<byte> base_pat, int len, int len_byte, LispObject trt, LispObject inverse_trt,
         int pos, int pos_byte, int lim, int lim_byte, int char_base)
        {
            int direction = ((n > 0) ? 1 : -1);
            int dirlen;
            int infinity, limit, stride_for_teases = 0;
            PtrEmulator<int> BM_tab;
            PtrEmulator<int> BM_tab_base;
            PtrEmulator<byte> cursor, p_limit;
            int i, j;
            PtrEmulator<byte> pat, pat_end;
            bool multibyte = !NILP(current_buffer.enable_multibyte_characters);

            PtrEmulator<byte> simple_translate = new PtrEmulator<byte>(new byte[256]);
            /* These are set to the preceding bytes of a byte to be translated
               if char_base is nonzero.  As the maximum byte length of a
               multibyte character is 5, we have to check at most four previous
               bytes.  */
            int translate_prev_byte1 = 0;
            int translate_prev_byte2 = 0;
            int translate_prev_byte3 = 0;
            int translate_prev_byte4 = 0;

            BM_tab = new PtrEmulator<int>(new int[256]);

            /* The general approach is that we are going to maintain that we know */
            /* the first (closest to the present position, in whatever direction */
            /* we're searching) character that could possibly be the last */
            /* (furthest from present position) character of a valid match.  We */
            /* advance the state of our knowledge by looking at that character */
            /* and seeing whether it indeed matches the last character of the */
            /* pattern.  If it does, we take a closer look.  If it does not, we */
            /* move our pointer (to putative last characters) as far as is */
            /* logically possible.  This amount of movement, which I call a */
            /* stride, will be the length of the pattern if the actual character */
            /* appears nowhere in the pattern, otherwise it will be the distance */
            /* from the last occurrence of that character to the end of the */
            /* pattern. */
            /* As a coding trick, an enormous stride is coded into the table for */
            /* characters that match the last character.  This allows use of only */
            /* a single test, a test for having gone past the end of the */
            /* permissible match region, to test for both possible matches (when */
            /* the stride goes past the end immediately) and failure to */
            /* match (where you get nudged past the end one stride at a time). */

            /* Here we make a "mickey mouse" BM table.  The stride of the search */
            /* is determined only by the last character of the putative match. */
            /* If that character does not match, we will stride the proper */
            /* distance to propose a match that superimposes it on the last */
            /* instance of a character that matches it (per trt), or misses */
            /* it entirely if there is none. */

            dirlen = len_byte * direction;
            infinity = dirlen - (lim_byte + pos_byte + len_byte + len_byte) * direction;

            /* Record position after the end of the pattern.  */
            pat_end = base_pat + len_byte;
            /* BASE_PAT points to a character that we start scanning from.
               It is the first character in a forward search,
               the last character in a backward search.  */
            if (direction < 0)
                base_pat = pat_end - 1;

            BM_tab_base = BM_tab;
            BM_tab += 256;
            j = dirlen;		/* to get it in a register */
            /* A character that does not appear in the pattern induces a */
            /* stride equal to the pattern length. */
            while (BM_tab_base != BM_tab)
            {
                --BM_tab;
                BM_tab.Value = j;

                --BM_tab;
                BM_tab.Value = j;

                --BM_tab;
                BM_tab.Value = j;

                --BM_tab;
                BM_tab.Value = j;
            }

            /* We use this for translation, instead of TRT itself.
               We fill this in to handle the characters that actually
               occur in the pattern.  Others don't matter anyway!  */
            simple_translate.bzero(simple_translate.Collection.Length, 0);

            for (i = 0; i < 256; i++)
                simple_translate[i] = (byte)i;

            if (char_base != 0)
            {
                /* Setup translate_prev_byte1/2/3/4 from CHAR_BASE.  Only a
               byte following them are the target of translation.  */
                byte[] str = new byte[MAX_MULTIBYTE_LENGTH];
                int lenn = CHAR_STRING((uint)char_base, str);

                translate_prev_byte1 = str[lenn - 2];
                if (lenn > 2)
                {
                    translate_prev_byte2 = str[lenn - 3];
                    if (lenn > 3)
                    {
                        translate_prev_byte3 = str[lenn - 4];
                        if (lenn > 4)
                            translate_prev_byte4 = str[lenn - 5];
                    }
                }
            }

            i = 0;
            while (i != infinity)
            {
                PtrEmulator<byte> ptr = new PtrEmulator<byte>(base_pat, i);
                i += direction;
                if (i == dirlen)
                    i = infinity;
                if (!NILP(trt))
                {
                    /* If the byte currently looking at is the last of a
                       character to check case-equivalents, set CH to that
                       character.  An ASCII character and a non-ASCII character
                       matching with CHAR_BASE are to be checked.  */
                    int ch = -1;

                    if (ASCII_BYTE_P(ptr.Value) || !multibyte)
                        ch = ptr.Value;
                    else if (char_base != 0
                         && ((pat_end - ptr) == 1 || CHAR_HEAD_P(ptr[1])))
                    {
                        PtrEmulator<byte> charstart = new PtrEmulator<byte>(ptr, -1);

                        while (!(CHAR_HEAD_P(charstart.Value)))
                            charstart--;
                        ch = (int)STRING_CHAR(charstart.Collection, charstart.Index, ptr - charstart + 1);
                        if (char_base != (ch & ~0x3F))
                            ch = -1;
                    }

                    if (ch >= 128)
                        j = (ch & 0x3F) | 128;
                    else
                        j = ptr.Value;

                    if (i == infinity)
                        stride_for_teases = BM_tab[j];

                    BM_tab[j] = dirlen - i;
                    /* A translation table is accompanied by its inverse -- see */
                    /* comment following downcase_table for details */
                    if (ch >= 0)
                    {
                        int starting_ch = ch;
                        int starting_j = j;

                        while (true)
                        {
                            TRANSLATE(ref ch, inverse_trt, ch);
                            if (ch >= 128)
                                j = (ch & 0x3F) | 128;
                            else
                                j = ch;

                            /* For all the characters that map into CH,
                               set up simple_translate to map the last byte
                               into STARTING_J.  */
                            simple_translate[j] = (byte)starting_j;
                            if (ch == starting_ch)
                                break;
                            BM_tab[j] = dirlen - i;
                        }
                    }
                }
                else
                {
                    j = ptr.Value;

                    if (i == infinity)
                        stride_for_teases = BM_tab[j];
                    BM_tab[j] = dirlen - i;
                }
                /* stride_for_teases tells how much to stride if we get a */
                /* match on the far character but are subsequently */
                /* disappointed, by recording what the stride would have been */
                /* for that character if the last character had been */
                /* different. */
            }
            infinity = dirlen - infinity;
            pos_byte += dirlen - ((direction > 0) ? direction : 0);
            /* loop invariant - POS_BYTE points at where last char (first
               char if reverse) of pattern would align in a possible match.  */
            while (n != 0)
            {
                int tail_end;
                PtrEmulator<byte> tail_end_ptr;

                /* It's been reported that some (broken) compiler thinks that
               Boolean expressions in an arithmetic context are unsigned.
               Using an explicit ?1:0 prevents this.  */
                if ((lim_byte - pos_byte - ((direction > 0) ? 1 : 0)) * direction
                < 0)
                    return (n * (0 - direction));
                /* First we do the part we can by pointers (maybe nothing) */
                QUIT();
                pat = base_pat;
                limit = pos_byte - dirlen + direction;
                if (direction > 0)
                {
                    limit = BUFFER_CEILING_OF(limit);
                    /* LIMIT is now the last (not beyond-last!) value POS_BYTE
                       can take on without hitting edge of buffer or the gap.  */
                    limit = System.Math.Min(limit, pos_byte + 20000);
                    limit = System.Math.Min(limit, lim_byte - 1);
                }
                else
                {
                    limit = BUFFER_FLOOR_OF(limit);
                    /* LIMIT is now the last (not beyond-last!) value POS_BYTE
                       can take on without hitting edge of buffer or the gap.  */
                    limit = System.Math.Max(limit, pos_byte - 20000);
                    limit = System.Math.Max(limit, lim_byte);
                }
                tail_end = BUFFER_CEILING_OF(pos_byte) + 1;
                tail_end_ptr = BYTE_POS_ADDR(tail_end);

                if ((limit - pos_byte) * direction > 20)
                {
                    PtrEmulator<byte> p2;

                    p_limit = BYTE_POS_ADDR(limit);
                    p2 = (cursor = BYTE_POS_ADDR(pos_byte));
                    /* In this loop, pos + cursor - p2 is the surrogate for pos */
                    while (true)		/* use one cursor setting as long as i can */
                    {
                        if (direction > 0) /* worth duplicating */
                        {
                            /* Use signed comparison if appropriate
                               to make cursor+infinity sure to be > p_limit.
                               Assuming that the buffer lies in a range of addresses
                               that are all "positive" (as ints) or all "negative",
                               either kind of comparison will work as long
                               as we don't step by infinity.  So pick the kind
                               that works when we do step by infinity.  */
                            if ((p_limit + infinity) > p_limit)
                                while (cursor <= p_limit)
                                    cursor += BM_tab[cursor.Value];
                            else
                                while (cursor <= p_limit)
                                    cursor += BM_tab[cursor.Value];
                        }
                        else
                        {
                            if ((p_limit + infinity) < p_limit)
                                while (cursor >= p_limit)
                                    cursor += BM_tab[cursor.Value];
                            else
                                while (cursor >= p_limit)
                                    cursor += BM_tab[cursor.Value];
                        }
                        /* If you are here, cursor is beyond the end of the searched region. */
                        /* This can happen if you match on the far character of the pattern, */
                        /* because the "stride" of that character is infinity, a number able */
                        /* to throw you well beyond the end of the search.  It can also */
                        /* happen if you fail to match within the permitted region and would */
                        /* otherwise try a character beyond that region */
                        if ((cursor - p_limit) * direction <= len_byte)
                            break;	/* a small overrun is genuine */
                        cursor -= infinity; /* large overrun = hit */
                        i = dirlen - direction;
                        if (!NILP(trt))
                        {
                            while ((i -= direction) + direction != 0)
                            {
                                int ch;
                                cursor -= direction;
                                /* Translate only the last byte of a character.  */
                                if (!multibyte
                                || ((cursor == tail_end_ptr
                                     || CHAR_HEAD_P(cursor[1]))
                                    && (CHAR_HEAD_P(cursor[0])
                                    /* Check if this is the last byte of
                                       a translable character.  */
                                    || (translate_prev_byte1 == cursor[-1]
                                        && (CHAR_HEAD_P((byte)translate_prev_byte1)
                                        || (translate_prev_byte2 == cursor[-2]
                                            && (CHAR_HEAD_P((byte)translate_prev_byte2)
                                            || (translate_prev_byte3 == cursor[-3]))))))))
                                    ch = simple_translate[cursor.Value];
                                else
                                    ch = cursor.Value;
                                if (pat[i] != ch)
                                    break;
                            }
                        }
                        else
                        {
                            while ((i -= direction) + direction != 0)
                            {
                                cursor -= direction;
                                if (pat[i] != cursor.Value)
                                    break;
                            }
                        }
                        cursor += dirlen - i - direction;	/* fix cursor */
                        if (i + direction == 0)
                        {
                            int position, start, end;

                            cursor -= direction;

                            position = pos_byte + (cursor - p2) + ((direction > 0)
                                                 ? 1 - len_byte : 0);
                            set_search_regs(position, len_byte);

                            if (NILP(V.inhibit_changing_match_data))
                            {
                                start = search_regs.start[0];
                                end = search_regs.end[0];
                            }
                            else
                            /* If Vinhibit_changing_match_data is non-nil,
                               search_regs will not be changed.  So let's
                               compute start and end here.  */
                            {
                                start = BYTE_TO_CHAR(position);
                                end = BYTE_TO_CHAR(position + len_byte);
                            }

                            if ((n -= direction) != 0)
                                cursor += dirlen; /* to resume search */
                            else
                                return direction > 0 ? end : start;
                        }
                        else
                            cursor += stride_for_teases; /* <sigh> we lose -  */
                    }
                    pos_byte += cursor - p2;
                }
                else
                /* Now we'll pick up a clump that has to be done the hard */
                /* way because it covers a discontinuity */
                {
                    limit = ((direction > 0)
                         ? BUFFER_CEILING_OF(pos_byte - dirlen + 1)
                         : BUFFER_FLOOR_OF(pos_byte - dirlen - 1));
                    limit = ((direction > 0)
                         ? System.Math.Min(limit + len_byte, lim_byte - 1)
                         : System.Math.Max(limit - len_byte, lim_byte));
                    /* LIMIT is now the last value POS_BYTE can have
                       and still be valid for a possible match.  */
                    while (true)
                    {
                        /* This loop can be coded for space rather than */
                        /* speed because it will usually run only once. */
                        /* (the reach is at most len + 21, and typically */
                        /* does not exceed len) */
                        while ((limit - pos_byte) * direction >= 0)
                            pos_byte += BM_tab[FETCH_BYTE(pos_byte)];
                        /* now run the same tests to distinguish going off the */
                        /* end, a match or a phony match. */
                        if ((pos_byte - limit) * direction <= len_byte)
                            break;	/* ran off the end */
                        /* Found what might be a match.
                       Set POS_BYTE back to last (first if reverse) pos.  */
                        pos_byte -= infinity;
                        i = dirlen - direction;
                        while ((i -= direction) + direction != 0)
                        {
                            int ch;
                            PtrEmulator<byte> ptr;
                            pos_byte -= direction;
                            ptr = BYTE_POS_ADDR(pos_byte);
                            /* Translate only the last byte of a character.  */
                            if (!multibyte
                                || ((ptr == tail_end_ptr
                                 || CHAR_HEAD_P(ptr[1]))
                                && (CHAR_HEAD_P(ptr[0])
                                /* Check if this is the last byte of a
                               translable character.  */
                                    || (translate_prev_byte1 == ptr[-1]
                                    && (CHAR_HEAD_P((byte)translate_prev_byte1)
                                        || (translate_prev_byte2 == ptr[-2]
                                        && (CHAR_HEAD_P((byte)translate_prev_byte2)
                                            || translate_prev_byte3 == ptr[-3])))))))
                                ch = simple_translate[ptr.Value];
                            else
                                ch = ptr.Value;
                            if (pat[i] != ch)
                                break;
                        }
                        /* Above loop has moved POS_BYTE part or all the way
                       back to the first pos (last pos if reverse).
                       Set it once again at the last (first if reverse) char.  */
                        pos_byte += dirlen - i - direction;
                        if (i + direction == 0)
                        {
                            int position, start, end;
                            pos_byte -= direction;

                            position = pos_byte + ((direction > 0) ? 1 - len_byte : 0);
                            set_search_regs(position, len_byte);

                            if (NILP(V.inhibit_changing_match_data))
                            {
                                start = search_regs.start[0];
                                end = search_regs.end[0];
                            }
                            else
                            /* If Vinhibit_changing_match_data is non-nil,
                               search_regs will not be changed.  So let's
                               compute start and end here.  */
                            {
                                start = BYTE_TO_CHAR(position);
                                end = BYTE_TO_CHAR(position + len_byte);
                            }

                            if ((n -= direction) != 0)
                                pos_byte += dirlen; /* to resume search */
                            else
                                return direction > 0 ? end : start;
                        }
                        else
                            pos_byte += stride_for_teases;
                    }
                }
                /* We have done one clump.  Can we continue? */
                if ((lim_byte - pos_byte) * direction < 0)
                    return ((0 - n) * direction);
            }
            return BYTE_TO_CHAR(pos_byte);
        }


        /* Record beginning BEG_BYTE and end BEG_BYTE + NBYTES
           for the overall match just found in the current buffer.
           Also clear out the match data for registers 1 and up.  */
        public static void set_search_regs(int beg_byte, int nbytes)
        {
            int i;

            if (!NILP(V.inhibit_changing_match_data))
                return;

            /* Make sure we have registers in which to store
               the match position.  */
            if (search_regs.num_regs == 0)
            {
                search_regs.start = new int[2];
                search_regs.end = new int[2];
                search_regs.num_regs = 2;
            }

            /* Clear out the other registers.  */
            for (i = 1; i < search_regs.num_regs; i++)
            {
                search_regs.start[i] = -1;
                search_regs.end[i] = -1;
            }

            search_regs.start[0] = BYTE_TO_CHAR(beg_byte);
            search_regs.end[0] = BYTE_TO_CHAR(beg_byte + nbytes);
            last_thing_searched = current_buffer;
        }

        /* Given STRING, a string of words separated by word delimiters,
           compute a regexp that matches those exact words separated by
           arbitrary punctuation.  If LAX is nonzero, the end of the string
           need not match a word boundary unless it ends in whitespace.  */
        public static LispObject wordify(LispObject stringg, bool lax)
        {
            PtrEmulator<byte> p, o;
            int i, i_byte, len, punct_count = 0, word_count = 0;
            LispObject val;
            int prev_c = 0;
            int adjust;
            bool whitespace_at_end;

            CHECK_STRING(stringg);
            p = SDATA(stringg);
            len = SCHARS(stringg);

            for (i = 0, i_byte = 0; i < len; )
            {
                int c = 0;

                FETCH_STRING_CHAR_AS_MULTIBYTE_ADVANCE(ref c, stringg, ref i, ref i_byte);

                if (SYNTAX((uint)c) != syntaxcode.Sword)
                {
                    punct_count++;
                    if (i > 0 && SYNTAX((uint)prev_c) == syntaxcode.Sword)
                        word_count++;
                }

                prev_c = c;
            }

            if (SYNTAX((uint)prev_c) == syntaxcode.Sword)
            {
                word_count++;
                whitespace_at_end = false;
            }
            else
                whitespace_at_end = true;

            if (word_count == 0)
                return empty_unibyte_string;

            adjust = -punct_count + 5 * (word_count - 1)
              + ((lax && !whitespace_at_end) ? 2 : 4);
            if (STRING_MULTIBYTE(stringg))
                val = make_uninit_multibyte_string(len + adjust,
                                SBYTES(stringg)
                                + adjust);
            else
                val = make_uninit_string(len + adjust);

            o = SDATA(val);
            o.SetValueAndInc((byte)'\\');
            o.SetValueAndInc((byte)'b');
            prev_c = 0;

            for (i = 0, i_byte = 0; i < len; )
            {
                int c = 0;
                int i_byte_orig = i_byte;

                FETCH_STRING_CHAR_AS_MULTIBYTE_ADVANCE(ref c, stringg, ref i, ref i_byte);

                if (SYNTAX((uint)c) == syntaxcode.Sword)
                {
                    PtrEmulator<byte>.bcopy(new PtrEmulator<byte>(SDATA(stringg), i_byte_orig),
                        o,
                        i_byte - i_byte_orig);
                    o += i_byte - i_byte_orig;
                }
                else if (i > 0 && SYNTAX((uint)prev_c) == syntaxcode.Sword && --word_count != 0)
                {
                    o.SetValueAndInc((byte)'\\');
                    o.SetValueAndInc((byte)'W');
                    o.SetValueAndInc((byte)'\\');
                    o.SetValueAndInc((byte)'W');
                    o.SetValueAndInc((byte)'*');
                }

                prev_c = c;
            }

            if (!lax || whitespace_at_end)
            {
                o.SetValueAndInc((byte)'\\');
                o.SetValueAndInc((byte)'b');
            }

            return val;
        }
    }

    public partial class F
    {
        public static LispObject looking_at(LispObject regexp)
        {
            return L.looking_at_1(regexp, false);
        }

        public static LispObject posix_looking_at(LispObject regexp)
        {
            return L.looking_at_1(regexp, true);
        }

        public static LispObject string_match(LispObject regexp, LispObject stringg, LispObject start)
        {
            return L.string_match_1(regexp, stringg, start, false);
        }

        public static LispObject posix_string_match(LispObject regexp, LispObject stringg, LispObject start)
        {
            return L.string_match_1(regexp, stringg, start, true);
        }

        public static LispObject match_beginning(LispObject subexp)
        {
            return L.match_limit(subexp, true);
        }

        public static LispObject match_end(LispObject subexp)
        {
            return L.match_limit(subexp, false);
        }

        public static LispObject match_data(LispObject integers, LispObject reuse, LispObject reseat)
        {
            LispObject tail, prev;
            LispObject[] data;
            int i, len;

            if (!L.NILP(reseat))
                for (tail = reuse; L.CONSP(tail); tail = L.XCDR(tail))
                    if (L.MARKERP(L.XCAR(tail)))
                    {
                        L.unchain_marker(L.XMARKER(L.XCAR(tail)));
                        L.XSETCAR(tail, Q.nil);
                    }

            if (L.NILP(L.last_thing_searched))
                return Q.nil;

            prev = Q.nil;

            data = new LispObject[2 * L.search_regs.num_regs + 1];

            len = 0;
            for (i = 0; i < L.search_regs.num_regs; i++)
            {
                int start = L.search_regs.start[i];
                if (start >= 0)
                {
                    if (L.EQ(L.last_thing_searched, Q.t)
                        || !L.NILP(integers))
                    {
                        data[2 * i] = L.XSETINT(start);
                        data[2 * i + 1] = L.XSETINT(L.search_regs.end[i]);
                    }
                    else if (L.BUFFERP(L.last_thing_searched))
                    {
                        data[2 * i] = F.make_marker();
                        F.set_marker(data[2 * i],
                             L.make_number(start),
                             L.last_thing_searched);
                        data[2 * i + 1] = F.make_marker();
                        F.set_marker(data[2 * i + 1],
                             L.make_number(L.search_regs.end[i]),
                             L.last_thing_searched);
                    }
                    else
                        /* last_thing_searched must always be Qt, a buffer, or Qnil.  */
                        L.abort();

                    len = 2 * i + 2;
                }
                else
                    data[2 * i] = data[2 * i + 1] = Q.nil;
            }

            if (L.BUFFERP(L.last_thing_searched) && !L.NILP(integers))
            {
                data[len] = L.last_thing_searched;
                len++;
            }

            /* If REUSE is not usable, cons up the values and return them.  */
            if (!L.CONSP(reuse))
                return F.list(len, data);

            /* If REUSE is a list, store as many value elements as will fit
               into the elements of REUSE.  */
            for (i = 0, tail = reuse; L.CONSP(tail);
                 i++, tail = L.XCDR(tail))
            {
                if (i < len)
                    L.XSETCAR(tail, data[i]);
                else
                    L.XSETCAR(tail, Q.nil);
                prev = tail;
            }

            /* If we couldn't fit all value elements into REUSE,
               cons up the rest of them and add them to the end of REUSE.  */
            if (i < len)
                L.XSETCDR(prev, F.list_starting(len - i, i, data));

            return reuse;
        }

        /* We used to have an internal use variant of `reseat' described as:

              If RESEAT is `evaporate', put the markers back on the free list
              immediately.  No other references to the markers must exist in this
              case, so it is used only internally on the unwind stack and
              save-match-data from Lisp.

           But it was ill-conceived: those supposedly-internal markers get exposed via
           the undo-list, so freeing them here is unsafe.  */
        public static LispObject set_match_data(LispObject list, LispObject reseat)
        {
            int i;
            LispObject marker;

            if (L.running_asynch_code)
                L.save_search_regs();

            L.CHECK_LIST(list);

            /* Unless we find a marker with a buffer or an explicit buffer
               in LIST, assume that this match data came from a string.  */
            L.last_thing_searched = Q.t;

            /* Allocate registers if they don't already exist.  */
            {
                int length = L.XINT(F.length(list)) / 2;

                if (length > L.search_regs.num_regs)
                {
                    if (L.search_regs.num_regs == 0)
                    {
                        L.search_regs.start = new int[length];
                        L.search_regs.end = new int[length];
                    }
                    else
                    {
                        System.Array.Resize(ref L.search_regs.start, length);
                        System.Array.Resize(ref L.search_regs.end, length);
                    }

                    for (i = (int) L.search_regs.num_regs; i < length; i++)
                        L.search_regs.start[i] = -1;

                    L.search_regs.num_regs = (uint) length;
                }

                for (i = 0; L.CONSP(list); i++)
                {
                    marker = L.XCAR(list);
                    if (L.BUFFERP(marker))
                    {
                        L.last_thing_searched = marker;
                        break;
                    }
                    if (i >= length)
                        break;
                    if (L.NILP(marker))
                    {
                        L.search_regs.start[i] = -1;
                        list = L.XCDR(list);
                    }
                    else
                    {
                        int from;
                        LispObject m;

                        m = marker;
                        if (L.MARKERP(marker))
                        {
                            if (L.XMARKER(marker).buffer == null)
                                marker = L.XSETINT(0);
                            else
                                L.last_thing_searched = L.XMARKER(marker).buffer;
                        }

                        L.CHECK_NUMBER_COERCE_MARKER(ref marker);
                        from = L.XINT(marker);

                        if (!L.NILP(reseat) && L.MARKERP(m))
                        {
                            L.unchain_marker(L.XMARKER(m));
                            L.XSETCAR(list, Q.nil);
                        }

                        list = L.XCDR(list);
                        if (!L.CONSP(list))
                            break;

                        m = marker = L.XCAR(list);

                        if (L.MARKERP(marker) && L.XMARKER(marker).buffer == null)
                            marker = L.XSETINT(0);

                        L.CHECK_NUMBER_COERCE_MARKER(ref marker);
                        L.search_regs.start[i] = from;
                        L.search_regs.end[i] = L.XINT(marker);

                        if (!L.NILP(reseat) && L.MARKERP(m))
                        {
                            L.unchain_marker(L.XMARKER(m));
                            L.XSETCAR(list, Q.nil);
                        }
                    }
                    list = L.XCDR(list);
                }

                for (; i < L.search_regs.num_regs; i++)
                    L.search_regs.start[i] = -1;
            }

            return Q.nil;
        }

        public static LispObject search_forward(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(stringg, bound, noerror, count, 1, false, false);
        }

        public static LispObject search_backward(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(stringg, bound, noerror, count, -1, false, false);
        }

        public static LispObject word_search_forward(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(L.wordify(stringg, false), bound, noerror, count, 1, true, false);
        }

        public static LispObject word_search_backward(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(L.wordify(stringg, false), bound, noerror, count, -1, true, false);
        }

        public static LispObject word_search_forward_lax(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(L.wordify(stringg, true), bound, noerror, count, 1, true, true);
        }

        public static LispObject word_search_backward_lax(LispObject stringg, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(L.wordify(stringg, true), bound, noerror, count, -1, true, false);
        }

        public static LispObject re_search_forward(LispObject regexp, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(regexp, bound, noerror, count, 1, true, false);
        }

        public static LispObject re_search_backward(LispObject regexp, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(regexp, bound, noerror, count, -1, true, false);
        }

        public static LispObject posix_search_forward(LispObject regexp, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(regexp, bound, noerror, count, 1, true, true);
        }

        public static LispObject posix_search_backward(LispObject regexp, LispObject bound, LispObject noerror, LispObject count)
        {
            return L.search_command(regexp, bound, noerror, count, -1, true, true);
        }

        public enum case_action_enum { nochange, all_caps, cap_initial };

        public static LispObject replace_match(LispObject newtext, LispObject fixedcase, LispObject literal, LispObject stringg, LispObject subexp)
        {
            case_action_enum case_action;
            int pos, pos_byte;
            bool some_multiletter_word;
            bool some_lowercase;
            bool some_uppercase;
            bool some_nonuppercase_initial;
            int c = 0, prevc;
            int sub;
            int opoint, newpoint;

            L.CHECK_STRING(newtext);

            if (!L.NILP(stringg))
                L.CHECK_STRING(stringg);

            case_action = case_action_enum.nochange;	/* We tried an initialization */
            /* but some C compilers blew it */

            if (L.search_regs.num_regs <= 0)
                L.error("`replace-match' called before any match found");

            if (L.NILP(subexp))
                sub = 0;
            else
            {
                L.CHECK_NUMBER(subexp);
                sub = L.XINT(subexp);
                if (sub < 0 || sub >= L.search_regs.num_regs)
                    L.args_out_of_range(subexp, L.make_number((int) L.search_regs.num_regs));
            }

            if (L.NILP(stringg))
            {
                if (L.search_regs.start[sub] < L.BEGV()
                || L.search_regs.start[sub] > L.search_regs.end[sub]
                || L.search_regs.end[sub] > L.ZV)
                    L.args_out_of_range(L.make_number(L.search_regs.start[sub]),
                               L.make_number(L.search_regs.end[sub]));
            }
            else
            {
                if (L.search_regs.start[sub] < 0
                || L.search_regs.start[sub] > L.search_regs.end[sub]
                || L.search_regs.end[sub] > L.SCHARS(stringg))
                    L.args_out_of_range(L.make_number(L.search_regs.start[sub]),
                               L.make_number(L.search_regs.end[sub]));
            }

            if (L.NILP(fixedcase))
            {
                /* Decide how to casify by examining the matched text. */
                int last;

                pos = L.search_regs.start[sub];
                last = L.search_regs.end[sub];

                if (L.NILP(stringg))
                    pos_byte = L.CHAR_TO_BYTE(pos);
                else
                    pos_byte = L.string_char_to_byte(stringg, pos);

                prevc = '\n';
                case_action = case_action_enum.all_caps;

                /* some_multiletter_word is set nonzero if any original word
               is more than one letter long. */
                some_multiletter_word = false;
                some_lowercase = false;
                some_nonuppercase_initial = false;
                some_uppercase = false;

                while (pos < last)
                {
                    if (L.NILP(stringg))
                    {
                        c = (int) L.FETCH_CHAR_AS_MULTIBYTE(pos_byte);
                        L.INC_BOTH(ref pos, ref pos_byte);
                    }
                    else
                        L.FETCH_STRING_CHAR_AS_MULTIBYTE_ADVANCE(ref c, stringg, ref pos, ref pos_byte);

                    if (L.LOWERCASEP((uint) c))
                    {
                        /* Cannot be all caps if any original char is lower case */

                        some_lowercase = true;
                        if (L.SYNTAX((uint) prevc) != syntaxcode.Sword)
                            some_nonuppercase_initial = true;
                        else
                            some_multiletter_word = true;
                    }
                    else if (L.UPPERCASEP((uint) c))
                    {
                        some_uppercase = true;
                        if (L.SYNTAX((uint) prevc) != syntaxcode.Sword)
                            ;
                        else
                            some_multiletter_word = true;
                    }
                    else
                    {
                        /* If the initial is a caseless word constituent,
                       treat that like a lowercase initial.  */
                        if (L.SYNTAX((uint) prevc) != syntaxcode.Sword)
                            some_nonuppercase_initial = true;
                    }

                    prevc = c;
                }

                /* Convert to all caps if the old text is all caps
               and has at least one multiletter word.  */
                if (!some_lowercase && some_multiletter_word)
                    case_action = case_action_enum.all_caps;
                /* Capitalize each word, if the old text has all capitalized words.  */
                else if (!some_nonuppercase_initial && some_multiletter_word)
                    case_action = case_action_enum.cap_initial;
                else if (!some_nonuppercase_initial && some_uppercase)
                    /* Should x -> yz, operating on X, give Yz or YZ?
                       We'll assume the latter.  */
                    case_action = case_action_enum.all_caps;
                else
                    case_action = case_action_enum.nochange;
            }

            /* Do replacement in a string.  */
            if (!L.NILP(stringg))
            {
                LispObject before, after;

                before = F.substring(stringg, L.make_number(0), L.make_number(L.search_regs.start[sub]));
                after = F.substring(stringg, L.make_number(L.search_regs.end[sub]), Q.nil);

                /* Substitute parts of the match into NEWTEXT
               if desired.  */
                if (L.NILP(literal))
                {
                    int lastpos = 0;
                    int lastpos_byte = 0;
                    /* We build up the substituted string in ACCUM.  */
                    LispObject accum;
                    LispObject middle;
                    int length = L.SBYTES(newtext);

                    accum = Q.nil;

                    for (pos_byte = 0, pos = 0; pos_byte < length; )
                    {
                        int substart = -1;
                        int subend = 0;
                        bool delbackslash = false;

                        L.FETCH_STRING_CHAR_ADVANCE(ref c, newtext, ref pos, ref pos_byte);

                        if (c == '\\')
                        {
                            L.FETCH_STRING_CHAR_ADVANCE(ref c, newtext, ref pos, ref pos_byte);

                            if (c == '&')
                            {
                                substart = L.search_regs.start[sub];
                                subend = L.search_regs.end[sub];
                            }
                            else if (c >= '1' && c <= '9')
                            {
                                if (L.search_regs.start[c - '0'] >= 0
                                && c <= L.search_regs.num_regs + '0')
                                {
                                    substart = L.search_regs.start[c - '0'];
                                    subend = L.search_regs.end[c - '0'];
                                }
                                else
                                {
                                    /* If that subexp did not match,
                                       replace \\N with nothing.  */
                                    substart = 0;
                                    subend = 0;
                                }
                            }
                            else if (c == '\\')
                                delbackslash = true;
                            else
                                L.error("Invalid use of `\\' in replacement text");
                        }
                        if (substart >= 0)
                        {
                            if (pos - 2 != lastpos)
                                middle = L.substring_both(newtext, lastpos,
                                             lastpos_byte,
                                             pos - 2, pos_byte - 2);
                            else
                                middle = Q.nil;
                            accum = L.concat3(accum, middle,
                                     F.substring(stringg,
                                             L.make_number(substart),
                                             L.make_number(subend)));
                            lastpos = pos;
                            lastpos_byte = pos_byte;
                        }
                        else if (delbackslash)
                        {
                            middle = L.substring_both(newtext, lastpos,
                                         lastpos_byte,
                                         pos - 1, pos_byte - 1);

                            accum = L.concat2(accum, middle);
                            lastpos = pos;
                            lastpos_byte = pos_byte;
                        }
                    }

                    if (pos != lastpos)
                        middle = L.substring_both(newtext, lastpos,
                                     lastpos_byte,
                                     pos, pos_byte);
                    else
                        middle = Q.nil;

                    newtext = L.concat2(accum, middle);
                }

                /* Do case substitution in NEWTEXT if desired.  */
                if (case_action == case_action_enum.all_caps)
                    newtext = F.upcase(newtext);
                else if (case_action == case_action_enum.cap_initial)
                    newtext = F.upcase_initials(newtext);

                return L.concat3(before, newtext, after);
            }

            /* Record point, then move (quietly) to the start of the match.  */
            if (L.PT() >= L.search_regs.end[sub])
                opoint = L.PT() - L.ZV;
            else if (L.PT() > L.search_regs.start[sub])
                opoint = L.search_regs.end[sub] - L.ZV;
            else
                opoint = L.PT();

            /* If we want non-literal replacement,
               perform substitution on the replacement string.  */
            if (L.NILP(literal))
            {
                int length = L.SBYTES(newtext);
                L.PtrEmulator<byte> substed;
                int substed_alloc_size, substed_len;
                bool buf_multibyte = !L.NILP(L.current_buffer.enable_multibyte_characters);
                bool str_multibyte = L.STRING_MULTIBYTE(newtext);
                LispObject rev_tbl;
                bool really_changed = false;

                rev_tbl = Q.nil;

                substed_alloc_size = length * 2 + 100;
                substed = new L.PtrEmulator<byte>(new byte[substed_alloc_size + 1]);
                substed_len = 0;

                /* Go thru NEWTEXT, producing the actual text to insert in
               SUBSTED while adjusting multibyteness to that of the current
               buffer.  */

                for (pos_byte = 0, pos = 0; pos_byte < length; )
                {
                    L.PtrEmulator<byte> str = new L.PtrEmulator<byte>(new byte[L.MAX_MULTIBYTE_LENGTH]);
                    L.PtrEmulator<byte> add_stuff = new L.PtrEmulator<byte>(); // NULL;
                    int add_len = 0;
                    int idx = -1;

                    if (str_multibyte)
                    {
                        L.FETCH_STRING_CHAR_ADVANCE_NO_CHECK(ref c, newtext, ref pos, ref pos_byte);
                        if (!buf_multibyte)
                            c = (int) L.multibyte_char_to_unibyte((uint) c, rev_tbl);
                    }
                    else
                    {
                        /* Note that we don't have to increment POS.  */
                        c = L.SREF(newtext, pos_byte++);
                        if (buf_multibyte)
                            c = (int) L.unibyte_char_to_multibyte((uint) c);
                    }

                    /* Either set ADD_STUFF and ADD_LEN to the text to put in SUBSTED,
                       or set IDX to a match index, which means put that part
                       of the buffer text into SUBSTED.  */

                    if (c == '\\')
                    {
                        really_changed = true;

                        if (str_multibyte)
                        {
                            L.FETCH_STRING_CHAR_ADVANCE_NO_CHECK(ref c, newtext,
                                                ref pos, ref pos_byte);
                            if (!buf_multibyte && !L.ASCII_CHAR_P((uint) c))
                                c = (int) L.multibyte_char_to_unibyte((uint) c, rev_tbl);
                        }
                        else
                        {
                            c = L.SREF(newtext, pos_byte++);
                            if (buf_multibyte)
                                c = (int) L.unibyte_char_to_multibyte((uint) c);
                        }

                        if (c == '&')
                            idx = sub;
                        else if (c >= '1' && c <= '9' && c <= L.search_regs.num_regs + '0')
                        {
                            if (L.search_regs.start[c - '0'] >= 1)
                                idx = c - '0';
                        }
                        else if (c == '\\')
                        {
                            add_len = 1; add_stuff = new L.PtrEmulator<byte>(System.Text.Encoding.UTF8.GetBytes("\\"));
                        }
                        else
                        {
                            // xfree(substed);
                            L.error("Invalid use of `\\' in replacement text");
                        }
                    }
                    else
                    {
                        add_len = L.CHAR_STRING((uint) c, str.Collection);
                        add_stuff = str;
                    }

                    /* If we want to copy part of a previous match,
                       set up ADD_STUFF and ADD_LEN to point to it.  */
                    if (idx >= 0)
                    {
                        int begbyte = L.CHAR_TO_BYTE(L.search_regs.start[idx]);
                        add_len = L.CHAR_TO_BYTE(L.search_regs.end[idx]) - begbyte;
                        if (L.search_regs.start[idx] < L.GPT && L.GPT < L.search_regs.end[idx])
                            L.move_gap(L.search_regs.start[idx]);
                        add_stuff = L.BYTE_POS_ADDR(begbyte);
                    }

                    /* Now the stuff we want to add to SUBSTED
                       is invariably ADD_LEN bytes starting at ADD_STUFF.  */

                    /* Make sure SUBSTED is big enough.  */
                    if (substed_len + add_len >= substed_alloc_size)
                    {
                        substed_alloc_size = substed_len + add_len + 500;
                        substed.Resize(substed_alloc_size + 1);
                    }

                    /* Now add to the end of SUBSTED.  */
                    if (add_stuff)
                    {
                        L.PtrEmulator<byte>.bcopy(add_stuff, new L.PtrEmulator<byte>(substed, substed_len), add_len);
                        // bcopy(add_stuff, substed + substed_len, add_len);
                        substed_len += add_len;
                    }
                }

                if (really_changed)
                {
                    if (buf_multibyte)
                    {
                        int nchars = L.multibyte_chars_in_text(substed.Collection, substed_len);

                        newtext = L.make_multibyte_string(substed.Collection, nchars, substed_len);
                    }
                    else
                        newtext = L.make_unibyte_string(substed.Collection, substed_len);
                }
            }

            /* Replace the old text with the new in the cleanest possible way.  */
            L.replace_range(L.search_regs.start[sub], L.search_regs.end[sub],
                   newtext, true, 0, true);
            newpoint = L.search_regs.start[sub] + L.SCHARS(newtext);

            if (case_action == case_action_enum.all_caps)
                F.upcase_region(L.make_number(L.search_regs.start[sub]),
                        L.make_number(newpoint));
            else if (case_action == case_action_enum.cap_initial)
                F.upcase_initials_region(L.make_number(L.search_regs.start[sub]),
                             L.make_number(newpoint));

            /* Adjust search data for this change.  */
            {
                int oldend = L.search_regs.end[sub];
                int oldstart = L.search_regs.start[sub];
                int change = newpoint - L.search_regs.end[sub];
                int i;

                for (i = 0; i < L.search_regs.num_regs; i++)
                {
                    if (L.search_regs.start[i] >= oldend)
                        L.search_regs.start[i] += change;
                    else if (L.search_regs.start[i] > oldstart)
                        L.search_regs.start[i] = oldstart;
                    if (L.search_regs.end[i] >= oldend)
                        L.search_regs.end[i] += change;
                    else if (L.search_regs.end[i] > oldstart)
                        L.search_regs.end[i] = oldstart;
                }
            }

            /* Put point back where it was in the text.  */
            if (opoint <= 0)
                L.TEMP_SET_PT(opoint + L.ZV);
            else
                L.TEMP_SET_PT(opoint);

            /* Now move point "officially" to the start of the inserted replacement.  */
            L.move_if_not_intangible(newpoint);

            return Q.nil;
        }

        /* Quote a string to inactivate reg-expr chars */
        public static LispObject regexp_quote(LispObject stringg)
        {
            L.PtrEmulator<byte> inn, outt, end;
            L.PtrEmulator<byte> temp;
            int backslashes_added = 0;

            L.CHECK_STRING(stringg);

            temp = new L.PtrEmulator<byte>(new byte[L.SBYTES(stringg) * 2]);

            /* Now copy the data into the new string, inserting escapes. */

            inn = L.SDATA(stringg);
            end = inn + L.SBYTES(stringg);
            outt = temp;

            for (; inn != end; inn++)
            {
                if (inn.Value == '['
                || inn.Value == '*' || inn.Value == '.' || inn.Value == '\\'
                || inn.Value == '?' || inn.Value == '+'
                || inn.Value == '^' || inn.Value == '$')
                {
                    outt.SetValueAndInc((byte) '\\');
                    backslashes_added++;
                }
                outt.SetValueAndInc(inn.Value);
            }

            return L.make_specified_string(temp.Collection,
                          L.SCHARS(stringg) + backslashes_added,
                          outt - temp,
                          L.STRING_MULTIBYTE(stringg));
        }
    }
}