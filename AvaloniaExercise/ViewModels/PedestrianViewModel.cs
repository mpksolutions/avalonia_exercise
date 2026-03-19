using System;
using System.Drawing;
using Avalonia.Threading;
using AvaloniaExercise.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaExercise.ViewModels;

public partial class PedestrianViewModel : ObservableObject, IDisposable
{
    private readonly Pedestrian _pedestrian;

    public Guid Id => _pedestrian.Id;
    public string Name => _pedestrian.Name;
    public string SpeciesIcon => _pedestrian.Species.ToIconName();
    public Color ShirtColor => _pedestrian.ShirtColor;
    public Color ShortsColor => _pedestrian.ShortsColor;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsWaiting))]
    [NotifyPropertyChangedFor(nameof(IsCrossing))]
    private PedestrianStatus _status;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WaitingLabel))]
    private int _waitingSeconds;

    public bool IsWaiting => Status == PedestrianStatus.WaitingToCross;
    public bool IsCrossing => Status == PedestrianStatus.Crossing;

    public string WaitingLabel =>
        WaitingSeconds >= 60
            ? $"Waited {WaitingSeconds / 60}m"
            : $"Waited {WaitingSeconds}s";

    public PedestrianViewModel(Pedestrian pedestrian)
    {
        _pedestrian = pedestrian;
        Status = pedestrian.Status;
        pedestrian.StatusChanged += OnStatusChanged;
        RefreshWaitingSeconds();
    }

    public void RefreshWaitingSeconds()
    {
        WaitingSeconds = (int)(DateTime.UtcNow - _pedestrian.ArrivedAtUtc).TotalSeconds;
    }

    private void OnStatusChanged(object? sender, PedestrianStatus status)
    {
        Dispatcher.UIThread.Post(() => Status = status);
    }

    public void Dispose()
    {
        _pedestrian.StatusChanged -= OnStatusChanged;
    }
}
