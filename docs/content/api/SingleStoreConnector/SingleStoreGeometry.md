# SingleStoreGeometry class

Represents SingleStore's internal GEOMETRY format: https://dev.mysql.com/doc/refman/8.0/en/gis-data-formats.html#gis-internal-format

```csharp
public sealed class SingleStoreGeometry
```

## Public Members

| name | description |
| --- | --- |
| static [FromMySql](SingleStoreGeometry/FromMySql.md)(…) | Constructs a [`SingleStoreGeometry`](./SingleStoreGeometry.md) from SingleStore's internal format. |
| static [FromWkb](SingleStoreGeometry/FromWkb.md)(…) | Constructs a [`SingleStoreGeometry`](./SingleStoreGeometry.md) from a SRID and Well-known Binary bytes. |
| [SRID](SingleStoreGeometry/SRID.md) { get; } | The Spatial Reference System ID of this geometry. |
| [Value](SingleStoreGeometry/Value.md) { get; } | The internal SingleStore form of this geometry. |
| [WKB](SingleStoreGeometry/WKB.md) { get; } | The Well-known Binary serialization of this geometry. |

## See Also

* namespace [SingleStoreConnector](../SingleStoreConnector.md)

<!-- DO NOT EDIT: generated by xmldocmd for SingleStoreConnector.dll -->