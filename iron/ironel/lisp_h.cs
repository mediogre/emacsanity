// here we have all the macros from lisp.h
// (until we get rid of them of course)
namespace IronElisp
{
    public partial class L
    {
        /* Flag bits in a character.  These also get used in termhooks.h.
           Richard Stallman <rms@gnu.ai.mit.edu> thinks that MULE
           (MUlti-Lingual Emacs) might need 22 bits for the character value
           itself, so we probably shouldn't use any bits lower than 0x0400000.  */
        public const uint CHAR_ALT = 0x0400000;
        public const uint CHAR_SUPER = 0x0800000;
        public const uint CHAR_HYPER = 0x1000000;
        public const uint CHAR_SHIFT = 0x2000000;
        public const uint CHAR_CTL = 0x4000000;
        public const uint CHAR_META = 0x8000000;

        public const uint CHAR_MODIFIER_MASK = (CHAR_ALT | CHAR_SUPER | CHAR_HYPER | CHAR_SHIFT | CHAR_CTL | CHAR_META);

        public static System.Type XTYPE(LispObject x)
        {
            return x.GetType();
        }

        public static void XSETCAR(LispObject c, LispObject n)
        {
            XCONS(c).Car = n;
        }

        public static void XSETCDR(LispObject c, LispObject n)
        {
            XCONS(c).Cdr = n;
        }

        public static LispKboardObjFwd XKBOARD_OBJFWD(LispObject x)
        {
            return x as LispKboardObjFwd;
        }

        public static LispIntFwd XINTFWD(LispObject x)
        {
            return x as LispIntFwd;
        }

        public static LispBoolFwd XBOOLFWD(LispObject x)
        {
            return x as LispBoolFwd;
        }

        public static LispObjFwd XOBJFWD(LispObject x)
        {
            return x as LispObjFwd;
        }

        public static LispBufferObjFwd XBUFFER_OBJFWD(LispObject x)
        {
            return x as LispBufferObjFwd;
        }

        public static LispBufferLocalValue XBUFFER_LOCAL_VALUE(LispObject x)
        {
            return x as LispBufferLocalValue;
        }

        public static LispObject XSETINT(int x)
        {
            return make_number(x);
        }

        public static bool BUFFER_OBJFWDP(LispObject x)
        {
            return x is LispBufferObjFwd;
        }

        public static bool BUFFER_LOCAL_VALUEP(LispObject x)
        {
            return x is LispBufferLocalValue;
        }

        public static bool CHAR_TABLE_P(LispObject x)
        {
            return x is LispCharTable;
        }

        public static bool BOOL_VECTOR_P(LispObject x)
        {
            return x is LispBoolVector;
        }

        public static LispBoolVector XBOOL_VECTOR(LispObject x)
        {
            return x as LispBoolVector;
        }

        public static bool VECTORP(LispObject x)
        {
            return x is LispVector;
        }

        public static LispVector XVECTOR(LispObject x)
        {
            return x as LispVector;
        }

        public static LispObject AREF(LispObject x, int idx)
        {
            return (x as LispVectorLike<LispObject>)[idx];
        }

        public static int ASIZE(LispObject ARRAY)
        {
            return ((LispVectorLike<LispObject>)ARRAY).Size;
        }

        public static void ASET(LispObject x, int idx, LispObject val)
        {
            ((LispVectorLike<LispObject>) x)[idx] = val;
        }

        public static bool COMPILEDP(LispObject x)
        {
            return x is LispCompiled;
        }

        public static bool SUBRP(LispObject x)
        {
            return x is LispSubr;
        }

        public static LispSubr XSUBR(LispObject x)
        {
            return x as LispSubr;
        }

        public static bool CONSP(LispObject x)
        {
            return x is LispCons;
        }

        public static bool EQ(LispObject x, LispObject y)
        {
            if (x is LispInt && y is LispInt)
                return ((LispInt)x).val == ((LispInt)y).val;

            return LispObject.ReferenceEquals(x, y);
        }

