using SharedClassLibrary.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnitTestProject.ServiceTests
{
    public class CaptchaServiceTests
    {
        private readonly CaptchaService captchaService;

        public CaptchaServiceTests()
        {
            captchaService = new CaptchaService();
        }

        [Theory]
        [InlineData("ABC123", "ABC123", true)]
        [InlineData("abc123", "ABC123", false)]
        [InlineData("abcdef", "abcdfe", false)]
        [InlineData("123456", "123456", true)]
        public void IsEnteredCaptchaValid_ReturnsExpectedResult(string generatedCaptcha, string enteredCaptcha, bool expectedResult)
        {
            // Act
            var isCaptchaValid = captchaService.IsEnteredCaptchaValid(generatedCaptcha, enteredCaptcha);

            // Assert
            Assert.Equal(expectedResult, isCaptchaValid);
        }

        [Fact]
        public void GenerateCaptcha_ReturnsValidAlphanumericCode()
        {
            // Act
            var captcha = captchaService.GenerateCaptcha();

            // Assert
            Assert.Matches("^[a-zA-Z0-9]+$", captcha); // Alphanumeric only
        }

        [Fact]
        public void GenerateCaptcha_ReturnsCorrectLength()
        {
            // Act
            var captcha = captchaService.GenerateCaptcha();

            // Assert
            Assert.InRange(captcha.Length, 6, 7); // The InRange method takes the low and high values as inclusive in the range check
        }
    }
}
