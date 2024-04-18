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

    private async void PeruutaVaraus(object? sender, EventArgs e)
    {

        if (!(sender is Button button)) return;
        if (!(button.CommandParameter is Varaus varaus)) return;

        if (varaus.vahvistus_pvm != null && varaus.vahvistus_pvm > new DateTime(1, 1, 1))
        {
            await DisplayAlert("Peruutus estetty", "Vahvistettua varausta ei voi peruuttaa.", "OK");
            return; // Keskeyt� peruutus, jos vahvistusp�iv�m��r� on asetettu
        }

        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa varauksen: {varaus.asiakkaannimi}, {varaus.mokkinimi} ?", "Kyll�", "Ei");
        if (!confirm) return;

        // Toteuta tietokannasta poistaminen t�ss�. Oletetaan, ett� DatabaseAccess-luokallasi on metodi, joka hoitaa t�m�n.
        var success = await databaseAccess.PeruutaVarausTietokannasta(varaus.varaus_id);
        if (success)
        {
            Varaukset.Remove(varaus);
        }
        else
        {
            await DisplayAlert("Virhe", "Varauksen peruuttaminen ep�onnistui.", "OK");
        }
        // await Navigation.PopAsync();
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
                    // K�sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}
