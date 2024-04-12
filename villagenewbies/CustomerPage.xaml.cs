using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace VillageNewbies;

public partial class CustomerPage : ContentPage
{
    private DatabaseAccess databaseAccess;
    public ObservableCollection<Asiakas> Asiakkaat { get; private set; }
    public CustomerPage()
	{
		InitializeComponent();
        databaseAccess = new DatabaseAccess();
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
        

    private async void HaeSukunimell‰_Clicked(object sender, EventArgs e)
    {
        string alkuKirjain = Sukunimi.Text; // Ota tekstikent‰n arvo
        if (string.IsNullOrWhiteSpace(alkuKirjain) )
        {
            await DisplayAlert("Huomio", "Anna v‰hint‰‰n yksi kirjain sukunimen alkukirjaimeksi.", "OK");
            return;
        }

        if (!Regex.IsMatch(alkuKirjain, @"^[A-Zƒ÷≈]+$"))
        {
            await DisplayAlert("Virhe", "Syˆt‰ vain kirjaimia (A-Zƒ÷≈).", "OK");
            return;
        }

        var asiakkaat = await databaseAccess.HaeAsiakkaatAlkukirjaimella(alkuKirjain);
        Asiakkaat.Clear(); // Tyhjenn‰ nykyiset asiakkaat
        foreach (var asiakas in asiakkaat)
        {
            Asiakkaat.Add(asiakas); // Lis‰‰ haetut asiakkaat
        }
    }

    //t‰h‰n loppuu asiakkaan tietojen muokkaus

    public class DatabaseAccess
    {
        private string connectionString;

        public DatabaseAccess()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
        }

        public async Task<List<Asiakas>> HaeAsiakkaatAlkukirjaimella(string alkuKirjain)
        {
            var asiakkaat = new List<Asiakas>();

            string query = "SELECT * FROM asiakas WHERE sukunimi LIKE @Sukunimi";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Sukunimi", alkuKirjain + "%");
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            asiakkaat.Add(new Asiakas
                            {
                                asiakas_id = Convert.ToInt32(reader["asiakas_id"]),  // Muuttaa arvon int:ksi turvallisesti
                                etunimi = reader["etunimi"].ToString(),               // K‰ytt‰‰ ToString()-metodia varmistaakseen, ett‰ arvo on merkkijono
                                sukunimi = reader["sukunimi"].ToString(),
                                lahiosoite = reader["lahiosoite"].ToString(),
                                postinro = reader["postinro"].ToString(),
                                email = reader["email"].ToString(),
                                puhelinnro = reader["puhelinnro"].ToString()
                            });
                        }
                    }
                }
            }
            return asiakkaat;
        }
    }
}
