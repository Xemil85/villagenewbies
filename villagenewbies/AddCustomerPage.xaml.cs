using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace VillageNewbies;

public partial class AddCustomerPage : ContentPage
{
    private Asiakas _asiakas; // Jäsenmuuttuja lisätään tässä, tämä lisätty koodi

    public AddCustomerPage()
    {
        InitializeComponent();
    }
        //lisätty koodi

    public AddCustomerPage(Asiakas asiakas) : this()
    {
            _asiakas = asiakas;
        // Täytä kentät asiakkaan tiedoilla

        if (_asiakas != null)
        {
            etunimi.Text = _asiakas.etunimi;
            sukunimi.Text = _asiakas.sukunimi;
            lähiosoite.Text = _asiakas.lahiosoite;
            postinro.Text = _asiakas.postinro.ToString(); // Olettaen että postinro on tietotyyppiä, joka vaatii muunnoksen stringiksi
            puhelinnro.Text = _asiakas.puhelinnro;
            sähköposti.Text = _asiakas.email;
            // Huomioi, että 'toimipaikka' puuttuu Asiakas-luokasta tässä esimerkissä
        }

    }

    // tähän lisätty 3


    // tähän lisätty 3 loppuu

        

    // asiakkaan vienti tietokantaan
    private async void LisaaAsiakas_Clicked(object sender, EventArgs e)
    {
        var puhelinnumero = puhelinnro.Text;
        //var puhelinnumeroRegex = new Regex(@"^\+?\d+$");
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);

        if (!puhelinnumeroOK)
        {
            // Näytä virheilmoitus
            await DisplayAlert("Virheellinen puhelinnumero", "Syötä puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
            return; // Lopeta metodin suoritus tähän
        }

        // jos kentät tyhjät ja yritetään tallentaa
        if (string.IsNullOrWhiteSpace(etunimi.Text) ||
        string.IsNullOrWhiteSpace(sukunimi.Text) ||
        string.IsNullOrWhiteSpace(lähiosoite.Text) ||
        string.IsNullOrWhiteSpace(postinro.Text) ||
        string.IsNullOrWhiteSpace(sähköposti.Text) ||
        string.IsNullOrWhiteSpace(puhelinnro.Text))
        {
            // Näytä varoitusikkuna
            await DisplayAlert("Täyttämättömät tiedot", "Täytä kaikki asiakastiedot ennen lähettämistä.", "OK");
            return; // Lopeta metodin suoritus tähän
        }
             

        if (int.TryParse(postinro.Text, out _))
        {
            var uusiAsiakas = new Asiakas
            {
                etunimi = etunimi.Text,
                sukunimi = sukunimi.Text,
                lahiosoite = lähiosoite.Text,
                postinro = postinro.Text,
                email = sähköposti.Text,
                puhelinnro = puhelinnro.Text
            };

            var databaseAccess = new DatabaseAccess();
            await databaseAccess.LisaaAsiakasTietokantaan(uusiAsiakas);
        }



        // lisää tähän: palaa edelliselle sivulle tai anna käyttäjälle palaute onnistuneesta lisäyksestä
        
        etunimi.Text = "";
        sukunimi.Text = "";
        lähiosoite.Text = "";
        postinro.Text = "";
        toimipaikka.Text = "";
        sähköposti.Text = "";
        puhelinnro.Text = "";
    }

    private async void PoistaAsiakasTietokannasta_Clicked(object sender, EventArgs e)
    {
        var vastaus = await DisplayAlert("Vahvista poisto", "Haluatko varmasti poistaa tämän asiakkaan?", "Kyllä", "Ei");
        if (vastaus)
        {
            var databaseAccess = new DatabaseAccess();
            await databaseAccess.PoistaAsiakasTietokannasta(_asiakas.asiakas_id);
            await DisplayAlert("Poistettu", "Asiakas on poistettu onnistuneesti.", "OK");
            // Palaa tarvittaessa edelliselle sivulle
            await Navigation.PopAsync();
        }
    }


    public class DatabaseAccess
    {
        public async Task LisaaAsiakasTietokantaan(Asiakas uusiAsiakas)
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

                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        Debug.WriteLine(uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Postinro", uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Etunimi", uusiAsiakas.etunimi);
                        command.Parameters.AddWithValue("@Sukunimi", uusiAsiakas.sukunimi);
                        command.Parameters.AddWithValue("@Lahiosoite", uusiAsiakas.lahiosoite);
                        command.Parameters.AddWithValue("@Email", uusiAsiakas.email);
                        command.Parameters.AddWithValue("@Puhelinnro", uusiAsiakas.puhelinnro);

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



        //tähän lisäys asiakkaan poistoon
        public async Task PoistaAsiakasTietokannasta(int asiakasId)
        {
            string connectionString = "server=localhost;database=vn;user=;password=;";
            using (var connection = new MySqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    var query = "DELETE FROM asiakas WHERE asiakas_id = @AsiakasId";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@AsiakasId", asiakasId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Käsittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                }
            }
        }


        // ja tähän loppuu asiakkaan poist


    }

    
}


