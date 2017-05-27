using System.Threading.Tasks;
using RunRabbitRun.Net.Sample.Models;

namespace RunRabbitRun.Net.Sample
{
    public interface IUserRepository
    {
        Task CreateAsync(User user);

        Task DeleteAsync(string userName);
    }

}