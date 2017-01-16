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

		void OnTriggerStay(Collider collider)
		{
			if (touchedObject == null && Interactable.GetInteractable(collider.gameObject) != null)
			{
				Interactable.GetInteractable(collider.gameObject).Select(true, GetComponent<Controller>(), highlightColor);
				touchedObject = collider.gameObject;
			}
		}

		void OnTriggerExit(Collider collider)
		{
			if (Interactable.GetInteractable(collider.gameObject) != null)
			{
				Interactable.GetInteractable(collider.gameObject).Select(false, GetComponent<Controller>(), null);
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