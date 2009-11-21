namespace IronElisp
{
    public partial class Q
    {
        public static LispObject nil;
        public static LispObject unbound;
        public static LispObject t;
        public static LispObject variable_documentation;

        public static LispObject quote, lambda, subr, error_conditions, error_message, top_level;
        public static LispObject error, quit, wrong_type_argument, args_out_of_range, void_function;
        public static LispObject cyclic_function_indirection, cyclic_variable_indirection;
        public static LispObject void_variable, setting_constant, invalid_read_syntax;

        public static LispObject invalid_function, wrong_number_of_arguments, no_catch;
        public static LispObject end_of_file, arith_error, beginning_of_buffer, end_of_buffer;
        public static LispObject buffer_read_only, text_read_only, mark_inactive;

        public static LispObject listp, consp, symbolp, keywordp, integerp, natnump, wholenump;
        public static LispObject stringp, arrayp, sequencep, bufferp, vectorp, char_or_string_p;
        public static LispObject markerp, buffer_or_string_p, integer_or_marker_p, boundp, fboundp;
        public static LispObject floatp, numberp, number_or_marker_p, char_table_p, vector_or_char_table_p;
        public static LispObject subrp, unevalled, many, cdr, ad_advice_info, ad_activate_internal;

        public static LispObject interactive_form;
    }

    public partial class L
    {
        public static void init_data()
        {
        }

        public static void syms_of_data()
        {
            Q.quote = intern("quote");
            Q.lambda = intern("lambda");
            Q.subr = intern("subr");
            Q.error_conditions = intern("error-conditions");
            Q.error_message = intern("error-message");
            Q.top_level = intern("top-level");

            Q.error = intern("error");
            Q.quit = intern("quit");
            Q.wrong_type_argument = intern("wrong-type-argument");
            Q.args_out_of_range = intern("args-out-of-range");
            Q.void_function = intern("void-function");
            Q.cyclic_function_indirection = intern("cyclic-function-indirection");
            Q.cyclic_variable_indirection = intern("cyclic-variable-indirection");
            Q.void_variable = intern("void-variable");
            Q.setting_constant = intern("setting-constant");
            Q.invalid_read_syntax = intern("invalid-read-syntax");

            Q.invalid_function = intern("invalid-function");
            Q.wrong_number_of_arguments = intern("wrong-number-of-arguments");
            Q.no_catch = intern("no-catch");
            Q.end_of_file = intern("end-of-file");
            Q.arith_error = intern("arith-error");
            Q.beginning_of_buffer = intern("beginning-of-buffer");
            Q.end_of_buffer = intern("end-of-buffer");
            Q.buffer_read_only = intern("buffer-read-only");
            Q.text_read_only = intern("text-read-only");
            Q.mark_inactive = intern("mark-inactive");

            Q.listp = intern("listp");
            Q.consp = intern("consp");
            Q.symbolp = intern("symbolp");
            Q.keywordp = intern("keywordp");
            Q.integerp = intern("integerp");
            Q.natnump = intern("natnump");
            Q.wholenump = intern("wholenump");
            Q.stringp = intern("stringp");
            Q.arrayp = intern("arrayp");
            Q.sequencep = intern("sequencep");
            Q.bufferp = intern("bufferp");
            Q.vectorp = intern("vectorp");
            Q.char_or_string_p = intern("char-or-string-p");
            Q.markerp = intern("markerp");
            Q.buffer_or_string_p = intern("buffer-or-string-p");
            Q.integer_or_marker_p = intern("integer-or-marker-p");
            Q.boundp = intern("boundp");
            Q.fboundp = intern("fboundp");

            Q.floatp = intern("floatp");
            Q.numberp = intern("numberp");
            Q.number_or_marker_p = intern("number-or-marker-p");

            Q.char_table_p = intern("char-table-p");
            Q.vector_or_char_table_p = intern("vector-or-char-table-p");

            Q.subrp = intern("subrp");
            Q.unevalled = intern("unevalled");
            Q.many = intern("many");

            Q.cdr = intern("cdr");

            /* Handle automatic advice activation */
            Q.ad_advice_info = intern("ad-advice-info");
            Q.ad_activate_internal = intern("ad-activate-internal");
        }

        /* Return the symbol holding SYMBOL's value.  Signal
           `cyclic-variable-indirection' if SYMBOL's chain of variable
           indirections contains a loop.  */
        public static LispSymbol indirect_variable(LispSymbol symbol)
        {
            LispSymbol tortoise, hare;

            hare = tortoise = symbol;

            while (hare.IsIndirectVariable)
            {
                hare = XSYMBOL(hare.Value);
                if (!hare.IsIndirectVariable)
                    break;

                hare = XSYMBOL(hare.Value);
                tortoise = XSYMBOL(tortoise.Value);

                if (hare == tortoise)
                {
                    xsignal1(Q.cyclic_variable_indirection, symbol);
                }
            }

            return hare;
        }

        /* Given the raw contents of a symbol value cell,
           return the Lisp value of the symbol.
           This does not handle buffer-local variables; use
           swap_in_symval_forwarding for that.  */
        public static LispObject do_symval_forwarding (LispObject valcontents)
        {
            if (MISCP(valcontents))
            {
                if (valcontents is LispIntFwd)
                {
                    return XSETINT(XINTFWD(valcontents).intvar);
                }

                if (valcontents is LispBoolFwd)
                {
                    return XBOOLFWD(valcontents).boolvar ? Q.t : Q.nil;
                }

                if (valcontents is LispObjFwd)
                {
                    return XOBJFWD(valcontents).objvar;
                }

                if (valcontents is LispBufferObjFwd)
                {
                    return PER_BUFFER_VALUE(current_buffer,
                                            XBUFFER_OBJFWD(valcontents).offset);
                }

                if (valcontents is LispKboardObjFwd)
                {
                    /* We used to simply use current_kboard here, but from Lisp
                       code, it's value is often unexpected.  It seems nicer to
                       allow constructions like this to work as intuitively expected:

                       (with-selected-frame frame
                       (define-key local-function-map "\eOP" [f1]))

                       On the other hand, this affects the semantics of
                       last-command and real-last-command, and people may rely on
                       that.  I took a quick look at the Lisp codebase, and I
                       don't think anything will break.  --lorentey  */
                    return FRAME_KBOARD(SELECTED_FRAME())[XKBOARD_OBJFWD(valcontents).offset];
                }
            }
            return valcontents;
        }

        /* Find the function at the end of a chain of symbol function indirections.  */

        /* If OBJECT is a symbol, find the end of its function chain and
           return the value found there.  If OBJECT is not a symbol, just
           return it.  If there is a cycle in the function chain, signal a
           cyclic-function-indirection error.

           This is like Findirect_function, except that it doesn't signal an
           error if the chain ends up unbound.  */
        public static LispObject indirect_function(LispObject obj)
        {
            LispObject tortoise, hare;

            hare = tortoise = obj;

            for (; ; )
            {
                if (!SYMBOLP(hare) || EQ(hare, Q.unbound))
                    break;
                hare = XSYMBOL(hare).Function;
                if (!SYMBOLP(hare) || EQ(hare, Q.unbound))
                    break;
                hare = XSYMBOL(hare).Function;

                tortoise = XSYMBOL(tortoise).Function;

                if (EQ(hare, tortoise))
                    xsignal1(Q.cyclic_function_indirection, obj);
            }

            return hare;
        }

        public static LispObject wrong_type_argument(LispObject predicate, LispObject value)
        {
            xsignal2(Q.wrong_type_argument, predicate, value);
            return Q.nil;
        }

        public static void args_out_of_range(LispObject a1, LispObject a2)
        {
            xsignal2(Q.args_out_of_range, a1, a2);
        }

        /* Store NEWVAL into SYMBOL, where VALCONTENTS is found in the value cell
           of SYMBOL.  If SYMBOL is buffer-local, VALCONTENTS should be the
           buffer-independent contents of the value cell: forwarded just one
           step past the buffer-localness.

           BUF non-zero means set the value in buffer BUF instead of the
           current buffer.  This only plays a role for per-buffer variables.  */
        public static void store_symval_forwarding (LispObject symbol, LispObject valcontents, LispObject newval, Buffer buf)
        {
            if (valcontents is LispIntFwd)
            {
                CHECK_NUMBER (newval);
                XINTFWD(valcontents).intvar = XINT (newval);
                return;
            }

            if (valcontents is LispBoolFwd)
            {
                XBOOLFWD (valcontents).boolvar = !NILP (newval);
                return;
            }

            if (valcontents is LispObjFwd)
            {
                XOBJFWD (valcontents).objvar = newval;

                /* If this variable is a default for something stored
                   in the buffer itself, such as default-fill-column,
                   find the buffers that don't have local values for it
                   and update them.  */
                if (XOBJFWD (valcontents).IsDefault)
                {
                    int offset = XOBJFWD (valcontents).Offset;
                    int idx = PER_BUFFER_IDX (offset);

                    if (idx <= 0)
                        return;

                    for (LispObject tail = V.buffer_alist; CONSP (tail); tail = XCDR (tail))
                    {
                        LispObject buff = F.cdr (XCAR (tail));
                        if (!BUFFERP (buff))
                            continue;
                        Buffer b = XBUFFER (buff);

                        if (! PER_BUFFER_VALUE_P (b, idx))
                            b[offset] = newval;
                    }
                }
                return;
            }

            if (valcontents is LispBufferObjFwd)
            {
                int offset = XBUFFER_OBJFWD (valcontents).offset;
                System.Type type = XBUFFER_OBJFWD (valcontents).slottype;

                if (type != null && ! NILP (newval) && XTYPE (newval) != type)
                {
                    buffer_slot_type_mismatch (newval, type);
                }

                if (buf == null)
                    buf = current_buffer;
                buf[offset] = newval;

                return;
            }

            if (valcontents is LispKboardObjFwd)
            {
                kboard k = FRAME_KBOARD (SELECTED_FRAME ());
                k[XKBOARD_OBJFWD(valcontents).offset] = newval;
                return;
            }

            valcontents = SYMBOL_VALUE (symbol);
            if (BUFFER_LOCAL_VALUEP (valcontents))
                XBUFFER_LOCAL_VALUE (valcontents).realvalue = newval;
            else
                SET_SYMBOL_VALUE (symbol, newval);
        }

        /* Set up the buffer-local symbol SYMBOL for validity in the current buffer.
           VALCONTENTS is the contents of its value cell,
           which points to a struct Lisp_Buffer_Local_Value.

           Return the value forwarded one step past the buffer-local stage.
           This could be another forwarding pointer.  */

        public static LispObject swap_in_symval_forwarding (LispObject symbol, LispObject valcontents)
        {
            LispObject tem1 = XBUFFER_LOCAL_VALUE (valcontents).buffer;

            if (NILP (tem1)
                || current_buffer != XBUFFER (tem1)
                || (XBUFFER_LOCAL_VALUE (valcontents).check_frame
                    && ! EQ (selected_frame, XBUFFER_LOCAL_VALUE (valcontents).frame)))
            {
                LispSymbol sym = XSYMBOL (symbol);
                if (sym.IsIndirectVariable)
                {
                    sym = indirect_variable (sym);
                    symbol = sym;
                }

                /* Unload the previously loaded binding.  */
                tem1 = XCAR (XBUFFER_LOCAL_VALUE (valcontents).cdr);
                F.setcdr (tem1,
                         do_symval_forwarding (XBUFFER_LOCAL_VALUE (valcontents).realvalue));
                /* Choose the new binding.  */
                tem1 = assq_no_quit(symbol, current_buffer.local_var_alist);
                XBUFFER_LOCAL_VALUE (valcontents).found_for_frame  = false;
                XBUFFER_LOCAL_VALUE (valcontents).found_for_buffer = false;
                if (NILP(tem1))
                {
                    if (XBUFFER_LOCAL_VALUE(valcontents).check_frame)
                        tem1 = assq_no_quit(symbol, XFRAME(selected_frame).param_alist);
                    if (!NILP(tem1))
                        XBUFFER_LOCAL_VALUE(valcontents).found_for_frame = true;
                    else
                        tem1 = XBUFFER_LOCAL_VALUE(valcontents).cdr;
                }
                else
                    XBUFFER_LOCAL_VALUE(valcontents).found_for_buffer = true;

                /* Load the new binding.  */
                XSETCAR (XBUFFER_LOCAL_VALUE (valcontents).cdr, tem1);
                XBUFFER_LOCAL_VALUE(valcontents).buffer = current_buffer;
                XBUFFER_LOCAL_VALUE (valcontents).frame = selected_frame;
                store_symval_forwarding (symbol,
                                         XBUFFER_LOCAL_VALUE (valcontents).realvalue,
                                         F.cdr (tem1), null);
            }
            return XBUFFER_LOCAL_VALUE (valcontents).realvalue;
        }

        /* Find the value of a symbol, returning Qunbound if it's not bound.
           This is helpful for code which just wants to get a variable's value
           if it has one, without signaling an error.
           Note that it must not be possible to quit
           within this function.  Great care is required for this.  */
        public static LispObject find_symbol_value(LispObject symbol)
        {
            LispObject valcontents;

            CHECK_SYMBOL(symbol);
            valcontents = SYMBOL_VALUE(symbol);

            if (BUFFER_LOCAL_VALUEP(valcontents))
                valcontents = swap_in_symval_forwarding(symbol, valcontents);

            return do_symval_forwarding(valcontents);
        }

        /* Return 1 if SYMBOL currently has a let-binding
           which was made in the buffer that is now current.  */
        public static bool let_shadows_buffer_binding_p(LispSymbol symbol)
        {
            foreach (specbinding p in specpdl)
            {
                if (p.func == null && CONSP(p.symbol))
                {
                    LispSymbol let_bound_symbol = XSYMBOL(XCAR(p.symbol));
                    if ((symbol == let_bound_symbol ||
                         (let_bound_symbol.IsIndirectVariable && symbol == indirect_variable(let_bound_symbol))) &&
                        XBUFFER(XCDR(XCDR(p.symbol))) == current_buffer)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /* Store the value NEWVAL into SYMBOL.
           If buffer-locality is an issue, BUF specifies which buffer to use.
           (0 stands for the current buffer.)

           If BINDFLAG is zero, then if this symbol is supposed to become
           local in every buffer where it is set, then we make it local.
           If BINDFLAG is nonzero, we don't do that.  */

        public static LispObject set_internal (LispObject symbol, LispObject newval, Buffer buf, bool bindflag)
        {
            bool voide = EQ (newval, Q.unbound);

            LispObject valcontents, innercontents, tem1, current_alist_element;

            if (buf == null)
                buf = current_buffer;

            /* If restoring in a dead buffer, do nothing.  */
            if (NILP (buf.name))
                return newval;

            CHECK_SYMBOL (symbol);
            if (SYMBOL_CONSTANT_P (symbol)
                && (NILP (F.keywordp (symbol))
                    || !EQ (newval, SYMBOL_VALUE (symbol))))
                xsignal1 (Q.setting_constant, symbol);

            innercontents = valcontents = SYMBOL_VALUE (symbol);

            if (BUFFER_OBJFWDP (valcontents))
            {
                int offset = XBUFFER_OBJFWD (valcontents).offset;
                int idx = PER_BUFFER_IDX (offset);
                if (idx > 0
                    && !bindflag
                    && !let_shadows_buffer_binding_p (XSYMBOL (symbol)))
                    SET_PER_BUFFER_VALUE_P (buf, idx, 1);
            }
            else if (BUFFER_LOCAL_VALUEP (valcontents))
            {
                /* valcontents is a struct Lisp_Buffer_Local_Value.   */
                if (XSYMBOL (symbol).IsIndirectVariable)
                    symbol = indirect_variable (XSYMBOL (symbol));

                /* What binding is loaded right now?  */
                current_alist_element
                    = XCAR (XBUFFER_LOCAL_VALUE (valcontents).cdr);

                /* If the current buffer is not the buffer whose binding is
                   loaded, or if there may be frame-local bindings and the frame
                   isn't the right one, or if it's a Lisp_Buffer_Local_Value and
                   the default binding is loaded, the loaded binding may be the
                   wrong one.  */
                if (!BUFFERP (XBUFFER_LOCAL_VALUE (valcontents).buffer)
                    || buf != XBUFFER (XBUFFER_LOCAL_VALUE (valcontents).buffer)
                    || (XBUFFER_LOCAL_VALUE (valcontents).check_frame
                        && !EQ (selected_frame, XBUFFER_LOCAL_VALUE (valcontents).frame))
                    /* Also unload a global binding (if the var is local_if_set). */
                    || (EQ (XCAR (current_alist_element),
                            current_alist_element)))
                {
                    /* The currently loaded binding is not necessarily valid.
                       We need to unload it, and choose a new binding.  */

                    /* Write out `realvalue' to the old loaded binding.  */
                    F.setcdr (current_alist_element,
                             do_symval_forwarding (XBUFFER_LOCAL_VALUE (valcontents).realvalue));

                    /* Find the new binding.  */
                    tem1 = F.assq (symbol, buf.local_var_alist);
                    XBUFFER_LOCAL_VALUE (valcontents).found_for_buffer = true;
                    XBUFFER_LOCAL_VALUE (valcontents).found_for_frame = false;

                    if (NILP (tem1))
                    {
                        /* This buffer still sees the default value.  */

                        /* If the variable is not local_if_set,
                           or if this is `let' rather than `set',
                           make CURRENT-ALIST-ELEMENT point to itself,
                           indicating that we're seeing the default value.
                           Likewise if the variable has been let-bound
                           in the current buffer.  */
                        if (bindflag || !XBUFFER_LOCAL_VALUE (valcontents).local_if_set
                            || let_shadows_buffer_binding_p (XSYMBOL (symbol)))
                        {
                            XBUFFER_LOCAL_VALUE (valcontents).found_for_buffer = false;

                            if (XBUFFER_LOCAL_VALUE (valcontents).check_frame)
                                tem1 = F.assq (symbol,
                                              XFRAME (selected_frame).param_alist);

                            if (! NILP (tem1))
                                XBUFFER_LOCAL_VALUE (valcontents).found_for_frame = true;
                            else
                                tem1 = XBUFFER_LOCAL_VALUE (valcontents).cdr;
                        }
                        /* If it's a Lisp_Buffer_Local_Value, being set not bound,
                           and we're not within a let that was made for this buffer,
                           create a new buffer-local binding for the variable.
                           That means, give this buffer a new assoc for a local value
                           and load that binding.  */
                        else
                        {
                            tem1 = F.cons (symbol, XCDR (current_alist_element));
                            buf.local_var_alist = F.cons (tem1, buf.local_var_alist);
                        }
                    }

                    /* Record which binding is now loaded.  */
                    XSETCAR (XBUFFER_LOCAL_VALUE (valcontents).cdr, tem1);

                    /* Set `buffer' and `frame' slots for the binding now loaded.  */
                    XBUFFER_LOCAL_VALUE(valcontents).buffer = buf;
                    XBUFFER_LOCAL_VALUE (valcontents).frame = selected_frame;
                }
                innercontents = XBUFFER_LOCAL_VALUE (valcontents).realvalue;

                /* Store the new value in the cons-cell.  */
                XSETCDR (XCAR (XBUFFER_LOCAL_VALUE (valcontents).cdr), newval);
            }

            /* If storing void (making the symbol void), forward only through
               buffer-local indicator, not through Lisp_Objfwd, etc.  */
            if (voide)
                store_symval_forwarding (symbol, Q.nil, newval, buf);
            else
                store_symval_forwarding (symbol, innercontents, newval, buf);

            return newval;
        }

        /* Access or set a buffer-local symbol's default value.  */

        /* Return the default value of SYMBOL, but don't check for voidness.
           Return Qunbound if it is void.  */
        public static LispObject default_value (LispObject symbol)
        {
            LispObject valcontents;

            CHECK_SYMBOL(symbol);
            valcontents = SYMBOL_VALUE(symbol);

            /* For a built-in buffer-local variable, get the default value
               rather than letting do_symval_forwarding get the current value.  */
            if (BUFFER_OBJFWDP(valcontents))
            {
                int offset = XBUFFER_OBJFWD(valcontents).offset;
                if (PER_BUFFER_IDX(offset) != 0)
                    return PER_BUFFER_DEFAULT(offset);
            }

            /* Handle user-created local variables.  */
            if (BUFFER_LOCAL_VALUEP(valcontents))
            {
                /* If var is set up for a buffer that lacks a local value for it,
               the current value is nominally the default value.
               But the `realvalue' slot may be more up to date, since
               ordinary setq stores just that slot.  So use that.  */
                LispObject current_alist_element, alist_element_car;
                current_alist_element
              = XCAR(XBUFFER_LOCAL_VALUE(valcontents).cdr);
                alist_element_car = XCAR(current_alist_element);
                if (EQ(alist_element_car, current_alist_element))
                    return do_symval_forwarding(XBUFFER_LOCAL_VALUE(valcontents).realvalue);
                else
                    return XCDR(XBUFFER_LOCAL_VALUE(valcontents).cdr);
            }
            /* For other variables, get the current value.  */
            return do_symval_forwarding(valcontents);
        }
    }

    public partial class F
    {
        public static LispObject default_boundp(LispObject symbol)
        {
            LispObject value = L.default_value(symbol);
            return (L.EQ(value, Q.unbound) ? Q.nil : Q.t);
        }

        public static LispObject default_value(LispObject symbol)
        {
            LispObject value = default_value(symbol);
            if (!L.EQ(value, Q.unbound))
                return value;

            L.xsignal1(Q.void_variable, symbol);
            return Q.nil;
        }

        public static LispObject set_default (LispObject symbol, LispObject value)
        {
            L.CHECK_SYMBOL (symbol);
            LispObject valcontents = L.SYMBOL_VALUE (symbol);

            /* Handle variables like case-fold-search that have special slots
               in the buffer.  Make them work apparently like Lisp_Buffer_Local_Value
               variables.  */
            if (L.BUFFER_OBJFWDP (valcontents))
            {
                int offset = L.XBUFFER_OBJFWD (valcontents).offset;
                int idx = L.PER_BUFFER_IDX (offset);

                L.buffer_defaults[offset] = value;
                
                /* If this variable is not always local in all buffers,
                   set it in the buffers that don't nominally have a local value.  */
                if (idx > 0)
                {
                    for (Buffer b = L.all_buffers; b != null; b = b.next)
                        if (!L.PER_BUFFER_VALUE_P(b, idx))
                            b[offset] = value;
                }
                return value;
            }

            if (!L.BUFFER_LOCAL_VALUEP (valcontents))
                return F.set (symbol, value);

            /* Store new value into the DEFAULT-VALUE slot.  */
            L.XSETCDR(L.XBUFFER_LOCAL_VALUE(valcontents).cdr, value);

            /* If the default binding is now loaded, set the REALVALUE slot too.  */
            LispObject current_alist_element = L.XCAR (L.XBUFFER_LOCAL_VALUE (valcontents).cdr);
            LispObject alist_element_buffer  = F.car(current_alist_element);
            if (L.EQ (alist_element_buffer, current_alist_element))
                L.store_symval_forwarding (symbol,
                                         L.XBUFFER_LOCAL_VALUE(valcontents).realvalue,
                                         value, null);

            return value;
        }
        
        public static LispObject keywordp(LispObject obj)
        {
            if (L.SYMBOLP(obj) && L.SREF(L.SYMBOL_NAME(obj), 0) == ':' && L.SYMBOL_INTERNED_IN_INITIAL_OBARRAY_P(obj))
                return Q.t;
            return Q.nil;
        }

        public static LispObject local_variable_p(LispObject variable, LispObject buffer)
        {
            LispObject valcontents;
            Buffer buf;

            if (L.NILP(buffer))
                buf = L.current_buffer;
            else
            {
                L.CHECK_BUFFER(buffer);
                buf = L.XBUFFER(buffer);
            }

            L.CHECK_SYMBOL(variable);
            LispSymbol sym = L.indirect_variable(L.XSYMBOL(variable));
            variable = sym;

            valcontents = sym.Value;
            if (L.BUFFER_LOCAL_VALUEP(valcontents))
            {
                LispObject tail, elt;

                for (tail = buf.local_var_alist; L.CONSP(tail); tail = L.XCDR(tail))
                {
                    elt = L.XCAR(tail);
                    if (L.EQ(variable, L.XCAR(elt)))
                        return Q.t;
                }
            }
            if (L.BUFFER_OBJFWDP(valcontents))
            {
                int offset = L.XBUFFER_OBJFWD(valcontents).offset;
                int idx = L.PER_BUFFER_IDX(offset);
                if (idx == -1 || L.PER_BUFFER_VALUE_P(buf, idx))
                    return Q.t;
            }
            return Q.nil;
        }

        /* Extract and set vector and string elements */
        public static LispObject aref(LispObject array, LispObject idx)
        {
            int idxval;

            L.CHECK_NUMBER(idx);
            idxval = L.XINT(idx);
            if (L.STRINGP(array))
            {
                int c, idxval_byte;

                if (idxval < 0 || idxval >= L.SCHARS(array))
                    L.args_out_of_range(array, idx);
                if (!L.STRING_MULTIBYTE(array))
                    return L.make_number((byte)L.SREF(array, idxval));
                idxval_byte = L.string_char_to_byte(array, idxval);

                c = (int)L.STRING_CHAR(L.SDATA(array), idxval_byte,
                         L.SBYTES(array) - idxval_byte);
                return L.make_number(c);
            }
            else if (L.BOOL_VECTOR_P(array))
            {
                int val;

                if (idxval < 0 || idxval >= L.XBOOL_VECTOR(array).Size)
                    L.args_out_of_range(array, idx);

                val = (byte)L.XBOOL_VECTOR(array)[idxval / LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR];
                return ((val & (1 << (idxval % LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR))) != 0 ? Q.t : Q.nil);
            }
#if COMEBACK_LATER
  else if (CHAR_TABLE_P (array))
    {
      CHECK_CHARACTER (idx);
      return CHAR_TABLE_REF (array, idxval);
    }
#endif
            else
            {
                int size = 0;
                if (L.VECTORP(array))
                    size = L.XVECTOR(array).Size;
                else if (L.COMPILEDP(array))
                    size = (array as LispCompiled).Size;
                else
                    L.wrong_type_argument(Q.arrayp, array);

                if (idxval < 0 || idxval >= size)
                    L.args_out_of_range(array, idx);
                return L.XVECTOR(array)[idxval];
            }
        }

        public static LispObject set(LispObject symbol, LispObject newval)
        {
            return L.set_internal(symbol, newval, L.current_buffer, false);
        }

        public static LispObject symbol_value(LispObject symbol)
        {
            LispObject val = L.find_symbol_value(symbol);
            if (!L.EQ(val, Q.unbound))
                return val;

            L.xsignal1(Q.void_variable, symbol);
            return Q.nil;
        }

        public static LispObject car(LispObject list)
        {
            return L.CAR(list);
        }

        public static LispObject cdr(LispObject list)
        {
            return L.CDR(list);
        }

        public static LispObject setcar(LispObject cell, LispObject newcar)
        {
            L.CHECK_CONS(cell);
            L.XSETCAR(cell, newcar);
            return newcar;
        }

        public static LispObject setcdr(LispObject cell, LispObject newcdr)
        {
            L.CHECK_CONS(cell);
            L.XSETCDR(cell, newcdr);
            return newcdr;
        }

        /* Extract and set components of symbols */
        public static LispObject boundp(LispObject symbol)
        {
            L.CHECK_SYMBOL(symbol);

            LispObject valcontents = L.SYMBOL_VALUE(symbol);

            if (L.BUFFER_LOCAL_VALUEP(valcontents))
                valcontents = L.swap_in_symval_forwarding(symbol, valcontents);

            return (L.EQ(valcontents, Q.unbound) ? Q.nil : Q.t);
        }

        public static LispObject symbol_function(LispObject symbol)
        {
            L.CHECK_SYMBOL(symbol);
            if (!L.EQ(L.XSYMBOL(symbol).function, Q.unbound))
                return L.XSYMBOL(symbol).function;

            L.xsignal1(Q.void_function, symbol);
            return Q.nil;
        }

        public static LispObject symbol_name(LispObject symbol)
        {
            L.CHECK_SYMBOL(symbol);
            return L.SYMBOL_NAME(symbol);
        }

        public static LispObject fset(LispObject symbol, LispObject definition)
        {
            LispObject function;

            L.CHECK_SYMBOL(symbol);
            if (L.NILP(symbol) || L.EQ(symbol, Q.t))
                L.xsignal1(Q.setting_constant, symbol);

            function = L.XSYMBOL(symbol).function;

            if (!L.NILP(V.autoload_queue) && !L.EQ(function, Q.unbound))
                V.autoload_queue = F.cons(F.cons(symbol, function), V.autoload_queue);

            if (L.CONSP(function) && L.EQ(L.XCAR(function), Q.autoload))
                F.put(symbol, Q.autoload, L.XCDR(function));

            L.XSYMBOL(symbol).function = definition;
            /* Handle automatic advice activation */
            if (L.CONSP(L.XSYMBOL(symbol).plist) && !L.NILP(F.get(symbol, Q.ad_advice_info)))
            {
                L.call2(Q.ad_activate_internal, symbol, Q.nil);
                definition = L.XSYMBOL(symbol).function;
            }
            return definition;
        }

        public static LispObject indirect_function(LispObject obj, LispObject noerror)
        {
            LispObject result;

            /* Optimize for no indirection.  */
            result = obj;
            if (L.SYMBOLP(result) && !L.EQ(result, Q.unbound))
            {
                result = L.XSYMBOL(result).function;
                if (L.SYMBOLP(result))
                    result = L.indirect_function(result);
            }

            if (!L.EQ(result, Q.unbound))
                return result;

            if (L.NILP(noerror))
                L.xsignal1(Q.void_function, obj);

            return Q.nil;
        }
    }
}
