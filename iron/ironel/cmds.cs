namespace IronElisp
{
    public partial class F
    {
        public static LispObject forward_char(LispObject n)
        {
            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            /* This used to just set point to point + XINT (n), and then check
               to see if it was within boundaries.  But now that SET_PT can
               potentially do a lot of stuff (calling entering and exiting
               hooks, etcetera), that's not a good approach.  So we validate the
               proposed position, then set point.  */
            {
                int new_point = L.PT() + L.XINT(n);

                if (new_point < L.BEGV())
                {
                    L.SET_PT(L.BEGV());
                    L.xsignal0(Q.beginning_of_buffer);
                }
                if (new_point > L.ZV)
                {
                    L.SET_PT(L.ZV);
                    L.xsignal0(Q.end_of_buffer);
                }

                L.SET_PT(new_point);
            }

            return Q.nil;
        }

        public static LispObject backward_char(LispObject n)
        {
            if (L.NILP(n))
                n = L.XSETINT(1);
            else
                L.CHECK_NUMBER(n);

            n = L.XSETINT(-L.XINT(n));
            return F.forward_char(n);
        }
    }
}