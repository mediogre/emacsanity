namespace IronElisp
{
    public class BufferText
    {
        /* Actual address of buffer contents.  If REL_ALLOC is defined,
           this address might change when blocks are relocated which can
           e.g. happen when malloc is called.  So, don't pass a pointer
           into a buffer's text to functions that malloc.  */
        public byte[] beg;

        public int gpt;		/* Char pos of gap in buffer.  */
        public int z;		/* Char pos of end of buffer.  */
        public int gpt_byte;		/* Byte pos of gap in buffer.  */
        public int z_byte;		/* Byte pos of end of buffer.  */
        public int gap_size;		/* Size of buffer's gap.  */
        public int modiff;			/* This counts buffer-modification events
                               for this buffer.  It is incremented for
                               each such event, and never otherwise
                               changed.  */
        public int chars_modiff;           /* This is modified with character change
                                       events for this buffer.  It is set to
                                       modiff for each such event, and never
                                       otherwise changed.  */
        public int save_modiff;		/* Previous value of modiff, as of last
                                   time buffer visited or saved a file.  */

        public int overlay_modiff;		/* Counts modifications to overlays.  */

        /* Minimum value of GPT - BEG since last redisplay that finished.  */
        public int beg_unchanged;

        /* Minimum value of Z - GPT since last redisplay that finished.  */
        public int end_unchanged;

        /* MODIFF as of last redisplay that finished; if it matches MODIFF,
           beg_unchanged and end_unchanged contain no useful information.  */
        public int unchanged_modified;

        /* BUF_OVERLAY_MODIFF of current buffer, as of last redisplay that
           finished; if it matches BUF_OVERLAY_MODIFF, beg_unchanged and
           end_unchanged contain no useful information.  */
        public int overlay_unchanged_modified;

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
        public int inhibit_shrinking;
    }
    
    public class Buffer : LispVectorLike<LispObject>, Indexable<LispObject>
    {
        public enum Offsets
        {
            undo_list,
            name,
            filename,
            directory,
            backed_up,
            save_length,
            auto_save_file_name,
            read_only,
            mark,
            local_var_alist,
            major_mode,
            mode_name,
            mode_line_format,
            header_line_format,
            keymap,            
            abbrev_table,
            syntax_table,
            category_table,
            case_fold_search,
            tab_width,
            fill_column,
            left_margin,
            auto_fill_function,
            buffer_file_type,
            downcase_table,
            upcase_table,
            case_canon_table,
            case_eqv_table,
            truncate_lines,
            word_wrap,
            ctl_arrow,
            direction_reversed,
            selective_display,
            selective_display_ellipses,
            minor_modes,
            overwrite_mode,
            abbrev_mode,            
            display_table,
            mark_active,
            enable_multibyte_characters,
            buffer_file_coding_system,
            file_format,
            auto_save_file_format,
            cache_long_line_scans,
            width_table,
            pt_marker,
            begv_marker,
            zv_marker,
            point_before_scroll,
            file_truename,
            invisibility_spec, 
            last_selected_window,
            display_count,
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
            display_time,            
            scroll_up_aggressively,
            scroll_down_aggressively,
            cursor_type,
            extra_line_spacing,
            cursor_in_non_selected_windows,

            SIZE
        }

        LispObject[] symbols = new LispObject[(int) Offsets.SIZE];

        public int Size
        {
            get
            {
                return symbols.Length;
            }
        }

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
        public BufferText own_text;

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
        public long modtime;
        /* The value of text->modiff at the last auto-save.  */
        public int auto_save_modified;
        /* The value of text->modiff at the last display error.
           Redisplay of this buffer is inhibited until it changes again.  */
        public int display_error_modiff;
        /* The time at which we detected a failure to auto-save,
           Or -1 if we didn't have a failure.  */
        public int auto_save_failure_time;
        /* Position in buffer at which display started
           the last time this buffer was displayed.  */
        public int last_window_start;

        /* Set nonzero whenever the narrowing is changed in this buffer.  */
        public int clip_changed;

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
        public region_cache newline_cache;
        public region_cache width_run_cache;

        /* Non-zero means don't use redisplay optimizations for
           displaying this buffer.  */
        public bool prevent_redisplay_optimizations_p;

        /* List of overlays that end at or before the current center,
           in order of end-position.  */
        public LispOverlay overlays_before;

        /* List of overlays that end after  the current center,
           in order of start-position.  */
        public LispOverlay overlays_after;

        /* Position where the overlay lists are centered.  */
        public int overlay_center;

        /* The name of this buffer.  */
        public LispObject name
        {
            get {return symbols[ (int) Offsets.name ]; }
            set {symbols[ (int) Offsets.name ] = value; }
        }

        /* "The mark".  This is a marker which may
           point into this buffer or may point nowhere.  */
        public LispObject mark
        {
            get {return symbols[ (int) Offsets.mark ]; }
            set {symbols[ (int) Offsets.mark ] = value; }
        }

        /* Alist of elements (SYMBOL . VALUE-IN-THIS-BUFFER)
           for all per-buffer variables of this buffer.  */
        public LispObject local_var_alist
        {
            get {return symbols[ (int) Offsets.local_var_alist ]; }
            set {symbols[ (int) Offsets.local_var_alist ] = value; }
        }

        /* Keys that are bound local to this buffer.  */
        public LispObject keymap
        {
            get {return symbols[ (int) Offsets.keymap ]; }
            set {symbols[ (int) Offsets.keymap ] = value; }
        }

        /* This buffer's syntax table.  */
        public LispObject syntax_table
        {
            get {return symbols[ (int) Offsets.syntax_table ]; }
            set {symbols[ (int) Offsets.syntax_table ] = value; }
        }
        
        /* This buffer's category table.  */
        public LispObject category_table
        {
            get {return symbols[ (int) Offsets.category_table ]; }
            set {symbols[ (int) Offsets.category_table ] = value; }
        }

        /* Case table for case-conversion in this buffer.
           This char-table maps each char into its lower-case version.  */
        public LispObject downcase_table
        {
            get {return symbols[ (int) Offsets.downcase_table ]; }
            set {symbols[ (int) Offsets.downcase_table ] = value; }
        }
        /* Char-table mapping each char to its upper-case version.  */
        public LispObject upcase_table
        {
            get {return symbols[ (int) Offsets.upcase_table ]; }
            set {symbols[ (int) Offsets.upcase_table ] = value; }
        }
        
        /* Char-table for conversion for case-folding search.  */
        public LispObject case_canon_table
        {
            get {return symbols[ (int) Offsets.case_canon_table ]; }
            set {symbols[ (int) Offsets.case_canon_table ] = value; }
        }
        
        /* Char-table of equivalences for case-folding search.  */
        public LispObject case_eqv_table
        {
            get {return symbols[ (int) Offsets.case_eqv_table ]; }
            set {symbols[ (int) Offsets.case_eqv_table ] = value; }
        }

        /* Alist of (FUNCTION . STRING) for each minor mode enabled in buffer.  */
        public LispObject minor_modes
        {
            get {return symbols[ (int) Offsets.minor_modes ]; }
            set {symbols[ (int) Offsets.minor_modes ] = value; }
        }

        /* If the width run cache is enabled, this table contains the
           character widths width_run_cache (see above) assumes.  When we
           do a thorough redisplay, we compare this against the buffer's
           current display table to see whether the display table has
           affected the widths of any characters.  If it has, we
           invalidate the width run cache, and re-initialize width_table.  */
        public LispObject width_table
        {
            get {return symbols[ (int) Offsets.width_table ]; }
            set {symbols[ (int) Offsets.width_table ] = value; }
        }

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           PT for this buffer when the buffer is not current.  */
        public LispObject pt_marker
        {
            get {return symbols[ (int) Offsets.pt_marker ]; }
            set {symbols[ (int) Offsets.pt_marker ] = value; }
        }

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           BEGV for this buffer when the buffer is not current.  */
        public LispObject begv_marker
        {
            get {return symbols[ (int) Offsets.begv_marker ]; }
            set {symbols[ (int) Offsets.begv_marker ] = value; }
        }

        /* In an indirect buffer, or a buffer that is the base of an
           indirect buffer, this holds a marker that records
           ZV for this buffer when the buffer is not current.  */
        public LispObject zv_marker
        {
            get {return symbols[ (int) Offsets.zv_marker ]; }
            set {symbols[ (int) Offsets.zv_marker ] = value; }
        }

        /* This is the last window that was selected with this buffer in it,
           or nil if that window no longer displays this buffer.  */
        public LispObject last_selected_window
        {
            get {return symbols[ (int) Offsets.last_selected_window ]; }
            set {symbols[ (int) Offsets.last_selected_window ] = value; }
        }

        /* Marker chain of buffer.  */
        public LispMarker BUF_MARKERS
        {
            get { return text.markers; }
            set { text.markers = value; }
        }
    }

    public partial class L
    {
        /* Reset buffer B's local variables info.
           Don't use this on a buffer that has already been in use;
           it does not treat permanent locals consistently.
           Instead, use Fkill_all_local_variables.

           If PERMANENT_TOO is 1, then we reset permanent
           buffer-local variables.  If PERMANENT_TOO is 0,
           we preserve those.  */
        public static void reset_buffer_local_variables(Buffer b, int permanent_too)
        {
            int offset;
            int i;

            /* Reset the major mode to Fundamental, together with all the
               things that depend on the major mode.
               default-major-mode is handled at a higher level.
               We ignore it here.  */
            b.major_mode = Q.fundamental_mode;
            b.keymap = Q.nil;
            b.mode_name = Q.SFundamental;
            b.minor_modes = Q.nil;

            /* If the standard case table has been altered and invalidated,
               fix up its insides first.  */
            if (!(CHAR_TABLE_P(XCHAR_TABLE(V.ascii_downcase_table).extras(0))
               && CHAR_TABLE_P(XCHAR_TABLE(V.ascii_downcase_table).extras(1))
               && CHAR_TABLE_P(XCHAR_TABLE(V.ascii_downcase_table).extras(2))))
                F.set_standard_case_table(V.ascii_downcase_table);

            b.downcase_table = V.ascii_downcase_table;
            b.upcase_table = XCHAR_TABLE(V.ascii_downcase_table).extras(0);
            b.case_canon_table = XCHAR_TABLE(V.ascii_downcase_table).extras(1);
            b.case_eqv_table = XCHAR_TABLE(V.ascii_downcase_table).extras(2);
            b.invisibility_spec = Q.t;
            //#ifndef DOS_NT
            //  b->buffer_file_type = Qnil;
            //#endif

            /* Reset all (or most) per-buffer variables to their defaults.  */
            if (permanent_too != 0)
                b.local_var_alist = Q.nil;
            else
            {
                LispObject tmp, prop, last = Q.nil;
                for (tmp = b.local_var_alist; CONSP(tmp); tmp = XCDR(tmp))
                    if (CONSP(XCAR(tmp))
                        && SYMBOLP(XCAR(XCAR(tmp)))
                        && !NILP(prop = F.get(XCAR(XCAR(tmp)), Q.permanent_local)))
                    {
                        /* If permanent-local, keep it.  */
                        last = tmp;
                        if (EQ(prop, Q.permanent_local_hook))
                        {
                            /* This is a partially permanent hook variable.
                               Preserve only the elements that want to be preserved.  */
                            LispObject list, newlist;
                            list = XCDR(XCAR(tmp));
                            if (!CONSP(list))
                                newlist = list;
                            else
                                for (newlist = Q.nil; CONSP(list); list = XCDR(list))
                                {
                                    LispObject elt = XCAR(list);
                                    /* Preserve element ELT if it's t,
                                   if it is a function with a `permanent-local-hook' property,
                                   or if it's not a symbol.  */
                                    if (!SYMBOLP(elt)
                                    || EQ(elt, Q.t)
                                    || !NILP(F.get(elt, Q.permanent_local_hook)))
                                        newlist = F.cons(elt, newlist);
                                }
                            XSETCDR(XCAR(tmp), F.nreverse(newlist));
                        }
                    }
                    /* Delete this local variable.  */
                    else if (NILP(last))
                        b.local_var_alist = XCDR(tmp);
                    else
                        XSETCDR(last, XCDR(tmp));
            }

            for (i = 0; i < last_per_buffer_idx; ++i)
                if (permanent_too != 0 || buffer_permanent_local_flags[i] == false)
                    SET_PER_BUFFER_VALUE_P(b, i, 0);

            /* For each slot that has a default value,
               copy that into the slot.  */

            /* buffer-local Lisp variables start at `undo_list',
               tho only the ones from `name' on are GC'd normally.  */
            for (offset = PER_BUFFER_VAR_OFFSET(Buffer.Offsets.undo_list);
                 offset < (int)Buffer.Offsets.SIZE;
                 offset += 1)
            {
                int idx = PER_BUFFER_IDX(offset);
                if ((idx > 0
                 && (permanent_too != 0
                     || buffer_permanent_local_flags[idx] == false))
                    /* Is -2 used anywhere?  */
                || idx == -2)
                    b[offset] = PER_BUFFER_DEFAULT(offset);
            }
        }

        /* Get overlays at POSN into array OVERLAYS with NOVERLAYS elements.
           If NEXTP is non-NULL, return next overlay there.
           See overlay_at arg CHANGE_REQ for meaning of CHRQ arg.  */
        public static void GET_OVERLAYS_AT(int posn, ref LispObject[] overlays, ref int noverlays, ref int nextp, int chrq)
        {
            int maxlen = 40;
            int prevp = 0;
            overlays = new LispObject[maxlen];
            noverlays = overlays_at(posn, 0, ref overlays, ref maxlen,
                         ref nextp, ref prevp, chrq);
            if (noverlays > maxlen)
            {
                maxlen = noverlays;
                overlays = new LispObject[maxlen];
                noverlays = overlays_at(posn, 0, ref overlays, ref maxlen,
                             ref nextp, ref prevp, chrq);
            }
        }

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
        public static int BEG
        {
            get
            {
                return 1;
            }
        }

        public static int BEG_BYTE()
        {
            return BEG;
        }

        /* Position of beginning of accessible range of buffer.  */
        public static int BEGV()
        {
            return current_buffer.begv;
        }

        public static int BEGV_BYTE()
        {
            return current_buffer.begv_byte;
        }

        /* Position of beginning of buffer.  */
        public static int BUF_BEG(Buffer buf)
        {
            return BEG;
        }

        /* Position of point in buffer.  The "+ 0" makes this
           not an l-value, so you can't assign to it.  Use SET_PT instead.  */
        public static int PT ()
        {
            return current_buffer.pt;
        }

        public static int PT_BYTE()
        {
            return current_buffer.pt_byte;
        }

        /* Position of end of accessible range of buffer.  */
        public static int ZV
        {
            get
            {
                return current_buffer.zv;
            }

            set
            {
                current_buffer.zv = value;
            }
        }

        public static int ZV_BYTE
        {
            get
            {
                return current_buffer.zv_byte;
            }

            set
            {
                current_buffer.zv_byte = value;
            }
        }

        /* Position of end of buffer.  */
        public static int Z
        {
            get
            {
                return current_buffer.text.z;
            }

            set
            {
                current_buffer.text.z = value;
            }
        }

        public static int Z_BYTE
        {
            get
            {
                return current_buffer.text.z_byte;
            }
            set
            {
                current_buffer.text.z_byte = value;
            }
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

        /* Position of gap in buffer.  */
        public static int BUF_GPT(Buffer buf)
        {
            return buf.text.gpt;
        }

        public static int BUF_GPT_BYTE(Buffer buf)
        {
            return buf.text.gpt_byte;
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

        /* Modification count.  */
        public static int BUF_MODIFF(Buffer buf)
        {
            return buf.text.modiff;
        }

        public static int BUF_BEG_UNCHANGED(Buffer buf)
        {
            return buf.text.beg_unchanged;
        }

        public static void BUF_BEG_UNCHANGED(Buffer buf, int val)
        {
            buf.text.beg_unchanged = val;
        }

        public static int BUF_END_UNCHANGED(Buffer buf)
        {
            return buf.text.end_unchanged;
        }

        public static void BUF_END_UNCHANGED(Buffer buf, int val)
        {
            buf.text.end_unchanged = val;
        }

        public static int BEG_UNCHANGED
        {
            get
            {
                return BUF_BEG_UNCHANGED(current_buffer);
            }

            set
            {
                BUF_END_UNCHANGED(current_buffer, value);
            }
        }

        public static int END_UNCHANGED
        {
            get
            {
                return BUF_END_UNCHANGED(current_buffer);
            }
            set
            {
                BUF_END_UNCHANGED(current_buffer, value);
            }
        }

        public static int BUF_UNCHANGED_MODIFIED(Buffer buf)
        {
            return (buf.text.unchanged_modified);
        }

        public static void BUF_UNCHANGED_MODIFIED(Buffer buf, int val)
        {
            buf.text.unchanged_modified = val;
        }

        /* Overlay modification count.  */
        public static int BUF_OVERLAY_MODIFF(Buffer buf)
        {
            return (buf.text.overlay_modiff);
        }

        /* Modification count as of last visit or save.  */
        public static int BUF_SAVE_MODIFF(Buffer buf)
        {
            return buf.text.save_modiff;
        }

        public static int BUF_OVERLAY_UNCHANGED_MODIFIED(Buffer buf)
        {
            return (buf.text.overlay_unchanged_modified);
        }

        public static void BUF_OVERLAY_UNCHANGED_MODIFIED(Buffer buf, int val)
        {
            buf.text.overlay_unchanged_modified = val;
        }

        /* Compute how many characters at the top and bottom of BUF are
           unchanged when the range START..END is modified.  This computation
           must be done each time BUF is modified.  */
        public static void BUF_COMPUTE_UNCHANGED(Buffer buf, int start, int end)
        {
            if (BUF_UNCHANGED_MODIFIED(buf) == BUF_MODIFF(buf)
            && (BUF_OVERLAY_UNCHANGED_MODIFIED(buf)
                == BUF_OVERLAY_MODIFF(buf)))
            {
                buf.text.beg_unchanged = (start) - BUF_BEG(buf);
                buf.text.end_unchanged = BUF_Z(buf) - (end);
            }
            else
            {
                if (BUF_Z(buf) - (end) < BUF_END_UNCHANGED(buf))
                    buf.text.end_unchanged = BUF_Z(buf) - (end);
                if ((start) - BUF_BEG(buf) < BUF_BEG_UNCHANGED(buf))
                    buf.text.beg_unchanged = (start) - BUF_BEG(buf);
            }
        }

        /* Interval tree of buffer.  */
        public static Interval BUF_INTERVALS(Buffer buf)
        {
            return buf.text.intervals;
        }

        /* Marker chain of buffer.  */
        public static LispMarker BUF_MARKERS(Buffer buf)
        {
            return buf.text.markers;
        }

        public static void BUF_MARKERS(Buffer buf, LispMarker m)
        {
            buf.text.markers = m;
        }

        public static void SET_PT(int position)
        {
            set_point(position);
        }

        public static void SET_PT_BOTH(int position, int b)
        {
            set_point_both(position, b);
        }

        public static void TEMP_SET_PT_BOTH(int position, int b)
        {
            temp_set_point_both(current_buffer, position, b);
        }

        /* Return the address of character at byte position POS in buffer BUF.
           Note that both arguments can be computed more than once.  */
        public static int BUF_BYTE_ADDRESS(Buffer buf, int pos)
        {
            return (pos - BEG_BYTE() + (pos >= buf.text.gpt_byte ? buf.text.gap_size : 0));
        }

        /* Return the byte at byte position N in buffer BUF.   */
        public static byte BUF_FETCH_BYTE(Buffer buf, int n)
        {
            return buf.text.beg[BUF_BYTE_ADDRESS(buf, n)];
        }

        /* Macros for setting the BEGV, ZV or PT of a given buffer.

           SET_BUF_PT* seet to be redundant.  Get rid of them?

           The ..._BOTH macros take both a charpos and a bytepos,
           which must correspond to each other.

           The macros without ..._BOTH take just a charpos,
           and compute the bytepos from it.  */
        public static void SET_BUF_BEGV(Buffer buf, int charpos)
        {
            buf.begv_byte = buf_charpos_to_bytepos(buf, charpos);
            buf.begv = charpos;
        }

        public static void SET_BUF_ZV(Buffer buf, int charpos)
        {
            buf.zv_byte = buf_charpos_to_bytepos(buf, charpos);
            buf.zv = charpos;
        }

        public static void SET_BUF_BEGV_BOTH(Buffer buf, int charpos, int bytepos)
        {
            buf.begv = charpos;
            buf.begv_byte = bytepos;
        }

        public static void SET_BUF_ZV_BOTH(Buffer buf, int charpos, int bytepos)
        {
            buf.zv = charpos;
            buf.zv_byte = bytepos;
        }

        public static void SET_BUF_PT_BOTH(Buffer buf, int charpos, int bytepos)
        {
            buf.pt = charpos;
            buf.pt_byte = bytepos;
        }

        /* Address of beginning of buffer.  */
        public static byte[] BUF_BEG_ADDR(Buffer buf)
        {
            return buf.text.beg;
        }

        /* Address of end of gap in buffer.  */
        public static int BUF_GAP_END_ADDR(Buffer buf)
        {
            return buf.text.gpt_byte + buf.text.gap_size - BEG_BYTE();
        }

        /* Size of gap.  */
        public static int BUF_GAP_SIZE(Buffer buf)
        {
            return buf.text.gap_size;
        }

        /* Convert a character position to a byte position.  */
        public static int CHAR_TO_BYTE(int charpos)
        {
            return buf_charpos_to_bytepos(current_buffer, charpos);
        }

        /* Modification count.  */
        public static int MODIFF
        {
            get
            {
                return current_buffer.text.modiff;
            }
            set
            {
                current_buffer.text.modiff = value;
            }
        }

        /* Character modification count.  */
        public static int CHARS_MODIFF
        {
            get
            {
                return current_buffer.text.chars_modiff;
            }

            set
            {
                current_buffer.text.chars_modiff = value;
            }
        }

        /* Modification count as of last visit or save.  */
        public static int SAVE_MODIFF()
        {
            return current_buffer.text.save_modiff;
        }

        public static void TEMP_SET_PT(int position)
        {
            temp_set_point(current_buffer, position);
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

        /* This structure holds the names of symbols whose values may be
           buffer-local.  It is indexed and accessed in the same way as the above. */
        public static LispObject[] buffer_local_symbols = new LispObject[(int)Buffer.Offsets.SIZE];

        /* Flags indicating which built-in buffer-local variables
           are permanent locals.  */
        public static bool[] buffer_permanent_local_flags = new bool[Buffer.MAX_PER_BUFFER_VARS];

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

        /* Return the offset in bytes of member VAR of struct buffer
           from the start of a buffer structure.  */
        public static int PER_BUFFER_VAR_OFFSET(Buffer.Offsets VAR)
        {
            return (int) VAR; 
            //((char *) &buffer_local_flags.VAR - (char *) &buffer_local_flags)
        }

        /* Return the index of buffer-local variable VAR.  Each per-buffer
           variable has an index > 0 associated with it, except when it always
           has buffer-local values, in which case the index is -1.  If this is
           0, this is a bug and means that the slot of VAR in
           buffer_local_flags wasn't intiialized.  */
        public static int PER_BUFFER_VAR_IDX(Buffer.Offsets VAR)
        {
            return PER_BUFFER_IDX ((int) PER_BUFFER_VAR_OFFSET (VAR));
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

        /* Return the default value of the per-buffer variable at offset
           OFFSET in the buffer structure.  */
        public static LispObject PER_BUFFER_DEFAULT(int OFFSET)
        {
            return buffer_defaults[OFFSET];
        }

        /* Return the buffer-local value of the per-buffer variable at offset
           OFFSET in the buffer structure.  */
        public static LispObject PER_BUFFER_VALUE(Buffer buffer, int offset)
        {
           return buffer[offset];
        }

        /* Return the symbol of the per-buffer variable at offset OFFSET in
           the buffer structure.  */
        public static LispObject PER_BUFFER_SYMBOL(int OFFSET)
        {
            return buffer_local_symbols[OFFSET];
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

        public static LispObject inhibit_read_only
        {
            get { return Defs.O[(int)Objects.inhibit_read_only]; }
            set { Defs.O[(int)Objects.inhibit_read_only] = value; }
        }

        /* Functions to call before and after each text change. */
        public static LispObject before_change_functions
        {
            get { return Defs.O[(int)Objects.before_change_functions]; }
            set { Defs.O[(int)Objects.before_change_functions] = value; }
        }

        public static LispObject after_change_functions
        {
            get { return Defs.O[(int)Objects.after_change_functions]; }
            set { Defs.O[(int)Objects.after_change_functions] = value; }
        }

        public static LispObject first_change_hook
        {
            get { return Defs.O[(int)Objects.first_change_hook]; }
            set { Defs.O[(int)Objects.first_change_hook] = value; }
        }
    }

    public partial class F
    {
        public static LispObject get_buffer_create(LispObject buffer_or_name)
        {
            LispObject buffer, name;
            Buffer b;

            buffer = F.get_buffer(buffer_or_name);
            if (!L.NILP(buffer))
                return buffer;

            if (L.SCHARS(buffer_or_name) == 0)
                L.error("Empty string for buffer name is not allowed");

            b = new Buffer();

            /* An ordinary buffer uses its own struct buffer_text.  */
            b.text = b.own_text;
            b.base_buffer = null;

            b.text.gap_size = 20;
            /* We allocate extra 1-byte at the tail and keep it always '\0' for
               anchoring a search.  */
            L.alloc_buffer_text(b, L.BUF_GAP_SIZE(b) + 1);

//            if (L.BUF_BEG_ADDR(b) == null)
//                L.buffer_memory_full();

            b.pt = L.BEG;
            b.text.gpt = L.BEG;
            b.begv = L.BEG;
            b.zv = L.BEG;
            b.text.z = L.BEG;
            b.pt_byte = L.BEG_BYTE();
            b.text.gpt_byte = L.BEG_BYTE();
            b.begv_byte = L.BEG_BYTE();
            b.zv_byte = L.BEG_BYTE();
            b.text.z_byte = L.BEG_BYTE();
            b.text.modiff = 1;
            b.text.chars_modiff = 1;
            b.text.overlay_modiff = 1;
            b.text.save_modiff = 1;
            b.text.intervals = null;
            b.text.unchanged_modified = 1;
            b.text.overlay_unchanged_modified = 1;
            b.text.end_unchanged = 0;
            b.text.beg_unchanged = 0;
            //            *(L.BUF_GPT_ADDR(b)) = *(L.BUF_Z_ADDR(b)) = 0; /* Put an anchor '\0'.  */

            b.newline_cache = null;
            b.width_run_cache = null;
            b.width_table = Q.nil;
            b.prevent_redisplay_optimizations_p = true;

            /* Put this on the chain of all buffers including killed ones.  */
            b.next = L.all_buffers;
            L.all_buffers = b;

            /* An ordinary buffer normally doesn't need markers
               to handle BEGV and ZV.  */
            b.pt_marker = Q.nil;
            b.begv_marker = Q.nil;
            b.zv_marker = Q.nil;

            name = F.copy_sequence(buffer_or_name);
            L.STRING_SET_INTERVALS(name, L.NULL_INTERVAL);
            b.name = name;

            b.undo_list = (L.SREF(name, 0) != ' ') ? Q.nil : Q.t;

            L.reset_buffer(b);
            L.reset_buffer_local_variables(b, 1);

            b.mark = F.make_marker();
            b.text.markers = null;
            b.name = name;

            /* Put this in the alist of all live buffers.  */
            buffer = b;
            V.buffer_alist = L.nconc2(V.buffer_alist, F.cons(F.cons(name, buffer), Q.nil));

            /* An error in calling the function here (should someone redefine it)
               can lead to infinite regress until you run out of stack.  rms
               says that's not worth protecting against.  */
            if (!L.NILP(F.fboundp(Q.ucs_set_table_for_input)))
                L.call1(Q.ucs_set_table_for_input, buffer);

            return buffer;
        }

        public static LispObject delete_overlay(LispObject overlay)
        {
            LispObject buffer;
            Buffer b;
            int count = L.SPECPDL_INDEX();

            L.CHECK_OVERLAY(overlay);

            buffer = F.marker_buffer(L.OVERLAY_START(overlay));
            if (L.NILP(buffer))
                return Q.nil;

            b = L.XBUFFER(buffer);
            L.specbind(Q.inhibit_quit, Q.t);

            b.overlays_before = L.unchain_overlay(b.overlays_before, L.XOVERLAY(overlay));
            b.overlays_after = L.unchain_overlay(b.overlays_after, L.XOVERLAY(overlay));

            L.modify_overlay(b,
                    L.marker_position(L.OVERLAY_START(overlay)),
                    L.marker_position(L.OVERLAY_END(overlay)));
            F.set_marker(L.OVERLAY_START(overlay), Q.nil, Q.nil);
            F.set_marker(L.OVERLAY_END(overlay), Q.nil, Q.nil);

            /* When deleting an overlay with before or after strings, turn off
               display optimizations for the affected buffer, on the basis that
               these strings may contain newlines.  This is easier to do than to
               check for that situation during redisplay.  */
            if (L.windows_or_buffers_changed == 0
                && (!L.NILP(F.overlay_get(overlay, Q.before_string))
                || !L.NILP(F.overlay_get(overlay, Q.after_string))))
                b.prevent_redisplay_optimizations_p = true;

            return L.unbind_to(count, Q.nil);
        }

        public static LispObject erase_buffer()
        {
            F.widen();

            L.del_range(L.BEG, L.Z);

            L.current_buffer.last_window_start = 1;
            /* Prevent warnings, or suspension of auto saving, that would happen
               if future size is less than past size.  Use of erase-buffer
               implies that the future text is not really related to the past text.  */
            L.current_buffer.save_length = L.XSETINT(0);
            return Q.nil;
        }

        public static LispObject kill_all_local_variables()
        {
            if (!L.NILP(V.run_hooks))
                L.call1(V.run_hooks, Q.change_major_mode_hook);

            /* Make sure none of the bindings in local_var_alist
               remain swapped in, in their symbols.  */

            L.swap_out_buffer_local_variables(L.current_buffer);

            /* Actually eliminate all local bindings of this buffer.  */

            L.reset_buffer_local_variables(L.current_buffer, 0);

            /* Force mode-line redisplay.  Useful here because all major mode
               commands call this function.  */
            L.update_mode_lines++;

            return Q.nil;
        }

        public static LispObject barf_if_buffer_read_only()
        {
            if (!L.NILP(L.current_buffer.read_only) && L.NILP(V.inhibit_read_only))
                L.xsignal1(Q.buffer_read_only, F.current_buffer());
            return Q.nil;
        }

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

        public static LispObject next_overlay_change(LispObject pos)
        {
            int noverlays;
            int endpos = 0;
            LispObject[] overlay_vec;
            int len;
            int i;

            L.CHECK_NUMBER_COERCE_MARKER(ref pos);

            len = 10;
            overlay_vec = new LispObject[len];

            int prevpos = 0;
            /* Put all the overlays we want in a vector in overlay_vec.
               Store the length in len.
               endpos gets the position where the next overlay starts.  */
            noverlays = L.overlays_at(L.XINT(pos), 1, ref overlay_vec, ref len,
               ref endpos, ref prevpos, 1);

            /* If any of these overlays ends before endpos,
               use its ending point instead.  */
            for (i = 0; i < noverlays; i++)
            {
                LispObject oend;
                int oendpos;

                oend = L.OVERLAY_END(overlay_vec[i]);
                oendpos = L.OVERLAY_POSITION(oend);
                if (oendpos < endpos)
                    endpos = oendpos;
            }

            return L.make_number(endpos);
        }

        public static LispObject previous_overlay_change(LispObject pos)
        {
            int noverlays;
            int prevpos = 0;
            LispObject[] overlay_vec;
            int len;

            L.CHECK_NUMBER_COERCE_MARKER(ref pos);

            /* At beginning of buffer, we know the answer;
               avoid bug subtracting 1 below.  */
            if (L.XINT(pos) == L.BEGV())
                return pos;

            len = 10;
            overlay_vec = new LispObject[len];

            /* Put all the overlays we want in a vector in overlay_vec.
               Store the length in len.
               prevpos gets the position of the previous change.  */
            int nextpos = 0;
            noverlays = L.overlays_at(L.XINT(pos), 1, ref overlay_vec, ref len,
                         ref nextpos, ref prevpos, 1);

            return L.make_number(prevpos);
        }

        public static LispObject overlay_get(LispObject overlay, LispObject prop)
        {
            L.CHECK_OVERLAY(overlay);
            return L.lookup_char_property(L.XOVERLAY(overlay).plist, prop, false);
        }

        public static LispObject other_buffer(LispObject buffer, LispObject visible_ok, LispObject frame)
        {
            // COMEBACK WHEN READY
            return Q.nil;
        }
    }

    public partial class L
    {
        public static void validate_region (ref LispObject b, ref LispObject e)
        {
            CHECK_NUMBER_COERCE_MARKER(ref b);
            CHECK_NUMBER_COERCE_MARKER(ref e);

            if (XINT(b) > XINT(e))
            {
                LispObject tem;
                tem = b; b = e; e = tem;
            }

            if (!(BEGV() <= XINT(b) && XINT(b) <= XINT(e) && XINT (e) <= ZV))
                args_out_of_range(b, e);
        }

        /* Reinitialize everything about a buffer except its name and contents
           and local variables.
           If called on an already-initialized buffer, the list of overlays
           should be deleted before calling this function, otherwise we end up
           with overlays that claim to belong to the buffer but the buffer
           claims it doesn't belong to it.  */
        public static void reset_buffer(Buffer b)
        {
            b.filename = Q.nil;
            b.file_truename = Q.nil;
            b.directory = (current_buffer != null) ? current_buffer.directory : Q.nil;
            b.modtime = 0;
            b.save_length = XSETINT(0);
            b.last_window_start = 1;
            /* It is more conservative to start out "changed" than "unchanged".  */
            b.clip_changed = 0;
            b.prevent_redisplay_optimizations_p = true;
            b.backed_up = Q.nil;
            b.auto_save_modified = 0;
            b.auto_save_failure_time = -1;
            b.auto_save_file_name = Q.nil;
            b.read_only = Q.nil;
            b.overlays_before = null;
            b.overlays_after = null;
            b.overlay_center = BEG;
            b.mark_active = Q.nil;
            b.point_before_scroll = Q.nil;
            b.file_format = Q.nil;
            b.auto_save_file_format = Q.t;
            b.last_selected_window = Q.nil;
            b.display_count = XSETINT(0);
            b.display_time = Q.nil;
            b.enable_multibyte_characters = buffer_defaults.enable_multibyte_characters;
            b.cursor_type = buffer_defaults.cursor_type;
            b.extra_line_spacing = buffer_defaults.extra_line_spacing;

            b.display_error_modiff = 0;
        }
        /* Make sure no local variables remain set up with buffer B
           for their current values.  */
        public static void swap_out_buffer_local_variables (Buffer b)
        {
            LispObject oalist, alist, sym, tem, buffer;

            buffer = b;
            oalist = b.local_var_alist;

            for (alist = oalist; CONSP(alist); alist = XCDR(alist))
            {
                sym = XCAR(XCAR(alist));

                /* Need not do anything if some other buffer's binding is now encached.  */
                tem = XBUFFER_LOCAL_VALUE(SYMBOL_VALUE(sym)).buffer;
                if (EQ(tem, buffer))
                {
                    /* Symbol is set up for this buffer's old local value:
                       swap it out!  */
                    swap_in_global_binding(sym);
                }
            }
        }

        /* Mark a section of BUF as needing redisplay because of overlays changes.  */
        public static void modify_overlay(Buffer buf, int start, int end)
        {
            if (start > end)
            {
                int temp = start;
                start = end;
                end = temp;
            }

            BUF_COMPUTE_UNCHANGED(buf, start, end);

            /* If this is a buffer not in the selected window,
               we must do other windows.  */
            if (buf != XBUFFER(XWINDOW(selected_window).buffer))
                windows_or_buffers_changed = 1;
            /* If multiple windows show this buffer, we must do other windows.  */
            else if (buffer_shared > 1)
                windows_or_buffers_changed = 1;
            /* If we modify an overlay at the end of the buffer, we cannot
               be sure that window end is still valid.  */
            else if (end >= ZV && start <= ZV)
                windows_or_buffers_changed = 1;

            buf.text.overlay_modiff++;
        }

        public static LispOverlay unchain_overlay(LispOverlay list, LispOverlay overlay)
        {
            LispOverlay tmp, prev;
            for (tmp = list, prev = null; tmp != null; prev = tmp, tmp = tmp.next)
                if (tmp == overlay)
                {
                    if (prev != null)
                        prev.next = tmp.next;
                    else
                        list = tmp.next;
                    overlay.next = null;
                    break;
                }
            return list;
        }

        public static void delete_all_overlays(Buffer b)
        {
            LispObject overlay;

            /* `reset_buffer' blindly sets the list of overlays to NULL, so we
               have to empty the list, otherwise we end up with overlays that
               think they belong to this buffer while the buffer doesn't know about
               them any more.  */
            while (b.overlays_before != null)
            {
                overlay = b.overlays_before;
                F.delete_overlay(overlay);
            }
            while (b.overlays_after != null)
            {
                overlay = b.overlays_after;
                F.delete_overlay(overlay);
            }
        }

        /* Set the current buffer to BUFFER provided it is alive.  */
        public static LispObject set_buffer_if_live (LispObject buffer)
        {
            if (!NILP(XBUFFER(buffer).name))
                F.set_buffer(buffer);
            return Q.nil;
        }

        /* Find all the overlays in the current buffer that contain position POS.
           Return the number found, and store them in a vector in *VEC_PTR.
           Store in *LEN_PTR the size allocated for the vector.
           Store in *NEXT_PTR the next position after POS where an overlay starts,
             or ZV if there are no more overlays between POS and ZV.
           Store in *PREV_PTR the previous position before POS where an overlay ends,
             or where an overlay starts which ends at or after POS;
             or BEGV if there are no such overlays from BEGV to POS.
           NEXT_PTR and/or PREV_PTR may be 0, meaning don't store that info.

           *VEC_PTR and *LEN_PTR should contain a valid vector and size
           when this function is called.

           If EXTEND is non-zero, we make the vector bigger if necessary.
           If EXTEND is zero, we never extend the vector,
           and we store only as many overlays as will fit.
           But we still return the total number of overlays.

           If CHANGE_REQ is true, then any position written into *PREV_PTR or
           *NEXT_PTR is guaranteed to be not equal to POS, unless it is the
           default (BEGV or ZV).  */
        public static int overlays_at(int pos, int extend, ref LispObject[] vec_ptr, ref int len_ptr, ref int next_ptr, ref int prev_ptr, int change_req)
        {
            LispObject overlay, start, end;
            LispOverlay tail;
            int idx = 0;
            int len = len_ptr;
            LispObject[] vec = vec_ptr;
            int next = ZV;
            int prev = BEGV();
            int inhibit_storing = 0;

            for (tail = current_buffer.overlays_before; tail != null; tail = tail.next)
            {
                int startpos, endpos;

                overlay = tail;

                start = OVERLAY_START(overlay);
                end = OVERLAY_END(overlay);
                endpos = OVERLAY_POSITION(end);
                if (endpos < pos)
                {
                    if (prev < endpos)
                        prev = endpos;
                    break;
                }
                startpos = OVERLAY_POSITION(start);
                /* This one ends at or after POS
               so its start counts for PREV_PTR if it's before POS.  */
                if (prev < startpos && startpos < pos)
                    prev = startpos;
                if (endpos == pos)
                    continue;
                if (startpos <= pos)
                {
                    if (idx == len)
                    {
                        /* The supplied vector is full.
                       Either make it bigger, or don't store any more in it.  */
                        if (extend != 0)
                        {
                            /* Make it work with an initial len == 0.  */
                            len *= 2;
                            if (len == 0)
                                len = 4;
                            len_ptr = len;
                            System.Array.Resize(ref vec, len);
                            vec_ptr = vec;
                        }
                        else
                            inhibit_storing = 1;
                    }

                    if (inhibit_storing == 0)
                        vec[idx] = overlay;
                    /* Keep counting overlays even if we can't return them all.  */
                    idx++;
                }
                else if (startpos < next)
                    next = startpos;
            }

            for (tail = current_buffer.overlays_after; tail != null; tail = tail.next)
            {
                int startpos, endpos;

                overlay = tail;

                start = OVERLAY_START(overlay);
                end = OVERLAY_END(overlay);
                startpos = OVERLAY_POSITION(start);
                if (pos < startpos)
                {
                    if (startpos < next)
                        next = startpos;
                    break;
                }
                endpos = OVERLAY_POSITION(end);
                if (pos < endpos)
                {
                    if (idx == len)
                    {
                        if (extend != 0)
                        {
                            /* Make it work with an initial len == 0.  */
                            len *= 2;
                            if (len == 0)
                                len = 4;
                            len_ptr = len;
                            System.Array.Resize(ref vec, len);
                            vec_ptr = vec;
                        }
                        else
                            inhibit_storing = 1;
                    }

                    if (inhibit_storing == 0)
                        vec[idx] = overlay;
                    idx++;

                    if (startpos < pos && startpos > prev)
                        prev = startpos;
                }
                else if (endpos < pos && endpos > prev)
                    prev = endpos;
                else if (endpos == pos && startpos > prev
                     && (change_req == 0 || startpos < pos))
                    prev = startpos;
            }

            next_ptr = next;
            prev_ptr = prev;
            return idx;
        }

        public class sortvec
        {
            public LispObject overlay;
            public int beg, end;
            public int priority;
        }

        public class CompareOverlays : System.Collections.Generic.IComparer<sortvec>
        {
            public int Compare(sortvec s1, sortvec s2)
            {
                if (s1.priority != s2.priority)
                {
                    return s1.priority - s2.priority;
                }
                if (s1.beg != s2.beg)
                {
                    return s1.beg - s2.beg;
                }
                if (s1.end != s2.end)
                {
                    return s2.end - s1.end;
                }
                return 0;
            }
        }

        /* Sort an array of overlays by priority.  The array is modified in place.
           The return value is the new size; this may be smaller than the original
           size if some of the overlays were invalid or were window-specific.  */
        public static int sort_overlays(LispObject[] overlay_vec, int noverlays, Window w)
        {
            int i, j;
            sortvec[] sortvec;
            sortvec = new sortvec[noverlays];

            /* Put the valid and relevant overlays into sortvec.  */

            for (i = 0, j = 0; i < noverlays; i++)
            {
                LispObject tem;
                LispObject overlay;

                overlay = overlay_vec[i];
                if (OVERLAY_VALID(overlay)
                && OVERLAY_POSITION(OVERLAY_START(overlay)) > 0
                && OVERLAY_POSITION(OVERLAY_END(overlay)) > 0)
                {
                    /* If we're interested in a specific window, then ignore
                       overlays that are limited to some other window.  */
                    if (w != null)
                    {
                        LispObject window;

                        window = F.overlay_get(overlay, Q.window);
                        if (WINDOWP(window) && XWINDOW(window) != w)
                            continue;
                    }

                    /* This overlay is good and counts: put it into sortvec.  */
                    sortvec[j].overlay = overlay;
                    sortvec[j].beg = OVERLAY_POSITION(OVERLAY_START(overlay));
                    sortvec[j].end = OVERLAY_POSITION(OVERLAY_END(overlay));
                    tem = F.overlay_get(overlay, Q.priority);
                    if (INTEGERP(tem))
                        sortvec[j].priority = XINT(tem);
                    else
                        sortvec[j].priority = 0;
                    j++;
                }
            }
            noverlays = j;

            /* Sort the overlays into the proper order: increasing priority.  */

            if (noverlays > 1)
            {
                System.Array.Sort(sortvec, 0, noverlays, new CompareOverlays());
            }

            for (i = 0; i < noverlays; i++)
                overlay_vec[i] = sortvec[i].overlay;
            return (noverlays);
        }

        /* Shift overlays in BUF's overlay lists, to center the lists at POS.  */
        public static void recenter_overlay_lists(Buffer buf, int pos)
        {
            LispObject overlay, beg, end;
            LispOverlay prev, tail, next;

            /* See if anything in overlays_before should move to overlays_after.  */

            /* We don't strictly need prev in this loop; it should always be nil.
               But we use it for symmetry and in case that should cease to be true
               with some future change.  */
            prev = null;
            for (tail = buf.overlays_before; tail != null; prev = tail, tail = next)
            {
                next = tail.next;
                overlay = tail;

                /* If the overlay is not valid, get rid of it.  */
                if (!OVERLAY_VALID(overlay))
                    abort();

                beg = OVERLAY_START(overlay);
                end = OVERLAY_END(overlay);

                if (OVERLAY_POSITION(end) > pos)
                {
                    /* OVERLAY needs to be moved.  */
                    int where = OVERLAY_POSITION(beg);
                    LispOverlay other, other_prev;

                    /* Splice the cons cell TAIL out of overlays_before.  */
                    if (prev != null)
                        prev.next = next;
                    else
                        buf.overlays_before = next;

                    /* Search thru overlays_after for where to put it.  */
                    other_prev = null;
                    for (other = buf.overlays_after; other != null;
                         other_prev = other, other = other.next)
                    {
                        LispObject otherbeg, otheroverlay;

                        otheroverlay = other;

                        otherbeg = OVERLAY_START(otheroverlay);
                        if (OVERLAY_POSITION(otherbeg) >= where)
                            break;
                    }

                    /* Add TAIL to overlays_after before OTHER.  */
                    tail.next = other;
                    if (other_prev != null)
                        other_prev.next = tail;
                    else
                        buf.overlays_after = tail;
                    tail = prev;
                }
                else
                    /* We've reached the things that should stay in overlays_before.
                       All the rest of overlays_before must end even earlier,
                       so stop now.  */
                    break;
            }

            /* See if anything in overlays_after should be in overlays_before.  */
            prev = null;
            for (tail = buf.overlays_after; tail != null; prev = tail, tail = next)
            {
                next = tail.next;
                overlay = tail;

                /* If the overlay is not valid, get rid of it.  */
                if (!OVERLAY_VALID(overlay))
                    abort();

                beg = OVERLAY_START(overlay);
                end = OVERLAY_END(overlay);

                /* Stop looking, when we know that nothing further
               can possibly end before POS.  */
                if (OVERLAY_POSITION(beg) > pos)
                    break;

                if (OVERLAY_POSITION(end) <= pos)
                {
                    /* OVERLAY needs to be moved.  */
                    int where = OVERLAY_POSITION(end);
                    LispOverlay other, other_prev;

                    /* Splice the cons cell TAIL out of overlays_after.  */
                    if (prev != null)
                        prev.next = next;
                    else
                        buf.overlays_after = next;

                    /* Search thru overlays_before for where to put it.  */
                    other_prev = null;
                    for (other = buf.overlays_before; other != null;
                         other_prev = other, other = other.next)
                    {
                        LispObject otherend, otheroverlay;

                        otheroverlay = other;

                        otherend = OVERLAY_END(otheroverlay);
                        if (OVERLAY_POSITION(otherend) <= where)
                            break;
                    }

                    /* Add TAIL to overlays_before before OTHER.  */
                    tail.next = other;
                    if (other_prev != null)
                        other_prev.next = tail;
                    else
                        buf.overlays_before = tail;
                    tail = prev;
                }
            }

            buf.overlay_center = pos;
        }

        public static void adjust_overlays_for_delete(int pos, int length)
        {
            if (current_buffer.overlay_center < pos)
                /* The deletion was to our right.  No change needed; the before- and
                   after-lists are still consistent.  */
            {}
            else if (current_buffer.overlay_center > pos + length)
                /* The deletion was to our left.  We need to adjust the center value
                   to account for the change in position, but the lists are consistent
                   given the new value.  */
                current_buffer.overlay_center -= length;
            else
                /* We're right in the middle.  There might be things on the after-list
                   that now belong on the before-list.  Recentering will move them,
                   and also update the center point.  */
                recenter_overlay_lists(current_buffer, pos);
        }

        /* Delete any zero-sized overlays at position POS, if the `evaporate'
           property is set.  */
        public static void evaporate_overlays(int pos)
        {
            LispObject overlay, hit_list;
            LispOverlay tail;

            hit_list = Q.nil;
            if (pos <= current_buffer.overlay_center)
                for (tail = current_buffer.overlays_before; tail != null; tail = tail.next)
                {
                    int endpos;
                    overlay = tail;
                    endpos = OVERLAY_POSITION(OVERLAY_END(overlay));
                    if (endpos < pos)
                        break;
                    if (endpos == pos && OVERLAY_POSITION(OVERLAY_START(overlay)) == pos
                        && !NILP(F.overlay_get(overlay, Q.evaporate)))
                        hit_list = F.cons(overlay, hit_list);
                }
            else
                for (tail = current_buffer.overlays_after; tail != null; tail = tail.next)
                {
                    int startpos;
                    overlay = tail;
                    startpos = OVERLAY_POSITION(OVERLAY_START(overlay));
                    if (startpos > pos)
                        break;
                    if (startpos == pos && OVERLAY_POSITION(OVERLAY_END(overlay)) == pos
                        && !NILP(F.overlay_get(overlay, Q.evaporate)))
                        hit_list = F.cons(overlay, hit_list);
                }
            for (; CONSP(hit_list); hit_list = XCDR(hit_list))
                F.delete_overlay(XCAR(hit_list));
        }

        /* Subroutine of report_overlay_modification.  */

        /* Lisp vector holding overlay hook functions to call.
           Vector elements come in pairs.
           Each even-index element is a list of hook functions.
           The following odd-index element is the overlay they came from.

           Before the buffer change, we fill in this vector
           as we call overlay hook functions.
           After the buffer change, we get the functions to call from this vector.
           This way we always call the same functions before and after the change.  */
        public static LispObject last_overlay_modification_hooks;

        /* Number of elements actually used in last_overlay_modification_hooks.  */
        public static int last_overlay_modification_hooks_used;

        /* Add one functionlist/overlay pair
           to the end of last_overlay_modification_hooks.  */
        public static void add_overlay_mod_hooklist (LispObject functionlist, LispObject overlay)
        {
            int oldsize = XVECTOR(last_overlay_modification_hooks).Size;

            if (last_overlay_modification_hooks_used == oldsize)
                last_overlay_modification_hooks = larger_vector (last_overlay_modification_hooks, oldsize * 2, Q.nil);

            ASET(last_overlay_modification_hooks, last_overlay_modification_hooks_used,	functionlist);
            last_overlay_modification_hooks_used++;

            ASET(last_overlay_modification_hooks, last_overlay_modification_hooks_used, overlay);
            last_overlay_modification_hooks_used++;
        }

        /* Run the modification-hooks of overlays that include
           any part of the text in START to END.
           If this change is an insertion, also
           run the insert-before-hooks of overlay starting at END,
           and the insert-after-hooks of overlay ending at START.

           This is called both before and after the modification.
           AFTER is nonzero when we call after the modification.

           ARG1, ARG2, ARG3 are arguments to pass to the hook functions.
           When AFTER is nonzero, they are the start position,
           the position after the inserted new text,
           and the length of deleted or replaced old text.  */
        public static void report_overlay_modification(LispObject start, LispObject end, bool after, LispObject arg1, LispObject arg2, LispObject arg3)
        {
            LispObject prop, overlay;
            LispOverlay tail;
            /* 1 if this change is an insertion.  */
            bool insertion = (after ? XINT(arg3) == 0 : EQ(start, end));

            overlay = Q.nil;
            tail = null;

            /* We used to run the functions as soon as we found them and only register
               them in last_overlay_modification_hooks for the purpose of the `after'
               case.  But running elisp code as we traverse the list of overlays is
               painful because the list can be modified by the elisp code so we had to
               copy at several places.  We now simply do a read-only traversal that
               only collects the functions to run and we run them afterwards.  It's
               simpler, especially since all the code was already there.  -stef  */

            if (!after)
            {
                /* We are being called before a change.
               Scan the overlays to find the functions to call.  */
                last_overlay_modification_hooks_used = 0;
                for (tail = current_buffer.overlays_before; tail != null; tail = tail.next)
                {
                    int startpos, endpos;
                    LispObject ostart, oend;

                    overlay = tail;

                    ostart = OVERLAY_START(overlay);
                    oend = OVERLAY_END(overlay);
                    endpos = OVERLAY_POSITION(oend);
                    if (XINT(start) > endpos)
                        break;
                    startpos = OVERLAY_POSITION(ostart);
                    if (insertion && (XINT(start) == startpos
                              || XINT(end) == startpos))
                    {
                        prop = F.overlay_get(overlay, Q.insert_in_front_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                    if (insertion && (XINT(start) == endpos
                              || XINT(end) == endpos))
                    {
                        prop = F.overlay_get(overlay, Q.insert_behind_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                    /* Test for intersecting intervals.  This does the right thing
                       for both insertion and deletion.  */
                    if (XINT(end) > startpos && XINT(start) < endpos)
                    {
                        prop = F.overlay_get(overlay, Q.modification_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                }

                for (tail = current_buffer.overlays_after; tail != null; tail = tail.next)
                {
                    int startpos, endpos;
                    LispObject ostart, oend;

                    overlay = tail;

                    ostart = OVERLAY_START(overlay);
                    oend = OVERLAY_END(overlay);
                    startpos = OVERLAY_POSITION(ostart);
                    endpos = OVERLAY_POSITION(oend);
                    if (XINT(end) < startpos)
                        break;
                    if (insertion && (XINT(start) == startpos
                              || XINT(end) == startpos))
                    {
                        prop = F.overlay_get(overlay, Q.insert_in_front_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                    if (insertion && (XINT(start) == endpos
                              || XINT(end) == endpos))
                    {
                        prop = F.overlay_get(overlay, Q.insert_behind_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                    /* Test for intersecting intervals.  This does the right thing
                       for both insertion and deletion.  */
                    if (XINT(end) > startpos && XINT(start) < endpos)
                    {
                        prop = F.overlay_get(overlay, Q.modification_hooks);
                        if (!NILP(prop))
                            add_overlay_mod_hooklist(prop, overlay);
                    }
                }
            }

            {
                /* Call the functions recorded in last_overlay_modification_hooks.
                   First copy the vector contents, in case some of these hooks
                   do subsequent modification of the buffer.  */
                int size = last_overlay_modification_hooks_used;
                LispObject[] copy = new LispObject[size];
                int i;

                System.Array.Copy(XVECTOR(last_overlay_modification_hooks).Contents, copy, size);

                for (i = 0; i < size; )
                {
                    LispObject prop1, overlay1;
                    prop1 = copy[i++];
                    overlay1 = copy[i++];
                    call_overlay_mod_hooks(prop1, overlay1, after, arg1, arg2, arg3);
                }
            }
        }

        public static void call_overlay_mod_hooks(LispObject list, LispObject overlay, bool after, LispObject arg1, LispObject arg2, LispObject arg3)
        {
            while (CONSP(list))
            {
                if (NILP(arg3))
                    call4(XCAR(list), overlay, after ? Q.t : Q.nil, arg1, arg2);
                else
                    call5(XCAR(list), overlay, after ? Q.t : Q.nil, arg1, arg2, arg3);
                list = XCDR(list);
            }
        }

        /* BUFFER_CEILING_OF (resp. BUFFER_FLOOR_OF), when applied to n, return
           the max (resp. min) p such that

           BYTE_POS_ADDR (p) - BYTE_POS_ADDR (n) == p - n       */
        public static int BUFFER_CEILING_OF(int BYTEPOS) 
        {
            return (((BYTEPOS) < GPT_BYTE && GPT < ZV ? GPT_BYTE : ZV_BYTE) - 1);
        }

        public static int BUFFER_FLOOR_OF(int BYTEPOS)
        {
            return (BEGV() <= GPT && GPT_BYTE <= (BYTEPOS) ? GPT_BYTE : BEGV_BYTE());
        }

        /* Return the address of byte position N in current buffer.  */
        public static PtrEmulator<byte> BYTE_POS_ADDR(int n)
        {
            return new PtrEmulator<byte>(BEG_ADDR(), (n >= GPT_BYTE ? GAP_SIZE : 0) + n - BEG_BYTE());
        }

        /* Return the address of char position N.  */
        public static PtrEmulator<byte> CHAR_POS_ADDR(int n)
        {
            return new PtrEmulator<byte>(BEG_ADDR(), ((n >= GPT ? GAP_SIZE : 0)
                      + buf_charpos_to_bytepos(current_buffer, n)
                      - BEG_BYTE()));
        }

        /* Position of gap in buffer.  */
        public static int GPT
        {
            get
            {
                return current_buffer.text.gpt;
            }
            set
            {
                current_buffer.text.gpt = value;
            }
        }

        public static int GPT_BYTE
        {
            get
            {
                return current_buffer.text.gpt_byte;
            }
            set
            {
                current_buffer.text.gpt_byte = value;
            }
        }

/*
        public static int GAP_SIZE()
        {
            return current_buffer.text.gap_size;
        }
*/
        /* Size of gap.  */
        public static int GAP_SIZE
        {
            get
            {
                return current_buffer.text.gap_size;
            }

            set
            {
                current_buffer.text.gap_size = value;
            }
        }

        /* Address of beginning of gap in buffer.  */
        public static PtrEmulator<byte> GPT_ADDR()
        {
            return new PtrEmulator<byte>(current_buffer.text.beg, current_buffer.text.gpt_byte - BEG_BYTE());
        }

        /* Address of end of gap in buffer.  */
        public static PtrEmulator<byte> GAP_END_ADDR()
        {
            return new PtrEmulator<byte>(current_buffer.text.beg, current_buffer.text.gpt_byte + current_buffer.text.gap_size - BEG_BYTE());
        }

        /* Address of beginning of buffer.  */
        public static byte[] BEG_ADDR()
        {
            return current_buffer.text.beg;
        }

        /* Address of beginning of accessible range of buffer.  */
        public static PtrEmulator<byte> BEGV_ADDR()
        {
            return BYTE_POS_ADDR(current_buffer.begv_byte);
        }

        /* Address of point in buffer.  */
        public static PtrEmulator<byte> PT_ADDR()
        {
            return BYTE_POS_ADDR(current_buffer.pt_byte);
        }

        /* Return the byte at byte position N.  */
        public static byte FETCH_BYTE(int n)
        {
            return BYTE_POS_ADDR(n).Value;
        }

        public static void FETCH_BYTE(int n, int val)
        {
            PtrEmulator<byte> tmp = BYTE_POS_ADDR(n);
            tmp.Value = (byte) val;
        }

        /* Return character code of multi-byte form at position POS.  If POS
           doesn't point the head of valid multi-byte form, only the byte at
           POS is returned.  No range checking.  */
        public static uint FETCH_MULTIBYTE_CHAR(int pos)				 	
        {
            _fetch_multibyte_char_p = ((pos >= GPT_BYTE ? GAP_SIZE : 0) 	
			       + pos - BEG_BYTE());
            return STRING_CHAR(BEG_ADDR(), _fetch_multibyte_char_p, 0);
        }

        /* Return character at position POS.  If the current buffer is unibyte
           and the character is not ASCII, make the returning character
           multibyte.  */
        public static uint FETCH_CHAR_AS_MULTIBYTE(int pos)			
        {
            return (!NILP(current_buffer.enable_multibyte_characters)
   ? FETCH_MULTIBYTE_CHAR((pos))
   : unibyte_to_multibyte_table[(FETCH_BYTE((pos)))]);
        }

        /* Convert PTR, the address of a byte in the buffer, into a byte position.  */
        public static int PTR_BYTE_POS(PtrEmulator<byte> ptr)
        {
            return (ptr - current_buffer.text.beg - (ptr - current_buffer.text.beg <= (GPT_BYTE - BEG_BYTE()) ? 0 : GAP_SIZE) + BEG_BYTE());
        }

        /* Return character at position POS.  */
        public static uint FETCH_CHAR(int pos)				      	
        {
            return (!NILP(current_buffer.enable_multibyte_characters)
   ? FETCH_MULTIBYTE_CHAR(pos)
   : FETCH_BYTE(pos));
    }

        /* Convert a byte position to a character position.  */
        public static int BYTE_TO_CHAR(int bytepos)
        {
            return buf_bytepos_to_charpos(current_buffer, bytepos);
        }

        /* Enlarge buffer B's text buffer by DELTA bytes.  DELTA < 0 means
           shrink it.  */
        public static void enlarge_buffer_text(Buffer b, int delta)
        {
            int nbytes = (BUF_Z_BYTE(b) - BUF_BEG_BYTE(b) + BUF_GAP_SIZE(b) + 1
                     + delta);

            System.Array.Resize(ref b.text.beg, nbytes);
            /*
              if (p == NULL)
                {
                  memory_full ();
                }
            */
        }

        public static void adjust_overlays_for_insert (int pos, int length)
        {
            /* After an insertion, the lists are still sorted properly,
               but we may need to update the value of the overlay center.  */
            if (current_buffer.overlay_center >= pos)
                current_buffer.overlay_center += length;
        }

        /* Fix up overlays that were garbled as a result of permuting markers
           in the range START through END.  Any overlay with at least one
           endpoint in this range will need to be unlinked from the overlay
           list and reinserted in its proper place.
           Such an overlay might even have negative size at this point.
           If so, we'll make the overlay empty. */
        public static void fix_start_end_in_overlays(int start, int end)
        {
            LispObject overlay;
            LispOverlay before_list = null, after_list = null;
            /* These are either nil, indicating that before_list or after_list
               should be assigned, or the cons cell the cdr of which should be
               assigned.  */
            LispOverlay beforep = null, afterp = null;
            /* 'Parent', likewise, indicates a cons cell or
               current_buffer->overlays_before or overlays_after, depending
               which loop we're in.  */
            LispOverlay tail, parent;
            int startpos, endpos;

            /* This algorithm shifts links around instead of consing and GCing.
               The loop invariant is that before_list (resp. after_list) is a
               well-formed list except that its last element, the CDR of beforep
               (resp. afterp) if beforep (afterp) isn't nil or before_list
               (after_list) if it is, is still uninitialized.  So it's not a bug
               that before_list isn't initialized, although it may look
               strange.  */
            for (parent = null, tail = current_buffer.overlays_before; tail != null; )
            {
                overlay = tail;

                endpos = OVERLAY_POSITION(OVERLAY_END(overlay));
                startpos = OVERLAY_POSITION(OVERLAY_START(overlay));

                /* If the overlay is backwards, make it empty.  */
                if (endpos < startpos)
                {
                    startpos = endpos;
                    F.set_marker(OVERLAY_START(overlay), make_number(startpos),
                             Q.nil);
                }

                if (endpos < start)
                    break;

                if (endpos < end
                || (startpos >= start && startpos < end))
                {
                    /* Add it to the end of the wrong list.  Later on,
                       recenter_overlay_lists will move it to the right place.  */
                    if (endpos < current_buffer.overlay_center)
                    {
                        if (afterp == null)
                            after_list = tail;
                        else
                            afterp.next = tail;
                        afterp = tail;
                    }
                    else
                    {
                        if (beforep == null)
                            before_list = tail;
                        else
                            beforep.next = tail;
                        beforep = tail;
                    }
                    if (parent == null)
                        current_buffer.overlays_before = tail.next;
                    else
                        parent.next = tail.next;
                    tail = tail.next;
                }
                else
                {
                    parent = tail;
                    tail = parent.next;
                }
            }
            for (parent = null, tail = current_buffer.overlays_after; tail != null; )
            {
                overlay = tail;

                startpos = OVERLAY_POSITION(OVERLAY_START(overlay));
                endpos = OVERLAY_POSITION(OVERLAY_END(overlay));

                /* If the overlay is backwards, make it empty.  */
                if (endpos < startpos)
                {
                    startpos = endpos;
                    F.set_marker(OVERLAY_START(overlay), make_number(startpos),
                             Q.nil);
                }

                if (startpos >= end)
                    break;

                if (startpos >= start
                || (endpos >= start && endpos < end))
                {
                    if (endpos < current_buffer.overlay_center)
                    {
                        if (afterp == null)
                            after_list = tail;
                        else
                            afterp.next = tail;
                        afterp = tail;
                    }
                    else
                    {
                        if (beforep == null)
                            before_list = tail;
                        else
                            beforep.next = tail;
                        beforep = tail;
                    }
                    if (parent == null)
                        current_buffer.overlays_after = tail.next;
                    else
                        parent.next = tail.next;
                    tail = tail.next;
                }
                else
                {
                    parent = tail; tail = parent.next;
                }
            }

            /* Splice the constructed (wrong) lists into the buffer's lists,
               and let the recenter function make it sane again.  */
            if (beforep != null)
            {
                beforep.next = current_buffer.overlays_before;
                current_buffer.overlays_before = before_list;
            }
            recenter_overlay_lists(current_buffer, current_buffer.overlay_center);

            if (afterp != null)
            {
                afterp.next = current_buffer.overlays_after;
                current_buffer.overlays_after = after_list;
            }
            recenter_overlay_lists(current_buffer, current_buffer.overlay_center);
        }

        /* We have two types of overlay: the one whose ending marker is
           after-insertion-marker (this is the usual case) and the one whose
           ending marker is before-insertion-marker.  When `overlays_before'
           contains overlays of the latter type and the former type in this
           order and both overlays end at inserting position, inserting a text
           increases only the ending marker of the latter type, which results
           in incorrect ordering of `overlays_before'.

           This function fixes ordering of overlays in the slot
           `overlays_before' of the buffer *BP.  Before the insertion, `point'
           was at PREV, and now is at POS.  */
        public static void fix_overlays_before(Buffer bp, int prev, int pos)
        {
            /* If parent is nil, replace overlays_before; otherwise, parent->next.  */
            LispOverlay tail = bp.overlays_before, parent = null, right_pair;
            LispObject tem;
            int end = 0;

            /* After the insertion, the several overlays may be in incorrect
               order.  The possibility is that, in the list `overlays_before',
               an overlay which ends at POS appears after an overlay which ends
               at PREV.  Since POS is greater than PREV, we must fix the
               ordering of these overlays, by moving overlays ends at POS before
               the overlays ends at PREV.  */

            /* At first, find a place where disordered overlays should be linked
               in.  It is where an overlay which end before POS exists. (i.e. an
               overlay whose ending marker is after-insertion-marker if disorder
               exists).  */
            while (tail != null)
            {
                tem = tail;
                end = OVERLAY_POSITION(OVERLAY_END(tem));
                if (end < pos) break;

                parent = tail;
                tail = tail.next;
            }

            /* If we don't find such an overlay,
               or the found one ends before PREV,
               or the found one is the last one in the list,
               we don't have to fix anything.  */
            if (tail == null || end < prev || tail.next == null)
                return;

            right_pair = parent;
            parent = tail;
            tail = tail.next;

            /* Now, end position of overlays in the list TAIL should be before
               or equal to PREV.  In the loop, an overlay which ends at POS is
               moved ahead to the place indicated by the CDR of RIGHT_PAIR.  If
               we found an overlay which ends before PREV, the remaining
               overlays are in correct order.  */
            while (tail != null)
            {
                tem = tail;
                end = OVERLAY_POSITION(OVERLAY_END(tem));

                if (end == pos)
                {			/* This overlay is disordered. */
                    LispOverlay found = tail;

                    /* Unlink the found overlay.  */
                    tail = found.next;
                    parent.next = tail;
                    /* Move an overlay at RIGHT_PLACE to the next of the found one,
                       and link it into the right place.  */
                    if (right_pair == null)
                    {
                        found.next = bp.overlays_before;
                        bp.overlays_before = found;
                    }
                    else
                    {
                        found.next = right_pair.next;
                        right_pair.next = found;
                    }
                }
                else if (end == prev)
                {
                    parent = tail;
                    tail = tail.next;
                }
                else			/* No more disordered overlay. */
                    break;
            }
        }

        /* Switch to buffer B temporarily for redisplay purposes.
           This avoids certain things that don't need to be done within redisplay.  */
        public static void set_buffer_temp(Buffer b)
        {
            Buffer old_buf;

            if (current_buffer == b)
                return;

            old_buf = current_buffer;
            current_buffer = b;

            if (old_buf != null)
            {
                /* If the old current buffer has markers to record PT, BEGV and ZV
               when it is not current, update them now.  */
                if (!NILP(old_buf.pt_marker))
                {
                    LispObject obuf;
                    obuf = old_buf;
                    set_marker_both(old_buf.pt_marker, obuf,
                             BUF_PT(old_buf), BUF_PT_BYTE(old_buf));
                }
                if (!NILP(old_buf.begv_marker))
                {
                    LispObject obuf;
                    obuf = old_buf;
                    set_marker_both(old_buf.begv_marker, obuf,
                             BUF_BEGV(old_buf), BUF_BEGV_BYTE(old_buf));
                }
                if (!NILP(old_buf.zv_marker))
                {
                    LispObject obuf;
                    obuf = old_buf;
                    set_marker_both(old_buf.zv_marker, obuf,
                             BUF_ZV(old_buf), BUF_ZV_BYTE(old_buf));
                }
            }

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
        }
    }

    public partial class Q
    {
        public static LispObject change_major_mode_hook;
        public static LispObject overlayp;
        public static LispObject priority, window, evaporate, before_string, after_string;

        public static LispObject permanent_local_hook;

        public static LispObject fundamental_mode, mode_class, permanent_local;
        public static LispObject protected_field;
        public static LispObject SFundamental;	/* A string "Fundamental" */

        public static LispObject modification_hooks;
        public static LispObject insert_in_front_hooks;
        public static LispObject insert_behind_hooks;

        public static LispObject before_change_functions;
        public static LispObject after_change_functions;

        public static LispObject first_change_hook;
        public static LispObject ucs_set_table_for_input;
    }
}