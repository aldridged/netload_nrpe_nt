using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.NetworkInformation;

namespace netload_nrpe_nt
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                //Globals
                int IFIndex = 0;
                double warn_level = 1.0;
                double crit_level = 2.0;
                double utilization_mbps = 0.0;

                //Check Args
                if (args.Length < 1)
                {
                    PrintUsage();
                    Environment.Exit(0);
                }

                if (args.Length == 1 && args[0] == "list")
                {
                    ListInterfaces();
                    Environment.Exit(0);
                }
                else if (args.Length < 3 || args.Length > 3)
                {
                    PrintUsage();
                    Environment.Exit(0);
                }
                else
                {
                    //Parse args
                    IFIndex = Convert.ToInt32(args[0]);
                    warn_level = Convert.ToDouble(args[1]);
                    crit_level = Convert.ToDouble(args[2]);
                }

                //Get Interfaces
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                if (nics == null || nics.Length < 1)
                {
                    Console.WriteLine("CRITICAL ERROR - No network interfaces found.");
                    Environment.Exit(2);
                }

                //Get stats
                IPv4InterfaceStatistics stats_init = nics[IFIndex].GetIPv4Statistics();
                System.Threading.Thread.Sleep(5000);  // 5 Second Sample Time
                IPv4InterfaceStatistics stats_final = nics[IFIndex].GetIPv4Statistics();
                long bytes_rec = stats_final.BytesReceived - stats_init.BytesReceived;
                long bytes_sent = stats_final.BytesSent - stats_init.BytesSent;
                if (bytes_rec > bytes_sent)
                {
                    utilization_mbps = (((double)bytes_rec * 8.0)/5.0) / 1048576.0;
                }
                else
                {
                    utilization_mbps = (((double)bytes_sent * 8.0)/5.0) / 1048576.0;
                }

                //Compare to levels and exit with correct status
                if (utilization_mbps >= crit_level)
                {
                    Console.WriteLine("CRITICAL - Network Utilization {0} Mbps", Math.Round(utilization_mbps,4));
                    Environment.Exit(2);
                }
                else if (utilization_mbps >= warn_level)
                {
                    Console.WriteLine("WARNING - Network Utilization {0} Mbps", Math.Round(utilization_mbps,4));
                    Environment.Exit(1);
                }
                else
                {
                    Console.WriteLine("OK - Network Utilization {0} Mbps", Math.Round(utilization_mbps,4));
                    Environment.Exit(0);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred - " + ex.ToString());
            }
        }

        static void ListInterfaces()
        {
            // Show list of interface names, index and speed
            int idx = 0;
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            if (nics == null || nics.Length < 1)
            {
                Console.WriteLine("CRITICAL ERROR - No network interfaces found.");
                Environment.Exit(2);
            }

            Console.WriteLine("Number of Interfaces: {0}", nics.Length);
            foreach (NetworkInterface adapter in nics)
            {
                Console.WriteLine("Index {0} - Name {1} - Speed {2}", idx, adapter.Name, adapter.Speed/100000);
                idx++;
            }
        }

        static void PrintUsage()
        {
            //Print command usage info
            Console.WriteLine("Usage: netload_nrpe_nt.exe [list] <interface index> <warning Mbps> <critical Mbps>");
        }
    }
}
