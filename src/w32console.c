/* Terminal hooks for GNU Emacs on the Microsoft W32 API.
   Copyright (C) 1992, 1999, 2001, 2002, 2003, 2004, 2005, 2006, 2007,
                 2008, 2009  Free Software Foundation, Inc.

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

/*
   Tim Fleehart (apollo@online.com)		1-17-92
   Geoff Voelker (voelker@cs.washington.edu)	9-12-93
*/


#include <config.h>

#include <stdlib.h>
#include <stdio.h>
#include <windows.h>
#include <string.h>

#include "lisp.h"
#include "character.h"
#include "coding.h"
#include "disptab.h"
#include "frame.h"
#include "termhooks.h"
#include "termchar.h"
#include "dispextern.h"
#include "w32inevt.h"

/* from window.c */
extern Lisp_Object Frecenter ();

/* from keyboard.c */
extern int detect_input_pending ();

/* from sysdep.c */
extern int read_input_pending ();

static COORD	cursor_coords;
static HANDLE	prev_screen, cur_screen;
static WORD	char_attr_normal;
static DWORD   prev_console_mode;

#ifndef USE_SEPARATE_SCREEN
static CONSOLE_CURSOR_INFO prev_console_cursor;
#endif

extern Lisp_Object Vtty_defined_color_alist;

/* Determine whether to make frame dimensions match the screen buffer,
   or the current window size.  The former is desirable when running
   over telnet, while the latter is more useful when working directly at
   the console with a large scroll-back buffer.  */
int w32_use_full_screen_buffer;
HANDLE  keyboard_handle;


/* Setting this as the ctrl handler prevents emacs from being killed when
   someone hits ^C in a 'suspended' session (child shell).
   Also ignore Ctrl-Break signals.  */

BOOL
ctrl_c_handler (unsigned long type)
{
  /* Only ignore "interrupt" events when running interactively.  */
  return (!noninteractive
	  && (type == CTRL_C_EVENT || type == CTRL_BREAK_EVENT));
}

static struct glyph glyph_base[256];
static BOOL  ceol_initialized = FALSE;

#undef	LEFT
#undef	RIGHT
#define	LEFT	1
#define	RIGHT	0

static unsigned int sound_type = 0xFFFFFFFF;
#define MB_EMACS_SILENT (0xFFFFFFFF - 1)

void
w32_sys_ring_bell (struct frame *f)
{
  if (sound_type == 0xFFFFFFFF)
    {
      Beep (666, 100);
    }
  else if (sound_type == MB_EMACS_SILENT)
    {
      /* Do nothing.  */
    }
  else
    MessageBeep (sound_type);
}

DEFUN ("set-message-beep", Fset_message_beep, Sset_message_beep, 1, 1, 0,
       doc: /* Set the sound generated when the bell is rung.
SOUND is 'asterisk, 'exclamation, 'hand, 'question, 'ok, or 'silent
to use the corresponding system sound for the bell.  The 'silent sound
prevents Emacs from making any sound at all.
SOUND is nil to use the normal beep.  */)
     (sound)
     Lisp_Object sound;
{
  CHECK_SYMBOL (sound);

  if (NILP (sound))
      sound_type = 0xFFFFFFFF;
  else if (EQ (sound, intern ("asterisk")))
      sound_type = MB_ICONASTERISK;
  else if (EQ (sound, intern ("exclamation")))
      sound_type = MB_ICONEXCLAMATION;
  else if (EQ (sound, intern ("hand")))
      sound_type = MB_ICONHAND;
  else if (EQ (sound, intern ("question")))
      sound_type = MB_ICONQUESTION;
  else if (EQ (sound, intern ("ok")))
      sound_type = MB_OK;
  else if (EQ (sound, intern ("silent")))
      sound_type = MB_EMACS_SILENT;
  else
      sound_type = 0xFFFFFFFF;

  return sound;
}

typedef int (*term_hook) ();

DEFUN ("set-screen-color", Fset_screen_color, Sset_screen_color, 2, 2, 0,
       doc: /* Set screen colors.  */)
    (foreground, background)
    Lisp_Object foreground;
    Lisp_Object background;
{
  char_attr_normal = XFASTINT (foreground) + (XFASTINT (background) << 4);

  Frecenter (Qnil);
  return Qt;
}

DEFUN ("set-cursor-size", Fset_cursor_size, Sset_cursor_size, 1, 1, 0,
       doc: /* Set cursor size.  */)
    (size)
    Lisp_Object size;
{
  CONSOLE_CURSOR_INFO cci;
  cci.dwSize = XFASTINT (size);
  cci.bVisible = TRUE;
  (void) SetConsoleCursorInfo (cur_screen, &cci);

  return Qt;
}

void
syms_of_ntterm ()
{
  DEFVAR_BOOL ("w32-use-full-screen-buffer",
               &w32_use_full_screen_buffer,
	       doc: /* Non-nil means make terminal frames use the full screen buffer dimensions.
This is desirable when running Emacs over telnet.
A value of nil means use the current console window dimensions; this
may be preferrable when working directly at the console with a large
scroll-back buffer.  */);
  w32_use_full_screen_buffer = 0;

  defsubr (&Sset_screen_color);
  defsubr (&Sset_cursor_size);
  defsubr (&Sset_message_beep);
}

/* arch-tag: a390a07f-f661-42bc-aeb4-e6d8bf860337
   (do not change this comment) */
