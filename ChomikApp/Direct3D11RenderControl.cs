using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ChomikEngine.Models;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using static Vortice.Direct3D11.D3D11;
using EngineVec3 = ChomikEngine.Models.Vector3;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace ChomikApp;

public sealed class Direct3D11RenderControl : UserControl, IDisposable
{
    private ID3D11Device? _device;
    private ID3D11DeviceContext? _context;
    private IDXGISwapChain1? _swapChain;
    private ID3D11RenderTargetView? _renderTargetView;
    private ID3D11DepthStencilView? _depthStencilView;
    private ID3D11Buffer? _vertexBuffer;
    private ID3D11Buffer? _indexBuffer;
    private ID3D11Buffer? _constantBuffer;
    private ID3D11InputLayout? _inputLayout;
    private ID3D11VertexShader? _vertexShader;
    private ID3D11PixelShader? _pixelShader;
    private ID3D11RasterizerState? _solidState;
    private ID3D11RasterizerState? _wireframeState;
    private Mesh? _mesh;
    private int _indexCount;
    private Vector3 _target = Vector3.Zero;
    private float _yaw = 0.9f;
    private float _pitch = 0.1f;
    private float _distance = 280f;
    private bool _blueprintMode;
    private bool _pbrMode = true;
    private bool _showEdges;
    private Point _lastMouse;
    private bool _leftButtonDown;
    private bool _rightButtonDown;
    private readonly System.Windows.Forms.Timer _renderTimer;

    public Direct3D11RenderControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        DoubleBuffered = false;
        _renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
        _renderTimer.Tick += (_, _) => Render();
        _renderTimer.Start();

