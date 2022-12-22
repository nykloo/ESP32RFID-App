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

namespace ESP32RFID.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        ESP32RfidClient client;
        public SettingViewModel()
        {
            Status = "Not Connected";
            client = ServiceProvider.GetService<ESP32RfidClient>();
            client.ConnectionState.Subscribe(state =>
            {
                if (state == ESP32RfidClientState.CONNECTED) { Status = "Connected"; }
                if (state == ESP32RfidClientState.DISCONNECTED) { Status = "Not Connected"; }
                if (state == ESP32RfidClientState.CONNECTING) { Status = "Connecting"; }
            });

        }

        public string Status { get => status; set => SetProperty(ref status, value); }
        public string Ssid { get => ssid; set => SetProperty(ref ssid, value); }
        public string Password { get => password; set => SetProperty(ref password, value); }
        public bool IsAp { get => isAp; set => SetProperty(ref isAp, value); }
        public Command SetWifi => new Command(() =>
        {
            client.ConfigureWifi(ssid, password, IsAp);
        });

        private string status = "Not Connected";
        private string ssid = "JuggSlug";
        private string password = "wittypotato608";
        private bool isAp = false;
    }
}
