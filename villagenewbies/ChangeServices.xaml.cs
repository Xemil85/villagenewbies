using CommunityToolkit.Maui.Views;
using MySql.Data.MySqlClient;
using System.Diagnostics;


namespace VillageNewbies;

public partial class ChangeServices : Popup
{
    private Palvelu _palvelu;
    private Dictionary<int, string> _tyyppiNimet = new Dictionary<int, string>();
    private int? selectedTypeId;

    public ChangeServices()
    {
        InitializeComponent();
        TallennaPalvelu.Clicked += TallennaPalvelu_Clicked;
        lataaTyypit();
    }

    public ChangeServices(Palvelu palvelu) : this()
    {
        _palvelu = palvelu;

        if (_palvelu != null)
        {
            palvelunimi.Text = _palvelu.nimi;
            palvelukuvaus.Text = _palvelu.kuvaus;
            palveluhinta.Text = _palvelu.hinta.ToString();
            SetTypePickerSelection();
        }
    }

    private async void TallennaPalvelu_Clicked(object? sender, EventArgs e)
    {
        var currentPage = Application.Current.MainPage;

        if (string.IsNullOrWhiteSpace(palvelunimi.Text) ||
            TypePicker.SelectedIndex == -1 ||
            string.IsNullOrWhiteSpace(palvelukuvaus.Text) ||
            string.IsNullOrWhiteSpace(palveluhinta.Text))
        {
            await currentPage.DisplayAlert("Virhe", "Täytä kaikki tiedot", "OK");
            return;
        }

        if (!double.TryParse(palveluhinta.Text, out double hinta))
        {
            await currentPage.DisplayAlert("Virhe", "Palvelun hinta tulee olla numero", "OK");
            return;
        }

        if (_palvelu == null)
        {
            _palvelu = new Palvelu();
        }

        _palvelu.nimi = palvelunimi.Text;
        _palvelu.tyyppi = selectedTypeId.Value;
        _palvelu.kuvaus = palvelukuvaus.Text;
        _palvelu.hinta = hinta;

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.TallennaPalveluTietokantaan(_palvelu);

        await currentPage.DisplayAlert("Palvelun muokkaus onnistui", "Palvelun tiedot on muutettu", "OK");

        palvelunimi.Text = "";
        TypePicker.SelectedIndex = -1;
        palvelukuvaus.Text = "";
        palveluhinta.Text = "";

        await CloseAsync();
    }

    private async void lataaTyypit()
    {
        var tyyppiAccess = new MokkiAccess(); 
        var tyypit = await tyyppiAccess.FetchAllPalveluTyypitAsync();
        _tyyppiNimet = tyypit.ToDictionary(a => a.tyyppi, a => a.nimi);
        TypePicker.ItemsSource = _tyyppiNimet.Values.ToList();
        if (_palvelu != null)
        {
            SetTypePickerSelection();
        }
    }

    private void OnTypeSelected(object sender, EventArgs e)
    {
        if (TypePicker.SelectedIndex == -1)
        {
            selectedTypeId = null;
            return;
        }

        var selectedTypeName = TypePicker.SelectedItem.ToString();
        selectedTypeId = _tyyppiNimet.FirstOrDefault(x => x.Value == selectedTypeName).Key;
    }

    private void SetTypePickerSelection()
    {
        if (_palvelu != null)
        {
            // Varmista, että tyyppi on olemassa _tyyppiNimet-sanakirjassa ennen kuin yrität asettaa sen.
            if (_tyyppiNimet.ContainsKey(_palvelu.tyyppi))
            {
                TypePicker.SelectedItem = _tyyppiNimet[_palvelu.tyyppi];
            }
            else
            {
                // Käsittele tilannetta, jossa tyyppi ei ole olemassa _tyyppiNimet-sanakirjassa.
                TypePicker.SelectedItem = null;
            }
        }
    }



    public class DatabaseAccess
    {
        public async Task TallennaPalveluTietokantaan(Palvelu palvelu)
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
                    var query = @"UPDATE palvelu 
                          SET nimi = @nimi, 
                              tyyppi = @tyyppi, 
                              kuvaus = @kuvaus, 
                              hinta = @hinta 
                          WHERE palvelu_id = @palvelu_id;";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@nimi", palvelu.nimi);
                        command.Parameters.AddWithValue("@tyyppi", palvelu.tyyppi);
                        command.Parameters.AddWithValue("@kuvaus", palvelu.kuvaus);
                        command.Parameters.AddWithValue("@hinta", palvelu.hinta);
                        command.Parameters.AddWithValue("@palvelu_id", palvelu.palvelu_id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                }
            }

        }
    }
}