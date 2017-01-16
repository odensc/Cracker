using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(Controller), typeof(Controller_BezierPointer))]
	public class Controller_Interact_Pointer : MonoBehaviour
	{
		#region Variables
		[Tooltip("The color to highlight with when an option is pointed at.")]
		public Color highlightColor = Color.clear;
		[Tooltip("The button to press to trigger an interaction.")]
		public ControllerButton interactButton = ControllerButton.Trigger;
		
		private Interactable pointedObject;
		private Controller_BezierPointer pointer;
		private Controller controller;
		#endregion

		#region Instance Methods
		void Start()
		{
			pointer = GetComponent<Controller_BezierPointer>();
			controller = GetComponent<Controller>();
			pointer.PointerIn += OnPointerIn;
			pointer.PointerOut += OnPointerOut;

			var trackedCon = GetComponent<SteamVR_TrackedController>();
			interactButton.AddClicked(trackedCon, StartInteract);
			interactButton.AddUnclicked(trackedCon, StopInteract);
		}

		void OnPointerIn(object sender, PointerEventArgs e)
		{
			// when the pointer goes over an interactable, trigger a slight haptic pulse and select it
			var interactable = Interactable.GetInteractable(e.hit.transform.gameObject);
			// make sure it's not a grabbable object
			if (interactable && !interactable.isGrabbable)
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

		void StartInteract(object sender, ClickedEventArgs e)
		{
			// when the trigger is pressed, trigger a haptic pulse and interact with the object
			if (pointedObject)
			{
				controller.TriggerHapticPulse(20);
				pointedObject.StartInteraction(controller);
			}
		}

		void StopInteract(object sender, ClickedEventArgs e)
		{
			// when the trigger is unpressed, stop interaction with the object
			if (pointedObject) pointedObject.StopInteraction(controller);
		}
		#endregion
	}
}