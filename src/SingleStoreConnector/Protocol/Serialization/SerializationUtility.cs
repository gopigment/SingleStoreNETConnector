namespace SingleStoreConnector.Protocol.Serialization;

internal static class SerializationUtility
{
	public static uint ReadUInt32(ReadOnlySpan<byte> span)
	{
		uint value = 0;
		for (int i = 0; i < span.Length; i++)
			value |= ((uint) span[i]) << (8 * i);
		return value;
	}

	public static void WriteUInt32(uint value, byte[] buffer, int offset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			buffer[offset + i] = (byte) (value & 0xFF);
			value >>= 8;
		}
	}
}
