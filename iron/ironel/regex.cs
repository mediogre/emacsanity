namespace IronElisp
{
    /* The following bits are used to determine the regexp syntax we
       recognize.  The set/not-set meanings where historically chosen so
       that Emacs syntax had the value 0.
       The bits are given in alphabetical order, and
       the definitions shifted by one from the previous bit; thus, when we
       add or remove a bit, only one other definition need change.  */
    using reg_syntax_t = System.UInt64;

    /* Type for byte offsets within the string.  POSIX mandates this.  */
    using regoff_t = System.Int32;

    using re_wchar_t = System.Int32;

    using RE_TRANSLATE_TYPE = LispObject;

    using regex_t = L.re_pattern_buffer;

    /* Type of source-pattern and string chars.  */
    using re_char = System.Byte;

/* Since offsets can go either forwards or backwards, this type needs to
   be able to hold values from -(MAX_BUF_SIZE - 1) to MAX_BUF_SIZE - 1.  */
/* int may be not enough when sizeof(int) == 2.  */
  using pattern_offset_t = System.Int64;

/* But patterns can have more than `MAX_REGNUM' registers.  We just
   ignore the excess.  */
 using regnum_t = System.Int32;


    /* Character classes.  */
    public enum re_wctype_t
    {
        RECC_ERROR = 0,
        RECC_ALNUM, RECC_ALPHA, RECC_WORD,
        RECC_GRAPH, RECC_PRINT,
        RECC_LOWER, RECC_UPPER,
        RECC_PUNCT, RECC_CNTRL,
        RECC_DIGIT, RECC_XDIGIT,
        RECC_BLANK, RECC_SPACE,
        RECC_MULTIBYTE, RECC_NONASCII,
        RECC_ASCII, RECC_UNIBYTE
    }

    public partial class L
    {
        /* Since we have one byte reserved for the register number argument to
           {start,stop}_memory, the maximum number of groups we can report
           things about is what fits in that byte.  */
        public const int MAX_REGNUM = 255;

        /* If this bit is not set, then \ inside a bracket expression is literal.
           If set, then such a \ quotes the following character.  */
        public const ulong RE_BACKSLASH_ESCAPE_IN_LISTS = 1;

        /* If this bit is not set, then + and ? are operators, and \+ and \? are
             literals.
           If set, then \+ and \? are operators and + and ? are literals.  */
        public const ulong RE_BK_PLUS_QM = (RE_BACKSLASH_ESCAPE_IN_LISTS << 1);

        /* If this bit is set, then character classes are supported.  They are:
             [:alpha:], [:upper:], [:lower:],  [:digit:], [:alnum:], [:xdigit:],
             [:space:], [:print:], [:punct:], [:graph:], and [:cntrl:].
           If not set, then character classes are not supported.  */
        public const ulong RE_CHAR_CLASSES = (RE_BK_PLUS_QM << 1);

        /* If this bit is set, then ^ and $ are always anchors (outside bracket
             expressions, of course).
           If this bit is not set, then it depends:
                ^  is an anchor if it is at the beginning of a regular
                   expression or after an open-group or an alternation operator;
                $  is an anchor if it is at the end of a regular expression, or
                   before a close-group or an alternation operator.

           This bit could be (re)combined with RE_CONTEXT_INDEP_OPS, because
           POSIX draft 11.2 says that * etc. in leading positions is undefined.
           We already implemented a previous draft which made those constructs
           invalid, though, so we haven't changed the code back.  */
        public const ulong RE_CONTEXT_INDEP_ANCHORS = (RE_CHAR_CLASSES << 1);

        /* If this bit is set, then special characters are always special
             regardless of where they are in the pattern.
           If this bit is not set, then special characters are special only in
             some contexts; otherwise they are ordinary.  Specifically,
             * + ? and intervals are only special when not after the beginning,
             open-group, or alternation operator.  */
        public const ulong RE_CONTEXT_INDEP_OPS = (RE_CONTEXT_INDEP_ANCHORS << 1);

        /* If this bit is set, then *, +, ?, and { cannot be first in an re or
             immediately after an alternation or begin-group operator.  */
        public const ulong RE_CONTEXT_INVALID_OPS = (RE_CONTEXT_INDEP_OPS << 1);

        /* If this bit is set, then . matches newline.
           If not set, then it doesn't.  */
        public const ulong RE_DOT_NEWLINE = (RE_CONTEXT_INVALID_OPS << 1);

        /* If this bit is set, then . doesn't match NUL.
           If not set, then it does.  */
        public const ulong RE_DOT_NOT_NULL = (RE_DOT_NEWLINE << 1);

        /* If this bit is set, nonmatching lists [^...] do not match newline.
           If not set, they do.  */
        public const ulong RE_HAT_LISTS_NOT_NEWLINE = (RE_DOT_NOT_NULL << 1);

        /* If this bit is set, either \{...\} or {...} defines an
             interval, depending on RE_NO_BK_BRACES.
           If not set, \{, \}, {, and } are literals.  */
        public const ulong RE_INTERVALS = (RE_HAT_LISTS_NOT_NEWLINE << 1);

        /* If this bit is set, +, ? and | aren't recognized as operators.
           If not set, they are.  */
        public const ulong RE_LIMITED_OPS = (RE_INTERVALS << 1);

        /* If this bit is set, newline is an alternation operator.
           If not set, newline is literal.  */
        public const ulong RE_NEWLINE_ALT = (RE_LIMITED_OPS << 1);

        /* If this bit is set, then `{...}' defines an interval, and \{ and \}
             are literals.
          If not set, then `\{...\}' defines an interval.  */
        public const ulong RE_NO_BK_BRACES = (RE_NEWLINE_ALT << 1);

        /* If this bit is set, (...) defines a group, and \( and \) are literals.
           If not set, \(...\) defines a group, and ( and ) are literals.  */
        public const ulong RE_NO_BK_PARENS = (RE_NO_BK_BRACES << 1);

        /* If this bit is set, then \<digit> matches <digit>.
           If not set, then \<digit> is a back-reference.  */
        public const ulong RE_NO_BK_REFS = (RE_NO_BK_PARENS << 1);

        /* If this bit is set, then | is an alternation operator, and \| is literal.
           If not set, then \| is an alternation operator, and | is literal.  */
        public const ulong RE_NO_BK_VBAR = (RE_NO_BK_REFS << 1);

        /* If this bit is set, then an ending range point collating higher
             than the starting range point, as in [z-a], is invalid.
           If not set, then when ending range point collates higher than the
             starting range point, the range is ignored.  */
        public const ulong RE_NO_EMPTY_RANGES = (RE_NO_BK_VBAR << 1);

        /* If this bit is set, then an unmatched ) is ordinary.
           If not set, then an unmatched ) is invalid.  */
        public const ulong RE_UNMATCHED_RIGHT_PAREN_ORD = (RE_NO_EMPTY_RANGES << 1);

        /* If this bit is set, succeed as soon as we match the whole pattern,
           without further backtracking.  */
        public const ulong RE_NO_POSIX_BACKTRACKING = (RE_UNMATCHED_RIGHT_PAREN_ORD << 1);

        /* If this bit is set, do not process the GNU regex operators.
           If not set, then the GNU regex operators are recognized. */
        public const ulong RE_NO_GNU_OPS = (RE_NO_POSIX_BACKTRACKING << 1);

        /* If this bit is set, then *?, +? and ?? match non greedily. */
        public const ulong RE_FRUGAL = (RE_NO_GNU_OPS << 1);

        /* If this bit is set, then (?:...) is treated as a shy group.  */
        public const ulong RE_SHY_GROUPS = (RE_FRUGAL << 1);

        /* If this bit is set, ^ and $ only match at beg/end of buffer.  */
        public const ulong RE_NO_NEWLINE_ANCHOR = (RE_SHY_GROUPS << 1);

        /* If this bit is set, turn on internal regex debugging.
           If not set, and debugging was on, turn it off.
           This only works if regex.c is compiled -DDEBUG.
           We define this bit always, so that all that's needed to turn on
           debugging is to recompile regex.c; the calling code can always have
           this bit set, and it won't affect anything in the normal case. */
        public const ulong RE_DEBUG = (RE_NO_NEWLINE_ANCHOR << 1);

        /* This global variable defines the particular regexp syntax to use (for
           some interfaces).  When a regexp is compiled, the syntax used is
           stored in the pattern buffer, so changing this does not affect
           already-compiled regexps.  */
        public static reg_syntax_t re_syntax_options;

        /* In Emacs, this is the string or buffer in which we
           are matching.  It is used for looking up syntax properties.  */
        public static LispObject re_match_object;

        /* Define combinations of the above bits for the standard possibilities.
           (The [[[ comments delimit what gets put into the Texinfo file, so
           don't delete them!)  */
        /* [[[begin syntaxes]]] */
        public const ulong RE_SYNTAX_EMACS	= (RE_CHAR_CLASSES | RE_INTERVALS | RE_SHY_GROUPS | RE_FRUGAL);

        public const ulong RE_SYNTAX_AWK =							
            (RE_BACKSLASH_ESCAPE_IN_LISTS   | RE_DOT_NOT_NULL			
             | RE_NO_BK_PARENS              | RE_NO_BK_REFS			
             | RE_NO_BK_VBAR                | RE_NO_EMPTY_RANGES			
             | RE_DOT_NEWLINE		  | RE_CONTEXT_INDEP_ANCHORS		
             | RE_UNMATCHED_RIGHT_PAREN_ORD | RE_NO_GNU_OPS);

        public const ulong RE_SYNTAX_GNU_AWK =						
            ((RE_SYNTAX_POSIX_EXTENDED | RE_BACKSLASH_ESCAPE_IN_LISTS | RE_DEBUG)	
             & ~(RE_DOT_NOT_NULL | RE_INTERVALS | RE_CONTEXT_INDEP_OPS));

        public const ulong RE_SYNTAX_POSIX_AWK =
            (RE_SYNTAX_POSIX_EXTENDED | RE_BACKSLASH_ESCAPE_IN_LISTS		
             | RE_INTERVALS	    | RE_NO_GNU_OPS); 

        public const ulong RE_SYNTAX_GREP = 
            (RE_BK_PLUS_QM              | RE_CHAR_CLASSES				
             | RE_HAT_LISTS_NOT_NEWLINE | RE_INTERVALS				
             | RE_NEWLINE_ALT); 

        public const ulong RE_SYNTAX_EGREP = 
            (RE_CHAR_CLASSES        | RE_CONTEXT_INDEP_ANCHORS			
             | RE_CONTEXT_INDEP_OPS | RE_HAT_LISTS_NOT_NEWLINE			
             | RE_NEWLINE_ALT       | RE_NO_BK_PARENS				
             | RE_NO_BK_VBAR); 

        public const ulong RE_SYNTAX_POSIX_EGREP = (RE_SYNTAX_EGREP | RE_INTERVALS | RE_NO_BK_BRACES); 

        /* P1003.2/D11.2, section 4.20.7.1, lines 5078ff.  */
        public const ulong RE_SYNTAX_ED = RE_SYNTAX_POSIX_BASIC; 

        public const ulong RE_SYNTAX_SED = RE_SYNTAX_POSIX_BASIC; 

        /* Syntax bits common to both basic and extended POSIX regex syntax.  */
        public const ulong _RE_SYNTAX_POSIX_COMMON =
            (RE_CHAR_CLASSES | RE_DOT_NEWLINE      | RE_DOT_NOT_NULL		
             | RE_INTERVALS  | RE_NO_EMPTY_RANGES); 

        public const ulong RE_SYNTAX_POSIX_BASIC = (_RE_SYNTAX_POSIX_COMMON | RE_BK_PLUS_QM); 

        /* Differs from ..._POSIX_BASIC only in that RE_BK_PLUS_QM becomes
           RE_LIMITED_OPS, i.e., \? \+ \| are not recognized.  Actually, this
           isn't minimal, since other operators, such as \`, aren't disabled.  */
        public const ulong RE_SYNTAX_POSIX_MINIMAL_BASIC =
            (_RE_SYNTAX_POSIX_COMMON | RE_LIMITED_OPS); 

        public const ulong RE_SYNTAX_POSIX_EXTENDED = 
            (_RE_SYNTAX_POSIX_COMMON  | RE_CONTEXT_INDEP_ANCHORS
             | RE_CONTEXT_INDEP_OPS   | RE_NO_BK_BRACES		
             | RE_NO_BK_PARENS        | RE_NO_BK_VBAR			
             | RE_CONTEXT_INVALID_OPS | RE_UNMATCHED_RIGHT_PAREN_ORD); 

        /* Differs from ..._POSIX_EXTENDED in that RE_CONTEXT_INDEP_OPS is
           removed and RE_NO_BK_REFS is added.  */
        public const ulong RE_SYNTAX_POSIX_MINIMAL_EXTENDED =
            (_RE_SYNTAX_POSIX_COMMON  | RE_CONTEXT_INDEP_ANCHORS			
             | RE_CONTEXT_INVALID_OPS | RE_NO_BK_BRACES				
             | RE_NO_BK_PARENS        | RE_NO_BK_REFS				
             | RE_NO_BK_VBAR	    | RE_UNMATCHED_RIGHT_PAREN_ORD); 
        /* [[[end syntaxes]]] */

        /* Maximum number of duplicates an interval can allow.  Some systems
           (erroneously) define this in other header files, but we want our
           value, so remove any previous define.  */
        /* If sizeof(int) == 2, then ((1 << 15) - 1) overflows.  */
        public const int RE_DUP_MAX = 0x7fff;

        /* Bits used to implement the multibyte-part of the various character classes
   such as [:alnum:] in a charset's range table.  */
        public const int BIT_WORD = 0x1;
        public const int BIT_LOWER = 0x2;
        public const int BIT_PUNCT = 0x4;
        public const int BIT_SPACE = 0x8;
        public const int BIT_UPPER = 0x10;
        public const int BIT_MULTIBYTE = 0x20;

        /* POSIX `cflags' bits (i.e., information for `regcomp').  */

        /* If this bit is set, then use extended regular expression syntax.
           If not set, then use basic regular expression syntax.  */
        public const ulong REG_EXTENDED = 1; 

        /* If this bit is set, then ignore case when matching.
           If not set, then case is significant.  */
        public const ulong REG_ICASE = (REG_EXTENDED << 1); 

        /* If this bit is set, then anchors do not match at newline
           characters in the string.
           If not set, then anchors do match at newlines.  */
        public const ulong REG_NEWLINE = (REG_ICASE << 1); 

        /* If this bit is set, then report only success or fail in regexec.
           If not set, then returns differ between not matching and errors.  */
        public const ulong REG_NOSUB = (REG_NEWLINE << 1); 


        /* POSIX `eflags' bits (i.e., information for regexec).  */

        /* If this bit is set, then the beginning-of-line operator doesn't match
           the beginning of the string (presumably because it's not the
           beginning of a line).
           If not set, then the beginning-of-line operator does match the
           beginning of the string.  */
        public const ulong REG_NOTBOL = 1; 

        /* Like REG_NOTBOL, except for the end-of-line.  */
        public const ulong REG_NOTEOL = (1 << 1);

        /* If any error codes are removed, changed, or added, update the
           `re_error_msg' table in regex.c.  */
        public enum reg_errcode_t
        {
            REG_NOERROR = 0,	/* Success.  */
            REG_NOMATCH,		/* Didn't find a match (for regexec).  */

            /* POSIX regcomp return error codes.  (In the order listed in the
               standard.)  */
            REG_BADPAT,		/* Invalid pattern.  */
            REG_ECOLLATE,		/* Not implemented.  */
            REG_ECTYPE,		/* Invalid character class name.  */
            REG_EESCAPE,		/* Trailing backslash.  */
            REG_ESUBREG,		/* Invalid back reference.  */
            REG_EBRACK,		/* Unmatched left bracket.  */
            REG_EPAREN,		/* Parenthesis imbalance.  */
            REG_EBRACE,		/* Unmatched \{.  */
            REG_BADBR,		/* Invalid contents of \{\}.  */
            REG_ERANGE,		/* Invalid range end.  */
            REG_ESPACE,		/* Ran out of memory.  */
            REG_BADRPT,		/* No preceding re for repetition op.  */

            /* Error codes we've added.  */
            REG_EEND,		/* Premature end.  */
            REG_ESIZE,		/* Compiled pattern bigger than 2^16 bytes.  */
            REG_ERPAREN,		/* Unmatched ) or \); not returned from regcomp.  */
            REG_ERANGEX		/* Range striding over charsets.  */
        }

        /* Return a bit-pattern to use in the range-table bits to match multibyte
           chars of class CC.  */
        public static int re_wctype_to_bit(re_wctype_t cc)
        {
            switch (cc)
            {
                case re_wctype_t.RECC_NONASCII:
                case re_wctype_t.RECC_PRINT:
                case re_wctype_t.RECC_GRAPH:
                case re_wctype_t.RECC_MULTIBYTE: return BIT_MULTIBYTE;
                case re_wctype_t.RECC_ALPHA:
                case re_wctype_t.RECC_ALNUM:
                case re_wctype_t.RECC_WORD: return BIT_WORD;
                case re_wctype_t.RECC_LOWER: return BIT_LOWER;
                case re_wctype_t.RECC_UPPER: return BIT_UPPER;
                case re_wctype_t.RECC_PUNCT: return BIT_PUNCT;
                case re_wctype_t.RECC_SPACE: return BIT_SPACE;
                case re_wctype_t.RECC_ASCII:
                case re_wctype_t.RECC_DIGIT:
                case re_wctype_t.RECC_XDIGIT:
                case re_wctype_t.RECC_CNTRL:
                case re_wctype_t.RECC_BLANK:
                case re_wctype_t.RECC_UNIBYTE:
                case re_wctype_t.RECC_ERROR: return 0;
                default:
                    abort();
                    return 0;
            }
        }

        public static int RE_TRANSLATE(LispObject TBL, int C)
        {
            return CHAR_TABLE_TRANSLATE(TBL, C);
        }

        public static bool RE_TRANSLATE_P(LispObject TBL)
        {
            return (XINT(TBL) != 0);
        }

        /* Converts the pointer to the char to BEG-based offset from the start.  */
        public static int PTR_TO_OFFSET(PtrEmulator<re_char> d, int size1, PtrEmulator<re_char> string1, PtrEmulator<re_char> string2)
        {
            return POS_AS_IN_BUFFER(POINTER_TO_OFFSET(d, size1, string1, string2));
        }

        public static int POS_AS_IN_BUFFER(int p)
        {
            return p + ((NILP(re_match_object) || BUFFERP(re_match_object)) ? 1 : 0);
        }

        public class re_pattern_buffer
        {
            /* [[[begin pattern_buffer]]] */
            /* Space that holds the compiled pattern.  It is declared as
                  `unsigned char *' because its elements are
                   sometimes used as array indexes.  */
            public byte[] buffer;

            /* Number of bytes to which `buffer' points.  */
            public int allocated;

            /* Number of bytes actually used in `buffer'.  */
            public int used;

            /* Syntax setting with which the pattern was compiled.  */
            public reg_syntax_t syntax;

            /* Pointer to a fastmap, if any, otherwise zero.  re_search uses
               the fastmap, if there is one, to skip over impossible
               starting points for matches.  */
            public byte[] fastmap;

            /* Either a translate table to apply to all characters before
               comparing them, or zero for no translation.  The translation
               is applied to a pattern when it is compiled and to a string
               when it is matched.  */
            public RE_TRANSLATE_TYPE translate;

            /* Number of subexpressions found by the compiler.  */
            public int re_nsub;

            /* Zero if this pattern cannot match the empty string, one else.
               Well, in truth it's used only in `re_search_2', to see
               whether or not we should use the fastmap, so we don't set
               this absolutely perfectly; see `re_compile_fastmap'.  */
            public bool can_be_null;

            /* If REGS_UNALLOCATED, allocate space in the `regs' structure
                 for `max (RE_NREGS, re_nsub + 1)' groups.
               If REGS_REALLOCATE, reallocate space if necessary.
               If REGS_FIXED, use what's there.  */
            public const int REGS_UNALLOCATED = 0;
            public const int REGS_REALLOCATE = 1;
            public const int REGS_FIXED = 2;
            public int regs_allocated;

            /* Set to zero when `regex_compile' compiles a pattern; set to one
               by `re_compile_fastmap' if it updates the fastmap.  */
            public bool fastmap_accurate;

            /* If set, `re_match_2' does not return information about
               subexpressions.  */
            public bool no_sub;

            /* If set, a beginning-of-line anchor doesn't match at the
               beginning of the string.  */
            public bool not_bol;

            /* Similarly for an end-of-line anchor.  */
            public bool not_eol;

            /* If true, the compilation of the pattern had to look up the syntax table,
               so the compiled pattern is only valid for the current syntax table.  */
            public bool used_syntax;

            /* If true, multi-byte form in the regexp pattern should be
               recognized as a multibyte character.  */
            public bool multibyte;

            /* If true, multi-byte form in the target of match should be
               recognized as a multibyte character.  */
            public bool target_multibyte;

            /* Charset of unibyte characters at compiling time. */
            public int charset_unibyte;

            /* [[[end pattern_buffer]]] */
        }

        /* This is the structure we store register match data in.  See
           regex.texinfo for a full description of what registers match.  */
        public class re_registers
        {
            public uint num_regs;
            public regoff_t[] start;
            public regoff_t[] end;
        }

        /* These are the command codes that appear in compiled regular
           expressions.  Some opcodes are followed by argument bytes.  A
           command code can specify any interpretation whatsoever for its
           arguments.  Zero bytes may appear in the compiled regular expression.  */
        public enum re_opcode_t
        {
            no_op = 0,

            /* Succeed right away--no more backtracking.  */
            succeed,

            /* Followed by one byte giving n, then by n literal bytes.  */
            exactn,

            /* Matches any (more or less) character.  */
            anychar,

            /* Matches any one char belonging to specified set.  First
               following byte is number of bitmap bytes.  Then come bytes
               for a bitmap saying which chars are in.  Bits in each byte
               are ordered low-bit-first.  A character is in the set if its
               bit is 1.  A character too large to have a bit in the map is
               automatically not in the set.

               If the length byte has the 0x80 bit set, then that stuff
               is followed by a range table:
                   2 bytes of flags for character sets (low 8 bits, high 8 bits)
                   See RANGE_TABLE_WORK_BITS below.
                   2 bytes, the number of pairs that follow (upto 32767)
                   pairs, each 2 multibyte characters,
                   each multibyte character represented as 3 bytes.  */
            charset,

            /* Same parameters as charset, but match any character that is
               not one of those specified.  */
            charset_not,

            /* Start remembering the text that is matched, for storing in a
               register.  Followed by one byte with the register number, in
               the range 0 to one less than the pattern buffer's re_nsub
               field.  */
            start_memory,

            /* Stop remembering the text that is matched and store it in a
               memory register.  Followed by one byte with the register
               number, in the range 0 to one less than `re_nsub' in the
               pattern buffer.  */
            stop_memory,

            /* Match a duplicate of something remembered. Followed by one
               byte containing the register number.  */
            duplicate,

            /* Fail unless at beginning of line.  */
            begline,

            /* Fail unless at end of line.  */
            endline,

            /* Succeeds if at beginning of buffer (if emacs) or at beginning
               of string to be matched (if not).  */
            begbuf,

            /* Analogously, for end of buffer/string.  */
            endbuf,

            /* Followed by two byte relative address to which to jump.  */
            jump,

            /* Followed by two-byte relative address of place to resume at
               in case of failure.  */
            on_failure_jump,

            /* Like on_failure_jump, but pushes a placeholder instead of the
               current string position when executed.  */
            on_failure_keep_string_jump,

            /* Just like `on_failure_jump', except that it checks that we
               don't get stuck in an infinite loop (matching an empty string
               indefinitely).  */
            on_failure_jump_loop,

            /* Just like `on_failure_jump_loop', except that it checks for
               a different kind of loop (the kind that shows up with non-greedy
               operators).  This operation has to be immediately preceded
               by a `no_op'.  */
            on_failure_jump_nastyloop,

            /* A smart `on_failure_jump' used for greedy * and + operators.
               It analyses the loop before which it is put and if the
               loop does not require backtracking, it changes itself to
               `on_failure_keep_string_jump' and short-circuits the loop,
               else it just defaults to changing itself into `on_failure_jump'.
               It assumes that it is pointing to just past a `jump'.  */
            on_failure_jump_smart,

            /* Followed by two-byte relative address and two-byte number n.
               After matching N times, jump to the address upon failure.
               Does not work if N starts at 0: use on_failure_jump_loop
               instead.  */
            succeed_n,

            /* Followed by two-byte relative address, and two-byte number n.
               Jump to the address N times, then fail.  */
            jump_n,

            /* Set the following two-byte relative address to the
               subsequent two-byte number.  The address *includes* the two
               bytes of number.  */
            set_number_at,

            wordbeg,	/* Succeeds if at word beginning.  */
            wordend,	/* Succeeds if at word end.  */

            wordbound,	/* Succeeds if at a word boundary.  */
            notwordbound,	/* Succeeds if not at a word boundary.  */

            symbeg,       /* Succeeds if at symbol beginning.  */
            symend,       /* Succeeds if at symbol end.  */

            /* Matches any character whose syntax is specified.  Followed by
               a byte which contains a syntax code, e.g., Sword.  */
            syntaxspec,

            /* Matches any character whose syntax is not that specified.  */
            notsyntaxspec

            , before_dot,	/* Succeeds if before point.  */
            at_dot,	/* Succeeds if at point.  */
            after_dot,	/* Succeeds if after point.  */

            /* Matches any character whose category-set contains the specified
               category.  The operator is followed by a byte which contains a
               category code (mnemonic ASCII character).  */
            categoryspec,

            /* Matches any character whose category-set does not contain the
               specified category.  The operator is followed by a byte which
               contains the category code (mnemonic ASCII character).  */
            notcategoryspec
        }

        /* If `regs_allocated' is REGS_UNALLOCATED in the pattern buffer,
           `re_match_2' returns information about at least this many registers
           the first time a `regs' structure is passed.  */
        public const int RE_NREGS = 30;

        /* POSIX specification for registers.  Aside from the different names than
           `re_registers', POSIX uses an array of structures, instead of a
           structure of arrays.  */
        public class regmatch_t
        {
            regoff_t rm_so;  /* Byte offset from string's start to substring's start.  */
            regoff_t rm_eo;  /* Byte offset from string's start to substring's end.  */
        }

        public const int CHAR_CLASS_MAX_LENGTH = 9; /* Namely, `multibyte'.  */

        public static bool STREQ(byte[] a, string b)
        {
            for (int i = 0; i < b.Length; i++)
            {
                if (b[i] != a[i])
                    return false;
            }

            return true;
        }

        /* Map a string to the char class it names (if any).  */
        public static re_wctype_t re_wctype(byte[] str)
        {
            if (STREQ(str, "alnum")) return re_wctype_t.RECC_ALNUM;
            else if (STREQ(str, "alpha")) return re_wctype_t.RECC_ALPHA;
            else if (STREQ(str, "word")) return re_wctype_t.RECC_WORD;
            else if (STREQ(str, "ascii")) return re_wctype_t.RECC_ASCII;
            else if (STREQ(str, "nonascii")) return re_wctype_t.RECC_NONASCII;
            else if (STREQ(str, "graph")) return re_wctype_t.RECC_GRAPH;
            else if (STREQ(str, "lower")) return re_wctype_t.RECC_LOWER;
            else if (STREQ(str, "print")) return re_wctype_t.RECC_PRINT;
            else if (STREQ(str, "punct")) return re_wctype_t.RECC_PUNCT;
            else if (STREQ(str, "space")) return re_wctype_t.RECC_SPACE;
            else if (STREQ(str, "upper")) return re_wctype_t.RECC_UPPER;
            else if (STREQ(str, "unibyte")) return re_wctype_t.RECC_UNIBYTE;
            else if (STREQ(str, "multibyte")) return re_wctype_t.RECC_MULTIBYTE;
            else if (STREQ(str, "digit")) return re_wctype_t.RECC_DIGIT;
            else if (STREQ(str, "xdigit")) return re_wctype_t.RECC_XDIGIT;
            else if (STREQ(str, "cntrl")) return re_wctype_t.RECC_CNTRL;
            else if (STREQ(str, "blank")) return re_wctype_t.RECC_BLANK;
            else return 0;
        }

        /* True if CH is in the char class CC.  */
        public static bool re_iswctype(uint ch, re_wctype_t cc)
        {
            switch (cc)
            {
                case re_wctype_t.RECC_ALNUM: return ISALNUM(ch);
                case re_wctype_t.RECC_ALPHA: return ISALPHA(ch);
                case re_wctype_t.RECC_BLANK: return ISBLANK(ch);
                case re_wctype_t.RECC_CNTRL: return ISCNTRL(ch);
                case re_wctype_t.RECC_DIGIT: return ISDIGIT(ch);
                case re_wctype_t.RECC_GRAPH: return ISGRAPH(ch);
                case re_wctype_t.RECC_LOWER: return ISLOWER(ch);
                case re_wctype_t.RECC_PRINT: return ISPRINT(ch);
                case re_wctype_t.RECC_PUNCT: return ISPUNCT(ch);
                case re_wctype_t.RECC_SPACE: return ISSPACE(ch);
                case re_wctype_t.RECC_UPPER: return ISUPPER(ch);
                case re_wctype_t.RECC_XDIGIT: return ISXDIGIT(ch);
                case re_wctype_t.RECC_ASCII: return IS_REAL_ASCII(ch);
                case re_wctype_t.RECC_NONASCII: return !IS_REAL_ASCII(ch);
                case re_wctype_t.RECC_UNIBYTE: return ISUNIBYTE(ch);
                case re_wctype_t.RECC_MULTIBYTE: return !ISUNIBYTE(ch);
                case re_wctype_t.RECC_WORD: return ISWORD(ch);
                case re_wctype_t.RECC_ERROR: return false;
                default:
                    abort();
                    return false;
            }
        }

        /* 1 if C is an ASCII character.  */
        public static bool IS_REAL_ASCII(uint c)
        {
            return ((c) < 128);
        }

        /* 1 if C is a unibyte character.  */
        public static bool ISUNIBYTE(uint c)
        {
            return (SINGLE_BYTE_CHAR_P((c)));
        }

        /* The Emacs definitions should not be directly affected by locales.  */

        /* In Emacs, these are only used for single-byte characters.  */
        public static bool ISDIGIT(uint c)
        {
            return ((c) >= '0' && (c) <= '9');
        }

        public static bool ISCNTRL(uint c)
        {
            return ((c) < ' ');
        }

        public static bool ISXDIGIT(uint c)
        {
            return (((c) >= '0' && (c) <= '9')		
		     || ((c) >= 'a' && (c) <= 'f')	
		     || ((c) >= 'A' && (c) <= 'F'));
        }

        /* This is only used for single-byte characters.  */
        public static bool ISBLANK(uint c)
        {
            return ((c) == ' ' || (c) == '\t');
        }

        /* The rest must handle multibyte characters.  */
        public static bool ISGRAPH(uint c)
        {
            return (SINGLE_BYTE_CHAR_P(c)				
		    ? (c) > ' ' && !((c) >= 127 && (c) <= 159)	
		    : true);
        }

        public static bool ISPRINT(uint c)
        {
            return (SINGLE_BYTE_CHAR_P(c)				
		    ? (c) >= ' ' && !((c) >= 127 && (c) <= 159)	
		    : true);
        }

        public static bool ISALNUM(uint c)
        {
            return (IS_REAL_ASCII(c)			
		    ? (((c) >= 'a' && (c) <= 'z')	
		       || ((c) >= 'A' && (c) <= 'Z')	
		       || ((c) >= '0' && (c) <= '9'))	
		    : SYNTAX (c) == syntaxcode.Sword);
        }

        public static bool ISALPHA(uint c)
        {
            return (IS_REAL_ASCII(c)			
		    ? (((c) >= 'a' && (c) <= 'z')	
		       || ((c) >= 'A' && (c) <= 'Z'))	
		    : SYNTAX (c) == syntaxcode.Sword);
        }

        public static bool ISLOWER(uint c)
        {
            return (LOWERCASEP(c));
        }

        public static bool ISPUNCT(uint c)
        {
            return (IS_REAL_ASCII(c)				
		    ? ((c) > ' ' && (c) < 127			
		       && !(((c) >= 'a' && (c) <= 'z')		
		            || ((c) >= 'A' && (c) <= 'Z')	
		            || ((c) >= '0' && (c) <= '9')))	
		    : SYNTAX (c) != syntaxcode.Sword);
        }

        public static bool ISSPACE(uint c)
        {
            return (SYNTAX(c) == syntaxcode.Swhitespace);
        }

        public static bool ISUPPER(uint c)
        {
            return (UPPERCASEP(c));
        }

        public static bool ISWORD(uint c)
        {
            return (SYNTAX(c) == syntaxcode.Sword);
        }

        /* Specify the precise syntax of regexps for compilation.  This provides
   for compatibility for various utilities which historically have
   different, incompatible syntaxes.

   The argument SYNTAX is a bit mask comprised of the various bits
   defined in regex.h.  We return the old syntax.  */
        public static reg_syntax_t re_set_syntax (reg_syntax_t syntax)
        {
            reg_syntax_t ret = re_syntax_options;

            re_syntax_options = syntax;
            return ret;
        }

        /* Regexp to use to replace spaces, or NULL meaning don't.  */
        public static re_char[] whitespace_regexp;

        public static void re_set_whitespace_regexp(byte[] regexp)
        {
            whitespace_regexp = regexp;
        }

        /* This table gives an error message for each of the error codes listed
           in regex.h.  Obviously the order here has to be same as there.
           POSIX doesn't require that we do anything for REG_NOERROR,
           but why not be nice?  */
        public static string[] re_error_msgid = {
            "Success",	/* REG_NOERROR */
            "No match",	/* REG_NOMATCH */
            "Invalid regular expression", /* REG_BADPAT */
            "Invalid collation character", /* REG_ECOLLATE */
            "Invalid character class name", /* REG_ECTYPE */
            "Trailing backslash", /* REG_EESCAPE */
            "Invalid back reference", /* REG_ESUBREG */
            "Unmatched [ or [^",	/* REG_EBRACK */
            "Unmatched ( or \\(", /* REG_EPAREN */
            "Unmatched \\{", /* REG_EBRACE */
            "Invalid content of \\{\\}", /* REG_BADBR */
            "Invalid range end",	/* REG_ERANGE */
            "Memory exhausted", /* REG_ESPACE */
            "Invalid preceding regular expression", /* REG_BADRPT */
            "Premature end of regular expression", /* REG_EEND */
            "Regular expression too big", /* REG_ESIZE */
            "Unmatched ) or \\)", /* REG_ERPAREN */
            "Range striding over charsets" /* REG_ERANGEX  */
        };

        /* Filling in the work area of a range.  */

        /* Actually extend the space in WORK_AREA.  */
        public static void extend_range_table_work_area (range_table_work_area work_area)
        {
            work_area.allocated += 16;
            if (work_area.table != null)
                System.Array.Resize (ref work_area.table, work_area.allocated);
            else
                work_area.table = new int[work_area.allocated];
        }

        /* Make sure that WORK_AREA can hold more N multibyte characters.
           This is used only in set_image_of_range and set_image_of_range_1.
           It expects WORK_AREA to be a pointer.
           If it can't get the space, it returns from the surrounding function.  */
        public static void EXTEND_RANGE_TABLE(range_table_work_area work_area, int n)
        {
            if ((work_area.used + n) > work_area.allocated)
            {
                extend_range_table_work_area(work_area);
                if (work_area.table == null)
                    throw new RegexException(reg_errcode_t.REG_ESPACE);
            }
        }

        /* Set a range (RANGE_START, RANGE_END) to WORK_AREA.  */
        public static void SET_RANGE_TABLE_WORK_AREA(range_table_work_area work_area, int range_start, int range_end)
        {
            EXTEND_RANGE_TABLE(work_area, 2);
            work_area.table[work_area.used++] = range_start;
            work_area.table[work_area.used++] = range_end;		
        }

        public static void CLEAR_RANGE_TABLE_WORK_USED(range_table_work_area work_area)
        {
            work_area.used = 0;
            work_area.bits = 0;
        }

        public static int RANGE_TABLE_WORK_USED(range_table_work_area work_area)
        {
            return work_area.used;
        }

        public static int RANGE_TABLE_WORK_BITS(range_table_work_area work_area)
        {
            return work_area.bits;
        }

        public static int RANGE_TABLE_WORK_ELT(range_table_work_area work_area, int i)
        {
            return work_area.table[i];
        }

        /* Store characters in the range FROM to TO in the bitmap at B (for
           ASCII and unibyte characters) and WORK_AREA (for multibyte
           characters) while translating them and paying attention to the
           continuity of translated characters.

           Implementation note: It is better to implement these fairly big
           macros by a function, but it's not that easy because macros called
           in this macro assume various local variables already declared.  */

        /* Both FROM and TO are ASCII characters.  */
        public static void SETUP_ASCII_RANGE(range_table_work_area work_area, int FROM, int TO, LispObject translate, PtrEmulator<byte> b)
        {
            int C0, C1;

            for (C0 = (FROM); C0 <= (TO); C0++)
            {
                C1 = TRANSLATE(C0, translate);
                if (!ASCII_CHAR_P((uint) C1))
                {
                    SET_RANGE_TABLE_WORK_AREA((work_area), C1, C1);
                    if ((C1 = RE_CHAR_TO_UNIBYTE(C1)) < 0)
                        C1 = C0;
                }
                SET_LIST_BIT(C1, b);
            }
        }

        public static void SET_RANGE_TABLE_WORK_AREA_BIT(range_table_work_area work_area, int bit)
        {
            (work_area).bits |= (bit);
        }

        /* Entry points for GNU code.  */

        /* re_compile_pattern is the GNU regular expression compiler: it
           compiles PATTERN (of length SIZE) and puts the result in BUFP.
           Returns 0 if the pattern was valid, otherwise an error string.

           Assumes the `allocated' (and perhaps `buffer') and `translate' fields
           are set in BUFP on entry.

           We call regex_compile to do the actual compilation.  */
        public static string re_compile_pattern(byte[] pattern, int length, ref re_pattern_buffer bufp)
        {
            reg_errcode_t ret;


            gl_state.current_syntax_table = current_buffer.syntax_table;

            /* GNU code is written to assume at least RE_NREGS registers will be set
               (and at least one extra will be -1).  */
            bufp.regs_allocated = re_pattern_buffer.REGS_UNALLOCATED;

            /* And GNU code determines whether or not to get register information
               by passing null for the REGS argument to re_match, etc., not by
               setting no_sub.  */
            bufp.no_sub = false;

            ret = regex_compile(pattern, length, re_syntax_options, bufp);

            if (ret == 0)
                return null;
            return re_error_msgid[(int)ret];
        }

        public struct PtrEmulator<T> where T: System.IComparable
        {
            private T[] collection_;
            private int idx_;

            /* Can't have this constructor in a struct, but default one is doing exactly what we want
            public PtrEmulator()
            {
                collection_ = null;
                idx_ = 0;
            }
            */

            public PtrEmulator(T[] col)
            {
                collection_ = col;
                idx_ = 0;
            }

            public PtrEmulator(T[] col, int idx)
            {
                collection_ = col;
                idx_ = idx;
            }

            public PtrEmulator(PtrEmulator<T> x, int idx)
            {
                collection_ = x.collection_;
                idx_ = x.idx_ + idx;
            }

            public PtrEmulator(PtrEmulator<T> x)
            {
                collection_ = x.collection_;
                idx_ = x.idx_;
            }

            public static implicit operator PtrEmulator<T>(T[] x)
            {
                return new PtrEmulator<T>(x);
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public override bool Equals(object o)
            {
                return this == (PtrEmulator<T>)o;
            }

            public static int memcmp(PtrEmulator<T> ptr1, PtrEmulator<T> ptr2, int num)
            {
                for (int i = 0; i < num; i++)
                {
                    int r = ptr1[i].CompareTo(ptr2[i]);

                    if (r > 0)
                        return 1;
                    else if (r < 0)
                        return -1;
                }

                return 0;
            }

            public static bool operator ==(PtrEmulator<T> a, PtrEmulator<T> b)
            {
                if (a.collection_ == b.collection_ && a.idx_ == b.idx_)
                    return true;
                return false;
            }

            public static bool operator != (PtrEmulator<T> a, PtrEmulator<T> b)
            {
                return !(a == b);
            }

            public static int operator - (PtrEmulator<T> a, PtrEmulator<T> b)
            {
                if (a.collection_ != b.collection_)
                    throw new System.Exception("Can't subtract ptrs to different collections");

                return a.idx_ - b.idx_;
            }

            public static int operator -(PtrEmulator<T> a, T[] col)
            {
                return a.idx_;
            }

            public static PtrEmulator<T> operator -(PtrEmulator<T> a, int x)
            {
                return new PtrEmulator<T>(a.collection_, a.idx_ - x);
            }

            public static bool operator <(PtrEmulator<T> a, PtrEmulator<T> b)
            {
                return a.idx_ < b.idx_;
            }

            public static bool operator >(PtrEmulator<T> a, PtrEmulator<T> b)
            {
                return a.idx_ > b.idx_;
            }

            public static bool operator <= (PtrEmulator<T> a, PtrEmulator<T> b)
            {
                return (a < b) || (a == b);
            }

            public static bool operator >=(PtrEmulator<T> a, PtrEmulator<T> b)
            {
                return (a > b) || (a == b);
            }

            public static PtrEmulator<T> operator + (PtrEmulator<T> a, int x)
            {
                return new PtrEmulator<T>(a.collection_, a.idx_ + x);
            }

            public static PtrEmulator<T> operator ++(PtrEmulator<T> a)
            {
                a.idx_ ++;
                return a;
            }

            public static PtrEmulator<T> operator --(PtrEmulator<T> a)
            {
                a.idx_ --;
                return a;
            }

            public static bool operator true(PtrEmulator<T> a)
            {
                return a.collection_ != null;
            }

            public static bool operator false(PtrEmulator<T> a)
            {
                return a.collection_ == null;
            }

            public static bool operator !(PtrEmulator<T> x)
            {
                return x.collection_ == null;
            }

            public T this[int index]
            {
                get
                {
                    return collection_[idx_ + index];
                }

                set
                {
                    collection_[idx_ + index] = value;
                }
            }

            public T Value
            {
                get
                {
                    if (collection_ == null)
                        throw new System.Exception("Deref without collection");

                    return collection_[idx_];
                }

                set
                {
                    if (collection_ == null)
                        throw new System.Exception("Deref without collection");

                    collection_[idx_] = value;
                }
            }

            public T ValueAndInc()
            {
                T result = this.Value;
                idx_++;
                return result;
            }

            public T SetValueAndInc(T val)
            {
                this.Value = val;
                idx_++;
                return val;
            }

            public T[] Collection
            {
                get {return collection_;}
                set { collection_ = value; }
            }

            public int Index
            {
                get {return idx_;}
            }

            public int Length
            {
                get 
                {
                    return collection_.Length - idx_;
                }
            }

            public void Resize(int new_size)
            {
                System.Array.Resize(ref collection_, new_size);
            }

            public void bzero(int len, T val)
            {
                int bound = System.Math.Min(collection_.Length, idx_ + len);
                for (int i = idx_; i < bound; i++)
                    collection_[i] = val;
            }

            // TODO: checks, overlapping pointers, etc
            public static void bcopy(PtrEmulator<T> from, PtrEmulator<T> to, int len)
            {
                for (int i = 0; i < len; i++)
                {
                    to.Value = from.Value;
                    to++;
                    from++;
                }
            }
        }

        /* Structure to manage work area for range table.  */
        public class range_table_work_area
        {
            public int[] table;			/* actual work area.  */
            public int allocated;		/* allocated size for work area in bytes.  */
            public int used;			/* actually used size in words.  */
            public int bits;			/* flag to record character classes */
        }

        public class compile_stack_elt_t
        {
            public pattern_offset_t begalt_offset;
            public pattern_offset_t fixup_alt_jump;
            public pattern_offset_t laststart_offset;
            public regnum_t regnum;
        }

        public class compile_stack_type
        {
            public compile_stack_elt_t[] stack;
            public uint size;
            public uint avail;			/* Offset of next open position.  */
        }
        public const int INIT_COMPILE_STACK_SIZE = 32;
        /* If the buffer isn't allocated when it comes in, use this.  */
        public const int INIT_BUF_SIZE = 32;

        /* This is not an arbitrary limit: the arguments which represent offsets
           into the pattern are two bytes long.  So if 2^15 bytes turns out to
           be too small, many things would have to change.  */
        public const int MAX_BUF_SIZE = (1 << 15);

        public static void IMMEDIATE_QUIT_CHECK()
        {
            if (immediate_quit != 0)
                QUIT();
        } 

        public class RegexException : System.Exception
        {
            private reg_errcode_t x_;

            public RegexException(reg_errcode_t x)
            {
                x_ = x;
            }

            public reg_errcode_t Value
            {
                get { return x_; }
            }
        }

        public static void EXTEND_BUFFER(re_pattern_buffer bufp,
                                         PtrEmulator<byte> b, PtrEmulator<byte> begalt,
                                         PtrEmulator<byte> fixup_alt_jump, PtrEmulator<byte> laststart, PtrEmulator<byte> pending_exact)
        {
            byte[] old_buffer = bufp.buffer;
            if (bufp.allocated == MAX_BUF_SIZE)
                throw new RegexException(reg_errcode_t.REG_ESIZE);
            bufp.allocated <<= 1;
            if (bufp.allocated > MAX_BUF_SIZE)
                bufp.allocated = MAX_BUF_SIZE;
            System.Array.Resize(ref bufp.buffer, bufp.allocated);
            if (bufp.buffer == null)
                throw new RegexException(reg_errcode_t.REG_ESPACE);

            /* If the buffer moved, move all the pointers into it.  */
            if (old_buffer != bufp.buffer)
            {
                byte[] new_buffer = bufp.buffer;
                b.Collection = new_buffer;
                begalt.Collection = new_buffer;
                if (fixup_alt_jump)
                    fixup_alt_jump.Collection = new_buffer;
                if (laststart)
                    laststart.Collection = new_buffer;
                if (pending_exact)
                    pending_exact.Collection = new_buffer;
            }
        }


        /* Make sure we have at least N more bytes of space in buffer.  */
        public static void GET_BUFFER_SPACE(int n, re_pattern_buffer bufp,
                                         PtrEmulator<byte> b, PtrEmulator<byte> begalt,
                                         PtrEmulator<byte> fixup_alt_jump, PtrEmulator<byte> laststart, PtrEmulator<byte> pending_exact)						
        {
            while ((b.Index + n) > bufp.allocated)
                EXTEND_BUFFER(bufp,
                              b, begalt, fixup_alt_jump, laststart, pending_exact);
        }

        /* Make sure we have one more byte of buffer space and then add C to it.  */
        public static void BUF_PUSH(int c, re_pattern_buffer bufp,
                                    PtrEmulator<byte> b, PtrEmulator<byte> begalt,
                                    PtrEmulator<byte> fixup_alt_jump, PtrEmulator<byte> laststart, PtrEmulator<byte> pending_exact)
        {
            GET_BUFFER_SPACE(1, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            b.Value = (byte)(c);
            b++;
        }

        /* Ensure we have two more bytes of buffer space and then append C1 and C2.  */
        public static void BUF_PUSH_2(int c1, int c2, re_pattern_buffer bufp,
                                    PtrEmulator<byte> b, PtrEmulator<byte> begalt,
                                    PtrEmulator<byte> fixup_alt_jump, PtrEmulator<byte> laststart, PtrEmulator<byte> pending_exact)
        {
            GET_BUFFER_SPACE(2, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            b.Value = (byte)(c1); b++;
            b.Value = (byte)(c2); b++;
        }

        /* `regex_compile' compiles PATTERN (of length SIZE) according to SYNTAX.
           Returns one of error codes defined in `regex.h', or zero for success.

           Assumes the `allocated' (and perhaps `buffer') and `translate'
           fields are set in BUFP on entry.

           If it succeeds, results are put in BUFP (if it returns an error, the
           contents of BUFP are undefined):
             `buffer' is the compiled pattern;
             `syntax' is set to SYNTAX;
             `used' is set to the length of the compiled pattern;
             `fastmap_accurate' is zero;
             `re_nsub' is the number of subexpressions in PATTERN;
             `not_bol' and `not_eol' are zero;

           The `fastmap' field is neither examined nor set.  */

        /* Insert the `jump' from the end of last alternative to "here".
           The space for the jump has already been allocated. */
        public static void FIXUP_ALT_JUMP(PtrEmulator<byte> fixup_alt_jump, PtrEmulator<byte> b)
        {
            if (fixup_alt_jump)
                STORE_JUMP(re_opcode_t.jump, fixup_alt_jump, b);
        }

        /* Common operations on the compiled pattern.  */

        /* Store NUMBER in two contiguous bytes starting at DESTINATION.  */
        public static void STORE_NUMBER(PtrEmulator<byte> destination, int number)
        {
            destination[0] = (byte)((number) & 0xFF);
            destination[1] = (byte)((number) >> 8);
        }

        /* Same as STORE_NUMBER, except increment DESTINATION to
           the byte after where the number is stored.  Therefore, DESTINATION
           must be an lvalue.  */
        public static void STORE_NUMBER_AND_INCR(ref PtrEmulator<byte> destination, int number)
        {
            STORE_NUMBER(destination, number);
            destination += 2;
        }

        /* As in Harbison and Steele.  */
        public static int SIGN_EXTEND_CHAR(byte c)
        {
            return ((((byte)(c)) ^ 128) - 128);
        }

        /* Put into DESTINATION a number stored in two contiguous bytes starting
           at SOURCE.  */
        public static void EXTRACT_NUMBER(ref int destination, PtrEmulator<byte> source)
        {
            destination = source.Value & 0xFF;
            destination += SIGN_EXTEND_CHAR((source + 1).Value) << 8;
        }

        /* Same as EXTRACT_NUMBER, except increment SOURCE to after the number.
           SOURCE must be an lvalue.  */
        public static void EXTRACT_NUMBER_AND_INCR(ref int destination, ref PtrEmulator<byte> source)
        {
            EXTRACT_NUMBER(ref destination, source);
            source += 2;
        }

        /* Store a multibyte character in three contiguous bytes starting
           DESTINATION, and increment DESTINATION to the byte after where the
           character is stored.  Therefore, DESTINATION must be an lvalue.  */
        public static void STORE_CHARACTER_AND_INCR(ref PtrEmulator<byte> destination, int character)
        {
            destination[0] = (byte) ((character) & 0xff);
            destination[1] = (byte) (((character) >> 8) & 0xff);
            destination[2] = (byte) ((character) >> 16);
            destination += 3;
        }

        /* Put into DESTINATION a character stored in three contiguous bytes
           starting at SOURCE.  */
        public static void EXTRACT_CHARACTER(ref re_wchar_t destination, PtrEmulator<byte> source)
        {
            destination = ((source)[0]
                   | ((source)[1] << 8)
                   | ((source)[2] << 16));
        }

        /* Macros for charset. */

        /* Size of bitmap of charset P in bytes.  P is a start of charset,
           i.e. *P is (re_opcode_t) charset or (re_opcode_t) charset_not.  */
        public static int CHARSET_BITMAP_SIZE(PtrEmulator<byte> p)
        {
            return (p[1] & 0x7F);
        }

        /* Nonzero if charset P has range table.  */
        public static bool CHARSET_RANGE_TABLE_EXISTS_P(PtrEmulator<byte> p)
        {
            return (p[1] & 0x80) != 0;
        }

        /* Return the address of range table of charset P.  But not the start
           of table itself, but the before where the number of ranges is
           stored.  `2 +' means to skip re_opcode_t and size of bitmap,
           and the 2 bytes of flags at the start of the range table.  */
        public static PtrEmulator<byte> CHARSET_RANGE_TABLE(PtrEmulator<byte> p)
        {
            return p + (4 + CHARSET_BITMAP_SIZE(p));
        }

        /* Extract the bit flags that start a range table.  */
        public static int CHARSET_RANGE_TABLE_BITS(PtrEmulator<byte> p)
        {
            return (p[2 + CHARSET_BITMAP_SIZE(p)] + p[3 + CHARSET_BITMAP_SIZE(p)] * 0x100);
        }


        /* Subroutines for `regex_compile'.  */

        /* Store OP at LOC followed by two-byte integer parameter ARG.  */
        public static void store_op1(re_opcode_t op, PtrEmulator<byte> loc, int arg)
        {
            loc.Value = (byte)op;
            STORE_NUMBER(loc + 1, arg);
        }

        /* Like `store_op1', but for two two-byte parameters ARG1 and ARG2.  */
        public static void store_op2(re_opcode_t op, PtrEmulator<byte> loc, int arg1, int arg2)
        {
            loc.Value = (byte)op;
            STORE_NUMBER(loc + 1, arg1);
            STORE_NUMBER(loc + 3, arg2);
        }

        /* Copy the bytes from LOC to END to open up three bytes of space at LOC
           for OP followed by two-byte integer parameter ARG.  */
        public static void insert_op1(re_opcode_t op, PtrEmulator<byte> loc, int arg, PtrEmulator<byte> end)
        {
            PtrEmulator<byte> pfrom = new PtrEmulator<byte>(end);
            PtrEmulator<byte> pto = end + 3;

            while (pfrom != loc)
            {
                --pto; --pfrom;
                pto.Value = pfrom.Value;
            }

            store_op1(op, loc, arg);
        }

        /* Like `insert_op1', but for two two-byte parameters ARG1 and ARG2.  */
        public static void insert_op2(re_opcode_t op, PtrEmulator<byte> loc, int arg1, int arg2, PtrEmulator<byte> end)
        {
            PtrEmulator<byte> pfrom = new PtrEmulator<byte>(end);
            PtrEmulator<byte> pto = end + 5;

            while (pfrom != loc)
            {
                --pto; --pfrom;
                pto.Value = pfrom.Value;
            }

            store_op2(op, loc, arg1, arg2);
        }

        /* P points to just after a ^ in PATTERN.  Return true if that ^ comes
           after an alternative or a begin-subexpression.  We assume there is at
           least one character before the ^.  */
        public static bool at_begline_loc_p(PtrEmulator<re_char> pattern, PtrEmulator<re_char> p, reg_syntax_t syntax)
        {
            PtrEmulator<re_char> prev = p + (-2);
            bool prev_prev_backslash = prev > pattern && prev[-1] == '\\';

            return
                /* After a subexpression?  */
                 (prev.Value == '(' && ((syntax & RE_NO_BK_PARENS) != 0 || prev_prev_backslash))
                /* After an alternative?	 */
              || (prev.Value == '|' && ((syntax & RE_NO_BK_VBAR) != 0 || prev_prev_backslash))
                /* After a shy subexpression?  */
              || ((syntax & RE_SHY_GROUPS) != 0 && (prev + (-2)) >= pattern
              && prev[-1] == '?' && prev[-2] == '('
              && ((syntax & RE_NO_BK_PARENS) != 0
                  || ((prev + (-3)) >= pattern && prev[-3] == '\\')));
        }

        /* The dual of at_begline_loc_p.  This one is for $.  We assume there is
           at least one character after the $, i.e., `P < PEND'.  */
        public static bool at_endline_loc_p(PtrEmulator<re_char> p, PtrEmulator<re_char> pend, reg_syntax_t syntax)
        {
            PtrEmulator<re_char> next = new PtrEmulator<re_char>(p);
            bool next_backslash = next.Value == '\\';
            PtrEmulator<re_char> next_next = p + 1 < pend ? p + 1 : new PtrEmulator<re_char>();

            return
                /* Before a subexpression?  */
                 ((syntax & RE_NO_BK_PARENS) != 0 ? next.Value == ')'
              : next_backslash && next_next.Collection != null && next_next.Value == ')')
                /* Before an alternative?  */
              || ((syntax & RE_NO_BK_VBAR) != 0 ? next.Value == '|'
              : next_backslash && next_next.Collection != null && next_next.Value == '|');
        }

        /* Returns true if REGNUM is in one of COMPILE_STACK's elements and
           false if it's not.  */
        public static bool group_in_compile_stack(compile_stack_type compile_stack, regnum_t regnum)
        {
            int this_element;

            for (this_element = (int)(compile_stack.avail - 1);
                 this_element >= 0;
                 this_element--)
                if (compile_stack.stack[this_element].regnum == regnum)
                    return true;

            return false;
        }

        public static uint RE_CHAR_TO_MULTIBYTE(int c)
        {
            return unibyte_to_multibyte_table[(c)];
        }

        public static int RE_CHAR_TO_UNIBYTE(int c)
        {
            return CHAR_TO_BYTE_SAFE((uint) c);
        }

        /* Set C a (possibly converted to multibyte) character before P.  P
           points into a string which is the virtual concatenation of STR1
           (which ends at END1) or STR2 (which ends at END2).  */
        public static void GET_CHAR_BEFORE_2(bool target_multibyte, ref uint c, PtrEmulator<re_char> p, PtrEmulator<re_char> str1, PtrEmulator<re_char> end1, PtrEmulator<re_char> str2, PtrEmulator<re_char> end2)
        {
            if (target_multibyte)
            {
                PtrEmulator<re_char> dtemp = new PtrEmulator<re_char>(p == str2 ? end1 : p);
                PtrEmulator<re_char> dlimit = new PtrEmulator<re_char>((p > str2 && p <= end2) ? str2 : str1);
                while (dtemp-- > dlimit && !CHAR_HEAD_P(dtemp.Value)) ;
                c = STRING_CHAR(dtemp.Collection, dtemp.Index, p - dtemp);
            }
            else
            {
                c = (p == str2 ? end1 : p)[-1];
                c = RE_CHAR_TO_MULTIBYTE((int) c);
            }
        }

        /* Set C a (possibly converted to multibyte) character at P, and set
           LEN to the byte length of that character.  */
        public static void GET_CHAR_AFTER(bool target_multibyte, ref uint c, PtrEmulator<re_char> p, ref int len)
        {
            if (target_multibyte)
                c = STRING_CHAR_AND_LENGTH(p.Collection, p.Index, 0, ref len);
            else
            {
                c = p.Value;
                len = 1;
                c = RE_CHAR_TO_MULTIBYTE((int) c);
            }
        }


        public const int BYTEWIDTH = 8; /* In bits.  */

        /* analyse_first.
           If fastmap is non-NULL, go through the pattern and fill fastmap
           with all the possible leading chars.  If fastmap is NULL, don't
           bother filling it up (obviously) and only return whether the
           pattern could potentially match the empty string.

           Return 1  if p..pend might match the empty string.
           Return 0  if p..pend matches at least one char.
           Return -1 if fastmap was not updated accurately.  */
        public static int analyse_first (PtrEmulator<byte> p, PtrEmulator<byte> pend, byte[] fastmap, bool multibyte)
        {
            int j = 0, k;
            bool not;

            /* If all elements for base leading-codes in fastmap is set, this
               flag is set true.  */
            bool match_any_multibyte_characters = false;

            //  assert (p);

            /* The loop below works as follows:
               - It has a working-list kept in the PATTERN_STACK and which basically
                 starts by only containing a pointer to the first operation.
               - If the opcode we're looking at is a match against some set of
                 chars, then we add those chars to the fastmap and go on to the
                 next work element from the worklist (done via `break').
               - If the opcode is a control operator on the other hand, we either
                 ignore it (if it's meaningless at this point, such as `start_memory')
                 or execute it (if it's a jump).  If the jump has several destinations
                 (i.e. `on_failure_jump'), then we push the other destination onto the
                 worklist.
               We guarantee termination by ignoring backward jumps (more or less),
               so that `p' is monotonically increasing.  More to the point, we
               never set `p' (or push) anything `<= p1'.  */

            while (p < pend)
            {
                /* `p1' is used as a marker of how far back a `on_failure_jump'
               can go without being ignored.  It is normally equal to `p'
               (which prevents any backward `on_failure_jump') except right
               after a plain `jump', to allow patterns such as:
                  0: jump 10
                  3..9: <body>
                  10: on_failure_jump 3
               as used for the *? operator.  */
                PtrEmulator<re_char> p1 = new PtrEmulator<re_char>(p);

                p++;
                switch ((re_opcode_t)p[-1])
                {
                    case re_opcode_t.succeed:
                        return 1;

                    case re_opcode_t.duplicate:
                        /* If the first character has to match a backreference, that means
                           that the group was empty (since it already matched).  Since this
                           is the only case that interests us here, we can assume that the
                           backreference must match the empty string.  */
                        p++;
                        continue;


                    /* Following are the cases which match a character.  These end
                   with `break'.  */

                    case re_opcode_t.exactn:
                        if (fastmap != null)
                        {
                            /* If multibyte is nonzero, the first byte of each
                           character is an ASCII or a leading code.  Otherwise,
                           each byte is a character.  Thus, this works in both
                           cases. */
                            fastmap[p[1]] = 1;
                            if (!multibyte)
                            {
                                /* For the case of matching this unibyte regex
                                   against multibyte, we must set a leading code of
                                   the corresponding multibyte character.  */
                                uint c = RE_CHAR_TO_MULTIBYTE(p[1]);

                                if (!CHAR_BYTE8_P(c))
                                    fastmap[CHAR_LEADING_CODE(c)] = 1;
                            }
                        }
                        break;


                    case re_opcode_t.anychar:
                        /* We could put all the chars except for \n (and maybe \0)
                           but we don't bother since it is generally not worth it.  */
                        if (fastmap == null) break;
                        return -1;


                    case re_opcode_t.charset_not:
                        if (fastmap == null) break;
                        {
                            /* Chars beyond end of bitmap are possible matches.  */
                            for (j = CHARSET_BITMAP_SIZE(p + (-1)) * BYTEWIDTH;
                             j < (1 << BYTEWIDTH); j++)
                                fastmap[j] = 1;
                        }
                        goto case re_opcode_t.charset;
                    /* Fallthrough */
                    case re_opcode_t.charset:
                        if (fastmap == null) break;
                        not = (re_opcode_t)p[-1] == re_opcode_t.charset_not;
                        for (j = CHARSET_BITMAP_SIZE(p + (-1)) * BYTEWIDTH - 1, p++;
                             j >= 0; j--)
                             if (( ( (p[j / BYTEWIDTH] & (1 << (j % BYTEWIDTH))) != 0 ? 1 : 0 ) ^ (not ? 1 : 0)) != 0)
                                fastmap[j] = 1;

                        // #ifdef emacs
                        if (/* Any leading code can possibly start a character
		 which doesn't match the specified set of characters.  */
                            not
                            ||
                            /* If we can match a character class, we can match any
                           multibyte characters.  */
                            (CHARSET_RANGE_TABLE_EXISTS_P(p + (-2))
                             && CHARSET_RANGE_TABLE_BITS(p + (-2)) != 0))
                        {
                            if (match_any_multibyte_characters == false)
                            {
                                for (j = (int)MIN_MULTIBYTE_LEADING_CODE;
                                     j <= MAX_MULTIBYTE_LEADING_CODE; j++)
                                    fastmap[j] = 1;
                                match_any_multibyte_characters = true;
                            }
                        }

                        else if (!not && CHARSET_RANGE_TABLE_EXISTS_P(p + (-2))
                             && match_any_multibyte_characters == false)
                        {
                            /* Set fastmap[I] to 1 where I is a leading code of each
                           multibyte characer in the range table. */
                            int c = 0, count = 0;
                            byte lc1, lc2;

                            /* Make P points the range table.  `+ 2' is to skip flag
                           bits for a character class.  */
                            p += CHARSET_BITMAP_SIZE(p + (-2)) + 2;

                            /* Extract the number of ranges in range table into COUNT.  */
                            EXTRACT_NUMBER_AND_INCR(ref count, ref p);
                            for (; count > 0; count--, p += 3)
                            {
                                /* Extract the start and end of each range.  */
                                EXTRACT_CHARACTER(ref c, p);
                                lc1 = CHAR_LEADING_CODE((uint)c);
                                p += 3;
                                EXTRACT_CHARACTER(ref c, p);
                                lc2 = CHAR_LEADING_CODE((uint)c);
                                for (j = lc1; j <= lc2; j++)
                                    fastmap[j] = 1;
                            }
                        }
                        // #endif
                        break;

                    case re_opcode_t.syntaxspec:
                    case re_opcode_t.notsyntaxspec:
                        if (fastmap == null) break;
                        /*#ifndef emacs
                              not = (re_opcode_t)p[-1] == notsyntaxspec;
                              k = *p++;
                              for (j = 0; j < (1 << BYTEWIDTH); j++)
                                if ((SYNTAX (j) == (enum syntaxcode) k) ^ not)
                                  fastmap[j] = 1;
                              break;
                        #else  /* emacs */
                        /* This match depends on text properties.  These end with
                           aborting optimizations.  */
                        return -1;

                    case re_opcode_t.categoryspec:
                    case re_opcode_t.notcategoryspec:
                        if (fastmap == null) break;
                        not = (re_opcode_t)p[-1] == re_opcode_t.notcategoryspec;
                        k = p.Value; p++;
                        for (j = (1 << BYTEWIDTH); j >= 0; j--)
                            if ((CHAR_HAS_CATEGORY(j, k)) ^ not)
                                fastmap[j] = 1;

                        /* Any leading code can possibly start a character which
                           has or doesn't has the specified category.  */
                        if (match_any_multibyte_characters == false)
                        {
                            for (j = (int)MIN_MULTIBYTE_LEADING_CODE;
                             j <= MAX_MULTIBYTE_LEADING_CODE; j++)
                                fastmap[j] = 1;
                            match_any_multibyte_characters = true;
                        }
                        break;

                    /* All cases after this match the empty string.  These end with
                   `continue'.  */

                    case re_opcode_t.before_dot:
                    case re_opcode_t.at_dot:
                    case re_opcode_t.after_dot:
                    // #endif /* !emacs */
                    case re_opcode_t.no_op:
                    case re_opcode_t.begline:
                    case re_opcode_t.endline:
                    case re_opcode_t.begbuf:
                    case re_opcode_t.endbuf:
                    case re_opcode_t.wordbound:
                    case re_opcode_t.notwordbound:
                    case re_opcode_t.wordbeg:
                    case re_opcode_t.wordend:
                    case re_opcode_t.symbeg:
                    case re_opcode_t.symend:
                        continue;


                    case re_opcode_t.jump:
                        EXTRACT_NUMBER_AND_INCR(ref j, ref p);
                        if (j < 0)
                            /* Backward jumps can only go back to code that we've already
                               visited.  `re_compile' should make sure this is true.  */
                            break;
                        p += j;
                        switch ((re_opcode_t)p.Value)
                        {
                            case re_opcode_t.on_failure_jump:
                            case re_opcode_t.on_failure_keep_string_jump:
                            case re_opcode_t.on_failure_jump_loop:
                            case re_opcode_t.on_failure_jump_nastyloop:
                            case re_opcode_t.on_failure_jump_smart:
                                p++;
                                break;
                            default:
                                continue;
                        };
                    /* Keep `p1' to allow the `on_failure_jump' we are jumping to
                       to jump back to "just after here".  */
                    /* Fallthrough */
                        goto case re_opcode_t.on_failure_jump;
                    case re_opcode_t.on_failure_jump:
                    case re_opcode_t.on_failure_keep_string_jump:
                    case re_opcode_t.on_failure_jump_nastyloop:
                    case re_opcode_t.on_failure_jump_loop:
                    case re_opcode_t.on_failure_jump_smart:
                        EXTRACT_NUMBER_AND_INCR(ref j, ref p);
                        if (p + j <= p1)
                        {
                            /* Backward jump to be ignored.  */
                        }
                        else
                        { /* We have to look down both arms.
		 We first go down the "straight" path so as to minimize
		 stack usage when going through alternatives.  */
                            int r = analyse_first(p, pend, fastmap, multibyte);
                            if (r != 0) return r;
                            p += j;
                        }
                        continue;


                    case re_opcode_t.jump_n:
                        /* This code simply does not properly handle forward jump_n.  */
                        // DEBUG_STATEMENT (EXTRACT_NUMBER (j, p); assert (j < 0));
                        p += 4;
                        /* jump_n can either jump or fall through.  The (backward) jump
                           case has already been handled, so we only need to look at the
                           fallthrough case.  */
                        continue;

                    case re_opcode_t.succeed_n:
                        /* If N == 0, it should be an on_failure_jump_loop instead.  */
                        // DEBUG_STATEMENT (EXTRACT_NUMBER (j, p + 2); assert (j > 0));
                        p += 4;
                        /* We only care about one iteration of the loop, so we don't
                           need to consider the case where this behaves like an
                           on_failure_jump.  */
                        continue;


                    case re_opcode_t.set_number_at:
                        p += 4;
                        continue;


                    case re_opcode_t.start_memory:
                    case re_opcode_t.stop_memory:
                        p += 1;
                        continue;


                    default:
                        abort(); /* We have listed all the cases.  */
                        break;
                } /* switch *p++ */

                /* Getting here means we have found the possible starting
               characters for one path of the pattern -- and that the empty
               string does not match.  We need not follow this path further.  */
                return 0;
            } /* while p */

            /* We reached the end without matching anything.  */
            return 1;
        } /* analyse_first */

        public static bool RE_MULTIBYTE_P(re_pattern_buffer bufp)
        { 
            return bufp.multibyte;
        }
        public static bool RE_TARGET_MULTIBYTE_P(re_pattern_buffer bufp)
        {
            return bufp.target_multibyte;
        }

        public static uint RE_STRING_CHAR(PtrEmulator<byte> p, int s, bool multibyte) 
        {
            return (multibyte ? (STRING_CHAR(p.Collection, p.Index, s)) : (p.Value));
        }

        public static uint RE_STRING_CHAR_AND_LENGTH(PtrEmulator<byte> p, int s, ref int len, bool multibyte)
        {
            if (multibyte)
            {
                return STRING_CHAR_AND_LENGTH(p.Collection, p.Index, s, ref len);
            }
            else
            {
                len = 1;
                return p.Value;
            }
        }

/* Get the next unsigned number in the uncompiled pattern.  */
        public static void GET_UNSIGNED_NUMBER(ref int num, ref int c, ref PtrEmulator<byte> p, PtrEmulator<byte> pend, bool multibyte)
        {
            if (p == pend)
                throw new RegexException(reg_errcode_t.REG_EBRACE);
            else
            {
                PATFETCH(ref c, ref p, pend, multibyte);
                while ('0' <= c && c <= '9')
                {
                    int prev;
                    if (num < 0)
                        num = 0;
                    prev = num;
                    num = num * 10 + c - '0';
                    if (num / 10 != prev)
                        throw new RegexException(reg_errcode_t.REG_BADBR);
                    if (p == pend)
                        throw new RegexException(reg_errcode_t.REG_EBRACE);
                    PATFETCH(ref c, ref p, pend, multibyte);
                }
            }
        }


        /* Fetch the next character in the uncompiled pattern, with no
           translation.  */
        public static void PATFETCH(ref int c, ref PtrEmulator<byte> p, PtrEmulator<byte> pend, bool multibyte)
        {
            int len = 0;
            if (p == pend)
            {
                throw new RegexException(reg_errcode_t.REG_EEND);
            }
            c = (int)RE_STRING_CHAR_AND_LENGTH(p, pend - p, ref len, multibyte);
            p += len;
        }

        /* Return the address of end of RANGE_TABLE.  COUNT is number of
           ranges (which is a pair of (start, end)) in the RANGE_TABLE.  `* 2'
           is start of range and end of range.  `* 3' is size of each start
           and end.  */
        public static PtrEmulator<byte> CHARSET_RANGE_TABLE_END(PtrEmulator<byte> range_table, int count)
        {
            return (range_table + (count) * 2 * 3);
        }

        /* Test if C is in RANGE_TABLE.  A flag NOT is negated if C is in.
           COUNT is number of ranges in RANGE_TABLE.  */
        public static void CHARSET_LOOKUP_RANGE_TABLE_RAW(ref bool not, uint c, PtrEmulator<re_char> range_table, int count)
        {
            re_wchar_t range_start = 0, range_end = 0;
            PtrEmulator<re_char> p;
            PtrEmulator<re_char> range_table_end = CHARSET_RANGE_TABLE_END(range_table, count);

            for (p = range_table; p < range_table_end; p += 2 * 3)
            {
                EXTRACT_CHARACTER(ref range_start, p);
                EXTRACT_CHARACTER(ref range_end, p + 3);

                if (range_start <= c && c <= range_end)
                {
                    not = !not;
                    break;
                }
            }
        }

        /* Test if C is in range table of CHARSET.  The flag NOT is negated if
           C is listed in it.  */
        public static void CHARSET_LOOKUP_RANGE_TABLE(ref bool not, uint c, PtrEmulator<byte> charset)
        {
            /* Number of ranges in range table. */
            int count = 0;
            PtrEmulator<re_char> range_table = CHARSET_RANGE_TABLE(charset);

            EXTRACT_NUMBER_AND_INCR(ref count, ref range_table);
            CHARSET_LOOKUP_RANGE_TABLE_RAW(ref not, c, range_table, count);
        }

        /* Set the bit for character C in a list.  */
        public static void SET_LIST_BIT(int c, PtrEmulator<byte> b)
        {
            b[c / BYTEWIDTH] |= (byte) (1 << (c % BYTEWIDTH));
        }

        public static bool COMPILE_STACK_EMPTY(compile_stack_type compile_stack)
        {
            return (compile_stack.avail == 0);
        }

        public static bool COMPILE_STACK_FULL(compile_stack_type compile_stack)
        {
            return (compile_stack.avail == compile_stack.size);
        }

        /* The next available element.  */
        public static compile_stack_elt_t COMPILE_STACK_TOP(compile_stack_type compile_stack)
        {
            return compile_stack.stack[compile_stack.avail];
        }

        /* Like re_search_2, below, but only one string is specified, and
           doesn't let you say where to stop matching. */
        public static int re_search(re_pattern_buffer bufp, PtrEmulator<byte> stringg, int size, int startpos, int range, re_registers regs)
        {
            return re_search_2(bufp, new PtrEmulator<byte>(), 0, stringg, size, startpos, range,
                        regs, size);
        }

        /* Head address of virtual concatenation of string.  */
        public static PtrEmulator<byte> HEAD_ADDR_VSTRING(int P, PtrEmulator<byte> string1, PtrEmulator<byte> string2, int size1)
        {
            return new PtrEmulator<byte>((P >= size1 ? string2 : string1));
        }


        /* Address of POS in the concatenation of virtual string. */
        public static PtrEmulator<byte> POS_ADDR_VSTRING(int POS, PtrEmulator<re_char> string1, PtrEmulator<re_char> string2, int size1)
        {
            return new PtrEmulator<byte>((POS >= size1 ? string2 - size1 : string1), POS);
        }

        /* End address of virtual concatenation of string.  */
        public static PtrEmulator<byte> STOP_ADDR_VSTRING(int P, PtrEmulator<byte> string1, PtrEmulator<byte> string2, int size1, int size2)
        {
            return new PtrEmulator<byte>((P >= size1 ? string2 + size2 : string1 + size1));
        }

        /* Using the compiled pattern in BUFP->buffer, first tries to match the
           virtual concatenation of STRING1 and STRING2, starting first at index
           STARTPOS, then at STARTPOS + 1, and so on.

           STRING1 and STRING2 have length SIZE1 and SIZE2, respectively.

           RANGE is how far to scan while trying to match.  RANGE = 0 means try
           only at STARTPOS; in general, the last start tried is STARTPOS +
           RANGE.

           In REGS, return the indices of the virtual concatenation of STRING1
           and STRING2 that matched the entire BUFP->buffer and its contained
           subexpressions.

           Do not consider matching one past the index STOP in the virtual
           concatenation of STRING1 and STRING2.

           We return either the position in the strings at which the match was
           found, -1 if no match, or -2 if error (such as failure
           stack overflow).  */
        public static int re_search_2(re_pattern_buffer bufp, PtrEmulator<byte> str1, int size1, PtrEmulator<byte> str2, int size2, int startpos, int range, re_registers regs, int stop)
        {
            int val;
            PtrEmulator<re_char> string1 = str1;
            PtrEmulator<re_char> string2 = str2;
            byte[] fastmap = bufp.fastmap;
            RE_TRANSLATE_TYPE translate = bufp.translate;
            int total_size = size1 + size2;
            int endpos = startpos + range;
            bool anchored_start;
            /* Nonzero if we are searching multibyte string.  */
            bool multibyte = RE_TARGET_MULTIBYTE_P(bufp);

            /* Check for out-of-range STARTPOS.  */
            if (startpos < 0 || startpos > total_size)
                return -1;

            /* Fix up RANGE if it might eventually take us outside
               the virtual concatenation of STRING1 and STRING2.
               Make sure we won't move STARTPOS below 0 or above TOTAL_SIZE.  */
            if (endpos < 0)
                range = 0 - startpos;
            else if (endpos > total_size)
                range = total_size - startpos;

            /* If the search isn't to be a backwards one, don't waste time in a
               search for a pattern anchored at beginning of buffer.  */
            if (bufp.used > 0 && (re_opcode_t)bufp.buffer[0] == re_opcode_t.begbuf && range > 0)
            {
                if (startpos > 0)
                    return -1;
                else
                    range = 0;
            }

            /* In a forward search for something that starts with \=.
               don't keep searching past point.  */
            if (bufp.used > 0 && (re_opcode_t)bufp.buffer[0] == re_opcode_t.at_dot && range > 0)
            {
                range = PT_BYTE() - BEGV_BYTE() - startpos;
                if (range < 0)
                    return -1;
            }

            /* Update the fastmap now if not correct already.  */
            if (fastmap != null && !bufp.fastmap_accurate)
                re_compile_fastmap(bufp);

            /* See whether the pattern is anchored.  */
            anchored_start = ((re_opcode_t)bufp.buffer[0] == re_opcode_t.begline);

            gl_state.obj = re_match_object;
            {
                int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(POS_AS_IN_BUFFER(startpos));

                SETUP_SYNTAX_TABLE_FOR_OBJECT(re_match_object, charpos, 1);
            }

            /* Loop through the string, looking for a place to start matching.  */
            for (; ; )
            {
                /* If the pattern is anchored,
               skip quickly past places we cannot match.
               We don't bother to treat startpos == 0 specially
               because that case doesn't repeat.  */
                if (anchored_start && startpos > 0)
                {
                    if (!((startpos <= size1 ? string1[startpos - 1]
                        : string2[startpos - size1 - 1])
                       == '\n'))
                        goto advance;
                }

                /* If a fastmap is supplied, skip quickly over characters that
               cannot be the start of a match.  If the pattern can match the
               null string, however, we don't need to skip characters; we want
               the first null string.  */
                if (fastmap != null && startpos < total_size && !bufp.can_be_null)
                {
                    PtrEmulator<re_char> d;
                    re_wchar_t buf_ch;

                    d = POS_ADDR_VSTRING(startpos, string1, string2, size1);

                    if (range > 0)	/* Searching forwards.  */
                    {
                        int lim = 0;
                        int irange = range;

                        if (startpos < size1 && startpos + range >= size1)
                            lim = range - (size1 - startpos);

                        /* Written out as an if-else to avoid testing `translate'
                       inside the loop.  */
                        if (RE_TRANSLATE_P(translate))
                        {
                            if (multibyte)
                                while (range > lim)
                                {
                                    int buf_charlen = 0;

                                    buf_ch = (int) STRING_CHAR_AND_LENGTH(d.Collection, d.Index, range - lim,
                                                     ref buf_charlen);
                                    buf_ch = RE_TRANSLATE(translate, buf_ch);
                                    if (fastmap[CHAR_LEADING_CODE((uint) buf_ch)] != 0)
                                        break;

                                    range -= buf_charlen;
                                    d += buf_charlen;
                                }
                            else
                                while (range > lim)
                                {
                                    re_wchar_t ch, translated;

                                    buf_ch = d.Value;
                                    ch = (int) RE_CHAR_TO_MULTIBYTE(buf_ch);
                                    translated = RE_TRANSLATE(translate, ch);
                                    if (translated != ch
                                        && (ch = RE_CHAR_TO_UNIBYTE(translated)) >= 0)
                                        buf_ch = ch;
                                    if (fastmap[buf_ch] != 0)
                                        break;
                                    d++;
                                    range--;
                                }
                        }
                        else
                        {
                            if (multibyte)
                                while (range > lim)
                                {
                                    int buf_charlen = 0;

                                    buf_ch = (int) STRING_CHAR_AND_LENGTH(d.Collection, d.Index, range - lim,
                                                     ref buf_charlen);
                                    if (fastmap[CHAR_LEADING_CODE((uint) buf_ch)] != 0)
                                        break;
                                    range -= buf_charlen;
                                    d += buf_charlen;
                                }
                            else
                                while (range > lim && fastmap[d.Value] == 0)
                                {
                                    d++;
                                    range--;
                                }
                        }
                        startpos += irange - range;
                    }
                    else				/* Searching backwards.  */
                    {
                        int room = (startpos >= size1
                            ? size2 + size1 - startpos
                            : size1 - startpos);
                        if (multibyte)
                        {
                            buf_ch = (int) STRING_CHAR(d.Collection, d.Index, room);
                            buf_ch = TRANSLATE(buf_ch, translate);
                            if (fastmap[CHAR_LEADING_CODE((uint) buf_ch)] == 0)
                                goto advance;
                        }
                        else
                        {
                            re_wchar_t ch, translated;

                            buf_ch = d.Value;
                            ch = (int) RE_CHAR_TO_MULTIBYTE(buf_ch);
                            translated = TRANSLATE(ch, translate);
                            if (translated != ch
                                && (ch = RE_CHAR_TO_UNIBYTE(translated)) >= 0)
                                buf_ch = ch;
                            if (fastmap[TRANSLATE(buf_ch, translate)] == 0)
                                goto advance;
                        }
                    }
                }

                /* If can't match the null string, and that's all we have left, fail.  */
                if (range >= 0 && startpos == total_size && fastmap != null
                && !bufp.can_be_null)
                    return -1;

                val = re_match_2_internal(bufp, string1, size1, string2, size2,
                           startpos, regs, stop);

                if (val >= 0)
                    return startpos;

                if (val == -2)
                    return -2;

            advance:
                if (range == 0)
                    break;
                else if (range > 0)
                {
                    /* Update STARTPOS to the next character boundary.  */
                    if (multibyte)
                    {
                        PtrEmulator<re_char> p = POS_ADDR_VSTRING(startpos, string1, string2, size1);
                        PtrEmulator<re_char> pend = STOP_ADDR_VSTRING(startpos, string1, string2, size1, size2);
                        int len = MULTIBYTE_FORM_LENGTH(p, pend - p);

                        range -= len;
                        if (range < 0)
                            break;
                        startpos += len;
                    }
                    else
                    {
                        range--;
                        startpos++;
                    }
                }
                else
                {
                    range++;
                    startpos--;

                    /* Update STARTPOS to the previous character boundary.  */
                    if (multibyte)
                    {
                        PtrEmulator<re_char> p = POS_ADDR_VSTRING(startpos, string1, string2, size1) + 1;
                        PtrEmulator<re_char> p0 = p;
                        PtrEmulator<re_char> phead = HEAD_ADDR_VSTRING(startpos, string1, string2, size1);

                        /* Find the head of multibyte form.  */
                        PREV_CHAR_BOUNDARY(ref p, phead);
                        range += p0 - 1 - p;
                        if (range > 0)
                            break;

                        startpos -= p0 - 1 - p;
                    }
                }
            }
            return -1;
        }

        /* Optimization routines.  */

        /* If the operation is a match against one or more chars,
           return a pointer to the next operation, else return NULL.  */
        public static PtrEmulator<re_char> skip_one_char(PtrEmulator<re_char> p)
        {
            p++;
            switch ((re_opcode_t)p[-1])
            {
                case re_opcode_t.anychar:
                    break;

                case re_opcode_t.exactn:
                    p += p.Value + 1;
                    break;

                case re_opcode_t.charset_not:
                case re_opcode_t.charset:
                    if (CHARSET_RANGE_TABLE_EXISTS_P(p + (-1)))
                    {
                        int mcnt = 0;
                        p = CHARSET_RANGE_TABLE(p + -1);
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        p = CHARSET_RANGE_TABLE_END(p, mcnt);
                    }
                    else
                        p += 1 + CHARSET_BITMAP_SIZE(p + -1);
                    break;

                case re_opcode_t.syntaxspec:
                case re_opcode_t.notsyntaxspec:
                // #ifdef emacs
                case re_opcode_t.categoryspec:
                case re_opcode_t.notcategoryspec:
                    // #endif /* emacs */
                    p++;
                    break;

                default:
                    p = new PtrEmulator<byte>();
                    break;
            }
            return p;
        }

        /* Jump over non-matching operations.  */
        public static PtrEmulator<byte> skip_noops(PtrEmulator<byte> p, PtrEmulator<byte> pend)
        {
            int mcnt = 0;
            while (p < pend)
            {
                switch ((re_opcode_t)p.Value)
                {
                    case re_opcode_t.start_memory:
                    case re_opcode_t.stop_memory:
                        p += 2; break;
                    case re_opcode_t.no_op:
                        p += 1; break;
                    case re_opcode_t.jump:
                        p += 1;
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        p += mcnt;
                        break;
                    default:
                        return p;
                }
            }
            // assert(p == pend);
            return p;
        }

        /* Non-zero if "p1 matches something" implies "p2 fails".  */
        static bool mutually_exclusive_p(re_pattern_buffer bufp, PtrEmulator<re_char> p1, PtrEmulator<re_char> p2)
        {
            re_opcode_t op2;
            bool multibyte = RE_MULTIBYTE_P(bufp);
            PtrEmulator<byte> pend = new PtrEmulator<byte>(bufp.buffer, bufp.used);

            //  assert (p1 >= bufp->buffer && p1 < pend
            //	  && p2 >= bufp->buffer && p2 <= pend);

            /* Skip over open/close-group commands.
               If what follows this loop is a ...+ construct,
               look at what begins its body, since we will have to
               match at least one of that.  */
            p2 = skip_noops(p2, pend);
            /* The same skip can be done for p1, except that this function
               is only used in the case where p1 is a simple match operator.  */
            /* p1 = skip_noops (p1, pend); */

            //  assert (p1 >= bufp->buffer && p1 < pend
            //	  && p2 >= bufp->buffer && p2 <= pend);

            op2 = p2 == pend ? re_opcode_t.succeed : (re_opcode_t)p2.Value;

            switch (op2)
            {
                case re_opcode_t.succeed:
                case re_opcode_t.endbuf:
                    /* If we're at the end of the pattern, we can change.  */
                    if (skip_one_char(p1))
                    {
                        DEBUG_PRINT1("  End of pattern: fast loop.\n");
                        return true;
                    }
                    break;

                case re_opcode_t.endline:
                case re_opcode_t.exactn:
                    {
                        uint c = (re_opcode_t)p2.Value == re_opcode_t.endline ? '\n'
                         : RE_STRING_CHAR(p2 + 2, pend - p2 - 2, multibyte);

                        if ((re_opcode_t)p1.Value == re_opcode_t.exactn)
                        {
                            if (c != RE_STRING_CHAR(p1 + 2, pend - p1 - 2, multibyte))
                            {
                                DEBUG_PRINT3("  '%c' != '%c' => fast loop.\n", c, p1[2]);
                                return true;
                            }
                        }

                        else if ((re_opcode_t)p1.Value == re_opcode_t.charset
                             || (re_opcode_t)p1.Value == re_opcode_t.charset_not)
                        {
                            bool not = (re_opcode_t)p1.Value == re_opcode_t.charset_not;

                            /* Test if C is listed in charset (or charset_not)
                               at `p1'.  */
                            if (!multibyte || IS_REAL_ASCII(c))
                            {
                                if (c < CHARSET_BITMAP_SIZE(p1) * BYTEWIDTH
                                    && (p1[2 + (int)c / BYTEWIDTH] & (1 << ((int) c % BYTEWIDTH))) != 0 )
                                    not = !not;
                            }
                            else if (CHARSET_RANGE_TABLE_EXISTS_P(p1))
                                CHARSET_LOOKUP_RANGE_TABLE(ref not, c, p1);

                            /* `not' is equal to 1 if c would match, which means
                               that we can't change to pop_failure_jump.  */
                            if (!not)
                            {
                                DEBUG_PRINT1("	 No match => fast loop.\n");
                                return true;
                            }
                        }
                        else if ((re_opcode_t)p1.Value == re_opcode_t.anychar
                             && c == '\n')
                        {
                            DEBUG_PRINT1("   . != \\n => fast loop.\n");
                            return true;
                        }
                    }
                    break;

                case re_opcode_t.charset:
                    {
                        if ((re_opcode_t)p1.Value == re_opcode_t.exactn)
                            /* Reuse the code above.  */
                            return mutually_exclusive_p(bufp, p2, p1);

                          /* It is hard to list up all the character in charset
                         P2 if it includes multibyte character.  Give up in
                         such case.  */
                        else if (!multibyte || !CHARSET_RANGE_TABLE_EXISTS_P(p2))
                        {
                            /* Now, we are sure that P2 has no range table.
                               So, for the size of bitmap in P2, `p2[1]' is
                               enough.  But P1 may have range table, so the
                               size of bitmap table of P1 is extracted by
                               using macro `CHARSET_BITMAP_SIZE'.

                               In a multibyte case, we know that all the character
                               listed in P2 is ASCII.  In a unibyte case, P1 has only a
                               bitmap table.  So, in both cases, it is enough to test
                               only the bitmap table of P1.  */

                            if ((re_opcode_t)p1.Value == re_opcode_t.charset)
                            {
                                int idx;
                                /* We win if the charset inside the loop
                               has no overlap with the one after the loop.  */
                                for (idx = 0;
                                 (idx < (int)p2[1]
                                  && idx < CHARSET_BITMAP_SIZE(p1));
                                 idx++)
                                    if ((p2[2 + idx] & p1[2 + idx]) != 0)
                                        break;

                                if (idx == p2[1]
                                || idx == CHARSET_BITMAP_SIZE(p1))
                                {
                                    DEBUG_PRINT1("	 No match => fast loop.\n");
                                    return true;
                                }
                            }
                            else if ((re_opcode_t)p1.Value == re_opcode_t.charset_not)
                            {
                                int idx;
                                /* We win if the charset_not inside the loop lists
                               every character listed in the charset after.  */
                                for (idx = 0; idx < (int)p2[1]; idx++)
                                    if (!(p2[2 + idx] == 0
                                           || (idx < CHARSET_BITMAP_SIZE(p1)
                                           && ((p2[2 + idx] & ~p1[2 + idx]) == 0))))
                                        break;

                                if (idx == p2[1])
                                {
                                    DEBUG_PRINT1("	 No match => fast loop.\n");
                                    return true;
                                }
                            }
                        }
                    }
                    break;

                case re_opcode_t.charset_not:
                    switch ((re_opcode_t)p1.Value)
                    {
                        case re_opcode_t.exactn:
                        case re_opcode_t.charset:
                            /* Reuse the code above.  */
                            return mutually_exclusive_p(bufp, p2, p1);
                        case re_opcode_t.charset_not:
                            /* When we have two charset_not, it's very unlikely that
                               they don't overlap.  The union of the two sets of excluded
                               chars should cover all possible chars, which, as a matter of
                               fact, is virtually impossible in multibyte buffers.  */
                            break;
                    }
                    break;

                case re_opcode_t.wordend:
                    return ((re_opcode_t)p1.Value == re_opcode_t.syntaxspec && (syntaxcode)p1[1] == syntaxcode.Sword);
                case re_opcode_t.symend:
                    return ((re_opcode_t)p1.Value == re_opcode_t.syntaxspec
                            && ((syntaxcode)p1[1] == syntaxcode.Ssymbol || (syntaxcode)p1[1] == syntaxcode.Sword));
                case re_opcode_t.notsyntaxspec:
                    return ((re_opcode_t)p1.Value == re_opcode_t.syntaxspec && p1[1] == p2[1]);

                case re_opcode_t.wordbeg:
                    return ((re_opcode_t)p1.Value == re_opcode_t.notsyntaxspec && (syntaxcode)p1[1] == syntaxcode.Sword);
                case re_opcode_t.symbeg:
                    return ((re_opcode_t)p1.Value == re_opcode_t.notsyntaxspec
                            && ((syntaxcode)p1[1] == syntaxcode.Ssymbol || (syntaxcode)p1[1] == syntaxcode.Sword));
                case re_opcode_t.syntaxspec:
                    return ((re_opcode_t)p1.Value == re_opcode_t.notsyntaxspec && p1[1] == p2[1]);

                case re_opcode_t.wordbound:
                    return (((re_opcode_t)p1.Value == re_opcode_t.notsyntaxspec
                         || (re_opcode_t)p1.Value == re_opcode_t.syntaxspec)
                        && (syntaxcode)p1[1] == syntaxcode.Sword);

                case re_opcode_t.categoryspec:
                    return ((re_opcode_t)p1.Value == re_opcode_t.notcategoryspec && p1[1] == p2[1]);
                case re_opcode_t.notcategoryspec:
                    return ((re_opcode_t)p1.Value == re_opcode_t.categoryspec && p1[1] == p2[1]);

                default:
                    break;
            }

            /* Safe default.  */
            return false;
        }

        /* Store a jump with opcode OP at LOC to location TO.  We store a
           relative address offset by the three bytes the jump itself occupies.  */
        public static void STORE_JUMP(re_opcode_t op, PtrEmulator<byte> loc, PtrEmulator<byte> to) 
        {
            store_op1(op, loc, (to - loc) - 3);
        }

        /* Likewise, for a two-argument jump.  */
        public static void STORE_JUMP2(re_opcode_t op, PtrEmulator<byte> loc, PtrEmulator<byte> to, int arg) 
        {
            store_op2(op, loc, (to - loc) - 3, arg);
        }

        /* Like `STORE_JUMP', but for inserting.  Assume `b' is the buffer end.  */
        public static void INSERT_JUMP(re_opcode_t op, PtrEmulator<byte> loc, PtrEmulator<byte> to, PtrEmulator<byte> b) 
        {
            insert_op1(op, loc, (to) - (loc) - 3, b);
        }

        /* Like `STORE_JUMP2', but for inserting.  Assume `b' is the buffer end.  */
        public static void INSERT_JUMP2(re_opcode_t op, PtrEmulator<byte> loc, PtrEmulator<byte> to, int arg, PtrEmulator<byte> b)
        {
            insert_op2(op, loc, (to - loc) - 3, arg, b);
        }

        public static int TRANSLATE(int d, RE_TRANSLATE_TYPE translate)
        {
            return (RE_TRANSLATE_P(translate) ? RE_TRANSLATE(translate, (d)) : (d));
        }

        /* Both FROM and TO are unibyte characters (0x80..0xFF).  */
        public static void SETUP_UNIBYTE_RANGE(range_table_work_area work_area, int FROM, int TO, LispObject translate, PtrEmulator<byte> b)
        {
            int C0, C1, C2, I;
            int USED = RANGE_TABLE_WORK_USED(work_area);

            for (C0 = (FROM); C0 <= (TO); C0++)
            {
                C1 = (int) RE_CHAR_TO_MULTIBYTE(C0);
                if (CHAR_BYTE8_P((uint) C1))
                    SET_LIST_BIT(C0, b);
                else
                {
                    C2 = TRANSLATE(C1, translate);
                    if (C2 == C1
                    || (C1 = RE_CHAR_TO_UNIBYTE(C2)) < 0)
                        C1 = C0;
                    SET_LIST_BIT(C1, b);
                    for (I = RANGE_TABLE_WORK_USED(work_area) - 2; I >= USED; I -= 2)
                    {
                        int from = RANGE_TABLE_WORK_ELT(work_area, I);
                        int to = RANGE_TABLE_WORK_ELT(work_area, I + 1);

                        if (C2 >= from - 1 && C2 <= to + 1)
                        {
                            if (C2 == from - 1)
                                work_area.table[I]--;
                            // RANGE_TABLE_WORK_ELT(work_area, I)--;
                            else if (C2 == to + 1)
                                work_area.table[I + 1]++;
                            //RANGE_TABLE_WORK_ELT(work_area, I + 1)++;
                            break;
                        }
                    }
                    if (I < USED)
                        SET_RANGE_TABLE_WORK_AREA((work_area), C2, C2);
                }
            }
        }

/* Both FROM and TO are mulitbyte characters.  */
        public static void SETUP_MULTIBYTE_RANGE(range_table_work_area work_area, int FROM, int TO, LispObject translate, PtrEmulator<byte> b)
        {
            int C0, C1, C2, I, USED = RANGE_TABLE_WORK_USED(work_area);

            SET_RANGE_TABLE_WORK_AREA((work_area), (FROM), (TO));
            for (C0 = (FROM); C0 <= (TO); C0++)
            {
                C1 = TRANSLATE(C0, translate);
                if ((C2 = RE_CHAR_TO_UNIBYTE(C1)) >= 0
                    || (C1 != C0 && (C2 = RE_CHAR_TO_UNIBYTE(C0)) >= 0))
                    SET_LIST_BIT(C2, b);
                if (C1 >= (FROM) && C1 <= (TO))
                    continue;
                for (I = RANGE_TABLE_WORK_USED(work_area) - 2; I >= USED; I -= 2)
                {
                    int from = RANGE_TABLE_WORK_ELT(work_area, I);
                    int to = RANGE_TABLE_WORK_ELT(work_area, I + 1);

                    if (C1 >= from - 1 && C1 <= to + 1)
                    {
                        if (C1 == from - 1)
                            work_area.table[I]--;
                        //RANGE_TABLE_WORK_ELT(work_area, I)--;
                        else if (C1 == to + 1)
                            work_area.table[I + 1]++;
                        //RANGE_TABLE_WORK_ELT(work_area, I + 1)++;
                        break;
                    }
                }
                if (I < USED)
                    SET_RANGE_TABLE_WORK_AREA((work_area), C1, C1);
            }
        }


        public static reg_errcode_t regex_compile(byte[] pattern_col, int size, reg_syntax_t syntax, re_pattern_buffer bufp)
        {
            try
            {
                PtrEmulator<byte> pattern = new PtrEmulator<byte>(pattern_col);
                /* We fetch characters from PATTERN here.  */
                re_wchar_t c = 0, c1 = 0;

                /* A random temporary spot in PATTERN.  */
                PtrEmulator<re_char> p1 = new PtrEmulator<re_char>();

                /* Points to the end of the buffer, where we should append.  */
                PtrEmulator<byte> b;

                /* Keeps track of unclosed groups.  */
                compile_stack_type compile_stack = new compile_stack_type();

                /* Points to the current (ending) position in the pattern.  */
                PtrEmulator<re_char> p = new PtrEmulator<re_char>(pattern);
                PtrEmulator<re_char> pend = new PtrEmulator<re_char>(pattern, size);

                /* How to translate the characters in the pattern.  */
                RE_TRANSLATE_TYPE translate = bufp.translate;

                /* Address of the count-byte of the most recently inserted `exactn'
                   command.  This makes it possible to tell if a new exact-match
                   character can be added to that command or if the character requires
                   a new `exactn' command.  */
                PtrEmulator<byte> pending_exact = new PtrEmulator<byte>();

                /* Address of start of the most recently finished expression.
                   This tells, e.g., postfix * where to find the start of its
                   operand.  Reset at the beginning of groups and alternatives.  */
                PtrEmulator<byte> laststart = new PtrEmulator<byte>();

                /* Address of beginning of regexp, or inside of last group.  */
                PtrEmulator<byte> begalt;

                /* Place in the uncompiled pattern (i.e., the {) to
                   which to go back if the interval is invalid.  */
                PtrEmulator<re_char> beg_interval = new PtrEmulator<re_char>();

                /* Address of the place where a forward jump should go to the end of
                   the containing expression.  Each alternative of an `or' -- except the
                   last -- ends with a forward jump of this sort.  */
                PtrEmulator<byte> fixup_alt_jump = new PtrEmulator<byte>();

                /* Work area for range table of charset.  */
                range_table_work_area range_table_work = new range_table_work_area();

                /* If the object matched can contain multibyte characters.  */
                bool multibyte = RE_MULTIBYTE_P(bufp);

                /* If a target of matching can contain multibyte characters.  */
                bool target_multibyte = RE_TARGET_MULTIBYTE_P(bufp);

                /* Nonzero if we have pushed down into a subpattern.  */
                bool in_subpattern = false;

                /* These hold the values of p, pattern, and pend from the main
                   pattern when we have pushed into a subpattern.  */
                PtrEmulator<re_char> main_p = new PtrEmulator<re_char>();
                PtrEmulator<re_char> main_pattern = new PtrEmulator<re_char>();
                PtrEmulator<re_char> main_pend = new PtrEmulator<re_char>();

#if EMACS_DEBUG
            debug++;
            DEBUG_PRINT1 ("\nCompiling pattern: ");
            if (debug > 0)
            {
                uint debug_count;

                for (debug_count = 0; debug_count < size; debug_count++)
                    putchar (pattern[debug_count]);
                putchar ('\n');
            }
#endif // DEBUG

                /* Initialize the compile stack.  */
                compile_stack.stack = new compile_stack_elt_t[INIT_COMPILE_STACK_SIZE];
                if (compile_stack.stack == null)
                    return reg_errcode_t.REG_ESPACE;

                compile_stack.size = INIT_COMPILE_STACK_SIZE;
                compile_stack.avail = 0;

                range_table_work.table = null;
                range_table_work.allocated = 0;

                /* Initialize the pattern buffer.  */
                bufp.syntax = syntax;
                bufp.fastmap_accurate = false;
                bufp.not_bol = bufp.not_eol = false;
                bufp.used_syntax = false;

                /* Set `used' to zero, so that if we return an error, the pattern
                   printer (for debugging) will think there's no pattern.  We reset it
                   at the end.  */
                bufp.used = 0;

                /* Always count groups, whether or not bufp->no_sub is set.  */
                bufp.re_nsub = 0;

                if (bufp.allocated == 0)
                {
                    if (bufp.buffer != null)
                    { /* If zero allocated, but buffer is non-null, try to realloc
                     enough space.  This loses if buffer's address is bogus, but
                     that is the user's responsibility.  */
                        System.Array.Resize(ref bufp.buffer, INIT_BUF_SIZE);
                    }
                    else
                    { /* Caller did not allocate a buffer.  Do it for them.  */
                        bufp.buffer = new byte[INIT_BUF_SIZE];
                    }
                    if (bufp.buffer == null)
                    {
                        return reg_errcode_t.REG_ESPACE;
                    }

                    bufp.allocated = INIT_BUF_SIZE;
                }

                begalt = new PtrEmulator<byte>(bufp.buffer);
                b = new PtrEmulator<byte>(bufp.buffer);

                /* Loop through the uncompiled pattern until we're at the end.  */
                while (true)
                {
                    if (p == pend)
                    {
                        /* If this is the end of an included regexp,
                           pop back to the main regexp and try again.  */
                        if (in_subpattern)
                        {
                            in_subpattern = false;
                            pattern = new PtrEmulator<byte>(main_pattern);
                            p = new PtrEmulator<byte>(main_p);
                            pend = new PtrEmulator<byte>(main_pend);
                            continue;
                        }
                        /* If this is the end of the main regexp, we are done.  */
                        break;
                    }

                    PATFETCH(ref c, ref p, pend, multibyte);

                    switch (c)
                    {
                    case ' ':
                        {
                            PtrEmulator<re_char> p11 = new PtrEmulator<re_char>(p);

                            /* If there's no special whitespace regexp, treat
                               spaces normally.  And don't try to do this recursively.  */
                            if (whitespace_regexp == null || in_subpattern)
                                goto normal_char;

                            /* Peek past following spaces.  */
                            while (p11 != pend)
                            {
                                if (p11.Value != ' ')
                                    break;
                                p11++;
                            }
                            /* If the spaces are followed by a repetition op,
                               treat them normally.  */
                            if (p11 != pend
                                && (p11.Value == '*' || p11.Value == '+' || p11.Value == '?'
                                    || (p11.Value == '\\' && p11 + 1 != pend && p11[1] == '{')))
                                goto normal_char;

                            /* Replace the spaces with the whitespace regexp.  */
                            in_subpattern = true;
                            main_p = new PtrEmulator<re_char>(p11);
                            main_pend = new PtrEmulator<re_char>(pend);
                            main_pattern = new PtrEmulator<re_char>(pattern);
                            p = new PtrEmulator<byte>(whitespace_regexp);
                            pattern = new PtrEmulator<byte>(whitespace_regexp);
                            pend = new PtrEmulator<byte>(p, p.Length);
                            break;
                        }

                    case '^':
                        {
                            if (   /* If at start of pattern, it's an operator.  */
                                p == pattern + 1
                                /* If context independent, it's an operator.  */
                                || (syntax & RE_CONTEXT_INDEP_ANCHORS) != 0
                                /* Otherwise, depends on what's come before.  */
                                || at_begline_loc_p(pattern, p, syntax))
                                BUF_PUSH((int)((syntax & RE_NO_NEWLINE_ANCHOR) != 0 ? re_opcode_t.begbuf : re_opcode_t.begline),
                                         bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            else
                                goto normal_char;
                        }
                        break;


                    case '$':
                        {
                            if (   /* If at end of pattern, it's an operator.  */
                                p == pend
                                /* If context independent, it's an operator.  */
                                || (syntax & RE_CONTEXT_INDEP_ANCHORS) != 0
                                /* Otherwise, depends on what's next.  */
                                || at_endline_loc_p(p, pend, syntax))
                                BUF_PUSH((int)((syntax & RE_NO_NEWLINE_ANCHOR) != 0 ? re_opcode_t.endbuf : re_opcode_t.endline),
                                         bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            else
                                goto normal_char;
                        }
                        break;


                    case '+':
                    case '?':
                        if ((syntax & RE_BK_PLUS_QM) != 0
                            || (syntax & RE_LIMITED_OPS) != 0)
                            goto normal_char;
                        goto case '*';
                    case '*':
                        handle_plus:
                        /* If there is no previous pattern... */
                        if (!laststart)
                        {
                            if ((syntax & RE_CONTEXT_INVALID_OPS) != 0)
                                return reg_errcode_t.REG_BADRPT;
                            else if ((syntax & RE_CONTEXT_INDEP_OPS) == 0)
                                goto normal_char;
                        }
                    {
                        /* 1 means zero (many) matches is allowed.  */
                        bool zero_times_ok = false, many_times_ok = false;
                        bool greedy = true;

                        /* If there is a sequence of repetition chars, collapse it
                           down to just one (the right one).  We can't combine
                           interval operators with these because of, e.g., `a{2}*',
                           which should only match an even number of `a's.  */

                        for (; ; )
                        {
                            if ((syntax & RE_FRUGAL) != 0
                                && c == '?' && (zero_times_ok || many_times_ok))
                                greedy = false;
                            else
                            {
                                zero_times_ok |= c != '+';
                                many_times_ok |= c != '?';
                            }

                            if (p == pend)
                                break;
                            else if (p.Value == '*'
                                     || ((syntax & RE_BK_PLUS_QM) == 0
                                         && (p.Value == '+' || p.Value == '?')))
                            {
                            }
                            else if ((syntax & RE_BK_PLUS_QM) != 0 && p.Value == '\\')
                            {
                                if (p + 1 == pend)
                                    return reg_errcode_t.REG_EESCAPE;
                                if (p[1] == '+' || p[1] == '?')
                                    PATFETCH(ref c, ref p, pend, multibyte); /* Gobble up the backslash.  */
                                else
                                    break;
                            }
                            else
                                break;
                            /* If we get here, we found another repeat character.  */
                            PATFETCH(ref c, ref p, pend, multibyte);
                        }

                        /* Star, etc. applied to an empty pattern is equivalent
                           to an empty pattern.  */
                        if (!laststart || laststart == b)
                            break;

                        /* Now we know whether or not zero matches is allowed
                           and also whether or not two or more matches is allowed.  */
                        if (greedy)
                        {
                            if (many_times_ok)
                            {
                                bool simple = skip_one_char(new PtrEmulator<byte>(laststart)) == b;
                                int startoffset = 0;
                                re_opcode_t ofj =
                                    /* Check if the loop can match the empty string.  */
                                    (simple || analyse_first(laststart, b, null, false) == 0)
                                    ? re_opcode_t.on_failure_jump : re_opcode_t.on_failure_jump_loop;
                                //assert (skip_one_char (laststart) <= b);

                                if (!zero_times_ok && simple)
                                { /* Since simple * loops can be made faster by using
                                     on_failure_keep_string_jump, we turn simple P+
                                     into PP* if P is simple.  */
                                    PtrEmulator<byte> p1x, p2;
                                    startoffset = (b - laststart);
                                    GET_BUFFER_SPACE(startoffset, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                                    p1x = new PtrEmulator<byte>(b); p2 = new PtrEmulator<byte>(laststart);
                                    while (p2 < p1)
                                    {
                                        b.Value = p2.Value;
                                        b++; p2++;
                                    }
                                    zero_times_ok = true;
                                }

                                GET_BUFFER_SPACE(6, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                                if (!zero_times_ok)
                                    /* A + loop.  */
                                    STORE_JUMP(ofj, b, b + 6);
                                else
                                    /* Simple * loops can use on_failure_keep_string_jump
                                       depending on what follows.  But since we don't know
                                       that yet, we leave the decision up to
                                       on_failure_jump_smart.  */
                                    INSERT_JUMP(simple ? re_opcode_t.on_failure_jump_smart : ofj,
                                                laststart + startoffset, b + 6, b);
                                b += 3;
                                STORE_JUMP(re_opcode_t.jump, b, laststart + startoffset);
                                b += 3;
                            }
                            else
                            {
                                /* A simple ? pattern.  */
                                // assert (zero_times_ok);
                                GET_BUFFER_SPACE(3, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                                INSERT_JUMP(re_opcode_t.on_failure_jump, laststart, b + 3, b);
                                b += 3;
                            }
                        }
                        else		/* not greedy */
                        { /* I wish the greedy and non-greedy cases could be merged. */

                            GET_BUFFER_SPACE(7, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact); /* We might use less.  */
                            if (many_times_ok)
                            {
                                bool emptyp = analyse_first(laststart, b, null, false) != 0;

                                /* The non-greedy multiple match looks like
                                   a repeat..until: we only need a conditional jump
                                   at the end of the loop.  */
                                if (emptyp) BUF_PUSH((int)re_opcode_t.no_op, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                                STORE_JUMP(emptyp ? re_opcode_t.on_failure_jump_nastyloop
                                           : re_opcode_t.on_failure_jump, b, laststart);
                                b += 3;
                                if (zero_times_ok)
                                {
                                    /* The repeat...until naturally matches one or more.
                                       To also match zero times, we need to first jump to
                                       the end of the loop (its conditional jump).  */
                                    INSERT_JUMP(re_opcode_t.jump, laststart, b, b);
                                    b += 3;
                                }
                            }
                            else
                            {
                                /* non-greedy a?? */
                                INSERT_JUMP(re_opcode_t.jump, laststart, b + 3, b);
                                b += 3;
                                INSERT_JUMP(re_opcode_t.on_failure_jump, laststart, laststart + 6, b);
                                b += 3;
                            }
                        }
                    }
                    pending_exact = new PtrEmulator<byte>();
                    break;


                    case '.':
                        laststart = b;
                        BUF_PUSH((int)re_opcode_t.anychar, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                        break;


                    case '[':
                        {
                            CLEAR_RANGE_TABLE_WORK_USED(range_table_work);

                            if (p == pend) return reg_errcode_t.REG_EBRACK;

                            /* Ensure that we have enough space to push a charset: the
                               opcode, the length count, and the bitset; 34 bytes in all.  */
                            GET_BUFFER_SPACE(34, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);

                            laststart = new PtrEmulator<byte>(b);

                            /* We test `*p == '^' twice, instead of using an if
                               statement, so we only need one BUF_PUSH.  */
                            BUF_PUSH((int)(p.Value == '^' ? re_opcode_t.charset_not : re_opcode_t.charset),
                                     bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            if (p.Value == '^')
                                p++;

                            /* Remember the first position in the bracket expression.  */
                            p1 = new PtrEmulator<byte>(p);

                            /* Push the number of bytes in the bitmap.  */
                            BUF_PUSH((int)((1 << BYTEWIDTH) / BYTEWIDTH),
                                     bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);

                            /* Clear the whole map.  */
                            b.bzero((1 << BYTEWIDTH) / BYTEWIDTH, 0);

                            /* charset_not matches newline according to a syntax bit.  */
                            if ((re_opcode_t)b[-2] == re_opcode_t.charset_not
                                && (syntax & RE_HAT_LISTS_NOT_NEWLINE) != 0)
                                SET_LIST_BIT('\n', b);

                            /* Read in characters and ranges, setting map bits.  */
                            for (; ; )
                            {
                                bool escaped_char = false;
                                PtrEmulator<byte> p2 = new PtrEmulator<byte>(p);
                                re_wchar_t ch;

                                if (p == pend) return reg_errcode_t.REG_EBRACK;

                                /* Don't translate yet.  The range TRANSLATE(X..Y) cannot
                                   always be determined from TRANSLATE(X) and TRANSLATE(Y)
                                   So the translation is done later in a loop.  Example:
                                   (let ((case-fold-search t)) (string-match "[A-_]" "A"))  */
                                PATFETCH(ref c, ref p, pend, multibyte);

                                /* \ might escape characters inside [...] and [^...].  */
                                if ((syntax & RE_BACKSLASH_ESCAPE_IN_LISTS) != 0 && c == '\\')
                                {
                                    if (p == pend) return reg_errcode_t.REG_EESCAPE;

                                    PATFETCH(ref c, ref p, pend, multibyte);
                                    escaped_char = true;
                                }
                                else
                                {
                                    /* Could be the end of the bracket expression.  If it's
                                       not (i.e., when the bracket expression is `[]' so
                                       far), the ']' character bit gets set way below.  */
                                    if (c == ']' && p2 != p1)
                                        break;
                                }

                                /* See if we're at the beginning of a possible character
                                   class.  */

                                if (!escaped_char &&
                                    (syntax & RE_CHAR_CLASSES) != 0 && c == '[' && p.Value == ':')
                                {
                                    /* Leave room for the null.  */
                                    byte[] str = new byte[CHAR_CLASS_MAX_LENGTH + 1];
                                    PtrEmulator<byte> class_beg;

                                    PATFETCH(ref c, ref p, pend, multibyte);
                                    c1 = 0;
                                    class_beg = new PtrEmulator<byte>(p);

                                    /* If pattern is `[[:'.  */
                                    if (p == pend) return reg_errcode_t.REG_EBRACK;

                                    for (; ; )
                                    {
                                        PATFETCH(ref c, ref p, pend, multibyte);
                                        if ((c == ':' && p.Value == ']') || p == pend)
                                            break;
                                        if (c1 < CHAR_CLASS_MAX_LENGTH)
                                            str[c1++] = (byte)c;
                                        else
                                        {
                                            /* This is in any case an invalid class name.  */
                                            // str[0] = '\0';
                                        }
                                    }
                                    // str[c1] = '\0';

                                    /* If isn't a word bracketed by `[:' and `:]':
                                       undo the ending character, the letters, and
                                       leave the leading `:' and `[' (but set bits for
                                       them).  */
                                    if (c == ':' && p.Value == ']')
                                    {
                                        re_wctype_t cc;

                                        cc = re_wctype(str);

                                        if (cc == 0)
                                            return reg_errcode_t.REG_ECTYPE;

                                        /* Throw away the ] at the end of the character
                                           class.  */
                                        PATFETCH(ref c, ref p, pend, multibyte);

                                        if (p == pend) return reg_errcode_t.REG_EBRACK;

                                        /*#ifndef emacs
                                          for (ch = 0; ch < (1 << BYTEWIDTH); ++ch)
                                          if (re_iswctype (btowc (ch), cc))
                                          {
                                          c = TRANSLATE (ch);
                                          if (c < (1 << BYTEWIDTH))
                                          SET_LIST_BIT (c);
                                          }
                                          #else  /* emacs */
                                        /* Most character classes in a multibyte match
                                           just set a flag.  Exceptions are is_blank,
                                           is_digit, is_cntrl, and is_xdigit, since
                                           they can only match ASCII characters.  We
                                           don't need to handle them for multibyte.
                                           They are distinguished by a negative wctype.  */

                                        for (ch = 0; ch < 256; ++ch)
                                        {
                                            c = (int)RE_CHAR_TO_MULTIBYTE(ch);
                                            if (!CHAR_BYTE8_P((uint)c)
                                                && re_iswctype((uint)c, cc))
                                            {
                                                SET_LIST_BIT(ch, b);
                                                c1 = TRANSLATE(c, translate);
                                                if (c1 == c)
                                                    continue;
                                                if (ASCII_CHAR_P((uint)c1))
                                                    SET_LIST_BIT(c1, b);
                                                else if ((c1 = RE_CHAR_TO_UNIBYTE(c1)) >= 0)
                                                    SET_LIST_BIT(c1, b);
                                            }
                                        }
                                        SET_RANGE_TABLE_WORK_AREA_BIT
                                            (range_table_work, re_wctype_to_bit(cc));
                                        // #endif	/* emacs */
                                        /* In most cases the matching rule for char classes
                                           only uses the syntax table for multibyte chars,
                                           so that the content of the syntax-table it is not
                                           hardcoded in the range_table.  SPACE and WORD are
                                           the two exceptions.  */
                                        if (((1 << (int) cc) & ((1 << (int) re_wctype_t.RECC_SPACE) | (1 << (int) re_wctype_t.RECC_WORD))) != 0)
                                            bufp.used_syntax = true;

                                        /* Repeat the loop. */
                                        continue;
                                    }
                                    else
                                    {
                                        /* Go back to right after the "[:".  */
                                        p = new PtrEmulator<byte>(class_beg);
                                        SET_LIST_BIT('[', b);

                                        /* Because the `:' may starts the range, we
                                           can't simply set bit and repeat the loop.
                                           Instead, just set it to C and handle below.  */
                                        c = ':';
                                    }
                                }

                                if (p < pend && p[0] == '-' && p[1] != ']')
                                {

                                    /* Discard the `-'. */
                                    PATFETCH(ref c1, ref p, pend, multibyte);

                                    /* Fetch the character which ends the range. */
                                    PATFETCH(ref c1, ref p, pend, multibyte);
                                    //#ifdef emacs
                                    if (CHAR_BYTE8_P((uint)c1)
                                        && !ASCII_CHAR_P((uint)c) && !CHAR_BYTE8_P((uint)c))
                                        /* Treat the range from a multibyte character to
                                           raw-byte character as empty.  */
                                        c = c1 + 1;
                                    //#endif	/* emacs */
                                }
                                else
                                    /* Range from C to C. */
                                    c1 = c;

                                if (c > c1)
                                {
                                    if ((syntax & RE_NO_EMPTY_RANGES) != 0)
                                        return reg_errcode_t.REG_ERANGEX;
                                    /* Else, repeat the loop.  */
                                }
                                else
                                {
                                    /*#if !emacs
                                    // Set the range into bitmap 
                                    for (; c <= c1; c++)
                                    {
                                    ch = TRANSLATE (c);
                                    if (ch < (1 << BYTEWIDTH))
                                    SET_LIST_BIT (ch);
                                    }
                                    // #else  // emacs */
                                    if (c < 128)
                                    {
                                        ch = System.Math.Min(127, c1);
                                        SETUP_ASCII_RANGE(range_table_work, c, ch, translate, b);
                                        c = ch + 1;
                                        if (CHAR_BYTE8_P((uint) c1))
                                            c = (int) BYTE8_TO_CHAR(128);
                                    }
                                    if (c <= c1)
                                    {
                                        if (CHAR_BYTE8_P((uint) c))
                                        {
                                            c = (int) CHAR_TO_BYTE8((uint) c);
                                            c1 = (int) CHAR_TO_BYTE8((uint) c1);
                                            for (; c <= c1; c++)
                                                SET_LIST_BIT(c, b);
                                        }
                                        else if (multibyte)
                                        {
                                            SETUP_MULTIBYTE_RANGE(range_table_work, c, c1, translate, b);
                                        }
                                        else
                                        {
                                            SETUP_UNIBYTE_RANGE(range_table_work, c, c1, translate, b);
                                        }
                                    }
                                    // #endif // emacs 
                                }
                            }

                            /* Discard any (non)matching list bytes that are all 0 at the
                               end of the map.  Decrease the map-length byte too.  */
                            while ((int)b[-1] > 0 && b[b[-1] - 1] == 0)
                                b[-1]--;
                            b += b[-1];

                            /* Build real range table from work area.  */
                            if (RANGE_TABLE_WORK_USED(range_table_work) != 0
                                || RANGE_TABLE_WORK_BITS(range_table_work) != 0)
                            {
                                int i;
                                int used = RANGE_TABLE_WORK_USED(range_table_work);

                                /* Allocate space for COUNT + RANGE_TABLE.  Needs two
                                   bytes for flags, two for COUNT, and three bytes for
                                   each character. */
                                GET_BUFFER_SPACE(4 + used * 3, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);

                                /* Indicate the existence of range table.  */
                                laststart[1] |= 0x80;

                                /* Store the character class flag bits into the range table.
                                   If not in emacs, these flag bits are always 0.  */
                                b.Value = (byte)(RANGE_TABLE_WORK_BITS(range_table_work) & 0xff); b++;
                                b.Value = (byte)(RANGE_TABLE_WORK_BITS(range_table_work) >> 8); b++;

                                STORE_NUMBER_AND_INCR(ref b, used / 2);
                                for (i = 0; i < used; i++)
                                    STORE_CHARACTER_AND_INCR
                                        (ref b, RANGE_TABLE_WORK_ELT(range_table_work, i));
                            }
                        }
                        break;


                    case '(':
                        if ((syntax & RE_NO_BK_PARENS) != 0)
                        {
                            handle_open(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval, ref compile_stack);
                            break;
                        }
                        else
                            goto normal_char;


                    case ')':
                        if ((syntax & RE_NO_BK_PARENS) != 0)
                        {
                            handle_close(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval, ref compile_stack);
                            break;
                        }
                        else
                            goto normal_char;


                    case '\n':
                        if ((syntax & RE_NEWLINE_ALT) != 0)
                        {
                            handle_alt(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                            break;
                        }
                        else
                            goto normal_char;


                    case '|':
                        if ((syntax & RE_NO_BK_VBAR) != 0)
                        {
                            handle_alt(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                            break;
                        }
                        else
                            goto normal_char;


                    case '{':
                        if ((syntax & RE_INTERVALS) != 0 && (syntax & RE_NO_BK_BRACES) != 0)
                        {
                            handle_interval(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval);
                            break;
                        }
                        else
                            goto normal_char;

                    case '\\':
                        if (p == pend) return reg_errcode_t.REG_EESCAPE;

                        /* Do not translate the character after the \, so that we can
                           distinguish, e.g., \B from \b, even if we normally would
                           translate, e.g., B to b.  */
                        PATFETCH(ref c, ref p, pend, multibyte);

                        switch (c)
                        {
                        case '(':
                            if ((syntax & RE_NO_BK_PARENS) != 0)
                                goto normal_backslash;

                            handle_open(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval, ref compile_stack);
                            break;                            

                        case ')':
                            if ((syntax & RE_NO_BK_PARENS) != 0) goto normal_backslash;

                            if (COMPILE_STACK_EMPTY(compile_stack))
                            {
                                if ((syntax & RE_UNMATCHED_RIGHT_PAREN_ORD) != 0)
                                    goto normal_backslash;
                                else
                                    return reg_errcode_t.REG_ERPAREN;
                            }

                            handle_close(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval, ref compile_stack);

                            break;


                        case '|':					/* `\|'.  */
                            if ((syntax & RE_LIMITED_OPS) != 0 || (syntax & RE_NO_BK_VBAR) != 0)
                                goto normal_backslash;
                            handle_alt(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                            break;


                        case '{':
                            /* If \{ is a literal.  */
                            if ((syntax & RE_INTERVALS) == 0
                                /* If we're at `\{' and it's not the open-interval
                                   operator.  */
                                || (syntax & RE_NO_BK_BRACES) != 0)
                                goto normal_backslash;

                            handle_interval(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart,
                                ref multibyte, ref translate, ref p, ref pend, ref bufp, ref begalt,
                                ref fixup_alt_jump, ref beg_interval);
                            break;

                            // #ifdef emacs
                            /* There is no way to specify the before_dot and after_dot
                               operators.  rms says this is ok.  --karl  */
                        case '=':
                            BUF_PUSH((int) re_opcode_t.at_dot, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case 's':
                            laststart = new PtrEmulator<byte>(b);
                            PATFETCH(ref c, ref p, pend, multibyte);
                            BUF_PUSH_2((int) re_opcode_t.syntaxspec, syntax_spec_code[c], bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case 'S':
                            laststart = new PtrEmulator<byte>(b);
                            PATFETCH(ref c, ref p, pend, multibyte);
                            BUF_PUSH_2((int) re_opcode_t.notsyntaxspec, syntax_spec_code[c], bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case 'c':
                            laststart = new PtrEmulator<byte>(b);
                            PATFETCH(ref c, ref p, pend, multibyte);
                            BUF_PUSH_2((int) re_opcode_t.categoryspec, c, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case 'C':
                            laststart = new PtrEmulator<byte>(b);
                            PATFETCH(ref c, ref p, pend, multibyte);
                            BUF_PUSH_2((int) re_opcode_t.notcategoryspec, c, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;
                            // #endif /* emacs */


                        case 'w':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            laststart = new PtrEmulator<byte>(b);
                            BUF_PUSH_2((int) re_opcode_t.syntaxspec, (int) syntaxcode.Sword, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;


                        case 'W':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            laststart = new PtrEmulator<byte>(b);
                            BUF_PUSH_2((int) re_opcode_t.notsyntaxspec, (int) syntaxcode.Sword, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;


                        case '<':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.wordbeg, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case '>':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.wordend, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case '_':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            laststart = new PtrEmulator<byte>(b);
                            PATFETCH(ref c, ref p, pend, multibyte);
                            if (c == '<')
                                BUF_PUSH((int) re_opcode_t.symbeg, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            else if (c == '>')
                                BUF_PUSH((int) re_opcode_t.symend, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            else
                                return reg_errcode_t.REG_BADPAT;
                            break;

                        case 'b':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.wordbound, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case 'B':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.notwordbound, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case '`':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.begbuf, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case '\'':
                            if ((syntax & RE_NO_GNU_OPS) != 0)
                                goto normal_char;
                            BUF_PUSH((int) re_opcode_t.endbuf, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            break;

                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            {
                                regnum_t reg;

                                if ((syntax & RE_NO_BK_REFS) != 0)
                                    goto normal_backslash;

                                reg = c - '0';

                                if (reg > bufp.re_nsub || reg < 1
                                    /* Can't back reference to a subexp before its end.  */
                                    || group_in_compile_stack(compile_stack, reg))
                                    return reg_errcode_t.REG_ESUBREG;

                                laststart = new PtrEmulator<byte>(b);
                                BUF_PUSH_2((int) re_opcode_t.duplicate, reg, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                            }
                            break;


                        case '+':
                        case '?':
                            if ((syntax & RE_BK_PLUS_QM) != 0)
                                goto handle_plus;
                            else
                                goto normal_backslash;

                        default:
                            normal_backslash:
                            /* You might think it would be useful for \ to mean
                               not to translate; but if we don't translate it
                               it will never match anything.  */
                            goto normal_char;
                        }
                        break;


                    default:
                    normal_char:
                        normal_char(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                                    ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                        break;
                    } /* switch (c) */
                } /* while p != pend */


                /* Through the pattern now.  */

                FIXUP_ALT_JUMP(fixup_alt_jump, b);

                if (!COMPILE_STACK_EMPTY(compile_stack))
                    return reg_errcode_t.REG_EPAREN;

                /* If we don't want backtracking, force success
                   the first time we reach the end of the compiled pattern.  */
                if ((syntax & RE_NO_POSIX_BACKTRACKING) != 0)
                    BUF_PUSH((int) re_opcode_t.succeed, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);

                /* We have succeeded; set the length of the buffer.  */
                bufp.used = b - bufp.buffer;

#if EMACS_DEBUG
            if (debug > 0)
            {
                re_compile_fastmap (bufp);
                DEBUG_PRINT1 ("\nCompiled pattern: \n");
                print_compiled_pattern (bufp);
            }
            debug--;
#endif // DEBUG

#if MATCH_MAY_NOT_ALLOCATE
            /* Initialize the failure stack to the largest possible stack.  This
               isn't necessary unless we're trying to avoid calling alloca in
               the search and match routines.  */
            {
                int num_regs = bufp.re_nsub + 1;

                if (fail_stack.size < re_max_failures * TYPICAL_FAILURE_SIZE)
                {
                    fail_stack.size = re_max_failures * TYPICAL_FAILURE_SIZE;

                    if (! fail_stack.stack)
                        fail_stack.stack
                            = (fail_stack_elt_t *) malloc (fail_stack.size
                                                           * sizeof (fail_stack_elt_t));
                    else
                        fail_stack.stack
                            = (fail_stack_elt_t *) realloc (fail_stack.stack,
                                                            (fail_stack.size
                                                             * sizeof (fail_stack_elt_t)));
                }

                regex_grow_registers (num_regs);
            }
#endif // not MATCH_MAY_ALLOCATE

                return reg_errcode_t.REG_NOERROR;
            }
            catch (RegexException e)
            {
                return e.Value;
            }
        }

        public static void handle_open(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump, ref PtrEmulator<byte> beg_interval,
ref compile_stack_type compile_stack)
        {
            bool shy = false;
            regnum_t regnum = 0;
            if (p + 1 < pend)
            {
                /* Look for a special (?...) construct */
                if ((syntax & RE_SHY_GROUPS) != 0 && p.Value == '?')
                {
                    PATFETCH(ref c, ref p, pend, multibyte); /* Gobble up the '?'.  */
                    while (!shy)
                    {
                        PATFETCH(ref c, ref p, pend, multibyte);
                        switch (c)
                        {
                            case ':': shy = true; break;
                            case '0':
                                /* An explicitly specified regnum must start
                                   with non-0. */
                                if (regnum == 0)
                                    throw new RegexException(reg_errcode_t.REG_BADPAT);
                                goto case '1';
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                                regnum = 10 * regnum + (c - '0'); break;
                            default:
                                /* Only (?:...) is supported right now. */
                                throw new RegexException(reg_errcode_t.REG_BADPAT);
                        }
                    }
                }
            }

            if (!shy)
                regnum = ++bufp.re_nsub;
            else if (regnum != 0)
            { /* It's actually not shy, but explicitly numbered.  */
                shy = false;
                if (regnum > bufp.re_nsub)
                    bufp.re_nsub = regnum;
                else if (regnum > bufp.re_nsub
                    /* Ideally, we'd want to check that the specified
                       group can't have matched (i.e. all subgroups
                       using the same regnum are in other branches of
                       OR patterns), but we don't currently keep track
                       of enough info to do that easily.  */
                         || group_in_compile_stack(compile_stack, regnum))
                    throw new RegexException(reg_errcode_t.REG_BADPAT);
            }
            else
                /* It's really shy.  */
                regnum = -bufp.re_nsub;

            if (COMPILE_STACK_FULL(compile_stack))
            {
                System.Array.Resize(ref compile_stack.stack, ((int)compile_stack.size << 1));
                if (compile_stack.stack == null) throw new RegexException(reg_errcode_t.REG_ESPACE);

                compile_stack.size <<= 1;
            }

            /* These are the values to restore when we hit end of this
               group.  They are all relative offsets, so that if the
               whole pattern moves because of realloc, they will still
               be valid.  */
            COMPILE_STACK_TOP(compile_stack).begalt_offset = begalt - bufp.buffer;
            COMPILE_STACK_TOP(compile_stack).fixup_alt_jump
                = fixup_alt_jump ? fixup_alt_jump - bufp.buffer + 1 : 0;
            COMPILE_STACK_TOP(compile_stack).laststart_offset = b - bufp.buffer;
            COMPILE_STACK_TOP(compile_stack).regnum = regnum;

            /* Do not push a start_memory for groups beyond the last one
               we can represent in the compiled pattern.  */
            if (regnum <= MAX_REGNUM && regnum > 0)
                BUF_PUSH_2((int)re_opcode_t.start_memory, regnum, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);

            compile_stack.avail++;

            fixup_alt_jump = new PtrEmulator<byte>();
            laststart = new PtrEmulator<byte>();
            begalt = new PtrEmulator<byte>(b);
            /* If we've reached MAX_REGNUM groups, then this open
               won't actually generate any code, so we'll have to
               clear pending_exact explicitly.  */
            pending_exact = new PtrEmulator<byte>();
        }

        public static void handle_close(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump, ref PtrEmulator<byte> beg_interval,
ref compile_stack_type compile_stack)
        {
            FIXUP_ALT_JUMP(fixup_alt_jump, b);

            /* See similar code for backslashed left paren above.  */
            if (COMPILE_STACK_EMPTY(compile_stack))
            {
                if ((syntax & RE_UNMATCHED_RIGHT_PAREN_ORD) != 0)
                {
                    normal_char(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                        ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                    return;
                }
                else
                    throw new RegexException(reg_errcode_t.REG_ERPAREN);
            }

            /* Since we just checked for an empty stack above, this
               ``can't happen''.  */
            // assert(compile_stack.avail != 0);
            {
                /* We don't just want to restore into `regnum', because
                   later groups should continue to be numbered higher,
                   as in `(ab)c(de)' -- the second group is #2.  */
                regnum_t regnum;

                compile_stack.avail--;
                begalt = new PtrEmulator<byte>(bufp.buffer, (int)COMPILE_STACK_TOP(compile_stack).begalt_offset);
                fixup_alt_jump
                    = COMPILE_STACK_TOP(compile_stack).fixup_alt_jump != 0
                    ? new PtrEmulator<byte>(bufp.buffer, (int)(COMPILE_STACK_TOP(compile_stack).fixup_alt_jump - 1))
                    : new PtrEmulator<byte>();
                laststart = new PtrEmulator<byte>(bufp.buffer, (int)COMPILE_STACK_TOP(compile_stack).laststart_offset);
                regnum = COMPILE_STACK_TOP(compile_stack).regnum;
                /* If we've reached MAX_REGNUM groups, then this open
                   won't actually generate any code, so we'll have to
                   clear pending_exact explicitly.  */
                pending_exact = new PtrEmulator<byte>();

                /* We're at the end of the group, so now we know how many
                   groups were inside this one.  */
                if (regnum <= MAX_REGNUM && regnum > 0)
                    BUF_PUSH_2((int)re_opcode_t.stop_memory, regnum, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            }
        }

        public static void handle_interval(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump, ref PtrEmulator<byte> beg_interval)
        {
            /* If got here, then the syntax allows intervals.  */

            /* At least (most) this many matches must be made.  */
            int lower_bound = 0, upper_bound = -1;

            beg_interval = new PtrEmulator<byte>(p);

            GET_UNSIGNED_NUMBER(ref lower_bound, ref c, ref p, pend, multibyte);

            if (c == ',')
                GET_UNSIGNED_NUMBER(ref upper_bound, ref c, ref p, pend, multibyte);
            else
                /* Interval such as `{1}' => match exactly once. */
                upper_bound = lower_bound;

            if (lower_bound < 0 || upper_bound > RE_DUP_MAX
                || (upper_bound >= 0 && lower_bound > upper_bound))
                throw new RegexException(reg_errcode_t.REG_BADBR);

            if ((syntax & RE_NO_BK_BRACES) == 0)
            {
                if (c != '\\')
                    throw new RegexException(reg_errcode_t.REG_BADBR);
                if (p == pend)
                    throw new RegexException(reg_errcode_t.REG_EESCAPE);
                PATFETCH(ref c, ref p, pend, multibyte);
            }

            if (c != '}')
                throw new RegexException(reg_errcode_t.REG_BADBR);

            /* We just parsed a valid interval.  */

            /* If it's invalid to have no preceding re.  */
            if (!laststart)
            {
                if ((syntax & RE_CONTEXT_INVALID_OPS) != 0)
                    throw new RegexException(reg_errcode_t.REG_BADRPT);
                else if ((syntax & RE_CONTEXT_INDEP_OPS) != 0)
                    laststart = new PtrEmulator<byte>(b);
                else
                {
                    /* If an invalid interval, match the characters as literals.  */
                    // assert (beg_interval);
                    p = new PtrEmulator<byte>(beg_interval);
                    beg_interval = new PtrEmulator<byte>();

                    /* normal_char and normal_backslash need `c'.  */
                    c = '{';

                    if ((syntax & RE_NO_BK_BRACES) == 0)
                    {
                        // assert(p > pattern && p[-1] == '\\');
                        normal_backslash(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                            ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                        return;
                    }
                    else
                    {
                        normal_char(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                            ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                        return;
                    }
                }
            }

            if (upper_bound == 0)
                /* If the upper bound is zero, just drop the sub pattern
                   altogether.  */
                b = new PtrEmulator<byte>(laststart);
            else if (lower_bound == 1 && upper_bound == 1)
            {
                /* Just match it once: nothing to do here.  */
            }

            /* Otherwise, we have a nontrivial interval.  When
               we're all done, the pattern will look like:
               set_number_at <jump count> <upper bound>
               set_number_at <succeed_n count> <lower bound>
               succeed_n <after jump addr> <succeed_n count>
               <body of loop>
               jump_n <succeed_n addr> <jump count>
               (The upper bound and `jump_n' are omitted if
               `upper_bound' is 1, though.)  */
            else
            { /* If the upper bound is > 1, we need to insert
                                     more at the end of the loop.  */
                uint nbytes = (upper_bound < 0 ? 3u
                               : upper_bound > 1 ? 5u : 0);
                uint startoffset = 0;

                GET_BUFFER_SPACE(20, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact); /* We might use less.  */

                if (lower_bound == 0)
                {
                    /* A succeed_n that starts with 0 is really a
                       a simple on_failure_jump_loop.  */
                    INSERT_JUMP(re_opcode_t.on_failure_jump_loop, laststart,
                                b + (3 + (int)nbytes), b);
                    b += 3;
                }
                else
                {
                    /* Initialize lower bound of the `succeed_n', even
                       though it will be set during matching by its
                       attendant `set_number_at' (inserted next),
                       because `re_compile_fastmap' needs to know.
                       Jump to the `jump_n' we might insert below.  */
                    INSERT_JUMP2(re_opcode_t.succeed_n, laststart,
                                 b + (5 + (int)nbytes),
                                 lower_bound, b);
                    b += 5;

                    /* Code to initialize the lower bound.  Insert
                       before the `succeed_n'.  The `5' is the last two
                       bytes of this `set_number_at', plus 3 bytes of
                       the following `succeed_n'.  */
                    insert_op2(re_opcode_t.set_number_at, laststart, 5, lower_bound, b);
                    b += 5;
                    startoffset += 5;
                }

                if (upper_bound < 0)
                {
                    /* A negative upper bound stands for infinity,
                       in which case it degenerates to a plain jump.  */
                    STORE_JUMP(re_opcode_t.jump, b, laststart + (int)startoffset);
                    b += 3;
                }
                else if (upper_bound > 1)
                { /* More than one repetition is allowed, so
                                         append a backward jump to the `succeed_n'
                                         that starts this interval.

                                         When we've reached this during matching,
                                         we'll have matched the interval once, so
                                         jump back only `upper_bound - 1' times.  */
                    STORE_JUMP2(re_opcode_t.jump_n, b, laststart + (int)startoffset,
                                upper_bound - 1);
                    b += 5;

                    /* The location we want to set is the second
                       parameter of the `jump_n'; that is `b-2' as
                       an absolute address.  `laststart' will be
                       the `set_number_at' we're about to insert;
                       `laststart+3' the number to set, the source
                       for the relative address.  But we are
                       inserting into the middle of the pattern --
                       so everything is getting moved up by 5.
                       Conclusion: (b - 2) - (laststart + 3) + 5,
                       i.e., b - laststart.

                       We insert this at the beginning of the loop
                       so that if we fail during matching, we'll
                       reinitialize the bounds.  */
                    insert_op2(re_opcode_t.set_number_at, laststart, b - laststart,
                               upper_bound - 1, b);
                    b += 5;
                }
            }
            pending_exact = new PtrEmulator<byte>();
            beg_interval = new PtrEmulator<byte>();
        }

        public static void handle_alt(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump)
        {
            if ((syntax & RE_LIMITED_OPS) != 0)
            {
                normal_char(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                    ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);
                return;
            }

            /* Insert before the previous alternative a jump which
               jumps to this alternative if the former fails.  */
            GET_BUFFER_SPACE(3, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            INSERT_JUMP(re_opcode_t.on_failure_jump, begalt, b + 6, b);
            pending_exact = new PtrEmulator<byte>();
            b += 3;

            /* The alternative before this one has a jump after it
               which gets executed if it gets matched.  Adjust that
               jump so it will jump to this alternative's analogous
               jump (put in below, which in turn will jump to the next
               (if any) alternative's such jump, etc.).  The last such
               jump jumps to the correct final destination.  A picture:
               _____ _____
               |   | |   |
               |   v |   v
               a | b	 | c

               If we are at `b', then fixup_alt_jump right now points to a
               three-byte space after `a'.  We'll put in the jump, set
               fixup_alt_jump to right after `b', and leave behind three
               bytes which we'll fill in when we get to after `c'.  */

            FIXUP_ALT_JUMP(fixup_alt_jump, b);

            /* Mark and leave space for a jump after this alternative,
               to be filled in later either by next alternative or
               when know we're at the end of a series of alternatives.  */
            fixup_alt_jump = new PtrEmulator<byte>(b);
            GET_BUFFER_SPACE(3, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            b += 3;

            laststart = new PtrEmulator<byte>();
            begalt = new PtrEmulator<byte>(b);
        }

        public static void normal_backslash(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump)
        {
            normal_char(ref syntax, ref c, ref c1, ref b, ref pending_exact, ref laststart, ref multibyte,
                ref translate, ref p, ref pend, ref bufp, ref begalt, ref fixup_alt_jump);            
        }

        public static void normal_char(ref reg_syntax_t syntax, ref int c, ref int c1, ref PtrEmulator<byte> b, ref PtrEmulator<byte> pending_exact, ref PtrEmulator<byte> laststart,
ref bool multibyte, ref LispObject translate, ref PtrEmulator<byte> p, ref PtrEmulator<byte> pend, ref re_pattern_buffer bufp,
ref PtrEmulator<byte> begalt, ref PtrEmulator<byte> fixup_alt_jump)
        {
        /* Expects the character in `c'.  */
        // normal_char:
            /* If no exactn currently being built.  */
            if (!pending_exact

                /* If last exactn not at current position.  */
                || pending_exact + (pending_exact.Value + 1) != b

                /* We have only one byte following the exactn for the count.  */
                || pending_exact.Value >= (1 << BYTEWIDTH) - MAX_MULTIBYTE_LENGTH

                /* If followed by a repetition operator.  */
                || (p != pend && (p.Value == '*' || p.Value == '^'))
                || ((syntax & RE_BK_PLUS_QM) != 0
                    ? p + 1 < pend && p.Value == '\\' && (p[1] == '+' || p[1] == '?')
                    : p != pend && (p.Value == '+' || p.Value == '?'))
                || ((syntax & RE_INTERVALS) != 0
                    && ((syntax & RE_NO_BK_BRACES) != 0
                        ? p != pend && p.Value == '{'
                        : p + 1 < pend && p[0] == '\\' && p[1] == '{')))
            {
                /* Start building a new exactn.  */

                laststart = new PtrEmulator<byte>(b);

                BUF_PUSH_2((int)re_opcode_t.exactn, 0, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
                pending_exact = b + -1;
            }

            GET_BUFFER_SPACE(MAX_MULTIBYTE_LENGTH, bufp, b, begalt, fixup_alt_jump, laststart, pending_exact);
            {
                int len;

                if (multibyte)
                {
                    c = TRANSLATE(c, translate);
                    len = CHAR_STRING((uint)c, b.Collection, b.Index);
                    b += len;
                }
                else
                {
                    c1 = (int)RE_CHAR_TO_MULTIBYTE(c);
                    if (!CHAR_BYTE8_P((uint)c1))
                    {
                        re_wchar_t c2 = TRANSLATE(c1, translate);

                        if (c1 != c2 && (c1 = RE_CHAR_TO_UNIBYTE(c2)) >= 0)
                            c = c1;
                    }
                    b.Value = (byte)c; b++;
                    len = 1;
                }
                pending_exact.Value = (byte)(pending_exact.Value + len);
            }
        }

        public class fail_stack_elt_t
        {
            public PtrEmulator<re_char> pointer;
            /* This should be the biggest `int' that's no bigger than a pointer.  */
            public int integer;
        }

        public class fail_stack_type
        {
            public fail_stack_elt_t[] stack;
            public int size;
            public int avail;	/* Offset of next open position.  */
            public int frame;	/* Offset of the cur constructed frame.  */
        }

        public static bool FAIL_STACK_EMPTY(fail_stack_type fail_stack)
        {
            return (fail_stack.frame == 0);
        }

        public static bool FAIL_STACK_FULL(fail_stack_type fail_stack)
        {
            return (fail_stack.avail == fail_stack.size);
        }

        /* Approximate number of failure points for which to initially allocate space
           when matching.  If this number is exceeded, we allocate more
           space, so it is not a hard limit.  */
        public const int INIT_FAILURE_ALLOC = 20;

        /* Estimate the size of data pushed by a typical failure stack entry.
           An estimate is all we need, because all we use this for
           is to choose a limit for how big to make the failure stack.  */
        /* BEWARE, the value `20' is hard-coded in emacs.c:main().  */
        public const int TYPICAL_FAILURE_SIZE = 20;

        /* Define macros to initialize and free the failure stack.
           Do `return -2' if the alloc fails.  */
        public static void INIT_FAIL_STACK(fail_stack_type fail_stack)
        {
            fail_stack.stack = new fail_stack_elt_t[INIT_FAILURE_ALLOC];

            // if (fail_stack.stack == null)
                // return -2;

            fail_stack.size = INIT_FAILURE_ALLOC;
            fail_stack.avail = 0;
            fail_stack.frame = 0;
        }

        /* Double the size of FAIL_STACK, up to a limit
           which allows approximately `re_max_failures' items.

           Return 1 if succeeds, and 0 if either ran out of memory
           allocating space for it or it was already too large.

           REGEX_REALLOCATE_STACK requires `destination' be declared.   */

        /* Factor to increase the failure stack size by
           when we increase it.
           This used to be 2, but 2 was too wasteful
           because the old discarded stacks added up to as much space
           were as ultimate, maximum-size stack.  */
        public static int FAIL_STACK_GROWTH_FACTOR = 4;

        /* How many items can still be added to the stack without overflowing it.  */
        public static int REMAINING_AVAIL_SLOTS(fail_stack_type fail_stack)
        {
            return fail_stack.size - fail_stack.avail;
        }

        /* Note that 4400 was enough to cause a crash on Alpha OSF/1,
           whose default stack limit is 2mb.  In order for a larger
           value to work reliably, you have to try to make it accord
           with the process stack limit.  */
        public static int re_max_failures = 40000;

        public static bool GROW_FAIL_STACK(fail_stack_type fail_stack)
        {
            if (fail_stack.size >= re_max_failures)
                return false;

            fail_stack.size = System.Math.Min(re_max_failures, (fail_stack.size * FAIL_STACK_GROWTH_FACTOR));
            System.Array.Resize(ref fail_stack.stack, fail_stack.size);

            return true;
        }

        public static void ENSURE_FAIL_STACK(fail_stack_type fail_stack, int space)
        {
            while (REMAINING_AVAIL_SLOTS(fail_stack) <= space)
            {
                if (!GROW_FAIL_STACK(fail_stack))
                    return;

                DEBUG_PRINT2("\n  Doubled stack; size now: %d\n", fail_stack.size);
                DEBUG_PRINT2("	 slots available: %d\n", REMAINING_AVAIL_SLOTS(fail_stack));
            }
        }

        /* Push a pointer value onto the failure stack.
           Assumes the variable `fail_stack'.  Probably should only
           be called from within `PUSH_FAILURE_POINT'.  */
        public static void PUSH_FAILURE_POINTER(fail_stack_type fail_stack, PtrEmulator<re_char> item)
        {
            fail_stack.stack[fail_stack.avail++].pointer = item;
        }

        /* This pushes an integer-valued item onto the failure stack.
           Assumes the variable `fail_stack'.  Probably should only
           be called from within `PUSH_FAILURE_POINT'.  */
        public static void PUSH_FAILURE_INT(fail_stack_type fail_stack, int item)
        {
            fail_stack.stack[fail_stack.avail++].integer = item;
        }

        /* These three POP... operations complement the three PUSH... operations.
           All assume that `fail_stack' is nonempty.  */
        public static PtrEmulator<re_char> POP_FAILURE_POINTER(fail_stack_type fail_stack)
        {
            return fail_stack.stack[--fail_stack.avail].pointer;
        }
        public static int POP_FAILURE_INT(fail_stack_type fail_stack)
        {
            return fail_stack.stack[--fail_stack.avail].integer;
        }
        public static fail_stack_elt_t POP_FAILURE_ELT(fail_stack_type fail_stack)
        {
            return fail_stack.stack[--fail_stack.avail];
        }

        /* Push register NUM onto the stack.  */
        public static void PUSH_FAILURE_REG(fail_stack_type fail_stack, PtrEmulator<re_char>[] regstart, PtrEmulator<re_char>[] regend, int num)
        {
            ENSURE_FAIL_STACK(fail_stack, 3);
            DEBUG_PRINT4("    Push reg %d (spanning %p -> %p)\n", num, regstart[num], regend[num]);

            PUSH_FAILURE_POINTER(fail_stack, regstart[num]);
            PUSH_FAILURE_POINTER(fail_stack, regend[num]);
            PUSH_FAILURE_INT(fail_stack, num);
        }

        /* Change the counter's value to VAL, but make sure that it will
           be reset when backtracking.  */
        public static void PUSH_NUMBER(fail_stack_type fail_stack, PtrEmulator<re_char> ptr, int val)
        {
            int c = 0;
            ENSURE_FAIL_STACK(fail_stack, 3);
            EXTRACT_NUMBER(ref c, ptr);
            DEBUG_PRINT4("    Push number %p = %d -> %d\n", ptr, c, val);
            PUSH_FAILURE_INT(fail_stack, c);
            PUSH_FAILURE_POINTER(fail_stack, ptr);
            PUSH_FAILURE_INT(fail_stack, -1);
            STORE_NUMBER(ptr, val);
        }

        /* Individual items aside from the registers.  */
        public static int NUM_NONREG_ITEMS = 3;

        /* Used to examine the stack (to detect infinite loops).  */
        public static PtrEmulator<re_char> FAILURE_PAT(fail_stack_type fail_stack, int h)
        {
            return fail_stack.stack[h - 1].pointer;
        }

        public static PtrEmulator<re_char> FAILURE_STR(fail_stack_type fail_stack, int h)
        {
            return fail_stack.stack[h - 2].pointer;
        }

        public static int NEXT_FAILURE_HANDLE(fail_stack_type fail_stack, int h)
        {
            return fail_stack.stack[h - 3].integer;
        }

        public static int TOP_FAILURE_HANDLE(fail_stack_type fail_stack)
        {
            return fail_stack.frame;
        }

        /* Pop a saved register off the stack.  */
        public static void POP_FAILURE_REG_OR_COUNT(fail_stack_type fail_stack, PtrEmulator<re_char>[] regstart, PtrEmulator<re_char>[] regend)
        {
            int reg = POP_FAILURE_INT(fail_stack);
            if (reg == -1)
            {
                /* It's a counter.  */
                /* Here, we discard `const', making re_match non-reentrant.  */
                PtrEmulator<re_char> ptr = POP_FAILURE_POINTER(fail_stack);
                reg = POP_FAILURE_INT(fail_stack);
                STORE_NUMBER(ptr, reg);
                DEBUG_PRINT3("     Pop counter %p = %d\n", ptr, reg);
            }
            else
            {
                regend[reg] = POP_FAILURE_POINTER(fail_stack);
                regstart[reg] = POP_FAILURE_POINTER(fail_stack);
                DEBUG_PRINT4("     Pop reg %d (spanning %p -> %p)\n",
                      reg, regstart[reg], regend[reg]);
            }
        }

        /* Check that we are not stuck in an infinite loop.  */
        public static void CHECK_INFINITE_LOOP(fail_stack_type fail_stack, ref bool cycle, re_pattern_buffer bufp, PtrEmulator<re_char> pat_cur, PtrEmulator<re_char> string_place)
        {
            int failure = TOP_FAILURE_HANDLE(fail_stack);
            /* Check for infinite matching loops */
            while (failure > 0
               && (FAILURE_STR(fail_stack, failure) == string_place
                   || FAILURE_STR(fail_stack, failure) == null))
            {
                // assert(FAILURE_PAT(failure) >= bufp->buffer
                //    && FAILURE_PAT(failure) <= bufp->buffer + bufp->used);
                if (FAILURE_PAT(fail_stack, failure) == pat_cur)
                {
                    cycle = true;
                    break;
                }
                DEBUG_PRINT2("  Other pattern: %p\n", FAILURE_PAT(fail_stack, failure));
                failure = NEXT_FAILURE_HANDLE(fail_stack, failure);
            }
            DEBUG_PRINT2("  Other string: %p\n", FAILURE_STR(fail_stack, failure));
        }

        /* Push the information about the state we will need
           if we ever fail back to it.

           Requires variables fail_stack, regstart, regend and
           num_regs be declared.  GROW_FAIL_STACK requires `destination' be
           declared.

           Does `return FAILURE_CODE' if runs out of memory.  */
        public static void PUSH_FAILURE_POINT(fail_stack_type fail_stack, re_pattern_buffer bufp,
                                              PtrEmulator<re_char> pattern, PtrEmulator<re_char> string_place,
                                              int size1, int size2,
                                              PtrEmulator<re_char> string1, PtrEmulator<re_char> string2,
                                              PtrEmulator<re_char> pend, ref uint nfailure_points_pushed)
        {
            /* Must be int, so when we don't save any registers, the arithmetic	
               of 0 + -1 isn't done as unsigned.  */

            DEBUG_STATEMENT(nfailure_points_pushed++);
            DEBUG_PRINT1("\nPUSH_FAILURE_POINT:\n");
            DEBUG_PRINT2("  Before push, next avail: %d\n", fail_stack.avail);
            DEBUG_PRINT2("			size: %d\n", fail_stack.size);

            ENSURE_FAIL_STACK(fail_stack, NUM_NONREG_ITEMS);

            DEBUG_PRINT1("\n");

            DEBUG_PRINT2("  Push frame index: %d\n", fail_stack.frame);
            PUSH_FAILURE_INT(fail_stack, fail_stack.frame);

            DEBUG_PRINT2("  Push string %p: `", string_place);
            DEBUG_PRINT_DOUBLE_STRING(string_place, string1, size1, string2, size2);
            DEBUG_PRINT1("'\n");
            PUSH_FAILURE_POINTER(fail_stack, string_place);

            DEBUG_PRINT2("  Push pattern %p: ", pattern);
            DEBUG_PRINT_COMPILED_PATTERN(bufp, pattern, pend);
            PUSH_FAILURE_POINTER(fail_stack, pattern);

            /* Close the frame by moving the frame pointer past it.  */
            fail_stack.frame = fail_stack.avail;
        }

        /* True if `size1' is non-NULL and PTR is pointing anywhere inside
           `string1' or just past its end.  This works if PTR is NULL, which is
           a good thing.  */
        public static bool FIRST_STRING_P(PtrEmulator<re_char> ptr, int size1, PtrEmulator<byte> string1)
        {
            return (size1 != 0 && (new PtrEmulator<re_char>(string1) <= ptr) && (ptr <= new PtrEmulator<re_char>(string1, size1)));
        }

        public static int debug = -100000;


        public static void DEBUG_STATEMENT(params object[] e)
        {
        }

        public static void DEBUG_PRINT1(string x)
        {
            if (debug > 0)
                System.Console.WriteLine(x);
        }
        public static void DEBUG_PRINT2(string x1, object x2)
        {
            if (debug > 0)
                System.Console.WriteLine(x1, x2);
        }
        public static void DEBUG_PRINT3(string x1, object x2, object x3)
        {
            if (debug > 0) System.Console.WriteLine(x1, x2, x3);
        }
        public static void DEBUG_PRINT4(string x1, object x2, object x3, object x4)
        {
            if (debug > 0) System.Console.WriteLine(x1, x2, x3, x4);
        }
        public static void DEBUG_PRINT_COMPILED_PATTERN(re_pattern_buffer p, PtrEmulator<re_char> s, PtrEmulator<re_char> e)
        {
            if (debug > 0)
            {
                // print_partial_compiled_pattern (s, e);
            }
        }
        public static void DEBUG_PRINT_DOUBLE_STRING(PtrEmulator<re_char> w, PtrEmulator<re_char> s1, int sz1, PtrEmulator<re_char> s2, int sz2)
        {
            if (debug > 0)
            {
                // print_double_string (w, s1, sz1, s2, sz2);
            }
        }

        /* This converts PTR, a pointer into one of the search strings `string1'
           and `string2' into an offset from the beginning of that string.  */
        public static int POINTER_TO_OFFSET(PtrEmulator<byte> ptr, int size1, PtrEmulator<re_char> string1, PtrEmulator<re_char> string2)
        {
            if (FIRST_STRING_P(ptr, size1, string1))
                return ptr - string1;
            else
                return (ptr - string2 + size1);
        }

        /* Call before fetching a character with *d.  This switches over to
           string2 if necessary.
           Check re_match_2_internal for a discussion of why end_match_2 might
           not be within string2 (but be equal to end_match_1 instead).  */
        public static bool PREFETCH(ref PtrEmulator<re_char> d, ref PtrEmulator<re_char> dend, PtrEmulator<re_char> string2, PtrEmulator<re_char> end_match_2)
        {
            while (d == dend)
            {
                /* End of string2 => fail.  */
                if (dend == end_match_2)
                    return true;
                /* End of string1 => advance to string2.  */
                d = new PtrEmulator<byte>(string2);
                dend = new PtrEmulator<byte>(end_match_2);
            }
            return false;
        }

        /* Call before fetching a char with *d if you already checked other limits.
           This is meant for use in lookahead operations like wordend, etc..
           where we might need to look at parts of the string that might be
           outside of the LIMITs (i.e past `stop').  */
        public static void PREFETCH_NOLIMIT(ref PtrEmulator<re_char> d, ref PtrEmulator<re_char> dend, PtrEmulator<re_char> end1, PtrEmulator<byte> string2, PtrEmulator<re_char> end_match_2)
        {
            if (d == end1)
            {
                d = new PtrEmulator<re_char>(string2);
                dend = new PtrEmulator<byte>(end_match_2);
            }
        }

        /* Test if at very beginning or at very end of the virtual concatenation
           of `string1' and `string2'.  If only one string, it's `string2'.  */
        public static bool AT_STRINGS_BEG(PtrEmulator<re_char> d, int size1, int size2, PtrEmulator<re_char> string1, PtrEmulator<re_char> string2)
        {
            return (d == new PtrEmulator<re_char>(size1 != 0 ? string1 : string2) || size2 == 0);
        }

        public static bool AT_STRINGS_END(PtrEmulator<re_char> d, PtrEmulator<re_char> end2)
        {
            return d == end2;
        }

        /* Pops what PUSH_FAIL_STACK pushes.

           We restore into the parameters, all of which should be lvalues:
             STR -- the saved data position.
             PAT -- the saved pattern position.
             REGSTART, REGEND -- arrays of string positions.

           Also assumes the variables `fail_stack' and (if debugging), `bufp',
           `pend', `string1', `size1', `string2', and `size2'.  */
        public static void POP_FAILURE_POINT(fail_stack_type fail_stack, re_pattern_buffer bufp, PtrEmulator<re_char> pend,
                                             int size1, int size2, PtrEmulator<byte> string1, PtrEmulator<byte> string2,
                                             ref uint nfailure_points_popped,
                                             PtrEmulator<re_char>[] regstart, PtrEmulator<re_char>[] regend,
                                             ref PtrEmulator<re_char> str, ref PtrEmulator<re_char> pat)
        {
            // assert(!FAIL_STACK_EMPTY());

            /* Remove failure points and point to how many regs pushed.  */
            DEBUG_PRINT1("POP_FAILURE_POINT:\n");
            DEBUG_PRINT2("  Before pop, next avail: %d\n", fail_stack.avail);
            DEBUG_PRINT2("		     size: %d\n", fail_stack.size);

            /* Pop the saved registers.  */
            while (fail_stack.frame < fail_stack.avail)
                POP_FAILURE_REG_OR_COUNT(fail_stack, regstart, regend);

            pat = POP_FAILURE_POINTER(fail_stack);
            DEBUG_PRINT2("  Popping pattern %p: ", pat);
            DEBUG_PRINT_COMPILED_PATTERN(bufp, pat, pend);

            /* If the saved string location is NULL, it came from an		
               on_failure_keep_string_jump opcode, and we want to throw away the	
               saved NULL, thus retaining our current position in the string.  */
            str = POP_FAILURE_POINTER(fail_stack);
            DEBUG_PRINT2("  Popping string %p: `", str);
            DEBUG_PRINT_DOUBLE_STRING(str, string1, size1, string2, size2);
            DEBUG_PRINT1("'\n");

            fail_stack.frame = POP_FAILURE_INT(fail_stack);
            DEBUG_PRINT2("  Popping  frame index: %d\n", fail_stack.frame);

            // assert(fail_stack.avail >= 0);
            // assert(fail_stack.frame <= fail_stack.avail);

            DEBUG_STATEMENT(nfailure_points_popped++);
        }

        /* Registers are set to a sentinel when they haven't yet matched.  */
        public static bool REG_UNSET(PtrEmulator<re_char> e)
        {
            return e == null;
        }
        /* re_match_2 matches the compiled pattern in BUFP against the
           the (virtual) concatenation of STRING1 and STRING2 (of length SIZE1
           and SIZE2, respectively).  We start matching at POS, and stop
           matching at STOP.

           If REGS is non-null and the `no_sub' field of BUFP is nonzero, we
           store offsets for the substring each group matched in REGS.  See the
           documentation for exactly how many groups we fill.

           We return -1 if no match, -2 if an internal error (such as the
           failure stack overflowing).  Otherwise, we return the length of the
           matched substring.  */
        public static int re_match_2(re_pattern_buffer bufp, PtrEmulator<byte> string1, int size1, PtrEmulator<byte> string2, int size2, int pos, re_registers regs, int stop)
        {
            int result;

            int charpos;
            gl_state.obj = re_match_object;
            charpos = SYNTAX_TABLE_BYTE_TO_CHAR(POS_AS_IN_BUFFER(pos));
            SETUP_SYNTAX_TABLE_FOR_OBJECT(re_match_object, charpos, 1);

            result = re_match_2_internal(bufp, string1, size1, string2, size2, pos, regs, stop);
            return result;
        }

        /* This is a separate function so that we can force an alloca cleanup
           afterwards.  */
        public static int re_match_2_internal(re_pattern_buffer bufp, PtrEmulator<byte> string1, int size1,
         PtrEmulator<byte> string2, int size2, int pos, re_registers regs, int stop)
        {
            /* General temporaries.  */
            int mcnt = 0;
            int reg;
            bool not;

            /* Just past the end of the corresponding string.  */
            PtrEmulator<re_char> end1, end2;

            /* Pointers into string1 and string2, just past the last characters in
               each to consider matching.  */
            PtrEmulator<re_char> end_match_1, end_match_2;

            /* Where we are in the data, and the end of the current string.  */
            PtrEmulator<re_char> d, dend;

            /* Used sometimes to remember where we were before starting matching
               an operator so that we can go back in case of failure.  This "atomic"
               behavior of matching opcodes is indispensable to the correctness
               of the on_failure_keep_string_jump optimization.  */
            PtrEmulator<re_char> dfail;

            /* Where we are in the pattern, and the end of the pattern.  */
            PtrEmulator<re_char> p = new PtrEmulator<re_char>(bufp.buffer);
            PtrEmulator<re_char> pend = new PtrEmulator<re_char>(p, bufp.used);

            /* We use this to map every character in the string.	*/
            RE_TRANSLATE_TYPE translate = bufp.translate;

            /* Nonzero if BUFP is setup from a multibyte regex.  */
            bool multibyte = RE_MULTIBYTE_P(bufp);

            /* Nonzero if STRING1/STRING2 are multibyte.  */
            bool target_multibyte = RE_TARGET_MULTIBYTE_P(bufp);

            /* Failure point stack.  Each place that can handle a failure further
               down the line pushes a failure point on this stack.  It consists of
               regstart, and regend for all registers corresponding to
               the subexpressions we're currently inside, plus the number of such
               registers, and, finally, two char *'s.  The first char * is where
               to resume scanning the pattern; the second one is where to resume
               scanning the strings.  */
            //#ifdef MATCH_MAY_ALLOCATE /* otherwise, this is global.  */
            fail_stack_type fail_stack = new fail_stack_type();
            //#endif
            // #if debug
            uint nfailure_points_pushed = 0, nfailure_points_popped = 0;
            //#endif

            /* We fill all the registers internally, independent of what we
               return, for use in backreferences.  The number here includes
               an element for register zero.  */
            int num_regs = bufp.re_nsub + 1;

            /* Information on the contents of registers. These are pointers into
               the input strings; they record just what was matched (on this
               attempt) by a subexpression part of the pattern, that is, the
               regnum-th regstart pointer points to where in the pattern we began
               matching and the regnum-th regend points to right after where we
               stopped matching the regnum-th subexpression.  (The zeroth register
               keeps track of what the whole pattern matches.)  */
            //#ifdef MATCH_MAY_ALLOCATE /* otherwise, these are global.  */
            PtrEmulator<re_char>[] regstart, regend;
            //#endif

            /* The following record the register info as found in the above
               variables when we find a match better than any we've seen before.
               This happens as we backtrack through the failure points, which in
               turn happens only if we have not yet matched the entire string. */
            bool best_regs_set = false;
            // #ifdef MATCH_MAY_ALLOCATE /* otherwise, these are global.  */
            PtrEmulator<re_char>[] best_regstart, best_regend;
            // #endif

            /* Logically, this is `best_regend[0]'.  But we don't want to have to
               allocate space for that if we're not allocating space for anything
               else (see below).  Also, we never need info about register 0 for
               any of the other register vectors, and it seems rather a kludge to
               treat `best_regend' differently than the rest.  So we keep track of
               the end of the best match so far in a separate variable.  We
               initialize this to NULL so that when we backtrack the first time
               and need to test it, it's not garbage.  */
            PtrEmulator<re_char> match_end = new PtrEmulator<re_char>();

            //#if DEBUG
            /* Counts the total number of registers pushed.  */
            uint num_regs_pushed = 0;
            //#endif

            DEBUG_PRINT1("\n\nEntering re_match_2.\n");

            INIT_FAIL_STACK(fail_stack);

            // #ifdef MATCH_MAY_ALLOCATE
            /* Do not bother to initialize all the register variables if there are
               no groups in the pattern, as it takes a fair amount of time.  If
               there are groups, we include space for register 0 (the whole
               pattern), even though we never use it, since it simplifies the
               array indexing.  We should fix this.  */
            if (bufp.re_nsub != 0)
            {
                regstart = new PtrEmulator<re_char>[num_regs];
                regend = new PtrEmulator<re_char>[num_regs];
                best_regstart = new PtrEmulator<re_char>[num_regs];
                best_regend = new PtrEmulator<re_char>[num_regs];
            }
            else
            {
                /* We must initialize all our variables to NULL, so that
               `FREE_VARIABLES' doesn't try to free them.  */
                regstart = regend = best_regstart = best_regend = null;
            }
            // #endif /* MATCH_MAY_ALLOCATE */

            /* The starting position is bogus.  */
            if (pos < 0 || pos > size1 + size2)
            {
                // FREE_VARIABLES ();
                return -1;
            }

            /* Initialize subexpression text positions to -1 to mark ones that no
               start_memory/stop_memory has been seen for. Also initialize the
               register information struct.  */
            for (reg = 1; reg < num_regs; reg++)
                regstart[reg] = regend[reg] = new PtrEmulator<byte>();

            /* We move `string1' into `string2' if the latter's empty -- but not if
               `string1' is null.  */
            if (size2 == 0 && string1 != null)
            {
                string2 = string1;
                size2 = size1;
                string1 = new PtrEmulator<byte>();
                size1 = 0;
            }
            end1 = new PtrEmulator<re_char>(string1, size1);
            end2 = new PtrEmulator<re_char>(string2, size2);

            /* `p' scans through the pattern as `d' scans through the data.
               `dend' is the end of the input string that `d' points within.  `d'
               is advanced into the following input string whenever necessary, but
               this happens before fetching; therefore, at the beginning of the
               loop, `d' can be pointing at the end of a string, but it cannot
               equal `string2'.  */
            if (pos >= size1)
            {
                /* Only match within string2.  */
                d = new PtrEmulator<re_char>(string2, pos - size1);
                dend = new PtrEmulator<re_char>(string2, stop - size1);
                end_match_2 = new PtrEmulator<re_char>(dend);
                end_match_1 = new PtrEmulator<re_char>(end1);	/* Just to give it a value.  */
            }
            else
            {
                if (stop < size1)
                {
                    /* Only match within string1.  */
                    end_match_1 = new PtrEmulator<re_char>(string1, stop);
                    /* BEWARE!
                       When we reach end_match_1, PREFETCH normally switches to string2.
                       But in the present case, this means that just doing a PREFETCH
                       makes us jump from `stop' to `gap' within the string.
                       What we really want here is for the search to stop as
                       soon as we hit end_match_1.  That's why we set end_match_2
                       to end_match_1 (since PREFETCH fails as soon as we hit
                       end_match_2).  */
                    end_match_2 = new PtrEmulator<re_char>(end_match_1);
                }
                else
                { /* It's important to use this code when stop == size so that
	     moving `d' from end1 to string2 will not prevent the d == dend
	     check from catching the end of string.  */
                    end_match_1 = new PtrEmulator<re_char>(end1);
                    end_match_2 = new PtrEmulator<re_char>(string2, stop - size1);
                }
                d = new PtrEmulator<re_char>(string1, pos);
                dend = new PtrEmulator<re_char>(end_match_1);
            }

            DEBUG_PRINT1("The compiled pattern is: ");
            DEBUG_PRINT_COMPILED_PATTERN(bufp, p, pend);
            DEBUG_PRINT1("The string to match is: `");
            DEBUG_PRINT_DOUBLE_STRING(d, string1, size1, string2, size2);
            DEBUG_PRINT1("'\n");

            /* This loops over pattern commands.  It exits by returning from the
               function if the match is complete, or it drops through if the match
               fails at this starting point in the input data.  */
            for (; ; )
            {
                DEBUG_PRINT2("\n%p: ", p);

                if (p != pend)
                    goto damn_it_not_end;

                /* End of pattern means we might have succeeded.  */
                DEBUG_PRINT1("end of pattern ... ");

                /* If we haven't matched the entire string, and we want the
                   longest match, try backtracking.  */
                if (d == end_match_2)
                    goto succeed_label;

                /* 1 if this match ends in the same string (string1 or string2)
               as the best previous match.  */
                bool same_str_p = (FIRST_STRING_P(match_end, size1, string1) == FIRST_STRING_P(d, size1, string1));
                /* 1 if this match is the best seen so far.  */
                bool best_match_p;

                /* AIX compiler got confused when this was combined
               with the previous declaration.  */
                if (same_str_p)
                    best_match_p = d > match_end;
                else
                    best_match_p = !FIRST_STRING_P(d, size1, string1);

                DEBUG_PRINT1("backtracking.\n");

                if (!FAIL_STACK_EMPTY(fail_stack))
                { /* More failure points to try.  */

                    /* If exceeds best match so far, save it.  */
                    if (!best_regs_set || best_match_p)
                    {
                        best_regs_set = true;
                        match_end = d;

                        DEBUG_PRINT1("\nSAVING match as best so far.\n");

                        for (reg = 1; reg < num_regs; reg++)
                        {
                            best_regstart[reg] = regstart[reg];
                            best_regend[reg] = regend[reg];
                        }
                    }
                    goto fail;
                }

                        /* If no failure points, don't restore garbage.  And if
                       last match is real best match, don't restore second
                       best one. */
                else if (!(best_regs_set && !best_match_p))
                    goto succeed_label;

//            restore_best_regs:
                /* Restore best match.  It may happen that `dend ==
                   end_match_1' while the restored d is in string2.
                   For example, the pattern `x.*y.*z' against the
                   strings `x-' and `y-z-', if the two strings are
                   not consecutive in memory.  */
                DEBUG_PRINT1("Restoring best registers.\n");

                d = new PtrEmulator<byte>(match_end);
                dend = new PtrEmulator<byte>((d >= new PtrEmulator<re_char>(string1) && d <= end1)
                             ? end_match_1 : end_match_2);

                for (reg = 1; reg < num_regs; reg++)
                {
                    regstart[reg] = best_regstart[reg];
                    regend[reg] = best_regend[reg];
                }
            /* d != end_match_2 */

              succeed_label:
                DEBUG_PRINT1("Accepting match.\n");

                /* If caller wants register contents data back, do it.  */
                if (regs != null && !bufp.no_sub)
                {
                    /* Have the register data arrays been allocated?	*/
                    if (bufp.regs_allocated == re_pattern_buffer.REGS_UNALLOCATED)
                    { /* No.  So allocate them with malloc.  We need one
		     extra element beyond `num_regs' for the `-1' marker
		     GNU code uses.  */
                        regs.num_regs = (uint)System.Math.Max(RE_NREGS, num_regs + 1);
                        regs.start = new regoff_t[regs.num_regs];
                        regs.end = new regoff_t[regs.num_regs];

                        bufp.regs_allocated = re_pattern_buffer.REGS_REALLOCATE;
                    }
                    else if (bufp.regs_allocated == re_pattern_buffer.REGS_REALLOCATE)
                    { /* Yes.  If we need more elements than were already
		     allocated, reallocate them.  If we need fewer, just
		     leave it alone.  */
                        if (regs.num_regs < num_regs + 1)
                        {
                            regs.num_regs = (uint)(num_regs + 1);
                            System.Array.Resize(ref regs.start, (int)regs.num_regs);
                            System.Array.Resize(ref regs.end, (int)regs.num_regs);
                        }
                    }
                    else
                    {
                        /* These braces fend off a "empty body in an else-statement"
                           warning under GCC when assert expands to nothing.  */
                        // assert(bufp.regs_allocated == re_pattern_buffer.REGS_FIXED);
                    }

                    /* Convert the pointer data in `regstart' and `regend' to
                   indices.  Register zero has to be set differently,
                   since we haven't kept track of any info for it.  */
                    if (regs.num_regs > 0)
                    {
                        regs.start[0] = pos;
                        regs.end[0] = POINTER_TO_OFFSET(d, size1, string1, string2);
                    }

                    /* Go through the first `min (num_regs, regs->num_regs)'
                   registers, since that is all we initialized.  */
                    for (reg = 1; reg < System.Math.Min(num_regs, regs.num_regs); reg++)
                    {
                        if (REG_UNSET(regstart[reg]) || REG_UNSET(regend[reg]))
                            regs.start[reg] = regs.end[reg] = -1;
                        else
                        {
                            regs.start[reg] = POINTER_TO_OFFSET(regstart[reg], size1, string1, string2);
                            regs.end[reg] = POINTER_TO_OFFSET(regend[reg], size1, string1, string2);
                        }
                    }

                    /* If the regs structure we return has more elements than
                   were in the pattern, set the extra elements to -1.  If
                   we (re)allocated the registers, this is the case,
                   because we always allocate enough to have at least one
                   -1 at the end.  */
                    for (reg = num_regs; reg < regs.num_regs; reg++)
                        regs.start[reg] = regs.end[reg] = -1;
                } /* regs && !bufp->no_sub */

                DEBUG_PRINT4("%u failure points pushed, %u popped (%u remain).\n",
                          nfailure_points_pushed, nfailure_points_popped,
                          nfailure_points_pushed - nfailure_points_popped);
                DEBUG_PRINT2("%u registers pushed.\n", num_regs_pushed);

                mcnt = POINTER_TO_OFFSET(d, size1, string1, string2) - pos;

                DEBUG_PRINT2("Returning %d from re_match_2.\n", mcnt);

                return mcnt;

            damn_it_not_end:
                /* Otherwise match next pattern command.  */
                switch (((re_opcode_t)p.ValueAndInc()))
                {
                    /* Ignore these.  Used to ignore the n of succeed_n's which
                       currently have n == 0.  */
                    case re_opcode_t.no_op:
                        DEBUG_PRINT1("EXECUTING no_op.\n");
                        break;

                    case re_opcode_t.succeed:
                        DEBUG_PRINT1("EXECUTING succeed.\n");
                        goto succeed_label;

                    /* Match the next n pattern characters exactly.  The following
                       byte in the pattern defines n, and the n bytes after that
                       are the characters to match.  */
                    case re_opcode_t.exactn:
                        mcnt = p.ValueAndInc();
                        DEBUG_PRINT2("EXECUTING exactn %d.\n", mcnt);

                        /* Remember the start point to rollback upon failure.  */
                        dfail = d;

                        /* The cost of testing `translate' is comparatively small.  */
                        if (target_multibyte)
                            do
                            {
                                int pat_charlen = 0, buf_charlen = 0;
                                int pat_ch, buf_ch;

                                if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                    goto fail;

                                if (multibyte)
                                    pat_ch = (int)STRING_CHAR_AND_LENGTH(p.Collection, p.Index, pend - p, ref pat_charlen);
                                else
                                {
                                    pat_ch = (int)RE_CHAR_TO_MULTIBYTE(p.Value);
                                    pat_charlen = 1;
                                }
                                buf_ch = (int)STRING_CHAR_AND_LENGTH(d.Collection, d.Index, dend - d, ref buf_charlen);

                                if (TRANSLATE(buf_ch, translate) != pat_ch)
                                {
                                    d = new PtrEmulator<byte>(dfail);
                                    goto fail;
                                }

                                p += pat_charlen;
                                d += buf_charlen;
                                mcnt -= pat_charlen;
                            }
                            while (mcnt > 0);
                        else
                            do
                            {
                                int pat_charlen = 0;
                                int pat_ch, buf_ch;

                                if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                    goto fail;

                                if (multibyte)
                                {
                                    pat_ch = (int)STRING_CHAR_AND_LENGTH(p.Collection, p.Index, pend - p, ref pat_charlen);
                                    pat_ch = (int)RE_CHAR_TO_UNIBYTE(pat_ch);
                                }
                                else
                                {
                                    pat_ch = p.Value;
                                    pat_charlen = 1;
                                }
                                buf_ch = (int)RE_CHAR_TO_MULTIBYTE(d.Value);
                                if (!CHAR_BYTE8_P((uint)buf_ch))
                                {
                                    buf_ch = TRANSLATE(buf_ch, translate);
                                    buf_ch = RE_CHAR_TO_UNIBYTE(buf_ch);
                                    if (buf_ch < 0)
                                        buf_ch = d.Value;
                                }
                                else
                                    buf_ch = d.Value;
                                if (buf_ch != pat_ch)
                                {
                                    d = new PtrEmulator<byte>(dfail);
                                    goto fail;
                                }
                                p += pat_charlen;
                                d++;
                            }
                            while (--mcnt > 0);
                        break;


                    /* Match any character except possibly a newline or a null.  */
                    case re_opcode_t.anychar:
                        {
                            int buf_charlen = 0;
                            re_wchar_t buf_ch;

                            DEBUG_PRINT1("EXECUTING anychar.\n");

                            if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                goto fail;
                            buf_ch = (int)RE_STRING_CHAR_AND_LENGTH(d, dend - d, ref buf_charlen, target_multibyte);
                            buf_ch = TRANSLATE(buf_ch, translate);

                            if (((bufp.syntax & RE_DOT_NEWLINE) == 0 && buf_ch == '\n') || ((bufp.syntax & RE_DOT_NOT_NULL) != 0 && buf_ch == '\0'))
                                goto fail;

                            DEBUG_PRINT2("  Matched `%d'.\n", d.Value);
                            d += buf_charlen;
                        }
                        break;

                    case re_opcode_t.charset:
                    case re_opcode_t.charset_not:
                        {
                            uint c;
                            bool nope = ((re_opcode_t)p[-1] == re_opcode_t.charset_not);
                            int len = 0;

                            /* Start of actual range_table, or end of bitmap if there is no
                               range table.  */
                            PtrEmulator<re_char> range_table = new PtrEmulator<byte>();

                            /* Nonzero if there is a range table.  */
                            bool range_table_exists;

                            /* Number of ranges of range table.  This is not included
                               in the initial byte-length of the command.  */
                            int count = 0;

                            /* Whether matching against a unibyte character.  */
                            bool unibyte_char = false;

                            DEBUG_PRINT2("EXECUTING charset%s.\n", nope ? "_not" : "");

                            range_table_exists = CHARSET_RANGE_TABLE_EXISTS_P(new PtrEmulator<re_char>(p, -1));

                            if (range_table_exists)
                            {
                                range_table = CHARSET_RANGE_TABLE(new PtrEmulator<re_char>(p, -1)); /* Past the bitmap.  */
                                EXTRACT_NUMBER_AND_INCR(ref count, ref range_table);
                            }

                            if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                goto fail;
                            c = RE_STRING_CHAR_AND_LENGTH(d, dend - d, ref len, target_multibyte);
                            if (target_multibyte)
                            {
                                int c1;

                                c = (uint)TRANSLATE((int)c, translate);
                                c1 = RE_CHAR_TO_UNIBYTE((int)c);
                                if (c1 >= 0)
                                {
                                    unibyte_char = true;
                                    c = (uint)c1;
                                }
                            }
                            else
                            {
                                int c1 = (int)RE_CHAR_TO_MULTIBYTE((int)c);

                                if (!CHAR_BYTE8_P((uint)c1))
                                {
                                    c1 = TRANSLATE(c1, translate);
                                    c1 = RE_CHAR_TO_UNIBYTE(c1);
                                    if (c1 >= 0)
                                    {
                                        unibyte_char = true;
                                        c = (uint)c1;
                                    }
                                }
                                else
                                    unibyte_char = true;
                            }

                            if (unibyte_char && c < (1 << BYTEWIDTH))
                            {			/* Lookup bitmap.  */
                                /* Cast to `unsigned' instead of `unsigned char' in
                                   case the bit list is a full 32 bytes long.  */
                                if (c < (uint)(CHARSET_BITMAP_SIZE(new PtrEmulator<re_char>(p, -1)) * BYTEWIDTH)
                                    && (p[(int)(1 + c / (uint)BYTEWIDTH)] & (1 << (int)(c % (uint)BYTEWIDTH))) != 0)
                                    nope = !nope;
                            }

                            else if (range_table_exists)
                            {
                                int class_bits = CHARSET_RANGE_TABLE_BITS(new PtrEmulator<re_char>(p, -1));

                                if (((class_bits & BIT_LOWER) != 0 && ISLOWER(c)) ||
                         ((class_bits & BIT_MULTIBYTE) != 0) ||
                         ((class_bits & BIT_PUNCT) != 0 && ISPUNCT(c)) ||
                         ((class_bits & BIT_SPACE) != 0 && ISSPACE(c)) ||
                         ((class_bits & BIT_UPPER) != 0 && ISUPPER(c)) || ((class_bits & BIT_WORD) != 0 && ISWORD(c)))
                                {
                                    nope = !nope;
                                }
                                else
                                    CHARSET_LOOKUP_RANGE_TABLE_RAW(ref nope, c, range_table, count);
                            }

                            if (range_table_exists)
                                p = CHARSET_RANGE_TABLE_END(range_table, count);
                            else
                                p += CHARSET_BITMAP_SIZE(new PtrEmulator<re_char>(p, -1)) + 1;

                            if (!nope) goto fail;

                            d += len;
                            break;
                        }


                    /* The beginning of a group is represented by start_memory.
                       The argument is the register number.  The text
                       matched within the group is recorded (in the internal
                       registers data structure) under the register number.  */
                    case re_opcode_t.start_memory:
                        DEBUG_PRINT2("EXECUTING start_memory %d:\n", p.Value);

                        /* In case we need to undo this operation (via backtracking).  */
                        PUSH_FAILURE_REG(fail_stack, regstart, regend, p.Value);

                        regstart[p.Value] = d;
                        regend[p.Value] = new PtrEmulator<byte>();	/* probably unnecessary.  -sm  */
                        DEBUG_PRINT2("  regstart: %d\n", POINTER_TO_OFFSET(regstart[p.Value], size1, string1, string2));

                        /* Move past the register number and inner group count.  */
                        p += 1;
                        break;


                    /* The stop_memory opcode represents the end of a group.  Its
                       argument is the same as start_memory's: the register number.  */
                    case re_opcode_t.stop_memory:
                        DEBUG_PRINT2("EXECUTING stop_memory %d:\n", p.Value);

                        // assert (!REG_UNSET (regstart[*p]));
                        /* Strictly speaking, there should be code such as:

                          assert (REG_UNSET (regend[*p]));
                          PUSH_FAILURE_REGSTOP ((unsigned int)*p);

                           But the only info to be pushed is regend[*p] and it is known to
                           be UNSET, so there really isn't anything to push.
                           Not pushing anything, on the other hand deprives us from the
                           guarantee that regend[*p] is UNSET since undoing this operation
                           will not reset its value properly.  This is not important since
                           the value will only be read on the next start_memory or at
                           the very end and both events can only happen if this stop_memory
                           is *not* undone.  */

                        regend[p.Value] = d;
                        DEBUG_PRINT2("      regend: %d\n", POINTER_TO_OFFSET(regend[p.Value], size1, string1, string2));

                        /* Move past the register number and the inner group count.  */
                        p += 1;
                        break;


                    /* \<digit> has been turned into a `duplicate' command which is
                       followed by the numeric value of <digit> as the register number.  */
                    case re_opcode_t.duplicate:
                        {
                            PtrEmulator<re_char> d2, dend2;
                            int regno = p.Value; /* Get which register to match against.  */
                            p++;

                            DEBUG_PRINT2("EXECUTING duplicate %d.\n", regno);

                            /* Can't back reference a group which we've never matched.  */
                            if (REG_UNSET(regstart[regno]) || REG_UNSET(regend[regno]))
                                goto fail;

                            /* Where in input to try to start matching.  */
                            d2 = new PtrEmulator<byte>(regstart[regno]);

                            /* Remember the start point to rollback upon failure.  */
                            dfail = new PtrEmulator<byte>(d);

                            /* Where to stop matching; if both the place to start and
                               the place to stop matching are in the same string, then
                               set to the place to stop, otherwise, for now have to use
                               the end of the first string.  */

                            dend2 = new PtrEmulator<byte>((FIRST_STRING_P(regstart[regno], size1, string1) == FIRST_STRING_P(regend[regno], size1, string1))
                                 ? regend[regno] : end_match_1);
                            for (; ; )
                            {
                                /* If necessary, advance to next segment in register
                                   contents.  */
                                while (d2 == dend2)
                                {
                                    if (dend2 == end_match_2) break;
                                    if (dend2 == regend[regno]) break;

                                    /* End of string1 => advance to string2. */
                                    d2 = new PtrEmulator<byte>(string2);
                                    dend2 = new PtrEmulator<byte>(regend[regno]);
                                }
                                /* At end of register contents => success */
                                if (d2 == dend2) break;

                                /* If necessary, advance to next segment in data.  */
                                if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                    goto fail;

                                /* How many characters left in this segment to match.  */
                                mcnt = dend - d;

                                /* Want how many consecutive characters we can match in
                                   one shot, so, if necessary, adjust the count.  */
                                if (mcnt > dend2 - d2)
                                    mcnt = dend2 - d2;

                                /* Compare that many; failure if mismatch, else move
                                   past them.  */
                                if (RE_TRANSLATE_P(translate)
                                    ? bcmp_translate(d, d2, mcnt, translate, target_multibyte) != 0
                                    : PtrEmulator<byte>.memcmp(d, d2, mcnt) != 0)
                                {
                                    d = new PtrEmulator<byte>(dfail);
                                    goto fail;
                                }
                                d += mcnt; d2 += mcnt;
                            }
                        }
                        break;


                    /* begline matches the empty string at the beginning of the string
                       (unless `not_bol' is set in `bufp'), and after newlines.  */
                    case re_opcode_t.begline:
                        DEBUG_PRINT1("EXECUTING begline.\n");

                        if (AT_STRINGS_BEG(d, size1, size2, string1, string2))
                        {
                            if (!bufp.not_bol) break;
                        }
                        else
                        {
                            uint c = 0;
                            GET_CHAR_BEFORE_2(target_multibyte, ref c, d, new PtrEmulator<byte>(string1), end1, new PtrEmulator<re_char>(string2), end2);
                            if (c == '\n')
                                break;
                        }
                        /* In all other cases, we fail.  */
                        goto fail;


                    /* endline is the dual of begline.  */
                    case re_opcode_t.endline:
                        DEBUG_PRINT1("EXECUTING endline.\n");

                        if (AT_STRINGS_END(d, end2))
                        {
                            if (!bufp.not_eol) break;
                        }
                        else
                        {
                            PREFETCH_NOLIMIT(ref d, ref dend, end1, string2, end_match_2);
                            if (d.Value == '\n')
                                break;
                        }
                        goto fail;


                    /* Match at the very beginning of the data.  */
                    case re_opcode_t.begbuf:
                        DEBUG_PRINT1("EXECUTING begbuf.\n");
                        if (AT_STRINGS_BEG(d, size1, size2, string1, string2))
                            break;
                        goto fail;


                    /* Match at the very end of the data.  */
                    case re_opcode_t.endbuf:
                        DEBUG_PRINT1("EXECUTING endbuf.\n");
                        if (AT_STRINGS_END(d, end2))
                            break;
                        goto fail;


                    /* on_failure_keep_string_jump is used to optimize `.*\n'.  It
                       pushes NULL as the value for the string on the stack.  Then
                       `POP_FAILURE_POINT' will keep the current value for the
                       string, instead of restoring it.  To see why, consider
                       matching `foo\nbar' against `.*\n'.  The .* matches the foo;
                       then the . fails against the \n.  But the next thing we want
                       to do is match the \n against the \n; if we restored the
                       string value, we would be back at the foo.

                       Because this is used only in specific cases, we don't need to
                       check all the things that `on_failure_jump' does, to make
                       sure the right things get saved on the stack.  Hence we don't
                       share its code.  The only reason to push anything on the
                       stack at all is that otherwise we would have to change
                       `anychar's code to do something besides goto fail in this
                       case; that seems worse than this.  */
                    case re_opcode_t.on_failure_keep_string_jump:
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        DEBUG_PRINT3("EXECUTING on_failure_keep_string_jump %d (to %p):\n",
                              mcnt, p + mcnt);

                        PUSH_FAILURE_POINT(fail_stack, bufp, p - 3, new PtrEmulator<re_char>(), size1, size2, string1, string2, pend, ref nfailure_points_pushed);
                        break;

                    /* A nasty loop is introduced by the non-greedy *? and +?.
                       With such loops, the stack only ever contains one failure point
                       at a time, so that a plain on_failure_jump_loop kind of
                       cycle detection cannot work.  Worse yet, such a detection
                       can not only fail to detect a cycle, but it can also wrongly
                       detect a cycle (between different instantiations of the same
                       loop).
                       So the method used for those nasty loops is a little different:
                       We use a special cycle-detection-stack-frame which is pushed
                       when the on_failure_jump_nastyloop failure-point is *popped*.
                       This special frame thus marks the beginning of one iteration
                       through the loop and we can hence easily check right here
                       whether something matched between the beginning and the end of
                       the loop.  */
                    case re_opcode_t.on_failure_jump_nastyloop:
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        DEBUG_PRINT3("EXECUTING on_failure_jump_nastyloop %d (to %p):\n",
                              mcnt, p + mcnt);

                        // assert((re_opcode_t)p[-4] == no_op);
                        {
                            bool cycle = false;
                            CHECK_INFINITE_LOOP(fail_stack, ref cycle, bufp, p - 4, d);
                            if (!cycle)
                                /* If there's a cycle, just continue without pushing
                               this failure point.  The failure point is the "try again"
                               option, which shouldn't be tried.
                               We want (x?)*?y\1z to match both xxyz and xxyxz.  */
                                PUSH_FAILURE_POINT(fail_stack, bufp, p - 3, d, size1, size2, string1, string2, pend, ref nfailure_points_pushed);
                        }
                        break;

                    /* Simple loop detecting on_failure_jump:  just check on the
                       failure stack if the same spot was already hit earlier.  */
                    case re_opcode_t.on_failure_jump_loop:
                        // on_failure:
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        DEBUG_PRINT3("EXECUTING on_failure_jump_loop %d (to %p):\n",
                              mcnt, p + mcnt);
                        {
                            bool cycle = false;
                            CHECK_INFINITE_LOOP(fail_stack, ref cycle, bufp, p - 3, d);
                            if (cycle)
                                /* If there's a cycle, get out of the loop, as if the matching
                               had failed.  We used to just `goto fail' here, but that was
                               aborting the search a bit too early: we want to keep the
                               empty-loop-match and keep matching after the loop.
                               We want (x?)*y\1z to match both xxyz and xxyxz.  */
                                p += mcnt;
                            else
                                PUSH_FAILURE_POINT(fail_stack, bufp, p - 3, d, size1, size2, string1, string2, pend, ref nfailure_points_pushed);
                        }
                        break;

                    /* Uses of on_failure_jump:

                       Each alternative starts with an on_failure_jump that points
                       to the beginning of the next alternative.  Each alternative
                       except the last ends with a jump that in effect jumps past
                       the rest of the alternatives.  (They really jump to the
                       ending jump of the following alternative, because tensioning
                       these jumps is a hassle.)

                       Repeats start with an on_failure_jump that points past both
                       the repetition text and either the following jump or
                       pop_failure_jump back to this on_failure_jump.  */
                    case re_opcode_t.on_failure_jump:
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        DEBUG_PRINT3("EXECUTING on_failure_jump %d (to %p):\n", mcnt, p + mcnt);

                        PUSH_FAILURE_POINT(fail_stack, bufp, p - 3, d, size1, size2, string1, string2, pend, ref nfailure_points_pushed);
                        break;

                    /* This operation is used for greedy *.
                       Compare the beginning of the repeat with what in the
                       pattern follows its end. If we can establish that there
                       is nothing that they would both match, i.e., that we
                       would have to backtrack because of (as in, e.g., `a*a')
                       then we can use a non-backtracking loop based on
                       on_failure_keep_string_jump instead of on_failure_jump.  */
                    case re_opcode_t.on_failure_jump_smart:
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                        DEBUG_PRINT3("EXECUTING on_failure_jump_smart %d (to %p).\n",
                              mcnt, p + mcnt);
                        {
                            PtrEmulator<re_char> p1 = new PtrEmulator<byte>(p); /* Next operation.  */
                            /* Here, we discard `const', making re_match non-reentrant.  */
                            PtrEmulator<re_char> p2 = new PtrEmulator<byte>(p, mcnt); /* Jump dest.  */
                            PtrEmulator<re_char> p3 = new PtrEmulator<byte>(p, -3); /* opcode location.  */

                            p -= 3;		/* Reset so that we will re-execute the
				   instruction once it's been changed. */

                            EXTRACT_NUMBER(ref mcnt, p2 - 2);

                            /* Ensure this is a indeed the trivial kind of loop
                               we are expecting.  */
                            // assert (skip_one_char (p1) == p2 - 3);
                            // assert ((re_opcode_t) p2[-3] == jump && p2 + mcnt == p);
                            DEBUG_STATEMENT(debug += 2);
                            if (mutually_exclusive_p(bufp, p1, p2))
                            {
                                /* Use a fast `on_failure_keep_string_jump' loop.  */
                                DEBUG_PRINT1("  smart exclusive => fast loop.\n");
                                p3.Value = (re_char)re_opcode_t.on_failure_keep_string_jump;
                                STORE_NUMBER(p2 - 2, mcnt + 3);
                            }
                            else
                            {
                                /* Default to a safe `on_failure_jump' loop.  */
                                DEBUG_PRINT1("  smart default => slow loop.\n");
                                p3.Value = (re_char)re_opcode_t.on_failure_jump;
                            }
                            DEBUG_STATEMENT(debug -= 2);
                        }
                        break;

                    /* Unconditionally jump (without popping any failure points).  */
                    case re_opcode_t.jump:
                        //	unconditional_jump:
                        IMMEDIATE_QUIT_CHECK();
                        EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);	/* Get the amount to jump.  */
                        DEBUG_PRINT2("EXECUTING jump %d ", mcnt);
                        p += mcnt;				/* Do the jump.  */
                        DEBUG_PRINT2("(to %p).\n", p);
                        break;


                    /* Have to succeed matching what follows at least n times.
                       After that, handle like `on_failure_jump'.  */
                    case re_opcode_t.succeed_n:
                        /* Signedness doesn't matter since we only compare MCNT to 0.  */
                        EXTRACT_NUMBER(ref mcnt, p + 2);
                        DEBUG_PRINT2("EXECUTING succeed_n %d.\n", mcnt);

                        /* Originally, mcnt is how many times we HAVE to succeed.  */
                        if (mcnt != 0)
                        {
                            /* Here, we discard `const', making re_match non-reentrant.  */
                            PtrEmulator<re_char> p2 = new PtrEmulator<re_char>(p, 2); /* counter loc.  */
                            mcnt--;
                            p += 4;
                            PUSH_NUMBER(fail_stack, p2, mcnt);
                        }
                        else
                            /* The two bytes encoding mcnt == 0 are two no_op opcodes.  */
                            goto case re_opcode_t.on_failure_jump_loop;
                        break;

                    case re_opcode_t.jump_n:
                        /* Signedness doesn't matter since we only compare MCNT to 0.  */
                        EXTRACT_NUMBER(ref mcnt, p + 2);
                        DEBUG_PRINT2("EXECUTING jump_n %d.\n", mcnt);

                        /* Originally, this is how many times we CAN jump.  */
                        if (mcnt != 0)
                        {
                            /* Here, we discard `const', making re_match non-reentrant.  */
                            PtrEmulator<re_char> p2 = new PtrEmulator<re_char>(p, 2); /* counter loc.  */
                            mcnt--;
                            PUSH_NUMBER(fail_stack, p2, mcnt);
                            goto case re_opcode_t.jump;
                        }
                        /* If don't have to jump any more, skip over the rest of command.  */
                        else
                            p += 4;
                        break;

                    case re_opcode_t.set_number_at:
                        {
                            PtrEmulator<re_char> p2;	/* Location of the counter.  */
                            DEBUG_PRINT1("EXECUTING set_number_at.\n");

                            EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                            /* Here, we discard `const', making re_match non-reentrant.  */
                            p2 = new PtrEmulator<byte>(p, mcnt);
                            /* Signedness doesn't matter since we only copy MCNT's bits .  */
                            EXTRACT_NUMBER_AND_INCR(ref mcnt, ref p);
                            DEBUG_PRINT3("  Setting %p to %d.\n", p2, mcnt);
                            PUSH_NUMBER(fail_stack, p2, mcnt);
                            break;
                        }

                    case re_opcode_t.wordbound:
                    case re_opcode_t.notwordbound:
                        not = (re_opcode_t)p[-1] == re_opcode_t.notwordbound;
                        DEBUG_PRINT2("EXECUTING %swordbound.\n", not ? "not" : "");

                        /* We SUCCEED (or FAIL) in one of the following cases: */

                        /* Case 1: D is at the beginning or the end of string.  */
                        if (AT_STRINGS_BEG(d, size1, size2, string1, string2) || AT_STRINGS_END(d, end2))
                            not = !not;
                        else
                        {
                            /* C1 is the character before D, S1 is the syntax of C1, C2
                           is the character at D, and S2 is the syntax of C2.  */
                            uint c1 = 0, c2 = 0;
                            syntaxcode s1, s2;
                            int dummy = 0;

                            int offset = PTR_TO_OFFSET(d - 1, size1, string1, string2);
                            int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(charpos);

                            GET_CHAR_BEFORE_2(target_multibyte, ref c1, d, new PtrEmulator<re_char>(string1), end1, new PtrEmulator<re_char>(string2), end2);
                            s1 = SYNTAX(c1);

                            UPDATE_SYNTAX_TABLE_FORWARD(charpos + 1);

                            PREFETCH_NOLIMIT(ref d, ref dend, end1, string2, end_match_2);
                            GET_CHAR_AFTER(target_multibyte, ref c2, d, ref dummy);
                            s2 = SYNTAX(c2);

                            if (/* Case 2: Only one of S1 and S2 is Sword.  */
                            ((s1 == syntaxcode.Sword) != (s2 == syntaxcode.Sword))
                                /* Case 3: Both of S1 and S2 are Sword, and macro
                                   WORD_BOUNDARY_P (C1, C2) returns nonzero.  */
                            || ((s1 == syntaxcode.Sword) && WORD_BOUNDARY_P(c1, c2)))
                                not = !not;
                        }
                        if (not)
                            break;
                        else
                            goto fail;

                    case re_opcode_t.wordbeg:
                        DEBUG_PRINT1("EXECUTING wordbeg.\n");

                        /* We FAIL in one of the following cases: */

                        /* Case 1: D is at the end of string.  */
                        if (AT_STRINGS_END(d, end2))
                            goto fail;
                        else
                        {
                            /* C1 is the character before D, S1 is the syntax of C1, C2
                           is the character at D, and S2 is the syntax of C2.  */
                            uint c1 = 0, c2 = 0;
                            syntaxcode s1, s2;
                            int dummy = 0;

                            int offset = PTR_TO_OFFSET(d, size1, string1, string2);
                            int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(charpos);

                            if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                goto fail;

                            GET_CHAR_AFTER(target_multibyte, ref c2, d, ref dummy);
                            s2 = SYNTAX(c2);

                            /* Case 2: S2 is not Sword. */
                            if (s2 != syntaxcode.Sword)
                                goto fail;

                            /* Case 3: D is not at the beginning of string ... */
                            if (!AT_STRINGS_BEG(d, size1, size2, string1, string2))
                            {
                                GET_CHAR_BEFORE_2(target_multibyte, ref c1, d, new PtrEmulator<byte>(string1), end1, new PtrEmulator<byte>(string2), end2);

                                UPDATE_SYNTAX_TABLE_BACKWARD(charpos - 1);

                                s1 = SYNTAX(c1);

                                /* ... and S1 is Sword, and WORD_BOUNDARY_P (C1, C2)
                                   returns 0.  */
                                if ((s1 == syntaxcode.Sword) && !WORD_BOUNDARY_P(c1, c2))
                                    goto fail;
                            }
                        }
                        break;

                    case re_opcode_t.wordend:
                        DEBUG_PRINT1("EXECUTING wordend.\n");

                        /* We FAIL in one of the following cases: */

                        /* Case 1: D is at the beginning of string.  */
                        if (AT_STRINGS_BEG(d, size1, size2, string1, string2))
                            goto fail;
                        else
                        {
                            /* C1 is the character before D, S1 is the syntax of C1, C2
                           is the character at D, and S2 is the syntax of C2.  */
                            uint c1 = 0, c2 = 0;
                            syntaxcode s1, s2;
                            int dummy = 0;

                            int offset = PTR_TO_OFFSET(d, size1, string1, string2) - 1;
                            int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(charpos);

                            GET_CHAR_BEFORE_2(target_multibyte, ref c1, d, new PtrEmulator<byte>(string1), end1, new PtrEmulator<byte>(string2), end2);
                            s1 = SYNTAX(c1);

                            /* Case 2: S1 is not Sword.  */
                            if (s1 != syntaxcode.Sword)
                                goto fail;

                            /* Case 3: D is not at the end of string ... */
                            if (!AT_STRINGS_END(d, end2))
                            {
                                PREFETCH_NOLIMIT(ref d, ref dend, end1, string2, end_match_2);
                                GET_CHAR_AFTER(target_multibyte, ref c2, d, ref dummy);

                                UPDATE_SYNTAX_TABLE_FORWARD(charpos);

                                s2 = SYNTAX(c2);

                                /* ... and S2 is Sword, and WORD_BOUNDARY_P (C1, C2)
                                   returns 0.  */
                                if ((s2 == syntaxcode.Sword) && !WORD_BOUNDARY_P(c1, c2))
                                    goto fail;
                            }
                        }
                        break;

                    case re_opcode_t.symbeg:
                        DEBUG_PRINT1("EXECUTING symbeg.\n");

                        /* We FAIL in one of the following cases: */

                        /* Case 1: D is at the end of string.  */
                        if (AT_STRINGS_END(d, end2))
                            goto fail;
                        else
                        {
                            /* C1 is the character before D, S1 is the syntax of C1, C2
                           is the character at D, and S2 is the syntax of C2.  */
                            uint c1 = 0, c2 = 0;
                            syntaxcode s1, s2;

                            int offset = PTR_TO_OFFSET(d, size1, string1, string2);
                            int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(charpos);

                            if (PREFETCH(ref d, ref dend, string2, end_match_2))
                                goto fail;

                            c2 = RE_STRING_CHAR(d, dend - d, target_multibyte);
                            s2 = SYNTAX(c2);

                            /* Case 2: S2 is neither Sword nor Ssymbol. */
                            if (s2 != syntaxcode.Sword && s2 != syntaxcode.Ssymbol)
                                goto fail;

                            /* Case 3: D is not at the beginning of string ... */
                            if (!AT_STRINGS_BEG(d, size1, size2, string1, string2))
                            {
                                GET_CHAR_BEFORE_2(target_multibyte, ref c1, d, new PtrEmulator<byte>(string1), end1, new PtrEmulator<byte>(string2), end2);

                                UPDATE_SYNTAX_TABLE_BACKWARD(charpos - 1);

                                s1 = SYNTAX(c1);

                                /* ... and S1 is Sword or Ssymbol.  */
                                if (s1 == syntaxcode.Sword || s1 == syntaxcode.Ssymbol)
                                    goto fail;
                            }
                        }
                        break;

                    case re_opcode_t.symend:
                        DEBUG_PRINT1("EXECUTING symend.\n");

                        /* We FAIL in one of the following cases: */

                        /* Case 1: D is at the beginning of string.  */
                        if (AT_STRINGS_BEG(d, size1, size2, string1, string2))
                            goto fail;
                        else
                        {
                            /* C1 is the character before D, S1 is the syntax of C1, C2
                           is the character at D, and S2 is the syntax of C2.  */
                            uint c1 = 0, c2 = 0;
                            syntaxcode s1, s2;

                            int offset = PTR_TO_OFFSET(d, size1, string1, string2) - 1;
                            int charpos = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(charpos);

                            GET_CHAR_BEFORE_2(target_multibyte, ref c1, d, new PtrEmulator<byte>(string1), end1, new PtrEmulator<byte>(string2), end2);
                            s1 = SYNTAX(c1);

                            /* Case 2: S1 is neither Ssymbol nor Sword.  */
                            if (s1 != syntaxcode.Sword && s1 != syntaxcode.Ssymbol)
                                goto fail;

                            /* Case 3: D is not at the end of string ... */
                            if (!AT_STRINGS_END(d, end2))
                            {
                                PREFETCH_NOLIMIT(ref d, ref dend, end1, string2, end_match_2);
                                c2 = RE_STRING_CHAR(d, dend - d, target_multibyte);

                                UPDATE_SYNTAX_TABLE_FORWARD(charpos + 1);

                                s2 = SYNTAX(c2);

                                /* ... and S2 is Sword or Ssymbol.  */
                                if (s2 == syntaxcode.Sword || s2 == syntaxcode.Ssymbol)
                                    goto fail;
                            }
                        }
                        break;

                    case re_opcode_t.syntaxspec:
                    case re_opcode_t.notsyntaxspec:
                        not = (re_opcode_t)p[-1] == re_opcode_t.notsyntaxspec;
                        mcnt = p.ValueAndInc();

                        DEBUG_PRINT3("EXECUTING %ssyntaxspec %d.\n", not ? "not" : "", mcnt);
                        if (PREFETCH(ref d, ref dend, string2, end_match_2))
                            goto fail;
                        {
                            int offset = PTR_TO_OFFSET(d, size1, string1, string2);
                            int pos1 = SYNTAX_TABLE_BYTE_TO_CHAR(offset);
                            UPDATE_SYNTAX_TABLE(pos1);
                        }
                        {
                            int len = 0;
                            uint c = 0;

                            GET_CHAR_AFTER(target_multibyte, ref c, d, ref len);
                            if ((SYNTAX(c) != (syntaxcode)mcnt) ^ not)
                                goto fail;
                            d += len;
                        }
                        break;

                    case re_opcode_t.before_dot:
                        DEBUG_PRINT1("EXECUTING before_dot.\n");
                        if (PTR_BYTE_POS(d) >= PT_BYTE())
                            goto fail;
                        break;

                    case re_opcode_t.at_dot:
                        DEBUG_PRINT1("EXECUTING at_dot.\n");
                        if (PTR_BYTE_POS(d) != PT_BYTE())
                            goto fail;
                        break;

                    case re_opcode_t.after_dot:
                        DEBUG_PRINT1("EXECUTING after_dot.\n");
                        if (PTR_BYTE_POS(d) <= PT_BYTE())
                            goto fail;
                        break;

                    case re_opcode_t.categoryspec:
                    case re_opcode_t.notcategoryspec:
                        not = (re_opcode_t)(p[-1]) == re_opcode_t.notcategoryspec;
                        mcnt = p.ValueAndInc();
                        DEBUG_PRINT3("EXECUTING %scategoryspec %d.\n", not ? "not" : "", mcnt);
                        if (PREFETCH(ref d, ref dend, string2, end_match_2))
                            goto fail;
                        {
                            int len = 0;
                            uint c = 0;

                            GET_CHAR_AFTER(target_multibyte, ref c, d, ref len);
                            if ((!CHAR_HAS_CATEGORY((int)c, mcnt)) ^ not)
                                goto fail;
                            d += len;
                        }
                        break;

                    default:
                        abort();
                        break;
                }
                continue;  /* Successfully executed one pattern command; keep going.  */


            /* We goto here if a matching operation fails. */
            fail:
                IMMEDIATE_QUIT_CHECK();
                if (!FAIL_STACK_EMPTY(fail_stack))
                {
                    PtrEmulator<re_char> str = new PtrEmulator<byte>(), pat = new PtrEmulator<byte>();
                    /* A restart point is known.  Restore to that state.  */
                    DEBUG_PRINT1("\nFAIL:\n");
                    POP_FAILURE_POINT(fail_stack, bufp, pend, size1, size2, string1, string2, ref nfailure_points_popped,
                                      regstart, regend,
                                      ref str, ref pat);
                    switch ((re_opcode_t)pat.ValueAndInc())
                    {
                        case re_opcode_t.on_failure_keep_string_jump:
                            // assert(str == NULL);
                            goto continue_failure_jump;

                        case re_opcode_t.on_failure_jump_nastyloop:
                            // assert ((re_opcode_t)pat[-2] == no_op);
                            PUSH_FAILURE_POINT(fail_stack, bufp, pat - 2, str, size1, size2, string1, string2, pend, ref nfailure_points_pushed);
                            /* Fallthrough */
                            goto case re_opcode_t.on_failure_jump_loop;
                        case re_opcode_t.on_failure_jump_loop:
                        case re_opcode_t.on_failure_jump:
                        case re_opcode_t.succeed_n:
                            d = new PtrEmulator<byte>(str);
                        continue_failure_jump:
                            EXTRACT_NUMBER_AND_INCR(ref mcnt, ref pat);
                        p = pat + mcnt;
                        break;

                        case re_opcode_t.no_op:
                        /* A special frame used for nastyloops. */
                        goto fail;

                        default:
                        abort();
                        break;
                    }

                    // assert (p >= bufp->buffer && p <= pend);

                    if (d >= new PtrEmulator<re_char>(string1) && d <= end1)
                        dend = new PtrEmulator<byte>(end_match_1);
                }
                else
                    break;   /* Matching at this starting point really fails.  */
            } /* for (;;) */

            // if (best_regs_set)
            // goto restore_best_regs;

            // FREE_VARIABLES ();

            return -1;         			/* Failure to match.  */
        }

        /* Return zero if TRANSLATE[S1] and TRANSLATE[S2] are identical for LEN
           bytes; nonzero otherwise.  */
        public static int bcmp_translate(PtrEmulator<re_char> s1, PtrEmulator<re_char> s2, int len, RE_TRANSLATE_TYPE translate, bool target_multibyte)
        {
            PtrEmulator<re_char> p1 = new PtrEmulator<byte>(s1), p2 = new PtrEmulator<byte>(s2);
            PtrEmulator<re_char> p1_end = new PtrEmulator<byte>(s1, len);
            PtrEmulator<re_char> p2_end = new PtrEmulator<byte>(s2, len);

            /* FIXME: Checking both p1 and p2 presumes that the two strings might have
               different lengths, but relying on a single `len' would break this. -sm  */
            while (p1 < p1_end && p2 < p2_end)
            {
                int p1_charlen = 0, p2_charlen = 0;
                uint p1_ch = 0, p2_ch = 0;

                GET_CHAR_AFTER(target_multibyte, ref p1_ch, p1, ref p1_charlen);
                GET_CHAR_AFTER(target_multibyte, ref p2_ch, p2, ref p2_charlen);

                if (RE_TRANSLATE(translate, (int) p1_ch) != RE_TRANSLATE(translate, (int) p2_ch))
                    return 1;

                p1 += p1_charlen;
                p2 += p2_charlen;
            }

            if (p1 != p1_end || p2 != p2_end)
                return 1;

            return 0;
        }

        /* re_compile_fastmap computes a ``fastmap'' for the compiled pattern in
           BUFP.  A fastmap records which of the (1 << BYTEWIDTH) possible
           characters can start a string that matches the pattern.  This fastmap
           is used by re_search to skip quickly over impossible starting points.

           Character codes above (1 << BYTEWIDTH) are not represented in the
           fastmap, but the leading codes are represented.  Thus, the fastmap
           indicates which character sets could start a match.

           The caller must supply the address of a (1 << BYTEWIDTH)-byte data
           area as BUFP->fastmap.

           We set the `fastmap', `fastmap_accurate', and `can_be_null' fields in
           the pattern buffer.

           Returns 0 if we succeed, -2 if an internal error.   */
        public static int re_compile_fastmap (re_pattern_buffer bufp)
        {
            byte[] fastmap = bufp.fastmap;
            int analysis;

            // assert(fastmap && bufp->buffer);
            System.Array.Clear(fastmap, 0, 1 << BYTEWIDTH);  /* Assume nothing's valid.  */
            bufp.fastmap_accurate = true;	    /* It will be when we're done.  */

            analysis = analyse_first(new PtrEmulator<byte>(bufp.buffer), new PtrEmulator<byte>(bufp.buffer, bufp.used),
                fastmap, RE_MULTIBYTE_P(bufp));
            bufp.can_be_null = (analysis != 0);
            return 0;
        } /* re_compile_fastmap */

        /* Set REGS to hold NUM_REGS registers, storing them in STARTS and
           ENDS.  Subsequent matches using PATTERN_BUFFER and REGS will use
           this memory for recording register information.  STARTS and ENDS
           must be allocated using the malloc library routine, and must each
           be at least NUM_REGS * sizeof (regoff_t) bytes long.

           If NUM_REGS == 0, then subsequent matches should allocate their own
           register data.

           Unless this function is called, the first search or match using
           PATTERN_BUFFER will allocate its own register data, without
           freeing the old data.  */
        public static void re_set_registers (re_pattern_buffer bufp, re_registers regs, uint num_regs, regoff_t[] starts, regoff_t[] ends)
        {
            if (num_regs != 0)
            {
                bufp.regs_allocated = re_pattern_buffer.REGS_REALLOCATE;
                regs.num_regs = num_regs;
                regs.start = starts;
                regs.end = ends;
            }
            else
            {
                bufp.regs_allocated = re_pattern_buffer.REGS_UNALLOCATED;
                regs.num_regs = 0;
                regs.start = regs.end = null;
            }
        }
    }
}