/* systty.h - System-dependent definitions for terminals.
   Copyright (C) 1993, 1994, 2001, 2002, 2003, 2004,
                 2005, 2006, 2007, 2008, 2009  Free Software Foundation, Inc.

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

/* Interrupt input is not used if there is no FIONREAD.  */
#ifndef FIONREAD
#undef SIGIO
#endif

/* Try to establish the correct character to disable terminal functions
   in a system-independent manner.  Note that USG (at least) define
   _POSIX_VDISABLE as 0!  */
#define CDISABLE 255

/* Just ignore this for now and hope for the best */
#define EMACS_GET_TTY_PGRP(fd, pgid) 0
#define EMACS_SET_TTY_PGRP(fd, pgif) 0

/* EMACS_GETPGRP (arg) returns the process group of the process.  */
#define EMACS_GETPGRP(x) getpgrp(x)

/* Manipulate a TTY's input/output processing parameters.  */

/* struct emacs_tty is a structure used to hold the current tty
   parameters.  If the terminal has several structures describing its
   state, for example a struct tchars, a struct sgttyb, a struct
   tchars, a struct ltchars, and a struct pagechars, struct
   emacs_tty should contain an element for each parameter struct
   that Emacs may change.

   EMACS_GET_TTY (int FD, struct emacs_tty *P) stores the parameters
   of the tty on FD in *P.  Return zero if all's well, or -1 if we ran
   into an error we couldn't deal with.

   EMACS_SET_TTY (int FD, struct emacs_tty *P, int flushp)
   sets the parameters of the tty on FD according to the contents of
   *P.  If flushp is non-zero, we discard queued input to be
   written before making the change.
   Return 0 if all went well, and -1 if anything failed.

   EMACS_TTY_TABS_OK (struct emacs_tty *P) is false if the kernel
   expands tabs to spaces upon output; in that case, there is no
   advantage to using tabs over spaces.  */


/* For each tty parameter structure that Emacs might want to save and restore,
   - include an element for it in this structure, and
   - extend the emacs_{get,set}_tty functions in sysdep.c to deal with the
     new members.  */

struct emacs_tty {
/* There is always one of the following elements, so there is no need
   for dummy get and set definitions.  */
  int main;
};

/* Define EMACS_GET_TTY and EMACS_SET_TTY,
   the macros for reading and setting parts of `struct emacs_tty'.

   These got pretty unmanageable (huge macros are hard to debug), and
   finally needed some code which couldn't be done as part of an
   expression, so we moved them out to their own functions in sysdep.c.  */
#define EMACS_GET_TTY(fd, p)        (emacs_get_tty ((fd), (p)))
#define EMACS_SET_TTY(fd, p, waitp) (emacs_set_tty ((fd), (p), (waitp)))
#ifdef P_  /* Unfortunately this file is sometimes included before lisp.h */
extern int emacs_get_tty P_ ((int, struct emacs_tty *));
extern int emacs_set_tty P_ ((int, struct emacs_tty *, int));
#endif

/* Define EMACS_TTY_TABS_OK.  */
#define EMACS_TTY_TABS_OK(p) 0

/* arch-tag: cf4b90bc-be41-401c-be98-40619178a712
   (do not change this comment) */
