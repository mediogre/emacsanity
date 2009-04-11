/* Interfaces to system-dependent kernel and library entries.
   Copyright (C) 1985, 1986, 1987, 1988, 1993, 1994, 1995, 1999, 2000, 2001,
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

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include <signal.h>
#include <stdio.h>
#include <setjmp.h>
#ifdef HAVE_PWD_H
#include <pwd.h>
#include <grp.h>
#endif /* HAVE_PWD_H */
#ifdef HAVE_LIMITS_H
#include <limits.h>
#endif /* HAVE_LIMITS_H */
#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif
#ifdef HAVE_ALLOCA_H
#include <alloca.h>
#endif /* HAVE_ALLOCA_H */

#include "lisp.h"
/* Including stdlib.h isn't necessarily enough to get srandom
   declared, e.g. without __USE_XOPEN_EXTENDED with glibc 2.  */

/* The w32 build defines select stuff in w32.h, which is included by
   sys/select.h (included below).   */
#ifndef WINDOWSNT
#include "sysselect.h"
#endif

#include "blockinput.h"

#ifdef WINDOWSNT
#define read sys_read
#define write sys_write
#include <windows.h>
#ifndef NULL
#define NULL 0
#endif
#endif /* not WINDOWSNT */

/* Does anyone other than VMS need this? */
#ifndef fwrite
#define sys_fwrite fwrite
#else
#undef fwrite
#endif

#include <sys/types.h>
#include <sys/stat.h>
#include <errno.h>

#ifdef HAVE_SETPGID
#if !defined (USG) || defined (BSD_PGRPS)
#undef setpgrp
#define setpgrp setpgid
#endif
#endif

/* Get SI_SRPC_DOMAIN, if it is available.  */
#ifdef HAVE_SYS_SYSTEMINFO_H
#include <sys/systeminfo.h>
#endif

#ifndef USE_CRT_DLL
#ifndef errno
extern int errno;
#endif
#endif

#include <sys/file.h>

#ifdef HAVE_FCNTL_H
#include <fcntl.h>
#endif

#include <sys/ioctl.h>

#include "systty.h"
#include "syswait.h"

extern int quit_char;

#include "keyboard.h"
#include "frame.h"
#include "window.h"
#include "termhooks.h"
#include "termchar.h"
#include "termopts.h"
#include "dispextern.h"
#include "process.h"

/* For serial_configure and serial_open.  */
extern Lisp_Object QCport, QCspeed, QCprocess;
extern Lisp_Object QCbytesize, QCstopbits, QCparity, Qodd, Qeven;
extern Lisp_Object QCflowcontrol, Qhw, Qsw, QCsummary;

#ifdef WINDOWSNT
#include <direct.h>
/* In process.h which conflicts with the local copy.  */
#define _P_WAIT 0
int _cdecl _spawnlp (int, const char *, const char *, ...);
int _cdecl _getpid (void);
extern char *getwd (char *);
#endif

#include "syssignal.h"
#include "systime.h"
#ifdef HAVE_UTIME_H
#include <utime.h>
#endif

#ifndef HAVE_UTIMES
#ifndef HAVE_STRUCT_UTIMBUF
/* We want to use utime rather than utimes, but we couldn't find the
   structure declaration.  We'll use the traditional one.  */
struct utimbuf {
  long actime;
  long modtime;
};
#endif
#endif

/* LPASS8 is new in 4.3, and makes cbreak mode provide all 8 bits.  */
#ifndef LPASS8
#define LPASS8 0
#endif

static int baud_convert[] =
  {
    0, 50, 75, 110, 135, 150, 200, 300, 600, 1200,
    1800, 2400, 4800, 9600, 19200, 38400
  };

#ifdef HAVE_SPEED_T
#include <termios.h>
#else
#if defined (HAVE_LIBNCURSES) && ! defined (NCURSES_OSPEED_T)
#else
#if defined (HAVE_TERMIOS_H) && defined (GNU_LINUX)
#include <termios.h>
#endif
#endif
#endif

int emacs_ospeed;

NO_RETURN void croak P_ ((char *)) ;

/* Temporary used by `sigblock' when defined in terms of signprocmask.  */

SIGMASKTYPE sigprocmask_set;


#if !defined (HAVE_GET_CURRENT_DIR_NAME) || defined (BROKEN_GET_CURRENT_DIR_NAME)

/* Return the current working directory.  Returns NULL on errors.
   Any other returned value must be freed with free. This is used
   only when get_current_dir_name is not defined on the system.  */
char*
get_current_dir_name ()
{
  char *buf;
  char *pwd;
  struct stat dotstat, pwdstat;
  /* If PWD is accurate, use it instead of calling getwd.  PWD is
     sometimes a nicer name, and using it may avoid a fatal error if a
     parent directory is searchable but not readable.  */
    if ((pwd = getenv ("PWD")) != 0
      && (IS_DIRECTORY_SEP (*pwd) || (*pwd && IS_DEVICE_SEP (pwd[1])))
      && stat (pwd, &pwdstat) == 0
      && stat (".", &dotstat) == 0
      && dotstat.st_ino == pwdstat.st_ino
      && dotstat.st_dev == pwdstat.st_dev
#ifdef MAXPATHLEN
      && strlen (pwd) < MAXPATHLEN
#endif
      )
    {
      buf = (char *) malloc (strlen (pwd) + 1);
      if (!buf)
        return NULL;
      strcpy (buf, pwd);
    }
#ifdef HAVE_GETCWD
  else
    {
      size_t buf_size = 1024;
      buf = (char *) malloc (buf_size);
      if (!buf)
        return NULL;
      for (;;)
        {
          if (getcwd (buf, buf_size) == buf)
            break;
          if (errno != ERANGE)
            {
              int tmp_errno = errno;
              free (buf);
              errno = tmp_errno;
              return NULL;
            }
          buf_size *= 2;
          buf = (char *) realloc (buf, buf_size);
          if (!buf)
            return NULL;
        }
    }
#else
  else
    {
      /* We need MAXPATHLEN here.  */
      buf = (char *) malloc (MAXPATHLEN + 1);
      if (!buf)
        return NULL;
      if (getwd (buf) == NULL)
        {
          int tmp_errno = errno;
          free (buf);
          errno = tmp_errno;
          return NULL;
        }
    }
#endif
  return buf;
}
#endif


/* Discard pending input on all input descriptors.  */

void
discard_tty_input ()
{
}

void
init_baud_rate (int fd)
{
  if (noninteractive)
    emacs_ospeed = 0;
  else
    {
#ifdef DOS_NT
    emacs_ospeed = 15;
#else  /* not DOS_NT */
#ifdef HAVE_TERMIOS
      struct termios sg;

      sg.c_cflag = B9600;
      tcgetattr (fd, &sg);
      emacs_ospeed = cfgetospeed (&sg);
#else /* not TERMIOS */
#ifdef HAVE_TERMIO
      struct termio sg;

      sg.c_cflag = B9600;
#ifdef HAVE_TCATTR
      tcgetattr (fd, &sg);
#else
      ioctl (fd, TCGETA, &sg);
#endif
      emacs_ospeed = sg.c_cflag & CBAUD;
#else /* neither TERMIOS nor TERMIO */
      struct sgttyb sg;

      sg.sg_ospeed = B9600;
      if (ioctl (fd, TIOCGETP, &sg) < 0)
	abort ();
      emacs_ospeed = sg.sg_ospeed;
#endif /* not HAVE_TERMIO */
#endif /* not HAVE_TERMIOS */
#endif /* not DOS_NT */
    }

  baud_rate = (emacs_ospeed < sizeof baud_convert / sizeof baud_convert[0]
	       ? baud_convert[emacs_ospeed] : 9600);
  if (baud_rate == 0)
    baud_rate = 1200;
}


