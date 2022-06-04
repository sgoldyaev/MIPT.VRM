using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MIPT.VRM.Client.Tools
{
    // A simple class meant to help create shaders.
    public class Shader
    {
        public readonly int Handle;

        private readonly Dictionary<string, int> _uniformLocations;

        public Shader(string vertPath, string fragPath)
        {
            var shaderSource = File.ReadAllText(vertPath);

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);

            GL.ShaderSource(vertexShader, shaderSource);

            CompileShader(vertexShader);

            shaderSource = File.ReadAllText(fragPath);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, shaderSource);
            CompileShader(fragmentShader);

            this.Handle = GL.CreateProgram();

            GL.AttachShader(this.Handle, vertexShader);
            GL.AttachShader(this.Handle, fragmentShader);

            LinkProgram(this.Handle);

            GL.DetachShader(this.Handle, vertexShader);
            GL.DetachShader(this.Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

            GL.GetProgram(this.Handle, GetProgramParameterName.ActiveUniforms, out var numberOfUniforms);

            this._uniformLocations = new Dictionary<string, int>();

            for (var i = 0; i < numberOfUniforms; i++)
            {
                var key = GL.GetActiveUniform(this.Handle, i, out _, out _);

                var location = GL.GetUniformLocation(this.Handle, key);

                this._uniformLocations.Add(key, location);
            }
        }

        private static void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private static void LinkProgram(int program)
        {
            GL.LinkProgram(program);

            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }

        public void Use()
        {
            GL.UseProgram(this.Handle);
        }

        public int GetAttribLocation(string attribName)
        {
            return GL.GetAttribLocation(this.Handle, attribName);
        }

        public void SetInt(string name, int data)
        {
            GL.UseProgram(this.Handle);
            GL.Uniform1(this._uniformLocations[name], data);
        }

        public void SetFloat(string name, float data)
        {
            GL.UseProgram(this.Handle);
            GL.Uniform1(this._uniformLocations[name], data);
        }

        public void SetMatrix4(string name, Matrix4 data)
        {
            GL.UseProgram(this.Handle);
            GL.UniformMatrix4(this._uniformLocations[name], true, ref data);
        }

        public void SetVector3(string name, Vector3 data)
        {
            GL.UseProgram(this.Handle);
            GL.Uniform3(this._uniformLocations[name], data);
        }
    }
}
