using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EpisodeRenamer.Core;

namespace CMD___Front
{
    class CMD_Main
    {

        static void Main(string[] args)
        {

            Core API = new Core();

            EpisodeRenamer.FrontEnd.IDisplay display = new EpisodeRenamer.FrontEnd.ConsoleDisplayer();

            var errors = API.Initialize();

            if (errors.Length != 0)
            {
                // Imprimo los errores y salgo

                display.ShowErrors(errors);
                display.warnUser("");

                return;
            }

            if (args.Length > 1)
            {
                return;
            }

            if (args.Contains("debug"))
                Utils.Debug.SetDebugMode(true);


            FSM.UserModeFSM fsm = new FSM.UserModeFSM(API, display);

            fsm.Transition(FSM.UserModeStates.Init);

            do
                fsm.StateDo();
            while (!fsm.hasToLeave);
        }
    }
}
