using System;
using System.Text;

/// <summary>
/// rc4加密解密
/// 加密是连续的,加密完成之后再解密
/// 可以用在日志加解密
/// </summary>
public static class RC4Tool
{
	private const int BYTELEN = 256;
	private  static string recordKey = string.Empty;
	private static int keyIndex; //计算获取密钥字节的一个临时索引,在加解密之前重置
	private static int dataIndex; //计算获取密钥字节的索引,在加解密之前重置
	public static byte[] keyStream = new byte[BYTELEN]; //密钥流
	public static byte[] keyStreamTemp = new byte[BYTELEN]; //密钥流temp

	public static void Initialized(string key)
	{
		if(key == recordKey)
			return;
		
		Reset();
		recordKey = key;
		//初始
		for(int i = 0; i < BYTELEN; i++)
		{
			keyStream[i] = (byte)i;
		}
		GenKeyStream(key);
	}

	//生成密钥流
	private static void GenKeyStream(string key)
	{
		byte[] s = Encoding.UTF8.GetBytes(key);
		byte b; int j, k; int kl = key.Length;
		for(int i = 0; i < BYTELEN; i++)
		{
			b = keyStream[i];
			j = (j + b + key[k]) % BYTELEN;
			keyStream[i] = keyStream[j];
			keyStream[j] = b;
			if(++k >= kl)
				k = 0;
		}
		keyStream.CopyTo(keyStreamTemp, 0);
	}

	private static void Reset()
	{
		keyIndex = 0;
		dataIndex = 0;
		keyStreamTemp.CopyTo(keyStream, 0);
	}

	//获取密钥位
	public static byte GetKeyStreamByte(int index)
	{
		int rIdx = (index + 1) % BYTELEN;
		byte b = keyStream[rIdx];
		keyIndex = (keyIndex + b) % BYTELEN;
		byte b2 = keyStream[keyIndex];
		keyStream[rIdx] = b2;
		keyStream[keyIndex] = b;
		int i3 = (b + b2) % BYTELEN;
		return keyStream[i3];
	}

	public static byte[] Encrypt(byte[] array)
	{
		int len = array.Length;
		byte[] r = new byte[len];
		for(int i = 0; i < len; i++)
		{
			byte keyByte = GetKeyStreamByte(dataIndex++);
			r[i] = (byte)(array[i] ^ keyByte);
		}

		return r;
	}

	//异或,所以解密加密一样
	public static byte[] Decrypt(byte[] array)
	{
		return Encrypt(array);
	}
}