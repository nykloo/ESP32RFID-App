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

/* Unmerged change from project 'ESP32RFID (net6.0-windows10.0.19041.0)'
Before:
using Command = MvvmHelpers.Commands.Command;
After:
using Command = MvvmHelpers.Commands.Command;
using ESP32RFID;
using ESP32RFID.ViewModels;
*/
using Command = MvvmHelpers.Commands.Command;

namespace ESP32RFID.ViewModels
{
    public class RfidWord : ObservableObject
    {
        private int _value;

        public byte Address { get; set; }
        public int Value { get => _value; set => SetProperty(ref _value, value); }
        public Command Read => new Command(() => { });
        public Command Write => new Command(() => { });
        public bool Protected => Address <= 4;
    }
    public class RawRfidViewModel : BaseViewModel
    {
        public RawRfidViewModel()
        {
            Status = "Not Connected";

        }



        public string Status { get => status; set => SetProperty(ref status, value); }
        public RfidWord[] RfidData { get => rfidData; set => SetProperty(ref rfidData, value); }
        public Command StartWrite => new Command(() =>
        {
            //   wsClient.Send(writeKyberMessage(Constants.CrystalList[SelectedCrystal]));

        });
        public Command StartRead => new Command(() =>
        {
        });
        public Command Disable => new Command(() =>
        {
        });
        public Command Connect => new Command(() =>
        {

        });



        private string status = "Not Connected";
        private RfidWord[] rfidData = new RfidWord[] { };


    }
}
