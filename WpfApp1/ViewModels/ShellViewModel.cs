using Caliburn.Micro;
using Manager;
using MaterialDesignThemes.Wpf;
using ReSourcer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ReSourcer.ViewModels
{
    public class ShellViewModel : Screen
    {
        #region Private Fields

        private const string HandlerHashRelease = "711CF1013A0EABF667855BEADC4112DD";
        private const string HandlerHashDebug = "F891A9B6DBB95AB173AA77006A70A47D";
        private GameManagerModel _selectedGameManagerModel;
        private int oldDemoCount = 0;
        private SnackbarMessageQueue snackbar = new SnackbarMessageQueue(new TimeSpan(0, 0, 8));
        private Demo _selectedDemo;
        private string _txtBxGameName;
        private GameManagersInformation gmInfo;
        private BindableCollection<GameManagerModel> _gameManagerModels = new BindableCollection<GameManagerModel>();
        private bool _gameInstanceRunning = false;
        private BindableCollection<Demo> demos = new BindableCollection<Demo>();
        private bool firstLoadLaunchForSteam = false;
        private bool launchForSteam = false;

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
                if (SelectedDemo != null)
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
            get
            {
                return new BindableCollection<Demo>(SelectedGameManagerModel.instance.DemoList);
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
       
        public bool LaunchForSteam
        {
            get { return launchForSteam; }
            set
            {
                launchForSteam = value;
                NotifyOfPropertyChange(() => LaunchForSteam);
                FileInfo gameExe;
                FileInfo managerLibrary;
                FileInfo xmlPath;
                FileInfo handlerExe;
                string thisExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                try
                {
                    if (IsManagerSteamHandled() == true && value == false && File.Exists(SelectedGameManagerModel.GamePath))
                    {
                        handlerExe = new FileInfo(SelectedGameManagerModel.GamePath);
                        handlerExe.Delete();
                        managerLibrary = new FileInfo(Path.GetDirectoryName(SelectedGameManagerModel.GamePath) + "\\Manager.dll");
                        managerLibrary.Delete();
                        xmlPath = new FileInfo(Path.GetDirectoryName(SelectedGameManagerModel.GamePath) + "\\.managerpath.txt");
                        xmlPath.Delete();
                        gameExe = new FileInfo(SelectedGameManagerModel.GamePath.Replace(".exe", "_real.exe"));
                        gameExe.MoveTo(SelectedGameManagerModel.GamePath);
                    }
                    else if (IsManagerSteamHandled() == false && value == true && File.Exists(SelectedGameManagerModel.GamePath))
                    {
                        gameExe = new FileInfo(SelectedGameManagerModel.GamePath);
                        gameExe.MoveTo(gameExe.FullName.Replace(".exe", "_real.exe"));
                        managerLibrary = new FileInfo(thisExecutablePath + "\\Manager.dll");
                        managerLibrary.CopyTo(Path.GetDirectoryName(SelectedGameManagerModel.GamePath) + "\\Manager.dll");
                        handlerExe = new FileInfo(thisExecutablePath + "\\SteamHandler.exe");
                        handlerExe.CopyTo(SelectedGameManagerModel.GamePath);

                        File.WriteAllText(gameExe.DirectoryName + "\\.managerpath.txt", thisExecutablePath + "\\gamemanagersinformation.xml");
                    }
                }
                catch (IOException e)
                {
                    MessageBox.Show(
                        e.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                
            }
        }

        public SnackbarMessageQueue Snackbar
        {
            get { return snackbar; }
            set { snackbar = value; }
        }

        public bool GameInstanceRunning
        {
            get { return _gameInstanceRunning; }
            set
            {
                _gameInstanceRunning = value;
                NotifyOfPropertyChange(() => GameInstanceRunning);
                NotifyOfPropertyChange(() => CanStartSelectedGame);
                NotifyOfPropertyChange(() => CanDeleteManager);
                NotifyOfPropertyChange(() => CanLaunchForSteam);
                NotifyOfPropertyChange(() => Demos);
                NotifyOfPropertyChange(() => DemoCountBadge);
            }
        }

        public string DeleteBtnGameName
        {
            get
            {
                return "Delete " + SelectedGameManagerModel.GameName + " Manager";
            }
        }

        public string DemoCountBadge
        {
            get
            {
                return Demos.Count.ToString() + " demos";
            }
        }

        public ShellViewModel()
        {
            LoadManagers();
            GameInstanceRunning = false;
            TxtBxGameName = null;
        }

        protected override void OnDeactivate(bool close)
        {
            gmInfo.Save();
        }

        public void LoadManagers()
        {
            if (GameManagerModels.Count != 0)
            {
                GameManagerModels.Clear();
            }
            
            gmInfo = GameManagersInformation.Instance();
            gmInfo.Load();
            List<GameManager> gmList = gmInfo.GameManagersList.ToList();
            foreach (var manager in gmList)
            {
                GameManagerModels.Add(new GameManagerModel(manager));
            }
            if (gmList.Count != 0)
            {
                SelectedGameManagerModel = GameManagerModels.First();
                
            }
            else
            {
                try
                {
                    SelectedGameManagerModel = new GameManagerModel(new GameManager());
                }
                catch (Exception e)
                {
                    MessageBox.Show(
                        e.Message,
                        "Error");
                }
                
            }
        }

        private bool IsManagerSteamHandled()
        {
            using (var md5 = MD5.Create())
            {
                try
                {
                    using (var stream = File.OpenRead(SelectedGameManagerModel.GamePath))
                    {
                        string hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                        return HandlerHashRelease == hash || HandlerHashDebug == hash;
                    }
                }
                catch (IOException e)
                {
                    MessageBox.Show(
                        e.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
            }
        }

        public GameManagerModel SelectedGameManagerModel
        {
            get { return _selectedGameManagerModel; }
            set
            {
                gmInfo.Save();
                if (value != null)
                {
                    _selectedGameManagerModel = value;
                }
                else
                {
                    _selectedGameManagerModel = new GameManagerModel(new GameManager());
                }

                if (_selectedGameManagerModel.GamePath != string.Empty)
                {
                    NotifyOfPropertyChange(() => SelectedGameManagerModel);
                    NotifyOfPropertyChange(() => TxtBlkDemoDir);
                    NotifyOfPropertyChange(() => TxtBlkGameExe);
                    NotifyOfPropertyChange(() => TxtBlkGamePath);
                    NotifyOfPropertyChange(() => TxtBxGameName);
                    NotifyOfPropertyChange(() => TxtBlkAutoexec);
                    NotifyOfPropertyChange(() => DeleteBtnGameName);
                    NotifyOfPropertyChange(() => CanDeleteManager);
                    NotifyOfPropertyChange(() => TxtBlkDemoInfo);
                    NotifyOfPropertyChange(() => Demos);
                    NotifyOfPropertyChange(() => DemoCountBadge);
                    firstLoadLaunchForSteam = IsManagerSteamHandled();
                    LaunchForSteam = IsManagerSteamHandled();
                    NotifyOfPropertyChange(() => CanLaunchForSteam);
                }
                else
                {
                    NotifyOfPropertyChange(() => Demos);
                    NotifyOfPropertyChange(() => DemoCountBadge);
                    NotifyOfPropertyChange(() => DeleteBtnGameName);
                    NotifyOfPropertyChange(() => CanDeleteManager);
                    NotifyOfPropertyChange(() => CanStartSelectedGame);
                    NotifyOfPropertyChange(() => CanLaunchForSteam);
                }
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

            if (!String.IsNullOrWhiteSpace(gm.GamePath))
            {
                gmInfo.AddGameManager(gm);
                gmInfo.Save();
                LoadManagers();
            }
        }

        public void OnFinished()
        {

            GameInstanceRunning = false;

            if (Demos.Count == this.oldDemoCount)
            {
                Snackbar.Enqueue("No demos archived.");
            }
            else if (Demos.Count - 1 == this.oldDemoCount)
            {
                string archivedCount = (Demos.Count - this.oldDemoCount).ToString();
                Snackbar.Enqueue(archivedCount + " demo was archived.");
            }
            else
            {
                string archivedCount = (Demos.Count - this.oldDemoCount).ToString();
                Snackbar.Enqueue(archivedCount + " demos were archived.");
            }

            NotifyOfPropertyChange(() => Snackbar);
        }

        public bool CanLaunchForSteam
        {
            get
            {
                return !String.IsNullOrWhiteSpace(TxtBlkGamePath) && !GameInstanceRunning;
            }
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
            this.oldDemoCount = Demos.Count;
            GameInstanceRunning = true;
            if (LaunchForSteam)
            {
                Task.Run(() =>
                {
                    try
                    {
                        using (var p = new Process())
                        {
                            p.StartInfo.FileName = SelectedGameManagerModel.GamePath;
                            p.StartInfo.Arguments = "-steam " + SelectedGameManagerModel.LaunchArgs;
                            p.Start();
                            while (!p.HasExited)
                            {
                                Thread.Sleep(1000);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(
                        e.Message,
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    }
                }).ContinueWith(result => OnFinished());               
            }
            else
            {
                Task.Run(() => SelectedGameManagerModel.Start(SelectedGameManagerModel.LaunchArgs)).ContinueWith(result => OnFinished());
            }        
        }

        public bool CanDeleteManager
        {
            get
            {
                return !String.IsNullOrWhiteSpace(TxtBlkGamePath) && !GameInstanceRunning;
            }           
        }

        public void DeleteManager(string txtBlkGamePath)
        {

            GameManager gm =
                gmInfo.GameManagersList.SingleOrDefault(s => s.GamePath == txtBlkGamePath);

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
