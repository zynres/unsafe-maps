// Copyright © 2026 Zynres.

using System.Runtime.InteropServices;

namespace SweetLib.Collections.Unsafe.HashSet;

public unsafe struct UnsafeHashSet<T> where T : unmanaged
{
    public uint* Bucket;
    public Slot<T>* Slot;

    private uint bucketCapacity;
    private readonly byte division;

    public uint Length;
    public uint Capacity;

    public UnsafeHashSet(uint capacity, byte division = 2)
    {
        this.division = division;

        Capacity = capacity;
        bucketCapacity = Math.Max(1u, capacity / division);

        Init(ref Slot, capacity, ref Bucket, bucketCapacity);
    }

    public void Add(in T value)
    {
        if (Length >= Capacity)
            Resize(Math.Max(Capacity * 2, Length + 1));

        if (Constaint(in value))
            return;

        int hash = value.GetHashCode();
        uint bucket_index = (uint)hash % bucketCapacity;
        uint* bucket = &Bucket[bucket_index];

        Slot<T>* slot = &Slot[Length];

        slot->Value = value;
        slot->Hash = hash;
        slot->Next = *bucket;

        *bucket = Length;
        Length++;
    }

    public void Set(uint index, in T value)
    {
        if (index >= Length)
            throw new IndexOutOfRangeException();

        Slot<T>* slot = &Slot[index];

        uint bucket_index = (uint)slot->Hash % bucketCapacity;
        uint* bucket = &Bucket[bucket_index];

        while (*bucket != uint.MaxValue)
        {
            Slot<T>* linkedSlot = &Slot[*bucket];

            if (linkedSlot == slot)
            {
                *bucket = slot->Next;

                break;
            }

            // point to the next element so that, if it matches the slot to be modified, 
            // we can update its next pointer to the next of the slot being modified.
            bucket = &linkedSlot->Next;
        }

        int hash = value.GetHashCode();
        bucket_index = (uint)hash % bucketCapacity;
        bucket = &Bucket[bucket_index];

        slot->Next = *bucket;
        slot->Value = value;
        slot->Hash = hash;

        *bucket = index;
    }

    // return ref readonly because the map is built based on the hash of the value, 
    // changing the value without assigning it to a new bucket would break the map.
    public readonly ref readonly T Get(uint index)
    {
        if (index >= Length)
            throw new IndexOutOfRangeException();

        return ref Slots[index].Value;
    }

    public readonly ref readonly T this[uint index] => ref Get(index);

    public readonly bool Contains(in T value)
    {
        int hash = value.GetHashCode();
        uint bucket_index = (uint)hash % bucketCapacity;

        uint* index = &Bucket[bucket_index];

        while (true)
        {
            if (*index == uint.MaxValue)
                return false;

            Slot<T>* slot = &Slot[*index];

            if (slot->Value.Equals(value))
                return true;

            index = &slot->Next;
        }
    }

    private void Resize(uint newCapacity)
    {
        uint newBucketCapacity = Math.Max(1u, newCapacity / division);
        Capacity = newCapacity;

        Slot<T>* newSlot = null;
        uint* newBucket = null;

        Init(ref newSlot, newCapacity, ref newBucket, newBucketCapacity);

        if (Slot != null)
        {
            Buffer.MemoryCopy(
                Slot, newSlot,
                newCapacity * sizeof(Slot<T>),
                Length * sizeof(Slot<T>));
        }

        NativeMemory.Free(Bucket);
        NativeMemory.Free(Slot);

        // remapping values in the bucket because its size was changed
        for (uint i = 0; i < Length; i++)
        {
            Slot<T>* slot = &newSlot[i];

            uint bucket_index = (uint)slot->Hash % newBucketCapacity;
            uint* bucket = &newBucket[bucket_index];

            slot->Next = *bucket;

            *bucket = i;
        }

        Slot = newSlot;
        Bucket = newBucket;
        bucketCapacity = newBucketCapacity;
    }

    private static void Init(ref Slot<T>* slot, uint capacity, ref uint* bucket, uint bucketCapacity)
    {
        slot = (Slot<T>*)NativeMemory.Alloc((nuint)(sizeof(Slot<T>) * capacity));
        bucket = (uint*)NativeMemory.Alloc(sizeof(uint) * bucketCapacity);

        // Fill the bucket values to maximum values, 
        // because the check for emptiness is performed using uint.MaxValue.
        NativeMemory.Fill(bucket.Data, sizeof(uint) * bucket.Capacity, 0xFF);
    }

    public void Dispose()
    {
        if (Bucket.Data != null)
        {
            NativeMemory.Free(Bucket.Data);
            Bucket.Data = null;
        }

        if (Slots != null)
        {
            NativeMemory.Free(Slots);
            Slots = null;
        }

        Bucket.Capacity = 0;
        Capacity = 0;
        Length = 0;
    }
}