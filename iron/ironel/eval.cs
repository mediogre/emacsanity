using System.Collections.Generic;

namespace IronElisp
{
    public class backtrace
    {
        public backtrace next;
        public LispObject function;
        public LispObject[] args;	/* Points to vector of args. */
        public int nargs;		/* Length of vector.
			   If nargs is UNEVALLED, args points to slot holding
			   list of unevalled args */

        public bool evalargs;
        /* Nonzero means call value of debugger when done with this operation. */
        public bool debug_on_exit;
    }

    /* This structure helps implement the `catch' and `throw' control
       structure.  A struct catchtag contains all the information needed
       to restore the state of the interpreter after a non-local jump.

       Handlers for error conditions (represented by `struct handler'
       structures) just point to a catch tag to do the cleanup required
       for their jumps.

       A call like (throw TAG VAL) searches for a catchtag whose `tag'
       member is TAG, and then unbinds to it.  The `val' member is used to
       hold VAL while the stack is unwound; `val' is returned as the value
       of the catch form.

       All the other members are concerned with restoring the interpreter
       state.  */
    public class catchtag
    {
        public LispObject tag;
        public LispObject val;

        public backtrace backlist;
        public Handler handlerlist;
        public int lisp_eval_depth;
        public int pdlcount;
        public byte_stack byte_stack;

        public catchtag(LispObject tag, backtrace bl, Handler hl, int depth, int count, byte_stack bs)
        {
            this.tag = tag;
            backlist = bl;
            handlerlist = hl;
            lisp_eval_depth = depth;
            pdlcount = count;
            byte_stack = bs;
        }
    }

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

        // Function to process declarations in defmacro forms.
        public static LispObject macro_declaration_function
        {
            get {return Defs.O[(int)Objects.macro_declaration_function];}
            set { Defs.O[(int)Objects.macro_declaration_function] = value; }
        }

