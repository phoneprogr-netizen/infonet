using System;
using System.Collections.Generic;

namespace AppInfonet.Models
{
    public class DashboardV2Response
    {
        public string? SearchPlate { get; set; }
        public List<DashboardDeviceItem> ItemList { get; set; } = new();
    }

    public class DashboardDeviceItem
    {
        public bool AllowedZeus { get; set; }
        public string COD_PERIF { get; set; } = string.Empty;
        public string? DeviceName { get; set; }
        public string? Plate { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? FullAddress { get; set; }
        public bool Quadro { get; set; }
        public double Velocita { get; set; }
        public double Batteria { get; set; }
        public List<DeviceInput> Inputs { get; set; } = new();
        public List<DeviceOutput> Outputs { get; set; } = new();
        public bool HasIgnition { get; set; }
        public double SpeedKmph { get; set; }
        public bool NotifySettingView { get; set; }

        // --- Proprietà "di comodo" per la UI ---

        // Nome visualizzato: targa + nome mezzo, oppure solo DeviceName, oppure COD_PERIF
        public string DisplayName =>
            !string.IsNullOrWhiteSpace(Plate) && !string.IsNullOrWhiteSpace(DeviceName)
                ? $"{Plate} - {DeviceName}"
                : !string.IsNullOrWhiteSpace(DeviceName)
                    ? DeviceName!
                    : !string.IsNullOrWhiteSpace(Plate)
                        ? Plate!
                        : COD_PERIF;

        // Data/ora ultimo fix in formato leggibile
        public string LastFixDisplay =>
            Timestamp.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        public string QuadroDisplay =>
            $"Quadro: {(Quadro ? "ON" : "OFF")}";

        public string SpeedDisplay =>
            $"Velocità: {SpeedKmph:0} km/h";

        public string BatteryDisplay =>
            $"Batteria: {Batteria:0.0} V";
    }

    public class DeviceInput
    {
        public int Index { get; set; }
        public string? Description { get; set; }
        public bool BooleanValue { get; set; }
        public string? RawValue { get; set; }
        public string? TrueValue { get; set; }
        public string? FalseValue { get; set; }
        public string? Value { get; set; }
        public bool IsEditable { get; set; }
        public bool CanThrowAlarm { get; set; }
        public string? Error { get; set; }
    }

    public class DeviceOutput
    {
        public int Index { get; set; }
        public string? Description { get; set; }
        public bool BooleanValue { get; set; }
        public string? RawValue { get; set; }
        public string? TrueValue { get; set; }
        public string? FalseValue { get; set; }
        public string? Value { get; set; }
        public bool IsEditable { get; set; }
        public bool CanThrowAlarm { get; set; }
        public string? Error { get; set; }
    }
}