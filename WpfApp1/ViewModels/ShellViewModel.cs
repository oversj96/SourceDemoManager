using Caliburn.Micro;
using ReSourcer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ReSourcer.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Private Fields

        private GameManagerModel _selectedGameManagerModel;
        private Demo _selectedDemo;
        private string _txtBxGameName;
        private GameManagersInformation gmInfo;
        private BindableCollection<GameManagerModel> _gameManagerModels = new BindableCollection<GameManagerModel>();
        private bool _gameInstanceRunning = false;
        private BindableCollection<Demo> demos = new BindableCollection<Demo>();

        #endregion

        #region Display Properties

        // Text Box Properties
        public string TxtBxGameName
        {
            get
            {
                return _txtBxGameName;
            }

            set
            {
                _txtBxGameName = value;
                NotifyOfPropertyChange(() => TxtBxGameName);
            }
        }

        public string TxtBxRecordCommand { get; set; } = "alias +showexec \"+showscores; exec AutoRecorder.cfg\"; alias -showexec \"-showscores\"; bind TAB +showexec;";

        // Text Block Properties
        public string TxtBlkGamePath
        {
            get
            {
                return SelectedGameManagerModel.GamePath;
            }
        }

        public string TxtBlkGameExe
        {
            get
            {
                return SelectedGameManagerModel.GameExe;
            }
        }

        public string TxtBlkDemoDir
        {
            get
            {
                return SelectedGameManagerModel.DemosDirectory;
            }
        }

        public string TxtBlkAutoexec
        {
            get
            {
                return SelectedGameManagerModel.AutoExec;
            }
        }

        public string TxtBlkDemoInfo
        {
            get
            {
                if(SelectedDemo != null)
                {
                    return SelectedDemo.ToString();
                }
                else
                {
                    return "";
                }               
            }
        }

        #endregion

        public BindableCollection<Demo> Demos
        {
            get { return demos; }
            set
            {
                demos = value;
                NotifyOfPropertyChange(() => Demos);
            }
        }



        public Demo SelectedDemo
        {
            get { return _selectedDemo; }
            set
            {
                _selectedDemo = value;
                NotifyOfPropertyChange(() => SelectedDemo);
                NotifyOfPropertyChange(() => TxtBlkDemoInfo);
            }
        }

        public List<List<Demo>> DemoLists
        {
            get
            {
                return Demo.DemoLists(SelectedGameManagerModel.instance);
            }
        }

        public void GetDemoInfo()
        {
            List<List<Demo>> lists = DemoLists;
            Demos.Clear();
            foreach (var list in lists)
            {
                foreach (var demo in list)
                {
                    Demos.Add(demo);
                }
            }
        }


        public bool GameInstanceRunning
        {
            get { return _gameInstanceRunning; }
            set
            {
                _gameInstanceRunning = value;
                NotifyOfPropertyChange(() => GameInstanceRunning);
                NotifyOfPropertyChange(() => CanStartSelectedGame);
            }
        }

        public string DeleteBtnGameName
        {
            get
            {
                return "Delete " + SelectedGameManagerModel.GameName + " Manager";
            }
        }

        public ShellViewModel()
        {
            LoadManagers();
            GameInstanceRunning = false;
            TxtBxGameName = null;
        }

        public void LoadManagers()
        {
            if (GameManagerModels.Count != 0)
            {
                GameManagerModels.Clear();
            }        

            gmInfo = GameManagersInformation.Instance();

            gmInfo.Load();

            List<GameManager> gmList = gmInfo.gameManagersList.ToList();

            foreach (var manager in gmList)
            {
                GameManagerModels.Add(new GameManagerModel(manager));
            }

            if(gmList.Count != 0)
            {
                SelectedGameManagerModel = GameManagerModels.First();
            }
            else
            {
                SelectedGameManagerModel = new GameManagerModel(new GameManager());
            }
        }

        public GameManagerModel SelectedGameManagerModel
        {
            get { return _selectedGameManagerModel; }
            set
            {
                if(value != null)
                {
                    _selectedGameManagerModel = value;
                }
                else
                {
                    _selectedGameManagerModel = new GameManagerModel(new GameManager());
                }

                NotifyOfPropertyChange(() => SelectedGameManagerModel);
                NotifyOfPropertyChange(() => TxtBlkDemoDir);
                NotifyOfPropertyChange(() => TxtBlkGameExe);
                NotifyOfPropertyChange(() => TxtBlkGamePath);
                NotifyOfPropertyChange(() => TxtBxGameName);
                NotifyOfPropertyChange(() => TxtBlkAutoexec);
                NotifyOfPropertyChange(() => DeleteBtnGameName);
                NotifyOfPropertyChange(() => TxtBlkDemoInfo);
                NotifyOfPropertyChange(() => DemoLists);
                List<List<Demo>> demoLists = Demo.DemoLists(SelectedGameManagerModel.instance);
                GetDemoInfo();
            }
        }


        public BindableCollection<GameManagerModel> GameManagerModels
        {
            get { return _gameManagerModels; }
            set
            {
                _gameManagerModels = value;
                NotifyOfPropertyChange(() => GameManagerModels);
            }
        }

        public void GameFinished()
        {
            GameInstanceRunning = false;
        }

        public bool CanAddManager(string txtBxGameName, string txtBoxRecordCommand)
        {
            return !String.IsNullOrWhiteSpace(txtBxGameName) && !GameInstanceRunning;
        }

        public void AddManager(string txtBxGameName, string txtBoxRecordCommand)
        {
            var gm = new GameManager();

            if (String.IsNullOrWhiteSpace(txtBoxRecordCommand))
            {
                gm = new GameManager(txtBxGameName);
            }
            else
            {
                gm = new GameManager(txtBxGameName, txtBoxRecordCommand);
            }

            gmInfo.AddGameManager(gm);
            gmInfo.Save();
            LoadManagers();
        }

        public bool CanStartSelectedGame
        {
            get
            {
                return !String.IsNullOrWhiteSpace(TxtBlkGamePath) && !GameInstanceRunning;
            }
        }

        public void StartSelectedGame(string txtBlkGamePath)
        {
            GameInstanceRunning = true;
            Task.Factory.StartNew(SelectedGameManagerModel.Start).ContinueWith(result => GameFinished());
        }

        public bool CanDeleteManager(string txtBlkGamePath)
        {
            return !String.IsNullOrWhiteSpace(txtBlkGamePath) && !GameInstanceRunning;
        }

        public void DeleteManager(string txtBlkGamePath)
        {

            GameManager gm =
                gmInfo.gameManagersList.SingleOrDefault(s => s.GamePath == txtBlkGamePath);

            if (MessageBox.Show($"Really delete { gm.GameName }?",
                "Deletion",
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                gmInfo.RemoveGameManager(gm.GameName);
                gmInfo.Save();
                LoadManagers();
            }
        }
    }
}
