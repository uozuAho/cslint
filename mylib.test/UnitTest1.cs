namespace mylib.test;

public class UnitTest1
{
    record MyTestRecord(string Name, int Age);

    [Fact]
    public void test_names_with_underscores_are_easier_to_read()
    {
        var someTestData = new int[] { 1, 2, 3 };
        var someMoreTestData = new int[][] { // this bracket should be on a newline
            [1,2,3],
            // use collection initialiser like above
            new [] {4, 5, 6}
        };

        Assert.Equal(someTestData, someMoreTestData[0]);

        var record1 = new MyTestRecord("bib", 31);
        var record2 = record1 with { Name = "bob" };

        Assert.Equal(record1, record2);
    }
}
