namespace IronElisp
{
    public partial class Q
    {
        public static LispObject characterp;
    }

    public partial class L
    {
        public const uint MAX_CHAR = 0x3FFFFF;

        /* Maximum N-byte character codes.  */
        public const uint MAX_1_BYTE_CHAR = 0x7F;
        public const uint MAX_2_BYTE_CHAR = 0x7FF;
        public const uint MAX_3_BYTE_CHAR = 0xFFFF;
        public const uint MAX_4_BYTE_CHAR = 0x1FFFFF;
        public const uint MAX_5_BYTE_CHAR = 0x3FFF7F;

        /* Minimum leading code of multibyte characters.  */
        public const uint MIN_MULTIBYTE_LEADING_CODE = 0xC0;

        /* Maximum leading code of multibyte characters.  */
        public const uint MAX_MULTIBYTE_LEADING_CODE = 0xF8;

        /* Maximum Unicode character code.  */
        public const uint MAX_UNICODE_CHAR = 0x10FFFF;

        /* This is the maximum byte length of multibyte form.  */
        public const int MAX_MULTIBYTE_LENGTH = 5;

        /* Variable used locally in the macro FETCH_MULTIBYTE_CHAR.  */
        public static int _fetch_multibyte_char_p;

        /* Mapping table from unibyte chars to multibyte chars.  */
        public static uint[] unibyte_to_multibyte_table = new uint[256];

        /* If C is not ASCII, make it unibyte. */
        public static void MAKE_CHAR_UNIBYTE(ref int c)
        {
            if (!ASCII_CHAR_P((uint) c))
                c = (int) CHAR_TO_BYTE8((uint) c);
        }

        /* If C is not ASCII, make it multibyte.  Assumes C < 256.  */
        public static void MAKE_CHAR_MULTIBYTE(ref int c)
        {
            // (eassert ((c) >= 0 && (c) < 256);
            c = (int)unibyte_to_multibyte_table[c];
        }

        /* Nonzero iff X is a character.  */
        public static bool CHARACTERP(LispObject x)
        {
            return (NATNUMP(x) && XINT(x) <= MAX_CHAR);
        }

        /* Nonzero iff C is valid as a character code.  GENERICP is not used.  */
        public static bool CHAR_VALID_P(int c, bool genericp)
        {
            return ((uint)c <= MAX_CHAR);
        }

        /* Check if Lisp object X is a character or not.  */
        public static void CHECK_CHARACTER(LispObject x)
        {
            CHECK_TYPE(CHARACTERP(x), Q.characterp, x);
        }

        /* Return the character code for raw 8-bit byte BYTE.  */
        public static uint BYTE8_TO_CHAR(byte b)
        {
            return b + 0x3FFF00u;
        }

        /* If character code C has modifier masks, reflect them to the
           character code if possible.  Return the resulting code.  */
        public static uint char_resolve_modifier_mask(uint c)
        {
            /* A non-ASCII character can't reflect modifier bits to the code.  */
            if (!ASCII_CHAR_P((c & ~CHAR_MODIFIER_MASK)))
                return c;

            /* For Meta, Shift, and Control modifiers, we need special care.  */
            if ((c & CHAR_SHIFT) != 0)
            {
                /* Shift modifier is valid only with [A-Za-z].  */
                if ((c & 255) >= 'A' && (c & 255) <= 'Z')
                    c &= ~CHAR_SHIFT;
                else if ((c & 255) >= 'a' && (c & 255) <= 'z')
                    c = (c & ~CHAR_SHIFT) - ('a' - 'A');
                /* Shift modifier for control characters and SPC is ignored.  */
                else if ((c & ~CHAR_MODIFIER_MASK) <= 0x20)
                    c &= ~CHAR_SHIFT;
            }
            if ((c & CHAR_CTL) != 0)
            {
                /* Simulate the code in lread.c.  */
                /* Allow `\C- ' and `\C-?'.  */
                if ((c & 255) == ' ')
                    c &= ~127u & ~CHAR_CTL;
                else if ((c & 255) == '?')
                    c = 127u | (c & ~127u & ~CHAR_CTL);
                /* ASCII control chars are made from letters (both cases),
                   as well as the non-letters within 0100...0137.  */
                else if ((c & 95) >= 65 && (c & 95) <= 90)
                    c &= (31 | (~127u & ~CHAR_CTL));
                else if ((c & 127) >= 64 && (c & 127) <= 95)
                    c &= (31 | (~127u & ~CHAR_CTL));
            }
            if ((c & CHAR_META) != 0)
            {
                /* Move the meta bit to the right place for a string.  */
                c = (c & ~CHAR_META) | 0x80;
            }

            return c;
        }

