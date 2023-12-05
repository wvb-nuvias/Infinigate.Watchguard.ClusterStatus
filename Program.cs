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

MySqlConnection conn;
MySqlCommand cmd;
long elapsedMs=0;

string homepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
string configfile = homepath + "/.Infinigate.Watchguard.ClusterStatus";

Console.Writeline("Using " + configfile + "...");

var config = Configuration.LoadFromFile(configfile);
var section = config["MySql"];

string mysql_pass = section["mysql_pass"].StringValue;
string mysql_host = section["mysql_host"].StringValue;
string mysql_base = section["mysql_base"].StringValue;
string mysql_user = section["mysql_user"].StringValue;

section = config["Teams"];
string teams_webhook_url = section["webhook_url_clusterstatus"].StringValue;

var watch = System.Diagnostics.Stopwatch.StartNew();

//TODO Check database for all the clusters
//TODO go over one by one

ClusterStatusResult cluster = Functions.GetClusterStatus("172.20.81.254");

foreach(MemberStatusResult member in cluster.Result) {
    Console.WriteLine(member);
}

watch.Stop();
elapsedMs = watch.ElapsedMilliseconds;
TimeSpan t = TimeSpan.FromMilliseconds(elapsedMs);

Console.WriteLine("Done Cluster Checks.");
Console.WriteLine("In Total, it took " + t.ToString(@"hh\:mm\:ss\:fff"));

//TODO write information to database