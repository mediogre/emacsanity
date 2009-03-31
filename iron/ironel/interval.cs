namespace IronElisp
{
    class Interval
    {
        int total_length;
        int position;

        Interval left;
        Interval right;

        Interval up_interval;
        LispObject up_object;

        bool is_up_object;

        bool write_protect;
        bool visible;
        bool front_sticky;
        bool rear_sticky;

        LispObject plist;

        public Interval()
        {
            total_length = 0;
            position = 0;
            left = null;
            right = null;

            ParentInterval = null;

            write_protect = false;
            visible = false;
            front_sticky = false;
            rear_sticky = false;

            plist = Q.nil;
        }

        Interval ParentInterval
        {
            get
            {
                if (!is_up_object)
                {
                    return up_interval;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                is_up_object = false;
                up_interval = value;
            }
        }

        LispObject ParentObject
        {
            get
            {
                if (is_up_object)
                {
                    return up_object;
                }
                else
                {
                    return null;
                }
            }

            set
            {
                is_up_object = true;
                up_object = value;
            }
        }
    }
}