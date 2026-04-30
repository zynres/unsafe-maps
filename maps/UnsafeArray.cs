using System.Runtime.InteropServices;

namespace unsafe_maps.maps;

public unsafe struct UnsafeArray<T> : IUnsafeMap<T>, IDisposable where T : unmanaged
{
    public T* Data;

    public int Length;
    public int Capasity;

    public UnsafeArray(int capasity)
    {    
        Length = 0;
        Capasity = capasity;
        Data = (T*)Marshal.AllocHGlobal(sizeof(T) * capasity);

        //new Span<T>(Data, capasity).Clear();
    }

    public void Set(int index, T value)
    {
        if (index > Capasity)
            throw new IndexOutOfRangeException();

        *(Data + index) = value;

        if (index >= Length)
            Length = index + 1;
    }

    public readonly ref T this[int index] => ref Data[index];

    public void Dispose()
    {
        if (Data != null)
        {
            Marshal.FreeHGlobal((IntPtr)Data);
            Data = null;
        }
    }
}
