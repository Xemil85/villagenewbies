using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;
using System.Diagnostics;


namespace VillageNewbies;

public partial class ServicesPage : ContentPage
{
    private DatabaseAccess databaseAccess = new DatabaseAccess();
    public ObservableCollection<Palvelu> Palvelut { get; private set; }

    public ServicesPage()
    {
        InitializeComponent();
        Palvelut = new ObservableCollection<Palvelu>();
        ServicesCollectionView.ItemsSource = Palvelut;
        LoadPalvelut();
        LisaaPalvelu.Clicked += LisaaPalvelu_Clicked;
        
    }

    private async void LisaaPalvelu_Clicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddServicesPage());
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

    private async void Poistapalvelu(object sender, EventArgs e)
    {
        if (!(sender is Button button)) return;
        if (!(button.CommandParameter is Palvelu palvelu)) return;

        bool confirm = await DisplayAlert("Vahvistus", $"Haluatko varmasti poistaa palvelun: {palvelu.nimi}?", "Kyllä", "Ei");
        if (!confirm) return;

        var success = await databaseAccess.PoistaPalveluTietokannasta(palvelu.palvelu_id);
        if (success)
        {
            Palvelut.Remove(palvelu);
        }
        else
        {
            await DisplayAlert("Virhe", "Alueen poistaminen epäonnistui.", "OK");
        }
    }

    public async void Muokkaapalvelu(object sender, EventArgs e)
    {
        if (!(sender is Button button)) return;

        var palvelu = button.CommandParameter as Palvelu;

        if (palvelu == null)
        {
            await DisplayAlert("Virhe", "Asiakastietojen lataaminen epäonnistui.", "OK");
            return;
        }

        var popup = new ChangeServices(palvelu);
        this.ShowPopup(popup);
    }

    public partial class DatabaseAccess
    {
        public async Task<bool> PoistaPalveluTietokannasta(int palvelu_id)
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

                    var query = "DELETE FROM palvelu WHERE palvelu_id = @Palvelu_id";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Palvelu_id", palvelu_id);
                        var result = await command.ExecuteNonQueryAsync();
                        return result > 0; // palauttaa true jos yksi tai usempi rivi poistettiin

                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }
    }
}

