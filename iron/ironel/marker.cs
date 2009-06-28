namespace IronElisp
{
    public partial class L
    {
        /* Set the position of MARKER, specifying both the
           character position and the corresponding byte position.  */
        public static LispObject set_marker_both (LispObject marker, LispObject buffer, int charpos, int bytepos)
        {
            Buffer b;
            LispMarker m;

            CHECK_MARKER (marker);
            m = XMARKER (marker);

            if (NILP (buffer))
                b = current_buffer;
            else
            {
                CHECK_BUFFER (buffer);
                b = XBUFFER (buffer);
                /* If buffer is dead, set marker to point nowhere.  */
                if (EQ (b.name, Q.nil))
                {
                    unchain_marker (m);
                    return marker;
                }
            }

            /* In a single-byte buffer, the two positions must be equal.  */
            if (BUF_Z (b) == BUF_Z_BYTE (b)
                && charpos != bytepos)
                abort ();
            /* Every character is at least one byte.  */
            if (charpos > bytepos)
                abort ();

            m.bytepos = bytepos;
            m.charpos = charpos;

            if (m.buffer != b)
            {
                unchain_marker (m);
                m.buffer = b;
                m.next = b.BUF_MARKERS;
                b.BUF_MARKERS = m;
            }

            return marker;
        }

        /* Remove MARKER from the chain of whatever buffer it is in.
           Leave it "in no buffer".

           This is called during garbage collection,
           so we must be careful to ignore and preserve mark bits,
           including those in chain fields of markers.  */
        public static void unchain_marker (LispMarker marker)
        {
            LispMarker tail, prev, next;
            Buffer b;

            b = marker.buffer;
            if (b == null)
                return;

            if (EQ (b.name, Q.nil))
                abort ();

            marker.buffer = null;

            tail = b.BUF_MARKERS;
            prev = null;
            while (tail != null)
            {
                next = tail.next;

                if (marker == tail)
                {
                    if (prev == null)
                    {
                        b.BUF_MARKERS = next;
                        /* Deleting first marker from the buffer's chain.  Crash
                           if new first marker in chain does not say it belongs
                           to the same buffer, or at least that they have the same
                           base buffer.  */
                        if (next != null && b.text != next.buffer.text)
                            abort ();
                    }
                    else
                        prev.next = next;
                    /* We have removed the marker from the chain;
                       no need to scan the rest of the chain.  */
                    return;
                }
                else
                    prev = tail;
                tail = next;
            }

            /* Marker was not in its chain.  */
            abort ();
        }
        
        /* Return the char position of marker MARKER, as a C integer.  */
        public static int marker_position (LispObject marker)
        {
            LispMarker m = XMARKER(marker);
            Buffer buf = m.buffer;

            if (buf == null)
                error("Marker does not point anywhere");

            return m.charpos;
        }

        /* Return the byte position of marker MARKER, as a C integer.  */
        public static int marker_byte_position(LispObject marker)
        {
            LispMarker m = XMARKER(marker);
            Buffer buf = m.buffer;
            int i = m.bytepos;

            if (buf == null)
                error("Marker does not point anywhere");

            if (i < BUF_BEG_BYTE(buf) || i > BUF_Z_BYTE(buf))
                abort();

            return i;
        }
    }
}