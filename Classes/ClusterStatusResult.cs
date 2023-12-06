using System;
using System.Net;
using SnmpSharpNet;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infinigate.Watchguard.Classes
{
    public enum ClusterRole
    {
        Idle = 0,
        Backup_Master = 2,
        Master = 3        
    }

    public class ClusterStatusResult
    {
        private List<MemberStatusResult>? _Result = new();
        public List<MemberStatusResult>? Result { get => _Result; set => _Result = value; }
        
        public ClusterStatusResult() {
            _Result = new();
        }

        public ClusterStatusResult(VbCollection vblist) {
            _Result = new();
            MemberStatusResult? member = null;
            string key="";
            string[]? spl = null;

            foreach( Vb v in vblist ) {
                if (v!=null) {
                    key=v.ToString();                
                    if (key!="") {
                        if (key.StartsWith("1.3.6.1.4.1.3097.6.6")) 
                        {
                            spl=key.Split(" ");

                            if (spl != null) {
                                if (spl[0].EndsWith(".2.0:")) {
                                    member=new();
                                    member.SerialNumber=spl[2];
                                }
                                if (spl[0].EndsWith(".3.0:")) {                        
                                    member!.RoleInt=int.Parse(spl[2]);
                                    switch (member.RoleInt) {
                                        case 0:
                                            member.Role=ClusterRole.Idle;
                                            break;
                                        case 2:
                                            member.Role=ClusterRole.Backup_Master;
                                            break;
                                        case 3:
                                            member.Role=ClusterRole.Master;
                                            break;
                                    }
                                }
                                if (spl[0].EndsWith(".4.0:")) {                        
                                    member!.SystemHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".5.0:")) {                        
                                    member!.HardwareHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".6.0:")) {                        
                                    member!.MonitoredPortHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".7.0:")) {
                                    member!.WeightedAvgIndex=int.Parse(spl[2]);
                                    _Result.Add(member);                        
                                }      
                                if (spl[0].EndsWith(".8.0:")) {
                                    member=new();
                                    member.SerialNumber=spl[2];
                                }
                                if (spl[0].EndsWith(".9.0:")) {                        
                                    member!.RoleInt=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".10.0:")) {                        
                                    member!.SystemHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".11.0:")) {                        
                                    member!.HardwareHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".12.0:")) {                        
                                    member!.MonitoredPortHealthIndex=int.Parse(spl[2]);
                                }
                                if (spl[0].EndsWith(".13.0:")) {
                                    member!.WeightedAvgIndex=int.Parse(spl[2]);
                                    _Result.Add(member);                        
                                }              
                            }                            
                        }
                    }
                }                
            }
        }

        public string ToJSON() {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }

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