namespace IronElisp
{
    public enum output_method
    {
        output_initial,
        output_termcap,
        output_x_window,
        output_msdos_raw,
        output_w32,
        output_mac,
        output_ns
    }

    public enum fullscreen_type
    {
        /* Values used as a bit mask, BOTH == WIDTH | HEIGHT.  */
        FULLSCREEN_NONE = 0,
        FULLSCREEN_WIDTH = 1,
        FULLSCREEN_HEIGHT = 2,
        FULLSCREEN_BOTH = 3,
        FULLSCREEN_WAIT = 4
    }

    public enum vertical_scroll_bar_type
    {
        vertical_scroll_bar_none,
        vertical_scroll_bar_left,
        vertical_scroll_bar_right
    }

    enum text_cursor_kinds
    {
        DEFAULT_CURSOR = -2,
        NO_CURSOR = -1,
        FILLED_BOX_CURSOR,
        HOLLOW_BOX_CURSOR,
        BAR_CURSOR,
        HBAR_CURSOR
    }

    public class Frame : LispVectorLike<LispObject>
    {
        public int Size
        {
            get
            {
                throw new System.Exception("Comeback");
            }
        }

        public LispObject this[int index]
        {
            get
            {
                throw new System.Exception("Comeback");
            }

            set
            {
                throw new System.Exception("Comeback");
            }
        }    
        Frame next;

        /* Name of this frame: a Lisp string.  It is used for looking up resources,
           as well as for the title in some cases.  */
        LispObject name;

        /* The name to use for the icon, the last time
           it was refreshed.  nil means not explicitly specified.  */
        LispObject icon_name;

        /* This is the frame title specified explicitly, if any.
           Usually it is nil.  */
        LispObject title;

        /* The frame which should receive keystrokes that occur in this
           frame, or nil if they should go to the frame itself.  This is
           usually nil, but if the frame is minibufferless, we can use this
           to redirect keystrokes to a surrogate minibuffer frame when
           needed.

           Note that a value of nil is different than having the field point
           to the frame itself.  Whenever the Fselect_frame function is used
           to shift from one frame to the other, any redirections to the
           original frame are shifted to the newly selected frame; if
           focus_frame is nil, Fselect_frame will leave it alone.  */
        public LispObject focus_frame;

        /* This frame's root window.  Every frame has one.
           If the frame has only a minibuffer window, this is it.
           Otherwise, if the frame has a minibuffer window, this is its sibling.  */
        public LispObject root_window;

        /* This frame's selected window.
           Each frame has its own window hierarchy
           and one of the windows in it is selected within the frame.
           The selected window of the selected frame is Emacs's selected window.  */
        public LispObject selected_window;

        /* This frame's minibuffer window.
           Most frames have their own minibuffer windows,
           but only the selected frame's minibuffer window
           can actually appear to exist.  */
        public LispObject minibuffer_window;

        /* Parameter alist of this frame.
           These are the parameters specified when creating the frame
           or modified with modify-frame-parameters.  */
        public LispObject param_alist;

        /* List of scroll bars on this frame.
           Actually, we don't specify exactly what is stored here at all; the
           scroll bar implementation code can use it to store anything it likes.
           This field is marked by the garbage collector.  It is here
           instead of in the `device' structure so that the garbage
           collector doesn't need to look inside the window-system-dependent
           structure.  */
        LispObject scroll_bars;
        LispObject condemned_scroll_bars;

        /* Vector describing the items to display in the menu bar.
           Each item has four elements in this vector.
           They are KEY, STRING, SUBMAP, and HPOS.
           (HPOS is not used in when the X toolkit is in use.)
           There are four additional elements of nil at the end, to terminate.  */
        LispObject menu_bar_items;

        /* Alist of elements (FACE-NAME . FACE-VECTOR-DATA).  */
        LispObject face_alist;

        /* A vector that records the entire structure of this frame's menu bar.
           For the format of the data, see extensive comments in xmenu.c.
           Only the X toolkit version uses this.  */
        LispObject menu_bar_vector;

        /* Predicate for selecting buffers for other-buffer.  */
        LispObject buffer_predicate;

        /* List of buffers viewed in this frame, for other-buffer.  */
        LispObject buffer_list;

        /* List of buffers that were viewed, then buried in this frame.  The
           most recently buried buffer is first.  For last-buffer.  */
        LispObject buried_buffer_list;

