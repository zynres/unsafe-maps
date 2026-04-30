using System.Runtime.InteropServices;

namespace unsafe_maps.maps;

public unsafe struct NativeList<T> : IUnsafeMap<T>, IDisposable where T : unmanaged
{
    public T* data;
    public int length;
    public int capacity;

    public NativeList(int capacity)
    {
        length = 0;
        this.capacity = capacity;
        data = (T*)Marshal.AllocHGlobal(sizeof(T) * capacity);

        //new Span<T>(data, capacity).Clear();
    }

    public void Add(T value)
    {
        if (length >= capacity)
        {
            Resize(capacity * 2);
        }

        data[length] = value;
        length++;
    }

    private void Resize(int newCapacity)
    {
        T* newData = (T*)Marshal.AllocHGlobal(sizeof(T) * newCapacity);

        for (int i = 0; i < length; i++)
        {
            newData[i] = data[i];
        }

        Marshal.FreeHGlobal((IntPtr)data);

        data = newData;
        capacity = newCapacity;
    }

    public readonly ref T this[int index] => ref data[index];

    public void Dispose()
    {
        Marshal.FreeHGlobal((IntPtr)data);
    }
}