        public static LispCons XCONS(LispObject x)
        {
            return x as LispCons;
        }

        public static LispObject XCAR(LispObject x)
        {
            return XCONS(x).Car;
        }

        public static LispObject XCDR(LispObject x)
        {
            return XCONS(x).Cdr;
        }

        public static bool NILP(LispObject x)
        {
            return x == Q.nil;
        }

        public static bool FRAMEP(LispObject x)
        {
            return x is Frame;
        }

        public static Frame XFRAME(LispObject x)
        {
            return x as Frame;
        }

        public static bool BUFFERP(LispObject x)
        {
            return x is Buffer;
        }

        public static Buffer XBUFFER(LispObject x)
        {
            return x as Buffer;
        }

        public static bool MISCP(LispObject x)
        {
            return x is LispMisc;
        }

        public static bool SYMBOLP(LispObject x)
        {
            return x is LispSymbol;
        }

        public static LispFloat XFLOAT(LispObject a)
        {
            return a as LispFloat;
        }

        public static double XFLOATINT(LispObject n)
        {
            return extract_float(n);
        }

        public static LispSymbol XSYMBOL(LispObject x)
        {
            return x as LispSymbol;
        }

        public static bool STRINGP(LispObject x)
        {
            return x is LispString;
        }

        public static LispString XSTRING(LispObject x)
        {
            return x as LispString;
        }

        public static byte[] SDATA(LispObject x)
        {
            return XSTRING(x).SData;
        }

        public static byte SREF(LispObject str, int index)
        {
            return SDATA(str)[index];
        }

        public static byte SSET(LispObject str, int index, byte newval)
        {
            return (SDATA(str)[index] = newval);
        }

        public static int SCHARS(LispObject str)
        {
            return XSTRING(str).Size;
        }

        /* Set text properties.  */
        public static void STRING_SET_INTERVALS(LispObject STR, Interval INT)
        {
            XSTRING(STR).intervals = INT;
        }

        public static void STRING_SET_CHARS(LispObject str, int newsize)
        {
            XSTRING(str).Size = newsize;
        }

        public static int SBYTES(LispObject str)
        {
            return STRING_BYTES(XSTRING(str));
        }

        public static int STRING_BYTES(LispString str)
        {
            return str.SizeBytes;
        }

        /* Nonzero if STR is a multibyte string.  */
        public static bool STRING_MULTIBYTE(LispObject STR)
        {
            return (XSTRING(STR).SizeBytes >= 0);
        }

        /* Mark STR as a unibyte string.  */
        public static void STRING_SET_UNIBYTE(ref LispObject STR)
        {
            if (EQ(STR, empty_multibyte_string))
                STR = empty_unibyte_string;
            else
                XSTRING(STR).SizeBytes = -1;
        }

        /* Mark STR as a multibyte string.  Assure that STR contains only
           ASCII characters in advance.  */
        public static void STRING_SET_MULTIBYTE(ref LispObject STR)
        {
            if (EQ(STR, empty_unibyte_string))
                STR = empty_multibyte_string;
            else
                XSTRING(STR).SizeBytes = XSTRING(STR).Size;
        }

        /* Value is the value of SYM, with defvaralias taken into
           account.  */
        public static LispObject SYMBOL_VALUE(LispObject sym)
        {
            return XSYMBOL(sym).IsIndirectVariable ? indirect_variable(XSYMBOL(sym)).Value : XSYMBOL(sym).Value;
        }


        /* Set SYM's value to VAL, taking defvaralias into account.  */
        public static void SET_SYMBOL_VALUE(LispObject sym, LispObject val)
        {
            if (XSYMBOL(sym).IsIndirectVariable)
                indirect_variable(XSYMBOL(sym)).Value = val;
            else
                XSYMBOL(sym).Value = val;
        }

