namespace IronElisp
{
    public partial class L
    {
        /* Generate output from a format-spec FORMAT,
   terminated at position FORMAT_END.
   Output goes in BUFFER, which has room for BUFSIZE chars.
   If the output does not fit, truncate it to fit.
   Returns the number of bytes stored into BUFFER.
   ARGS points to the vector of arguments, and NARGS says how many.
   A double counts as two arguments.
   String arguments are passed as C strings.
   Integers are passed as C integers.  */

        public static void doprnt(System.Text.StringBuilder buffer, string format, params object[] args)
        {
            // COMEBACK_WHEN_READY
        }
    }
}