namespace IronElisp
{
    public partial class V
    {
        /* Display table to use for vectors that don't specify their own.  */
        public static LispObject standard_display_table
        {
            get { return Defs.O[(int)Objects.standard_display_table]; }
            set { Defs.O[(int)Objects.standard_display_table] = value; }
        }
    }

    public partial class L
    {
        public static LispObject selected_frame;

        /* Nonzero upon entry to redisplay means do not assume anything about
           current contents of actual terminal frame; clear and redraw it.  */
        public static int frame_garbaged;

        /* Like bcopy except never gets confused by overlap.  Let this be the
           first function defined in this file, or change emacs.c where the
           address of this function is used.  */
        public static void safe_bcopy (byte[] from_ary, int from, byte[] to_ary, int to, int size)
        {
            System.Array.Copy(from_ary, from, to_ary, to, size);
        }

        /* Re-allocate/ re-compute glyph matrices on frame F.  If F is null,
           do it for all frames; otherwise do it just for the given frame.
           This function must be called when a new frame is created, its size
           changes, or its window configuration changes.  */
        public static void adjust_glyphs(Frame f)
        {
            // COMEBACK WHEN READY
            //if (f != null)
            //  adjust_frame_glyphs (f);
            //else
            //  {
            //    LispObject tail, lisp_frame;

            //    FOR_EACH_FRAME (tail, lisp_frame)
            //  adjust_frame_glyphs (XFRAME (lisp_frame));
            //  }
        }

        /* Clear MATRIX.

           This empties all rows in MATRIX by setting the enabled_p flag for
           all rows of the matrix to zero.  The function prepare_desired_row
           will eventually really clear a row when it sees one with a zero
           enabled_p flag.

           Resets update hints to defaults value.  The only update hint
           currently present is the flag MATRIX->no_scrolling_p.  */
        public static void clear_glyph_matrix (glyph_matrix matrix)
        {
            if (matrix != null)
            {
                enable_glyph_matrix_rows(matrix, 0, matrix.nrows, 0);
                matrix.no_scrolling_p = false;
            }
        }

        /* Enable a range of rows in glyph matrix MATRIX.  START and END are
           the row indices of the first and last + 1 row to enable.  If
           ENABLED_P is non-zero, enabled_p flags in rows will be set to 1.  */
        public static void enable_glyph_matrix_rows (glyph_matrix matrix, int start, int end, int enabled_p)
        {
            // xassert(start <= end);
            // xassert(start >= 0 && start < matrix->nrows);
            // xassert(end >= 0 && end <= matrix->nrows);

            for (; start < end; ++start)
                matrix.rows[start].enabled_p = enabled_p != 0;
        }
    }
}