using System.Reactive;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Websocket.Client;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;

/* Unmerged change from project 'ESP32RFID (net6.0)'
Before:
using System.Net;
After:
using System.Net;
using ESP32RFID;
using ESP32RFID.Services;
*/
using System.Net;

namespace ESP32RFID.Services
{
    public enum ESP32RfidClientState
    {
        CONNECTED,
        CONNECTING,
        DISCONNECTED,
    }
    public class RfidResult
    {
        public int data { get; set; }
        public byte address { get; set; }
        public bool error { get; set; }
    };
    public interface IESP32RfidClient
    {
        public IObservable<RfidResult> ReadResults { get; }
        public IObservable<RfidResult> WriteResults { get; }
        public IObservable<string> Errors { get; }
        public IObservable<ESP32RfidClientState> ConnectionState { get; }
        public Task Connect();
        public void ReadKyber();

        public void WriteKyber(int value);
        public void ReadRfid(byte address);
        public void WriteRfid(byte address, int value);
        public void ConfigureWifi(string ssid, string password, bool isAp = false);
        public void DisableRfid();
    }
    public class ESP32RfidClient : IESP32RfidClient
    {
        private Subject<ESP32RfidClientState> connectionState = new Subject<ESP32RfidClientState>();
        public IObservable<ESP32RfidClientState> ConnectionState => connectionState.AsObservable();
        private Subject<RfidResult> readResults = new Subject<RfidResult>();
        public IObservable<RfidResult> ReadResults => readResults.AsObservable();
        private Subject<RfidResult> writeResults = new Subject<RfidResult>();
        public IObservable<RfidResult> WriteResults => writeResults.AsObservable();
        private Subject<string> errors = new Subject<string>();
        public IObservable<string> Errors => errors.AsObservable();
        WebsocketClient wsClient;
        bool Connected;
        ~ESP32RfidClient()
        {
            wsClient?.Dispose();
        }
        public ESP32RfidClient(string host = "kyber.local", WebsocketClient client = null)
        {
            wsClient = client;
            _ = Task.Run(async () =>
            {
                var url = new Uri($"ws://{host}/ws");
                Debug.WriteLine($"ws for {url}");
                wsClient ??= new WebsocketClient(url);
                wsClient.ReconnectTimeout = TimeSpan.FromSeconds(30);
                wsClient.ReconnectionHappened.Subscribe(info => {
                    connectionState.OnNext(ESP32RfidClientState.CONNECTED);
                }
                );
                wsClient.DisconnectionHappened.Subscribe(info => connectionState.OnNext(ESP32RfidClientState.CONNECTING));
                wsClient.MessageReceived.Subscribe(handleMessage);
                await Task.Run(() => StartSendingPing(client));


            });
            ConnectionState.Subscribe(state =>
            {
                if (state == ESP32RfidClientState.CONNECTED)
                {
                    Connected = true;
                }
                else
                {
                    Connected = false;
                }
            });
        }
        private async Task StartSendingPing(WebsocketClient client)
        {
            while (true)
            {
                await Task.Delay(5000);
                wsClient.Send("ping");
            }
        }
        public async Task Connect()
        {
            connectionState.OnNext(ESP32RfidClientState.CONNECTING);
            try
            {
                await wsClient.StartOrFail();
                connectionState.OnNext(ESP32RfidClientState.CONNECTED);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                connectionState.OnNext(ESP32RfidClientState.DISCONNECTED);

            }
        }
        public void ReadKyber()
        {
            sendMessage(readKyberMessage());

        }

        public void WriteKyber(int value)
        {
            sendMessage(writeKyberMessage(value));
        }
        public void ReadRfid(byte address)
        {
            sendMessage(readMessage(address));

        }
        public void WriteRfid(byte address, int value)
        {
            sendMessage(writeMessage(address, value));

        }
        public void ConfigureWifi(string ssid, string password, bool isAp = false)
        {
            sendMessage(configureWifiMessage(ssid, password, isAp));

        }
        public void DisableRfid()
        {
            sendMessage(disableMessage());

        }
        private void sendMessage(string message)
        {
            try
            {
                if (Connected)
                {
                    Debug.WriteLine("Sending");
                    Debug.WriteLine(message);
                    wsClient.Send(message);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        string readKyberMessage()
        {
            return "{message_type:\"read_kyber\"}";
        }
        string writeKyberMessage(int value)
        {
            return $"{{message_type:\"write_kyber\", data:{value}}}";
        }
        string readMessage(byte address)
        {
            return $"{{message_type:\"read\",address:{address}}}";
        }
        string writeMessage(byte address, int value)
        {
            return $"{{message_type:\"write\",address:{address} , data:{value}}}";
        }
        string configureWifiMessage(string ssid, string password, bool isAp)
        {
            return $"{{message_type:\"configure_wifi\",ssid:\"{ssid}\",password:\"{password}\",is_ap:{isAp}}}";
        }

        string disableMessage()
        {
            return "{message_type:\"disable_rfid\"}";
        }
        private void handleMessage(ResponseMessage msg)
        {
            Debug.WriteLine(msg.Text);
            try
            {
                if (msg.Text == "pong")
                {
                    Connected = true;
                }
                else
                {
                    var json = JObject.Parse(msg.Text);
                    if (json["message_type"].Value<string>() == "response")
                    {
                        if (json["response_of"].Value<string>() == "write_kyber" || json["response_of"].Value<string>() == "write")
                        {
                            var result = new RfidResult()
                            {
                                data = json["data"].Value<int>(),
                                address = json["address"].Value<byte>(),
                                error = json["error"].Value<bool>()
                            };
                            writeResults.OnNext(result);
                            if (result.error)
                            {
                                errors.OnNext("Write Error");
                            }

                        }
                        if (json["response_of"].Value<string>() == "read_kyber" || json["response_of"].Value<string>() == "read")
                        {
                            var result = new RfidResult()
                            {
                                data = json["data"].Value<int>(),
                                address = json["address"].Value<byte>(),
                                error = json["error"].Value<bool>()
                            };
                            readResults.OnNext(result);
                            if (result.error)
                            {
                                errors.OnNext("Write Error");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
