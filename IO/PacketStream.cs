﻿using System;
using System.IO;
using System.Text;

namespace MineLib.Network.IO
{
    // -- Credits to umby24 for encryption support, as taken from CWrapped.
    public partial class PacketStream : IDisposable
    {
        // -- Credits to SirCmpwn for encryption support, as taken from SMProxy.
        private Stream _stream;
        private AesStream _crypto;
        public bool EncEnabled;
        private byte[] _buffer;

        public PacketStream(Stream stream)
        {
            _stream = stream;
        }

        public void InitEncryption(byte[] key)
        {
            _crypto = new AesStream(_stream, key);
        }

        // -- Strings

        public void WriteString(string value)
        {
            byte[] length = GetVarIntBytes((long) value.Length);
            byte[] final = new byte[value.Length + length.Length];

            Buffer.BlockCopy(length, 0, final, 0, length.Length);
            Buffer.BlockCopy(Encoding.UTF8.GetBytes(value), 0, final, length.Length, value.Length);

            WriteByteArray(final);
        }

        // -- Shorts

        public void WriteShort(short value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            WriteByteArray(bytes);
        }

        // -- Integer

        public void WriteInt(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            WriteByteArray(bytes);
        }

        // -- VarInt

        public int ReadVarInt()
        {
            int result = 0;
            int length = 0;

            while (true)
            {
                byte current = ReadByte();
                result |= (current & 0x7F) << length++*7;

                if (length > 6)
                    throw new InvalidDataException("Invalid varint: Too long.");

                if ((current & 0x80) != 0x80)
                    break;
            }

            return result;
        }

        public void WriteVarInt(long value)
        {
            WriteByteArray(GetVarIntBytes(value));
        }

        public static byte[] GetVarIntBytes(long value)
        {
            byte[] byteBuffer = new byte[10];
            short pos = 0;

            do
            {
                byte byteVal = (byte) (value & 0x7F);
                value >>= 7;

                if (value != 0)
                    byteVal |= 0x80;

                byteBuffer[pos] = byteVal;
                pos += 1;
            } while (value != 0);

            byte[] result = new byte[pos];
            Buffer.BlockCopy(byteBuffer, 0, result, 0, pos);

            return result;
        }

        // -- Long

        public void WriteLong(long value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            WriteByteArray(bytes);
        }

        // -- Doubles

        public void WriteDouble(double value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            WriteByteArray(bytes);
        }

        // -- Floats

        public void WriteFloat(float value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            Array.Reverse(bytes);

            WriteByteArray(bytes);
        }

        // -- Bytes

        public new byte ReadByte()
        {
            return ReadSingleByte();
        }

        public new void WriteByte(byte value)
        {
            try
            {
                SendSingleByte(value);
            }
            catch
            {
                return;
            }
        }

        // -- SByte

        public void WriteSByte(sbyte value)
        {
            try
            {
                SendSingleByte(unchecked((byte)value));
            }
            catch
            {
                return;
            }
        }

        // -- Bool

        public void WriteBool(bool value)
        {
            try
            {
                SendSingleByte(Convert.ToByte(value));
            }
            catch
            {
                return;
            }
        }

        // -- IntegerArray

        public void WriteIntArray(int[] value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                WriteInt(value[i]);
            }
        }

        // -- StringArray

        public void WriteStringArray(string[] value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                WriteString(value[i]);
            }
        }

        // -- VarIntArray

        public void WriteVarIntArray(int[] value)
        {
            int length = value.Length;

            for (int i = 0; i < length; i++)
            {
                WriteVarInt(value[i]);
            }
        }

        // -- ByteArray

        public byte[] ReadByteArray(int value)
        {
            if (!EncEnabled)
            {
                byte[] myBytes = new byte[value];
                int BytesRead;

                BytesRead = _stream.Read(myBytes, 0, value);

                while (true)
                {
                    if (BytesRead != value)
                    {
                        int newSize = value - BytesRead;
                        int BytesRead1 = _stream.Read(myBytes, BytesRead - 1, newSize);

                        if (BytesRead1 != newSize)
                        {
                            value = newSize;
                            BytesRead = BytesRead1;
                        }
                        else break;
                    }
                    else break;
                }

                return myBytes;
            }
            else
            {
                byte[] myBytes = new byte[value];
                int BytesRead;

                BytesRead = _crypto.Read(myBytes, 0, value);

                while (true)
                {
                    if (BytesRead != value)
                    {
                        int newSize = value - BytesRead;
                        int BytesRead1 = _crypto.Read(myBytes, BytesRead - 1, newSize);

                        if (BytesRead1 != newSize)
                        {
                            value = newSize;
                            BytesRead = BytesRead1;
                        }
                        else break;
                    }
                    else break;
                }

                return myBytes;
            }
        }

        public void WriteByteArray(byte[] value)
        {
            if (_buffer != null)
            {
                int tempLength = _buffer.Length + value.Length;
                byte[] tempBuff = new byte[tempLength];

                Buffer.BlockCopy(_buffer, 0, tempBuff, 0, _buffer.Length);
                Buffer.BlockCopy(value, 0, tempBuff, _buffer.Length, value.Length);

                _buffer = tempBuff;
            }
            else
                _buffer = value;
        }

        #region Send and Receive

        private byte ReadSingleByte()
        {
            if (EncEnabled)
                return (byte) _crypto.ReadByte();
            else
                return (byte) _stream.ReadByte();
        }

        private void SendSingleByte(byte thisByte)
        {
            if (_buffer != null)
            {
                byte[] tempBuff = new byte[_buffer.Length + 1];

                Buffer.BlockCopy(_buffer, 0, tempBuff, 0, _buffer.Length);
                tempBuff[_buffer.Length] = thisByte;

                _buffer = tempBuff;
            }
            else
            {
                _buffer = new byte[] {thisByte};
            }
        }

        public void Purge()
        {
            byte[] lenBytes = GetVarIntBytes(_buffer.Length);

            byte[] tempBuff = new byte[_buffer.Length + lenBytes.Length];

            Buffer.BlockCopy(lenBytes, 0, tempBuff, 0, lenBytes.Length);
            Buffer.BlockCopy(_buffer, 0, tempBuff, lenBytes.Length, _buffer.Length);

            if (EncEnabled)
                _crypto.Write(tempBuff, 0, tempBuff.Length);
            else
                _stream.Write(tempBuff, 0, tempBuff.Length);

            _buffer = null;
        }

        #endregion

        public new void Dispose()
        {
            if (_stream != null)
                _stream.Dispose();

            if (_crypto != null)
                _crypto.Dispose();
        }
    }
}
