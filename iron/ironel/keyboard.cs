namespace IronElisp
{
    public class kboard
    {
        kboard next_kboard;

        enum Offsets
        {
            Vprefix_arg,
            Vlast_prefix_arg,
            Vwindow_system,
            Vdefault_minibuffer_frame,
            Vlast_command,
            Vreal_last_command,
            Vlast_repeatable_command,
            Vkeyboard_translate_table,
            Voverriding_terminal_local_map,
            Vsystem_key_alist,
            Vlocal_function_key_map,
            Vinput_decode_map,
            defining_kbd_macro,
            Vlast_kbd_macro,

            SIZE
        }

        LispObject[] symbols = new LispObject[(int) Offsets.SIZE];

        public LispObject this[int index]
        {
            get { return symbols[index]; }
            set { symbols[index] = value; }
        }
        
        /* The prefix argument for the next command, in raw form.  */
        public LispObject Vprefix_arg
        {
            get { return symbols[(int) Offsets.Vprefix_arg]; }
            set { symbols[(int) Offsets.Vprefix_arg] = value;}
        }

        /* Saved prefix argument for the last command, in raw form.  */
        public LispObject Vlast_prefix_arg
        {
            get { return symbols[ (int) Offsets.Vlast_prefix_arg]; }
            set { symbols[ (int) Offsets.Vlast_prefix_arg] = value; }
        }
        
        /* The kind of display: x, w32, ...  */
        public LispObject Vwindow_system
        {
            get { return symbols[ (int) Offsets.Vwindow_system]; }
            set { symbols[ (int) Offsets.Vwindow_system] = value; }
        }
        
        /* Minibufferless frames on this display use this frame's minibuffer.  */
        public LispObject Vdefault_minibuffer_frame
        {
            get { return symbols[ (int) Offsets.Vdefault_minibuffer_frame]; }
            set { symbols[ (int) Offsets.Vdefault_minibuffer_frame] = value; }
        }
        
        /* Last command executed by the editor command loop, not counting
           commands that set the prefix argument.  */
        public LispObject Vlast_command
        {
            get { return symbols[ (int) Offsets.Vlast_command]; }
            set { symbols[ (int) Offsets.Vlast_command] = value; }
        }
        
        /* Normally same as last-command, but never modified by other commands.  */
        public LispObject Vreal_last_command
        {
            get { return symbols[ (int) Offsets.Vreal_last_command]; }
            set { symbols[ (int) Offsets.Vreal_last_command] = value; }
        }
        
        /* Last command that may be repeated by `repeat'.  */
        public LispObject Vlast_repeatable_command
        {
            get { return symbols[ (int) Offsets.Vlast_repeatable_command]; }
            set { symbols[ (int) Offsets.Vlast_repeatable_command] = value; }
        }
        
        /* User-supplied table to translate input characters through.  */
        public LispObject Vkeyboard_translate_table
        {
            get { return symbols[ (int) Offsets.Vkeyboard_translate_table]; }
            set { symbols[ (int) Offsets.Vkeyboard_translate_table] = value; }
        }
        
        /* If non-nil, a keymap that overrides all others but applies only to
           this KBOARD.  Lisp code that uses this instead of calling read-char
           can effectively wait for input in the any-kboard state, and hence
           avoid blocking out the other KBOARDs.  See universal-argument in
           lisp/simple.el for an example.  */
        public LispObject Voverriding_terminal_local_map
        {
            get { return symbols[ (int) Offsets.Voverriding_terminal_local_map]; }
            set { symbols[ (int) Offsets.Voverriding_terminal_local_map] = value; }
        }
        
        /* Alist of system-specific X windows key symbols.  */
        public LispObject Vsystem_key_alist
        {
            get { return symbols[ (int) Offsets.Vsystem_key_alist]; }
            set { symbols[ (int) Offsets.Vsystem_key_alist] = value; }
        }
        
        /* Keymap mapping keys to alternative preferred forms.
           See the DEFVAR for more documentation.  */
        public LispObject Vlocal_function_key_map
        {
            get { return symbols[ (int) Offsets.Vlocal_function_key_map]; }
            set { symbols[ (int) Offsets.Vlocal_function_key_map] = value; }
        }
        
        /* Keymap mapping ASCII function key sequences onto their preferred
           forms.  Initialized by the terminal-specific lisp files.  See the
           DEFVAR for more documentation.  */
        public LispObject Vinput_decode_map
        {
            get { return symbols[ (int) Offsets.Vinput_decode_map]; }
            set { symbols[ (int) Offsets.Vinput_decode_map] = value; }
        }
        
        /* Non-nil while a kbd macro is being defined.  */
        public LispObject defining_kbd_macro
        {
            get { return symbols[ (int) Offsets.defining_kbd_macro]; }
            set { symbols[ (int) Offsets.defining_kbd_macro] = value; }
        }
        
        /* Last anonymous kbd macro defined.  */
        public LispObject Vlast_kbd_macro
        {
            get { return symbols[ (int) Offsets.Vlast_kbd_macro]; }
            set { symbols[ (int) Offsets.Vlast_kbd_macro] = value; }
        }

        /* Unread events specific to this kboard.  */
        LispObject kbd_queue;

        /* The start of storage for the current keyboard macro.  */
        // Lisp_Object *kbd_macro_buffer;
        LispObject kbd_macro_buffer;

        /* Where to store the next keystroke of the macro.  */
        // Lisp_Object *kbd_macro_ptr;
        LispObject kbd_macro_ptr;

        /* The finalized section of the macro starts at kbd_macro_buffer and
           ends before this.  This is not the same as kbd_macro_ptr, because
           we advance this to kbd_macro_ptr when a key's command is complete.
           This way, the keystrokes for "end-kbd-macro" are not included in the
           macro.  This also allows us to throw away the events added to the
           macro by the last command: all the events between kbd_macro_end and
           kbd_macro_ptr belong to the last command; see
           cancel-kbd-macro-events.  */
        // Lisp_Object *kbd_macro_end;
        LispObject kbd_macro_end;

        /* Allocated size of kbd_macro_buffer.  */
        int kbd_macro_bufsize;

        /* Cache for modify_event_symbol.  */
        LispObject system_key_syms;

        /* Number of displays using this KBOARD.  Normally 1, but can be
           larger when you have multiple screens on a single X display.  */
        int reference_count;

        /* The text we're echoing in the modeline - partial key sequences,
           usually.  This is nil when not echoing.  */
        LispObject echo_string;

        /* This flag indicates that events were put into kbd_queue
           while Emacs was running for some other KBOARD.
           The flag means that, when Emacs goes into the any-kboard state again,
           it should check this KBOARD to see if there is a complete command
           waiting.

           Note that the kbd_queue field can be non-nil even when
           kbd_queue_has_data is 0.  When we push back an incomplete
           command, then this flag is 0, meaning we don't want to try
           reading from this KBOARD again until more input arrives.  */
        bool kbd_queue_has_data;
        
        /* Nonzero means echo each character as typed.  */
        bool immediate_echo;

        /* If we have echoed a prompt string specified by the user,
           this is its length in characters.  Otherwise this is -1.  */
        int echo_after_prompt;
    }

    public partial class L
    {
        public static int immediate_quit = 0;
        public static bool waiting_for_input = false;

        /* Total number of times read_char has returned, outside of macros.  */
        public static int num_nonmacro_input_events;
    }

    public partial class V
    {
        public static LispObject throw_on_input;

        /* Form to evaluate (if non-nil) when Emacs is started.  */
        public static LispObject top_level
        {
            get { return Defs.O[(int)Objects.top_level]; }
            set { Defs.O[(int)Objects.top_level] = value; }
        }

        /* Non-nil means deactivate the mark at end of this command.  */
        public static LispObject deactivate_mark
        {
            get { return Defs.O[(int)Objects.deactivate_mark]; }
            set { Defs.O[(int)Objects.deactivate_mark] = value; }
        }
    }

    public partial class F
    {
        public static LispObject top_level()
        {
            if (L.display_hourglass_p)
                L.cancel_hourglass();

            return lisp_throw(Q.top_level, Q.nil);
        }
    }
}