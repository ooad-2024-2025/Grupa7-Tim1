using ClosedXML.Excel;
using eDnevnik.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace eDnevnik.Services
{
    public class ExcelReportService
    {
        private readonly ApplicationDbContext _context;

        public ExcelReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        // POSTOJEĆA METODA - ostavi kako jeste
        public async Task<byte[]> GenerateReportAsync(string korisnikId)
        {
            var ucenik = await _context.Users
                .Include(u => u.Razred)
                .FirstOrDefaultAsync(u => u.Id == korisnikId || u.RoditeljId == korisnikId);

            if (ucenik == null)
                throw new System.Exception("Učenik nije pronađen.");

            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == ucenik.Id)
                .ToListAsync();

            var izostanci = await _context.Izostanak
                .Where(i => i.UcenikId == ucenik.Id)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Izvještaj");

            ws.Cell(1, 1).Value = $"Učenik: {ucenik.Ime} {ucenik.Prezime}";
            ws.Cell(2, 1).Value = $"Razred: {ucenik.Razred?.Naziv}";
            ws.Cell(3, 1).Value = $"Vladanje: {ucenik.Vladanje}";

            ws.Cell(5, 1).Value = "Ocjene";
            ws.Cell(6, 1).Value = "Predmet";
            ws.Cell(6, 2).Value = "Vrijednost";
            ws.Cell(6, 3).Value = "Komentar";
            ws.Cell(6, 4).Value = "Datum";

            int row = 7;
            foreach (var o in ocjene)
            {
                ws.Cell(row, 1).Value = o.Predmet.Naziv;
                ws.Cell(row, 2).Value = o.Vrijednost;
                ws.Cell(row, 3).Value = o.Komentar;
                ws.Cell(row, 4).Value = o.Datum.ToShortDateString();
                row++;
            }

            row++;
            ws.Cell(row++, 1).Value = $"Broj izostanaka: {izostanci.Count}";

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // NOVA METODA ZA PDF
        public async Task<byte[]> GeneratePdfReportAsync(string korisnikId)
        {
            var ucenik = await _context.Users
                .Include(u => u.Razred)
                .FirstOrDefaultAsync(u => u.Id == korisnikId);

            if (ucenik == null)
                throw new System.Exception("Učenik nije pronađen.");

            var ocjene = await _context.Ocjena
                .Include(o => o.Predmet)
                .Where(o => o.UcenikId == korisnikId)
                .OrderBy(o => o.Predmet.Naziv)
                .ThenBy(o => o.Datum)
                .ToListAsync();

            var izostanci = await _context.Izostanak
                .Include(i => i.Cas)
                .ThenInclude(c => c.Predmet)
                .Where(i => i.UcenikId == korisnikId)
                .OrderByDescending(i => i.Cas.Termin)
                .ToListAsync();

            return GeneratePDF(ucenik, ocjene, izostanci);
        }

        // NOVA METODA ZA GENERIRANJE PDF-a
        private byte[] GeneratePDF(eDnevnik.Models.Korisnik ucenik, List<eDnevnik.Models.Ocjena> ocjene, List<eDnevnik.Models.Izostanak> izostanci)
        {
            using var stream = new MemoryStream();
            var document = new Document(PageSize.A4, 25, 25, 30, 30);
            var writer = PdfWriter.GetInstance(document, stream);

            document.Open();

            // Font setup za Unicode
            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            var titleFont = new iTextSharp.text.Font(baseFont, 18, iTextSharp.text.Font.BOLD);
            var headerFont = new iTextSharp.text.Font(baseFont, 14, iTextSharp.text.Font.BOLD);
            var normalFont = new iTextSharp.text.Font(baseFont, 10, iTextSharp.text.Font.NORMAL);
            var smallFont = new iTextSharp.text.Font(baseFont, 8, iTextSharp.text.Font.NORMAL);

            // Naslov
            var title = new Paragraph($"ŠKOLSKI IZVJEŠTAJ", titleFont);
            title.Alignment = Element.ALIGN_CENTER;
            title.SpacingAfter = 20;
            document.Add(title);

            // Osnovne informacije
            var infoTable = new PdfPTable(2);
            infoTable.WidthPercentage = 100;
            infoTable.SpacingAfter = 20;

            AddTableRow(infoTable, "Ime i prezime:", ucenik.FullName, headerFont, normalFont);
            AddTableRow(infoTable, "Razred:", ucenik.Razred?.Naziv ?? "Nepoznat", headerFont, normalFont);
            AddTableRow(infoTable, "Vladanje:", GetVladanjeText(ucenik.Vladanje), headerFont, normalFont);
            AddTableRow(infoTable, "Datum izvještaja:", DateTime.Now.ToString("dd.MM.yyyy"), headerFont, normalFont);

            document.Add(infoTable);

            // Statistike
            var prosjek = ocjene.Any() ? ocjene.Average(o => o.Vrijednost) : 0;
            var statsTable = new PdfPTable(4);
            statsTable.WidthPercentage = 100;
            statsTable.SpacingAfter = 20;

            AddTableRow(statsTable, "Opći prosjek:", prosjek.ToString("F2"), headerFont, normalFont);
            AddTableRow(statsTable, "Broj ocjena:", ocjene.Count.ToString(), headerFont, normalFont);
            AddTableRow(statsTable, "Broj predmeta:", ocjene.Select(o => o.PredmetId).Distinct().Count().ToString(), headerFont, normalFont);
            AddTableRow(statsTable, "Broj izostanaka:", izostanci.Count.ToString(), headerFont, normalFont);

            document.Add(statsTable);

            // Ocjene po predmetima
            if (ocjene.Any())
            {
                var ocjeneHeader = new Paragraph("OCJENE PO PREDMETIMA", headerFont);
                ocjeneHeader.SpacingAfter = 10;
                document.Add(ocjeneHeader);

                var ocjenePoPremetima = ocjene.GroupBy(o => o.Predmet);

                foreach (var predmetGrupa in ocjenePoPremetima)
                {
                    var predmetTable = new PdfPTable(4);
                    predmetTable.WidthPercentage = 100;
                    predmetTable.SpacingAfter = 10;
                    predmetTable.SetWidths(new float[] { 3, 1, 2, 4 });

                    // Header
                    var predmetNaziv = predmetGrupa.Key?.Naziv ?? "Nepoznat predmet";
                    var predmetProsjek = predmetGrupa.Average(o => o.Vrijednost);
                    AddTableCell(predmetTable, $"{predmetNaziv} (Prosjek: {predmetProsjek:F2})", headerFont, true);
                    AddTableCell(predmetTable, "", headerFont, true);
                    AddTableCell(predmetTable, "", headerFont, true);
                    AddTableCell(predmetTable, "", headerFont, true);

                    AddTableCell(predmetTable, "Datum", normalFont, true);
                    AddTableCell(predmetTable, "Ocjena", normalFont, true);
                    AddTableCell(predmetTable, "Tip", normalFont, true);
                    AddTableCell(predmetTable, "Komentar", normalFont, true);

                    foreach (var ocjena in predmetGrupa.OrderBy(o => o.Datum))
                    {
                        AddTableCell(predmetTable, ocjena.Datum.ToString("dd.MM.yyyy"), smallFont);
                        AddTableCell(predmetTable, ocjena.Vrijednost.ToString(), smallFont);
                        AddTableCell(predmetTable, "Usmena", smallFont);
                        AddTableCell(predmetTable, ocjena.Komentar ?? "-", smallFont);
                    }

                    document.Add(predmetTable);
                }
            }

            // Footer
            var footer = new Paragraph($"Generirano: {DateTime.Now:dd.MM.yyyy u HH:mm} | eDnevnik sistem", smallFont);
            footer.Alignment = Element.ALIGN_CENTER;
            footer.SpacingBefore = 30;
            document.Add(footer);

            document.Close();
            return stream.ToArray();
        }

        // Helper metode za PDF
        private void AddTableRow(PdfPTable table, string label, string value, iTextSharp.text.Font labelFont, iTextSharp.text.Font valueFont)
        {
            var labelCell = new PdfPCell(new Phrase(label, labelFont));
            labelCell.Border = Rectangle.NO_BORDER;
            labelCell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(labelCell);

            var valueCell = new PdfPCell(new Phrase(value, valueFont));
            valueCell.Border = Rectangle.NO_BORDER;
            valueCell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(valueCell);
        }

        private void AddTableCell(PdfPTable table, string text, iTextSharp.text.Font font, bool isHeader = false)
        {
            var cell = new PdfPCell(new Phrase(text, font));
            if (isHeader)
            {
                cell.BackgroundColor = BaseColor.LightGray;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
            }
            table.AddCell(cell);
        }

        private string GetVladanjeText(eDnevnik.Data.@enum.StatusVladanja vladanje)
        {
            return vladanje switch
            {
                eDnevnik.Data.@enum.StatusVladanja.Primjereno => "Odlično",
                eDnevnik.Data.@enum.StatusVladanja.VrloDobro => "Vrlo dobro",
                eDnevnik.Data.@enum.StatusVladanja.Dobro => "Dobro",
                eDnevnik.Data.@enum.StatusVladanja.Zadovoljava => "Dovoljno",
                eDnevnik.Data.@enum.StatusVladanja.Neprimjereno => "Nedovoljno",
                _ => "Neocijenjeno"
            };
        }
    }
}