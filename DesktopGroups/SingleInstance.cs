using System.IO;
using System.IO.Pipes;
using System.Windows;

namespace DesktopGroups;

/// <summary>
/// Garantiza una sola instancia. Si ya hay una corriendo, le envía
/// los argumentos por Named Pipe y termina.
/// </summary>
public static class SingleInstance
{
    private const string PipeName = "DesktopGroupsPipe";
    private static System.Threading.Mutex? _mutex;

    /// <summary>
    /// Retorna true si esta ES la primera instancia (debe seguir corriendo).
    /// Retorna false si ya había una instancia (ya le envió los args y debe salir).
    /// </summary>
    public static bool TryBecomePrimary(string[] args, out CancellationTokenSource cts)
    {
        cts = new CancellationTokenSource();
        _mutex = new System.Threading.Mutex(true, "DesktopGroupsMutex", out bool isNew);

        if (isNew)
        {
            // Somos la primera instancia: ponemos a escuchar el pipe
            StartPipeServer(cts.Token);
            return true;
        }
        else
        {
            // Ya hay una instancia: le enviamos los args y salimos
            SendArgsToRunningInstance(args);
            return false;
        }
    }

    private static void StartPipeServer(CancellationToken token)
    {
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        PipeName, PipeDirection.In,
                        1, PipeTransmissionMode.Message,
                        PipeOptions.Asynchronous);

                    await server.WaitForConnectionAsync(token);

                    using var reader = new StreamReader(server);
                    var message = await reader.ReadLineAsync();

                    if (!string.IsNullOrEmpty(message))
                    {
                        // Ejecutar en el hilo UI
                        System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                        {
                            HandleMessage(message);
                        });
                    }
                }
                catch (OperationCanceledException) { break; }
                catch { /* pipe roto, reintentar */ }
            }
        }, token);
    }

    private static void SendArgsToRunningInstance(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
            client.Connect(1000); // timeout 1 segundo
            using var writer = new StreamWriter(client);
            writer.WriteLine(string.Join(" ", args));
            writer.Flush();
        }
        catch { }
    }

    private static void HandleMessage(string message)
    {
        var parts = message.Split(' ');
        if (parts.Length >= 2 && parts[0] == "--open")
        {
            var groupId = parts[1];
            var group   = GroupManager.Instance.Groups.FirstOrDefault(g => g.Id == groupId);
            if (group != null)
                DrawerManager.OpenDrawer(group);
        }
    }
}
