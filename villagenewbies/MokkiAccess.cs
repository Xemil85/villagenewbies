using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace VillageNewbies
{
    public class MokkiAccess
    {
        private const string ConnectionString = "server=localhost;port=3306;database=vn;user=root;password="; //hox. Tuohon salasana 

        public async Task<List<Mokki>> FetchAllMokitAsync()
        {
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
                            mokkinimi = reader.GetString("mokkinimi"),
                            kuvaus = reader.GetString("kuvaus"),
                            varustelu = reader.GetString("varustelu")
                        };

                        mokit.Add(mokki);
                    }
                }

                return mokit;
            }
        }
    }
}
