using CHIP8;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

Display display = new Display();
Input input = new Input();
Chip chip = new(display, input);

Window window = new(GameWindowSettings.Default, new NativeWindowSettings()
{
    API = ContextAPI.OpenGL,
    Profile = ContextProfile.Compatability, // or Core
    APIVersion = new Version(3, 3), // must be >= 3.2
    Title = "CHIP8 Emulator",
    Size = new Vector2i(64 * 20, 32 * 20),
    Flags = ContextFlags.Default,
    WindowBorder = WindowBorder.Fixed
}, chip, display, input);

chip.LoadFont();
//chip.LoadROM("roms/test_opcode.ch8");
//chip.LoadROM("roms/bc_test.ch8");
//chip.LoadROM("roms/snake.ch8");
//chip.LoadROM("roms/petdog.ch8");

window.Run();
