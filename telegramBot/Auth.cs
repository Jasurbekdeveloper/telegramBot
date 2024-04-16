using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace telegramBot;

internal class Auth
{
    private static NpgsqlConnection npgsqlConnection = new NpgsqlConnection("Host = localhost; Database = testBot; Username = postgres; Password = 4422");

   
    public static async Task<bool> LoginAsync(long telegramId)
    {
        await npgsqlConnection.OpenAsync();
        string query = "Select * from public.client where telegram_id = @id";

        using(NpgsqlCommand n = new NpgsqlCommand(query, npgsqlConnection))
        {
            n.Parameters.AddWithValue("@id", telegramId);

            var res = await n.ExecuteReaderAsync();
            bool hasRow = res.HasRows;
            await npgsqlConnection.CloseAsync();

            return hasRow;
        }
    }

    public static async Task<bool> RegistrAsync(Client client)
    {
        await npgsqlConnection.OpenAsync();

        string query = $"INSERT INTO public.client(name, lastname, phone_number, telegram_id) VALUES('{client.Name}', '{client.LastName}', '{client.PhoneNumber}', {client.TelegramId});";
        

        using(NpgsqlCommand n = new NpgsqlCommand(query, npgsqlConnection))
        {
           
            int res = await n.ExecuteNonQueryAsync();
            await npgsqlConnection.CloseAsync();

            return res > 0;
        }
    }
    public static async Task<List<Employee>> SearchEmployeesBySkillNameAsync(int skillId)
    {
        string? jsonString = null;
        await npgsqlConnection.OpenAsync();

        string query = "select public.search_employee(@skillId);";

        using(NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
        {
            command.Parameters.AddWithValue("@skillId", skillId);
            var result = await command.ExecuteReaderAsync();

            while(await result.ReadAsync())
            {
                jsonString = result.GetString(0);
            }

            await npgsqlConnection.CloseAsync();
        }

        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        if (jsonString is "data not found") return null;
        List<Employee> employees = JsonSerializer.Deserialize<List<Employee>>(jsonString, jsonSerializerOptions);

        return employees;

    }

    public static async Task<List<Skill>> SearchSkillByNameAsync(string skillName)
    {
        string jsonString = null;
        await npgsqlConnection.OpenAsync();

        string query = "select search_skill(@skillName);";

        using (NpgsqlCommand command = new NpgsqlCommand(query, npgsqlConnection))
        {
            command.Parameters.AddWithValue("@skillName", skillName);
            var result = await command.ExecuteReaderAsync();
            while (await result.ReadAsync())
            {
                jsonString = result.GetString(0);
            }
            await Console.Out.WriteLineAsync(jsonString );

            await npgsqlConnection.CloseAsync();
        }

        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        if (jsonString is "data not found") return null;
        List<Skill> skills = JsonSerializer.Deserialize<List<Skill>>(jsonString, jsonSerializerOptions);

        return skills;

    }

}
class Client
{
    public int Id { get; set; }
    public long TelegramId { get; set; }
    public string Name { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
}

class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }

    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
}

class Skill
{
    public int Id { get; set; }
    public string Name { get; set; }
}

