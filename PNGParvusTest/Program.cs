using System;
using System.Diagnostics;
using PNGParuvs;

namespace PNGParvusTest
{
    struct Color
    {
        public byte R, G, B;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    class TestImage : IPNG<Color>
    {
        public uint Width => SIZE;

        public uint Height => SIZE;

        private const uint SIZE = 256;

        public Color GetColor(int u, int v)
        {
            return new Color((byte)u, (byte)v, 128);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            PNGParvus.Write<TestImage, Color>(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\test.png", new TestImage());
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
        }
    }
}
