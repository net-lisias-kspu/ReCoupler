/*
	This file is part of ReCoupler /L Unleashed
		© 2023 Lisias T : http://lisias.net <support@lisias.net>
		© 2017-2022 Booots <David Boots>

	ReCoupler is double licensed, as follows:
		* SKL 1.0 : https://ksp.lisias.net/SKL-1_0.txt
		* GPL 2.0 : https://www.gnu.org/licenses/gpl-2.0.txt

	And you are allowed to choose the License that better suit your needs.

	ReCoupler /L Unleashed is distributed in the hope that it will be
	useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.

	You should have received a copy of the SKL Standard License 1.0
	along with ReCoupler /L Unleashed. If not, see <https://ksp.lisias.net/SKL-1_0.txt>.

	You should have received a copy of the GNU General Public License 2.0
	along with ReCoupler /L Unleashed. If not, see <https://www.gnu.org/licenses/>.

*/
namespace ReCoupler
{
    internal static class ReCouplerSettings
    {
        public const float connectRadius_default = 0.1f;
        public const float connectAngle_default = 91;
        public const bool allowRoboJoints_default = false;
        public const bool allowKASJoints_default = false;
        public const string configURL = "ReCoupler/ReCouplerSettings/ReCouplerSettings";

        public static float connectRadius = connectRadius_default;
        public static float connectAngle = connectAngle_default;
        public static bool allowRoboJoints = allowRoboJoints_default;
        public static bool allowKASJoints = allowKASJoints_default;

        public static bool showGUI = true;
        public static bool isCLSInstalled = false;
        public static bool settingsLoaded = false;

        public static void LoadSettings()
        {
            float loadedRadius = connectRadius;
            float loadedAngle = connectAngle;
            bool loadedAllowRoboJoints = allowRoboJoints;
            bool loadedAllowKASJoints = allowKASJoints;
            bool loadedShowGUI = showGUI;
            if (settingsLoaded)
                return;

            var cfgs = GameDatabase.Instance.GetConfigs("ReCouplerSettings");
            if (cfgs.Length > 0)
            {
                for (int i = 0; i < cfgs.Length; i++)
                {
                    if (cfgs[i].url.Equals(configURL))
                    {
                        if (!float.TryParse(cfgs[i].config.GetValue("connectRadius"), out loadedRadius))
                            loadedRadius = connectRadius;
                        else
                            connectRadius = loadedRadius;

                        if (!float.TryParse(cfgs[i].config.GetValue("connectAngle"), out loadedAngle))
                            loadedAngle = connectAngle;
                        else
                            connectAngle = loadedAngle;

                        if (!bool.TryParse(cfgs[i].config.GetValue("allowRoboJoints"), out loadedAllowRoboJoints))
                            loadedAllowRoboJoints = allowRoboJoints;
                        else
                            allowRoboJoints = loadedAllowRoboJoints;

                        if (!bool.TryParse(cfgs[i].config.GetValue("allowKASJoints"), out loadedAllowKASJoints))
                            loadedAllowKASJoints = allowKASJoints;
                        else
                            allowKASJoints = loadedAllowKASJoints;

                        if (!bool.TryParse(cfgs[i].config.GetValue("showGUI"), out loadedShowGUI))
                            showGUI = true;
                        else
                            showGUI = loadedShowGUI;
                        break;
                    }
                    else if (i == cfgs.Length - 1)
                    {
                        loadedRadius = connectRadius;
                        loadedAngle = connectAngle;
                        UnityEngine.Debug.LogWarning("ReCouplerSettings: Couldn't find the correct settings file. Using default values.");
                    }
                }
            }
            else
            {
                loadedRadius = connectRadius;
                loadedAngle = connectAngle;
                UnityEngine.Debug.LogWarning("ReCouplerSettings: Couldn't find the settings file. Using default values.");
            }

            settingsLoaded = true;
        }
    }
}
