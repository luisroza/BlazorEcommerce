namespace BlazorEcommerce.Shared
{
    public class Address
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string ZipPostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
