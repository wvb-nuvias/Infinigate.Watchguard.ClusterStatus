using System;
using System.Net;
using SnmpSharpNet;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infinigate.Watchguard.Classes
{
    public class MemberStatusResult
    {
        private string _SerialNumber = "";
        private ClusterRole? _Role;
        private int? _ClusterRoleInt;
        private int? _SystemHealthIndex = 0;
        private int? _HardwareHealthIndex = 0;
        private int? _MonitoredPortHealthIndex = 0;
        private int? _WeightedAvgIndex = 0;

        public string SerialNumber { get => _SerialNumber; set => _SerialNumber = value; }
        public ClusterRole? Role { get => _Role; set => _Role = value; }
        public int? RoleInt { get => _ClusterRoleInt; set => _ClusterRoleInt = value; }
        public int? SystemHealthIndex { get => _SystemHealthIndex; set => _SystemHealthIndex = value; }
        public int? HardwareHealthIndex { get => _HardwareHealthIndex; set => _HardwareHealthIndex = value; }
        public int? MonitoredPortHealthIndex { get => _MonitoredPortHealthIndex; set => _MonitoredPortHealthIndex = value; }
        public int? WeightedAvgIndex { get => _WeightedAvgIndex; set => _WeightedAvgIndex = value; }
       
        public override string ToString() {
            string tmp = _SerialNumber + "\n";

            tmp += "-------------\n";
            tmp += "Role: " + _ClusterRoleInt + "\n";
            tmp += "SHI: " + _SystemHealthIndex + "\n";
            tmp += "HHI: " + _HardwareHealthIndex + "\n";
            tmp += "PHI: " + _MonitoredPortHealthIndex + "\n";
            tmp += "WAI: " + _WeightedAvgIndex + "\n";            

            return tmp;
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}