        HandleCreated += OnHandleCreated;
        HandleDestroyed += OnHandleDestroyed;
        Resize += (_, _) => ResizeSwapChain();
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        MouseWheel += OnMouseWheel;
        MouseDoubleClick += (_, _) => FitCamera();
    }

    public void LoadMesh(Mesh mesh)
    {
        _mesh = mesh;
        CreateGeometryBuffers();
        FitCameraToMesh();
        Render();
    }

    public void FitCamera() => FitCameraToMesh();

    public void SetBlueprintMode(bool enabled) { _blueprintMode = enabled; Render(); }
    public void SetPbrMode(bool enabled) { _pbrMode = enabled; Render(); }
    public void SetShowEdges(bool enabled) { _showEdges = enabled; Render(); }

    private void OnHandleCreated(object? sender, EventArgs e)
    {
        try { InitializeDevice(); }
        catch (Exception ex) { LogException(ex); }
    }

    private void OnHandleDestroyed(object? sender, EventArgs e)
    {
        _renderTimer.Stop();
        ReleaseResources();
    }

    protected override void OnPaint(PaintEventArgs e) { base.OnPaint(e); Render(); }

    private void OnMouseDown(object? sender, MouseEventArgs e)
    {
        _lastMouse = e.Location;
        _leftButtonDown = e.Button == MouseButtons.Left;
        _rightButtonDown = e.Button == MouseButtons.Right;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e)
    {
        if (_device == null) return;
        var delta = new Point(e.X - _lastMouse.X, e.Y - _lastMouse.Y);
        _lastMouse = e.Location;

        if (_leftButtonDown)
        {
            _yaw += delta.X * 0.0085f;
            _pitch = Math.Clamp(_pitch + delta.Y * 0.0085f, -1.4f, 1.2f);
            Render();
        }
        else if (_rightButtonDown)
        {
            var pan = new Vector3(-delta.X * 0.15f, delta.Y * 0.15f, 0);
            var rotation = Matrix4x4.CreateFromYawPitchRoll(_yaw, 0, 0);
            _target += Vector3.Transform(pan, rotation);
            Render();
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e)
    {
        _leftButtonDown = false;
        _rightButtonDown = false;
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e)
    {
        _distance = Math.Clamp(_distance * (1f - e.Delta * 0.0015f), 40f, 2000f);
        Render();
    }

    private void InitializeDevice()
    {
        if (_device != null) return;

        var flags = DeviceCreationFlags.BgraSupport;
        var levels = new[] { FeatureLevel.Level_11_0, FeatureLevel.Level_10_1, FeatureLevel.Level_10_0 };
        D3D11CreateDevice(null, DriverType.Hardware, flags, levels, out _device, out _, out _context).CheckError();

        using var factory = DXGI.CreateDXGIFactory2<IDXGIFactory2>(false);
        var swapChainDesc = new SwapChainDescription1
        {
            Width = ClientSize.Width > 0 ? (uint)ClientSize.Width : 100u,
            Height = ClientSize.Height > 0 ? (uint)ClientSize.Height : 100u,
            Format = Format.R8G8B8A8_UNorm,
            SampleDescription = new SampleDescription(1u, 0u),
            BufferUsage = Usage.RenderTargetOutput,
            BufferCount = 2,
            Scaling = Scaling.Stretch,
            SwapEffect = SwapEffect.FlipDiscard,
            AlphaMode = AlphaMode.Ignore,
            Flags = SwapChainFlags.AllowModeSwitch
        };

        _swapChain = factory.CreateSwapChainForHwnd(_device, Handle, swapChainDesc, null, null);
        factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAltEnter);
        CreateShaders();
        CreateStates();
        CreateRenderTargets();
    }

    private void ResizeSwapChain()
    {
        if (_swapChain == null || _device == null) return;
        _renderTargetView?.Dispose();
        _depthStencilView?.Dispose();
        _swapChain.ResizeBuffers(2, (uint)Math.Max(1, ClientSize.Width), (uint)Math.Max(1, ClientSize.Height), Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);
        CreateRenderTargets();
        Render();
    }

    private void CreateRenderTargets()
    {
        if (_swapChain == null || _device == null) return;
        using var backBuffer = _swapChain.GetBuffer<ID3D11Texture2D>(0);
        _renderTargetView = _device.CreateRenderTargetView(backBuffer);

        using var depthBuffer = _device.CreateTexture2D(new Texture2DDescription
        {
            Width = ClientSize.Width > 0 ? (uint)ClientSize.Width : 1u,
            Height = ClientSize.Height > 0 ? (uint)ClientSize.Height : 1u,
            MipLevels = 1, ArraySize = 1,
            Format = Format.D24_UNorm_S8_UInt,
            SampleDescription = new SampleDescription(1u, 0u),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.DepthStencil
        });
        _depthStencilView = _device.CreateDepthStencilView(depthBuffer);
    }

    private void CreateShaders()
    {
        if (_device == null) return;

        const string vsSource = @"
cbuffer SceneBuffer : register(b0) {
    float4x4 model; float4x4 view; float4x4 projection; float4 flags;
};
struct VSIn  { float3 pos : POSITION; float3 normal : NORMAL; float4 color : COLOR; };
struct PSIn  { float4 pos : SV_POSITION; float3 normal : NORMAL; float4 color : COLOR; };
PSIn VSMain(VSIn i) {
    PSIn o;
    float4 wp = mul(float4(i.pos, 1.0f), model);
    o.pos = mul(mul(wp, view), projection);
    o.normal = normalize(mul((float3x3)model, i.normal));
    o.color = i.color;
    return o;
}";

        const string psSource = @"
cbuffer SceneBuffer : register(b0) {
    float4x4 model; float4x4 view; float4x4 projection; float4 flags;
};
struct PSIn { float4 pos : SV_POSITION; float3 normal : NORMAL; float4 color : COLOR; };
float4 PSMain(PSIn i) : SV_Target {
    float3 n = normalize(i.normal);
    float3 light = normalize(float3(0.25f, 0.75f, -0.5f));
    float ndl = saturate(dot(n, light));
    float3 base = i.color.rgb;
    float3 ambient = base * 0.18f;
    float3 diffuse = base * ndl;
    float3 viewDir = normalize(-i.pos.xyz);
    float spec = pow(saturate(dot(n, normalize(light + viewDir))), 30.0f);
    float3 shaded = ambient + diffuse + spec * 0.22f;
    if (flags.y > 0.5f) shaded = lerp(ambient + diffuse, shaded, 0.85f);
    if (flags.x > 0.5f) {
        float edge = saturate(1.0f - abs(n.y) * 1.4f);
        return float4(0.18f, 0.62f, 0.84f, 1.0f) * edge + float4(0.08f, 0.12f, 0.16f, 1.0f);
    }
    return float4(shaded, 1.0f);
}";

        var vsBc = Compiler.Compile(vsSource, "VSMain", string.Empty, "vs_5_0", ShaderFlags.None, EffectFlags.None);
        var psBc = Compiler.Compile(psSource, "PSMain", string.Empty, "ps_5_0", ShaderFlags.None, EffectFlags.None);
        _vertexShader = _device.CreateVertexShader(vsBc.Span);
        _pixelShader = _device.CreatePixelShader(psBc.Span);

        _inputLayout = _device.CreateInputLayout(new[]
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElementDescription("NORMAL",   0, Format.R32G32B32_Float, 12, 0),
            new InputElementDescription("COLOR",    0, Format.R32G32B32A32_Float, 24, 0)
        }, vsBc.Span);

        _constantBuffer = _device.CreateBuffer(new BufferDescription
        {
            ByteWidth = (uint)Marshal.SizeOf<SceneBuffer>(),
            BindFlags = BindFlags.ConstantBuffer,
            Usage = ResourceUsage.Default
        });
    }

    private void CreateStates()
    {
        if (_device == null) return;
        _solidState = _device.CreateRasterizerState(new RasterizerDescription
        {
            FillMode = FillMode.Solid, CullMode = CullMode.Back, DepthClipEnable = true
        });
        _wireframeState = _device.CreateRasterizerState(new RasterizerDescription
        {
            FillMode = FillMode.Wireframe, CullMode = CullMode.None, DepthClipEnable = true
        });
    }

    private void CreateGeometryBuffers()
    {
        _vertexBuffer?.Dispose(); _indexBuffer?.Dispose();
        _vertexBuffer = null; _indexBuffer = null; _indexCount = 0;

        if (_device == null || _mesh == null || _mesh.Triangles.Count == 0) return;

        var vertices = new List<Vertex>();
        var indices = new List<uint>();
        var color = new Vector4(0.65f, 0.74f, 0.9f, 1.0f);

        foreach (var tri in _mesh.Triangles)
        {
            vertices.Add(new Vertex(ToNumerics(tri.A), ToNumerics(tri.Normal), color));
            vertices.Add(new Vertex(ToNumerics(tri.B), ToNumerics(tri.Normal), color));
            vertices.Add(new Vertex(ToNumerics(tri.C), ToNumerics(tri.Normal), color));
            uint baseIdx = (uint)(vertices.Count - 3);
            indices.Add(baseIdx); indices.Add(baseIdx + 1); indices.Add(baseIdx + 2);
        }

        _vertexBuffer = _device.CreateBuffer(vertices.ToArray(), new BufferDescription
        {
            ByteWidth = (uint)(Marshal.SizeOf<Vertex>() * vertices.Count),
            BindFlags = BindFlags.VertexBuffer, Usage = ResourceUsage.Default
        });
        _indexBuffer = _device.CreateBuffer(indices.ToArray(), new BufferDescription
        {
            ByteWidth = (uint)(sizeof(uint) * indices.Count),
            BindFlags = BindFlags.IndexBuffer, Usage = ResourceUsage.Default
        });
        _indexCount = indices.Count;
    }

    private void FitCameraToMesh()
    {
        if (_mesh == null || _mesh.Triangles.Count == 0) return;
        var center = _mesh.Center();
        _target = ToNumerics(center);

        var (mn, mx) = _mesh.Bounds();
        float dx = mx.X - mn.X, dy = mx.Y - mn.Y, dz = mx.Z - mn.Z;
        float radius = MathF.Sqrt(dx * dx + dy * dy + dz * dz) / 2f;
        _distance = Math.Max(80f, radius * 2.5f);
    }

    private void Render()
    {
        if (_device == null || _context == null || _renderTargetView == null || _depthStencilView == null) return;
        if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

        _context.OMSetRenderTargets(_renderTargetView, _depthStencilView);
        _context.RSSetViewport(0, 0, (uint)ClientSize.Width, (uint)ClientSize.Height, 0f, 1f);
        _context.ClearRenderTargetView(_renderTargetView, new Color4(0.08f, 0.1f, 0.14f, 1f));
        _context.ClearDepthStencilView(_depthStencilView, DepthStencilClearFlags.Depth, 1f, 0);

        if (_vertexBuffer != null && _indexBuffer != null && _indexCount > 0)
        {
            UpdateSceneBuffer();
            _context.IASetInputLayout(_inputLayout);
            _context.IASetVertexBuffers(0, new[] { _vertexBuffer }, new[] { (uint)Marshal.SizeOf<Vertex>() }, new[] { 0u });
            _context.IASetIndexBuffer(_indexBuffer, Format.R32_UInt, 0);
            _context.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            _context.VSSetShader(_vertexShader);
            _context.PSSetShader(_pixelShader);
            _context.VSSetConstantBuffer(0, _constantBuffer);
            _context.PSSetConstantBuffer(0, _constantBuffer);
            _context.RSSetState(_showEdges ? _wireframeState : _solidState);
            _context.DrawIndexed((uint)_indexCount, 0, 0);
        }

        _swapChain?.Present(1, PresentFlags.None);
    }

    private void UpdateSceneBuffer()
    {
        if (_context == null || _constantBuffer == null) return;
        var eye = _target + new Vector3(
            _distance * MathF.Cos(_pitch) * MathF.Cos(_yaw),
            _distance * MathF.Sin(_pitch),
            _distance * MathF.Cos(_pitch) * MathF.Sin(_yaw));

        float aspect = Math.Max(0.5f, (float)ClientSize.Width / Math.Max(1, ClientSize.Height));
        var scene = new SceneBuffer
        {
            Model      = Matrix4x4.Transpose(Matrix4x4.Identity),
            View       = Matrix4x4.Transpose(Matrix4x4.CreateLookAt(eye, _target, Vector3.UnitY)),
            Projection = Matrix4x4.Transpose(Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 4f, aspect, 0.1f, 10000f)),
            Flags      = new Vector4(_blueprintMode ? 1f : 0f, _pbrMode ? 1f : 0f, _showEdges ? 1f : 0f, 0f)
        };
        _context.UpdateSubresource(in scene, _constantBuffer);
    }

    private void ReleaseResources()
    {
        _renderTargetView?.Dispose(); _depthStencilView?.Dispose();
        _swapChain?.Dispose(); _vertexBuffer?.Dispose(); _indexBuffer?.Dispose();
        _constantBuffer?.Dispose(); _inputLayout?.Dispose();
        _vertexShader?.Dispose(); _pixelShader?.Dispose();
        _solidState?.Dispose(); _wireframeState?.Dispose();
        _device?.Dispose(); _context?.Dispose();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _renderTimer.Dispose(); ReleaseResources(); }
        base.Dispose(disposing);
    }

    private static Vector3 ToNumerics(EngineVec3 v) => new(v.X, v.Y, v.Z);

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Vertex(Vector3 position, Vector3 normal, Vector4 color)
    {
        public readonly Vector3 Position = position;
        public readonly Vector3 Normal = normal;
        public readonly Vector4 Color = color;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SceneBuffer
    {
        public Matrix4x4 Model, View, Projection;
        public Vector4 Flags;
    }

    private static void LogException(Exception ex)
    {
        try { File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "render-error.txt"), ex.ToString()); }
        catch { /* ignore */ }
    }
}
