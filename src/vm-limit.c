/* Functions for memory limit warnings.
   Copyright (C) 1990, 1992, 2001, 2002, 2003, 2004, 2005, 2006, 2007,
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

#ifdef emacs
#include <config.h>
#include "lisp.h"
#endif

#ifndef emacs
#include <stddef.h>
typedef size_t SIZE;
typedef void *POINTER;
#define EXCEEDS_LISP_PTR(x) 0
#endif

#include "mem-limits.h"

#ifdef HAVE_GETRLIMIT
#include <sys/resource.h>
#endif

/*
  Level number of warnings already issued.
  0 -- no warnings issued.
  1 -- 75% warning already issued.
  2 -- 85% warning already issued.
  3 -- 95% warning issued; keep warning frequently.
*/
enum warnlevel { not_warned, warned_75, warned_85, warned_95 };

static enum warnlevel warnlevel;

/* Function to call to issue a warning;
   0 means don't issue them.  */
static void (*warn_function) ();

/* Start of data space; can be changed by calling malloc_init.  */
static POINTER data_space_start;

/* Number of bytes of writable memory we can expect to be able to get.  */
static unsigned long lim_data;

static void
get_lim_data ()
{
  extern unsigned long reserved_heap_size;
  lim_data = reserved_heap_size;
}

/* Verify amount of memory available, complaining if we're near the end. */

static void
check_memory_limits ()
{
#ifdef REL_ALLOC
  extern POINTER (*real_morecore) ();
#endif
  extern POINTER (*__morecore) ();

  register POINTER cp;
  unsigned long five_percent;
  unsigned long data_size;
  enum warnlevel new_warnlevel;

  if (lim_data == 0)
    get_lim_data ();
  five_percent = lim_data / 20;

  /* Find current end of memory and issue warning if getting near max */
#ifdef REL_ALLOC
  if (real_morecore)
    cp = (char *) (*real_morecore) (0);
  else
#endif
  cp = (char *) (*__morecore) (0);
  data_size = (char *) cp - (char *) data_space_start;

  if (!warn_function)
    return;

  /* What level of warning does current memory usage demand?  */
  new_warnlevel
    = (data_size > five_percent * 19) ? warned_95
    : (data_size > five_percent * 17) ? warned_85
    : (data_size > five_percent * 15) ? warned_75
    : not_warned;

  /* If we have gone up a level, give the appropriate warning.  */
  if (new_warnlevel > warnlevel || new_warnlevel == warned_95)
    {
      warnlevel = new_warnlevel;
      switch (warnlevel)
	{
	case warned_75:
	  (*warn_function) ("Warning: past 75% of memory limit");
	  break;

	case warned_85:
	  (*warn_function) ("Warning: past 85% of memory limit");
	  break;

	case warned_95:
	  (*warn_function) ("Warning: past 95% of memory limit");
	}
    }
  /* Handle going down in usage levels, with some hysteresis.  */
  else
    {
      /* If we go down below 70% full, issue another 75% warning
	 when we go up again.  */
      if (data_size < five_percent * 14)
	warnlevel = not_warned;
      /* If we go down below 80% full, issue another 85% warning
	 when we go up again.  */
      else if (warnlevel > warned_75 && data_size < five_percent * 16)
	warnlevel = warned_75;
      /* If we go down below 90% full, issue another 95% warning
	 when we go up again.  */
      else if (warnlevel > warned_85 && data_size < five_percent * 18)
	warnlevel = warned_85;
    }

  if (EXCEEDS_LISP_PTR (cp))
    (*warn_function) ("Warning: memory in use exceeds lisp pointer size");
}

/* Enable memory usage warnings.
   START says where the end of pure storage is.
   WARNFUN specifies the function to call to issue a warning.  */

void
memory_warnings (start, warnfun)
     POINTER start;
     void (*warnfun) ();
{
  extern void (* __after_morecore_hook) ();     /* From gmalloc.c */

  if (start)
    data_space_start = start;
  else
    data_space_start = start_of_data ();

  warn_function = warnfun;
  __after_morecore_hook = check_memory_limits;

  /* Force data limit to be recalculated on each run.  */
  lim_data = 0;
}

/* arch-tag: eab04eda-1f69-447a-8d9f-95f0a3983ca5
   (do not change this comment) */
