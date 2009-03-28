/* Fully extensible Emacs, running on Unix, intended for GNU.
   Copyright (C) 1985, 1986, 1987, 1993, 1994, 1995, 1997, 1998, 1999,
                 2001, 2002, 2003, 2004, 2005, 2006, 2007, 2008, 2009
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


#include <config.h>
#include <signal.h>
#include <stdio.h>

#include <sys/types.h>
#include <sys/file.h>

#ifdef WINDOWSNT
#include <fcntl.h>
#include <windows.h> /* just for w32.h */
#include "w32.h"
#include "w32heap.h" /* for prototype of sbrk */
#endif

#include "lisp.h"
#include "commands.h"
#include "intervals.h"
#include "buffer.h"
#include "window.h"

#include "systty.h"
#include "blockinput.h"
#include "syssignal.h"
#include "process.h"
#include "frame.h"
#include "termhooks.h"
#include "keyboard.h"
#include "keymap.h"

#ifdef HAVE_SETLOCALE
#include <locale.h>
#endif

extern void malloc_warning P_ ((char *));
extern void set_time_zone_rule P_ ((char *));

/* Make these values available in GDB, which doesn't see macros.  */

int gdb_use_lsb = 0;
int gdb_use_union = 0;

EMACS_INT gdb_valbits = VALBITS;
EMACS_INT gdb_gctypebits = GCTYPEBITS;
EMACS_INT gdb_data_seg_bits = 0;

EMACS_INT PVEC_FLAG = PSEUDOVECTOR_FLAG;
EMACS_INT gdb_array_mark_flag = ARRAY_MARK_FLAG;
/* GDB might say "No enum type named pvec_type" if we don't have at
   least one symbol with that type, and then xbacktrace could fail.  */
enum pvec_type gdb_pvec_type = PVEC_TYPE_MASK;

/* Command line args from shell, as list of strings.  */
Lisp_Object Vcommand_line_args;

/* The name under which Emacs was invoked, with any leading directory
   names discarded.  */
Lisp_Object Vinvocation_name;

/* The directory name from which Emacs was invoked.  */
Lisp_Object Vinvocation_directory;

/* The directory name in which to find subdirs such as lisp and etc.
   nil means get them only from PATH_LOADSEARCH.  */
Lisp_Object Vinstallation_directory;

/* The values of `current-time' before and after Emacs initialization.  */
Lisp_Object Vbefore_init_time, Vafter_init_time;

/* Hook run by `kill-emacs' before it does really anything.  */
Lisp_Object Vkill_emacs_hook;

/* Empty lisp strings.  To avoid having to build any others.  */
Lisp_Object empty_unibyte_string, empty_multibyte_string;

/* Search path separator.  */
Lisp_Object Vpath_separator;

/* Set nonzero after Emacs has started up the first time.
  Prevents reinitialization of the Lisp world and keymaps
  on subsequent starts.  */
int initialized;

/* Variable whose value is symbol giving operating system type.  */
Lisp_Object Vsystem_type;

/* Variable whose value is string giving configuration built for.  */
Lisp_Object Vsystem_configuration;

/* Variable whose value is string giving configuration options,
   for use when reporting bugs.  */
Lisp_Object Vsystem_configuration_options;

Lisp_Object Qfile_name_handler_alist;

/* Current and previous system locales for messages and time.  */
Lisp_Object Vsystem_messages_locale;
Lisp_Object Vprevious_system_messages_locale;
Lisp_Object Vsystem_time_locale;
Lisp_Object Vprevious_system_time_locale;

/* If non-zero, emacs should not attempt to use a window-specific code,
   but instead should use the virtual terminal under which it was started.  */
int inhibit_window_system;

/* If nonzero, set Emacs to run at this priority.  This is also used
   in child_setup and sys_suspend to make sure subshells run at normal
   priority; those functions have their own extern declaration.  */
EMACS_INT emacs_priority;

/* If non-zero, a filter or a sentinel is running.  Tested to save the match
   data on the first attempt to change it inside asynchronous code.  */
int running_asynch_code;

/* An address near the bottom of the stack.
   Tells GC how to save a copy of the stack.  */
char *stack_bottom;

/* The address where the heap starts (from the first sbrk (0) call).  */
static void *my_heap_start;

/* The gap between BSS end and heap start as far as we can tell.  */
static unsigned long heap_bss_diff;

/* If the gap between BSS end and heap start is larger than this
   output a warning in dump-emacs.  */
#define MAX_HEAP_BSS_DIFF (1024*1024)


#ifdef HAVE_WINDOW_SYSTEM
extern Lisp_Object Vinitial_window_system;
#endif /* HAVE_WINDOW_SYSTEM */

extern Lisp_Object Vauto_save_list_file_name;

extern Lisp_Object Vinhibit_redisplay;

/* Nonzero means running Emacs without interactive terminal.  */

int noninteractive;

/* Value of Lisp variable `noninteractive'.
   Normally same as C variable `noninteractive'
   but nothing terrible happens if user sets this one.  */

int noninteractive1;

/* Name for the server started by the daemon.*/
static char *daemon_name;

/* Save argv and argc.  */
char **initial_argv;
int initial_argc;

static void sort_args ();
void syms_of_emacs ();

/* MSVC needs each string be shorter than 2048 bytes, so the usage
   strings below are split to not overflow this limit.  */
#define USAGE1 "\
Usage: %s [OPTION-OR-FILENAME]...\n\
\n\
Run Emacs, the extensible, customizable, self-documenting real-time\n\
display editor.  The recommended way to start Emacs for normal editing\n\
is with no options at all.\n\
\n\
Run M-x info RET m emacs RET m emacs invocation RET inside Emacs to\n\
read the main documentation for these command-line arguments.\n\
\n\
Initialization options:\n\
\n\
--batch                     do not do interactive display; implies -q\n\
--daemon                    start a server in the background\n\
--debug-init                enable Emacs Lisp debugger for init file\n\
--display, -d DISPLAY       use X server DISPLAY\n\
--multibyte, --no-unibyte   inhibit the effect of EMACS_UNIBYTE\n\
--no-desktop                do not load a saved desktop\n\
--no-init-file, -q          load neither ~/.emacs nor default.el\n\
--no-shared-memory, -nl     do not use shared memory\n\
--no-site-file              do not load site-start.el\n\
--no-splash                 do not display a splash screen on startup\n\
--quick, -Q                 equivalent to -q --no-site-file --no-splash\n\
--script FILE               run FILE as an Emacs Lisp script\n\
--unibyte, --no-multibyte   run Emacs in unibyte mode\n\
--user, -u USER             load ~USER/.emacs instead of your own\n\
\n%s"

#define USAGE2 "\
Action options:\n\
\n\
FILE                    visit FILE using find-file\n\
+LINE                   go to line LINE in next FILE\n\
+LINE:COLUMN            go to line LINE, column COLUMN, in next FILE\n\
--directory, -L DIR     add DIR to variable load-path\n\
--eval EXPR             evaluate Emacs Lisp expression EXPR\n\
--execute EXPR          evaluate Emacs Lisp expression EXPR\n\
--file FILE             visit FILE using find-file\n\
--find-file FILE        visit FILE using find-file\n\
--funcall, -f FUNC      call Emacs Lisp function FUNC with no arguments\n\
--insert FILE           insert contents of FILE into current buffer\n\
--kill                  exit without asking for confirmation\n\
--load, -l FILE         load Emacs Lisp FILE using the load function\n\
--visit FILE            visit FILE using find-file\n\
\n"

