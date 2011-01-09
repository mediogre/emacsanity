namespace IronElisp
{
    /* Values returned from coordinates_in_window.  */
    public enum window_part
    {
        ON_NOTHING,
        ON_TEXT,
        ON_MODE_LINE,
        ON_VERTICAL_BORDER,
        ON_HEADER_LINE,
        ON_LEFT_FRINGE,
        ON_RIGHT_FRINGE,
        ON_LEFT_MARGIN,
        ON_RIGHT_MARGIN,
        ON_SCROLL_BAR
    }

    /* Starting with Emacs 20.3, characters from strings and buffers have
       both a character and a byte position associated with them.  The
       following structure holds such a pair of positions.  */
    public struct text_pos
    {
        /* Character position.  */
        public int charpos;

        /* Corresponding byte position.  */
        public int bytepos;
    }

    /* When rendering glyphs, redisplay scans string or buffer text,
       overlay strings in that text, and does display table or control
       character translations.  The following structure captures a
       position taking all this into account.  */
    public struct display_pos
    {
        /* Buffer or string position.  */
        text_pos pos;
        
        /* If this is a position in an overlay string, overlay_string_index
           is the index of that overlay string in the sequence of overlay
           strings at `pos' in the order redisplay processes them.  A value
           < 0 means that this is not a position in an overlay string.  */
        int overlay_string_index;

        /* If this is a position in an overlay string, string_pos is the
           position within that string.  */
        text_pos string_pos;

        /* If the character at the position above is a control character or
           has a display table entry, dpvec_index is an index in the display
           table or control character translation of that character.  A
           value < 0 means this is not a position in such a translation.  */
        int dpvec_index;
    }

    /* IDs of important faces known by the C face code.  These are the IDs
       of the faces for CHARSET_ASCII.  */
    public enum face_id
    {
        DEFAULT_FACE_ID,
        MODE_LINE_FACE_ID,
        MODE_LINE_INACTIVE_FACE_ID,
        TOOL_BAR_FACE_ID,
        FRINGE_FACE_ID,
        HEADER_LINE_FACE_ID,
        SCROLL_BAR_FACE_ID,
        BORDER_FACE_ID,
        CURSOR_FACE_ID,
        MOUSE_FACE_ID,
        MENU_FACE_ID,
        VERTICAL_BORDER_FACE_ID,
        BASIC_FACE_ID_SENTINEL
    }

    /* Enumeration of glyph types.  Glyph structures contain a type field
       containing one of the enumerators defined here.  */
    public enum glyph_type
    {
        /* Glyph describes a character.  */
        CHAR_GLYPH,

        /* Glyph describes a static composition.  */
        COMPOSITE_GLYPH,

        /* Glyph describes an image.  */
        IMAGE_GLYPH,

        /* Glyph is a space of fractional width and/or height.  */
        STRETCH_GLYPH
    }

    /* Structure describing how to use partial glyphs (images slicing) */
    public struct glyph_slice
    {
        uint x;
        uint y;
        uint width;
        uint height;
    }

    public class glyph
    {
        /* Position from which this glyph was drawn.  If `object' below is a
           Lisp string, this is a position in that string.  If it is a
           buffer, this is a position in that buffer.  A value of -1
           together with a null object means glyph is a truncation glyph at
           the start of a row.  */
        int charpos;

        /* Lisp object source of this glyph.  Currently either a buffer or
           a string, if the glyph was produced from characters which came from
           a buffer or a string; or 0 if the glyph was inserted by redisplay
           for its own purposes such as padding.  */
        LispObject obj;

        /* Width in pixels.  */
        short pixel_width;

        /* Ascent and descent in pixels.  */
        short ascent, descent;

        /* Vertical offset.  If < 0, the glyph is displayed raised, if > 0
           the glyph is displayed lowered.  */
        short voffset;

        /* Which kind of glyph this is---character, image etc.  Value
           should be an enumerator of type enum glyph_type.  */
        ushort type;

        /* 1 means this glyph was produced from multibyte text.  Zero
           means it was produced from unibyte text, i.e. charsets aren't
           applicable, and encoding is not performed.  */
        bool multibyte_p;

        /* Non-zero means draw a box line at the left or right side of this
           glyph.  This is part of the implementation of the face attribute
           `:box'.  */
        bool left_box_line_p;
        bool right_box_line_p;

        /* Non-zero means this glyph's physical ascent or descent is greater
           than its logical ascent/descent, i.e. it may potentially overlap
           glyphs above or below it.  */
        bool overlaps_vertically_p;

        /* For terminal frames, 1 means glyph is a padding glyph.  Padding
           glyphs are us
      ed for characters whose visual shape consists of
           more than one glyph (e.g. Asian characters).  All but the first
           glyph of such a glyph sequence have the padding_p flag set.  This
           flag is used only to minimize code changes.  A better way would
           probably be to use the width field of glyphs to express padding.

           For graphic frames, 1 means the pixel width of the glyph in a
           font is 0, but 1-pixel is padded on displaying for correct cursor
           displaying.  The member `pixel_width' above is set to 1.  */
        bool padding_p;

        /* 1 means the actual glyph is not available, draw a box instead.
           This can happen when a font couldn't be loaded, or a character
           doesn't have a glyph in a font.  */
        bool glyph_not_available_p;


        /* Non-zero means don't display cursor here.  */
        bool avoid_cursor_p;

        /* Face of the glyph.  This is a realized face ID,
           an index in the face cache of the frame.  */
        uint face_id;

        /* Type of font used to display the character glyph.  May be used to
           determine which set of functions to use to obtain font metrics
           for the glyph.  On W32, value should be an enumerator of the type
           w32_char_font_type.  Otherwise it equals FONT_TYPE_UNKNOWN.  */
        uint font_type;

        glyph_slice slice = new glyph_slice();

#if COMEBACK_LATER
  /* A union of sub-structures for different glyph types.  */
  union
  {
    /* Character code for character glyphs (type == CHAR_GLYPH).  */
    unsigned ch;

    /* Sub-structures for type == COMPOSITION_GLYPH.  */
    struct
    {
      /* Flag to tell if the composition is automatic or not.  */
      unsigned automatic : 1;
      /* ID of the composition.  */
      unsigned id    : 23;
      /* Start and end indices of glyphs of the composition.  */
      unsigned from : 4;
      unsigned to : 4;
    } cmp;

    /* Image ID for image glyphs (type == IMAGE_GLYPH).  */
    unsigned img_id;

    /* Sub-structure for type == STRETCH_GLYPH.  */
    struct
    {
      /* The height of the glyph.  */
      unsigned height  : 16;

      /* The ascent of the glyph.  */
      unsigned ascent  : 16;
    }
    stretch;

    /* Used to compare all bit-fields above in one step.  */
    unsigned val;
  } u;
#endif
    }

    public class face
    {
        // COMEBACK_WHEN_READY !!!
    }

    public class face_cache
    {
        // COMEBACK_WHEN_READY !!!
    }

    public class glyph_matrix
    {
        /* The pool from which glyph memory is allocated, if any.  This is
           null for frame matrices and for window matrices managing their
           own storage.  */
        public glyph_pool pool;

        /* Vector of glyph row structures.  The row at nrows - 1 is reserved
           for the mode line.  */
        public glyph_row[] rows;

        /* Number of elements allocated for the vector rows above.  */
        public int rows_allocated;

        /* The number of rows used by the window if all lines were displayed
           with the smallest possible character height.  */
        public int nrows;

        /* Origin within the frame matrix if this is a window matrix on a
           frame having a frame matrix.  Both values are zero for
           window-based redisplay.  */
        public int matrix_x, matrix_y;

        /* Width and height of the matrix in columns and rows.  */
        public int matrix_w, matrix_h;

        /* If this structure describes a window matrix of window W,
           window_left_col is the value of W->left_col, window_top_line the
           value of W->top_line, window_height and window_width are width and
           height of W, as returned by window_box, and window_vscroll is the
           value of W->vscroll at the time the matrix was last adjusted.
           Only set for window-based redisplay.  */
        public int window_left_col, window_top_line;
        public int window_height, window_width;
        public int window_vscroll;

        /* Number of glyphs reserved for left and right marginal areas when
           the matrix was last adjusted.  */
        public int left_margin_glyphs, right_margin_glyphs;

        /* Flag indicating that scrolling should not be tried in
           update_window.  This flag is set by functions like try_window_id
           which do their own scrolling.  */
        public bool no_scrolling_p;

        /* Non-zero means window displayed in this matrix has a top mode
           line.  */
        public bool header_line_p;

        /* The buffer this matrix displays.  Set in
           mark_window_display_accurate_1.  */
        public Buffer buffer;

        /* Values of BEGV and ZV as of last redisplay.  Set in
           mark_window_display_accurate_1.  */
        public int begv, zv;
    }

    /* Area in window glyph matrix.  If values are added or removed, the
       function mark_object in alloc.c has to be changed.  */
    public enum glyph_row_area
    {
        LEFT_MARGIN_AREA,
        TEXT_AREA,
        RIGHT_MARGIN_AREA,
        LAST_AREA
    }

    public class XRectangle
    {
        public int x, y;
        public uint width, height;
    }

    /* Rows of glyphs in a windows or frame glyph matrix.

       Each row is partitioned into three areas.  The start and end of
       each area is recorded in a pointer as shown below.

       +--------------------+-------------+---------------------+
       |  left margin area  |  text area  |  right margin area  |
       +--------------------+-------------+---------------------+
       |                    |             |                     |
       glyphs[LEFT_MARGIN_AREA]           glyphs[RIGHT_MARGIN_AREA]
                |                                   |
                glyphs[TEXT_AREA]                   |
                                      glyphs[LAST_AREA]

       Rows in frame matrices reference glyph memory allocated in a frame
       glyph pool (see the description of struct glyph_pool).  Rows in
       window matrices on frames having frame matrices reference slices of
       the glyphs of corresponding rows in the frame matrix.

       Rows in window matrices on frames having no frame matrices point to
       glyphs allocated from the heap via xmalloc;
       glyphs[LEFT_MARGIN_AREA] is the start address of the allocated
       glyph structure array.  */
    public class glyph_row
    {
        /* Pointers to beginnings of areas.  The end of an area A is found at
           A + 1 in the vector.  The last element of the vector is the end
           of the whole row.

           Kludge alert: Even if used[TEXT_AREA] == 0, glyphs[TEXT_AREA][0]'s
           position field is used.  It is -1 if this row does not correspond
           to any text; it is some buffer position if the row corresponds to
           an empty display line that displays a line end.  This is what old
           redisplay used to do.  (Except in code for terminal frames, this
           kludge is no longer used, I believe. --gerd).

           See also start, end, displays_text_p and ends_at_zv_p for cleaner
           ways to do it.  The special meaning of positions 0 and -1 will be
           removed some day, so don't use it in new code.  */
        glyph[][] glyphs = { null, null, null, null };

        /* Number of glyphs actually filled in areas.  */
        short[] used = new short[(int)glyph_row_area.LAST_AREA];

        /* Window-relative x and y-position of the top-left corner of this
           row.  If y < 0, this means that eabs (y) pixels of the row are
           invisible because it is partially visible at the top of a window.
           If x < 0, this means that eabs (x) pixels of the first glyph of
           the text area of the row are invisible because the glyph is
           partially visible.  */
        int x, y;

        /* Width of the row in pixels without taking face extension at the
           end of the row into account, and without counting truncation
           and continuation glyphs at the end of a row on ttys.  */
        int pixel_width;

        /* Logical ascent/height of this line.  The value of ascent is zero
           and height is 1 on terminal frames.  */
        public int ascent, height;

        /* Physical ascent/height of this line.  If max_ascent > ascent,
           this line overlaps the line above it on the display.  Otherwise,
           if max_height > height, this line overlaps the line beneath it.  */
        int phys_ascent, phys_height;

        /* Portion of row that is visible.  Partially visible rows may be
           found at the top and bottom of a window.  This is 1 for tty
           frames.  It may be < 0 in case of completely invisible rows.  */
        int visible_height;

        /* Extra line spacing added after this row.  Do not consider this
           in last row when checking if row is fully visible.  */
        int extra_line_spacing;

        /* Hash code.  This hash code is available as soon as the row
           is constructed, i.e. after a call to display_line.  */
        uint hash;

        /* First position in this row.  This is the text position, including
           overlay position information etc, where the display of this row
           started, and can thus be less the position of the first glyph
           (e.g. due to invisible text or horizontal scrolling).  */
        display_pos start = new display_pos();

        /* Text position at the end of this row.  This is the position after
           the last glyph on this row.  It can be greater than the last
           glyph position + 1, due to truncation, invisible text etc.  In an
           up-to-date display, this should always be equal to the start
           position of the next row.  */
        display_pos end = new display_pos();

        /* Non-zero means the overlay arrow bitmap is on this line.
           -1 means use default overlay arrow bitmap, else
           it specifies actual fringe bitmap number.  */
        int overlay_arrow_bitmap;

        /* Left fringe bitmap number (enum fringe_bitmap_type).  */
        uint left_user_fringe_bitmap;

        /* Right fringe bitmap number (enum fringe_bitmap_type).  */
        uint right_user_fringe_bitmap;

        /* Left fringe bitmap number (enum fringe_bitmap_type).  */
        uint left_fringe_bitmap;

        /* Right fringe bitmap number (enum fringe_bitmap_type).  */
        uint right_fringe_bitmap;

        /* Face of the left fringe glyph.  */
        uint left_user_fringe_face_id;

        /* Face of the right fringe glyph.  */
        uint right_user_fringe_face_id;

        /* Face of the left fringe glyph.  */
        uint left_fringe_face_id;

        /* Face of the right fringe glyph.  */
        uint right_fringe_face_id;

        /* 1 means that we must draw the bitmaps of this row.  */
        bool redraw_fringe_bitmaps_p;

        /* In a desired matrix, 1 means that this row must be updated.  In a
           current matrix, 0 means that the row has been invalidated, i.e.
           the row's contents do not agree with what is visible on the
           screen.  */
        public bool enabled_p;

        /* 1 means row displays a text line that is truncated on the left or
           right side.  */
        bool truncated_on_left_p;
        bool truncated_on_right_p;

        /* 1 means that this row displays a continued line, i.e. it has a
           continuation mark at the right side.  */
        bool continued_p;

        /* 0 means that this row does not contain any text, i.e. it is
           a blank line at the window and buffer end.  */
        bool displays_text_p;

        /* 1 means that this line ends at ZV.  */
        bool ends_at_zv_p;

        /* 1 means the face of the last glyph in the text area is drawn to
           the right end of the window.  This flag is used in
           update_text_area to optimize clearing to the end of the area.  */
        bool fill_line_p;

        /* Non-zero means display a bitmap on X frames indicating that this
           line contains no text and ends in ZV.  */
        bool indicate_empty_line_p;

        /* 1 means this row contains glyphs that overlap each other because
           of lbearing or rbearing.  */
        bool contains_overlapping_glyphs_p;

        /* 1 means this row is as wide as the window it is displayed in, including
           scroll bars, fringes, and internal borders.  This also
           implies that the row doesn't have marginal areas.  */
        bool full_width_p;

        /* Non-zero means row is a mode or header-line.  */
        bool mode_line_p;

        /* 1 in a current row means this row is overlapped by another row.  */
        bool overlapped_p;

        /* 1 means this line ends in the middle of a character consisting
           of more than one glyph.  Some glyphs have been put in this row,
           the rest are put in rows below this one.  */
        bool ends_in_middle_of_char_p;

        /* 1 means this line starts in the middle of a character consisting
           of more than one glyph.  Some glyphs have been put in the
           previous row, the rest are put in this row.  */
        bool starts_in_middle_of_char_p;

        /* 1 in a current row means this row overlaps others.  */
        bool overlapping_p;

        /* 1 means some glyphs in this row are displayed in mouse-face.  */
        bool mouse_face_p;

        /* 1 means this row was ended by a newline from a string.  */
        bool ends_in_newline_from_string_p;

        /* 1 means this row width is exactly the width of the window, and the
           final newline character is hidden in the right fringe.  */
        bool exact_window_width_line_p;

        /* 1 means this row currently shows the cursor in the right fringe.  */
        bool cursor_in_fringe_p;

        /* 1 means the last glyph in the row is part of an ellipsis.  */
        bool ends_in_ellipsis_p;

        /* Non-zero means display a bitmap on X frames indicating that this
           the first line of the buffer.  */
        bool indicate_bob_p;

        /* Non-zero means display a bitmap on X frames indicating that this
           the top line of the window, but not start of the buffer.  */
        bool indicate_top_line_p;

        /* Non-zero means display a bitmap on X frames indicating that this
           the last line of the buffer.  */
        bool indicate_eob_p;

        /* Non-zero means display a bitmap on X frames indicating that this
           the bottom line of the window, but not end of the buffer.  */
        bool indicate_bottom_line_p;

        /* Continuation lines width at the start of the row.  */
        int continuation_lines_width;

        /* Non-NULL means the current clipping area.  This is temporarily
           set while exposing a region.  Coordinates are frame-relative.  */
        XRectangle clip;
    }

    /* Glyph Pool.

       Glyph memory for frame-based redisplay is allocated from the heap
       in one vector kept in a glyph pool structure which is stored with
       the frame.  The size of the vector is made large enough to cover
       all windows on the frame.

       Both frame and window glyph matrices reference memory from a glyph
       pool in frame-based redisplay.

       In window-based redisplay, no glyphs pools exist; windows allocate
       and free their glyph memory themselves.  */
    public class glyph_pool
    {
        /* Vector of glyphs allocated from the heap.  */
        public glyph[] glyphs;

        /* Allocated size of `glyphs'.  */
        int nglyphs;

        /* Number of rows and columns in a matrix.  */
        int nrows, ncolumns;
    }

    /* Iterator for composition (both for static and automatic).  */
    public class composition_it
    {
        /* Next position at which to check the composition.  */
        public int stop_pos = 0;
        /* ID number of the composition or glyph-string.  If negative, we
           are not iterating over a composition now.  */
        public int id = 0;
        /* If non-negative, character that triggers the automatic
           composition at `stop_pos', and this is an automatic compositoin.
           If negative, this is a static composition.  This is set to -2
           temporarily if searching of composition reach a limit or a
           newline.  */
        public int ch = 0;
        /* If this an automatic composition, how many characters to look back
           from the position where a character triggering the composition
           exists.  */
        public int lookback = 0;
        /* If non-negative, number of glyphs of the glyph-string.  */
        public int nglyphs = 0;
        /* Number of characters and bytes of the current grapheme cluster.  */
        public int nchars = 0, nbytes = 0;
        /* Indices of the glyphs for the current grapheme cluster.  */
        public int from = 0, to = 0;
        /* Width of the current grapheme cluster in units of pixels on a
           graphic display and in units of canonical characters on a
           terminal display.  */
        public int width = 0;
    }

    public partial class L
    {
        /* Return a pointer to the row reserved for the mode line in MATRIX.
           Row MATRIX->nrows - 1 is always reserved for the mode line.  */
        public static glyph_row MATRIX_MODE_LINE_ROW(glyph_matrix MATRIX)
        {
            return MATRIX.rows[MATRIX.nrows - 1];
        }

        /* Return a pointer to the row reserved for the header line in MATRIX.
           This is always the first row in MATRIX because that's the only
           way that works in frame-based redisplay.  */
        public static glyph_row MATRIX_HEADER_LINE_ROW(glyph_matrix MATRIX)
        {
            return MATRIX.rows[0];
        }

        /* Return the height of the mode line in glyph matrix MATRIX, or zero
           if not known.  This macro is called under circumstances where
           MATRIX might not have been allocated yet.  */
        public static int MATRIX_MODE_LINE_HEIGHT(glyph_matrix MATRIX)
        {
            return (MATRIX != null && MATRIX.rows != null ?
                     MATRIX_MODE_LINE_ROW(MATRIX).height : 0);
        }

        /* Return the height of the header line in glyph matrix MATRIX, or zero
           if not known.  This macro is called under circumstances where
           MATRIX might not have been allocated yet.  */
        public static int MATRIX_HEADER_LINE_HEIGHT(glyph_matrix MATRIX)
        {
            return (MATRIX != null && MATRIX.rows != null ?
                     MATRIX_HEADER_LINE_ROW(MATRIX).height : 0);
        }

        /* Value is non-zero if window W wants a mode line.  */
        public static bool WINDOW_WANTS_MODELINE_P(Window W)
        {
            return (!MINI_WINDOW_P((W))
             && !W.pseudo_window_p
             && FRAME_WANTS_MODELINE_P(XFRAME(WINDOW_FRAME((W))))
             && BUFFERP(W.buffer)
             && !NILP(XBUFFER(W.buffer).mode_line_format)
             && WINDOW_TOTAL_LINES(W) > 1);
        }

        /* Value is non-zero if window W wants a header line.  */
        public static bool WINDOW_WANTS_HEADER_LINE_P(Window W)
        {
            return (!MINI_WINDOW_P(W)
             && !W.pseudo_window_p
             && FRAME_WANTS_MODELINE_P(XFRAME(WINDOW_FRAME(W)))
             && BUFFERP(W.buffer)
             && !NILP(XBUFFER(W.buffer).header_line_format)
             && WINDOW_TOTAL_LINES(W) > 1 + (NILP(XBUFFER(W.buffer).mode_line_format) ? 0 : 1));
        }

        /* Return the desired face id for the mode line of a window, depending
           on whether the window is selected or not, or if the window is the
           scrolling window for the currently active minibuffer window.

           Due to the way display_mode_lines manipulates with the contents of
           selected_window, this macro needs three arguments: SELW which is
           compared against the current value of selected_window, MBW which is
           compared against minibuf_window (if SELW doesn't match), and SCRW
           which is compared against minibuf_selected_window (if MBW matches).  */
        public static face_id CURRENT_MODE_LINE_FACE_ID_3(Window SELW, Window MBW, Window SCRW)
        {
            return ((!mode_line_in_non_selected_windows || (SELW) == XWINDOW(selected_window) ||
                     (minibuf_level > 0 && !NILP(minibuf_selected_window) &&
                                           (MBW) == XWINDOW(minibuf_window) &&
                                           (SCRW) == XWINDOW(minibuf_selected_window))) ?
              face_id.MODE_LINE_FACE_ID :
            face_id.MODE_LINE_INACTIVE_FACE_ID);
        }

        /* Return the desired face id for the mode line of window W.  */
        public static face_id CURRENT_MODE_LINE_FACE_ID(Window W)
        {
            return (CURRENT_MODE_LINE_FACE_ID_3(W, XWINDOW(selected_window), W));
        }

        /* Return the current height of the mode line of window W.  If not
           known from current_mode_line_height, look at W's current glyph
           matrix, or return a default based on the height of the font of the
           face `mode-line'.  */
        public static int CURRENT_MODE_LINE_HEIGHT(Window W)
        {
            return (current_mode_line_height >= 0
             ? current_mode_line_height
             : (MATRIX_MODE_LINE_HEIGHT(W.current_matrix) != 0
            ? MATRIX_MODE_LINE_HEIGHT(W.current_matrix)
            : estimate_mode_line_height(XFRAME(W.frame),
                             CURRENT_MODE_LINE_FACE_ID(W))));
        }

        /* Return the current height of the header line of window W.  If not
           known from current_header_line_height, look at W's current glyph
           matrix, or return an estimation based on the height of the font of
           the face `header-line'.  */
        public static int CURRENT_HEADER_LINE_HEIGHT(Window W)
        {
            return (current_header_line_height >= 0
             ? current_header_line_height
             : (MATRIX_HEADER_LINE_HEIGHT(W.current_matrix) != 0
            ? MATRIX_HEADER_LINE_HEIGHT(W.current_matrix)
            : estimate_mode_line_height(XFRAME(W.frame),
                             face_id.HEADER_LINE_FACE_ID)));
        }
    }
}