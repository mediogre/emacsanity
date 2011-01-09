namespace IronElisp
{
    /* Windows are allocated as if they were vectors, but then the
    Lisp data type is changed to Lisp_Window.  They are garbage
    collected along with the vectors.

    All windows in use are arranged into a tree, with pointers up and down.

    Windows that are leaves of the tree are actually displayed
    and show the contents of buffers.  Windows that are not leaves
    are used for representing the way groups of leaf windows are
    arranged on the frame.  Leaf windows never become non-leaves.
    They are deleted only by calling delete-window on them (but
    this can be done implicitly).  Combination windows can be created
    and deleted at any time.

    A leaf window has a non-nil buffer field, and also
     has markers in its start and pointm fields.  Non-leaf windows
     have nil in these fields.

    Non-leaf windows are either vertical or horizontal combinations.

    A vertical combination window has children that are arranged on the frame
    one above the next.  Its vchild field points to the uppermost child.
    The parent field of each of the children points to the vertical
    combination window.  The next field of each child points to the
    child below it, or is nil for the lowest child.  The prev field
    of each child points to the child above it, or is nil for the
    highest child.

    A horizontal combination window has children that are side by side.
    Its hchild field points to the leftmost child.  In each child
    the next field points to the child to the right and the prev field
    points to the child to the left.

    The children of a vertical combination window may be leaf windows
    or horizontal combination windows.  The children of a horizontal
    combination window may be leaf windows or vertical combination windows.

    At the top of the tree are two windows which have nil as parent.
    The second of these is minibuf_window.  The first one manages all
    the frame area that is not minibuffer, and is called the root window.
    Different windows can be the root at different times;
    initially the root window is a leaf window, but if more windows
    are created then that leaf window ceases to be root and a newly
    made combination window becomes root instead.

    In any case, on screens which have an ordinary window and a
    minibuffer, prev of the minibuf window is the root window and next of
    the root window is the minibuf window.  On minibufferless screens or
    minibuffer-only screens, the root window and the minibuffer window are
    one and the same, so its prev and next members are nil.

    A dead window has its buffer, hchild, and vchild windows all nil.  */

    public class cursor_pos
    {
        /* Pixel position.  These are always window relative.  */
        int x, y;

        /* Glyph matrix position.  */
        int hpos, vpos;

        public void zero()
        {
            x = 0;
            y = 0;
            hpos = 0;
            vpos = 0;
        }
    }

    public class Window : LispObject
    {
        /* The first two fields are really the header of a vector */
        /* The window code does not refer to them.  */
        //    uint size;
        //    LispVector vec_next;

        /* The frame this window is on.  */
        public LispObject frame;
        /* t if this window is a minibuffer window.  */
        public LispObject mini_p;
        /* Following child (to right or down) at same level of tree */
        public LispObject next;
        /* Preceding child (to left or up) at same level of tree */
        public LispObject prev;
        /* First child of this window. */
        /* vchild is used if this is a vertical combination,
           hchild if this is a horizontal combination. */
        public LispObject hchild, vchild;
        /* The window this one is a child of. */
        public LispObject parent;
        /* The upper left corner coordinates of this window,
           as integers relative to upper left corner of frame = 0, 0 */
        public LispObject left_col;
        public LispObject top_line;
        /* The size of the window */
        public LispObject total_lines;
        public LispObject total_cols;
        /* The buffer displayed in this window */
        /* Of the fields vchild, hchild and buffer, only one is non-nil.  */
        public LispObject buffer;
        /* A marker pointing to where in the text to start displaying */
        public LispObject start;
        /* A marker pointing to where in the text point is in this window,
           used only when the window is not selected.
           This exists so that when multiple windows show one buffer
           each one can have its own value of point.  */
        public LispObject pointm;
        /* Non-nil means next redisplay must use the value of start
           set up for it in advance.  Set by scrolling commands.  */
        public LispObject force_start;
        /* Non-nil means we have explicitly changed the value of start,
           but that the next redisplay is not obliged to use the new value.
           This is used in Fdelete_other_windows to force a call to
           Vwindow_scroll_functions; also by Frecenter with argument.  */
        public LispObject optional_new_start;
        /* Number of columns display within the window is scrolled to the left.  */
        public LispObject hscroll;
        /* Minimum hscroll for automatic hscrolling.  This is the value
           the user has set, by set-window-hscroll for example.  */
        public LispObject min_hscroll;
        /* Number saying how recently window was selected */
        public LispObject use_time;
        /* Unique number of window assigned when it was created */
        public LispObject sequence_number;
        /* No permanent meaning; used by save-window-excursion's bookkeeping */
        public LispObject temslot;
        /* text.modified of displayed buffer as of last time display completed */
        public LispObject last_modified;
        /* BUF_OVERLAY_MODIFIED of displayed buffer as of last complete update.  */
        public LispObject last_overlay_modified;
        /* Value of point at that time */
        public LispObject last_point;
        /* Non-nil if the buffer was "modified" when the window
           was last updated.  */
        public LispObject last_had_star;
        /* This window's vertical scroll bar.  This field is only for use
           by the window-system-dependent code which implements the
           scroll bars; it can store anything it likes here.  If this
           window is newly created and we haven't displayed a scroll bar in
           it yet, or if the frame doesn't have any scroll bars, this is nil.  */
        public LispObject vertical_scroll_bar;

        /* Width of left and right marginal areas.  A value of nil means
           no margin.  */
        public LispObject left_margin_cols, right_margin_cols;

        /* Width of left and right fringes.
           A value of nil or t means use frame values.  */
        public LispObject left_fringe_width, right_fringe_width;

        /* Non-nil means fringes are drawn outside display margins;
           othersize draw them between margin areas and text.  */
        public LispObject fringes_outside_margins;

        /* Pixel width of scroll bars.
           A value of nil or t means use frame values.  */
        public LispObject scroll_bar_width;
        /* Type of vertical scroll bar.  A value of nil means
           no scroll bar.  A value of t means use frame value.  */
        public LispObject vertical_scroll_bar_type;

        /* Frame coords of mark as of last time display completed */
        /* May be nil if mark does not exist or was not on frame */
        public LispObject last_mark_x;
        public LispObject last_mark_y;
        /* Z - the buffer position of the last glyph in the current matrix
           of W.  Only valid if WINDOW_END_VALID is not nil.  */
        public LispObject window_end_pos;
        /* Glyph matrix row of the last glyph in the current matrix
           of W.  Only valid if WINDOW_END_VALID is not nil.  */
        public LispObject window_end_vpos;
        /* t if window_end_pos is truly valid.
           This is nil if nontrivial redisplay is preempted
           since in that case the frame image that window_end_pos
           did not get onto the frame.  */
        public LispObject window_end_valid;
        /* Non-nil means must regenerate mode line of this window */
        public LispObject update_mode_line;
        /* Non-nil means current value of `start'
           was the beginning of a line when it was chosen.  */
        public LispObject start_at_line_beg;
        /* Display-table to use for displaying chars in this window.
           Nil means use the buffer's own display-table.  */
        public LispObject display_table;
        /* Non-nil means window is marked as dedicated.  */
        public LispObject dedicated;
        /* Line number and position of a line somewhere above the
           top of the screen.  */
        /* If this field is nil, it means we don't have a base line.  */
        public LispObject base_line_number;
        /* If this field is nil, it means we don't have a base line.
           If it is a buffer, it means don't display the line number
           as long as the window shows that buffer.  */
        public LispObject base_line_pos;
        /* If we have highlighted the region (or any part of it),
           this is the mark position that we used, as an integer.  */
        public LispObject region_showing;
        /* The column number currently displayed in this window's mode line,
           or nil if column numbers are not being displayed.  */
        public LispObject column_number_displayed;
        /* If redisplay in this window goes beyond this buffer position,
           must run the redisplay-end-trigger-hook.  */
        public LispObject redisplay_end_trigger;
        /* Non-nil means resizing windows will attempt to resize this window
           proportionally.  */
        public LispObject resize_proportionally;

        /* Original window height and top before mini-window was enlarged. */
        public LispObject orig_total_lines, orig_top_line;

        /* An alist with parameteres.  */
        public LispObject window_parameters;

        /* No Lisp data may follow below this point without changing
           mark_object in alloc.c.  The member current_matrix must be the
           first non-Lisp member.  */

        /* Glyph matrices.  */
        public glyph_matrix current_matrix;
        public glyph_matrix desired_matrix;

        /* Scaling factor for the glyph_matrix size calculation in this window.
           Used if window contains many small images or uses proportional fonts,
           as the normal  may yield a matrix which is too small.  */
        public int nrows_scale_factor, ncols_scale_factor;

        /* Cursor position as of last update that completed without
           pause.  This is the position of last_point.  */
        public cursor_pos last_cursor = new cursor_pos();

        /* Intended cursor position.   This is a position within the
           glyph matrix.  */
        public cursor_pos cursor = new cursor_pos();

        /* Where the cursor actually is.  */
        public cursor_pos phys_cursor = new cursor_pos();

        /* Cursor type and width of last cursor drawn on the window.
           Used for X and w32 frames; -1 initially.  */
        public int phys_cursor_type, phys_cursor_width;

        /* This is handy for undrawing the cursor.  */
        public int phys_cursor_ascent, phys_cursor_height;

        /* Non-zero means the cursor is currently displayed.  This can be
           set to zero by functions overpainting the cursor image.  */
        public bool phys_cursor_on_p;

        /* 0 means cursor is logically on, 1 means it's off.  Used for
           blinking cursor.  */
        public bool cursor_off_p;

        /* Value of cursor_off_p as of the last redisplay.  */
        public bool last_cursor_off_p;

        /* 1 means desired matrix has been build and window must be
           updated in update_frame.  */
        public bool must_be_updated_p;

        /* Flag indicating that this window is not a real one.
           Currently only used for menu bar windows of frames.  */
        public bool pseudo_window_p;

        /* 1 means the window start of this window is frozen and may not
           be changed during redisplay.  If point is not in the window,
           accept that.  */
        public bool frozen_window_start_p;

        /* Amount by which lines of this window are scrolled in
           y-direction (smooth scrolling).  */
        public int vscroll;

        /* Z_BYTE - the buffer position of the last glyph in the current matrix
           of W.  Only valid if WINDOW_END_VALID is not nil.  */
        public int window_end_bytepos;
    }

    public partial class L
    {
        /* The smallest acceptable dimensions for a window.  Anything smaller
           might crash Emacs.  */
        public const int MIN_SAFE_WINDOW_WIDTH = 2;
        public const int MIN_SAFE_WINDOW_HEIGHT = 1;

        /* Nonzero after init_window_once has finished.  */
        public static bool window_initialized;

        /* This is the window in which the terminal's cursor should
           be left when nothing is being done with it.  This must
           always be a leaf window, and its buffer is selected by
           the top level editing loop at the end of each command.

           This value is always the same as
           FRAME_SELECTED_WINDOW (selected_frame).  */
        public static LispObject selected_window;

        /* The mini-buffer window of the selected frame.
           Note that you cannot test for mini-bufferness of an arbitrary window
           by comparing against this; but you can test for mini-bufferness of
           the selected window.  */
        public static LispObject minibuf_window;

        /* Non-nil means it is the window whose mode line should be
           shown as the selected window when the minibuffer is selected.  */
        public static LispObject minibuf_selected_window;

        /* Non-zero means line and page scrolling on tall lines (with images)
           does partial scrolling by modifying window-vscroll.  */
        public static bool auto_window_vscroll_p
        {
            get { return Defs.B[(int)Bools.auto_window_vscroll_p]; }
            set { Defs.B[(int)Bools.auto_window_vscroll_p] = value; }
        }

        /* Non-zero means to use mode-line-inactive face in all windows but the
           selected-window and the minibuffer-scroll-window when the
           minibuffer is active.  */
        public static bool mode_line_in_non_selected_windows
        {
            get { return Defs.B[(int)Bools.mode_line_in_non_selected_windows]; }
            set { Defs.B[(int)Bools.mode_line_in_non_selected_windows] = value; }
        }

        /* 1 if W is a minibuffer window.  */
        public static bool MINI_WINDOW_P(Window W)
        {
            return (!NILP(W.mini_p));
        }

        /* General window layout:

           LEFT_EDGE_COL         RIGHT_EDGE_COL
           |                                  |
           |                                  |
           |  BOX_LEFT_EDGE_COL               |
           |  |           BOX_RIGHT_EDGE_COL  |
           |  |                            |  |
           v  v                            v  v
           <-><-><---><-----------><---><-><->
            ^  ^   ^        ^        ^   ^  ^
            |  |   |        |        |   |  |
            |  |   |        |        |   |  +-- RIGHT_SCROLL_BAR_COLS
            |  |   |        |        |   +----- RIGHT_FRINGE_WIDTH
            |  |   |        |        +--------- RIGHT_MARGIN_COLS
            |  |   |        |
            |  |   |        +------------------ TEXT_AREA_COLS
            |  |   |
            |  |   +--------------------------- LEFT_MARGIN_COLS
            |  +------------------------------- LEFT_FRINGE_WIDTH
            +---------------------------------- LEFT_SCROLL_BAR_COLS

        */


        /* A handy macro.  */
        public static Frame WINDOW_XFRAME(Window W)
        {
            return XFRAME(WINDOW_FRAME(W));
        }

        /* Return the canonical column width of the frame of window W.  */
        public static int WINDOW_FRAME_COLUMN_WIDTH(Window W)
        {
            return FRAME_COLUMN_WIDTH(WINDOW_XFRAME(W));
        }

        /* Return the canonical column width of the frame of window W.  */
        public static int WINDOW_FRAME_LINE_HEIGHT(Window W)
        {
            return FRAME_LINE_HEIGHT(WINDOW_XFRAME(W));
        }

        /* Return the width of window W in canonical column units.
           This includes scroll bars and fringes.  */
        public static int WINDOW_TOTAL_COLS(Window W)
        {
            return XINT(W.total_cols);
        }

        /* Return the height of window W in canonical line units.
           This includes header and mode lines, if any.  */
        public static int WINDOW_TOTAL_LINES(Window W)
        {
            return XINT(W.total_lines);
        }

        /* Return the total pixel width of window W.  */
        public static int WINDOW_TOTAL_WIDTH(Window W)
        {
            return WINDOW_TOTAL_COLS(W) * WINDOW_FRAME_COLUMN_WIDTH(W);
        }

        /* Return the total pixel height of window W.  */
        public static int WINDOW_TOTAL_HEIGHT(Window W)
        {
            return WINDOW_TOTAL_LINES(W) * WINDOW_FRAME_LINE_HEIGHT(W);
        }


        /* Return the canonical frame column at which window W starts.
           This includes a left-hand scroll bar, if any.  */
        public static int WINDOW_LEFT_EDGE_COL(Window W)
        {
            return XINT(W.left_col);
        }

        /* Return the canonical frame column before which window W ends.
           This includes a right-hand scroll bar, if any.  */
        public static int WINDOW_RIGHT_EDGE_COL(Window W)
        {
            return WINDOW_LEFT_EDGE_COL(W) + WINDOW_TOTAL_COLS(W);
        }

        /* Return the canonical frame line at which window W starts.
           This includes a header line, if any.  */
        public static int WINDOW_TOP_EDGE_LINE(Window W)
        {
            return XINT(W.top_line);
        }

        /* Return the canonical frame line before which window W ends.
           This includes a mode line, if any.  */
        public static int WINDOW_BOTTOM_EDGE_LINE(Window W)
        {
            return WINDOW_TOP_EDGE_LINE(W) + WINDOW_TOTAL_LINES(W);
        }


        /* Return the frame x-position at which window W starts.
           This includes a left-hand scroll bar, if any.  */
        public static int WINDOW_LEFT_EDGE_X(Window W)
        {
            return FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_LEFT_EDGE_COL(W) * WINDOW_FRAME_COLUMN_WIDTH(W);
        }

        /* Return the frame x- position before which window W ends.
           This includes a right-hand scroll bar, if any.  */
        public static int WINDOW_RIGHT_EDGE_X(Window W)
        {
            return FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_RIGHT_EDGE_COL(W) * WINDOW_FRAME_COLUMN_WIDTH(W);
        }

        /* Return the frame y-position at which window W starts.
           This includes a header line, if any.  */
        public static int WINDOW_TOP_EDGE_Y(Window W)
        {
            return FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_TOP_EDGE_LINE(W) * WINDOW_FRAME_LINE_HEIGHT(W);
        }

        /* Return the frame y-position before which window W ends.
           This includes a mode line, if any.  */
        public static int WINDOW_BOTTOM_EDGE_Y(Window W)
        {
            return FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_BOTTOM_EDGE_LINE(W) * WINDOW_FRAME_LINE_HEIGHT(W);
        }


        /* 1 if window W takes up the full width of its frame.  */
        public static bool WINDOW_FULL_WIDTH_P(Window W)
        {
            return WINDOW_TOTAL_COLS(W) == FRAME_TOTAL_COLS(WINDOW_XFRAME(W));
        }

        /* 1 if window W's has no other windows to its left in its frame.  */
        public static bool WINDOW_LEFTMOST_P(Window W)
        {
            return (WINDOW_LEFT_EDGE_COL(W) == 0);
        }

        /* 1 if window W's has no other windows to its right in its frame.  */
        public static bool WINDOW_RIGHTMOST_P(Window W)
        {
            return (WINDOW_RIGHT_EDGE_COL(W) == FRAME_TOTAL_COLS(WINDOW_XFRAME(W)));
        }


        /* Return the frame column at which the text (or left fringe) in
           window W starts.  This is different from the `LEFT_EDGE' because it
           does not include a left-hand scroll bar if any.  */
        public static int WINDOW_BOX_LEFT_EDGE_COL(Window W)
        {
            return WINDOW_LEFT_EDGE_COL(W) + WINDOW_LEFT_SCROLL_BAR_COLS(W);
        }

        /* Return the window column before which the text in window W ends.
           This is different from WINDOW_RIGHT_EDGE_COL because it does not
           include a scroll bar or window-separating line on the right edge.  */
        public static int WINDOW_BOX_RIGHT_EDGE_COL(Window W)
        {
            return WINDOW_RIGHT_EDGE_COL(W) - WINDOW_RIGHT_SCROLL_BAR_COLS(W);
        }

        /* Return the frame position at which the text (or left fringe) in
           window W starts.  This is different from the `LEFT_EDGE' because it
           does not include a left-hand scroll bar if any.  */
        public static int WINDOW_BOX_LEFT_EDGE_X(Window W)
        {
            return FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_BOX_LEFT_EDGE_COL(W) * WINDOW_FRAME_COLUMN_WIDTH(W);
        }

        /* Return the window column before which the text in window W ends.
           This is different from WINDOW_RIGHT_EDGE_COL because it does not
           include a scroll bar or window-separating line on the right edge.  */
        public static int WINDOW_BOX_RIGHT_EDGE_X(Window W)
        {
            return (FRAME_INTERNAL_BORDER_WIDTH(WINDOW_XFRAME(W)) + WINDOW_BOX_RIGHT_EDGE_COL(W) * WINDOW_FRAME_COLUMN_WIDTH(W));
        }


        /* Width of left margin area in columns.  */
        public static int WINDOW_LEFT_MARGIN_COLS(Window W)
        {
            return (NILP(W.left_margin_cols) ? 0 : XINT(W.left_margin_cols));
        }

        /* Width of right marginal area in columns.  */
        public static int WINDOW_RIGHT_MARGIN_COLS(Window W)
        {
            return (NILP(W.right_margin_cols) ? 0 : XINT(W.right_margin_cols));
        }

        /* Width of left margin area in pixels.  */
        public static int WINDOW_LEFT_MARGIN_WIDTH(Window W)
        {
            return (NILP(W.left_margin_cols) ? 0 : (XINT(W.left_margin_cols) * WINDOW_FRAME_COLUMN_WIDTH(W)));
        }

        /* Width of right marginal area in pixels.  */
        public static int WINDOW_RIGHT_MARGIN_WIDTH(Window W)
        {
            return (NILP(W.right_margin_cols) ? 0 : (XINT(W.right_margin_cols) * WINDOW_FRAME_COLUMN_WIDTH(W)));
        }
        /* Total width of fringes reserved for drawing truncation bitmaps,
           continuation bitmaps and alike.  The width is in canonical char
           units of the frame.  This must currently be the case because window
           sizes aren't pixel values.  If it weren't the case, we wouldn't be
           able to split windows horizontally nicely.  */
        public static int WINDOW_FRINGE_COLS(Window W)
        {
            return ((INTEGERP(W.left_fringe_width) || INTEGERP(W.right_fringe_width)) ?
          ((WINDOW_LEFT_FRINGE_WIDTH(W) + WINDOW_RIGHT_FRINGE_WIDTH(W) + WINDOW_FRAME_COLUMN_WIDTH(W) - 1) /
           WINDOW_FRAME_COLUMN_WIDTH(W)) : FRAME_FRINGE_COLS(WINDOW_XFRAME(W)));
        }

        /* Column-width of the left and right fringe.  */
        public static int WINDOW_LEFT_FRINGE_COLS(Window W)
        {
            return ((WINDOW_LEFT_FRINGE_WIDTH(W) + WINDOW_FRAME_COLUMN_WIDTH(W) - 1) / WINDOW_FRAME_COLUMN_WIDTH(W));
        }

        public static int WINDOW_RIGHT_FRINGE_COLS(Window W)
        {
            return ((WINDOW_RIGHT_FRINGE_WIDTH(W) + WINDOW_FRAME_COLUMN_WIDTH(W) - 1) / WINDOW_FRAME_COLUMN_WIDTH(W));
        }

        /* Pixel-width of the left and right fringe.  */
        public static int WINDOW_LEFT_FRINGE_WIDTH(Window W)
        {
            return (INTEGERP(W.left_fringe_width) ? XINT(W.left_fringe_width) : FRAME_LEFT_FRINGE_WIDTH(WINDOW_XFRAME(W)));
        }

        public static int WINDOW_RIGHT_FRINGE_WIDTH(Window W)
        {
            return (INTEGERP(W.right_fringe_width) ? XINT(W.right_fringe_width) : FRAME_RIGHT_FRINGE_WIDTH(WINDOW_XFRAME(W)));
        }

        /* Total width of fringes in pixels.  */
        public static int WINDOW_TOTAL_FRINGE_WIDTH(Window W)
        {
            return (WINDOW_LEFT_FRINGE_WIDTH(W) + WINDOW_RIGHT_FRINGE_WIDTH(W));
        }

        /* Are fringes outside display margins in window W.  */
        public static bool WINDOW_HAS_FRINGES_OUTSIDE_MARGINS(Window W)
        {
            return (!NILP(W.fringes_outside_margins));
        }

        /* Say whether scroll bars are currently enabled for window W,
           and which side they are on.  */
        public static vertical_scroll_bar_type WINDOW_VERTICAL_SCROLL_BAR_TYPE(Window w)
        {
            return (EQ(w.vertical_scroll_bar_type, Q.t)
             ? FRAME_VERTICAL_SCROLL_BAR_TYPE(WINDOW_XFRAME(w))
             : EQ(w.vertical_scroll_bar_type, Q.left)
             ? vertical_scroll_bar_type.vertical_scroll_bar_left
             : EQ(w.vertical_scroll_bar_type, Q.right)
             ? vertical_scroll_bar_type.vertical_scroll_bar_right
             : vertical_scroll_bar_type.vertical_scroll_bar_none);
        }

        public static bool WINDOW_HAS_VERTICAL_SCROLL_BAR(Window w)
        {
            return (EQ(w.vertical_scroll_bar_type, Q.t)
             ? FRAME_HAS_VERTICAL_SCROLL_BARS(WINDOW_XFRAME(w))
             : !NILP(w.vertical_scroll_bar_type));
        }

        public static bool WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_LEFT(Window w)
        {
            return (EQ(w.vertical_scroll_bar_type, Q.t)
             ? FRAME_HAS_VERTICAL_SCROLL_BARS_ON_LEFT(WINDOW_XFRAME(w))
             : EQ(w.vertical_scroll_bar_type, Q.left));
        }

        public static bool WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_RIGHT(Window w)
        {
            return (EQ(w.vertical_scroll_bar_type, Q.t)
             ? FRAME_HAS_VERTICAL_SCROLL_BARS_ON_RIGHT(WINDOW_XFRAME(w))
             : EQ(w.vertical_scroll_bar_type, Q.right));
        }

        /* Width that a scroll bar in window W should have, if there is one.
           Measured in pixels.  If scroll bars are turned off, this is still
           nonzero.  */
        public static int WINDOW_CONFIG_SCROLL_BAR_WIDTH(Window w)
        {
            return (INTEGERP(w.scroll_bar_width)
             ? XINT(w.scroll_bar_width)
             : FRAME_CONFIG_SCROLL_BAR_WIDTH(WINDOW_XFRAME(w)));
        }

        /* Width that a scroll bar in window W should have, if there is one.
           Measured in columns (characters).  If scroll bars are turned off,
           this is still nonzero.  */
        public static int WINDOW_CONFIG_SCROLL_BAR_COLS(Window w)
        {
            return (INTEGERP(w.scroll_bar_width)
             ? ((XINT(w.scroll_bar_width)
                 + WINDOW_FRAME_COLUMN_WIDTH(w) - 1)
                / WINDOW_FRAME_COLUMN_WIDTH(w))
             : FRAME_CONFIG_SCROLL_BAR_COLS(WINDOW_XFRAME(w)));
        }

        /* Width of a scroll bar in window W, measured in columns (characters),
           but only if scroll bars are on the left.  If scroll bars are on
           the right in this frame, or there are no scroll bars, value is 0.  */
        public static int WINDOW_LEFT_SCROLL_BAR_COLS(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_LEFT(w)
             ? (WINDOW_CONFIG_SCROLL_BAR_COLS(w))
             : 0);
        }

        /* Width of a left scroll bar area in window W , measured in pixels.  */
        public static int WINDOW_LEFT_SCROLL_BAR_AREA_WIDTH(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_LEFT(w)
             ? (WINDOW_CONFIG_SCROLL_BAR_COLS(w) * WINDOW_FRAME_COLUMN_WIDTH(w))
             : 0);
        }

        /* Width of a scroll bar in window W, measured in columns (characters),
           but only if scroll bars are on the right.  If scroll bars are on
           the left in this frame, or there are no scroll bars, value is 0.  */
        public static int WINDOW_RIGHT_SCROLL_BAR_COLS(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_RIGHT(w)
             ? WINDOW_CONFIG_SCROLL_BAR_COLS(w)
             : 0);
        }

        /* Width of a left scroll bar area in window W , measured in pixels.  */
        public static int WINDOW_RIGHT_SCROLL_BAR_AREA_WIDTH(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_RIGHT(w)
             ? (WINDOW_CONFIG_SCROLL_BAR_COLS(w) * WINDOW_FRAME_COLUMN_WIDTH(w))
             : 0);
        }


        /* Actual width of a scroll bar in window W, measured in columns.  */
        public static int WINDOW_SCROLL_BAR_COLS(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR(w)
             ? WINDOW_CONFIG_SCROLL_BAR_COLS(w)
             : 0);
        }

        /* Width of a left scroll bar area in window W , measured in pixels.  */
        public static int WINDOW_SCROLL_BAR_AREA_WIDTH(Window w)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR(w)
             ? (WINDOW_CONFIG_SCROLL_BAR_COLS(w) * WINDOW_FRAME_COLUMN_WIDTH(w))
             : 0);
        }


        /* Return the frame position where the scroll bar of window W starts.  */
        public static int WINDOW_SCROLL_BAR_AREA_X(Window W)
        {
            return (WINDOW_HAS_VERTICAL_SCROLL_BAR_ON_RIGHT(W)
             ? WINDOW_BOX_RIGHT_EDGE_X(W)
             : WINDOW_LEFT_EDGE_X(W));
        }


        /* Height in pixels, and in lines, of the mode line.
           May be zero if W doesn't have a mode line.  */
        public static int WINDOW_MODE_LINE_HEIGHT(Window W)
        {
            return (WINDOW_WANTS_MODELINE_P(W)
             ? CURRENT_MODE_LINE_HEIGHT(W)
             : 0);
        }

        public static bool WINDOW_MODE_LINE_LINES(Window W)
        {
            return (!!WINDOW_WANTS_MODELINE_P(W));
        }

        /* Height in pixels, and in lines, of the header line.
           Zero if W doesn't have a header line.  */
        public static int WINDOW_HEADER_LINE_HEIGHT(Window W)
        {
            return (WINDOW_WANTS_HEADER_LINE_P((W))
             ? CURRENT_HEADER_LINE_HEIGHT(W)
             : 0);
        }

        public static bool WINDOW_HEADER_LINE_LINES(Window W)
        {
            return (!!WINDOW_WANTS_HEADER_LINE_P((W)));
        }

        /* Pixel height of window W without mode line.  */
        public static int WINDOW_BOX_HEIGHT_NO_MODE_LINE(Window W)
        {
            return (WINDOW_TOTAL_HEIGHT(W) - WINDOW_MODE_LINE_HEIGHT(W));
        }

        /* Pixel height of window W without mode and header line.  */
        public static int WINDOW_BOX_TEXT_HEIGHT(Window W)
        {
            return (WINDOW_TOTAL_HEIGHT(W) - WINDOW_MODE_LINE_HEIGHT(W) - WINDOW_HEADER_LINE_HEIGHT(W));
        }

        /* Convert window W relative pixel X to frame pixel coordinates.  */
        public static int WINDOW_TO_FRAME_PIXEL_X(Window W, int X)
        {
            return (X + WINDOW_BOX_LEFT_EDGE_X(W));
        }

        /* Convert window W relative pixel Y to frame pixel coordinates.  */
        public static int WINDOW_TO_FRAME_PIXEL_Y(Window W, int Y)
        {
            return (Y + WINDOW_TOP_EDGE_Y(W));
        }

        /* Convert frame relative pixel X to window relative pixel X.  */
        public static int FRAME_TO_WINDOW_PIXEL_X(Window W, int X)
        {
            return (X - WINDOW_BOX_LEFT_EDGE_X(W));
        }

        /* Convert frame relative pixel Y to window relative pixel Y.  */
        public static int FRAME_TO_WINDOW_PIXEL_Y(Window W, int Y)
        {
            return (Y - WINDOW_TOP_EDGE_Y(W));
        }

        /* Convert a text area relative x-position in window W to frame X
           pixel coordinates.  */
        public static int WINDOW_TEXT_TO_FRAME_PIXEL_X(Window W, int X)
        {
            return (window_box_left(W, (int) glyph_row_area.TEXT_AREA) + X);
        }

        /* Value is non-zero if WINDOW is a live window.  */
        public static bool WINDOW_LIVE_P(Window WINDOW)
        {
            return (WINDOWP(WINDOW) && !NILP(XWINDOW(WINDOW).buffer));
        }

        public static Window decode_window (LispObject window)
        {
            if (NILP(window))
                return XWINDOW(selected_window);

            CHECK_LIVE_WINDOW(window);
            return XWINDOW(window);
        }

        public delegate bool foreach_window_delegate(Window w, ref LispObject d);

        /* Call FN for all leaf windows on frame F.  FN is called with the
           first argument being a pointer to the leaf window, and with
           additional argument USER_DATA.  Stops when FN returns 0.  */
        public static void foreach_window (Frame f, foreach_window_delegate fn, ref LispObject user_data)
        {
            /* delete_frame may set FRAME_ROOT_WINDOW (f) to Qnil.  */
            if (WINDOWP(FRAME_ROOT_WINDOW(f)))
                foreach_window_1(XWINDOW(FRAME_ROOT_WINDOW(f)), fn, ref user_data);
        }

        /* Helper function for foreach_window.  Call FN for all leaf windows
           reachable from W.  FN is called with the first argument being a
           pointer to the leaf window, and with additional argument USER_DATA.
           Stop when FN returns 0.  Value is 0 if stopped by FN.  */
        public static bool foreach_window_1 (Window w, foreach_window_delegate fn, ref LispObject user_data)
        {
            bool cont;

            for (cont = true; w != null && cont; )
            {
                if (!NILP(w.hchild))
                    cont = foreach_window_1(XWINDOW(w.hchild), fn, ref user_data);
                else if (!NILP(w.vchild))
                    cont = foreach_window_1(XWINDOW(w.vchild), fn, ref user_data);
                else
                    cont = fn(w, ref user_data);

                w = NILP(w.next) ? null : XWINDOW(w.next);
            }

            return cont;
        }

        /* Add window W to *USER_DATA.  USER_DATA is actually a Lisp_Object
           pointer.  This is a callback function for foreach_window, used in
           function window_list.  */
        public static bool add_window_to_list (Window w, ref LispObject list)
        {
            list = F.cons(w, list);
            return true;
        }

        /* Return a list of all windows, for use by next_window.  If
           Vwindow_list is a list, return that list.  Otherwise, build a new
           list, cache it in Vwindow_list, and return that.  */
        public static LispObject window_list()
        {
            if (!CONSP(V.window_list))
            {
                LispObject tail;

                V.window_list = Q.nil;
                for (tail = V.frame_list; CONSP(tail); tail = XCDR(tail))
                {
                    LispObject args_0;
                    LispObject args_1;

                    /* We are visiting windows in canonical order, and add
                       new windows at the front of args[1], which means we
                       have to reverse this list at the end.  */
                    args_1 = Q.nil;
                    foreach_window(XFRAME(XCAR(tail)), add_window_to_list, ref args_1);
                    args_0 = V.window_list;
                    args_1 = F.nreverse(args_1);
                    V.window_list = F.nconc(2, args_0, args_1);
                }
            }

            return V.window_list;
        }

        /* Value is non-zero if WINDOW satisfies the constraints given by
           OWINDOW, MINIBUF and ALL_FRAMES.

           MINIBUF	t means WINDOW may be minibuffer windows.
                `lambda' means WINDOW may not be a minibuffer window.
                a window means a specific minibuffer window

           ALL_FRAMES	t means search all frames,
                nil means search just current frame,
                `visible' means search just visible frames,
                0 means search visible and iconified frames,
                a window means search the frame that window belongs to,
                a frame means consider windows on that frame, only.  */
        public static bool candidate_window_p(LispObject window, LispObject owindow, LispObject minibuf, LispObject all_frames)
        {
            Window w = XWINDOW(window);
            Frame f = XFRAME(w.frame);
            bool candidate_p = true;

            if (!BUFFERP(w.buffer))
                candidate_p = false;
            else if (MINI_WINDOW_P(w)
                     && (EQ(minibuf, Q.lambda)
                     || (WINDOWP(minibuf) && !EQ(minibuf, window))))
            {
                /* If MINIBUF is `lambda' don't consider any mini-windows.
                   If it is a window, consider only that one.  */
                candidate_p = false;
            }
            else if (EQ(all_frames, Q.t))
                candidate_p = true;
            else if (NILP(all_frames))
            {
                // xassert (WINDOWP (owindow));
                candidate_p = EQ(w.frame, XWINDOW(owindow).frame);
            }
            else if (EQ(all_frames, Q.visible))
            {
                FRAME_SAMPLE_VISIBILITY(f);
                candidate_p = FRAME_VISIBLE_P(f)
              && (FRAME_TERMINAL(XFRAME(w.frame))
                  == FRAME_TERMINAL(XFRAME(selected_frame)));

            }
            else if (INTEGERP(all_frames) && XINT(all_frames) == 0)
            {
                FRAME_SAMPLE_VISIBILITY(f);
                candidate_p = (FRAME_VISIBLE_P(f) || FRAME_ICONIFIED_P(f)
#if HAVE_X_WINDOWS
		     /* Yuck!!  If we've just created the frame and the
			window-manager requested the user to place it
			manually, the window may still not be considered
			`visible'.  I'd argue it should be at least
			something like `iconified', but don't know how to do
			that yet.  --Stef  */
		     || (FRAME_X_P (f) && f->output_data.x->asked_for_visible
			 && !f->output_data.x->has_been_visible)
#endif
)
              && (FRAME_TERMINAL(XFRAME(w.frame))
                  == FRAME_TERMINAL(XFRAME(selected_frame)));
            }
            else if (WINDOWP(all_frames))
                candidate_p = (EQ(FRAME_MINIBUF_WINDOW(f), all_frames)
                       || EQ(XWINDOW(all_frames).frame, w.frame)
                       || EQ(XWINDOW(all_frames).frame, FRAME_FOCUS_FRAME(f)));
            else if (FRAMEP(all_frames))
                candidate_p = EQ(all_frames, w.frame);

            return candidate_p;
        }


        /* Decode arguments as allowed by Fnext_window, Fprevious_window, and
           Fwindow_list.  See there for the meaning of WINDOW, MINIBUF, and
           ALL_FRAMES.  */
        public static void decode_next_window_args (ref LispObject window, ref LispObject minibuf, ref LispObject all_frames)
        {
            if (NILP(window))
                window = selected_window;
            else
                CHECK_LIVE_WINDOW(window);

            /* MINIBUF nil may or may not include minibuffers.  Decide if it
               does.  */
            if (NILP(minibuf))
                minibuf = minibuf_level != 0 ? minibuf_window : Q.lambda;
            else if (!EQ(minibuf, Q.t))
                minibuf = Q.lambda;

            /* Now *MINIBUF can be t => count all minibuffer windows, `lambda'
               => count none of them, or a specific minibuffer window (the
               active one) to count.  */

            /* ALL_FRAMES nil doesn't specify which frames to include.  */
            if (NILP(all_frames))
                all_frames = (!EQ(minibuf, Q.lambda)
           ? FRAME_MINIBUF_WINDOW(XFRAME(XWINDOW(window).frame))
           : Q.nil);
            else if (EQ(all_frames, Q.visible))
            { }
            else if (EQ(all_frames, make_number(0)))
            { }
            else if (FRAMEP(all_frames))
            { }
            else if (!EQ(all_frames, Q.t))
                all_frames = Q.nil;

            /* Now *ALL_FRAMES is t meaning search all frames, nil meaning
               search just current frame, `visible' meaning search just visible
               frames, 0 meaning search visible and iconified frames, or a
               window, meaning search the frame that window belongs to, or a
               frame, meaning consider windows on that frame, only.  */
        }


        public static LispObject window_list_1(LispObject window, LispObject minibuf, LispObject all_frames)
        {
            LispObject tail, list, rest;

            decode_next_window_args(ref window, ref minibuf, ref all_frames);
            list = Q.nil;

            for (tail = window_list(); CONSP(tail); tail = XCDR(tail))
                if (candidate_window_p(XCAR(tail), window, minibuf, all_frames))
                    list = F.cons(XCAR(tail), list);

            /* Rotate the list to start with WINDOW.  */
            list = F.nreverse(list);
            rest = F.memq(window, list);
            if (!NILP(rest) && !EQ(rest, list))
            {
                for (tail = list; !EQ(XCDR(tail), rest); tail = XCDR(tail))
                    ;
                XSETCDR(tail, Q.nil);
                list = nconc2(rest, list);
            }
            return list;
        }

        /* Look at all windows, performing an operation specified by TYPE
           with argument OBJ.
           If FRAMES is Qt, look at all frames;
                        Qnil, look at just the selected frame;
                Qvisible, look at visible frames;
                    a frame, just look at windows on that frame.
           If MINI is non-zero, perform the operation on minibuffer windows too.  */
        public enum window_loop_enum
        {
            WINDOW_LOOP_UNUSED,
            GET_BUFFER_WINDOW,		/* Arg is buffer */
            GET_LRU_WINDOW,		/* Arg is t for full-width windows only */
            DELETE_OTHER_WINDOWS,		/* Arg is window not to delete */
            DELETE_BUFFER_WINDOWS,	/* Arg is buffer */
            GET_LARGEST_WINDOW,
            UNSHOW_BUFFER,		/* Arg is buffer */
            REDISPLAY_BUFFER_WINDOWS,	/* Arg is buffer */
            CHECK_ALL_WINDOWS
        }

        public static LispObject window_loop(window_loop_enum type, LispObject obj, bool mini, LispObject frames)
        {
            LispObject window, windows, best_window, frame_arg;
            Frame f;

            /* If we're only looping through windows on a particular frame,
               frame points to that frame.  If we're looping through windows
               on all frames, frame is 0.  */
            if (FRAMEP(frames))
                f = XFRAME(frames);
            else if (NILP(frames))
                f = SELECTED_FRAME();
            else
                f = null;

            if (f != null)
                frame_arg = Q.lambda;
            else if (EQ(frames, make_number(0)))
                frame_arg = frames;
            else if (EQ(frames, Q.visible))
                frame_arg = frames;
            else
                frame_arg = Q.t;

            /* frame_arg is Qlambda to stick to one frame,
               Qvisible to consider all visible frames,
               or Qt otherwise.  */

            /* Pick a window to start with.  */
            if (WINDOWP(obj))
                window = obj;
            else if (f != null)
                window = FRAME_SELECTED_WINDOW(f);
            else
                window = FRAME_SELECTED_WINDOW(SELECTED_FRAME());

            windows = window_list_1(window, mini ? Q.t : Q.nil, frame_arg);
            best_window = Q.nil;

            for (; CONSP(windows); windows = XCDR(windows))
            {
                Window w;

                window = XCAR(windows);
                w = XWINDOW(window);

                /* Note that we do not pay attention here to whether the frame
               is visible, since Fwindow_list skips non-visible frames if
               that is desired, under the control of frame_arg.  */
                if (!MINI_WINDOW_P(w)
                    /* For UNSHOW_BUFFER, we must always consider all windows.  */
                || type == window_loop_enum.UNSHOW_BUFFER
                || (mini && minibuf_level > 0))
                    switch (type)
                    {
                        case window_loop_enum.GET_BUFFER_WINDOW:
                            if (EQ(w.buffer, obj)
                                /* Don't find any minibuffer window
                                   except the one that is currently in use.  */
                            && (MINI_WINDOW_P(w)
                                ? EQ(window, minibuf_window)
                                : true))
                            {
                                if (NILP(best_window))
                                    best_window = window;
                                else if (EQ(window, selected_window))
                                    /* Prefer to return selected-window.  */
                                    return (window);
                                else if (EQ(F.window_frame(window), selected_frame))
                                    /* Prefer windows on the current frame.  */
                                    best_window = window;
                            }
                            break;

                        case window_loop_enum.GET_LRU_WINDOW:
                            /* `obj' is an integer encoding a bitvector.
                               `obj & 1' means consider only full-width windows.
                               `obj & 2' means consider also dedicated windows. */
                            if (((XINT(obj) & 1) != 0 && !WINDOW_FULL_WIDTH_P(w))
                            || ((XINT(obj) & 2) == 0 && !NILP(w.dedicated))
                                /* Minibuffer windows are always ignored.  */
                            || MINI_WINDOW_P(w))
                                break;
                            if (NILP(best_window)
                            || (XINT(XWINDOW(best_window).use_time)
                                > XINT(w.use_time)))
                                best_window = window;
                            break;

                        case window_loop_enum.DELETE_OTHER_WINDOWS:
                            if (!EQ(window, obj))
                                F.delete_window(window);
                            break;

                        case window_loop_enum.DELETE_BUFFER_WINDOWS:
                            if (EQ(w.buffer, obj))
                            {
                                Frame ff = XFRAME(WINDOW_FRAME(w));

                                /* If this window is dedicated, and in a frame of its own,
                                   kill the frame.  */
                                if (EQ(window, FRAME_ROOT_WINDOW(ff))
                                    && !NILP(w.dedicated)
                                    && other_visible_frames(ff))
                                {
                                    /* Skip the other windows on this frame.
                                       There might be one, the minibuffer!  */
                                    while (CONSP(XCDR(windows))
                                       && EQ(XWINDOW(XCAR(windows)).frame,
                                          XWINDOW(XCAR(XCDR(windows))).frame))
                                        windows = XCDR(windows);

                                    /* Now we can safely delete the frame.  */
                                    delete_frame(w.frame, Q.nil);
                                }
                                else if (NILP(w.parent))
                                {
                                    /* If we're deleting the buffer displayed in the
                                       only window on the frame, find a new buffer to
                                       display there.  */
                                    LispObject buffer;
                                    buffer = F.other_buffer(obj, Q.nil, w.frame);
                                    /* Reset dedicated state of window.  */
                                    w.dedicated = Q.nil;
                                    F.set_window_buffer(window, buffer, Q.nil);
                                    if (EQ(window, selected_window))
                                        F.set_buffer(w.buffer);
                                }
                                else
                                    F.delete_window(window);
                            }
                            break;

                        case window_loop_enum.GET_LARGEST_WINDOW:
                            { /* nil `obj' means to ignore dedicated windows.  */
                                /* Ignore dedicated windows and minibuffers.  */
                                if (MINI_WINDOW_P(w) || (NILP(obj) && !NILP(w.dedicated)))
                                    break;

                                if (NILP(best_window))
                                    best_window = window;
                                else
                                {
                                    Window b = XWINDOW(best_window);
                                    if (XINT(w.total_lines) * XINT(w.total_cols)
                                        > XINT(b.total_lines) * XINT(b.total_cols))
                                        best_window = window;
                                }
                            }
                            break;

                        case window_loop_enum.UNSHOW_BUFFER:
                            if (EQ(w.buffer, obj))
                            {
                                LispObject buffer;
                                Frame ff = XFRAME(w.frame);

                                /* Find another buffer to show in this window.  */
                                buffer = F.other_buffer(obj, Q.nil, w.frame);

                                /* If this window is dedicated, and in a frame of its own,
                                   kill the frame.  */
                                if (EQ(window, FRAME_ROOT_WINDOW(ff))
                                    && !NILP(w.dedicated)
                                    && other_visible_frames(ff))
                                {
                                    /* Skip the other windows on this frame.
                                       There might be one, the minibuffer!  */
                                    while (CONSP(XCDR(windows))
                                       && EQ(XWINDOW(XCAR(windows)).frame,
                                          XWINDOW(XCAR(XCDR(windows))).frame))
                                        windows = XCDR(windows);

                                    /* Now we can safely delete the frame.  */
                                    delete_frame(w.frame, Q.nil);
                                }
                                else if (!NILP(w.dedicated) && !NILP(w.parent))
                                {
                                    /* If this window is dedicated and not the only window
                                       in its frame, then kill it.  */
                                    F.delete_window(w);
                                }
                                else
                                {
                                    /* Otherwise show a different buffer in the window.  */
                                    w.dedicated = Q.nil;
                                    F.set_window_buffer(window, buffer, Q.nil);
                                    if (EQ(window, selected_window))
                                        F.set_buffer(w.buffer);
                                }
                            }
                            break;

                        case window_loop_enum.REDISPLAY_BUFFER_WINDOWS:
                            if (EQ(w.buffer, obj))
                            {
                                mark_window_display_accurate(window, 0);
                                w.update_mode_line = Q.t;
                                XBUFFER(obj).prevent_redisplay_optimizations_p = true;
                                ++update_mode_lines;
                                best_window = window;
                            }
                            break;

                        /* Check for a window that has a killed buffer.  */
                        case window_loop_enum.CHECK_ALL_WINDOWS:
                            if (!NILP(w.buffer)
                            && NILP(XBUFFER(w.buffer).name))
                                abort();
                            break;

                        case window_loop_enum.WINDOW_LOOP_UNUSED:
                            break;
                    }
            }

            return best_window;
        }

        /* Make WINDOW display BUFFER as its contents.  RUN_HOOKS_P non-zero
           means it's allowed to run hooks.  See make_frame for a case where
           it's not allowed.  KEEP_MARGINS_P non-zero means that the current
           margins, fringes, and scroll-bar settings of the window are not
           reset from the buffer's local settings.  */
        public static void set_window_buffer(LispObject window, LispObject buffer, bool run_hooks_p, bool keep_margins_p)
        {
            Window w = XWINDOW(window);
            Buffer b = XBUFFER(buffer);
            int count = SPECPDL_INDEX();
            bool samebuf = EQ(buffer, w.buffer);

            w.buffer = buffer;

            if (EQ(window, selected_window))
                b.last_selected_window = window;

            /* Let redisplay errors through.  */
            b.display_error_modiff = 0;

            /* Update time stamps of buffer display.  */
            if (INTEGERP(b.display_count))
                b.display_count = XSETINT(XINT(b.display_count) + 1);
            b.display_time = F.current_time();

            w.window_end_pos = XSETINT(0);
            w.window_end_vpos = XSETINT(0);
            w.last_cursor.zero();
            w.window_end_valid = Q.nil;
            if (!(keep_margins_p && samebuf))
            {
                /* If we're not actually changing the buffer, don't reset hscroll and
                     vscroll.  This case happens for example when called from
                     change_frame_size_1, where we use a dummy call to
                     Fset_window_buffer on the frame's selected window (and no other)
                     just in order to run window-configuration-change-hook.
                     Resetting hscroll and vscroll here is problematic for things like
                     image-mode and doc-view-mode since it resets the image's position
                     whenever we resize the frame.  */
                w.hscroll = w.min_hscroll = make_number(0);
                w.vscroll = 0;
                set_marker_both(w.pointm, buffer, BUF_PT(b), BUF_PT_BYTE(b));
                set_marker_restricted(w.start,
                           make_number(b.last_window_start),
                           buffer);
                w.start_at_line_beg = Q.nil;
                w.force_start = Q.nil;
                w.last_modified = XSETINT(0);
                w.last_overlay_modified = XSETINT(0);
            }
            /* Maybe we could move this into the `if' but it's not obviously safe and
               I doubt it's worth the trouble.  */
            windows_or_buffers_changed++;

            /* We must select BUFFER for running the window-scroll-functions.  */
            /* We can't check ! NILP (Vwindow_scroll_functions) here
               because that might itself be a local variable.  */
            if (window_initialized)
            {
                record_unwind_protect(F.set_buffer, F.current_buffer());
                F.set_buffer(buffer);
            }

            XMARKER(w.pointm).insertion_type = !NILP(V.window_point_insertion_type);

            if (!keep_margins_p)
            {
                /* Set left and right marginal area width etc. from buffer.  */

                /* This may call adjust_window_margins three times, so
               temporarily disable window margins.  */
                LispObject save_left = w.left_margin_cols;
                LispObject save_right = w.right_margin_cols;

                w.left_margin_cols = w.right_margin_cols = Q.nil;

                F.set_window_fringes(window,
                         b.left_fringe_width, b.right_fringe_width,
                         b.fringes_outside_margins);

                F.set_window_scroll_bars(window,
                             b.scroll_bar_width,
                             b.vertical_scroll_bar_type, Q.nil);

                w.left_margin_cols = save_left;
                w.right_margin_cols = save_right;

                F.set_window_margins(window,
                         b.left_margin_cols, b.right_margin_cols);
            }

            if (run_hooks_p)
            {
                if (!NILP(V.window_scroll_functions))
                    run_hook_with_args_2(Q.window_scroll_functions, window,
                                  F.marker_position(w.start));
                run_window_configuration_change_hook(XFRAME(WINDOW_FRAME(w)));
            }

            unbind_to(count, Q.nil);
        }

        public static void run_window_configuration_change_hook (Frame f)
        {
            // COMEBACK WHEN READY
            //int count = SPECPDL_INDEX();
            //LispObject frame, global_wcch = F.default_value (Q.window_configuration_change_hook);
            //frame = f;

            //if (NILP(V.run_hooks))
            //    return;

            //if (SELECTED_FRAME() != f)
            //{
            //    record_unwind_protect(select_frame_norecord, F.selected_frame());
            //    F.select_frame(frame, Q.t);
            //}

            ///* Use the right buffer.  Matters when running the local hooks.  */
            //if (current_buffer != XBUFFER(F.window_buffer(Q.nil)))
            //{
            //    record_unwind_protect(F.set_buffer, F.current_buffer());
            //    F.set_buffer(F.window_buffer(Q.nil));
            //}

            ///* Look for buffer-local values.  */
            //{
            //    LispObject windows = F.window_list(frame, Q.lambda, Q.nil);
            //    for (; CONSP(windows); windows = XCDR(windows))
            //    {
            //        LispObject window = XCAR(windows);
            //        LispObject buffer = F.window_buffer(window);
            //        if (!NILP(F.local_variable_p(Q.window_configuration_change_hook,
            //          buffer)))
            //        {
            //            int count = SPECPDL_INDEX();
            //            record_unwind_protect(select_window_norecord, F.selected_window());
            //            select_window_norecord(window);
            //            run_funs(F.buffer_local_value(Q.window_configuration_change_hook, buffer));
            //            unbind_to(count, Q.nil);
            //        }
            //    }
            //}

            //run_funs(global_wcch);
            //unbind_to(count, Q.nil);
        }

        /* Adjust the margins of window W if text area is too small.
           Return 1 if window width is ok after adjustment; 0 if window
           is still too narrow.  */
        public static int adjust_window_margins (Window w)
        {
            int box_cols = (WINDOW_TOTAL_COLS(w) - WINDOW_FRINGE_COLS(w) - WINDOW_SCROLL_BAR_COLS (w));
            int margin_cols = (WINDOW_LEFT_MARGIN_COLS(w) + WINDOW_RIGHT_MARGIN_COLS (w));

            if (box_cols - margin_cols >= MIN_SAFE_WINDOW_WIDTH)
                return 1;

            if (margin_cols < 0 || box_cols < MIN_SAFE_WINDOW_WIDTH)
                return 0;

            /* Window's text area is too narrow, but reducing the window
               margins will fix that.  */
            margin_cols = box_cols - MIN_SAFE_WINDOW_WIDTH;
            if (WINDOW_RIGHT_MARGIN_COLS(w) > 0)
            {
                if (WINDOW_LEFT_MARGIN_COLS(w) > 0)
                    w.left_margin_cols = w.right_margin_cols = make_number(margin_cols / 2);
                else
                    w.right_margin_cols = make_number(margin_cols);
            }
            else
                w.left_margin_cols = make_number(margin_cols);
            return 1;
        }

        /* Record info on buffer window w is displaying
           when it is about to cease to display that buffer.  */
        public static void unshow_buffer(Window w)
        {
            LispObject buf;
            Buffer b;

            buf = w.buffer;
            b = XBUFFER(buf);
            if (b != XMARKER(w.pointm).buffer)
                abort();

            /* last_window_start records the start position that this buffer
               had in the last window to be disconnected from it.
               Now that this statement is unconditional,
               it is possible for the buffer to be displayed in the
               selected window, while last_window_start reflects another
               window which was recently showing the same buffer.
               Some people might say that might be a good thing.  Let's see.  */
            b.last_window_start = marker_position(w.start);

            /* Point in the selected window's buffer
               is actually stored in that buffer, and the window's pointm isn't used.
               So don't clobber point in that buffer.  */
            if (!EQ(buf, XWINDOW(selected_window).buffer)
                /* This line helps to fix Horsley's testbug.el bug.  */
                && !(WINDOWP(b.last_selected_window)
                 && w != XWINDOW(b.last_selected_window)
                 && EQ(buf, XWINDOW(b.last_selected_window).buffer)))
                temp_set_point_both(b,
                         clip_to_bounds(BUF_BEGV(b),
                                 XMARKER(w.pointm).charpos,
                                 BUF_ZV(b)),
                         clip_to_bounds(BUF_BEGV_BYTE(b),
                                 marker_byte_position(w.pointm),
                                 BUF_ZV_BYTE(b)));

            if (WINDOWP(b.last_selected_window)
                && w == XWINDOW(b.last_selected_window))
                b.last_selected_window = Q.nil;
        }
    }

    public partial class F
    {
        public static LispObject set_window_fringes(LispObject window, LispObject left_width, LispObject right_width, LispObject outside_margins)
        {
            Window w = L.decode_window(window);

            if (!L.NILP(left_width))
                L.CHECK_NATNUM(left_width);
            if (!L.NILP(right_width))
                L.CHECK_NATNUM(right_width);

            /* Do nothing on a tty.  */
            if (L.FRAME_WINDOW_P(L.WINDOW_XFRAME(w))
                && (!L.EQ(w.left_fringe_width, left_width)
                || !L.EQ(w.right_fringe_width, right_width)
                || !L.EQ(w.fringes_outside_margins, outside_margins)))
            {
                w.left_fringe_width = left_width;
                w.right_fringe_width = right_width;
                w.fringes_outside_margins = outside_margins;

                L.adjust_window_margins(w);

                L.clear_glyph_matrix(w.current_matrix);
                w.window_end_valid = Q.nil;

                ++L.windows_or_buffers_changed;
                L.adjust_glyphs(L.XFRAME(L.WINDOW_FRAME(w)));
            }

            return Q.nil;
        }

        public static LispObject set_window_scroll_bars(LispObject window, LispObject width, LispObject vertical_type, LispObject horizontal_type)
        {
            Window w = L.decode_window(window);

            if (!L.NILP(width))
            {
                L.CHECK_NATNUM(width);

                if (L.XINT(width) == 0)
                    vertical_type = Q.nil;
            }

            if (!(L.NILP(vertical_type)
              || L.EQ(vertical_type, Q.left)
              || L.EQ(vertical_type, Q.right)
              || L.EQ(vertical_type, Q.t)))
                L.error("Invalid type of vertical scroll bar");

            if (!L.EQ(w.scroll_bar_width, width)
                || !L.EQ(w.vertical_scroll_bar_type, vertical_type))
            {
                w.scroll_bar_width = width;
                w.vertical_scroll_bar_type = vertical_type;

                L.adjust_window_margins(w);

                L.clear_glyph_matrix(w.current_matrix);
                w.window_end_valid = Q.nil;

                ++L.windows_or_buffers_changed;
                L.adjust_glyphs(L.XFRAME(L.WINDOW_FRAME(w)));
            }

            return Q.nil;
        }

        public static LispObject set_window_margins(LispObject window, LispObject left_width, LispObject right_width)
        {
            Window w = L.decode_window(window);

            /* Translate negative or zero widths to nil.
               Margins that are too wide have to be checked elsewhere.  */

            if (!L.NILP(left_width))
            {
                L.CHECK_NUMBER(left_width);
                if (L.XINT(left_width) <= 0)
                    left_width = Q.nil;
            }

            if (!L.NILP(right_width))
            {
                L.CHECK_NUMBER(right_width);
                if (L.XINT(right_width) <= 0)
                    right_width = Q.nil;
            }

            if (!L.EQ(w.left_margin_cols, left_width) || !L.EQ(w.right_margin_cols, right_width))
            {
                w.left_margin_cols = left_width;
                w.right_margin_cols = right_width;

                L.adjust_window_margins(w);

                ++L.windows_or_buffers_changed;
                L.adjust_glyphs(L.XFRAME(L.WINDOW_FRAME(w)));
            }

            return Q.nil;
        }

        public static LispObject set_window_buffer(LispObject window, LispObject buffer_or_name, LispObject keep_margins)
        {
            LispObject tem, buffer;
            Window w = L.decode_window(window);

            window = w;
            buffer = F.get_buffer(buffer_or_name);
            L.CHECK_BUFFER(buffer);
            if (L.NILP(L.XBUFFER(buffer).name))
                L.error("Attempt to display deleted buffer");

            tem = w.buffer;
            if (L.NILP(tem))
                L.error("Window is deleted");
            else if (!L.EQ(tem, Q.t))
            /* w->buffer is t when the window is first being set up.  */
            {
                if (!L.EQ(tem, buffer))
                    if (L.EQ(w.dedicated, Q.t))
                        L.error("Window is dedicated to `%s'", L.SDATA(L.XBUFFER(tem).name));
                    else
                        w.dedicated = Q.nil;

                L.unshow_buffer(w);
            }

            L.set_window_buffer(window, buffer, true, !L.NILP(keep_margins));
            return Q.nil;
        }

        public static LispObject delete_window(LispObject window)
        {
            // COMEBACK WHEN READY
/*
            Frame f;
            if (L.NILP(window))
                window = L.selected_window;
            f = L.XFRAME(L.WINDOW_FRAME(L.XWINDOW(window)));
            L.delete_window(window);

            L.run_window_configuration_change_hook(f);
*/
            return Q.nil;
        }

        public static LispObject get_buffer_window(LispObject buffer_or_name, LispObject frame)
        {
            LispObject buffer;

            if (L.NILP(buffer_or_name))
                buffer = F.current_buffer();
            else
                buffer = F.get_buffer(buffer_or_name);

            if (L.BUFFERP(buffer))
                return L.window_loop(L.window_loop_enum.GET_BUFFER_WINDOW, buffer, true, frame);
            else
                return Q.nil;
        }

        public static LispObject set_window_point(LispObject window, LispObject pos)
        {
            Window w = L.decode_window(window);

            L.CHECK_NUMBER_COERCE_MARKER(ref pos);
            if (w == L.XWINDOW(L.selected_window)
                && L.XBUFFER(w.buffer) == L.current_buffer)
                F.goto_char(pos);
            else
                L.set_marker_restricted(w.pointm, pos, w.buffer);

            /* We have to make sure that redisplay updates the window to show
               the new value of point.  */
            if (!L.EQ(window, L.selected_window))
                ++L.windows_or_buffers_changed;

            return pos;
        }

        public static LispObject save_window_excursion(LispObject args)
        {
            LispObject val;
            int count = L.SPECPDL_INDEX();

            L.record_unwind_protect(F.set_window_configuration,
                       F.current_window_configuration(Q.nil));
            val = F.progn(args);
            return L.unbind_to(count, val);
        }

        public static LispObject set_window_configuration(LispObject configuration)
        {
            // COME BACK WHEN READY
            return null;
        }

        public static LispObject current_window_configuration(LispObject frame)
        {
            // COME BACK WHEN READY
            return null;
        }
    }

    public partial class Q
    {
        public static LispObject window_live_p;
    }

    public partial class V
    {
        /* A list of all windows for use by next_window and Fwindow_list.
           Functions creating or deleting windows should invalidate this cache
           by setting it to nil.  */
        public static LispObject window_list;

        /* Non-nil means that text is inserted before window's markers.  */
        public static LispObject window_point_insertion_type
        {
            get { return Defs.O[(int)Objects.window_point_insertion_type]; }
            set { Defs.O[(int)Objects.window_point_insertion_type] = value; }
        }
    }
}