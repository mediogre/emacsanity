namespace IronElisp
{
    public partial class L
    {
        /* Extract a doc string from a file.  FILEPOS says where to get it.
           If it is an integer, use that position in the standard DOC-... file.
           If it is (FILE . INTEGER), use FILE as the file name
           and INTEGER as the position in that file.
           But if INTEGER is negative, make it positive.
           (A negative integer is used for user variables, so we can distinguish
           them without actually fetching the doc string.)

           If the location does not point to the beginning of a docstring
           (e.g. because the file has been modified and the location is stale),
           return nil.

           If UNIBYTE is nonzero, always make a unibyte string.
        
           If DEFINITION is nonzero, assume this is for reading
           a dynamic function definition; convert the bytestring
           and the constants vector with appropriate byte handling,
           and return a cons cell.  */
        public static LispObject get_doc_string(LispObject filepos, int unibyte, int definition)
        {
            // COMEBACK_WHEN_READY!!! (strings and buffers)
            return Q.nil;
        }

        public static LispObject read_doc_string(LispObject filepos)
        {
            return get_doc_string(filepos, 0, 1);
        }
    }
}