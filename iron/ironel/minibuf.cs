namespace IronElisp
{
    public partial class L
    {
        /* Depth in minibuffer invocations.  */
        public static int minibuf_level;
    }

    public partial class F
    {
        public static LispObject read_minibuffer(LispObject prompt, LispObject initial_contents)
        {
#if COMEBACK_LATER
  CHECK_STRING (prompt);
  return read_minibuf (Vminibuffer_local_map, initial_contents,
		       prompt, Qnil, 1, Qminibuffer_history,
		       make_number (0), Qnil, 0, 0);
#endif 
            throw new System.Exception("Comeback Already!");
        }
    }
}