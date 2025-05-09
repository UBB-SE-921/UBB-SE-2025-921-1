using SharedClassLibrary.Domain;
using SharedClassLibrary.IRepository;

namespace SharedClassLibrary.Service
{
    /// <summary>
    /// Service class for managing buyer address-related operations.
    /// </summary>
    public class BuyerAddressService : IBuyerAddressService
    {
        private readonly IBuyerRepository _buyerRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuyerAddressService"/> class.
        /// </summary>
        /// <param name="buyerRepository">The buyer repository instance.</param>
        public BuyerAddressService(IBuyerRepository buyerRepository)
        {
            _buyerRepository = buyerRepository;
        }

        /// <inheritdoc/>
        public async Task<List<Address>> GetAllAddressesAsync()
        {
            return await _buyerRepository.GetAllAddressesAsync();
        }

        /// <inheritdoc/>
        public async Task<Address> GetAddressByIdAsync(int id)
        {
            var address = await _buyerRepository.GetAddressByIdAsync(id);
            if (address == null)
            {
                throw new KeyNotFoundException($"Address with ID {id} not found.");
            }

            return address;
        }

        /// <inheritdoc/>
        public async Task AddAddressAsync(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            await _buyerRepository.AddAddressAsync(address);
        }

        /// <inheritdoc/>
        public async Task UpdateAddressAsync(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            var existingAddress = await _buyerRepository.GetAddressByIdAsync(address.Id);
            if (existingAddress == null)
            {
                throw new KeyNotFoundException($"Address with ID {address.Id} not found.");
            }

            existingAddress.StreetLine = address.StreetLine;
            existingAddress.City = address.City;
            existingAddress.Country = address.Country;
            existingAddress.PostalCode = address.PostalCode;

            await _buyerRepository.UpdateAddressAsync(existingAddress);
        }

        /// <inheritdoc/>
        public async Task DeleteAddressAsync(int id)
        {
            var address = await _buyerRepository.GetAddressByIdAsync(id);
            if (address == null)
            {
                throw new KeyNotFoundException($"Address with ID {id} not found.");
            }

            await _buyerRepository.DeleteAddressAsync(address);
        }

        /// <inheritdoc/>
        public async Task<bool> AddressExistsAsync(int id)
        {
            return await _buyerRepository.AddressExistsAsync(id);
        }
    }
}
