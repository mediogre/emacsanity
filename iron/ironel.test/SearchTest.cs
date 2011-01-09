using Xunit;
using IronElisp;

public class SearchTest
{
    [Fact]
    public void simple_search()
    {
        L.init_obarray();
        L.syms_of_search();
    }
}