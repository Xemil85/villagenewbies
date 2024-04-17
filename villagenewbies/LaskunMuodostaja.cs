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

            // Tallenna laskun tiedot ensin tietokantaan ilman PDF-tiedostoa
            int laskuId = await _laskuAccess.TallennaLasku(lasku);

            // Luo PDF lasku iText7-kirjastolla muistivirrassa
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(memoryStream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);                         
              
                // Lisää sisältöä PDF-dokumenttiin
                document.Add(new Paragraph($"Lasku #{laskuId}"));
                document.Add(new Paragraph($"Asiakkaan nimi: {asiakas.etunimi} {asiakas.sukunimi}"));
                document.Add(new Paragraph($"Mökin nimi: {mokki.mokkinimi}"));
                document.Add(new Paragraph($"Varausaika: {varaus.varattu_alkupvm} - {varaus.varattu_loppupvm}"));
                document.Add(new Paragraph($"Kokonaishinta: {lasku.Summa}€"));
                document.Add(new Paragraph($"ALV: {lasku.Alv}€"));

                foreach (Palvelu palvelu in palvelut)
                {
                    document.Add(new Paragraph($"{palvelu.nimi}: {palvelu.hinta}€ (ALV sisältyy hintaan)"));
                }
                // Suljetaan dokumentti
                document.Close();

                // Tallennetaan laskun PDF-tiedoston sisältö tietokantaan
                byte[] pdfContent = memoryStream.ToArray();

            }
        }
    

        private double LaskeKokonaishinta(Varaus varaus, Mokki mokki, List<Palvelu> palvelut)
        {
            DateTime alkupvm = DateTime.Parse(varaus.varattu_alkupvm);
            DateTime loppupvm = DateTime.Parse(varaus.varattu_loppupvm);
            TimeSpan varauksenKesto = loppupvm - alkupvm;
            int varauksenPaivat = varauksenKesto.Days;

            // Mökin hinta per päivä kerrottuna varauksen päivien määrällä
            double mokinHinta = varauksenPaivat * mokki.hinta;

            // Palveluiden hinnat (oletetaan, että hinta on kokonaishinta koko varauksen ajalle ja sisältää ALV:n)
            double palveluidenHinta = palvelut.Sum(p => p.hinta);

            // Kokonaishinta on mökin hinta plus palveluiden hinnat
            return mokinHinta + palveluidenHinta;
        }
    }
}
