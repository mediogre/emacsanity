/* Heap management routines for GNU Emacs on the Microsoft W32 API.
   Copyright (C) 1994, 2001, 2002, 2003, 2004, 2005, 2006, 2007,
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
   Geoff Voelker (voelker@cs.washington.edu)			     7-29-94
*/

#ifdef HAVE_CONFIG_H
#include <config.h>
#endif

#include <stdlib.h>
#include <stdio.h>

#include "w32heap.h"
#include "lisp.h"  /* for VALMASK */

#define RVA_TO_PTR(rva) ((unsigned char *)((DWORD)(rva) + (DWORD)GetModuleHandle (NULL)))

/* This gives us the page size and the size of the allocation unit on NT.  */
SYSTEM_INFO sysinfo_cache;

/* This gives us version, build, and platform identification.  */
OSVERSIONINFO osinfo_cache;

unsigned long syspage_mask = 0;

/* The major and minor versions of NT.  */
int w32_major_version;
int w32_minor_version;
int w32_build_number;

/* Distinguish between Windows NT and Windows 95.  */
int os_subtype;

/* Cache information describing the NT system for later use.  */
void
cache_system_info (void)
{
  union
    {
      struct info
	{
	  char  major;
	  char  minor;
	  short platform;
	} info;
      DWORD data;
    } version;

  /* Cache the version of the operating system.  */
  version.data = GetVersion ();
  w32_major_version = version.info.major;
  w32_minor_version = version.info.minor;

  if (version.info.platform & 0x8000)
    os_subtype = OS_WIN95;
  else
    os_subtype = OS_NT;

  /* Cache page size, allocation unit, processor type, etc.  */
  GetSystemInfo (&sysinfo_cache);
  syspage_mask = sysinfo_cache.dwPageSize - 1;

  /* Cache os info.  */
  osinfo_cache.dwOSVersionInfoSize = sizeof (OSVERSIONINFO);
  GetVersionEx (&osinfo_cache);

  w32_build_number = osinfo_cache.dwBuildNumber;
  if (os_subtype == OS_WIN95)
    w32_build_number &= 0xffff;
}

/* arch-tag: 9a6a9860-040d-422d-8905-450dd535cd9c
   (do not change this comment) */
