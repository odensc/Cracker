using UnityEngine;
using System.Collections;

namespace Cracker
{
	[RequireComponent(typeof(Controller))]
	public class Controller_Interact_Touch : MonoBehaviour
	{
		#region Variables
		[Tooltip("The color to highlight touched objects with.")]
		public Color highlightColor = Color.clear;

		private GameObject touchedObject = null;
		#endregion

		#region Instance Methods
		void Start()
		{
			// TODO: More accurate collider
			// add a trigger collider for the top of the controller
			SphereCollider collider = gameObject.AddComponent<SphereCollider>();
			collider.radius = 0.05f;
			collider.center = new Vector3(0f, -0.035f, -0.01f);
			collider.isTrigger = true;
		}

		/**
		 * <summary>
		 * Checks if the provided GameObject has an Interactable component.
		 * </summary>
		 * <param name="go">The GameObject to check.</param>
		 * <returns>If <paramref name="go"/> has an Interactable component.</returns>
		 */
		bool IsInteractable(GameObject go)
		{
			return go.GetComponent<Interactable>() != null;
		}

		void OnTriggerStay(Collider collider)
		{
			if (touchedObject == null && IsInteractable(collider.gameObject))
			{
				collider.GetComponent<Interactable>().Select(true, GetComponent<Controller>(), highlightColor);
				touchedObject = collider.gameObject;
			}
		}

		void OnTriggerExit(Collider collider)
		{
			if (IsInteractable(collider.gameObject))
			{
				collider.GetComponent<Interactable>().Select(false, GetComponent<Controller>(), null);
			}

			touchedObject = null;
		}
		#endregion

		#region Public Methods
		public GameObject GetTouchedObject()
		{
			return touchedObject;
		}
		#endregion
	}
}