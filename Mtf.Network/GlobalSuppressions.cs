// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types")]
[assembly: SuppressMessage("Style", "IDE0022:Use block body for method")]
[assembly: SuppressMessage("Design", "CA1032:Implement standard exception constructors")]
[assembly: SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
[assembly: SuppressMessage("Security", "CA5394:Do not use insecure randomness")]
[assembly: SuppressMessage("Design", "CA1008:Enums should have zero value")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Mtf.Network.FtpClient.dataSocket")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Mtf.Network.FtpClient.dataReceiveCancellationTokenSource")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Mtf.Network.PipeServer.writer")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Mtf.Network.PipeServer.cancellationTokenSource")]
[assembly: SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "<Pending>", Scope = "member", Target = "~F:Mtf.Network.PipeClient.namedPipeClientStream")]
[assembly: SuppressMessage("Style", "IDE0010:Add missing cases", Justification = "<Pending>", Scope = "member", Target = "~M:Mtf.Network.SnmpClient.DataArrivedEventHandler(System.Object,Mtf.Network.EventArg.DataArrivedEventArgs)")]
