namespace IronElisp
{
    public partial class Q
    {
        /* Coding system emacs-mule and raw-text are for converting only
           end-of-line format.  */
        public static LispObject emacs_mule;
        public static LispObject raw_text;
        public static LispObject utf_8_emacs;

        /* If a symbol has this property, evaluate the value to define the
           symbol as a coding system.  */
        public static LispObject coding_system_define_form;
    }

    public partial class V
    {
        public static LispObject coding_system_hash_table;
    }

    public partial class L
    {
        public static byte[] emacs_mule_bytes = new byte[256];

        /* Encode the file name NAME using the specified coding system
           for file names, if any.  */
        public static LispObject ENCODE_FILE(LispObject name)						   
        {
            return (!NILP(V.file_name_coding_system)
             && !EQ(V.file_name_coding_system, make_number(0))
             ? code_convert_string_norecord(name, V.file_name_coding_system, true)
             : (!NILP(V.default_file_name_coding_system)
                && !EQ(V.default_file_name_coding_system, make_number(0))
                ? code_convert_string_norecord(name, V.default_file_name_coding_system, true)
                : name));
        }

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

        /* Encode or decode STRING according to CODING_SYSTEM.
           Do not set Vlast_coding_system_used.

           This function is called only from macros DECODE_FILE and
           ENCODE_FILE, thus we ignore character composition.  */
        public static LispObject code_convert_string_norecord(LispObject stringg, LispObject coding_system, bool encodep)
        {
            return code_convert_string(stringg, coding_system, Q.t, encodep, false, true);
        }

        /* Return the ID of CODING_SYSTEM_SYMBOL.  */
        public static int CODING_SYSTEM_ID(LispObject coding_system_symbol)
        {
            return hash_lookup(XHASH_TABLE(V.coding_system_hash_table), coding_system_symbol);
        }

/* Return 1 if CODING_SYSTEM_SYMBOL is a coding system.  */
        public static bool CODING_SYSTEM_P(LispObject coding_system_symbol)
        {
            return (CODING_SYSTEM_ID(coding_system_symbol) >= 0 || (!NILP(coding_system_symbol) && !NILP(F.coding_system_p(coding_system_symbol))));
        }

    }

    public partial class F
    {
        public static LispObject decode_coding_string(LispObject str, LispObject coding_system, LispObject nocopy, LispObject buffer)
        {
            return L.code_convert_string(str, coding_system, buffer,
                                         false, !L.NILP(nocopy), false);
        }

        public static LispObject coding_system_p(LispObject obj)
        {
            if (L.NILP(obj) || L.CODING_SYSTEM_ID(obj) >= 0)
                return Q.t;
            if (!L.SYMBOLP(obj)
                || L.NILP(F.get(obj, Q.coding_system_define_form)))
                return Q.nil;
            return Q.t;
        }
    }
}