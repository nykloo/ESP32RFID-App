using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmHelpers;
using MvvmHelpers.Interfaces;
using MvvmHelpers.Commands;
using Plugin.BLE;
using System.Diagnostics;
using Websocket.Client;
using Newtonsoft.Json.Linq;
using Command = MvvmHelpers.Commands.Command;
using ESP32RFID.Services;
using System.Reactive.Linq;
using ServiceProvider = ESP32RFID.Services.ServiceProvider;
    
namespace ESP32RFID.ViewModels
{
    public class ToolbarViewModel : BaseViewModel
    {
        IESP32RfidClient client;
        public ToolbarViewModel():this(null) { }
        public ToolbarViewModel(IESP32RfidClient esp32RfidClient =null)
        {
            client = esp32RfidClient;
            Status = "Not Connected";
            client ??= ServiceProvider.GetService<IESP32RfidClient>();
            client.Errors.Subscribe(value =>
            {
                Debug.WriteLine(value);
            });
            client.ConnectionState.Subscribe(state =>
            {
                if (state == ESP32RfidClientState.CONNECTED) { Status = "Connected"; }
                if (state == ESP32RfidClientState.DISCONNECTED) { Status = "Not Connected"; }
                if (state == ESP32RfidClientState.CONNECTING) { Status = "Connecting"; }
            });

        }

        public string Status { get => status; set => SetProperty(ref status, value); }
        public Command Connect => new Command(() =>
        {
            client.Connect();
        });

        private string status = "Not Connected";
    }
}
