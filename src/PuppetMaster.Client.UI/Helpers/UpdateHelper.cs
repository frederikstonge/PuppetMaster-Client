﻿using System.Linq;
using System.Threading.Tasks;
using PuppetMaster.Client.UI.Properties;
using Squirrel;

namespace PuppetMaster.Client.UI.Helpers
{
    public static class UpdateHelper
    {       
        public static async Task<bool> HasUpdateAsync()
        {
            using var mgr = new GithubUpdateManager(Settings.Default.RepositoryUrl);
            if (mgr.IsInstalledApp)
            {
                var updateInfo = await mgr.CheckForUpdate();
                return updateInfo != null && updateInfo.ReleasesToApply.Any();
            }

            return false;
        }

        public static async Task UpdateAsync()
        {
            using var mgr = new GithubUpdateManager(Settings.Default.RepositoryUrl);
            if (mgr.IsInstalledApp)
            {
                var newVersion = await mgr.UpdateApp();

                if (newVersion != null)
                {
                    UpdateManager.RestartApp();
                }
            }
        }
    }
}
