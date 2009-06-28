using System.Collections.Generic;

namespace IronElisp
{
    public partial class Q
    {
        public static LispObject autoload, macro, exit, interactive, commandp, defun;
        public static LispObject inhibit_quit;
        public static LispObject and_rest, and_optional;
        public static LispObject debug_on_error;
        public static LispObject declare;
        public static LispObject debug;
    }

    public partial class V
    {
        public static LispObject run_hooks;

        /* Non-nil means record all fset's and provide's, to be undone
           if the file being autoloaded is not fully loaded.
           They are recorded by being consed onto the front of Vautoload_queue:
           (FUN . ODEF) for a defun, (0 . OFEATURES) for a provide.  */
        public static LispObject autoload_queue;

        public static LispObject inhibit_quit
        {
            get { return Defs.O[(int)Objects.inhibit_quit]; }
            set { Defs.O[(int)Objects.inhibit_quit] = value; }
        }

        public static LispObject quit_flag
        {
            get { return Defs.O[(int)Objects.quit_flag]; }
            set { Defs.O[(int)Objects.quit_flag] = value; }
        }

        // List of conditions (non-nil atom means all) which cause a backtrace
        // if an error is handled by the command loop's error handler. 
        public static LispObject stack_trace_on_error
        {
            get { return Defs.O[(int)Objects.stack_trace_on_error]; }
            set { Defs.O[(int)Objects.stack_trace_on_error] = value; }
        }

        // List of conditions (non-nil atom means all) which enter the debugger
        // if an error is handled by the command loop's error handler.
        public static LispObject debug_on_error
        {
            get { return Defs.O[(int)Objects.debug_on_error]; }
            set { Defs.O[(int)Objects.debug_on_error] = value; }
        }

        // List of conditions and regexps specifying error messages which
        // do not enter the debugger even if Vdebug_on_error says they should.  
        public static LispObject debug_ignored_errors
        {
            get { return Defs.O[(int)Objects.debug_ignored_errors]; }
            set { Defs.O[(int)Objects.debug_ignored_errors] = value; }
        }

        // Non-nil means call the debugger even if the error will be handled.
        public static LispObject debug_on_signal
        {
            get { return Defs.O[(int)Objects.debug_on_signal]; }
            set { Defs.O[(int)Objects.debug_on_signal] = value; }
        }

        // Hook for edebug to use.
        public static LispObject signal_hook_function
        {
            get { return Defs.O[(int)Objects.signal_hook_function]; }
            set { Defs.O[(int)Objects.signal_hook_function] = value; }
        }

        public static LispObject debugger
        {
            get { return Defs.O[(int)Objects.debugger]; }
            set { Defs.O[(int)Objects.debugger] = value; }
        }

        // The function from which the last `signal' was called.  Set in Fsignal.
        public static LispObject signaling_function;
    }

    public partial class L
    {
        // int specpdl_size = 50;
        // specbinding specpdl_ptr;
        // not used really, but defined just in case
        public static int max_specpdl_size = 1000; 
        public static Stack<specbinding> specpdl = new Stack<specbinding>();

        public static backtrace backtrace_list;
        public static Stack<catchtag> catchlist = new Stack<catchtag>();
        public static Handler handlerlist;

        public static int lisp_eval_depth;
        public static int max_lisp_eval_depth = 400;

        public static bool debug_on_next_call;

        // Nonzero means enter debugger if a quit signal
        // is handled by the command loop's error handler.
        public static bool debug_on_quit;

        /* The value of num_nonmacro_input_events as of the last time we
           started to enter the debugger.  If we decide to enter the debugger
           again when this is still equal to num_nonmacro_input_events, then we
           know that the debugger itself has an error, and we should just
           signal the error instead of entering an infinite loop of debugger
           invocations.  */
        public static int when_entered_debugger;

        public static void init_eval_once ()
        {
            V.run_hooks = Q.nil; 
        }

        public static int SPECPDL_INDEX()
        {
            return specpdl.Count;
        }

        public static void init_eval ()
        {
            specpdl.Clear ();

            handlerlist = null;
            backtrace_list = null;

            V.quit_flag = Q.nil;
            debug_on_next_call = false;

            lisp_eval_depth = 0;

            when_entered_debugger = -1;
        }

        /* unwind-protect function used by call_debugger.  */
        public static LispObject restore_stack_limits(LispObject data)
        {
            max_specpdl_size = XINT(XCAR(data));
            max_lisp_eval_depth = XINT(XCDR(data));
            return Q.nil;
        }

        /* Call the Lisp debugger, giving it argument ARG.  */
        public static LispObject call_debugger (LispObject arg)
        {
            int count = SPECPDL_INDEX ();
            int old_max = max_specpdl_size;

            /* Temporarily bump up the stack limits,
               so the debugger won't run out of stack.  */

            max_specpdl_size += 1;
            record_unwind_protect (restore_stack_limits,
                                   F.cons (make_number (old_max),
                                          make_number (max_lisp_eval_depth)));
            max_specpdl_size = old_max;

            if (lisp_eval_depth + 40 > max_lisp_eval_depth)
                max_lisp_eval_depth = lisp_eval_depth + 40;

            if (SPECPDL_INDEX () + 100 > max_specpdl_size)
                max_specpdl_size = SPECPDL_INDEX () + 100;

            if (display_hourglass_p)
                cancel_hourglass ();

            debug_on_next_call = false;
            when_entered_debugger = num_nonmacro_input_events;

            /* Resetting redisplaying_p to 0 makes sure that debug output is
               displayed if the debugger is invoked during redisplay.  */
            bool debug_while_redisplaying = redisplaying_p;
            redisplaying_p = false;
            specbind (intern ("debugger-may-continue"),
                      debug_while_redisplaying ? Q.nil : Q.t);
            specbind (Q.inhibit_redisplay, Q.nil);
            specbind (Q.debug_on_error, Q.nil);

            LispObject val = apply1 (V.debugger, arg);

            /* Interrupting redisplay and resuming it later is not safe under
               all circumstances.  So, when the debugger returns, abort the
               interrupted redisplay by going back to the top-level.  */
            if (debug_while_redisplaying)
                F.top_level ();

            return unbind_to (count, val);
        }

