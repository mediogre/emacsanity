namespace IronElisp
{
/* Bits in the modifiers member of the input_event structure.
   Note that reorder_modifiers assumes that the bits are in canonical
   order.

   The modifiers applied to mouse clicks are rather ornate.  The
   window-system-specific code should store mouse clicks with
   up_modifier or down_modifier set.  Having an explicit down modifier
   simplifies some of window-system-independent code; without it, the
   code would have to recognize down events by checking if the event
   is a mouse click lacking the click and drag modifiers.

   The window-system independent code turns all up_modifier events
   bits into drag_modifier, click_modifier, double_modifier, or
   triple_modifier events.  The click_modifier has no written
   representation in the names of the symbols used as event heads,
   but it does appear in the Qevent_symbol_components property of the
   event heads.  */
    enum Modifiers : uint
    {
        up_modifier = 1,		/* Only used on mouse buttons - always
				   turned into a click or a drag modifier
				   before lisp code sees the event.  */
        down_modifier = 2,		/* Only used on mouse buttons.  */
        drag_modifier = 4,		/* This is never used in the event
				   queue; it's only used internally by
				   the window-system-independent code.  */
        click_modifier = 8,		/* See drag_modifier.  */
        double_modifier = 16,          /* See drag_modifier.  */
        triple_modifier = 32,          /* See drag_modifier.  */

        /* The next four modifier bits are used also in keyboard events at
           the Lisp level.

           It's probably not the greatest idea to use the 2^23 bit for any
           modifier.  It may or may not be the sign bit, depending on
           VALBITS, so using it to represent a modifier key means that
           characters thus modified have different integer equivalents
           depending on the architecture they're running on.  Oh, and
           applying XINT to a character whose 2^23 bit is set sign-extends
           it, so you get a bunch of bits in the mask you didn't want.

           The CHAR_ macros are defined in lisp.h.  */
        alt_modifier = L.CHAR_ALT,	/* Under X, the XK_Alt_[LR] keysyms.  */
        super_modifier = L.CHAR_SUPER,	/* Under X, the XK_Super_[LR] keysyms.  */
        hyper_modifier = L.CHAR_HYPER,	/* Under X, the XK_Hyper_[LR] keysyms.  */
        shift_modifier = L.CHAR_SHIFT,
        ctrl_modifier = L.CHAR_CTL,
        meta_modifier = L.CHAR_META	/* Under X, the XK_Meta_[LR] keysyms.  */
    }

    public class Terminal
    {
        public kboard kboard;
    }

    public partial class L
    {
        public static Terminal FRAME_TERMINAL(Frame f)
        {
            return f.terminal;
        }
    }
}