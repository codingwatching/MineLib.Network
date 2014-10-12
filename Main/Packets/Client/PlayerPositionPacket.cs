using MineLib.Network.IO;

namespace MineLib.Network.Main.Packets.Client
{
    public struct PlayerPositionPacket : IPacket
    {
        public double X, FeetY, Z;
        public bool OnGround;

        public byte ID { get { return 0x04; } }

        public void ReadPacket(PacketByteReader reader)
        {
            X = reader.ReadDouble();
            FeetY = reader.ReadDouble();
            Z = reader.ReadDouble();
            OnGround = reader.ReadBoolean();
        }

        public void WritePacket(ref PacketStream stream)
        {
            stream.WriteVarInt(ID);
            stream.WriteDouble(X);
            stream.WriteDouble(FeetY);
            stream.WriteDouble(Z);
            stream.WriteBoolean(OnGround);
            stream.Purge();
        }
    }
}