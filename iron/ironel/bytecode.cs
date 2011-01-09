namespace IronElisp
{
    public class byte_stack
    {
        public int pc;
        public int top;
        public LispObject[] bottom;
        public LispObject byte_string;
        public byte[] byte_string_start;
        public LispObject constants;

        public byte_stack next;

        // like fetch but does not move
        public byte PEEK()
        {
            return byte_string_start[pc];
        }
        /* Fetch the next byte from the bytecode stream */
        public byte FETCH()
        { 
            return byte_string_start[pc++];
        }

        /* Fetch two bytes from the bytecode stream and make a 16-bit number
           out of them */
        public int FETCH2()
        {
            int op = FETCH();
            return op + (FETCH() << 8);
        }

        /* Push x onto the execution stack. */
        public void PUSH(LispObject x)
        { 
            top++;
            bottom[top] = x;
        }

        /* Pop a value off the execution stack.  */
        public LispObject POP()
        {
            return bottom[top--];
        }

        /* Discard n values from the execution stack.  */
        public void DISCARD(int n)
        {
            top -= n;
        }

        /* Get the value which is at the top of the execution stack, but don't
        pop it. */
        public LispObject TOP
        {
            get { return bottom[top]; }
            set { bottom[top] = value;}
        }

        /*  Byte codes: */
        public const int Bvarref = 8; 
        public const int Bvarset = 16; 
        public const int Bvarbind = 24; 
        public const int Bcall = 32; 
        public const int Bunbind = 40; 

        public const int Bnth = 56; 
        public const int Bsymbolp = 57; 
        public const int Bconsp = 58; 
        public const int Bstringp = 59; 
        public const int Blistp = 60; 
        public const int Beq = 61; 
        public const int Bmemq = 62; 
        public const int Bnot = 63; 
        public const int Bcar = 64; 
        public const int Bcdr = 65; 
        public const int Bcons = 66; 
        public const int Blist1 = 67; 
        public const int Blist2 = 68; 
        public const int Blist3 = 69; 
        public const int Blist4 = 70; 
        public const int Blength = 71; 
        public const int Baref = 72; 
        public const int Baset = 73; 
        public const int Bsymbol_value = 74; 
        public const int Bsymbol_function = 75; 
        public const int Bset = 76; 
        public const int Bfset = 77; 
        public const int Bget = 78; 
        public const int Bsubstring = 79; 
        public const int Bconcat2 = 80; 
        public const int Bconcat3 = 81; 
        public const int Bconcat4 = 82; 
        public const int Bsub1 = 83; 
        public const int Badd1 = 84; 
        public const int Beqlsign = 85; 
        public const int Bgtr = 86; 
        public const int Blss = 87; 
        public const int Bleq = 88; 
        public const int Bgeq = 89; 
        public const int Bdiff = 90; 
        public const int Bnegate = 91; 
        public const int Bplus = 92; 
        public const int Bmax = 93; 
        public const int Bmin = 94; 
        public const int Bmult = 95; 

        public const int Bpoint = 96; 
/* Was Bmark in v17.  */
        public const int Bsave_current_buffer = 97; 
        public const int Bgoto_char = 98; 
        public const int Binsert = 99; 
        public const int Bpoint_max = 100; 
        public const int Bpoint_min = 101; 
        public const int Bchar_after = 102; 
        public const int Bfollowing_char = 103; 
        public const int Bpreceding_char = 104; 
        public const int Bcurrent_column = 105; 
        public const int Bindent_to = 106; 
        public const int Bscan_buffer = 107;  /* No longer generated as of v18 */
        public const int Beolp = 108; 
        public const int Beobp = 109; 
        public const int Bbolp = 110; 
        public const int Bbobp = 111; 
        public const int Bcurrent_buffer = 112; 
        public const int Bset_buffer = 113; 
        public const int Bsave_current_buffer_1 = 114;  /* Replacing Bsave_current_buffer.  */
        public const int Bread_char = 114;  /* No longer generated as of v19 */
        public const int Bset_mark = 115;  /* this loser is no longer generated as of v18 */
        public const int Binteractive_p = 116;  /* Needed since interactive-p takes unevalled args */

        public const int Bforward_char = 117; 
        public const int Bforward_word = 118; 
        public const int Bskip_chars_forward = 119; 
        public const int Bskip_chars_backward = 120; 
        public const int Bforward_line = 121; 
        public const int Bchar_syntax = 122; 
        public const int Bbuffer_substring = 123; 
        public const int Bdelete_region = 124; 
        public const int Bnarrow_to_region = 125; 
        public const int Bwiden = 126; 
        public const int Bend_of_line = 127; 

        public const int Bconstant2 = 129; 
        public const int Bgoto = 130; 
        public const int Bgotoifnil = 131; 
        public const int Bgotoifnonnil = 132; 
        public const int Bgotoifnilelsepop = 133; 
        public const int Bgotoifnonnilelsepop = 134; 
        public const int Breturn = 135; 
        public const int Bdiscard = 136; 
        public const int Bdup = 137; 

        public const int Bsave_excursion = 138; 
        public const int Bsave_window_excursion = 139; 
        public const int Bsave_restriction = 140; 
        public const int Bcatch = 141; 

        public const int Bunwind_protect = 142; 
        public const int Bcondition_case = 143; 
        public const int Btemp_output_buffer_setup = 144; 
        public const int Btemp_output_buffer_show = 145; 

        public const int Bunbind_all = 146; 

        public const int Bset_marker = 147; 
        public const int Bmatch_beginning = 148; 
        public const int Bmatch_end = 149; 
        public const int Bupcase = 150; 
        public const int Bdowncase = 151; 

        public const int Bstringeqlsign = 152; 
        public const int Bstringlss = 153; 
        public const int Bequal = 154; 
        public const int Bnthcdr = 155; 
        public const int Belt = 156; 
        public const int Bmember = 157; 
        public const int Bassq = 158; 
        public const int Bnreverse = 159; 
        public const int Bsetcar = 160; 
        public const int Bsetcdr = 161; 
        public const int Bcar_safe = 162; 
        public const int Bcdr_safe = 163; 
        public const int Bnconc = 164; 
        public const int Bquo = 165; 
        public const int Brem = 166; 
        public const int Bnumberp = 167; 
        public const int Bintegerp = 168; 

        public const int BRgoto = 170; 
        public const int BRgotoifnil = 171; 
        public const int BRgotoifnonnil = 172; 
        public const int BRgotoifnilelsepop = 173; 
        public const int BRgotoifnonnilelsepop = 174; 

        public const int BlistN = 175; 
        public const int BconcatN = 176; 
        public const int BinsertN = 177; 

        public const int Bconstant = 192; 
        public const int CONSTANTLIM = 64; 
    }

    public partial class L
    {
        public static byte_stack byte_stack_list;

        /* A version of the QUIT macro which makes sure that the stack top is
           set before signaling `quit'.  */
        public static void BYTE_CODE_QUIT()
        {
            if (!NILP (V.quit_flag) && NILP (V.inhibit_quit))
            {				
                LispObject flag = V.quit_flag;
	            V.quit_flag = Q.nil;

	            if (EQ (V.throw_on_input, flag))
	                F.lisp_throw (V.throw_on_input, Q.t);
	            F.signal (Q.quit, Q.nil);
            }
        }
    }

    public partial class F
    {
        public static LispObject byte_code(LispObject bytestr, LispObject vector, LispObject maxdepth)
        {
            int count = L.SPECPDL_INDEX ();

            int op;
            /* Lisp_Object v1, v2; */
            LispObject[] vectorp;

            int bytestr_length;
            byte_stack stack = new byte_stack ();
            int top;
            LispObject result;

            L.CHECK_STRING (bytestr);
            L.CHECK_VECTOR (vector);
            L.CHECK_NUMBER (maxdepth);

            if (L.STRING_MULTIBYTE (bytestr))
                /* BYTESTR must have been produced by Emacs 20.2 or the earlier
                   because they produced a raw 8-bit string for byte-code and now
                   such a byte-code string is loaded as multibyte while raw 8-bit
                   characters converted to multibyte form.  Thus, now we must
                   convert them back to the originally intended unibyte form.  */
                bytestr = F.string_as_unibyte (bytestr);

            bytestr_length = L.SBYTES (bytestr);
            vectorp = L.XVECTOR (vector).Contents;

            stack.byte_string = bytestr;
            stack.pc = 0;
            stack.byte_string_start = L.SDATA (bytestr);
            stack.constants = vector;
            stack.bottom = new LispObject[L.XINT (maxdepth)];
            stack.top = -1;
            stack.next = L.byte_stack_list;
            L.byte_stack_list = stack;

            while (true)
            {
                op = stack.FETCH();

                switch (op)
                {
                case byte_stack.Bvarref + 7:
                    op = stack.FETCH2();
                    goto varref;

                case byte_stack.Bvarref:
                case byte_stack.Bvarref + 1:
                case byte_stack.Bvarref + 2:
                case byte_stack.Bvarref + 3:
                case byte_stack.Bvarref + 4:
                case byte_stack.Bvarref + 5:
                    op = op - byte_stack.Bvarref;
                    goto varref;

                    /* This seems to be the most frequently executed byte-code
                       among the Bvarref's, so avoid a goto here.  */
                case byte_stack.Bvarref+6:
                    op = stack.FETCH();
                    varref:
                    {
                        LispObject v1, v2;

                        v1 = vectorp[op];
                        if (L.SYMBOLP (v1))
                        {
                            v2 = L.SYMBOL_VALUE (v1);
                            if (L.MISCP (v2) || L.EQ (v2, Q.unbound))
                            {
                                v2 = F.symbol_value (v1);
                            }
                        }
                        else
                        {
                            v2 = F.symbol_value (v1);
                        }
                        stack.PUSH (v2);
                        break;
                    }

                case byte_stack.Bgotoifnil:
                    {
                        LispObject v1;

                        op = stack.FETCH2();
                        v1 = stack.POP();
                        if (L.NILP (v1))
                        {
                            L.BYTE_CODE_QUIT();
                            stack.pc = op;
                        }
                        break;
                    }

                case byte_stack.Bcar:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        stack.TOP = L.CAR (v1);
                        break;
                    }

                case byte_stack.Beq:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = L.EQ (v1, stack.TOP) ? Q.t : Q.nil;
                        break;
                    }

                case byte_stack.Bmemq:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.memq (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bcdr:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        stack.TOP = L.CDR (v1);
                        break;
                    }

                case byte_stack.Bvarset:
                case byte_stack.Bvarset+1:
                case byte_stack.Bvarset+2:
                case byte_stack.Bvarset+3:
                case byte_stack.Bvarset+4:
                case byte_stack.Bvarset+5:
                    op -= byte_stack.Bvarset;
                    goto varset;

                case byte_stack.Bvarset+7:
                    op = stack.FETCH2();
                    goto varset;

                case byte_stack.Bvarset+6:
                    op = stack.FETCH();
                    varset:
                    {
                        LispObject sym, val;

                        sym = vectorp[op];
                        val = stack.TOP;

                        /* Inline the most common case.  */
                        if (L.SYMBOLP (sym)
                            && !L.EQ (val, Q.unbound)
                            && !L.XSYMBOL (sym).IsIndirectVariable
                            && !L.SYMBOL_CONSTANT_P (sym)
                            && !L.MISCP (L.XSYMBOL (sym).Value))
                            L.XSYMBOL (sym).Value = val;
                        else
                        {
                            L.set_internal (sym, val, L.current_buffer, false);
                        }
                    }
                    stack.POP();
                    break;

                case byte_stack.Bdup:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        stack.PUSH (v1);
                        break;
                    }

                    /* ------------------ */

                case byte_stack.Bvarbind+6:
                    op = stack.FETCH();
                    goto varbind;

                case byte_stack.Bvarbind+7:
                    op = stack.FETCH2();
                    goto varbind;

                case byte_stack.Bvarbind:
                case byte_stack.Bvarbind+1:
                case byte_stack.Bvarbind+2:
                case byte_stack.Bvarbind+3:
                case byte_stack.Bvarbind+4:
                case byte_stack.Bvarbind+5:
                    op -= byte_stack.Bvarbind;
                    varbind:
                    L.specbind (vectorp[op], stack.POP());
                    break;

                case byte_stack.Bcall+6:
                    op = stack.FETCH();
                    goto docall;

                case byte_stack.Bcall+7:
                    op = stack.FETCH2();
                    goto docall;

                case byte_stack.Bcall:
                case byte_stack.Bcall+1:
                case byte_stack.Bcall+2:
                case byte_stack.Bcall+3:
                case byte_stack.Bcall+4:
                case byte_stack.Bcall+5:
                    op -= byte_stack.Bcall;
                    docall:
                    {
                        stack.DISCARD (op);
                        LispObject[] args = new LispObject[op + 1];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, op + 1);
                        stack.TOP = F.funcall (op + 1, args);
                        break;
                    }

                case byte_stack.Bunbind+6:
                    op = stack.FETCH();
                    goto dounbind;

                case byte_stack.Bunbind+7:
                    op = stack.FETCH2();
                    goto dounbind;

                case byte_stack.Bunbind:
                case byte_stack.Bunbind+1:
                case byte_stack.Bunbind+2:
                case byte_stack.Bunbind+3:
                case byte_stack.Bunbind+4:
                case byte_stack.Bunbind+5:
                    op -= byte_stack.Bunbind;
                    dounbind:
                    L.unbind_to (L.SPECPDL_INDEX () - op, Q.nil);
                    break;

                case byte_stack.Bunbind_all:
                    /* To unbind back to the beginning of this frame.  Not used yet,
                       but will be needed for tail-recursion elimination.  */
                    L.unbind_to (count, Q.nil);
                    break;

                case byte_stack.Bgoto:
                    L.BYTE_CODE_QUIT();
                    op = stack.FETCH2();    /* pc = FETCH2 loses since FETCH2 contains pc++ */
                    stack.pc = op;
                    break;

                case byte_stack.Bgotoifnonnil:
                    {
                        LispObject v1;
                        op = stack.FETCH2();
                        v1 = stack.POP();
                        if (!L.NILP (v1))
                        {
                            L.BYTE_CODE_QUIT();
                            stack.pc = op;
                        }
                        break;
                    }

                case byte_stack.Bgotoifnilelsepop:
                    op = stack.FETCH2();
                    if (L.NILP (stack.TOP))
                    {
                        L.BYTE_CODE_QUIT();
                        stack.pc = op;
                    }
                    else stack.DISCARD (1);
                    break;

                case byte_stack.Bgotoifnonnilelsepop:
                    op = stack.FETCH2();
                    if (!L.NILP (stack.TOP))
                    {
                        L.BYTE_CODE_QUIT();
                        stack.pc = op;
                    }
                    else stack.DISCARD (1);
                    break;

                case byte_stack.BRgoto:
                    L.BYTE_CODE_QUIT();
                    stack.pc += (int) stack.PEEK() - 127;
                    break;

                case byte_stack.BRgotoifnil:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        if (L.NILP (v1))
                        {
                            L.BYTE_CODE_QUIT();
                            stack.pc += (int) stack.PEEK() - 128;
                        }
                        stack.pc++;
                        break;
                    }

                case byte_stack.BRgotoifnonnil:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        if (!L.NILP (v1))
                        {
                            L.BYTE_CODE_QUIT();
                            stack.pc += (int) stack.PEEK() - 128;
                        }
                        stack.pc++;
                        break;
                    }

                case byte_stack.BRgotoifnilelsepop:
                    op = stack.FETCH();
                    if (L.NILP (stack.TOP))
                    {
                        L.BYTE_CODE_QUIT();
                        stack.pc += op - 128;
                    }
                    else stack.DISCARD (1);
                    break;

                case byte_stack.BRgotoifnonnilelsepop:
                    op = stack.FETCH();
                    if (!L.NILP (stack.TOP))
                    {
                        L.BYTE_CODE_QUIT();
                        stack.pc += op - 128;
                    }
                    else stack.DISCARD (1);
                    break;

                case byte_stack.Breturn:
                    result = stack.POP();
                    goto exit;

                case byte_stack.Bdiscard:
                    stack.DISCARD (1);
                    break;

                case byte_stack.Bconstant2:
                    stack.PUSH (vectorp[stack.FETCH2()]);
                    break;

                case byte_stack.Bsave_excursion:
                    L.record_unwind_protect (L.save_excursion_restore,
                                             L.save_excursion_save ());
                    break;

                case byte_stack.Bsave_current_buffer:
                case byte_stack.Bsave_current_buffer_1:
                    L.record_unwind_protect (L.set_buffer_if_live, F.current_buffer ());
                    break;

                case byte_stack.Bsave_window_excursion:
                    stack.TOP = F.save_window_excursion (stack.TOP);
                    break;

                case byte_stack.Bsave_restriction:
                    L.record_unwind_protect (L.save_restriction_restore,
                                             L.save_restriction_save ());
                    break;

                case byte_stack.Bcatch:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = L.internal_catch (stack.TOP, F.eval, v1);
                        break;
                    }

                case byte_stack.Bunwind_protect:
                    L.record_unwind_protect (F.progn, stack.POP());
                    break;

                case byte_stack.Bcondition_case:
                    {
                        LispObject handlers, body;
                        handlers = stack.POP();
                        body = stack.POP();

                        stack.TOP = L.internal_lisp_condition_case (stack.TOP, body, handlers);
                        break;
                    }

                case byte_stack.Btemp_output_buffer_setup:
                    L.CHECK_STRING (stack.TOP);
                    L.temp_output_buffer_setup (System.Text.Encoding.UTF8.GetString(L.SDATA (stack.TOP)));
                    stack.TOP = V.standard_output;
                    break;

                case byte_stack.Btemp_output_buffer_show:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        L.temp_output_buffer_show (stack.TOP);
                        stack.TOP = v1;
                        /* pop binding of standard-output */
                        L.unbind_to (L.SPECPDL_INDEX () - 1, Q.nil);
                        break;
                    }

                case byte_stack.Bnth:
                    {
                        LispObject v1, v2;
                        v1 = stack.POP();
                        v2 = stack.TOP;
                        L.CHECK_NUMBER (v2);

                        op = L.XINT (v2);
                        L.immediate_quit = 1;
                        while (--op >= 0 && L.CONSP (v1))
                            v1 = L.XCDR (v1);
                        L.immediate_quit = 0;
                        stack.TOP = L.CAR (v1);
                        break;
                    }

                case byte_stack.Bsymbolp:
                    stack.TOP = L.SYMBOLP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case byte_stack.Bconsp:
                    stack.TOP = L.CONSP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case byte_stack.Bstringp:
                    stack.TOP = L.STRINGP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case byte_stack.Blistp:
                    stack.TOP = L.CONSP (stack.TOP) || L.NILP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case byte_stack.Bnot:
                    stack.TOP = L.NILP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case byte_stack.Bcons:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.cons (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Blist1:
                    stack.TOP = F.cons (stack.TOP, Q.nil);
                    break;

                case byte_stack.Blist2:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.cons (stack.TOP, F.cons (v1, Q.nil));
                        break;
                    }

                case byte_stack.Blist3:
                    {
                        stack.DISCARD(2);
                        LispObject[] args = new LispObject[3];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 3);
                        stack.TOP = F.list(3, args);
                    }
                    break;

                case byte_stack.Blist4:
                    {
                        stack.DISCARD(3);
                        LispObject[] args = new LispObject[4];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 4);
                        stack.TOP = F.list(4, args);
                    }
                    break;

                case byte_stack.BlistN:
                    {
                        op = stack.FETCH();
                        stack.DISCARD(op - 1);
                        LispObject[] args = new LispObject[op];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, op);
                        stack.TOP = F.list(op, args);
                    }
                    break;

                case byte_stack.Blength:
                    stack.TOP = F.length (stack.TOP);
                    break;

                case byte_stack.Baref:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.aref(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Baset:
                    {
                        LispObject v1, v2;
                        v2 = stack.POP(); v1 = stack.POP();
                        stack.TOP = F.aset(stack.TOP, v1, v2);
                        break;
                    }

                case byte_stack.Bsymbol_value:
                    stack.TOP = F.symbol_value (stack.TOP);
                    break;

                case byte_stack.Bsymbol_function:
                    stack.TOP = F.symbol_function (stack.TOP);
                    break;

                case byte_stack.Bset:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.set(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bfset:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.fset (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bget:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.get(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bsubstring:
                    {
                        LispObject v1, v2;
                        v2 = stack.POP(); v1 = stack.POP();
                        stack.TOP = F.substring(stack.TOP, v1, v2);
                        break;
                    }

                case byte_stack.Bconcat2:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.concat(2, args);
                    }
                    break;

                case byte_stack.Bconcat3:
                    {
                        stack.DISCARD(2);
                        LispObject[] args = new LispObject[3];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 3);
                        stack.TOP = F.concat(3, args);
                    }
                    break;

                case byte_stack.Bconcat4:
                    {
                        stack.DISCARD(3);
                        LispObject[] args = new LispObject[4];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 4);
                        stack.TOP = F.concat(4, args);
                    }
                    break;

                case byte_stack.BconcatN:
                    {
                        op = stack.FETCH();
                        stack.DISCARD(op - 1);
                        LispObject[] args = new LispObject[op];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, op);
                        stack.TOP = F.concat(op, args);
                    }
                    break;

                case byte_stack.Bsub1:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        if (L.INTEGERP (v1))
                        {
                            v1 = L.XSETINT (L.XINT (v1) - 1);
                            stack.TOP = v1;
                        }
                        else
                        {
                            stack.TOP = F.sub1 (v1);
                        }
                        break;
                    }

                case byte_stack.Badd1:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        if (L.INTEGERP (v1))
                        {
                            v1 = L.XSETINT (L.XINT (v1) + 1);
                            stack.TOP = v1;
                        }
                        else
                        {
                            stack.TOP = F.add1 (v1);
                        }
                        break;
                    }

                case byte_stack.Beqlsign:
                    {
                        LispObject v1, v2;
                        v2 = stack.POP(); v1 = stack.TOP;
                        L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref v1);
                        L.CHECK_NUMBER_OR_FLOAT_COERCE_MARKER(ref v2);

                        if (L.FLOATP (v1) || L.FLOATP (v2))
                        {
                            double f1, f2;

                            f1 = (L.FLOATP (v1) ? L.XFLOAT_DATA (v1) : L.XINT (v1));
                            f2 = (L.FLOATP (v2) ? L.XFLOAT_DATA (v2) : L.XINT (v2));
                            stack.TOP = (f1 == f2 ? Q.t : Q.nil);
                        }
                        else
                            stack.TOP = (L.XINT(v1) == L.XINT(v2) ? Q.t : Q.nil);
                        break;
                    }

                case byte_stack.Bgtr:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.gtr (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Blss:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.lss (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bleq:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.leq(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bgeq:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.geq(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bdiff:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.minus(2, args);
                        break;
                    }

                case byte_stack.Bnegate:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        if (L.INTEGERP (v1))
                        {
                            v1 = L.XSETINT(-L.XINT(v1));
                            stack.TOP = v1;
                        }
                        else
                        {
                            stack.TOP = F.minus (1, stack.TOP);
                        }
                        break;
                    }

                case byte_stack.Bplus:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.plus(2, args);
                        break;
                    }

                case byte_stack.Bmax:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.max(2, args);
                        break;
                    }

                case byte_stack.Bmin:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.min(2, args);
                        break;
                    }

                case byte_stack.Bmult:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.times(2, args);
                        break;
                    }

                case byte_stack.Bquo:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.quo(2, args);
                        break;
                    }

                case byte_stack.Brem:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.rem (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bpoint:
                    {
                        LispObject v1;
                        v1 = L.XSETINT (L.PT());
                        stack.PUSH(v1);
                        break;
                    }

                case byte_stack.Bgoto_char:
                    {
                        stack.TOP = F.goto_char(stack.TOP);
                    }
                    break;

                case byte_stack.Binsert:
                    {
                        stack.TOP = F.insert(1, stack.TOP);
                    }
                    break;

                case byte_stack.BinsertN:
                    {
                        op = stack.FETCH();
                        stack.DISCARD(op - 1);
                        LispObject[] args = new LispObject[op];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, op);
                        stack.TOP = F.insert(op, args);
                    }
                    break;

                case byte_stack.Bpoint_max:
                    {
                        LispObject v1;
                        v1 = L.XSETINT (L.ZV);
                        stack.PUSH(v1);
                        break;
                    }

                case byte_stack.Bpoint_min:
                    {
                        LispObject v1;
                        v1 = L.XSETINT(L.BEGV());
                        stack.PUSH(v1);
                        break;
                    }

                case byte_stack.Bchar_after:
                    stack.TOP = F.char_after(stack.TOP);
                    break;

                case byte_stack.Bfollowing_char:
                    {
                        LispObject v1;
                        v1 = F.following_char ();
                        stack.PUSH (v1);
                        break;
                    }

                case byte_stack.Bpreceding_char:
                    {
                        LispObject v1;
                        v1 = F.previous_char ();
                        stack.PUSH (v1);
                        break;
                    }

                case byte_stack.Bcurrent_column:
                    {
                        LispObject v1;
                        v1 = L.XSETINT ((int) L.current_column ()); /* iftc */
                        stack.PUSH(v1);
                        break;
                    }

                case byte_stack.Bindent_to:
                    stack.TOP = F.indent_to (stack.TOP, Q.nil);
                    break;

                case byte_stack.Beolp:
                    stack.PUSH (F.eolp ());
                    break;

                case byte_stack.Beobp:
                    stack.PUSH (F.eobp ());
                    break;

                case byte_stack.Bbolp:
                    stack.PUSH (F.bolp ());
                    break;

                case byte_stack.Bbobp:
                    stack.PUSH (F.bobp ());
                    break;

                case byte_stack.Bcurrent_buffer:
                    stack.PUSH (F.current_buffer ());
                    break;

                case byte_stack.Bset_buffer:
                    stack.TOP = F.set_buffer (stack.TOP);
                    break;

                case byte_stack.Binteractive_p:
                    stack.PUSH(F.interactive_p());
                    break;

                case byte_stack.Bforward_char:
                    stack.TOP = F.forward_char(stack.TOP);
                    break;

                case byte_stack.Bforward_word:
                    stack.TOP = F.forward_word (stack.TOP);
                    break;

                case byte_stack.Bskip_chars_forward:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.skip_chars_forward(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bskip_chars_backward:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.skip_chars_backward(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bforward_line:
                    stack.TOP = F.forward_line (stack.TOP);
                    break;

                case byte_stack.Bchar_syntax:
                    {
                        int c;

                        L.CHECK_CHARACTER (stack.TOP);
                        c = L.XINT (stack.TOP);
                        if (L.NILP (L.current_buffer.enable_multibyte_characters))
                            L.MAKE_CHAR_MULTIBYTE (ref c);
                        stack.TOP = L.XSETINT (L.syntax_code_spec[(int) L.SYNTAX ((uint) c)]);
                    }
                    break;

                case byte_stack.Bbuffer_substring:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.buffer_substring (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bdelete_region:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.delete_region(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bnarrow_to_region:
                    {
                        LispObject v1;
                        v1 = stack.POP();
                        stack.TOP = F.narrow_to_region(stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bwiden:
                    stack.PUSH (F.widen ());
                    break;

                case byte_stack.Bend_of_line:
                    stack.TOP = F.end_of_line (stack.TOP);
                    break;

                case byte_stack.Bset_marker:
                    {
                        LispObject v1, v2;
                        v1 = stack.POP();
                        v2 = stack.POP();
                        stack.TOP = F.set_marker(stack.TOP, v2, v1);
                        break;
                    }

                case byte_stack.Bmatch_beginning:
                    stack.TOP = F.match_beginning (stack.TOP);
                    break;

                case byte_stack.Bmatch_end:
                    stack.TOP = F.match_end (stack.TOP);
                    break;

                case byte_stack.Bupcase:
                    stack.TOP = F.upcase (stack.TOP);
                    break;

                case byte_stack.Bdowncase:
                    stack.TOP = F.downcase (stack.TOP);
                    break;

                case byte_stack.Bstringeqlsign:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.string_equal (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bstringlss:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.string_lessp (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bequal:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.equal (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bnthcdr:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.nthcdr (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Belt:
                    {
                        LispObject v1, v2;
                        if (L.CONSP (stack.TOP))
                        {
                            /* Exchange args and then do nth.  */
                            v2 = stack.POP ();
                            v1 = stack.TOP;
                            L.CHECK_NUMBER (v2);

                            op = L.XINT (v2);
                            L.immediate_quit = 1;
                            while (--op >= 0 && L.CONSP (v1))
                                v1 = L.XCDR (v1);
                            L.immediate_quit = 0;
                            stack.TOP = L.CAR (v1);
                        }
                        else
                        {
                            v1 = stack.POP ();
                            stack.TOP = F.elt (stack.TOP, v1);
                        }
                        break;
                    }

                case byte_stack.Bmember:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.member (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bassq:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.assq (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bnreverse:
                    stack.TOP = F.nreverse (stack.TOP);
                    break;

                case byte_stack.Bsetcar:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.setcar (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bsetcdr:
                    {
                        LispObject v1;
                        v1 = stack.POP ();
                        stack.TOP = F.setcdr (stack.TOP, v1);
                        break;
                    }

                case byte_stack.Bcar_safe:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        stack.TOP = L.CAR_SAFE (v1);
                        break;
                    }

                case byte_stack.Bcdr_safe:
                    {
                        LispObject v1;
                        v1 = stack.TOP;
                        stack.TOP = L.CDR_SAFE (v1);
                        break;
                    }

                case byte_stack.Bnconc:
                    {
                        stack.DISCARD(1);
                        LispObject[] args = new LispObject[2];
                        System.Array.Copy(stack.bottom, stack.top, args, 0, 2);
                        stack.TOP = F.nconc(2, args);
                    }
                    break;

                case byte_stack.Bnumberp:
                    stack.TOP = (L.NUMBERP (stack.TOP) ? Q.t : Q.nil);
                    break;

                case byte_stack.Bintegerp:
                    stack.TOP = L.INTEGERP (stack.TOP) ? Q.t : Q.nil;
                    break;

                case 0:
                    L.abort();
                    break;
                case 255:
                default:
                    stack.PUSH (vectorp[op - byte_stack.Bconstant]);
                    break;
                }
            }

            exit:

            L.byte_stack_list = L.byte_stack_list.next;

            /* Binds and unbinds are supposed to be compiled balanced.  */
            if (L.SPECPDL_INDEX () != count)
                L.abort();

            return result;
        }
    }

    public partial class Q
    {
        public static LispObject bytecode;
    }
}
