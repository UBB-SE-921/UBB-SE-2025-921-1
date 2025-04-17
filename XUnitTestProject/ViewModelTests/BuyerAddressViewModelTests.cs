using SharedClassLibrary.Domain;
using MarketPlace924.ViewModel;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XUnitTestProject.ViewModelTests
{
    public class BuyerAddressViewModelTests
    {
        [Theory]
        [InlineData("","")]
        [InlineData("Ohio","Ohio")]
        public void Constructor_InitializesCityCorrectly(string city, string expected)
        {
            var address = new Address();
            address.City = city;

            var buyerAddressViewModel = new BuyerAddressViewModel(address);

            Assert.Equal(buyerAddressViewModel.Address.City, expected);
        }

        [Fact]
        public void Address_SetDifferentValue_PropertyChangedRaised()
        {
            var initialAddress = new Address { City = "New York" };
            var newAddress = new Address { City = "Boston" };
            var viewModel = new BuyerAddressViewModel(initialAddress);
            
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(BuyerAddressViewModel.Address))
                {
                    propertyChangedRaised = true;
                }
            };

            viewModel.Address = newAddress;

            Assert.True(propertyChangedRaised);
            Assert.Equal(newAddress, viewModel.Address);
        }

        [Fact]
        public void Address_SetSameValue_PropertyChangedNotRaised()
        {
            // Arrange
            var address = new Address { City = "Chicago" };
            var viewModel = new BuyerAddressViewModel(address);
            
            var propertyChangedRaised = false;
            viewModel.PropertyChanged += (sender, args) => 
            {
                propertyChangedRaised = true;
            };

            // Act
            viewModel.Address = address; // Same reference

            // Assert
            Assert.False(propertyChangedRaised);
        }

        [Fact]
        public void PropertyChanged_EventSubscription()
        {
            // Arrange
            var address = new Address { City = "Seattle" };
            var viewModel = new BuyerAddressViewModel(address);
            
            // Track if event was raised
            var eventRaised = false;
            var eventPropertyName = string.Empty;
            
            // Subscribe to event
            viewModel.PropertyChanged += (sender, args) => 
            {
                eventRaised = true;
                eventPropertyName = args.PropertyName;
            };
            
            var newAddress = new Address { City = "Portland" };
            viewModel.Address = newAddress;
            
            Assert.True(eventRaised);
            Assert.Equal(nameof(BuyerAddressViewModel.Address), eventPropertyName);
        }
    }
}
