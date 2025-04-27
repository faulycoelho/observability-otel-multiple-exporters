using Elastic.CommonSchema;
using InternalApi03.Notification.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace InternalApi03.Notification.Seed
{
    public class SeedData
    {
        public static async Task SeedUsersAsync(IDatabase db)
        { 
            var users = new List<UserEntity>
            {
                new UserEntity { Id=10, Name = "Alice", Email = "Alice@email.com" },
                new UserEntity { Id=20, Name = "Bob", Email = "Bob@email.com" },
                new UserEntity { Id=30, Name = "Charlie" , Email = "Charlie@email.com"},
                new UserEntity { Id=40, Name = "Diana" , Email = "Diana@email.com"}
            };

            foreach (var user in users)
            {
                var key = $"user:{user.Id}";
                var value = JsonSerializer.Serialize(user);
                await db.StringSetAsync(key, value, when: When.NotExists);
            }
        }
    }
}