        public static void do_debug_on_call(LispObject code)
        {
            debug_on_next_call = false;
            backtrace_list.debug_on_exit = true;
            call_debugger(F.cons(code, Q.nil));
        }

        public static void specbind(LispObject symbol, LispObject value)
        {
            CHECK_SYMBOL(symbol);

            /* The most common case is that of a non-constant symbol with a
               trivial value.  Make that as fast as we can.  */
            LispObject valcontents = SYMBOL_VALUE(symbol);
            if (!MISCP(valcontents) && !SYMBOL_CONSTANT_P(symbol))
            {
                specbinding sb = new specbinding(symbol, valcontents, null);
                specpdl.Push(sb);
                SET_SYMBOL_VALUE(symbol, value);
            }
            else
            {
                LispObject ovalue = find_symbol_value(symbol);
                specbinding sb = new specbinding();
                sb.func = null;
                sb.old_value = ovalue;

                valcontents = XSYMBOL(symbol).Value;

                if (BUFFER_LOCAL_VALUEP(valcontents) || BUFFER_OBJFWDP(valcontents))
                {
                    LispObject where;

                    LispObject current_buffer = F.current_buffer();

                    /* For a local variable, record both the symbol and which
                       buffer's or frame's value we are saving.  */
                    if (!NILP(F.local_variable_p(symbol, Q.nil)))
                        where = current_buffer;
                    else if (BUFFER_LOCAL_VALUEP(valcontents) && XBUFFER_LOCAL_VALUE(valcontents).found_for_frame)
                        where = XBUFFER_LOCAL_VALUE(valcontents).frame;
                    else
                        where = Q.nil;

                    /* We're not using the `unused' slot in the specbinding
                       structure because this would mean we have to do more
                       work for simple variables.  */
                    sb.symbol = F.cons(symbol, F.cons(where, current_buffer));

                    /* If SYMBOL is a per-buffer variable which doesn't have a
                       buffer-local value here, make the `let' change the global
                       value by changing the value of SYMBOL in all buffers not
                       having their own value.  This is consistent with what
                       happens with other buffer-local variables.  */
                    if (NILP(where) && BUFFER_OBJFWDP(valcontents))
                    {
                        specpdl.Push(sb);
                        F.set_default(symbol, value);
                        return;
                    }
                }
                else
                {
                    sb.symbol = symbol;
                }

                specpdl.Push(sb);
                set_internal(symbol, value, null, true);
            }
        }

        public static void record_unwind_protect(subr1 function, LispObject arg)
        {
            specbinding sb = new specbinding();
            sb.func = function;
            sb.symbol = Q.nil;
            sb.old_value = arg;

            specpdl.Push(sb);
        }

        public static LispObject unbind_to(int count, LispObject value)
        {
            LispObject quitf = V.quit_flag;
            V.quit_flag = Q.nil;

            while (specpdl.Count != count)
            {
                /* Copy the binding, and decrement specpdl_ptr, before we do
	               the work to unbind it.  We decrement first
	               so that an error in unbinding won't try to unbind
	               the same entry again, and we copy the binding first
	               in case more bindings are made during some of the code we run.  */

                specbinding this_binding = specpdl.Pop();

                if (this_binding.func != null)
                    this_binding.func(this_binding.old_value);

                    /* If the symbol is a list, it is really (SYMBOL WHERE
                 	   . CURRENT-BUFFER) where WHERE is either nil, a buffer, or a
	                   frame.  If WHERE is a buffer or frame, this indicates we
	                   bound a variable that had a buffer-local or frame-local
	                   binding.  WHERE nil means that the variable had the default
	                   value when it was bound.  CURRENT-BUFFER is the buffer that
                       was current when the variable was bound.  */
                else if (CONSP (this_binding.symbol))
                {
                    LispObject symbol, where;

                    symbol = XCAR(this_binding.symbol);
                    where = XCAR(XCDR(this_binding.symbol));

                    if (NILP(where))
                        F.set_default(symbol, this_binding.old_value);
                    else if (BUFFERP(where))
                        set_internal(symbol, this_binding.old_value, XBUFFER(where), true);
                    else
                        set_internal(symbol, this_binding.old_value, null, true);
                }
                else
                {
                    /* If variable has a trivial value (no forwarding), we can
                       just set it.  No need to check for constant symbols here,
                       since that was already done by specbind.  */
                    if (!MISCP(SYMBOL_VALUE(this_binding.symbol)))
                        SET_SYMBOL_VALUE(this_binding.symbol, this_binding.old_value);
                    else
                        set_internal(this_binding.symbol, this_binding.old_value, null, true);
                }
            }

            if (NILP(V.quit_flag) && !NILP(quitf))
                V.quit_flag = quitf;

            return value;
        }

        public static LispObject internal_catch(LispObject tag, subr1 func, LispObject arg)
        {
            catchlist.Push(new catchtag(tag, backtrace_list, handlerlist, lisp_eval_depth, SPECPDL_INDEX(), byte_stack_list));
            try
            {
                return func(arg);
            }
            catch (LispCatch lc)
            {
                catchtag ct = catchlist.Peek();

                unbind_to(ct.pdlcount, Q.nil);
                handlerlist = ct.handlerlist;

                // make sure it's the catch we are expecting
                if (lc.CatchTag != ct)
                    throw;

                byte_stack_list = ct.byte_stack;
                backtrace_list  = ct.backlist;
                lisp_eval_depth = ct.lisp_eval_depth;

                return lc.Value;
            }
            finally
            {
                catchlist.Pop();
            }
        }

