using ClosedXML.Excel;
using eDnevnik.Data;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eDnevnik.Services
{
    public class ExcelReportService
    {
        private readonly ApplicationDbContext _context;

        public ExcelReportService(ApplicationDbContext context)
        {
            _context = context;
        }

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
    }
}