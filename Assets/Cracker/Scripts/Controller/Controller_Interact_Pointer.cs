using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(Controller), typeof(SteamVR_LaserPointer))]
	public class Controller_Interact_Pointer : MonoBehaviour
	{
		#region Variables
		[Tooltip("The color to highlight with when an option is pointed at.")]
		public Color highlightColor = Color.clear;
		
		private Interactable pointedObject;
		private SteamVR_LaserPointer pointer;
		private Controller controller;
		#endregion

		#region Instance Methods
		void Start()
		{
			pointer = GetComponent<SteamVR_LaserPointer>();
			controller = GetComponent<Controller>();
			pointer.PointerIn += OnPointerIn;
			pointer.PointerOut += OnPointerOut;
			GetComponent<SteamVR_TrackedController>().TriggerClicked += DoStartInteract;
			GetComponent<SteamVR_TrackedController>().TriggerUnclicked += DoStopInteract;
		}

		void OnPointerIn(object sender, PointerEventArgs e)
		{
			// when the pointer goes over an interactable, trigger a slight haptic pulse and select it
			var interactable = e.target.GetComponent<Interactable>() ?? e.target.GetComponentInParent<Interactable>();
			if (interactable)
			{
				interactable.Select(true, controller, highlightColor);
				controller.TriggerHapticPulse(1);
				pointedObject = interactable;
			}
		}

		void OnPointerOut(object sender, PointerEventArgs e)
		{
			// when the pointer leaves an interactable, deselect it
			if (pointedObject) pointedObject.Select(false, controller, null);
			pointedObject = null;
		}

		void DoStartInteract(object sender, ClickedEventArgs e)
		{
			// when the trigger is pressed, trigger a haptic pulse and interact with the object
			if (pointedObject)
			{
				controller.TriggerHapticPulse(20);
				pointedObject.StartInteraction(controller);
			}
		}

		void DoStopInteract(object sender, ClickedEventArgs e)
		{
			// when the trigger is unpressed, stop interaction with the object
			if (pointedObject) pointedObject.StopInteraction(controller);
		}
		#endregion
	}
}