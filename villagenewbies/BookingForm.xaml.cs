using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using VillageNewbies.Views;

namespace VillageNewbies;

public partial class BookingForm : ContentPage
{
    private Mokki _mokki;
    private List<Palvelu> selectedServices;
    private string _aloitusPaiva;
    private string _lopetusPaiva;
    private List<Asiakas> _asiakkaat;
    private Dictionary<int, string> _asiakasNimet = new Dictionary<int, string>();
    private int? selectedAsiakasId;

    public BookingForm(Mokki mokki, List<Palvelu> palvelu, string aloitusPaiva, string lopetusPaiva)
    {
        InitializeComponent();
        _mokki = mokki;
        selectedServices = palvelu;
        _aloitusPaiva = aloitusPaiva;
        _lopetusPaiva = lopetusPaiva;
        LataaAsiakkaat();
        TeeVaraus.Clicked += TeeVaraus_Clicked;
    }

    private async void TeeVaraus_Clicked(object? sender, EventArgs e)
    {
        DateTime aloitusPaiva = DateTime.Parse(_aloitusPaiva);
        DateTime lopetusPaiva = DateTime.Parse(_lopetusPaiva);

        TimeSpan aloitusAika = new TimeSpan(18, 0, 0);
        TimeSpan lopetusAika = new TimeSpan(14, 0, 0);

        var varattuAlkupvm = new DateTime(aloitusPaiva.Year, aloitusPaiva.Month, aloitusPaiva.Day, aloitusAika.Hours, aloitusAika.Minutes, aloitusAika.Seconds);
        var varattuLoppupvm = new DateTime(lopetusPaiva.Year, lopetusPaiva.Month, lopetusPaiva.Day, lopetusAika.Hours, lopetusAika.Minutes, lopetusAika.Seconds);

        DateTime vahvistusPvm;
        if ((aloitusPaiva - DateTime.Today).TotalDays <= 14)
        {
            vahvistusPvm = DateTime.Today;  // Vahvistetaan heti, jos varaus on alle 14 p‰iv‰n p‰‰ss‰.
        }
        else
        {
            vahvistusPvm = aloitusPaiva.AddDays(-14);  // Aseta vahvistusp‰iv‰m‰‰r‰ 14 p‰iv‰‰ ennen alkup‰iv‰m‰‰r‰‰.
        }

        
        if (string.IsNullOrWhiteSpace(Sahkoposti.Text) ||
            string.IsNullOrWhiteSpace(Etunimi.Text) ||
            string.IsNullOrWhiteSpace(Sukunimi.Text) ||
            string.IsNullOrWhiteSpace(Lahiosoite.Text) ||
            string.IsNullOrWhiteSpace(Puhelin.Text) ||
            string.IsNullOrWhiteSpace(Postinro.Text))
        {
            await DisplayAlert("T‰ytt‰m‰ttˆm‰t tiedot", "T‰yt‰ kaikki asiakastiedot ennen varauksen l‰hett‰mist‰.", "OK");
            return;
        }

        var puhelinnumero = Puhelin.Text;

        if (!puhelinnumero.All(char.IsDigit))
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Syˆt‰ kelvollinen puhelinnumero.", "OK");
            return;
        }

        // Tarkistetaan puhelinnumeron muoto
        var puhelinnumeroRegex = new Regex(@"^(\+358\d{9}|0\d{9})$");
        var puhelinnumeroOK = puhelinnumeroRegex.IsMatch(puhelinnumero);

        if (!puhelinnumeroOK)
        {
            await DisplayAlert("Virheellinen puhelinnumero", "Syˆt‰ puhelinnumero muodossa +358451234567 tai 0451234567.", "OK");
            return;
        }

        var postinumero = Postinro.Text;
        if (postinumero.Length != 5)
        {
            await DisplayAlert("Virheellinen postinumero", "Postinumeron tulee olla 5 numeron pituinen.", "OK");
            return;
        }

        var asiakasId = 0;

        if (Asiakaspicker.SelectedIndex == -1) // Oletetaan, ett‰ -1 tarkoittaa uutta asiakasta
        {
            Asiakas uusiAsiakas = new Asiakas
            {
                email = Sahkoposti.Text,
                etunimi = Etunimi.Text,
                sukunimi = Sukunimi.Text,
                lahiosoite = Lahiosoite.Text,
                puhelinnro = Puhelin.Text,
                postinro = Postinro.Text,
            };
            var databaseAsiakasAccess = new DatabaseAccess();
            asiakasId = await databaseAsiakasAccess.LisaaTaiPaivitaAsiakas(uusiAsiakas, Toimipaikka.Text);  // P‰ivitetty kutsu uudelle funktiolle
        }
        else
        {
            asiakasId = selectedAsiakasId.Value;
        }

