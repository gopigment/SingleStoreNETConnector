using System.Buffers.Binary;
using SingleStoreConnector.Protocol;
using SingleStoreConnector.Protocol.Serialization;

namespace SingleStoreConnector.Core;

internal sealed class BatchedCommandPayloadCreator : ICommandPayloadCreator
{
	public static ICommandPayloadCreator Instance { get; } = new BatchedCommandPayloadCreator();

	public bool WriteQueryCommand(ref CommandListPosition commandListPosition, IDictionary<string, CachedProcedure?> cachedProcedures, ByteBufferWriter writer, bool appendSemicolon)
	{
		writer.Write((byte) CommandKind.Multi);
		bool? firstResult = default;
		bool wroteCommand;
		ReadOnlySpan<byte> padding = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0 };
		do
		{
			// save room for command length
			var position = writer.Position;
			writer.Write(padding);

			wroteCommand = SingleCommandPayloadCreator.Instance.WriteQueryCommand(ref commandListPosition, cachedProcedures, writer, appendSemicolon);
			firstResult ??= wroteCommand;

			// write command length
			var commandLength = writer.Position - position - padding.Length;
			var span = writer.ArraySegment.AsSpan(position);
			span[0] = 0xFE;
			BinaryPrimitives.WriteUInt64LittleEndian(span[1..], (ulong) commandLength);
		} while (wroteCommand);

		// remove the padding that was saved for the final command (which wasn't written)
		writer.TrimEnd(padding.Length);
		return firstResult.Value;
	}
}
