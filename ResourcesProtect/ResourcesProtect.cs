using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 资源加密
/// 考虑到效率效果
/// MAX_ENCRYPT_LEN之前的数据使用aes加密
/// MAX_ENCRYPT_LEN之后的数据使用混淆
/// </summary>
public class ResourcesProtect
{
    const int MAX_ENCRYPT_LEN = 512; //最大Aes加密长度,超过部分按规则混合
    const int CONFUSE_OFFSET = 16; //混淆间隔
    const int BLOCK_SIZE = 16; //校准对齐基数

    /// <summary>
    /// 加密资源文件
    /// </summary>
    static public void DoEncrypt(string resPath, string key, string desPath, Func<string, bool> filter = null)
    {
        if (null != filter && !filter(resPath))
            return;

        byte[] buffer = Read(resPath);
        byte[] encryptBuffer = ExecuteEncrypt(buffer, key);
        Write(encryptBuffer, desPath);
    }

    /// <summary>
    /// 解密资源文件
    /// </summary>
    static public void DoDecrypt(string resPath, string key, string desPath, Func<string, bool> filter = null)
    {
        if (null != filter && !filter(resPath))
            return;

        byte[] buffer = Read(resPath);
        byte[] decryptBuffer = ExecuteDecrypt(buffer, key);
        Write(decryptBuffer, desPath);
    }

    static public byte[] Read(string resPath)
    {
        byte[] buffer = null;
        try
        {
            FileStream stream = new FileStream(resPath, FileMode.Open);
            if (null != stream)
            {
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                buffer = reader.ReadBytes((int)stream.Length);
                stream.Close();
                stream.Dispose();
                reader.Close();
            }
        }
        catch (Exception)
        {
            throw;
        }

        return buffer;
    }

    static public void Write(byte[] buffer, string resPath)
    {
        try
        {
            FileStream stream = new FileStream(resPath, FileMode.Create);
            if (null != stream)
            {
                BinaryWriter writer = new BinaryWriter(stream);
                writer.Write(buffer, 0, buffer.Length);
                stream.Close();
                stream.Dispose();
                writer.Close();
            }
        }
        catch (Exception)
        {
            throw;
        }
    }

    /// <summary>
    /// aes加混淆加密接口
    /// </summary>
    static public byte[] ExecuteEncrypt(byte[] buffer, string key)
    {
        //这里后续附加混淆规则
        int bufferLen = buffer.Length;
        if (bufferLen > MAX_ENCRYPT_LEN)
        {
            //规定 ECBMode:(明文长度/16 + 1) * 16 = 密文长度
            int retLen = bufferLen - MAX_ENCRYPT_LEN + (MAX_ENCRYPT_LEN / BLOCK_SIZE + 1) * BLOCK_SIZE;
            byte[] encryBuffer = new byte[MAX_ENCRYPT_LEN];
            byte[] retBuffer = new byte[retLen];
            Array.Copy(buffer, encryBuffer, MAX_ENCRYPT_LEN);
            byte[] beEncryBuffer = AesEncrypt.Encrypt(encryBuffer, key);
            beEncryBuffer.CopyTo(retBuffer, 0);
            Array.Copy(buffer, MAX_ENCRYPT_LEN, retBuffer, beEncryBuffer.Length, bufferLen - MAX_ENCRYPT_LEN);

            for (int i = beEncryBuffer.Length, j = 0; i < retBuffer.Length; i += CONFUSE_OFFSET)
            {
                retBuffer[i] += buffer[j];
                j += 2;
                if (j >= MAX_ENCRYPT_LEN)
                    j = 0;
            }

            return retBuffer;
        }
        else
        {
            return AesEncrypt.Encrypt(buffer, key);
        }
    }

    /// <summary>
    /// aes加混淆解密接口
    /// </summary>
    static public byte[] ExecuteDecrypt(byte[] buffer, string key)
    {
        //这里后续附加混淆规则
        int bufferLen = buffer.Length;
        if (bufferLen > MAX_ENCRYPT_LEN)
        {
            //规定 ECBMode:(明文长度/16 + 1) * 16 = 密文长度
            int decryLen = (MAX_ENCRYPT_LEN / BLOCK_SIZE + 1) * BLOCK_SIZE;
            byte[] decryBuffer = new byte[decryLen];
            Array.Copy(buffer, decryBuffer, decryLen);
            byte[] beDecryBuffer = AesEncrypt.Decrypt(decryBuffer, key);
            int retLen = bufferLen - decryLen + beDecryBuffer.Length;
            byte[] retBuffer = new byte[retLen];
            beDecryBuffer.CopyTo(retBuffer, 0);
            Array.Copy(buffer, decryLen, retBuffer, beDecryBuffer.Length, retBuffer.Length - beDecryBuffer.Length);

            for (int i = beDecryBuffer.Length, j = 0; i < retLen; i += CONFUSE_OFFSET)
            {
                retBuffer[i] -= beDecryBuffer[j];
                j += 2;
                if (j >= beDecryBuffer.Length)
                    j = 0;
            }

            return retBuffer;
        }
        else
        {
            return AesEncrypt.Decrypt(buffer, key);
        }
    }

}
