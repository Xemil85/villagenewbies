using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;


namespace VillageNewbies;

public partial class ServicesPage : ContentPage
{
    public ObservableCollection<Palvelu> Palvelut { get; private set; }

    public ServicesPage()
    {
        InitializeComponent();
        Palvelut = new ObservableCollection<Palvelu>();
        ServicesCollectionView.ItemsSource = Palvelut;
        LoadPalvelut();
    }

    private async Task LoadPalvelut()
    {
        // Tämä on pseudokoodia. TODO Toteuta oikea tietokantahaku
        var mokkiAccess = new MokkiAccess();
        var palveluList = await mokkiAccess.FetchAllPalveluAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var palvelu in palveluList)
            {
                Palvelut.Add(palvelu);
            }
            ServicesCollectionView.ItemsSource = Palvelut;
        });
    }
}