        public static LispObject CAR_SAFE(LispObject c)
        {
            return (CONSP(c) ? XCAR(c) : Q.nil);
        }

        /* Take the car or cdr of something whose type is not known.  */
        public static LispObject CAR(LispObject c)
        {
            if (CONSP(c))
                return XCAR(c);
            else if (NILP(c))
                return Q.nil;
            else
                return wrong_type_argument(Q.listp, c);
        }

        public static LispObject CDR(LispObject c)
        {
            if (CONSP(c))
                return XCDR(c);
            else if (NILP(c))
                return Q.nil;
            else
                return wrong_type_argument(Q.listp, c);
        }

        public static void abort()
        {
            System.Environment.Exit(0);
        }

        /* Check quit-flag and quit if it is non-nil.
           Typing C-g does not directly cause a quit; it only sets Vquit_flag.
           So the program needs to do QUIT at times when it is safe to quit.
           Every loop that might run for a long time or might not exit
           ought to do QUIT at least once, at a safe place.
           Unless that is impossible, of course.
           But it is very desirable to avoid creating loops where QUIT is impossible.

           Exception: if you set immediate_quit to nonzero,
           then the handler that responds to the C-g does the quit itself.
           This is a good thing to do around a loop that has no side effects
           and (in particular) cannot call arbitrary Lisp code.  */
        public static void QUIT()
        {
            if (!NILP(V.quit_flag) && NILP(V.inhibit_quit))
            {
                LispObject flag = V.quit_flag;
                V.quit_flag = Q.nil;
                if (EQ(V.throw_on_input, flag))
                    F.lisp_throw(V.throw_on_input, Q.t);
                F.signal(Q.quit, Q.nil);
            }
        }

        public static bool CHECK_TYPE(bool ok, LispObject xxxp, LispObject x)
        {
            if (! ok)
                wrong_type_argument(xxxp, x);

            return true;
        }

        public static void CHECK_LIST(LispObject x)
        {
            CHECK_TYPE(CONSP(x) || NILP(x), Q.listp, x);
        }

        public static void CHECK_SYMBOL(LispObject x)
        {
            CHECK_TYPE(SYMBOLP(x), Q.symbolp, x);
        }

        public static void CHECK_CONS(LispObject x)
        {
            CHECK_TYPE(CONSP(x), Q.consp, x);
        }

        public static void CHECK_NUMBER(LispObject x)
        {
            CHECK_TYPE(INTEGERP(x), Q.integerp, x);
        }

        public static void CHECK_NUMBER_OR_FLOAT(LispObject x)
        {
            CHECK_TYPE(FLOATP(x) || INTEGERP(x), Q.numberp, x);
        }

        public static double XFLOAT_DATA(LispObject f)
        {
            return XFLOAT(f).val;
        }

        public static void CHECK_NATNUM(LispObject x)
        {
            CHECK_TYPE (NATNUMP (x), Q.wholenump, x);
        }

        public static void CHECK_BUFFER(LispObject x)
        {
            CHECK_TYPE(BUFFERP(x), Q.bufferp, x);
        }

        public static void CHECK_LIST_CONS(LispObject x, LispObject y)
        {
            CHECK_TYPE(CONSP(x), Q.listp, y);
        }
        public static void CHECK_LIST_END(LispObject x, LispObject y)
        {
            CHECK_TYPE(NILP(x), Q.listp, y);
        }

        public static void CHECK_STRING(LispObject x)
        {
            CHECK_TYPE(STRINGP(x), Q.stringp, x);
        }

        public static bool SYMBOL_CONSTANT_P(LispObject sym)
        {
            return XSYMBOL(sym).Constant;
        }

        public static bool INTEGERP(LispObject x)
        {
            return x is LispInt;
        }

        public static bool FLOATP(LispObject x)
        {
            return x is LispFloat;
        }

