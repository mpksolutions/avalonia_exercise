using AvaloniaExercise.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaExercise.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly IPedestrianSensorService _pedestrianSensorService;
    private readonly ITrafficLightService _trafficLightService;

    public MainWindowViewModel(IPedestrianSensorService pedestrianSensorService, ITrafficLightService trafficLightService)
    {
        _pedestrianSensorService = pedestrianSensorService;
        _trafficLightService = trafficLightService;
    }
}