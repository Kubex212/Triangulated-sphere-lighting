using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GK2
{
    public class BitmapOptimized : IDisposable
    {
        public Bitmap Bitmap { get; private set; }
        public Int32[] Colors { get; private set; }
        public bool Disposed { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        protected GCHandle BitsHandle { get; private set; }

        public void Dispose()
        {
            if (!Disposed)
            {
                Disposed = true;
                Bitmap.Dispose();
                BitsHandle.Free();
            }         
        }
        public BitmapOptimized(int width, int height)
        {
            Width = width;
            Height = height;
            Colors = new Int32[width * height];
            BitsHandle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
            Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
        }
        public BitmapOptimized(Bitmap img)
        {
            Width = img.Width;
            Height = img.Height;
            Colors = new Int32[Width * Height];
            BitsHandle = GCHandle.Alloc(Colors, GCHandleType.Pinned);
            Bitmap = new Bitmap(Width, Height, Width * 4, PixelFormat.Format32bppPArgb, BitsHandle.AddrOfPinnedObject());
            Copy(img);
        }
        public void Copy(Bitmap bm)
        {
            GK2.instance.Enabled = false;
            for (int x = 0; x < bm.Width; x++)
                for (int y = 0; y < bm.Height; y++)
                    SetPixel(x, y, bm.GetPixel(x, y));
            GK2.instance.Enabled = true;
        }
        public void SetPixel(int x, int y, Color colour)
        {
            Colors[x + (y * Width)] = colour.ToArgb();
        }

        public Color GetPixel(int x, int y)
        {
            return Color.FromArgb(Colors[x + (y * Width)]);
        }


    }
}
