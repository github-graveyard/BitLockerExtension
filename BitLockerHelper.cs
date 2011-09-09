using Microsoft.Win32;
using System.IO;
using System;
using System.Windows.Forms;
using System.Diagnostics;
namespace BitLockerExtension {
	internal static class BitLockerHelper {

		public static void Install() {

			try {
				//Step 1: Copy the Programm in the Systemdirectory
				File.Copy(Application.ExecutablePath, applicationDestination, true);

				//Step 2: Create Registry Key
				var rootHive = driveHive;
				if (rootHive == null)
					throw new Exception("Failed to open \"Drive\\shell\"");

				//Create a new RegistryKey or open it 
				var lockHive = rootHive.CreateSubKey("lock-bde");
				if (lockHive == null)
					throw new Exception("Failed to open or create \"Drive\\shell\\lock-bde\"");

				//Write Caption
				lockHive.SetValue(string.Empty, "Laufwerk sperren");

				//This Value defines the Moment when the new ContextMenu entry will be displayed.
				//In this case, the Entry should only show up, the the selected drive is encrypted with bitlocker
				//currently unlocked
				lockHive.SetValue("AppliesTo", "System.Volume.BitLockerProtection:=1");

				//This one indicates, that the ContextMenu-Entry will display an little Shieldicon besides the text.
				lockHive.SetValue("HasLUAShield", string.Empty);

				//Now it's time to create the Command that executes this file again
				var commandHive = lockHive.CreateSubKey("command");
				if (commandHive == null)
					throw new Exception("Failed to open or create \"Drive\\shell\\lock-bde\\command");

				commandHive.SetValue(string.Empty, string.Format("\"{0}\" %V", applicationDestination));
			}
			catch(Exception exc) {
				MessageBox.Show(exc.Message, "BitLockerExtension", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public static void Uninstall() {
			try {
				//Remove Registrykey(s)
				var rootHive = driveHive;
				if (rootHive == null)
					return;
                
				var lockHive = rootHive.OpenSubKey("lock-bde");
				if(lockHive!= null)
					rootHive.DeleteSubKeyTree("lock-bde");

				try {
					//Try to delete the exe. This will fail if the calling one it the same as the installed one.
					if(File.Exists(applicationDestination))
						File.Delete(applicationDestination);
				}
				catch {
				}

			}
			catch (Exception exc) {
				MessageBox.Show(exc.Message, "BitLockerExtension", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public static void lockDrive(string driveLetter) {
			try {
				driveLetter = driveLetter.Replace("\\", "");

				var psi = new ProcessStartInfo("manage-bde", string.Format("-lock {0} -ForceDismount", driveLetter))
				          {CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden};
				Process.Start(psi);
			}
			catch (Exception exc) {
				MessageBox.Show(string.Format("Failed to Lock drive:\r\n{0}", exc.Message), "BitLockerExtension",
								MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private static RegistryKey driveHive {
			get { return Registry.ClassesRoot.OpenSubKey("Drive\\shell", true); }
		}

		private static string applicationDestination{get { return Path.Combine(Environment.SystemDirectory, new FileInfo(Application.ExecutablePath).Name); }}

	}
}
