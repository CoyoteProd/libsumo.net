using System.Collections.Generic;

namespace LibSumo.Net.lib.command
{
	/// <summary>
	/// @author  Alexander Bischof
	/// </summary>
	public class ChannelType
	{
		public static readonly ChannelType JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID = new ChannelType("JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID", InnerEnum.JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID, 10);
		public static readonly ChannelType JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID = new ChannelType("JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID", InnerEnum.JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID, 11);
		public static readonly ChannelType JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID = new ChannelType("JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID", InnerEnum.JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID, 13);
		public static readonly ChannelType JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID = new ChannelType("JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID", InnerEnum.JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID, (256 / 2) - 1);
		public static readonly ChannelType JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID = new ChannelType("JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID", InnerEnum.JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID, (256 / 2) - 2);
		public static readonly ChannelType JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID = new ChannelType("JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID", InnerEnum.JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID, (256 / 2) - 3);

		private static readonly IList<ChannelType> valueList = new List<ChannelType>();

		static ChannelType()
		{
			valueList.Add(JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID);
			valueList.Add(JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID);
			valueList.Add(JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID);
			valueList.Add(JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID);
			valueList.Add(JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID);
			valueList.Add(JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID);
		}

		public enum InnerEnum
		{
			JUMPINGSUMO_CONTROLLER_TO_DEVICE_NONACK_ID,
			JUMPINGSUMO_CONTROLLER_TO_DEVICE_ACK_ID,
			JUMPINGSUMO_CONTROLLER_TO_DEVICE_VIDEO_ACK_ID,
			JUMPINGSUMO_DEVICE_TO_CONTROLLER_NAVDATA_ID,
			JUMPINGSUMO_DEVICE_TO_CONTROLLER_EVENT_ID,
			JUMPINGSUMO_DEVICE_TO_CONTROLLER_VIDEO_DATA_ID
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;
		private readonly byte id;

		internal ChannelType(string name, InnerEnum innerEnum, int id)
		{
			this.id = (byte) id;
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public byte Id
		{
			get
			{
				return id;
			}
		}

		public static IList<ChannelType> values()
		{
			return valueList;
		}

		public InnerEnum InnerEnumValue()
		{
			return innerEnumValue;
		}

		public int ordinal()
		{
			return ordinalValue;
		}

		public override string ToString()
		{
			return nameValue;
		}

		public static ChannelType valueOf(string name)
		{
			foreach (ChannelType enumInstance in ChannelType.values())
			{
				if (enumInstance.nameValue == name)
				{
					return enumInstance;
				}
			}
			throw new System.ArgumentException(name);
		}
	}
}