// here we have all the macros from lisp.h
// (until we get rid of them of course)
namespace IronElisp
{
    public partial class L
    {
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
            return XVECTOR(x).contents[idx];
        }

        public static int ASIZE(LispObject ARRAY)
        {
            return XVECTOR(ARRAY).Size;
        }

        public static void ASET(LispObject x, int idx, LispObject val)
        {
            XVECTOR(x).contents[idx] = val;
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
            return x == y;
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

        public static char SREF(LispObject str, int index)
        {
            return SDATA(str)[index];
        }

        public static string SDATA(LispObject x)
        {
            return XSTRING(x).SData;
        }

        public static int SCHARS(LispObject str)
        {
            return XSTRING(str).Size;
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

        public static void CHECK_BUFFER(LispObject x)
        {
            CHECK_TYPE(BUFFERP(x), Q.bufferp, x);
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

        public static int XINT(LispObject x)
        {
            return (x as LispInt).val;
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
    }
}