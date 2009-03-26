/* Terminal control module for terminals described by TERMCAP
   Copyright (C) 1985, 1986, 1987, 1993, 1994, 1995, 1998, 2000, 2001,
                 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009
                 Free Software Foundation, Inc.

This file is part of GNU Emacs.

GNU Emacs is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

GNU Emacs is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with GNU Emacs.  If not, see <http://www.gnu.org/licenses/>.  */

/* New redisplay, TTY faces by Gerd Moellmann <gerd@gnu.org>.  */

#include <config.h>
#include <stdio.h>
#include <ctype.h>
#include <string.h>
#include <errno.h>
#include <sys/file.h>

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#if HAVE_TERMIOS_H
#include <termios.h>		/* For TIOCNOTTY. */
#endif

#include <signal.h>
#include <stdarg.h>

#include "lisp.h"
#include "termchar.h"
#include "termopts.h"
#include "buffer.h"
#include "character.h"
#include "charset.h"
#include "coding.h"
#include "composite.h"
#include "keyboard.h"
#include "frame.h"
#include "disptab.h"
#include "termhooks.h"
#include "dispextern.h"
#include "window.h"
#include "keymap.h"
#include "blockinput.h"
#include "syssignal.h"
#include "systty.h"
#include "intervals.h"

#ifndef O_RDWR
#define O_RDWR 2
#endif

#ifndef O_NOCTTY
#define O_NOCTTY 0
#endif

/* If true, use "vs", otherwise use "ve" to make the cursor visible.  */

static int visible_cursor;

/* Display space properties */

extern Lisp_Object Qspace, QCalign_to, QCwidth;

/* Functions to call after suspending a tty. */
Lisp_Object Vsuspend_tty_functions;

/* Functions to call after resuming a tty. */
Lisp_Object Vresume_tty_functions;

/* Chain of all tty device parameters. */
struct tty_display_info *tty_list;

/* Nonzero means no need to redraw the entire frame on resuming a
   suspended Emacs.  This is useful on terminals with multiple
   pages, where one page is used for Emacs and another for all
   else. */
int no_redraw_on_reenter;

/* Meaning of bits in no_color_video.  Each bit set means that the
   corresponding attribute cannot be combined with colors.  */

enum no_color_bit
{
  NC_STANDOUT	 = 1 << 0,
  NC_UNDERLINE	 = 1 << 1,
  NC_REVERSE	 = 1 << 2,
  NC_BLINK	 = 1 << 3,
  NC_DIM	 = 1 << 4,
  NC_BOLD	 = 1 << 5,
  NC_INVIS	 = 1 << 6,
  NC_PROTECT	 = 1 << 7,
  NC_ALT_CHARSET = 1 << 8
};

/* internal state */

/* The largest frame width in any call to calculate_costs.  */

int max_frame_cols;

/* The largest frame height in any call to calculate_costs.  */

int max_frame_lines;

/* Non-zero if we have dropped our controlling tty and therefore
   should not open a frame on stdout. */
static int no_controlling_tty;

/* Provided for lisp packages.  */

static int system_uses_terminfo;

char *tparam ();

extern char *tgetstr ();

/* Buffers to store the source and result of code conversion for terminal.  */
static unsigned char *encode_terminal_src;
static unsigned char *encode_terminal_dst;
/* Allocated sizes of the above buffers.  */
static int encode_terminal_src_size;
static int encode_terminal_dst_size;

#ifndef old
/* char_ins_del_cost[n] is cost of inserting N characters.
   char_ins_del_cost[-n] is cost of deleting N characters.
   The length of this vector is based on max_frame_cols.  */

int *char_ins_del_vector;

#define char_ins_del_cost(f) (&char_ins_del_vector[FRAME_COLS ((f))])
#endif

