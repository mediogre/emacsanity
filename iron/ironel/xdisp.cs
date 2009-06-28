namespace IronElisp
{
    public partial class L
    {
        // Non-zero while redisplay_internal is in progress.
        public static bool redisplaying_p;

        /* Non-zero means we're allowed to display a hourglass pointer.  */
        public static bool display_hourglass_p;
    } 

    public partial class Q
    {
        // Non-nil means don't actually do any redisplay.
        public static LispObject inhibit_redisplay; 
    }

    public partial class V
    {
        // Non-nil means don't actually do any redisplay.
        public static LispObject inhibit_redisplay;
    }
}