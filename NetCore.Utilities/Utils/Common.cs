using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;

namespace NetCore.Utilities.Utils
{
    public static class Common
    {
        /// <summary>
        /// Data Protection 설정
        /// </summary>
        /// <param name="services">등록할 서비스</param>
        /// <param name="keyPath">키 경로</param>
        /// <param name="applicationName">애플리케이션 이름</param>
        /// <param name="cryptoType">암호화 유형</param>
        public static void SetDataProtection(IServiceCollection services, string keyPath, string applicationName, Enum cryptoType)
        {
            var builder = services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(keyPath))   //키 저장 경로
                .SetDefaultKeyLifetime(TimeSpan.FromDays(7))            //키 유효기간
                .SetApplicationName(applicationName);                 //암호화에 사용할 애플리케이션 이름

            switch (cryptoType)
            {
                case Enums.CryptoType.Unmanaged:
                    builder.UseCryptographicAlgorithms(
                        new AuthenticatedEncryptorConfiguration() { 
                            EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                            ValidationAlgorithm = ValidationAlgorithm.HMACSHA512
                        }); 
                    break;
                case Enums.CryptoType.Managed:
                    builder.UseCustomCryptographicAlgorithms(
                        new ManagedAuthenticatedEncryptorConfiguration()
                        {
                            EncryptionAlgorithmType = typeof(Aes),
                            EncryptionAlgorithmKeySize = 256,
                            ValidationAlgorithmType = typeof(HMACSHA512)                            
                        });
                    break;
                    //Cryptography API : Next Generation (CNG)
                case Enums.CryptoType.CngCbc:
                    if(OperatingSystem.IsWindows())
                    {
                        builder.UseCustomCryptographicAlgorithms(
                            new CngCbcAuthenticatedEncryptorConfiguration()
                            {
                                EncryptionAlgorithm = "AES",
                                EncryptionAlgorithmProvider = null,
                                EncryptionAlgorithmKeySize = 256,
                                HashAlgorithm = "SHA512",
                                HashAlgorithmProvider = null
                            });
                    }
                    break;
                case Enums.CryptoType.CngGcm:
                    if(OperatingSystem.IsWindows())
                    {
                        builder.UseCustomCryptographicAlgorithms(
                            new CngGcmAuthenticatedEncryptorConfiguration()
                            {
                                EncryptionAlgorithm = "AES",
                                EncryptionAlgorithmProvider = null,
                                EncryptionAlgorithmKeySize = 256
                            });
                    }
                    break;
            }

        }
    }
}