DEFUN ("tty-type", Ftty_type, Stty_type, 0, 1, 0,
       doc: /* Return the type of the tty device that TERMINAL uses.
Returns nil if TERMINAL is not on a tty device.

TERMINAL can be a terminal id, a frame or nil (meaning the selected
frame's terminal).  */)
     (terminal)
     Lisp_Object terminal;
{
    return Qnil;
}

void
syms_of_term ()
{
//  DEFVAR_BOOL ("system-uses-terminfo", &system_uses_terminfo,
//    doc: /* Non-nil means the system uses terminfo rather than termcap.
//This variable can be used by terminal emulator packages.  */);
//#ifdef TERMINFO
//  system_uses_terminfo = 1;
//#else
//  system_uses_terminfo = 0;
//#endif
//
//  DEFVAR_LISP ("suspend-tty-functions", &Vsuspend_tty_functions,
//    doc: /* Functions to be run after suspending a tty.
//The functions are run with one argument, the terminal id to be suspended.
//See `suspend-tty'.  */);
//  Vsuspend_tty_functions = Qnil;
//
//
//  DEFVAR_LISP ("resume-tty-functions", &Vresume_tty_functions,
//    doc: /* Functions to be run after resuming a tty.
//The functions are run with one argument, the terminal id that was revived.
//See `resume-tty'.  */);
//  Vresume_tty_functions = Qnil;
//
//  DEFVAR_BOOL ("visible-cursor", &visible_cursor,
//	       doc: /* Non-nil means to make the cursor very visible.
//This only has an effect when running in a text terminal.
//What means \"very visible\" is up to your terminal.  It may make the cursor
//bigger, or it may make it blink, or it may do nothing at all.  */);
//  visible_cursor = 1;

//  defsubr (&Stty_display_color_p);
//  defsubr (&Stty_display_color_cells);
//  defsubr (&Stty_no_underline);
  defsubr (&Stty_type);
//  defsubr (&Scontrolling_tty_p);
//  defsubr (&Ssuspend_tty);
//  defsubr (&Sresume_tty);

  encode_terminal_src = NULL;
  encode_terminal_dst = NULL;
}


void
calculate_costs (struct frame *frame)
{
  FRAME_COST_BAUD_RATE (frame) = baud_rate;
}

struct fkey_table {
  char *cap, *name;
};

  /* Termcap capability names that correspond directly to X keysyms.
     Some of these (marked "terminfo") aren't supplied by old-style
     (Berkeley) termcap entries.  They're listed in X keysym order;
     except we put the keypad keys first, so that if they clash with
     other keys (as on the IBM PC keyboard) they get overridden.
  */

