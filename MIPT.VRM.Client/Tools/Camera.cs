using OpenTK.Mathematics;

namespace MIPT.VRM.Client.Tools
{
    public class Camera
    {
        private Vector3 _front = -Vector3.UnitZ;

        private Vector3 _up = Vector3.UnitY;

        private Vector3 _right = Vector3.UnitX;

        private float _pitch;

        private float _yaw = -MathHelper.PiOver2;

        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            this.Position = position;
            this.AspectRatio = aspectRatio;
        }

        public Vector3 Position { get; set; }

        public float AspectRatio { private get; set; }

        public Vector3 Front => this._front;

        public Vector3 Up => this._up;

        public Vector3 Right => this._right;

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(this._pitch);
            set
            {
                var angle = MathHelper.Clamp(value, -89f, 89f);
                this._pitch = MathHelper.DegreesToRadians(angle);
                this.UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(this._yaw);
            set
            {
                this._yaw = MathHelper.DegreesToRadians(value);
                this.UpdateVectors();
            }
        }

        public float Fov
        {
            get => MathHelper.RadiansToDegrees(this._fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                this._fov = MathHelper.DegreesToRadians(angle);
            }
        }

        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(this.Position, this.Position + this._front, this._up);
        }

        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(this._fov, this.AspectRatio, 0.01f, 100f);
        }

        private void UpdateVectors()
        {
            this._front.X = MathF.Cos(this._pitch) * MathF.Cos(this._yaw);
            this._front.Y = MathF.Sin(this._pitch);
            this._front.Z = MathF.Cos(this._pitch) * MathF.Sin(this._yaw);

            this._front = Vector3.Normalize(this._front);

            this._right = Vector3.Normalize(Vector3.Cross(this._front, Vector3.UnitY));
            this._up = Vector3.Normalize(Vector3.Cross(this._right, this._front));
        }
    }
}