        /* If C is a character to be unified with a Unicode character, return
           the unified Unicode character.  */
        public static void MAYBE_UNIFY_CHAR(ref uint c)
        {
            if (c > MAX_UNICODE_CHAR && c <= MAX_5_BYTE_CHAR)
            {
                LispObject val = CHAR_TABLE_REF(V.char_unify_table, c);
                if (INTEGERP(val))
                    c = (uint)XINT(val);
                else if (!NILP(val))
                    c = maybe_unify_char(c, val);
            }
        }

        /* Return the width of ASCII character C.  The width is measured by
           how many columns C will occupy on the screen when displayed in the
           current buffer.  */
        public static int ASCII_CHAR_WIDTH(uint c)
        {
            return (c < 0x20
   ? (c == '\t'
      ? XINT(current_buffer.tab_width)
      : (c == '\n' ? 0 : (NILP(current_buffer.ctl_arrow) ? 4 : 2)))
   : (c < 0x7f
      ? 1
      : ((NILP(current_buffer.ctl_arrow) ? 4 : 2))));
        }
        /* Return the width of character C.  The width is measured by how many
           columns C will occupy on the screen when displayed in the current
           buffer.  */
        public static int CHAR_WIDTH(uint c)
        {
            return (ASCII_CHAR_P(c)
   ? ASCII_CHAR_WIDTH(c)
   : XINT(CHAR_TABLE_REF(V.char_width_table, c)));
        }

        public static int char_string(uint c, byte[] p)
        {
            return char_string(c, p, 0);
        }
        /* Store multibyte form of character C at P.  If C has modifier bits,
           handle them appropriately.  */
        public static int char_string(uint c, byte[] p, int i)
        {
            int bytes = 0;

            if ((c & CHAR_MODIFIER_MASK) > 0)
            {
                c = char_resolve_modifier_mask(c);
                /* If C still has any modifier bits, just ignore it.  */
                c &= ~CHAR_MODIFIER_MASK;
            }

            MAYBE_UNIFY_CHAR(ref c);

            if (c <= MAX_3_BYTE_CHAR)
            {
                bytes = CHAR_STRING(c, p, i);
            }
            else if (c <= MAX_4_BYTE_CHAR)
            {
                p[i] = (byte)(0xF0 | (c >> 18));
                p[i + 1] = (byte)(0x80 | ((c >> 12) & 0x3F));
                p[i + 2] = (byte)(0x80 | ((c >> 6) & 0x3F));
                p[i + 3] = (byte)(0x80 | (c & 0x3F));
                bytes = 4;
            }
            else if (c <= MAX_5_BYTE_CHAR)
            {
                p[i] = 0xF8;
                p[i + 1] = (byte)(0x80 | ((c >> 18) & 0x0F));
                p[i + 2] = (byte)(0x80 | ((c >> 12) & 0x3F));
                p[i + 3] = (byte)(0x80 | ((c >> 6) & 0x3F));
                p[i + 4] = (byte)(0x80 | (c & 0x3F));
                bytes = 5;
            }
            else if (c <= MAX_CHAR)
            {
                c = CHAR_TO_BYTE8(c);
                bytes = BYTE8_STRING(c, p, i);
            }
            else
                error("Invalid character: %d", c);

            return bytes;
        }

        /* Like STRING_CHAR, but advance P to the end of multibyte form.  */
        public static uint STRING_CHAR_ADVANCE(byte[] p, ref int i)
        {
            if ((p[i] & 0x80) == 0)
            {
                i += 1;
                return p[i - 1];
            }
            else
            {
                if ((p[i] & 0x20) == 0)
                {
                    i += 2;
                    return ((uint)((p[i - 2] & 0x1F) << 6) |
                            (uint)(p[i - 1] & 0x3F) |
                            (uint)((p[i - 2]) < 0xC2 ? 0x3FFF80u : 0));
                }
                else
                {
                    if ((p[i] & 0x10) == 0)
                    {
                        i += 3;
                        return ((uint)((p[i - 3] & 0x0F) << 12) |
                                (uint)((p[i - 2] & 0x3F) << 6) |
                                (uint)(p[i - 1] & 0x3F));
                    }
                    else
                    {
                        int len;
                        return string_char(p, i, out i, out len);
                    }
                }
            }
        }

