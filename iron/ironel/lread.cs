namespace IronElisp
{
    class LispReader
    {
        internal static LispHash initial_obarray;

        internal static void init_obarray()
        {
            initial_obarray = V.obarray = new LispHash();

            Q.nil = F.make_symbol(Alloc.make_string("nil"));
            Q.nil.Interned = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            Q.nil.Constant = true;
            Q.nil.Value = Q.nil;
            Q.nil.Function = Q.unbound;
            Q.nil.Plist = Q.nil;

            V.obarray["nil"] = Q.nil;

            Q.unbound = F.make_symbol(Alloc.make_string("unbound"));
            Q.unbound.Value = Q.unbound;
            Q.unbound.Function = Q.unbound;

            Q.t = intern("t");
            Q.t.Value = Q.t;
            Q.t.Constant = true;

            Q.variable_documentation = intern("variable-documentation");

            // init read buffer?
        }

        internal static LispSymbol oblookup(LispHash obarray, string str)
        {
            return obarray[str];
        }

        internal static LispSymbol intern(string str)
        {
            LispHash obarray = V.obarray;
            obarray = check_obarray(obarray);

            LispSymbol tem = oblookup(obarray, str);

            if (tem != null)
                return tem;

            return F.intern(Alloc.make_string(str), obarray);
        }

        internal static LispHash check_obarray(LispObject obarray)
        {
/* need to check if V.obarray is invalid and set it ot initial obarray
            if (h.Size == 0)
            {
                if (ReferenceEquals(h, V.obarray))
                {
                    V.obarray = initial_obarray;
                }
            }
*/

            return obarray as LispHash;
        }
    }

    partial class V
    {
        internal static LispHash obarray;
    }

    partial class F
    {
        internal static LispSymbol intern(LispObject str, LispObject obarray)
        {
            if (obarray == null)
            {
                obarray = V.obarray;
            }

            LispHash hash = LispReader.check_obarray(obarray);
            LispString s  = str as LispString;

            LispSymbol sym = hash[s.Sdata];
            if (sym != null)
            {
                return sym;
            }

            LispSymbol.symbol_interned intern_type = LispSymbol.symbol_interned.SYMBOL_INTERNED;
            if (ReferenceEquals(hash, LispReader.initial_obarray))
            {
                intern_type = LispSymbol.symbol_interned.SYMBOL_INTERNED_IN_INITIAL_OBARRAY;
            }

            sym = F.make_symbol(s);
            sym.Interned = intern_type;

            if (ReferenceEquals(hash, LispReader.initial_obarray) && s.Sdata.Length > 0 && s.Sdata[0] == ':')
            {
                sym.Constant = true;
                sym.Value = sym;
            }

            hash[s.Sdata] = sym;
            return sym;
        }
    }
}