#define USAGE3 "\
Display options:\n\
\n\
--background-color, -bg COLOR   window background color\n\
--basic-display, -D             disable many display features;\n\
                                  used for debugging Emacs\n\
--border-color, -bd COLOR       main border color\n\
--border-width, -bw WIDTH       width of main border\n\
--color, --color=MODE           override color mode for character terminals;\n\
                                  MODE defaults to `auto', and can also\n\
                                  be `never', `auto', `always',\n\
                                  or a mode name like `ansi8'\n\
--cursor-color, -cr COLOR       color of the Emacs cursor indicating point\n\
--font, -fn FONT                default font; must be fixed-width\n\
--foreground-color, -fg COLOR   window foreground color\n\
--fullheight, -fh               make the first frame high as the screen\n\
--fullscreen, -fs               make first frame fullscreen\n\
--fullwidth, -fw                make the first frame wide as the screen\n\
--geometry, -g GEOMETRY         window geometry\n\
--no-bitmap-icon, -nbi          do not use picture of gnu for Emacs icon\n\
--iconic                        start Emacs in iconified state\n\
--internal-border, -ib WIDTH    width between text and main border\n\
--line-spacing, -lsp PIXELS     additional space to put between lines\n\
--mouse-color, -ms COLOR        mouse cursor color in Emacs window\n\
--name NAME                     title for initial Emacs frame\n\
--no-blinking-cursor, -nbc      disable blinking cursor\n\
--reverse-video, -r, -rv        switch foreground and background\n\
--title, -T TITLE               title for initial Emacs frame\n\
--vertical-scroll-bars, -vb     enable vertical scroll bars\n\
--xrm XRESOURCES                set additional X resources\n\
--parent-id XID                 set parent window\n\
--help                          display this help and exit\n\
--version                       output version information and exit\n\
\n"

#define USAGE4 "\
You can generally also specify long option names with a single -; for\n\
example, -batch as well as --batch.  You can use any unambiguous\n\
abbreviation for a --option.\n\
\n\
Various environment variables and window system resources also affect\n\
Emacs' operation.  See the main documentation.\n\
\n\
Report bugs to %s.  First, please see the Bugs\n\
section of the Emacs manual or the file BUGS.\n"


/* Signal code for the fatal signal that was received.  */
int fatal_error_code;

/* Nonzero if handling a fatal error already.  */
int fatal_error_in_progress;

/* If non-null, call this function from fatal_error_signal before
   committing suicide.  */

void (*fatal_error_signal_hook) P_ ((void));

/* Handle bus errors, invalid instruction, etc.  */
SIGTYPE
fatal_error_signal (sig)
     int sig;
{
  SIGNAL_THREAD_CHECK (sig);
  fatal_error_code = sig;
  signal (sig, SIG_DFL);

  TOTALLY_UNBLOCK_INPUT;

  /* If fatal error occurs in code below, avoid infinite recursion.  */
  if (! fatal_error_in_progress)
    {
      fatal_error_in_progress = 1;

      shut_down_emacs (sig, 0, Qnil);
    }

  /* Signal the same code; this time it will really be fatal.
     Remember that since we're in a signal handler, the signal we're
     going to send is probably blocked, so we have to unblock it if we
     want to really receive it.  */
  sigunblock (sigmask (fatal_error_code));

  if (fatal_error_signal_hook)
    fatal_error_signal_hook ();

  kill (getpid (), fatal_error_code);
}

/* Code for dealing with Lisp access to the Unix command line.  */

static void
init_cmdargs (argc, argv, skip_args)
     int argc;
     char **argv;
     int skip_args;
{
  register int i;
  Lisp_Object name, dir, tem;
  int count = SPECPDL_INDEX ();
  Lisp_Object raw_name;

  initial_argv = argv;
  initial_argc = argc;

  raw_name = build_string (argv[0]);

  /* Add /: to the front of the name
     if it would otherwise be treated as magic.  */
  tem = Ffind_file_name_handler (raw_name, Qt);
  if (! NILP (tem))
    raw_name = concat2 (build_string ("/:"), raw_name);

  Vinvocation_name = Ffile_name_nondirectory (raw_name);
  Vinvocation_directory = Ffile_name_directory (raw_name);

  /* If we got no directory in argv[0], search PATH to find where
     Emacs actually came from.  */
  if (NILP (Vinvocation_directory))
    {
      Lisp_Object found;
      int yes = openp (Vexec_path, Vinvocation_name,
		       Vexec_suffixes, &found, make_number (X_OK));
      if (yes == 1)
	{
	  /* Add /: to the front of the name
	     if it would otherwise be treated as magic.  */
	  tem = Ffind_file_name_handler (found, Qt);
	  if (! NILP (tem))
	    found = concat2 (build_string ("/:"), found);
	  Vinvocation_directory = Ffile_name_directory (found);
	}
    }

  if (!NILP (Vinvocation_directory)
      && NILP (Ffile_name_absolute_p (Vinvocation_directory)))
    /* Emacs was started with relative path, like ./emacs.
       Make it absolute.  */
    Vinvocation_directory = Fexpand_file_name (Vinvocation_directory, Qnil);

  Vinstallation_directory = Qnil;

  if (!NILP (Vinvocation_directory))
    {
      dir = Vinvocation_directory;
      name = Fexpand_file_name (Vinvocation_name, dir);
      while (1)
	{
	  Lisp_Object tem, lib_src_exists;
	  Lisp_Object etc_exists, info_exists;

	  /* See if dir contains subdirs for use by Emacs.
	     Check for the ones that would exist in a build directory,
	     not including lisp and info.  */
	  tem = Fexpand_file_name (build_string ("lib-src"), dir);
	  lib_src_exists = Ffile_exists_p (tem);

	  info_exists = Qnil;

	  if (!NILP (lib_src_exists) || !NILP (info_exists))
	    {
	      tem = Fexpand_file_name (build_string ("etc"), dir);
	      etc_exists = Ffile_exists_p (tem);
	      if (!NILP (etc_exists))
		{
		  Vinstallation_directory
		    = Ffile_name_as_directory (dir);
		  break;
		}
	    }

	  /* See if dir's parent contains those subdirs.  */
	  tem = Fexpand_file_name (build_string ("../lib-src"), dir);
	  lib_src_exists = Ffile_exists_p (tem);


	  info_exists = Qnil;

	  if (!NILP (lib_src_exists) || !NILP (info_exists))
	    {
	      tem = Fexpand_file_name (build_string ("../etc"), dir);
	      etc_exists = Ffile_exists_p (tem);
	      if (!NILP (etc_exists))
		{
		  tem = Fexpand_file_name (build_string (".."), dir);
		  Vinstallation_directory
		    = Ffile_name_as_directory (tem);
		  break;
		}
	    }

	  /* If the Emacs executable is actually a link,
	     next try the dir that the link points into.  */
	  tem = Ffile_symlink_p (name);
	  if (!NILP (tem))
	    {
	      name = Fexpand_file_name (tem, dir);
	      dir = Ffile_name_directory (name);
	    }
	  else
	    break;
	}
    }

  Vcommand_line_args = Qnil;

  for (i = argc - 1; i >= 0; i--)
    {
      if (i == 0 || i > skip_args)
	/* For the moment, we keep arguments as is in unibyte strings.
	   They are decoded in the function command-line after we know
	   locale-coding-system.  */
	Vcommand_line_args
	  = Fcons (make_unibyte_string (argv[i], strlen (argv[i])),
		   Vcommand_line_args);
    }

  unbind_to (count, Qnil);
}

