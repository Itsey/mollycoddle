namespace TestXUnitProject {
    using Plisky.Test;

    public class PassingRulesTests {


        [Fact]
        [Bug(1234)]
        public void TestWithCorrectAttributes1() {
            Assert.True(true);
        }

        [Fact]
        [Integration]
        public void TestWithCorrectAttributes2() {
            Assert.True(true);
        }

        [Fact]
        [Build(BuildType.Any)]
        public void TestWithCorrectAttributes3() {
            Assert.True(true);
        }

        [Theory]
        [Build(BuildType.Any)]
        [InlineData("a")]
        public void TestWithCorrectAttributes4(string a) {
            Assert.Equal("a", a);
            Assert.True(true);
        }
    }
}