        /* A dummy window used to display menu bars under X when no X
           toolkit support is available.  */
        LispObject menu_bar_window;

        /* A window used to display the tool-bar of a frame.  */
        LispObject tool_bar_window;

        /* Desired and current tool-bar items.  */
        LispObject tool_bar_items;

        /* Desired and current contents displayed in tool_bar_window.  */
        LispObject desired_tool_bar_string, current_tool_bar_string;

        /* Beyond here, there should be no more Lisp_Object components.  */

        /* Cache of realized faces.  */
        face_cache face_cache;

        /* Number of elements in `menu_bar_vector' that have meaningful data.  */
        int menu_bar_items_used;

        /* A buffer to hold the frame's name.  We can't use the Lisp
           string's pointer (`name', above) because it might get relocated.  */
        string namebuf;

        /* Glyph pool and matrix. */
        glyph_pool current_pool;
        glyph_pool desired_pool;
        glyph_matrix desired_matrix;
        glyph_matrix current_matrix;

        /* 1 means that glyphs on this frame have been initialized so it can
           be used for output.  */
        bool glyphs_initialized_p;

        /* Set to non-zero in change_frame_size when size of frame changed
           Clear the frame in clear_garbaged_frames if set.  */
        bool resized_p;

        /* Set to non-zero in when we want for force a flush_display in
           update_frame, usually after resizing the frame.  */
        bool force_flush_display_p;

        /* Set to non-zero if the default face for the frame has been
           realized.  Reset to zero whenever the default face changes.
           Used to see the difference between a font change and face change.  */
        bool default_face_done_p;

        /* Set to non-zero if this frame has already been hscrolled during
           current redisplay.  */
        bool already_hscrolled_p;

        /* Set to non-zero when current redisplay has updated frame.  */
        bool updated_p;

        /* Set to non-zero to minimize tool-bar height even when
           auto-resize-tool-bar is set to grow-only.  */
        bool minimize_tool_bar_window_p;

        /* Margin at the top of the frame.  Used to display the tool-bar.  */
        int tool_bar_lines;

        int n_tool_bar_rows;
        int n_tool_bar_items;

        /* A buffer for decode_mode_line. */
        byte[] decode_mode_spec_buffer;

        /* See do_line_insertion_deletion_costs for info on these arrays. */
        /* Cost of inserting 1 line on this frame */
        int[] insert_line_cost;
        /* Cost of deleting 1 line on this frame */
        int[] delete_line_cost;
        /* Cost of inserting n lines on this frame */
        int[] insert_n_lines_cost;
        /* Cost of deleting n lines on this frame */
        int[] delete_n_lines_cost;

        /* Size of this frame, excluding fringes, scroll bars etc.,
           in units of canonical characters.  */
        int text_lines, text_cols;

        /* Total size of this frame (i.e. its native window), in units of
           canonical characters.  */
        public int total_lines, total_cols;

        /* New text height and width for pending size change.
           0 if no change pending.  */
        int new_text_lines, new_text_cols;

        /* Pixel position of the frame window (x and y offsets in root window).  */
        int left_pos, top_pos;

        /* Size of the frame window in pixels.  */
        int pixel_height, pixel_width;

        /* Dots per inch of the screen the frame is on.  */
        double resx, resy;

        /* These many pixels are the difference between the outer window (i.e. the
           left and top of the window manager decoration) and FRAME_X_WINDOW. */
        int x_pixels_diff, y_pixels_diff;

        /* This is the gravity value for the specified window position.  */
        int win_gravity;

        /* The geometry flags for this window.  */
        int size_hint_flags;

        /* Border width of the frame window as known by the (X) window system.  */
        int border_width;

        /* Width of the internal border.  This is a line of background color
           just inside the window's border.  When the frame is selected,
           a highlighting is displayed inside the internal border.  */
        public int internal_border_width;

        /* Canonical X unit.  Width of default font, in pixels.  */
        public int column_width;

        /* Widht of space glyph of default font, in pixels.  */
        int space_width;

        /* Canonical Y unit.  Height of a line, in pixels.  */
        public int line_height;

        /* The output method says how the contents of this frame are
           displayed.  It could be using termcap, or using an X window.
           This must be the same as the terminal->type. */
        public output_method output_method;

