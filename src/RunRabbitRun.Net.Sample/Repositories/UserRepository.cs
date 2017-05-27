using System;
using System.Threading.Tasks;
using RunRabbitRun.Net.Sample.Models;

namespace RunRabbitRun.Net.Sample
{
    public class UserRepository : IUserRepository
    {
        public async Task CreateAsync(User user)
        {
            //Console.WriteLine($"User with UserName({user.UserName}) created");
        }

        public async Task DeleteAsync(string userName)
        {
            //Console.WriteLine($"User with UserName({userName}) deleted");
        }
    }
}