        /* Return a character whose multibyte form is at P.  Set LEN is not
           NULL, it must be a pointer to integer.  In that case, set *LEN to
           the byte length of the multibyte form.  If ADVANCED is not NULL, is
           must be a pointer to unsigned char.  In that case, set *ADVANCED to
           the ending address (i.e. the starting address of the next
           character) of the multibyte form.  */
        public static uint string_char(byte[] p, int i, out int advanced, out int len)
        {
            uint c;
            int saved_p = i;

            if (p[i] < 0x80 || (p[i] & 0x20) == 0 || (p[i] & 0x10) == 0)
            {
                c = STRING_CHAR_ADVANCE(p, ref i);
            }
            else if ((p[i] & 0x08) == 0)
            {
                c = ((uint)((p[i] & 0xF) << 18) |
                      (uint)((p[i + 1] & 0x3F) << 12) |
                      (uint)((p[i + 2] & 0x3F) << 6) |
                      (uint)(p[i + 3] & 0x3F));
                i += 4;
            }
            else
            {
                c = ((uint)((p[i + 1] & 0x3F) << 18) |
                      (uint)((p[i + 2] & 0x3F) << 12) |
                      (uint)((p[i + 3] & 0x3F) << 6) |
                      (uint)(p[i + 4] & 0x3F));
                i += 5;
            }

            MAYBE_UNIFY_CHAR(ref c);

            len = i - saved_p;
            advanced = i;
            return c;
        }

        /* If P is after LIMIT, advance P to the previous character boundary.
           Assumes that P is already at a character boundary of the same
           mulitbyte form whose beginning address is LIMIT.  */
        public static void PREV_CHAR_BOUNDARY(ref PtrEmulator<byte> p, PtrEmulator<byte> limit)
        {
            if (p > limit)
            {
                PtrEmulator<byte> p0 = p;
                do
                {
                    p0--;
                } while (p0 >= limit && !CHAR_HEAD_P(p0.Value));
                p = (BYTES_BY_CHAR_HEAD(p0.Value) == p - p0) ? p0 : p - 1;
            }
        }

        /* Return the character code of character whose multibyte form is at
           P.  The argument LEN is ignored.  It will be removed in the
           future.  */
        public static uint STRING_CHAR(byte[] p, int i, int len)
        {
            if ((p[i] & 0x80) == 0)
            {
                return p[i];
            }
            else
            {
                if ((p[i] & 0x20) == 0)
                {
                    return (uint)(((p[i] & 0x1F) << 6) |
                            (p[i + 1] & 0x3F)) +
                        ((p[i]) < 0xC2 ? 0x3FFF80u : 0);
                }
                else
                {
                    if ((p[i] & 0x10) == 0)
                    {
                        return (uint)((p[i] & 0x0F) << 12) |
                            (uint)((p[i + 1] & 0x3F) << 6) |
                            (uint)(p[i + 2] & 0x3F);
                    }
                    else
                    {
                        int dummy, dummy2;
                        return string_char(p, i, out dummy, out dummy2);
                    }
                }
            }
        }

        public static uint STRING_CHAR_AND_LENGTH(PtrEmulator<byte> t, int len, ref int actual_len)
        {
            return STRING_CHAR_AND_LENGTH(t.Collection, t.Index, len, ref actual_len);
        }

        /* Like STRING_CHAR, but set ACTUAL_LEN to the length of multibyte
           form.  The argument LEN is ignored.  It will be removed in the
           future.  */
        public static uint STRING_CHAR_AND_LENGTH(byte[] p, int i, int len, ref int actual_len)
        {
            if ((p[i] & 0x80) == 0)
            {
                actual_len = 1;
                return p[i];
            }
            else
            {
                if ((p[i] & 0x20) == 0)
                {
                    actual_len = 2;
                    return (uint)(((p[i] & 0x1F) << 6) |
                            (p[i + 1] & 0x3F)) +
                        (p[i] < 0xC2 ? 0x3FFF80u : 0);
                }
                else
                {
                    if ((p[i] & 0x10) == 0)
                    {
                        actual_len = 3;
                        return ((uint)((p[i] & 0x0F) << 12) |
                                (uint)((p[i + 1] & 0x3F) << 6) |
                                (uint)(p[i + 2] & 0x3F));
                    }
                    else
                    {
                        int dummy;
                        return string_char(p, i, out dummy, out actual_len);
                    }
                }
            }
        }

        /* Fetch the "next" character from Lisp string STRING at byte position
           BYTEIDX, character position CHARIDX.  Store it into OUTPUT.

           All the args must be side-effect-free.
           BYTEIDX and CHARIDX must be lvalues;
           we increment them past the character fetched.  */
        public static void FETCH_STRING_CHAR_ADVANCE(ref int OUTPUT, LispObject STRING, ref int CHARIDX, ref int BYTEIDX)
        {
            CHARIDX++;
            if (STRING_MULTIBYTE(STRING))
            {
                int len = 0;

                OUTPUT = (int)STRING_CHAR_AND_LENGTH(SDATA(STRING), BYTEIDX, 0, ref len);
                BYTEIDX += len;
            }
            else
            {
                OUTPUT = SREF(STRING, BYTEIDX);
                BYTEIDX++;
            }
        }

