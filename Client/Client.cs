using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SocketsSSLClient
{
    public class SocketClient
    {
        String host;
        int socketNo;

        public SocketClient(String _host, int _socketNo)
        {
            host = _host;
            socketNo = _socketNo;
        }

        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
            {
                return true;
            }

            return false;
        }

        public void Connect()
        {
            TcpClient client = new TcpClient();
            try
            {          
                client.Connect(new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], socketNo));
                NetworkStream nstr = client.GetStream();

                SslStream sslStream = new SslStream(nstr, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);

                sslStream.AuthenticateAsClient(host);

                StreamReader sr = new StreamReader(new BufferedStream(sslStream), Encoding.UTF8);
                StreamWriter sw = new StreamWriter(sslStream, Encoding.UTF8);

                while (true)
                {
                    try
                    {
                        string data;
                        Console.Write("Enter text (q - exits): ");
                        data = Console.ReadLine();

                        if (data.CompareTo("q") == 0)
                        {
                            break;
                        }


                        sw.WriteLine(data);
                        sw.Flush();
                        Console.WriteLine("Send data: " + data);
                        string recv = sr.ReadLine();
                        Console.Write("Recv data: ");
                        string[] sentences = ClientSys.display(recv);
                        foreach (var s in sentences)
                        {
                            Console.WriteLine(s);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        break;
                    }
                }
                sslStream.Close();
                nstr.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            client.Close();
        }
    }

    class ClientSys
    {
        public static string[] display(string data)
        {
            string[] subs = data.Split("<br>");
            return subs;
        }
    }

}
