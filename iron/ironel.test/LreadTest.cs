using Xunit;
using IronElisp;

public class LreadTest
{
    [Fact]
    public void first_read()
    {
        L.init_obarray();
        LispObject r = F.read(L.build_string("(10 20 42)"));

        Assert.Equal(10, ((LispInt)F.car(r)).val);
        Assert.Equal(20, ((LispInt)F.car(F.cdr(r))).val);
        Assert.Equal(42, ((LispInt)F.car(F.cdr(F.cdr(r)))).val);
        Assert.Equal(3,  ((LispInt)F.length(r)).val);
    }
}