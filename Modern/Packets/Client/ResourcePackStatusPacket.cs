﻿using MineLib.Network.IO;
using MineLib.Network.Modern.Enums;

namespace MineLib.Network.Modern.Packets.Client
{
    public struct ResourcePackStatusPacket : IPacket
    {
        public string Hash;
        public ResourcePackStatus Result;

        public byte ID { get { return 0x19; } }

        public void ReadPacket(PacketByteReader reader)
        {
            Hash = reader.ReadString();
            Result = (ResourcePackStatus) reader.ReadVarInt();
        }

        public void WritePacket(ref PacketStream stream)
        {
            stream.WriteVarInt(ID);
            stream.WriteString(Hash);
            stream.WriteVarInt((int) Result);
            stream.Purge();
        }
    }
}