        var uusiVaraus = new Varaus
        {
            asiakas_id = asiakasId,
            mokki_id = _mokki.mokki_id,
            varattu_pvm = DateTime.Now,
            varattu_alkupvm = varattuAlkupvm,
            varattu_loppupvm = varattuLoppupvm,
            vahvistus_pvm = vahvistusPvm
        };
        var databaseAccess = new DatabaseAccess();
        var varausId = await databaseAccess.LisaaVarausTietokantaan(uusiVaraus);

        if (selectedServices.Count > 0)
        {
            foreach (var palvelu in selectedServices)
            {
                var palveluVaraus = new Varauksen_Palvelut
                {
                    varaus_id = varausId,
                    palvelu_id = palvelu.palvelu_id,
                    lkm = selectedServices.Count
                };
                await databaseAccess.LisaaVarauksenPalvelutTietokantaan(palveluVaraus);
            }
        }

        await DisplayAlert("Uusi varaus lis‰tty", "Uusi varaus on onnistuneesti lis‰tty.", "OK");

        Asiakaspicker.SelectedIndex = -1;
        Sahkoposti.Text = "";
        Etunimi.Text = "";
        Sukunimi.Text = "";
        Lahiosoite.Text = "";
        Puhelin.Text = "";

        await Navigation.PushAsync(new MainPage());
    }



    private async void LataaAsiakkaat()
    {
        var asiakkaatAccess = new MokkiAccess(); // Oletetaan, ett‰ t‰m‰ luokka hakee tietokannasta
        _asiakkaat = await asiakkaatAccess.FetchAllAsiakasAsync();

        // Muunna haetut alueet sanakirjaksi
        _asiakasNimet = _asiakkaat.ToDictionary(a => a.asiakas_id, a => a.etunimi + " " + a.sukunimi);
        Asiakaspicker.ItemsSource = _asiakasNimet.Values.ToList();

    }

    private void OnAsiakasSelected(object sender, EventArgs e)
    {
        if (Asiakaspicker.SelectedIndex == -1)
        {
            selectedAsiakasId = null;
            Sahkoposti.Text = "";
            Etunimi.Text = "";
            Sukunimi.Text = "";
            Lahiosoite.Text = "";
            Postinro.Text = "";
            Puhelin.Text = "";
            return;
        }

        var selectedAsiakasName = Asiakaspicker.SelectedItem.ToString();
        selectedAsiakasId = _asiakasNimet.FirstOrDefault(x => x.Value == selectedAsiakasName).Key;
        var selectedAsiakas = _asiakkaat[Asiakaspicker.SelectedIndex];

        Sahkoposti.Text = selectedAsiakas.email; // Oletetaan, ett‰ asiakas-objektilla on n‰m‰ kent‰t
        Etunimi.Text = selectedAsiakas.etunimi;
        Sukunimi.Text = selectedAsiakas.sukunimi;
        Lahiosoite.Text = selectedAsiakas.lahiosoite;
        Postinro.Text = selectedAsiakas.postinro;
        Toimipaikka.Text = selectedAsiakas.toimipaikka;
        Puhelin.Text = selectedAsiakas.puhelinnro;
    }

    public class DatabaseAccess
    {
        public async Task<int> LisaaVarausTietokantaan(Varaus uusiVaraus)
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

                    var query = @"INSERT INTO varaus (asiakas_id, mokki_mokki_id, varattu_pvm, vahvistus_pvm, varattu_alkupvm, varattu_loppupvm) VALUES (@Asiakas_id, @Mokki_id, @Varattu_pvm, @Vahvistus_pvm, @Varattu_alkupvm, @Varattu_loppupvm);
                          SELECT LAST_INSERT_ID();";
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Asiakas_id", uusiVaraus.asiakas_id);
                        command.Parameters.AddWithValue("@Mokki_id", uusiVaraus.mokki_id);
                        command.Parameters.AddWithValue("@Varattu_pvm", uusiVaraus.varattu_pvm);
                        command.Parameters.AddWithValue("@Vahvistus_pvm", uusiVaraus.vahvistus_pvm);
                        command.Parameters.AddWithValue("@Varattu_alkupvm", uusiVaraus.varattu_alkupvm);
                        command.Parameters.AddWithValue("@Varattu_loppupvm", uusiVaraus.varattu_loppupvm);

                        int varausId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return varausId;
                    }
                }
                catch (Exception ex)
                {
                    // K‰sittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public async Task LisaaVarauksenPalvelutTietokantaan(Varauksen_Palvelut uusiVarauksenPalvelut)
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

                    var query = "INSERT INTO varauksen_palvelut (varaus_id, palvelu_id, lkm) VALUES (@Varaus_id, @Palvelu_id, @Lkm)";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Varaus_id", uusiVarauksenPalvelut.varaus_id);
                        command.Parameters.AddWithValue("@Palvelu_id", uusiVarauksenPalvelut.palvelu_id);
                        command.Parameters.AddWithValue("@Lkm", uusiVarauksenPalvelut.lkm);

                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // K‰sittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public async Task<int> LisaaTaiPaivitaAsiakas(Asiakas uusiAsiakas, string toimipaikka)
        {
            if (await OnkoAsiakasOlemassa(uusiAsiakas.puhelinnro))
            {
                // Asiakas on jo olemassa, joten p‰ivitet‰‰n tiedot
                return await TallennaAsiakasTietokantaan(uusiAsiakas);
            }
            else
            {
                // Asiakasta ei ole olemassa, joten lis‰t‰‰n uusi
                return await LisaaAsiakasTietokantaan(uusiAsiakas, toimipaikka);
            }
        }

        public async Task<int> LisaaAsiakasTietokantaan(Asiakas uusiAsiakas, string toimipaikka)
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

                    if (!await OnkoPostinumeroOlemassa(uusiAsiakas.postinro))
                    {
                        await LisaaPostinumero(uusiAsiakas.postinro, toimipaikka); // Olettaen, ett‰ uusiAsiakas sis‰lt‰‰ paikkakunnan
                    }

                    var query = "INSERT INTO asiakas (postinro, etunimi, sukunimi, lahiosoite, email, puhelinnro) VALUES (@Postinro, @Etunimi, @Sukunimi, @Lahiosoite, @Email, @Puhelinnro); SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        Debug.WriteLine(uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Postinro", uusiAsiakas.postinro);
                        command.Parameters.AddWithValue("@Etunimi", uusiAsiakas.etunimi);
                        command.Parameters.AddWithValue("@Sukunimi", uusiAsiakas.sukunimi);
                        command.Parameters.AddWithValue("@Lahiosoite", uusiAsiakas.lahiosoite);
                        command.Parameters.AddWithValue("@Email", uusiAsiakas.email);
                        command.Parameters.AddWithValue("@Puhelinnro", uusiAsiakas.puhelinnro);

                        int asiakasId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return asiakasId;
                    }


                }
                catch (Exception ex)
                {
                    // K‰sittely mahdollisille poikkeuksille
                    Debug.WriteLine(ex.Message);
                    throw;
                }
            }
        }

        public async Task<bool> OnkoAsiakasOlemassa(string puhelinnro) // tarkistetaan, onk asiakas jo tietokannassa
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\.."));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var tarkistusQuery = "SELECT COUNT(*) FROM asiakas WHERE puhelinnro = @Puhelinnro";

                using (var command = new MySqlCommand(tarkistusQuery, connection))
                {
                    command.Parameters.AddWithValue("@Puhelinnro", puhelinnro);
                    var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return tulos > 0;
                }
            }
        }

        public async Task<int> TallennaAsiakasTietokantaan(Asiakas asiakas)
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
                    var query = @"UPDATE asiakas 
                          SET etunimi = @etunimi, 
                              sukunimi = @sukunimi, 
                              lahiosoite = @lahiosoite, 
                              postinro = @postinro, 
                              email = @email, 
                              puhelinnro = @puhelinnro 
                          WHERE asiakas_id = @asiakas_id;
                          SELECT LAST_INSERT_ID();";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@etunimi", asiakas.etunimi);
                        command.Parameters.AddWithValue("@sukunimi", asiakas.sukunimi);
                        command.Parameters.AddWithValue("@lahiosoite", asiakas.lahiosoite);
                        command.Parameters.AddWithValue("@postinro", asiakas.postinro);
                        command.Parameters.AddWithValue("@email", asiakas.email);
                        command.Parameters.AddWithValue("@puhelinnro", asiakas.puhelinnro);
                        command.Parameters.AddWithValue("@asiakas_id", asiakas.asiakas_id);

                        await command.ExecuteNonQueryAsync();
                        int asiakasId = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return asiakasId;
                    }
                }
                catch (Exception ex)
                {
                    // K‰sittely mahdollisille poikkeuksille
                    Console.WriteLine(ex.Message);
                    throw;
                }
            }

        }

        public async Task<bool> OnkoPostinumeroOlemassa(string postinro)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT COUNT(*) FROM posti WHERE postinro = @Postinro";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);

                    var tulos = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return tulos > 0;
                }
            }
        }

        public async Task LisaaPostinumero(string postinro, string toimipaikka)
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = "INSERT INTO posti (postinro, toimipaikka) VALUES (@Postinro, @Toimipaikka)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Postinro", postinro);
                    command.Parameters.AddWithValue("@Toimipaikka", toimipaikka);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}