namespace IronElisp
{
    public partial class L
    {
        public static void fatal(string str, params object[] args)
        {
            System.Console.Error.Write("emacs: ");
            System.Console.Error.WriteLine(str, args);
            System.Console.Error.Flush();
            System.Environment.Exit(1);
        }
    }
}