using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace eDnevnik.Hubs
{
    public class ChatHub : Hub
    {
        public async Task PosaljiPoruku(string korisnik, string poruka, string vrijeme)
        {
            await Clients.All.SendAsync("PrimiPoruku", korisnik, poruka, vrijeme);
        }
    }
}
