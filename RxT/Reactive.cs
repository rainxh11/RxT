using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using RxT.Annotations;

namespace RxT
{
    /// <summary>
    /// Readonly Wrapper Reactive Object for Reactive<T> 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Computed<T> : IObservable<T>, INotifyPropertyChanged
    {
        /// <summary>
        /// Create new Computed Reactive Object
        /// </summary>
        /// <param name="reactiveObject">Reactive Object by reference</param>
        /// <param name="modifier">Modifier function to apply filters to the inner Observable withing 'reactiveObject' parameter</param>
        public Computed(in Reactive<T> reactiveObject, Func<IObservable<T>, IObservable<T>> modifier)
        {
            _source = reactiveObject;
            _modifier = modifier;
        }

        private readonly Reactive<T> _source;

        private readonly Func<IObservable<T>, IObservable<T>> _modifier;


        public T Value => _source.Value;

        public IDisposable Subscribe(IObserver<T> observer)
        {
            return _modifier(_source.InnerObservable)
                .Do(_ => PropertyChanged?
                    .Invoke(this,
                        new PropertyChangedEventArgs(nameof(Value))))
                .Subscribe(observer);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class Reactive<T> : INotifyPropertyChanged, IObservable<T>

    {
        private T _value;
        internal readonly IObservable<T> InnerObservable;

        public event PropertyChangedEventHandler? PropertyChanged;

        public static implicit operator T(Reactive<T> reactiveValue) => reactiveValue.Value;
        public static explicit operator Reactive<T>(T obj) => new(obj);


        public virtual IDisposable Subscribe(IObserver<T> observer) =>
            InnerObservable
                .Subscribe(observer);

        /// <summary>
        /// Trigger that 'Value' was changed and notify the Observable
        /// </summary>
        public virtual void TriggerChange() =>
            PropertyChanged?
                .Invoke(this,
                    new PropertyChangedEventArgs(nameof(Value)));

        public virtual bool CanTriggerChange(T currentValue, T newValue) => true;

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
    }
}