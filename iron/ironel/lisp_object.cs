namespace IronElisp
{
    public interface Indexable<T>
    {
        T this[int index]
        {
            get;
            set;
        }
    }


    public interface LispObject
    {
    }

    public class LispInt : LispObject
    {
        public int val;

        public LispInt(int x)
        {
            val = x;
        }
    }

    public class LispFloat : LispObject
    {
        public double val;

        public LispFloat(double x)
        {
            val = x;
        }
    }

    public class LispSymbol : LispObject
    {
        public enum symbol_interned
        {
            SYMBOL_UNINTERNED = 0,
            SYMBOL_INTERNED = 1,
            SYMBOL_INTERNED_IN_INITIAL_OBARRAY = 2
        }

        bool is_indirect_variable;
        bool is_constant;
        symbol_interned interned;

        public LispObject xname;
        LispObject value;
        public LispObject function;
        public LispObject plist;

        public LispObject next;

        public LispSymbol(LispString name)
            : this(name, symbol_interned.SYMBOL_UNINTERNED)
        {
        }

        public LispSymbol(LispString name, symbol_interned itype)
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

        public symbol_interned Interned
        {
            get
            {
                return interned;
            }

            set
            {
                interned = value;
            }
        }

        public bool Constant
        {
            get { return is_constant; }
            set { is_constant = value; }
        }

        public LispObject Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public LispObject Plist
        {
            get { return plist; }
            set { plist = value; }
        }

        public LispObject Function
        {
            get { return function; }
            set { function = value; }
        }

        public bool IsIndirectVariable
        {
            get
            {
                return is_indirect_variable;
            }

            set
            {
                is_indirect_variable = value;
            }
        }
    }

    public class LispString : LispObject
    {
        int size;
        int size_byte;
        public Interval intervals;

        byte[] data;

        public LispString(int nchars, int nbytes)
        {
            size = nchars;
            size_byte = nbytes;

            data = new byte[nbytes];
        }

        public byte[] SData
        {
            get
            {
                return data;
            }
        }

        public int Size
        {
            get { return size; }
            set { size = value; }
        }

        public int SizeBytes
        {
            get
            {
                return size_byte < 0 ? size : size_byte;
            }

            set
            {
                size_byte = value;
            }
        }

        public void bcopy(byte[] contents, int length)
        {
            System.Array.Copy(contents, data, length);
        }

        public void bcopy(byte[] contents, int to, int length)
        {
            System.Array.Copy(contents, 0, data, to, length);
        }

        public void bcopy(byte[] contents, int from, int to, int length)
        {
            System.Array.Copy(contents, from, data, to, length);
        }

        public bool bcmp(byte[] other)
        {
            if (ReferenceEquals(data, other))
                return true;

            if (other == null)
                return false;

            if (other.Length != data.Length)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != other[i])
                    return false;
            }

            return true;
        }
    }

    public class LispCompiled : LispVector
    {
        public const int COMPILED_ARGLIST = 0;
        public const int COMPILED_BYTECODE = 1;
        public const int COMPILED_CONSTANTS = 2;
        public const int COMPILED_STACK_DEPTH = 3;
        public const int COMPILED_DOC_STRING = 4;
        public const int COMPILED_INTERACTIVE = 5;

        public LispCompiled(int size) : base (size)
        {
        }
    }

    public class LispMisc : LispObject
    {
    }

    public class LispMarker : LispMisc
    {
        /* This flag is temporarily used in the functions
           decode/encode_coding_object to record that the marker position
           must be adjusted after the conversion.  */
        public bool need_adjustment;

        /* 1 means normal insertion at the marker's position
           leaves the marker after the inserted text.  */
        public bool insertion_type;

        /* This is the buffer that the marker points into, or 0 if it points nowhere.
           Note: a chain of markers can contain markers pointing into different
           buffers (the chain is per buffer_text rather than per buffer, so it's
           shared between indirect buffers).  */
        /* This is used for (other than NULL-checking):
           - Fmarker_buffer
           - Fset_marker: check eq(oldbuf, newbuf) to avoid unchain+rechain.
           - unchain_marker: to find the list from which to unchain.
           - Fkill_buffer: to unchain the markers of current indirect buffer.
        */
        public Buffer buffer;

        /* The remaining fields are meaningless in a marker that
           does not point anywhere.  */

        /* For markers that point somewhere,
           this is used to chain of all the markers in a given buffer.  */
        /* We could remove it and use an array in buffer_text instead.
           That would also allow to preserve it ordered.  */
        public LispMarker next;

        /* This is the char position where the marker points.  */
        public int charpos;

        /* This is the byte position.  */
        public int bytepos;
    }

    /* Forwarding pointer to an int variable.
       This is allowed only in the value cell of a symbol,
       and it means that the symbol's value really lives in the
       specified int variable.  */
    public class LispIntFwd : LispMisc
    {
        private Indexable<int> target;
        private int index;

        public LispIntFwd(Indexable<int> t, int i)
        {
            target = t;
            index = i;
        }

        public int intvar
        {
            get { return target[index]; }
            set { target[index] = value; }
        }
    }

    /* Boolean forwarding pointer to an int variable.
       This is like Lisp_Intfwd except that the ostensible
       "value" of the symbol is t if the int variable is nonzero,
       nil if it is zero.  */
    public class  LispBoolFwd : LispMisc
    {
        private Indexable<bool> target;
        private int index;

        public LispBoolFwd(Indexable<bool> t, int i)
        {
            target = t;
            index = i;
        }

        public bool boolvar
        {
            get { return target[index]; }
            set { target[index] = value; }
        }
    }
    
    /* Forwarding pointer to a Lisp_Object variable.
       This is allowed only in the value cell of a symbol,
       and it means that the symbol's value really lives in the
       specified variable.  */
    public class LispObjFwd : LispMisc
    {
        private Indexable<LispObject> target;
        private int index;
        private bool is_default;

        public LispObjFwd(Indexable<LispObject> t, int i)
            : this(t, i, false)
        {
        }

        public LispObjFwd(Indexable<LispObject> t, int i, bool def)
        {
            target = t;
            index = i;
            is_default = def;
        }

        // This property provides the indirection level needed for forwarding symbol's value
        public LispObject objvar
        {
            get
            {
                return target[index];
            }

            set
            {
                target[index] = value;
            }
        }

        public int Offset
        {
            get { return index; }
        }

        // Is this forwarding goes to default buffer value?
        public bool IsDefault
        {
            get { return is_default; }
        }
    }

    /* Like Lisp_Objfwd except that value lives in a slot in the
       current buffer.  Value is byte index of slot within buffer.  */
    public class LispBufferObjFwd : LispMisc
    {
        public System.Type slottype; /* Qnil, Lisp_Int, Lisp_Symbol, or Lisp_String.  */
        public int offset;
    }

    /* struct Lisp_Buffer_Local_Value is used in a symbol value cell when
       the symbol has buffer-local or frame-local bindings.  (Exception:
       some buffer-local variables are built-in, with their values stored
       in the buffer structure itself.  They are handled differently,
       using struct Lisp_Buffer_Objfwd.)

       The `realvalue' slot holds the variable's current value, or a
       forwarding pointer to where that value is kept.  This value is the
       one that corresponds to the loaded binding.  To read or set the
       variable, you must first make sure the right binding is loaded;
       then you can access the value in (or through) `realvalue'.

       `buffer' and `frame' are the buffer and frame for which the loaded
       binding was found.  If those have changed, to make sure the right
       binding is loaded it is necessary to find which binding goes with
       the current buffer and selected frame, then load it.  To load it,
       first unload the previous binding, then copy the value of the new
       binding into `realvalue' (or through it).  Also update
       LOADED-BINDING to point to the newly loaded binding.

       `local_if_set' indicates that merely setting the variable creates a local
       binding for the current buffer.  Otherwise the latter, setting the
       variable does not do that; only make-local-variable does that.  */
    public class LispBufferLocalValue : LispMisc
    {
        /* 1 means that merely setting the variable creates a local
           binding for the current buffer */
        public bool local_if_set;

        /* 1 means this variable is allowed to have frame-local bindings,
           so check for them when looking for the proper binding.  */
        public bool check_frame;

        /* 1 means that the binding now loaded was found
           as a local binding for the buffer in the `buffer' slot.  */
        public bool found_for_buffer;

        /* 1 means that the binding now loaded was found
           as a local binding for the frame in the `frame' slot.  */
        public bool found_for_frame;

        public LispObject realvalue;

        /* The buffer and frame for which the loaded binding was found.  */
        /* Having both is only needed if we want to allow variables that are
           both buffer local and frame local (in which case, we currently give
           precedence to the buffer-local binding).  I don't think such
           a combination is desirable.  --Stef  */
        public LispObject buffer, frame;

        /* A cons cell, (LOADED-BINDING . DEFAULT-VALUE).

           LOADED-BINDING is the binding now loaded.  It is a cons cell
           whose cdr is the binding's value.  The cons cell may be an
           element of a buffer's local-variable alist, or an element of a
           frame's parameter alist, or it may be this cons cell.

           DEFAULT-VALUE is the variable's default value, seen when the
           current buffer and selected frame do not have their own
           bindings for the variable.  When the default binding is loaded,
           LOADED-BINDING is actually this very cons cell; thus, its car
           points to itself.  */
        public LispObject cdr;
    }

    /* START and END are markers in the overlay's buffer, and
       PLIST is the overlay's property list.  */
    public class LispOverlay : LispMisc
    {
        LispOverlay next;
        public LispObject start, end, plist;
    }

    /* Like Lisp_Objfwd except that value lives in a slot in the
       current kboard.  */
    public class LispKboardObjFwd : LispMisc
    {
        public int offset;
    }

    /* Hold a C pointer for later use.
       This type of object is used in the arg to record_unwind_protect.  */
    public class LispSaveValue : LispMisc
    {
        /* If DOGC is set, POINTER is the address of a memory
           area containing INTEGER potential Lisp_Objects.  */
        bool dogc;
        System.Object pointer;
        int integer;
    }

    public interface LispVectorLike<T> : LispObject
    {
        int Size
        {
            get;
        }

        T this[int index]
        {
            get;
            set;
        }
    }

    public class LispVector : LispVectorLike<LispObject>
    {
        private LispObject[] contents;

        public LispVector(int size)
        {
            contents = new LispObject[size];
        }

        public int Size
        {
            get { return contents.Length; }
        }

        public LispObject this[int index]
        {
            get
            {
                return contents[index];
            }

            set
            {
                contents[index] = value;
            }
        }

        // make a copy of our list
        public LispObject[] Contents
        {
            get
            {
                return contents;
            }
        }
    }

    public delegate LispObject subr0();
    public delegate LispObject subr1(LispObject a1);
    public delegate LispObject subr2(LispObject a1, LispObject a2);
    public delegate LispObject subr3(LispObject a1, LispObject a2, LispObject a3);
    public delegate LispObject subr4(LispObject a1, LispObject a2, LispObject a3, LispObject a4);
    public delegate LispObject subr5(LispObject a1, LispObject a2, LispObject a3, LispObject a4, LispObject a5);
    public delegate LispObject subr6(LispObject a1, LispObject a2, LispObject a3, LispObject a4, LispObject a5, LispObject a6);
    public delegate LispObject subr7(LispObject a1, LispObject a2, LispObject a3, LispObject a4, LispObject a5, LispObject a6, LispObject a7);
    public delegate LispObject subr8(LispObject a1, LispObject a2, LispObject a3, LispObject a4, LispObject a5, LispObject a6, LispObject a7, LispObject a8);

    public delegate LispObject subr_many(int num_args, params LispObject[] args);

    public class LispSubr : LispObject
    {
        public const int UNEVALLED = -1;
        public const int MANY = -2;

        public int min_args, max_args;
        public string symbol_name;
        public string intspec;
        public string doc;

        public subr0 function0;
        public subr1 function1;
        public subr2 function2;
        public subr3 function3;
        public subr4 function4;
        public subr5 function5;
        public subr6 function6;
        public subr7 function7;
        public subr8 function8;
        public subr_many function_many;

        public LispSubr(string name, subr0 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function0 = fn;
        }

        public LispSubr(string name, subr1 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function1 = fn;
        }

        public LispSubr(string name, subr2 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function2 = fn;
        }

        public LispSubr(string name, subr3 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function3 = fn;
        }

        public LispSubr(string name, subr4 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function4 = fn;
        }

        public LispSubr(string name, subr5 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function5 = fn;
        }

        public LispSubr(string name, subr6 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function6 = fn;
        }

        public LispSubr(string name, subr7 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function7 = fn;
        }

        public LispSubr(string name, subr8 fn, int min, int max, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = max;
            intspec = int_spec;
            doc = doc_string;

            function8 = fn;
        }

        public LispSubr(string name, subr_many fn, int min, string int_spec, string doc_string)
        {
            symbol_name = name;
            min_args = min;
            max_args = MANY;
            intspec = int_spec;
            doc = doc_string;

            function_many = fn;
        }
    }

    public class LispCons : LispObject
    {
        LispObject car_;
        LispObject cdr_;

        public LispCons(LispObject first, LispObject rest)
        {
            car_ = first;
            cdr_ = rest;
        }

        public LispObject Car
        {
            get { return car_; }
            set { car_ = value; }
        }

        public LispObject Cdr
        {
            get { return cdr_; }
            set { cdr_ = value; }
        }        
    }

    // LispCharTable is said to be used just like LispVector sometimes.
    // This means that we'll need to have an indexer which will map indices into fields, like:
    // 0 -> defalt
    // 1 -> parent
    // etc
    // or we could write properties Default {get {return content[0];}}
    // Either way we'll need to create these mappings from contents array to discrete fields. 
    public class LispCharTable : LispVectorLike<LispObject>
    {
        public int Size
        {
            get
            {
                throw new System.Exception("Comeback");
            }
        }

        public LispObject this[int index]
        {
            get
            {
                throw new System.Exception("Comeback");
            }

            set
            {
                throw new System.Exception("Comeback");
            }
        }    
        public const int CHARTAB_SIZE_BITS_0 = 6;
        public const int CHARTAB_SIZE_BITS_1 = 4;
        public const int CHARTAB_SIZE_BITS_2 = 5;
        public const int CHARTAB_SIZE_BITS_3 = 7;

        /* This is the vector's size field, which also holds the
           pseudovector type information.  It holds the size, too.
           The size counts the defalt, parent, purpose, ascii,
           contents, and extras slots.  */
        // EMACS_UINT size;
        // struct Lisp_Vector *next;

        /* This holds a default value,
           which is used whenever the value for a specific character is nil.  */
        LispObject defalt;

        /* This points to another char table, which we inherit from when the
           value for a specific character is nil.  The `defalt' slot takes
           precedence over this.  */
        LispObject parent;

        /* This is a symbol which says what kind of use this char-table is
           meant for.  */
        LispObject purpose;

        /* The bottom sub char-table for characters of the range 0..127.  It
           is nil if none of ASCII character has a specific value.  */
        LispObject ascii;

        LispObject[] contents = new LispObject[1 << CHARTAB_SIZE_BITS_0];

        /* These hold additional data.  It is a vector.  */
        LispObject[] extras;

        public LispCharTable(int n_extras)
        {
            extras = new LispObject[n_extras];
        }
    }

    /* A boolvector is a kind of vectorlike, with contents are like a string.  */
    public class LispBoolVector : LispVectorLike<byte>
    {
        /* Number of bits to put in each character in the internal representation
           of bool vectors.  This should not vary across implementations.  */
        public const int BOOL_VECTOR_BITS_PER_CHAR = 8;

        /* This is the size in bits.  */
        int size;
        /* This contains the actual bits, packed into bytes.  */
        byte[] data;

        public int Size
        {
            get
            {
                return size;
            }
        }

        public byte this[int index]
        {
            get
            {
                return data[index];
            }

            set
            {
                data[index] = value;
            }
        }        
    }

    public class LispHashTable : LispVectorLike<LispObject>
    {
        public LispHashTable ()
        {
            test               = Q.nil;
            weak               = Q.nil;
            rehash_size        = Q.nil;
            rehash_threshold   = Q.nil;
            hash               = Q.nil;
            next               = Q.nil;
            next_free          = Q.nil;
            index              = Q.nil;
            user_hash_function = Q.nil; 
            user_cmp_function  = Q.nil;
        }
        
        /* Function used to compare keys.  */
        public LispObject test;

        /* Nil if table is non-weak.  Otherwise a symbol describing the
           weakness of the table.  */
        public LispObject weak;

        /* When the table is resized, and this is an integer, compute the
           new size by adding this to the old size.  If a float, compute the
           new size by multiplying the old size with this factor.  */
        public LispObject rehash_size;

        /* Resize hash table when number of entries/ table size is >= this
           ratio, a float.  */
        public LispObject rehash_threshold;

        /* Vector of hash codes.. If hash[I] is nil, this means that that
           entry I is unused.  */
        public LispObject hash;

        /* Vector used to chain entries.  If entry I is free, next[I] is the
           entry number of the next free item.  If entry I is non-free,
           next[I] is the index of the next entry in the collision chain.  */
        public LispObject next;

        /* Index of first free entry in free list.  */
        public LispObject next_free;

        /* Bucket vector.  A non-nil entry is the index of the first item in
           a collision chain.  This vector's size can be larger than the
           hash table size to reduce collisions.  */
        public LispObject index;

        /* User-supplied hash function, or nil.  */
        public LispObject user_hash_function;

        /* User-supplied key comparison function, or nil.  */
        public LispObject user_cmp_function;

        /* Only the fields above are traced normally by the GC.  The ones below
           `count'.  are special and are either ignored by the GC or traced in
           a special way (e.g. because of weakness).  */

        /* Number of key/value entries in the table.  */
        public uint count;

        /* Vector of keys and values.  The key of item I is found at index
           2 * I, the value is found at index 2 * I + 1.
           This is gc_marked specially if the table is weak.  */
        public LispObject key_and_value;

        /* Next weak hash table if this is a weak hash table.  The head
           of the list is in weak_hash_tables.  */
        public LispHashTable next_weak;

        public delegate bool cmp_func(LispHashTable h, LispObject a, uint ai, LispObject b, uint bi);
        public delegate uint hash_func(LispHashTable h, LispObject o);

        /* C function to compare two keys.  */
        public cmp_func cmpfn;

        /* C function to compute hash code.  */
        public hash_func hashfn;
        
        public int Size
        {
            get
            {
                throw new System.Exception("Comeback");
            }
        }

        public LispObject this[int index]
        {
            get
            {
                throw new System.Exception("Comeback");
            }

            set
            {
                throw new System.Exception("Comeback");
            }
        }    
    }

    public class specbinding
    {
        public LispObject symbol;
        public LispObject old_value;
        public subr1 func;

        public specbinding() : this(null, null, null)
        {            
        }

        public specbinding(LispObject sym, LispObject old, subr1 f)
        {
            symbol    = sym;
            old_value = old;
            func      = f;
        }
    }

    public class Handler
    {
        public LispObject handler;
        public LispObject var;
        public LispObject chosen_clause;

        /* Used to effect the longjump out to the handler.  */
        public catchtag tag;

        public Handler next;
    }

    [System.Serializable]
    public class LispCatch : System.Exception
    {
        private catchtag tag;
        private LispObject value;

        public LispCatch(catchtag tag, LispObject value)
        {
            this.tag = tag;
            this.value = value;
        }

        public catchtag CatchTag
        {
            get { return tag; }
        }

        public LispObject Value
        {
            get { return value; }
        }
    }
}