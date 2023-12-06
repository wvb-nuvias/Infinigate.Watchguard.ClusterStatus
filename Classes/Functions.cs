using System;
using System.Net;
using SnmpSharpNet;

namespace Infinigate.Watchguard.Classes
{
    public class Functions {
        public static ClusterStatusResult GetClusterStatus(string IP) {
            ClusterStatusResult tmp = new();
            IpAddress ipa = new IpAddress (IP);
            UdpTarget target = new UdpTarget((IPAddress)ipa);
            SecureAgentParameters param = new SecureAgentParameters();

            if (!target.Discovery(param)) {
                Console.WriteLine("Discovery failed. Unable to continue...");
                target.Close();
                return tmp;
            }

            Pdu pdu = new Pdu();
            pdu.Type = PduType.GetBulk;            
            pdu.VbList.Add("1.3.6.1.4.1.3097.6.6");

            param.SecurityName.Set("ois");
            param.Authentication = AuthenticationDigests.SHA1;
            param.AuthenticationSecret.Set("ois_Nagios_SNMP4");
            param.Privacy = PrivacyProtocols.DES;
            param.PrivacySecret.Set("SNMP4ois_Nagios");
            
            SnmpV3Packet? result;
            try {
                result = (SnmpV3Packet)target.Request(pdu, param);
            } catch( Exception ex ) {
                Console.WriteLine ("Error: {0}", ex.Message);
                result = null;
            }

            if( result != null ) {                                
                ClusterStatusResult cluster = new(result.Pdu.VbList);
                tmp=cluster;
            }

            return tmp;
        }
    
        public static ClusterStatusResult GetClusterStatus(string IP, string SNMPVersion, string SNMPUser, string SNMPCommunity, string AuthenticationProtocol, string AuthenticationSecret, string PrivacyProtocol, string PrivacySecret) {
            ClusterStatusResult tmp = new();
            IpAddress ipa = new IpAddress (IP);
            UdpTarget target = new UdpTarget((IPAddress)ipa);
            
            Pdu pdu = new Pdu();
           
            SnmpV1Packet? resultv1 = null;
            SnmpV2Packet? resultv2 = null;
            SnmpV3Packet? resultv3 = null;

            switch (SNMPVersion) {
                case "v1":
                    OctetString community = new OctetString(SNMPCommunity);
                    AgentParameters paramv1 = new AgentParameters(community);
                    paramv1.Version = SnmpVersion.Ver1;

                    pdu.Type = PduType.Get;

                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.1.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.2.0");                    
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.3.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.4.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.5.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.6.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.7.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.8.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.9.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.10.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.11.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.12.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.13.0");
                    
                    try {
                        resultv1 = (SnmpV1Packet)target.Request(pdu,paramv1);
                    } catch( Exception ex ) {
                        Console.WriteLine ("Error: {0}", ex.Message);
                        resultv1 = null;
                    }
                    break;
                case "v2c":
                    OctetString community2 = new OctetString(SNMPCommunity);
                    AgentParameters paramv2 = new AgentParameters(community2);
                    paramv2.Version = SnmpVersion.Ver2;

                    pdu.Type = PduType.Get;

                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.1.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.2.0");                    
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.3.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.4.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.5.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.6.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.7.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.8.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.9.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.10.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.11.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.12.0");
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6.13.0");
                    
                    try {
                        resultv2 = (SnmpV2Packet)target.Request(pdu,paramv2);
                    } catch( Exception ex ) {
                        Console.WriteLine ("Error: {0}", ex.Message);
                        resultv2 = null;
                    }

                    break;
                case "v3":
                    SecureAgentParameters param = new SecureAgentParameters();

                    if (!target.Discovery(param)) {
                        Console.WriteLine("Discovery failed. Unable to continue...");
                        target.Close();
                        return tmp;
                    }

                    pdu.Type = PduType.GetBulk;
                    pdu.VbList.Add("1.3.6.1.4.1.3097.6.6");
                    
                    //einddatum featurekey get
                    //pdu.Type = PduType.Get;
                    //pdu.VbList.Add("1.3.6.1.4.1.3097.6.1.5.0");

                    param.SecurityName.Set(SNMPUser);
                    param.Authentication = AuthenticationProtocolFromString(AuthenticationProtocol);
                    param.AuthenticationSecret.Set(AuthenticationSecret);
                    param.Privacy = PrivacyProtocolFromString(PrivacyProtocol);
                    param.PrivacySecret.Set(PrivacySecret);                    
                                        
                    try {
                        resultv3 = (SnmpV3Packet)target.Request(pdu, param);
                    } catch( Exception ex ) {
                        Console.WriteLine ("Error: {0}", ex.Message);
                        resultv3 = null;
                    }
                    break;                
            }

            if( resultv3 != null ) {                                
                ClusterStatusResult cluster = new(resultv3.Pdu.VbList);
                tmp=cluster;
            }
            if( resultv1 != null ) {                                
                ClusterStatusResult cluster = new(resultv1.Pdu.VbList);
                tmp=cluster;
            }

            return tmp;
        }
    
        public static AuthenticationDigests AuthenticationProtocolFromString(string auth) {
            AuthenticationDigests tmp = AuthenticationDigests.None;
            if (auth=="MD5") tmp = AuthenticationDigests.MD5;
            if (auth=="SHA") tmp = AuthenticationDigests.SHA1;
            return tmp;
        }

        public static PrivacyProtocols PrivacyProtocolFromString(string priv) {
            PrivacyProtocols tmp = PrivacyProtocols.None;
            if (priv=="DES") tmp = PrivacyProtocols.DES;
            if (priv=="3DES") tmp = PrivacyProtocols.TripleDES;
            if (priv=="AES") tmp = PrivacyProtocols.AES128;
            if (priv=="AES128") tmp = PrivacyProtocols.AES128;
            if (priv=="AES192") tmp = PrivacyProtocols.AES192;
            if (priv=="AES256") tmp = PrivacyProtocols.AES256;
            return tmp;
        }
    }
}