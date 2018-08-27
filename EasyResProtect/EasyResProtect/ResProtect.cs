using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EasyResProtect
{
    /// <summary>
    /// MAX_ENCRYPT_LEN之前的数据进行全Aes加密,之后的数据进行规制混合。
    /// 待测试检验。
    /// </summary>
    class ResProtect
    {
        const int MAX_ENCRYPT_LEN = 512; //最大Aes加密长度
        const int PASSWORD_LENGTH = 32;
        const int CONFUSE_OFFSET = 16; //混淆间隔
        const int BLOCK_SIZE = 16;
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
            int bufferLen = buffer.Length;
            if (bufferLen > MAX_ENCRYPT_LEN)
            {
                //ECBMode:(明文长度/16 + 1) * 16 = 密文长度
                int retLen = bufferLen - MAX_ENCRYPT_LEN + (MAX_ENCRYPT_LEN / BLOCK_SIZE + 1) * BLOCK_SIZE;
                byte[] encryBuffer = new byte[MAX_ENCRYPT_LEN];
                byte[] retBuffer = new byte[retLen];
                Array.Copy(buffer, encryBuffer, MAX_ENCRYPT_LEN);
                byte[] beEncryBuffer = Encrypt(encryBuffer, key);
                beEncryBuffer.CopyTo(retBuffer, 0);
                Array.Copy(buffer, MAX_ENCRYPT_LEN, retBuffer, beEncryBuffer.Length, bufferLen - MAX_ENCRYPT_LEN);

                for (int i = beEncryBuffer.Length, j = 0; i < retBuffer.Length; i += CONFUSE_OFFSET)
                {
                    retBuffer[i] += buffer[j]; j += 2;
                    if (j >= MAX_ENCRYPT_LEN)
                        j = 0;
                }

                return retBuffer;
            }
            else
            {
                return Encrypt(buffer, key);
            }
        }
        
        static byte[] ExecuteDecrypt(byte[] buffer, string key)
        {
            //这里后续附加混淆规则
            int bufferLen = buffer.Length;
            if (bufferLen > MAX_ENCRYPT_LEN)
            {
                //ECBMode:(明文长度/16 + 1) * 16 = 密文长度
                int decryLen = (MAX_ENCRYPT_LEN / BLOCK_SIZE + 1) * BLOCK_SIZE;
                byte[] decryBuffer = new byte[decryLen];
                Array.Copy(buffer, decryBuffer, decryLen);
                byte[] beDecryBuffer = Encrypt(decryBuffer, key);
                int retLen = bufferLen - decryLen + beDecryBuffer.Length;
                byte[] retBuffer = new byte[retLen];
                beDecryBuffer.CopyTo(retBuffer, 0);
                Array.Copy(buffer, decryLen, retBuffer, beDecryBuffer.Length, retBuffer.Length - beDecryBuffer.Length);

                for (int i = beDecryBuffer.Length, j = 0; i < retLen; i += CONFUSE_OFFSET)
                {
                    retBuffer[i] -= beDecryBuffer[j]; j += 2;
                    if (j >= beDecryBuffer.Length)
                        j = 0;
                }

                return retBuffer;
            }
            else
            {
                return Decrypt(buffer, key);
            }
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
