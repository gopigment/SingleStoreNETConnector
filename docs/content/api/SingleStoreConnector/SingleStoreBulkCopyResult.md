# SingleStoreBulkCopyResult class

Represents the result of a [`SingleStoreBulkCopy`](./SingleStoreBulkCopy.md) operation.

```csharp
public sealed class SingleStoreBulkCopyResult
```

## Public Members

| name | description |
| --- | --- |
| [RowsInserted](SingleStoreBulkCopyResult/RowsInserted.md) { get; } | The number of rows that were inserted during the bulk copy operation. |
| [Warnings](SingleStoreBulkCopyResult/Warnings.md) { get; } | The warnings, if any. Users of [`SingleStoreBulkCopy`](./SingleStoreBulkCopy.md) should check that this collection is empty to avoid potential data loss from failed data type conversions. |

## See Also

* namespace [SingleStoreConnector](../SingleStoreConnector.md)

<!-- DO NOT EDIT: generated by xmldocmd for SingleStoreConnector.dll -->