DEFUN ("invocation-name", Finvocation_name, Sinvocation_name, 0, 0, 0,
       doc: /* Return the program name that was used to run Emacs.
Any directory names are omitted.  */)
     ()
{
  return Fcopy_sequence (Vinvocation_name);
}

DEFUN ("invocation-directory", Finvocation_directory, Sinvocation_directory,
       0, 0, 0,
       doc: /* Return the directory name in which the Emacs executable was located.  */)
     ()
{
  return Fcopy_sequence (Vinvocation_directory);
}


#ifdef HAVE_TZSET
/* A valid but unlikely value for the TZ environment value.
   It is OK (though a bit slower) if the user actually chooses this value.  */
static char dump_tz[] = "UtC0";
#endif

/* Test whether the next argument in ARGV matches SSTR or a prefix of
   LSTR (at least MINLEN characters).  If so, then if VALPTR is non-null
   (the argument is supposed to have a value) store in *VALPTR either
   the next argument or the portion of this one after the equal sign.
   ARGV is read starting at position *SKIPPTR; this index is advanced
   by the number of arguments used.

   Too bad we can't just use getopt for all of this, but we don't have
   enough information to do it right.  */

static int
argmatch (argv, argc, sstr, lstr, minlen, valptr, skipptr)
     char **argv;
     int argc;
     char *sstr;
     char *lstr;
     int minlen;
     char **valptr;
     int *skipptr;
{
  char *p = NULL;
  int arglen;
  char *arg;

  /* Don't access argv[argc]; give up in advance.  */
  if (argc <= *skipptr + 1)
    return 0;

  arg = argv[*skipptr+1];
  if (arg == NULL)
    return 0;
  if (strcmp (arg, sstr) == 0)
    {
      if (valptr != NULL)
	{
	  *valptr = argv[*skipptr+2];
	  *skipptr += 2;
	}
      else
	*skipptr += 1;
      return 1;
    }
  arglen = (valptr != NULL && (p = index (arg, '=')) != NULL
	    ? p - arg : strlen (arg));
  if (lstr == 0 || arglen < minlen || strncmp (arg, lstr, arglen) != 0)
    return 0;
  else if (valptr == NULL)
    {
      *skipptr += 1;
      return 1;
    }
  else if (p != NULL)
    {
      *valptr = p+1;
      *skipptr += 1;
      return 1;
    }
  else if (argv[*skipptr+2] != NULL)
    {
      *valptr = argv[*skipptr+2];
      *skipptr += 2;
      return 1;
    }
  else
    {
      return 0;
    }
}

#define REPORT_EMACS_BUG_ADDRESS "bug-gnu-emacs@gnu.org"
#define REPORT_EMACS_BUG_PRETEST_ADDRESS "emacs-pretest-bug@gnu.org"

/* This function is used to determine an address to which bug report should
   be sent.  */

char *
bug_reporting_address ()
{
  int count = 0;
  Lisp_Object temp;
  char *string;

  temp = Fsymbol_value (intern ("emacs-version"));

  /* When `emacs-version' is invalid, use normal address.  */
  if (!STRINGP(temp))
    return REPORT_EMACS_BUG_ADDRESS;

  string = SDATA (temp);

  /* Count dots in `emacs-version'.  */
  while (*string)
    {
      if (*string == '.')
	count++;
      string++;
    }

  /* When `emacs-version' has at least three dots, it is development or
     pretest version of Emacs.  */
  return count >= 3 ? REPORT_EMACS_BUG_PRETEST_ADDRESS : REPORT_EMACS_BUG_ADDRESS;
}


