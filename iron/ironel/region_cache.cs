namespace IronElisp
{
    public class boundary
    {
        public int pos;
        public int val;
    }

    public class region_cache
    {
        /* A sorted array of locations where the known-ness of the buffer
           changes.  */
        System.Collections.Generic.List<boundary> boundaries;

        /* boundaries[gap_start ... gap_start + gap_len - 1] is the gap.  */
        int gap_start, gap_len;

        /* The number of elements allocated to boundaries, not including the
           gap.  */
        int cache_len;

        /* The areas that haven't changed since the last time we cleaned out
           invalid entries from the cache.  These overlap when the buffer is
           entirely unchanged.  */
        int beg_unchanged, end_unchanged;

        /* The first and last positions in the buffer.  Because boundaries
           store their positions relative to the start (BEG) and end (Z) of
           the buffer, knowing these positions allows us to accurately
           interpret positions without having to pass the buffer structure
           or its endpoints around all the time.

           Yes, buffer_beg is always 1.  It's there for symmetry with
           buffer_end and the BEG and BUF_BEG macros.  */
        int buffer_beg, buffer_end;
    }
}