        public static void unwind_to_catch(catchtag catchtag, LispObject value)
        {
            immediate_quit = false;
            throw new LispCatch(catchtag, value);
            // the meat of the original unwind_to_catch is in catch body of internal_xxx
        }

        /* Internal version of Fsignal that never returns.
           Used for anything but Qquit (which can return from Fsignal).  */
        public static void xsignal(LispObject error_symbol, LispObject data)
        {
            F.signal(error_symbol, data);
            abort();
        }

        /* Like xsignal, but takes 0, 1, 2, or 3 args instead of a list.  */
        public static void xsignal0(LispObject error_symbol)
        {
            xsignal(error_symbol, Q.nil);
        }

        public static void xsignal1(LispObject error_symbol, LispObject arg)
        {
            xsignal(error_symbol, list1(arg));
        }

        public static void xsignal2(LispObject error_symbol, LispObject arg1, LispObject arg2)
        {
            xsignal(error_symbol, list2(arg1, arg2));
        }

        public static void xsignal3(LispObject error_symbol, LispObject arg1, LispObject arg2, LispObject arg3)
        {
            xsignal(error_symbol, list3(arg1, arg2, arg3));
        }

        /* Return nonzero if LIST is a non-nil atom or
           a list containing one of CONDITIONS.  */

        public static bool wants_debugger (LispObject list, LispObject conditions)
        {
            if (NILP (list))
                return false;
            
            if (! CONSP (list))
                return true;

            while (CONSP (conditions))
            {
                LispObject cur = XCAR (conditions);
                for (LispObject tail = list; CONSP (tail); tail = XCDR (tail))
                    if (EQ (XCAR (tail), cur))
                        return true;
                conditions = XCDR (conditions);
            }
            return true;
        }

        /* Return 1 if an error with condition-symbols CONDITIONS,
           and described by SIGNAL-DATA, should skip the debugger
           according to debugger-ignored-errors.  */

        public static bool skip_debugger (LispObject conditions, LispObject data)
        {
            bool first_string = true;
            LispObject error_message = Q.nil; 

            for (LispObject tail = V.debug_ignored_errors; CONSP (tail); tail = XCDR (tail))
            {
                if (STRINGP (XCAR (tail)))
                {
                    if (first_string)
                    {
                        error_message = F.error_message_string (data);
                        first_string = false;
                    }

                    if (fast_string_match (XCAR (tail), error_message) >= 0)
                        return true;
                }
                else
                {
                    for (LispObject contail = conditions; CONSP (contail); contail = XCDR (contail))
                        if (EQ (XCAR (tail), XCAR (contail)))
                            return true;
                }
            }

            return false;
        }

        /* Call the debugger if calling it is currently enabled for CONDITIONS.
           SIG and DATA describe the signal, as in find_handler_clause.  */
        public static bool maybe_call_debugger (LispObject conditions, LispObject sig, LispObject data)
        {
            LispObject combined_data = F.cons (sig, data);

            if ((EQ (sig, Q.quit) ? debug_on_quit : wants_debugger (V.debug_on_error, conditions))
                && ! skip_debugger (conditions, combined_data)
                /* rms: what's this for? */
                && when_entered_debugger < num_nonmacro_input_events)
            {
                call_debugger (F.cons (Q.error, F.cons (combined_data, Q.nil)));
                return true;
            }   

            return false;
        }

        /* Value of Qlambda means we have called debugger and user has continued.
           There are two ways to pass SIG and DATA:
           = SIG is the error symbol, and DATA is the rest of the data.
           = SIG is nil, and DATA is (SYMBOL . REST-OF-DATA).
           This is for memory-full errors only.

           We need to increase max_specpdl_size temporarily around
           anything we do that can push on the specpdl, so as not to get
           a second error here in case we're handling specpdl overflow.  */

        public static LispObject find_handler_clause (LispObject handlers, LispObject conditions, LispObject sig, LispObject data)
        {
            bool debugger_called = false; 
            bool debugger_considered = false;

            /* t is used by handlers for all conditions, set up by C code.  */
            if (EQ (handlers, Q.t))
                return Q.t;

            /* Don't run the debugger for a memory-full error.
               (There is no room in memory to do that!)  */
            if (NILP (sig))
                debugger_considered = true;

            /* error is used similarly, but means print an error message
               and run the debugger if that is enabled.  */
            if (EQ (handlers, Q.error)
                || !NILP (V.debug_on_signal)) /* This says call debugger even if
                                                there is a handler.  */
            {
                if (!NILP (sig) && wants_debugger (V.stack_trace_on_error, conditions))
                {
                    max_lisp_eval_depth += 15;
                    max_specpdl_size++;

                    internal_with_output_to_temp_buffer ("*Backtrace*", F.backtrace);

                    max_specpdl_size--;
                    max_lisp_eval_depth -= 15;
                }

                if (!debugger_considered)
                {
                    debugger_considered = true;
                    debugger_called = maybe_call_debugger (conditions, sig, data);
                }

                /* If there is no handler, return saying whether we ran the debugger.  */
                if (EQ (handlers, Q.error))
                {
                    if (debugger_called)
                        return Q.lambda;
                    return Q.t;
                }
            }

            for (LispObject h = handlers; CONSP (h); h = F.cdr (h))
            {
                LispObject handler = F.car (h);
                if (!CONSP (handler))
                    continue;
                
                LispObject condit = F.car (handler);
                /* Handle a single condition name in handler HANDLER.  */
                if (SYMBOLP (condit))
                {
                    LispObject tem = F.memq (F.car (handler), conditions);
                    if (!NILP (tem))
                        return handler;
                }
                /* Handle a list of condition names in handler HANDLER.  */
                else if (CONSP (condit))
                {
                    for (LispObject tail = condit; CONSP (tail); tail = XCDR (tail))
                    {
                        LispObject tem = F.memq(F.car(tail), conditions);
                        if (!NILP (tem))
                        {
                            /* This handler is going to apply.
                               Does it allow the debugger to run first?  */
                            if (! debugger_considered && !NILP (F.memq (Q.debug, condit)))
                                maybe_call_debugger (conditions, sig, data);
                            return handler;
                        }
                    }
                }
            }

            return Q.nil;
        }

