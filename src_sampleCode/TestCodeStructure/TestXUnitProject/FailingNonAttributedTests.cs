namespace TestXUnitProject {

    // This is removed, these tests are deliberately there to fail the NDepend rules to test the failing
    // conditions.  However this would cause the build to fail so they are removed when not being used for
    // active development of the rules.
#if false
    public class FailingNonAttributedTests {


        [Fact]        
        public void NdependFailNoAttribute1() {
            Assert.True(true);
        }

        [Fact]
        public void NdependFailNoAttribute2() {
            Assert.True(true);
        }

        [Fact]
        public void NdependFailNoAttribute3() {
            Assert.True(true);
        }


        [Theory]        
        [InlineData("a")]
        public void NdependFailNoAttribute4(string a) {
            Assert.Equal("a", a);
            Assert.True(true);
        }
    }
#endif
}