/*ARGSUSED*/
void
set_exclusive_use (fd)
     int fd;
{
#ifdef FIOCLEX
  ioctl (fd, FIOCLEX, 0);
#endif
  /* Ok to do nothing if this feature does not exist */
}

#ifndef subprocesses

wait_without_blocking ()
{
#ifdef BSD_SYSTEM
  wait3 (0, WNOHANG | WUNTRACED, 0);
#else
  croak ("wait_without_blocking");
#endif
  synch_process_alive = 0;
}

#endif /* not subprocesses */

int wait_debugging;   /* Set nonzero to make following function work under dbx
			 (at least for bsd).  */

SIGTYPE
wait_for_termination_signal ()
{}

/* Wait for subprocess with process id `pid' to terminate and
   make sure it will get eliminated (not remain forever as a zombie) */

void
wait_for_termination (pid)
     int pid;
{
  while (1)
    {
      wait (0);
      break;
    }
}

#ifdef subprocesses

/*
 *	flush any pending output
 *      (may flush input as well; it does not matter the way we use it)
 */

void
flush_pending_output (channel)
     int channel;
{
#ifdef HAVE_TERMIOS
  /* If we try this, we get hit with SIGTTIN, because
     the child's tty belongs to the child's pgrp. */
#else
#ifdef TCFLSH
  ioctl (channel, TCFLSH, 1);
#else
#ifdef TIOCFLUSH
  int zero = 0;
  /* 3rd arg should be ignored
     but some 4.2 kernels actually want the address of an int
     and nonzero means something different.  */
  ioctl (channel, TIOCFLUSH, &zero);
#endif
#endif
#endif
}

/*  Set up the terminal at the other end of a pseudo-terminal that
    we will be controlling an inferior through.
    It should not echo or do line-editing, since that is done
    in Emacs.  No padding needed for insertion into an Emacs buffer.  */

void
child_setup_tty (out)
     int out;
{
#ifndef DOS_NT
  struct emacs_tty s;

  EMACS_GET_TTY (out, &s);

#if defined (HAVE_TERMIO) || defined (HAVE_TERMIOS)
  s.main.c_oflag |= OPOST;	/* Enable output postprocessing */
  s.main.c_oflag &= ~ONLCR;	/* Disable map of NL to CR-NL on output */
#ifdef NLDLY
  /* http://lists.gnu.org/archive/html/emacs-devel/2008-05/msg00406.html
     Some versions of GNU Hurd do not have FFDLY?  */
#ifdef FFDLY
  s.main.c_oflag &= ~(NLDLY|CRDLY|TABDLY|BSDLY|VTDLY|FFDLY);
  				/* No output delays */
#else
  s.main.c_oflag &= ~(NLDLY|CRDLY|TABDLY|BSDLY|VTDLY);
  				/* No output delays */
#endif
#endif
  s.main.c_lflag &= ~ECHO;	/* Disable echo */
  s.main.c_lflag |= ISIG;	/* Enable signals */
#ifdef IUCLC
  s.main.c_iflag &= ~IUCLC;	/* Disable downcasing on input.  */
#endif
#ifdef ISTRIP
  s.main.c_iflag &= ~ISTRIP;	/* don't strip 8th bit on input */
#endif
#ifdef OLCUC
  s.main.c_oflag &= ~OLCUC;	/* Disable upcasing on output.  */
#endif
  s.main.c_oflag &= ~TAB3;	/* Disable tab expansion */
  s.main.c_cflag = (s.main.c_cflag & ~CSIZE) | CS8; /* Don't strip 8th bit */
  s.main.c_lflag |= ICANON;	/* Enable erase/kill and eof processing */
  s.main.c_cc[VEOF] = 04;	/* insure that EOF is Control-D */
  s.main.c_cc[VERASE] = CDISABLE;	/* disable erase processing */
  s.main.c_cc[VKILL] = CDISABLE;	/* disable kill processing */

#ifdef HPUX
  s.main.c_cflag = (s.main.c_cflag & ~CBAUD) | B9600; /* baud rate sanity */
#endif /* HPUX */

#ifdef SIGNALS_VIA_CHARACTERS
  /* the QUIT and INTR character are used in process_send_signal
     so set them here to something useful.  */
  if (s.main.c_cc[VQUIT] == CDISABLE)
    s.main.c_cc[VQUIT] = '\\'&037;	/* Control-\ */
  if (s.main.c_cc[VINTR] == CDISABLE)
    s.main.c_cc[VINTR] = 'C'&037;	/* Control-C */
#endif /* not SIGNALS_VIA_CHARACTERS */

#ifdef AIX
/* AIX enhanced edit loses NULs, so disable it */
#ifndef IBMR2AIX
  s.main.c_line = 0;
  s.main.c_iflag &= ~ASCEDIT;
#endif
  /* Also, PTY overloads NUL and BREAK.
     don't ignore break, but don't signal either, so it looks like NUL.  */
  s.main.c_iflag &= ~IGNBRK;
  s.main.c_iflag &= ~BRKINT;
  /* rms: Formerly it set s.main.c_cc[VINTR] to 0377 here
     unconditionally.  Then a SIGNALS_VIA_CHARACTERS conditional
     would force it to 0377.  That looks like duplicated code.  */
#ifndef SIGNALS_VIA_CHARACTERS
  /* QUIT and INTR work better as signals, so disable character forms */
  s.main.c_cc[VQUIT] = CDISABLE;
  s.main.c_cc[VINTR] = CDISABLE;
  s.main.c_lflag &= ~ISIG;
#endif /* no TIOCGPGRP or no TIOCGLTC or no TIOCGETC */
  s.main.c_cc[VEOL] = CDISABLE;
  s.main.c_cflag = (s.main.c_cflag & ~CBAUD) | B9600; /* baud rate sanity */
#endif /* AIX */

#else /* not HAVE_TERMIO */

  s.main.sg_flags &= ~(ECHO | CRMOD | ANYP | ALLDELAY | RAW | LCASE
		       | CBREAK | TANDEM);
  s.main.sg_flags |= LPASS8;
  s.main.sg_erase = 0377;
  s.main.sg_kill = 0377;
  s.lmode = LLITOUT | s.lmode;        /* Don't strip 8th bit */

#endif /* not HAVE_TERMIO */

  EMACS_SET_TTY (out, &s, 0);

#endif /* not DOS_NT */
}

#endif /* subprocesses */

/* Record a signal code and the handler for it.  */
struct save_signal
{
  int code;
  SIGTYPE (*handler) P_ ((int));
};

static void save_signal_handlers P_ ((struct save_signal *));
static void restore_signal_handlers P_ ((struct save_signal *));

/* Suspend the Emacs process; give terminal to its superior.  */

void
sys_suspend ()
{
/* On a system where suspending is not implemented,
   instead fork a subshell and let it talk directly to the terminal
   while we wait.  */
  sys_subshell ();
}

/* Fork a subshell.  */

