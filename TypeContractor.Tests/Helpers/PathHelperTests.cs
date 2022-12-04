using TypeContractor.Helpers;

namespace TypeContractor.Tests.Helpers
{
    public class PathHelperTests
    {
        [Fact]
        public void Empty_Path_Returns_Current_Directory()
        {
            var result = PathHelpers.RelativePath("Local", "File");

            result.Should().Be(".");
        }

        [Fact]
        public void Same_Path_Returns_Current_Directory()
        {
            var result = PathHelpers.RelativePath("Some.Deeply.Nested.Class", "Some.Deeply.Nested.Interface");

            result.Should().Be(".");
        }

        [Fact]
        public void Target_One_Level_Up_Returns_Doubledot()
        {
            var result = PathHelpers.RelativePath("ExampleContracts.Permissions", "ExampleContracts.v1.FilePermissionDto");

            result.Should().Be("../");
        }

        [Fact]
        public void Target_One_Level_Down_Returns_Nested()
        {
            var result = PathHelpers.RelativePath("Some.Nested.Deeper.Class", "Some.Nested.Interface");

            result.Should().Be("./");
        }

        [Fact]
        public void No_Common_Path_Throws()
        {
            Assert.Throws<ArgumentException>(() => PathHelpers.RelativePath("These.Are.Different", "Not.Even.Close"));
        }
    }
}
