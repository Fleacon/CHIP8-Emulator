namespace CHIP8;

public class Display
{
    private const int Width = 64;
    private const int Height = 32;
    
    public int Size { get; } = Width * Height;
    public byte[] Pixels { get; private set; } = new byte[Width * Height];

    public Display()
    {
        Pixels = Enumerable.Repeat((byte)0, Pixels.Length).ToArray();
    }

    public void Clear()
    {
        Pixels = Enumerable.Repeat((byte)0, Pixels.Length).ToArray();
    }
    
    public void DrawScreen(byte[] newPixels)
    {
        Pixels = newPixels;
    }

    public bool DrawSprite(byte[] sprite, int x, int y)
    {
        bool isOverlapping = false;
        
        for (int row = 0; row < sprite.Length; row++)
        {
            byte spriteRow = sprite[row];

            for (int bit = 0; bit < 8; bit++)
            {
                int pixelX = (x + bit) % Width;
                int pixelY = (y + row) % Height;
                int pixelIndex = pixelY * Width + pixelX;
                
                bool spritePixel = (spriteRow & (0x80 >> bit)) != 0;
                if (spritePixel)
                {
                    // XOR draw
                    if (Pixels[pixelIndex] == 1)
                    {
                        isOverlapping = true;
                    }

                    Pixels[pixelIndex] ^= 1;
                }
            }
        }
        return isOverlapping;
    }
}