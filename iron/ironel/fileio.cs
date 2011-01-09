namespace IronElisp
{
    public partial class Q
    {
        public static LispObject expand_file_name;
        public static LispObject file_exists_p;

        /* Property name of a file name handler,
           which gives a list of operations it handles..  */
        public static LispObject operations;

        public static LispObject verify_visited_file_modtime;
    }

    public partial class V
    {
        public static LispObject file_name_coding_system
        {
            get { return Defs.O[(int)Objects.file_name_coding_system]; }
            set { Defs.O[(int)Objects.file_name_coding_system] = value; }
        }

        public static LispObject default_file_name_coding_system
        {
            get { return Defs.O[(int)Objects.default_file_name_coding_system]; }
            set { Defs.O[(int)Objects.default_file_name_coding_system] = value; }
        }

        public static LispObject inhibit_file_name_handlers
        {
            get { return Defs.O[(int)Objects.inhibit_file_name_handlers]; }
            set { Defs.O[(int)Objects.inhibit_file_name_handlers] = value; }
        }

        public static LispObject inhibit_file_name_operation
        {
            get { return Defs.O[(int)Objects.inhibit_file_name_operation]; }
            set { Defs.O[(int)Objects.inhibit_file_name_operation] = value; }
        }

        public static LispObject file_name_handler_alist
        {
            get { return Defs.O[(int)Objects.file_name_handler_alist]; }
            set { Defs.O[(int)Objects.file_name_handler_alist] = value; }
        }

    }

    public partial class L
    {
        public static bool IS_DRIVE(char x) { return char.IsLetter(x);}
        public static bool IS_DEVICE_SEP(char c) { return c == ':';}
    }

    public partial class F
    {
        public static LispObject find_file_name_handler(LispObject filename, LispObject operation)
        {
            /* This function must not munge the match data.  */
            LispObject chain, inhibited_handlers, result;
            int pos = -1;

            result = Q.nil;
            L.CHECK_STRING(filename);

            if (L.EQ(operation, V.inhibit_file_name_operation))
                inhibited_handlers = V.inhibit_file_name_handlers;
            else
                inhibited_handlers = Q.nil;

            for (chain = V.file_name_handler_alist; L.CONSP(chain);
                 chain = L.XCDR(chain))
            {
                LispObject elt;
                elt = L.XCAR(chain);
                if (L.CONSP(elt))
                {
                    LispObject stringg = L.XCAR(elt);
                    int match_pos;
                    LispObject handler = L.XCDR(elt);
                    LispObject operations = Q.nil;

                    if (L.SYMBOLP(handler))
                        operations = F.get(handler, Q.operations);

                    if (L.STRINGP(stringg)
                        && (match_pos = L.fast_string_match(stringg, filename)) > pos
                        && (L.NILP(operations) || !L.NILP(F.memq(operation, operations))))
                    {
                        LispObject tem;

                        handler = L.XCDR(elt);
                        tem = F.memq(handler, inhibited_handlers);
                        if (L.NILP(tem))
                        {
                            result = handler;
                            pos = match_pos;
                        }
                    }
                }

                L.QUIT();
            }
            return result;
        }
        public static LispObject file_name_nondirectory(LispObject filename)
        {
            throw new System.Exception();
#if COMEBACK_LATER
  register const unsigned char *beg, *p, *end;
  Lisp_Object handler;

  CHECK_STRING (filename);

  /* If the file name has special constructs in it,
     call the corresponding file handler.  */
  handler = Ffind_file_name_handler (filename, Qfile_name_nondirectory);
  if (!NILP (handler))
    return call2 (handler, Qfile_name_nondirectory, filename);

  beg = SDATA (filename);
  end = p = beg + SBYTES (filename);

  while (p != beg && !IS_DIRECTORY_SEP (p[-1])
ifdef DOS_NT
	 /* only recognise drive specifier at beginning */
	 && !(p[-1] == ':'
	      /* handle the "/:d:foo" case correctly  */
	      && (p == beg + 2 || (p == beg + 4 && IS_DIRECTORY_SEP (*beg))))
endiffff
	 )
    p--;

  return make_specified_string (p, -1, end - p, STRING_MULTIBYTE (filename));
#endif
        }

        public static LispObject expand_file_name(LispObject name, LispObject default_directory)
        {
            return name;
#if COMEBACK_LATER
            /* These point to SDATA and need to be careful with string-relocation
               during GC (via DECODE_FILE).  */
            byte[] nm;
            byte[] newdir;
            /* This should only point to alloca'd data.  */
            byte[] target;

            int tlen;
            //struct passwd *pw;

            int drive = 0;
            int collapse_newdir = 1;
            bool is_escaped = false;

            int length;
            LispObject handler, result;
            bool multibyte;
            LispObject hdir;

            L.CHECK_STRING (name);

            /* If the file name has special constructs in it,
               call the corresponding file handler.  */
            handler = F.find_file_name_handler (name, Q.expand_file_name);
            if (!L.NILP (handler))
                return L.call3 (handler, Q.expand_file_name, name, default_directory);

            /* Use the buffer's default-directory if DEFAULT_DIRECTORY is omitted.  */
            if (L.NILP (default_directory))
                default_directory = L.current_buffer.directory;
            if (! L.STRINGP (default_directory))
            {
                /* "/" is not considered a root directory on DOS_NT, so using "/"
                   here causes an infinite recursion in, e.g., the following:

                   (let (default-directory)
                   (expand-file-name "a"))

                   To avoid this, we set default_directory to the root of the
                   current drive.  */
                default_directory = L.build_string (L.emacs_root_dir ());
            }

            if (!L.NILP (default_directory))
            {
                handler = F.find_file_name_handler (default_directory, Q.expand_file_name);
                if (!L.NILP (handler))
                    return L.call3 (handler, Q.expand_file_name, name, default_directory);
            }

            {
                byte[] o = L.SDATA (default_directory);

                /* Make sure DEFAULT_DIRECTORY is properly expanded.
                   It would be better to do this down below where we actually use
                   default_directory.  Unfortunately, calling Fexpand_file_name recursively
                   could invoke GC, and the strings might be relocated.  This would
                   be annoying because we have pointers into strings lying around
                   that would need adjusting, and people would add new pointers to
                   the code and forget to adjust them, resulting in intermittent bugs.
                   Putting this call here avoids all that crud.

                   The EQ test avoids infinite recursion.  */
                if (! L.NILP (default_directory) && !L.EQ (default_directory, name)
                    /* Save time in some common cases - as long as default_directory
                       is not relative, it can be canonicalized with name below (if it
                       is needed at all) without requiring it to be expanded now.  */

                    /* Detect MSDOS file names with drive specifiers.  */
                    && ! (L.IS_DRIVE ((char) o[0]) && L.IS_DEVICE_SEP ((char) o[1])
                          && L.IS_DIRECTORY_SEP ((char) o[2]))
                    /* Detect Windows file names in UNC format.  */
                    && ! (L.IS_DIRECTORY_SEP ((char)o[0]) && L.IS_DIRECTORY_SEP ((char) o[1]))
                    )
                {
                    default_directory = F.expand_file_name (default_directory, Q.nil);
                }
            }
            multibyte = L.STRING_MULTIBYTE (name);
            if (multibyte != L.STRING_MULTIBYTE (default_directory))
            {
                if (multibyte)
                    default_directory = L.string_to_multibyte (default_directory);
                else
                {
                    name = L.string_to_multibyte (name);
                    multibyte = true;
                }
            }

            nm = L.SDATA (name);

            /* Make a local copy of nm[] to protect it from GC in DECODE_FILE below. */
            byte[] tmp = new byte[nm.Length];
            System.Array.Copy(nm, tmp, tmp.Length);
            nm = tmp;
            int nm_index = 0;

            /* Note if special escape prefix is present, but remove for now.  */
            if (nm[nm_index + 0] == '/' && nm[nm_index + 1] == ':')
            {
                is_escaped = true;
                nm_index += 2;
            }

            /* Find and remove drive specifier if present; this makes nm absolute
               even if the rest of the name appears to be relative.  Only look for
               drive specifier at the beginning.  */
            if (L.IS_DRIVE ((char) nm[nm_index + 0]) && L.IS_DEVICE_SEP ((char) nm[nm_index + 1]))
            {
                drive = nm[nm_index + 0];
                nm_index += 2;
            }

            /* If we see "c://somedir", we want to strip the first slash after the
               colon when stripping the drive letter.  Otherwise, this expands to
               "//somedir".  */
            if (drive != 0 && L.IS_DIRECTORY_SEP ((char) nm[nm_index + 0]) && L.IS_DIRECTORY_SEP ((char) nm[nm_index + 1]))
                nm_index++;

            /* Discard any previous drive specifier if nm is now in UNC format. */
            if (L.IS_DIRECTORY_SEP ((char) nm[nm_index + 0]) && L.IS_DIRECTORY_SEP ((char) nm[nm_index + 1]))
            {
                drive = 0;
            }

            /* If nm is absolute, look for `/./' or `/../' or `//''sequences; if
               none are found, we can probably return right away.  We will avoid
               allocating a new string if name is already fully expanded.  */
            if (
                L.IS_DIRECTORY_SEP ((char) nm[nm_index + 0])
                && (drive != 0 || L.IS_DIRECTORY_SEP ((char)nm[nm_index + 1])) && !is_escaped
                )
            {
                /* If it turns out that the filename we want to return is just a
                   suffix of FILENAME, we don't need to go through and edit
                   things; we just need to construct a new string using data
                   starting at the middle of FILENAME.  If we set lose to a
                   non-zero value, that means we've discovered that we can't do
                   that cool trick.  */
                int lose = 0;
                int p = nm_index;
                int p_end = nm.Length;

                while (p < p_end)
                {
                    /* Since we know the name is absolute, we can assume that each
                       element starts with a "/".  */

                    /* "." and ".." are hairy.  */
                    if (L.IS_DIRECTORY_SEP ((char) nm[p])
                        && nm[p + 1] == '.'
                        && (L.IS_DIRECTORY_SEP ((char) nm[p + 2])
                            || nm[p + 2] == 0
                            || (nm[p + 2] == '.' && (L.IS_DIRECTORY_SEP ((char) nm[p + 3])
                                                || nm[p + 3] == 0))))
                        lose = 1;
                    /* We want to replace multiple `/' in a row with a single
                       slash.  */
                    else if (p > 0
                             && L.IS_DIRECTORY_SEP ((char) nm[p])
                             && L.IS_DIRECTORY_SEP ((char) nm[p + 1]))
                        lose = 1;
                    p++;
                }
                if (lose == 0)
                {
                    /* Make sure directories are all separated with / or \ as
                       desired, but avoid allocation of a new string when not
                       required. */
                    CORRECT_DIR_SEPS (nm);
                    if (L.IS_DIRECTORY_SEP ((char) nm[nm_index + 1]))
                    {
                        if (strcmp (nm, SDATA (name)) != 0)
                            name = L.make_specified_string (nm, -1, strlen (nm), multibyte);
                    }
                    else
                        /* drive must be set, so this is okay */
                        if (strcmp (nm - 2, SDATA (name)) != 0)
                        {
                            char temp[] = " :";

                            name = make_specified_string (nm, -1, p - nm, multibyte);
                            temp[0] = DRIVE_LETTER (drive);
                            name = concat2 (build_string (temp), name);
                        }
                    return name;
                }
            }

            /* At this point, nm might or might not be an absolute file name.  We
               need to expand ~ or ~user if present, otherwise prefix nm with
               default_directory if nm is not absolute, and finally collapse /./
               and /foo/../ sequences.

               We set newdir to be the appropriate prefix if one is needed:
               - the relevant user directory if nm starts with ~ or ~user
               - the specified drive's working dir (DOS/NT only) if nm does not
               start with /
               - the value of default_directory.

               Note that these prefixes are not guaranteed to be absolute (except
               for the working dir of a drive).  Therefore, to ensure we always
               return an absolute name, if the final prefix is not absolute we
               append it to the current working directory.  */

            newdir = null;

            if (nm[0] == '~')		/* prefix ~ */
            {
                if (L.IS_DIRECTORY_SEP ((char) nm[1])
                    || nm[1] == 0)	/* ~ by itself */
                {
                    LispObject tem;

                    if (!(newdir = (unsigned char *) egetenv ("HOME")))
                        newdir = (unsigned char *) "";
                    nm++;
                    /* egetenv may return a unibyte string, which will bite us since
                       we expect the directory to be multibyte.  */
                    tem = L.build_string (newdir);
                    if (!L.STRING_MULTIBYTE (tem))
                    {
                        hdir = L.DECODE_FILE (tem);
                        newdir = L.SDATA (hdir);
                    }
                    collapse_newdir = 0;
                }
                else			/* ~user/filename */
                {
                    unsigned char *o, *p;
                    for (p = nm; *p && (!IS_DIRECTORY_SEP (*p)); p++);
                    o = alloca (p - nm + 1);
                    bcopy ((char *) nm, o, p - nm);
                    o [p - nm] = 0;

                    pw = (struct passwd *) getpwnam (o + 1);
                    if (pw)
                    {
                        newdir = (unsigned char *) pw -> pw_dir;
                        nm = p;
                        collapse_newdir = 0;
                    }

                    /* If we don't find a user of that name, leave the name
                       unchanged; don't move nm forward to p.  */
                }
            }

            /* On DOS and Windows, nm is absolute if a drive name was specified;
               use the drive's current directory as the prefix if needed.  */
            if (!newdir && drive)
            {
                /* Get default directory if needed to make nm absolute. */
                if (!IS_DIRECTORY_SEP (nm[0]))
                {
                    newdir = alloca (MAXPATHLEN + 1);
                    if (!getdefdir (toupper (drive) - 'A' + 1, newdir))
                        newdir = NULL;
                }
                if (!newdir)
                {
                    /* Either nm starts with /, or drive isn't mounted. */
                    newdir = alloca (4);
                    newdir[0] = DRIVE_LETTER (drive);
                    newdir[1] = ':';
                    newdir[2] = '/';
                    newdir[3] = 0;
                }
            }

            /* Finally, if no prefix has been specified and nm is not absolute,
               then it must be expanded relative to default_directory. */

            if (1
                && !(IS_DIRECTORY_SEP (nm[0]) && IS_DIRECTORY_SEP (nm[1]))
                && !newdir)
            {
                newdir = SDATA (default_directory);
                /* Note if special escape prefix is present, but remove for now.  */
                if (newdir[0] == '/' && newdir[1] == ':')
                {
                    is_escaped = 1;
                    newdir += 2;
                }
            }

            if (newdir)
            {
                /* First ensure newdir is an absolute name. */
                if (
                    /* Detect MSDOS file names with drive specifiers.  */
                    ! (IS_DRIVE (newdir[0])
                       && IS_DEVICE_SEP (newdir[1]) && IS_DIRECTORY_SEP (newdir[2]))
                    /* Detect Windows file names in UNC format.  */
                    && ! (IS_DIRECTORY_SEP (newdir[0]) && IS_DIRECTORY_SEP (newdir[1]))
                    )
                {
                    /* Effectively, let newdir be (expand-file-name newdir cwd).
                       Because of the admonition against calling expand-file-name
                       when we have pointers into lisp strings, we accomplish this
                       indirectly by prepending newdir to nm if necessary, and using
                       cwd (or the wd of newdir's drive) as the new newdir. */

                    if (IS_DRIVE (newdir[0]) && IS_DEVICE_SEP (newdir[1]))
                    {
                        drive = newdir[0];
                        newdir += 2;
                    }
                    if (!IS_DIRECTORY_SEP (nm[0]))
                    {
                        char * tmp = alloca (strlen (newdir) + strlen (nm) + 3);
                        file_name_as_directory (tmp, newdir);
                        strcat (tmp, nm);
                        nm = tmp;
                    }
                    newdir = alloca (MAXPATHLEN + 1);
                    if (drive)
                    {
                        if (!getdefdir (toupper (drive) - 'A' + 1, newdir))
                            newdir = "/";
                    }
                    else
                        getwd (newdir);
                }

                /* Strip off drive name from prefix, if present. */
                if (IS_DRIVE (newdir[0]) && IS_DEVICE_SEP (newdir[1]))
                {
                    drive = newdir[0];
                    newdir += 2;
                }

                /* Keep only a prefix from newdir if nm starts with slash
                   (//server/share for UNC, nothing otherwise).  */
                if (IS_DIRECTORY_SEP (nm[0]) && collapse_newdir)
                {
                    if (IS_DIRECTORY_SEP (newdir[0]) && IS_DIRECTORY_SEP (newdir[1]))
                    {
                        unsigned char *p;
                        newdir = strcpy (alloca (strlen (newdir) + 1), newdir);
                        p = newdir + 2;
                        while (*p && !IS_DIRECTORY_SEP (*p)) p++;
                        p++;
                        while (*p && !IS_DIRECTORY_SEP (*p)) p++;
                        *p = 0;
                    }
                    else
                        newdir = "";
                }
            }

            if (newdir)
            {
                /* Get rid of any slash at the end of newdir, unless newdir is
                   just / or // (an incomplete UNC name).  */
                length = strlen (newdir);
                if (length > 1 && IS_DIRECTORY_SEP (newdir[length - 1])
                    && !(length == 2 && IS_DIRECTORY_SEP (newdir[0]))
                    )
                {
                    unsigned char *temp = (unsigned char *) alloca (length);
                    bcopy (newdir, temp, length - 1);
                    temp[length - 1] = 0;
                    newdir = temp;
                }
                tlen = length + 1;
            }
            else
                tlen = 0;

            /* Now concatenate the directory and name to new space in the stack frame */
            tlen += strlen (nm) + 1;
            /* Reserve space for drive specifier and escape prefix, since either
               or both may need to be inserted.  (The Microsoft x86 compiler
               produces incorrect code if the following two lines are combined.)  */
            target = (unsigned char *) alloca (tlen + 4);
            target += 4;
            *target = 0;

            if (newdir)
            {
                if (nm[0] == 0 || IS_DIRECTORY_SEP (nm[0]))
                {
                    /* If newdir is effectively "C:/", then the drive letter will have
                       been stripped and newdir will be "/".  Concatenating with an
                       absolute directory in nm produces "//", which will then be
                       incorrectly treated as a network share.  Ignore newdir in
                       this case (keeping the drive letter).  */
                    if (!(drive && nm[0] && IS_DIRECTORY_SEP (newdir[0])
                          && newdir[1] == '\0'))
                        strcpy (target, newdir);
                }
                else
                    file_name_as_directory (target, newdir);
            }

            strcat (target, nm);

            /* Now canonicalize by removing `//', `/.' and `/foo/..' if they
               appear.  */
            {
                unsigned char *p = target;
                unsigned char *o = target;

                while (*p)
                {
                    if (!IS_DIRECTORY_SEP (*p))
                    {
                        *o++ = *p++;
                    }
                    else if (p[1] == '.'
                             && (IS_DIRECTORY_SEP (p[2])
                                 || p[2] == 0))
                    {
                        /* If "/." is the entire filename, keep the "/".  Otherwise,
                           just delete the whole "/.".  */
                        if (o == target && p[2] == '\0')
                            *o++ = *p;
                        p += 2;
                    }
                    else if (p[1] == '.' && p[2] == '.'
                             /* `/../' is the "superroot" on certain file systems.
                                Turned off on DOS_NT systems because they have no
                                "superroot" and because this causes us to produce
                                file names like "d:/../foo" which fail file-related
                                functions of the underlying OS.  (To reproduce, try a
                                long series of "../../" in default_directory, longer
                                than the number of levels from the root.)  */
                             && o != target
                             && (IS_DIRECTORY_SEP (p[3]) || p[3] == 0))
                    {
                        unsigned char *prev_o = o;
                        while (o != target && (--o) && !IS_DIRECTORY_SEP (*o))
                            ;
                        /* Don't go below server level in UNC filenames.  */
                        if (o == target + 1 && IS_DIRECTORY_SEP (*o)
                            && IS_DIRECTORY_SEP (*target))
                            o = prev_o;
                        else
                            /* Keep initial / only if this is the whole name.  */
                            if (o == target && IS_ANY_SEP (*o) && p[3] == 0)
                                ++o;
                        p += 3;
                    }
                    else if (p > target && IS_DIRECTORY_SEP (p[1]))
                        /* Collapse multiple `/' in a row.  */
                        p++;
                    else
                    {
                        *o++ = *p++;
                    }
                }

                /* At last, set drive name. */
                /* Except for network file name.  */
                if (!(IS_DIRECTORY_SEP (target[0]) && IS_DIRECTORY_SEP (target[1])))
                {
                    if (!drive) abort ();
                    target -= 2;
                    target[0] = DRIVE_LETTER (drive);
                    target[1] = ':';
                }
                /* Reinsert the escape prefix if required.  */
                if (is_escaped)
                {
                    target -= 2;
                    target[0] = '/';
                    target[1] = ':';
                }
                CORRECT_DIR_SEPS (target);

                result = make_specified_string (target, -1, o - target, multibyte);
            }

            /* Again look to see if the file name has special constructs in it
               and perhaps call the corresponding file handler.  This is needed
               for filenames such as "/foo/../user@host:/bar/../baz".  Expanding
               the ".." component gives us "/user@host:/bar/../baz" which needs
               to be expanded again. */
            handler = Ffind_file_name_handler (result, Qexpand_file_name);
            if (!NILP (handler))
                return call3 (handler, Qexpand_file_name, result, default_directory);

            return result;
#endif
        }

        public static LispObject file_exists_p(LispObject filename)
        {
            LispObject absname;
            LispObject handler;
            //  struct stat statbuf;

            L.CHECK_STRING(filename);
            absname = F.expand_file_name(filename, Q.nil);

            /* If the file name has special constructs in it,
               call the corresponding file handler.  */
            handler = F.find_file_name_handler(absname, Q.file_exists_p);
            if (!L.NILP(handler))
                return L.call2(handler, Q.file_exists_p, absname);

            absname = L.ENCODE_FILE(absname);

            string fname = System.Text.Encoding.UTF8.GetString(L.SDATA(absname));
            return new System.IO.FileInfo(fname).Exists ? Q.t : Q.nil;
        }

        public static LispObject verify_visited_file_modtime(LispObject buf)
        {
            //  struct buffer *b;
            //  struct stat st;
            LispObject handler;
            LispObject filename;

            L.CHECK_BUFFER(buf);
            Buffer b = L.XBUFFER(buf);

            if (!L.STRINGP(b.filename)) return Q.t;
            if (b.modtime == 0) return Q.t;

            /* If the file name has special constructs in it,
               call the corresponding file handler.  */
            handler = F.find_file_name_handler(b.filename, Q.verify_visited_file_modtime);
            if (!L.NILP(handler))
                return L.call2(handler, Q.verify_visited_file_modtime, buf);

            filename = L.ENCODE_FILE(b.filename);

            string fname = System.Text.Encoding.UTF8.GetString(L.SDATA(filename));
            System.IO.FileInfo info = new System.IO.FileInfo(fname);
            
            long now;
            if (!info.Exists)
                now = -1;
            else
                now = info.LastWriteTime.ToBinary();

            if (now == b.modtime
                /* If both are positive, accept them if they are off by one second.  */
                || (now > 0 && b.modtime > 0
                && (System.DateTime.FromBinary(now).Subtract(System.DateTime.FromBinary(b.modtime)).Seconds <= 1)
                    ))
                return Q.t;
            return Q.nil;
        }
    }
}