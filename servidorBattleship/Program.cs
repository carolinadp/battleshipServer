using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServidorBattleShip
{
    class Program
    {
        private static TcpListener escuchador = null;
        public static int[] ships = { 5, 4, 3, 3, 2 };
        public static int width = 10, height = 10;
        public static Socket[] sockets = null;
        public static NetworkStream []streams;
        public static StreamWriter [] writers;
        public static StreamReader[] readers;

        static void Main(string[] args)
        {
            escuchador = new TcpListener(System.Net.IPAddress.Any, 2307);
            escuchador.Start();
            Task[] clientes = new Task[2];
            for (int i = 0; i < 2; i++)
            {
                clientes[i] = find(i);
            }
            Task.WaitAll(clientes);

            streams = new NetworkStream[2];
            readers = new StreamReader[2];
            writers = new StreamWriter[2];

            for (int i = 0; i < 2; i++)
            {
                streams[i] = new NetworkStream(sockets[i]);
                readers[i] = new StreamReader(streams[i]);
                writers[i] = new StreamWriter(streams[i]);
            }

            int turn = 0;

            while (sockets[0].Connected && sockets[1].Connected)
            {
                try
                {
                    // manda tiro
                    String line = readers[turn].ReadLine();
                    writers[(turn + 1) % 2].WriteLine(line);
                    writers[(turn + 1) % 2].Flush();

                    // manda resultado de tiro
                    line = readers[(turn + 1) % 2].ReadLine();
                    writers[turn].WriteLine(line);
                    writers[turn].Flush();

                    turn = (turn + 1) % 2;
                } catch(IOException ex)
                {
                    Console.WriteLine("Se ha perdido la conexión");
                }
            }


            Console.WriteLine("Adios");
            for (int i = 0; i < 2; i++)
            {
                readers[i].Close();
                writers[i].Close();
                streams[i].Close();
                sockets[i].Close();
            }

        }

        public static async Task find(int turn)
        {
            Socket socket = await escuchador.AcceptSocketAsync();
            if (socket.Connected)
            {
                Console.WriteLine("Cliente " + socket.RemoteEndPoint + " se conecto");
                NetworkStream stream = new NetworkStream(socket);
                StreamWriter writer = new StreamWriter(stream);
                StreamReader reader = new StreamReader(stream);


                // Entregar tamaños de naves
                String[] parts = new String[ships.Length];
                for (int i = 0; i < ships.Length; i++)
                {
                    parts[i] = ships[i].ToString();
                }
                String line = String.Join(" ", parts);

                await writer.WriteLineAsync(line);
                writer.Flush();

                // Entregar tamaño de la matriz
                line = width.ToString() + " " + height.ToString();

                await writer.WriteLineAsync(line);
                writer.Flush();


                // Entregar jugador
                await writer.WriteLineAsync(turn.ToString());
                writer.Flush();
                reader.Close();
                writer.Close();
                stream.Close();
            }
            if (sockets == null)
            {
                sockets = new Socket[2];
            }
            sockets[turn] = socket;
        }

        public static async Task Escucha(int n)
        {
            Socket socket = await escuchador.AcceptSocketAsync();
            if (socket.Connected)
            {
                
                Console.WriteLine("Cliente " + socket.RemoteEndPoint + " se conecto");
                NetworkStream stream = new NetworkStream(socket);
                StreamWriter writer = new StreamWriter(stream);
                StreamReader reader = new StreamReader(stream);

                /*
                await writer.WriteLineAsync("Jugador " + n.ToString());
                await writer.FlushAsync();
                */

                String[] parts = new String[ships.Length];
                for (int i=0;i<ships.Length; i++)
                {
                    parts[i] = ships[i].ToString();
                }
                String line = String.Join(" ", parts);

                await writer.WriteLineAsync(line);

                line = width.ToString() + " " + height.ToString();

                await writer.WriteLineAsync(line);

                while (true)
                {
                    line = await reader.ReadLineAsync();
                    if (line.Equals("adios"))
                    {
                        Console.WriteLine("Adios");
                        break;
                    }
                    await writer.WriteLineAsync("ok");
                    await writer.FlushAsync();

                }
                reader.Close();
                writer.Close();
                stream.Close();

            }
            socket.Close();
        }

    }
}