/* ARGSUSED */
int
main (int argc, char **argv)
{
  Lisp_Object dummy;
  char stack_bottom_variable;
  int do_initial_setlocale;
  int skip_args = 0;

  int no_loadup = 0;
  char *junk = 0;
  char *dname_arg = 0;

  extern Lisp_Object *stack_base;
  stack_base = &dummy;

  if (!initialized)
    {
      extern char my_endbss[];
      extern char *my_endbss_static;

      if (my_heap_start == 0)
        my_heap_start = sbrk (0);

      heap_bss_diff = (char *)my_heap_start - max (my_endbss, my_endbss_static);
    }

  sort_args (argc, argv);
  argc = 0;
  while (argv[argc]) argc++;

  if (argmatch (argv, argc, "-version", "--version", 3, NULL, &skip_args)
      /* We don't know the version number unless this is a dumped Emacs.
         So ignore --version otherwise.  */
      && initialized)
    {
      Lisp_Object tem, tem2;
      tem = Fsymbol_value (intern ("emacs-version"));
      tem2 = Fsymbol_value (intern ("emacs-copyright"));
      if (!STRINGP (tem))
	{
	  fprintf (stderr, "Invalid value of `emacs-version'\n");
	  exit (1);
	}
      if (!STRINGP (tem2))
	{
	  fprintf (stderr, "Invalid value of `emacs-copyright'\n");
	  exit (1);
	}
      else
	{
	  printf ("GNU Emacs %s\n", SDATA (tem));
	  printf ("%s\n", SDATA(tem2));
	  printf ("GNU Emacs comes with ABSOLUTELY NO WARRANTY.\n");
	  printf ("You may redistribute copies of Emacs\n");
	  printf ("under the terms of the GNU General Public License.\n");
	  printf ("For more information about these matters, ");
	  printf ("see the file named COPYING.\n");
	  exit (0);
	}
    }

  /* Record (approximately) where the stack begins.  */
  stack_bottom = &stack_bottom_variable;

  clearerr (stdin);

#ifndef SYSTEM_MALLOC
  /* Arrange to get warning messages as memory fills up.  */
  memory_warnings (0, malloc_warning);

  /* Call malloc at least once, to run the initial __malloc_hook.
     Also call realloc and free for consistency.  */
  free (realloc (malloc (4), 4));

# ifndef SYNC_INPUT
  /* Arrange to disable interrupt input inside malloc etc.  */
  uninterrupt_malloc ();
# endif /* not SYNC_INPUT */
#endif	/* not SYSTEM_MALLOC */

  /* We do all file input/output as binary files.  When we need to translate
     newlines, we do that manually.  */
  _fmode = O_BINARY;

  /* Skip initial setlocale if LC_ALL is "C", as it's not needed in that case.
     The build procedure uses this while dumping, to ensure that the
     dumped Emacs does not have its system locale tables initialized,
     as that might cause screwups when the dumped Emacs starts up.  */
  {
    char *lc_all = getenv ("LC_ALL");
    do_initial_setlocale = ! lc_all || strcmp (lc_all, "C");
  }

  /* Set locale now, so that initial error messages are localized properly.
     fixup_locale must wait until later, since it builds strings.  */
  if (do_initial_setlocale)
    setlocale (LC_ALL, "");

  inhibit_window_system = 0;

  /* Handle the -batch switch, which means don't do interactive display.  */
  noninteractive = 0;
  if (argmatch (argv, argc, "-batch", "--batch", 5, NULL, &skip_args))
    {
      noninteractive = 1;
      Vundo_outer_limit = Qnil;
    }
  if (argmatch (argv, argc, "-script", "--script", 3, &junk, &skip_args))
    {
      noninteractive = 1;	/* Set batch mode.  */
      /* Convert --script to -scriptload, un-skip it, and sort again
	 so that it will be handled in proper sequence.  */
      /* FIXME broken for --script=FILE - is that supposed to work?  */
      argv[skip_args - 1] = "-scriptload";
      skip_args -= 2;
      sort_args (argc, argv);
    }

  /* Handle the --help option, which gives a usage message.  */
  if (argmatch (argv, argc, "-help", "--help", 3, NULL, &skip_args))
    {
      printf (USAGE1, argv[0], USAGE2);
      printf (USAGE3);
      printf (USAGE4, bug_reporting_address ());
      exit (0);
    }

  if (argmatch (argv, argc, "-daemon", "--daemon", 5, NULL, &skip_args)
      || argmatch (argv, argc, "-daemon", "--daemon", 5, &dname_arg, &skip_args))
    {
      fprintf (stderr, "This platform does not support the -daemon flag.\n");
      exit (1);
    }

  init_signals ();

  /* Don't catch SIGHUP if dumping.  */
  if (1
#ifndef CANNOT_DUMP
      && initialized
#endif
      )
    {
      sigblock (sigmask (SIGHUP));
      /* In --batch mode, don't catch SIGHUP if already ignored.
	 That makes nohup work.  */
      if (! noninteractive
	  || signal (SIGHUP, SIG_IGN) != SIG_IGN)
	signal (SIGHUP, fatal_error_signal);
      sigunblock (sigmask (SIGHUP));
    }

  if (
#ifndef CANNOT_DUMP
      ! noninteractive || initialized
#else
      1
#endif
      )
    {
      /* Don't catch these signals in batch mode if dumping.
	 On some machines, this sets static data that would make
	 signal fail to work right when the dumped Emacs is run.  */
      signal (SIGQUIT, fatal_error_signal);
      signal (SIGILL, fatal_error_signal);
      signal (SIGTRAP, fatal_error_signal);
#ifdef SIGABRT
      signal (SIGABRT, fatal_error_signal);
#endif
      signal (SIGFPE, fatal_error_signal);
      signal (SIGSEGV, fatal_error_signal);
      signal (SIGTERM, fatal_error_signal);
    }

  noninteractive1 = noninteractive;

/* Perform basic initializations (not merely interning symbols).  */

  if (!initialized)
    {
      init_alloc_once ();
      init_obarray ();
      init_eval_once ();
      init_character_once ();
      init_charset_once ();
      init_coding_once ();
      init_syntax_once ();	/* Create standard syntax table.  */
      init_category_once ();	/* Create standard category table.  */
		      /* Must be done before init_buffer.  */
      init_casetab_once ();
      init_buffer_once ();	/* Create buffer table and some buffers.  */
      init_minibuf_once ();	/* Create list of minibuffers.  */
				/* Must precede init_window_once.  */

      /* Call syms_of_xfaces before init_window_once because that
	 function creates Vterminal_frame.  Termcap frames now use
	 faces, and the face implementation uses some symbols as
	 face names.  */
      syms_of_xfaces ();
      /* XXX syms_of_keyboard uses some symbols in keymap.c.  It would
         be better to arrange things not to have this dependency.  */
      syms_of_keymap ();
      /* Call syms_of_keyboard before init_window_once because
	 keyboard sets up symbols that include some face names that
	 the X support will want to use.  This can happen when
	 CANNOT_DUMP is defined.  */
      syms_of_keyboard ();

      /* Called before syms_of_fileio, because it sets up Qerror_condition.  */
      syms_of_data ();
      syms_of_fileio ();
      /* Before syms_of_coding to initialize Vgc_cons_threshold.  */
      syms_of_alloc ();
      /* Before syms_of_coding because it initializes Qcharsetp.  */
      syms_of_charset ();
      /* Before init_window_once, because it sets up the
	 Vcoding_system_hash_table.  */
      syms_of_coding ();	/* This should be after syms_of_fileio.  */

      init_window_once ();	/* Init the window system.  */
      init_fileio_once ();	/* Must precede any path manipulation.  */
#ifdef HAVE_WINDOW_SYSTEM
      init_fringe_once ();	/* Swap bitmaps if necessary. */
#endif /* HAVE_WINDOW_SYSTEM */
    }

  init_alloc ();

  if (do_initial_setlocale)
    {
      fixup_locale ();
      Vsystem_messages_locale = Vprevious_system_messages_locale;
      Vsystem_time_locale = Vprevious_system_time_locale;
    }

  init_eval ();
  init_data ();
#ifdef CLASH_DETECTION
  init_filelock ();
#endif
  init_atimer ();
  running_asynch_code = 0;

  /* Handle --unibyte and the EMACS_UNIBYTE envvar,
     but not while dumping.  */
  if (1)
    {
      int inhibit_unibyte = 0;

      /* --multibyte overrides EMACS_UNIBYTE.  */
      if (argmatch (argv, argc, "-no-unibyte", "--no-unibyte", 4, NULL, &skip_args)
	  || argmatch (argv, argc, "-multibyte", "--multibyte", 4, NULL, &skip_args)
	  /* Ignore EMACS_UNIBYTE before dumping.  */
	  || (!initialized && noninteractive))
	inhibit_unibyte = 1;

      /* --unibyte requests that we set up to do everything with single-byte
	 buffers and strings.  We need to handle this before calling
	 init_lread, init_editfns and other places that generate Lisp strings
	 from text in the environment.  */
      /* Actually this shouldn't be needed as of 20.4 in a generally
	 unibyte environment.  As handa says, environment values
	 aren't now decoded; also existing buffers are now made
	 unibyte during startup if .emacs sets unibyte.  Tested with
	 8-bit data in environment variables and /etc/passwd, setting
	 unibyte and Latin-1 in .emacs. -- Dave Love  */
      if (argmatch (argv, argc, "-unibyte", "--unibyte", 4, NULL, &skip_args)
	  || argmatch (argv, argc, "-no-multibyte", "--no-multibyte", 4, NULL, &skip_args)
	  || (getenv ("EMACS_UNIBYTE") && !inhibit_unibyte))
	{
	  Lisp_Object old_log_max;
	  Lisp_Object symbol, tail;

	  symbol = intern ("default-enable-multibyte-characters");
	  Fset (symbol, Qnil);

	  if (initialized)
	    {
	      /* Erase pre-dump messages in *Messages* now so no abort.  */
	      old_log_max = Vmessage_log_max;
	      XSETFASTINT (Vmessage_log_max, 0);
	      message_dolog ("", 0, 1, 0);
	      Vmessage_log_max = old_log_max;
	    }

	  for (tail = Vbuffer_alist; CONSP (tail);
	       tail = XCDR (tail))
	    {
	      Lisp_Object buffer;

	      buffer = Fcdr (XCAR (tail));
	      /* Make a multibyte buffer unibyte.  */
	      if (BUF_Z_BYTE (XBUFFER (buffer)) > BUF_Z (XBUFFER (buffer)))
		{
		  struct buffer *current = current_buffer;

		  set_buffer_temp (XBUFFER (buffer));
		  Fset_buffer_multibyte (Qnil);
		  set_buffer_temp (current);
		}
	    }
	}
    }

  no_loadup
    = argmatch (argv, argc, "-nl", "--no-loadup", 6, NULL, &skip_args);

  /* argmatch must not be used after here,
     except when bulding temacs
     because the -d argument has not been skipped in skip_args.  */

#ifdef WINDOWSNT
  globals_of_w32 ();
  /* Initialize environment from registry settings.  */
  init_environment (argv);
  init_ntproc ();	/* must precede init_editfns.  */
#endif

  /* egetenv is a pretty low-level facility, which may get called in
     many circumstances; it seems flimsy to put off initializing it
     until calling init_callproc.  */
  set_initial_environment ();

  init_buffer ();	/* Init default directory of main buffer.  */

  init_callproc_1 ();	/* Must precede init_cmdargs and init_sys_modes.  */
  init_cmdargs (argc, argv, skip_args);	/* Must precede init_lread.  */

  if (initialized)
    {
      /* Erase any pre-dump messages in the message log, to avoid confusion.  */
      Lisp_Object old_log_max;
      old_log_max = Vmessage_log_max;
      XSETFASTINT (Vmessage_log_max, 0);
      message_dolog ("", 0, 1, 0);
      Vmessage_log_max = old_log_max;
    }

  init_callproc ();	/* Must follow init_cmdargs but not init_sys_modes.  */
  init_lread ();

  /* Intern the names of all standard functions and variables;
     define standard keys.  */

  if (!initialized)
    {
      /* The basic levels of Lisp must come first.  */
      /* And data must come first of all
	 for the sake of symbols like error-message.  */
      syms_of_data ();
      syms_of_chartab ();
      syms_of_lread ();
      syms_of_print ();
      syms_of_eval ();
      syms_of_fns ();
      syms_of_floatfns ();

      syms_of_buffer ();
      syms_of_bytecode ();
      syms_of_callint ();
      syms_of_casefiddle ();
      syms_of_casetab ();
      syms_of_callproc ();
      syms_of_category ();
      syms_of_ccl ();
      syms_of_character ();
      syms_of_cmds ();
#ifndef NO_DIR_LIBRARY
      syms_of_dired ();
#endif /* not NO_DIR_LIBRARY */
      syms_of_display ();
      syms_of_doc ();
      syms_of_editfns ();
      syms_of_emacs ();
#ifdef CLASH_DETECTION
      syms_of_filelock ();
#endif /* CLASH_DETECTION */
      syms_of_indent ();
      syms_of_insdel ();
      /* syms_of_keymap (); */
      syms_of_macros ();
      syms_of_marker ();
      syms_of_minibuf ();
      syms_of_process ();
      syms_of_search ();
      syms_of_frame ();
      syms_of_syntax ();
      syms_of_terminal ();
      syms_of_term ();

      syms_of_undo ();
#ifdef HAVE_SOUND
      syms_of_sound ();
#endif
      syms_of_textprop ();
      syms_of_composite ();
#ifdef WINDOWSNT
      syms_of_ntproc ();
#endif /* WINDOWSNT */
      syms_of_window ();
      syms_of_xdisp ();
      syms_of_font ();
#ifdef HAVE_WINDOW_SYSTEM
      syms_of_fringe ();
      syms_of_image ();
#endif /* HAVE_WINDOW_SYSTEM */

      syms_of_menu ();

#ifdef HAVE_NTGUI
      syms_of_w32term ();
      syms_of_w32fns ();
      syms_of_w32select ();
      syms_of_w32menu ();
      syms_of_fontset ();
#endif /* HAVE_NTGUI */

#ifdef SYMS_SYSTEM
      SYMS_SYSTEM;
#endif

      keys_of_casefiddle ();
      keys_of_cmds ();
      keys_of_buffer ();
      keys_of_keyboard ();
      keys_of_keymap ();
      keys_of_window ();
    }
  else
    {
      /* Initialization that must be done even if the global variable
	 initialized is non zero.  */
#ifdef HAVE_NTGUI
      globals_of_w32fns ();
      globals_of_w32menu ();
      globals_of_w32select ();
#endif  /* HAVE_NTGUI */
    }

  init_charset ();

  init_editfns (); /* init_process uses Voperating_system_release. */
  init_process (); /* init_display uses add_keyboard_wait_descriptor. */
  init_keyboard ();	/* This too must precede init_sys_modes.  */
  if (!noninteractive)
    init_display ();	/* Determine terminal type.  Calls init_sys_modes.  */
  init_fns ();
  init_xdisp ();
#ifdef HAVE_WINDOW_SYSTEM
  init_fringe ();
  init_image ();
#endif /* HAVE_WINDOW_SYSTEM */
  init_macros ();
  init_floatfns ();
#ifdef HAVE_SOUND
  init_sound ();
#endif
  init_window ();

  if (!initialized)
    {
      char *file;
      /* Handle -l loadup, args passed by Makefile.  */
      if (argmatch (argv, argc, "-l", "--load", 3, &file, &skip_args))
	Vtop_level = Fcons (intern ("load"),
			    Fcons (build_string (file), Qnil));
      /* Unless next switch is -nl, load "loadup.el" first thing.  */
      if (! no_loadup)
	Vtop_level = Fcons (intern ("load"),
			    Fcons (build_string ("loadup.el"), Qnil));
    }

  if (initialized)
    {
#ifdef HAVE_TZSET
      {
	/* If the execution TZ happens to be the same as the dump TZ,
	   change it to some other value and then change it back,
	   to force the underlying implementation to reload the TZ info.
	   This is needed on implementations that load TZ info from files,
	   since the TZ file contents may differ between dump and execution.  */
	char *tz = getenv ("TZ");
	if (tz && !strcmp (tz, dump_tz))
	  {
	    ++*tz;
	    tzset ();
	    --*tz;
	  }
      }
#endif
    }

  initialized = 1;

#ifdef LOCALTIME_CACHE
  /* Some versions of localtime have a bug.  They cache the value of the time
     zone rather than looking it up every time.  Since localtime() is
     called to bolt the undumping time into the undumped emacs, this
     results in localtime ignoring the TZ environment variable.
     This flushes the new TZ value into localtime.  */
  tzset ();
#endif /* defined (LOCALTIME_CACHE) */

  /* Enter editor command loop.  This never returns.  */
  Frecursive_edit ();
  /* NOTREACHED */
  return 0;
}

