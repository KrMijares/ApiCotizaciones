using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using EjercicioEntrevista.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace EjercicioEntrevista.Controllers
{

    [Produces("application/json")]
    [Route("[controller]")]
    public class CotizacionController : Controller
    {

        private readonly ApplicationDbContext context;

        public CotizacionController(ApplicationDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public IEnumerable<Cotizacion> Get()
        {
            return context.Cotizaciones.ToList();
        }

        // Para ejecutar el get se debe correr el nombre del controlador mas el nombre a buscar /cotizacion/dolar
        [HttpGet("{nombre}")]
        public IActionResult GetByMoneda(string nombre)
        {
            
            //Como estamos trabajando con una tabla generica la misma se refresca cada vez que se compila el codigo.
            if (!context.Cotizaciones.Any())
            {
                context.Cotizaciones.AddRange(new List<Cotizacion>()
                {
                    new Cotizacion(){Moneda = "dolar", Precio = calcularCotizacion("dolar")},
                    new Cotizacion(){Moneda = "euro", Precio = calcularCotizacion("euro")},
                    new Cotizacion(){Moneda = "real", Precio = calcularCotizacion("real")}
                });
                context.SaveChanges();
            }
            else
            {
                var result = context.Cotizaciones.SingleOrDefault(x => x.Moneda == nombre);
                if (result != null)
                {
                    result.Precio = calcularCotizacion(nombre);
                    context.SaveChanges();
                }
            }

            //Se realiza la busqueda de la cotización que queremos mostrar
            var cotizacion = context.Cotizaciones.FirstOrDefault(x => x.Moneda == nombre);

            //Si no conseguimos ninguna cotización enviamos un mensaje de respuesta NotFound
            if (cotizacion == null)
            {
                return NotFound();
            }

            //Si conseguimos cotización enviamos un mensaje de respuesta OK que incluye la cotización
            return Ok(cotizacion);
        }

        //Metodo utilizado para consultar la API del cambio
        public float calcularCotizacion(string moneda)
        {
            string URL = "";
            if(moneda == "dolar")
            {
                URL = "https://api.cambio.today/v1/quotes/USD/ARS/json?quantity=1&key=1617|RCv_zF8bHMB5MHLKZEh^8hbMTpqL8M0C";
            } else if (moneda == "euro")
            {
                URL = "https://api.cambio.today/v1/quotes/EUR/ARS/json?quantity=1&key=1617|RCv_zF8bHMB5MHLKZEh^8hbMTpqL8M0C";
            } else if (moneda == "real")
            {
                URL = "https://api.cambio.today/v1/quotes/BRL/ARS/json?quantity=1&key=1617|RCv_zF8bHMB5MHLKZEh^8hbMTpqL8M0C";
            }
            //Pasamos la URL al request, luego declaramos los headers y el tipo de petición HTTTP
            var request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;

            var response = (HttpWebResponse)request.GetResponse();
            var rawJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
            var json = JObject.Parse(rawJson); //Convertimos la respuesta en un objecto JSON
            var result = json["result"]["value"].ToObject<float>(); //Obtenemos el valor y lo convertimos de un JToken a un float
            return result;
        }
    }
}