void
sys_subshell ()
{
#ifdef DOS_NT	/* Demacs 1.1.2 91/10/20 Manabu Higashida */
  int st;
  char oldwd[MAXPATHLEN+1]; /* Fixed length is safe on MSDOS.  */
#endif
  int pid;
  struct save_signal saved_handlers[5];
  Lisp_Object dir;
  unsigned char *str = 0;
  int len;

  saved_handlers[0].code = SIGINT;
  saved_handlers[1].code = SIGQUIT;
  saved_handlers[2].code = SIGTERM;
#ifdef SIGIO
  saved_handlers[3].code = SIGIO;
  saved_handlers[4].code = 0;
#else
  saved_handlers[3].code = 0;
#endif

  /* Mentioning current_buffer->buffer would mean including buffer.h,
     which somehow wedges the hp compiler.  So instead...  */

  dir = intern ("default-directory");
  if (NILP (Fboundp (dir)))
    goto xyzzy;
  dir = Fsymbol_value (dir);
  if (!STRINGP (dir))
    goto xyzzy;

  dir = expand_and_dir_to_file (Funhandled_file_name_directory (dir), Qnil);
  str = (unsigned char *) alloca (SCHARS (dir) + 2);
  len = SCHARS (dir);
  bcopy (SDATA (dir), str, len);
  if (str[len - 1] != '/') str[len++] = '/';
  str[len] = 0;
 xyzzy:

#ifdef DOS_NT
  pid = 0;
#if __DJGPP__ > 1
  save_signal_handlers (saved_handlers);
  synch_process_alive = 1;
#endif /* __DJGPP__ > 1 */
#else
  pid = vfork ();
  if (pid == -1)
    error ("Can't spawn subshell");
#endif

  if (pid == 0)
    {
      char *sh = 0;

#ifdef DOS_NT    /* MW, Aug 1993 */
      getwd (oldwd);
      if (sh == 0)
	sh = (char *) egetenv ("SUSPEND");	/* KFS, 1994-12-14 */
#endif
      if (sh == 0)
	sh = (char *) egetenv ("SHELL");
      if (sh == 0)
	sh = "sh";

      /* Use our buffer's default directory for the subshell.  */
      if (str)
	chdir ((char *) str);

#ifdef subprocesses
      close_process_descs ();	/* Close Emacs's pipes/ptys */
#endif

      /* Waits for process completion */
      pid = _spawnlp (_P_WAIT, sh, sh, NULL);
      chdir (oldwd);
      if (pid == -1)
	write (1, "Can't execute subshell", 22);
    }

  /* Do this now if we did not do it before.  */
  save_signal_handlers (saved_handlers);
  synch_process_alive = 1;

#ifndef DOS_NT
  wait_for_termination (pid);
#endif
  restore_signal_handlers (saved_handlers);
  synch_process_alive = 0;
}

static void
save_signal_handlers (saved_handlers)
     struct save_signal *saved_handlers;
{
  while (saved_handlers->code)
    {
      saved_handlers->handler
	= (SIGTYPE (*) P_ ((int))) signal (saved_handlers->code, SIG_IGN);
      saved_handlers++;
    }
}

static void
restore_signal_handlers (saved_handlers)
     struct save_signal *saved_handlers;
{
  while (saved_handlers->code)
    {
      signal (saved_handlers->code, saved_handlers->handler);
      saved_handlers++;
    }
}

/* If SIGIO is broken, don't do anything. */
void
init_sigio (int fd)
{
}

void
reset_sigio (int fd)
{
}

void
request_sigio (void)
{
}

void
unrequest_sigio (void)
{
}

/* Saving and restoring the process group of Emacs's terminal.  */

#ifdef BSD_PGRPS

/* The process group of which Emacs was a member when it initially
   started.

   If Emacs was in its own process group (i.e. inherited_pgroup ==
   getpid ()), then we know we're running under a shell with job
   control (Emacs would never be run as part of a pipeline).
   Everything is fine.

   If Emacs was not in its own process group, then we know we're
   running under a shell (or a caller) that doesn't know how to
   separate itself from Emacs (like sh).  Emacs must be in its own
   process group in order to receive SIGIO correctly.  In this
   situation, we put ourselves in our own pgroup, forcibly set the
   tty's pgroup to our pgroup, and make sure to restore and reinstate
   the tty's pgroup just like any other terminal setting.  If
   inherited_group was not the tty's pgroup, then we'll get a
   SIGTTmumble when we try to change the tty's pgroup, and a CONT if
   it goes foreground in the future, which is what should happen.

   This variable is initialized in emacs.c.  */
int inherited_pgroup;

/* Split off the foreground process group to Emacs alone.  When we are
   in the foreground, but not started in our own process group,
   redirect the tty device handle FD to point to our own process
   group.  We need to be in our own process group to receive SIGIO
   properly.  */
static void
narrow_foreground_group (int fd)
{
  int me = getpid ();

  setpgrp (0, inherited_pgroup);
#if 0
  /* XXX inherited_pgroup should not be zero here, but GTK seems to
     mess this up. */
  if (! inherited_pgroup)
    abort ();                   /* Should not happen. */
#endif
  if (inherited_pgroup != me)
      EMACS_SET_TTY_PGRP (fd, &me); /* XXX This only works on the controlling tty. */
  setpgrp (0, me);
}

/* Set the tty to our original foreground group.  */
static void
widen_foreground_group (int fd)
{
  if (inherited_pgroup != getpid ())
    EMACS_SET_TTY_PGRP (fd, &inherited_pgroup);
  setpgrp (0, inherited_pgroup);
}

#endif /* BSD_PGRPS */

/* Getting and setting emacs_tty structures.  */

/* Set *TC to the parameters associated with the terminal FD.
   Return zero if all's well, or -1 if we ran into an error we
   couldn't deal with.  */
int
emacs_get_tty (fd, settings)
     int fd;
     struct emacs_tty *settings;
{
  /* Retrieve the primary parameters - baud rate, character size, etcetera.  */
#ifdef HAVE_TCATTR
  /* We have those nifty POSIX tcmumbleattr functions.  */
  bzero (&settings->main, sizeof (settings->main));
  if (tcgetattr (fd, &settings->main) < 0)
    return -1;

#else
#ifdef HAVE_TERMIO
  /* The SYSV-style interface?  */
  if (ioctl (fd, TCGETA, &settings->main) < 0)
    return -1;

#else
#ifndef DOS_NT
  /* I give up - I hope you have the BSD ioctls.  */
  if (ioctl (fd, TIOCGETP, &settings->main) < 0)
    return -1;
#endif /* not DOS_NT */
#endif
#endif

  /* Suivant - Do we have to get struct ltchars data?  */
#ifdef HAVE_LTCHARS
  if (ioctl (fd, TIOCGLTC, &settings->ltchars) < 0)
    return -1;
#endif

  /* How about a struct tchars and a wordful of lmode bits?  */
#ifdef HAVE_TCHARS
  if (ioctl (fd, TIOCGETC, &settings->tchars) < 0
      || ioctl (fd, TIOCLGET, &settings->lmode) < 0)
    return -1;
#endif

  /* We have survived the tempest.  */
  return 0;
}


/* Set the parameters of the tty on FD according to the contents of
   *SETTINGS.  If FLUSHP is non-zero, we discard input.
   Return 0 if all went well, and -1 if anything failed.  */