/* Sort the args so we can find the most important ones
   at the beginning of argv.  */

/* First, here's a table of all the standard options.  */

struct standard_args
{
  char *name;
  char *longname;
  int priority;
  int nargs;
};

struct standard_args standard_args[] =
{
  { "-version", "--version", 150, 0 },
  { "-batch", "--batch", 100, 0 },
  { "-script", "--script", 100, 1 },
  { "-daemon", "--daemon", 99, 0 },
  { "-help", "--help", 90, 0 },
  { "-no-unibyte", "--no-unibyte", 83, 0 },
  { "-multibyte", "--multibyte", 82, 0 },
  { "-unibyte", "--unibyte", 81, 0 },
  { "-no-multibyte", "--no-multibyte", 80, 0 },
  { "-nl", "--no-loadup", 70, 0 },
  /* -d must come last before the options handled in startup.el.  */
  { "-d", "--display", 60, 1 },
  { "-display", 0, 60, 1 },
  /* Now for the options handled in startup.el.  */
  { "-Q", "--quick", 55, 0 },
  { "-quick", 0, 55, 0 },
  { "-q", "--no-init-file", 50, 0 },
  { "-no-init-file", 0, 50, 0 },
  { "-no-site-file", "--no-site-file", 40, 0 },
  { "-u", "--user", 30, 1 },
  { "-user", 0, 30, 1 },
  { "-debug-init", "--debug-init", 20, 0 },
  { "-nbi", "--no-bitmap-icon", 15, 0 },
  { "-iconic", "--iconic", 15, 0 },
  { "-D", "--basic-display", 12, 0},
  { "-basic-display", 0, 12, 0},
  { "-bg", "--background-color", 10, 1 },
  { "-background", 0, 10, 1 },
  { "-fg", "--foreground-color", 10, 1 },
  { "-foreground", 0, 10, 1 },
  { "-bd", "--border-color", 10, 1 },
  { "-bw", "--border-width", 10, 1 },
  { "-ib", "--internal-border", 10, 1 },
  { "-ms", "--mouse-color", 10, 1 },
  { "-cr", "--cursor-color", 10, 1 },
  { "-nbc", "--no-blinking-cursor", 10, 0 },
  { "-fn", "--font", 10, 1 },
  { "-font", 0, 10, 1 },
  { "-fs", "--fullscreen", 10, 0 },
  { "-fw", "--fullwidth", 10, 0 },
  { "-fh", "--fullheight", 10, 0 },
  { "-g", "--geometry", 10, 1 },
  { "-geometry", 0, 10, 1 },
  { "-T", "--title", 10, 1 },
  { "-title", 0, 10, 1 },
  { "-name", "--name", 10, 1 },
  { "-xrm", "--xrm", 10, 1 },
  { "-parent-id", "--parent-id", 10, 1 },
  { "-r", "--reverse-video", 5, 0 },
  { "-rv", 0, 5, 0 },
  { "-reverse", 0, 5, 0 },
  { "-hb", "--horizontal-scroll-bars", 5, 0 },
  { "-vb", "--vertical-scroll-bars", 5, 0 },
  { "-color", "--color", 5, 0},
  { "-no-splash", "--no-splash", 3, 0 },
  { "-no-desktop", "--no-desktop", 3, 0 },
  /* These have the same priority as ordinary file name args,
     so they are not reordered with respect to those.  */
  { "-L", "--directory", 0, 1 },
  { "-directory", 0, 0, 1 },
  { "-l", "--load", 0, 1 },
  { "-load", 0, 0, 1 },
  /* This has no longname, because using --scriptload confuses sort_args,
     because then the --script long option seems to match twice; ie
     you can't have a long option which is a prefix of another long
     option.  In any case, this is entirely an internal option.  */
  { "-scriptload", NULL, 0, 1 },
  { "-f", "--funcall", 0, 1 },
  { "-funcall", 0, 0, 1 },
  { "-eval", "--eval", 0, 1 },
  { "-execute", "--execute", 0, 1 },
  { "-find-file", "--find-file", 0, 1 },
  { "-visit", "--visit", 0, 1 },
  { "-file", "--file", 0, 1 },
  { "-insert", "--insert", 0, 1 },

