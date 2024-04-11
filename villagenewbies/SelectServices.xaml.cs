using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class SelectServices : ContentPage
{
    public ObservableCollection<Palvelu> Palvelut { get; private set; }
    private Mokki _mokki;
    public SelectServices(Mokki mokki)
	{
		InitializeComponent();
		_mokki = mokki;
        Palvelut = new ObservableCollection<Palvelu>();
        ServicesCollectionView.ItemsSource = Palvelut;
        LoadPalvelut();
	}

    private async Task LoadPalvelut()
    {
        //TODO Toteuta oikea tietokantahaku
        var mokkiAccess = new MokkiAccess();
        var palveluList = await mokkiAccess.FetchAllPalveluWithAlueAsync(_mokki.alue_id);
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var palvelu in palveluList)
            {
                Palvelut.Add(palvelu);
            }
            ServicesCollectionView.ItemsSource = Palvelut;
        });
    }

    private async void Valitsepalvelu(object sender, EventArgs e)
    {
        if (!(sender is Button button)) return;

        var palvelu = button.CommandParameter as Palvelu;
        if (palvelu == null)
        {
            await DisplayAlert("Virhe", "Palvelutietojen lataaminen epäonnistui.", "OK");
            return;
        }
        await Navigation.PushAsync(new BookingForm(_mokki, palvelu));
    }
}