using System.Reactive.Linq;
using RxT;

var search = new Reactive<string>("");


var searchSampled = new Computed<string>(search, (observable) => observable.Sample(TimeSpan.FromSeconds(2)));

searchSampled
    .Do(Console.WriteLine)
    .Subscribe();

while (true)
{
    search.Value = Path.GetRandomFileName();
    await Task.Delay(TimeSpan.FromMilliseconds(10));
}