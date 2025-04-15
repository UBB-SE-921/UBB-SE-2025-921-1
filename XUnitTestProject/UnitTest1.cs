namespace XUnitTestProject
{
    using Xunit;
    using MarketPlace924.Domain;
    using Windows.Security.Authentication.OnlineId;
    using Microsoft.Web.WebView2.Core;

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            User user = new User();

            user.UserId = 100;

            Assert.Equal(100, user.UserId);
        }

        [Theory]
        [InlineData(100)]
        [InlineData(200)]
        public void Test2(int ID)
        {
            User user = new User();

            user.UserId = ID;

            Assert.Equal(ID, user.UserId);
        }
    }
}