static struct fkey_table keys[] =
{
  {"kh", "home"},	/* termcap */
  {"kl", "left"},	/* termcap */
  {"ku", "up"},		/* termcap */
  {"kr", "right"},	/* termcap */
  {"kd", "down"},	/* termcap */
  {"%8", "prior"},	/* terminfo */
  {"%5", "next"},	/* terminfo */
  {"@7", "end"},	/* terminfo */
  {"@1", "begin"},	/* terminfo */
  {"*6", "select"},	/* terminfo */
  {"%9", "print"},	/* terminfo */
  {"@4", "execute"},	/* terminfo --- actually the `command' key */
  /*
   * "insert" --- see below
   */
  {"&8", "undo"},	/* terminfo */
  {"%0", "redo"},	/* terminfo */
  {"%7", "menu"},	/* terminfo --- actually the `options' key */
  {"@0", "find"},	/* terminfo */
  {"@2", "cancel"},	/* terminfo */
  {"%1", "help"},	/* terminfo */
  /*
   * "break" goes here, but can't be reliably intercepted with termcap
   */
  {"&4", "reset"},	/* terminfo --- actually `restart' */
  /*
   * "system" and "user" --- no termcaps
   */
  {"kE", "clearline"},	/* terminfo */
  {"kA", "insertline"},	/* terminfo */
  {"kL", "deleteline"},	/* terminfo */
  {"kI", "insertchar"},	/* terminfo */
  {"kD", "deletechar"},	/* terminfo */
  {"kB", "backtab"},	/* terminfo */
  /*
   * "kp_backtab", "kp-space", "kp-tab" --- no termcaps
   */
  {"@8", "kp-enter"},	/* terminfo */
  /*
   * "kp-f1", "kp-f2", "kp-f3" "kp-f4",
   * "kp-multiply", "kp-add", "kp-separator",
   * "kp-subtract", "kp-decimal", "kp-divide", "kp-0";
   * --- no termcaps for any of these.
   */
  {"K4", "kp-1"},	/* terminfo */
  /*
   * "kp-2" --- no termcap
   */
  {"K5", "kp-3"},	/* terminfo */
  /*
   * "kp-4" --- no termcap
   */
  {"K2", "kp-5"},	/* terminfo */
  /*
   * "kp-6" --- no termcap
   */
  {"K1", "kp-7"},	/* terminfo */
  /*
   * "kp-8" --- no termcap
   */
  {"K3", "kp-9"},	/* terminfo */
  /*
   * "kp-equal" --- no termcap
   */
  {"k1", "f1"},
  {"k2", "f2"},
  {"k3", "f3"},
  {"k4", "f4"},
  {"k5", "f5"},
  {"k6", "f6"},
  {"k7", "f7"},
  {"k8", "f8"},
  {"k9", "f9"},

  {"&0", "S-cancel"},    /*shifted cancel key*/
  {"&9", "S-begin"},     /*shifted begin key*/
  {"*0", "S-find"},      /*shifted find key*/
  {"*1", "S-execute"},   /*shifted execute? actually shifted command key*/
  {"*4", "S-delete"},    /*shifted delete-character key*/
  {"*7", "S-end"},       /*shifted end key*/
  {"*8", "S-clearline"}, /*shifted clear-to end-of-line key*/
  {"#1", "S-help"},      /*shifted help key*/
  {"#2", "S-home"},      /*shifted home key*/
  {"#3", "S-insert"},    /*shifted insert-character key*/
  {"#4", "S-left"},      /*shifted left-arrow key*/
  {"%d", "S-menu"},      /*shifted menu? actually shifted options key*/
  {"%c", "S-next"},      /*shifted next key*/
  {"%e", "S-prior"},     /*shifted previous key*/
  {"%f", "S-print"},     /*shifted print key*/
  {"%g", "S-redo"},      /*shifted redo key*/
  {"%i", "S-right"},     /*shifted right-arrow key*/
  {"!3", "S-undo"}       /*shifted undo key*/
  };

static char **term_get_fkeys_address;
static KBOARD *term_get_fkeys_kboard;

/***********************************************************************
		       Character Display Information
 ***********************************************************************/

/* Avoid name clash with functions defined in xterm.c */
#ifdef static
#define append_glyph append_glyph_term
#define produce_stretch_glyph produce_stretch_glyph_term
#define append_composite_glyph append_composite_glyph_term
#define produce_composite_glyph produce_composite_glyph_term
#endif

static void append_glyph P_ ((struct it *));
static void produce_stretch_glyph P_ ((struct it *));
static void append_composite_glyph P_ ((struct it *));
static void produce_composite_glyph P_ ((struct it *));

/* Append glyphs to IT's glyph_row.  Called from produce_glyphs for
   terminal frames if IT->glyph_row != NULL.  IT->char_to_display is
   the character for which to produce glyphs; IT->face_id contains the
   character's face.  Padding glyphs are appended if IT->c has a
   IT->pixel_width > 1.  */

static void
append_glyph (it)
     struct it *it;
{
  struct glyph *glyph, *end;
  int i;

  xassert (it->glyph_row);
  glyph = (it->glyph_row->glyphs[it->area]
	   + it->glyph_row->used[it->area]);
  end = it->glyph_row->glyphs[1 + it->area];

