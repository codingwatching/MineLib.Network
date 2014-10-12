using MineLib.Network.IO;
using MineLib.Network.Main.Data;

namespace MineLib.Network.Main.Packets.Server
{
    public struct EntityTeleportPacket : IPacket
    {
        public int EntityID;
        public Vector3 Vector3;
        public sbyte Yaw, Pitch;
        public bool OnGround;

        public byte ID { get { return 0x18; } }

        public void ReadPacket(PacketByteReader reader)
        {
            EntityID = reader.ReadVarInt();
            Vector3 = Vector3.FromReaderIntFixedPoint(reader);
            Yaw = reader.ReadSByte();
            Pitch = reader.ReadSByte();
            OnGround = reader.ReadBoolean();
        }

        public void WritePacket(ref PacketStream stream)
        {
            stream.WriteVarInt(ID);
            stream.WriteVarInt(EntityID);
            Vector3.ToStreamIntFixedPoint(ref stream);
            stream.WriteSByte(Yaw);
            stream.WriteSByte(Pitch);
            stream.WriteBoolean(OnGround);
            stream.Purge();
        }
    }
}