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
using TeamsHook.NET;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SharpConfig;
using Infinigate.Watchguard.Classes;

MySqlConnection? conn = null;
MySqlCommand cmd;
long elapsedMs=0;
bool sendTeams=false;

string homepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string configfile = homepath + "/.Infinigate.Watchguard.ClusterStatus";

string teams_webhook_url = "";
string mysql_pass = "";
string mysql_host = "";
string mysql_base = "";
string mysql_user = "";
string connstring = "";
string sql = "";
string previous_json = "";
int rowcount = 0;
int status=0;
List<ClusterStatusResult> clusterstatusresults = new();
ClusterStatusResult clusterstatusresult;

string clusterstatusresult_query="INSERT INTO librenms.clusterstatus (device_id,`datetime`,status,json) VALUES ";
clusterstatusresult_query += "(@DeviceId,@DatumTijd,@Status,@Json);";

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

    section = config["Teams"];
    teams_webhook_url = section["webhook_url_clusterstatus"].StringValue;

    if (teams_webhook_url!="") {
        sendTeams=true;
    }
       
    Console.WriteLine("Connecting to MySQL...");
    conn = new MySqlConnection(connstring);
    conn.Open();
    
    Console.WriteLine("Creating Table if not exist...");
    sql = "CREATE TABLE if not exists librenms.clusterstatus (status_id INT NOT NULL AUTO_INCREMENT PRIMARY KEY, device_id INT, `datetime` DATETIME, status INT, json TEXT);";
    cmd=new MySqlCommand(sql,conn);
    cmd.ExecuteNonQuery();

    sql = "SELECT DISTINCT hostname, sysName, device_id, snmp_disable ,snmpver , authname ,authalgo ,authpass ,cryptoalgo ,cryptopass,community FROM librenms.devices WHERE serial LIKE '%,%' AND snmp_disable=0 AND disable_notify=0 AND disabled=0 AND `ignore`=0 AND status=1";
    cmd=new MySqlCommand(sql,conn);
    MySqlDataReader reader=cmd.ExecuteReader();

    if (reader.HasRows) {
        while (reader.Read()) {
            rowcount++;
        }
        reader.Close();
        Console.WriteLine(rowcount + " clusters found.");

        reader=cmd.ExecuteReader();
        while (reader.Read()) {
            Console.WriteLine("IP:" + reader.GetString(0) + " - " + reader.GetString(1) + " (" + reader.GetString(2) + ")" );
            Console.WriteLine("---------------------------------------------------------");
                        
            clusterstatusresult = Functions.GetClusterStatus(reader.GetInt32(2), reader.GetString(0), reader.GetString(4), reader.GetString(5), reader.GetString(10), reader.GetString(6), reader.GetString(7), reader.GetString(8), reader.GetString(9));
                        
            Console.WriteLine(clusterstatusresult.ToJSON());
            Console.WriteLine("---------------------------------------------------------\n");

            clusterstatusresults.Add(clusterstatusresult);
        }
        reader.Close();

        Console.WriteLine("Adding statusses to database table and treating results...");
        foreach (ClusterStatusResult result in clusterstatusresults) {
            status=0;
            
            sql = "SELECT * FROM librenms.clusterstatus WHERE device_id=" + result.DeviceId + " ORDER BY `datetime` DESC LIMIT 1";
            cmd=new MySqlCommand(sql,conn);
            reader=cmd.ExecuteReader();
            if (reader.HasRows) {
                reader.Read();
                previous_json = reader.GetString(4);
            }
            reader.Close();

            if (previous_json!=result.ToJSON()) {
                //cluster status has changed, act...

                status=1;

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

watch.Stop();
elapsedMs = watch.ElapsedMilliseconds;
TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);

Console.WriteLine("In Total, it took " + t.ToString(@"hh\:mm\:ss\:fff"));

if (conn!=null) {
    if (conn.State.ToString()=="Open") {
        conn.Close();
    }
}

if (sendTeams) {
    var tclient = new TeamsHookClient();
    var card = new MessageCard();
    card.Title="Check of Clusters";
    card.Text="Cluster Check Done.\n It took " + t.ToString(@"hh\:mm\:ss\:fff");
    await tclient.PostAsync(teams_webhook_url, card);
}

//TODO add overview of cluster status history to device page in LibreNMS