using System;
using System.Linq;
using System.Text;

internal class Program
{
    private static void Main(string[] args)
    {
        string input = "Hello, World!";
        string hash = GetSHA1Hash(input);
        Console.WriteLine($"SHA-1 хеш: {hash}");
    }

    private static string GetSHA1Hash(string input)
    {
        byte[] message = Encoding.UTF8.GetBytes(input);
        byte[] hash = ComputeSHA1(message);
        return string.Concat(hash.Select(b => b.ToString("x2")));
    }

    private static byte[] ComputeSHA1(byte[] message)
    {
        uint h0 = 0x67452301;
        uint h1 = 0xEFCDAB89;
        uint h2 = 0x98BADCFE;
        uint h3 = 0x10325476;
        uint h4 = 0xC3D2E1F0;
        
        long ml = (long)message.Length * 8; 

        byte[] paddedMessage = PadMessage(message, ml);

        for (int chunkStart = 0; chunkStart < paddedMessage.Length; chunkStart += 64)
        {
            uint[] w = new uint[80];
            
            for (int i = 0; i < 16; i++)
            {
                w[i] = BytesToUInt32BE(paddedMessage, chunkStart + i * 4);
            }

            for (int i = 16; i < 80; i++)
            {
                w[i] = LeftRotate(w[i - 3] ^ w[i - 8] ^ w[i - 14] ^ w[i - 16], 1);
            }

            uint a = h0;
            uint b = h1;
            uint c = h2;
            uint d = h3;
            uint e = h4;

            for (int i = 0; i < 80; i++)
            {
                uint f, k;

                if (i <= 19)
                {
                    f = (b & c) | (~b & d);
                    k = 0x5A827999;
                }
                else if (i <= 39)
                {
                    f = b ^ c ^ d;
                    k = 0x6ED9EBA1;
                }
                else if (i <= 59)
                {
                    f = (b & c) | (b & d) | (c & d);
                    k = 0x8F1BBCDC;
                }
                else
                {
                    f = b ^ c ^ d;
                    k = 0xCA62C1D6;
                }

                uint temp = LeftRotate(a, 5) + f + e + k + w[i];
                e = d;
                d = c;
                c = LeftRotate(b, 30);
                b = a;
                a = temp;
            }

            h0 += a;
            h1 += b;
            h2 += c;
            h3 += d;
            h4 += e;
        }

        byte[] result = new byte[20];
        UInt32ToBytesBE(h0, result, 0);
        UInt32ToBytesBE(h1, result, 4);
        UInt32ToBytesBE(h2, result, 8);
        UInt32ToBytesBE(h3, result, 12);
        UInt32ToBytesBE(h4, result, 16);

        return result;
    }

    private static byte[] PadMessage(byte[] message, long messageBitLength)
    {
        int messageLen = message.Length;
        int paddingLen = (55 - (messageLen % 64)) % 64;
        byte[] padded = new byte[messageLen + paddingLen + 9];

        Array.Copy(message, padded, messageLen);

        padded[messageLen] = 0x80;

        UInt64ToBytesBE(messageBitLength, padded, padded.Length - 8);

        return padded;
    }

    private static uint BytesToUInt32BE(byte[] data, int offset)
    {
        return ((uint)data[offset] << 24) |
               ((uint)data[offset + 1] << 16) |
               ((uint)data[offset + 2] << 8) |
               ((uint)data[offset + 3]);
    }

    private static void UInt32ToBytesBE(uint value, byte[] data, int offset)
    {
        data[offset] = (byte)(value >> 24);
        data[offset + 1] = (byte)(value >> 16);
        data[offset + 2] = (byte)(value >> 8);
        data[offset + 3] = (byte)value;
    }

    private static void UInt64ToBytesBE(long value, byte[] data, int offset)
    {
        data[offset] = (byte)(value >> 56);
        data[offset + 1] = (byte)(value >> 48);
        data[offset + 2] = (byte)(value >> 40);
        data[offset + 3] = (byte)(value >> 32);
        data[offset + 4] = (byte)(value >> 24);
        data[offset + 5] = (byte)(value >> 16);
        data[offset + 6] = (byte)(value >> 8);
        data[offset + 7] = (byte)value;
    }

    private static uint LeftRotate(uint value, int count)
    {
        return (value << count) | (value >> (32 - count));
    }
}