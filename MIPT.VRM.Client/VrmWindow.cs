using System.Drawing;
using MIPT.VRM.Client.Tools;
using MIPT.VRM.Common.Entities;
using MIPT.VRM.Common.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MIPT.VRM.Client
{
    public class VrmWindow : GameWindow
    {
        private readonly List<VrmObjectState> _states = new ()
        {
            new VrmObjectState(Guid.NewGuid(), Matrix4.Identity + Matrix4.CreateTranslation(-2 * Vector3.UnitX), 1.0f),
            new VrmObjectState(Guid.NewGuid(), Matrix4.Identity + Matrix4.CreateTranslation(3 * Vector3.UnitX), 1.5f),
        };

        private MeshData meshData;
        
        private int _indicesBufferObject;

        private int _vertexBufferObject;

        private int _textureBufferObject;

        private int _normalsBufferObject;

        private int _jointsBufferObject;

        private int _weightsBufferObject;

        private int _vertexArrayObject;

        private Shader _shader;

        private Texture _texture;

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
            var modelFile = new FileInfo("Resources/model.dae");
            var textureFile = new FileInfo("Resources/diffuse.png");
            var entityData = ColladaLoader.LoadColladaModel(modelFile, 50);
            // Vao model = createVao(entityData.Mesh);
            this.meshData = entityData.Mesh;
            // Texture texture = loadTexture(textureFile);
            var skeletonData = entityData.Joints;
            // Joint headJoint = createJoints(skeletonData.HeadJoint);
            // return new AnimatedModel(model, texture, headJoint, skeletonData.JointCount);

            
            base.OnLoad();

            GL.ClearColor(Color.Olive);

            GL.Enable(EnableCap.DepthTest);

            this._vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(this._vertexArrayObject);

            this._indicesBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this._indicesBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, meshData.Indices.Length * sizeof(int), meshData.Indices, BufferUsageHint.StaticDraw);

            this._vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.Vertices.Length * sizeof(float), meshData.Vertices, BufferUsageHint.StaticDraw);
            
            this._textureBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._textureBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.TextureCoords.Length * sizeof(float), meshData.TextureCoords, BufferUsageHint.StaticDraw);
            
            this._normalsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._normalsBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.Normals.Length * sizeof(float), meshData.Normals, BufferUsageHint.StaticDraw);
            
            this._jointsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._jointsBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.JointIds.Length * sizeof(int), meshData.JointIds, BufferUsageHint.StaticDraw);
            
            this._weightsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, this._weightsBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, meshData.VertexWeights.Length * sizeof(float), meshData.VertexWeights, BufferUsageHint.StaticDraw);
            
            /*
            this._shader = new Shader("Shaders/animatedEntityVertex.glsl", "Shaders/animatedEntityFragment.glsl");
            this._shader.Use();

            GL.EnableVertexAttribArray(this._vertexBufferObject);
            GL.VertexAttribPointer(this._vertexBufferObject, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.EnableVertexAttribArray(this._textureBufferObject);
            GL.VertexAttribPointer(this._textureBufferObject, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            GL.EnableVertexAttribArray(this._normalsBufferObject);
            GL.VertexAttribPointer(this._normalsBufferObject, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.EnableVertexAttribArray(this._jointsBufferObject);
            GL.VertexAttribPointer(this._jointsBufferObject, 3, VertexAttribPointerType.Int, false, 3 * sizeof(int), 0);

            GL.EnableVertexAttribArray(this._weightsBufferObject);
            GL.VertexAttribPointer(this._weightsBufferObject, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            */
            
            this._shader = new Shader("Shaders/shader.vert", "Shaders/shader.frag");
            this._shader.Use();

            var vertexLocation = this._shader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            var colorLocation = this._shader.GetAttribLocation("aColor");
            GL.EnableVertexAttribArray(colorLocation);
            GL.VertexAttribPointer(colorLocation, 3, VertexAttribPointerType.Float, false, 1 * sizeof(float), 3 * sizeof(float));
            
            // this._texture = Texture.LoadFromFile("Resources/container.png");
            // this._texture.Use(TextureUnit.Texture0);

            // this._texture2 = Texture.LoadFromFile("Resources/awesomeface.png");
            // this._texture2.Use(TextureUnit.Texture1);

            // this._shader.SetInt("texture0", 0);
            // this._shader.SetInt("texture1", 1);

            _texture = Texture.LoadFromFile("Resources/diffuse.png");
            _texture.Use(TextureUnit.Texture0);

            // We initialize the camera so that it is 3 units back from where the rectangle is.
            // We also give it the proper aspect ratio.
            this._camera = new Camera(Vector3.UnitZ * 2, this.Size.X / (float)this.Size.Y);

            // We make the mouse cursor invisible and captured so we can have proper FPS-camera movement.
            this.CursorGrabbed = false;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            this._time += 40.0 * e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(this._vertexArrayObject);

            this._texture.Use(TextureUnit.Texture0);
            this._shader.Use();

            foreach (var objectState in this._states)
            {
                var radians = (float)MathHelper.DegreesToRadians(this._time * objectState.Speed);
                var model = objectState.Coord * Matrix4.CreateRotationY(radians);

                /* */
                this._shader.SetMatrix4("model", model);
                this._shader.SetMatrix4("view", this._camera.GetViewMatrix());
                this._shader.SetMatrix4("projection", this._camera.GetProjectionMatrix());
                /* */

                GL.DrawArrays(PrimitiveType.Triangles, 0,this.meshData.Indices.Length / 3);
                //GL.DrawElements(PrimitiveType.Triangles, this.meshData.Indices.Length, DrawElementsType.UnsignedInt, 0);
            }
            
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
