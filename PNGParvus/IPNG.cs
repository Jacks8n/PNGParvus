namespace PNGParuvs
{
    public interface IPNG<T>
    {
        uint Width { get; }
        
        uint Height { get; }

        T GetColor(int u, int v);
    }
}
