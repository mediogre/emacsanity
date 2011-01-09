namespace IronElisp
{
    public partial class Q
    {
        public static LispObject charsetp;
    }

    public partial class V
    {
        /* Hash table that contains attributes of each charset.  Keys are
           charset symbols, and values are vectors of charset attributes.  */
        public static LispObject charset_hash_table;
    }

    public partial class L
    {
        /* Leading-code followed by extended leading-code.    DIMENSION/COLUMN */
        public const byte EMACS_MULE_LEADING_CODE_PRIVATE_11 = 0x9A; /* 1/1 */
        public const byte EMACS_MULE_LEADING_CODE_PRIVATE_12 = 0x9B; /* 1/2 */
        public const byte EMACS_MULE_LEADING_CODE_PRIVATE_21 = 0x9C; /* 2/2 */
        public const byte EMACS_MULE_LEADING_CODE_PRIVATE_22 = 0x9D; /* 2/2 */

        public static charset[] emacs_mule_charset = new charset[256];

        /* If nonzero, don't load charset maps.  */
        public static bool inhibit_load_charset_map
        {
            get { return Defs.B[(int)Bools.inhibit_load_charset_map]; }
            set { Defs.B[(int)Bools.inhibit_load_charset_map] = value; }
        }

        /* Indices to charset attributes vector.  */
        public enum charset_attr_index
        {
            /* ID number of the charset.  */
            charset_id,

            /* Name of the charset (symbol).  */
            charset_name,

            /* Property list of the charset.  */
            charset_plist,

            /* If the method of the charset is `MAP', the value is a mapping
               vector or a file name that contains mapping vector.  Otherwise,
               nil.  */
            charset_map,

            /* If the method of the charset is `MAP', the value is a vector
               that maps code points of the charset to characters.  The vector
               is indexed by a character index.  A character index is
               calculated from a code point and the code-space table of the
               charset.  */
            charset_decoder,

            /* If the method of the charset is `MAP', the value is a
               char-table that maps characters of the charset to code
               points.  */
            charset_encoder,

            /* If the method of the charset is `SUBSET', the value is a vector
               that has this form:

               [ CHARSET-ID MIN-CODE MAX-CODE OFFSET ]

               CHARSET-ID is an ID number of a parent charset.  MIN-CODE and
               MAX-CODE specify the range of characters inherited from the
               parent.  OFFSET is an integer value to add to a code point of
               the parent charset to get the corresponding code point of this
               charset.  */
            charset_subset,

            /* If the method of the charset is `SUPERSET', the value is a list
               whose elements have this form:

               (CHARSET-ID . OFFSET)

               CHARSET-IDs are ID numbers of parent charsets.  OFFSET is an
               integer value to add to a code point of the parent charset to
               get the corresponding code point of this charset.  */
            charset_superset,

            /* The value is a mapping vector or a file name that contains the
               mapping.  This defines how characters in the charset should be
               unified with Unicode.  The value of the member
               `charset_deunifier' is created from this information.  */
            charset_unify_map,

            /* If characters in the charset must be unified Unicode, the value
               is a char table that maps a unified Unicode character code to
               the non-unified character code in the charset.  */
            charset_deunifier,

            /* The length of the charset attribute vector.  */
            charset_attr_max
        }
        
        /* Methods for converting code points and characters of charsets.  */
        public enum charset_method
        {
            /* For a charset of this method, a character code is calculated
               from a character index (which is calculated from a code point)
               simply by adding an offset value.  */
            CHARSET_METHOD_OFFSET,

            /* For a charset of this method, a decoder vector and an encoder
               char-table is used for code point <-> character code
               conversion.  */
            CHARSET_METHOD_MAP,

            /* A charset of this method is a subset of another charset.  */
            CHARSET_METHOD_SUBSET,

            /* A charset of this method is a superset of other charsets.  */
            CHARSET_METHOD_SUPERSET
        }

        public class charset
        {
            /* Index to charset_table.  */
            public int id;

            /* Index to Vcharset_hash_table.  */
            public int hash_index;

            /* Dimension of the charset: 1, 2, 3, or 4.  */
            public int dimension;

            /* Byte code range of each dimension.  <code_space>[4N] is a mininum
               byte code of the (N+1)th dimension, <code_space>[4N+1] is a
               maximum byte code of the (N+1)th dimension, <code_space>[4N+2] is
               (<code_space>[4N+1] - <code_space>[4N] + 1), <code_space>[4N+3]
               is a number of characters containd in the first to (N+1)th
               dismesions.  We get `char-index' of a `code-point' from this
               information.  */
            public uint[] code_space = new uint[16];

            /* If B is a byte of Nth dimension of a code-point, the (N-1)th bit
               of code_space_mask[B] is set.  This array is used to quickly
               check if a code-point is in a valid range.  */
            public byte[] code_space_mask;

            /* 1 if there's no gap in code-points.  */
            public bool code_linear_p;

            /* If the charset is treated as 94-chars in ISO-2022, the value is 0.
               If the charset is treated as 96-chars in ISO-2022, the value is 1.  */
            public int iso_chars_96;

            /* ISO final byte of the charset: 48..127.  It may be -1 if the
               charset doesn't conform to ISO-2022.  */
            public int iso_final;

            /* ISO revision number of the charset.  */
            public int iso_revision;

            /* If the charset is identical to what supported by Emacs 21 and the
               priors, the identification number of the charset used in those
               version.  Otherwise, -1.  */
            public int emacs_mule_id;

            /* Nonzero if the charset is compatible with ASCII.  */
            public bool ascii_compatible_p;

            /* Nonzero if the charset is supplementary.  */
            public int supplementary_p;

            /* Nonzero if all the code points are representable by Lisp_Int.  */
            public bool compact_codes_p;

            /* The method for encoding/decoding characters of the charset.  */
            public charset_method method;

            /* Mininum and Maximum code points of the charset.  */
            public uint min_code, max_code;

            /* Offset value used by macros CODE_POINT_TO_INDEX and
                INDEX_TO_CODE_POINT. .  */
            public uint char_index_offset;

            /* Mininum and Maximum character codes of the charset.  If the
               charset is compatible with ASCII, min_char is a minimum non-ASCII
               character of the charset.  If the method of charset is
               CHARSET_METHOD_OFFSET, even if the charset is unified, min_char
               and max_char doesn't change.  */
            public int min_char, max_char;

            /* The code returned by ENCODE_CHAR if a character is not encodable
               by the charset.  */
            public uint invalid_code;

            /* If the method of the charset is CHARSET_METHOD_MAP, this is a
               table of bits used to quickly and roughly guess if a character
               belongs to the charset.

               The first 64 elements are 512 bits for characters less than
               0x10000.  Each bit corresponds to 128-character block.  The last
               126 elements are 1008 bits for the greater characters
               (0x10000..0x3FFFFF).  Each bit corresponds to 4096-character
               block.

               If a bit is 1, at least one character in the corresponding block is
               in this charset.  */
            public byte[] fast_map = new byte[190];

            /* Offset value to calculate a character code from code-point, and
               visa versa.  */
            public uint code_offset;

            public bool unified_p;
        }

        /* Table of struct charset.  */
        public static charset[] charset_table;

        public static int charset_table_size;
        public static int charset_table_used;

        /* Charset of unibyte characters.  */
        // FIXME_REAL_LATER: this is working only because unibyte charset
        // happens to be the first charset defined. That charset should be
        // explicitly used instead of "charset_unibyte"
        public static int charset_unibyte = 0;

        public static int CODE_POINT_TO_INDEX(charset charsett, uint code)
        {
            if (charsett.code_linear_p)
            {
                return (int) (code - charsett.min_code); 
            }
            else
            {
                if ((charsett.code_space_mask[(code) >> 24] & 0x8) != 0 &&
                    (charsett.code_space_mask[((code) >> 16) & 0xFF] & 0x4) != 0 &&
                    (charsett.code_space_mask[((code) >> 8) & 0xFF] & 0x2) != 0 &&
                    (charsett.code_space_mask[(code) & 0xFF] & 0x1) != 0)
                {
                    return (int) (
                            (((code >> 24) - charsett.code_space[12]) * charsett.code_space[11]) +
                            ((((code >> 16) & 0xFF) - charsett.code_space[8]) * charsett.code_space[7]) +
                            ((((code >> 8) & 0xFF) - charsett.code_space[4]) * charsett.code_space[3]) +
                            ((code & 0xFF) - charsett.code_space[0]) - charsett.char_index_offset
                            ); 
                }
                else
                {
                    return -1;
                }
            }
        }

        public static charset CHARSET_FROM_ID(int id)
        {
            return charset_table[id];
        }

        public static int CHARSET_ID(charset cset)
        {
            return cset.id;
        }

        public static int CHARSET_HASH_INDEX(charset cset)
        {
            return cset.hash_index;
        }

        public static int CHARSET_DIMENSION(charset cset)
        {
            return cset.dimension;
        }

        public static uint[] CHARSET_CODE_SPACE(charset cset)
        {
            return cset.code_space;
        }

        public static bool CHARSET_CODE_LINEAR_P(charset cset)
        {
            return cset.code_linear_p;
        }

        public static int CHARSET_ISO_CHARS_96(charset cset)
        {
            return cset.iso_chars_96;
        }

        public static int CHARSET_ISO_FINAL(charset cset)
        {
            return cset.iso_final;
        }

        public static int CHARSET_ISO_REVISION(charset cset)
        {
            return cset.iso_revision;
        }

        public static int CHARSET_EMACS_MULE_ID(charset cset)
        {
            return cset.emacs_mule_id;
        }

        public static bool CHARSET_ASCII_COMPATIBLE_P(charset cset)
        {
            return cset.ascii_compatible_p;
        }

        public static bool CHARSET_COMPACT_CODES_P(charset cset)
        {
            return cset.compact_codes_p;
        }

        public static charset_method CHARSET_METHOD(charset cset)
        {
            return cset.method;
        }

        public static uint CHARSET_MIN_CODE(charset cset)
        {
            return cset.min_code;
        }

        public static uint CHARSET_MAX_CODE(charset cset)
        {
            return cset.max_code;
        }

        public static uint CHARSET_INVALID_CODE(charset cset)
        {
            return cset.invalid_code;
        }

        public static int CHARSET_MIN_CHAR(charset cset)
        {
            return cset.min_char;
        }

        public static int CHARSET_MAX_CHAR(charset cset)
        {
            return cset.max_char;
        }

        public static uint CHARSET_CODE_OFFSET(charset cset)
        {
            return cset.code_offset;
        }

        public static bool CHARSET_UNIFIED_P(charset cset)
        {
            return cset.unified_p;
        }

        public static LispObject CHARSET_ATTR_ID(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_id);
        }

        public static LispObject CHARSET_ATTR_NAME(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_name);
        }
        
        public static LispObject CHARSET_ATTR_PLIST(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_plist);
        }
        
        public static LispObject CHARSET_ATTR_MAP(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_map);
        }
        
        public static LispObject CHARSET_ATTR_DECODER(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_decoder);
        }
        
        public static LispObject CHARSET_ATTR_ENCODER(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_encoder);
        }
        
        public static LispObject CHARSET_ATTR_SUBSET(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_subset);
        }
        
        public static LispObject CHARSET_ATTR_SUPERSET(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_superset);
        }
        
        public static LispObject CHARSET_ATTR_UNIFY_MAP(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_unify_map);
        }
        
        public static LispObject CHARSET_ATTR_DEUNIFIER(LispObject attrs)
        {
            return AREF (attrs, (int) charset_attr_index.charset_deunifier);
        }

        public static LispObject CHARSET_NAME(charset cset)
        {
            return (CHARSET_ATTR_NAME (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_MAP(charset cset)
        {
            return (CHARSET_ATTR_MAP (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_DECODER(charset cset)
        {
            return (CHARSET_ATTR_DECODER (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_ENCODER(charset cset)
        {
            return (CHARSET_ATTR_ENCODER (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_SUBSET(charset cset)
        {
            return (CHARSET_ATTR_SUBSET (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_SUPERSET(charset cset)
        {
            return (CHARSET_ATTR_SUPERSET (CHARSET_ATTRIBUTES (cset)));
        }
        
        public static LispObject CHARSET_UNIFY_MAP(charset cset)
        {
            return (CHARSET_ATTR_UNIFY_MAP (CHARSET_ATTRIBUTES (cset)));
        }

        public static LispObject CHARSET_DEUNIFIER(charset cset)
        {
            return (CHARSET_ATTR_DEUNIFIER (CHARSET_ATTRIBUTES (cset)));
        }

        /* Return an index to Vcharset_hash_table of the charset whose symbol
           is SYMBOL.  */
        public static int CHARSET_SYMBOL_HASH_INDEX(LispObject symbol)
        {
            return hash_lookup(XHASH_TABLE(V.charset_hash_table), symbol);
        }

        /* Return the attribute vector of CHARSET.  */
        public static LispObject CHARSET_ATTRIBUTES(charset cset)
        {
            return (HASH_VALUE (XHASH_TABLE (V.charset_hash_table), cset.hash_index));
        }

        /* Nonzero if the charset who has FAST_MAP may contain C.  */
        public static byte CHARSET_FAST_MAP_REF(uint c, byte[] fast_map)		
        {
            if (c < 0x10000)
            {
                return (byte)(fast_map[(c) >> 10] & (uint)(1 << (int)((c >> 7) & 7)));
            }
            else
            {
                return (byte)(fast_map[((c) >> 15) + 62] & (uint)(1 << (int)((c >> 12) & 7)));
            }
        }

        /* Return a code point of CHAR in CHARSET.
           Try some optimization before calling encode_char.  */
        public static uint ENCODE_CHAR(charset cset, uint c)
        {
            if (ASCII_CHAR_P(c) && cset.ascii_compatible_p)
            {
                return c;
            }
            else
            {
                if (cset.unified_p ||
                    cset.method == charset_method.CHARSET_METHOD_SUBSET ||
                    cset.method == charset_method.CHARSET_METHOD_SUPERSET)
                {
                    return encode_char(cset, c);
                }
                else
                {
                    if (c < cset.min_char || c > cset.max_char)
                    {
                        return cset.invalid_code;
                    }
                    else
                    {
                        if (cset.method == charset_method.CHARSET_METHOD_OFFSET)
                        {
                            if (cset.code_linear_p)
                            {
                                return c - cset.code_offset + cset.min_code;
                            }
                            else
                            {
                                return encode_char(cset, c);
                            }
                        }
                        else
                        {
                            if ((cset).method == charset_method.CHARSET_METHOD_MAP)
                            {
                                if ((cset).compact_codes_p && CHAR_TABLE_P(CHARSET_ENCODER(cset)))
                                {
                                    LispObject charset_work = CHAR_TABLE_REF(CHARSET_ENCODER(cset), (c));
                                    if (NILP(charset_work))
                                    {
                                        return (cset).invalid_code;
                                    }
                                    else
                                    {
                                        return (uint)XINT(charset_work);
                                    }
                                }
                                else
                                {
                                    return encode_char((cset), (c));
                                }
                            }
                            else
                            {
                                return encode_char((cset), (c));
                            }
                        }
                    }
                }
            }
        }

        /* Check if X is a valid charset symbol.  If valid, set ID to the id
           number of the charset.  Otherwise, signal an error. */
        public static void CHECK_CHARSET_GET_ID(LispObject x, ref int id)
        {
            int idx;

            if (!SYMBOLP(x) || (idx = CHARSET_SYMBOL_HASH_INDEX(x)) < 0)
            {
                wrong_type_argument(Q.charsetp, x);
                return;
            }

            id = XINT(AREF(HASH_VALUE(XHASH_TABLE(V.charset_hash_table), idx),
                     (int) charset_attr_index.charset_id));
        }

        public static void CHECK_CHARSET_GET_CHARSET(LispObject x, ref charset cset)
        {
            int id = 0;
            CHECK_CHARSET_GET_ID(x, ref id);
            cset = CHARSET_FROM_ID(id);		
        }

        /* Return a unified character code for C (>= 0x110000).  VAL is a
           value of Vchar_unify_table for C; i.e. it is nil, an integer, or a
           charset symbol.  */
        public static uint maybe_unify_char(uint c, LispObject val)
        {
            charset cset = null;

            if (INTEGERP(val))
                return (uint)XINT(val);
            if (NILP(val))
                return c;

            CHECK_CHARSET_GET_CHARSET(val, ref cset);
            load_charset(cset, 1);
            if (!inhibit_load_charset_map)
            {
                val = CHAR_TABLE_REF(V.char_unify_table, c);
                if (!NILP(val))
                    c = (uint)XINT(val);
            }
            else
            {
                uint code_index = c - CHARSET_CODE_OFFSET(cset);
                int unified = GET_TEMP_CHARSET_WORK_DECODER(code_index);

                if (unified > 0)
                    c = (uint) unified;
            }
            return c;
        }

        /* Return a character correponding to the code-point CODE of
           CHARSET.  */
        public static int decode_char (charset charsett, uint code)
        {
            int c, char_index;
            charset_method method = CHARSET_METHOD (charsett);

            if (code < CHARSET_MIN_CODE (charsett) || code > CHARSET_MAX_CODE (charsett))
                return -1;

            if (method == charset_method.CHARSET_METHOD_SUBSET)
            {
                LispObject subset_info;

                subset_info = CHARSET_SUBSET (charsett);
                charsett = CHARSET_FROM_ID (XINT (AREF (subset_info, 0)));
                code -= (uint) XINT (AREF (subset_info, 3));
                if (code < XINT (AREF (subset_info, 1))
                    || code > XINT (AREF (subset_info, 2)))
                    c = -1;
                else
                    c = DECODE_CHAR (charsett, code);
            }
            else if (method == charset_method.CHARSET_METHOD_SUPERSET)
            {
                LispObject parents;

                parents = CHARSET_SUPERSET (charsett);
                c = -1;
                for (; CONSP (parents); parents = XCDR (parents))
                {
                    int id = XINT (XCAR (XCAR (parents)));
                    int code_offset = XINT (XCDR (XCAR (parents)));
                    uint this_code = code - (uint) code_offset;

                    charsett = CHARSET_FROM_ID (id);
                    if ((c = DECODE_CHAR (charsett, this_code)) >= 0)
                        break;
                }
            }
            else
            {
                char_index = CODE_POINT_TO_INDEX (charsett, code);
                if (char_index < 0)
                    return -1;

                if (method == charset_method.CHARSET_METHOD_MAP)
                {
                    LispObject decoder;

                    decoder = CHARSET_DECODER (charsett);
                    if (! VECTORP (decoder))
                    {
                        load_charset (charsett, 1);
                        decoder = CHARSET_DECODER (charsett);
                    }
                    if (VECTORP (decoder))
                        c = XINT (AREF (decoder, char_index));
                    else
                        c = GET_TEMP_CHARSET_WORK_DECODER ((uint) char_index);
                }
                else			/* method == CHARSET_METHOD_OFFSET */
                {
                    c = char_index + (int) CHARSET_CODE_OFFSET (charsett);
                    if (CHARSET_UNIFIED_P (charsett)
                        && c > MAX_UNICODE_CHAR)
                    {
                        uint cc = (uint) c;
                        MAYBE_UNIFY_CHAR (ref cc);
                        c = (int)cc;
                    }
                }
            }

            return c;
        }

        /* Return a code-point of CHAR in CHARSET.  If CHAR doesn't belong to
           CHARSET, return CHARSET_INVALID_CODE (CHARSET).  If STRICT is true,
           use CHARSET's strict_max_char instead of max_char.  */
        public static uint encode_char(charset cset, uint c)
        {
            uint code;
            charset_method method = CHARSET_METHOD(cset);

            if (CHARSET_UNIFIED_P(cset))
            {
                LispObject deunifier;
                int code_index = -1;

                deunifier = CHARSET_DEUNIFIER(cset);
                if (!CHAR_TABLE_P(deunifier))
                {
                    load_charset(cset, 2);
                    deunifier = CHARSET_DEUNIFIER(cset);
                }
                if (CHAR_TABLE_P(deunifier))
                {
                    LispObject deunified = CHAR_TABLE_REF(deunifier, c);

                    if (INTEGERP(deunified))
                        code_index = XINT(deunified);
                }
                else
                {
                    code_index = GET_TEMP_CHARSET_WORK_ENCODER(c);
                }
                if (code_index >= 0)
                    c = CHARSET_CODE_OFFSET(cset) + (uint)code_index;
            }

            if (method == charset_method.CHARSET_METHOD_SUBSET)
            {
                LispObject subset_info;
                charset this_charset;

                subset_info = CHARSET_SUBSET(cset);
                this_charset = CHARSET_FROM_ID(XINT(AREF(subset_info, 0)));
                code = ENCODE_CHAR(this_charset, c);
                if (code == CHARSET_INVALID_CODE(this_charset)
                    || code < XINT(AREF(subset_info, 1))
                    || code > XINT(AREF(subset_info, 2)))
                    return CHARSET_INVALID_CODE(cset);
                code += (uint)XINT(AREF(subset_info, 3));
                return code;
            }

            if (method == charset_method.CHARSET_METHOD_SUPERSET)
            {
                LispObject parents;

                parents = CHARSET_SUPERSET(cset);
                for (; CONSP(parents); parents = XCDR(parents))
                {
                    int id = XINT(XCAR(XCAR(parents)));
                    uint code_offset = (uint)XINT(XCDR(XCAR(parents)));
                    charset this_charset = CHARSET_FROM_ID(id);

                    code = ENCODE_CHAR(this_charset, c);
                    if (code != CHARSET_INVALID_CODE(this_charset))
                        return code + code_offset;
                }
                return CHARSET_INVALID_CODE(cset);
            }

            if (CHARSET_FAST_MAP_REF(c, cset.fast_map) == 0 ||
                 c < CHARSET_MIN_CHAR(cset) ||
                 c > CHARSET_MAX_CHAR(cset))
                return CHARSET_INVALID_CODE(cset);

            if (method == charset_method.CHARSET_METHOD_MAP)
            {
                LispObject encoder;
                LispObject val;

                encoder = CHARSET_ENCODER(cset);
                if (!CHAR_TABLE_P(CHARSET_ENCODER(cset)))
                {
                    load_charset(cset, 2);
                    encoder = CHARSET_ENCODER(cset);
                }
                if (CHAR_TABLE_P(encoder))
                {
                    val = CHAR_TABLE_REF(encoder, c);
                    if (NILP(val))
                        return CHARSET_INVALID_CODE(cset);
                    code = (uint)XINT(val);
                    if (!CHARSET_COMPACT_CODES_P(cset))
                        code = INDEX_TO_CODE_POINT(cset, code);
                }
                else
                {
                    code = (uint) GET_TEMP_CHARSET_WORK_ENCODER(c);
                    code = INDEX_TO_CODE_POINT(cset, code);
                }
            }
            else				/* method == CHARSET_METHOD_OFFSET */
            {
                uint code_index = c - CHARSET_CODE_OFFSET(cset);

                code = INDEX_TO_CODE_POINT(cset, code_index);
            }

            return code;
        }

        public static int GET_TEMP_CHARSET_WORK_ENCODER(uint C)
        {
#if COMEBACK_LATER
  ((C) == temp_charset_work->zero_index_char ? 0			  \
   : (C) < 0x20000 ? (temp_charset_work->table.encoder[(C)]		  \
		      ? (int) temp_charset_work->table.encoder[(C)] : -1) \
   : temp_charset_work->table.encoder[(C) - 0x10000]			  \
   ? temp_charset_work->table.encoder[(C) - 0x10000] : -1)
#endif
            throw new System.Exception("No encoder yet");
        }

        public static int GET_TEMP_CHARSET_WORK_DECODER(uint CODE)
        {
#if COMEBACK_LATER
  (temp_charset_work->table.decoder[(CODE)])
#endif
            throw new System.Exception("No decoder yet");
        }

        /* Convert the character index IDX to code-point CODE for CHARSET.
           It is assumed that IDX is in a valid range.  */
        public static uint INDEX_TO_CODE_POINT(charset cset, uint idx)
        {
            if (cset.code_linear_p)
            {
                return idx + cset.min_code;
            }
            else
            {
                idx += cset.char_index_offset; 
                return ((cset.code_space[0] + idx % cset.code_space[2]) |
                        ((cset.code_space[4] + (idx / cset.code_space[3] % cset.code_space[6])) << 8) |
                        ((cset.code_space[8] + (idx / cset.code_space[7] % cset.code_space[10])) << 16) |
                        ((cset.code_space[12] + (idx / cset.code_space[11])) << 24)); 
            }
        }

        /* Load a mapping table for CHARSET.  CONTROL-FLAG tells what kind of
           map it is (see the comment of load_charset_map for the detail).  */
        public static void load_charset(charset cset, int control_flag)
        {
#if COMEBACK_LATER
  Lisp_Object map;

  if (inhibit_load_charset_map
      && temp_charset_work
      && charset == temp_charset_work->current
      && (control_flag == 2 == temp_charset_work->for_encoder))
    return;

  if (CHARSET_METHOD (charset) == CHARSET_METHOD_MAP)
    map = CHARSET_MAP (charset);
  else if (CHARSET_UNIFIED_P (charset))
    map = CHARSET_UNIFY_MAP (charset);
  if (STRINGP (map))
    load_charset_map_from_file (charset, map, control_flag);
  else
    load_charset_map_from_vector (charset, map, control_flag);
}
#endif
            throw new System.Exception("Not yet!");
        }

        public static int DECODE_CHAR(charset charsett, uint code)
        {
            if (ASCII_BYTE_P ((byte) code) && charsett.ascii_compatible_p)
            {
                return (int) code; 
            }
            else
            {                                                       
                if (code < charsett.min_code || code > charsett.max_code)
                {
                    return -1; 
                }
                else
                {
                    if (charsett.unified_p)
                    {
                        return decode_char (charsett, code); 
                    }
                    else
                    {
                        if (charsett.method == charset_method.CHARSET_METHOD_OFFSET)
                        {
                            if (charsett.code_linear_p)
                            {
                                return (int) (code - charsett.min_code + charsett.code_offset);
                            }
                            else
                            {
                                return decode_char (charsett, code); 
                            }
                        }
                        else
                        {
                            if (charsett.method == charset_method.CHARSET_METHOD_MAP)
                            {
                                if (charsett.code_linear_p && VECTORP (CHARSET_DECODER (charsett)))
                                {
                                    return XINT (AREF (CHARSET_DECODER (charsett), (int) (code - charsett.min_code)));
                                }
                                else
                                {
                                    return decode_char (charsett, code);
                                }
                            }
                            else
                            {
                                return decode_char (charsett, code);
                            }
                        }
                    }
                }
            }
        }
    }
}