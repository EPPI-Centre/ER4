using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERxWebClient2
{
	public interface ITypedHubClient
	{
		Task BroadcastMessage(string type, string payload);
		//void addChatMessage(string v);
	}
}
