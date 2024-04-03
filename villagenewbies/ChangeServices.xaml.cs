using CommunityToolkit.Maui.Views;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace VillageNewbies;

public partial class ChangeServices : Popup
{
    private Palvelu _palvelu;

    public ChangeServices()
    {
        InitializeComponent();
        TallennaPalvelu.Clicked += TallennaPalvelu_Clicked;
    }

    private async void TallennaPalvelu_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(palvelunimi.Text) ||
            string.IsNullOrWhiteSpace(Palvelutyyppi.Text) ||
            string.IsNullOrWhiteSpace(palvelukuvaus.Text) ||
            string.IsNullOrWhiteSpace(palveluhinta.Text))
        {
            Debug.WriteLine("Täytä kaikki tiedot");
            return;
        }

        if (_palvelu == null)
        {
            _palvelu = new Palvelu();
        }

        _palvelu.nimi = palvelunimi.Text;
        _palvelu.tyyppi = int.Parse(Palvelutyyppi.Text);
        _palvelu.kuvaus = palvelukuvaus.Text;
        _palvelu.hinta = double.Parse(palveluhinta.Text);

        var databaseAccess = new DatabaseAccess();
        await databaseAccess.TallennaPalveluTietokantaan(_palvelu);

        palvelunimi.Text = "";
        Palvelutyyppi.Text = "";
        palvelukuvaus.Text = "";
        palveluhinta.Text = "";

        await CloseAsync();
    }

    public ChangeServices(Palvelu palvelu) : this()
	{
        _palvelu = palvelu;

        if (_palvelu != null)
        {
            palvelunimi.Text = _palvelu.nimi;
            Palvelutyyppi.Text = _palvelu.tyyppi.ToString();
            palvelukuvaus.Text = _palvelu.kuvaus;
            palveluhinta.Text = _palvelu.hinta.ToString();
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