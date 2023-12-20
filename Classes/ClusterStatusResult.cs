using System;
using System.Net;
using SnmpSharpNet;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infinigate.Watchguard.Classes
{
    public class ClusterStatusResult
    {
        private List<MemberStatusResult>? _Result = new();
        private int? _DeviceId = 0;
        private string? _DeviceName = "";
        public List<MemberStatusResult>? Result { get => _Result; set => _Result = value; }
        public int? DeviceId { get => _DeviceId; set => _DeviceId = value; }
        public string? DeviceName { get => _DeviceName; set => _DeviceName = value; }
        
        public ClusterStatusResult() {
            _Result = new();
        }

        public ClusterStatusResult(int deviceid, string devicename, VbCollection vblist) {
            _Result = new();
            _DeviceId = deviceid;            
            _DeviceName = devicename;
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
}