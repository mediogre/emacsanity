namespace IronElisp
{
    class LispObject
    {
    }

    class LispSymbol : LispObject
    {
        internal enum symbol_interned
        {
            SYMBOL_UNINTERNED = 0,
            SYMBOL_INTERNED = 1,
            SYMBOL_INTERNED_IN_INITIAL_OBARRAY = 2
        }

        bool is_indirect_variable;
        bool is_constant;
        symbol_interned interned;

        LispObject xname;
        LispObject value;
        LispObject function;
        LispObject plist;

        LispObject next;

        internal LispSymbol(LispString name) : this(name, symbol_interned.SYMBOL_UNINTERNED)
        {
        }

        internal LispSymbol(LispString name, symbol_interned itype)
        {
            xname = name;
            plist = Q.nil;
            value = Q.unbound;
            function = Q.unbound;
            next = null;
            interned = itype;
            is_constant = false;
            is_indirect_variable = false;
        }

        internal symbol_interned Interned
        {
            set
            {
                interned = value;
            }
        }

        internal bool Constant
        {
            get { return is_constant; }
            set { is_constant = value; }
        }

        internal LispObject Value
        {
            get { return value; }
            set { this.value = value; }
        }

        internal LispObject Plist
        {
            get { return plist; }
            set { plist = value; }
        }

        internal LispObject Function
        {
            get { return function; }
            set { function = value; }
        }
    }

    class LispString : LispObject
    {
        // int size;
        // int size_byte;
        Interval intervals;

        // should this be a byte array???
        string data;

        internal LispString(string str)
        {
            data = str;
        }

        internal string Sdata
        {
            get
            {
                return data;
            }
        }
    }

    class LispVector : LispObject
    {
        System.Collections.Generic.List<LispObject> contents;
    }

    class LispVectorLike : LispObject
    {
    }

    class LispSubr : LispVectorLike
    {
        // do we need this???
       int size;
       delegate LispObject function();
       int min_args, max_args;
       string symbol_name;
       string intspec;
       string doc;
    }
    
    class LispCons : LispObject
    {
        LispObject car_;
        LispObject cdr_;

        LispCons(LispObject first, LispObject rest)
        {
            car_ = first;
            cdr_ = rest;
        }

        LispObject Car
        {
            get { return car_; }
            set { car_ = value; }
        }

        LispObject Cdr
        {
            get { return cdr_; }
            set { cdr_ = value; }
        }        
    }

    // GNUEmacs uses vectors with hashed buckets of single-linked symbols
    // We just use a hashtable here - it's 21st century, right?
    class LispHash : LispObject
    {
        System.Collections.Generic.Dictionary<string, LispSymbol> table;

        public LispHash()
        {
            table = new System.Collections.Generic.Dictionary<string, LispSymbol>();
        }

        public LispSymbol this[string index]
        {
            get
            {
                return table[index];
            }

            set
            {
                table[index] = value;
            }
        }

        public int Size
        {
            get
            {
                return table.Count;
            }
        }
    }
}