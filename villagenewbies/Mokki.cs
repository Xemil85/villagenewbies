using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VillageNewbies
{
    public class Mokki
    {
        public int mokki_id { get; set; }
        public int alue_id { get; set; }
        public int postinro { get; set; }
        public string mokkinimi { get; set; }
        public string katuosoite { get; set; }
        public double hinta { get; set; }
        public string kuvaus { get; set; }
        public string varustelu { get; set; }
    }

    public class Palvelu
    {
        public int palvelu_id { get; set; }
        public int alue_id { get; set; }
        public string nimi { get; set; }
        public int tyyppi { get; set; } // Saattaa olla mahdollisesti parempi käyttää enum?
        public string kuvaus { get; set; }
        public double hinta { get; set; }
        public double alv { get; set; }
    }

    public class Asiakas
    {
        public int asiakas_id { get; set; }
        public string postinro { get; set; }
        public string etunimi { get; set; }
        public string sukunimi { get; set; }
        public string lahiosoite { get; set; }
        public string email { get; set; }
        public string puhelinnro { get; set; }
    }
}
