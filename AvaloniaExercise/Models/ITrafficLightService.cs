using System;
using System.Threading.Tasks;

namespace AvaloniaExercise.Models;

public interface ITrafficLightService
{
    /// <summary>
    /// The current status of the traffic lights
    /// </summary>
    TrafficLightStatus Status { get; }

    /// <summary>
    /// Whether a crossing is currently requested
    /// </summary>
    bool IsCrossingRequested { get; }

    /// <summary>
    /// When a crossing is active, will return the UTC time that the crossing opportunity will end. When a crossing is inactive will return null.
    /// </summary>
    DateTime? CrossingTimeExpiryUtc { get; }

    /// <summary>
    /// Raised when <see cref="Status"/> property is changed with the updated value
    /// </summary>
    event EventHandler<TrafficLightStatus>? StatusChanged;

    /// <summary>
    /// Raised when <see cref="IsCrossingRequested"/> property is changed with the updated value
    /// </summary>
    event EventHandler<bool>? IsCrossingRequestedChanged;

    /// <summary>
    /// Raised when <see cref="CrossingTimeExpiryUtc"/> property is changed with the updated value
    /// </summary>
    event EventHandler<DateTime?>? CrossingTimeExpiryUtcChanged;

    /// <summary>
    /// Requests a crossing. The returned <see cref="Task"/> can be awaited and will resume at the end of the next crossing
    /// opportunity. If a crossing is already in progress will return immediately.
    /// </summary>
    /// <returns></returns>
    Task RequestCrossingAsync();
}

public enum TrafficLightStatus
{
    Red,
    Amber,
    Green
}