using System;
using System.Windows.Forms;

namespace BitLockerExtension {
	internal static class Program {
		[STAThread]
		private static void Main(string[] args) {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			//No Argument passed, install the Application and ContextMenuEntry
			if (args.Length == 0) {
				BitLockerHelper.Install();
			}
			else if (args.Length == 1) {

				var argument = args[0].ToLower();
				if (argument == "/uninstall") {
					BitLockerHelper.Uninstall();
					return;
				}
				BitLockerHelper.lockDrive(argument);
			}

		}
	}
}
