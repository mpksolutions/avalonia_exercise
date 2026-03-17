using System;
using System.Collections.Generic;

namespace AvaloniaExercise.Models;

public interface IPedestrianSensorService
{
    /// <summary>
    /// The collection of <see cref="Pedestrian"/>s currently on the street.
    /// </summary>
    IEnumerable<Pedestrian> Pedestrians { get; }

    /// <summary>
    /// Raise when a <see cref="Pedestrian"/> is added or removed from the <see cref="Pedestrians"/> collection.
    /// </summary>
    event EventHandler<PedestriansChangedEventArgs>? PedestriansChanged;
}

public class PedestriansChangedEventArgs : EventArgs
{
    public required Pedestrian Pedestrian { get; init; }

    public required PedestrianOperation Operation { get; init; }
}

public enum PedestrianOperation
{
    Arrived,
    Left
}