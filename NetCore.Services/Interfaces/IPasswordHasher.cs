namespace NetCore.Services.Interfaces
{
    public interface IPasswordHasher
    {
        string GetGUIDSalt();
        string GetRNGSalt();
        string GetPasswordHash(string userId, string password, string guidSalt, string rngSalt);
        bool MatchThePasswordInfo(string userId, string password);
    }
}