  /* This should be processed after ordinary file name args and the like.  */
  { "-kill", "--kill", -10, 0 },
};

/* Reorder the elements of ARGV (assumed to have ARGC elements)
   so that the highest priority ones come first.
   Do not change the order of elements of equal priority.
   If an option takes an argument, keep it and its argument together.

   If an option that takes no argument appears more
   than once, eliminate all but one copy of it.  */

static void
sort_args (argc, argv)
     int argc;
     char **argv;
{
  char **new = (char **) xmalloc (sizeof (char *) * argc);
  /* For each element of argv,
     the corresponding element of options is:
     0 for an option that takes no arguments,
     1 for an option that takes one argument, etc.
     -1 for an ordinary non-option argument.  */
  int *options = (int *) xmalloc (sizeof (int) * argc);
  int *priority = (int *) xmalloc (sizeof (int) * argc);
  int to = 1;
  int incoming_used = 1;
  int from;
  int i;

  /* Categorize all the options,
     and figure out which argv elts are option arguments.  */
  for (from = 1; from < argc; from++)
    {
      options[from] = -1;
      priority[from] = 0;
      if (argv[from][0] == '-')
	{
	  int match, thislen;
	  char *equals;

	  /* If we have found "--", don't consider
	     any more arguments as options.  */
	  if (argv[from][1] == '-' && argv[from][2] == 0)
	    {
	      /* Leave the "--", and everything following it, at the end.  */
	      for (; from < argc; from++)
		{
		  priority[from] = -100;
		  options[from] = -1;
		}
	      break;
	    }

	  /* Look for a match with a known old-fashioned option.  */
	  for (i = 0; i < sizeof (standard_args) / sizeof (standard_args[0]); i++)
	    if (!strcmp (argv[from], standard_args[i].name))
	      {
		options[from] = standard_args[i].nargs;
		priority[from] = standard_args[i].priority;
		if (from + standard_args[i].nargs >= argc)
		  fatal ("Option `%s' requires an argument\n", argv[from]);
		from += standard_args[i].nargs;
		goto done;
	      }

	  /* Look for a match with a known long option.
	     MATCH is -1 if no match so far, -2 if two or more matches so far,
	     >= 0 (the table index of the match) if just one match so far.  */
	  if (argv[from][1] == '-')
	    {
	      match = -1;
	      thislen = strlen (argv[from]);
	      equals = index (argv[from], '=');
	      if (equals != 0)
		thislen = equals - argv[from];

	      for (i = 0;
		   i < sizeof (standard_args) / sizeof (standard_args[0]); i++)
		if (standard_args[i].longname
		    && !strncmp (argv[from], standard_args[i].longname,
				 thislen))
		  {
		    if (match == -1)
		      match = i;
		    else
		      match = -2;
		  }

	      /* If we found exactly one match, use that.  */
	      if (match >= 0)
		{
		  options[from] = standard_args[match].nargs;
		  priority[from] = standard_args[match].priority;
		  /* If --OPTION=VALUE syntax is used,
		     this option uses just one argv element.  */
		  if (equals != 0)
		    options[from] = 0;
		  if (from + options[from] >= argc)
		    fatal ("Option `%s' requires an argument\n", argv[from]);
		  from += options[from];
		}
	      /* FIXME When match < 0, shouldn't there be some error,
		 or at least indication to the user that there was a
		 problem?  */
	    }
	done: ;
	}
    }

  /* Copy the arguments, in order of decreasing priority, to NEW.  */
  new[0] = argv[0];
  while (incoming_used < argc)
    {
      int best = -1;
      int best_priority = -9999;

      /* Find the highest priority remaining option.
	 If several have equal priority, take the first of them.  */
      for (from = 1; from < argc; from++)
	{
	  if (argv[from] != 0 && priority[from] > best_priority)
	    {
	      best_priority = priority[from];
	      best = from;
	    }
	  /* Skip option arguments--they are tied to the options.  */
	  if (options[from] > 0)
	    from += options[from];
	}

      if (best < 0)
	abort ();

      /* Copy the highest priority remaining option, with its args, to NEW.
         Unless it is a duplicate of the previous one.  */
      if (! (options[best] == 0
	     && ! strcmp (new[to - 1], argv[best])))
	{
	  new[to++] = argv[best];
	  for (i = 0; i < options[best]; i++)
	    new[to++] = argv[best + i + 1];
	}

      incoming_used += 1 + (options[best] > 0 ? options[best] : 0);

      /* Clear out this option in ARGV.  */
      argv[best] = 0;
      for (i = 0; i < options[best]; i++)
	argv[best + i + 1] = 0;
    }

  /* If duplicate options were deleted, fill up extra space with null ptrs.  */
  while (to < argc)
    new[to++] = 0;

  bcopy (new, argv, sizeof (char *) * argc);

  xfree (options);
  xfree (new);
  xfree (priority);
}

