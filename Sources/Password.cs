using System;
using System.Security.Cryptography;
using System.Text;

namespace UUM_Hytroc
{
    class Password
    {
        
        public string Decrypt(string Pass)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes("hardcoded"));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateDecryptor())
                    {
                        byte[] data = Convert.FromBase64String(Pass);//*
                        byte[] result = transform.TransformFinalBlock(data, 0, data.Length);
                        return UTF8Encoding.UTF8.GetString(result);
                    }
                }
            }
        }
    }
}
