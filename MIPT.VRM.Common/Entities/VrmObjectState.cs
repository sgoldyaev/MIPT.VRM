using OpenTK.Mathematics;

namespace MIPT.VRM.Common.Entities;

public class VrmObjectState
{
    public readonly long Id;
    public Matrix4 Coord;
    public readonly float Speed;

    public VrmObjectState(long id, Matrix4 coord, float speed)
    {
        this.Id = id;
        this.Coord = coord;
        this.Speed = speed;
    }

    public void Update(Vector3 coord)
    {
        this.Coord += Matrix4.CreateTranslation(coord);
    }
}

public class VrmCommand
{
    public readonly long Id;
    public readonly Vector3 Coord;

    public VrmCommand(long id, Vector3 coord)
    {
        this.Id = id;
        this.Coord = coord;
    }
}
