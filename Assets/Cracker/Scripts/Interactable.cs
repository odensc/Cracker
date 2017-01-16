using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;

namespace Cracker
{
	public struct InteractEventArgs
	{
		public Interactable interactable;
		public Controller controller;
	}

	public delegate void InteractEventHandler(object sender, InteractEventArgs e);

	[RequireComponent(typeof(Rigidbody))]
	public class Interactable : MonoBehaviour
	{
		#region Variables
		[Tooltip("Allows Controller_Interact_Grab to grab this object.")]
		public bool isGrabbable = true;
		[Tooltip("Highlight this object when selected.")]
		public bool highlightOnSelect = true;
		[Tooltip("The grab mechanic to use.")]
		public GrabMechanic grabMechanic = new GrabMechanic();
		[Tooltip("Apply the controller velocity when this object is ungrabbed.")]
		public bool applyVelocity = true;
		[Tooltip("The amount to multiply the applied velocity by.")]
		public Vector3 velocityMultiplier = Vector3.one;
		[Tooltip("Teleport the position/rotation of this object to the controller when grabbed.")]
		public bool updateTransform = true;
		[Tooltip("The point for the controller to attach to on this object. Update Transform must be enabled.")]
		public Transform attachPoint;

		[Tooltip("Pause collisions when grabbing an object.")]
		public OverridableProperty<bool> pauseCollisions = false;

		[System.NonSerialized]
		public Controller controller;

		public event InteractEventHandler InteractStart;
		public event InteractEventHandler InteractStop;
		public event InteractEventHandler Selected;
		public event InteractEventHandler Deselected;

		private Dictionary<string, Color[]> origColors;
		#endregion

		#region Events
		public virtual void OnInteractStart(InteractEventArgs e)
		{
			if (InteractStart != null) InteractStart(this, e);
		}

		public virtual void OnInteractStop(InteractEventArgs e)
		{
			if (InteractStop != null) InteractStop(this, e);
		}

		public virtual void OnSelected(InteractEventArgs e)
		{
			if (Selected != null) Selected(this, e);
		}

		public virtual void OnDeselected(InteractEventArgs e)
		{
			if (Deselected != null) Deselected(this, e);
		}
		#endregion

		#region Instance Methods
		public virtual void Start()
		{
			origColors = GetOriginalColors();
			// unselect on scene change
			SceneManager.activeSceneChanged += (Scene prev, Scene active) => Select(false, controller);

			// enable kinematic if toggle
			if (grabMechanic.toggleKinematic)
				GetComponent<Rigidbody>().isKinematic = true;
		}

		public virtual void OnDisable()
		{
			// unselect on disable
			Select(false, controller);
		}

		public virtual void OnJointBreak(float force)
		{
			var grab = controller.GetComponent<Controller_Interact_Grab>();
			var touch = controller.GetComponent<Controller_Interact_Touch>();
			grab.DoGrabOff(null, new ClickedEventArgs());
			if (!touch.GetTouchedObject()) Select(false, controller);
		}
		#endregion

		#region Public Methods
		/**
		 * <summary>
		 * Gets the provided GameObject or its parents Interactable component.
		 * </summary>
		 * <param name="go">The GameObject to check.</param>
		 * <returns>The Interactable component of <paramref name="go"/> or its parent. Returns null if there is no component on either or if it is disabled.</returns>
		 */
		public static Interactable GetInteractable(GameObject go)
		{
			var interact = go.GetComponent<Interactable>() ?? go.GetComponentInParent<Interactable>();
			return interact ? (interact.enabled ? interact : null) : null;
		}

		/**
		 * <returns>If this object is currently being interacted with.</returns>
		 */
		public bool IsInteracting()
		{
			return controller != null;
		}

