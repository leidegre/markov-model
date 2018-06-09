using System;
using System.IO;

namespace MarkovModel.Utils
{
    public static class BinaryUtils
    {
        public static int VarInt63Size(long v)
        {
            if (!(0 <= v)) throw new ArgumentOutOfRangeException(nameof(v));

            if (v <= (1L << 7) - 1) // 1 byte
            {
                return 1; // byte
            }

            if (v <= (1L << 14) - 1)
            {
                return 2; // bytes
            }

            if (v <= (1L << 21) - 1)
            {
                return 3; // bytes
            }

            if (v <= (1L << 28) - 1)
            {
                return 4; // bytes
            }

            if (v <= (1L << 35) - 1)
            {
                return 4; // bytes
            }

            if (v <= (1L << 42) - 1)
            {
                return 6; // bytes
            }

            if (v <= (1L << 49) - 1)
            {
                return 7; // bytes
            }

            if (v <= (1L << 56) - 1)
            {
                return 8; // bytes
            }

            return 9; // bytes
        }

        public static void WriteVarInt63(this Stream outputStream, long v)
        {
            if (!(0 <= v)) throw new ArgumentOutOfRangeException(nameof(v));

            if (v <= (1L << 7) - 1) // 1 byte
            {
                outputStream.WriteByte((byte)v);
                return;
            }

            if (v <= (1L << 14) - 1) // 2 bytes 
            {
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 21) - 1) // 3 bytes 
            {
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 28) - 1) // 4 bytes 
            {
                outputStream.WriteByte((byte)((v >> 21) | 0x80));
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 35) - 1) // 5 bytes 
            {
                outputStream.WriteByte((byte)((v >> 28) | 0x80));
                outputStream.WriteByte((byte)((v >> 21) | 0x80));
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 42) - 1) // 6 bytes 
            {
                outputStream.WriteByte((byte)((v >> 35) | 0x80));
                outputStream.WriteByte((byte)((v >> 28) | 0x80));
                outputStream.WriteByte((byte)((v >> 21) | 0x80));
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 49) - 1) // 7 bytes 
            {
                outputStream.WriteByte((byte)((v >> 42) | 0x80));
                outputStream.WriteByte((byte)((v >> 35) | 0x80));
                outputStream.WriteByte((byte)((v >> 28) | 0x80));
                outputStream.WriteByte((byte)((v >> 21) | 0x80));
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            if (v <= (1L << 56) - 1) // 8 bytes 
            {
                outputStream.WriteByte((byte)((v >> 49) | 0x80));
                outputStream.WriteByte((byte)((v >> 42) | 0x80));
                outputStream.WriteByte((byte)((v >> 35) | 0x80));
                outputStream.WriteByte((byte)((v >> 28) | 0x80));
                outputStream.WriteByte((byte)((v >> 21) | 0x80));
                outputStream.WriteByte((byte)((v >> 14) | 0x80));
                outputStream.WriteByte((byte)((v >> 7) | 0x80));
                outputStream.WriteByte((byte)(v & 0x7F));
                return;
            }

            // 9 bytes 
            outputStream.WriteByte((byte)((v >> 56) | 0x80));
            outputStream.WriteByte((byte)((v >> 49) | 0x80));
            outputStream.WriteByte((byte)((v >> 42) | 0x80));
            outputStream.WriteByte((byte)((v >> 35) | 0x80));
            outputStream.WriteByte((byte)((v >> 28) | 0x80));
            outputStream.WriteByte((byte)((v >> 21) | 0x80));
            outputStream.WriteByte((byte)((v >> 14) | 0x80));
            outputStream.WriteByte((byte)((v >> 7) | 0x80));
            outputStream.WriteByte((byte)(v & 0x7F));
        }

        public static long ReadVarInt63(this Stream inputStream)
        {
            int n = 0;

            long v = 0;
            _ReadByte:
            {
                if (!(n < 9)) throw new OverflowException();
                var b = inputStream.ReadByte();
                if (b == -1) throw new EndOfStreamException();
#pragma warning disable CS0675 // Bitwise-or operator used on a sign-extended operand
                v = (v << 7) | (b & 0x7F);
#pragma warning restore CS0675
                if ((b & 0x80) == 0x80)
                {
                    n++;
                    goto _ReadByte;
                }
            }

            return v;
        }
    }
}
