using System;
using System.Threading.Tasks;

namespace AvaloniaExercise.Models.Impl;

// We don't expect you to edit this file. Unless you really want/need to!

public class TrafficLightService : ITrafficLightService
{
    private const int CrossingTimeMs = 5000;

    private readonly PedestrianSensorService _pedestrianSensorService;

    public TrafficLightService(PedestrianSensorService pedestrianSensorService)
    {
        _pedestrianSensorService = pedestrianSensorService;
    }

    public TrafficLightStatus Status { get; private set; } = TrafficLightStatus.Green;

    public bool IsCrossingRequested { get; private set; }

    public DateTime? CrossingTimeExpiryUtc { get; private set; }


    public event EventHandler<TrafficLightStatus>? StatusChanged;

    public event EventHandler<bool>? IsCrossingRequestedChanged;

    public event EventHandler<DateTime?>? CrossingTimeExpiryUtcChanged;


    public async Task RequestCrossingAsync()
    {
        if (IsCrossingRequested && Status != TrafficLightStatus.Green)
            return;

        UpdateIsCrossingRequested(true);

        await Task.Delay(Random.Shared.Next(1000, 5000)).ConfigureAwait(false);

        UpdateStatus(TrafficLightStatus.Amber);
        await Task.Delay(2000).ConfigureAwait(false);
        UpdateStatus(TrafficLightStatus.Red);
        UpdateIsCrossingRequested(false);

        _pedestrianSensorService.SetAllWaitingPedestriansCrossing();

        UpdateCrossingTimeExpiryUtc(DateTime.UtcNow + TimeSpan.FromMilliseconds(CrossingTimeMs));
        await Task.Delay(CrossingTimeMs).ConfigureAwait(false);
        UpdateCrossingTimeExpiryUtc(null);

        _pedestrianSensorService.SetAllCrossingPedestriansCrossed();

        UpdateStatus(TrafficLightStatus.Amber);
        await Task.Delay(1000).ConfigureAwait(false);
        UpdateStatus(TrafficLightStatus.Green);
    }

    private void UpdateIsCrossingRequested(bool isCrossingRequested)
    {
        if (IsCrossingRequested != isCrossingRequested)
        {
            IsCrossingRequested = isCrossingRequested;
            IsCrossingRequestedChanged?.Invoke(this, IsCrossingRequested);
        }
    }

    private void UpdateStatus(TrafficLightStatus status)
    {
        if (Status != status)
        {
            Status = status;
            StatusChanged?.Invoke(this, Status);
        }
    }

    private void UpdateCrossingTimeExpiryUtc(DateTime? time)
    {
        if (CrossingTimeExpiryUtc != time)
        {
            CrossingTimeExpiryUtc = time;
            CrossingTimeExpiryUtcChanged?.Invoke(this, CrossingTimeExpiryUtc);
        }
    }
}