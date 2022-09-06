using System.ComponentModel;
using System.Reactive.Linq;

namespace RxT;

/// <summary>
///     Readonly Wrapper Reactive Object for Reactive<T>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Computed<T> : IObservable<T>, INotifyPropertyChanged
{
    private readonly Func<IObservable<T>, IObservable<T>> _modifier;

    private readonly Reactive<T> _source;

    /// <summary>
    ///     Create new Computed Reactive Object
    /// </summary>
    /// <param name="reactiveObject">Reactive Object by reference</param>
    /// <param name="modifier">Modifier function to apply filters to the inner Observable withing 'reactiveObject' parameter</param>
    public Computed(in Reactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = reactiveObject;
        _modifier = modifier;
    }


    public T Value => _source.Value;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _modifier(_source.InnerObservable)
            .Do(_ => PropertyChanged?
                .Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value))))
            .Subscribe(observer);
    }
}

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

    public T Value
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
    ///     Trigger that 'Value' was changed and notify the Observable
    /// </summary>
    public virtual void TriggerChange()
    {
        PropertyChanged?
            .Invoke(this,
                new PropertyChangedEventArgs(nameof(Value)));
    }

    public virtual bool CanTriggerChange(T currentValue, T newValue)
    {
        return true;
    }
}