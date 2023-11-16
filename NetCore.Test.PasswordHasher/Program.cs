// See https://aka.ms/new-console-template for more information
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;

Console.Write("아이디를 입력하세요: ");
string? userId = Console.ReadLine();

Console.Write("비밀번호를 입력하세요: ");
string? password = Console.ReadLine();

string guidSalt = Guid.NewGuid().ToString();

if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
{
    Console.WriteLine("아이디와 비밀번호를 입력하세요.");
    return;
}

string rngSalt = GetRNGSalt();
string passwordHashed = GetPasswordHash(userId, password, guidSalt, rngSalt);

//데이터베이스의 비밀번호정보와 지금 입력한 비밀번호정보가 일치하는지 확인하는 변수
//로그인 성공
bool check = CheckThePasswordInfo(userId, password, guidSalt, rngSalt, passwordHashed); 

Console.WriteLine($"UserId:{userId}");
Console.WriteLine($"Password: {password}");
Console.WriteLine($"GUIDSalt: {guidSalt}");
Console.WriteLine($"RNGSalt: {rngSalt}");
Console.WriteLine($"PasswordHash: {passwordHashed}");
Console.WriteLine($"check: {(check ? "비밀번호 정보가 일치": "비밀번호 정보가 불일치")}");

Console.ReadLine();

static string GetRNGSalt()
{
    // generate a 128-bit salt using a secure PRNG
    byte[] salt = new byte[128 / 8];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(salt);
    }

    return Convert.ToBase64String(salt);
}

static string GetPasswordHash(string userId, string password, string guidSalt, string rngSalt)
{
    // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
    return Convert.ToBase64String(KeyDerivation.Pbkdf2(
        password: userId + password + guidSalt,
        salt: Encoding.UTF8.GetBytes(rngSalt),
        prf: KeyDerivationPrf.HMACSHA512,
        iterationCount: 45000,
        numBytesRequested: 256 / 8));
}

static bool CheckThePasswordInfo(string userId, string password, string guidSalt, string rngSalt, string passwordHash)
{
    return GetPasswordHash(userId, password, guidSalt, rngSalt).Equals(passwordHash);
}