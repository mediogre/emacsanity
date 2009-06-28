using System.Text.RegularExpressions;
namespace IronElisp
{
    public partial class L
    {
        public static LispObject unwind_set_match_data(LispObject list)
        {
            /* It is NOT ALWAYS safe to free (evaporate) the markers immediately.  */
            return F.set_match_data(list, Q.t);
        }

        /* Called to unwind protect the match data.  */
        public static void record_unwind_save_match_data()
        {
            record_unwind_protect(unwind_set_match_data, F.match_data(Q.nil, Q.nil, Q.nil));
        }

        public static int fast_string_match(LispObject regexp, LispObject str)
        {
            // COMEBACK_WHEN_READY
            // TODO: when regex.cs is ready (if ever), use the original code 
            // but so far we'll just try to do simple .net regexp matching

            Match m = Regex.Match(SDATA(str), SDATA(regexp));
            if (m.Success)
            {
                return m.Index;
            }
            else
            {
                return -1;
            }
        }
    }

    public partial class F
    {
        public static LispObject match_data(LispObject integers, LispObject reuse, LispObject reseat)
        {
            // COMEBACK_WHEN_READY
            /*
            Lisp_Object tail, prev;
            Lisp_Object* data;
            int i, len;

            if (!NILP(reseat))
                for (tail = reuse; CONSP(tail); tail = XCDR(tail))
                    if (MARKERP(XCAR(tail)))
                    {
                        unchain_marker(XMARKER(XCAR(tail)));
                        XSETCAR(tail, Qnil);
                    }

            if (NILP(last_thing_searched))
                return Qnil;

            prev = Qnil;

            data = (Lisp_Object*)alloca((2 * search_regs.num_regs + 1)
                           * sizeof(Lisp_Object));

            len = 0;
            for (i = 0; i < search_regs.num_regs; i++)
            {
                int start = search_regs.start[i];
                if (start >= 0)
                {
                    if (EQ(last_thing_searched, Qt)
                        || !NILP(integers))
                    {
                        XSETFASTINT(data[2 * i], start);
                        XSETFASTINT(data[2 * i + 1], search_regs.end[i]);
                    }
                    else if (BUFFERP(last_thing_searched))
                    {
                        data[2 * i] = Fmake_marker();
                        Fset_marker(data[2 * i],
                             make_number(start),
                             last_thing_searched);
                        data[2 * i + 1] = Fmake_marker();
                        Fset_marker(data[2 * i + 1],
                             make_number(search_regs.end[i]),
                             last_thing_searched);
                    }
                    else
                        // last_thing_searched must always be Qt, a buffer, or Qnil.
                        abort();

                    len = 2 * i + 2;
                }
                else
                    data[2 * i] = data[2 * i + 1] = Qnil;
            }

            if (BUFFERP(last_thing_searched) && !NILP(integers))
            {
                data[len] = last_thing_searched;
                len++;
            }

            // If REUSE is not usable, cons up the values and return them.
            if (!CONSP(reuse))
                return Flist(len, data);

            // If REUSE is a list, store as many value elements as will fit
            // into the elements of REUSE.  
            for (i = 0, tail = reuse; CONSP(tail);
                 i++, tail = XCDR(tail))
            {
                if (i < len)
                    XSETCAR(tail, data[i]);
                else
                    XSETCAR(tail, Qnil);
                prev = tail;
            }

            // If we couldn't fit all value elements into REUSE,
            // cons up the rest of them and add them to the end of REUSE.  
            if (i < len)
                XSETCDR(prev, Flist(len - i, data + i));

            return reuse;
*/
            return Q.nil;
        }

        /* We used to have an internal use variant of `reseat' described as:

              If RESEAT is `evaporate', put the markers back on the free list
              immediately.  No other references to the markers must exist in this
              case, so it is used only internally on the unwind stack and
              save-match-data from Lisp.

           But it was ill-conceived: those supposedly-internal markers get exposed via
           the undo-list, so freeing them here is unsafe.  */
        public static LispObject set_match_data(LispObject list, LispObject reseat)
        {
            // COMEBACK_WHEN_READY!!!
            /*
  register int i;
  register Lisp_Object marker;

  if (running_asynch_code)
    save_search_regs ();

  CHECK_LIST (list);

  // Unless we find a marker with a buffer or an explicit buffer
  //   in LIST, assume that this match data came from a string.  
  last_thing_searched = Qt;

  // Allocate registers if they don't already exist. 
  {
    int length = XFASTINT (Flength (list)) / 2;

    if (length > search_regs.num_regs)
      {
	if (search_regs.num_regs == 0)
	  {
	    search_regs.start
	      = (regoff_t *) xmalloc (length * sizeof (regoff_t));
	    search_regs.end
	      = (regoff_t *) xmalloc (length * sizeof (regoff_t));
	  }
	else
	  {
	    search_regs.start
	      = (regoff_t *) xrealloc (search_regs.start,
				       length * sizeof (regoff_t));
	    search_regs.end
	      = (regoff_t *) xrealloc (search_regs.end,
				       length * sizeof (regoff_t));
	  }

	for (i = search_regs.num_regs; i < length; i++)
	  search_regs.start[i] = -1;

	search_regs.num_regs = length;
      }

    for (i = 0; CONSP (list); i++)
      {
	marker = XCAR (list);
	if (BUFFERP (marker))
	  {
	    last_thing_searched = marker;
	    break;
	  }
	if (i >= length)
	  break;
	if (NILP (marker))
	  {
	    search_regs.start[i] = -1;
	    list = XCDR (list);
	  }
	else
	  {
	    int from;
	    Lisp_Object m;

	    m = marker;
	    if (MARKERP (marker))
	      {
		if (XMARKER (marker)->buffer == 0)
		  XSETFASTINT (marker, 0);
		else
		  XSETBUFFER (last_thing_searched, XMARKER (marker)->buffer);
	      }

	    CHECK_NUMBER_COERCE_MARKER (marker);
	    from = XINT (marker);

	    if (!NILP (reseat) && MARKERP (m))
	      {
		unchain_marker (XMARKER (m));
		XSETCAR (list, Qnil);
	      }

	    if ((list = XCDR (list), !CONSP (list)))
	      break;

	    m = marker = XCAR (list);

	    if (MARKERP (marker) && XMARKER (marker)->buffer == 0)
	      XSETFASTINT (marker, 0);

	    CHECK_NUMBER_COERCE_MARKER (marker);
	    search_regs.start[i] = from;
	    search_regs.end[i] = XINT (marker);

	    if (!NILP (reseat) && MARKERP (m))
	      {
		unchain_marker (XMARKER (m));
		XSETCAR (list, Qnil);
	      }
	  }
	list = XCDR (list);
      }

    for (; i < search_regs.num_regs; i++)
      search_regs.start[i] = -1;
  }

  return Qnil;
*/
            return Q.nil;
        }
    }
}