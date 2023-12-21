//title           :Infinigate.Watchguard.ClusterStatus
//description     :This dotnet app runs cluster status checks on all clusters in LibreNMS
//author          :Wouter Vanbelleghem<wouter.vanbelleghem@infinigate.com>
//date            :23/11/2023
//version         :0.1
//usage           :Infinigate.Watchguard.ClusterStatus or dotnet run in folder
//==============================================================================

using System;
using System.Net;
using System.Collections;
using System.Collections.Generic;
using SnmpSharpNet;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpConfig;
using Infinigate.Watchguard.Classes;
using Spryng;
using Spryng.Models.Sms;
using Newtonsoft.Json.Linq;

class Program
{        
    static MySqlConnection? conn = null;
    static MySqlCommand? cmd = null;
    static long elapsedMs=0;

    static string homepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    static string configfile = homepath + "/.Infinigate.Watchguard.ClusterStatus";

    static string teams_webhook_url = "";
    static string mysql_pass = "";
    static string mysql_host = "";
    static string mysql_base = "";
    static string mysql_user = "";
    static string connstring = "";
    static string spryng_user = "";
    static string spryng_pass = "";
    static string spryng_message = "";
    static string teams_message = "";
    static string spryng_phone = "";
    static string sql = "";
    static string previous_json = "";
    static int rowcount = 0;
    static int status = 0;
    static bool act = false;
    static bool alert = false;
    static bool portsnok = false;
    static bool sendTeams = false;
    static bool sendReport = false;
    static bool clusternok = false;
    
    static List<ClusterStatusResult> clusterstatusresults = new();
    static ClusterStatusResult? clusterstatusresult;
    static ClusterStatusResult? previous_result;

    static string clusterstatusresult_query="INSERT INTO librenms.clusterstatus (device_id,`datetime`,status,json) VALUES (@DeviceId,@DatumTijd,@Status,@Json);";

