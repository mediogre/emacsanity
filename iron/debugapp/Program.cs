using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IronElisp;

namespace debugapp
{
    class Program
    {
        static void Main(string[] args)
        {
            L.init_obarray();
            IronElisp.LispObject r = IronElisp.F.read(IronElisp.L.build_string("(10 20 42)"));

            Console.WriteLine("{0} {1} {2} {3}",
                              ((LispInt)F.car(r)).val,
                              ((LispInt)F.car(F.cdr(r))).val,
                              ((LispInt)F.car(F.cdr(F.cdr(r)))).val,
                              ((LispInt)F.length(r)).val);
        }
    }
}
