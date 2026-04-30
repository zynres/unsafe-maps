using System.Runtime.InteropServices;

namespace unsafe_maps.maps;

public unsafe struct UnsafeArray<T> : IUnsafeMap<T>, IDisposable where T : unmanaged
{
    public T* Data { get; set; }

    public int Length { get; set; }
    public int Capacity { get; set; }

    public UnsafeArray(int capasity)
    {    
        Length = 0;
        Capacity = capasity;
        Data = (T*)Marshal.AllocHGlobal(sizeof(T) * capasity);

        //new Span<T>(Data, capasity).Clear();
    }

    public void Set(int index, T value)
    {
        if (index > Capacity)
            throw new IndexOutOfRangeException();

        *(Data + index) = value;

        if (index >= Length)
            Length = index + 1;
    }

    public void SetLength(int length)
    {
        Length = length;
    }

    public readonly ref T this[int index] => ref Data[index];

    public readonly void CopyTo(IUnsafeMap<T> map)
    {
        if (Data == null)
            return;

        Buffer.MemoryCopy(
            Data, map.Data,
            map.Capacity * sizeof(T),
            map.Length * sizeof(T));
    }

    public void Dispose()
    {
        if (Data != null)
        {
            Marshal.FreeHGlobal((IntPtr)Data);
            Data = null;
        }
    }
}
