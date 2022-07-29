using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Xaml.Behaviors.Input;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.UI.Services;
using PuppetMaster.Client.UI.ViewModels;
using PuppetMaster.Client.UI.ViewModels.Internal;
using PuppetMaster.Client.Valorant.Api;
using Squirrel;

namespace PuppetMaster.Client.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container;
        private ShellViewModel? _shellViewModel;

        public Bootstrapper()
        {
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: OnAppInstall,
                onAppUninstall: OnAppUninstall,
                onEveryRun: OnAppRun);

            _container = new SimpleContainer();
            Initialize();
        }

        public SimpleContainer Container => _container;

        protected override void Configure()
        {
            ////if (DateTime.Now > new DateTime(2022, 07, 21))
            ////{
            ////    Application.Current.Shutdown();
            ////}

            ConfigureParser();
            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IBackendFacade, BackendFacade>();

            _container.Singleton<ShellViewModel>();
            _container.Singleton<MainViewModel>();

            _container.Singleton<LoginRegisterViewModel>();
            _container.PerRequest<LoginViewModel>();
            _container.PerRequest<RegisterViewModel>();

            _container.Singleton<AccountViewModel>();
            _container.PerRequest<UpdateUserViewModel>();
            _container.PerRequest<ChangePasswordViewModel>();

            _container.Singleton<IGameService, ValorantGameService>(ValorantGameService.GameName);

#if DEBUG
            _container.Singleton<InternalShellViewModel>();
            _container.Singleton<IInternalShellTabItem, ControlsViewModel>();
#endif

            // Valorant specific
            var valorantClient = new ValorantClient();
            valorantClient.Start();
            _container.Instance<IValorantClient>(valorantClient);

            // Global Instances
            _container.Instance(DialogCoordinator.Instance);
            _container.Instance(Application);
            _container.Instance(this);
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container!.GetInstance(service, key);
        }

        protected override void BuildUp(object instance)
        {
            _container!.BuildUp(instance);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container!.GetAllInstances(service);
        }

        protected override async void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var eventAggregator = IoC.Get<IEventAggregator>();
            if (eventAggregator != null)
            {
                var errorMessage = new ErrorMessage()
                {
                    Title = "Unhandled error",
                    Message = e.Exception.Message
                };

                await eventAggregator.PublishOnUIThreadAsync(errorMessage);
            }
            else
            {
                MessageBox.Show(e.Exception.Message, "Unhandled error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            e.Handled = true;
            base.OnUnhandledException(sender, e);
        }

        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            _shellViewModel = IoC.Get<ShellViewModel>();
            var windowManager = IoC.Get<IWindowManager>();
            await windowManager.ShowWindowAsync(_shellViewModel);
        }

        protected override async void OnExit(object sender, EventArgs e)
        {
            if (_shellViewModel != null)
            {
                await _shellViewModel.DeactivateAsync(true);
            }

            base.OnExit(sender, e);
        }

        private static void OnAppInstall(SemanticVersion version, IAppTools tools)
        {
            tools.CreateShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private static void OnAppUninstall(SemanticVersion version, IAppTools tools)
        {
            tools.RemoveShortcutForThisExe(ShortcutLocation.StartMenu | ShortcutLocation.Desktop);
        }

        private static void OnAppRun(SemanticVersion version, IAppTools tools, bool firstRun)
        {
            tools.SetProcessAppUserModelId();
        }

        private static void ConfigureParser()
        {
            var defaultCreateTrigger = Parser.CreateTrigger;

            Parser.CreateTrigger = (target, triggerText) =>
            {
                if (triggerText == null)
                {
                    return defaultCreateTrigger(target, null);
                }

                var triggerDetail = triggerText
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty);

                var splits = triggerDetail.Split(null as char[], StringSplitOptions.RemoveEmptyEntries);
                switch (splits[0])
                {
                    case "Key":
                        var key = (Key)Enum.Parse(typeof(Key), splits[1], true);
                        return new KeyTrigger { Key = key };
                }

                return defaultCreateTrigger(target, triggerText);
            };
        }
    }
}
