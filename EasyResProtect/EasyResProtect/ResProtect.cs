using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyResProtect
{
    class ResProtect
    {
        const int PASSWORD_LENGTH = 32;
        static byte[] IV = new byte[] { 0xa2, 0xb4, 0xc6, 0xd8, 0xe0, 0xfB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
        static AesCryptoServiceProvider aesProvider;

        static void Init(string key)
        {
            if (null == aesProvider)
            {
                aesProvider = new AesCryptoServiceProvider
                {
                    //KeySize = PASSWORD_LENGTH,
                    IV = IV,
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7,
                };
            }
            if (!string.IsNullOrEmpty(key))
            {
                key = GenPassWord(key);
                aesProvider.Key = Encoding.UTF8.GetBytes(key);
            }
        }

        static public void DoEncrypt(string resPath, string key, Func<string, bool> filter = null)
        {
            if (null != filter && !filter(resPath))
                return;

            byte[] buffer = BinaryRes.Read(resPath);
            byte[] encryptBuffer = ExecuteEncrypt(buffer, key);
            BinaryRes.Write(encryptBuffer, resPath);
        }
        
        static public void DoDecrypt(string resPath, string key, Func<string, bool> filter = null)
        {
            if (null != filter && !filter(resPath))
                return;

            byte[] buffer = BinaryRes.Read(resPath);
            byte[] decryptBuffer = ExecuteDecrypt(buffer, key);
            BinaryRes.Write(buffer, resPath);
        }

        static byte[] ExecuteEncrypt(byte[] buffer, string key)
        {
            //这里后续附加混淆规则
            return Encrypt(buffer, key);
        }
        
        static byte[] ExecuteDecrypt(byte[] buffer, string key)
        {
            //这里后续附加混淆规则
            return Decrypt(buffer, key);
        }

        static byte[] Encrypt(byte[] buffer, string key)
        {
            Init(key);
            var encrytor = aesProvider.CreateEncryptor();
            byte[] finalArray = encrytor.TransformFinalBlock(buffer, 0, buffer.Length);
            encrytor.Dispose();
            return finalArray;
        }

        static byte[] Decrypt(byte[] buffer, string key)
        {
            Init(key);
            var decrytor = aesProvider.CreateDecryptor();
            byte[] finalArray = decrytor.TransformFinalBlock(buffer, 0, buffer.Length);
            decrytor.Dispose();
            return finalArray;
        }

        static string GenPassWord(string key)
        {
            int len = key.Length;
            if (len < PASSWORD_LENGTH)
            {
                key = key.PadRight(PASSWORD_LENGTH - len, 'D');
            }
            else if (len > PASSWORD_LENGTH)
            {
                key = key.Substring(0, len);
            }
            return key;
        }

        static public void Clear()
        {
            if (null != aesProvider)
            {
                aesProvider.Clear();
            }
        }

    }
}