DEFUN ("kill-emacs", Fkill_emacs, Skill_emacs, 0, 1, "P",
       doc: /* Exit the Emacs job and kill it.
If ARG is an integer, return ARG as the exit program code.
If ARG is a string, stuff it as keyboard input.

The value of `kill-emacs-hook', if not void,
is a list of functions (of no args),
all of which are called before Emacs is actually killed.  */)
     (arg)
     Lisp_Object arg;
{
  if (feof (stdin))
    arg = Qt;

  if (!NILP (Vrun_hooks) && !noninteractive)
    call1 (Vrun_hooks, intern ("kill-emacs-hook"));

  shut_down_emacs (0, 0, STRINGP (arg) ? arg : Qnil);

  /* If we have an auto-save list file,
     kill it because we are exiting Emacs deliberately (not crashing).
     Do it after shut_down_emacs, which does an auto-save.  */
  if (STRINGP (Vauto_save_list_file_name))
    unlink (SDATA (Vauto_save_list_file_name));

  exit (INTEGERP (arg) ? XINT (arg) : EXIT_SUCCESS);
  /* NOTREACHED */
  return Qnil;
}


/* Perform an orderly shutdown of Emacs.  Autosave any modified
   buffers, kill any child processes, clean up the terminal modes (if
   we're in the foreground), and other stuff like that.  Don't perform
   any redisplay; this may be called when Emacs is shutting down in
   the background, or after its X connection has died.

   If SIG is a signal number, print a message for it.

   This is called by fatal signal handlers, X protocol error handlers,
   and Fkill_emacs.  */

void
shut_down_emacs (sig, no_x, stuff)
     int sig, no_x;
     Lisp_Object stuff;
{
  /* Prevent running of hooks from now on.  */
  Vrun_hooks = Qnil;

  /* Don't update display from now on.  */
  Vinhibit_redisplay = Qt;

  fflush (stdout);
  reset_all_sys_modes ();

  stuff_buffered_input (stuff);

#ifdef subprocesses
  inhibit_sentinels = 1;
#endif
  kill_buffer_processes (Qnil);
  Fdo_auto_save (Qt, Qnil);

#ifdef CLASH_DETECTION
  unlock_all_files ();
#endif

#ifdef WINDOWSNT
  term_ntproc ();
#endif

  /* Do this only if terminating normally, we want glyph matrices
     etc. in a core dump.  */
  if (sig == 0 || sig == SIGTERM)
    {
      check_glyph_memory ();
      check_message_stack ();
    }
}



#ifndef CANNOT_DUMP
DEFUN ("dump-emacs", Fdump_emacs, Sdump_emacs, 2, 2, 0,
       doc: /* Dump current state of Emacs into executable file FILENAME.
Take symbols from SYMFILE (presumably the file you executed to run Emacs).
This is used in the file `loadup.el' when building Emacs.

You must run Emacs in batch mode in order to dump it.  */)
     (filename, symfile)
     Lisp_Object filename, symfile;
{
  extern char my_edata[];
  Lisp_Object tem;
  Lisp_Object symbol;
  int count = SPECPDL_INDEX ();

  check_pure_size ();

  if (! noninteractive)
    error ("Dumping Emacs works only in batch mode");

  /* Bind `command-line-processed' to nil before dumping,
     so that the dumped Emacs will process its command line
     and set up to work with X windows if appropriate.  */
  symbol = intern ("command-line-processed");
  specbind (symbol, Qnil);

  CHECK_STRING (filename);
  filename = Fexpand_file_name (filename, Qnil);
  if (!NILP (symfile))
    {
      CHECK_STRING (symfile);
      if (SCHARS (symfile))
	symfile = Fexpand_file_name (symfile, Qnil);
    }

  tem = Vpurify_flag;
  Vpurify_flag = Qnil;

#ifdef HAVE_TZSET
  set_time_zone_rule (dump_tz);
#endif

  fflush (stdout);

  unexec (SDATA (filename),
	  !NILP (symfile) ? SDATA (symfile) : 0, my_edata, 0, 0);

  Vpurify_flag = tem;

  return unbind_to (count, Qnil);
}

#endif /* not CANNOT_DUMP */

#if HAVE_SETLOCALE
/* Recover from setlocale (LC_ALL, "").  */
void
fixup_locale ()
{
  /* The Emacs Lisp reader needs LC_NUMERIC to be "C",
     so that numbers are read and printed properly for Emacs Lisp.  */
  setlocale (LC_NUMERIC, "C");
}

/* Set system locale CATEGORY, with previous locale *PLOCALE, to
   DESIRED_LOCALE.  */
static void
synchronize_locale (category, plocale, desired_locale)
     int category;
     Lisp_Object *plocale;
     Lisp_Object desired_locale;
{
  if (! EQ (*plocale, desired_locale))
    {
      *plocale = desired_locale;
      setlocale (category, (STRINGP (desired_locale)
			    ? (char *) SDATA (desired_locale)
			    : ""));
    }
}

/* Set system time locale to match Vsystem_time_locale, if possible.  */
void
synchronize_system_time_locale ()
{
  synchronize_locale (LC_TIME, &Vprevious_system_time_locale,
		      Vsystem_time_locale);
}