int
emacs_set_tty (fd, settings, flushp)
     int fd;
     struct emacs_tty *settings;
     int flushp;
{
  /* Set the primary parameters - baud rate, character size, etcetera.  */
#ifdef HAVE_TCATTR
  int i;
  /* We have those nifty POSIX tcmumbleattr functions.
     William J. Smith <wjs@wiis.wang.com> writes:
     "POSIX 1003.1 defines tcsetattr to return success if it was
     able to perform any of the requested actions, even if some
     of the requested actions could not be performed.
     We must read settings back to ensure tty setup properly.
     AIX requires this to keep tty from hanging occasionally."  */
  /* This make sure that we don't loop indefinitely in here.  */
  for (i = 0 ; i < 10 ; i++)
    if (tcsetattr (fd, flushp ? TCSAFLUSH : TCSADRAIN, &settings->main) < 0)
      {
	if (errno == EINTR)
	  continue;
	else
	  return -1;
      }
    else
      {
	struct termios new;

	bzero (&new, sizeof (new));
	/* Get the current settings, and see if they're what we asked for.  */
	tcgetattr (fd, &new);
	/* We cannot use memcmp on the whole structure here because under
	 * aix386 the termios structure has some reserved field that may
	 * not be filled in.
	 */
	if (   new.c_iflag == settings->main.c_iflag
	    && new.c_oflag == settings->main.c_oflag
	    && new.c_cflag == settings->main.c_cflag
	    && new.c_lflag == settings->main.c_lflag
	    && memcmp (new.c_cc, settings->main.c_cc, NCCS) == 0)
	  break;
	else
	  continue;
      }

#else
#ifdef HAVE_TERMIO
  /* The SYSV-style interface?  */
  if (ioctl (fd, flushp ? TCSETAF : TCSETAW, &settings->main) < 0)
    return -1;

#else
#ifndef DOS_NT
  /* I give up - I hope you have the BSD ioctls.  */
  if (ioctl (fd, (flushp) ? TIOCSETP : TIOCSETN, &settings->main) < 0)
    return -1;
#endif /* not DOS_NT */

#endif
#endif

  /* Suivant - Do we have to get struct ltchars data?  */
#ifdef HAVE_LTCHARS
  if (ioctl (fd, TIOCSLTC, &settings->ltchars) < 0)
    return -1;
#endif

  /* How about a struct tchars and a wordful of lmode bits?  */
#ifdef HAVE_TCHARS
  if (ioctl (fd, TIOCSETC, &settings->tchars) < 0
      || ioctl (fd, TIOCLSET, &settings->lmode) < 0)
    return -1;
#endif

  /* We have survived the tempest.  */
  return 0;
}



#ifdef F_SETOWN
int old_fcntl_owner[MAXDESC];
#endif /* F_SETOWN */

/* This may also be defined in stdio,
   but if so, this does no harm,
   and using the same name avoids wasting the other one's space.  */

#if defined (USG)
unsigned char _sobuf[BUFSIZ+8];
#else
char _sobuf[BUFSIZ];
#endif

#ifdef HAVE_LTCHARS
static struct ltchars new_ltchars = {-1,-1,-1,-1,-1,-1};
#endif
#ifdef HAVE_TCHARS
static struct tchars new_tchars = {-1,-1,-1,-1,-1,-1};
#endif

/* Initialize the terminal mode on all tty devices that are currently
   open. */

void
init_all_sys_modes (void)
{
    // NOP in Emacsanity
}

/* Set the logical window size associated with descriptor FD
   to HEIGHT and WIDTH.  This is used mainly with ptys.  */

int
set_window_size (fd, height, width)
     int fd, height, width;
{
  return -1;
}



/* Prepare all terminal devices for exiting Emacs. */

void
reset_all_sys_modes (void)
{
    // NOP in Emacsanity
}

#if !defined(CANNOT_DUMP) || !defined(SYSTEM_MALLOC)
/* Some systems that cannot dump also cannot implement these.  */

/*
 *	Return the address of the start of the text segment prior to
 *	doing an unexec.  After unexec the return value is undefined.
 *	See crt0.c for further explanation and _start.
 *
 */

#if !(defined (__NetBSD__) && defined (__ELF__))
#ifndef HAVE_TEXT_START
char *
start_of_text ()
{
#ifdef TEXT_START
  return ((char *) TEXT_START);
#else
  extern int _start ();
  return ((char *) _start);
#endif /* TEXT_START */
}
#endif /* not HAVE_TEXT_START */
#endif

/*
 *	Return the address of the start of the data segment prior to
 *	doing an unexec.  After unexec the return value is undefined.
 *	See crt0.c for further information and definition of data_start.
 *
 *	Apparently, on BSD systems this is etext at startup.  On
 *	USG systems (swapping) this is highly mmu dependent and
 *	is also dependent on whether or not the program is running
 *	with shared text.  Generally there is a (possibly large)
 *	gap between end of text and start of data with shared text.
 *
 *	On Uniplus+ systems with shared text, data starts at a
 *	fixed address.  Each port (from a given oem) is generally
 *	different, and the specific value of the start of data can
 *	be obtained via the UniPlus+ specific "uvar" system call,
 *	however the method outlined in crt0.c seems to be more portable.
 *
 *	Probably what will have to happen when a USG unexec is available,
 *	at least on UniPlus, is temacs will have to be made unshared so
 *	that text and data are contiguous.  Then once loadup is complete,
 *	unexec will produce a shared executable where the data can be
 *	at the normal shared text boundary and the startofdata variable
 *	will be patched by unexec to the correct value.
 *
 */

#ifndef start_of_data
char *
start_of_data ()
{
#ifdef DATA_START
  return ((char *) DATA_START);
#else
#ifdef ORDINARY_LINK
  /*
   * This is a hack.  Since we're not linking crt0.c or pre_crt0.c,
   * data_start isn't defined.  We take the address of environ, which
   * is known to live at or near the start of the system crt0.c, and
   * we don't sweat the handful of bytes that might lose.
   */
  extern char **environ;

  return ((char *) &environ);
#else
  extern int data_start;
  return ((char *) &data_start);
#endif /* ORDINARY_LINK */
#endif /* DATA_START */
}
#endif /* start_of_data */
#endif /* NEED_STARTS (not CANNOT_DUMP or not SYSTEM_MALLOC) */

/* init_system_name sets up the string for the Lisp function
   system-name to return. */

extern Lisp_Object Vsystem_name;

#ifdef HAVE_SOCKETS
#include <sys/socket.h>
#include <netdb.h>
#endif /* HAVE_SOCKETS */

#ifdef TRY_AGAIN
#ifndef HAVE_H_ERRNO
extern int h_errno;
#endif
#endif /* TRY_AGAIN */

void
init_system_name ()
{
#ifndef HAVE_GETHOSTNAME
  struct utsname uts;
  uname (&uts);
  Vsystem_name = build_string (uts.nodename);
#else /* HAVE_GETHOSTNAME */
  unsigned int hostname_size = 256;
  char *hostname = (char *) alloca (hostname_size);

  /* Try to get the host name; if the buffer is too short, try
     again.  Apparently, the only indication gethostname gives of
     whether the buffer was large enough is the presence or absence
     of a '\0' in the string.  Eech.  */
  for (;;)
    {
      gethostname (hostname, hostname_size - 1);
      hostname[hostname_size - 1] = '\0';

      /* Was the buffer large enough for the '\0'?  */
      if (strlen (hostname) < hostname_size - 1)
	break;

      hostname_size <<= 1;
      hostname = (char *) alloca (hostname_size);
    }
#ifdef HAVE_SOCKETS
  /* Turn the hostname into the official, fully-qualified hostname.
     Don't do this if we're going to dump; this can confuse system
     libraries on some machines and make the dumped emacs core dump. */
    if (! index (hostname, '.'))
      {
	int count;
#ifdef HAVE_GETADDRINFO
        struct addrinfo *res;
        struct addrinfo hints;
        int ret;

        memset (&hints, 0, sizeof(hints));
        hints.ai_socktype = SOCK_STREAM;
        hints.ai_flags = AI_CANONNAME;

	for (count = 0;; count++)
	  {
            if ((ret = getaddrinfo (hostname, NULL, &hints, &res)) == 0
                || ret != EAI_AGAIN)
              break;

            if (count >= 5)
	      break;
	    Fsleep_for (make_number (1), Qnil);
	  }

        if (ret == 0)
          {
            struct addrinfo *it = res;
            while (it)
              {
                char *fqdn = it->ai_canonname;
                if (fqdn && index (fqdn, '.')
                    && strcmp (fqdn, "localhost.localdomain") != 0)
                  break;
                it = it->ai_next;
              }
            if (it)
              {
                hostname = alloca (strlen (it->ai_canonname) + 1);
                strcpy (hostname, it->ai_canonname);
              }
            freeaddrinfo (res);
          }
#else /* !HAVE_GETADDRINFO */
        struct hostent *hp;
	for (count = 0;; count++)
	  {

#ifdef TRY_AGAIN
	    h_errno = 0;
#endif
	    hp = gethostbyname (hostname);
#ifdef TRY_AGAIN
	    if (! (hp == 0 && h_errno == TRY_AGAIN))
#endif

	      break;

	    if (count >= 5)
	      break;
	    Fsleep_for (make_number (1), Qnil);
	  }

	if (hp)
	  {
	    char *fqdn = (char *) hp->h_name;

	    if (!index (fqdn, '.'))
	      {
		/* We still don't have a fully qualified domain name.
		   Try to find one in the list of alternate names */
		char **alias = hp->h_aliases;
		while (*alias
		       && (!index (*alias, '.')
			   || !strcmp (*alias, "localhost.localdomain")))
		  alias++;
		if (*alias)
		  fqdn = *alias;
	      }
	    hostname = fqdn;
	  }
#endif /* !HAVE_GETADDRINFO */
      }
#endif /* HAVE_SOCKETS */
  Vsystem_name = build_string (hostname);
#endif /* HAVE_GETHOSTNAME */
  {
    unsigned char *p;
    for (p = SDATA (Vsystem_name); *p; p++)
      if (*p == ' ' || *p == '\t')
	*p = '-';
  }
}

