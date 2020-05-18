using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Configuration;

namespace MVC_Project.Utils
{
    public class SecurityUtil
    {
        /// <summary>
        /// Empty constructor
        /// </summary>

        public static string EncryptPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static RijndaelManaged GetRijndaelManaged(string salt)
        {
            var saltBytes = Encoding.ASCII.GetBytes(salt);
            var _key = new Rfc2898DeriveBytes(KEY, saltBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = _key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = _key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }

        private const string _KEY = "I2ExMi1zNG0kN2FnJ185";

        public static string KEY
        {
            get
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(_KEY));
            }
        }

        public static string Encrypt(string text)
        {
            var aesAlg = GetRijndaelManaged(ConfigurationManager.AppSettings["ENCRYPTION_SALT"]);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string Decrypt(string text)
        {
            string decryptedText = string.Empty;
            try
            {
                if (!string.IsNullOrEmpty(text))
                {
                    var aesAlg = GetRijndaelManaged(ConfigurationManager.AppSettings["ENCRYPTION_SALT"]);
                    var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                    var cipher = Convert.FromBase64String(text);

                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                decryptedText = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }

            }

            catch (Exception ex) { ex.ToString(); }
            return decryptedText;
        }

    }
}

