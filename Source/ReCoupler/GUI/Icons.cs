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
using KSPe.Annotations;

using UnityEngine;
using H = KSPe.IO.Hierarchy<ReCoupler.Startup>;
using T = KSPe.Util.Image.Texture2D;

namespace ReCoupler.GUI
{
	internal static class Icons
	{
		private const string ICONSDIR = "Icons";

		private static Texture2D _IconOff = null;
		internal static Texture2D IconOff => _IconOff ?? (_IconOff = T.LoadFromFile(H.GAMEDATA.Solve("PluginData", ICONSDIR, "ReCoupler_Icon_off")));

		private static Texture2D _IconOn = null;
		internal static Texture2D IconOn => _IconOn ?? (_IconOn = T.LoadFromFile(H.GAMEDATA.Solve("PluginData", ICONSDIR, "ReCoupler_Icon")));

		private static Texture2D _BlizzyOff = null;
		internal static Texture2D BlizzyOff => _BlizzyOff ?? (_BlizzyOff = T.LoadFromFile(H.GAMEDATA.Solve("PluginData", ICONSDIR, "ReCoupler_blizzy_Icon_off")));

		private static Texture2D _BlizzyOn = null;
		internal static Texture2D BlizzyOn => _BlizzyOn ?? (_BlizzyOn = T.LoadFromFile(H.GAMEDATA.Solve("PluginData", ICONSDIR, "ReCoupler_blizzy_Icon")));
	}

	[KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
	internal class IconPreloader : MonoBehaviour
	{
		[UsedImplicitly]
		private void Start()
		{   // Preload the icons on Space Center to avoid halting the Editor at first entry.
			Log.dbg(H.GAMEDATA.Solve("PluginData", "Icons", "ReCoupler_Icon_off"));
			Icons.IconOff.GetInstanceID();
			Icons.IconOn.GetInstanceID();
			Icons.BlizzyOff.GetInstanceID();
			Icons.BlizzyOn.GetInstanceID();
		}
	}
}
