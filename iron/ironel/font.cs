namespace IronElisp
{
    public class font_driver
    {
        // COMEBACK_WHEN_READY!!!
    }

    public class font_driver_list
    {
        /* 1 iff this driver is currently used.  It is igonred in the global
           font driver list.*/
        bool on;
        /* Pointer to the font driver.  */
        font_driver driver;
        /* Pointer to the next element of the chain.  */
        font_driver_list next;
    }

    public class font_data_list
    {
        /* Pointer to the font driver.  */
        font_driver driver;
        /* Data specific to the font driver.  */
        object data;
        /* Pointer to the next element of the chain.  */
        font_data_list next;
    }
}