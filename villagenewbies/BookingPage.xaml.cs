using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;

namespace VillageNewbies;

public partial class BookingPage : ContentPage
{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
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

            if (varaus != null)
            {
                // Tarkista, onko vahvistuspäivämäärä saavutettu
                if (DateTime.Today >= varaus.vahvistus_pvm)
                {
                    var palvelut = await mokkiAccess.FetchPalvelutByVarausIdAsync(varausId);
                    if (palvelut != null)
                    {
                        LaskunMuodostaja laskunMuodostaja = new LaskunMuodostaja();
                        await laskunMuodostaja.LuoJaTallennaLaskuPdf(varaus, palvelut);
                        await DisplayAlert("Valmis", "Lasku varaukselle " + varausId + " on muodostettu.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Virhe", "Palveluiden tietoja ei voitu hakea.", "OK");
                    }
                }
                else
                {
                    // Ilmoita käyttäjälle, että vahvistuspäivämäärää ei ole vielä saavutettu
                    await DisplayAlert("Laskun muodostus estetty", "Laskua ei voida muodostaa ennen vahvistuspäivämäärää.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Virhe", "Varauksen tietoja ei voitu hakea.", "OK");
            }
        }
    }


    private async void PeruutaVaraus(object? sender, EventArgs e)
    {
        if (!(sender is Button button)) return;
        if (!(button.CommandParameter is Varaus varaus)) return;

        // Tarkista, onko nykyinen päivämäärä saavuttanut vahvistuspäivämäärän
        if (DateTime.Today >= varaus.vahvistus_pvm)
        {
            await DisplayAlert("Peruutus estetty", "Vahvistettua varausta ei voi peruuttaa. Vahvistuspäivämäärä on jo saavutettu tai ohitettu.", "OK");
            return; // Keskeytä peruutus, jos vahvistuspäivämäärä on saavutettu tai mennyt
        }

        // Pyydä käyttäjältä vahvistusta peruutukseen
        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa varauksen: {varaus.asiakkaannimi}, {varaus.mokkinimi}?", "Kyllä", "Ei");
        if (!confirm) return;

        // Toteuta tietokannasta poistaminen tässä
        var success = await databaseAccess.PeruutaVarausTietokannasta(varaus.varaus_id);
        if (success)
        {
            Varaukset.Remove(varaus);
            await DisplayAlert("Peruutettu", "Varaus on peruutettu onnistuneesti.", "OK");
        }
        else
        {
            await DisplayAlert("Virhe", "Varauksen peruuttaminen epäonnistui.", "OK");
        }
    }


    public partial class DatabaseAccess
    {
        public async Task<bool> PeruutaVarausTietokannasta(int varaus_id)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "update varaus set peruutettu = 1 where varaus_id = @Varaus_id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Varaus_id", varaus_id);
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0; // palauttaa true jos yksi tai usempi rivi poistettiin

                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}
