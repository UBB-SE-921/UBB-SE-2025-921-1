using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MarketPlace924.Domain;
using MarketPlace924.Helper;
using MarketPlace924.Service;
using MarketPlace924.ViewModel;
using Microsoft.UI.Xaml.Controls;
using Moq;
using Xunit;

namespace XUnitTestProject.ViewModelTests
{
	public class SignUpViewModelTests
	{
		private readonly Mock<IUserService> userServiceMock;
		private readonly SignUpViewModel viewModel;
		
		public SignUpViewModelTests()
		{
			// setup mocks
			this.userServiceMock = new Mock<IUserService>();
			
			// create view model
			this.viewModel = new SignUpViewModel(this.userServiceMock.Object);
		}
		
		[Fact]
		public void Constructor_InitializesPropertiesCorrectly()
		{
			// assert initial state
			Assert.NotNull(this.viewModel.SignupCommand);
			Assert.Equal(string.Empty, this.viewModel.Username);
			Assert.Equal(string.Empty, this.viewModel.Email);
			Assert.Equal(string.Empty, this.viewModel.PhoneNumber);
			Assert.Equal(string.Empty, this.viewModel.Password);
			Assert.Equal(0, this.viewModel.Role);
			Assert.Null(this.viewModel.NavigateToLogin);
		}
		
		[Fact]
		public void PropertyChanged_WhenUsernameChanges_IsRaised()
		{
			// arrange
			var propertyChangedRaised = false;
			this.viewModel.PropertyChanged += (s, e) => 
			{
				if (e.PropertyName == nameof(this.viewModel.Username))
				{
					propertyChangedRaised = true;
				}
			};
			
			// act
			this.viewModel.Username = "testuser";
			
			// assert
			Assert.True(propertyChangedRaised);
			Assert.Equal("testuser", this.viewModel.Username);
		}
		
		[Fact]
		public void PropertyChanged_WhenEmailChanges_IsRaised()
		{
			// arrange
			var propertyChangedRaised = false;
			this.viewModel.PropertyChanged += (s, e) => 
			{
				if (e.PropertyName == nameof(this.viewModel.Email))
				{
					propertyChangedRaised = true;
				}
			};
			
			// act
			this.viewModel.Email = "test@example.com";
			
			// assert
			Assert.True(propertyChangedRaised);
			Assert.Equal("test@example.com", this.viewModel.Email);
		}
		
		[Fact]
		public void PropertyChanged_WhenPhoneNumberChanges_IsRaised()
		{
			// arrange
			var propertyChangedRaised = false;
			this.viewModel.PropertyChanged += (s, e) => 
			{
				if (e.PropertyName == nameof(this.viewModel.PhoneNumber))
				{
					propertyChangedRaised = true;
				}
			};
			
			// act
			this.viewModel.PhoneNumber = "1234567890";
			
			// assert
			Assert.True(propertyChangedRaised);
			Assert.Equal("1234567890", this.viewModel.PhoneNumber);
		}
		
		[Fact]
		public void PropertyChanged_WhenPasswordChanges_IsRaised()
		{
			// arrange
			var propertyChangedRaised = false;
			this.viewModel.PropertyChanged += (s, e) => 
			{
				if (e.PropertyName == nameof(this.viewModel.Password))
				{
					propertyChangedRaised = true;
				}
			};
			
			// act
			this.viewModel.Password = "password123";
			
			// assert
			Assert.True(propertyChangedRaised);
			Assert.Equal("password123", this.viewModel.Password);
		}
		
		[Fact]
		public void PropertyChanged_WhenRoleChanges_IsRaised()
		{
			// arrange
			var propertyChangedRaised = false;
			this.viewModel.PropertyChanged += (s, e) => 
			{
				if (e.PropertyName == nameof(this.viewModel.Role))
				{
					propertyChangedRaised = true;
				}
			};
			
			// act
			this.viewModel.Role = 1;
			
			// assert
			Assert.True(propertyChangedRaised);
			Assert.Equal(1, this.viewModel.Role);
		}
		
		[Fact]
		public void NavigateToLogin_SetAndInvoked_CanBeSetAndInvoked()
		{
			// arrange
			var navigateCalled = false;
			Action navigateAction = () => navigateCalled = true;
			
			// act
			this.viewModel.NavigateToLogin = navigateAction;
			this.viewModel.NavigateToLogin?.Invoke();
			
			// assert
			Assert.True(navigateCalled);
		}
		
		[Fact]
		public void ExecuteSignup_WithValidData_CallsRegisterUser()
		{
			// arrange
			var navigateCalled = false;
			this.viewModel.NavigateToLogin = () => navigateCalled = true;
			
			this.viewModel.Username = "testuser";
			this.viewModel.Email = "test@example.com";
			this.viewModel.PhoneNumber = "1234567890";
			this.viewModel.Password = "password123";
			this.viewModel.Role = 1;
			
			this.userServiceMock.Setup(x => x.RegisterUser(
				this.viewModel.Username, 
				this.viewModel.Password, 
				this.viewModel.Email, 
				this.viewModel.PhoneNumber, 
				this.viewModel.Role))
				.ReturnsAsync(true);
			
			// act
			this.viewModel.SignupCommand.Execute(null);
			
			// assert
			this.userServiceMock.Verify(x => x.RegisterUser(
				this.viewModel.Username, 
				this.viewModel.Password, 
				this.viewModel.Email, 
				this.viewModel.PhoneNumber, 
				this.viewModel.Role), Times.Once);
		}
	}
}