/* Set system messages locale to match Vsystem_messages_locale, if
   possible.  */
void
synchronize_system_messages_locale ()
{
}
#endif /* HAVE_SETLOCALE */

Lisp_Object
decode_env_path (evarname, defalt)
     char *evarname, *defalt;
{
  register char *path, *p;
  Lisp_Object lpath, element, tem;

  /* It's okay to use getenv here, because this function is only used
     to initialize variables when Emacs starts up, and isn't called
     after that.  */
  if (evarname != 0)
    path = (char *) getenv (evarname);
  else
    path = 0;
  if (!path)
    path = defalt;
#ifdef DOS_NT
  /* Ensure values from the environment use the proper directory separator.  */
  if (path)
    {
      p = alloca (strlen (path) + 1);
      strcpy (p, path);
      path = p;

      if ('/' == DIRECTORY_SEP)
	dostounix_filename (path);
      else
	unixtodos_filename (path);
    }
#endif
  lpath = Qnil;
  while (1)
    {
      p = index (path, SEPCHAR);
      if (!p) p = path + strlen (path);
      element = (p - path ? make_string (path, p - path)
		 : build_string ("."));

      /* Add /: to the front of the name
	 if it would otherwise be treated as magic.  */
      tem = Ffind_file_name_handler (element, Qt);

      /* However, if the handler says "I'm safe",
	 don't bother adding /:.  */
      if (SYMBOLP (tem))
	{
	  Lisp_Object prop;
	  prop = Fget (tem, intern ("safe-magic"));
	  if (! NILP (prop))
	    tem = Qnil;
	}

      if (! NILP (tem))
	element = concat2 (build_string ("/:"), element);

      lpath = Fcons (element, lpath);
      if (*p)
	path = p + 1;
      else
	break;
    }
  return Fnreverse (lpath);
}

DEFUN ("daemonp", Fdaemonp, Sdaemonp, 0, 0, 0,
       doc: /* Return non-nil if the current emacs process is a daemon.
If the daemon was given a name argument, return that name. */)
  ()
{
    return Qnil;
}

void
syms_of_emacs ()
{
  Qfile_name_handler_alist = intern ("file-name-handler-alist");
  staticpro (&Qfile_name_handler_alist);

#ifndef CANNOT_DUMP
  defsubr (&Sdump_emacs);
#endif

  defsubr (&Skill_emacs);

  defsubr (&Sinvocation_name);
  defsubr (&Sinvocation_directory);
  defsubr (&Sdaemonp);

  DEFVAR_LISP ("command-line-args", &Vcommand_line_args,
	       doc: /* Args passed by shell to Emacs, as a list of strings.
Many arguments are deleted from the list as they are processed.  */);

  DEFVAR_LISP ("system-type", &Vsystem_type,
	       doc: /* Value is symbol indicating type of operating system you are using.
Special values:
  `gnu'         compiled for a GNU Hurd system.
  `gnu/linux'   compiled for a GNU/Linux system.
  `darwin'      compiled for Darwin (GNU-Darwin, Mac OS X, ...).
  `ms-dos'      compiled as an MS-DOS application.
  `windows-nt'  compiled as a native W32 application.
  `cygwin'      compiled using the Cygwin library.
Anything else indicates some sort of Unix system.  */);
  Vsystem_type = intern (SYSTEM_TYPE);

  DEFVAR_LISP ("system-configuration", &Vsystem_configuration,
	       doc: /* Value is string indicating configuration Emacs was built for.
On MS-Windows, the value reflects the OS flavor and version on which
Emacs is running.  */);
  Vsystem_configuration = build_string (EMACS_CONFIGURATION);

  DEFVAR_LISP ("system-configuration-options", &Vsystem_configuration_options,
	       doc: /* String containing the configuration options Emacs was built with.  */);
  Vsystem_configuration_options = build_string (EMACS_CONFIG_OPTIONS);

  DEFVAR_BOOL ("noninteractive", &noninteractive1,
	       doc: /* Non-nil means Emacs is running without interactive terminal.  */);

  DEFVAR_LISP ("kill-emacs-hook", &Vkill_emacs_hook,
	       doc: /* Hook to be run when `kill-emacs' is called.
Since `kill-emacs' may be invoked when the terminal is disconnected (or
in other similar situations), functions placed on this hook should not
expect to be able to interact with the user.  To ask for confirmation,
see `kill-emacs-query-functions' instead.

The hook is not run in batch mode, i.e., if `noninteractive' is non-nil.  */);
  Vkill_emacs_hook = Qnil;

  DEFVAR_INT ("emacs-priority", &emacs_priority,
	      doc: /* Priority for Emacs to run at.
This value is effective only if set before Emacs is dumped,
and only if the Emacs executable is installed with setuid to permit
it to change priority.  (Emacs sets its uid back to the real uid.)
Currently, you need to define SET_EMACS_PRIORITY in `config.h'
before you compile Emacs, to enable the code for this feature.  */);
  emacs_priority = 0;

  DEFVAR_LISP ("path-separator", &Vpath_separator,
	       doc: /* String containing the character that separates directories in
search paths, such as PATH and other similar environment variables.  */);
  {
    char c = SEPCHAR;
    Vpath_separator = make_string (&c, 1);
  }

  DEFVAR_LISP ("invocation-name", &Vinvocation_name,
	       doc: /* The program name that was used to run Emacs.
Any directory names are omitted.  */);

  DEFVAR_LISP ("invocation-directory", &Vinvocation_directory,
	       doc: /* The directory in which the Emacs executable was found, to run it.
The value is nil if that directory's name is not known.  */);

  DEFVAR_LISP ("installation-directory", &Vinstallation_directory,
	       doc: /* A directory within which to look for the `lib-src' and `etc' directories.
This is non-nil when we can't find those directories in their standard
installed locations, but we can find them near where the Emacs executable
was found.  */);
  Vinstallation_directory = Qnil;

  DEFVAR_LISP ("system-messages-locale", &Vsystem_messages_locale,
	       doc: /* System locale for messages.  */);
  Vsystem_messages_locale = Qnil;

  DEFVAR_LISP ("previous-system-messages-locale",
	       &Vprevious_system_messages_locale,
	       doc: /* Most recently used system locale for messages.  */);
  Vprevious_system_messages_locale = Qnil;

  DEFVAR_LISP ("system-time-locale", &Vsystem_time_locale,
	       doc: /* System locale for time.  */);
  Vsystem_time_locale = Qnil;

  DEFVAR_LISP ("previous-system-time-locale", &Vprevious_system_time_locale,
	       doc: /* Most recently used system locale for time.  */);
  Vprevious_system_time_locale = Qnil;

  DEFVAR_LISP ("before-init-time", &Vbefore_init_time,
	       doc: /* Value of `current-time' before Emacs begins initialization.  */);
  Vbefore_init_time = Qnil;

  DEFVAR_LISP ("after-init-time", &Vafter_init_time,
	       doc: /* Value of `current-time' after loading the init files.
This is nil during initialization.  */);
  Vafter_init_time = Qnil;
}

/* arch-tag: 7bfd356a-c720-4612-8ab6-aa4222931c2e
   (do not change this comment) */
