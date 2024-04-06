using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Maui.Controls;
using System.Diagnostics;

namespace VillageNewbies
{
    public class MokkiAccess
    {
        public async Task<List<Mokki>> FetchAllMokitAsync()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            var mokit = new List<Mokki>();

            using(var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT * FROM mokki;", connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mokki = new Mokki
                        {
                            mokki_id = reader.GetInt32("mokki_id"),
                            alue_id = reader.GetInt32("alue_id"),
                            mokkinimi = reader.GetString("mokkinimi"),
                            katuosoite = reader.GetString("katuosoite"),
                            postinro = reader.GetInt32("postinro"),
                            henkilomaara = reader.GetInt32("henkilomaara"),
                            hinta = reader.GetDouble("hinta"),
                            kuvaus = reader.GetString("kuvaus"),
                            varustelu = reader.GetString("varustelu")
                        };

                        mokit.Add(mokki);
                    }
                }

                return mokit;
            }
        }

        public async Task<List<Palvelu>> FetchAllPalveluAsync()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            var palvelut = new List<Palvelu>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("select palvelu_id, palvelu.alue_id, palvelu.nimi, alue.nimi as sijainti, tyyppi, kuvaus, hinta, alv from palvelu inner join alue on palvelu.alue_id = alue.alue_id", connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var palvelu = new Palvelu
                        {
                            palvelu_id = reader.GetInt32("palvelu_id"),
                            alue_id = reader.GetInt32("alue_id"),
                            nimi = reader.GetString("nimi"),
                            sijainti = reader.GetString("sijainti"),
                            tyyppi = reader.GetInt32("tyyppi"),
                            kuvaus = reader.GetString("kuvaus"),
                            hinta = reader.GetDouble("hinta"),
                            alv = reader.GetDouble("alv"),
                        };

                        palvelut.Add(palvelu);
                    }
                }

                return palvelut;
            }
        }

        public async Task<List<Asiakas>> FetchAllAsiakasAsync()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            var asiakkaat = new List<Asiakas>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT * FROM asiakas;", connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var asiakas = new Asiakas
                        {
                            asiakas_id = reader.GetInt32("asiakas_id"),
                            postinro = reader.GetString("postinro"),
                            etunimi = reader.GetString("etunimi"),
                            sukunimi = reader.GetString("sukunimi"),
                            lahiosoite = reader.GetString("lahiosoite"),
                            email = reader.GetString("email"),
                            puhelinnro = reader.GetString("puhelinnro"),
                        };

                        asiakkaat.Add(asiakas);
                    }
                }

                return asiakkaat;
            }
        }

        public async Task<List<Alue>> FetchAllAlueAsync()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            var alueet = new List<Alue>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                using (var command = new MySqlCommand("SELECT * FROM alue;", connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var alue = new Alue
                        {
                            alue_id = reader.GetInt32("alue_id"),
                            nimi = reader.GetString("nimi"),    
                        };

                        alueet.Add(alue);
                    }
                }

                return alueet;
            }
        }








    }
}
