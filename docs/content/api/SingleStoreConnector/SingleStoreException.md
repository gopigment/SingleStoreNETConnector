# SingleStoreException class

[`SingleStoreException`](./SingleStoreException.md) is thrown when SingleStore Server returns an error code, or there is a communication error with the server.

```csharp
public sealed class SingleStoreException : DbException
```

## Public Members

| name | description |
| --- | --- |
| override [Data](SingleStoreException/Data.md) { get; } | Gets a collection of key/value pairs that provide additional information about the exception. |
| [ErrorCode](SingleStoreException/ErrorCode.md) { get; } | A [`SingleStoreErrorCode`](./SingleStoreErrorCode.md) value identifying the kind of error. |
| override [IsTransient](SingleStoreException/IsTransient.md) { get; } | Returns `true` if this exception could indicate a transient error condition (that could succeed if retried); otherwise, `false`. |
| [Number](SingleStoreException/Number.md) { get; } | A [`SingleStoreErrorCode`](./SingleStoreErrorCode.md) value identifying the kind of error. Prefer to use the [`ErrorCode`](./SingleStoreException/ErrorCode.md) property. |
| override [SqlState](SingleStoreException/SqlState.md) { get; } | A `SQLSTATE` code identifying the kind of error. |
| override [GetObjectData](SingleStoreException/GetObjectData.md)(…) | Sets the SerializationInfo with information about the exception. |

## See Also

* namespace [SingleStoreConnector](../SingleStoreConnector.md)

<!-- DO NOT EDIT: generated by xmldocmd for SingleStoreConnector.dll -->