﻿using MineLib.Network.IO;
using MineLib.Network.Modern.Data;

namespace MineLib.Network.Modern.Packets.Server
{
    public struct StatisticsPacket : IPacket
    {
        public StatisticsEntryList StatisticsEntryList;

        public byte ID { get { return 0x37; } }

        public void ReadPacket(PacketByteReader reader)
        {
            StatisticsEntryList = StatisticsEntryList.FromReader(reader);
        }

        public void WritePacket(ref PacketStream stream)
        {
            stream.WriteVarInt(ID);
            StatisticsEntryList.ToStream(ref stream);
            stream.Purge();
        }
    }
}