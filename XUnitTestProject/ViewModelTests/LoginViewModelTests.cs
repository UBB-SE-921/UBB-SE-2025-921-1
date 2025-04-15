using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SharedClassLibrary.Domain;
using MarketPlace924.Helper;
using MarketPlace924.Service;
using MarketPlace924.ViewModel;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Moq;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
    public class LoginViewModelTests : IDisposable
    {
        private readonly Mock<IUserService> userServiceMock;
        private readonly Mock<IOnLoginSuccessCallback> successCallbackMock;
        private readonly Mock<ICaptchaService> captchaServiceMock;
        private readonly TraceListener debugListener;
        private readonly LoginViewModel viewModel;

        public LoginViewModelTests()
        {
            // Setup trace listener
            this.debugListener = new DebugListener();
            Trace.Listeners.Add(this.debugListener);

            // Setup mocks
            this.userServiceMock = new Mock<IUserService>();
            this.successCallbackMock = new Mock<IOnLoginSuccessCallback>();
            this.captchaServiceMock = new Mock<ICaptchaService>();
            
            // Setup captcha service mock
            this.captchaServiceMock.Setup(x => x.GenerateCaptcha())
                .Returns("TEST123");
            
            // Create view model with mocked dependencies
            this.viewModel = new LoginViewModel(
                this.userServiceMock.Object, 
                this.successCallbackMock.Object,
                this.captchaServiceMock.Object);
        }

        public void Dispose()
        {
            Trace.Listeners.Remove(this.debugListener);
        }

        [Fact]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Assert
            Assert.NotNull(this.viewModel.LoginCommand);
            Assert.True(this.viewModel.IsLoginEnabled);
            Assert.Equal("TEST123", this.viewModel.CaptchaText);
            Assert.Null(this.viewModel.ErrorMessage);
            Assert.Null(this.viewModel.Email);
            Assert.Null(this.viewModel.Password);
            Assert.Null(this.viewModel.CaptchaEnteredCode);
            Assert.Equal(string.Empty, this.viewModel.FailedAttemptsText);
        }

        [Fact]
        public void Constructor_WithNullUserService_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new LoginViewModel(null, this.successCallbackMock.Object));
                
            Assert.Equal("userService", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithNullSuccessCallback_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                new LoginViewModel(this.userServiceMock.Object, null));
                
            Assert.Equal("successCallback", exception.ParamName);
        }

        [Fact]
        public void Constructor_WithValidParameters_InitializesCaptchaService()
        {
            // This test verifies that captchaService is initialized and used 
            // in the constructor by checking the CaptchaText property
            
            // Assert
            Assert.Equal("TEST123", this.viewModel.CaptchaText);
            this.captchaServiceMock.Verify(x => x.GenerateCaptcha(), Times.Once());
        }

        [Fact]
        public void UserService_IsAccessibleThroughProperty()
        {
            // Act & Assert
            Assert.Same(this.userServiceMock.Object, this.viewModel.UserService);
        }

        [Fact]
        public void NavigateToSignUp_CanBeSetAndRetrieved()
        {
            // Arrange
            bool navigateCalled = false;
            Action navigateAction = () => navigateCalled = true;
            
            // Act
            this.viewModel.NavigateToSignUp = navigateAction;
            this.viewModel.NavigateToSignUp?.Invoke();
            
            // Assert
            Assert.True(navigateCalled);
        }

        [Fact]
        public void PropertyChanged_WhenEmailChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.Email))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.Email = "test@example.com";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("test@example.com", this.viewModel.Email);
        }

        [Fact]
        public void PropertyChanged_WhenPasswordChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.Password))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.Password = "password123";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("password123", this.viewModel.Password);
        }

        [Fact]
        public void PropertyChanged_WhenErrorMessageChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.ErrorMessage))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.ErrorMessage = "Test error";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("Test error", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void PropertyChanged_WhenIsLoginEnabledChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.IsLoginEnabled))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.IsLoginEnabled = false;

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.False(this.viewModel.IsLoginEnabled);
        }

        [Fact]
        public void PropertyChanged_WhenCaptchaTextChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.CaptchaText))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.CaptchaText = "NEW123";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("NEW123", this.viewModel.CaptchaText);
        }

        [Fact]
        public void PropertyChanged_WhenCaptchaEnteredCodeChanges_IsRaised()
        {
            // Arrange
            var propertyChangedRaised = false;
            this.viewModel.PropertyChanged += (s, e) => 
            {
                if (e.PropertyName == nameof(this.viewModel.CaptchaEnteredCode))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            this.viewModel.CaptchaEnteredCode = "TEST123";

            // Assert
            Assert.True(propertyChangedRaised);
            Assert.Equal("TEST123", this.viewModel.CaptchaEnteredCode);
        }

        [Fact]
        public void FailedAttemptsText_WithNoAttempts_ReturnsEmptyString()
        {
            // Act & Assert
            Assert.Equal(string.Empty, this.viewModel.FailedAttemptsText);
        }

        [Fact]
        public void FailedAttemptsText_WithAttempts_ReturnsFormattedString()
        {
            // Arrange
            // Force a failed login to set hasAttemptedLogin to true and incrementing failed attempts
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "wrongpassword";
            this.viewModel.CaptchaEnteredCode = "TEST123";
            this.viewModel.CaptchaText = "TEST123";

            this.userServiceMock.Setup(x => x.LoginAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((true, "Login successful", new User { Email = "test@example.com" }));

            this.userServiceMock.Setup(x => x.IsUserSuspended(It.IsAny<string>()))
                .ReturnsAsync(false);

            this.userServiceMock.Setup(x => x.CanUserLogin(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            this.userServiceMock.Setup(x => x.GetFailedLoginsCountByEmail(It.IsAny<string>()))
                .ReturnsAsync(2);

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Failed Login Attempts: 3", this.viewModel.FailedAttemptsText);
        }

        [Fact]
        public void ExecuteLogin_WithEmptyEmail_ShowsErrorMessage()
        {
            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Email is required", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void ExecuteLogin_WithEmptyPassword_ShowsErrorMessage()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Password is required", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void ExecuteLogin_WithEmptyCaptchaEnteredCode_ShowsErrorMessage()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "password123";

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Captcha is required", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void ExecuteLogin_WithEmptyCaptchaText_ShowsErrorMessage()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "password123";
            this.viewModel.CaptchaEnteredCode = "TEST123";
            this.viewModel.CaptchaText = string.Empty;

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Captcha is required", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void ExecuteLogin_WithInvalidCaptcha_ShowsErrorMessageAndGeneratesNewCaptcha()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "password123";
            this.viewModel.CaptchaEnteredCode = "WRONG";
            this.viewModel.CaptchaText = "TEST123";

            this.userServiceMock.Setup(x => x.LoginAsync(
                this.viewModel.Email, 
                this.viewModel.Password, 
                this.viewModel.CaptchaEnteredCode, 
                this.viewModel.CaptchaText))
                .ReturnsAsync((false, "Captcha verification failed.", (User)null));

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Captcha verification failed.", this.viewModel.ErrorMessage);
            this.captchaServiceMock.Verify(x => x.GenerateCaptcha(), Times.Exactly(2)); // Once in constructor, once after failed login
        }

        [Fact]
        public void ExecuteLogin_WithSuspendedUser_ShowsBanMessage()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "password123";
            this.viewModel.CaptchaEnteredCode = "TEST123";
            this.viewModel.CaptchaText = "TEST123";

            this.userServiceMock.Setup(x => x.LoginAsync(
                this.viewModel.Email, 
                this.viewModel.Password, 
                this.viewModel.CaptchaEnteredCode, 
                this.viewModel.CaptchaText))
                .ReturnsAsync((true, "Login successful", new User { Email = this.viewModel.Email }));

            this.userServiceMock.Setup(x => x.IsUserSuspended(this.viewModel.Email))
                .ReturnsAsync(true);

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Contains("Too many failed attempts", this.viewModel.ErrorMessage);
        }

        [Fact]
        public void ExecuteLogin_WithInvalidCredentials_IncrementsFailedAttempts()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "wrongpassword";
            this.viewModel.CaptchaEnteredCode = "TEST123";
            this.viewModel.CaptchaText = "TEST123";

            var testUser = new User { Email = this.viewModel.Email };
            
            this.userServiceMock.Setup(x => x.LoginAsync(
                this.viewModel.Email, 
                this.viewModel.Password, 
                this.viewModel.CaptchaEnteredCode, 
                this.viewModel.CaptchaText))
                .ReturnsAsync((true, "Login successful", testUser));

            this.userServiceMock.Setup(x => x.IsUserSuspended(this.viewModel.Email))
                .ReturnsAsync(false);

            this.userServiceMock.Setup(x => x.CanUserLogin(this.viewModel.Email, this.viewModel.Password))
                .ReturnsAsync(false);

            this.userServiceMock.Setup(x => x.GetFailedLoginsCountByEmail(this.viewModel.Email))
                .ReturnsAsync(2);

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Login failed", this.viewModel.ErrorMessage);
            this.userServiceMock.Verify(x => x.UpdateUserFailedLoginsCount(testUser, 3), Times.Once);
        }

        [Fact]
        public void ExecuteLogin_WithValidCredentials_ResetsFailedAttemptsAndCallsSuccessCallback()
        {
            // Arrange
            this.viewModel.Email = "test@example.com";
            this.viewModel.Password = "correctpassword";
            this.viewModel.CaptchaEnteredCode = "TEST123";
            this.viewModel.CaptchaText = "TEST123";

            var testUser = new User { Email = this.viewModel.Email };
            
            this.userServiceMock.Setup(x => x.LoginAsync(
                this.viewModel.Email, 
                this.viewModel.Password, 
                this.viewModel.CaptchaEnteredCode, 
                this.viewModel.CaptchaText))
                .ReturnsAsync((true, "Login successful", testUser));

            this.userServiceMock.Setup(x => x.IsUserSuspended(this.viewModel.Email))
                .ReturnsAsync(false);

            this.userServiceMock.Setup(x => x.CanUserLogin(this.viewModel.Email, this.viewModel.Password))
                .ReturnsAsync(true);

            // Act
            this.viewModel.LoginCommand.Execute(null);

            // Assert
            Assert.Equal("Login successful!", this.viewModel.ErrorMessage);
            this.userServiceMock.Verify(x => x.UpdateUserFailedLoginsCount(testUser, 0), Times.Once);
            this.successCallbackMock.Verify(x => x.OnLoginSuccess(testUser), Times.Once);
        }
    }

    // Helper class to capture trace output
    public class DebugListener : TraceListener
    {
        public List<string> Messages { get; } = new List<string>();

        public override void Write(string message)
        {
            Messages.Add(message);
        }

        public override void WriteLine(string message)
        {
            Messages.Add(message);
        }
    }
}
