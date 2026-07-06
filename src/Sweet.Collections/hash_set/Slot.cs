namespace unsafe_maps.src.hash_set;

public struct Slot<T>
{
    public int Hash;
    public uint? Next;

    public T Value;
}