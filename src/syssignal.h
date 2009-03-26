/* syssignal.h - System-dependent definitions for signals.
   Copyright (C) 1993, 1999, 2001, 2002, 2003, 2004,
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

extern void init_signals P_ ((void));

#ifndef sigunblock
#define sigunblock(SIG) \
{ SIGMASKTYPE omask = sigblock (SIGEMPTYMASK); sigsetmask (omask & ~SIG); }
#endif

#ifndef SIGMASKTYPE
#define SIGMASKTYPE int
#endif

#ifndef SIGEMPTYMASK
#define SIGEMPTYMASK (0)
#endif

#ifndef SIGFULLMASK
#define SIGFULLMASK (0xffffffff)
#endif

#ifndef sigmask
#define sigmask(no) (1L << ((no) - 1))
#endif

#ifndef sigunblock
#define sigunblock(SIG) \
{ SIGMASKTYPE omask = sigblock (SIGFULLMASK); sigsetmask (omask & ~SIG); }
#endif

#define sigfree() sigsetmask (SIGEMPTYMASK)

#if defined (SIGINFO) && defined (BROKEN_SIGINFO)
#undef SIGINFO
#endif
#if defined (SIGIO) && defined (BROKEN_SIGIO)
# undef SIGIO
# if defined (__Lynx__)
# undef SIGPOLL /* Defined as SIGIO on LynxOS */
# endif
#endif
#if defined (SIGPOLL) && defined (BROKEN_SIGPOLL)
#undef SIGPOLL
#endif
#if defined (SIGTSTP) && defined (BROKEN_SIGTSTP)
#undef SIGTSTP
#endif
#if defined (SIGURG) && defined (BROKEN_SIGURG)
#undef SIGURG
#endif
#if defined (SIGAIO) && defined (BROKEN_SIGAIO)
#undef SIGAIO
#endif
#if defined (SIGPTY) && defined (BROKEN_SIGPTY)
#undef SIGPTY
#endif


#if NSIG < NSIG_MINIMUM
# ifdef NSIG
#  undef NSIG
# endif
# define NSIG NSIG_MINIMUM
#endif

#define EMACS_KILLPG(gid, signo) (kill (gid, signo))

/* Define SIGCHLD as an alias for SIGCLD.  There are many conditionals
   testing SIGCHLD.  */
#ifdef SIGCLD
#ifndef SIGCHLD
#define SIGCHLD SIGCLD
#endif /* SIGCHLD */
#endif /* ! defined (SIGCLD) */

#ifndef HAVE_STRSIGNAL
/* strsignal is in sysdep.c */
char *strsignal ();
#endif

#define SIGNAL_THREAD_CHECK(signo)

/* arch-tag: 4580e86a-340d-4574-9e11-a742b6e1a152
   (do not change this comment) */
