using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CHIP8;

internal class Window : GameWindow
{
    private Input input;
    private Display display;
    private Chip chip;
    
    private byte[] frameBuffer = new byte[64*32];
    private int scale = 20;
    private int textureId;
    
    private const double TimerInterval = 1.0 / 60.0; // 60Hz
    private double timerAccumulator = 0.0;
    private const int InstructionsPerSecond = 500;
    private double instructionAccumulator = 0;
    private double InstructionInterval => 1.0 / InstructionsPerSecond;

    public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings, Chip chip, Display display, Input input) : base(gameWindowSettings, nativeWindowSettings)
    {
        this.chip = chip;
        this.display = display;
        this.input = input;
    }

    protected override void OnLoad()
    {
        base.OnLoad();

        // Create OpenGL texture
        textureId = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.R8,
            64, 32, 0, PixelFormat.Red, PixelType.UnsignedByte, frameBuffer);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f); // Teal background
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        
        instructionAccumulator += args.Time;
        timerAccumulator += args.Time;

        // Run instructions at throttled rate
        while (instructionAccumulator >= InstructionInterval)
        {
            chip.Cycle();
            instructionAccumulator -= InstructionInterval;
        }

        // Accumulate elapsed time and update timers at 60Hz
        timerAccumulator += args.Time;
        if (timerAccumulator >= TimerInterval)
        {
            chip.UpdateTimers();
            timerAccumulator -= TimerInterval;
        }
    }

    protected override void OnRenderFrame(OpenTK.Windowing.Common.FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        for (int i = 0; i < display.Size; i++)
        {
            frameBuffer[i] = (byte)(display.Pixels[i] == 1 ? 255 : 0);
        }
        
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, 64, 32, PixelFormat.Red, PixelType.UnsignedByte, frameBuffer);
        
        GL.Enable(EnableCap.Texture2D);
        GL.Begin(PrimitiveType.Quads);
        GL.TexCoord2(0, 1); GL.Vertex2(-1, -1);
        GL.TexCoord2(1, 1); GL.Vertex2(1, -1);
        GL.TexCoord2(1, 0); GL.Vertex2(1, 1);
        GL.TexCoord2(0, 0); GL.Vertex2(-1, 1);
        GL.End();

        SwapBuffers();
        var err = GL.GetError();
        if (err != ErrorCode.NoError)
            Console.WriteLine($"OpenGL Error: {err}");
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        input.SetKeyDown(e.Key);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        input.SetKeyUp(e.Key);
    }
}