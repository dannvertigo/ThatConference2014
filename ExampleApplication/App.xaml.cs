﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace ExampleApplication
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                // Set the default language
                rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();

            RegisterMaintenanceBackgroundTask();
            RegisterTimerBackgroundTaskAsync();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }



        private const string DownloadTimerTaskName = "DownloadTimerTask";

        private async Task RegisterTimerBackgroundTaskAsync() 
        {
            IBackgroundTaskRegistration downloadTimereTask = BackgroundTaskRegistration.AllTasks.SingleOrDefault(x => x.Value.Name == DownloadTimerTaskName).Value;

            if (downloadTimereTask == null)
            {
                await DispatcherHelper.RunOnUIThreadAsync(async () =>
                {
                    //request access
                    var currentStatus = await BackgroundExecutionManager.RequestAccessAsync();

                    if (currentStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                    currentStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
                    {

                        var builder = new BackgroundTaskBuilder();

                        builder.Name = DownloadTimerTaskName;
                        builder.TaskEntryPoint = "ExampleBackgroundTask.DownloadFilesTask";
                        builder.SetTrigger(new TimeTrigger(15, false));
                        builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                        builder.CancelOnConditionLoss = true;

                        downloadTimereTask = builder.Register();

                        downloadTimereTask.Completed += BackgroundTask_Completed;
                    }
                });
            }
            else 
            {
                downloadTimereTask.Completed += BackgroundTask_Completed;
            }  
        }


        private const string DownloadMaintenanceTaskName = "DownloadMaintenanceTask";
        private void RegisterMaintenanceBackgroundTask() 
        {
            IBackgroundTaskRegistration downloadMaintenanceTask = BackgroundTaskRegistration.AllTasks.SingleOrDefault(x => x.Value.Name == DownloadMaintenanceTaskName).Value;

            if (downloadMaintenanceTask == null)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = DownloadMaintenanceTaskName;
                builder.TaskEntryPoint = "ExampleBackgroundTask.DownloadFilesTask";
                builder.SetTrigger(new MaintenanceTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
                builder.CancelOnConditionLoss = true;

                downloadMaintenanceTask = builder.Register(); 
            }

            downloadMaintenanceTask.Completed += BackgroundTask_Completed;
        }

        void BackgroundTask_Completed(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            //notify user
        }
    }
}
