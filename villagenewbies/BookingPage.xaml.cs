using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class BookingPage : ContentPage
{
    public ObservableCollection<Varaus> Varaukset { get; private set; }
    public BookingPage()
	{
		InitializeComponent();
        Varaukset = new ObservableCollection<Varaus>();
        BookingsCollectionView.ItemsSource = Varaukset;
        LoadVaraukset();
	}

    private async Task LoadVaraukset()
    {
        //TODO Toteuta oikea tietokantahaku
        var mokkiAccess = new MokkiAccess();
        var varausList = await mokkiAccess.FetchAllVarausAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var varaus in varausList)
            {
                Varaukset.Add(varaus);
            }
            BookingsCollectionView.ItemsSource = Varaukset;
        });
    }

    private async void OnMuodostaLaskuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is int varausId)
        {
            var mokkiAccess = new MokkiAccess();
            var varaus = await mokkiAccess.FetchVarausByIdAsync(varausId);
            var palvelut = await mokkiAccess.FetchPalvelutByVarausIdAsync(varausId);

            if (varaus != null && palvelut != null)
            {
                LaskunMuodostaja laskunMuodostaja = new LaskunMuodostaja();
                await laskunMuodostaja.LuoJaTallennaLaskuPdf(varaus, palvelut);
                await DisplayAlert("Valmis", "Lasku varaukselle " + varausId + " on muodostettu.", "OK");
            }
            else
            {
                await DisplayAlert("Virhe", "Varauksen tai palveluiden tietoja ei voitu hakea.", "OK");
            }
        }
    }
}
