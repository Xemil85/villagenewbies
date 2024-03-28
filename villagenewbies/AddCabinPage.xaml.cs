using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace VillageNewbies;

public partial class AddCabinPage : ContentPage
{
    public AddCabinPage()
    {
        InitializeComponent();
    }

    private async void LisaaMokki_Clicked(object sender, EventArgs e)
    {
        // jos kentät tyhjät ja yritetään tallentaa
        if (string.IsNullOrWhiteSpace(mokkinimi.Text) ||
            string.IsNullOrWhiteSpace(katuosoite.Text) ||
            string.IsNullOrWhiteSpace(postinro.Text) ||
            !int.TryParse(postinro.Text, out int parsedPostinro) ||
            string.IsNullOrWhiteSpace(hinta.Text) ||
            !double.TryParse(hinta.Text, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedHinta) ||
            string.IsNullOrWhiteSpace(henkilomaara.Text) ||
            !int.TryParse(henkilomaara.Text, out int parsedHenkilomaara) ||
            string.IsNullOrWhiteSpace(kuvaus.Text) ||
            string.IsNullOrWhiteSpace(varustelu.Text))
        
            {// Näytä virheviesti tai käsittele virhetilanne tässä
                await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki tiedot ennen lähettämistä.", "Ok");
                return; // Lopeta metodin suoritus tähän
            }
            
                var uusiMokki = new Mokki
                {
                    mokki_id = int.Parse(mokki_id.Text),
                    alue_id = int.Parse(alue_id.Text),
                    mokkinimi = mokkinimi.Text,
                    katuosoite = katuosoite.Text,
                    postinro = int.Parse(postinro.Text),
                    hinta = double.Parse(hinta.Text),
                    henkilomaara = int.Parse(hinta.Text),
                    kuvaus = kuvaus.Text,
                    varustelu = varustelu.Text
                };

                var databaseAccess = new DatabaseAccess();
                await databaseAccess.LisaaMokkiTietokantaan(uusiMokki);
        

        mokki_id.Text = "";
        alue_id.Text = "";
        mokkinimi.Text = "";
        katuosoite.Text = "";
        postinro.Text = "";
        hinta.Text = "";
        henkilomaara.Text = "";
        kuvaus.Text = "";
        varustelu.Text = "";

    }
        
}



public partial class DatabaseAccess
{
    public async Task LisaaMokkiTietokantaan(Mokki uusiMokki)
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

                var query = "INSERT INTO mokki (mokki_id, alue_id, mokkinimi, katuosoite, postinro, henkilomaara, hinta, kuvaus, varustelu)  VALUES (@Mokki_id, @Alue_id, @Mokkinimi, @Katuosoite, @Postinro, @Hinta, @Henkilomaara, @Kuvaus, @Varustelu)";

                using (var command = new MySqlCommand(query, connection))
                {
                    Debug.WriteLine(uusiMokki.postinro);
                    command.Parameters.AddWithValue("@Mokki_id", uusiMokki.mokki_id);
                    command.Parameters.AddWithValue("@Alue_id", uusiMokki.alue_id);
                    command.Parameters.AddWithValue("@Mokkinimi", uusiMokki.mokkinimi);
                    command.Parameters.AddWithValue("@Katuosoite", uusiMokki.katuosoite);
                    command.Parameters.AddWithValue("@Postinro", uusiMokki.postinro);
                    command.Parameters.AddWithValue("@Hinta", uusiMokki.hinta);
                    command.Parameters.AddWithValue("@Henkilomaara", uusiMokki.henkilomaara);
                    command.Parameters.AddWithValue("@Kuvaus", uusiMokki.kuvaus);
                    command.Parameters.AddWithValue("@Varustelu", uusiMokki.varustelu);
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

    
