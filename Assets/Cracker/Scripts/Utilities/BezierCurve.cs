using UnityEngine;

namespace Cracker
{
	/**
	 * <summary>
	 * Generates a bezier curve from multiple points.
	 * </summary> 
	 * <remarks>
	 * Reference: https://en.wikibooks.org/wiki/Cg_Programming/Unity/B%C3%A9zier_Curves
	 * </remarks>
	 */
	public class BezierCurve : MonoBehaviour
	{
		#region Variables
		[Tooltip("The starting transform of the curve.")]
		public Transform startTransform;
		[Tooltip("The starting color of the curve.")]
		public Color startColor = Color.white;
		[Tooltip("The ending color of the curve.")]
		public Color endColor = Color.white;
		[Tooltip("The material of the curve.")]
		public Material material;
		[Tooltip("The amount of points in the curve. The more points, the smoother the curve is.")]
		public int amountOfPoints = 20;
		[Tooltip("The starting width of the curve.")]
		public float startWidth = 0;
		[Tooltip("The ending width of the curve.")]
		public float endWidth = 0.1F;
		[System.NonSerialized]
		public RaycastHit hit;

		private LineRenderer lineRenderer;
		private float defaultDistance = 100;
		#endregion

		#region Instance Methods
		void Start()
		{
			// find material
			material = material ?? new Material(Shader.Find("Unlit/Color"));

			// add and initialize a line renderer
			lineRenderer = gameObject.AddComponent<LineRenderer>();
			lineRenderer.useWorldSpace = true;
			lineRenderer.material = material;
		}

		void FixedUpdate()
		{
			// update line renderer
			lineRenderer.SetColors(startColor, endColor);
			lineRenderer.SetWidth(startWidth, endWidth);
			lineRenderer.SetVertexCount(amountOfPoints);

			var points = CalculatePoints();
			// render the curve
			for (int i = 0; i < amountOfPoints; i++) 
			{
				float t = i / (amountOfPoints - 1F);
				Vector3 position = (1F - t) * (1F - t) * points[0]
				   + 2F * (1F - t) * t * points[1]
				   + t * t * points[2];
				lineRenderer.SetPosition(i, position);
			}
		}

		Vector3[] CalculatePoints()
		{
			Vector3[] points = new Vector3[3];
			RaycastHit hit;
			points[0] = startTransform.position;
			if (Physics.Raycast(startTransform.position, startTransform.forward, out hit))
			{
				points[1] = Vector3.Lerp(hit.point, startTransform.position, 0.5F);
				points[2] = hit.point;
			}
			else
			{
				points[1] = startTransform.position + (startTransform.forward * defaultDistance);
				points[2] = points[1];
			}

			this.hit = hit;
			return points;
		}
		#endregion
	}
}