        /* Like FETCH_STRING_CHAR_ADVANCE, but return a multibyte character
           even if STRING is unibyte.  */
        public static void FETCH_STRING_CHAR_AS_MULTIBYTE_ADVANCE(ref int OUTPUT, LispObject STRING, ref int CHARIDX, ref int BYTEIDX)
        {
            CHARIDX++;
            if (STRING_MULTIBYTE(STRING))
            {
                PtrEmulator<byte> ptr = new PtrEmulator<byte>(SDATA(STRING), BYTEIDX);
                int len = 0;

                OUTPUT = (int) STRING_CHAR_AND_LENGTH(ptr, 0, ref len);
                BYTEIDX += len;
            }
            else
            {
                OUTPUT = SREF(STRING, BYTEIDX);
                BYTEIDX++;
                MAKE_CHAR_MULTIBYTE(ref OUTPUT);
            }
        }

        /* Like FETCH_STRING_CHAR_ADVANCE, but fetch character from the current
           buffer.  */
        public static void FETCH_CHAR_ADVANCE(ref int OUTPUT, ref int CHARIDX, ref int BYTEIDX)
        {
            CHARIDX++;
            if (!NILP(current_buffer.enable_multibyte_characters))
            {
                PtrEmulator<byte> ptr = BYTE_POS_ADDR(BYTEIDX);
                int len = 0;

                OUTPUT = (int) STRING_CHAR_AND_LENGTH(ptr.Collection, ptr.Index, 0, ref len);
                BYTEIDX += len;
            }
            else
            {
                PtrEmulator<byte> ptr = BYTE_POS_ADDR(BYTEIDX);
                OUTPUT = ptr.Value;
                BYTEIDX++;
            }
        }

        /* Like FETCH_STRING_CHAR_ADVANCE, but assumes STRING is multibyte.  */
        public static void FETCH_STRING_CHAR_ADVANCE_NO_CHECK(ref int OUTPUT, LispObject STRING, ref int CHARIDX, ref int BYTEIDX)
        {
            int len = 0;

            OUTPUT = (int)STRING_CHAR_AND_LENGTH(SDATA(STRING), BYTEIDX, 0, ref len);
            BYTEIDX += len;
            CHARIDX++;
        }

        /* Increment the buffer byte position POS_BYTE of the current buffer to
           the next character boundary.  No range checking of POS.  */
        public static void INC_POS(ref int pos_byte)
        {
            PtrEmulator<byte> p = BYTE_POS_ADDR(pos_byte);
            pos_byte += BYTES_BY_CHAR_HEAD(p.Value);
        }

        /* Decrement the buffer byte position POS_BYTE of the current buffer to
           the previous character boundary.  No range checking of POS.  */
        public static void DEC_POS(ref int pos_byte)
        {
            int p;

            pos_byte--;
            if (pos_byte < GPT_BYTE)
                p = pos_byte - BEG_BYTE();
            else
                p = GAP_SIZE + pos_byte - BEG_BYTE();

            while (!CHAR_HEAD_P(BEG_ADDR()[p]))
            {
                p--;
                pos_byte--;
            }
        }

        /* Increment both CHARPOS and BYTEPOS, each in the appropriate way.  */
        public static void INC_BOTH(ref int charpos, ref int bytepos)
        {
            charpos++;
            if (NILP(current_buffer.enable_multibyte_characters))
                bytepos++;
            else
                INC_POS(ref bytepos);
        }

        /* Decrement both CHARPOS and BYTEPOS, each in the appropriate way.  */
        public static void DEC_BOTH(ref int charpos, ref int bytepos)
        {
            charpos--;
            if (NILP(current_buffer.enable_multibyte_characters))
                bytepos--;
            else
                DEC_POS(ref bytepos);
        }

        /* Increment the buffer byte position POS_BYTE of the current buffer to
           the next character boundary.  This macro relies on the fact that
           *GPT_ADDR and *Z_ADDR are always accessible and the values are
           '\0'.  No range checking of POS_BYTE.  */
        public static void BUF_INC_POS(Buffer buf, ref int pos_byte)
        {
            int p = BUF_BYTE_ADDRESS(buf, pos_byte);
            pos_byte += BYTES_BY_CHAR_HEAD(buf.text.beg[p]);
        }

        /* Decrement the buffer byte position POS_BYTE of the current buffer to
           the previous character boundary.  No range checking of POS_BYTE.  */
        public static void BUF_DEC_POS(Buffer buf, ref int pos_byte)
        {
            int p;
            pos_byte--;
            if (pos_byte < BUF_GPT_BYTE(buf))
                p = pos_byte - BEG_BYTE();
            else
                p = BUF_GAP_SIZE(buf) + pos_byte - BEG_BYTE();
            while (!CHAR_HEAD_P(BUF_BEG_ADDR(buf)[p]))
            {
                p--;
                pos_byte--;
            }
        }

        /* Nonzero iff C is a character that corresponds to a raw 8-bit
           byte.  */
        public static bool CHAR_BYTE8_P(uint c)
        {
            return (c > MAX_5_BYTE_CHAR);
        }

