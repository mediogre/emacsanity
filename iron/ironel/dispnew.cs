namespace IronElisp
{
    public partial class L
    {
        public static LispObject selected_frame;

        /* Like bcopy except never gets confused by overlap.  Let this be the
           first function defined in this file, or change emacs.c where the
           address of this function is used.  */
        public static void safe_bcopy (byte[] from_ary, int from, byte[] to_ary, int to, int size)
        {
            System.Array.Copy(from_ary, from, to_ary, to, size);
        }
    }
}