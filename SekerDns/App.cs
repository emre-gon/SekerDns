using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

namespace SetDns
{
	public class App : Application
	{
		public App()
		{
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
		}

		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		[STAThread]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			Process currentProcess = Process.GetCurrentProcess();
			Process process1 = Process.GetProcesses().Where<Process>((Process process) => {
				if (process.Id == currentProcess.Id)
				{
					return false;
				}
				return process.ProcessName.Equals(currentProcess.ProcessName, StringComparison.Ordinal);
			}).FirstOrDefault<Process>();
			if (process1 != null)
			{
				Application.Current.Shutdown();
				App.ShowWindow(process1.MainWindowHandle, 5);
				App.SetForegroundWindow(process1.MainWindowHandle);
			}
		}

		[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
	}
}