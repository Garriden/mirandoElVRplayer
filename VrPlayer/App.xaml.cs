﻿using System.Windows;

using VrPlayer.Models;
using VrPlayer.Models.Config;
using VrPlayer.Models.Media;
using VrPlayer.Models.Plugins;
using VrPlayer.Models.State;
using VrPlayer.ViewModels;
using WPFMediaKit.DirectShow.Controls;

namespace VrPlayer
{
    public partial class App : Application
    {
        private readonly ViewModelFactory _viewModelFactory;
        public ViewModelFactory ViewModelFactory
        {
            get { return _viewModelFactory; }
        }

        private App()
        {
            IApplicationConfig config = new AppSettingsApplicationConfig();
            _audioEngine = new X3DAudioEngine();
            GraphPlayerElement mediaPlayer = new GraphPlayerElement();
            IPluginManager pluginManager = new StaticPluginManager(config);
            IApplicationState state = new DefaultApplicationState(config, mediaPlayer, _audioEngine);
            _viewModelFactory = new ViewModelFactory(config, pluginManager, state);
        }

        //Todo: Dispose all elements
        IAudioEngine _audioEngine;
            
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            _audioEngine.Dispose();
        }
    }
}
