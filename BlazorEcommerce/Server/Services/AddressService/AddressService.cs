namespace BlazorEcommerce.Server.Services.AddressService
{
    public class AddressService : IAddressService
    {
        private readonly IAuthService _authService;
        private readonly DataContext _context;

        public AddressService(IAuthService authService, DataContext context)
        {
            _authService = authService;
            _context = context;
        }
        public async Task<ServiceResponse<Address>> GetAddress()
        {
            var userId = _authService.GetUserId();
            var address = await _context.Addresses!.FirstOrDefaultAsync(ad => ad.UserId == userId);

            return new ServiceResponse<Address> {Data = address};
        }

        public async Task<ServiceResponse<Address>> AddOrUpdateAddress(Address address)
        {
            var response = new ServiceResponse<Address>();
            var dbAddress = (await GetAddress()).Data;
            if (dbAddress == null)
            {
                address.UserId = _authService.GetUserId();
                _context.Addresses!.Add(address);
                response.Data = address;
            }
            else
            {
                dbAddress.FirstName = address.FirstName;
                dbAddress.LastName = address.LastName;
                dbAddress.StateProvince = address.StateProvince;
                dbAddress.City = address.City;
                dbAddress.Street = address.Street;
                dbAddress.Country = address.Country;
                dbAddress.ZipPostalCode = address.ZipPostalCode;
                response.Data = dbAddress;
            }

            await _context.SaveChangesAsync();
            return response;
        }
    }
}
