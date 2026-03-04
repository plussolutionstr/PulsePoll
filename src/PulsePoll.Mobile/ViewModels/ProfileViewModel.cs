using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using PulsePoll.Mobile.Models;
using PulsePoll.Mobile.Services;

namespace PulsePoll.Mobile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    public ProfileViewModel(MockDataService dataService)
    {
        var profile = dataService.GetProfile();
        FullName = profile.FullName;
        Email = profile.Email;
        Tier = profile.Tier;
        Points = profile.Points;
        CompletedCount = profile.CompletedCount;
        DisqualifiedCount = profile.DisqualifiedCount;
        SuccessRate = profile.SuccessRate;
        Demographics = new ObservableCollection<DemographicField>(profile.Demographics);
        Interests = new ObservableCollection<string>(profile.Interests);
    }

    [ObservableProperty] private string _fullName = "";
    [ObservableProperty] private string _email = "";
    [ObservableProperty] private string _tier = "";
    [ObservableProperty] private int _points;
    [ObservableProperty] private int _completedCount;
    [ObservableProperty] private int _disqualifiedCount;
    [ObservableProperty] private int _successRate;
    [ObservableProperty] private ObservableCollection<DemographicField> _demographics = [];
    [ObservableProperty] private ObservableCollection<string> _interests = [];
}
