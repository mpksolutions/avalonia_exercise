using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NameGenerator.Generators;

namespace AvaloniaExercise.Models.Impl;

// We don't expect you to edit this file. Unless you really want/need to!

public class PedestrianSensorService : IPedestrianSensorService, IAsyncDisposable
{
    private const int ArrivalIntervalMsMin = 500;
    private const int ArrivalIntervalMsMax = 3000;

    private static readonly IReadOnlyList<Color> ColorPalette =
    [
        Color.Red, Color.OrangeRed, Color.Orange, Color.Gold, Color.Yellow,
        Color.YellowGreen, Color.Green, Color.Teal, Color.CornflowerBlue,
        Color.Blue, Color.Indigo, Color.Violet, Color.DeepPink, Color.HotPink,
        Color.White, Color.LightGray, Color.DimGray, Color.SaddleBrown, Color.Tan
    ];

    private readonly RealNameGenerator _realNameGenerator;
    private readonly Lock _lock;
    private readonly List<Pedestrian> _pedestrians;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _randomPedestrianArrivalTask;
    private EventHandler<PedestriansChangedEventArgs>? _pedestriansChanged;

    public PedestrianSensorService()
    {
        _realNameGenerator = new RealNameGenerator { SpaceCharacter = " " };
        _lock = new Lock();

        lock (_lock)
        {
            _pedestrians = new();
            var seedPedestriansCount = Random.Shared.Next(6, 13);
            for (int i = 0; i < seedPedestriansCount; ++i)
                _pedestrians.Add(CreateRandomPedestrian());
        }

        _cancellationTokenSource = new();
        _randomPedestrianArrivalTask = RandomPedestrianArrivalLoopAsync(_cancellationTokenSource.Token);
    }

    public async ValueTask DisposeAsync()
    {
        await _cancellationTokenSource.CancelAsync().ConfigureAwait(false);
        await _randomPedestrianArrivalTask.ConfigureAwait(false);
    }

    public IEnumerable<Pedestrian> Pedestrians
    {
        get
        {
            lock (_lock)
                return _pedestrians.ToList();
        }
    }

    public event EventHandler<PedestriansChangedEventArgs>? PedestriansChanged
    {
        add
        {
            lock (_lock)
                _pedestriansChanged += value;
        }
        remove
        {
            lock (_lock)
                _pedestriansChanged -= value;
        }
    }

    public void SetAllWaitingPedestriansCrossing()
    {
        lock (_lock)
        {
            foreach (var pedestrian in _pedestrians.Where(p => p.Status == PedestrianStatus.WaitingToCross))
                pedestrian.UpdateStatus(PedestrianStatus.Crossing);
        }
    }

    public void SetAllCrossingPedestriansCrossed()
    {
        lock (_lock)
        {
            foreach (var pedestrian in _pedestrians.Where(p => p.Status == PedestrianStatus.Crossing))
                pedestrian.UpdateStatus(PedestrianStatus.Crossed);
        }
    }


    private Pedestrian CreateRandomPedestrian()
    {
        return new Pedestrian
        {
            Name = _realNameGenerator.Generate(),
            Species = (PedestrianSpecies)Random.Shared.Next(Enum.GetValues<PedestrianSpecies>().Length),
            ShirtColor = ColorPalette[Random.Shared.Next(ColorPalette.Count)],
            ShortsColor = ColorPalette[Random.Shared.Next(ColorPalette.Count)],
            ArrivedAtUtc = DateTime.UtcNow
        };
    }

    private void RemoveCrossedPedestrians()
    {
        List<Pedestrian> pedestriansToRemove;
        EventHandler<PedestriansChangedEventArgs>? pedestriansChangedLocalCopy;

        lock (_lock)
        {
            pedestriansToRemove = _pedestrians.Where(p => p.Status == PedestrianStatus.Crossed).ToList();
            pedestriansChangedLocalCopy = _pedestriansChanged;
            foreach (var pedestrian in pedestriansToRemove)
                _pedestrians.Remove(pedestrian);
        }

        foreach (var pedestrian in pedestriansToRemove)
        {
            pedestriansChangedLocalCopy?.Invoke(this, new PedestriansChangedEventArgs
            {
                Operation = PedestrianOperation.Left,
                Pedestrian = pedestrian
            });
        }
    }

    private async Task RandomPedestrianArrivalLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(Random.Shared.Next(ArrivalIntervalMsMin, ArrivalIntervalMsMax), cancellationToken).ConfigureAwait(false);

                RemoveCrossedPedestrians();

                var pedestrian = CreateRandomPedestrian();
                EventHandler<PedestriansChangedEventArgs>? pedestriansChangedLocalCopy;

                lock (_lock)
                {
                    _pedestrians.Add(pedestrian);
                    pedestriansChangedLocalCopy = _pedestriansChanged;
                }

                pedestriansChangedLocalCopy?.Invoke(this, new PedestriansChangedEventArgs
                {
                    Operation = PedestrianOperation.Arrived,
                    Pedestrian = pedestrian
                });
            }
        }
        catch (OperationCanceledException) { }
    }
}