        public static LispObject un_autoload(LispObject oldqueue)
        {
            LispObject queue, first, second;

            /* Queue to unwind is current value of Vautoload_queue.
               oldqueue is the shadowed value to leave in Vautoload_queue.  */
            queue = V.autoload_queue;
            V.autoload_queue = oldqueue;
            while (CONSP(queue))
            {
                first = XCAR(queue);
                second = F.cdr(first);
                first = F.car(first);
                if (EQ(first, make_number(0)))
                    V.features = second;
                else
                    F.fset(first, second);
                queue = XCDR(queue);
            }
            return Q.nil;
        }

        /* Load an autoloaded function.
           FUNNAME is the symbol which is the function's name.
           FUNDEF is the autoload definition (a list).  */
        public static void do_autoload(LispObject fundef, LispObject funname)
        {
            int count = SPECPDL_INDEX();
            LispObject fun;

            /* This is to make sure that loadup.el gives a clear picture
               of what files are preloaded and when.  */
            //if (!NILP(Vpurify_flag))
            //error("Attempt to autoload %s while preparing to dump", SDATA(SYMBOL_NAME(funname)));

            fun = funname;
            CHECK_SYMBOL(funname);

            /* Preserve the match data.  */
            record_unwind_save_match_data();

            /* If autoloading gets an error (which includes the error of failing
               to define the function being called), we use Vautoload_queue
               to undo function definitions and `provide' calls made by
               the function.  We do this in the specific case of autoloading
               because autoloading is not an explicit request "load this file",
               but rather a request to "call this function".
     
               The value saved here is to be restored into Vautoload_queue.  */
            record_unwind_protect(un_autoload, V.autoload_queue);
            V.autoload_queue = Q.t;
            F.load(F.car(F.cdr(fundef)), Q.nil, Q.t, Q.nil, Q.t);

            /* Once loading finishes, don't undo it.  */
            V.autoload_queue = Q.t;
            unbind_to(count, Q.nil);

            fun = F.indirect_function(fun, Q.nil);

            if (!NILP(F.equal(fun, fundef)))
                error("Autoloading failed to define function %s", SDATA(SYMBOL_NAME(funname)));
        }

        public static void error(string m, params object[] args)
        {
            // COMEBACK_WHEN_READY!!! (doprnt1)
            xsignal1(Q.error, make_string(m));
        }
    }

    public partial class F
    {
        public static LispObject progn(LispObject args)
        {
            LispObject val = Q.nil;

            while (L.CONSP(args))
            {
                val = eval(L.XCAR(args));
                args = L.XCDR(args);
            }

            return val;
        }

        public static LispObject lisp_throw (LispObject tag, LispObject value)
        {
            if (!L.NILP (tag))
            {
                foreach (catchtag c in L.catchlist)
                {
                    if (L.EQ(c.tag, tag))
                        L.unwind_to_catch(c, value);
                }
            }
            L.xsignal2(Q.no_catch, tag, value);
            return Q.nil;
        }

        public static LispObject signal(LispObject error_symbol, LispObject data)
        {
            /* When memory is full, ERROR-SYMBOL is nil,
               and DATA is (REAL-ERROR-SYMBOL . REAL-DATA).
               That is a special case--don't do this in other situations.  */

            Handler allhandlers = L.handlerlist;

            LispObject real_error_symbol;

            L.immediate_quit = false;
            if (L.waiting_for_input)
                L.abort();

            if (L.NILP(error_symbol))
                real_error_symbol = F.car(data);
            else
                real_error_symbol = error_symbol;

            /* This hook is used by edebug.  */
            if (!L.NILP(V.signal_hook_function)
                && !L.NILP(error_symbol))
            {
                /* Edebug takes care of restoring these variables when it exits.  */
                if (L.lisp_eval_depth + 20 > L.max_lisp_eval_depth)
                    L.max_lisp_eval_depth = L.lisp_eval_depth + 20;

                // we don't much care about max pdl size, or do we?
                if (L.SPECPDL_INDEX() + 40 > L.max_specpdl_size)
                    L.max_specpdl_size = L.SPECPDL_INDEX() + 40;

                L.call2(V.signal_hook_function, error_symbol, data);
            }

            LispObject conditions = F.get(real_error_symbol, Q.error_conditions);

            /* Remember from where signal was called.  Skip over the frame for
               `signal' itself.  If a frame for `error' follows, skip that,
               too.  Don't do this when ERROR_SYMBOL is nil, because that
               is a memory-full error.  */
            V.signaling_function = Q.nil;
            if (L.backtrace_list != null && !L.NILP(error_symbol))
            {
                backtrace bp = L.backtrace_list.next;
                if (bp != null && bp.function != null && bp.function == Q.error)
                    bp = bp.next;
                if (bp != null && bp.function != null)
                    V.signaling_function = bp.function;
            }

            for (; L.handlerlist != null; L.handlerlist = L.handlerlist.next)
            {
                LispObject clause = L.find_handler_clause(L.handlerlist.handler, conditions,
                                              error_symbol, data);

                if (L.EQ(clause, Q.lambda))
                {
                    /* We can't return values to code which signaled an error, but we
                       can continue code which has signaled a quit.  */
                    if (L.EQ(real_error_symbol, Q.quit))
                        return Q.nil;
                    else
                        L.error("Cannot return from the debugger in an error");
                }

                if (!L.NILP(clause))
                {
                    LispObject unwind_data;
                    Handler h = L.handlerlist;

                    L.handlerlist = allhandlers;

                    if (L.NILP(error_symbol))
                        unwind_data = data;
                    else
                        unwind_data = F.cons(error_symbol, data);
                    h.chosen_clause = clause;
                    L.unwind_to_catch(h.tag, unwind_data);
                }
            }

            L.handlerlist = allhandlers;
            /* If no handler is present now, try to run the debugger,
               and if that fails, throw to top level.  */
            L.find_handler_clause(Q.error, conditions, error_symbol, data);
            if (L.catchlist.Count != 0)
                F.lisp_throw(Q.top_level, Q.t);

            if (!L.NILP(error_symbol))
                data = F.cons(error_symbol, data);

            LispObject error_string = F.error_message_string(data);
            L.fatal("%s", L.SDATA(error_string), 0);
            return Q.nil;
        }

