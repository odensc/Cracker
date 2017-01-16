using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(SteamVR_TrackedController))]
	public class Controller : MonoBehaviour
	{
		#region Variables
		private const ushort MAX_HAPTIC_STRENGTH = 3999;

		private SteamVR_TrackedObject trackedObj;
		private float hapticPulseDuration;
		private ushort hapticPulseStrength;
		#endregion

		#region Instance Methods
		void Start()
		{
			trackedObj = GetComponent<SteamVR_TrackedObject>();
		}

		void Update()
		{
			var device = SteamVR_Controller.Input((int) trackedObj.index);
			if (hapticPulseDuration > 0 && hapticPulseStrength > 0)
			{
				device.TriggerHapticPulse(hapticPulseStrength);
				hapticPulseDuration -= Time.deltaTime * 1000;
			}
		}
		#endregion

		#region Public Methods
		/**
		 * <summary>
		 * Triggers a haptic pulse for <paramref name="duration"/> milliseconds, with the specified <paramref name="strength"/>.
		 * </summary>
		 * <param name="duration">The duration in milliseconds of the pulse.</param>
		 * <param name="strength">The strength of the pulse. Defaults to 1000. Will be limited to the maximum of 3999.</param>
		 */
		public void TriggerHapticPulse(int duration, ushort strength=1000)
		{
			this.hapticPulseDuration = duration;
			this.hapticPulseStrength = (ushort) Mathf.Min(strength, MAX_HAPTIC_STRENGTH);
		}
		#endregion
	}
}
