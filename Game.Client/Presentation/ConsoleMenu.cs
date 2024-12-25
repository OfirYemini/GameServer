using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GameClient.Domain;

public class ConsoleMenu: BackgroundService
{
    private readonly IGameServerWs _client;
    private readonly ILogger<ConsoleMenu> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public ConsoleMenu(IGameServerWs client, ILogger<ConsoleMenu> logger, IHostApplicationLifetime hostApplicationLifetime)
    {
        _client = client;
        _logger = logger;
        _hostApplicationLifetime = hostApplicationLifetime;

        _client.OnMessageReceived += PrintMessage;
    }

    private void PrintMessage(ServerResponse serverResponse)
    {
        if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.LoginResponse)
        {
            var response = serverResponse.LoginResponse;
            Console.WriteLine($"LoginResponse: PlayerId={response.PlayerId}");
        }
        else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.UpdateResponse)
        {
            var response = serverResponse.UpdateResponse;
            Console.WriteLine($"UpdateResponse: New Balance={response.NewBalance}");
        }
        else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.SendGiftResponse)
        {
            var response = serverResponse.SendGiftResponse;
            Console.WriteLine($"SendGiftResponse: New Balance={response.NewBalance}");
        }
        else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.GiftEvent)
        {
            var response = serverResponse.GiftEvent;
            Console.WriteLine($"GiftEvent from player {response.FromPlayer} : ResourceValue={response.ResourceValue} ({response.ResourceType})");
        }
        else if (serverResponse.InnerResponseCase == ServerResponse.InnerResponseOneofCase.ServerError)
        {
            var response = serverResponse.ServerError;
            Console.WriteLine($"Unexpected error received. {response.Message} with error id {response.ErrorId}");
        }
        else
        {
            Console.WriteLine("Unexpected response received.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bool exit = false;
        while (!exit && !stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("\n-- MAIN MENU --");
            Console.WriteLine("1) Login");
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
                    await _client.CloseWebsocket();
                    _hostApplicationLifetime.StopApplication();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1-4.");
                    break;
            }
            await Task.Delay(2500, stoppingToken);
        }
    }

    private async Task HandleLogin()
    {
        String[] devices =
        [
            "f78681e1-4521-4b39-a3b9-f2e7e51636a7", "145bf581-3cc0-4428-add8-fb2300544195",
            "15b1ec0f-389b-40f9-b800-dd4d78275420"
        ];
        Console.WriteLine($"1. {devices[0]}\t2. {devices[1]}\t3. {devices[2]}\t4.other");
        var choice = GetValidInput<int>("Your choice (1-4): ");
        string deviceId = choice switch
        {
            1 or 2 or 3 => devices[choice-1],
            4 => GetValidInput<string>("DeviceId: "),
            _ => GetValidInput<string>("DeviceId: ")
        };
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
        var resourceType = GetValidInput<int>("Resource Type (Coins=0,Rolls=1) : ");
        var amount = GetValidInput<int>("Amount: ");

        await _client.SendGiftAsync(friendId, (ResourceType)resourceType, amount);
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
