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

        public static void args_out_of_range_3(LispObject a1, LispObject a2, LispObject a3)
        {
            xsignal3(Q.args_out_of_range, a1, a2, a3);
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

        /* Set up SYMBOL to refer to its global binding.
           This makes it safe to alter the status of other bindings.  */
        public static void swap_in_global_binding(LispObject symbol)
        {
            LispObject valcontents = SYMBOL_VALUE(symbol);
            LispBufferLocalValue blv = XBUFFER_LOCAL_VALUE(valcontents);
            LispObject cdr = blv.cdr;

            /* Unload the previously loaded binding.  */
            F.setcdr(XCAR(cdr),
                 do_symval_forwarding(blv.realvalue));

            /* Select the global binding in the symbol.  */
            XSETCAR(cdr, cdr);
            store_symval_forwarding(symbol, blv.realvalue, XCDR(cdr), null);

            /* Indicate that the global binding is set up now.  */
            blv.frame = Q.nil;
            blv.buffer = Q.nil;
            blv.found_for_frame = false;
            blv.found_for_buffer = false;
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

            else if (L.CHAR_TABLE_P(array))
            {
                L.CHECK_CHARACTER(idx);
                return L.CHAR_TABLE_REF(array, (uint) idxval);
            }

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

        public static LispObject aset(LispObject array, LispObject idx, LispObject newelt)
        {
            int idxval;

            L.CHECK_NUMBER (idx);
            idxval = L.XINT (idx);
            L.CHECK_ARRAY (array, Q.arrayp);

            if (L.VECTORP (array))
            {
                if (idxval < 0 || idxval >= L.XVECTOR (array).Size)
                    L.args_out_of_range (array, idx);
                L.XVECTOR (array)[idxval] = newelt;
            }
            else if (L.BOOL_VECTOR_P (array))
            {
                int val;

                if (idxval < 0 || idxval >= L.XBOOL_VECTOR (array).Size)
                    L.args_out_of_range (array, idx);

                val = (byte) L.XBOOL_VECTOR (array)[idxval / LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR];

                if (! L.NILP (newelt))
                    val |= 1 << (idxval % LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR);
                else
                    val &= ~(1 << (idxval % LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR));
                L.XBOOL_VECTOR (array)[idxval / LispBoolVector.BOOL_VECTOR_BITS_PER_CHAR] = (byte) val;
            }
            else if (L.CHAR_TABLE_P (array))
            {
                L.CHECK_CHARACTER (idx);
                L.CHAR_TABLE_SET (array, idxval, newelt);
            }
            else if (L.STRING_MULTIBYTE (array))
            {
                int idxval_byte, prev_bytes = 0, new_bytes, nbytes;
                byte[] workbuf = new byte[L.MAX_MULTIBYTE_LENGTH];
                int p0 = 0;

                if (idxval < 0 || idxval >= L.SCHARS (array))
                    L.args_out_of_range (array, idx);
                L.CHECK_CHARACTER (newelt);

                nbytes = L.SBYTES (array);

                idxval_byte = L.string_char_to_byte (array, idxval);
                byte[] b1 = L.SDATA(array);
                int p1 = idxval_byte;
                L.PARSE_MULTIBYTE_SEQ (L.SDATA (array), idxval_byte, nbytes - idxval_byte, ref prev_bytes);
                new_bytes = L.CHAR_STRING ((uint) L.XINT (newelt), workbuf);
                if (prev_bytes != new_bytes)
                {
                    /* We must relocate the string data.  */
                    int nchars = L.SCHARS (array);
                    byte[] str = new byte[nbytes];
                    System.Array.Copy(L.SDATA (array), str, nbytes);

                    L.XSTRING(array).allocate_string_data(nchars, nbytes + new_bytes - prev_bytes);

                    L.XSTRING(array).bcopy(str, idxval_byte);

                    b1 = L.SDATA(array);
                    p1 = idxval_byte;
                    L.XSTRING(array).bcopy (str, idxval_byte + prev_bytes, p1 + new_bytes,
                           nbytes - (idxval_byte + prev_bytes));

                    L.clear_string_char_byte_cache ();
                }
                while (new_bytes-- != 0)
                    b1[p1++] = workbuf[p0++];
            }
            else
            {
                if (idxval < 0 || idxval >= L.SCHARS (array))
                    L.args_out_of_range (array, idx);
                L.CHECK_NUMBER (newelt);

                if (L.XINT (newelt) >= 0 && ! L.SINGLE_BYTE_CHAR_P ((uint) L.XINT (newelt)))
                {
                    int i;

                    for (i = L.SBYTES (array) - 1; i >= 0; i--)
                        if (L.SREF (array, i) >= 0x80)
                            L.args_out_of_range (array, newelt);
                    /* ARRAY is an ASCII string.  Convert it to a multibyte
                       string, and try `aset' again.  */
                    L.STRING_SET_MULTIBYTE (ref array);
                    return F.aset (array, idx, newelt);
                }
                L.SSET (array, idxval, (byte) L.XINT (newelt));
            }

            return newelt;
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

        public static LispObject add1(LispObject number)
        {
            L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref number);

            if (L.FLOATP(number))
                return (L.make_float(1.0 + L.XFLOAT_DATA(number)));

            number = L.XSETINT(L.XINT(number) + 1);
            return number;
        }

        public static LispObject sub1(LispObject number)
        {
            L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref number);

            if (L.FLOATP(number))
                return (L.make_float(-1.0 + L.XFLOAT_DATA(number)));

            number = L.XSETINT(L.XINT(number) - 1);
            return number;
        }
    }

    public partial class L
    {
        public enum arithop
        {
            Aadd,
            Asub,
            Amult,
            Adiv,
            Alogand,
            Alogior,
            Alogxor,
            Amax,
            Amin
        }

        public static LispObject arith_driver(arithop code, int nargs, params LispObject[] args)
        {
            LispObject val;
            int argnum;
            int accum = 0;
            int next;

            switch (code)
            {
                case arithop.Alogior:
                case arithop.Alogxor:
                case arithop.Aadd:
                case arithop.Asub:
                    accum = 0;
                    break;
                case arithop.Amult:
                    accum = 1;
                    break;
                case arithop.Alogand:
                    accum = -1;
                    break;
                default:
                    break;
            }

            for (argnum = 0; argnum < nargs; argnum++)
            {
                /* Using args[argnum] as argument to CHECK_NUMBER_... */
                val = args[argnum];
                CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref val);

                if (FLOATP(val))
                    return float_arith_driver((double)accum, argnum, code,
                                   nargs, args);
                args[argnum] = val;
                next = XINT(args[argnum]);
                switch (code)
                {
                    case arithop.Aadd:
                        accum += next;
                        break;
                    case arithop.Asub:
                        accum = argnum != 0 ? accum - next : nargs == 1 ? -next : next;
                        break;
                    case arithop.Amult:
                        accum *= next;
                        break;
                    case arithop.Adiv:
                        if (argnum == 0)
                            accum = next;
                        else
                        {
                            if (next == 0)
                                xsignal0(Q.arith_error);
                            accum /= next;
                        }
                        break;
                    case arithop.Alogand:
                        accum &= next;
                        break;
                    case arithop.Alogior:
                        accum |= next;
                        break;
                    case arithop.Alogxor:
                        accum ^= next;
                        break;
                    case arithop.Amax:
                        if (argnum == 0 || next > accum)
                            accum = next;
                        break;
                    case arithop.Amin:
                        if (argnum == 0 || next < accum)
                            accum = next;
                        break;
                }
            }

            val = XSETINT(accum);
            return val;
        }

        public static LispObject float_arith_driver(double accum, int argnum, arithop code, int nargs, params LispObject[] args)
        {
            LispObject val;
            double next;

            for (; argnum < nargs; argnum++)
            {
                val = args[argnum];    /* using args[argnum] as argument to CHECK_NUMBER_... */
                CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref val);

                if (FLOATP(val))
                {
                    next = XFLOAT_DATA(val);
                }
                else
                {
                    args[argnum] = val;    /* runs into a compiler bug. */
                    next = XINT(args[argnum]);
                }
                switch (code)
                {
                    case arithop.Aadd:
                        accum += next;
                        break;
                    case arithop.Asub:
                        accum = argnum != 0 ? accum - next : nargs == 1 ? -next : next;
                        break;
                    case arithop.Amult:
                        accum *= next;
                        break;
                    case arithop.Adiv:
                        if (argnum == 0)
                            accum = next;
                        else
                        {
                            accum /= next;
                        }
                        break;
                    case arithop.Alogand:
                    case arithop.Alogior:
                    case arithop.Alogxor:
                        return wrong_type_argument(Q.integer_or_marker_p, val);
                    case arithop.Amax:
                        if (argnum == 0 || double.IsNaN(next) || next > accum)
                            accum = next;
                        break;
                    case arithop.Amin:
                        if (argnum == 0 || double.IsNaN(next) || next < accum)
                            accum = next;
                        break;
                }
            }

            return make_float(accum);
        }

        /* Arithmetic functions */
        public enum comparison { equal, notequal, less, grtr, less_or_equal, grtr_or_equal };

        public static LispObject arithcompare(LispObject num1, LispObject num2, comparison comparison)
        {
            double f1 = 0, f2 = 0;
            bool floatp = false;

            CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref num1);
            CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref num2);

            if (FLOATP(num1) || FLOATP(num2))
            {
                floatp = true;
                f1 = (FLOATP(num1)) ? XFLOAT_DATA(num1) : XINT(num1);
                f2 = (FLOATP(num2)) ? XFLOAT_DATA(num2) : XINT(num2);
            }

            switch (comparison)
            {
                case comparison.equal:
                    if (floatp ? f1 == f2 : XINT(num1) == XINT(num2))
                        return Q.t;
                    return Q.nil;

                case comparison.notequal:
                    if (floatp ? f1 != f2 : XINT(num1) != XINT(num2))
                        return Q.t;
                    return Q.nil;

                case comparison.less:
                    if (floatp ? f1 < f2 : XINT(num1) < XINT(num2))
                        return Q.t;
                    return Q.nil;

                case comparison.less_or_equal:
                    if (floatp ? f1 <= f2 : XINT(num1) <= XINT(num2))
                        return Q.t;
                    return Q.nil;

                case comparison.grtr:
                    if (floatp ? f1 > f2 : XINT(num1) > XINT(num2))
                        return Q.t;
                    return Q.nil;

                case comparison.grtr_or_equal:
                    if (floatp ? f1 >= f2 : XINT(num1) >= XINT(num2))
                        return Q.t;
                    return Q.nil;

                default:
                    abort();
                    return Q.nil;
            }
        }
    }

    public partial class F
    {
        public static LispObject fboundp(LispObject symbol)
        {
            L.CHECK_SYMBOL(symbol);
            return (L.EQ(L.XSYMBOL(symbol).function, Q.unbound) ? Q.nil : Q.t);
        }

        public static LispObject plus(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Aadd, nargs, args);
        }

        public static LispObject minus(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Asub, nargs, args);
        }

        public static LispObject times(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Amult, nargs, args);
        }

        public static LispObject quo(int nargs, params LispObject[] args)
        {
            int argnum;
            for (argnum = 2; argnum < nargs; argnum++)
                if (L.FLOATP(args[argnum]))
                    return L.float_arith_driver(0, 0, L.arithop.Adiv, nargs, args);
            return L.arith_driver(L.arithop.Adiv, nargs, args);
        }

        public static LispObject rem(LispObject x, LispObject y)
        {
            LispObject val;

            L.CHECK_NUMBER_COERCE_MARKER(ref x);
            L.CHECK_NUMBER_COERCE_MARKER(ref y);

            if (L.XINT(y) == 0)
                L.xsignal0(Q.arith_error);

            val = L.XSETINT(L.XINT(x) % L.XINT(y));
            return val;
        }

        public static LispObject mod(LispObject x, LispObject y)
        {
            LispObject val;
            int i1, i2;

            L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref x);
            L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref y);

            if (L.FLOATP(x) || L.FLOATP(y))
                return L.fmod_float(x, y);

            i1 = L.XINT(x);
            i2 = L.XINT(y);

            if (i2 == 0)
                L.xsignal0(Q.arith_error);

            i1 %= i2;

            /* If the "remainder" comes out with the wrong sign, fix it.  */
            if (i2 < 0 ? i1 > 0 : i1 < 0)
                i1 += i2;

            val = L.XSETINT(i1);
            return val;
        }

        public static LispObject max(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Amax, nargs, args);
        }

        public static LispObject min(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Amin, nargs, args);
        }

        public static LispObject logand(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Alogand, nargs, args);
        }

        public static LispObject logior(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Alogior, nargs, args);
        }

        public static LispObject logxor(int nargs, params LispObject[] args)
        {
            return L.arith_driver(L.arithop.Alogxor, nargs, args);
        }

        public static LispObject eqlsign(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.equal);
        }

        public static LispObject lss(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.less);
        }

        public static LispObject gtr(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.grtr);
        }

        public static LispObject leq(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.less_or_equal);
        }

        public static LispObject geq(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.grtr_or_equal);
        }

        public static LispObject neq(LispObject num1, LispObject num2)
        {
            return L.arithcompare(num1, num2, L.comparison.notequal);
        }
    }
}
