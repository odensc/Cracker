using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(Controller_Interact_Touch))]
	public class Controller_Interact_Grab : MonoBehaviour
	{
		#region Variables
		[Tooltip("The controller button to trigger a grab.")]
		public ControllerButton controllerButton = ControllerButton.Trigger;
		[Tooltip("The render model component to attach grabbed objects to.")]
		public string modelAttachComponent = "tip";
		[Header("Global Options")]
		[Tooltip("Pause collisions when grabbing an object.")]
		public bool pauseCollisions = false;

		[System.NonSerialized]
		public Rigidbody _attachComponent;

		private SteamVR_TrackedObject trackedObj;
		private Controller controller;
		private Joint joint;
		private Interactable grabbedObj;
		private Transform prevParent;
		#endregion

		#region Instance Methods
		void Start()
		{
			controller = GetComponent<Controller>();
			trackedObj = GetComponent<SteamVR_TrackedObject>();
			SteamVR_Utils.Event.Listen("render_model_loaded", OnRenderModelLoaded);
			if (GetComponentInChildren<SteamVR_RenderModel>().FindComponent(modelAttachComponent))
				OnRenderModelLoaded(GetComponentInChildren<SteamVR_RenderModel>(), true);

			var trackedCon = GetComponent<SteamVR_TrackedController>();
			controllerButton.AddClicked(trackedCon, DoGrabOn);
			controllerButton.AddUnclicked(trackedCon, DoGrabOff);
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
				_attachComponent = attach.AddComponent<Rigidbody>();
				_attachComponent.isKinematic = true;
			}
		}

		void DoGrabOn(object sender, ClickedEventArgs e)
		{
			if (!grabbedObj)
			{
				var go = GetComponent<Controller_Interact_Touch>().GetTouchedObject();
				if (!go) return;
				// controller_touch checks for interactable, so it's guaranteed we have one
				var interactable = Interactable.GetInteractable(go);
				if (go != null && interactable.isGrabbable && !interactable.IsInteracting())
				{
					// triggers a slight haptic pulse, attach the grabbed object with a joint, and start interaction
					controller.TriggerHapticPulse(1, 1000);
					SetCollisions(interactable, false);
					if (interactable.updateTransform)
					{
						// if there's an attach point, snap to it
						if (interactable.attachPoint)
						{
							interactable.transform.rotation = _attachComponent.transform.rotation * Quaternion.Euler(interactable.attachPoint.localEulerAngles);
							interactable.transform.position = _attachComponent.transform.position -
								(interactable.attachPoint.position - interactable.transform.position);

						}
						// otherwise snap to controller
						else
						{
							interactable.transform.position = _attachComponent.transform.position;
						}
					}

					switch (interactable.grabMechanic.type)
					{
						case GrabMechanic.GrabMechanicType.FixedJoint:
							joint = interactable.gameObject.AddComponent<FixedJoint>();
							joint.breakForce = interactable.grabMechanic.breakForce;
							joint.connectedBody = _attachComponent;
							if (interactable.grabMechanic.toggleKinematic)
								interactable.GetComponent<Rigidbody>().isKinematic = false;
							break;

						case GrabMechanic.GrabMechanicType.SpringJoint:
							var spring = interactable.gameObject.AddComponent<SpringJoint>();
							spring.spring = interactable.grabMechanic.springJointStrength;
							spring.damper = interactable.grabMechanic.springJointDamper;
							spring.anchor = interactable.transform.InverseTransformPoint(_attachComponent.position);
							joint = spring;
							joint.breakForce = interactable.grabMechanic.breakForce;
							joint.connectedBody = _attachComponent;
							break;

						case GrabMechanic.GrabMechanicType.ParentTransform:
							prevParent = interactable.transform.parent;
							interactable.transform.parent = transform;
							break;
					}

					grabbedObj = interactable;
					interactable.StartInteraction(controller);
				}
			}
		}

		public void DoGrabOff(object sender, ClickedEventArgs e)
		{
			if (grabbedObj)
			{
				// triggers a slight haptic pulse, drops grabbed object, and stop interaction
				var rigidbody = grabbedObj.GetComponent<Rigidbody>();
				controller.TriggerHapticPulse(1, 650);
				if (joint)
					Destroy(joint);
				else if (prevParent && grabbedObj.grabMechanic.restoreParent)
					grabbedObj.transform.parent = prevParent;
				else if (prevParent)
					grabbedObj.transform.parent = null;

				joint = null;
				prevParent = null;
				SetCollisions(grabbedObj, true);
				grabbedObj.StopInteraction(controller);

				if (grabbedObj.applyVelocity)
				{
					// applies the velocities of the controller to the object when thrown/dropped
					var device = SteamVR_Controller.Input((int) trackedObj.index);
					var origin = trackedObj.origin ? trackedObj.origin : trackedObj.transform.parent;
					var vel = origin ? origin.TransformVector(device.velocity) : device.velocity;
					var angVel = origin ? origin.TransformVector(device.angularVelocity) : device.angularVelocity;
					vel.Scale(grabbedObj.velocityMultiplier);
					rigidbody.velocity = vel;
					rigidbody.angularVelocity = angVel;

					rigidbody.maxAngularVelocity = rigidbody.angularVelocity.magnitude;
				}

				if (grabbedObj.grabMechanic.toggleKinematic)
					rigidbody.isKinematic = true;

				grabbedObj = null;
			}
		}

		void SetCollisions(Interactable interactable, bool state)
		{
			if (pauseCollisions)
			{
				foreach (Rigidbody body in interactable.GetComponentsInChildren<Rigidbody>())
					body.detectCollisions = state;

				interactable.GetComponent<Rigidbody>().detectCollisions = state;
			}
		}
		#endregion
	}
}