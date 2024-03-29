using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class CustomerPage : ContentPage
{
    public ObservableCollection<Asiakas> Asiakkaat { get; private set; }
    public CustomerPage()
	{
		InitializeComponent();
        Asiakkaat = new ObservableCollection<Asiakas>();
        CustomersCollectionView.ItemsSource = Asiakkaat;
        LoadAsiakaatAsync();
        LisaaAsiakas.Clicked += LisaaAsiakas_Clicked;
    }

    private async void LisaaAsiakas_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddCustomerPage());
    }

    private async Task LoadAsiakaatAsync()
    {
        var asiakkaatAccess = new MokkiAccess();
        var asiakkaatList = await asiakkaatAccess.FetchAllAsiakasAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var asiakas in asiakkaatList)
            {
                Asiakkaat.Add(asiakas);
            }
            CustomersCollectionView.ItemsSource = Asiakkaat;
        });
    }

    // lis‰tty t‰h‰n asiakkaan tietojen muokkaus

    private async void MuokkaaAsiakasta_Clicked(object sender, EventArgs e)
    {
        if (!(sender is Button button)) return;

        var asiakas = button.CommandParameter as Asiakas;
        if (asiakas == null)
        {
            await DisplayAlert("Virhe", "Asiakastietojen lataaminen ep‰onnistui.", "OK");
            return;
        }

        // Siirryt‰‰n muokkaussivulle ja v‰litet‰‰n asiakas-olio konstruktorin kautta
        await Navigation.PushAsync(new AddCustomerPage(asiakas));
    }

    //t‰h‰n loppuu asiakkaan tietojen muokkaus


}