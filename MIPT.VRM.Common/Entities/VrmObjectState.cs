using OpenTK.Mathematics;

namespace MIPT.VRM.Common.Entities;

public class VrmObjectState
{
    public readonly Guid Id;
    public readonly Matrix4 Coord;
    public readonly float Speed;

    public VrmObjectState(Guid id, Matrix4 coord, float speed)
    {
        this.Id = id;
        this.Coord = coord;
        this.Speed = speed;
    }
}