        public static LispObject apply (int nargs, params LispObject[] args)
        {
            int i;
            int nvars = 0;
            LispObject[] funcall_args = null;

            LispObject fun = args [0];
            LispObject spread_arg = args [nargs - 1];
            L.CHECK_LIST (spread_arg);

            int numargs = L.XINT (F.length (spread_arg));

            if (numargs == 0)
            {
                return F.funcall (nargs - 1, args);
            }
            else if (numargs == 1)
            {
                args [nargs - 1] = L.XCAR (spread_arg);
                return F.funcall (nargs, args);
            }

            numargs += nargs - 2;

            /* Optimize for no indirection.  */
            if (L.SYMBOLP(fun) && !L.EQ(fun, Q.unbound))
            {
                fun = L.XSYMBOL(fun).Function;
                if (L.SYMBOLP(fun))
                    fun = L.indirect_function(fun);
            }
            if (L.EQ (fun, Q.unbound))
            {
                /* Let funcall get the error */
                fun = args[0];
                goto funcall;
            }

            if (L.SUBRP (fun))
            {
                if (numargs < L.XSUBR (fun).min_args
                    || (L.XSUBR (fun).max_args >= 0 && L.XSUBR (fun).max_args < numargs))
                    goto funcall;		/* Let funcall get the error */
                else if (L.XSUBR (fun).max_args > numargs)
                {
                    /* Avoid making funcall cons up a yet another new vector of arguments
                       by explicitly supplying nil's for optional values */
                    funcall_args = new LispObject[1 + L.XSUBR(fun).max_args];
                    for (i = numargs; i < L.XSUBR (fun).max_args;)
                        funcall_args[++i] = Q.nil;
                    nvars = 1 + L.XSUBR (fun).max_args;
                }
            }
            funcall:
            /* We add 1 to numargs because funcall_args includes the
               function itself as well as its arguments.  */
            if (funcall_args == null)
            {
                funcall_args = new LispObject[1 + numargs];
                nvars = 1 + numargs;
            }

            System.Array.Copy(args, funcall_args, nargs);

            /* Spread the last arg we got.  Its first element goes in
               the slot that it used to occupy, hence this value of I.  */
            i = nargs - 1;
            while (!L.NILP (spread_arg))
            {
                funcall_args [i++] = L.XCAR (spread_arg);
                spread_arg = L.XCDR (spread_arg);
            }

            return (F.funcall (nvars, funcall_args));
        }

