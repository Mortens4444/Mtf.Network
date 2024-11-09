# Mtf.Network Library Documentation

## Overview

The `Mtf.Network` library provides functionality for creating and managing inter-process communication using named pipes. It includes the `PipeServer` and `PipeClient` classes, which facilitate sending and receiving messages asynchronously. This document covers installation, class properties, methods, events, and usage examples for integrating with named pipes in .NET applications.

## Installation

To use `Mtf.Network`, add the package to your project:

1. **Add Package**:
   Run the following in your project directory:

   ```bash
   dotnet add package Mtf.Network
   ```

2. **Include the Namespace**:
   Add the `Mtf.Network` namespace at the beginning of your code:

   ```csharp
   using Mtf.Network;
   ```

## Class: PipeServer

The `PipeServer` class is responsible for handling server-side functionality for named pipes. It accepts connections, listens for messages, and sends data asynchronously to connected clients.

### Constructor

**`PipeServer()`**

- Initializes a new instance of the `PipeServer` class.

### Properties

| Property                     | Type                   | Description                                                                 |
|------------------------------|------------------------|-----------------------------------------------------------------------------|
| `PipeName`                   | `string`               | The name of the named pipe used for communication. Defaults to `String.Empty`. |
| `Encoding`                   | `Encoding`             | The encoding used for message reading and writing. Defaults to `UTF8`.        |
| `PipeDirection`              | `PipeDirection`        | Specifies the direction of the pipe (In, Out, or InOut). Defaults to `InOut`. |
| `PipeTransmissionMode`       | `PipeTransmissionMode` | Defines the transmission mode of the pipe (Byte or Message). Defaults to `Byte`. |
| `PipeOptions`                | `PipeOptions`          | Specifies pipe options (e.g., `Asynchronous`). Defaults to `Asynchronous`.    |

### Methods

#### Server Management

- **`Start()`**  
  Starts the pipe server, waits for client connections, and begins reading incoming messages asynchronously.

- **`Stop()`**  
  Stops the pipe server, canceling any active operations.

#### Message Sending

- **`SendAsync(string message)`**  
  Sends a message asynchronously to the connected pipe client. Throws an exception if the server is not started or the message is `null`.

#### Resource Management

- **`Dispose()`**  
  Disposes of the resources used by the `PipeServer` class, including canceling any active tasks and cleaning up the `StreamWriter`.

### Events

- **`MessageReceived`**  
  Occurs when a message is received from the client.

  **EventArgs**: `MessageEventArgs`  
  - Contains the received message.

- **`ErrorOccurred`**  
  Occurs when an error occurs during communication or server operation.

  **EventArgs**: `ExceptionEventArgs`  
  - Contains the exception details.

### Example Usage

```csharp
using Mtf.Network;
using System;
using System.Threading.Tasks;

public class PipeServerExample
{
    public async Task Example()
    {
        // Create a new PipeServer instance
        var pipeServer = new PipeServer { PipeName = "TestPipe" };

        // Subscribe to the MessageReceived event
        pipeServer.MessageReceived += (sender, e) => 
        {
            Console.WriteLine($"Received message: {e.Message}");
        };

        // Start the pipe server
        pipeServer.Start();

        // Send a message asynchronously
        await pipeServer.SendAsync("Hello from the server!");

        // Stop the pipe server
        pipeServer.Stop();
    }
}
```

---

## Class: PipeClient

The `PipeClient` class is responsible for managing client-side functionality for named pipes. It connects to a pipe server, sends messages, and listens for incoming messages asynchronously.

### Constructor

**`PipeClient()`**

- Initializes a new instance of the `PipeClient` class.

### Properties

| Property                     | Type                   | Description                                                                 |
|------------------------------|------------------------|-----------------------------------------------------------------------------|
| `ServerName`                 | `string`               | The name of the server hosting the named pipe. Defaults to `"."`.            |
| `PipeName`                   | `string`               | The name of the named pipe to connect to.                                      |
| `PipeOptions`                | `PipeOptions`          | Specifies pipe options (e.g., `Asynchronous`). Defaults to `Asynchronous`.   |
| `Encoding`                   | `Encoding`             | The encoding used for message reading and writing. Defaults to `UTF8`.       |
| `PipeDirection`              | `PipeDirection`        | Specifies the direction of the pipe (In, Out, or InOut). Defaults to `InOut`. |

### Methods

#### Client Management

- **`ConnectAsync()`**  
  Connects to the server's named pipe asynchronously. If the connection fails, the `ErrorOccurred` event is triggered.

- **`Disconnect()`**  
  Disconnects from the named pipe server and releases resources.

#### Message Sending and Receiving

- **`SendAsync(string message, CancellationToken cancellationToken = default)`**  
  Sends a message asynchronously to the connected pipe server. Throws an exception if the connection fails.

- **`ReceiveAsync(int bufferSize = 1024, CancellationToken cancellationToken = default)`**  
  Receives a message asynchronously from the pipe server. Returns the received message as a string.

#### Listening for Messages

- **`StartListening(int bufferSize = 1024)`**  
  Starts listening for incoming messages from the server.

- **`StopListening()`**  
  Stops the listening operation.

#### Resource Management

- **`Dispose()`**  
  Disposes of the resources used by the `PipeClient` class, including canceling active operations.

### Events

- **`MessageReceived`**  
  Occurs when a message is received from the server.

  **EventArgs**: `MessageEventArgs`  
  - Contains the received message.

- **`ErrorOccurred`**  
  Occurs when an error occurs during communication or client operation.

  **EventArgs**: `ExceptionEventArgs`  
  - Contains the exception details.

### Example Usage

```csharp
using Mtf.Network;
using System;
using System.Threading;
using System.Threading.Tasks;

public class PipeClientExample
{
    public async Task Example()
    {
        // Create a new PipeClient instance
        var pipeClient = new PipeClient { PipeName = "TestPipe" };

        // Subscribe to the MessageReceived event
        pipeClient.MessageReceived += (sender, e) => 
        {
            Console.WriteLine($"Received message: {e.Message}");
        };

        // Connect to the server
        await pipeClient.ConnectAsync();

        // Send a message asynchronously
        await pipeClient.SendAsync("Hello from the client!");

        // Receive a message from the server
        var receivedMessage = await pipeClient.ReceiveAsync();
        Console.WriteLine($"Received message: {receivedMessage}");

        // Start listening for incoming messages
        pipeClient.StartListening();

        // Wait for a message
        await Task.Delay(1000);

        // Stop listening
        pipeClient.StopListening();

        // Disconnect from the server
        pipeClient.Disconnect();
    }
}
```

### Notes

- **Exception Handling**: Ensure exception handling is in place for communication errors during connection, sending, or receiving data.
- **Dispose**: Always dispose of the `PipeClient` and `PipeServer` instances to free up resources after use.
- **Listening**: If you want to continuously listen for messages, make sure to call `StartListening()` and `StopListening()` when appropriate.