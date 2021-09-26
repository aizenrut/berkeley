using Berkeley.Common;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Berkeley.Slave
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var endpoint = Utils.ObterEndpoint(args[0]);
            var masterEndpoint = Utils.ObterEndpoint(Constantes.PORTA_MASTER);

            var slave = new UdpClient(endpoint);
            var token = new CancellationTokenSource();

            var horarioSlave = Utils.GerarHorarioAleatorio();

            Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    var requestBytes = slave.Receive(ref masterEndpoint);

                    var request = Encoding.UTF8.GetString(requestBytes);
                    var contents = request.Split(' ');

                    if (contents[0] == Constantes.DATA_HORA)
                    {
                        var response = $"{Constantes.DATA_HORA} {horarioSlave:s}";
                        var responseBytes = Encoding.UTF8.GetBytes(response);

                        Console.WriteLine($"Respondendo solicitação de horário. Atual: {horarioSlave:G}");

                        slave.Send(responseBytes, responseBytes.Length, masterEndpoint);
                    }
                    else if (contents[0] == Constantes.AJUSTE && long.TryParse(contents[1], out var ajuste))
                    {
                        horarioSlave = horarioSlave.AddTicks(ajuste);
                        Console.WriteLine($"Horário ajustado em {ajuste} ticks. Novo: {horarioSlave:G}");
                    }
                }
            });

            Console.WriteLine($"Slave executando em {endpoint.Address}:{endpoint.Port}");
            Console.WriteLine();
            Console.ReadLine();

            token.Cancel();
        }
    }
}
