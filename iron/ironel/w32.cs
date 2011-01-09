namespace IronElisp
{
    public partial class L
    {
        public static bool IS_DIRECTORY_SEP(char c) { return (c == '/' || c == '\\'); }

        /* Parse the root part of file name, if present.  Return length and
            optionally store pointer to char after root.  */
        static int parse_root(string str, int name, ref int pPath)
        {
            int start = name;

            if (str == null)
                return 0;

            int end = str.Length;
            /* find the root name of the volume if given */
            if (char.IsLetter(str, name) && str[name + 1] == ':')
            {
                /* skip past drive specifier */
                name += 2;
                if (IS_DIRECTORY_SEP(str[name]))
                    name++;
            }
            else if (IS_DIRECTORY_SEP(str[name]) && IS_DIRECTORY_SEP(str[name + 1]))
            {
                int slashes = 2;
                name += 2;
                do
                {
                    if (IS_DIRECTORY_SEP(str[name]) && --slashes == 0)
                        break;
                    name++;
                }
                while (name < end);
                if (IS_DIRECTORY_SEP(str[name]))
                    name++;
            }

            pPath = name;

            return name - start;
        }

        public static string emacs_root_dir()
        {
            //  static char root_dir[FILENAME_MAX];

            string p = System.Environment.GetEnvironmentVariable("emacs_dir");
            if (p == null)
                abort();

            int path = 0;
            int end = parse_root(p, 0, ref path);
            string root_dir = p.Substring(0, end);
            dostounix_filename(ref root_dir);
            return root_dir;
        }

        /* Destructively turn backslashes into slashes.  */
        public static void dostounix_filename (ref string p)
        {
            normalize_filename(ref p, '/');
        }

        /* Normalize filename by converting all path separators to
           the specified separator.  Also conditionally convert upper
           case path name components to lower case.  */
        public static void normalize_filename (ref string out_str, char path_sep)
        {
            int fp = 0;
            char sep;
            int elem;
            int end = out_str.Length;

            System.Text.StringBuilder str = new System.Text.StringBuilder(out_str);

            /* Always lower-case drive letters a-z, even if the filesystem
               preserves case in filenames.
               This is so filenames can be compared by string comparison
               functions that are case-sensitive.  Even case-preserving filesystems
               do not distinguish case in drive letters.  */
            if (str[fp + 1] == ':' && str[fp] >= 'A' && str[fp] <= 'Z')
            {
                str[fp] = char.ToLower(str[fp]);
                fp += 2;
            }

            if (NILP(V.w32_downcase_file_names))
            {
                while (fp < end)
                {
                    if (str[fp] == '/' || str[fp] == '\\')
                        str[fp] = path_sep;
                    fp++;
                }

                out_str = str.ToString();
                return;
            }

            sep = path_sep;		/* convert to this path separator */
            elem = fp;			/* start of current path element */

            do
            {
                if (str[fp] >= 'a' && str[fp] <= 'z')
                    elem = 0;			/* don't convert this element */

                if (str[fp] == 0 || str[fp] == ':')
                {
                    sep = str[fp];		/* restore current separator (or 0) */
                    str[fp] = '/';		/* after conversion of this element */
                }

                if (str[fp] == '/' || str[fp] == '\\')
                {
                    if (elem != 0 && elem != fp)
                    {
                        while (elem < fp)
                        {
                            str[elem] = char.ToLower(str[elem]);
                            elem++;
                        }
                    }
                    str[fp] = sep;		/* convert (or restore) path separator */
                    elem = fp + 1;		/* next element starts after separator */
                    sep = path_sep;
                }

                fp++;
            } while (fp < end);
            out_str = str.ToString();
        }
    }
}