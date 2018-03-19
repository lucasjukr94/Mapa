using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapa
{
    public class EnderecoVm
    {
        public string CEP { get; set; }
        public string CEPComTraco { get; set; }
        public string Estado { get; set; }
        public string Endereco { get; set; }
        public string Cidade { get; set; }
        public string Bairro { get; set; }
        public double lati { get; set; }
        public double longi { get; set; }
    }
}