#if !defined HAVE_STRSIGNAL && !HAVE_DECL_SYS_SIGLIST
static char *my_sys_siglist[NSIG];
# ifdef sys_siglist
#  undef sys_siglist
# endif
# define sys_siglist my_sys_siglist
#endif

void
init_signals ()
{
#ifdef POSIX_SIGNALS
  sigemptyset (&empty_mask);
  sigfillset (&full_mask);
#endif

#if !defined HAVE_STRSIGNAL && !HAVE_DECL_SYS_SIGLIST
  if (! initialized)
    {
# ifdef SIGABRT
      sys_siglist[SIGABRT] = "Aborted";
# endif
# ifdef SIGAIO
      sys_siglist[SIGAIO] = "LAN I/O interrupt";
# endif
# ifdef SIGALRM
      sys_siglist[SIGALRM] = "Alarm clock";
# endif
# ifdef SIGBUS
      sys_siglist[SIGBUS] = "Bus error";
# endif
# ifdef SIGCLD
      sys_siglist[SIGCLD] = "Child status changed";
# endif
# ifdef SIGCHLD
      sys_siglist[SIGCHLD] = "Child status changed";
# endif
# ifdef SIGCONT
      sys_siglist[SIGCONT] = "Continued";
# endif
# ifdef SIGDANGER
      sys_siglist[SIGDANGER] = "Swap space dangerously low";
# endif
# ifdef SIGDGNOTIFY
      sys_siglist[SIGDGNOTIFY] = "Notification message in queue";
# endif
# ifdef SIGEMT
      sys_siglist[SIGEMT] = "Emulation trap";
# endif
# ifdef SIGFPE
      sys_siglist[SIGFPE] = "Arithmetic exception";
# endif
# ifdef SIGFREEZE
      sys_siglist[SIGFREEZE] = "SIGFREEZE";
# endif
# ifdef SIGGRANT
      sys_siglist[SIGGRANT] = "Monitor mode granted";
# endif
# ifdef SIGHUP
      sys_siglist[SIGHUP] = "Hangup";
# endif
# ifdef SIGILL
      sys_siglist[SIGILL] = "Illegal instruction";
# endif
# ifdef SIGINT
      sys_siglist[SIGINT] = "Interrupt";
# endif
# ifdef SIGIO
      sys_siglist[SIGIO] = "I/O possible";
# endif
# ifdef SIGIOINT
      sys_siglist[SIGIOINT] = "I/O intervention required";
# endif
# ifdef SIGIOT
      sys_siglist[SIGIOT] = "IOT trap";
# endif
# ifdef SIGKILL
      sys_siglist[SIGKILL] = "Killed";
# endif
# ifdef SIGLOST
      sys_siglist[SIGLOST] = "Resource lost";
# endif
# ifdef SIGLWP
      sys_siglist[SIGLWP] = "SIGLWP";
# endif
# ifdef SIGMSG
      sys_siglist[SIGMSG] = "Monitor mode data available";
# endif
# ifdef SIGPHONE
      sys_siglist[SIGWIND] = "SIGPHONE";
# endif
# ifdef SIGPIPE
      sys_siglist[SIGPIPE] = "Broken pipe";
# endif
# ifdef SIGPOLL
      sys_siglist[SIGPOLL] = "Pollable event occurred";
# endif
# ifdef SIGPROF
      sys_siglist[SIGPROF] = "Profiling timer expired";
# endif
# ifdef SIGPTY
      sys_siglist[SIGPTY] = "PTY I/O interrupt";
# endif
# ifdef SIGPWR
      sys_siglist[SIGPWR] = "Power-fail restart";
# endif
# ifdef SIGQUIT
      sys_siglist[SIGQUIT] = "Quit";
# endif
# ifdef SIGRETRACT
      sys_siglist[SIGRETRACT] = "Need to relinguish monitor mode";
# endif
# ifdef SIGSAK
      sys_siglist[SIGSAK] = "Secure attention";
# endif
# ifdef SIGSEGV
      sys_siglist[SIGSEGV] = "Segmentation violation";
# endif
# ifdef SIGSOUND
      sys_siglist[SIGSOUND] = "Sound completed";
# endif
# ifdef SIGSTOP
      sys_siglist[SIGSTOP] = "Stopped (signal)";
# endif
# ifdef SIGSTP
      sys_siglist[SIGSTP] = "Stopped (user)";
# endif
# ifdef SIGSYS
      sys_siglist[SIGSYS] = "Bad argument to system call";
# endif
# ifdef SIGTERM
      sys_siglist[SIGTERM] = "Terminated";
# endif
# ifdef SIGTHAW
      sys_siglist[SIGTHAW] = "SIGTHAW";
# endif
# ifdef SIGTRAP
      sys_siglist[SIGTRAP] = "Trace/breakpoint trap";
# endif
# ifdef SIGTSTP
      sys_siglist[SIGTSTP] = "Stopped (user)";
# endif
# ifdef SIGTTIN
      sys_siglist[SIGTTIN] = "Stopped (tty input)";
# endif
# ifdef SIGTTOU
      sys_siglist[SIGTTOU] = "Stopped (tty output)";
# endif
# ifdef SIGURG
      sys_siglist[SIGURG] = "Urgent I/O condition";
# endif
# ifdef SIGUSR1
      sys_siglist[SIGUSR1] = "User defined signal 1";
# endif
# ifdef SIGUSR2
      sys_siglist[SIGUSR2] = "User defined signal 2";
# endif
# ifdef SIGVTALRM
      sys_siglist[SIGVTALRM] = "Virtual timer expired";
# endif
# ifdef SIGWAITING
      sys_siglist[SIGWAITING] = "Process's LWPs are blocked";
# endif
# ifdef SIGWINCH
      sys_siglist[SIGWINCH] = "Window size changed";
# endif
# ifdef SIGWIND
      sys_siglist[SIGWIND] = "SIGWIND";
# endif
# ifdef SIGXCPU
      sys_siglist[SIGXCPU] = "CPU time limit exceeded";
# endif
# ifdef SIGXFSZ
      sys_siglist[SIGXFSZ] = "File size limit exceeded";
# endif
    }
#endif /* !defined HAVE_STRSIGNAL && !defined HAVE_DECL_SYS_SIGLIST */
}

