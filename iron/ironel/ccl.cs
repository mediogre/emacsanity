namespace IronElisp
{
/* Structure to hold information about running CCL code.  Read
   comments in the file ccl.c for the detail of each field.  */
    public class ccl_program
    {
        int idx;			/* Index number of the CCL program.
				   -1 means that the program was given
				   by a vector, not by a program
				   name.  */
        int size;			/* Size of the compiled code.  */
        LispObject prog;		/* Pointer into the compiled code.  */
        int ic;			/* Instruction Counter (index for PROG).  */
        int eof_ic;			/* Instruction Counter for end-of-file
				   processing code.  */
        int[] reg = new int[8];			/* CCL registers, reg[7] is used for
				   condition flag of relational
				   operations.  */
        int private_state;            /* CCL instruction may use this
				   for private use, mainly for saving
				   internal states on suspending.
				   This variable is set to 0 when ccl is
				   set up.  */
        int last_block;		/* Set to 1 while processing the last
				   block. */
        int status;			/* Exit status of the CCL program.  */
        int buf_magnification;	/* Output buffer magnification.  How
				   many times bigger the output buffer
				   should be than the input buffer.  */
        int stack_idx;		/* How deep the call of CCL_Call is nested.  */
        int src_multibyte;		/* 1 if the input buffer is multibyte.  */
        int dst_multibyte;		/* 1 if the output buffer is multibyte.  */
        int cr_consumed;		/* Flag for encoding DOS-like EOL
				   format when the CCL program is used
				   for encoding by a coding
				   system.  */
        int consumed;
        int produced;
        int suppress_error;		/* If nonzero, don't insert error
				   message in the output.  */
        int eight_bit_control;	/* If nonzero, ccl_driver counts all
				   eight-bit-control bytes written by
				   CCL_WRITE_CHAR.  After execution,
				   if no such byte is written, set
				   this value to zero.  */
        int quit_silently;		/* If nonzero, don't append "CCL:
				   Quitted" to the generated text when
				   CCL program is quitted. */
    }
}