        /* Return the raw 8-bit byte for character C.  */
        public static uint CHAR_TO_BYTE8(uint c)
        {
            if (CHAR_BYTE8_P(c))
            {
                return c - 0x3FFF00;
            }
            else
            {
                return multibyte_char_to_unibyte(c, Q.nil);
            }
        }

        /* Return the raw 8-bit byte for character C,
           or -1 if C doesn't correspond to a byte.  */
        public static int CHAR_TO_BYTE_SAFE(uint c)
        {
            return (CHAR_BYTE8_P(c)
             ? (int) c - 0x3FFF00
             : multibyte_char_to_unibyte_safe(c));
        }

        /* Convert the multibyte character C to unibyte 8-bit character based
           on the current value of charset_unibyte.  If dimension of
           charset_unibyte is more than one, return (C & 0xFF).

           The argument REV_TBL is now ignored.  It will be removed in the
           future.  */

        public static uint multibyte_char_to_unibyte(uint c, LispObject rev_tbl)
        {
            if (CHAR_BYTE8_P(c))
                return CHAR_TO_BYTE8(c);

            charset cset = CHARSET_FROM_ID(charset_unibyte);
            uint c1 = ENCODE_CHAR(cset, c);
            return ((c1 != CHARSET_INVALID_CODE(cset)) ? c1 : c & 0xFF);
        }

        /* Like multibyte_char_to_unibyte, but return -1 if C is not supported
           by charset_unibyte.  */
        public static int multibyte_char_to_unibyte_safe(uint c)
        {
            charset charset;
            uint c1;

            if (CHAR_BYTE8_P(c))
                return (int) CHAR_TO_BYTE8(c);
            charset = CHARSET_FROM_ID(charset_unibyte);
            c1 = ENCODE_CHAR(charset, c);
            return ((c1 != CHARSET_INVALID_CODE(charset)) ? (int) c1 : -1);
        }

        /* Nonzero iff C is an ASCII character.  */
        public static bool ASCII_CHAR_P(uint c)
        {
            return c < 0x80;
        }

        /* Return byte length of multibyte form for character C.  */
        public static int CHAR_BYTES(uint c)
        {
            return ((c) <= MAX_1_BYTE_CHAR ? 1
           : (c) <= MAX_2_BYTE_CHAR ? 2
           : (c) <= MAX_3_BYTE_CHAR ? 3
           : (c) <= MAX_4_BYTE_CHAR ? 4
           : (c) <= MAX_5_BYTE_CHAR ? 5
           : 2);
        }

        /* Return the leading code of multibyte form of C.  */
        public static byte CHAR_LEADING_CODE(uint c)
        {
            return ((c) <= MAX_1_BYTE_CHAR ? (byte) c
             : (c) <= MAX_2_BYTE_CHAR ? (byte) (0xC0 | ((c) >> 6))
             : (c) <= MAX_3_BYTE_CHAR ? (byte) (0xE0 | ((c) >> 12))
             : (c) <= MAX_4_BYTE_CHAR ? (byte) (0xF0 | ((c) >> 18))
             : (c) <= MAX_5_BYTE_CHAR ? (byte) 0xF8
             : (byte) (0xC0 | (((c) >> 6) & 0x01)));
        }

        public static int CHAR_STRING(uint c, byte[] p)
        {
            return CHAR_STRING(c, p, 0);
        }

        /* Store multibyte form of the character C in P.  The caller should
           allocate at least MAX_MULTIBYTE_LENGTH bytes area at P in advance.
           Returns the length of the multibyte form.  */
        public static int CHAR_STRING(uint c, byte[] p, int i)
        {
            if (c <= MAX_1_BYTE_CHAR)
            {
                p[i] = (byte)c;
                return 1;
            }
            else
            {
                if (c <= MAX_2_BYTE_CHAR)
                {
                    p[i] = (byte)(0xC0 | (c >> 6));
                    p[i + 1] = (byte)(0x80 | (c & 0x3F));
                    return 2;
                }
                else
                {
                    if (c <= MAX_3_BYTE_CHAR)
                    {
                        p[i] = (byte)(0xE0 | (c >> 12));
                        p[i + 1] = (byte)(0x80 | ((c >> 6) & 0x3F));
                        p[i + 2] = (byte)(0x80 | (c & 0x3F));
                        return 3;
                    }
                    else
                    {
                        return char_string(c, p, i);
                    }
                }
            }
        }

        public static int BYTE8_STRING(uint b, byte[] p)
        {
            return BYTE8_STRING(b, p, 0);
        }

        /* Store multibyte form of byte B in P.  The caller should allocate at
           least MAX_MULTIBYTE_LENGTH bytes area at P in advance.  Returns the
           length of the multibyte form.  */
        public static int BYTE8_STRING(uint b, byte[] p, int i)
        {
            p[i] = (byte)(0xC0 | ((b >> 6) & 0x01));
            p[i + 1] = (byte)(0x80 | (b & 0x3F));
            return 2;
        }