        public static LispObject funcall (int nargs, params LispObject[] args)
        {
            int numargs = nargs - 1;
            LispObject val = null;
            LispObject[] internal_args;

            L.QUIT ();

            if (++L.lisp_eval_depth > L.max_lisp_eval_depth)
            {
                if (L.max_lisp_eval_depth < 100)
                    L.max_lisp_eval_depth = 100;
                if (L.lisp_eval_depth > L.max_lisp_eval_depth)
                    L.error ("Lisp nesting exceeds `max-lisp-eval-depth'");
            }

            backtrace backtrace = new backtrace ();
            backtrace.next = L.backtrace_list;
            L.backtrace_list = backtrace;
            backtrace.function = args[0];
            backtrace.args = new LispObject[args.Length - 1];
            if (args.Length - 1 > 0)
            {
                System.Array.Copy(args, 1, backtrace.args, 0, args.Length - 1);
            }
            backtrace.nargs = nargs - 1;
            backtrace.evalargs = false;
            backtrace.debug_on_exit = false;

            if (L.debug_on_next_call)
                L.do_debug_on_call (Q.lambda);

            LispObject original_fun = args[0];

        retry:

            /* Optimize for no indirection.  */
            LispObject fun = original_fun;
            if (L.SYMBOLP (fun) && !L.EQ (fun, Q.unbound)
                && (fun = L.XSYMBOL (fun).Function) != null && L.SYMBOLP (fun))
                fun = L.indirect_function (fun);

            if (L.SUBRP (fun))
            {
                if (numargs < L.XSUBR (fun).min_args
                    || (L.XSUBR (fun).max_args >= 0 && L.XSUBR (fun).max_args < numargs))
                {
                    L.xsignal2 (Q.wrong_number_of_arguments, original_fun, L.make_number(numargs));
                }

                if (L.XSUBR (fun).max_args == LispSubr.UNEVALLED)
                    L.xsignal1 (Q.invalid_function, original_fun);

                if (L.XSUBR (fun).max_args == LispSubr.MANY)
                {
                    // FIXME: use own copy of args and not abuse backtrace?
                    val = L.XSUBR(fun).function_many (numargs, backtrace.args);
                    goto done;
                }

                if (L.XSUBR(fun).max_args > numargs)
                {
                    internal_args = (LispObject[])System.Array.CreateInstance(typeof(LispObject), L.XSUBR(fun).max_args);
                    System.Array.Copy(args, 1, internal_args, 0, numargs);
                    for (int i = numargs; i < L.XSUBR(fun).max_args; i++)
                        internal_args[i] = Q.nil;
                }
                else
                {
                    // internal_args = (LispObject[])System.Array.CreateInstance(typeof(LispObject), L.XSUBR(fun).max_args);
                    // System.Array.Copy(args, 1, internal_args, 0, numargs);
                    internal_args = backtrace.args;
                }
                switch (L.XSUBR (fun).max_args)
                {
                case 0:
                    val = L.XSUBR(fun).function0 ();
                    goto done;
                case 1:
                    val = L.XSUBR(fun).function1 (internal_args[0]);
                    goto done;
                case 2:
                    val = L.XSUBR(fun).function2 (internal_args[0], internal_args[1]);
                    goto done;
                case 3:
                    val = L.XSUBR(fun).function3 (internal_args[0], internal_args[1],
                                                    internal_args[2]);
                    goto done;
                case 4:
                    val = L.XSUBR(fun).function4 (internal_args[0], internal_args[1],
                                                    internal_args[2], internal_args[3]);
                    goto done;
                case 5:
                    val = L.XSUBR(fun).function5 (internal_args[0], internal_args[1],
                                                    internal_args[2], internal_args[3],
                                                    internal_args[4]);
                    goto done;
                case 6:
                    val = L.XSUBR(fun).function6 (internal_args[0], internal_args[1],
                                                    internal_args[2], internal_args[3],
                                                    internal_args[4], internal_args[5]);
                    goto done;
                case 7:
                    val = L.XSUBR(fun).function7 (internal_args[0], internal_args[1],
                                                    internal_args[2], internal_args[3],
                                                    internal_args[4], internal_args[5],
                                                    internal_args[6]);
                    goto done;

                case 8:
                    val = L.XSUBR(fun).function8 (internal_args[0], internal_args[1],
                                                    internal_args[2], internal_args[3],
                                                    internal_args[4], internal_args[5],
                                                    internal_args[6], internal_args[7]);
                    goto done;

                default:

                    /* If a subr takes more than 8 arguments without using MANY
                       or UNEVALLED, we need to extend this function to support it.
                       Until this is done, there is no way to call the function.  */
                    L.abort();
                    break;
                }
            }

            if (L.COMPILEDP (fun))
                val = L.funcall_lambda (fun, numargs, backtrace.args);
            else
            {
                if (L.EQ (fun, Q.unbound))
                    L.xsignal1 (Q.void_function, original_fun);

                if (!L.CONSP (fun))
                    L.xsignal1 (Q.invalid_function, original_fun);

                LispObject funcar = L.XCAR(fun);
                if (!L.SYMBOLP (funcar))
                    L.xsignal1 (Q.invalid_function, original_fun);

                if (L.EQ (funcar, Q.lambda))
                    val = L.funcall_lambda (fun, numargs, backtrace.args);
                else if (L.EQ (funcar, Q.autoload))
                {
                    L.do_autoload (fun, original_fun);
                    goto retry;
                }
                else
                    L.xsignal1 (Q.invalid_function, original_fun);
            }
        done:
            L.lisp_eval_depth--;

            if (backtrace.debug_on_exit)
                val = L.call_debugger (F.cons (Q.exit, F.cons (val, Q.nil)));

            L.backtrace_list = backtrace.next;
            return val;
        }

