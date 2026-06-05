using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Engine.Core.Rendering;

public class Shader : IDisposable
{
    public int Handle { get; private set; }

    public Shader(string vertexSource, string fragmentSource)
    {
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexSource);
        GL.CompileShader(vertexShader);
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vSuccess);
        if (vSuccess == 0)
            throw new Exception($"Vertex shader error: {GL.GetShaderInfoLog(vertexShader)}");

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentSource);
        GL.CompileShader(fragmentShader);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fSuccess);
        if (fSuccess == 0)
            throw new Exception($"Fragment shader error: {GL.GetShaderInfoLog(fragmentShader)}");

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vertexShader);
        GL.AttachShader(Handle, fragmentShader);
        GL.LinkProgram(Handle);
        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int lSuccess);
        if (lSuccess == 0)
            throw new Exception($"Program link error: {GL.GetProgramInfoLog(Handle)}");

        GL.DetachShader(Handle, vertexShader);
        GL.DetachShader(Handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public int GetUniformLocation(string name)
    {
        return GL.GetUniformLocation(Handle, name);
    }

    public void SetInt(string name, int value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    public void SetFloat(string name, float value)
    {
        GL.Uniform1(GetUniformLocation(name), value);
    }

    public void SetMatrix4(string name, Matrix4 matrix)
    {
        GL.UniformMatrix4(GetUniformLocation(name), false, ref matrix);
    }

    public void Dispose()
    {
        GL.DeleteProgram(Handle);
    }
}
