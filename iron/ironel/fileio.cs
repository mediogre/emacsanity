namespace IronElisp
{
    public partial class F
    {
        public static LispObject file_name_nondirectory(LispObject filename)
        {
            throw new System.Exception();
#if COMEBACK_LATER
  register const unsigned char *beg, *p, *end;
  Lisp_Object handler;

  CHECK_STRING (filename);

  /* If the file name has special constructs in it,
     call the corresponding file handler.  */
  handler = Ffind_file_name_handler (filename, Qfile_name_nondirectory);
  if (!NILP (handler))
    return call2 (handler, Qfile_name_nondirectory, filename);

  beg = SDATA (filename);
  end = p = beg + SBYTES (filename);

  while (p != beg && !IS_DIRECTORY_SEP (p[-1])
ifdef DOS_NT
	 /* only recognise drive specifier at beginning */
	 && !(p[-1] == ':'
	      /* handle the "/:d:foo" case correctly  */
	      && (p == beg + 2 || (p == beg + 4 && IS_DIRECTORY_SEP (*beg))))
endiffff
	 )
    p--;

  return make_specified_string (p, -1, end - p, STRING_MULTIBYTE (filename));
#endif
        }
    }
}