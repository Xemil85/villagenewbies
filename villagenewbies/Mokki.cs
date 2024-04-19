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
        public string sijainti { get; set; }
        public int postinro { get; set; }
        public string mokkinimi { get; set; }
        public string katuosoite { get; set; }
        public double hinta { get; set; }
        public string kuvaus { get; set; }
        public string varustelu { get; set; }
        public int henkilomaara { get; set; }
    }
    public class Alue
    {
        public string nimi { get; set; }
        public int alue_id { get; set; }
    }

    public class Palvelu
    {
        public int palvelu_id { get; set; }
        public int alue_id { get; set; }
        public string nimi { get; set; }
        public string sijainti { get; set; }
        public int tyyppi { get; set; } 
        public string kuvaus { get; set; }
        public double hinta { get; set; }
        public double alv { get; set; }
    }

    public class PalveluTyyppi
    {
        public int tyyppi { get; set; }
        public string nimi { get; set; }
    }

    public class Asiakas
    {
        public int asiakas_id { get; set; }
        public string postinro { get; set; }
        public string toimipaikka { get; set; }
        public string etunimi { get; set; }
        public string sukunimi { get; set; }
        public string lahiosoite { get; set; }
        public string email { get; set; }
        public string puhelinnro { get; set; }
    }

    public class Varaus
    {
        public int varaus_id { get; set; }
        public int asiakas_id { get; set; }
        public string asiakkaannimi { get; set; }
        public int mokki_id { get; set; }
        public string mokkinimi { get; set; }
        public int maara { get; set; }
        public DateTime varattu_pvm { get; set; }
        public DateTime vahvistus_pvm { get; set; }
        public DateTime varattu_alkupvm { get; set; }
        public DateTime varattu_loppupvm { get; set; }
        public int peruutettu { get; set; }
    }

    public class Varauksen_Palvelut
    {
        public int varaus_id { get; set; }
        public int palvelu_id { get; set; }
        public int lkm { get; set; }
    }

    public class Lasku
    {
        public int LaskuId { get; set; }
        public int VarausId { get; set; }
        public double Summa { get; set; }
        public double Alv { get; set; }
        public bool Maksettu { get; set; }
    }

    public class VarausViewModel
    {
        public Varaus Varaus { get; set; }
        public string Mokkinimi { get; set; }
    }

}