        /* The terminal device that this frame uses.  If this is NULL, then
           the frame has been deleted. */
        public Terminal terminal;

        /* Device-dependent, frame-local auxiliary data used for displaying
           the contents.  When the frame is deleted, this data is deleted as
           well. */
        w32_output output_data;

        /* List of font-drivers available on the frame. */
        font_driver_list font_driver_list;
        /* List of data specific to font-driver and frame, but common to
           faces.  */
        font_data_list font_data_list;

        /* Total width of fringes reserved for drawing truncation bitmaps,
           continuation bitmaps and alike.  The width is in canonical char
           units of the frame.  This must currently be the case because window
           sizes aren't pixel values.  If it weren't the case, we wouldn't be
           able to split windows horizontally nicely.  */
        public int fringe_cols;

        /* The extra width (in pixels) currently allotted for fringes.  */
        public int left_fringe_width, right_fringe_width;

        /* See FULLSCREEN_ enum below */
        fullscreen_type want_fullscreen;

        /* Number of lines of menu bar.  */
        int menu_bar_lines;

        bool external_menu_bar;

        /* Nonzero if last attempt at redisplay on this frame was preempted.  */
        bool display_preempted;

        /* visible is nonzero if the frame is currently displayed; we check
           it to see if we should bother updating the frame's contents.
           DON'T SET IT DIRECTLY; instead, use FRAME_SET_VISIBLE.

           Note that, since invisible frames aren't updated, whenever a
           frame becomes visible again, it must be marked as garbaged.  The
           FRAME_SAMPLE_VISIBILITY macro takes care of this.

           On ttys and on Windows NT/9X, to avoid wasting effort updating
           visible frames that are actually completely obscured by other
           windows on the display, we bend the meaning of visible slightly:
           if greater than 1, then the frame is obscured - we still consider
           it to be "visible" as seen from lisp, but we don't bother
           updating it.  We must take care to garbage the frame when it
           ceaces to be obscured though.

           iconified is nonzero if the frame is currently iconified.

           Asynchronous input handlers should NOT change these directly;
           instead, they should change async_visible or async_iconified, and
           let the FRAME_SAMPLE_VISIBILITY macro set visible and iconified
           at the next redisplay.

           These should probably be considered read-only by everyone except
           FRAME_SAMPLE_VISIBILITY.

           These two are mutually exclusive.  They might both be zero, if the
           frame has been made invisible without an icon.  */
        public byte visible;
        public byte iconified;

        /* Let's not use bitfields for volatile variables.  */

        /* Asynchronous input handlers change these, and
           FRAME_SAMPLE_VISIBILITY copies them into visible and iconified.
           See FRAME_SAMPLE_VISIBILITY, below.  */
        public volatile byte async_visible, async_iconified;

        /* Nonzero if this frame should be redrawn.  */
        public volatile byte garbaged;

        /* True if frame actually has a minibuffer window on it.
           0 if using a minibuffer window that isn't on this frame.  */
        bool has_minibuffer;

        /* 0 means, if this frame has just one window,
           show no modeline for that window.  */
        public bool wants_modeline;

        /* Non-zero if the hardware device this frame is displaying on can
           support scroll bars.  */
        public bool can_have_scroll_bars;

        /* Non-0 means raise this frame to the top of the heap when selected.  */
        bool auto_raise;

        /* Non-0 means lower this frame to the bottom of the stack when left.  */
        bool auto_lower;

        /* True if frame's root window can't be split.  */
        bool no_split;

        /* If this is set, then Emacs won't change the frame name to indicate
           the current buffer, etcetera.  If the user explicitly sets the frame
           name, this gets set.  If the user sets the name to Qnil, this is
           cleared.  */
        bool explicit_name;

        /* Nonzero if size of some window on this frame has changed.  */
        bool window_sizes_changed;

        /* Nonzero if the mouse has moved on this display device
           since the last time we checked.  */
        bool mouse_moved;

        /* If can_have_scroll_bars is non-zero, this is non-zero if we should
           actually display them on this frame.  */
        public vertical_scroll_bar_type vertical_scroll_bar_type;

        /* What kind of text cursor should we draw in the future?
           This should always be filled_box_cursor or bar_cursor.  */
        text_cursor_kinds desired_cursor;

        /* Width of bar cursor (if we are using that).  */
        int cursor_width;

