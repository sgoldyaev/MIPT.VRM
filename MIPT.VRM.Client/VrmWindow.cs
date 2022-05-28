using System.Drawing;
using MIPT.VRM.Client.Tools;
using MIPT.VRM.Common.Entities;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MIPT.VRM.Client
{
    public class VrmWindow : GameWindow
    {
        private readonly VrmObject Cube = new VrmObject();
        
        private int _elementBufferObject;

        private int _vertexBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

        // private Texture _texture2;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        private double _time;

        public VrmWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            GL.ClearColor(Color.Olive);

            GL.Enable(EnableCap.DepthTest);

            this._vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this._vertexArrayObject);

            this._vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, this.Cube.Vertices.Length * sizeof(float), this.Cube.Vertices, BufferUsageHint.StaticDraw);
            
            this._shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            this._shader.Use();

            var vertexLocation = this._shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            var colorLocation = this._shader.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            // this._texture = Texture.LoadFromFile("Resources/container.png");
            // this._texture.Use(TextureUnit.Texture0);

            // this._texture2 = Texture.LoadFromFile("Resources/awesomeface.png");
            // this._texture2.Use(TextureUnit.Texture1);

            // this._shader.SetInt("texture0", 0);
            // this._shader.SetInt("texture1", 1);

            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            this._camera = new Camera(Vector3.UnitZ * 3, this.Size.X / (float)this.Size.Y);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            this.CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            this._time += 40.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(this._vertexArrayObject);

            // this._texture.Use(TextureUnit.Texture0);
            // this._texture2.Use(TextureUnit.Texture1);
            this._shader.Use();

            var model = Matrix4.Identity * Matrix4.CreateRotationX((float)MathHelper.DegreesToRadians(this._time));
            this._shader.SetMatrix4("model", model);
            this._shader.SetMatrix4("view", this._camera.GetViewMatrix());
            this._shader.SetMatrix4("projection", this._camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0,(this.Cube.Vertices.Length / 6));

            this.SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            
            

            if (!this.IsFocused) // Check to see if the window is focused
            {
                return;
            }

            var input = this.KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                this.Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (input.IsKeyDown(Keys.W))
            {
                this._camera.Position += this._camera.Front * cameraSpeed * (float)e.Time; // Forward
            }

            if (input.IsKeyDown(Keys.S))
            {
                this._camera.Position -= this._camera.Front * cameraSpeed * (float)e.Time; // Backwards
            }
            if (input.IsKeyDown(Keys.A))
            {
                this._camera.Position -= this._camera.Right * cameraSpeed * (float)e.Time; // Left
            }
            if (input.IsKeyDown(Keys.D))
            {
                this._camera.Position += this._camera.Right * cameraSpeed * (float)e.Time; // Right
            }
            if (input.IsKeyDown(Keys.Space))
            {
                this._camera.Position += this._camera.Up * cameraSpeed * (float)e.Time; // Up
            }
            if (input.IsKeyDown(Keys.LeftShift))
            {
                this._camera.Position -= this._camera.Up * cameraSpeed * (float)e.Time; // Down
            }

            // Get the mouse state
            var mouse = this.MouseState;

            return;

            if (this._firstMove) // This bool variable is initially set to true.
            {
                this._lastPos = new Vector2(mouse.X, mouse.Y);
                this._firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - this._lastPos.X;
                var deltaY = mouse.Y - this._lastPos.Y;
                this._lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                this._camera.Yaw += deltaX * sensitivity;
                this._camera.Pitch -= deltaY * sensitivity; // Reversed since y-coordinates range from bottom to top
            }
        }

        // In the mouse wheel function, we manage all the zooming of the camera.
        // This is simply done by changing the FOV of the camera.
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            this._camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, this.Size.X, this.Size.Y);
            this._camera.AspectRatio = this.Size.X / (float)this.Size.Y;
        }
    }
}
