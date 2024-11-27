---
lastmod: 2021-12-24
date: 2016-10-16
menu:
  main:
    parent: tutorials
title: Migrating from Connector/NET
weight: 20
---

Migrating from Connector/NET
============================

### Namespace

SingleStoreConnector supports the same core API as SingleStore Connector/NET, but the classes are in a different
namespace. Change `using SingleStore.Data.SingleStoreClient;` to `using SingleStoreConnector;`.

### DbProviderFactories

The `SingleStoreClientFactory` type is named `SingleStoreConnectorFactory` in SingleStoreConnector.

In a .NET Framework application, make the following `app.config` change to register SingleStoreConnector instead of SingleStore.Data.

```xml
<system.data>
  <DbProviderFactories>
    <!-- REMOVE THIS -->
    <!-- add name="SingleStore Data Provider" invariant="SingleStore.Data.SingleStoreClient" description=".Net Framework Data Provider for SingleStore" type="SingleStore.Data.SingleStoreClient.SingleStoreClientFactory, SingleStore.Data, Version=8.0.20.0, Culture=neutral, PublicKeyToken=c5687fc88969c44d" / -->

    <!-- ADD THIS -->
    <add name="SingleStoreConnector" invariant="SingleStoreConnector" description="SingleStore Connector for .NET" type="SingleStoreConnector.SingleStoreConnectorFactory, SingleStoreConnector, Culture=neutral, PublicKeyToken=d33d3e53aa5f8c92" />
  </DbProviderFactories>
</system.data>
```

### Connection String Differences

SingleStoreConnector has some different default connection string options:

<table class="table table-striped table-hover">
  <thead>
    <th style="width:20%">Option</th>
    <th style="width:20%">SingleStoreConnector</th>
    <th style="width:20%">Oracle’s Connector/NET</th>
    <th style="width:40%">Notes</th>
  </thead>
  <tr>
    <td><code>CharacterSet</code>, <code>CharSet</code></td>
    <td>Ignored; <code>utf8mb4</code> is always used</td>
    <td>(server-defined)</td>
    <td>SingleStoreConnector always uses <code>utf8mb4</code> to send and receive strings from SingleStore Server. This option may be specified (for backwards compatibility) but it will be ignored.</td>
  </tr>
  <tr>
    <td><code>ConnectionReset</code></td>
    <td>Default is <code>true</code></td>
    <td>Default is <code>false</code></td>
    <td>SingleStoreConnector always resets pooled connections by default so that the connection is in a known state. This fixes <a href="https://bugs.mysql.com/bug.php?id=77421">SingleStore Bug 77421</a>.</td>
  </tr>
  <tr>
    <td><code>IgnoreCommandTransaction</code></td>
    <td>Default is <code>false</code></td>
    <td>(not configurable, effective default is <code>true</code>)</td>
    <td>See remarks under SingleStoreCommand below.</td>
  </tr>
  <tr>
    <td><code>IgnorePrepare</code></td>
    <td>Default is <code>false</code></td>
    <td><code>true</code> for ≤ 8.0.22; <code>false</code> for ≥ 8.0.23</td>
    <td>This is a change if migrating from an older version of Connector/NET.</td>
  </tr>
  <tr>
    <td><code>LoadBalance</code></td>
    <td>Default is <code>RoundRobin</code></td>
    <td>(not configurable, effective default is <code>FailOver</code>)</td>
    <td>Connector/NET currently has <a href="https://bugs.mysql.com/bug.php?id=81650" title="SingleStore bug #81650">a bug</a> that prevents multiple host names being used.</td>
  </tr>
  <tr>
    <td><code>ServerRSAPublicKeyFile</code></td>
    <td>(no default)</td>
    <td>(not configurable)</td>
    <td>Specify a file containing the server’s RSA public key to allow <code>sha256_password</code> authentication over an insecure connection.</td>
  </tr>
</table>

Connector/NET uses `CertificateFile` to specify the client’s private key, unless `SslCert` and `SslKey` are specified, in which case
it is used to specify the server’s CA certificate file; `SslCa` is just an alias for this option. SingleStoreConnector always uses `CertificateFile`
for the client’s private key (in PFX format); `SslCa` (aka `CACertificateFile`) is a separate option to specify the server’s CA certificate.

