using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaExercise.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaExercise.ViewModels;

public partial class MainWindowViewModel : ObservableObject, IDisposable
{
    private readonly IPedestrianSensorService _pedestrianSensorService;
    private readonly ITrafficLightService _trafficLightService;
    private readonly DispatcherTimer _countdownTimer;

    [ObservableProperty]
    private TrafficLightStatus _trafficLightStatus;

    [ObservableProperty]
    private bool _isCrossingRequested;

    [ObservableProperty]
    private string _crossingCountdown = string.Empty;

    [ObservableProperty]
    private int _totalCrossed;

    public int WaitingCount => WaitingPedestrians.Count;
    public int CrossingCount => CrossingPedestrians.Count;
    public bool HasCrossingPedestrians => CrossingPedestrians.Count > 0;
    public bool HasWaitingPedestrians => WaitingPedestrians.Count > 0;

    private ObservableCollection<PedestrianViewModel> Pedestrians { get; } = new();
    public ObservableCollection<PedestrianViewModel> CrossingPedestrians { get; } = new();
    public ObservableCollection<PedestrianViewModel> WaitingPedestrians { get; } = new();

    public MainWindowViewModel(IPedestrianSensorService pedestrianSensorService, ITrafficLightService trafficLightService)
    {
        _pedestrianSensorService = pedestrianSensorService;
        _trafficLightService = trafficLightService;

        _trafficLightStatus = trafficLightService.Status;
        _isCrossingRequested = trafficLightService.IsCrossingRequested;

        _trafficLightService.StatusChanged += OnServiceStatusChanged;
        _trafficLightService.IsCrossingRequestedChanged += OnCrossingRequestedChanged;
        _trafficLightService.CrossingTimeExpiryUtcChanged += OnCrossingTimeExpiryChanged;
        _pedestrianSensorService.PedestriansChanged += OnPedestriansChanged;

        foreach (var pedestrian in _pedestrianSensorService.Pedestrians)
            AddPedestrianViewModel(new PedestrianViewModel(pedestrian));

        _countdownTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _countdownTimer.Tick += (_, _) => OnCountdownTick();
        _countdownTimer.Start();
    }

    [RelayCommand(CanExecute = nameof(CanRequestCrossing))]
    private async Task RequestCrossingAsync() => await _trafficLightService.RequestCrossingAsync();

    private bool CanRequestCrossing() => TrafficLightStatus == TrafficLightStatus.Green && !IsCrossingRequested;

    partial void OnTrafficLightStatusChanged(TrafficLightStatus value) =>
        RequestCrossingCommand.NotifyCanExecuteChanged();

    partial void OnIsCrossingRequestedChanged(bool value) =>
        RequestCrossingCommand.NotifyCanExecuteChanged();

    private void OnServiceStatusChanged(object? sender, TrafficLightStatus status) =>
        Dispatcher.UIThread.Post(() => TrafficLightStatus = status);

    private void OnCrossingRequestedChanged(object? sender, bool value) =>
        Dispatcher.UIThread.Post(() => IsCrossingRequested = value);

    private void OnCrossingTimeExpiryChanged(object? sender, DateTime? expiry) =>
        Dispatcher.UIThread.Post(() => UpdateCountdown(expiry));

    private void OnCountdownTick()
    {
        RefreshCountdown();
        foreach (var vm in WaitingPedestrians)
            vm.RefreshWaitingSeconds();
    }

    private void RefreshCountdown() => UpdateCountdown(_trafficLightService.CrossingTimeExpiryUtc);

    private void UpdateCountdown(DateTime? expiry)
    {
        if (!expiry.HasValue)
        {
            CrossingCountdown = string.Empty;
            return;
        }

        var remaining = expiry.Value - DateTime.UtcNow;
        CrossingCountdown = remaining > TimeSpan.Zero
            ? $"{(int)Math.Ceiling(remaining.TotalSeconds)}"
            : string.Empty;
    }

    private void OnPedestriansChanged(object? sender, PedestriansChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (e.Operation == PedestrianOperation.Arrived)
            {
                AddPedestrianViewModel(new PedestrianViewModel(e.Pedestrian));
            }
            else if (e.Operation == PedestrianOperation.Left)
            {
                var existing = Pedestrians.FirstOrDefault(p => p.Id == e.Pedestrian.Id);
                if (existing != null)
                {
                    existing.PropertyChanged -= OnPedestrianPropertyChanged;
                    existing.Dispose();
                    Pedestrians.Remove(existing);
                    WaitingPedestrians.Remove(existing);
                    CrossingPedestrians.Remove(existing);
                    NotifyCountsChanged();
                }
            }
        });
    }

    private void AddPedestrianViewModel(PedestrianViewModel vm)
    {
        vm.PropertyChanged += OnPedestrianPropertyChanged;
        Pedestrians.Add(vm);
        if (vm.IsCrossing)
            CrossingPedestrians.Add(vm);
        else
            WaitingPedestrians.Add(vm);
        NotifyCountsChanged();
    }

    private bool _rebuildPending;

    private void OnPedestrianPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PedestrianViewModel.Status) && !_rebuildPending)
        {
            _rebuildPending = true;
            Dispatcher.UIThread.Post(RebuildFilteredCollections);
        }
    }

    private void RebuildFilteredCollections()
    {
        _rebuildPending = false;

        foreach (var vm in WaitingPedestrians.Where(p => !p.IsWaiting).ToList())
            WaitingPedestrians.Remove(vm);

        var finishedCrossing = CrossingPedestrians.Where(p => !p.IsCrossing).ToList();
        TotalCrossed += finishedCrossing.Count;
        foreach (var vm in finishedCrossing)
            CrossingPedestrians.Remove(vm);

        foreach (var vm in Pedestrians.Where(p => p.IsCrossing && !CrossingPedestrians.Contains(p)))
            CrossingPedestrians.Add(vm);
        foreach (var vm in Pedestrians.Where(p => p.IsWaiting && !WaitingPedestrians.Contains(p)))
            WaitingPedestrians.Add(vm);

        NotifyCountsChanged();
    }

    private void NotifyCountsChanged()
    {
        OnPropertyChanged(nameof(WaitingCount));
        OnPropertyChanged(nameof(CrossingCount));
        OnPropertyChanged(nameof(HasCrossingPedestrians));
        OnPropertyChanged(nameof(HasWaitingPedestrians));
    }

    public void Dispose()
    {
        _trafficLightService.StatusChanged -= OnServiceStatusChanged;
        _trafficLightService.IsCrossingRequestedChanged -= OnCrossingRequestedChanged;
        _trafficLightService.CrossingTimeExpiryUtcChanged -= OnCrossingTimeExpiryChanged;
        _pedestrianSensorService.PedestriansChanged -= OnPedestriansChanged;
        _countdownTimer.Stop();
    }
}
