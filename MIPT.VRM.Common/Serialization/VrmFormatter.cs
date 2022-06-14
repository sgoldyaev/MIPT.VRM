using MIPT.VRM.Common.Entities;
using OpenTK.Mathematics;

namespace MIPT.VRM.Common.Serialization;

public class VrmFormatter
{
    public byte[] WriteStates(params VrmObjectState[] objectState)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(objectState.Length);

            foreach (var state in objectState)
            {
                writer.Write(state.Id);
                writer.Write(state.Coord.M11);
                writer.Write(state.Coord.M12);
                writer.Write(state.Coord.M13);
                writer.Write(state.Coord.M14);
                writer.Write(state.Coord.M21);
                writer.Write(state.Coord.M22);
                writer.Write(state.Coord.M23);
                writer.Write(state.Coord.M24);
                writer.Write(state.Coord.M31);
                writer.Write(state.Coord.M32);
                writer.Write(state.Coord.M33);
                writer.Write(state.Coord.M34);
                writer.Write(state.Coord.M41);
                writer.Write(state.Coord.M42);
                writer.Write(state.Coord.M43);
                writer.Write(state.Coord.M44);
            }

            writer.Flush();
            stream.Flush();
            return stream.ToArray();
        }
    }

    public VrmObjectState[] ReadStates(params byte[] message)
    {
        using (var stream = new MemoryStream(message))
        using (var reader = new BinaryReader(stream))
        {
            var items = reader.ReadInt32();
            var result = new VrmObjectState[items];

            for (var i = 0; i < items; i++)
            {
                var matrix = new Matrix4();

                var id = reader.ReadInt64();
                matrix.M11 = reader.ReadSingle();
                matrix.M12 = reader.ReadSingle();
                matrix.M13 = reader.ReadSingle();
                matrix.M14 = reader.ReadSingle();
                matrix.M21 = reader.ReadSingle();
                matrix.M22 = reader.ReadSingle();
                matrix.M23 = reader.ReadSingle();
                matrix.M24 = reader.ReadSingle();
                matrix.M31 = reader.ReadSingle();
                matrix.M32 = reader.ReadSingle();
                matrix.M33 = reader.ReadSingle();
                matrix.M34 = reader.ReadSingle();
                matrix.M41 = reader.ReadSingle();
                matrix.M42 = reader.ReadSingle();
                matrix.M43 = reader.ReadSingle();
                matrix.M44 = reader.ReadSingle();
                result[i] = new VrmObjectState(id, matrix, 0.0F);
            }

            return result;
        }
    }

    public byte[] WriteCommand(VrmCommand command)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(command.Id);
            writer.Write(command.Coord.X);
            writer.Write(command.Coord.Y);
            writer.Write(command.Coord.Z);

            writer.Flush();
            return stream.ToArray();
        }
    }

    public VrmCommand ReadCommand(params byte[] message)
    {
        using (var stream = new MemoryStream(message))
        using (var reader = new BinaryReader(stream))
        {
            var vector = new Vector3();

            var id = reader.ReadInt64();
            vector.X = reader.ReadSingle();
            vector.Y = reader.ReadSingle();
            vector.Z = reader.ReadSingle();

            return new VrmCommand(id, vector);
        }
    }
}
