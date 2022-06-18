namespace TestXUnitProject {
    using System.CodeDom.Compiler;

    [GeneratedCode("testing","1.0")]
    public class GeneratedCodeTests {
        // NDepend rule should ignore any tests that are generated code.

        [Fact]
        public void TestWithCorrectAttributes1() {
            Assert.True(true);
        }

        [Fact]
        public void TestWithCorrectAttributes2() {
            Assert.True(true);
        }

        [Theory]
        [InlineData("a")]
        public void TestWithCorrectAttributes3(string data) {
            Assert.True(true);
        }
    }
}