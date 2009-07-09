namespace IronElisp
{
    public partial class L
    {
        /* Set nonzero after Emacs has started up the first time.
          Prevents reinitialization of the Lisp world and keymaps
          on subsequent starts.  */
        public static bool initialized;

        // Nonzero means running Emacs without interactive terminal.
        public static bool noninteractive;
    }
}