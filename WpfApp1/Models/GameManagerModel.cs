using System;
using Manager;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReSourcer.Models
{
    public class GameManagerModel
    {
        public GameManager instance;

        public string GameName { get; set; }

        public string GamePath { get; set; }

        public string AutoExec { get; set; }

        public string CfgPath { get; set; }

        public string GameExe { get; set; }

        public string DemosDirectory { get; set; }

        public string RecordCommand { get; set; }

        private string _launchArgs;

        public string LaunchArgs
        {
            get { return _launchArgs; }
            set
            {
                _launchArgs = value;
                instance.LaunchArgs = value;
            }
        }


        public GameManagerModel(GameManager manager)
        {
            instance = manager;
            this.GameName = manager.GameName;
            this.GamePath = manager.GamePath;
            this.AutoExec = manager.AutoExecAbsPath;
            this.CfgPath = manager.RecorderCfgPath;
            this.GameExe = manager.GameExe;
            this.DemosDirectory = manager.DemosDirectory;
            this.RecordCommand = manager.RecordCommand;
            this.LaunchArgs = manager.LaunchArgs;
        }

        public void Start(string extraArgs)
        {
            instance.Start(extraArgs);
        }
    }
}