        public static LispObject eval (LispObject form)
        {
            LispObject fun, val = null, original_fun, original_args;
            LispObject funcar;
            backtrace backtrace = new backtrace();

            if (L.SYMBOLP(form))
                return symbol_value(form);
            if (!L.CONSP(form))
                return form;

            L.QUIT();

            if (++L.lisp_eval_depth > L.max_lisp_eval_depth)
            {
                if (L.max_lisp_eval_depth < 100)
                    L.max_lisp_eval_depth = 100;
                if (L.lisp_eval_depth > L.max_lisp_eval_depth)
                    L.error("Lisp nesting exceeds `max-lisp-eval-depth'");
            }

            original_fun = F.car(form);
            original_args = F.cdr(form);

            backtrace.next = L.backtrace_list;
            L.backtrace_list = backtrace;
            backtrace.function = original_fun; /* This also protects them from gc */
            backtrace.args = new LispObject[] {original_args};
            backtrace.nargs = LispSubr.UNEVALLED;
            backtrace.evalargs = true;
            backtrace.debug_on_exit = false;

            if (L.debug_on_next_call)
                L.do_debug_on_call(Q.t);

            /* At this point, only original_fun and original_args
               have values that will be used below */
        retry:

            /* Optimize for no indirection.  */
            fun = original_fun;
            if (L.SYMBOLP(fun) && !L.EQ(fun, Q.unbound))
            {
                fun = L.XSYMBOL(fun).function;
                if (L.SYMBOLP(fun))
                    fun = L.indirect_function(fun);
            }

            if (L.SUBRP(fun))
            {
                LispObject[] argvals = new LispObject[8];
                int i, maxargs;

                LispObject args_left = original_args;
                LispObject numargs = F.length(args_left);

                if (L.XINT(numargs) < L.XSUBR(fun).min_args ||
                    (L.XSUBR(fun).max_args >= 0 && L.XSUBR(fun).max_args < L.XINT(numargs)))
                    L.xsignal2(Q.wrong_number_of_arguments, original_fun, numargs);

                if (L.XSUBR(fun).max_args == LispSubr.UNEVALLED)
                {
                    backtrace.evalargs = false;
                    val = L.XSUBR(fun).function1(args_left);
                    goto done;
                }

                if (L.XSUBR(fun).max_args == LispSubr.MANY)
                {
                    /* Pass a vector of evaluated arguments */
                    int argnum = 0;

                    LispObject[] vals = new LispObject[L.XINT(numargs)];

                    while (!L.NILP(args_left))
                    {
                        vals[argnum++] = F.eval(F.car(args_left));
                        args_left = F.cdr(args_left);
                    }

                    backtrace.args = vals;
                    backtrace.nargs = L.XINT(numargs);

                    val = L.XSUBR(fun).function_many(L.XINT(numargs), vals);
                    goto done;
                }

                maxargs = L.XSUBR(fun).max_args;
                for (i = 0; i < maxargs; args_left = F.cdr(args_left))
                {
                    argvals[i] = F.eval(F.car(args_left));
                    ++i;
                }

                backtrace.args = argvals;
                backtrace.nargs = L.XINT(numargs);

                switch (i)
                {
                    case 0:
                        val = L.XSUBR(fun).function0();
                        goto done;
                    case 1:
                        val = L.XSUBR(fun).function1(argvals[0]);
                        goto done;
                    case 2:
                        val = L.XSUBR(fun).function2(argvals[0], argvals[1]);
                        goto done;
                    case 3:
                        val = L.XSUBR(fun).function3(argvals[0], argvals[1],
                                                        argvals[2]);
                        goto done;
                    case 4:
                        val = L.XSUBR(fun).function4(argvals[0], argvals[1],
                                                        argvals[2], argvals[3]);
                        goto done;
                    case 5:
                        val = L.XSUBR(fun).function5(argvals[0], argvals[1], argvals[2],
                                                        argvals[3], argvals[4]);
                        goto done;
                    case 6:
                        val = L.XSUBR(fun).function6(argvals[0], argvals[1], argvals[2],
                                                        argvals[3], argvals[4], argvals[5]);
                        goto done;
                    case 7:
                        val = L.XSUBR(fun).function7(argvals[0], argvals[1], argvals[2],
                                                        argvals[3], argvals[4], argvals[5],
                                                        argvals[6]);
                        goto done;

                    case 8:
                        val = L.XSUBR(fun).function8(argvals[0], argvals[1], argvals[2],
                                                        argvals[3], argvals[4], argvals[5],
                                                        argvals[6], argvals[7]);
                        goto done;

                    default:
                        /* Someone has created a subr that takes more arguments than
                           is supported by this code.  We need to either rewrite the
                           subr to use a different argument protocol, or add more
                           cases to this switch.  */
                        L.abort();
                        break;
                }
            }
            if (L.COMPILEDP(fun))
                val = L.apply_lambda(fun, original_args, true);
            else
            {
                if (L.EQ(fun, Q.unbound))
                    L.xsignal1(Q.void_function, original_fun);
                if (!L.CONSP(fun))
                    L.xsignal1(Q.invalid_function, original_fun);
                funcar = L.XCAR(fun);
                if (!L.SYMBOLP(funcar))
                    L.xsignal1(Q.invalid_function, original_fun);
                if (L.EQ(funcar, Q.autoload))
                {
                    L.do_autoload(fun, original_fun);
                    goto retry;
                }
                if (L.EQ(funcar, Q.macro))
                    val = F.eval(L.apply1(F.cdr(fun), original_args));
                else if (L.EQ(funcar, Q.lambda))
                    val = L.apply_lambda(fun, original_args, true);
                else
                    L.xsignal1(Q.invalid_function, original_fun);
            }
        done:

            L.lisp_eval_depth--;
            if (backtrace.debug_on_exit)
                val = L.call_debugger(F.cons(Q.exit, F.cons(val, Q.nil)));
            L.backtrace_list = backtrace.next;

            return val;
        }

        public static LispObject fetch_bytecode (LispObject obj)
        {
            if (L.COMPILEDP (obj) && L.CONSP (L.AREF (obj, LispCompiled.COMPILED_BYTECODE)))
            {
                LispObject tem = L.read_doc_string(L.AREF(obj, LispCompiled.COMPILED_BYTECODE));
                if (!L.CONSP (tem))
                {
                    tem = L.AREF (obj, LispCompiled.COMPILED_BYTECODE);
                    if (L.CONSP (tem) && L.STRINGP (L.XCAR (tem)))
                        L.error("Invalid byte code in %s", L.SDATA(L.XCAR(tem)));
                    else
                        L.error("Invalid byte code");
                }
                L.ASET (obj, LispCompiled.COMPILED_BYTECODE, L.XCAR (tem));
                L.ASET (obj, LispCompiled.COMPILED_CONSTANTS, L.XCDR (tem));
            }
            return obj;
        }

        public static LispObject backtrace_debug(LispObject level, LispObject flag)
        {
            backtrace backlist = L.backtrace_list;

            L.CHECK_NUMBER(level);

            for (int i = 0; backlist != null && i < L.XINT(level); i++)
            {
                backlist = backlist.next;
            }

            if (backlist != null)
                backlist.debug_on_exit = !L.NILP(flag);

            return flag;
        }

        public static LispObject backtrace()
        {
            backtrace backlist = L.backtrace_list;
            int i;
            LispObject tail;
            LispObject tem;

            V.print_level = L.make_number(3);

            tail = Q.nil;

            while (backlist != null)
            {
                L.write_string(backlist.debug_on_exit ? "* " : "  ", 2);
                if (backlist.nargs == LispSubr.UNEVALLED)
                {
                    F.prin1(F.cons(backlist.function, backlist.args[0]), Q.nil);
                    L.write_string("\n", -1);
                }
                else
                {
                    tem = backlist.function;
                    F.prin1(tem, Q.nil);	/* This can QUIT */
                    L.write_string("(", -1);
                    if (backlist.nargs == LispSubr.MANY)
                    {
                        for (tail = backlist.args[0], i = 0; !L.NILP(tail); tail = F.cdr(tail), i++)
                        {
                            if (i > 0)
                                L.write_string(" ", -1);
                            F.prin1(F.car(tail), Q.nil);
                        }
                    }
                    else
                    {
                        for (i = 0; i < backlist.nargs; i++)
                        {
                            if (i > 0)
                                L.write_string(" ", -1);
                            F.prin1(backlist.args[i], Q.nil);
                        }
                    }
                    L.write_string(")\n", -1);
                }
                backlist = backlist.next;
            }

            V.print_level = Q.nil;
            return Q.nil;
        }
    }