        // The function from which the last `signal' was called.  Set in Fsignal.
        public static LispObject signaling_function;
    }

    public partial class L
    {
        public static int max_specpdl_size
        {
            get { return Defs.I[(int)Ints.max_specpdl_size]; }
            set { Defs.I[(int)Ints.max_specpdl_size] = value; }
        }

        public static int max_lisp_eval_depth
        {
            get { return Defs.I[(int)Ints.max_lisp_eval_depth]; }
            set { Defs.I[(int)Ints.max_lisp_eval_depth] = value; }
        }

        public static Stack<specbinding> specpdl = new Stack<specbinding>();

        public static backtrace backtrace_list;
        public static Stack<catchtag> catchlist = new Stack<catchtag>();
        /* Chain of condition handlers currently in effect.
           The elements of this chain are contained in the stack frames
           of Fcondition_case and internal_condition_case.
           When an error is signaled (by calling Fsignal, below),
           this chain is searched for an element that applies.  */
        public static Handler handlerlist;

        public static int lisp_eval_depth;


        // Nonzero means enter debugger before next function call
        public static bool debug_on_next_call
        {
            get { return Defs.B[(int)Bools.debug_on_next_call]; }
            set { Defs.B[(int)Bools.debug_on_next_call] = value; }
        }


        /* Non-zero means debugger may continue.  This is zero when the
           debugger is called during redisplay, where it might not be safe to
           continue the interrupted redisplay. */
        public static bool debugger_may_continue
        {
            get { return Defs.B[(int)Bools.debugger_may_continue]; }
            set { Defs.B[(int)Bools.debugger_may_continue] = value; }
        }

        // Nonzero means enter debugger if a quit signal
        // is handled by the command loop's error handler.
        public static bool debug_on_quit
        {
            get { return Defs.B[(int)Bools.debug_on_quit]; }
            set { Defs.B[(int)Bools.debug_on_quit] = value; }
        }

        /* The value of num_nonmacro_input_events as of the last time we
           started to enter the debugger.  If we decide to enter the debugger
           again when this is still equal to num_nonmacro_input_events, then we
           know that the debugger itself has an error, and we should just
           signal the error instead of entering an infinite loop of debugger
           invocations.  */
        public static int when_entered_debugger;

        public static void init_eval_once ()
        {
            max_specpdl_size = 1000;
            max_lisp_eval_depth = 400;
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
    }

    public partial class F
    {
        public static LispObject or(LispObject args)
        {
            LispObject val = Q.nil;

            while (L.CONSP(args))
            {
                val = F.eval(L.XCAR(args));
                if (! L.NILP(val))
                    break;
                args = L.XCDR(args);
            }

            return val;
        }

        public static LispObject and(LispObject args)
        {
            LispObject val = Q.t;

            while (L.CONSP(args))
            {
                val = F.eval(L.XCAR(args));
                if (L.NILP(val))
                    break;
                args = L.XCDR(args);
            }

            return val;
        }

        public static LispObject lisp_if(LispObject args)
        {
            LispObject cond = F.eval(F.car(args));

            if (!L.NILP(cond))
                return F.eval(F.car(F.cdr(args)));
            return F.progn(F.cdr(F.cdr(args)));
        }

        public static LispObject cond(LispObject args)
        {
            LispObject clause, val;

            val = Q.nil;
            while (! L.NILP(args))
            {
                clause = F.car(args);
                val = F.eval(F.car(clause));
                if (!L.NILP(val))
                {
                    if (!L.EQ(L.XCDR(clause), Q.nil))
                        val = F.progn(L.XCDR(clause));
                    break;
                }
                args = L.XCDR(args);
            }

            return val;
        }

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

        public static LispObject prog1(LispObject args)
        {
            LispObject val;
            LispObject args_left;
            int argnum = 0;

            if (L.NILP(args))
                return Q.nil;

            args_left = args;
            val = Q.nil;

            do
            {
                if (argnum++ == 0)
                    val = F.eval(F.car(args_left));
                else
                    F.eval(F.car(args_left));
                args_left = F.cdr(args_left);
            }
            while (!L.NILP(args_left));

            return val;
        }

        public static LispObject prog2(LispObject args)
        {
            LispObject val;
            LispObject args_left;
            int argnum = -1;

            val = Q.nil;

            if (L.NILP(args))
                return Q.nil;

            args_left = args;
            val = Q.nil;

            do
            {
                if (argnum++ == 0)
                    val = F.eval(F.car(args_left));
                else
                    F.eval(F.car(args_left));
                args_left = F.cdr(args_left);
            }
            while (!L.NILP(args_left));

            return val;
        }

        public static LispObject setq(LispObject args)
        {
            LispObject args_left;
            LispObject val, sym;

            if (L.NILP(args))
                return Q.nil;

            args_left = args;

            do
            {
                val = F.eval(F.car(F.cdr(args_left)));
                sym = F.car(args_left);
                F.set(sym, val);
                args_left = F.cdr(F.cdr(args_left));
            }
            while (!L.NILP(args_left));

            return val;
        }

        public static LispObject quote(LispObject args)
        {
            if (!L.NILP(F.cdr(args)))
                L.xsignal2(Q.wrong_number_of_arguments, Q.quote, F.length(args));
            return F.car(args);
        }

        public static LispObject function(LispObject args)
        {
            if (!L.NILP(F.cdr(args)))
                L.xsignal2(Q.wrong_number_of_arguments, Q.function, F.length(args));
            return F.car(args);
        }

        public static LispObject interactive_p()
        {
            return (L.INTERACTIVE() && L.interactive_p(true)) ? Q.t : Q.nil;
        }

        public static LispObject called_interactively_p()
        {
            return L.interactive_p(true) ? Q.t : Q.nil;
        }
    }

    public partial class L
    {
        /*  Return 1 if function in which this appears was called using
            call-interactively.

            EXCLUDE_SUBRS_P non-zero means always return 0 if the function
            called is a built-in.  */
        public static bool interactive_p (bool exclude_subrs_p)
        {
            backtrace btp;
            LispObject fun;

            btp = backtrace_list;

            /* If this isn't a byte-compiled function, there may be a frame at
               the top for Finteractive_p.  If so, skip it.  */
            fun = F.indirect_function(btp.function, Q.nil);
            if (SUBRP(fun) && (XSUBR(fun) == S.interactive_p
                        || XSUBR(fun) == S.called_interactively_p))
                btp = btp.next;

            /* If we're running an Emacs 18-style byte-compiled function, there
               may be a frame for Fbytecode at the top level.  In any version of
               Emacs there can be Fbytecode frames for subexpressions evaluated
               inside catch and condition-case.  Skip past them.

               If this isn't a byte-compiled function, then we may now be
               looking at several frames for special forms.  Skip past them.  */
            while (btp != null
               && (EQ(btp.function, Q.bytecode)
                   || btp.nargs == LispSubr.UNEVALLED))
                btp = btp.next;

            /* btp now points at the frame of the innermost function that isn't
               a special form, ignoring frames for Finteractive_p and/or
               Fbytecode at the top.  If this frame is for a built-in function
               (such as load or eval-region) return nil.  */
            fun = F.indirect_function(btp.function, Q.nil);
            if (exclude_subrs_p && SUBRP(fun))
                return false;

            /* btp points to the frame of a Lisp function that called interactive-p.
               Return t if that function was called interactively.  */
            if (btp != null && btp.next != null && EQ(btp.next.function, Q.call_interactively))
                return true;
            return false;
        }
    }

    public partial class F
    {
        public static LispObject defun(LispObject args)
        {
            LispObject defn;

            LispObject fn_name = F.car(args);
            L.CHECK_SYMBOL(fn_name);
            defn = F.cons(Q.lambda, F.cdr(args));

            if (L.CONSP(L.XSYMBOL(fn_name).function) && L.EQ(L.XCAR(L.XSYMBOL(fn_name).function), Q.autoload))
                L.LOADHIST_ATTACH(F.cons(Q.t, fn_name));
            F.fset(fn_name, defn);
            L.LOADHIST_ATTACH(F.cons(Q.defun, fn_name));
            return fn_name;
        }

        public static LispObject defmacro(LispObject args)
        {
            LispObject defn;
            LispObject lambda_list, doc, tail;

            LispObject fn_name = F.car(args);
            L.CHECK_SYMBOL(fn_name);
            lambda_list = F.car(F.cdr(args));
            tail = F.cdr(F.cdr(args));

            doc = Q.nil;
            if (L.STRINGP(F.car(tail)))
            {
                doc = L.XCAR(tail);
                tail = L.XCDR(tail);
            }

            while (L.CONSP(F.car(tail))
               && L.EQ(F.car(F.car(tail)), Q.declare))
            {
                if (!L.NILP(V.macro_declaration_function))
                {
                    L.call2(V.macro_declaration_function, fn_name, F.car(tail));
                }

                tail = F.cdr(tail);
            }

            if (L.NILP(doc))
                tail = F.cons(lambda_list, tail);
            else
                tail = F.cons(lambda_list, F.cons(doc, tail));
            defn = F.cons(Q.macro, F.cons(Q.lambda, tail));

            if (L.CONSP(L.XSYMBOL(fn_name).function)
                && L.EQ(L.XCAR(L.XSYMBOL(fn_name).function), Q.autoload))
                L.LOADHIST_ATTACH(F.cons(Q.t, fn_name));
            F.fset(fn_name, defn);
            L.LOADHIST_ATTACH(F.cons(Q.defun, fn_name));
            return fn_name;
        }

        public static LispObject defvaralias(LispObject new_alias, LispObject base_variable, LispObject docstring)
        {
            L.CHECK_SYMBOL(new_alias);
            L.CHECK_SYMBOL(base_variable);

            if (L.SYMBOL_CONSTANT_P(new_alias))
                L.error("Cannot make a constant an alias");

            LispSymbol sym = L.XSYMBOL(new_alias);
            /* http://lists.gnu.org/archive/html/emacs-devel/2008-04/msg00834.html
               If n_a is bound, but b_v is not, set the value of b_v to n_a.
               This is for the sake of define-obsolete-variable-alias and user
               customizations.  */
            if (L.NILP(F.boundp(base_variable)) && !L.NILP(F.boundp(new_alias)))
                L.XSYMBOL(base_variable).Value = sym.Value;
            sym.IsIndirectVariable = true;
            sym.Value = base_variable;
            sym.Constant = L.SYMBOL_CONSTANT_P(base_variable);
            L.LOADHIST_ATTACH(new_alias);
            if (!L.NILP(docstring))
                F.put(new_alias, Q.variable_documentation, docstring);
            else
                F.put(new_alias, Q.variable_documentation, Q.nil);

            return base_variable;
        }

        public static LispObject defvar (LispObject args)
        {
            LispObject sym, tem, tail;

            sym = F.car (args);
            tail = F.cdr (args);
            if (!L.NILP (F.cdr (F.cdr (tail))))
                L.error ("Too many arguments");

            tem = F.default_boundp (sym);
            if (!L.NILP(tail))
            {
                if (L.SYMBOL_CONSTANT_P(sym))
                {
                    /* For upward compatibility, allow (defvar :foo (quote :foo)).  */
                    tem = F.car(tail);
                    if (!(L.CONSP(tem)
                           && L.EQ(L.XCAR(tem), Q.quote)
                           && L.CONSP(L.XCDR(tem))
                           && L.EQ(L.XCAR(L.XCDR(tem)), sym)))
                        L.error("Constant symbol `%s' specified in defvar",
                                 L.SDATA(L.SYMBOL_NAME(sym)));
                }

                if (L.NILP(tem))
                    F.set_default(sym, F.eval(F.car(tail)));
                else
                { /* Check if there is really a global binding rather than just a let
                     binding that shadows the global unboundness of the var.  */
                    foreach (specbinding pdl in L.specpdl)
                    {
                        if (L.EQ(pdl.symbol, sym) && pdl.func == null
                            && L.EQ(pdl.old_value, Q.unbound))
                        {
                            L.message_with_string("Warning: defvar ignored because %s is let-bound",
                                                 L.SYMBOL_NAME(sym), true);
                            break;
                        }
                    }
                }
                tail = F.cdr(tail);
                tem = F.car(tail);
                if (!L.NILP(tem))
                {
                    F.put(sym, Q.variable_documentation, tem);
                }
                L.LOADHIST_ATTACH(sym);
            }
            else
            {
                /* Simple (defvar <var>) should not count as a definition at all.
                   It could get in the way of other definitions, and unloading this
                   package could try to make the variable unbound.  */
            }

            return sym;
        }

        public static LispObject defconst(LispObject args)
        {
            LispObject sym, tem;

            sym = F.car(args);
            if (!L.NILP(F.cdr(F.cdr(F.cdr(args)))))
                L.error("Too many arguments");

            tem = F.eval(F.car(F.cdr(args)));

            F.set_default(sym, tem);
            tem = F.car(F.cdr(F.cdr(args)));
            if (!L.NILP(tem))
            {
                F.put(sym, Q.variable_documentation, tem);
            }
            F.put(sym, Q.risky_local_variable, Q.t);
            L.LOADHIST_ATTACH(sym);
            return sym;
        }
    }

    public partial class L
    {
        /* Error handler used in Fuser_variable_p.  */
        public static LispObject user_variable_p_eh(LispObject ignore)
        {
            return Q.nil;
        }

        public static LispObject lisp_indirect_variable(LispObject sym)
        {
            return indirect_variable(XSYMBOL(sym));
        }
    }

    public partial class F
    {
        public static LispObject user_variable_p(LispObject variable)
        {
            LispObject documentation;

            if (!L.SYMBOLP(variable))
                return Q.nil;

            /* If indirect and there's an alias loop, don't check anything else.  */
            if (L.XSYMBOL(variable).IsIndirectVariable
                && L.NILP(L.internal_condition_case_1(L.lisp_indirect_variable, variable,

                                                    Q.t, L.user_variable_p_eh)))
                return Q.nil;

            while (true)
            {
                documentation = F.get(variable, Q.variable_documentation);
                if (L.INTEGERP(documentation) && L.XINT(documentation) < 0)
                    return Q.t;
                if (L.STRINGP(documentation)
                    && (L.SREF(documentation, 0) == '*'))
                    return Q.t;
                /* If it is (STRING . INTEGER), a negative integer means a user variable.  */
                if (L.CONSP(documentation)
                    && L.STRINGP(L.XCAR(documentation))
                    && L.INTEGERP(L.XCDR(documentation))
                    && L.XINT(L.XCDR(documentation)) < 0)
                    return Q.t;
                /* Customizable?  See `custom-variable-p'.  */
                if ((!L.NILP(F.get(variable, L.intern("standard-value"))))
                    || (!L.NILP(F.get(variable, L.intern("custom-autoload")))))
                    return Q.t;

                if (!L.XSYMBOL(variable).IsIndirectVariable)
                    return Q.nil;

                /* An indirect variable?  Let's follow the chain.  */
                variable = L.XSYMBOL(variable).Value;
            }
        }

        public static LispObject letX(LispObject args)
        {
            LispObject varlist, val, elt;
            int count = L.SPECPDL_INDEX();

            varlist = F.car(args);
            while (!L.NILP(varlist))
            {
                L.QUIT();
                elt = F.car(varlist);
                if (L.SYMBOLP(elt))
                    L.specbind(elt, Q.nil);
                else if (!L.NILP(F.cdr(F.cdr(elt))))
                    L.signal_error("`let' bindings can have only one value-form", elt);
                else
                {
                    val = F.eval(F.car(F.cdr(elt)));
                    L.specbind(F.car(elt), val);
                }
                varlist = F.cdr(varlist);
            }
            val = F.progn(F.cdr(args));
            return L.unbind_to(count, val);
        }

        public static LispObject let(LispObject args)
        {
            LispObject[] temps;
            LispObject tem;
            LispObject elt, varlist;
            int count = L.SPECPDL_INDEX();
            int argnum;

            varlist = F.car(args);

            /* Make space to hold the values to give the bound variables */
            elt = F.length(varlist);
            temps = new LispObject[L.XINT(elt)];

            /* Compute the values and store them in `temps' */
            for (argnum = 0; L.CONSP(varlist); varlist = L.XCDR(varlist))
            {
                L.QUIT();
                elt = L.XCAR(varlist);
                if (L.SYMBOLP(elt))
                    temps[argnum++] = Q.nil;
                else if (!L.NILP(F.cdr(F.cdr(elt))))
                    L.signal_error("`let' bindings can have only one value-form", elt);
                else
                    temps[argnum++] = F.eval(F.car(F.cdr(elt)));
            }

            varlist = F.car(args);
            for (argnum = 0; L.CONSP(varlist); varlist = L.XCDR(varlist))
            {
                elt = L.XCAR(varlist);
                tem = temps[argnum++];
                if (L.SYMBOLP(elt))
                    L.specbind(elt, tem);
                else
                    L.specbind(F.car(elt), tem);
            }

            elt = F.progn(F.cdr(args));
            return L.unbind_to(count, elt);
        }

        public static LispObject lisp_while(LispObject args)
        {
            LispObject test, body;

            test = F.car(args);
            body = F.cdr(args);
            while (!L.NILP(F.eval(test)))
            {
                L.QUIT();
                F.progn(body);
            }

            return Q.nil;
        }

        public static LispObject macroexpand(LispObject form, LispObject environment)
        {
            /* With cleanups from Hallvard Furuseth.  */
            LispObject expander, sym, def, tem;

            while (true)
            {
                /* Come back here each time we expand a macro call,
               in case it expands into another macro call.  */
                if (!L.CONSP(form))
                    break;
                /* Set SYM, give DEF and TEM right values in case SYM is not a symbol. */
                def = sym = L.XCAR(form);
                tem = Q.nil;
                /* Trace symbols aliases to other symbols
               until we get a symbol that is not an alias.  */
                while (L.SYMBOLP(def))
                {
                    L.QUIT();
                    sym = def;
                    tem = F.assq(sym, environment);
                    if (L.NILP(tem))
                    {
                        def = L.XSYMBOL(sym).function;
                        if (!L.EQ(def, Q.unbound))
                            continue;
                    }
                    break;
                }
                /* Right now TEM is the result from SYM in ENVIRONMENT,
               and if TEM is nil then DEF is SYM's function definition.  */
                if (L.NILP(tem))
                {
                    /* SYM is not mentioned in ENVIRONMENT.
                       Look at its function definition.  */
                    if (L.EQ(def, Q.unbound) || !L.CONSP(def))
                        /* Not defined or definition not suitable */
                        break;
                    if (L.EQ(L.XCAR(def), Q.autoload))
                    {
                        /* Autoloading function: will it be a macro when loaded?  */
                        tem = F.nth(L.make_number(4), def);
                        if (L.EQ(tem, Q.t) || L.EQ(tem, Q.macro))
                        /* Yes, load it and try again.  */
                        {
                            L.do_autoload(def, sym);
                            continue;
                        }
                        else
                            break;
                    }
                    else if (!L.EQ(L.XCAR(def), Q.macro))
                        break;
                    else expander = L.XCDR(def);
                }
                else
                {
                    expander = L.XCDR(tem);
                    if (L.NILP(expander))
                        break;
                }
                form = L.apply1(expander, L.XCDR(form));
            }
            return form;
        }

        public static LispObject lisp_catch(LispObject args)
        {
            LispObject tag = F.eval(F.car(args));
            return L.internal_catch(tag, F.progn, F.cdr(args));
        }
    }

    public partial class L
    {
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
                backtrace_list = ct.backlist;
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
            throw new LispCatch(catchtag, value);            // the meat of the original unwind_to_catch is in catch body of internal_xxx
        }
    }

    public partial class F
    {
        public static LispObject lisp_throw(LispObject tag, LispObject value)
        {
            if (!L.NILP(tag))
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

        public static LispObject unwind_protect(LispObject args)
        {
            LispObject val;
            int count = L.SPECPDL_INDEX();

            L.record_unwind_protect(F.progn, F.cdr(args));
            val = F.eval(F.car(args));
            return L.unbind_to(count, val);
        }

        public static LispObject condition_case (LispObject args)
        {
            LispObject bodyform, handlers;
            LispObject var;

            var      = F.car (args);
            bodyform = F.car (F.cdr (args));
            handlers = F.cdr (F.cdr (args));

            return L.internal_lisp_condition_case (var, bodyform, handlers);
        }
    }

    public partial class L
    {
        /* Like Fcondition_case, but the args are separate
           rather than passed in a list.  Used by Fbyte_code.  */
        public static LispObject internal_lisp_condition_case (LispObject var, LispObject bodyform, LispObject handlers)
        {
            LispObject val;

            CHECK_SYMBOL (var);

            for (val = handlers; CONSP (val); val = XCDR (val))
            {
                LispObject tem = XCAR (val);
                if (! (NILP (tem)
                       || (CONSP (tem)
                           && (SYMBOLP (XCAR (tem))
                               || CONSP (XCAR (tem))))))
                    error ("Invalid condition handler", tem);
            }

            catchlist.Push(new catchtag(Q.nil, backtrace_list, handlerlist, lisp_eval_depth, SPECPDL_INDEX(), byte_stack_list));

            Handler h = new Handler(); 
            try
            {
                h.var = var;
                h.handler = handlers;
                h.next = handlerlist;
                h.tag = catchlist.Peek ();
                handlerlist = h;

                val = F.eval (bodyform);

                handlerlist = h.next;
                return val;
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
                backtrace_list = ct.backlist;
                lisp_eval_depth = ct.lisp_eval_depth;

                if (!NILP (h.var))
                    specbind (h.var, lc.Value);
                val = F.progn (F.cdr (h.chosen_clause));

                /* Note that this just undoes the binding of h.var;
                   we've already unwound the stack when we caught LispCatch */
                unbind_to (ct.pdlcount, Q.nil);
                return val;
            }
            finally
            {
                catchlist.Pop();
            }
        }

        /* Call the function BFUN with no arguments, catching errors within it
           according to HANDLERS.  If there is an error, call HFUN with
           one argument which is the data that describes the error:
           (SIGNALNAME . DATA)

           HANDLERS can be a list of conditions to catch.
           If HANDLERS is Qt, catch all errors.
           If HANDLERS is Qerror, catch all errors
           but allow the debugger to run if that is enabled.  */
        public static LispObject internal_condition_case (subr0 bfun, LispObject handlers, subr1 hfun)
        {
            catchlist.Push(new catchtag(Q.nil, backtrace_list, handlerlist, lisp_eval_depth, SPECPDL_INDEX(), byte_stack_list));
            try
            {
                Handler h = new Handler(); 
                h.handler = handlers;
                h.var = Q.nil;
                h.next = handlerlist;
                h.tag = catchlist.Peek ();
                handlerlist = h;

                LispObject val = bfun ();

                handlerlist = h.next;
                return val;
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
                backtrace_list = ct.backlist;
                lisp_eval_depth = ct.lisp_eval_depth;

                return hfun (lc.Value);
            }
            finally
            {
                catchlist.Pop();
            }
        }

        /* Like internal_condition_case but call BFUN with ARG as its argument.  */
        public static LispObject internal_condition_case_1 (subr1 bfun, LispObject arg, LispObject handlers, subr1 hfun)
        {
            catchlist.Push(new catchtag(Q.nil, backtrace_list, handlerlist, lisp_eval_depth, SPECPDL_INDEX(), byte_stack_list));
            try
            {
                Handler h = new Handler(); 
                h.handler = handlers;
                h.var = Q.nil;
                h.next = handlerlist;
                h.tag = catchlist.Peek ();
                handlerlist = h;

                LispObject val = bfun (arg);

                handlerlist = h.next;
                return val;
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
                backtrace_list = ct.backlist;
                lisp_eval_depth = ct.lisp_eval_depth;

                return hfun (lc.Value);
            }
            finally
            {
                catchlist.Pop();
            }
        }

        /* Like internal_condition_case but call BFUN with NARGS as first,
           and ARGS as second argument.  */
        public static LispObject internal_condition_case_2 (subr_many bfun, int nargs, LispObject[] args, LispObject handlers, subr1 hfun)
        {
            catchlist.Push(new catchtag(Q.nil, backtrace_list, handlerlist, lisp_eval_depth, SPECPDL_INDEX(), byte_stack_list));
            try
            {
                Handler h = new Handler(); 
                h.handler = handlers;
                h.var = Q.nil;
                h.next = handlerlist;
                h.tag = catchlist.Peek ();
                handlerlist = h;

                LispObject val = bfun (nargs, args);

                handlerlist = h.next;
                return val;
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
                backtrace_list = ct.backlist;
                lisp_eval_depth = ct.lisp_eval_depth;

                return hfun (lc.Value);
            }
            finally
            {
                catchlist.Pop();
            }
        }
    }

    public partial class F
    {
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
    }

    public partial class L
    {
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

        /* Signal `error' with message S, and additional arg ARG.
           If ARG is not a genuine list, make it a one-element list.  */
        public static void signal_error(string s, LispObject arg)
        {
            LispObject tortoise, hare;

            hare = tortoise = arg;
            while (CONSP(hare))
            {
                hare = XCDR(hare);
                if (!CONSP(hare))
                    break;

                hare = XCDR(hare);
                tortoise = XCDR(tortoise);

                if (EQ(hare, tortoise))
                    break;
            }

            if (!NILP(hare))
                arg = F.cons(arg, Q.nil);	/* Make it a list.  */

            xsignal(Q.error, F.cons(make_string(s), arg));
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
           There are two ways to pass SIG and DATA:          = SIG is the error symbol, and DATA is the rest of the data.
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

        public static void error(string m, params object[] args)
        {
            System.Text.StringBuilder buffer = new System.Text.StringBuilder();
            doprnt (buffer, m, args);
            xsignal1(Q.error, make_string(buffer.ToString()));
        }
    }

    public partial class F
    {
        public static LispObject commandp (LispObject function, LispObject for_call_interactively)
        {
            LispObject fun;
            LispObject funcar;
            LispObject if_prop = Q.nil;

            fun = function;

            fun = L.indirect_function (fun); /* Check cycles. */
            if (L.NILP (fun) || L.EQ (fun, Q.unbound))
                return Q.nil;

            /* Check an `interactive-form' property if present, analogous to the
               function-documentation property. */
            fun = function;
            while (L.SYMBOLP (fun))
            {
                LispObject tmp = F.get (fun, Q.interactive_form);
                if (!L.NILP (tmp))
                    if_prop = Q.t;
                fun = F.symbol_function (fun);
            }

            /* Emacs primitives are interactive if their DEFUN specifies an
               interactive spec.  */
            if (L.SUBRP (fun))
                return L.XSUBR (fun).intspec != null ? Q.t : if_prop;

            /* Bytecode objects are interactive if they are long enough to
               have an element whose index is COMPILED_INTERACTIVE, which is
               where the interactive spec is stored.  */
            else if (L.COMPILEDP (fun))
                return (L.ASIZE (fun) > LispCompiled.COMPILED_INTERACTIVE
                        ? Q.t : if_prop);

            /* Strings and vectors are keyboard macros.  */
            if (L.STRINGP (fun) || L.VECTORP (fun))
                return (L.NILP (for_call_interactively) ? Q.t : Q.nil);

            /* Lists may represent commands.  */
            if (!L.CONSP (fun))
                return Q.nil;
            funcar = L.XCAR (fun);
            if (L.EQ (funcar, Q.lambda))
                return !L.NILP (F.assq (Q.interactive, F.cdr (L.XCDR (fun)))) ? Q.t : if_prop;
            if (L.EQ (funcar, Q.autoload))
                return !L.NILP (F.car (F.cdr (F.cdr (L.XCDR (fun))))) ? Q.t : if_prop;
            else
                return Q.nil;
        }

        public static LispObject autoload(LispObject function, LispObject file, LispObject docstring, LispObject interactive, LispObject type)
        {
            L.CHECK_SYMBOL(function);
            L.CHECK_STRING(file);

            /* If function is defined and not as an autoload, don't override */
            if (!L.EQ(L.XSYMBOL(function).function, Q.unbound)
                && !(L.CONSP(L.XSYMBOL(function).function)
                 && L.EQ(L.XCAR(L.XSYMBOL(function).function), Q.autoload)))
                return Q.nil;

            if (L.NILP(V.purify_flag))
                /* Only add entries after dumping, because the ones before are
                   not useful and else we get loads of them from the loaddefs.el.  */
                L.LOADHIST_ATTACH(F.cons(Q.autoload, function));

            return F.fset(function, F.cons(Q.autoload, F.list(4, file, docstring, interactive, type)));
        }
    }
    
    public partial class L
    {
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
    }

    public partial class F
    {
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
    }

    public partial class L
    {
        public enum run_hooks_condition {to_completion, until_success, until_failure};
    }

    public partial class F
    {
        public static LispObject run_hooks (int nargs, params LispObject[] args)
        {
            LispObject[] hook = new LispObject[1];

            for (int i = 0; i < nargs; i++)
            {
                hook[0] = args[i];
                L.run_hook_with_args (1, hook, L.run_hooks_condition.to_completion);
            }

            return Q.nil;
        }

        public static LispObject run_hook_with_args (int nargs, params LispObject[] args)
        {
            return L.run_hook_with_args(nargs, args, L.run_hooks_condition.to_completion);
        }

        public static LispObject run_hook_with_args_until_success (int nargs, params LispObject[] args)
        {
            return L.run_hook_with_args(nargs, args, L.run_hooks_condition.until_success);
        }

        public static LispObject run_hook_with_args_until_failure (int nargs, params LispObject[] args)
        {
            return L.run_hook_with_args(nargs, args, L.run_hooks_condition.until_failure);
        }
    }

    public partial class L
    {
        /* ARGS[0] should be a hook symbol.
           Call each of the functions in the hook value, passing each of them
           as arguments all the rest of ARGS (all NARGS - 1 elements).
           COND specifies a condition to test after each call
           to decide whether to stop.
           The caller (or its caller, etc) must gcpro all of ARGS,
           except that it isn't necessary to gcpro ARGS[0].  */
        public static LispObject run_hook_with_args (int nargs, LispObject[] args, run_hooks_condition cond)
        {
            LispObject sym, val, ret;
            LispObject globals;

            /* If we are dying or still initializing,
               don't do anything--it would probably crash if we tried.  */
            if (NILP (V.run_hooks))
                return Q.nil;

            sym = args[0];
            val = find_symbol_value (sym);
            ret = (cond == run_hooks_condition.until_failure ? Q.t : Q.nil);

            if (EQ (val, Q.unbound) || NILP (val))
                return ret;
            else if (!CONSP (val) || EQ (XCAR (val), Q.lambda))
            {
                args[0] = val;
                return F.funcall (nargs, args);
            }
            else
            {
                globals = Q.nil;

                for (;
                     CONSP(val) && ((cond == run_hooks_condition.to_completion)
                                     || (cond == run_hooks_condition.until_success ? NILP(ret)
                                         : !NILP (ret)));
                     val = XCDR (val))
                {
                    if (EQ (XCAR (val), Q.t))
                    {
                        /* t indicates this hook has a local binding;
                           it means to run the global binding too.  */

                        for (globals = F.default_value (sym);
                             CONSP(globals) && ((cond == run_hooks_condition.to_completion)
                                                 || (cond == run_hooks_condition.until_success ? NILP(ret)
                                                     : !NILP (ret)));
                             globals = XCDR (globals))
                        {
                            args[0] = XCAR (globals);
                            /* In a global value, t should not occur.  If it does, we
                               must ignore it to avoid an endless loop.  */
                            if (!EQ (args[0], Q.t))
                                ret = F.funcall (nargs, args);
                        }
                    }
                    else
                    {
                        args[0] = XCAR (val);
                        ret = F.funcall (nargs, args);
                    }
                }

                return ret;
            }
        }

        /* Run a hook symbol ARGS[0], but use FUNLIST instead of the actual
           present value of that symbol.
           Call each element of FUNLIST,
           passing each of them the rest of ARGS.
           The caller (or its caller, etc) must gcpro all of ARGS,
           except that it isn't necessary to gcpro ARGS[0].  */

        public static LispObject run_hook_list_with_args (LispObject funlist, int nargs, params LispObject[] args)
        {
            LispObject sym;
            LispObject val;
            LispObject globals;

            sym = args[0];
            globals = Q.nil;

            for (val = funlist; CONSP (val); val = XCDR (val))
            {
                if (EQ (XCAR (val), Q.t))
                {
                    /* t indicates this hook has a local binding;
                       it means to run the global binding too.  */

                    for (globals = F.default_value (sym);
                         CONSP (globals);
                         globals = XCDR (globals))
                    {
                        args[0] = XCAR (globals);
                        /* In a global value, t should not occur.  If it does, we
                           must ignore it to avoid an endless loop.  */
                        if (!EQ (args[0], Q.t))
                            F.funcall (nargs, args);
                    }
                }
                else
                {
                    args[0] = XCAR (val);
                    F.funcall (nargs, args);
                }
            }
            return Q.nil;
        }

        /* Run the hook HOOK, giving each function the two args ARG1 and ARG2.  */
        public static void run_hook_with_args_2 (LispObject hook, LispObject arg1, LispObject arg2)
        {
            F.run_hook_with_args (3, hook, arg1, arg2);
        }

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
    }

    public partial class F
    {
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
    }

    public partial class L
    {
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

    public partial class F
    {
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
    }
    
    public partial class L
    {
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
    }

    public partial class F
    {
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

        public static LispObject backtrace_frame(LispObject nframes)
        {
            backtrace backlist = L.backtrace_list;
            int i;
            LispObject tem;

            L.CHECK_NATNUM (nframes);

            /* Find the frame requested.  */
            for (i = 0; backlist != null && i < L.XINT (nframes); i++)
                backlist = backlist.next;

            if (backlist == null)
                return Q.nil;
            if (backlist.nargs == LispSubr.UNEVALLED)
                return F.cons (Q.nil, F.cons (backlist.function, backlist.args[0]));
            else
            {
                if (backlist.nargs == LispSubr.MANY)
                    tem = backlist.args[0];
                else
                    tem = F.list (backlist.nargs, backlist.args);

                return F.cons (Q.t, F.cons (backlist.function, tem));
            }
        }
    }

    public partial class L
    {
        public static void syms_of_eval ()
        {
            defvar_int ("max-specpdl-size", Ints.max_specpdl_size,
                        @" *Limit on number of Lisp variable bindings and `unwind-protect's.
If Lisp code tries to increase the total number past this amount,
an error is signaled.
You can safely use a value considerably larger than the default value,
if that proves inconveniently small.  However, if you increase it too far,
Emacs could run out of memory trying to make the stack bigger.");

            defvar_int ("max-lisp-eval-depth", Ints.max_lisp_eval_depth,
                        @" *Limit on depth in `eval', `apply' and `funcall' before error.

This limit serves to catch infinite recursions for you before they cause
actual stack overflow in C, which would be fatal for Emacs.
You can safely make it considerably larger than its default value,
if that proves inconveniently small.  However, if you increase it too far,
Emacs could overflow the real C stack, and crash.");

            defvar_lisp ("quit-flag", Objects.quit_flag,
                         @" Non-nil causes `eval' to abort, unless `inhibit-quit' is non-nil.
If the value is t, that means do an ordinary quit.
If the value equals `throw-on-input', that means quit by throwing
to the tag specified in `throw-on-input'; it's for handling `while-no-input'.
Typing C-g sets `quit-flag' to t, regardless of `inhibit-quit',
but `inhibit-quit' non-nil prevents anything from taking notice of that.");
            V.quit_flag = Q.nil;

            defvar_lisp ("inhibit-quit", Objects.inhibit_quit,
                         @" Non-nil inhibits C-g quitting from happening immediately.
Note that `quit-flag' will still be set by typing C-g,
so a quit will be signaled as soon as `inhibit-quit' is nil.
To prevent this happening, set `quit-flag' to nil
before making `inhibit-quit' nil.");
            V.inhibit_quit = Q.nil;

            Q.inhibit_quit = intern ("inhibit-quit");
            Q.autoload = intern ("autoload");
            Q.debug_on_error = intern ("debug-on-error");
            Q.macro = intern ("macro");
            Q.declare = intern ("declare");

            /* Note that the process handling also uses Qexit, but we don't want
               to staticpro it twice, so we just do it here.  */
            Q.exit = intern ("exit");

            Q.interactive = intern ("interactive");
            Q.commandp = intern ("commandp");
            Q.defun = intern ("defun");
            Q.and_rest = intern ("&rest");
            Q.and_optional = intern ("&optional");
            Q.debug = intern ("debug");

            defvar_lisp ("stack-trace-on-error", Objects.stack_trace_on_error,
                         @" *Non-nil means errors display a backtrace buffer.
More precisely, this happens for any error that is handled
by the editor command loop.
If the value is a list, an error only means to display a backtrace
if one of its condition symbols appears in the list.");
            V.stack_trace_on_error = Q.nil;

            defvar_lisp ("debug-on-error", Objects.debug_on_error,
                         @" *Non-nil means enter debugger if an error is signaled.
Does not apply to errors handled by `condition-case' or those
matched by `debug-ignored-errors'.
If the value is a list, an error only means to enter the debugger
if one of its condition symbols appears in the list.
When you evaluate an expression interactively, this variable
is temporarily non-nil if `eval-expression-debug-on-error' is non-nil.
The command `toggle-debug-on-error' toggles this.
See also the variable `debug-on-quit'.");
            V.debug_on_error = Q.nil;

            defvar_lisp ("debug-ignored-errors", Objects.debug_ignored_errors,
                         @" *List of errors for which the debugger should not be called.
Each element may be a condition-name or a regexp that matches error messages.
If any element applies to a given error, that error skips the debugger
and just returns to top level.
This overrides the variable `debug-on-error'.
It does not apply to errors handled by `condition-case'.");
            V.debug_ignored_errors = Q.nil;

            defvar_bool ("debug-on-quit", Bools.debug_on_quit,
                         @" *Non-nil means enter debugger if quit is signaled (C-g, for example).
Does not apply if quit is handled by a `condition-case'.");
            debug_on_quit = false;

            defvar_bool ("debug-on-next-call", Bools.debug_on_next_call,
                         @" Non-nil means enter debugger before next `eval', `apply' or `funcall'.");
            
            defvar_bool ("debugger-may-continue", Bools.debugger_may_continue,
                         @" Non-nil means debugger may continue execution.
This is nil when the debugger is called under circumstances where it
might not be safe to continue.");
            debugger_may_continue = true;

            defvar_lisp ("debugger", Objects.debugger,
                         @" Function to call to invoke debugger.
If due to frame exit, args are `exit' and the value being returned;
this function's value will be returned instead of that.
If due to error, args are `error' and a list of the args to `signal'.
If due to `apply' or `funcall' entry, one arg, `lambda'.
If due to `eval' entry, one arg, t.");
            V.debugger = Q.nil;

            defvar_lisp ("signal-hook-function", Objects.signal_hook_function,
                         @" If non-nil, this is a function for `signal' to call.
It receives the same arguments that `signal' was given.
The Edebug package uses this to regain control.");
            V.signal_hook_function = Q.nil;

            defvar_lisp ("debug-on-signal", Objects.debug_on_signal,
                         @" *Non-nil means call the debugger regardless of condition handlers.
Note that `debug-on-error', `debug-on-quit' and friends
still determine whether to handle the particular condition.");
            V.debug_on_signal = Q.nil;

            defvar_lisp ("macro-declaration-function", Objects.macro_declaration_function,
                         @" Function to process declarations in a macro definition.
The function will be called with two args MACRO and DECL.
MACRO is the name of the macro being defined.
DECL is a list `(declare ...)' containing the declarations.
The value the function returns is not used.");
            V.macro_declaration_function = Q.nil;

            V.run_hooks = intern ("run-hooks");
            V.autoload_queue = Q.nil;
            V.signaling_function = Q.nil;

            defsubr ("or", F.or, 0, LispSubr.UNEVALLED, null,
                     @" Eval args until one of them yields non-nil, then return that value.
The remaining args are not evalled at all.
If all args return nil, return nil.
usage: (or CONDITIONS...)");
            defsubr ("and", F.and, 0, LispSubr.UNEVALLED, null,
                     @" Eval args until one of them yields nil, then return nil.
The remaining args are not evalled at all.
If no arg yields nil, return the last arg's value.
usage: (and CONDITIONS...)");
            defsubr ("if", F.lisp_if, 2, LispSubr.UNEVALLED, null,
                     @" If COND yields non-nil, do THEN, else do ELSE...
Returns the value of THEN or the value of the last of the ELSE's.
THEN must be one expression, but ELSE... can be zero or more expressions.
If COND yields nil, and there are no ELSE's, the value is nil.
usage: (if COND THEN ELSE...)");
            defsubr ("cond", F.cond, 0, LispSubr.UNEVALLED, null,
                     @" Try each clause until one succeeds.
Each clause looks like (CONDITION BODY...).  CONDITION is evaluated
and, if the value is non-nil, this clause succeeds:
then the expressions in BODY are evaluated and the last one's
value is the value of the cond-form.
If no clause succeeds, cond returns nil.
If a clause has one element, as in (CONDITION),
CONDITION's value if non-nil is returned from the cond-form.
usage: (cond CLAUSES...)");
            defsubr ("progn", F.progn, 0, LispSubr.UNEVALLED, null,
                     @" Eval BODY forms sequentially and return value of last one.
usage: (progn BODY...)");
            defsubr ("prog1", F.prog1, 1, LispSubr.UNEVALLED, null,
                     @" Eval FIRST and BODY sequentially; return value from FIRST.
The value of FIRST is saved during the evaluation of the remaining args,
whose values are discarded.
usage: (prog1 FIRST BODY...)");
            defsubr ("prog2", F.prog2, 2, LispSubr.UNEVALLED, null,
                     @" Eval FORM1, FORM2 and BODY sequentially; return value from FORM2.
The value of FORM2 is saved during the evaluation of the
remaining args, whose values are discarded.
usage: (prog2 FORM1 FORM2 BODY...)");
            defsubr ("setq", F.setq, 0, LispSubr.UNEVALLED, null,
                     @" Set each SYM to the value of its VAL.
The symbols SYM are variables; they are literal (not evaluated).
The values VAL are expressions; they are evaluated.
Thus, (setq x (1+ y)) sets `x' to the value of `(1+ y)'.
The second VAL is not computed until after the first SYM is set, and so on;
each VAL can use the new value of variables set earlier in the `setq'.
The return value of the `setq' form is the value of the last VAL.
usage: (setq [SYM VAL]...)");
            defsubr ("quote", F.quote, 1, LispSubr.UNEVALLED, null,
                     @" Return the argument, without evaluating it.  `(quote x)' yields `x'.
usage: (quote ARG)");
            defsubr ("function", F.function, 1, LispSubr.UNEVALLED, null,
                     @" Like `quote', but preferred for objects which are functions.
In byte compilation, `function' causes its argument to be compiled.
`quote' cannot do that.
usage: (function ARG)");
            defsubr ("defun", F.defun, 2, LispSubr.UNEVALLED, null,
                     @" Define NAME as a function.
The definition is (lambda ARGLIST [DOCSTRING] BODY...).
See also the function `interactive'.
usage: (defun NAME ARGLIST [DOCSTRING] BODY...)");
            defsubr ("defmacro", F.defmacro, 2, LispSubr.UNEVALLED, null,
                     @" Define NAME as a macro.
The actual definition looks like
 (macro lambda ARGLIST [DOCSTRING] [DECL] BODY...).
When the macro is called, as in (NAME ARGS...),
the function (lambda ARGLIST BODY...) is applied to
the list ARGS... as it appears in the expression,
and the result should be a form to be evaluated instead of the original.

DECL is a declaration, optional, which can specify how to indent
calls to this macro, how Edebug should handle it, and which argument
should be treated as documentation.  It looks like this:
  (declare SPECS...)
The elements can look like this:
  (indent INDENT)
	Set NAME's `lisp-indent-function' property to INDENT.

  (debug DEBUG)
	Set NAME's `edebug-form-spec' property to DEBUG.  (This is
	equivalent to writing a `def-edebug-spec' for the macro.)

  (doc-string ELT)
	Set NAME's `doc-string-elt' property to ELT.

usage: (defmacro NAME ARGLIST [DOCSTRING] [DECL] BODY...)");
            defsubr ("defvar", F.defvar, 1, LispSubr.UNEVALLED, null,
                     @" Define SYMBOL as a variable, and return SYMBOL.
You are not required to define a variable in order to use it,
but the definition can supply documentation and an initial value
in a way that tags can recognize.

INITVALUE is evaluated, and used to set SYMBOL, only if SYMBOL's value is void.
If SYMBOL is buffer-local, its default value is what is set;
 buffer-local values are not affected.
INITVALUE and DOCSTRING are optional.
If DOCSTRING starts with *, this variable is identified as a user option.
 This means that M-x set-variable recognizes it.
 See also `user-variable-p'.
If INITVALUE is missing, SYMBOL's value is not set.

If SYMBOL has a local binding, then this form affects the local
binding.  This is usually not what you want.  Thus, if you need to
load a file defining variables, with this form or with `defconst' or
`defcustom', you should always load that file _outside_ any bindings
for these variables.  \(`defconst' and `defcustom' behave similarly in
this respect.)
usage: (defvar SYMBOL &optional INITVALUE DOCSTRING)");
            defsubr ("defvaralias", F.defvaralias, 2, 3, null,
                     @" Make NEW-ALIAS a variable alias for symbol BASE-VARIABLE.
Aliased variables always have the same value; setting one sets the other.
Third arg DOCSTRING, if non-nil, is documentation for NEW-ALIAS.  If it is
omitted or nil, NEW-ALIAS gets the documentation string of BASE-VARIABLE,
or of the variable at the end of the chain of aliases, if BASE-VARIABLE is
itself an alias.  If NEW-ALIAS is bound, and BASE-VARIABLE is not,
then the value of BASE-VARIABLE is set to that of NEW-ALIAS.
The return value is BASE-VARIABLE.");
            defsubr ("defconst", F.defconst, 2, LispSubr.UNEVALLED, null,
                     @" Define SYMBOL as a constant variable.
The intent is that neither programs nor users should ever change this value.
Always sets the value of SYMBOL to the result of evalling INITVALUE.
If SYMBOL is buffer-local, its default value is what is set;
 buffer-local values are not affected.
DOCSTRING is optional.

If SYMBOL has a local binding, then this form sets the local binding's
value.  However, you should normally not make local bindings for
variables defined with this form.
usage: (defconst SYMBOL INITVALUE [DOCSTRING])");
            defsubr ("user-variable-p", F.user_variable_p, 1, 1, null,
                     @" Return t if VARIABLE is intended to be set and modified by users.
\(The alternative is a variable used internally in a Lisp program.)
A variable is a user variable if
\(1) the first character of its documentation is `*', or
\(2) it is customizable (its property list contains a non-nil value
    of `standard-value' or `custom-autoload'), or
\(3) it is an alias for another user variable.
Return nil if VARIABLE is an alias and there is a loop in the
chain of symbols.");
            defsubr ("let", F.let, 1, LispSubr.UNEVALLED, null,
                     @" Bind variables according to VARLIST then eval BODY.
The value of the last form in BODY is returned.
Each element of VARLIST is a symbol (which is bound to nil)
or a list (SYMBOL VALUEFORM) (which binds SYMBOL to the value of VALUEFORM).
All the VALUEFORMs are evalled before any symbols are bound.
usage: (let VARLIST BODY...)");
            defsubr ("let*", F.letX, 1, LispSubr.UNEVALLED, null,
                     @" Bind variables according to VARLIST then eval BODY.
The value of the last form in BODY is returned.
Each element of VARLIST is a symbol (which is bound to nil)
or a list (SYMBOL VALUEFORM) (which binds SYMBOL to the value of VALUEFORM).
Each VALUEFORM can refer to the symbols already bound by this VARLIST.
usage: (let* VARLIST BODY...)");
            defsubr ("while", F.lisp_while, 1, LispSubr.UNEVALLED, null,
                     @" If TEST yields non-nil, eval BODY... and repeat.
The order of execution is thus TEST, BODY, TEST, BODY and so on
until TEST returns nil.
usage: (while TEST BODY...)");
            defsubr ("macroexpand", F.macroexpand, 1, 2, null,
                     @" Return result of expanding macros at top level of FORM.
If FORM is not a macro call, it is returned unchanged.
Otherwise, the macro is expanded and the expansion is considered
in place of FORM.  When a non-macro-call results, it is returned.

The second optional arg ENVIRONMENT specifies an environment of macro
definitions to shadow the loaded ones for use in file byte-compilation.");
            defsubr ("catch", F.lisp_catch, 1, LispSubr.UNEVALLED, null,
                     @" Eval BODY allowing nonlocal exits using `throw'.
TAG is evalled to get the tag to use; it must not be nil.

Then the BODY is executed.
Within BODY, a call to `throw' with the same TAG exits BODY and this `catch'.
If no throw happens, `catch' returns the value of the last BODY form.
If a throw happens, it specifies the value to return from `catch'.
usage: (catch TAG BODY...)");
            defsubr ("throw", F.lisp_throw, 2, 2, null,
                     @" Throw to the catch for TAG and return VALUE from it.
Both TAG and VALUE are evalled.");
            defsubr ("unwind-protect", F.unwind_protect, 1, LispSubr.UNEVALLED, null,
                     @" Do BODYFORM, protecting with UNWINDFORMS.
If BODYFORM completes normally, its value is returned
after executing the UNWINDFORMS.
If BODYFORM exits nonlocally, the UNWINDFORMS are executed anyway.
usage: (unwind-protect BODYFORM UNWINDFORMS...)");
            defsubr ("condition-case", F.condition_case, 2, LispSubr.UNEVALLED, null,
                     @" Regain control when an error is signaled.
Executes BODYFORM and returns its value if no error happens.
Each element of HANDLERS looks like (CONDITION-NAME BODY...)
where the BODY is made of Lisp expressions.

A handler is applicable to an error
if CONDITION-NAME is one of the error's condition names.
If an error happens, the first applicable handler is run.

The car of a handler may be a list of condition names
instead of a single condition name.  Then it handles all of them.

When a handler handles an error, control returns to the `condition-case'
and it executes the handler's BODY...
with VAR bound to (ERROR-SYMBOL . SIGNAL-DATA) from the error.
(If VAR is nil, the handler can't access that information.)
Then the value of the last BODY form is returned from the `condition-case'
expression.

See also the function `signal' for more info.
usage: (condition-case VAR BODYFORM &rest HANDLERS)");
            defsubr ("signal", F.signal, 2, 2, null,
                     @" Signal an error.  Args are ERROR-SYMBOL and associated DATA.
This function does not return.

An error symbol is a symbol with an `error-conditions' property
that is a list of condition names.
A handler for any of those names will get to handle this signal.
The symbol `error' should normally be one of them.

DATA should be a list.  Its elements are printed as part of the error message.
See Info anchor `(elisp)Definition of signal' for some details on how this
error message is constructed.
If the signal is handled, DATA is made available to the handler.
See also the function `condition-case'.");
            S.interactive_p = defsubr("interactive-p", F.interactive_p, 0, 0, null,
                     @" Return t if the function was run directly by user input.
This means that the function was called with `call-interactively'
\(which includes being called as the binding of a key)
and input is currently coming from the keyboard (not in keyboard macro),
and Emacs is not running in batch mode (`noninteractive' is nil).

The only known proper use of `interactive-p' is in deciding whether to
display a helpful message, or how to display it.  If you're thinking
of using it for any other purpose, it is quite likely that you're
making a mistake.  Think: what do you want to do when the command is
called from a keyboard macro?

If you want to test whether your function was called with
`call-interactively', the way to do that is by adding an extra
optional argument, and making the `interactive' spec specify non-nil
unconditionally for that argument.  (`p' is a good way to do this.)");
            S.called_interactively_p = defsubr("called-interactively-p", F.called_interactively_p, 0, 0, null,
                     @" Return t if the function using this was called with `call-interactively'.
This is used for implementing advice and other function-modifying
features of Emacs.

The cleanest way to test whether your function was called with
`call-interactively' is by adding an extra optional argument,
and making the `interactive' spec specify non-nil unconditionally
for that argument.  (`p' is a good way to do this.)");
            defsubr ("commandp", F.commandp, 1, 2, null,
                     @" Non-nil if FUNCTION makes provisions for interactive calling.
This means it contains a description for how to read arguments to give it.
The value is nil for an invalid function or a symbol with no function
definition.

Interactively callable functions include strings and vectors (treated
as keyboard macros), lambda-expressions that contain a top-level call
to `interactive', autoload definitions made by `autoload' with non-nil
fourth argument, and some of the built-in functions of Lisp.

Also, a symbol satisfies `commandp' if its function definition does so.

If the optional argument FOR-CALL-INTERACTIVELY is non-nil,
then strings and vectors are not accepted.");
            defsubr ("autoload", F.autoload, 2, 5, null,
                     @" Define FUNCTION to autoload from FILE.
FUNCTION is a symbol; FILE is a file name string to pass to `load'.
Third arg DOCSTRING is documentation for the function.
Fourth arg INTERACTIVE if non-nil says function can be called interactively.
Fifth arg TYPE indicates the type of the object:
   nil or omitted says FUNCTION is a function,
   `keymap' says FUNCTION is really a keymap, and
   `macro' or t says FUNCTION is really a macro.
Third through fifth args give info about the real definition.
They default to nil.
If FUNCTION is already defined other than as an autoload,
this does nothing and returns nil.");
            defsubr ("eval", F.eval, 1, 1, null,
                     @" Evaluate FORM and return its value.");
            defsubr ("apply", F.apply, 2, LispSubr.MANY, null,
                     @" Call FUNCTION with our remaining args, using our last arg as list of args.
Then return the value FUNCTION returns.
Thus, (apply '+ 1 2 '(3 4)) returns 10.
usage: (apply FUNCTION &rest ARGUMENTS)");
            defsubr ("funcall", F.funcall, 1, LispSubr.MANY, null,
                     @" Call first argument as a function, passing remaining arguments to it.
Return the value that function returns.
Thus, (funcall 'cons 'x 'y) returns (x . y).
usage: (funcall FUNCTION &rest ARGUMENTS)");
            defsubr ("run-hooks", F.run_hooks, 0, LispSubr.MANY, null,
                     @" Run each hook in HOOKS.
Each argument should be a symbol, a hook variable.
These symbols are processed in the order specified.
If a hook symbol has a non-nil value, that value may be a function
or a list of functions to be called to run the hook.
If the value is a function, it is called with no arguments.
If it is a list, the elements are called, in order, with no arguments.

Major modes should not use this function directly to run their mode
hook; they should use `run-mode-hooks' instead.

Do not use `make-local-variable' to make a hook variable buffer-local.
Instead, use `add-hook' and specify t for the LOCAL argument.
usage: (run-hooks &rest HOOKS)");
            defsubr ("run-hook-with-args", F.run_hook_with_args, 1, LispSubr.MANY, null,
                     @" Run HOOK with the specified arguments ARGS.
HOOK should be a symbol, a hook variable.  If HOOK has a non-nil
value, that value may be a function or a list of functions to be
called to run the hook.  If the value is a function, it is called with
the given arguments and its return value is returned.  If it is a list
of functions, those functions are called, in order,
with the given arguments ARGS.
It is best not to depend on the value returned by `run-hook-with-args',
as that may change.

Do not use `make-local-variable' to make a hook variable buffer-local.
Instead, use `add-hook' and specify t for the LOCAL argument.
usage: (run-hook-with-args HOOK &rest ARGS)");
            defsubr ("run-hook-with-args-until-success", F.run_hook_with_args_until_success, 1, LispSubr.MANY, null,
                     @" Run HOOK with the specified arguments ARGS.
HOOK should be a symbol, a hook variable.  If HOOK has a non-nil
value, that value may be a function or a list of functions to be
called to run the hook.  If the value is a function, it is called with
the given arguments and its return value is returned.
If it is a list of functions, those functions are called, in order,
with the given arguments ARGS, until one of them
returns a non-nil value.  Then we return that value.
However, if they all return nil, we return nil.

Do not use `make-local-variable' to make a hook variable buffer-local.
Instead, use `add-hook' and specify t for the LOCAL argument.
usage: (run-hook-with-args-until-success HOOK &rest ARGS)");
            defsubr ("run-hook-with-args-until-failure", F.run_hook_with_args_until_failure, 1, LispSubr.MANY, null,
                     @" Run HOOK with the specified arguments ARGS.
HOOK should be a symbol, a hook variable.  If HOOK has a non-nil
value, that value may be a function or a list of functions to be
called to run the hook.  If the value is a function, it is called with
the given arguments and its return value is returned.
If it is a list of functions, those functions are called, in order,
with the given arguments ARGS, until one of them returns nil.
Then we return nil.  However, if they all return non-nil, we return non-nil.

Do not use `make-local-variable' to make a hook variable buffer-local.
Instead, use `add-hook' and specify t for the LOCAL argument.
usage: (run-hook-with-args-until-failure HOOK &rest ARGS)");
            defsubr ("fetch-bytecode", F.fetch_bytecode, 1, 1, null,
                     @" If byte-compiled OBJECT is lazy-loaded, fetch it now.");
            defsubr ("backtrace-debug", F.backtrace_debug, 2, 2, null,
                     @" Set the debug-on-exit flag of eval frame LEVEL levels down to FLAG.
The debugger is entered when that frame exits, if the flag is non-nil.");
            defsubr ("backtrace", F.backtrace, 0, 0, "",
                     @" Print a trace of Lisp function calls currently active.
Output stream used is value of `standard-output'.");
            defsubr("backtrace-frame", F.backtrace_frame, 1, 1, null,
                     @" Return the function and arguments NFRAMES up from current execution point.
If that frame has not evaluated the arguments yet (or is a special form),
the value is (nil FUNCTION ARG-FORMS...).
If that frame has evaluated its arguments and called its function already,
the value is (t FUNCTION ARG-VALUES...).
A &rest arg is represented as the tail of the list ARG-VALUES.
FUNCTION is whatever was supplied as car of evaluated list,
or a lambda expression for macro calls.
If NFRAMES is more than the number of frames, the value is nil.");
        }
    }

    public partial class S
    {
        public static LispSubr interactive_p;
        public static LispSubr called_interactively_p;
    }
}