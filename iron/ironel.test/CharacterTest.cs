using Xunit;

public class CharacterTest
{
    [Fact]
    public void TestParseMultibyte()
    {
        // '$'
        byte[] ary = System.Text.Encoding.UTF8.GetBytes("\u0024");
        int num_chars = 0;
        int num_bytes = 0;
        IronElisp.L.parse_str_as_multibyte(ary, ary.Length, ref num_chars, ref num_bytes);

        Assert.Equal(1, num_chars);
        Assert.Equal(1, num_bytes);

        // '¢' 
        ary = System.Text.Encoding.UTF8.GetBytes("\u00a2");
        IronElisp.L.parse_str_as_multibyte(ary, ary.Length, ref num_chars, ref num_bytes);

        Assert.Equal(1, num_chars);
        Assert.Equal(2, num_bytes);

        // '€'
        ary = System.Text.Encoding.UTF8.GetBytes("\u20AC");
        IronElisp.L.parse_str_as_multibyte(ary, ary.Length, ref num_chars, ref num_bytes);

        Assert.Equal(1, num_chars);
        Assert.Equal(3, num_bytes);

        // some mandarin 𤭢
        ary = System.Text.Encoding.UTF8.GetBytes("\U00024B62");
        IronElisp.L.parse_str_as_multibyte(ary, ary.Length, ref num_chars, ref num_bytes);

        Assert.Equal(1, num_chars);
        Assert.Equal(4, num_bytes);

        ary = System.Text.Encoding.UTF8.GetBytes("\u0024\u00a2\u20AC\U00024B62");
        IronElisp.L.parse_str_as_multibyte(ary, ary.Length, ref num_chars, ref num_bytes);

        Assert.Equal(4, num_chars);
        Assert.Equal(10, num_bytes);
    }
}