		/**
		 * <summary>
		 * Selects this interactable.
		 * This will fire a Selected/Deselected event,
		 * and highlight/unhighlight this object, depending on the <paramref name="state"/> parameter.
		 * </summary>
		 * <param name="state">If true, selects this object. If false, deselects this object.</param>
		 * <param name="controller">The Controller that is interacting with this object.</param>
		 * <param name="highlightColor">The Color to highlight the object. If null, there will be no highlight color. Will only be used if highlightOnSelect is enabled.</param>
		 */
		public void Select(bool state, Controller controller, Color? highlightColor = null)
		{
			if (state && !IsInteracting())
			{
				if (highlightOnSelect)
				{
					ChangeColor(highlightColor);
				}

				InteractEventArgs e = new InteractEventArgs();
				e.interactable = this;
				e.controller = controller;
				OnSelected(e);
			}
			else
			{
				if (IsInteracting()) return;
				if (highlightOnSelect) ChangeColor(null);
				InteractEventArgs e = new InteractEventArgs();
				e.interactable = this;
				e.controller = controller;
				OnDeselected(e);
			}
		}

		/**
		 * <summary>
		 * Starts an interaction with this object.
		 * </summary>
		 * <param name="controller">The Controller that is going to interact with this object.</param>
		 */
		public void StartInteraction(Controller controller)
		{
			this.controller = controller;
			if (highlightOnSelect) ChangeColor(null);
			InteractEventArgs e = new InteractEventArgs();
			e.interactable = this;
			e.controller = controller;
			OnInteractStart(e);
		}

		/**
		 * <summary>
		 * Stops an interaction with this object.
		 * </summary>
		 * <param name="controller">The Controller that is going to stop interacting with this object.</param>
		 */
		public void StopInteraction(Controller controller)
		{
			if (controller != this.controller) return;

			var touch = controller.GetComponent<Controller_Interact_Touch>();
			if (highlightOnSelect && touch.GetTouchedObject())
				ChangeColor(touch.highlightColor);

			this.controller = null;
			InteractEventArgs e = new InteractEventArgs();
			e.interactable = this;
			e.controller = controller;
			OnInteractStop(e);
		}
		#endregion

		#region Highlighting
		private Renderer[] GetRenderers()
		{
			return GetComponents<Renderer>().Concat(GetComponentsInChildren<Renderer>()).ToArray();
		}

		private Dictionary<string, Color[]> GetOriginalColors()
		{
			var colors = new Dictionary<string, Color[]>();
			Renderer[] renderers = GetRenderers();
			// save each materials color for each renderer
			foreach (Renderer renderer in renderers)
			{
				colors[renderer.name] = new Color[renderer.materials.Length];
				for (int i = 0; i < renderer.materials.Length; i++)
					if (renderer.materials[i].HasProperty("_Color"))
						colors[renderer.name][i] = renderer.materials[i].color;
			}

			return colors;
		}

		private void ChangeColor(Color? color)
		{
			Renderer[] renderers = GetRenderers();
			foreach (Renderer renderer in renderers)
			{
				for (int i = 0; i < renderer.materials.Length; i++)
					if (renderer.materials[i].HasProperty("_Color") && origColors.ContainsKey(renderer.name))
						renderer.materials[i].color = color == null ? origColors[renderer.name][i] : color.Value;
			}
		}
		#endregion
	}

	[System.Serializable]
	public class GrabMechanic
	{
		[System.Serializable]
		public enum GrabMechanicType
		{
			FixedJoint,
			SpringJoint,
			ParentTransform,
			None
		}

		[Tooltip("The type of grab mechanic to use.")]
		public GrabMechanicType type = GrabMechanicType.FixedJoint;
		[Tooltip("The strength of the spring joint.")]
		public float springJointStrength = 0;
		[Tooltip("The damper of the spring joint.")]
		public float springJointDamper = 0;
		[Tooltip("The break force of the joint. Defaults to no breaking.")]
		public float breakForce = Mathf.Infinity;
		[Tooltip("Sets the rigidbody to kinematic while not grabbed, and not kinematic while grabbed.")]
		public bool toggleKinematic = false;
		[Tooltip("Should the previous parent be restored on drop?")]
		public bool restoreParent = true;
	}
}