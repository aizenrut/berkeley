// Alunos: Bruno Ricardo Junkes, Igor Christofer Eisenhut e Manoella Marcondes Junkes

using System;
using System.Net;

namespace Berkeley.Common
{
    public static class Utils
    {
        public static IPEndPoint ObterEndpoint(string porta)
        {
            if (string.IsNullOrWhiteSpace(porta))
                throw new ArgumentException("Informe a porta");

            if (!int.TryParse(porta, out var portaInt))
                throw new ArgumentException($"Porta inválida");

            return new IPEndPoint(IPAddress.Parse(Constantes.IP_LOCALHOST), portaInt);
        }

        public static DateTime GerarHorarioAleatorio()
        {
            var numeroAleatorio = new Random().Next(13);
            var negativar = new Random().Next(2);

            if (negativar == 1)
            {
                numeroAleatorio = -numeroAleatorio;
            }

            return DateTime.Now.AddHours(numeroAleatorio);
        }
    }
}
