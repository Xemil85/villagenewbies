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

        public LaskunMuodostaja()
        {
            _laskuAccess = new LaskuAccess();
            // Oletetaan, että LaskuAccess-luokassa on metodi tallentamaan lasku tietokantaan.
        }

        public async Task LuoJaTallennaLaskuPdf(Varaus varaus, List<Palvelu> palvelut, string filePath)
        {
            // Tähän tulee logiikka laskun kokonaishinnan laskemiseksi
            double kokonaishinta = LaskeKokonaishinta(varaus, palvelut);

            // Luodaan lasku-olio
            Lasku lasku = new Lasku
            {
                VarausId = varaus.varaus_id,
                Summa = kokonaishinta,
                Alv = 0.24 * kokonaishinta, // Oletetaan että ALV on 24%
                Maksettu = false
            };

            // Tallennetaan laskun tiedot tietokantaan
            await _laskuAccess.TallennaLasku(lasku);

            // Tässä luodaan itse PDF
            PdfWriter writer = new PdfWriter(filePath);
            PdfDocument pdf = new PdfDocument(writer);
            Document document = new Document(pdf);

            // Lisää sisältöä PDF-dokumenttiin
            document.Add(new Paragraph($"Lasku #{lasku.LaskuId}"));
            // Lisää muita laskun tietoja...

            // Suljetaan dokumentti
            document.Close();

            // Tässä kohtaa voit esimerkiksi palauttaa polun luotuun PDF-tiedostoon tai tallentaa sen
            // tietokantaan, jos se on tarpeen.
        }

        private double LaskeKokonaishinta(Varaus varaus, List<Palvelu> palvelut)
        {
            // Tähän tulee logiikka laskun kokonaishinnan laskemiseksi
            // Huomioi, että tämä metodi on pseudokoodia ja sinun tulee korvata se
            // sovelluksesi todellisen hinnanlaskentalogiikan mukaan.
            return 0.0;
        }
    }
}