  for (i = 0;
       i < it->pixel_width && glyph < end;
       ++i)
    {
      glyph->type = CHAR_GLYPH;
      glyph->pixel_width = 1;
      glyph->u.ch = it->char_to_display;
      glyph->face_id = it->face_id;
      glyph->padding_p = i > 0;
      glyph->charpos = CHARPOS (it->position);
      glyph->object = it->object;

      ++it->glyph_row->used[it->area];
      ++glyph;
    }
}


/* Produce glyphs for the display element described by IT.  *IT
   specifies what we want to produce a glyph for (character, image, ...),
   and where in the glyph matrix we currently are (glyph row and hpos).
   produce_glyphs fills in output fields of *IT with information such as the
   pixel width and height of a character, and maybe output actual glyphs at
   the same time if IT->glyph_row is non-null.  See the explanation of
   struct display_iterator in dispextern.h for an overview.

   produce_glyphs also stores the result of glyph width, ascent
   etc. computations in *IT.

   IT->glyph_row may be null, in which case produce_glyphs does not
   actually fill in the glyphs.  This is used in the move_* functions
   in xdisp.c for text width and height computations.

   Callers usually don't call produce_glyphs directly;
   instead they use the macro PRODUCE_GLYPHS.  */

void
produce_glyphs (it)
     struct it *it;
{
  /* If a hook is installed, let it do the work.  */

  /* Nothing but characters are supported on terminal frames.  */
  xassert (it->what == IT_CHARACTER
	   || it->what == IT_COMPOSITION
	   || it->what == IT_STRETCH);

  if (it->what == IT_STRETCH)
    {
      produce_stretch_glyph (it);
      goto done;
    }

  if (it->what == IT_COMPOSITION)
    {
      produce_composite_glyph (it);
      goto done;
    }

  /* Maybe translate single-byte characters to multibyte.  */
  it->char_to_display = it->c;

  if (it->c >= 040 && it->c < 0177)
    {
      it->pixel_width = it->nglyphs = 1;
      if (it->glyph_row)
	append_glyph (it);
    }
  else if (it->c == '\n')
    it->pixel_width = it->nglyphs = 0;
  else if (it->c == '\t')
    {
      int absolute_x = (it->current_x
			+ it->continuation_lines_width);
      int next_tab_x
	= (((1 + absolute_x + it->tab_width - 1)
	    / it->tab_width)
	   * it->tab_width);
      int nspaces;

      /* If part of the TAB has been displayed on the previous line
	 which is continued now, continuation_lines_width will have
	 been incremented already by the part that fitted on the
	 continued line.  So, we will get the right number of spaces
	 here.  */
      nspaces = next_tab_x - absolute_x;

      if (it->glyph_row)
	{
	  int n = nspaces;

	  it->char_to_display = ' ';
	  it->pixel_width = it->len = 1;

	  while (n--)
	    append_glyph (it);
	}

      it->pixel_width = nspaces;
      it->nglyphs = nspaces;
    }
  else if (CHAR_BYTE8_P (it->c))
    {
      if (unibyte_display_via_language_environment
	  && (it->c >= 0240))
	{
	  it->char_to_display = unibyte_char_to_multibyte (it->c);
	  it->pixel_width = CHAR_WIDTH (it->char_to_display);
	  it->nglyphs = it->pixel_width;
	  if (it->glyph_row)
	    append_glyph (it);
	}
      else
	{
	  /* Coming here means that it->c is from display table, thus
	     we must send the raw 8-bit byte as is to the terminal.
	     Although there's no way to know how many columns it
	     occupies on a screen, it is a good assumption that a
	     single byte code has 1-column width.  */
	  it->pixel_width = it->nglyphs = 1;
	  if (it->glyph_row)
	    append_glyph (it);
	}
    }
  else
    {
      it->pixel_width = CHAR_WIDTH (it->c);
      it->nglyphs = it->pixel_width;

      if (it->glyph_row)
	append_glyph (it);
    }

