using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Collections.Generic;

namespace CHIP8;

public class Input
{
    public Dictionary<Keys, byte> KeyMap = new()
    {
        { Keys.D1, 0x01 }, { Keys.D2, 0x02 }, { Keys.D3, 0x03 }, { Keys.D4, 0x0C },
        { Keys.Q, 0x04 }, { Keys.W, 0x05 }, { Keys.E, 0x06 }, { Keys.R, 0x0D },
        { Keys.A, 0x07 }, { Keys.S, 0x08 }, { Keys.D, 0x09 }, { Keys.F, 0x0E },
        { Keys.Y, 0x0A }, { Keys.X, 0x00 }, { Keys.C, 0x0B }, { Keys.V, 0x0F }
    };
    
    public bool[] KeyStates { get; } = new bool[16];

    public bool IsKeyDown(byte byteKey) => KeyStates[byteKey];
    public bool IsAnyKeyDown() => KeyStates.Contains(true);
    public void SetKeyDown(Keys key)
    {
        if (KeyMap.TryGetValue(key, out byte byteKey))
        {
            KeyStates[byteKey] = true;
        }
    }

    public void SetKeyUp(Keys key)
    {
        if (KeyMap.TryGetValue(key, out byte chip8Key))
        {
            KeyStates[chip8Key] = false;
        }
    }
}