﻿using System;
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
using ESP32RFID.Services.IotUpdater;
using ESP32RFID.Services;

namespace ESP32RFID.ViewModels
{
    public class SettingViewModel : BaseViewModel
    {
        ESP32RfidClient client;
        updater _updater = new updater();
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
        public string FirmwarePath { get => firmwarePath; set => SetProperty(ref firmwarePath, value); }
        public string LogText { get => logText; set => SetProperty(ref logText, value); }
        public bool IsAp { get => isAp; set => SetProperty(ref isAp, value); }
        public Command SetWifi => new Command(() =>
        {
            try
            {
                client.ConfigureWifi(ssid, password, IsAp);
            }catch
            (Exception ex)
            {
                logText += ex.ToString();
            }
        });

        public AsyncCommand SelectFirmware => new AsyncCommand(async () =>

        {
            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result != null)
                {
                    if (result.FileName.EndsWith(".bin", StringComparison.OrdinalIgnoreCase) )
                    {
                        FirmwarePath= result.FullPath;
                    }
                }

            }
            catch (Exception ex)
            {
                LogText += ex.ToString();
            }
        });
        public Command UploadFirmware => new Command(() =>
        {
            try
            {
                Task.Run(() => { 
                var update = File.ReadAllBytes(firmwarePath);
                _updater.update("kyber.local", 3232, 3232, "", update, (msg) => {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LogText += msg + "\n";
                    });
                });
                });
            }
            catch (Exception ex)
            {
                LogText += ex.ToString();
            }
        });

        private string status = "Not Connected";
        private string ssid = "JuggSlug";
        private string password = "wittypotato608";
        private string firmwarePath = "";
        private string logText = "";
        private bool isAp = false;
    }
}
