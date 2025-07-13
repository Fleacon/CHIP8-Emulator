using OpenTK;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CHIP8;

public class Chip
{
    private byte[] Memory = new byte[4096];
    private ushort ProgramCounter = 0;
    private Stack<ushort> ProgramStack = new();
    private byte[] Registers = new byte[16];
    private ushort IndexRegister = 0;
    private byte DelayTimer = 0;
    private byte SoundTimer = 0;
    private Random random = new Random();
    
    private Display display;
    private Input input;

    private const ushort FontAddress = 0x050;
    private const ushort StartAddress = 0x200;

    private byte x;
    private byte y;
    private byte n;
    private byte kk;
    private ushort nnn;
    
    private byte[] fontShapes =
    [
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    ];

    public ushort OpCode;

    public Chip(Display display, Input input)
    {
        this.display = display;
        this.input = input;
    }

    public void LoadFont()
    {
        ProgramCounter = FontAddress;
        foreach (var font in fontShapes)
        {
            Memory[ProgramCounter] = font;
            ProgramCounter++;
        }
    }

    public void LoadROM(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found", filePath);
        }
        
        byte[] rom = File.ReadAllBytes(filePath);
        
        if (rom.Length + StartAddress > Memory.Length)
        {
            throw new IndexOutOfRangeException("ROM size is too large");
        }
        
