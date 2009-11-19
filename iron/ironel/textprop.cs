namespace IronElisp
{
    public partial class F
    {
        public static LispObject set_text_properties(LispObject start, LispObject end, LispObject properties, LispObject obj)
        {
            return L.set_text_properties(start, end, properties, obj, Q.t);
        }

        /* Callers note, this can GC when OBJECT is a buffer (or nil).  */
        public static LispObject add_text_properties(LispObject start, LispObject end, LispObject properties, LispObject obj)
        {
            throw new System.Exception();
#if COMEBACK_LATER
            Interval i, unchanged;
            int s, len, modified = 0;

            properties = validate_plist(properties);
            if (L.NILP(properties))
                return Q.nil;

            if (L.NILP(obj))
                L.XSETBUFFER(obj, current_buffer);

            i = validate_interval_range(obj, &start, &end, hard);
            if (L.NULL_INTERVAL_P(i))
                return Q.nil;

            s = L.XINT(start);
            len = L.XINT(end) - s;

            /* If we're not starting on an interval boundary, we have to
              split this interval.  */
            if (i.position != s)
            {
                /* If this interval already has the properties, we can
                   skip it.  */
                if (interval_has_all_properties(properties, i))
                {
                    int got = (L.LENGTH(i) - (s - i.position));
                    if (got >= len)
                        return (Q.nil);
                    len -= got;
                    i = next_interval(i);
                }
                else
                {
                    unchanged = i;
                    i = split_interval_right(unchanged, s - unchanged.position);
                    copy_properties(unchanged, i);
                }
            }

            if (BUFFERP(obj))
                modify_region(XBUFFER(obj), L.XINT(start), L.XINT(end), 1);

            /* We are at the beginning of interval I, with LEN chars to scan.  */
            for (; ; )
            {
                if (i == 0)
                    abort();

                if (L.LENGTH(i) >= len)
                {
                    if (interval_has_all_properties(properties, i))
                    {
                        if (BUFFERP(obj))
                            signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                         L.XINT(end) - L.XINT(start));

                        return modified ? Q.t : Q.nil;
                    }

                    if (L.LENGTH(i) == len)
                    {
                        add_properties(properties, i, obj);
                        if (BUFFERP(obj))
                            signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                         L.XINT(end) - L.XINT(start));
                        return Q.t;
                    }

                    /* i doesn't have the properties, and goes past the change limit */
                    unchanged = i;
                    i = split_interval_left(unchanged, len);
                    copy_properties(unchanged, i);
                    add_properties(properties, i, obj);
                    if (BUFFERP(obj))
                        signal_after_change(L.XINT(start), L.XINT(end) - L.XINT(start),
                                 L.XINT(end) - L.XINT(start));
                    return Q.t;
                }

                len -= L.LENGTH(i);
                modified += add_properties(properties, i, obj);
                i = next_interval(i);
            }
#endif
        }
    }

    public partial class L
    {
        /* Replace properties of text from START to END with new list of
           properties PROPERTIES.  OBJECT is the buffer or string containing
           the text.  OBJECT nil means use the current buffer.
           SIGNAL_AFTER_CHANGE_P nil means don't signal after changes.  Value
           is nil if the function _detected_ that it did not replace any
           properties, non-nil otherwise.  */
        public static LispObject set_text_properties(LispObject start, LispObject end, LispObject properties,
                                                     LispObject obj, LispObject signal_after_change_p)
        {
            throw new System.Exception("Come back");
#if COMEBACK_LATER
  register INTERVAL i;
  Lisp_Object ostart, oend;

  ostart = start;
  oend = end;

  properties = validate_plist (properties);

  if (NILP (obj))
    XSETBUFFER (obj, current_buffer);

  /* If we want no properties for a whole string,
     get rid of its intervals.  */
  if (NILP (properties) && STRINGP (obj)
      && XFASTINT (start) == 0
      && XFASTINT (end) == SCHARS (obj))
    {
      if (! STRING_INTERVALS (obj))
	return Qnil;

      STRING_SET_INTERVALS (obj, NULL_INTERVAL);
      return Qt;
    }

  i = validate_interval_range (obj, &start, &end, soft);

  if (NULL_INTERVAL_P (i))
    {
      /* If buffer has no properties, and we want none, return now.  */
      if (NILP (properties))
	return Qnil;

      /* Restore the original START and END values
	 because validate_interval_range increments them for strings.  */
      start = ostart;
      end = oend;

      i = validate_interval_range (obj, &start, &end, hard);
      /* This can return if start == end.  */
      if (NULL_INTERVAL_P (i))
	return Qnil;
    }

  if (BUFFERP (obj))
    modify_region (XBUFFER (obj), XINT (start), XINT (end), 1);

  set_text_properties_1 (start, end, properties, obj, i);

  if (BUFFERP (obj) && !NILP (signal_after_change_p))
    signal_after_change (XINT (start), XINT (end) - XINT (start),
			 XINT (end) - XINT (start));
  return Qt;
#endif
        }

        /* Return a list representing the text properties of OBJECT between
           START and END.  if PROP is non-nil, report only on that property.
           Each result list element has the form (S E PLIST), where S and E
           are positions in OBJECT and PLIST is a property list containing the
           text properties of OBJECT between S and E.  Value is nil if OBJECT
           doesn't contain text properties between START and END.  */

        public static LispObject text_property_list(LispObject obj, LispObject start, LispObject end, LispObject prop)
        {
            throw new System.Exception();
#if COMEBACK_LATER
  struct interval *i;
  Lisp_Object result;

  result = Qnil;

  i = validate_interval_range (object, &start, &end, soft);
  if (!NULL_INTERVAL_P (i))
    {
      int s = XINT (start);
      int e = XINT (end);

      while (s < e)
	{
	  int interval_end, len;
	  Lisp_Object plist;

	  interval_end = i->position + LENGTH (i);
	  if (interval_end > e)
	    interval_end = e;
	  len = interval_end - s;

	  plist = i->plist;

	  if (!NILP (prop))
	    for (; CONSP (plist); plist = Fcdr (XCDR (plist)))
	      if (EQ (XCAR (plist), prop))
		{
		  plist = Fcons (prop, Fcons (Fcar (XCDR (plist)), Qnil));
		  break;
		}

	  if (!NILP (plist))
	    result = Fcons (Fcons (make_number (s),
				   Fcons (make_number (s + len),
					  Fcons (plist, Qnil))),
			    result);

	  i = next_interval (i);
	  if (NULL_INTERVAL_P (i))
	    break;
	  s = i->position;
	}
    }

  return result;
#endif
        }

        /* Add text properties to OBJECT from LIST.  LIST is a list of triples
           (START END PLIST), where START and END are positions and PLIST is a
           property list containing the text properties to add.  Adjust START
           and END positions by DELTA before adding properties.  Value is
           non-zero if OBJECT was modified.  */
        public static bool add_text_properties_from_list(LispObject obj, LispObject list, LispObject delta)
        {
            bool modified_p = false;

            for (; CONSP(list); list = XCDR(list))
            {
                LispObject item, start, end, plist, tem;

                item = XCAR(list);
                start = make_number(XINT(XCAR(item)) + XINT(delta));
                end = make_number(XINT(XCAR(XCDR(item))) + XINT(delta));
                plist = XCAR(XCDR(XCDR(item)));

                tem = F.add_text_properties(start, end, plist, obj);
                if (!NILP(tem))
                    modified_p = true;
            }

            return modified_p;
        }
    }
}