    static void Main(string[] args) {
        if (args.Length!=0) {
            if (args[0]=="report") {
                sendReport=true;
            }
        }

        //debug
        sendReport=true;

        var watch = System.Diagnostics.Stopwatch.StartNew();

        try {
            Console.WriteLine("Using " + configfile + "...");
            var config = Configuration.LoadFromFile(configfile);
            var section = config["MySql"];

            mysql_pass = section["mysql_pass"].StringValue;
            mysql_host = section["mysql_host"].StringValue;
            mysql_base = section["mysql_base"].StringValue;
            mysql_user = section["mysql_user"].StringValue;

            connstring = "Server=" + mysql_host + ";Database=" + mysql_base + ";Uid=" + mysql_user + ";Pwd=" + mysql_pass + ";SslMode=none;convert zero datetime=True";

            section = config["Spryng"];
            spryng_user = section["username"].StringValue;
            spryng_pass = section["password"].StringValue;

            section = config["Teams"];
            teams_webhook_url = section["webhook_url_clusterstatus"].StringValue;

            if (teams_webhook_url!="") {
                sendTeams=true;
            }
            
            Console.WriteLine("Connecting to MySQL...");
            conn = new MySqlConnection(connstring);
            conn.Open();

            sql = "SELECT transport_config FROM librenms.alert_transports at2 LEFT JOIN librenms.transport_group_transport tgt ON at2.transport_id = tgt.transport_id WHERE tgt.transport_group_id = 1";
            cmd = new MySqlCommand(sql,conn);
            MySqlDataReader reader=cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();                                            
                var jsonObject = JObject.Parse(reader.GetString(0));
                spryng_phone=jsonObject.SelectToken("spryng-mobile").ToString();;

            }
            reader.Close();

            //debug
            spryng_phone="32477081318";

            Console.WriteLine("Mobile Nr for SMS : " + spryng_phone);
            
            Console.WriteLine("Creating Table if not exist...");
            sql = "CREATE TABLE if not exists librenms.clusterstatus (status_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, device_id INT, `datetime` DATETIME, status INT, json TEXT);";
            cmd=new MySqlCommand(sql,conn);
            cmd.ExecuteNonQuery();

            sql = "SELECT DISTINCT hostname, sysName, device_id, snmp_disable ,snmpver , authname ,authalgo ,authpass ,cryptoalgo ,cryptopass,community FROM librenms.devices WHERE serial LIKE '%,%' AND snmp_disable=0 AND disable_notify=0 AND disabled=0 AND `ignore`=0 AND status=1";
            cmd=new MySqlCommand(sql,conn);
            reader=cmd.ExecuteReader();

            if (reader.HasRows) {
                while (reader.Read()) {
                    rowcount++;
                }
                reader.Close();
                Console.WriteLine(rowcount + " clusters found.");

                reader=cmd.ExecuteReader();
                while (reader.Read()) {
                    if (!sendReport) {
                        Console.WriteLine("IP:" + reader.GetString(0) + " - " + reader.GetString(1) + " (" + reader.GetString(2) + ")" );
                        Console.WriteLine("---------------------------------------------------------");
                    }

                    clusterstatusresult = Functions.GetClusterStatus(reader.GetInt32(2), reader.GetString(1), reader.GetString(0), reader.GetString(4), reader.GetString(5), reader.GetString(10), reader.GetString(6), reader.GetString(7), reader.GetString(8), reader.GetString(9));
                                
                    if (!sendReport) {
                        Console.WriteLine(clusterstatusresult.ToJSON());
                        Console.WriteLine("---------------------------------------------------------\n");
                    }

                    clusterstatusresults.Add(clusterstatusresult);
                }
                reader.Close();

                Console.WriteLine("Adding statusses to database table and treating results...");
                foreach (ClusterStatusResult result in clusterstatusresults) {
                    status = 0;
                    alert = false;
                    portsnok = false;
                    clusternok = false;
                    spryng_message = "";
                    
                    if (!sendReport) {
                        sql = "SELECT * FROM librenms.clusterstatus WHERE device_id=" + result.DeviceId + " ORDER BY `datetime` DESC LIMIT 1";
                        cmd = new MySqlCommand(sql,conn);
                        reader=cmd.ExecuteReader();
                        if (reader.HasRows) {
                            reader.Read();
                            previous_json = reader.GetString(4);
                            act = true;                
                        } else {
                            act = false;
                        }
                        reader.Close();
                    }
                    //debug
                    //act=true;
                    
                    if (sendReport) {
                        if ((result.Result[0].RoleInt!=2 && result.Result[0].RoleInt!=3) || (result.Result[1].RoleInt!=2 && result.Result[1].RoleInt!=3)) {
                            clusternok=true;
                        }
                        if (result.Result[0].MonitoredPortHealthIndex<100 || result.Result[1].MonitoredPortHealthIndex<100) {
                            clusternok=true;
                        }
                        if (clusternok) {
                            teams_message += result.DeviceName + " cluster is Not OK.\n";
                        } else {
                            teams_message += result.DeviceName + " cluster is OK.\n";
                        }                
                    } else {                        
                        if (act) {
                            if (previous_json!=result.ToJSON()) {                    
                                previous_result=Newtonsoft.Json.JsonConvert.DeserializeObject<ClusterStatusResult>(previous_json);

                                if (previous_result.Result[0].Role!=result.Result[0].Role) {                                                            
                                    alert=true;
                                    spryng_message += "The cluster status of " + result.DeviceName + " has changed.\n";
                                    spryng_message += "From " + previous_result.Result[0].Role.ToString() + " ";
                                    spryng_message += "To " + result.Result[0].Role.ToString() + "\n\n";                                
                                }
                                if (previous_result.Result[1].Role!=result.Result[1].Role) {                                                            
                                    alert=true;
                                    spryng_message += "The cluster status of " + result.DeviceName + " has changed.\n";
                                    spryng_message += "From " + previous_result.Result[1].Role.ToString() + " ";
                                    spryng_message += "To " + result.Result[1].Role.ToString() + "\n\n";                                
                                }
                                
                                if (result.Result[0].MonitoredPortHealthIndex<100) {
                                    portsnok=true;
                                    spryng_message += "The cluster port health of " + result.DeviceName + " (" + result.Result[0].Role.ToString() + ") is too low (" + result.Result[0].MonitoredPortHealthIndex + ").\n\n";                        
                                }
                                if (result.Result[1].MonitoredPortHealthIndex<100) {
                                    portsnok=true;
                                    spryng_message += "The cluster port health of " + result.DeviceName + " (" + result.Result[1].Role.ToString() + ") is too low (" + result.Result[1].MonitoredPortHealthIndex + ").\n\n";
                                }
                                                    
                                if (alert || portsnok) {
                                    Console.WriteLine(spryng_message);
                                    var spryngclient = SpryngHttpClient.CreateClientWithPassword(spryng_user, spryng_pass);
                                    
                                    SmsRequest request = new SmsRequest()
                                    {                                          
                                        Destinations = new string[] { spryng_phone },
                                        Sender = "RedAlert",
                                        Body = "RED Alert - Cluster Check\n\n" + spryng_message
                                    };

                                    try
                                    {
                                        spryngclient.ExecuteSmsRequest(request);
                                        Console.WriteLine("SMS has been send!");
                                    }
                                    catch (SpryngHttpClientException ex)
                                    {
                                        Console.WriteLine("Send SMS : An Exception occured!\n{0}", ex.Message);
                                    }

                                    if (sendTeams) {   
                                        Task.Run(() => 
                                            {                                     
                                                Functions.SendTeamsMessage(teams_webhook_url,"Cluster Alert", spryng_message + "\nCluster Alert Sent.");
                                            });
                                    }                        
                                }

                                status=1;
                            }
                        }
                    }

                    cmd=new MySqlCommand(clusterstatusresult_query,conn);
                    cmd.Parameters.AddWithValue("@DeviceId", result.DeviceId);
                    cmd.Parameters.AddWithValue("@DatumTijd", DateTime.Now);
                    cmd.Parameters.AddWithValue("@Status", status);
                    cmd.Parameters.AddWithValue("@Json", result.ToJSON());            
                    cmd.ExecuteNonQuery(); 
                }
            }

            Console.WriteLine("Done Cluster Checks.");
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }

        if (sendReport) {
            Console.WriteLine("\nReport:\n------\n" + teams_message);
        }
        
        watch.Stop();
        elapsedMs = watch.ElapsedMilliseconds;
        TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);

        Console.WriteLine("In Total, it took " + t.ToString(@"hh\:mm\:ss\:fff"));

        if (conn!=null) {
            if (conn.State.ToString()=="Open") {
                conn.Close();
            }
        }
        
        if (sendTeams && sendReport) {
            Console.WriteLine("Sending Teams Message...");
            Task.Run(() => 
            {
                Functions.SendTeamsMessage(teams_webhook_url,"Cluster Report",teams_message + "\n" + "Cluster Report Done.\n It took " + t.ToString(@"hh\:mm\:ss\:fff"));
            });            
        }
    }
}
//TODO add overview of cluster status history to device page in LibreNMS