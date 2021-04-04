// IrcClient.Handlers.IHandler
using IrcClient.Models;
using System.Threading.Tasks;

namespace IrcClient.Handlers
{
    public interface IHandler
    {
        Task<bool> Handle(IrcMessage message);
    }
}
