namespace IronElisp
{
    public partial class L
    {
        /* Extract a Lisp number as a `double', or signal an error.  */
        public static double extract_float(LispObject num)
        {
            CHECK_NUMBER_OR_FLOAT(num);

            if (FLOATP(num))
                return XFLOAT_DATA(num);
            return (double)XINT(num);
        }

        public static LispObject fmod_float(LispObject x, LispObject y)
        {
            double f1, f2;

            f1 = FLOATP(x) ? XFLOAT_DATA(x) : XINT(x);
            f2 = FLOATP(y) ? XFLOAT_DATA(y) : XINT(y);


            /* If the "remainder" comes out with the wrong sign, fix it.  */
            f1 = f1 % f2;
            f1 = (f2 < 0 ? f1 > 0 : f1 < 0) ? f1 + f2 : f1;

            return make_float(f1);
        }
    }
}