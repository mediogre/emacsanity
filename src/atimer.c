/* Asynchronous timers.
   Copyright (C) 2000, 2001, 2002, 2003, 2004, 2005,
                 2006, 2007, 2008, 2009  Free Software Foundation, Inc.

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
#include <lisp.h>
#include <syssignal.h>
#include <systime.h>
#include <blockinput.h>
#include <atimer.h>

#ifdef HAVE_UNISTD_H
#include <unistd.h>
#endif

#ifdef HAVE_SYS_TIME_H
#include <sys/time.h>
#endif

/* Free-list of atimer structures.  */

static struct atimer *free_atimers;

/* List of currently not running timers due to a call to
   lock_atimer.  */

static struct atimer *stopped_atimers;

/* List of active atimers, sorted by expiration time.  The timer that
   will become ripe next is always at the front of this list.  */

static struct atimer *atimers;

/* Function prototypes.  */

static void schedule_atimer P_ ((struct atimer *));
static struct atimer *append_atimer_lists P_ ((struct atimer *,
					       struct atimer *));

/* Start a new atimer of type TYPE.  TIME specifies when the timer is
   ripe.  FN is the function to call when the timer fires.
   CLIENT_DATA is stored in the client_data member of the atimer
   structure returned and so made available to FN when it is called.

   If TYPE is ATIMER_ABSOLUTE, TIME is the absolute time at which the
   timer fires.

   If TYPE is ATIMER_RELATIVE, the timer is ripe TIME s/us in the
   future.

   In both cases, the timer is automatically freed after it has fired.

   If TYPE is ATIMER_CONTINUOUS, the timer fires every TIME s/us.

   Value is a pointer to the atimer started.  It can be used in calls
   to cancel_atimer; don't free it yourself.  */

struct atimer *
start_atimer (type, time, fn, client_data)
     enum atimer_type type;
     EMACS_TIME time;
     atimer_callback fn;
     void *client_data;
{
  struct atimer *t;

  /* Round TIME up to the next full second if we don't have
     itimers.  */
#ifndef HAVE_SETITIMER
  if (EMACS_USECS (time) != 0)
    {
      EMACS_SET_USECS (time, 0);
      EMACS_SET_SECS (time, EMACS_SECS (time) + 1);
    }
#endif /* not HAVE_SETITIMER */

  /* Get an atimer structure from the free-list, or allocate
     a new one.  */
  if (free_atimers)
    {
      t = free_atimers;
      free_atimers = t->next;
    }
  else
    t = (struct atimer *) xmalloc (sizeof *t);

  /* Fill the atimer structure.  */
  bzero (t, sizeof *t);
  t->type = type;
  t->fn = fn;
  t->client_data = client_data;

  /* Compute the timer's expiration time.  */
  switch (type)
    {
    case ATIMER_ABSOLUTE:
      t->expiration = time;
      break;

    case ATIMER_RELATIVE:
      EMACS_GET_TIME (t->expiration);
      EMACS_ADD_TIME (t->expiration, t->expiration, time);
      break;

    case ATIMER_CONTINUOUS:
      EMACS_GET_TIME (t->expiration);
      EMACS_ADD_TIME (t->expiration, t->expiration, time);
      t->interval = time;
      break;
    }

  /* Insert the timer in the list of active atimers.  */
  schedule_atimer (t);

  return t;
}


/* Cancel and free atimer TIMER.  */

void
cancel_atimer (timer)
     struct atimer *timer;
{
  int i;

  for (i = 0; i < 2; ++i)
    {
      struct atimer *t, *prev;
      struct atimer **list = i ? &stopped_atimers : &atimers;

      /* See if TIMER is active or stopped.  */
      for (t = *list, prev = NULL; t && t != timer; prev = t, t = t->next)
	;

      /* If it is, take it off the its list, and put in on the
	 free-list.  We don't bother to arrange for setting a
	 different alarm time, since a too early one doesn't hurt.  */
      if (t)
	{
	  if (prev)
	    prev->next = t->next;
	  else
	    *list = t->next;

	  t->next = free_atimers;
	  free_atimers = t;
	  break;
	}
    }
}


/* Append two lists of atimers LIST1 and LIST2 and return the
   result list.  */

static struct atimer *
append_atimer_lists (list1, list2)
     struct atimer *list1, *list2;
{
  if (list1 == NULL)
    return list2;
  else if (list2 == NULL)
    return list1;
  else
    {
      struct atimer *p;

      for (p = list1; p->next; p = p->next)
	;
      p->next = list2;
      return list1;
    }
}


/* Stop all timers except timer T.  T null means stop all timers.  */

void
stop_other_atimers (t)
     struct atimer *t;
{
  if (t)
    {
      struct atimer *p, *prev;

      /* See if T is active.  */
      for (p = atimers, prev = NULL; p && p != t; prev = p, p = p->next)
	;

      if (p == t)
	{
	  if (prev)
	    prev->next = t->next;
	  else
	    atimers = t->next;
	  t->next = NULL;
	}
      else
	/* T is not active.  Let's handle this like T == 0.  */
	t = NULL;
    }

  stopped_atimers = append_atimer_lists (atimers, stopped_atimers);
  atimers = t;
}


/* Run all timers again, if some have been stopped with a call to
   stop_other_atimers.  */

void
run_all_atimers ()
{
  if (stopped_atimers)
    {
      struct atimer *t = atimers;
      struct atimer *next;

      atimers = stopped_atimers;
      stopped_atimers = NULL;

      while (t)
	{
	  next = t->next;
	  schedule_atimer (t);
	  t = next;
	}
    }
}


/* A version of run_all_timers suitable for a record_unwind_protect.  */

Lisp_Object
unwind_stop_other_atimers (dummy)
     Lisp_Object dummy;
{
  run_all_atimers ();
  return Qnil;
}

/* Insert timer T into the list of active atimers `atimers', keeping
   the list sorted by expiration time.  T must not be in this list
   already.  */

static void
schedule_atimer (t)
     struct atimer *t;
{
  struct atimer *a = atimers, *prev = NULL;

  /* Look for the first atimer that is ripe after T.  */
  while (a && EMACS_TIME_GT (t->expiration, a->expiration))
    prev = a, a = a->next;

  /* Insert T in front of the atimer found, if any.  */
  if (prev)
    prev->next = t;
  else
    atimers = t;

  t->next = a;
}

void
init_atimer ()
{
  free_atimers = atimers = NULL;
}

/* arch-tag: e6308261-eec6-404b-89fb-6e5909518d70
   (do not change this comment) */
