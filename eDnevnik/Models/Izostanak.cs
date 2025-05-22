using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using eDnevnik.Data.@enum;

namespace eDnevnik.Models
{
    public class Izostanak
     {
        [Key]
        public int Id { get; set; }
        public string? Komentar { get; set; }

        public StatusIzostanka status { get; set; } = StatusIzostanka.Neopravdan;
        [ForeignKey("Korisnik")]
        public string UcenikId { set; get; }
        public Korisnik Ucenik { get; set; }
        [ForeignKey("Cas")]
        public int CasId { set; get; }
        public Cas Cas { get; set; }

        public Izostanak() { }

    }
}
