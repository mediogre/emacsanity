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
    }
}