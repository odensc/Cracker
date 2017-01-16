using System;
using UnityEngine;

namespace Cracker
{
	public struct PointerEventArgs
	{
		public Controller controller;
		public RaycastHit hit;
	}

	public delegate void PointerEventHandler(object sender, PointerEventArgs e);

	[RequireComponent(typeof(Controller))]
	public class Controller_BezierPointer : MonoBehaviour
	{
		#region Variables
		[Tooltip("The starting color of the curve.")]
		public Color startColor = Color.white;
		[Tooltip("The ending color of the curve.")]
		public Color endColor = Color.white;
		[Tooltip("The amount of points in the curve. The more points, the smoother the curve is.")]
		public int amountOfPoints = 20;
		[Tooltip("The starting width of the curve.")]
		public float startWidth = 0;
		[Tooltip("The ending width of the curve.")]
		public float endWidth = 0.1F;
		[Tooltip("The button to press to show the pointer.")]
		public ControllerButton showButton = ControllerButton.Trigger;

		public event PointerEventHandler PointerIn;
		public event PointerEventHandler PointerOut;

		private SteamVR_TrackedController trackedCon;
		private Controller controller;
		private BezierCurve curve;
		private RaycastHit lastHit;
		private bool pointerEnabled = false;
		#endregion

		#region Events
		public virtual void OnPointerIn(PointerEventArgs e)
		{
			if (PointerIn != null)
				PointerIn(this, e);
		}

		public virtual void OnPointerOut(PointerEventArgs e)
		{
			if (PointerOut != null)
				PointerOut(this, e);
		}
		#endregion

		#region Instance Methods
		void Start()
		{
			trackedCon = GetComponent<SteamVR_TrackedController>();
			controller = GetComponent<Controller>();
			curve = gameObject.AddComponent<BezierCurve>();
			curve.startColor = startColor;
			curve.endColor = endColor;
			curve.amountOfPoints = amountOfPoints;
			curve.startWidth = startWidth;
			curve.endWidth = endWidth;
			curve.startTransform = transform;

			showButton.AddClicked(trackedCon, ToggleEvent);
			showButton.AddUnclicked(trackedCon, ToggleEvent);
		}

		void Update()
		{
			GetComponent<LineRenderer>().enabled = pointerEnabled;
			if (!pointerEnabled) return;
			// if pointer left collider
			if (lastHit.transform != curve.hit.transform)
			{
				PointerEventArgs args = new PointerEventArgs();
				args.controller = controller;
				args.hit = lastHit;
				OnPointerOut(args);
			}

			// if pointer entered collider
			if (curve.hit.transform != null && lastHit.transform != curve.hit.transform)
			{
				PointerEventArgs args = new PointerEventArgs();
				args.controller = controller;
				args.hit = curve.hit;
				OnPointerIn(args);
			}

			lastHit = curve.hit;
		}

		void ToggleEvent(object sender, ClickedEventArgs e)
		{
			Toggle(!pointerEnabled);

			if (!pointerEnabled)
			{
				PointerEventArgs args = new PointerEventArgs();
				args.controller = controller;
				args.hit = lastHit;
				OnPointerOut(args);
			}
			else if (curve.hit.transform != null)
			{
				PointerEventArgs args = new PointerEventArgs();
				args.controller = controller;
				args.hit = curve.hit;
				OnPointerIn(args);
			}
		}
		#endregion

		#region Public Methods
		public void Toggle(bool state)
		{
			pointerEnabled = state;
		}
		#endregion
	}
}