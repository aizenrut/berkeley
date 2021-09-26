// Alunos: Bruno Ricardo Junkes, Igor Christofer Eisenhut e Manoella Marcondes Junkes

using Berkeley.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Berkeley.Master
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var endpoint = Utils.ObterEndpoint(Constantes.PORTA_MASTER);
            var master = new UdpClient(endpoint);
            var token = new CancellationTokenSource();

            var slaves = new List<IPEndPoint>();

            foreach (var porta in args)
            {
                slaves.Add(Utils.ObterEndpoint(porta));
            }

            var horarioMaster = Utils.GerarHorarioAleatorio();
            var horasPorSlave = new List<(IPEndPoint, DateTime)>();

            Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    Console.WriteLine($"Horario: {horarioMaster:G}");

                    var bytesDataHora = Encoding.UTF8.GetBytes(Constantes.DATA_HORA);

                    horasPorSlave.Clear();

                    foreach (var slave in slaves)
                    {
                        master.Send(bytesDataHora, bytesDataHora.Length, slave);

                        var copy = slave;
                        var responseBytes = master.Receive(ref copy);

                        var response = Encoding.UTF8.GetString(responseBytes);
                        var contents = response.Split(' ');

                        if (contents[0] == Constantes.DATA_HORA && DateTime.TryParse(contents[1], out var slaveDateTime))
                        {
                            Console.WriteLine($"Recebido o horário de {slave}: {slaveDateTime:G}");

                            horasPorSlave.Add((slave, slaveDateTime));
                        }
                    }

                    var horarios = horasPorSlave
                        .Select(x => x.Item2)
                        .Append(horarioMaster);

                    var media = (long)horarios.Average(x => x.Ticks);
                    var horarioMedio = new DateTime(media);

                    Console.WriteLine($"Horário médio calculado: {horarioMedio:G}");


                    if (horarioMaster.CompareTo(horarioMedio) == 0)
                    {
                        Console.WriteLine("Master está com o horário correto");
                    }
                    else
                    {
                        var diferencaMaster = horarioMaster.Ticks - media;

                        horarioMaster = horarioMaster.AddTicks(-diferencaMaster);

                        Console.WriteLine($"Horário ajustado em {-diferencaMaster} ticks. Novo: {horarioMaster:G}");
                    }

                    foreach (var grupo in horasPorSlave)
                    {
                        if (grupo.Item2.CompareTo(horarioMedio) == 0)
                        {
                            Console.WriteLine($"{grupo.Item1} está com o horário correto");
                            continue;
                        }

                        var diferencaSlave = grupo.Item2.Ticks - media;

                        var mensagem = $"{Constantes.AJUSTE} {-diferencaSlave}";
                        var bytesAjuste = Encoding.UTF8.GetBytes(mensagem);

                        Console.WriteLine($"Enviando solicitação de ajuste em {-diferencaSlave} ticks para {grupo.Item1}");

                        master.Send(bytesAjuste, bytesAjuste.Length, grupo.Item1);
                    }

                    Console.WriteLine();

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            });

            Console.WriteLine($"Master executando em {endpoint.Address}:{endpoint.Port}");
            Console.WriteLine();
            Console.ReadLine();

            token.Cancel();
        }
    }
}
