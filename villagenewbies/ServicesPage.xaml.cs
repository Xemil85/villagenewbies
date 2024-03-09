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

    private void LoadPalvelut()
    {
        // Oletetaan, ett‰ saadaan palveluiden tiedot tietokannasta seuraavasti:
        List<Palvelu> palvelutFromDb = GetServicesFromDatabase();
        foreach (var palvelu in palvelutFromDb)
        {
            Palvelut.Add(palvelu);
        }
    }

    private List<Palvelu> GetServicesFromDatabase()
    {
        // T‰m‰ on pseudokoodia. TODO Toteuta oikea tietokantahaku
        return new List<Palvelu>
            {
                // Oletetaan ett‰ tuodaan t‰ll‰ tavoin palvelu-instansseja tietokannan tiedoista
                new Palvelu { nimi = "Koiravaljakkoajelu", kuvaus = "Opastettu koiravaljakkoajelu...", hinta = 150.00, alv = 24 },
                // Lis‰‰ kaikki palvelut...
            };
    }
}

// T‰m‰ luokka kuvaa palvelua ja sen kentti‰ tietokannassas
public class Palvelu
{
    public int palvelu_id { get; set; }
    public int alue_id { get; set; }
    public string nimi { get; set; }
    public int tyyppi { get; set; } // Saattaa olla mahdollisesti parempi k‰ytt‰‰ enum?
    public string kuvaus { get; set; }
    public double hinta { get; set; }
    public double alv { get; set; }
}

