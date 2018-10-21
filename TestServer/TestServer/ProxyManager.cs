using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestServer
{
    class ProxyManager : Singleton<ProxyManager>
    {
        List<IProxy> proxyList;

        public void Init()
        {
            proxyList = new List<IProxy>()
            {
                BattleProxy.Instance,

            };
        }
    }


}