        /* The byte length of multibyte form at unibyte string P ending at
           PEND.  If STR doesn't point to a valid multibyte form, return 0.  */
        public static int MULTIBYTE_LENGTH(byte[] p, int i, int pend)
        {
            if (i >= pend)
                return 0;
            else
            {
                if ((p[i] & 0x80) == 0)
                    return 1;
                else
                {
                    if ((i + 1 >= pend) || ((p[i + 1] & 0xC0) != 0x80))
                        return 0;
                    else
                    {
                        if ((p[i] & 0xE0) == 0xC0)
                            return 2;
                        else
                        {
                            if ((i + 2 >= pend) || ((p[i + 2] & 0xC0) != 0x80))
                                return 0;
                            else
                            {
                                if ((p[i] & 0xF0) == 0xE0)
                                    return 3;
                                else
                                {
                                    if ((i + 3 >= pend) || ((p[i + 3] & 0xC0) != 0x80))
                                        return 0;
                                    else
                                    {
                                        if ((p[i] & 0xF8) == 0xF0)
                                            return 4;
                                        else
                                        {
                                            if ((i + 4 >= pend) || ((p[i + 4] & 0xC0) != 0x80))
                                                return 0;
                                            else
                                            {
                                                if (p[i] == 0xF8 && (p[i + 1] & 0xF0) == 0x80)
                                                    return 5;
                                                else
                                                    return 0;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /* Like MULTIBYTE_LENGTH, but don't check the ending address.  */
        public static int MULTIBYTE_LENGTH_NO_CHECK(byte[] p, int i)
        {
            if ((p[i] & 0x80) == 0)
                return 1;
            else
            {
                if ((p[i + 1] & 0xC0) != 0x80)
                    return 0;
                else
                {
                    if ((p[i] & 0xE0) == 0xC0)
                        return 2;
                    else
                    {
                        if ((p[i + 2] & 0xC0) != 0x80)
                            return 0;
                        else
                        {
                            if ((p[i] & 0xF0) == 0xE0)
                                return 3;
                            else
                            {
                                if ((p[i + 3] & 0xC0) != 0x80)
                                    return 0;
                                else
                                {
                                    if ((p[i] & 0xF8) == 0xF0)
                                        return 4;
                                    else
                                    {
                                        if ((p[i + 4] & 0xC0) != 0x80)
                                            return 0;
                                        else
                                        {
                                            if (p[i] == 0xF8 && (p[i + 1] & 0xF0) == 0x80)
                                                return 5;
                                            else
                                                return 0;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /* Parse unibyte text at STR of LEN bytes as a multibyte text, count
           characters and bytes in it, and store them in *NCHARS and *NBYTES
           respectively.  On counting bytes, pay attention to that 8-bit
           characters not constructing a valid multibyte sequence are
           represented by 2-byte in a multibyte text.  */
        public static void parse_str_as_multibyte(byte[] ary, int len, ref int nchars, ref int nbytes)
        {
            int str = 0;
            int endp = len;
            int n, chars = 0, bytes = 0;

            if (len >= MAX_MULTIBYTE_LENGTH)
            {
                int adjusted_endp = endp - MAX_MULTIBYTE_LENGTH;
                while (str < adjusted_endp)
                {
                    if ((n = MULTIBYTE_LENGTH_NO_CHECK(ary, str)) > 0)
                    {
                        str += n;
                        bytes += n;
                    }
                    else
                    {
                        str++;
                        bytes += 2;
                    }
                    chars++;
                }
            }
            while (str < endp)
            {
                if ((n = MULTIBYTE_LENGTH(ary, str, endp)) > 0)
                {
                    str += n;
                    bytes += n;
                }
                else
                {
                    str++;
                    bytes += 2;
                }
                chars++;
            }

            nchars = chars;
            nbytes = bytes;
        }

        /* Arrange unibyte text at STR of NBYTES bytes as a multibyte text.
           It actually converts only such 8-bit characters that don't contruct
           a multibyte sequence to multibyte forms of Latin-1 characters.  If
           NCHARS is nonzero, set *NCHARS to the number of characters in the
           text.  It is assured that we can use LEN bytes at STR as a work
           area and that is enough.  Return the number of bytes of the
           resulting text.  */

        public static int str_as_multibyte(byte[] str, int len, int nbytes, ref int nchars)
        {
            int p = 0, endp = nbytes;
            int chars = 0;
            int n;

            if (nbytes >= MAX_MULTIBYTE_LENGTH)
            {
                int adjusted_endp = endp - MAX_MULTIBYTE_LENGTH;
                while (p < adjusted_endp
                       && (n = MULTIBYTE_LENGTH_NO_CHECK(str, p)) > 0)
                {
                    p += n;
                    chars++;
                }
            }
            while ((n = MULTIBYTE_LENGTH(str, p, endp)) > 0)
            {
                p += n;
                chars++;
            }

            nchars = chars;
            if (p == endp)
                return nbytes;

            int to = p;
            nbytes = endp - p;
            endp = len;
            safe_bcopy(str, p, str, endp - nbytes, nbytes);
            p = endp - nbytes;

            if (nbytes >= MAX_MULTIBYTE_LENGTH)
            {
                int adjusted_endp = endp - MAX_MULTIBYTE_LENGTH;
                while (p < adjusted_endp)
                {
                    if ((n = MULTIBYTE_LENGTH_NO_CHECK(str, p)) > 0)
                    {
                        while (n-- != 0)
                        {
                            str[to++] = str[p++];
                        }
                    }
                    else
                    {
                        uint c = str[p++];
                        c = BYTE8_TO_CHAR((byte)c);
                        to += CHAR_STRING(c, str, to);
                    }
                }
                chars++;
            }
            while (p < endp)
            {
                if ((n = MULTIBYTE_LENGTH(str, p, endp)) > 0)
                {
                    while (n-- != 0)
                    {
                        str[to++] = str[p++];
                    }
                }
                else
                {
                    uint c = str[p++];
                    c = BYTE8_TO_CHAR((byte)c);
                    to += CHAR_STRING(c, str, to);
                }
                chars++;
            }

            nchars = chars;
            return to;
        }

        /* Nonzero iff BYTE is the 1st byte of a multibyte form of a character
           that corresponds to a raw 8-bit byte.  */
        public static bool CHAR_BYTE8_HEAD_P(uint b)
        {
            return (b == 0xC0 || b == 0xC1);
        }

        /* Arrange multibyte text at STR of LEN bytes as a unibyte text.  It
           actually converts characters in the range 0x80..0xFF to
           unibyte.  */
        public static int str_as_unibyte(byte[] str, int bytes)
        {
            int p = 0;
            int endp = bytes;

            int to;
            uint c;
            int len;

            while (p < endp)
            {
                c = str[p];
                len = BYTES_BY_CHAR_HEAD(c);
                if (CHAR_BYTE8_HEAD_P(c))
                    break;
                p += len;
            }
            to = p;
            while (p < endp)
            {
                c = str[p];
                len = BYTES_BY_CHAR_HEAD(c);
                if (CHAR_BYTE8_HEAD_P(c))
                {
                    c = STRING_CHAR_ADVANCE(str, ref p);
                    str[to++] = (byte)CHAR_TO_BYTE8(c);
                }
                else
                {
                    while (len-- > 0)
                    {
                        str[to++] = str[p++];
                    }
                }
            }
            return to;
        }

        /* Nonzero iff C is an ASCII byte.  */
        public static bool ASCII_BYTE_P(byte c)
        {
            return c < 0x80;
        }

        /* Nonzero iff C is a character of code less than 0x100.  */
        public static bool SINGLE_BYTE_CHAR_P(uint c)
        {
            return c < 0x100;
        }

        /* Nonzero iff BYTE starts a non-ASCII character in a multibyte
           form.  */
        public static bool LEADING_CODE_P(byte b)
        {
            return ((b & 0xC0) == 0xC0);
        }

        /* Nonzero iff BYTE is a trailing code of a non-ASCII character in a
           multibyte form.  */
        public static bool TRAILING_CODE_P(byte b)
        {
            return ((b & 0xC0) == 0x80);
        }

        /* Nonzero iff BYTE starts a character in a multibyte form.
           This is equivalent to:
            (ASCII_BYTE_P (byte) || LEADING_CODE_P (byte))  */
        public static bool CHAR_HEAD_P(byte b)
        {
            return ((b & 0xC0) != 0x80);
        }

        /* Kept for backward compatibility.  This macro will be removed in the
           future.  */
        public static bool BASE_LEADING_CODE_P(byte b)
        {
            return LEADING_CODE_P(b);
        }

        /* How many bytes a character that starts with BYTE occupies in a
           multibyte form.  */
        public static int BYTES_BY_CHAR_HEAD(uint b)
        {
            if ((b & 0x80) == 0)
                return 1;
            else
            {
                if ((b & 0x20) == 0)
                    return 2;
                else
                {
                    if ((b & 0x10) == 0)
                        return 3;
                    else
                    {
                        if ((b & 0x08) == 0)
                            return 4;
                        else
                            return 5;
                    }
                }
            }
        }

        /* Return the length of the multi-byte form at string STR of length
           LEN while assuming that STR points a valid multi-byte form.  As
           this macro isn't necessary anymore, all callers will be changed to
           use BYTES_BY_CHAR_HEAD directly in the future.  */
        public static int MULTIBYTE_FORM_LENGTH(PtrEmulator<byte> str, int len)
        {
            return BYTES_BY_CHAR_HEAD(str.Value);
        }

        /* Return the number of characters in the NBYTES bytes at PTR.
           This works by looking at the contents and checking for multibyte
           sequences while assuming that there's no invalid sequence.  It
           ignores enable-multibyte-characters.  */

        public static int multibyte_chars_in_text(byte[] contents, int ptr, int nbytes)
        {
            int endp = nbytes;
            int chars = 0;

            while (ptr < endp)
            {
                int len = MULTIBYTE_LENGTH(contents, ptr, endp);

                if (len == 0)
                    abort();
                ptr += len;
                chars++;
            }

            return chars;
        }

        /* Convert the unibyte character C to the corresponding multibyte
           character.  If C can't be converted, return C.  */
        public static uint unibyte_char_to_multibyte(uint c)
        {
            return (c < 256 ? unibyte_to_multibyte_table[c] : c);
        }

        /* Parse multibyte string STR of length LENGTH and set BYTES to the
           byte length of a character at STR while assuming that STR points a
           valid multibyte form.  As this macro isn't necessary anymore, all
           callers will be changed to use BYTES_BY_CHAR_HEAD directly in the
           future.  */
        public static void PARSE_MULTIBYTE_SEQ(byte[] str, int idx, int length, ref int bytes)
        {
            bytes = BYTES_BY_CHAR_HEAD(str[idx]);
        }

        /* Return the number of characters in the NBYTES bytes at PTR.
           This works by looking at the contents and checking for multibyte
           sequences while assuming that there's no invalid sequence.
           However, if the current buffer has enable-multibyte-characters =
           nil, we treat each byte as a character.  */
        public static int chars_in_text(byte[] ptr, int nbytes)
        {
            /* current_buffer is null at early stages of Emacs initialization.  */
            if (current_buffer == null
                || NILP(current_buffer.enable_multibyte_characters))
                return nbytes;

            return multibyte_chars_in_text(ptr, nbytes);
        }

        /* Return the number of characters in the NBYTES bytes at PTR.
           This works by looking at the contents and checking for multibyte
           sequences while assuming that there's no invalid sequence.  It
           ignores enable-multibyte-characters.  */
        public static int multibyte_chars_in_text(byte[] str, int nbytes)
        {
            int ptr = 0;
            int endp = nbytes;
            int chars = 0;

            while (ptr < endp)
            {
                int len = MULTIBYTE_LENGTH(str, ptr, endp);

                if (len == 0)
                    abort();
                ptr += len;
                chars++;
            }

            return chars;
        }
    }

    public partial class V
    {
        /* Char-table of information about which character to unify to which
           Unicode character.  Mainly used by the macro MAYBE_UNIFY_CHAR.  */
        public static LispObject char_unify_table;

        /* Vector of translation table ever defined.
           ID of a translation table is used to index this vector.  */
        public static LispObject translation_table_vector
        {
            get { return Defs.O[(int)Objects.translation_table_vector]; }
            set { Defs.O[(int)Objects.translation_table_vector] = value; }
        }

        /* A char-table for characters which may invoke auto-filling.  */
        public static LispObject auto_fill_chars
        {
            get { return Defs.O[(int)Objects.auto_fill_chars]; }
            set { Defs.O[(int)Objects.auto_fill_chars] = value; }
        }

        /* A char-table.  An element is a column-width of the corresponding
           character.  */
        public static LispObject char_width_table
        {
            get { return Defs.O[(int)Objects.char_width_table]; }
            set { Defs.O[(int)Objects.char_width_table] = value; }
        }

        /* A char-table.  An element is a symbol indicating the direction
           property of corresponding character.  */
        public static LispObject char_direction_table
        {
            get { return Defs.O[(int)Objects.char_direction_table]; }
            set { Defs.O[(int)Objects.char_direction_table] = value; }
        }

        /* A char-table.  An element is non-nil iff the corresponding
           character has a printable glyph.  */
        public static LispObject printable_chars
        {
            get { return Defs.O[(int)Objects.printable_chars]; }
            set { Defs.O[(int)Objects.printable_chars] = value; }
        }

        /* Char table of scripts.  */
        public static LispObject char_script_table
        {
            get { return Defs.O[(int)Objects.char_script_table]; }
            set { Defs.O[(int)Objects.char_script_table] = value; }
        }

        /* Alist of scripts vs representative characters.  */
        public static LispObject script_representative_chars
        {
            get { return Defs.O[(int)Objects.script_representative_chars]; }
            set { Defs.O[(int)Objects.script_representative_chars] = value; }
        }
        
        public static LispObject unicode_category_table
        {
            get { return Defs.O[(int)Objects.unicode_category_table]; }
            set { Defs.O[(int)Objects.unicode_category_table] = value; }
        }
    }
}
