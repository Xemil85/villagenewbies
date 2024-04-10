using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class SelectServices : ContentPage
{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
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