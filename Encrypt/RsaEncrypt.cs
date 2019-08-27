using XLua;
using System;
using System.Text;
//c#加解密库都存在于此
using System.Security.Cryptography;

/// <summary>
/// PKCS#1
/// </summary>
[LuaCallCSharp]
public class RsaEncrypt
{

    #region same public key
    static public CspParameters GetCspparametersKey(string key)
    {
        return new CspParameters() { KeyContainerName = key };
    }

    /// <summary>
    /// 基于Base64编码的RSA加密
    /// </summary>
    static public string Encrypt(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            return string.Empty;

        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetCspparametersKey(key)))
        {
            var rsaBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(value), false);
            return Convert.ToBase64String(rsaBytes);
        }
    }

    /// <summary>
    /// 基于Base64编码的RSA解密
    /// </summary>
    static public string Decrypt(string key, string value)
    {
        if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
            return string.Empty;

        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(GetCspparametersKey(key)))
        {
            var rsaBytes = rsa.Decrypt(Convert.FromBase64String(value), false);
            return Encoding.UTF8.GetString(rsaBytes);
        }
    }

    /// <summary>
    /// 基于Base64编码的RSA加密
    /// </summary>
    static public string EncryptDer(string base64Key, string value)
    {
        if (string.IsNullOrEmpty(base64Key) || string.IsNullOrEmpty(value))
            return string.Empty;

        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.ImportParameters(ConvertFromPemPublicKey(base64Key));
            var rsaBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(value), false);
            return Convert.ToBase64String(rsaBytes);
        }
    }

    /// <summary>
    /// 构造Parameters
    /// </summary>
    static public RSAParameters ConvertFromPemPublicKey(string pemFileConent)
    {
        if (string.IsNullOrEmpty(pemFileConent))
        {
            throw new ArgumentNullException("pemFileConent", "This arg cann't be empty.");
        }
        pemFileConent = pemFileConent.Replace("-----BEGIN PUBLIC KEY-----", "").
                                      Replace("-----END PUBLIC KEY-----", "").
                                      Replace("\n", "").
                                      Replace("\r", "");
        byte[] keyData = Convert.FromBase64String(pemFileConent);
        bool keySize512 = (keyData.Length == 94);
        bool keySize1024 = (keyData.Length == 162);
        bool keySize2048 = (keyData.Length == 294);
        //DHLog.Log("rsa keysize = {0}", keyData.Length);
        if (!(keySize1024 || keySize2048 || keySize512))
        {
            throw new ArgumentException("pem file content is incorrect, Only support the key size is 1024 or 2048");
        }

        var pemModulus = (keySize1024 ? new byte[128] : new byte[256]);

        //1024或者2048密钥
        if (keySize1024 || keySize2048)
        {
            Array.Copy(keyData, (keySize1024 ? 29 : 33), pemModulus, 0, (keySize1024 ? 128 : 256));
        }

        //512位长度密钥
        if (keySize512)
        {
            pemModulus = new byte[64];
            Array.Copy(keyData, 25, pemModulus, 0, pemModulus.Length);
        }

        var pemPublicExponent = new byte[3];
        Array.Copy(keyData, keyData.Length - 3, pemPublicExponent, 0, 3);

        var para = new RSAParameters { Modulus = pemModulus, Exponent = pemPublicExponent };
        return para;
    }

    #endregion

}
