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

        public async Task<Mokki> FetchMokkiByIdAsync(int mokkiId)
        {
            var kaikkiMokit = await FetchAllMokitAsync();
            var mokki = kaikkiMokit.FirstOrDefault(m => m.mokki_id == mokkiId);
            if (mokki != null)
            {
                return mokki;
            }
            else
            {
                // Heitä poikkeus tai käsittele tilanne, jos mökkiä ei löydy
                throw new Exception("Mökkiä ei löytynyt annetulla ID:llä.");
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

        public async Task<List<Palvelu>> FetchAllPalveluWithAlueAsync(int alueid)
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

                using (var command = new MySqlCommand($"select palvelu_id, palvelu.alue_id, palvelu.nimi, alue.nimi as sijainti, tyyppi, kuvaus, hinta, alv from palvelu inner join alue on palvelu.alue_id = alue.alue_id where palvelu.alue_id = {alueid}", connection))

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

        public async Task<List<Palvelu>> FetchPalvelutByVarausIdAsync(int varausId)
        {
            var palvelut = new List<Palvelu>();

            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = @"
            SELECT palvelu.*
            FROM palvelu
            INNER JOIN varauksen_palvelut ON palvelu.palvelu_id = varauksen_palvelut.palvelu_id
            WHERE varauksen_palvelut.varaus_id = @VarausId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VarausId", varausId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var palvelu = new Palvelu
                            {
                                palvelu_id = reader.GetInt32("palvelu_id"),
                                alue_id = reader.GetInt32("alue_id"),
                                nimi = reader.GetString("nimi"),
                                tyyppi = reader.GetInt32("tyyppi"),
                                kuvaus = reader.GetString("kuvaus"),
                                hinta = reader.GetDouble("hinta"),
                                alv = reader.GetDouble("alv"),
                            };
                            palvelut.Add(palvelu);
                        }
                    }
                }
            }
            return palvelut;
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

        public async Task<Asiakas> FetchAsiakasByIdAsync(int asiakasId)
        {
            // Kutsu olemassaolevaa metodia, joka palauttaa kaikki asiakkaat
            var kaikkiAsiakkaat = await FetchAllAsiakasAsync();
            // Etsi ja palauta oikea asiakas listasta
            return kaikkiAsiakkaat.FirstOrDefault(a => a.asiakas_id == asiakasId);
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


        public async Task<List<Varaus>> FetchAllVarausAsync()
        {
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));

            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            var varaukset = new List<Varaus>();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = @"SELECT
                                    varaus.varaus_id,
                                    varaus.asiakas_id,
                                    concat(asiakas.etunimi, ' ', asiakas.sukunimi) AS nimi,
                                    mokki_id,-- Tässä liitämme asiakkaan nimen
                                    mokki.mokkinimi AS mokkinimi, -- Tässä liitämme mökin nimen
                                    COALESCE(varauksen_palvelut.lkm, 0) AS maara,
                                    varaus.varattu_pvm,
                                    varaus.vahvistus_pvm,
                                    varaus.varattu_alkupvm,
                                    varaus.varattu_loppupvm,
                                    varaus.peruutettu
                                FROM varaus
                                LEFT JOIN varauksen_palvelut ON varaus.varaus_id = varauksen_palvelut.varaus_id
                                LEFT JOIN asiakas ON varaus.asiakas_id = asiakas.asiakas_id
                                LEFT JOIN mokki ON varaus.mokki_mokki_id = mokki.mokki_id
                                WHERE peruutettu = 0
                                GROUP BY
                                    varaus.varaus_id,
                                    nimi,
                                    mokki.mokkinimi,
                                    COALESCE(varauksen_palvelut.lkm, 0),
                                    varaus.varattu_pvm,
                                    varaus.vahvistus_pvm,
                                    varaus.varattu_alkupvm,
                                    varaus.varattu_loppupvm,
                                    varaus.peruutettu;";

                using (var command = new MySqlCommand(query, connection))

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var varaus = new Varaus
                        {
                            varaus_id = reader.GetInt32("varaus_id"),
                            //asiakas_id = reader.GetInt32("asiakas_id"),
                            asiakkaannimi = reader.GetString("nimi"),
                            //mokki_id = reader.GetInt32("mokki_mokki_id"),
                            mokkinimi = reader.GetString("mokkinimi"),
                            maara = reader.GetInt32("maara"),
                            varattu_pvm = reader.GetDateTime("varattu_pvm"),
                            vahvistus_pvm = reader.GetDateTime("vahvistus_pvm"),
                            varattu_alkupvm = reader.GetDateTime("varattu_alkupvm"),
                            varattu_loppupvm = reader.GetDateTime("varattu_loppupvm"),
                            peruutettu = reader.GetInt32("peruutettu"),
                        };

                        varaukset.Add(varaus);
                    }
                }

                return varaukset;
            }
        }

        public async Task<Varaus> FetchVarausByIdAsync(int varausId)
        {
            Varaus varaus = null;
            string projectDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = Path.GetFullPath(Path.Combine(projectDirectory, @"..\..\..\..\..\"));
            DotNetEnv.Env.Load(projectRoot);
            var env = Environment.GetEnvironmentVariables();

            string ConnectionString = $"server={env["SERVER"]};port={env["SERVER_PORT"]};database={env["SERVER_DATABASE"]};user={env["SERVER_USER"]};password={env["SERVER_PASSWORD"]}";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT * FROM varaus WHERE varaus_id = @VarausId;";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VarausId", varausId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            varaus = new Varaus
                            {
                                varaus_id = reader.GetInt32("varaus_id"),
                                asiakas_id = reader.GetInt32("asiakas_id"),
                                mokki_id = reader.GetInt32("mokki_mokki_id"),
                                varattu_pvm = reader.GetDateTime("varattu_pvm"),
                                vahvistus_pvm = reader.GetDateTime("vahvistus_pvm"),
                                varattu_alkupvm = reader.GetDateTime("varattu_alkupvm"),
                                varattu_loppupvm = reader.GetDateTime("varattu_loppupvm"),
                                peruutettu = reader.GetInt32("peruutettu"),
                            };
                        }
                    }
                }
            }

            return varaus;
        }
    }
}
