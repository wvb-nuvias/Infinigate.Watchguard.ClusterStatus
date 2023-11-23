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
    }
}