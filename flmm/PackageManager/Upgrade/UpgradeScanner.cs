﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Fomm.PackageManager.Upgrade
{
	/// <summary>
	/// Checks to see if any fomods' versions have changed.
	/// </summary>
	public class UpgradeScanner
	{
		protected static readonly string m_strUpgradeMessage = "A different verion of {0} has been detected. The installed verion is {1}, the new verion is {2}. Would you like to upgrade?" + Environment.NewLine + "Selecting No will replace the FOMOD in FOMM's plugin list, but won't change any files.";
		
		/// <summary>
		/// Scans the mods folder for fomods that have versions that differ from their versions in the install log.
		/// </summary>
		/// <remarks>
		/// If fomods with versions that differ from those in the install log are found, the use is asked whether
		/// to replace or upgrade the fomod. Replacing the fomod merely changes the version in the install log,
		/// but makes no system changes. Upgrading the fomod performs an in-place upgrade.
		/// </remarks>
		public void Scan()
		{
			IList<InstallLog.FomodInfo> lstMods = InstallLog.Current.GetVersionedModList();
			fomod fomodMod = null;
			List<fomod> lstModsToUpgrade = new List<fomod>();
			List<fomod> lstModsToReplace = new List<fomod>();
			foreach (InstallLog.FomodInfo fifMod in lstMods)
			{
				fomodMod = new fomod(Path.Combine(Program.PackageDir, fifMod.BaseName + ".fomod"));
				if (!fomodMod.VersionS.Equals(fifMod.Version))
				{
					switch (MessageBox.Show(String.Format(m_strUpgradeMessage, fomodMod.Name, fifMod.Version, fomodMod.VersionS), "Upgrade", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
					{
						case DialogResult.Yes:
							lstModsToUpgrade.Add(fomodMod);
							break;
						case DialogResult.No:
							lstModsToReplace.Add(fomodMod);
							break;
					}
				}
			}

			Replace(lstModsToReplace);
			Upgrade(lstModsToUpgrade);
		}

		/// <summary>
		/// Upgrades the given fomods.
		/// </summary>
		/// <param name="p_lstModsToUpgrade">The list of fomods to upgrade.</param>
		private void Upgrade(IList<fomod> p_lstModsToUpgrade)
		{
			ModUpgrader mduUpgrader = null;
			foreach (fomod fomodMod in p_lstModsToUpgrade)
			{
				mduUpgrader = new ModUpgrader(fomodMod);
				mduUpgrader.Upgrade();
			}
		}

		/// <summary>
		/// Replaces the given fomods in the install log.
		/// </summary>
		/// <param name="p_lstModsToReplace">The list of fomods to replace.</param>
		protected void Replace(IList<fomod> p_lstModsToReplace)
		{
			if (p_lstModsToReplace.Count > 0)
			{
				foreach (fomod fomodMod in p_lstModsToReplace)
					InstallLog.Current.UpdateMod(fomodMod);
				InstallLog.Current.Save();
			}
		}
	}
}