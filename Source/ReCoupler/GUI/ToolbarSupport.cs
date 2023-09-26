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
using UnityEngine;

using KSPe.Annotations;
using Toolbar = KSPe.UI.Toolbar;
using GUI = KSPe.UI.GUI;
using GUILayout = KSPe.UI.GUILayout;
using KSPe.UI.Toolbar;
using KSP.UI.Screens;

namespace ReCoupler.GUI
{
	[KSPAddon(KSPAddon.Startup.MainMenu, true)]
	public class ToolbarController : MonoBehaviour
	{
		private static ToolbarController _Instance = null;
		internal static ToolbarController Instance => _Instance;
		private static KSPe.UI.Toolbar.Toolbar ToolbarInstance => KSPe.UI.Toolbar.Controller.Instance.Get<ToolbarController>();

		[UsedImplicitly]
		private void Awake()
		{
			_Instance = this;
		}

		[UsedImplicitly]
		private void Start()
		{
			KSPe.UI.Toolbar.Controller.Instance.Register<ToolbarController>(Version.FriendlyName);
		}

		[UsedImplicitly]
		private void OnDestroy()
		{
			ToolbarInstance.Destroy();
		}

		private ReCouplerGUI owner = null;
		private Button button;

		internal void Create(ReCouplerGUI owner)
		{
			this.owner = owner;
			if (null != this.button)
			{
				ToolbarInstance.ButtonsActive(true, true);
				return;
			}

			button = Toolbar.Button.Create(this
						, ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.FLIGHT
						, Icons.IconOn, Icons.BlizzyOn
						, Icons.IconOff, Icons.BlizzyOff
					)
				;

			button.Toolbar
						.Add(Toolbar.Button.ToolbarEvents.Kind.Active,
							new Toolbar.Button.Event(this.OnRaisingEdge, this.OnFallingEdge)
						);
			;

			ToolbarInstance.Add(button);
		}

		internal void Destroy()
		{
			ToolbarInstance.ButtonsActive(false, false);
			this.owner = null;
		}

		internal void CloseApplication()
		{
			if (null == this.owner) return; // Better safer than sorry!
			this.button.Active = false;
		}

		private void OnRaisingEdge()
		{
			if (null == this.owner) return; // Better safer than sorry!
			this.owner.OnTrue();
		}

		private void OnFallingEdge()
		{
			if (null == this.owner) return; // Better safer than sorry!
			this.owner.OnFalse();
		}

	}
}
