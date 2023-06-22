using OleksaScrSvr.VM.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace OleksaScrSvr.Commands
{
  public class LogoutCommand : CommandBase
    {
         readonly AcntStore _accountStore;

        public LogoutCommand(AcntStore accountStore)
        {
            _accountStore = accountStore;
        }

        public override void Execute(object? parameter)
        {
            _accountStore.Logout();
        }
    }
}
