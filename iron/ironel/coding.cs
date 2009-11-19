namespace IronElisp
{
    public partial class Q
    {
        /* Coding system emacs-mule and raw-text are for converting only
           end-of-line format.  */
        public static LispObject emacs_mule;
        public static LispObject raw_text;
        public static LispObject utf_8_emacs;
    }

    public partial class L
    {
        public static byte[] emacs_mule_bytes = new byte[256];

        public static LispObject code_convert_string (LispObject str, LispObject coding_system, LispObject dst_object,
                                                      bool encodep, bool nocopy, bool norecord)
        {
            throw new System.Exception();
#if COMEBACK_LATER
            struct coding_system coding;
            int chars, bytes;

            CHECK_STRING (str);
            if (NILP (coding_system))
            {
                if (! norecord)
                    Vlast_coding_system_used = Qno_conversion;
                if (NILP (dst_object))
                    return (nocopy ? Fcopy_sequence (str) : str);
            }

            if (NILP (coding_system))
                coding_system = Qno_conversion;
            else
                CHECK_CODING_SYSTEM (coding_system);
            if (NILP (dst_object))
                dst_object = Qt;
            else if (! EQ (dst_object, Qt))
                CHECK_BUFFER (dst_object);

            setup_coding_system (coding_system, &coding);
            coding.mode |= CODING_MODE_LAST_BLOCK;
            chars = SCHARS (str);
            bytes = SBYTES (str);
            if (encodep)
                encode_coding_object (&coding, str, 0, 0, chars, bytes, dst_object);
            else
                decode_coding_object (&coding, str, 0, 0, chars, bytes, dst_object);
            if (! norecord)
                Vlast_coding_system_used = CODING_ID_NAME (coding.id);

            return (BUFFERP (dst_object)
                    ? make_number (coding.produced_char)
                    : coding.dst_object);
#endif
        }
    }

    public partial class F
    {
        public static LispObject decode_coding_string(LispObject str, LispObject coding_system, LispObject nocopy, LispObject buffer)
        {
            return L.code_convert_string(str, coding_system, buffer,
                                         false, !L.NILP(nocopy), false);
        }
    }
}