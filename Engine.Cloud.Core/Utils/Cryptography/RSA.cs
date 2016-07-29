using System;
using System.Security.Cryptography;

namespace Utils
{
    public class RSA
    {
        public static string Encrypt(string text, string key)
        {
            Throw.IfIsNullOrEmpty(text);
            Throw.IfIsNullOrEmpty(key);

            CspParameters cspp = new CspParameters();
            cspp.KeyContainerName = key;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;

            byte[] bytes = rsa.Encrypt(System.Text.UTF8Encoding.UTF8.GetBytes(text), true);

            return BitConverter.ToString(bytes);
        }

        public static string Decrypt(string text, string key)
        {
            Throw.IfIsNullOrEmpty(text);
            Throw.IfIsNullOrEmpty(key);

            CspParameters cspp = new CspParameters();
            cspp.KeyContainerName = key;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspp);
            rsa.PersistKeyInCsp = true;

            string[] decryptArray = text.Split(new string[] { "-" }, StringSplitOptions.None);
            byte[] decryptByteArray = Array.ConvertAll<string, byte>(decryptArray, (s => Convert.ToByte(byte.Parse(s, System.Globalization.NumberStyles.HexNumber))));


            byte[] bytes = rsa.Decrypt(decryptByteArray, true);

            return System.Text.UTF8Encoding.UTF8.GetString(bytes);
        }
    }
}
