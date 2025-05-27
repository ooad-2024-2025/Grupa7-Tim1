
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eDnevnik.Models { 
public class Razred
{
    [Key]
    public int Id { get; set; }

    public string Naziv { get; set; }


        [ForeignKey("Korisnik")]
        public string NastavnikId { get; set; }
    public Korisnik Nastavnik { get; set; }

   
       
} }