 done:
  /* Advance current_x by the pixel width as a convenience for
     the caller.  */
  if (it->area == TEXT_AREA)
    it->current_x += it->pixel_width;
  it->ascent = it->max_ascent = it->phys_ascent = it->max_phys_ascent = 0;
  it->descent = it->max_descent = it->phys_descent = it->max_phys_descent = 1;
}


/* Produce a stretch glyph for iterator IT.  IT->object is the value
   of the glyph property displayed.  The value must be a list
   `(space KEYWORD VALUE ...)' with the following KEYWORD/VALUE pairs
   being recognized:

   1. `:width WIDTH' specifies that the space should be WIDTH *
   canonical char width wide.  WIDTH may be an integer or floating
   point number.

   2. `:align-to HPOS' specifies that the space should be wide enough
   to reach HPOS, a value in canonical character units.  */

static void
produce_stretch_glyph (it)
     struct it *it;
{
  /* (space :width WIDTH ...)  */
  Lisp_Object prop, plist;
  int width = 0, align_to = -1;
  int zero_width_ok_p = 0;
  double tem;

  /* List should start with `space'.  */
  xassert (CONSP (it->object) && EQ (XCAR (it->object), Qspace));
  plist = XCDR (it->object);

  /* Compute the width of the stretch.  */
  if ((prop = Fplist_get (plist, QCwidth), !NILP (prop))
      && calc_pixel_width_or_height (&tem, it, prop, 0, 1, 0))
    {
      /* Absolute width `:width WIDTH' specified and valid.  */
      zero_width_ok_p = 1;
      width = (int)(tem + 0.5);
    }
  else if ((prop = Fplist_get (plist, QCalign_to), !NILP (prop))
	   && calc_pixel_width_or_height (&tem, it, prop, 0, 1, &align_to))
    {
      if (it->glyph_row == NULL || !it->glyph_row->mode_line_p)
	align_to = (align_to < 0
		    ? 0
		    : align_to - window_box_left_offset (it->w, TEXT_AREA));
      else if (align_to < 0)
	align_to = window_box_left_offset (it->w, TEXT_AREA);
      width = max (0, (int)(tem + 0.5) + align_to - it->current_x);
      zero_width_ok_p = 1;
    }
  else
    /* Nothing specified -> width defaults to canonical char width.  */
    width = FRAME_COLUMN_WIDTH (it->f);

  if (width <= 0 && (width < 0 || !zero_width_ok_p))
    width = 1;

  if (width > 0 && it->glyph_row)
    {
      Lisp_Object o_object = it->object;
      Lisp_Object object = it->stack[it->sp - 1].string;
      int n = width;

      if (!STRINGP (object))
	object = it->w->buffer;
      it->object = object;
      it->char_to_display = ' ';
      it->pixel_width = it->len = 1;
      while (n--)
	append_glyph (it);
      it->object = o_object;
    }
  it->pixel_width = width;
  it->nglyphs = width;
}


/* Append glyphs to IT's glyph_row for the composition IT->cmp_id.
   Called from produce_composite_glyph for terminal frames if
   IT->glyph_row != NULL.  IT->face_id contains the character's
   face.  */

static void
append_composite_glyph (it)
     struct it *it;
{
  struct glyph *glyph;