        Array.Copy(rom, 0, Memory, StartAddress, rom.Length);
        ProgramCounter = StartAddress;
    }

    public void Cycle()
    {
        // Fetch
        OpCode = (ushort)((Memory[ProgramCounter] << 8) | Memory[ProgramCounter + 1]);
        ProgramCounter += 2;
        
        // Decode
        x = (byte)((OpCode & 0x0F00) >> 8);
        y = (byte)((OpCode & 0x00F0) >> 4);
        n = (byte)(OpCode & 0x000F);
        kk = (byte)(OpCode & 0x00FF);
        nnn = (ushort)(OpCode & 0x0FFF);

        // Execute
        switch (OpCode & 0xF000)
        {
            case 0x0000:
                switch (OpCode)
                {
                    case 0x00E0: OP_00E0(); break;
                    case 0x00EE: OP_00EE(); break;
                    default: throw new NotImplementedException($"Unknown opcode {OpCode:X4}");
                }
            break;
            case 0x1000: OP_1nnn(); break;
            case 0x2000: OP_2nnn(); break;
            case 0x3000: OP_3xkk(); break;
            case 0x4000: OP_4xkk(); break;
            case 0x5000: OP_5xy0(); break;
            case 0x6000: OP_6xkk(); break;
            case 0x7000: OP_7xkk(); break;
            case 0x8000:
                switch (OpCode & 0x000F)
                {
                    case 0x0: OP_8xy0(); break;
                    case 0x1: OP_8xy1(); break;
                    case 0x2: OP_8xy2(); break;
                    case 0x3: OP_8xy3(); break;
                    case 0x4: OP_8xy4(); break;
                    case 0x5: OP_8xy5(); break;
                    case 0x6: OP_8xy6(); break;
                    case 0x7: OP_8xy7(); break;
                    case 0xE: OP_8xyE(); break;
                    default: throw new NotImplementedException($"Unknown opcode {OpCode:X4}");
                }
            break;
            case 0x9000: OP_9xy0(); break;
            case 0xA000: OP_Annn(); break;
            case 0xB000: OP_Bnnn(); break;
            case 0xC000: OP_Cxkk(); break;
            case 0xD000: OP_Dxyn(); break;
            case 0xE000:
                switch (OpCode & 0x00FF)
                {
                    case 0x9E: OP_Ex9E(); break;
                    case 0xA1: OP_ExA1(); break;
                    default: throw new NotImplementedException($"Unknown opcode {OpCode:X4}");
                }
            break;
            case 0xF000:
                switch (OpCode & 0x00FF)
                {
                    case 0x07: OP_Fx07(); break;
                    case 0x0A: OP_Fx0A(); break;
                    case 0x15: OP_Fx15(); break;
                    case 0x18: OP_Fx18(); break;
                    case 0x1E: OP_Fx1E(); break;
                    case 0x29: OP_Fx29(); break;
                    case 0x33: OP_Fx33(); break;
                    case 0x55: OP_Fx55(); break;
                    case 0x65: OP_Fx65(); break;
                    default: throw new NotImplementedException($"Unknown opcode {OpCode:X4}");
                }
            break;
            default: throw new NotImplementedException($"Unknown opcode {OpCode:X4}");
        }
    }

    public void UpdateTimers()
    {
        if (DelayTimer > 0)
            DelayTimer--;

        if (SoundTimer > 0)
        {
            SoundTimer--;
            if (SoundTimer == 0)
            {
                // TODO: Sound
            }
        }
    }
    
    // Instructions
    
    // Clear Display
    public void OP_00E0()
    {
        display.Clear();
    }
    
    // Return from Subroutine
    public void OP_00EE()
    {
        ProgramCounter = ProgramStack.Pop();
    }
    
    // Jump To Address
    public void OP_1nnn()
    {
        ushort address = (ushort)(OpCode & 0x0FFF);
        ProgramCounter = address;
    }
    
    // Call Subroutine at Address
    public void OP_2nnn()
    {
        ProgramStack.Push(ProgramCounter);
        ushort address = (ushort)(OpCode & 0x0FFF);
        ProgramCounter = address;
    }
    
    // Skip next instruction if Vx == kk
    public void OP_3xkk()
    {
        if (Registers[x] == kk)
            SkipInstruction();
    }

    // Skip next instruction if Vx != kk
    public void OP_4xkk()
    {
        if (Registers[x] != kk)
            SkipInstruction();
    }
    
    // Skip next instruction if Vx == Vy
    public void OP_5xy0()
    {
        if (Registers[x] == Registers[y])
            SkipInstruction();
    }
    
    // Set Vx to kk
    public void OP_6xkk()
    {
        Registers[x] = kk;
    }
    
    // Set Vx = Vx + kk
    public void OP_7xkk()
    {
        Registers[x] += kk;
    }
    
    // Set Vx = Vy
    public void OP_8xy0()
    {
        Registers[x] = Registers[y];
    }
    
    // Set Vx = Vx OR Vy
    public void OP_8xy1()
    {
        Registers[x] = (byte)(Registers[x] | Registers[y]);
    }
    
    // Set Vx = Vx AND Vy
    public void OP_8xy2()
    {
        Registers[x] = (byte)(Registers[x] & Registers[y]);
    }
    
    // Set Vx = Vx XOR Vy
    public void OP_8xy3()
    {
        Registers[x] = (byte)(Registers[x] ^ Registers[y]);
    }
    
    // Set Vx = Vx + Vy, Set VF = Carry
    public void OP_8xy4()
    {
        int val = Registers[x] + Registers[y];
        if (val > 255)
        {
            Registers[0xF] = 1;
        }
        else
        {
            Registers[0xF] = 0;
        }
        Registers[x] = (byte)val;
    }
    
    // Set Vx = Vx - Vy
    public void OP_8xy5()
    {
        if (Registers[x] > Registers[y])
        {
            Registers[0xF] = 1;
        }
        else
        {
            Registers[0xF] = 0;
        }
        Registers[x] = (byte)(Registers[x] - Registers[y]);
    }
    
    // Set Vx = Vx ShiftR 1 (Divide)
    public void OP_8xy6()
    {
        // least-significant bit
        byte lsb = (byte)(Registers[x] & 0x01);
        Registers[0xF] = lsb;
        Registers[x] = (byte)(Registers[x] >> 1);
    }
    
    // Set Vx = Vy - Vx
    public void OP_8xy7()
    {
        if (Registers[y] > Registers[x])
        {
            Registers[0xF] = 1;
        }
        else
        {
            Registers[0xF] = 0;
        }
        Registers[x] = (byte)(Registers[y] - Registers[x]);
    }
    
    // Set Vx = Vx ShiftL 1 (Multiply)
    public void OP_8xyE()
    {
        // most-significant bit
        byte msb = (byte)(Registers[x] & 0x80);
        Registers[0xF] = msb;
        Registers[x] = (byte)(Registers[x] << 1);
    }
    
    // Skip next Instruction if Vx != Vy
    public void OP_9xy0()
    {
        if (Registers[x] != Registers[y])
            SkipInstruction();
    }

    // Set I = nnn
    public void OP_Annn()
    {
        IndexRegister = nnn;
    }

    // Jump to nnn + V0 
    public void OP_Bnnn()
    {
        ProgramCounter = (ushort)(nnn + Registers[0]);
    }

    // Set Vx = RandomByte AND kk
    public void OP_Cxkk()
    {
        Registers[x] = (byte)(random.Next(256) & kk);
    }
    
    // Display n-Byte Sprite starting at memory location I at (Vx, Vy), VF = collision
    public void OP_Dxyn()
    {
        byte[] sprite = new byte[n];
        for (int i = 0; i < n; i++)
        {
            sprite[i] = Memory[IndexRegister + i];
        }
        bool isOverlapping = display.DrawSprite(sprite, Registers[x], Registers[y]);
        if (isOverlapping)
        {
            Registers[0xF] = 1;
        }
        else
        {
            Registers[0xF] = 0;
        }
    }
    
    // Skip next Instruction if pressed key == Vx
    public void OP_Ex9E()
    {
        if (input.IsKeyDown(Registers[x]))
        {
            SkipInstruction();
        }
    }
    
    // Skip next Instruction if pressed key != Vx
    public void OP_ExA1()
    {
        if (!input.IsKeyDown(Registers[x]))
        {
            SkipInstruction();
        }
    }
    
    // Set Vx = delay Timer
    public void OP_Fx07()
    {
        Registers[x] = DelayTimer;
    }

    // Wait/Stop Excecution for Keypress, Vx = Key
    public void OP_Fx0A()
    {
        if (input.IsAnyKeyDown())
        {
            // first Key in Array is set
            for (int i = 0; i < input.KeyStates.Length; i++)
            {
                if (input.KeyStates[i])
                    Registers[x] = (byte)i;
                return;
            }
        }
        else
        {
            ProgramCounter -= 2;
        }
    }
    
    // Set delay timer = Vx
    public void OP_Fx15()
    {
        DelayTimer = Registers[x];
    }

    // Set sound timer = Vx
    public void OP_Fx18()
    {
        SoundTimer = Registers[x];
    }
    
    // Set I = I + Vx
    public void OP_Fx1E()
    {
        IndexRegister += Registers[x];
    }
    
    // Set I = Font Address
    public void OP_Fx29()
    {
        IndexRegister = (byte) (FontAddress + (5 * Registers[x]));
    }
    
    // Convert Binary Vx Value to Decimal and Save at I, I+1, I+2
    public void OP_Fx33()
    {
        byte val = Registers[x];
        Memory[IndexRegister + 2] = (byte)(val % 10);
        val /= 10;
        
        Memory[IndexRegister + 1] = (byte)(val % 10);
        val /= 10;
        
        Memory[IndexRegister] = (byte)(val % 10);
    }
    
    // Store register V0-Vx in Memory starting at I
    public void OP_Fx55()
    {
        for (int i = 0; i <= x; i++)
        {
            Memory[IndexRegister + i] = Registers[i];
        }
    }
    
    // Read register V0-Vx in Memory starting at I
    public void OP_Fx65()
    {
        for (int i = 0; i <= x; i++)
        {
            Registers[i] = Memory[IndexRegister + i];
        }
    }
    
    private void SkipInstruction()
    {
        ProgramCounter += 2;
    }
}