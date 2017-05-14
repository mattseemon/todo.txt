using System;
using System.Collections.Generic;
using Caliburn.Micro;
using NLog.Config;
using NLog.Targets;
using Seemon.Todo.Utilities;
using Splat;
using Akavache;
using Seemon.Todo.ViewModels;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using Hardcodet.Wpf.TaskbarNotification;
using ReactiveUI;
using System.Windows.Threading;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Squirrel;
using System.Threading.Tasks;

namespace Seemon.Todo
{
    public class AppBootstrapper : BootstrapperBase, IEnableLogger
    {
        private UserSettings settings = null;
        private IUpdateManager updateManager = null;
        public bool ShowWelcomeWindow;

        private TaskbarIcon notifyIcon = null;

        public static IBlobCache OldBlobCache = null;
        

        public ReactiveCommand<object> EnableDebugLoggingCommand { get; private set; }

        static AppBootstrapper()
        {
            BlobCache.ApplicationName = "todotxt";
        }

        public AppBootstrapper()
        {
            InitializeUpdater();
            Initialize();
        }

        public void InitializeUpdater()
        {
            try
            {
#if DEBUG
                updateManager = new UpdateManager(AppInfo.UpdateLocation, "todo.txt");
#else
                updateManager = UpdateManager.GitHubUpdateManager(AppInfo.UpdateLocation, "todo.txt").Result;
#endif
                SquirrelAwareApp.HandleEvents(
                    onInitialInstall: v =>
                    {
                        this.Log().Info("TODO.TXT - On Initial Install Run");
                        updateManager.CreateShortcutForThisExe();
                    },
                    onAppUpdate: v =>
                    {
                        updateManager.CreateShortcutForThisExe();
                        updateManager.CreateShortcutsForExecutable("todotxt.exe", ShortcutLocation.Startup, true, string.Empty, null);
                    },
                    onAppUninstall: v => updateManager.RemoveShortcutForThisExe(),
                    onFirstRun: () => ShowWelcomeWindow = true);
            }
            catch(Exception ex)
            {
                this.Log().ErrorException("Updated Failed", ex);
            }
        }

        protected override void Configure()
        {
            this.ConfigureLogging();
        }

        protected override object GetInstance(Type service, string key)
        {
            return Locator.Current.GetService(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return Locator.Current.GetServices(service);
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            this.Log().Info("todo.txt is starting...");
            this.Log().Info("**********************************");
            this.Log().Info("**                              **");
            this.Log().Info("**           TODO.TXT           **");
            this.Log().Info("**                              **");
            this.Log().Info("**********************************");
            this.Log().Info("Application version: " + AppInfo.Version);
            this.Log().Info("OS Version: " + Environment.OSVersion.VersionString);
            this.Log().Info("Current culture: " + CultureInfo.InstalledUICulture.Name);

#if PORTABLE
            BlobCache.UserAccount = new Akavache.Sqlite3.SQLitePersistentBlobCache(AppInfo.PortableStoragePath, BlobCache.TaskpoolScheduler);
#endif

            this.settings = new UserSettings(BlobCache.UserAccount);
            Locator.CurrentMutable.RegisterConstant(this.settings, typeof(UserSettings));
            Locator.CurrentMutable.RegisterLazySingleton(() => new WindowManager(), typeof(IWindowManager));

            this.EnableDebugLoggingCommand = ReactiveCommand.Create();
            this.EnableDebugLoggingCommand.Subscribe(x => this.EnableDebugLogging());

            this.settings.WhenAnyValue(x => x.EnableDebugLogging).InvokeCommand(this, x => x.EnableDebugLoggingCommand);

            
            Locator.CurrentMutable.RegisterLazySingleton(() => new ShellViewModel(), typeof(ShellViewModel));
            Locator.CurrentMutable.Register(() => this.updateManager, typeof(IUpdateManager));

            this.notifyIcon = (TaskbarIcon)App.Current.FindResource("NotifyIcon");
            Locator.CurrentMutable.Register(() => this.notifyIcon, typeof(TaskbarIcon));

            CultureInfo.CurrentCulture.ClearCachedData();
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            this.ConfigureLager();
            this.DisplayRootViewFor<ShellViewModel>();
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                return;

            this.Log().FatalException("An unhandled exception occured, opeing the crash report.", e.Exception);

            if (this.Application.MainWindow != null)
                this.Application.MainWindow.Hide();

            var windowManager = Locator.Current.GetService<IWindowManager>();
            windowManager.ShowDialog(new CrashViewModel(e.Exception));

            e.Handled = true;

            Application.Current.Shutdown();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            this.Log().Info("Starting todo.txt shutdown");

            if (updateManager != null)
                this.updateManager.Dispose();

            this.Log().Info("Shutting down BlobCache");
            BlobCache.Shutdown().Wait();

            this.Log().Info("Shutting down NLog");
            NLog.LogManager.Shutdown();

            this.Log().Info("Shutdoown Complete");
        }

        private void EnableDebugLogging()
        {
            foreach (var rule in NLog.LogManager.Configuration.LoggingRules)
            {
                if (this.settings.EnableDebugLogging)
                    rule.EnableLoggingForLevel(NLog.LogLevel.Debug);
                else
                    rule.EnableLoggingForLevel(NLog.LogLevel.Info);
            }
            NLog.LogManager.ReconfigExistingLoggers();
        }

        private void ConfigureLogging()
        {
            var logConfig = new LoggingConfiguration();

            var target = new FileTarget()
            {
                FileName = AppInfo.LogFilePath,
                Layout = @"${longdate}|${level}|${message} ${exception:format=ToString,StackTrace}",
                ArchiveAboveSize = 1024 * 1024 * 2,
                ArchiveNumbering = ArchiveNumberingMode.Sequence
            };

            logConfig.LoggingRules.Add(new LoggingRule("*", NLog.LogLevel.Info, target));
            NLog.LogManager.Configuration = logConfig;

            Locator.CurrentMutable.RegisterConstant(new Logger(NLog.LogManager.GetCurrentClassLogger()), typeof(ILogger));
        }

        private void ConfigureLager()
        {
            this.Log().Info("Initializing Lager settings storage.");
            this.settings.InitializeAsync().Wait();
            this.Log().Info("Settings storage initialized.");
        }

        private void StartUpWindows(bool enable)
        {
            if (enable)
                StartUpManager.AddApplicationToCurrentUserStartup();
            else
                StartUpManager.RemoveApplicationFromCurrentUserStartup();
        }

        public static void MigrateSettings(IBlobCache from, IBlobCache to)
        {
            Lager.SettingsStorage fromSettings = new UserSettings(from);
            Lager.SettingsStorage toSettings = new UserSettings(to);

            foreach (PropertyInfo setting in fromSettings.GetType().GetProperties())
            {
                toSettings.GetType().GetProperty(setting.Name).SetValue(toSettings, setting.GetValue(fromSettings));
            }
        }
    }
}