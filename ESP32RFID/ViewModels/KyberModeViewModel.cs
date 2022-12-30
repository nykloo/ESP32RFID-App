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
    public class KyberModeViewModel : BaseViewModel
    {
        IESP32RfidClient client;
        public KyberModeViewModel():this(null) { }
        public KyberModeViewModel(IESP32RfidClient esp32RfidClient =null)
        {
            client = esp32RfidClient;
            Status = "Not Connected";
            client ??= ServiceProvider.GetService<IESP32RfidClient>();
            client.ReadResults.Where(x=>x.address==6).Subscribe(value =>
            {
                ReadCrystal = Constants.CrystalList.First(x => x.Value == value.data).Key;
            });
            client.WriteResults.Subscribe(value =>
            {
                ReadCrystal = Constants.CrystalList.First(x => x.Value == value.data).Key;
            });
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
        public string LastReadValue { get => lastReadValue; set => SetProperty(ref lastReadValue, value); }
        public string[] CrystalNames { get => crystalNames; set => SetProperty(ref crystalNames, value); }
        public string SelectedCrystal { get => selectedCrystal; set => SetProperty(ref selectedCrystal, value); }
        public string ReadCrystal { get => readCrystal; set => SetProperty(ref readCrystal, value); }
        public Command StartWrite => new Command(() =>
        {
            client.WriteKyber(Constants.CrystalList[SelectedCrystal]);
        });
        public Command StartRead => new Command(() =>
        {
            client.ReadKyber();
        });
        public Command Disable => new Command(() =>
        {
            client.DisableRfid();
        });
        public Command Connect => new Command(() =>
        {

            client.Connect();
        });



        private string status = "Not Connected";
        private string lastReadValue = "";
        private string[] crystalNames = Constants.CrystalList.Keys.ToArray();
        private string selectedCrystal = Constants.CrystalList.Keys.First();
        private string readCrystal = "";
    }
}
