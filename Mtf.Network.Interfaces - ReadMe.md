# Mtf.Network.Interfaces Library Documentation

## Overview

The `Mtf.Network.Interfaces` namespace defines key interfaces used for modular development in the `Mtf.Network` ecosystem. These interfaces cover cryptographic operations, image acquisition, process output handling, and screen-related utilities, enabling extensibility and platform abstraction.

This document details each interface, its purpose, method signatures, and example use cases.

---

## Interface: `ICipher`

The `ICipher` interface defines methods for symmetric or asymmetric encryption and decryption of strings and byte arrays.

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Encrypt(string plainText)` | `string` | Encrypts a plaintext string and returns the ciphertext as a string. |
| `Decrypt(string cipherText)` | `string` | Decrypts the encrypted string back into plaintext. |
| `Encrypt(byte[] plainBytes)` | `byte[]` | Encrypts a byte array and returns the encrypted byte array. |
| `Decrypt(byte[] cipherBytes)` | `byte[]` | Decrypts the byte array and returns the original data. |

### Example Implementation

```csharp
public class CaesarCipher : ICipher
{
    public string Encrypt(string plainText) => new string(plainText.Select(c => (char)(c + 3)).ToArray());
    public string Decrypt(string cipherText) => new string(cipherText.Select(c => (char)(c - 3)).ToArray());
    public byte[] Encrypt(byte[] plainBytes) => plainBytes.Select(b => (byte)(b + 3)).ToArray();
    public byte[] Decrypt(byte[] cipherBytes) => cipherBytes.Select(b => (byte)(b - 3)).ToArray();
}
```

---

## Interface: `IImageSource`

The `IImageSource` interface abstracts the mechanism for capturing image data asynchronously, typically from a video source or screen.

### Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `CaptureAsync(CancellationToken token)` | `Task<byte[]>` | Captures a frame and returns the result as a byte array. The operation can be canceled via the token. |

### Example Usage

```csharp
public class DummyCamera : IImageSource
{
    public Task<byte[]> CaptureAsync(CancellationToken token)
    {
        var dummyImage = new byte[] { 0xFF, 0xD8, 0xFF }; // JPEG header as example
        return Task.FromResult(dummyImage);
    }
}
```

---

## Interface: `IProcessResultParser`

The `IProcessResultParser` interface provides hooks for reading and processing output and error streams from external processes, commonly used with `Process.Start`.

### Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `ErrorDataReceived(object sender, DataReceivedEventArgs e)` | `sender`: the process; `e`: error line | Called when a line is received on the error stream. |
| `OutputDataReceived(object sender, DataReceivedEventArgs e)` | `sender`: the process; `e`: output line | Called when a line is received on the standard output. |

### Example Usage

```csharp
public class ConsoleProcessLogger : IProcessResultParser
{
    public void ErrorDataReceived(object sender, DataReceivedEventArgs e) => Console.Error.WriteLine(e.Data);
    public void OutputDataReceived(object sender, DataReceivedEventArgs e) => Console.WriteLine(e.Data);
}
```

---

## Interface: `IScreenInfoProvider`

The `IScreenInfoProvider` interface provides information about a screen device, including its dimensions and ID.

### Properties & Methods

| Member | Return Type | Description |
|--------|-------------|-------------|
| `Id` | `string` | A unique identifier for the screen (e.g., `"\\.\DISPLAY1"`). |
| `GetBounds()` | `Rectangle` | Returns the bounding rectangle of the screen. |
| `GetPrimaryScreenSize()` | `Size` | Returns the size of the primary screen (regardless of the current screen). |

### Example Implementation

```csharp
public class WindowsScreenInfoProvider : IScreenInfoProvider
{
    public string Id => Screen.PrimaryScreen.DeviceName;
    public Rectangle GetBounds() => Screen.PrimaryScreen.Bounds;
    public Size GetPrimaryScreenSize() => Screen.PrimaryScreen.Bounds.Size;
}
```

---

## Summary

These interfaces in `Mtf.Network.Interfaces` offer a clean and flexible foundation for:

- **ICipher**: Cryptographic transformation abstraction.
- **IImageSource**: Capturing images from dynamic sources.
- **IProcessResultParser**: Parsing stdout/stderr of subprocesses.
- **IScreenInfoProvider**: Retrieving monitor/screen info.

They are designed to facilitate testable, maintainable, and scalable applications using composition and dependency injection.