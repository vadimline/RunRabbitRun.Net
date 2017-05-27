using System.Threading.Tasks;
using RunRabbitRun.Net.Test.Models;

namespace RunRabbitRun.Net.Test.Dependencies
{
    public interface IUserRepository
    {
        Task CreateAsync(User user);

        Task DeleteAsync(string userName);
    }

}