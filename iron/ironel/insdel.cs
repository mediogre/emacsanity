namespace IronElisp
{
    public partial class L
    {
        /* Copy NBYTES bytes of text from FROM_ADDR to TO_ADDR.
           FROM_MULTIBYTE says whether the incoming text is multibyte.
           TO_MULTIBYTE says whether to store the text as multibyte.
           If FROM_MULTIBYTE != TO_MULTIBYTE, we convert.

           Return the number of bytes stored at TO_ADDR.  */
        public static int copy_text (byte[] from_addr, byte[] to_addr, 
                                     int to_index,
	                                 int nbytes, bool from_multibyte, bool to_multibyte)
        {
            if (from_multibyte == to_multibyte)
            {
                System.Array.Copy(from_addr, 0, to_addr, to_index, nbytes);
                return nbytes;
            }
            else if (from_multibyte)
            {
                int nchars = 0;
                int bytes_left = nbytes;
                LispObject tbl = Q.nil;

                while (bytes_left > 0)
                {
                    int from_index = 0;

                    int thislen = 0;
                    uint c = STRING_CHAR_AND_LENGTH (from_addr, from_index, bytes_left, ref thislen);
                    if (! ASCII_CHAR_P (c))
                        c &= 0xFF;
                    to_addr[to_index++] = (byte) c;
                    from_index += thislen;
                    bytes_left -= thislen;
                    nchars++;
                }
                return nchars;
            }
            else
            {
                int initial_to_addr = to_index;
                int from_index = 0;

                /* Convert single-byte to multibyte.  */
                while (nbytes > 0)
                {
                    uint c = from_addr[from_index++];

                    if (c >= 0200)
                    {
                        c = unibyte_char_to_multibyte (c);
                        to_index += CHAR_STRING (c, to_addr, to_index);
                        nbytes--;
                    }
                    else
                    {
                        /* Special case for speed.  */
                        to_addr[to_index++] = (byte) c;
                        nbytes--;
                    }
                }
                return to_index - initial_to_addr;
            }
        }

        /* Return the number of bytes it would take
           to convert some single-byte text to multibyte.
           The single-byte text consists of NBYTES bytes at PTR.  */
        public static int count_size_as_multibyte(byte[] ptr, int nbytes)
        {
            int i;
            int outgoing_nbytes = 0;

            int p = 0;
            for (i = 0; i < nbytes; i++)
            {
                uint c = ptr[p++];

                if (c < 0200)
                    outgoing_nbytes++;
                else
                {
                    c = unibyte_char_to_multibyte(c);
                    outgoing_nbytes += CHAR_BYTES(c);
                }
            }

            return outgoing_nbytes;
        }
    }
}