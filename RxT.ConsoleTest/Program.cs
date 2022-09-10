using System.Reactive.Linq;
using RxT;

var bird = new Reactive<Bird>(new Bird());

var birdSampled = bird.SpawnComputed(observable => observable.Sample(TimeSpan.FromSeconds(1)));

birdSampled
    .Do(bird => Console.WriteLine(bird.Name))
    .Subscribe();


while (true)
{
    birdSampled.ModifySource((b) => { b.Name = Path.GetRandomFileName() + "_computed"; });
    await Task.Delay(TimeSpan.FromMilliseconds(1));
}


class Bird
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
}