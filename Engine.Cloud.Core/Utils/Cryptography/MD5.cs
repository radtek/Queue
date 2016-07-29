using System;
using System.Text;

namespace Utils
{
    public class MD5
    {
        public static string Encrypt(string text)
        {
            Throw.IfIsNullOrEmpty(text);

            var md5Hasher = System.Security.Cryptography.MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(text));

            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString("x2"));

            return sBuilder.ToString();
        }
    }
}
