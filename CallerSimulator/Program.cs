using System.Net.Http.Json;

// 🧠 Parse command-line arguments
string? url = null;
int postBatches = 5;
int batchSize = 2;
int getCount = 3;

for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--url":
            url = i + 1 < args.Length ? args[++i] : null;
            break;
        case "--batches":
            postBatches = int.TryParse(args[++i], out var pb) ? pb : postBatches;
            break;
        case "--batchSize":
            batchSize = int.TryParse(args[++i], out var bs) ? bs : batchSize;
            break;
        case "--getCount":
            getCount = int.TryParse(args[++i], out var gc) ? gc : getCount;
            break;
    }
}

if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var apiUri))
{
    Console.WriteLine("Missing or invalid --url parameter.\n");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run -- --url <API_URL> [--batches 5] [--batchSize 2] [--getCount 3]");
    return;
}

var client = new HttpClient { BaseAddress = apiUri };

string[] firstNames = ["Leia", "Sadie", "Jose", "Sara", "Frank", "Dewey", "Tomas", "Joel", "Lukas", "Carlos", "Gabriel", "Tenmma", "Boromir", "Tom", "Sam", "Simon", "Tatyl", "Jen", "Gloria"];
string[] lastNames = ["Liberty", "Ray", "Harrison", "Ronan", "Drew", "Powell", "Larsen", "Chan", "Anderson", "Lane", "Jhonson", "Bombadillo", "Gamgee", "Hurtado", "Barn", "Morgan", "Goldberg", "Zidane", "Harrigan"];
int nextId = 1;
Random rng = new();
List<Task> tasks = new();

for (int i = 0; i < postBatches; i++)
{
    tasks.Add(Task.Run(async () =>
    {
        var customers = new List<CustomerDto>();
        for (int j = 0; j < batchSize; j++)
        {
            customers.Add(new CustomerDto
            {
                FirstName = firstNames[rng.Next(firstNames.Length)],
                LastName = lastNames[rng.Next(lastNames.Length)],
                Age = rng.Next(10, 91),
                Id = Interlocked.Increment(ref nextId)
            });
        }

        var res = await client.PostAsJsonAsync("/customers", customers);
        if (res.IsSuccessStatusCode)
        {
            Console.WriteLine($"POST [{string.Join(", ", customers.Select(c => c.Id))}]");
        }
        else
        {
            var error = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"POST ERROR: {res.StatusCode}\n{error}");
        }
    }));
}


for (int i = 0; i < getCount; i++)
{
    tasks.Add(Task.Run(async () =>
    {
        await Task.Delay(rng.Next(100)); 
        var res = await client.GetAsync("/customers");
        if (res.IsSuccessStatusCode)
        {
            var text = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"GET [{res.StatusCode}] - {text.Length} bytes");
        }
        else
        {
            Console.WriteLine($"GET ERROR: {res.StatusCode}");
        }
    }));
}

await Task.WhenAll(tasks);
Console.WriteLine("Simulation complete.");


record CustomerDto
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int Age { get; set; }
    public int Id { get; set; }
}