#ifndef HAVE_RANDOM
#ifdef random
#define HAVE_RANDOM
#endif
#endif

/* Figure out how many bits the system's random number generator uses.
   `random' and `lrand48' are assumed to return 31 usable bits.
   BSD `rand' returns a 31 bit value but the low order bits are unusable;
   so we'll shift it and treat it like the 15-bit USG `rand'.  */

#ifndef RAND_BITS
# ifdef HAVE_RANDOM
#  define RAND_BITS 31
# else /* !HAVE_RANDOM */
#  ifdef HAVE_LRAND48
#   define RAND_BITS 31
#   define random lrand48
#  else /* !HAVE_LRAND48 */
#   define RAND_BITS 15
#   if RAND_MAX == 32767
#    define random rand
#   else /* RAND_MAX != 32767 */
#    if RAND_MAX == 2147483647
#     define random() (rand () >> 16)
#    else /* RAND_MAX != 2147483647 */
#     ifdef USG
#      define random rand
#     else
#      define random() (rand () >> 16)
#     endif /* !USG */
#    endif /* RAND_MAX != 2147483647 */
#   endif /* RAND_MAX != 32767 */
#  endif /* !HAVE_LRAND48 */
# endif /* !HAVE_RANDOM */
#endif /* !RAND_BITS */

void
seed_random (arg)
     long arg;
{
#ifdef HAVE_RANDOM
  srandom ((unsigned int)arg);
#else
# ifdef HAVE_LRAND48
  srand48 (arg);
# else
  srand ((unsigned int)arg);
# endif
#endif
}

/*
 * Build a full Emacs-sized word out of whatever we've got.
 * This suffices even for a 64-bit architecture with a 15-bit rand.
 */
long
get_random ()
{
  long val = random ();
#if VALBITS > RAND_BITS
  val = (val << RAND_BITS) ^ random ();
#if VALBITS > 2*RAND_BITS
  val = (val << RAND_BITS) ^ random ();
#if VALBITS > 3*RAND_BITS
  val = (val << RAND_BITS) ^ random ();
#if VALBITS > 4*RAND_BITS
  val = (val << RAND_BITS) ^ random ();
#endif /* need at least 5 */
#endif /* need at least 4 */
#endif /* need at least 3 */
#endif /* need at least 2 */
  return val & ((1L << VALBITS) - 1);
}

#ifndef HAVE_STRERROR
#ifndef WINDOWSNT
char *
strerror (errnum)
     int errnum;
{
  extern char *sys_errlist[];
  extern int sys_nerr;

  if (errnum >= 0 && errnum < sys_nerr)
    return sys_errlist[errnum];
  return (char *) "Unknown error";
}
#endif /* not WINDOWSNT */
#endif /* ! HAVE_STRERROR */

int
emacs_open (path, oflag, mode)
     const char *path;
     int oflag, mode;
{
  register int rtnval;

  while ((rtnval = open (path, oflag, mode)) == -1
	 && (errno == EINTR))
    QUIT;
  return (rtnval);
}

int
emacs_close (fd)
     int fd;
{
  int did_retry = 0;
  register int rtnval;

  while ((rtnval = close (fd)) == -1
	 && (errno == EINTR))
    did_retry = 1;

  /* If close is interrupted SunOS 4.1 may or may not have closed the
     file descriptor.  If it did the second close will fail with
     errno = EBADF.  That means we have succeeded.  */
  if (rtnval == -1 && did_retry && errno == EBADF)
    return 0;

  return rtnval;
}

int
emacs_read (fildes, buf, nbyte)
     int fildes;
     char *buf;
     unsigned int nbyte;
{
  register int rtnval;

  while ((rtnval = read (fildes, buf, nbyte)) == -1
	 && (errno == EINTR))
    QUIT;
  return (rtnval);
}

int
emacs_write (fildes, buf, nbyte)
     int fildes;
     const char *buf;
     unsigned int nbyte;
{
  register int rtnval, bytes_written;

  bytes_written = 0;

  while (nbyte > 0)
    {
      rtnval = write (fildes, buf, nbyte);

      if (rtnval == -1)
	{
	  if (errno == EINTR)
	    {
#ifdef SYNC_INPUT
	      /* I originally used `QUIT' but that might causes files to
		 be truncated if you hit C-g in the middle of it.  --Stef  */
	      process_pending_signals ();
#endif
	      continue;
	    }
	  else
	    return (bytes_written ? bytes_written : -1);
	}

      buf += rtnval;
      nbyte -= rtnval;
      bytes_written += rtnval;
    }
  return (bytes_written);
}

#ifdef USG
/*
 *	All of the following are for USG.
 *
 *	On USG systems the system calls are INTERRUPTIBLE by signals
 *	that the user program has elected to catch.  Thus the system call
 *	must be retried in these cases.  To handle this without massive
 *	changes in the source code, we remap the standard system call names
 *	to names for our own functions in sysdep.c that do the system call
 *	with retries.  Actually, for portability reasons, it is good
 *	programming practice, as this example shows, to limit all actual
 *	system calls to a single occurrence in the source.  Sure, this
 *	adds an extra level of function call overhead but it is almost
 *	always negligible.   Fred Fish, Unisoft Systems Inc.
 */

/*
 *	Warning, this function may not duplicate 4.2 action properly
 *	under error conditions.
 */

#ifndef MAXPATHLEN
/* In 4.1, param.h fails to define this.  */
#define MAXPATHLEN 1024
#endif

#ifndef HAVE_GETWD

char *
getwd (pathname)
     char *pathname;
{
  char *npath, *spath;
  extern char *getcwd ();

  BLOCK_INPUT;			/* getcwd uses malloc */
  spath = npath = getcwd ((char *) 0, MAXPATHLEN);
  if (spath == 0)
    {
      UNBLOCK_INPUT;
      return spath;
    }
  /* On Altos 3068, getcwd can return @hostname/dir, so discard
     up to first slash.  Should be harmless on other systems.  */
  while (*npath && *npath != '/')
    npath++;
  strcpy (pathname, npath);
  free (spath);			/* getcwd uses malloc */
  UNBLOCK_INPUT;
  return pathname;
}

#endif /* HAVE_GETWD */

/*
 *	Emulate rename using unlink/link.  Note that this is
 *	only partially correct.  Also, doesn't enforce restriction
 *	that files be of same type (regular->regular, dir->dir, etc).
 */

#ifndef HAVE_RENAME

rename (from, to)
     const char *from;
     const char *to;
{
  if (access (from, 0) == 0)
    {
      unlink (to);
      if (link (from, to) == 0)
	if (unlink (from) == 0)
	  return (0);
    }
  return (-1);
}

#endif


#if defined(HPUX) && !defined(HAVE_PERROR)

/* HPUX curses library references perror, but as far as we know
   it won't be called.  Anyway this definition will do for now.  */

perror ()
{
}
#endif /* HPUX and not HAVE_PERROR */

#ifndef HAVE_DUP2

/*
 *	Emulate BSD dup2.  First close newd if it already exists.
 *	Then, attempt to dup oldd.  If not successful, call dup2 recursively
 *	until we are, then close the unsuccessful ones.
 */

dup2 (oldd, newd)
     int oldd;
     int newd;
{
  register int fd, ret;

  emacs_close (newd);

#ifdef F_DUPFD
  return fcntl (oldd, F_DUPFD, newd);
#else
  fd = dup (old);
  if (fd == -1)
    return -1;
  if (fd == new)
    return new;
  ret = dup2 (old,new);
  emacs_close (fd);
  return ret;
#endif
}

#endif /* not HAVE_DUP2 */

/*
 *	Gettimeofday.  Simulate as much as possible.  Only accurate
 *	to nearest second.  Emacs doesn't use tzp so ignore it for now.
 *	Only needed when subprocesses are defined.
 */

