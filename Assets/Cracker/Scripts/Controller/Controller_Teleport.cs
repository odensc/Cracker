using UnityEngine;

namespace Cracker
{
	[RequireComponent(typeof(Controller_BezierPointer))]
	public class Controller_Teleport : MonoBehaviour
	{
		public enum TeleportType
		{
			TeleportTypeUseTerrain,
			TeleportTypeUseCollider,
			TeleportTypeUseZeroY
		}

		#region Variables
		public TeleportType teleportType = TeleportType.TeleportTypeUseZeroY;

		private Transform reference
		{
			get
			{
				var top = SteamVR_Render.Top();
				return (top != null) ? top.origin : null;
			}
		}
		#endregion Variables

		#region Instance Methods
		void Start()
		{
			var button = GetComponent<Controller_BezierPointer>().showButton;
			button.AddUnclicked(GetComponent<SteamVR_TrackedController>(), OnUnclicked);

			if (teleportType == TeleportType.TeleportTypeUseTerrain)
			{
				// Start the player at the level of the terrain
				var t = reference;
				if (t != null)
					t.position = new Vector3(t.position.x, Terrain.activeTerrain.SampleHeight(t.position), t.position.z);
			}
		}

		void OnUnclicked(object sender, ClickedEventArgs e)
		{
			Teleport();
		}

		void Teleport()
		{
			var t = reference;
			if (t == null)
				return;

			float refY = t.position.y;

			Plane plane = new Plane(Vector3.up, -refY);
			Ray ray = new Ray(this.transform.position, transform.forward);

			bool hasGroundTarget = false;
			float dist = 0f;
			if (teleportType == TeleportType.TeleportTypeUseTerrain)
			{
				RaycastHit hitInfo;
				TerrainCollider tc = Terrain.activeTerrain.GetComponent<TerrainCollider>();
				hasGroundTarget = tc.Raycast(ray, out hitInfo, 100);
				dist = hitInfo.distance;
			}
			else if (teleportType == TeleportType.TeleportTypeUseCollider)
			{
				RaycastHit hitInfo;
				hasGroundTarget = Physics.Raycast(ray, out hitInfo);
				dist = hitInfo.distance;
			}
			else
			{
				hasGroundTarget = plane.Raycast(ray, out dist);
			}

			if (hasGroundTarget)
			{
				Vector3 headPosOnGround = new Vector3(SteamVR_Render.Top().head.localPosition.x, 0, SteamVR_Render.Top().head.localPosition.z);
				t.position = ray.origin + ray.direction * dist - new Vector3(t.GetChild(0).localPosition.x, 0, t.GetChild(0).localPosition.z) - headPosOnGround;
			}
		}
		#endregion
	}
}