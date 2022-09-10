using System.ComponentModel;
using System.Reactive.Linq;

namespace RxT;

public class Reactive<T> : INotifyPropertyChanged, IObservable<T>

{
    internal readonly IObservable<T> InnerObservable;
    private T _value;

    public Reactive(T value)
    {
        _value = value;
        InnerObservable = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                x => PropertyChanged += x,
                x => PropertyChanged -= x)
            .Select(_ => _value);
    }

    public virtual T Value
    {
        get => _value;
        set
        {
            var canTriggerChange = CanTriggerChange(_value, value);
            _value = value;
            if (canTriggerChange) TriggerChange();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;


    public virtual IDisposable Subscribe(IObserver<T> observer)
    {
        return InnerObservable
            .Subscribe(observer);
    }

    public static implicit operator T(Reactive<T> reactiveValue)
    {
        return reactiveValue.Value;
    }

    public static explicit operator Reactive<T>(T obj)
    {
        return new(obj);
    }

    /// <summary>
    /// Create Computed object from a reference of this object
    /// </summary>
    /// <param name="modifier">Observable Filter Function</param>
    /// <returns></returns>
    public virtual Computed<T> SpawnComputed(Func<IObservable<T>, IObservable<T>> modifier)
    {
        return new Computed<T>(this, modifier);
    }

    /// <summary>
    /// Create Computed object from a reference of this object with a transform function
    /// </summary>
    /// <typeparam name="TResult">Transformed Type</typeparam>
    /// <param name="transform">Transform Function</param>
    /// <param name="modifier">Observable Filter Function</param>
    /// <returns></returns>
    public virtual Computed<T, TResult> SpawnComputed<TResult>(Func<T, TResult> transform,
        Func<IObservable<T>, IObservable<T>> modifier = null)
    {
        return new Computed<T, TResult>(this, transform, modifier);
    }

    /// <summary>
    ///     Trigger that 'Value' was changed and notify the Observable
    /// </summary>
    public virtual void TriggerChange()
    {
        PropertyChanged?
            .Invoke(this,
                new PropertyChangedEventArgs(nameof(Value)));
    }

    public void Modify(Action<T, Action> modifyFunction)
    {
        modifyFunction(_value, TriggerChange);
    }

    public void Modify(Action<T> modifyFunction)
    {
        modifyFunction(_value);
        TriggerChange();
    }

    public virtual bool CanTriggerChange(T currentValue, T newValue)
    {
        return true;
    }
}