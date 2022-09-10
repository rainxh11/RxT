namespace RxT;

public class ConcurrentReactive<T> : Reactive<T>
{
    public ConcurrentReactive(T value) : base(value)
    {
    }

    public override T Value
    {
        get => base.Value;
        set
        {
            lock (base.Value!)
            {
                base.Value = value;
            }
        }
    }
}