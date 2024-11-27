namespace SingleStoreConnector.Core;

internal sealed class CachedParameter
{
	public CachedParameter(int ordinalPosition, string? mode, string name, string dataType, bool unsigned, int length)
	{
		Position = ordinalPosition;
		if (Position == 0)
			Direction = ParameterDirection.ReturnValue;
		else if (string.Equals(mode, "in", StringComparison.OrdinalIgnoreCase))
			Direction = ParameterDirection.Input;
		else if (string.Equals(mode, "inout", StringComparison.OrdinalIgnoreCase))
			Direction = ParameterDirection.InputOutput;
		else if (string.Equals(mode, "out", StringComparison.OrdinalIgnoreCase))
			Direction = ParameterDirection.Output;
		Name = name;
		SingleStoreDbType = TypeMapper.Instance.GetSingleStoreDbType(dataType, unsigned, length);
		Length = length;
	}

	public int Position { get; }
	public ParameterDirection Direction { get; }
	public string Name { get; }
	public SingleStoreDbType SingleStoreDbType { get; }
	public int Length { get; }
}
