using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MUnit;
using MUnit.Engine;
using MUnit.Transport;
using Verse;

namespace AwesomeInventory.Test 
{
    public class TestServer : GameComponent
    {
        private static MUnitEngine _engine = new MUnitEngine(new MUnitLogger(MUnit.Framework.MessageLevel.Debug));
        private static TCPServer _server;

        static TestServer()
        {
            Log.Warning(Assembly.Load("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089").Location);
            _server = new TCPServer(_engine);
        }

        public TestServer(Game game)
        {
            Log.Warning(Directory.GetCurrentDirectory());
        }

        public override void GameComponentTick()
        {  
            _server.Start();
        }
    }
}
