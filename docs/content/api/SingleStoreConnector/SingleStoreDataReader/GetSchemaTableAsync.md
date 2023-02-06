# SingleStoreDataReader.GetSchemaTableAsync method

Returns a DataTable that contains metadata about the columns in the result set.

```csharp
public override Task<DataTable?> GetSchemaTableAsync(CancellationToken cancellationToken = default)
```

| parameter | description |
| --- | --- |
| cancellationToken | A token to cancel the operation. |

## Return Value

A DataTable containing metadata about the columns in the result set.

## Remarks

This method runs synchronously; prefer to call [`GetSchemaTable`](./GetSchemaTable.md) to avoid the overhead of allocating an unnecessary `Task`.

## See Also

* class [SingleStoreDataReader](../SingleStoreDataReader.md)
* namespace [SingleStoreConnector](../../SingleStoreConnector.md)

<!-- DO NOT EDIT: generated by xmldocmd for SingleStoreConnector.dll -->