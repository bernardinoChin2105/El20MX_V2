using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MVC_Project.Utils
{
    public class EncryptorText
    {

        private const string Vector = "jR4wWYx2C0lIpPzL";
        public const string Clave = "hKZj4zNXkuNVZRB5uXWXD3LuT2CtYELn";

        public static string DataEncrypt(string text, string llave = "")
        {
            try
            {
                var initialVectorBytes = Encoding.ASCII.GetBytes(Vector);
                byte[] keyBytes = null;
                if (!string.IsNullOrEmpty(llave))
                {
                    keyBytes = Encoding.ASCII.GetBytes(llave);
                }
                else
                {
                    keyBytes = Encoding.ASCII.GetBytes(Clave);
                }

                var plainTextBytes = Encoding.UTF8.GetBytes(text);
                var symmetricKey = new RijndaelManaged { Padding = PaddingMode.ANSIX923, Mode = CipherMode.CBC };
                var encryptor = symmetricKey.CreateEncryptor(keyBytes, initialVectorBytes);
                var memoryStream = new MemoryStream();
                var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                cryptoStream.FlushFinalBlock();
                var cipherTextBytes = memoryStream.ToArray();
                memoryStream.Close();
                cryptoStream.Close();
                var textoCifradoFinal = Convert.ToBase64String(cipherTextBytes);
                return textoCifradoFinal;
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public static string DataDecrypt(string texto, string llave = "")
        {
            try
            {
                var initialVectorBytes = Encoding.ASCII.GetBytes(Vector);

                byte[] keyBytes = null;
                if (!string.IsNullOrEmpty(llave))
                {
                    keyBytes = Encoding.ASCII.GetBytes(llave);
                }
                else
                {
                    keyBytes = Encoding.ASCII.GetBytes(Clave);
                }

                var cipherTextBytes = Convert.FromBase64String(texto);
                var symmetricKey = new RijndaelManaged { Padding = PaddingMode.ANSIX923, Mode = CipherMode.CBC };
                var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes);
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                var character = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                string textoDescifradoFinal = character.ToString();
                return textoDescifradoFinal;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string KeyDecrypt(string text)
        {
            try
            {
                var initialVectorBytes = Encoding.ASCII.GetBytes(Vector);
                var keyBytes = Encoding.ASCII.GetBytes(Clave);

                var cipherTextBytes = Convert.FromBase64String(text);
                var symmetricKey = new RijndaelManaged { Padding = PaddingMode.ANSIX923, Mode = CipherMode.CBC };
                var decryptor = symmetricKey.CreateDecryptor(keyBytes, initialVectorBytes);
                var memoryStream = new MemoryStream(cipherTextBytes);
                var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                var plainTextBytes = new byte[cipherTextBytes.Length];
                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                memoryStream.Close();
                cryptoStream.Close();
                var textoDescifradoFinal = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                return textoDescifradoFinal;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
