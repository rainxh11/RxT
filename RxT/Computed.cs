using System.ComponentModel;
using System.Reactive.Linq;

namespace RxT;

/// <summary>
///     Readonly Wrapper Reactive Object for Reactive<T>
/// </summary>
/// <typeparam name="T"></typeparam>
public class Computed<T> : IObservable<T>, INotifyPropertyChanged
{
    internal readonly Func<IObservable<T>, IObservable<T>> Modifier;

    private readonly Reactive<T> _source;

    /// <summary>
    ///     Create new Computed Reactive Object
    /// </summary>
    /// <param name="reactiveObject">Reactive Object by reference</param>
    /// <param name="modifier">Modifier function to apply filters to the inner Observable withing 'reactiveObject' parameter</param>
    public Computed(in Reactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = reactiveObject;
        Modifier = modifier;
    }

    public Computed(in ConcurrentReactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
    {
        _source = reactiveObject;
        Modifier = modifier;
    }


    public T Value => _source.Value;

    public event PropertyChangedEventHandler? PropertyChanged;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        return Modifier(_source.InnerObservable)
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