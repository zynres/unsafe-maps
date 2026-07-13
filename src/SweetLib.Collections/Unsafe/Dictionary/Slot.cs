namespace SweetLib.Collections.Unsafe.Dictionary;

public struct Slot<TKey, TValue> 
    where TKey : unmanaged 
    where TValue : unmanaged
{
    public int Hash;
    public uint Next;

    public TKey Key;
    public TValue Value;
}