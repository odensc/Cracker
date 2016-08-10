using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(Controller_Interact_Touch))]
	public class Controller_Interact_Grab : MonoBehaviour
	{
		#region Variables
		[Tooltip("The render model component to attach grabbed objects to.")]
		public string modelAttachComponent = "tip";
		[Tooltip("Pause collisions when grabbing an object.")]
		public bool pauseCollisions = false;

		private SteamVR_TrackedObject trackedObj;
		private Controller controller;
		private FixedJoint joint;
		private Rigidbody attachPoint;
		#endregion

		#region Instance Methods
		void Start()
		{
			controller = GetComponent<Controller>();
			trackedObj = GetComponent<SteamVR_TrackedObject>();
			SteamVR_Utils.Event.Listen("render_model_loaded", OnRenderModelLoaded);
			GetComponent<SteamVR_TrackedController>().TriggerClicked += new ClickedEventHandler(DoGrabOn);
			GetComponent<SteamVR_TrackedController>().TriggerUnclicked += new ClickedEventHandler(DoGrabOff);
		}

		void OnRenderModelLoaded(params object[] args)
		{
			// add a kinematic rigidbody to the attach point
			if ((bool) args[1])
			{
				SteamVR_RenderModel renderModel = (SteamVR_RenderModel) args[0];
				// make sure this is our render model
				if (!renderModel.transform.IsChildOf(transform)) return;

				GameObject attach = renderModel.FindComponent(modelAttachComponent).GetChild(0).gameObject;
				attachPoint = attach.AddComponent<Rigidbody>();
				attachPoint.isKinematic = true;
			}
		}

		void DoGrabOn(object sender, ClickedEventArgs e)
		{
			if (joint == null)
			{
				var go = GetComponent<Controller_Interact_Touch>().GetTouchedObject();
				if (!go) return;
				// controller_touch checks for interactable, so it's guaranteed we have one
				var interactable = go.GetComponent<Interactable>();
				if (go != null && interactable.isGrabbable && !interactable.IsInteracting())
				{
					// triggers a slight haptic pulse, attach the grabbed object with a joint, and start interaction
					controller.TriggerHapticPulse(1);
					SetCollisions(go, false);
					go.transform.position = attachPoint.transform.position;

					joint = go.AddComponent<FixedJoint>();
					joint.connectedBody = attachPoint;
					interactable.StartInteraction(controller);
				}
			}
		}

		void DoGrabOff(object sender, ClickedEventArgs e)
		{
			if (joint != null)
			{
				// triggers a slight haptic pulse, drops grabbed object, and stop interaction
				var go = joint.gameObject;
				var rigidbody = go.GetComponent<Rigidbody>();
				var interactable = go.GetComponent<Interactable>();
				controller.TriggerHapticPulse(1);
				Destroy(joint);
				joint = null;
				SetCollisions(go, true);
				interactable.StopInteraction(controller);

				if (interactable.applyVelocity)
				{
					// applies the velocities of the controller to the object when thrown/dropped
					var device = SteamVR_Controller.Input((int)trackedObj.index);
					var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
					var vel = origin ? origin.TransformVector(device.velocity) : device.velocity;
					var angVel = origin ? origin.TransformVector(device.angularVelocity) : device.angularVelocity;
					rigidbody.velocity = vel;
					rigidbody.angularVelocity = angVel;

					rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
				}
			}
		}

		void SetCollisions(GameObject go, bool state)
		{
			if (pauseCollisions) go.GetComponent<Rigidbody>().detectCollisions = state;
		}
		#endregion
	}
}