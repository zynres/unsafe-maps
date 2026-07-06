namespace unsafe_maps.src.dictionary;

public unsafe struct UnsafeDictionary<TKey, TValue> 
    where TKey : unmanaged 
    where TValue : unmanaged
{
    public int* Bucket;
    public Slot<TKey, TValue>* Slot;

    public int Lenght;
    public int Capacity;
}