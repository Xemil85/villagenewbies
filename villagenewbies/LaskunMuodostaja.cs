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
            // Hae mökin tiedot
            Mokki mokki = await _mokkiAccess.FetchMokkiByIdAsync(varaus.mokki_id);

            // Hae asiakkaan tiedot
            Asiakas asiakas = await _mokkiAccess.FetchAsiakasByIdAsync(varaus.asiakas_id);

            // Laske kokonaishinta
            double kokonaishinta = LaskeKokonaishinta(varaus, mokki, palvelut);

            // Luodaan lasku-olio
            Lasku lasku = new Lasku
            {
                VarausId = varaus.varaus_id,
                Summa = kokonaishinta,
                Alv = 0.24 * kokonaishinta,
                Maksettu = false
            };

            // Tallenna lasku tietokantaan ja hanki laskuId
            int laskuId = await _laskuAccess.TallennaLaskuIlmanPdf(lasku);

            // Luo PDF-tiedoston nimi, joka sisältää laskun ID:n
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string pdfFileName = Path.Combine(desktopPath, $"Lasku_{laskuId}.pdf");

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
                document.Add(new Paragraph($"Varausaika: {varaus.varattu_alkupvm} - {varaus.varattu_loppupvm}"));
                document.Add(new Paragraph($"ALV: {lasku.Alv}€"));
                foreach (Palvelu palvelu in palvelut)
                {
                    document.Add(new Paragraph($"{palvelu.nimi}: {palvelu.hinta}€ (ALV sisältyy hintaan)"));
                }
                document.Add(new Paragraph("\nMaksettava summa: " + String.Format("{0:0.00} €", lasku.Summa + lasku.Alv)));
                document.Add(new Paragraph("Eräpäivä: " + DateTime.Now.AddDays(14).ToString("dd.MM.yyyy")));
                document.Add(new Paragraph("Tilinumero: FI12 3456 7890 1234 56"));
                document.Add(new Paragraph("Viitenumero: 1234567"));
                
                // Suljetaan dokumentti
                document.Close();
            }
        }



        private double LaskeKokonaishinta(Varaus varaus, Mokki mokki, List<Palvelu> palvelut)
        {
            DateTime alkupvm = DateTime.Parse(varaus.varattu_alkupvm.ToString());
            DateTime loppupvm = DateTime.Parse(varaus.varattu_loppupvm.ToString());
            TimeSpan varauksenKesto = loppupvm - alkupvm;
            int varauksenPaivat = varauksenKesto.Days;

            // Jos varaus kestää vähemmän kuin yhden vuorokauden, käsittele se yhden vuorokauden mittaisena.
            if (varauksenPaivat == 0)
            {
                varauksenPaivat = 1;
            }

            double mokinHinta = varauksenPaivat * mokki.hinta;
            double palveluidenHinta = palvelut.Sum(p => p.hinta);

            return mokinHinta + palveluidenHinta;
        }
    }
}