  xassert (it->glyph_row);
  glyph = it->glyph_row->glyphs[it->area] + it->glyph_row->used[it->area];
  if (glyph < it->glyph_row->glyphs[1 + it->area])
    {
      glyph->type = COMPOSITE_GLYPH;
      glyph->pixel_width = it->pixel_width;
      glyph->u.cmp.id = it->cmp_it.id;
      if (it->cmp_it.ch < 0)
	{
	  glyph->u.cmp.automatic = 0;
	  glyph->u.cmp.id = it->cmp_it.id;
	}
      else
	{
	  glyph->u.cmp.automatic = 1;
	  glyph->u.cmp.id = it->cmp_it.id;
	  glyph->u.cmp.from = it->cmp_it.from;
	  glyph->u.cmp.to = it->cmp_it.to - 1;
	}

      glyph->face_id = it->face_id;
      glyph->padding_p = 0;
      glyph->charpos = CHARPOS (it->position);
      glyph->object = it->object;

      ++it->glyph_row->used[it->area];
      ++glyph;
    }
}


/* Produce a composite glyph for iterator IT.  IT->cmp_id is the ID of
   the composition.  We simply produces components of the composition
   assuming that that the terminal has a capability to layout/render
   it correctly.  */

static void
produce_composite_glyph (it)
     struct it *it;
{
  int c;

  if (it->cmp_it.ch < 0)
    {
      struct composition *cmp = composition_table[it->cmp_it.id];

      it->pixel_width = cmp->width;
    }
  else
    {
      Lisp_Object gstring = composition_gstring_from_id (it->cmp_it.id);

      it->pixel_width = composition_gstring_width (gstring, it->cmp_it.from,
						   it->cmp_it.to, NULL);
    }
  it->nglyphs = 1;
  if (it->glyph_row)
    append_composite_glyph (it);
}


/* Get information about special display element WHAT in an
   environment described by IT.  WHAT is one of IT_TRUNCATION or
   IT_CONTINUATION.  Maybe produce glyphs for WHAT if IT has a
   non-null glyph_row member.  This function ensures that fields like
   face_id, c, len of IT are left untouched.  */

void
produce_special_glyphs (it, what)
     struct it *it;
     enum display_element_type what;
{
  struct it temp_it;
  Lisp_Object gc;
  GLYPH glyph;

  temp_it = *it;
  temp_it.dp = NULL;
  temp_it.what = IT_CHARACTER;
  temp_it.len = 1;
  temp_it.object = make_number (0);
  bzero (&temp_it.current, sizeof temp_it.current);

  if (what == IT_CONTINUATION)
    {
      /* Continuation glyph.  */
      SET_GLYPH_FROM_CHAR (glyph, '\\');
      if (it->dp
	  && (gc = DISP_CONTINUE_GLYPH (it->dp), GLYPH_CODE_P (gc))
	  && GLYPH_CODE_CHAR_VALID_P (gc))
	{
	  SET_GLYPH_FROM_GLYPH_CODE (glyph, gc);
	  spec_glyph_lookup_face (XWINDOW (it->window), &glyph);
	}
    }
  else if (what == IT_TRUNCATION)
    {
      /* Truncation glyph.  */
      SET_GLYPH_FROM_CHAR (glyph, '$');
      if (it->dp
	  && (gc = DISP_TRUNC_GLYPH (it->dp), GLYPH_CODE_P (gc))
	  && GLYPH_CODE_CHAR_VALID_P (gc))
	{
	  SET_GLYPH_FROM_GLYPH_CODE (glyph, gc);
	  spec_glyph_lookup_face (XWINDOW (it->window), &glyph);
	}
    }
  else
    abort ();

  temp_it.c = GLYPH_CHAR (glyph);
  temp_it.face_id = GLYPH_FACE (glyph);
  temp_it.len = CHAR_BYTES (temp_it.c);

  produce_glyphs (&temp_it);
  it->pixel_width = temp_it.pixel_width;
  it->nglyphs = temp_it.pixel_width;
}

void
fatal (const char *str, ...)
{
  va_list ap;
  va_start (ap, str);
  fprintf (stderr, "emacs: ");
  vfprintf (stderr, str, ap);
  va_end (ap);
  fflush (stderr);
  exit (1);
}

/* arch-tag: 498e7449-6f2e-45e2-91dd-b7d4ca488193
   (do not change this comment) */