#ifdef subprocesses
#ifndef HAVE_GETTIMEOFDAY
#ifdef HAVE_TIMEVAL

/* ARGSUSED */
int
gettimeofday (tp, tzp)
     struct timeval *tp;
     struct timezone *tzp;
{
  extern long time ();

  tp->tv_sec = time ((long *)0);
  tp->tv_usec = 0;
  if (tzp != 0)
    tzp->tz_minuteswest = -1;
  return 0;
}

#endif
#endif
#endif /* subprocess && !HAVE_GETTIMEOFDAY && HAVE_TIMEVAL */

/*
 *	This function will go away as soon as all the stubs fixed. (fnf)
 */

void
croak (badfunc)
     char *badfunc;
{
  printf ("%s not yet implemented\r\n", badfunc);
  reset_all_sys_modes ();
  exit (1);
}

#endif /* USG */

/* Directory routines for systems that don't have them. */

#ifdef SYSV_SYSTEM_DIR

#include <dirent.h>

#if !defined (HAVE_CLOSEDIR)

int
closedir (DIR *dirp /* stream from opendir */)
{
  int rtnval;

  rtnval = emacs_close (dirp->dd_fd);

  /* Some systems (like Solaris) allocate the buffer and the DIR all
     in one block.  Why in the world are we freeing this ourselves
     anyway?  */
#if ! defined (SOLARIS2)
  xfree ((char *) dirp->dd_buf); /* directory block defined in <dirent.h> */
#endif
  xfree ((char *) dirp);

  return rtnval;
}
#endif /* not HAVE_CLOSEDIR */
#endif /* SYSV_SYSTEM_DIR */


int
set_file_times (filename, atime, mtime)
     const char *filename;
     EMACS_TIME atime, mtime;
{
#ifdef HAVE_UTIMES
  struct timeval tv[2];
  tv[0] = atime;
  tv[1] = mtime;
  return utimes (filename, tv);
#else /* not HAVE_UTIMES */
  struct _utimbuf utb;
  utb.actime = EMACS_SECS (atime);
  utb.modtime = EMACS_SECS (mtime);
  return _utime (filename, &utb);
#endif /* not HAVE_UTIMES */
}

/* mkdir and rmdir functions, for systems which don't have them.  */

#ifndef HAVE_MKDIR
/*
 * Written by Robert Rother, Mariah Corporation, August 1985.
 *
 * If you want it, it's yours.  All I ask in return is that if you
 * figure out how to do this in a Bourne Shell script you send me
 * a copy.
 *					sdcsvax!rmr or rmr@uscd
 *
 * Severely hacked over by John Gilmore to make a 4.2BSD compatible
 * subroutine.  11Mar86; hoptoad!gnu
 *
 * Modified by rmtodd@uokmax 6-28-87 -- when making an already existing dir,
 * subroutine didn't return EEXIST.  It does now.
 */

/*
 * Make a directory.
 */
#ifdef MKDIR_PROTOTYPE
MKDIR_PROTOTYPE
#else
int
mkdir (dpath, dmode)
     char *dpath;
     int dmode;
#endif
{
  int cpid, status, fd;
  struct stat statbuf;

  if (stat (dpath, &statbuf) == 0)
    {
      errno = EEXIST;		/* Stat worked, so it already exists */
      return -1;
    }

  /* If stat fails for a reason other than non-existence, return error */
  if (errno != ENOENT)
    return -1;

  synch_process_alive = 1;
  switch (cpid = fork ())
    {

    case -1:			/* Error in fork */
      return (-1);		/* Errno is set already */

    case 0:			/* Child process */
      /*
		 * Cheap hack to set mode of new directory.  Since this
		 * child process is going away anyway, we zap its umask.
		 * FIXME, this won't suffice to set SUID, SGID, etc. on this
		 * directory.  Does anybody care?
		 */
      status = umask (0);	/* Get current umask */
      status = umask (status | (0777 & ~dmode));	/* Set for mkdir */
      fd = emacs_open ("/dev/null", O_RDWR, 0);
      if (fd >= 0)
        {
	  dup2 (fd, 0);
	  dup2 (fd, 1);
	  dup2 (fd, 2);
        }
      execl ("/bin/mkdir", "mkdir", dpath, (char *) 0);
      _exit (-1);		/* Can't exec /bin/mkdir */

    default:			/* Parent process */
      wait_for_termination (cpid);
    }

  if (synch_process_death != 0 || synch_process_retcode != 0
      || synch_process_termsig != 0)
    {
      errno = EIO;		/* We don't know why, but */
      return -1;		/* /bin/mkdir failed */
    }

  return 0;
}
#endif /* not HAVE_MKDIR */

#ifndef HAVE_RMDIR
int
rmdir (dpath)
     char *dpath;
{
  int cpid, status, fd;
  struct stat statbuf;

  if (stat (dpath, &statbuf) != 0)
    {
      /* Stat just set errno.  We don't have to */
      return -1;
    }

  synch_process_alive = 1;
  switch (cpid = fork ())
    {

    case -1:			/* Error in fork */
      return (-1);		/* Errno is set already */

    case 0:			/* Child process */
      fd = emacs_open ("/dev/null", O_RDWR, 0);
      if (fd >= 0)
        {
	  dup2 (fd, 0);
	  dup2 (fd, 1);
	  dup2 (fd, 2);
        }
      execl ("/bin/rmdir", "rmdir", dpath, (char *) 0);
      _exit (-1);		/* Can't exec /bin/rmdir */

    default:			/* Parent process */
      wait_for_termination (cpid);
    }

  if (synch_process_death != 0 || synch_process_retcode != 0
      || synch_process_termsig != 0)
    {
      errno = EIO;		/* We don't know why, but */
      return -1;		/* /bin/rmdir failed */
    }

  return 0;
}
#endif /* !HAVE_RMDIR */


#ifndef BSTRING

#ifndef bzero

void
bzero (b, length)
     register char *b;
     register int length;
{
  while (length-- > 0)
    *b++ = 0;
}

#endif /* no bzero */
#endif /* BSTRING */

#if (!defined (BSTRING) && !defined (bcopy)) || defined (NEED_BCOPY)
#undef bcopy

/* Saying `void' requires a declaration, above, where bcopy is used
   and that declaration causes pain for systems where bcopy is a macro.  */
bcopy (b1, b2, length)
     register char *b1;
     register char *b2;
     register int length;
{
  while (length-- > 0)
    *b2++ = *b1++;
}
#endif /* (!defined (BSTRING) && !defined (bcopy)) || defined (NEED_BCOPY) */

#ifndef BSTRING
#ifndef bcmp
int
bcmp (b1, b2, length)	/* This could be a macro! */
     register char *b1;
     register char *b2;
     register int length;
{
  while (length-- > 0)
    if (*b1++ != *b2++)
      return 1;

  return 0;
}
#endif /* no bcmp */
#endif /* not BSTRING */

#ifndef HAVE_STRSIGNAL
char *
strsignal (code)
     int code;
{
  char *signame = 0;

  if (0 <= code && code < NSIG)
    {
      /* Cast to suppress warning if the table has const char *.  */
      signame = (char *) sys_siglist[code];
    }

  return signame;
}
#endif /* HAVE_STRSIGNAL */

#ifdef HAVE_TERMIOS
/* For make-serial-process  */
int serial_open (char *port)
{
  int fd = -1;

  fd = emacs_open ((char*) port,
		   O_RDWR
#ifdef O_NONBLOCK
		   | O_NONBLOCK
#else
		   | O_NDELAY
#endif
#ifdef O_NOCTTY
		   | O_NOCTTY
#endif
		   , 0);
  if (fd < 0)
    {
      error ("Could not open %s: %s",
	     port, emacs_strerror (errno));
    }
#ifdef TIOCEXCL
  ioctl (fd, TIOCEXCL, (char *) 0);
#endif

  return fd;
}
#endif /* TERMIOS  */