        /* What kind of text cursor should we draw when the cursor blinks off?
           This can be filled_box_cursor or bar_cursor or no_cursor.  */
        text_cursor_kinds blink_off_cursor;

        /* Width of bar cursor (if we are using that) for blink-off state.  */
        int blink_off_cursor_width;

        /* Storage for messages to this frame. */
        byte[] message_buf;

        /* Nonnegative if current redisplay should not do scroll computation
           for lines beyond a certain vpos.  This is the vpos.  */
        int scroll_bottom_vpos;

        /* Configured width of the scroll bar, in pixels and in characters.
           config_scroll_bar_cols tracks config_scroll_bar_width if the
           latter is positive; a zero value in config_scroll_bar_width means
           to compute the actual width on the fly, using config_scroll_bar_cols
           and the current font width.  */
        public int config_scroll_bar_width;
        public int config_scroll_bar_cols;

        /* The size of the extra width currently allotted for vertical
           scroll bars in this frame, in pixels.  */
        int scroll_bar_actual_width;

        /* The baud rate that was used to calculate costs for this frame.  */
        int cost_calculation_baud_rate;

        /* frame opacity
           alpha[0]: alpha transparency of the active frame
           alpha[1]: alpha transparency of inactive frames
           Negative values mean not to change alpha.  */
        double[] alpha = new double[2];

        /* Exponent for gamma correction of colors.  1/(VIEWING_GAMMA *
           SCREEN_GAMMA) where viewing_gamma is 0.4545 and SCREEN_GAMMA is a
           frame parameter.  0 means don't do gamma correction.  */
        double gamma;

        /* Additional space to put between text lines on this frame.  */
        int extra_line_spacing;

        /* All display backends seem to need these two pixel values. */
        long background_pixel;
        long foreground_pixel;
    }

    public partial class L
    {
        /* Nonzero if frame F is still alive (not deleted).  */
        public static bool FRAME_LIVE_P(Frame f)
        {
            return f.terminal != null;
        }

        public static Frame SELECTED_FRAME()
        {
            if (FRAMEP(selected_frame) && FRAME_LIVE_P(XFRAME(selected_frame)))
                return XFRAME(selected_frame);

            abort();
            return null;
        }

        public static kboard FRAME_KBOARD(Frame f)
        {
            return f.terminal.kboard;
        }

        /* Given a window, return its frame as a Lisp_Object.  */
        public static LispObject WINDOW_FRAME(Window w)
        {
            return w.frame;
        }

        /* Canonical y-unit on frame F.
           This value currently equals the line height of the frame (which is
           the height of the default font of F).  */
        public static int FRAME_LINE_HEIGHT(Frame F)
        {
            return (F.line_height);
        }

        /* Canonical x-unit on frame F.
           This value currently equals the average width of the default font of F.  */
        public static int FRAME_COLUMN_WIDTH(Frame F)
        {
            return (F.column_width);
        }

        /* Pixel-width of internal border lines */
        public static int FRAME_INTERNAL_BORDER_WIDTH(Frame F)
        {
            return (F.internal_border_width);
        }

        /* Nonzero if frame F supports scroll bars.
           If this is zero, then it is impossible to enable scroll bars
           on frame F.  */
        public static bool FRAME_CAN_HAVE_SCROLL_BARS(Frame f)
        {
            return (f.can_have_scroll_bars);
        }

        /* This frame slot says whether scroll bars are currently enabled for frame F,
           and which side they are on.  */
        public static vertical_scroll_bar_type FRAME_VERTICAL_SCROLL_BAR_TYPE(Frame f)
        {
            return (f.vertical_scroll_bar_type);
        }
        public static bool FRAME_HAS_VERTICAL_SCROLL_BARS(Frame f)
        {
            return (f.vertical_scroll_bar_type != vertical_scroll_bar_type.vertical_scroll_bar_none);
        }
        public static bool FRAME_HAS_VERTICAL_SCROLL_BARS_ON_LEFT(Frame f)
        {
            return (f.vertical_scroll_bar_type == vertical_scroll_bar_type.vertical_scroll_bar_left);
        }
        public static bool FRAME_HAS_VERTICAL_SCROLL_BARS_ON_RIGHT(Frame f)
        {
            return (f.vertical_scroll_bar_type == vertical_scroll_bar_type.vertical_scroll_bar_right);
        }

