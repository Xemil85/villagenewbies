using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
namespace VillageNewbies;

public partial class DeleteAreaPage : ContentPage

{   private DatabaseAccess databaseAccess = new DatabaseAccess();
    public ObservableCollection<Alue> Alueet { get; private set; }
    public DeleteAreaPage()
	{
		InitializeComponent();
        Alueet = new ObservableCollection<Alue>();
        AreasCollectionView.ItemsSource = Alueet;
        LoadAlueetAsync();
    }

    private async Task LoadAlueetAsync()
    {
        var alueetAccess = new MokkiAccess();
        var alueetList = await alueetAccess.FetchAllAlueAsync();
        MainThread.InvokeOnMainThreadAsync(() =>
        {
            foreach (var alue in alueetList)
            {
                Alueet.Add(alue);
            }
            AreasCollectionView.ItemsSource = Alueet;
        });
    }

    private async void PoistaAlue_Clicked(object? sender, EventArgs e)
    {

        if (!(sender is Button button)) return;
        if (!(button.CommandParameter is Alue alue)) return;

        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa alueen: {alue.nimi}?", "Kyllä", "Ei");
        if (!confirm) return;

        // Toteuta tietokannasta poistaminen tässä. Oletetaan, että DatabaseAccess-luokallasi on metodi, joka hoitaa tämän.
        var success = await databaseAccess.PoistaAlueTietokannasta(alue.alue_id);
        if (success)
        {
            Alueet.Remove(alue);
        }
        else
        {
            await DisplayAlert("Virhe", "Alueen poistaminen epäonnistui.", "OK");
        }
       // await Navigation.PopAsync();
    }
}


public partial class DatabaseAccess
{
    public async Task<bool> PoistaAlueTietokannasta(int alue_id)
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

                var query = "DELETE FROM alue WHERE alue_id = @Alue_id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Alue_id", alue_id);
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