        public static bool NATNUMP(LispObject x)
        {
            return (INTEGERP (x) && XINT (x) >= 0);
        }
        
        public static int XINT(LispObject x)
        {
            return (x as LispInt).val;
        }

        public static uint XUINT (LispObject x)
        {
            return (uint) XINT (x); 
        }

        public static bool SYMBOL_INTERNED_IN_INITIAL_OBARRAY_P(LispObject sym)
        {
            return XSYMBOL(sym).Interned == LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
        }

        /* Value is name of symbol.  */
        public static LispObject SYMBOL_NAME(LispObject sym)
        {
            return XSYMBOL(sym).xname;
        }

        public static bool INTFWDP(LispObject x)
        {
            return x is LispIntFwd;
        }
        public static bool BOOLFWDP(LispObject x)
        {
            return x is LispBoolFwd;
        }

        public static bool OBJFWDP(LispObject x)
        {
            return x is LispObjFwd;
        }

        public static LispMarker XMARKER(LispObject x)
        {
            return x as LispMarker;
        }

        public static bool MARKERP(LispObject x)
        {
            return x is LispMarker;
        }

        public static void CHECK_MARKER(LispObject x)
        {
            CHECK_TYPE(MARKERP(x), Q.markerp, x);
        }

        public static bool OVERLAYP(LispObject x)
        {
            return x is LispOverlay;
        }

        public static LispOverlay XOVERLAY(LispObject x)
        {
            return x as LispOverlay;
        }

        /* Get text properties.  */
        public static Interval STRING_INTERVALS(LispObject STR)
        {
            return XSTRING(STR).intervals;
        }

        public static void LOADHIST_ATTACH(LispObject x)
        {
            if (initialized)
                V.current_load_list = F.cons(x, V.current_load_list);
        }

        public static LispHashTable XHASH_TABLE(LispObject x)
        {
            return x as LispHashTable;
        }

        /* Value is the key part of entry IDX in hash table H.  */
        public static LispObject HASH_KEY(LispHashTable H, int IDX)
        {
            return AREF(H.key_and_value, 2 * (IDX));
        }

        /* Value is the value part of entry IDX in hash table H.  */
        public static LispObject HASH_VALUE(LispHashTable H, int IDX)
        {
            return AREF(H.key_and_value, 2 * (IDX) + 1);
        }

        /* Value is the index of the next entry following the one at IDX
           in hash table H.  */
        public static LispObject HASH_NEXT(LispHashTable H, int IDX)
        {
            return AREF(H.next, IDX);
        }

        /* Value is the hash code computed for entry IDX in hash table H.  */
        public static LispObject HASH_HASH(LispHashTable H, int IDX)
        {
            return AREF(H.hash, IDX);
        }

        /* Value is the index of the element in hash table H that is the
           start of the collision list at index IDX in the index vector of H.  */
        public static LispObject HASH_INDEX(LispHashTable H, int IDX)
        {
            return AREF(H.index, IDX);
        }

        /* Value is the size of hash table H.  */
        public static int HASH_TABLE_SIZE(LispHashTable H)
        {
            return XVECTOR(H.next).Size;
        }

        /* Almost equivalent to Faref (CT, IDX) with optimization for ASCII
           characters.  Do not check validity of CT.  */
        public static LispObject CHAR_TABLE_REF(LispObject CT, uint IDX)
        {
#if COMEBACK_LATER
  ((ASCII_CHAR_P (IDX)							 \
    && SUB_CHAR_TABLE_P (XCHAR_TABLE (CT)->ascii)			 \
    && !NILP (XSUB_CHAR_TABLE (XCHAR_TABLE (CT)->ascii)->contents[IDX])) \
   ? XSUB_CHAR_TABLE (XCHAR_TABLE (CT)->ascii)->contents[IDX]		 \
   : char_table_ref ((CT), (IDX)))
#endif
            throw new System.Exception("Come back with hashes");
        }
    }
}