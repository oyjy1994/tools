using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class AesEncrypt
{
    const int PASSWORD_LENGTH = 32; //密钥长度16，24，32
    static byte[] IV = new byte[]
    {
        0x00, 0x01, 0x02, 0x03,
        0x04, 0x05, 0x06, 0x07,
        0x08, 0x09, 0x0A, 0x0B,
        0x0C, 0x0D, 0x0E, 0x0F
    }; //只能16字节

    static AesCryptoServiceProvider aesProvider;

    static void Init(string key, bool fromBase64 = false)
    {
        if (null == aesProvider)
        {
            aesProvider = new AesCryptoServiceProvider
            {
                // KeySize = PASSWORD_LENGTH,
                IV = IV,
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7,
            };
        }
        if (!string.IsNullOrEmpty(key))
        {
            if (fromBase64)
                key = Encoding.UTF8.GetString(Convert.FromBase64String(key));
            key = GenPassWord(key);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            aesProvider.Key = keyBytes;
        }
    }

    /// <summary>
    /// 纯aes加密
    /// </summary>
    static public byte[] Encrypt(byte[] buffer, string key)
    {
        Init(key);
        var encrytor = aesProvider.CreateEncryptor();
        byte[] finalArray = encrytor.TransformFinalBlock(buffer, 0, buffer.Length);
        encrytor.Dispose();
        return finalArray;
    }

    /// <summary>
    /// 纯aes解密
    /// </summary>
    static public byte[] Decrypt(byte[] buffer, string key)
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
            key = key.PadRight(PASSWORD_LENGTH, 'D');
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
