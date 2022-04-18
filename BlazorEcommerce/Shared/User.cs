namespace BlazorEcommerce.Shared
{
    public class User
    {
        public Guid UserId { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public byte[]? PasswordHash { get; set; }
        public byte[]? PasswordSalt { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;
        public Address? Address { get; set; }
        public string Role { get; set; } = "Customer";
    }
}
