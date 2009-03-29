/* unexec for GNU Emacs on Windows NT.
   Copyright (C) 1994, 2001, 2002, 2003, 2004, 2005,
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

/*
   Geoff Voelker (voelker@cs.washington.edu)                         8-12-94
*/

#include <config.h>

#include <stdio.h>
#include <fcntl.h>
#include <time.h>
#include <windows.h>

/* Include relevant definitions from IMAGEHLP.H, which can be found
   in \\win32sdk\mstools\samples\image\include\imagehlp.h. */

PIMAGE_NT_HEADERS
(__stdcall * pfnCheckSumMappedFile) (LPVOID BaseAddress,
				    DWORD FileLength,
				    LPDWORD HeaderSum,
				    LPDWORD CheckSum);

extern BOOL ctrl_c_handler (unsigned long type);

extern char my_begdata[];
extern char my_edata[];
extern char my_begbss[];
extern char my_endbss[];
extern char *my_begbss_static;
extern char *my_endbss_static;

#include "w32heap.h"

#undef min
#undef max
#define min(x, y) (((x) < (y)) ? (x) : (y))
#define max(x, y) (((x) > (y)) ? (x) : (y))

/* Basically, our "initialized" flag.  */
BOOL using_dynamic_heap = FALSE;

int open_input_file (file_data *p_file, char *name);
int open_output_file (file_data *p_file, char *name, unsigned long size);
void close_file_data (file_data *p_file);

void get_section_info (file_data *p_file);
void copy_executable_and_dump_data (file_data *, file_data *);
void dump_bss_and_heap (file_data *p_infile, file_data *p_outfile);

/* Cached info about the .data section in the executable.  */
PIMAGE_SECTION_HEADER data_section;
PCHAR  data_start = 0;
DWORD  data_size = 0;

/* Cached info about the .bss section in the executable.  */
PIMAGE_SECTION_HEADER bss_section;
PCHAR  bss_start = 0;
DWORD  bss_size = 0;
DWORD  extra_bss_size = 0;
/* bss data that is static might be discontiguous from non-static.  */
PIMAGE_SECTION_HEADER bss_section_static;
PCHAR  bss_start_static = 0;
DWORD  bss_size_static = 0;
DWORD  extra_bss_size_static = 0;

PIMAGE_SECTION_HEADER heap_section;

#ifdef HAVE_NTGUI
HINSTANCE hinst = NULL;
HINSTANCE hprevinst = NULL;
LPSTR lpCmdLine = "";
int nCmdShow = 0;
#endif /* HAVE_NTGUI */

int
open_input_file (file_data *p_file, char *filename)
{
  HANDLE file;
  HANDLE file_mapping;
  void  *file_base;
  unsigned long size, upper_size;

  file = CreateFile (filename, GENERIC_READ, FILE_SHARE_READ, NULL,
		     OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
  if (file == INVALID_HANDLE_VALUE)
    return FALSE;

  size = GetFileSize (file, &upper_size);
  file_mapping = CreateFileMapping (file, NULL, PAGE_READONLY,
				    0, size, NULL);
  if (!file_mapping)
    return FALSE;

  file_base = MapViewOfFile (file_mapping, FILE_MAP_READ, 0, 0, size);
  if (file_base == 0)
    return FALSE;

  p_file->name = filename;
  p_file->size = size;
  p_file->file = file;
  p_file->file_mapping = file_mapping;
  p_file->file_base = file_base;

  return TRUE;
}

int
open_output_file (file_data *p_file, char *filename, unsigned long size)
{
  HANDLE file;
  HANDLE file_mapping;
  void  *file_base;

  file = CreateFile (filename, GENERIC_READ | GENERIC_WRITE, 0, NULL,
		     CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, 0);
  if (file == INVALID_HANDLE_VALUE)
    return FALSE;

  file_mapping = CreateFileMapping (file, NULL, PAGE_READWRITE,
				    0, size, NULL);
  if (!file_mapping)
    return FALSE;

  file_base = MapViewOfFile (file_mapping, FILE_MAP_WRITE, 0, 0, size);
  if (file_base == 0)
    return FALSE;

  p_file->name = filename;
  p_file->size = size;
  p_file->file = file;
  p_file->file_mapping = file_mapping;
  p_file->file_base = file_base;

  return TRUE;
}

/* Close the system structures associated with the given file.  */
void
close_file_data (file_data *p_file)
{
  UnmapViewOfFile (p_file->file_base);
  CloseHandle (p_file->file_mapping);
  /* For the case of output files, set final size.  */
  SetFilePointer (p_file->file, p_file->size, NULL, FILE_BEGIN);
  SetEndOfFile (p_file->file);
  CloseHandle (p_file->file);
}



/* Return pointer to section header for section containing the given
   relative virtual address. */
IMAGE_SECTION_HEADER *
rva_to_section (DWORD rva, IMAGE_NT_HEADERS * nt_header)
{
  PIMAGE_SECTION_HEADER section;
  int i;

  section = IMAGE_FIRST_SECTION (nt_header);

  for (i = 0; i < nt_header->FileHeader.NumberOfSections; i++)
    {
      /* Some linkers (eg. the NT SDK linker I believe) swapped the
	 meaning of these two values - or rather, they ignored
	 VirtualSize entirely and always set it to zero.  This affects
	 some very old exes (eg. gzip dated Dec 1993).  Since
	 w32_executable_type relies on this function to work reliably,
	 we need to cope with this.  */
      DWORD real_size = max (section->SizeOfRawData,
			     section->Misc.VirtualSize);
      if (rva >= section->VirtualAddress
	  && rva < section->VirtualAddress + real_size)
	return section;
      section++;
    }
  return NULL;
}




/* arch-tag: fe1d3d1c-ef88-4917-ab22-f12ab16b3254
   (do not change this comment) */
