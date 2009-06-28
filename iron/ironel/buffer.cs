namespace IronElisp
{
    public class BufferText
    {
        /* Actual address of buffer contents.  If REL_ALLOC is defined,
           this address might change when blocks are relocated which can
           e.g. happen when malloc is called.  So, don't pass a pointer
           into a buffer's text to functions that malloc.  */
        // unsigned char *beg; COMEBACK_WHEN_READY!!!

        int gpt;		/* Char pos of gap in buffer.  */
        public int z;		/* Char pos of end of buffer.  */
        int gpt_byte;		/* Byte pos of gap in buffer.  */
        public int z_byte;		/* Byte pos of end of buffer.  */
        int gap_size;		/* Size of buffer's gap.  */
        int modiff;			/* This counts buffer-modification events
                               for this buffer.  It is incremented for
                               each such event, and never otherwise
                               changed.  */
        int chars_modiff;           /* This is modified with character change
                                       events for this buffer.  It is set to
                                       modiff for each such event, and never
                                       otherwise changed.  */
        int save_modiff;		/* Previous value of modiff, as of last
                                   time buffer visited or saved a file.  */

        int overlay_modiff;		/* Counts modifications to overlays.  */

        /* Minimum value of GPT - BEG since last redisplay that finished.  */
        int beg_unchanged;

        /* Minimum value of Z - GPT since last redisplay that finished.  */
        int end_unchanged;

        /* MODIFF as of last redisplay that finished; if it matches MODIFF,
           beg_unchanged and end_unchanged contain no useful information.  */
        int unchanged_modified;

        /* BUF_OVERLAY_MODIFF of current buffer, as of last redisplay that
           finished; if it matches BUF_OVERLAY_MODIFF, beg_unchanged and
           end_unchanged contain no useful information.  */
        int overlay_unchanged_modified;

        /* Properties of this buffer's text.  */
        public Interval intervals;

        /* The markers that refer to this buffer.
           This is actually a single marker ---
           successive elements in its marker `chain'
           are the other markers referring to this buffer.  */
        public LispMarker markers;

        /* Usually 0.  Temporarily set to 1 in decode_coding_gap to
           prevent Fgarbage_collect from shrinking the gap and loosing
           not-yet-decoded bytes.  */
        int inhibit_shrinking;
    }
    
    public class Buffer : LispVectorLike, Indexable<LispObject>
    {
        public enum Offsets
        {
            header_line_format,
            mode_line_format,
            major_mode,
            mode_name,
            abbrev_table,
            abbrev_mode,
            case_fold_search,
            fill_column,
            left_margin,
            tab_width,
            ctl_arrow,
            enable_multibyte_characters,
            buffer_file_coding_system,
            direction_reversed,
            truncate_lines,
            word_wrap,
            buffer_file_type,
            directory,
            auto_fill_function,
            filename,
            file_truename,
            auto_save_file_name,
            read_only,
            backed_up,
            save_length,
            selective_display,
            selective_display_ellipses,
            overwrite_mode,
            display_table,
            left_margin_cols,
            right_margin_cols,
            left_fringe_width,
            right_fringe_width,
            fringes_outside_margins,
            scroll_bar_width,
            vertical_scroll_bar_type,
            indicate_empty_lines,
            indicate_buffer_boundaries,
            fringe_indicator_alist,
            fringe_cursor_alist,
            scroll_up_aggressively,
            scroll_down_aggressively,
            undo_list,
            mark_active,
            cache_long_line_scans,
            point_before_scroll,
            file_format,
            auto_save_file_format,
            invisibility_spec,
            display_count,
            display_time,
            cursor_type,
            extra_line_spacing,
            cursor_in_non_selected_windows,

            SIZE
        }

        LispObject[] symbols = new LispObject[(int) Offsets.SIZE];

        public LispObject this[int index]
        {
            get { return symbols[index]; }
            set { symbols[index] = value; }
        }

        /* Analogous to mode_line_format for the line displayed at the top
           of windows.  Nil means don't display that line.  */
        public LispObject header_line_format
        {
            get {return symbols[ (int) Offsets.header_line_format ]; }
            set {symbols[ (int) Offsets.header_line_format ] = value; }
        }
        
        /* Mode line element that controls format of mode line.  */
        public LispObject mode_line_format
        {
            get {return symbols[ (int) Offsets.mode_line_format ]; }
            set {symbols[ (int) Offsets.mode_line_format ] = value; }
        }
        
        /* Symbol naming major mode (eg, lisp-mode).  */
        public LispObject major_mode
        {
            get {return symbols[ (int) Offsets.major_mode ]; }
            set {symbols[ (int) Offsets.major_mode ] = value; }
        }
        
        /* Pretty name of major mode (eg, "Lisp"). */
        public LispObject mode_name
        {
            get {return symbols[ (int) Offsets.mode_name ]; }
            set {symbols[ (int) Offsets.mode_name ] = value; }
        }
        
        /* This buffer's local abbrev table.  */
        public LispObject abbrev_table
        {
            get {return symbols[ (int) Offsets.abbrev_table ]; }
            set {symbols[ (int) Offsets.abbrev_table ] = value; }
        }
        
        /* non-nil means abbrev mode is on.  Expand abbrevs automatically.  */
        public LispObject abbrev_mode
        {
            get {return symbols[ (int) Offsets.abbrev_mode ]; }
            set {symbols[ (int) Offsets.abbrev_mode ] = value; }
        }
        
        public LispObject case_fold_search
        {
            get {return symbols[ (int) Offsets.case_fold_search ]; }
            set {symbols[ (int) Offsets.case_fold_search ] = value; }
        }
        
        public LispObject fill_column
        {
            get {return symbols[ (int) Offsets.fill_column ]; }
            set {symbols[ (int) Offsets.fill_column ] = value; }
        }
        
        public LispObject left_margin
        {
            get {return symbols[ (int) Offsets.left_margin ]; }
            set {symbols[ (int) Offsets.left_margin ] = value; }
        }
        
        /* tab-width is buffer-local so that redisplay can find it
           in buffers that are not current.  */
        public LispObject tab_width
        {
            get {return symbols[ (int) Offsets.tab_width ]; }
            set {symbols[ (int) Offsets.tab_width ] = value; }
        }
        
        /* Non-nil means display ctl chars with uparrow.  */
        public LispObject ctl_arrow
        {
            get {return symbols[ (int) Offsets.ctl_arrow ]; }
            set {symbols[ (int) Offsets.ctl_arrow ] = value; }
        }
        
        /* Non-nil means the buffer contents are regarded as multi-byte
           form of characters, not a binary code.  */
        public LispObject enable_multibyte_characters
        {
            get {return symbols[ (int) Offsets.enable_multibyte_characters ]; }
            set {symbols[ (int) Offsets.enable_multibyte_characters ] = value; }
        }
        
        /* Coding system to be used for encoding the buffer contents on
           saving.  */
        public LispObject buffer_file_coding_system
        {
            get {return symbols[ (int) Offsets.buffer_file_coding_system ]; }
            set {symbols[ (int) Offsets.buffer_file_coding_system ] = value; }
        }
        
        /* Non-nil means display text from right to left.  */
        public LispObject direction_reversed
        {
            get {return symbols[ (int) Offsets.direction_reversed ]; }
            set {symbols[ (int) Offsets.direction_reversed ] = value; }
        }
        
        /* Non-nil means do not display continuation lines.  */
        public LispObject truncate_lines
        {
            get {return symbols[ (int) Offsets.truncate_lines ]; }
            set {symbols[ (int) Offsets.truncate_lines ] = value; }
        }
        
        /* Non-nil means to use word wrapping when displaying continuation lines.  */
        public LispObject word_wrap
        {
            get {return symbols[ (int) Offsets.word_wrap ]; }
            set {symbols[ (int) Offsets.word_wrap ] = value; }
        }
        
        /* nil: text, t: binary.
           This value is meaningful only on certain operating systems.  */
        /* Actually, we don't need this flag any more because end-of-line
           is handled correctly according to the buffer-file-coding-system
           of the buffer.  Just keeping it for backward compatibility.  */
        public LispObject buffer_file_type
        {
            get {return symbols[ (int) Offsets.buffer_file_type ]; }
            set {symbols[ (int) Offsets.buffer_file_type ] = value; }
        }
        
        /* Dir for expanding relative file names.  */
        public LispObject directory
        {
            get {return symbols[ (int) Offsets.directory ]; }
            set {symbols[ (int) Offsets.directory ] = value; }
        }
        
        /* Function to call when insert space past fill column.  */
        public LispObject auto_fill_function
        {
            get {return symbols[ (int) Offsets.auto_fill_function ]; }
            set {symbols[ (int) Offsets.auto_fill_function ] = value; }
        }
        
        /* The name of the file visited in this buffer, or nil.  */
        public LispObject filename
        {
            get {return symbols[ (int) Offsets.filename ]; }
            set {symbols[ (int) Offsets.filename ] = value; }
        }
        
        /* Truename of the visited file, or nil.  */
        public LispObject file_truename
        {
            get {return symbols[ (int) Offsets.file_truename ]; }
            set {symbols[ (int) Offsets.file_truename ] = value; }
        }
        
        /* File name used for auto-saving this buffer.
           This is not in the  struct buffer_text
           because it's not used in indirect buffers at all.  */
        public LispObject auto_save_file_name
        {
            get {return symbols[ (int) Offsets.auto_save_file_name ]; }
            set {symbols[ (int) Offsets.auto_save_file_name ] = value; }
        }
        
        /* Non-nil if buffer read-only.  */
        public LispObject read_only
        {
            get {return symbols[ (int) Offsets.read_only ]; }
            set {symbols[ (int) Offsets.read_only ] = value; }
        }
        
        /* True if this buffer has been backed up (if you write to the
           visited file and it hasn't been backed up, then a backup will
           be made).  */
        /* This isn't really used by the C code, so could be deleted.  */
        public LispObject backed_up
        {
            get {return symbols[ (int) Offsets.backed_up ]; }
            set {symbols[ (int) Offsets.backed_up ] = value; }
        }
        
        /* Length of file when last read or saved.
           This is not in the  struct buffer_text
           because it's not used in indirect buffers at all.  */
        public LispObject save_length
        {
            get {return symbols[ (int) Offsets.save_length ]; }
            set {symbols[ (int) Offsets.save_length ] = value; }
        }
        
        /* Non-nil means do selective display;
           see doc string in syms_of_buffer (buffer.c) for details.  */
        public LispObject selective_display
        {
            get {return symbols[ (int) Offsets.selective_display ]; }
            set {symbols[ (int) Offsets.selective_display ] = value; }
        }
        
        /* Non-nil means show ... at end of line followed by invisible lines.  */
        public LispObject selective_display_ellipses
        {
            get {return symbols[ (int) Offsets.selective_display_ellipses ]; }
            set {symbols[ (int) Offsets.selective_display_ellipses ] = value; }
        }
        
        /* t if "self-insertion" should overwrite; `binary' if it should also
           overwrite newlines and tabs - for editing executables and the like.  */
        public LispObject overwrite_mode
        {
            get {return symbols[ (int) Offsets.overwrite_mode ]; }
            set {symbols[ (int) Offsets.overwrite_mode ] = value; }
        }
        
        /* Display table to use for text in this buffer.  */
        public LispObject display_table
        {
            get {return symbols[ (int) Offsets.display_table]; }
            set {symbols[ (int) Offsets.display_table] = value; }
        }
        
        /* Widths of left and right marginal areas for windows displaying
           this buffer.  */
        public LispObject left_margin_cols
        {
            get {return symbols[ (int) Offsets.left_margin_cols ]; }
            set {symbols[ (int) Offsets.left_margin_cols ] = value; }
        }
        
        public LispObject right_margin_cols
        {
            get {return symbols[ (int) Offsets.right_margin_cols ]; }
            set {symbols[ (int) Offsets.right_margin_cols ] = value; }
        }
        
        /* Widths of left and right fringe areas for windows displaying
           this buffer.  */
        public LispObject left_fringe_width
        {
            get {return symbols[ (int) Offsets.left_fringe_width ]; }
            set {symbols[ (int) Offsets.left_fringe_width ] = value; }
        }
        
        public LispObject right_fringe_width
        {
            get {return symbols[ (int) Offsets.right_fringe_width ]; }
            set {symbols[ (int) Offsets.right_fringe_width ] = value; }
        }
        
        /* Non-nil means fringes are drawn outside display margins;
           othersize draw them between margin areas and text.  */
        public LispObject fringes_outside_margins
        {
            get {return symbols[ (int) Offsets.fringes_outside_margins ]; }
            set {symbols[ (int) Offsets.fringes_outside_margins ] = value; }
        }
        
        /* Width and type of scroll bar areas for windows displaying
           this buffer.  */
        public LispObject scroll_bar_width
        {
            get {return symbols[ (int) Offsets.scroll_bar_width ]; }
            set {symbols[ (int) Offsets.scroll_bar_width ] = value; }
        }
        
        public LispObject vertical_scroll_bar_type
        {
            get {return symbols[ (int) Offsets.vertical_scroll_bar_type ]; }
            set {symbols[ (int) Offsets.vertical_scroll_bar_type ] = value; }
        }
        
        /* Non-nil means indicate lines not displaying text (in a style
           like vi).  */
        public LispObject indicate_empty_lines
        {
            get {return symbols[ (int) Offsets.indicate_empty_lines ]; }
            set {symbols[ (int) Offsets.indicate_empty_lines ] = value; }
        }
        
        /* Non-nil means indicate buffer boundaries and scrolling.  */
        public LispObject indicate_buffer_boundaries
        {
            get {return symbols[ (int) Offsets.indicate_buffer_boundaries ]; }
            set {symbols[ (int) Offsets.indicate_buffer_boundaries ] = value; }
        }
        
        /* Logical to physical fringe bitmap mappings.  */
        public LispObject fringe_indicator_alist
        {
            get {return symbols[ (int) Offsets.fringe_indicator_alist ]; }
            set {symbols[ (int) Offsets.fringe_indicator_alist ] = value; }
        }
        
        /* Logical to physical cursor bitmap mappings.  */
        public LispObject fringe_cursor_alist
        {
            get {return symbols[ (int) Offsets.fringe_cursor_alist ]; }
            set {symbols[ (int) Offsets.fringe_cursor_alist ] = value; }
        }
        
        /* If scrolling the display because point is below the bottom of a
           window showing this buffer, try to choose a window start so
           that point ends up this number of lines from the top of the
           window.  Nil means that scrolling method isn't used.  */
        public LispObject scroll_up_aggressively
        {
            get {return symbols[ (int) Offsets.scroll_up_aggressively ]; }
            set {symbols[ (int) Offsets.scroll_up_aggressively ] = value; }
        }
        
        /* If scrolling the display because point is above the top of a
           window showing this buffer, try to choose a window start so
           that point ends up this number of lines from the bottom of the
           window.  Nil means that scrolling method isn't used.  */
        public LispObject scroll_down_aggressively
        {
            get {return symbols[ (int) Offsets.scroll_down_aggressively ]; }
            set {symbols[ (int) Offsets.scroll_down_aggressively ] = value; }
        }
        
        /* Changes in the buffer are recorded here for undo.
           t means don't record anything.
           This information belongs to the base buffer of an indirect buffer,
           But we can't store it in the  struct buffer_text
           because local variables have to be right in the  struct buffer.
           So we copy it around in set_buffer_internal.
           This comes before `name' because it is marked in a special way.  */
        public LispObject undo_list
        {
            get {return symbols[ (int) Offsets.undo_list ]; }
            set {symbols[ (int) Offsets.undo_list ] = value; }
        }
        
        /* t means the mark and region are currently active.  */
        public LispObject mark_active
        {
            get {return symbols[ (int) Offsets.mark_active ]; }
            set {symbols[ (int) Offsets.mark_active ] = value; }
        }
        
        /* True if the newline position cache and width run cache are
           enabled.  See search.c and indent.c.  */
        public LispObject cache_long_line_scans
        {
            get {return symbols[ (int) Offsets.cache_long_line_scans ]; }
            set {symbols[ (int) Offsets.cache_long_line_scans ] = value; }
        }
        
        /* This holds the point value before the last scroll operation.
           Explicitly setting point sets this to nil.  */
        public LispObject point_before_scroll
        {
            get {return symbols[ (int) Offsets.point_before_scroll ]; }
            set {symbols[ (int) Offsets.point_before_scroll ] = value; }
        }
        
        /* List of symbols naming the file format used for visited file.  */
        public LispObject file_format
        {
            get {return symbols[ (int) Offsets.file_format ]; }
            set {symbols[ (int) Offsets.file_format ] = value; }
        }
        
        /* List of symbols naming the file format used for auto-save file.  */
        public LispObject auto_save_file_format
        {
            get {return symbols[ (int) Offsets.auto_save_file_format ]; }
            set {symbols[ (int) Offsets.auto_save_file_format ] = value; }
        }
        
        /* Invisibility spec of this buffer.
           t => any non-nil `invisible' property means invisible.
           A list => `invisible' property means invisible
           if it is memq in that list.  */
        public LispObject invisibility_spec
        {
            get {return symbols[ (int) Offsets.invisibility_spec ]; }
            set {symbols[ (int) Offsets.invisibility_spec ] = value; }
        }
        
        /* Incremented each time the buffer is displayed in a window.  */
        public LispObject display_count
        {
            get {return symbols[ (int) Offsets.display_count ]; }
            set {symbols[ (int) Offsets.display_count ] = value; }
        }
        
        /* Time stamp updated each time this buffer is displayed in a window.  */
        public LispObject display_time
        {
            get {return symbols[ (int) Offsets.display_time ]; }
            set {symbols[ (int) Offsets.display_time ] = value; }
        }
        
        /* Desired cursor type in this buffer.  See the doc string of
           per-buffer variable `cursor-type'.  */
        public LispObject cursor_type
        {
            get {return symbols[ (int) Offsets.cursor_type ]; }
            set {symbols[ (int) Offsets.cursor_type ] = value; }
        }
        
        /* An integer > 0 means put that number of pixels below text lines
           in the display of this buffer.  */
        public LispObject extra_line_spacing
        {
            get {return symbols[ (int) Offsets.extra_line_spacing ]; }
            set {symbols[ (int) Offsets.extra_line_spacing ] = value; }
        }
        
        /* *Cursor type to display in non-selected windows.
           t means to use hollow box cursor.
           See `cursor-type' for other values.  */
        public LispObject cursor_in_non_selected_windows
        {
            get {return symbols[ (int) Offsets.cursor_in_non_selected_windows ]; }
            set {symbols[ (int) Offsets.cursor_in_non_selected_windows ] = value; }
        }
        
        /* Next buffer, in chain of all buffers including killed buffers.
           This chain is used only for garbage collection, in order to
           collect killed buffers properly.
           Note that vectors and most pseudovectors are all on one chain,
           but buffers are on a separate chain of their own.  */
        public Buffer next;

        /* This structure holds the coordinates of the buffer contents
           in ordinary buffers.  In indirect buffers, this is not used.  */
        BufferText own_text;

        /* This points to the `struct buffer_text' that used for this buffer.
           In an ordinary buffer, this is the own_text field above.
           In an indirect buffer, this is the own_text field of another buffer.  */
        public BufferText text;

        /* Char position of point in buffer.  */
        public int pt;
        /* Byte position of point in buffer.  */
        public int pt_byte;
        /* Char position of beginning of accessible range.  */
        public int begv;
        /* Byte position of beginning of accessible range.  */
        public int begv_byte;
        /* Char position of end of accessible range.  */
        public int zv;
        /* Byte position of end of accessible range.  */
        public int zv_byte;

        /* In an indirect buffer, this points to the base buffer.
           In an ordinary buffer, it is 0.  */
        public Buffer base_buffer;

        /* A non-zero value in slot IDX means that per-buffer variable
           with index IDX has a local value in this buffer.  The index IDX
           for a buffer-local variable is stored in that variable's slot
           in buffer_local_flags as a Lisp integer.  If the index is -1,
           this means the variable is always local in all buffers.  */
        public const int MAX_PER_BUFFER_VARS = 50;
        public sbyte[] local_flags = new sbyte[MAX_PER_BUFFER_VARS];

        /* Set to the modtime of the visited file when read or written.
           -1 means visited file was nonexistent.
           0 means visited file modtime unknown; in no case complain
           about any mismatch on next save attempt.  */
        int modtime;
        /* The value of text->modiff at the last auto-save.  */
        int auto_save_modified;
        /* The value of text->modiff at the last display error.
           Redisplay of this buffer is inhibited until it changes again.  */
        int display_error_modiff;
        /* The time at which we detected a failure to auto-save,
           Or -1 if we didn't have a failure.  */
        int auto_save_failure_time;
        /* Position in buffer at which display started
           the last time this buffer was displayed.  */
        int last_window_start;

        /* Set nonzero whenever the narrowing is changed in this buffer.  */
        int clip_changed;

        /* If the long line scan cache is enabled (i.e. the buffer-local
           variable cache-long-line-scans is non-nil), newline_cache
           points to the newline cache, and width_run_cache points to the
           width run cache.

           The newline cache records which stretches of the buffer are
           known *not* to contain newlines, so that they can be skipped
           quickly when we search for newlines.

           The width run cache records which stretches of the buffer are
           known to contain characters whose widths are all the same.  If
           the width run cache maps a character to a value > 0, that value is
           the character's width; if it maps a character to zero, we don't
           know what its width is.  This allows compute_motion to process
           such regions very quickly, using algebra instead of inspecting
           each character.   See also width_table, below.  */
        region_cache newline_cache;
        region_cache width_run_cache;

        /* Non-zero means don't use redisplay optimizations for
           displaying this buffer.  */
        bool prevent_redisplay_optimizations_p;

        /* List of overlays that end at or before the current center,
           in order of end-position.  */
        LispOverlay overlays_before;

        /* List of overlays that end after  the current center,
           in order of start-position.  */
        LispOverlay overlays_after;

        /* Position where the overlay lists are centered.  */
        int overlay_center;

        /* The name of this buffer.  */
        public LispObject name;

        /* "The mark".  This is a marker which may
           point into this buffer or may point nowhere.  */
        LispObject mark;

        /* Alist of elements (SYMBOL . VALUE-IN-THIS-BUFFER)
           for all per-buffer variables of this buffer.  */
        public LispObject local_var_alist;

        /* Keys that are bound local to this buffer.  */
        LispObject keymap;

        /* This buffer's syntax table.  */
        LispObject syntax_table;
        /* This buffer's category table.  */
        LispObject category_table;

        /* Case table for case-conversion in this buffer.
           This char-table maps each char into its lower-case version.  */
        LispObject downcase_table;
        /* Char-table mapping each char to its upper-case version.  */
        LispObject upcase_table;
        /* Char-table for conversion for case-folding search.  */
        LispObject case_canon_table;
        /* Char-table of equivalences for case-folding search.  */
        LispObject case_eqv_table;

        /* Alist of (FUNCTION . STRING) for each minor mode enabled in buffer.  */
        LispObject minor_modes;

        /* If the width run cache is enabled, this table contains the
           character widths width_run_cache (see above) assumes.  When we
           do a thorough redisplay, we compare this against the buffer's
           current display table to see whether the display table has
           affected the widths of any characters.  If it has, we
           invalidate the width run cache, and re-initialize width_table.  */
        LispObject width_table;

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           PT for this buffer when the buffer is not current.  */
        public LispObject pt_marker;

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           BEGV for this buffer when the buffer is not current.  */
        public LispObject begv_marker;

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           ZV for this buffer when the buffer is not current.  */
        public LispObject zv_marker;

        /* This is the last window that was selected with this buffer in it,
           or nil if that window no longer displays this buffer.  */
        LispObject last_selected_window;

        /* Marker chain of buffer.  */
        public LispMarker BUF_MARKERS
        {
            get { return text.markers; }
            set { text.markers = value; }
        }
    }

    public partial class L
    {
        /* 1 if the OV is an overlay object.  */
        public static bool OVERLAY_VALID(LispObject OV)
        {
            return OVERLAYP (OV);
        }

        /* Return the marker that stands for where OV starts in the buffer.  */
        public static LispObject OVERLAY_START(LispObject OV)
        {
            return XOVERLAY(OV).start;
        }

        /* Return the marker that stands for where OV ends in the buffer.  */
        public static LispObject OVERLAY_END(LispObject OV)
        {
            return XOVERLAY(OV).end;
        }

        /* Return the plist of overlay OV.  */
        public static LispObject OVERLAY_PLIST(LispObject OV)
        {
            return XOVERLAY (OV).plist;
        }

        /* Return the actual buffer position for the marker P.
           We assume you know which buffer it's pointing into.  */
        public static int OVERLAY_POSITION(LispObject P)
        {
            if (MARKERP (P))
            {
                return marker_position (P);
            }
            else
            {
                abort ();
                return 0;
            }
        }
        
        /* Position of beginning of buffer.  */
        public static int BEG()
        {
            return 1;
        }

        public static int BEG_BYTE()
        {
            return BEG();
        }
        /* Position of beginning of buffer.  */
        public static int BUF_BEG(Buffer buf)
        {
            return BEG();
        }
        public static int BUF_BEG_BYTE(Buffer buf)
        {
            return BEG_BYTE();
        }

        /* Position of beginning of accessible range of buffer.  */
        public static int BUF_BEGV(Buffer buf)
        {
            return buf.begv;
        }
        public static int BUF_BEGV_BYTE(Buffer buf)
        {
            return buf.begv_byte;
        }

        /* Position of point in buffer.  */
        public static int BUF_PT(Buffer buf)
        {
            return buf.pt;
        }
        public static int BUF_PT_BYTE(Buffer buf)
        {
            return buf.pt_byte;
        }

        /* Position of end of accessible range of buffer.  */
        public static int BUF_ZV(Buffer buf)
        {
            return buf.zv;
        }
        public static int BUF_ZV_BYTE(Buffer buf)
        {
            return buf.zv_byte;
        }

        /* Position of end of buffer.  */
        public static int BUF_Z(Buffer buf)
        {
            return buf.text.z;
        }
        public static int BUF_Z_BYTE(Buffer buf)
        {
            return buf.text.z_byte;
        }
    }

    public partial class L
    {
        /* This structure marks which slots in a buffer have corresponding
           default values in buffer_defaults.
           Each such slot has a nonzero value in this structure.
           The value has only one nonzero bit.

           When a buffer has its own local value for a slot,
           the entry for that slot (found in the same slot in this structure)
           is turned on in the buffer's local_flags array.

           If a slot in this structure is -1, then even though there may
           be a DEFVAR_PER_BUFFER for the slot, there is no default value for it;
           and the corresponding slot in buffer_defaults is not used.

           If a slot is -2, then there is no DEFVAR_PER_BUFFER for it,
           but there is a default value which is copied into each buffer.

           If a slot in this structure corresponding to a DEFVAR_PER_BUFFER is
           zero, that is a bug */
        public static int[] buffer_local_flags = new int[(int)Buffer.Offsets.SIZE];
        public static Buffer current_buffer;

        /* First buffer in chain of all buffers (in reverse order of creation).
           Threaded through ->next.  */
        public static Buffer all_buffers;

        /* This structure holds the default values of the buffer-local variables
           defined with DEFVAR_PER_BUFFER, that have special slots in each buffer.
           The default value occupies the same slot in this structure
           as an individual buffer's value occupies in that buffer.
           Setting the default value also goes through the alist of buffers
           and stores into each buffer that does not say it has a local value.  */
        public static Buffer buffer_defaults = new Buffer();

        // Number of per-buffer variables used.
        public static int last_per_buffer_idx;


        /* Somebody has tried to store a value with an unacceptable type
           in the slot with offset OFFSET.  */
        public static void buffer_slot_type_mismatch(LispObject newval, System.Type type)
        {
            LispObject predicate;

            if (type == typeof(LispInt))
                predicate = Q.integerp;
            else if (type == typeof(LispString))
                predicate = Q.stringp;
            else if (type == typeof(LispSymbol))
                predicate = Q.symbolp;
            else
            {
                abort();
                return;
            }

            wrong_type_argument(predicate, newval);
        }

        /* Value is non-zero if the variable with index IDX has a local value
           in buffer B.  */
        public static bool PER_BUFFER_VALUE_P(Buffer B, int IDX)
        {
            if (IDX < 0 || IDX >= last_per_buffer_idx) 
            {
                abort ();
                return false;
            }
            else
            {
                return B.local_flags[IDX] != 0; 
            }
        }

        /* Set whether per-buffer variable with index IDX has a buffer-local
           value in buffer B.  VAL zero means it hasn't.  */
        public static void SET_PER_BUFFER_VALUE_P(Buffer B, int IDX, sbyte VAL)
        {
            if (IDX < 0 || IDX >= last_per_buffer_idx)	
                abort ();
            B.local_flags[IDX] = VAL;
        }

        /* Return the index value of the per-buffer variable at offset OFFSET
           in the buffer structure.

           If the slot OFFSET has a corresponding default value in
           buffer_defaults, the index value is positive and has only one
           nonzero bit.  When a buffer has its own local value for a slot, the
           bit for that slot (found in the same slot in this structure) is
           turned on in the buffer's local_flags array.

           If the index value is -1, even though there may be a
           DEFVAR_PER_BUFFER for the slot, there is no default value for it;
           and the corresponding slot in buffer_defaults is not used.

           If the index value is -2, then there is no DEFVAR_PER_BUFFER for
           the slot, but there is a default value which is copied into each
           new buffer.

           If a slot in this structure corresponding to a DEFVAR_PER_BUFFER is
           zero, that is a bug */
        public static int PER_BUFFER_IDX(int OFFSET) 
        {
            return buffer_local_flags[OFFSET];
        }

        public static LispObject PER_BUFFER_VALUE(Buffer buffer, int offset)
        {
           return buffer[offset];
        }

        public static void nsberror(LispObject spec)
        {
            if (STRINGP(spec))
                error("No buffer named %s", SDATA(spec));
            error("Invalid buffer argument");
        }

        /* Like Fassoc, but use Fstring_equal to compare
           (which ignores text properties),
           and don't ever QUIT.  */
        public static LispObject assoc_ignore_text_properties(LispObject key, LispObject list)
        {
            for (LispObject tail = list; CONSP(tail); tail = XCDR(tail))
            {
                LispObject elt = XCAR(tail);
                LispObject tem = F.string_equal(F.car(elt), key);
                if (!NILP(tem))
                    return elt;
            }
            return Q.nil;
        }

        /* Set the current buffer to B.

           We previously set windows_or_buffers_changed here to invalidate
           global unchanged information in beg_unchanged and end_unchanged.
           This is no longer necessary because we now compute unchanged
           information on a buffer-basis.  Every action affecting other
           windows than the selected one requires a select_window at some
           time, and that increments windows_or_buffers_changed.  */
        public static void set_buffer_internal (Buffer b)
        {
            if (current_buffer != b)
                set_buffer_internal_1 (b);
        }

        /* Set the current buffer to B, and do not set windows_or_buffers_changed.
           This is used by redisplay.  */
        public static void set_buffer_internal_1(Buffer b)
        {
            Buffer old_buf;
            LispObject tail, valcontents;
            LispObject tem;

            if (current_buffer == b)
                return;

            old_buf = current_buffer;
            current_buffer = b;
            last_known_column_point = -1;   /* invalidate indentation cache */

            if (old_buf != null)
            {
                /* Put the undo list back in the base buffer, so that it appears
               that an indirect buffer shares the undo list of its base.  */
                if (old_buf.base_buffer != null)
                    old_buf.base_buffer.undo_list = old_buf.undo_list;

                /* If the old current buffer has markers to record PT, BEGV and ZV
               when it is not current, update them now.  */
                if (!NILP(old_buf.pt_marker))
                {
                    LispObject obuf = old_buf;
                    set_marker_both(old_buf.pt_marker, obuf,
                             BUF_PT(old_buf), BUF_PT_BYTE(old_buf));
                }
                if (!NILP(old_buf.begv_marker))
                {
                    LispObject obuf = old_buf;
                    set_marker_both(old_buf.begv_marker, obuf,
                             BUF_BEGV(old_buf), BUF_BEGV_BYTE(old_buf));
                }
                if (!NILP(old_buf.zv_marker))
                {
                    LispObject obuf = old_buf;
                    set_marker_both(old_buf.zv_marker, obuf,
                             BUF_ZV(old_buf), BUF_ZV_BYTE(old_buf));
                }
            }

            /* Get the undo list from the base buffer, so that it appears
               that an indirect buffer shares the undo list of its base.  */
            if (b.base_buffer != null)
                b.undo_list = b.base_buffer.undo_list;

            /* If the new current buffer has markers to record PT, BEGV and ZV
               when it is not current, fetch them now.  */
            if (!NILP(b.pt_marker))
            {
                b.pt = marker_position(b.pt_marker);
                b.pt_byte = marker_byte_position(b.pt_marker);
            }
            if (!NILP(b.begv_marker))
            {
                b.begv = marker_position(b.begv_marker);
                b.begv_byte = marker_byte_position(b.begv_marker);
            }
            if (!NILP(b.zv_marker))
            {
                b.zv = marker_position(b.zv_marker);
                b.zv_byte = marker_byte_position(b.zv_marker);
            }

            /* Look down buffer's list of local Lisp variables
               to find and update any that forward into C variables. */

            for (tail = b.local_var_alist; CONSP(tail); tail = XCDR(tail))
            {
                valcontents = SYMBOL_VALUE(XCAR(XCAR(tail)));
                if ((BUFFER_LOCAL_VALUEP(valcontents)))
                {
                    tem = XBUFFER_LOCAL_VALUE(valcontents).realvalue;
                    if (BOOLFWDP(tem) || INTFWDP(tem) || OBJFWDP(tem))
                        /* Just reference the variable
                             to cause it to become set for this buffer.  */
                        F.symbol_value(XCAR(XCAR(tail)));
                }
            }

            /* Do the same with any others that were local to the previous buffer */

            if (old_buf != null)
                for (tail = old_buf.local_var_alist; CONSP(tail); tail = XCDR(tail))
                {
                    valcontents = SYMBOL_VALUE(XCAR(XCAR(tail)));
                    if ((BUFFER_LOCAL_VALUEP(valcontents)))
                    {
                        tem = XBUFFER_LOCAL_VALUE(valcontents).realvalue;
                        if (BOOLFWDP(tem) || INTFWDP(tem) || OBJFWDP(tem))
                            /* Just reference the variable
                                     to cause it to become set for this buffer.  */
                            F.symbol_value(XCAR(XCAR(tail)));
                    }
                }
        }
    }

    public partial class V
    {
        // Alist of all buffer names vs the buffers.
        public static LispObject buffer_alist;
    }

    public partial class F
    {
        public static LispObject get_buffer(LispObject buffer_or_name)
        {
            if (L.BUFFERP(buffer_or_name))
                return buffer_or_name;

            L.CHECK_STRING(buffer_or_name);

            return F.cdr(L.assoc_ignore_text_properties(buffer_or_name, V.buffer_alist));
        }

        public static LispObject current_buffer()
        {
            return L.current_buffer;
        }

        public static LispObject set_buffer(LispObject buffer_or_name)
        {
            LispObject buffer = F.get_buffer(buffer_or_name);
            if (L.NILP(buffer))
                L.nsberror(buffer_or_name);
            if (L.NILP(L.XBUFFER(buffer).name))
                L.error("Selecting deleted buffer");
            L.set_buffer_internal(L.XBUFFER(buffer));
            return buffer;
        }
    }
}