Some connection string options that are supported in Connector/NET are not supported in SingleStoreConnector. For a full list of options that are
supported in SingleStoreConnector, see the Connection Options.

### Async

Connector/NET implements the standard ADO.NET async methods, and adds some new ones (e.g., `SingleStoreConnection.BeginTransactionAsync`,
`SingleStoreDataAdapter.FillAsync`) that don't exist in ADO.NET. None of these methods have an asynchronous implementation,
but all execute synchronously then return a completed `Task`. This is a [longstanding known bug](https://bugs.mysql.com/bug.php?id=70111)
in Connector/NET.

Because the Connector/NET methods aren't actually asynchronous, porting client code to SingleStoreConnector (which is asynchronous)
can expose bugs that only occur when an async method completes asynchronously and resumes the `await`-ing code
on a background thread. To avoid deadlocks, make sure to [never block on async code](https://blog.stephencleary.com/2012/07/dont-block-on-async-code.html) (e.g., with `.Result`), use async all the way, use `ConfigureAwait` correctly,
and follow the [best practices in async programming](https://msdn.microsoft.com/en-us/magazine/jj991977.aspx).

### Implicit Conversions

Connector/NET allows `SingleStoreDataReader.GetString()` to be called on many non-textual columns, and will implicitly
convert the value to a `string` (using the current locale). This is a frequent source of locale-dependent bugs, so
SingleStoreConnector follows typical ADO.NET practice (e.g., SqlClient, npgsql) and disallows this (by throwing an `InvalidCastException`).

To fix this, use the accessor method (e.g., `GetInt32`, `GetDouble`) that matches the column type, or perform an
explicit conversion to `string` by calling `GetValue(x).ToString()` (optionally supplying the right `CultureInfo` to use
for formatting).

### TransactionScope

SingleStoreConnector adds full distributed transaction support (for client code using [`System.Transactions.Transaction`](https://docs.microsoft.com/en-us/dotnet/api/system.transactions.transaction
)),
while Connector/NET uses regular database transactions. As a result, code that uses `TransactionScope` or `SingleStoreConnection.EnlistTransaction`
may execute differently with SingleStoreConnector. To get Connector/NET-compatible behavior, set
`UseXaTransactions=false` in your connection string.

### SingleStoreConnection

Connector/NET allows a `SingleStoreConnection` object to be reused after it has been disposed. SingleStoreConnector requires a new `SingleStoreConnection`
object to be created. See [#331](https://github.com/mysql-net/SingleStoreConnector/issues/331) for more details.

The return value of `SingleStoreConnection.BeginTransactionAsync` has changed from `Task<SingleStoreTransaction>` to
`ValueTask<SingleStoreTransaction>` to match the [standard API in .NET Core 3.0](https://github.com/dotnet/corefx/issues/35012).
(This method does always perform I/O, so `ValueTask` is not an optimization for SingleStoreConnector.)

### SingleStoreConnectionStringBuilder

All `string` properties on `SingleStoreConnectionStringBuilder` will return the empty string (instead of `null`) if the property isn't set.

### SingleStoreCommand

Connector/NET allows a command to be executed even when `SingleStoreCommand.Transaction` references a commited, rolled back, or
disposed `SingleStoreTransaction`. SingleStoreConnector will throw an `InvalidOperationException` if the `SingleStoreCommand.Transaction`
property doesn’t reference the active transaction. This fixes <a href="https://bugs.mysql.com/bug.php?id=88611">SingleStore Bug 88611</a>.
To disable this strict validation, set <code>IgnoreCommandTransaction=true</code>
in the connection string.

If `SingleStoreCommand.CommandType` is `CommandType.StoredProcedure`, the stored procedure name assigned to `SingleStoreCommand.CommandText` must have any special characters escaped or quoted. Connector/NET will automatically quote some characters (such as spaces); SingleStoreConnector leaves this up to the developer.

### SingleStoreDataAdapter

Connector/NET provides `SingleStoreDataAdapter.FillAsync`, `FillSchemaAsync`, and `UpdateAsync` methods, but these methods
have a synchronous implementation. SingleStoreConnector only adds “Async” methods when they can be implemented asynchronously.
This functionality depends on [dotnet/corefx#20658](https://github.com/dotnet/corefx/issues/20658) being implemented first.
To migrate code, change it to call the synchronous methods instead.

### SingleStoreGeometry

The Connector/NET `SingleStoreGeometry` type assumes that the geometry can only be a simple point. SingleStoreConnector
removes most of the API that is based on those assumptions.

To avoid ambiguity, there are two different factory methods for constructing a `SingleStoreGeometry`. Use the static factory method `SingleStoreGeometry.FromMySql` (if you have a byte array containing SingleStore's internal format), or `FromWkb` if you have
Well-known Binary bytes.

### SingleStoreInfoMessageEventArgs

The `SingleStoreError[] SingleStoreInfoMessageEventArgs.errors` property has changed to `IReadOnlyList<SingleStoreError> SingleStoreInfoMessageEventArgs.Errors`.

### SingleStoreParameter

Connector/NET will automatically convert unknown `SingleStoreParameter.Value` values to a `string` by calling `ToString()`,
then convert that to bytes by calling `Encoding.GetBytes()` using the packet’s encoding. This is error-prone and
can introduce culture-sensitive conversions.

SingleStoreConnector requires all parameter values to be of a known, supported type.

### SingleStoreParameterCollection

Connector/NET will assign the names `@Parameter1`, `@Parameter2`, etc. to unnamed `SingleStoreParameter` objects that are
added to the `SingleStoreCommand.Parameters` parameter collection. These generated names may be used in the SQL assigned to
`SingleStoreCommand.CommandText`. SingleStoreConnector requires all `SingleStoreParameter` objects to be explicitly given a name,
or used only as positional parameters if they’re unnamed.

### Exceptions

For consistency with other ADO.NET providers, SingleStoreConnector will throw `InvalidOperationException` (instead of `SingleStoreException`)
for various precondition checks that indicate misuse of the API (and not a problem related to SingleStore Server).

### Fixed Bugs

The following bugs in Connector/NET are fixed by switching to SingleStoreConnector. (~~Strikethrough~~ indicates bugs that have since been fixed in a newer version of Connector/NET, but were fixed first in SingleStoreConnector.)

* [#14115](https://bugs.mysql.com/bug.php?id=14115): Compound statements are not supported by `SingleStoreCommand.Prepare`
* [#37283](https://bugs.mysql.com/bug.php?id=37283), [#70587](https://bugs.mysql.com/bug.php?id=70587): Distributed transactions are not supported
* [#50773](https://bugs.mysql.com/bug.php?id=50773): Can’t use multiple connections within one TransactionScope
* [#61477](https://bugs.mysql.com/bug.php?id=61477): `ColumnOrdinal` in schema table is 1-based
* [#66476](https://bugs.mysql.com/bug.php?id=66476): Connection pool uses queue instead of stack
* [#70111](https://bugs.mysql.com/bug.php?id=70111): `Async` methods execute synchronously
* ~~[#70686](https://bugs.mysql.com/bug.php?id=70686): `TIME(3)` and `TIME(6)` fields serialize milliseconds incorrectly~~
* [#72494](https://bugs.mysql.com/bug.php?id=72494), [#83330](https://bugs.mysql.com/bug.php?id=83330): EndOfStreamException inserting large blob with UseCompression=True
* [#73610](https://bugs.mysql.com/bug.php?id=73610): Invalid password exception has wrong number
* [#73788](https://bugs.mysql.com/bug.php?id=73788): Can’t use `DateTimeOffset`
* ~~[#75604](https://bugs.mysql.com/bug.php?id=75604): Crash after 29.4 days of uptime~~
* [#75917](https://bugs.mysql.com/bug.php?id=75917), [#76597](https://bugs.mysql.com/bug.php?id=76597), [#77691](https://bugs.mysql.com/bug.php?id=77691), [#78650](https://bugs.mysql.com/bug.php?id=78650), [#78919](https://bugs.mysql.com/bug.php?id=78919), [#80921](https://bugs.mysql.com/bug.php?id=80921), [#82136](https://bugs.mysql.com/bug.php?id=82136): “Reading from the stream has failed” when connecting to a server
* [#77421](https://bugs.mysql.com/bug.php?id=77421): Connection is not reset when pulled from the connection pool
* [#78426](https://bugs.mysql.com/bug.php?id=78426): Unknown database exception has wrong number
* [#78760](https://bugs.mysql.com/bug.php?id=78760): Error when using tabs and newlines in SQL statements
* ~~[#78917](https://bugs.mysql.com/bug.php?id=78917), [#79196](https://bugs.mysql.com/bug.php?id=79196), [#82292](https://bugs.mysql.com/bug.php?id=82292), [#89040](https://bugs.mysql.com/bug.php?id=89040): `TINYINT(1)` values start being returned as `sbyte` after `NULL`~~
* ~~[#80030](https://bugs.mysql.com/bug.php?id=80030): Slow to connect with pooling disabled~~
* ~~[#81650](https://bugs.mysql.com/bug.php?id=81650), [#88962](https://bugs.mysql.com/bug.php?id=88962): `Server` connection string option may now contain multiple, comma separated hosts that will be tried in order until a connection succeeds~~
* [#83229](https://bugs.mysql.com/bug.php?id=83329): “Unknown command” exception inserting large blob with UseCompression=True
* ~~[#83649](https://bugs.mysql.com/bug.php?id=83649): Connection cannot be made using IPv6~~
* ~~[#84220](https://bugs.mysql.com/bug.php?id=84220): Cannot call a stored procedure with `.` in its name~~
* ~~[#84701](https://bugs.mysql.com/bug.php?id=84701): Can’t create a parameter using a 64-bit enum with a value greater than int.MaxValue~~
* [#85185](https://bugs.mysql.com/bug.php?id=85185): `ConnectionReset=True` does not preserve connection charset
* ~~[#86263](https://bugs.mysql.com/bug.php?id=86263): Transaction isolation level affects all transactions in session~~
* ~~[#87307](https://bugs.mysql.com/bug.php?id=87307): NextResult hangs instead of timing out~~
* ~~[#87316](https://bugs.mysql.com/bug.php?id=87316): SingleStoreCommand.CommandTimeout can be set to a negative value~~
* ~~[#87868](https://bugs.mysql.com/bug.php?id=87868): `ColumnSize` in schema table is incorrect for `CHAR(36)` and `BLOB` columns~~
* ~~[#87876](https://bugs.mysql.com/bug.php?id=87876): `IsLong` is schema table is incorrect for `LONGTEXT` and `LONGBLOB` columns~~
* ~~[#88058](https://bugs.mysql.com/bug.php?id=88058): `decimal(n, 0)` has wrong `NumericPrecision`~~
* [#88124](https://bugs.mysql.com/bug.php?id=88124): CommandTimeout isn’t reset when calling Read/NextResult
* ~~[#88472](https://bugs.mysql.com/bug.php?id=88472): `TINYINT(1)` is not returned as `bool` if `SingleStoreCommand.Prepare` is called~~
* [#88611](https://bugs.mysql.com/bug.php?id=88611): `SingleStoreCommand` can be executed even if it has “wrong” transaction
* ~~[#88660](https://bugs.mysql.com/bug.php?id=88660): `SingleStoreClientFactory.Instance.CreateDataAdapter()` and `CreateCommandBuilder` return `null`~~
* [#89085](https://bugs.mysql.com/bug.php?id=89085): `SingleStoreConnection.Database` not updated after `USE database;`
* ~~[#89159](https://bugs.mysql.com/bug.php?id=89159), [#97242](https://bugs.mysql.com/bug.php?id=97242): `SingleStoreDataReader` cannot outlive `SingleStoreCommand`~~
* [#89335](https://bugs.mysql.com/bug.php?id=89335): `SingleStoreCommandBuilder.DeriveParameters` fails for `JSON` type
* ~~[#89639](https://bugs.mysql.com/bug.php?id=89639): `ReservedWords` schema contains incorrect data~~
* ~~[#90086](https://bugs.mysql.com/bug.php?id=90086): `SingleStoreDataReader` is closed by an unrelated command disposal~~
* [#91123](https://bugs.mysql.com/bug.php?id=91123): Database names are case-sensitive when calling a stored procedure
* [#91199](https://bugs.mysql.com/bug.php?id=91199): Can't insert `SingleStoreDateTime` values
* ~~[#91751](https://bugs.mysql.com/bug.php?id=91751): `YEAR` column retrieved incorrectly with prepared command~~
* ~~[#91752](https://bugs.mysql.com/bug.php?id=91752): `00:00:00` is converted to `NULL` with prepared command~~
* [#91753](https://bugs.mysql.com/bug.php?id=91753): Unnamed parameter not supported by `SingleStoreCommand.Prepare`
* [#91754](https://bugs.mysql.com/bug.php?id=91754): Inserting 16MiB `BLOB` shifts it by four bytes when prepared
* ~~[#91770](https://bugs.mysql.com/bug.php?id=91770): `TIME(n)` column loses microseconds with prepared command~~
* [#92367](https://bugs.mysql.com/bug.php?id=92367): `SingleStoreDataReader.GetDateTime` and `GetValue` return inconsistent values
* [#92465](https://bugs.mysql.com/bug.php?id=92465): “There is already an open DataReader” `SingleStoreException` thrown from `TransactionScope.Dispose`
* [#92734](https://bugs.mysql.com/bug.php?id=92734): `SingleStoreParameter.Clone` doesn't copy all property values
* [#92789](https://bugs.mysql.com/bug.php?id=92789): Illegal connection attributes written for non-ASCII values
* ~~[#92912](https://bugs.mysql.com/bug.php?id=92912): `SingleStoreDbType.LongText` values encoded incorrectly with prepared statements~~
* ~~[#92982](https://bugs.mysql.com/bug.php?id=92982), [#93399](https://bugs.mysql.com/bug.php?id=93399)~~: `FormatException` thrown when connecting to SingleStore Server 8.0.13~~
* [#93047](https://bugs.mysql.com/bug.php?id=93047): `SingleStoreDataAdapter` throws timeout exception when an error occurs
* ~~[#93202](https://bugs.mysql.com/bug.php?id=93202): Connector runs `SHOW VARIABLES` when connection is made~~
* [#93220](https://bugs.mysql.com/bug.php?id=93220): Can’t call FUNCTION when parameter name contains parentheses
* [#93370](https://bugs.mysql.com/bug.php?id=93370): `SingleStoreParameterCollection.Add` precondition check isn't consistent
* [#93374](https://bugs.mysql.com/bug.php?id=93374): `SingleStoreDataReader.GetStream` throws `IndexOutOfRangeException`
* [#93825](https://bugs.mysql.com/bug.php?id=93825): `SingleStoreException` loses data when serialized
* ~~[#94075](https://bugs.mysql.com/bug.php?id=94075): `SingleStoreCommand.Cancel` throws exception~~
* [#94760](https://bugs.mysql.com/bug.php?id=94760): `SingleStoreConnection.OpenAsync(CancellationToken)` doesn’t respect cancellation token
* [#95348](https://bugs.mysql.com/bug.php?id=95348): Inefficient query when executing stored procedures
* [#95436](https://bugs.mysql.com/bug.php?id=95436): Client doesn't authenticate with PEM certificate
* ~~[#95984](https://bugs.mysql.com/bug.php?id=95984): “Incorrect arguments to mysqld_stmt_execute” using prepared statement with `SingleStoreDbType.JSON`~~
* [#95986](https://bugs.mysql.com/bug.php?id=95986): “Incorrect integer value” using prepared statement with `SingleStoreDbType.Int24`
* ~~[#96355](https://bugs.mysql.com/bug.php?id=96355), [#96614](https://bugs.mysql.com/bug.php?id=96614): `Could not load file or assembly 'Renci.SshNet'` when opening connection~~
* ~~[#96498](https://bugs.mysql.com/bug.php?id=96498): `WHERE` clause using `SingleStoreGeometry` as parameter finds no rows~~
* ~~[#96499](https://bugs.mysql.com/bug.php?id=96499): `SingleStoreException` when inserting a `SingleStoreGeometry` value~~
* [#96500](https://bugs.mysql.com/bug.php?id=96500): `SingleStoreDataReader.GetFieldValue<SingleStoreGeometry>` throws `InvalidCastException`
* [#96636](https://bugs.mysql.com/bug.php?id=96636): `SingleStoreConnection.Open()` slow under load when using SSL
* [#96717](https://bugs.mysql.com/bug.php?id=96717): Not compatible with SingleStore Server 5.0
* [#97061](https://bugs.mysql.com/bug.php?id=97061): `SingleStoreCommand.LastInsertedId` returns 0 after executing multiple statements
* [#97067](https://bugs.mysql.com/bug.php?id=97067): Aggregate functions on BIT(n) columns return wrong result
* ~~[#97300](https://bugs.mysql.com/bug.php?id=97300): `GetSchemaTable()` returns table for stored procedure with output parameters~~
* ~~[#97448](https://bugs.mysql.com/bug.php?id=97448): Connecting fails if more than one IP is found in DNS for a named host~~
* [#97473](https://bugs.mysql.com/bug.php?id=97473): `SingleStoreConnection.Clone` discloses connection password
* [#97738](https://bugs.mysql.com/bug.php?id=97738): Cannot use PEM files when account uses `require subject`
* [#97872](https://bugs.mysql.com/bug.php?id=97872): `KeepAlive` in connection string throws exception on .NET Core
* ~~[#98322](https://bugs.mysql.com/bug.php?id=98322): `new SingleStoreConnection(null)` throws `NullReferenceException`~~
* [#99091](https://bugs.mysql.com/bug.php?id=99091): Unexpected return value getting integer for `TINYINT(1)` column
* ~~[#99793](https://bugs.mysql.com/bug.php?id=99793): Prepared stored procedure command doesn't verify parameter types~~
* ~~[#100159](https://bugs.mysql.com/bug.php?id=100159): SQL with DateTime parameter returns String value~~
* ~~[#100208](https://bugs.mysql.com/bug.php?id=100208): `GetSchema("Procedures")` returns `ROUTINE_DEFINITION` of `"System.Byte[]"`~~
* ~~[#100218](https://bugs.mysql.com/bug.php?id=100218): `TIME(n)` microsecond values deserialized incorrectly with prepared command~~
* ~~[#100306](https://bugs.mysql.com/bug.php?id=100306): `Command.Prepare` sends wrong statement to server~~
* ~~[#100522](https://bugs.mysql.com/bug.php?id=100522): `	SingleStoreCommand.Parameters.Insert(-1)` succeeds but should fail~~
* ~~[#101252](https://bugs.mysql.com/bug.php?id=101252): Can't query `CHAR(36)` column containing `NULL`~~
* [#101253](https://bugs.mysql.com/bug.php?id=101253): Default value for `SingleStoreParameter.Value` changed from null to `0`
* ~~[#101302](https://bugs.mysql.com/bug.php?id=101302): Stored Procedure `BOOL` parameter can only be mapped to `SingleStoreDbType.Byte`~~
* [#101485](https://bugs.mysql.com/bug.php?id=101485): Stored Procedure `JSON` parameter throws “Unhandled type encountered” `SingleStoreException`
* [#101507](https://bugs.mysql.com/bug.php?id=101507): `SingleStoreCommand.Cancel` throws `NullReferenceException` for a closed connection
* ~~[#101714](https://bugs.mysql.com/bug.php?id=101714): Extremely slow performance reading result sets~~
* [#102593](https://bugs.mysql.com/bug.php?id=102593): Can't use `MemoryStream` as `SingleStoreParameter.Value`
* ~~[#103390](https://bugs.mysql.com/bug.php?id=103390): Can't query `CHAR(36)` column if `SingleStoreCommand` is prepared~~
* ~~[#103430](https://bugs.mysql.com/bug.php?id=103430): Can't connect using named pipe on Windows~~
* [#103801](https://bugs.mysql.com/bug.php?id=103801): `TimeSpan` parameters lose microseconds with prepared statement
* [#103819](https://bugs.mysql.com/bug.php?id=103819): Can't use `StringBuilder` containing non-BMP characters as `SingleStoreParameter.Value`
* [#104910](https://bugs.mysql.com/bug.php?id=104910): `SingleStoreConnectionStringBuilder.TryGetValue` always returns `false`
* [#104913](https://bugs.mysql.com/bug.php?id=104913): Cannot execute stored procedure with backtick in name
* [#105209](https://bugs.mysql.com/bug.php?id=105209): Timespan value of zero can't be read with prepared command
* [#105728](https://bugs.mysql.com/bug.php?id=105728): Named command parameters override query attribute values
* [#105730](https://bugs.mysql.com/bug.php?id=105730): `SingleStoreCommand.Clone` doesn't clone attributes
* [#105768](https://bugs.mysql.com/bug.php?id=105768): `SingleStoreCommandBuilder` doesn't support tables with `BIGINT UNSIGNED` primary key
* [#105965](https://bugs.mysql.com/bug.php?id=105965): `SingleStoreParameterCollection.Add(object)` has quadratic performance
