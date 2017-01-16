using System;
using UnityEngine;

namespace Cracker
{

	[Serializable]
	public class OverridableProperty<T> : ISerializationCallbackReceiver
	{
		[Serializable]
		public enum Option
		{
			Inherit,
			Override
		}

		public Option option = Option.Inherit;
		public T value;

		[SerializeField, HideInInspector]
		private string valueSerialized;

		public OverridableProperty(T value)
		{
			this.value = value;
		}

		public static implicit operator OverridableProperty<T>(T value)
		{
			return new OverridableProperty<T>(value);
		}

		public void OnBeforeSerialize()
		{
			var type = value.GetType();
			if (type == typeof(int))
				valueSerialized = "i" + value.ToString();
			else if (type == typeof(float))
				valueSerialized = "f" + value.ToString();
			else if (type == typeof(bool))
				valueSerialized = "b" + value.ToString();
		}

		public void OnAfterDeserialize()
		{
			if (valueSerialized.Length <= 0)
				return;

			var serialized = valueSerialized.Substring(1);
			value = (T) Convert.ChangeType(serialized, typeof(T));
		}

	}
}