using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameClient.Domain;

public class ConsoleMenu: BackgroundService
{
    private readonly IGameServerWs _client;
    private readonly ILogger<ConsoleMenu> _logger;

    public ConsoleMenu(IGameServerWs client, ILogger<ConsoleMenu> logger)
    {
        _client = client;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool exit = false;
        while (!exit && !stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("\n-- MAIN MENU --");
            Console.WriteLine("1) Login (f78681e1-4521-4b39-a3b9-f2e7e51636a7,145bf581-3cc0-4428-add8-fb2300544195,15b1ec0f-389b-40f9-b800-dd4d78275420)");
            Console.WriteLine("2) Update Resource");
            Console.WriteLine("3) Send Gift");
            Console.WriteLine("4) Exit");
            Console.Write("Enter choice: ");
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await HandleLogin();
                    break;
                case "2":
                    await HandleUpdateResource();
                    break;
                case "3":
                    await HandleSendGift();
                    break;
                case "4":
                    exit = true;
                    _logger.LogInformation("Exiting application...");
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1-4.");
                    break;
            }
        }
    }

    private async Task HandleLogin()
    {
        var deviceId = GetValidInput<string>("DeviceId: ");
        await _client.LoginAsync(deviceId);
    }

    private async Task HandleUpdateResource()
    {
        var resourceType = GetValidInput<int>("Resource Type (Coins=0,Rolls=1) : ");
        var amount = GetValidInput<int>("Amount: ");
        await _client.UpdateResourceAsync((ResourceType)resourceType, amount);
    }

    private async Task HandleSendGift()
    {
        var friendId = GetValidInput<int>("Friend ID: ");
        var resourceType = GetValidInput<ResourceType>("Resource Type (Coins=0,Rolls=1) : ");
        var amount = GetValidInput<int>("Amount: ");

        await _client.SendGiftAsync(friendId, resourceType, amount);
    }
    
    private T GetValidInput<T>(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (typeof(T) == typeof(string))
            {
                if (!string.IsNullOrWhiteSpace(input))
                {
                    return (T)(object)input!;
                }
                Console.WriteLine("Input cannot be empty. Please try again.");
            }
            else
            {
                try
                {
                    var converted = (T)Convert.ChangeType(input, typeof(T));
                    return converted;
                }
                catch
                {
                    Console.WriteLine($"Invalid {typeof(T).Name}. Please try again.");
                }
            }
        }
    }
}
