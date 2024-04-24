using System;
using System.IO;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;


namespace VillageNewbies
{
    public class LaskunMuodostaja
    {
        private readonly LaskuAccess _laskuAccess;
        private readonly MokkiAccess _mokkiAccess;

        public LaskunMuodostaja()
        {
            _laskuAccess = new LaskuAccess();
            _mokkiAccess = new MokkiAccess();
        }

        public async Task LuoJaTallennaLaskuPdf(Varaus varaus, List<Palvelu> palvelut)
        {
            // Hae mökin ja asiakkaan tiedot
            Mokki mokki = await _mokkiAccess.FetchMokkiByIdAsync(varaus.mokki_id);
            Asiakas asiakas = await _mokkiAccess.FetchAsiakasByIdAsync(varaus.asiakas_id);

            // Laske kokonaishinta
            double kokonaishinta = LaskeKokonaishinta(varaus, mokki, palvelut);

            // Laske ALV (24% kokonaishinnasta)
            double alv = 0.24 * kokonaishinta;

            // Luodaan lasku-olio ilman ALV:n lisäämistä uudelleen
            Lasku lasku = new Lasku
            {
                VarausId = varaus.varaus_id,
                Summa = kokonaishinta, // Tämä on hinta, johon sisältyy ALV
                Alv = alv,
                Maksettu = false
            };


            // Tallenna lasku tietokantaan ja hanki laskuId
            int laskuId = await _laskuAccess.TallennaLaskuIlmanPdf(lasku);
            
            // Luo PDF-tiedoston nimi, joka sisältää laskun ID:n
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop); // Polku työpöydälle
            string pdfFileName = Path.Combine(desktopPath, $"Lasku_{laskuId}.pdf"); // PDF-tiedoston nimi

            // Luo PDF lasku iText7-kirjastolla ja tallenna se paikallisesti
            using (FileStream fileStream = new FileStream(pdfFileName, FileMode.Create, FileAccess.Write))
            {
                PdfWriter writer = new PdfWriter(fileStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Lisää sisältöä PDF-dokumenttiin
                document.Add(new Paragraph($"Village Newbies lasku #{laskuId}"));
                document.Add(new Paragraph($"Asiakkaan nimi: {asiakas.etunimi} {asiakas.sukunimi}"));
                document.Add(new Paragraph($"Mökin nimi: {mokki.mokkinimi}"));
                document.Add(new Paragraph($"Mökin hinta per vuorokausi: {mokki.hinta}€"));
                document.Add(new Paragraph($"Varausaika: {varaus.varattu_alkupvm} - {varaus.varattu_loppupvm}"));
                document.Add(new Paragraph($"ALV: {alv.ToString("F2")}€ (ALV sisältyy hintaan)")); // Näytä ALV

                foreach (Palvelu palvelu in palvelut)
                {
                    document.Add(new Paragraph($"{palvelu.nimi}: {palvelu.hinta}€ (ALV sisältyy hintaan)"));
                }

                // Maksettava summa ilman ALV:n lisäystä
                document.Add(new Paragraph("\nMaksettava summa: " + String.Format("{0:0.00} €", kokonaishinta))); // Käytä kokonaishintaa ilman ALV:n uudelleenlisäystä

                // Eräpäivä 14 päivää varauksen loppumispäivämäärästä
                DateTime erapaiva = varaus.varattu_loppupvm.AddDays(14);
                document.Add(new Paragraph("Eräpäivä: " + erapaiva.ToString("dd.MM.yyyy")));

                document.Add(new Paragraph("Tilinumero: FI12 3456 7890 1234 56"));
                document.Add(new Paragraph("Viitenumero: 1234567"));

                // Suljetaan dokumentti
                document.Close();
            }
        }


        private double LaskeKokonaishinta(Varaus varaus, Mokki mokki, List<Palvelu> palvelut)
        {
            // Aloitus- ja lopetuspäivämäärät
            DateTime alkupvm = varaus.varattu_alkupvm;
            DateTime loppupvm = varaus.varattu_loppupvm;

            // Laske päivien määrä aloituksesta lopetukseen mukaan lukien
            int varauksenPaivat = (loppupvm.Date - alkupvm.Date).Days + 1; 

            // Mökin kokonaishinta
            double mokinHinta = varauksenPaivat * mokki.hinta;

            // Palveluiden kokonaishinta
            double palveluidenHinta = palvelut.Sum(p => p.hinta);

            return mokinHinta + palveluidenHinta; // Kokonaishinta
        }

    }
}
