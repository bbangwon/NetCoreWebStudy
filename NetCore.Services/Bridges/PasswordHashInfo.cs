namespace NetCore.Services.Bridges
{
    public class PasswordHashInfo
    {
        public required string GUIDSalt { get; set; }
        public required string RNGSalt { get; set; }
        public required string PasswordHash { get; set; }
    }
}
