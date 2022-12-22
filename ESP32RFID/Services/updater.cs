using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESP32RFID.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading;

    namespace IotUpdater
    {
        public class updater
        {
            TcpListener listener;
            public enum FLASH_Options
            {
                FLASH = 0,
                SPIFFS = 100,
                AUTH = 200,
            }

             public static string GetMD5checksum(byte[] inputData)
            {

                //convert byte array to stream
                System.IO.MemoryStream stream = new System.IO.MemoryStream();
                stream.Write(inputData, 0, inputData.Length);

                //important: get back to start of stream
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                //get a string value for the MD5 hash.
                using (var md5Instance = System.Security.Cryptography.MD5.Create())
                {
                    var hashResult = md5Instance.ComputeHash(stream);

                    //***I did some formatting here, you may not want to remove the dashes, or use lower case depending on your application
                    return BitConverter.ToString(hashResult).Replace("-", "").ToLowerInvariant();
                }
            }
            public bool update(string remoteAddr, int remotePort, int localPort, string password, byte[] file_data,Action<string> log, FLASH_Options command = FLASH_Options.FLASH 
            )
            {
                IPAddress address = IPAddress.Any;

                listener = new TcpListener(address, localPort);
                listener.Server.NoDelay = true;
                listener.Start();
                log($"Server started. Listening to TCP clients at 0.0.0.0:{localPort}");
                long content_size = file_data.Length;
                string file_md5 = GetMD5checksum(file_data);
                log($"Upload size {content_size}");

                string message = string.Format("0 {1} {2} {3}\n", command, localPort, content_size, file_md5);
                byte[] message_bytes = Encoding.ASCII.GetBytes(message);
                log("Sending invitation to " + remoteAddr);


                UdpClient sock2 = new UdpClient();
                sock2.Send(message_bytes, message_bytes.Length, remoteAddr,remotePort);
                var t = sock2.ReceiveAsync();
                t.Wait(10000);
                var res = t.Result;
                if (res == null)
                {
                    log("No Response");
                    {
                        //client.Close();
                        listener.Stop();
                        return false;
                    }
                }
                var res_text = Encoding.ASCII.GetString(res.Buffer);
                if (res_text != "OK")
                {
                    log("AUTH requeired and not implemented");
                    {
                        //client.Close();
                        listener.Stop();
                        return false;
                    }
                }
                sock2.Close();
                log("Waiting for device ...");
                listener.Server.SendTimeout = 10000;
                listener.Server.ReceiveTimeout = 10000;
                DateTime startTime = DateTime.Now;
                TcpClient client = null;
                log("Awaiting Connection");
                while ((DateTime.Now - startTime).TotalSeconds < 10)
                {
                    if (listener.Pending())
                    {

                        client = listener.AcceptTcpClient();
                        client.NoDelay = true;
                        break;
                    }
                    else
                        Thread.Sleep(10);
                }
                if (client == null)
                {
                    log("No response from device");
                    {
                        listener.Stop();
                        return false;
                    }
                }
                log("Got Connection");
                using (MemoryStream fs = new MemoryStream(file_data))
                {
                    int offset = 0;
                    byte[] chunk = new byte[1460];
                    int chunk_size = 0;
                    int read_count = 0;
                    string resp = "";
                    while (content_size > offset)
                    {
                        chunk_size = fs.Read(chunk, 0, 1460);
                        offset += chunk_size;
                        if (client.Available > 0)
                        {
                            resp = Encoding.ASCII.GetString(chunk, 0, read_count);
                            Debug.Write(resp);
                        }
                        log($"Written {offset} out of {content_size} ({(float)(offset) / (float)(content_size) * 100})");
                        client.Client.Send(chunk, 0, chunk_size, SocketFlags.None);
                        client.ReceiveTimeout = 10000;

                    }
                    client.ReceiveTimeout = 60000;
                    read_count = client.Client.Receive(chunk);
                    resp = Encoding.ASCII.GetString(chunk, 0, read_count);
                    while (!resp.Contains("O"))
                    {
                        if (resp.Contains("E"))
                        {
                            client.Close();
                            listener.Stop();
                            return false;
                        }
                        read_count = client.Client.Receive(chunk);
                        resp = Encoding.ASCII.GetString(chunk, 0, read_count);
                        Debug.Write(resp);

                        //return;
                    }
                    log("\r\nAll done");
                    client.Close();
                    listener.Stop();
                    return true;
                }





            }

        }
    }
}
