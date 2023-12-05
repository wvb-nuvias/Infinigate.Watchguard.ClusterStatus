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

            // Construct a Protocol Data Unit (PDU)
            Pdu pdu = new Pdu();
            // Set the request type (default is Get)
            pdu.Type = PduType.GetBulk;
            // Add variables you wish to query
            pdu.VbList.Add("1.3.6.1.4.1.3097.6.6");

            param.SecurityName.Set("ois");
            param.Authentication = AuthenticationDigests.SHA1;
            param.AuthenticationSecret.Set("ois_Nagios_SNMP4");
            param.Privacy = PrivacyProtocols.DES;
            param.PrivacySecret.Set("SNMP4ois_Nagios");

            // Make a request. Request can throw a number of errors so wrap it in try/catch
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
            UdpTarget target = new UdpTarget((IPAddress)ipa,161,2000,1);
            
            // Construct a Protocol Data Unit (PDU)
            Pdu pdu = new Pdu();
            // Set the request type (default is Get)
            pdu.Type = PduType.GetBulk;
            // Add variables you wish to query
            pdu.VbList.Add("1.3.6.1.4.1.3097.6.6");

            SnmpV1Packet? resultv1 = null;
            SnmpV3Packet? resultv3 = null;

            switch (SNMPVersion) {
                case "v1":
                    OctetString community = new OctetString(SNMPCommunity);
                    AgentParameters paramv1 = new AgentParameters(community);
                    paramv1.Version = SnmpVersion.Ver1;

                    //TODO The error generated by library is a bug in this library, snmpwalk runs fine
                    try {
                        resultv1 = (SnmpV1Packet)target.Request(pdu,paramv1);
                    } catch( Exception ex ) {
                        Console.WriteLine ("Error: {0}", ex.Message);
                        resultv1 = null;
                    }
                    break;
                case "v2c":

                    break;
                case "v3":
                    SecureAgentParameters param = new SecureAgentParameters();

                    if (!target.Discovery(param)) {
                        Console.WriteLine("Discovery failed. Unable to continue...");
                        target.Close();
                        return tmp;
                    }

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