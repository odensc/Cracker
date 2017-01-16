namespace Cracker
{
	public enum ControllerButton
	{
		Trigger,
		Grip,
		TrackpadClick,
		TrackpadTouch,
		Menu,
		System
	}

	public static class ControllerButtonExtensions
	{
		/**
		 * <summary>
		 * Adds a clicked event handler for a ControllerButton.
		 * </summary>
		 * <param name="trackedCon">The tracked controller to get the event handler from.</param>
		 * <param name="handler">The event handler to add.</param>
		 */
		public static void AddClicked(this ControllerButton button, SteamVR_TrackedController trackedCon, ClickedEventHandler handler)
		{
			switch (button)
			{
				case ControllerButton.Trigger:
					trackedCon.TriggerClicked += handler;
					break;

				case ControllerButton.Grip:
					trackedCon.Gripped += handler;
					break;

				case ControllerButton.TrackpadClick:
					trackedCon.PadClicked += handler;
					break;

				case ControllerButton.TrackpadTouch:
					trackedCon.PadTouched += handler;
					break;

				case ControllerButton.Menu:
					trackedCon.MenuButtonClicked += handler;
					break;

				case ControllerButton.System:
					trackedCon.SteamClicked += handler;
					break;
			}
		}

		/**
		 * <summary>
		 * Adds an unclicked event handler for a ControllerButton.
		 * </summary>
		 * <param name="trackedCon">The tracked controller to get the event handler from.</param>
		 * <param name="handler">The event handler to add.</param>
		 */
		public static void AddUnclicked(this ControllerButton button, SteamVR_TrackedController trackedCon, ClickedEventHandler handler)
		{
			switch (button)
			{
				case ControllerButton.Trigger:
					trackedCon.TriggerUnclicked += handler;
					break;

				case ControllerButton.Grip:
					trackedCon.Ungripped += handler;
					break;

				case ControllerButton.TrackpadClick:
					trackedCon.PadUnclicked += handler;
					break;

				case ControllerButton.TrackpadTouch:
					trackedCon.PadUntouched += handler;
					break;

				case ControllerButton.Menu:
					trackedCon.MenuButtonUnclicked += handler;
					break;
			}
		}
	}
}
