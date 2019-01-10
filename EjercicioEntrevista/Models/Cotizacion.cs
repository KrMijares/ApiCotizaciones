using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EjercicioEntrevista.Models
{
    public class Cotizacion
    {
        [Key]
        public string Moneda { get; set; }
        public double Precio { get; set; }
    }
}
