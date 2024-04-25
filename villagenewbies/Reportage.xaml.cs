using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Data;

namespace VillageNewbies;

public partial class Reportage : ContentPage
{

    private DatabaseAccess databaseAccess;
    public ObservableCollection<VarausViewModel> Varaukset { get; } = new ObservableCollection<VarausViewModel>();
    public ObservableCollection<PalveluViewModel> Palvelut { get; } = new ObservableCollection<PalveluViewModel>(); private Dictionary<int, string> alueidenDictionary;
    public Reportage()
    {
        InitializeComponent();
        databaseAccess = new DatabaseAccess();
        BookinReportage.ItemsSource = Varaukset;
        ServicesReportage.ItemsSource = Palvelut;
        alueidenDictionary = new Dictionary<int, string>();
        InitializePicker();
        this.BindingContext = this;
    }

    private async void InitializePicker()
    {
        // Aseta datepickerit n‰ytt‰m‰‰n t‰m‰n p‰iv‰n p‰iv‰m‰‰r‰
        StartDatePicker.Date = DateTime.Today;
        EndDatePicker.Date = DateTime.Today.AddDays(1); // Oletus loppup‰iv‰m‰‰r‰ on huomenna

        await LataaAlueet();
    }

    public async Task LataaAlueet()
    {
        alueidenDictionary = await databaseAccess.HaeAlueet();
        AreaPicker.ItemsSource = alueidenDictionary.Values.ToList();
        if (alueidenDictionary.Count > 0)
            AreaPicker.SelectedIndex = 0;
    }

    private async void Varaustenhaku_Clicked(object sender, EventArgs e)
    {

                
        if (AreaPicker.SelectedIndex != -1 && AreaPicker.SelectedItem != null)
        {

            var selectedAreaName = (string)AreaPicker.SelectedItem;

            if (alueidenDictionary.ContainsValue(selectedAreaName))
            {
                var selectedAreaId = alueidenDictionary.FirstOrDefault(x => x.Value == selectedAreaName).Key;
                var alkuPvm = StartDatePicker.Date;
                var loppuPvm = EndDatePicker.Date;

                // Tarkistetaan, ett‰ loppup‰iv‰m‰‰r‰ ei ole ennen alkup‰iv‰m‰‰r‰‰
                if (loppuPvm < alkuPvm)
                {
                    await DisplayAlert("Virheelliset p‰iv‰m‰‰r‰t", "Tarkista syˆtˆt.", "OK");
                    return;
                }

                var varauksetLista = await databaseAccess.HaeVaraukset(selectedAreaId, alkuPvm, loppuPvm);
                Varaukset.Clear();

                if (varauksetLista.Count > 0)
                {
                    foreach (var varausViewModel in varauksetLista)
                    {
                        Varaukset.Add(varausViewModel);
                    }
                }
                else
                {
                    // Ei varauksia valitulla aikav‰lill‰, n‰ytet‰‰n ilmoitus.
                    await DisplayAlert("Ei varauksia", "Valitulla ajalla ei lˆytynyt varauksia.", "OK");
                }
            }
            else
            {
                // Jos alueen valinta ei t‰sm‰‰, n‰ytet‰‰n virheilmoitus.
                await DisplayAlert("Virhe", "Valitse alue uudelleen.", "OK");
            }
        }
        else
        {
            // Aluetta ei ole valittu, n‰ytet‰‰n virheilmoitus.
            await DisplayAlert("Virhe", "Valitse ensin alue.", "OK");
        }    
    }

    private async void Palveluhaku_Clicked(object sender, EventArgs e)
    {
        if (AreaPicker.SelectedIndex != -1 && AreaPicker.SelectedItem != null)
        {

            var selectedAreaName = (string)AreaPicker.SelectedItem;

            if (alueidenDictionary.ContainsValue(selectedAreaName))
            {
                var selectedAreaId = alueidenDictionary.FirstOrDefault(x => x.Value == selectedAreaName).Key;
                var alkuPvm = StartDatePicker.Date;
                var loppuPvm = EndDatePicker.Date;
                var palvelutLista = await databaseAccess.HaePalvelutVarauksille(selectedAreaId, alkuPvm, loppuPvm);
                Palvelut.Clear();

                foreach (var palvelu in palvelutLista)
                {
                    Palvelut.Add(palvelu);
                }
            }
            else
            {
                DisplayAlert("Virhe", "Palveluiden haku ei ole viel‰ toteutettu.", "OK");
            }
        }
    }
    public class DatabaseAccess
    {
        private readonly string connectionString;

        public DatabaseAccess()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            connectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";
        }

        public async Task<Dictionary<int, string>> HaeAlueet()
        {
            Dictionary<int, string> alueet = new Dictionary<int, string>();
            using (var connection = new MySqlConnection(connectionString))

            {
                await connection.OpenAsync();
                string query = "SELECT alue_id, nimi FROM alue";
                using (var command = new MySqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            alueet.Add(reader.GetInt32("alue_id"), reader.GetString("nimi"));

                        }
                    }
                }
            }
            return alueet;
        }


        public async Task<List<VarausViewModel>> HaeVaraukset(int alueId, DateTime startDate, DateTime endDate)
        {
            List<VarausViewModel> varaukset = new List<VarausViewModel>();
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT v.varaus_id, m.mokkinimi, v.varattu_alkupvm, v.varattu_loppupvm
                FROM varaus v
                JOIN mokki m ON v.mokki_mokki_id = m.mokki_id
                WHERE m.alue_id = @AlueId
                AND v.varattu_loppupvm >= @StartDate
                AND v.varattu_alkupvm <= @EndDate;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AlueId", alueId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var varausViewModel = new VarausViewModel
                            {
                                Varaus = new Varaus
                                {
                                    varaus_id = reader.GetInt32("varaus_id"),
                                    varattu_alkupvm = reader.GetDateTime("varattu_alkupvm"),
                                    varattu_loppupvm = reader.GetDateTime("varattu_loppupvm")
                                },
                                Mokkinimi = reader.GetString("mokkinimi")
                            };
                            varaukset.Add(varausViewModel);
                        }
                    }
                }
            }
            return varaukset;
        }

        public async Task<List<PalveluViewModel>> HaePalvelutVarauksille(int alueId, DateTime startDate, DateTime endDate)
        {
            List<PalveluViewModel> palvelut = new List<PalveluViewModel>();
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT p.nimi, COUNT(*) AS Lkm
                FROM varauksen_palvelut vp
                JOIN varaus v ON vp.varaus_id = v.varaus_id
                JOIN mokki m ON v.mokki_mokki_id = m.mokki_id
                JOIN palvelu p ON vp.palvelu_id = p.palvelu_id
                WHERE m.alue_id = @AlueId
                AND v.varattu_loppupvm >= @StartDate
                AND v.varattu_alkupvm <= @EndDate
                GROUP BY p.nimi;";

                
                
                
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AlueId", alueId);
                    command.Parameters.AddWithValue("@StartDate", startDate);
                    command.Parameters.AddWithValue("@EndDate", endDate);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            palvelut.Add(new PalveluViewModel
                            {
                                //VarausId = reader.GetInt32("varaus_id"),
                                PalvelunNimi = reader.GetString("nimi"),
                                Lkm = reader.GetInt32("Lkm")
                            });
                        }
                    }
                }
                return palvelut;
            }
        }
    }
}