    public partial class L
    {
        /* Apply fn to arg */
        public static LispObject apply1(LispObject fn, LispObject arg)
        {
            if (NILP(arg))
                return (F.funcall(1, fn));

            return F.apply(2, fn, arg);
        }

        /* Call function fn on no arguments */
        public static LispObject call0(LispObject fn)
        {
            return (F.funcall(1, fn));
        }

        /* Call function fn with 1 argument arg1 */
        /* ARGSUSED */
        public static LispObject call1(LispObject fn, LispObject arg1)
        {
            return (F.funcall(2, fn, arg1));
        }

        /* Call function fn with 2 arguments arg1, arg2 */
        /* ARGSUSED */
        public static LispObject call2(LispObject fn, LispObject arg1, LispObject arg2)
        {
            return F.funcall(3, fn, arg1, arg2);
        }

        /* Call function fn with 3 arguments arg1, arg2, arg3 */
        /* ARGSUSED */
        public static LispObject call3(LispObject fn, LispObject arg1, LispObject arg2, LispObject arg3)
        {
            return F.funcall(4, fn, arg1, arg2, arg3);
        }

        /* Call function fn with 4 arguments arg1, arg2, arg3, arg4 */
        /* ARGSUSED */
        public static LispObject call4(LispObject fn, LispObject arg1, LispObject arg2, LispObject arg3, LispObject arg4)
        {
            return F.funcall(5, fn, arg1, arg2, arg3, arg4);
        }

        /* Call function fn with 5 arguments arg1, arg2, arg3, arg4, arg5 */
        /* ARGSUSED */
        public static LispObject call5(LispObject fn, LispObject arg1, LispObject arg2, LispObject arg3, LispObject arg4, LispObject arg5)
        {
            return F.funcall(6, fn, arg1, arg2, arg3, arg4, arg5);
        }

        /* Call function fn with 6 arguments arg1, arg2, arg3, arg4, arg5, arg6 */
        /* ARGSUSED */
        public static LispObject call6(LispObject fn, LispObject arg1, LispObject arg2, LispObject arg3, LispObject arg4, LispObject arg5, LispObject arg6)
        {
            return F.funcall(7, fn, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        public static LispObject apply_lambda(LispObject fun, LispObject args, bool eval_flag)
        {
            int i;
            LispObject tem;

            LispObject numargs = F.length(args);
            LispObject[] arg_vector = new LispObject[XINT(numargs)];
            LispObject args_left = args;

            for (i = 0; i < XINT(numargs); )
            {
                tem = F.car(args_left);
                args_left = F.cdr(args_left);
                if (eval_flag)
                    tem = F.eval(tem);
                arg_vector[i++] = tem;
            }

            if (eval_flag)
            {
                backtrace_list.args = arg_vector;
                backtrace_list.nargs = i;
            }
            backtrace_list.evalargs = false;
            tem = funcall_lambda(fun, XINT(numargs), arg_vector);

            /* Do the debug-on-exit now, while arg_vector still exists.  */
            if (backtrace_list.debug_on_exit)
                tem = call_debugger(F.cons(Q.exit, F.cons(tem, Q.nil)));
            /* Don't do it again when we return to eval.  */
            backtrace_list.debug_on_exit = false;
            return tem;
        }

        /* Apply a Lisp function FUN to the NARGS evaluated arguments in ARG_VECTOR
           and return the result of evaluation.
           FUN must be either a lambda-expression or a compiled-code object.  */
        public static LispObject funcall_lambda (LispObject fun, int nargs, LispObject[] arg_vector)
        {
            LispObject val, syms_left, next;
            int count = SPECPDL_INDEX ();

            if (CONSP (fun))
            {
                syms_left = XCDR (fun);
                if (CONSP (syms_left))
                    syms_left = XCAR (syms_left);
                else
                    xsignal1 (Q.invalid_function, fun);
            }
            else if (COMPILEDP(fun))
                syms_left = AREF(fun, LispCompiled.COMPILED_ARGLIST);
            else
            {
                abort();
                return Q.nil;
            }

            int i = 0;
            bool optional = false;
            bool rest = false;
            for (; CONSP (syms_left); syms_left = XCDR (syms_left))
            {
                QUIT();

                next = XCAR (syms_left);
                if (!SYMBOLP (next))
                    xsignal1 (Q.invalid_function, fun);

                if (EQ (next, Q.and_rest))
                    rest = true;
                else if (EQ (next, Q.and_optional))
                    optional = true;
                else if (rest)
                {
                    LispObject[] temp = new LispObject[nargs - i];
                    System.Array.Copy(arg_vector, i, temp, 0, nargs - i);
                    specbind (next, F.list (nargs - i, temp));
                    i = nargs;
                }
                else if (i < nargs)
                    specbind (next, arg_vector[i++]);
                else if (!optional)
                    xsignal2 (Q.wrong_number_of_arguments, fun, make_number (nargs));
                else
                    specbind (next, Q.nil);
            }

            if (!NILP (syms_left))
                xsignal1 (Q.invalid_function, fun);
            else if (i < nargs)
                xsignal2 (Q.wrong_number_of_arguments, fun, make_number (nargs));

            if (CONSP (fun))
                val = F.progn (XCDR (XCDR (fun)));
            else
            {
                /* If we have not actually read the bytecode string
                   and constants vector yet, fetch them from the file.  */
                if (CONSP (AREF (fun, LispCompiled.COMPILED_BYTECODE)))
                    F.fetch_bytecode (fun);
                val = F.byte_code (AREF (fun, LispCompiled.COMPILED_BYTECODE),
                                  AREF (fun, LispCompiled.COMPILED_CONSTANTS),
                                  AREF (fun, LispCompiled.COMPILED_STACK_DEPTH));
            }

            return unbind_to (count, val);
        }
    }    
}