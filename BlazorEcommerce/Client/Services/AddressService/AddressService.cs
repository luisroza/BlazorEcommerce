namespace BlazorEcommerce.Client.Services.AddressService
{
    public class AddressService : IAddressService
    {
        private readonly HttpClient _http;

        public AddressService(HttpClient http)
        {
            _http = http;
        }

        public async Task<Address?> GetAddress()
        {
            ServiceResponse<Address?>? response = await _http.GetFromJsonAsync<ServiceResponse<Address>>("api/address");
            return response.Data;
        }

        public async Task<Address?> AddOrUpdateAddress(Address? address)
        {
            var response = await _http.PostAsJsonAsync("api/address", address);
            var serviceResponse = await response.Content.ReadFromJsonAsync<ServiceResponse<Address>>();
            return serviceResponse?.Data;
        }
    }
}
