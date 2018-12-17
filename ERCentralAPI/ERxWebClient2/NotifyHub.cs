using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ERxWebClient2
{
	public class NotifyHub: Hub<ITypedHubClient>
	{
		//private readonly static ConnectionMapping<string> _connections =
		//	new ConnectionMapping<string>();

		//public void SendChatMessage(string who, string message)
		//{
		//	string name = Context.User.Identity.Name;

		//	foreach (var connectionId in _connections.GetConnections(who))
		//	{
		//		Clients.Client(connectionId).addChatMessage(name + ": " + message);
		//	}
		//}

		//public override Task OnConnected()
		//{
		//	MyUsers.TryAdd(Context.User.Identity.Name, new MyUserType() { ConnectionId = Context.ConnectionId, UserName = Context.User.Identity.Name });
		//	string name = Context.User.Identity.Name;

		//	Groups.Add(Context.ConnectionId, name);

		//	return base.OnConnected();
		//}


	}
}