#ifdef HAVE_TERMIOS

#if !defined (HAVE_CFMAKERAW)
/* Workaround for targets which are missing cfmakeraw.  */
/* Pasted from man page.  */
static void cfmakeraw (struct termios *termios_p)
{
    termios_p->c_iflag &= ~(IGNBRK|BRKINT|PARMRK|ISTRIP|INLCR|IGNCR|ICRNL|IXON);
    termios_p->c_oflag &= ~OPOST;
    termios_p->c_lflag &= ~(ECHO|ECHONL|ICANON|ISIG|IEXTEN);
    termios_p->c_cflag &= ~(CSIZE|PARENB);
    termios_p->c_cflag |= CS8;
}
#endif /* !defined (HAVE_CFMAKERAW */

#if !defined (HAVE_CFSETSPEED)
/* Workaround for targets which are missing cfsetspeed.  */
static int cfsetspeed (struct termios *termios_p, speed_t vitesse)
{
  return (cfsetispeed (termios_p, vitesse)
	  + cfsetospeed (termios_p, vitesse));
}
#endif

/* For serial-process-configure  */
void
serial_configure (struct Lisp_Process *p,
		      Lisp_Object contact)
{
  Lisp_Object childp2 = Qnil;
  Lisp_Object tem = Qnil;
  struct termios attr;
  int err = -1;
  char summary[4] = "???"; /* This usually becomes "8N1".  */

  childp2 = Fcopy_sequence (p->childp);

  /* Read port attributes and prepare default configuration.  */
  err = tcgetattr (p->outfd, &attr);
  if (err != 0)
    error ("tcgetattr() failed: %s", emacs_strerror (errno));
  cfmakeraw (&attr);
#if defined (CLOCAL)
  attr.c_cflag |= CLOCAL;
#endif
#if defined (CREAD)
  attr.c_cflag |= CREAD;
#endif

  /* Configure speed.  */
  if (!NILP (Fplist_member (contact, QCspeed)))
    tem = Fplist_get (contact, QCspeed);
  else
    tem = Fplist_get (p->childp, QCspeed);
  CHECK_NUMBER (tem);
  err = cfsetspeed (&attr, XINT (tem));
  if (err != 0)
    error ("cfsetspeed(%d) failed: %s", XINT (tem), emacs_strerror (errno));
  childp2 = Fplist_put (childp2, QCspeed, tem);

  /* Configure bytesize.  */
  if (!NILP (Fplist_member (contact, QCbytesize)))
    tem = Fplist_get (contact, QCbytesize);
  else
    tem = Fplist_get (p->childp, QCbytesize);
  if (NILP (tem))
    tem = make_number (8);
  CHECK_NUMBER (tem);
  if (XINT (tem) != 7 && XINT (tem) != 8)
    error (":bytesize must be nil (8), 7, or 8");
  summary[0] = XINT(tem) + '0';
#if defined (CSIZE) && defined (CS7) && defined (CS8)
  attr.c_cflag &= ~CSIZE;
  attr.c_cflag |= ((XINT (tem) == 7) ? CS7 : CS8);
#else
  /* Don't error on bytesize 8, which should be set by cfmakeraw.  */
  if (XINT (tem) != 8)
    error ("Bytesize cannot be changed");
#endif
  childp2 = Fplist_put (childp2, QCbytesize, tem);

  /* Configure parity.  */
  if (!NILP (Fplist_member (contact, QCparity)))
    tem = Fplist_get (contact, QCparity);
  else
    tem = Fplist_get (p->childp, QCparity);
  if (!NILP (tem) && !EQ (tem, Qeven) && !EQ (tem, Qodd))
    error (":parity must be nil (no parity), `even', or `odd'");
#if defined (PARENB) && defined (PARODD) && defined (IGNPAR) && defined (INPCK)
  attr.c_cflag &= ~(PARENB | PARODD);
  attr.c_iflag &= ~(IGNPAR | INPCK);
  if (NILP (tem))
    {
      summary[1] = 'N';
    }
  else if (EQ (tem, Qeven))
    {
      summary[1] = 'E';
      attr.c_cflag |= PARENB;
      attr.c_iflag |= (IGNPAR | INPCK);
    }
  else if (EQ (tem, Qodd))
    {
      summary[1] = 'O';
      attr.c_cflag |= (PARENB | PARODD);
      attr.c_iflag |= (IGNPAR | INPCK);
    }
#else
  /* Don't error on no parity, which should be set by cfmakeraw.  */
  if (!NILP (tem))
    error ("Parity cannot be configured");
#endif
  childp2 = Fplist_put (childp2, QCparity, tem);

  /* Configure stopbits.  */
  if (!NILP (Fplist_member (contact, QCstopbits)))
    tem = Fplist_get (contact, QCstopbits);
  else
    tem = Fplist_get (p->childp, QCstopbits);
  if (NILP (tem))
    tem = make_number (1);
  CHECK_NUMBER (tem);
  if (XINT (tem) != 1 && XINT (tem) != 2)
    error (":stopbits must be nil (1 stopbit), 1, or 2");
  summary[2] = XINT (tem) + '0';
#if defined (CSTOPB)
  attr.c_cflag &= ~CSTOPB;
  if (XINT (tem) == 2)
    attr.c_cflag |= CSTOPB;
#else
  /* Don't error on 1 stopbit, which should be set by cfmakeraw.  */
  if (XINT (tem) != 1)
    error ("Stopbits cannot be configured");
#endif
  childp2 = Fplist_put (childp2, QCstopbits, tem);

  /* Configure flowcontrol.  */
  if (!NILP (Fplist_member (contact, QCflowcontrol)))
    tem = Fplist_get (contact, QCflowcontrol);
  else
    tem = Fplist_get (p->childp, QCflowcontrol);
  if (!NILP (tem) && !EQ (tem, Qhw) && !EQ (tem, Qsw))
    error (":flowcontrol must be nil (no flowcontrol), `hw', or `sw'");
#if defined (CRTSCTS)
  attr.c_cflag &= ~CRTSCTS;
#endif
#if defined (CNEW_RTSCTS)
  attr.c_cflag &= ~CNEW_RTSCTS;
#endif
#if defined (IXON) && defined (IXOFF)
  attr.c_iflag &= ~(IXON | IXOFF);
#endif
  if (NILP (tem))
    {
      /* Already configured.  */
    }
  else if (EQ (tem, Qhw))
    {
#if defined (CRTSCTS)
      attr.c_cflag |= CRTSCTS;
#elif defined (CNEW_RTSCTS)
      attr.c_cflag |= CNEW_RTSCTS;
#else
      error ("Hardware flowcontrol (RTS/CTS) not supported");
#endif
    }
  else if (EQ (tem, Qsw))
    {
#if defined (IXON) && defined (IXOFF)
      attr.c_iflag |= (IXON | IXOFF);
#else
      error ("Software flowcontrol (XON/XOFF) not supported");
#endif
    }
  childp2 = Fplist_put (childp2, QCflowcontrol, tem);

  /* Activate configuration.  */
  err = tcsetattr (p->outfd, TCSANOW, &attr);
  if (err != 0)
    error ("tcsetattr() failed: %s", emacs_strerror (errno));

  childp2 = Fplist_put (childp2, QCsummary, build_string (summary));
  p->childp = childp2;

}
#endif /* TERMIOS  */

/* arch-tag: edb43589-4e09-4544-b325-978b5b121dcf
   (do not change this comment) */
