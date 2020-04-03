using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Management;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net;
using Microsoft.Win32;

namespace SetDns
{
	public partial class MainWindow : Window
	{
		public readonly static Dictionary<string, string[]> DnsServers;

		static MainWindow()
		{
			Dictionary<string, string[]> strs = new Dictionary<string, string[]>()
			{
				{ "Otomatik", null },
				{ "OpenDns", new string[] { "208.67.222.222", "208.67.220.220" } },
				{ "DnsAdventage", new string[] { "156.154.70.1", "156.154.71.1" } },
				{ "Google", new string[] { "8.8.8.8", "8.8.4.4" } },
				{ "Yandex", new string[] { "77.88.8.8", "77.88.8.1" } },
				{ "Norton", new string[] { "199.85.126.10", "199.85.127.10" } },
				{ "CloudFlare", new string[] { "1.1.1.1", "1.0.0.1" } }
			};
			MainWindow.DnsServers = strs;

		}

		public MainWindow()
		{
			this.InitializeComponent();
			PaintCurrent();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			var button = ((Button)sender);
			string serverName = (string)button.Content;
			try
			{
				MainWindow.SetNameservers(MainWindow.DnsServers[serverName]);
				this.myWindow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
			}
			catch (Exception exception)
			{
				this.myWindow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Tomato"));
			}
			finally
			{
				PaintCurrent();
			}
		}

		private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			this.myWindow.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightYellow"));
		}

		public static void SetNameservers(string[] dnsServers)
		{
			using (ManagementClass managementClass = new ManagementClass("Win32_NetworkAdapterConfiguration"))
			{
				using (ManagementObjectCollection instances = managementClass.GetInstances())
				{
					IEnumerable<ManagementObject> item = 
						from ManagementObject objMO in instances
						where (bool)objMO["IPEnabled"]
						select objMO;
					foreach (ManagementObject managementObject in 
						from f in item
						where !f["Caption"].ToString().Contains("VMware")
						select f)
					{
						using (ManagementBaseObject methodParameters = managementObject.GetMethodParameters("SetDNSServerSearchOrder"))
						{
							methodParameters["DNSServerSearchOrder"] = dnsServers;
							managementObject.InvokeMethod("SetDNSServerSearchOrder", methodParameters, null);
						}
					}
				}
			}
		}

		public void PaintCurrent()
		{
			foreach(var str in DnsServers)
			{
				((Button)Grd.FindName(str.Key)).ClearValue(Button.BackgroundProperty);
			}

			var adresses = GetDnsAdress();

			foreach (var ip in adresses)
			{
				var selectedServers = MainWindow.DnsServers
								.Where(f => f.Value != null && f.Value.Contains(ip.ToString())).Select(f => f.Key);

				foreach (var server in selectedServers)
				{
					((Button)Grd.FindName(server)).Background
						= new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
				}			
			}

			string auto;
			if (IsAutoDns())
			{
				Otomatik.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("LightGreen"));
				auto = "\nOtomatik ";
			} else
				auto = "";		


			lbl.Text = $"Þu an: {auto}\n{string.Join("\n", adresses)}";
		}


		public static IEnumerable<IPAddress> GetDnsAdress()
		{
			var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(f => f.OperationalStatus == OperationalStatus.Up
							&& !f.Description.Contains("VMware")
							&& !f.Description.Contains("Loopback"));
					
			

			var ips = networkInterfaces.SelectMany(f => f.GetIPProperties().DnsAddresses);

			return ips;
			
		}

		private static bool IsAutoDns()
		{
			var firstInterface = NetworkInterface.GetAllNetworkInterfaces()
				.Where(f => f.OperationalStatus == OperationalStatus.Up
							&& !f.Description.Contains("VMware")
							&& !f.Description.Contains("Loopback")).First();

			string path = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\" 
				+ firstInterface.Id;
			string ns = (string)Registry.GetValue(path, "NameServer", null);
			return String.IsNullOrEmpty(ns);
		}
	}
}