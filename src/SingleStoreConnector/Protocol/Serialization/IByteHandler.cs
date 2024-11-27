using SingleStoreConnector.Utilities;

namespace SingleStoreConnector.Protocol.Serialization;

internal interface IByteHandler : IDisposable
{
	/// <summary>
	/// The remaining timeout (in milliseconds) for the next I/O read. Use <see cref="Constants.InfiniteTimeout"/> to represent no (or, infinite) timeout.
	/// </summary>
	int RemainingTimeout { get; set; }

	/// <summary>
	/// Reads data from this byte handler.
	/// </summary>
	/// <param name="buffer">The buffer to read into.</param>
	/// <param name="ioBehavior">The <see cref="IOBehavior"/> to use when reading data.</param>
	/// <returns>A <see cref="ValueTask{Int32}"/> holding the number of bytes read. If reading failed, this will be zero.</returns>
	ValueTask<int> ReadBytesAsync(Memory<byte> buffer, IOBehavior ioBehavior);

	/// <summary>
	/// Writes data to this byte handler.
	/// </summary>
	/// <param name="data">The data to write.</param>
	/// <param name="ioBehavior">The <see cref="IOBehavior"/> to use when writing.</param>
	ValueTask WriteBytesAsync(ReadOnlyMemory<byte> data, IOBehavior ioBehavior);
}
