using System.Collections.Concurrent;

namespace ERxWebClient2.Zotero
{
    public class ZoteroConcurrentDictionary
    {
            private readonly ConcurrentDictionary<string, string> _session
                = new ConcurrentDictionary<string, string>();

            public ConcurrentDictionary<string, string> Session => _session;
       
    }
}