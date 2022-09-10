using System.ComponentModel;
using System.Reactive.Linq;

namespace RxT;

public class Computed<T, TResult>
{
    private readonly Func<IObservable<T>, IObservable<T>> _modifier;
    private readonly Reactive<T> _source;
    readonly Func<T, TResult> _transform;
    private TResult _transformValue;

    public TResult Value => _transformValue;

    public Computed(in Reactive<T> reactiveObject,
        Func<T, TResult> transform,
        Func<IObservable<T>, IObservable<T>>? modifier = null)
    {
        _transform = transform;
        _source = reactiveObject;
        _modifier = modifier ?? new Func<IObservable<T>, IObservable<T>>(x => x);
    }

    public Computed(in ConcurrentReactive<T> reactiveObject,
        Func<T, TResult> transform,
        Func<IObservable<T>, IObservable<T>>? modifier = null)
    {
        _transform = transform;
        _source = reactiveObject;
        _modifier = modifier ?? new Func<IObservable<T>, IObservable<T>>(x => x);
    }

    public Computed(T source,
        Func<T, TResult> transform,
        Func<IObservable<T>, IObservable<T>>? modifier = null)
    {
        _transform = transform;
        _source = new Reactive<T>(source);
        _modifier = modifier ?? new Func<IObservable<T>, IObservable<T>>(x => x);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _modifier(_source.InnerObservable)
            .Do(x => _transformValue = _transform(x))
            .Do(_ => PropertyChanged?
                .Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value))))
            .Subscribe(observer);
    }

    public void ModifySource(Action<T, Action> modifyFunction)
    {
        modifyFunction(_source.Value, _source.TriggerChange);
    }

    public void ModifySource(Action<T> modifyFunction)
    {
        modifyFunction(_source.Value);
        _source.TriggerChange();
    }
}

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
    /// <param name="modifier">_modifier function to apply filters to the inner Observable withing 'reactiveObject' parameter</param>
    public Computed(in Reactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = reactiveObject;
        _modifier = modifier;
    }

    public Computed(in ConcurrentReactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = reactiveObject;
        _modifier = modifier;
    }

    public Computed(in T source,
        Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = new Reactive<T>(source);
        _modifier = modifier;
    }


    public virtual T Value => _source.Value;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return _modifier(_source.InnerObservable)
            .Do(_ => PropertyChanged?
                .Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value))))
            .Subscribe(observer);
    }

    public void ModifySource(Action<T, Action> modifyFunction)
    {
        modifyFunction(_source.Value, _source.TriggerChange);
    }

    public void ModifySource(Action<T> modifyFunction)
    {
        modifyFunction(_source.Value);
        _source.TriggerChange();
    }
}