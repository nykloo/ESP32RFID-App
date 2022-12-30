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

/* Unmerged change from project 'ESP32RFID (net7.0-windows10.0.19041.0)'
Before:
using Command = MvvmHelpers.Commands.Command;
After:
using Command = MvvmHelpers.Commands.Command;
using ESP32RFID;
using ESP32RFID.ViewModels;
*/
using Command = MvvmHelpers.Commands.Command;
using ESP32RFID.Services;
using System.Reactive.Linq;

namespace ESP32RFID.ViewModels
{
    public class RfidWord : ObservableObject
    {
        public RfidWord(byte address,IESP32RfidClient client)
        {
            Address = address;
            this.client = client;
            client.ReadResults.Where(x=>x.address==Address).Subscribe((x) => {
                Value = x.data.ToString("X8");
                Waiting = false;

            });
            client.WriteResults.Subscribe((x) => { });
        }
        private string _value;
        private bool _waiting;
        private readonly IESP32RfidClient client;

        public byte Address { get; init; }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public bool Waiting { get => _waiting; set => SetProperty(ref _waiting, value); }
        public Command Read => new Command(() => {
            client.ReadRfid(Address);
            Waiting = true;
            });
        public Command Write => new Command(() => {
            client.WriteRfid(Address, Convert.ToInt32($"0x{Value}", 16));
        });
        public bool Protected => Address == 2;
    }
    public class RawRfidViewModel : BaseViewModel
    {
        public RawRfidViewModel() : this(null) { }

        public RawRfidViewModel(IESP32RfidClient client = null)
        {
            client ??= ServiceProvider.GetService<IESP32RfidClient>();
            Status = "Not Connected";
            for (byte i = 0; i < 16; i++)
            {
                RfidData.Add(new RfidWord(i, client));
            }
        }

        private void FullScan()
        {
            Task.Run(async () =>
            {
                for (byte i = 0; i < 16; i++)
                {
                    if (!RfidData[i].Protected)
                    {
                        RfidData[i].Read.Execute(null);
                        while (RfidData[i].Waiting)
                        {
                            await Task.Delay(100);
                        }
                    }
                }
            });
        }
        public Command StartFullScan=> new Command(() => { 
            FullScan();
        });
        public string Status { get => status; set => SetProperty(ref status, value); }
        public List<RfidWord> RfidData { get => rfidData; set => SetProperty(ref rfidData, value); }


        private string status = "Not Connected";
        private List<RfidWord> rfidData = new List<RfidWord>();


    }
}
