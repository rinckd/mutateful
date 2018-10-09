using Mutate4l.Dto;
using Mutate4l.IO;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Mutate4l.Cli
{
    class CliHandler
    {
        public static void Start()
        {
            TcpListener server = null;
            try
            {
                // Set the TcpListener on port 13000.
                Int32 port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data);

                        // Process the data sent by the client.
                        data = data.ToUpper();

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        stream.Write(msg, 0, msg.Length);
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();

            /*
                        while (true)
                        {
                            var data = UdpConnector.WaitForData();
                            Console.WriteLine($"Received data: {data}");
                            var result = ParseAndProcessCommand(data);
                            if (!result.Success)
                                Console.WriteLine(result.ErrorMessage);
                        }
            */
        }

        public static Result ParseAndProcessCommand(string command)
        {
            var structuredCommand = Parser.ParseFormulaToChainedCommand(command);
            if (!structuredCommand.Success)
                return new Result(structuredCommand.ErrorMessage);

            var result = ClipProcessor.ProcessChainedCommand(structuredCommand.Result);

            return new Result(result.Success, result.ErrorMessage);
        }

        public static void DoDump(string command)
        {
            var arguments = command.Split(' ').Skip(1);
            foreach (var arg in arguments)
            {
                var clipReference = Parser.ResolveClipReference(arg);
                var clip = UdpConnector.GetClip(clipReference.Item1, clipReference.Item2);
                Console.WriteLine($"Clip length {clip.Length}");
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
                /*                                foreach (var note in clip.Notes)
                                                {
                                                    Console.WriteLine($"Note start: {note.Start} duration: {note.Duration} pitch: {note.Pitch} velocity: {note.Velocity}");
                                                }*/
            }
        }

        public static void DoSvg(string command)
        {
            var arguments = command.Split(' ').Skip(1);
            var options = arguments.Where(x => x.StartsWith("-"));
            var clipReferences = arguments.Except(options);
            int octaves = 2;
            int startNote = 60; // C3
            foreach (var option in options)
            {
                if (option.StartsWith("-octaves:"))
                {
                    octaves = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                if (option.StartsWith("-startnote:"))
                {
                    startNote = int.Parse(option.Substring(option.IndexOf(':') + 1));
                }
                //Console.WriteLine($"option: {option}");
            }
            Console.WriteLine($"start: {startNote}");
            Console.WriteLine($"octaves: {octaves}");
            foreach (var clipReference in clipReferences)
            {
                var clipRefParsed = Parser.ResolveClipReference(clipReference);
                var clip = UdpConnector.GetClip(clipRefParsed.Item1, clipRefParsed.Item2);
                Console.WriteLine(Utility.IOUtilities.ClipToString(clip));
                var output = "<svg version=\"1.1\" baseProfile=\"full\" width=\"400\" height=\"300\" xmlns=\"http://www.w3.org/2000/svg\">";
                var yDelta = 300 / (octaves * 12);
                // piano + horizontal guides
                for (int i = 0; i <= octaves * 12; i++)
                {
                    bool white = i % 12 == 0 || i % 12 == 2 || i % 12 == 4 || i % 12 == 5 || i % 12 == 7 || i % 12 == 9 || i % 12 == 11;
                    output += $"<rect style=\"fill:#{(white ? "ffffff" : "000000")};fill-opacity:1;stroke:#8e8e8e;stroke-width:1;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"0\" y=\"{300 - yDelta - (i * yDelta)}\" width=\"30\" height=\"{yDelta}\" />";
                    output += $"<line x1=\"30\" x2=\"400\" y1=\"{300 - yDelta - (i * yDelta)}\" y2=\"{300 - yDelta - (i * yDelta)}\" stroke-width=\"1\" stroke=\"#bbbbbb\" />";
                }
                // vertical guides
                var xDelta = 370 / clip.Length;
                for (decimal i = 0; i < clip.Length; i += 4m / 8) // 8ths for now
                {
                    output += $"<line x1=\"{30 + (i * xDelta)}\" x2=\"{30 + (i * xDelta)}\" y1=\"0\" y2=\"300\" stroke-width=\"1\" stroke=\"#dddddd\" />";
                }
                foreach (var note in clip.Notes)
                {
                    if (note.Pitch >= startNote && note.Pitch <= startNote + (octaves * 12))
                    {
                        output += $"<rect style=\"fill:#ebebbc;fill-opacity:1;stroke:#8e8e8e;stroke-width:0.52916664;stroke-miterlimit:4;stroke-dasharray:none;stroke-opacity:1\" x=\"{30 + (note.Start * xDelta)}\" y=\"{(startNote + (octaves * 12) - note.Pitch) * yDelta}\" width=\"{note.Duration * xDelta}\" height=\"{yDelta}\" />";
                    }
                }
                output += "</svg>";
                Console.WriteLine(output);
            }
        }
    }
}