        /* Width that a scroll bar in frame F should have, if there is one.
           Measured in pixels.
           If scroll bars are turned off, this is still nonzero.  */
        public static int FRAME_CONFIG_SCROLL_BAR_WIDTH(Frame f)
        {
            return (f.config_scroll_bar_width);
        }

        /* Width that a scroll bar in frame F should have, if there is one.
           Measured in columns (characters).
           If scroll bars are turned off, this is still nonzero.  */
        public static int FRAME_CONFIG_SCROLL_BAR_COLS(Frame f)
        {
            return (f.config_scroll_bar_cols);
        }

        /* Width of a scroll bar in frame F, measured in columns (characters),
           but only if scroll bars are on the left.  If scroll bars are on
           the right in this frame, or there are no scroll bars, value is 0.  */
        public static int FRAME_LEFT_SCROLL_BAR_COLS(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS_ON_LEFT(f)
             ? FRAME_CONFIG_SCROLL_BAR_COLS(f) : 0);
        }

        /* Width of a left scroll bar in frame F, measured in pixels */
        public static int FRAME_LEFT_SCROLL_BAR_AREA_WIDTH(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS_ON_LEFT(f)
             ? (FRAME_CONFIG_SCROLL_BAR_COLS(f) * FRAME_COLUMN_WIDTH(f))
             : 0);
        }

        /* Width of a scroll bar in frame F, measured in columns (characters),
           but only if scroll bars are on the right.  If scroll bars are on
           the left in this frame, or there are no scroll bars, value is 0.  */
        public static int FRAME_RIGHT_SCROLL_BAR_COLS(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS_ON_RIGHT(f)
             ? FRAME_CONFIG_SCROLL_BAR_COLS(f)
             : 0);
        }

        /* Width of a right scroll bar area in frame F, measured in pixels */
        public static int FRAME_RIGHT_SCROLL_BAR_AREA_WIDTH(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS_ON_RIGHT(f)
             ? (FRAME_CONFIG_SCROLL_BAR_COLS(f) * FRAME_COLUMN_WIDTH(f))
             : 0);
        }

        /* Actual width of a scroll bar in frame F, measured in columns.  */
        public static int FRAME_SCROLL_BAR_COLS(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS(f)
             ? FRAME_CONFIG_SCROLL_BAR_COLS(f)
             : 0);
        }

        /* Actual width of a scroll bar area in frame F, measured in pixels.  */
        public static int FRAME_SCROLL_BAR_AREA_WIDTH(Frame f)
        {
            return (FRAME_HAS_VERTICAL_SCROLL_BARS(f)
             ? (FRAME_CONFIG_SCROLL_BAR_COLS(f) * FRAME_COLUMN_WIDTH(f))
             : 0);
        }

        /* Total width of frame F, in columns (characters),
           including the width used by scroll bars if any.  */
        public static int FRAME_TOTAL_COLS(Frame f)
        {
            return (f.total_cols);
        }

        /* Total width of fringes reserved for drawing truncation bitmaps,
           continuation bitmaps and alike.  The width is in canonical char
           units of the frame.  This must currently be the case because window
           sizes aren't pixel values.  If it weren't the case, we wouldn't be
           able to split windows horizontally nicely.  */
        public static int FRAME_FRINGE_COLS(Frame F)
        {
            return (F.fringe_cols);
        }

        /* Pixel-width of the left and right fringe.  */
        public static int FRAME_LEFT_FRINGE_WIDTH(Frame F)
        {
            return (F.left_fringe_width);
        }
        public static int FRAME_RIGHT_FRINGE_WIDTH(Frame F)
        {
            return (F.right_fringe_width);
        }

        /* Not really implemented.  */
        public static bool FRAME_WANTS_MODELINE_P(Frame f)
        {
            return f.wants_modeline;
        }

        /* The currently selected window of the window tree of frame F.  */
        public static LispObject FRAME_SELECTED_WINDOW(Frame f)
        {
            return f.selected_window;
        }

        /* The root window of the window tree of frame F.  */
        public static LispObject FRAME_ROOT_WINDOW(Frame f)
        {
            return f.root_window;
        }

        /* Emacs's redisplay code could become confused if a frame's
           visibility changes at arbitrary times.  For example, if a frame is
           visible while the desired glyphs are being built, but becomes
           invisible before they are updated, then some rows of the
           desired_glyphs will be left marked as enabled after redisplay is
           complete, which should never happen.  The next time the frame
           becomes visible, redisplay will probably barf.

           Currently, there are no similar situations involving iconified, but
           the principle is the same.

           So instead of having asynchronous input handlers directly set and
           clear the frame's visibility and iconification flags, they just set
           the async_visible and async_iconified flags; the redisplay code
           calls the FRAME_SAMPLE_VISIBILITY macro before doing any redisplay,
           which sets visible and iconified from their asynchronous
           counterparts.

           Synchronous code must use the FRAME_SET_VISIBLE macro.

           Also, if a frame used to be invisible, but has just become visible,
           it must be marked as garbaged, since redisplay hasn't been keeping
           up its contents.
        
           Note that a tty frame is visible if and only if it is the topmost
           frame. */
        public static void FRAME_SAMPLE_VISIBILITY(Frame f)
        {
            if (f.async_visible != 0 && f.visible != f.async_visible)
            {
                SET_FRAME_GARBAGED(f);
            }
            else
            {
                f.visible = f.async_visible;
                f.iconified = f.async_iconified;
            }
        }

        public static void SET_FRAME_GARBAGED(Frame f)
        {
            frame_garbaged = 1;
            f.garbaged = 1;
        }

        public static bool FRAME_VISIBLE_P(Frame f)
        {
            return f.visible != 0;
        }

        /* Nonzero if frame F is currently iconified.  */
        public static bool FRAME_ICONIFIED_P(Frame f)
        {
            return f.iconified != 0;
        }

        /* The minibuffer window of frame F, if it has one; otherwise nil.  */
        public static LispObject FRAME_MINIBUF_WINDOW(Frame f)
        {
            return f.minibuffer_window;
        }

        public static LispObject FRAME_FOCUS_FRAME(Frame f)
        {
            return f.focus_frame;
        }

        /* Return 1 if it is ok to delete frame F;
           0 if all frames aside from F are invisible.
           (Exception: if F is the terminal frame, and we are using X, return 1.)  */
        public static bool other_visible_frames(Frame f)
        {
            /* We know the selected frame is visible,
               so if F is some other frame, it can't be the sole visible one.  */
            if (f == SELECTED_FRAME())
            {
                LispObject frames;
                int count = 0;

                for (frames = V.frame_list;
                 CONSP(frames);
                 frames = XCDR(frames))
                {
                    LispObject thiss;

                    thiss = XCAR(frames);
                    /* Verify that the frame's window still exists
                       and we can still talk to it.  And note any recent change
                       in visibility.  */
//#if HAVE_WINDOW_SYSTEM
                    if (FRAME_WINDOW_P(XFRAME(thiss)))
                    {
                        // x_sync(XFRAME(thiss));
                        FRAME_SAMPLE_VISIBILITY(XFRAME(thiss));
                    }
//#endif

                    if (FRAME_VISIBLE_P(XFRAME(thiss))
                        || FRAME_ICONIFIED_P(XFRAME(thiss))
                        /* Allow deleting the terminal frame when at least
                       one X frame exists!  */
                        || (FRAME_WINDOW_P(XFRAME(thiss)) && !FRAME_WINDOW_P(f)))
                        count++;
                }
                return count > 1;
            }
            return true;
        }
        
        public static bool FRAME_W32_P(Frame f)
        {
            return f.output_method == output_method.output_w32;
        }

        /* FRAME_WINDOW_P tests whether the frame is a window, and is
           defined to be the predicate for the window system being used.  */
        public static bool FRAME_WINDOW_P(Frame f)
        {
            return FRAME_W32_P(f);
        }

        /* Delete FRAME.  When FORCE equals Qnoelisp, delete FRAME
          unconditionally.  x_connection_closed and delete_terminal use
          this.  Any other value of FORCE implements the semantics
          described for Fdelete_frame.  */
        public static LispObject delete_frame(LispObject frame, LispObject force)
        {
            // COME BACK WHEN READY
            return null;
        }
    }

    public partial class Q
    {
        public static LispObject left, right;
        public static LispObject visible;
    }

    public partial class F
    {
        public static LispObject window_frame(LispObject window)
        {
            L.CHECK_LIVE_WINDOW(window);
            return L.XWINDOW(window).frame;
        }
    }

    public partial class V
    {
        public static LispObject frame_list;
    }
}