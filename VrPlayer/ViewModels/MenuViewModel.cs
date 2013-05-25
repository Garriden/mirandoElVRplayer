﻿using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Data;
using Microsoft.Win32;
using VrPlayer.Contracts;
using VrPlayer.Contracts.Distortions;
using VrPlayer.Contracts.Effects;
using VrPlayer.Contracts.Projections;
using VrPlayer.Contracts.Trackers;
using VrPlayer.Helpers.Converters;
using VrPlayer.Helpers.Mvvm;
using VrPlayer.Models.Metadata;
using VrPlayer.Models.Plugins;
using VrPlayer.Models.State;
using VrPlayer.Models.Config;

namespace VrPlayer.ViewModels
{
	public class MenuViewModel: ViewModelBase
	{
        private readonly IApplicationConfig _config;
        public IApplicationConfig Config
        {
            get { return _config; }
        }

        private readonly IApplicationState _state;
        public IApplicationState State
        {
            get { return _state; }
        }

        private readonly IPluginManager _pluginManager;
        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        #region Data

        public List<MenuItem> StereoInputMenu
        {
            get
            {
                var menuItems = new List<MenuItem>();
                foreach (var stereoMode in Enum.GetValues(typeof(StereoMode)))
                {
                    var menuItem = new MenuItem
                    {
                        Header = stereoMode.ToString(),
                        Command = new DelegateCommand(SetStereoInput),
                        CommandParameter = stereoMode,
                    };
                    var binding = new Binding
                    {
                        Source = _state,
                        Path = new PropertyPath("StereoInput"),
                        Converter = new CompareStringParameterConverter(),
                        ConverterParameter = stereoMode
                    };
                    menuItem.SetBinding(MenuItem.IsCheckedProperty, binding);
                    menuItems.Add(menuItem);
                }
                return menuItems;
            }
        }

        public List<MenuItem> StereoOutputMenu
        {
            get
            {
                var menuItems = new List<MenuItem>();
                foreach (var stereoMode in Enum.GetValues(typeof(StereoMode)))
                {
                    var menuItem = new MenuItem
                    {
                        Header = stereoMode.ToString(),
                        Command = new DelegateCommand(SetStereoOutput),
                        CommandParameter = stereoMode,
                    };
                    var binding = new Binding
                    {
                        Source = _state,
                        Path = new PropertyPath("StereoOutput"),
                        Converter = new CompareStringParameterConverter(),
                        ConverterParameter = stereoMode
                    };
                    menuItem.SetBinding(MenuItem.IsCheckedProperty, binding);
                    menuItems.Add(menuItem);
                }
                return menuItems;
            }
        }
        
        #endregion

        #region Commands

        private readonly ICommand _loadCommand;
        public ICommand LoadCommand
        {
            get { return _loadCommand; }
        }
        
        private readonly ICommand _openFileCommand;
        public ICommand OpenFileCommand
        {
            get { return _openFileCommand; }
        }

        private readonly ICommand _openUrlCommand;
        public ICommand OpenUrlCommand
        {
            get { return _openUrlCommand; }
        }        
        
        private readonly ICommand _browseSamplesCommand;
        public ICommand BrowseSamplesCommand
        {
            get { return _browseSamplesCommand; }
        }

        private readonly ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get { return _exitCommand; }
        }

        private readonly ICommand _settingsCommand;
        public ICommand SettingsCommand
        {
            get { return _settingsCommand; }
        }
        
        private readonly ICommand _launchWebBrowserCommand;
        public ICommand LaunchWebBrowserCommand
        {
            get { return _launchWebBrowserCommand; }
        }

        private readonly ICommand _aboutCommand;
        public ICommand AboutCommand
        {
            get { return _aboutCommand; }
        }

        private readonly ICommand _changeProjectionCommand;
        public ICommand ChangeProjectionCommand
        {
            get { return _changeProjectionCommand; }
        }

        private readonly ICommand _changeEffectCommand;
        public ICommand ChangeEffectCommand
        {
            get { return _changeEffectCommand; }
        }

        private readonly ICommand _changeDistortionCommand;
        public ICommand ChangeDistortionCommand
        {
            get { return _changeDistortionCommand; }
        }

        private readonly ICommand _changeTrackerCommand;
        public ICommand ChangeTrackerCommand
        {
            get { return _changeTrackerCommand; }
        }
        
        #endregion

        public MenuViewModel(IApplicationState state, IPluginManager pluginManager, IApplicationConfig config)
        {
            _pluginManager = pluginManager;
            _state = state;
            _config = config;

            //Commands
            _loadCommand = new DelegateCommand(Load);
            _openFileCommand = new DelegateCommand(OpenFile);
            _openUrlCommand = new DelegateCommand(OpenUrl);
            _browseSamplesCommand = new DelegateCommand(BrowseSamples);
            _exitCommand = new DelegateCommand(Exit);
            _changeProjectionCommand = new DelegateCommand(SetProjection);
            _changeEffectCommand = new DelegateCommand(SetEffect);
            _changeDistortionCommand = new DelegateCommand(SetDistortion);
            _changeTrackerCommand = new DelegateCommand(SetTracker);
            _settingsCommand = new DelegateCommand(ShowSettings);
            _launchWebBrowserCommand = new DelegateCommand(LaunchWebBrowser);
            _aboutCommand = new DelegateCommand(ShowAbout);

            //Todo: Extract Default values
            _state.EffectPlugin = _state.EffectPlugin ?? _pluginManager.Effects.FirstOrDefault();
            _state.ProjectionPlugin = _state.ProjectionPlugin ?? _pluginManager.Projections.FirstOrDefault();
            _state.TrackerPlugin = _state.TrackerPlugin ?? _pluginManager.Trackers.FirstOrDefault();
            _state.DistortionPlugin = _state.DistortionPlugin ?? _pluginManager.Distortions.FirstOrDefault();
            
            //Todo: Should not set the media value directly
            string[] parameters = Environment.GetCommandLineArgs();
            if (parameters.Length > 1)
            {
                Uri uri = new Uri(parameters[1]);
                string uriWithoutScheme = uri.Host + uri.PathAndQuery;
                _state.MediaPlayer.Source = new Uri(uriWithoutScheme, UriKind.RelativeOrAbsolute);
            }
            else
            {
                //Todo: Use embeded resource for default media file
                var samples = new DirectoryInfo(_config.SamplesFolder);
                if (samples.GetFiles().Any())
                {
                    _state.MediaPlayer.Source = new Uri(samples.GetFiles().First().FullName, UriKind.RelativeOrAbsolute);
                }
            }
            _state.MediaPlayer.Play();
        }

        #region Logic

        private void OpenFile(object o)
        {
            //Todo: Extract dialog to UI layer
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Movies|*.avi;*.flv;*.f4v;*.mp4;*.wmv;*.mpeg;*.mkv|Images|*.jpg;*.jpeg;*.png;*.bmp;*.gif|All Files|*.*";
            if (openFileDialog.ShowDialog().Value)
            {
                Load(openFileDialog.FileName);
            }
        }

        private void OpenUrl(object o)
        {
            var dialog = new UrlInputDialog();
            if (dialog.ShowDialog() == true)
            {
                Load(dialog.ResponseText);
            }
        }

		private void Load(object o)
		{
			var filePath = (string)o;
            
            if (_config.MetaDataReadOnLoad)
            {
                try
                {
                    //Todo: Extract metadata parsing
                    var parser = new MetadataParser(filePath);
                    var metadata = parser.Parse();

                    if (!string.IsNullOrEmpty(metadata.ProjectionType))
                    {
                        _state.ProjectionPlugin = _pluginManager.Projections.FirstOrDefault(
                            plugin => plugin.Content.GetType().FullName == metadata.ProjectionType);
                    }

                    if (!string.IsNullOrEmpty(metadata.FormatType))
                    {
                        _state.StereoInput = (StereoMode)Enum.Parse(typeof(StereoMode), metadata.FormatType);
                        //Todo: Stereo input should not be assigned to Projection manually
                        if (_state.ProjectionPlugin != null)
                            _state.ProjectionPlugin.Content.StereoMode = _state.StereoInput;
                    }

                    if (!string.IsNullOrEmpty(metadata.Effects))
                    {
                        _state.EffectPlugin = _pluginManager.Effects
                            .Where(plugin => plugin.Content != null)
                            .FirstOrDefault(plugin => plugin.Content.GetType().FullName == metadata.Effects);
                    }
                }
                catch (Exception exc)
                {
                    //Todo: log
                    MessageBox.Show(String.Format("Unable to parse meta data from file '{0}: {1}'", filePath, exc.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }    
            }
            
            try
            {
                _state.MediaPlayer.Source = new Uri(filePath, UriKind.Absolute);
            }
            catch (Exception exc)
            {
                //Todo: log
                MessageBox.Show(String.Format("Unable to load '{0}'", filePath), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BrowseSamples(object o)
		{   
            var dirInfo = new DirectoryInfo(_config.SamplesFolder);
            if (Directory.Exists(dirInfo.FullName))
            {
                Process.Start(dirInfo.FullName);
            }
            else
            {
                MessageBox.Show(string.Format("Invalid samples directory: '{0}'.", _config.SamplesFolder), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LaunchWebBrowser(object o)
		{
            Process.Start("http://www.vrplayer.tv");
        }

        private void Exit(object o)
        {
            //Todo: Close windows instead. Move to UI
            App.Current.Shutdown();
        }

        private void SetEffect(object o)
        {
            _state.EffectPlugin = (IPlugin<EffectBase>)o;
        }

        private void SetStereoInput(object o)
        {
            _state.StereoInput = (StereoMode)o;
            _state.ProjectionPlugin.Content.StereoMode = _state.StereoInput;
        }

        private void SetStereoOutput(object o)
        {
            _state.StereoOutput = (StereoMode)o;
        }

        private void SetProjection(object o)
        {
            var projectionPlugin = (IPlugin<IProjection>)o;
            projectionPlugin.Content.StereoMode = _state.StereoInput;
            _state.ProjectionPlugin = projectionPlugin;
        }

        private void SetTracker(object o)
        {
            _state.TrackerPlugin = (IPlugin<ITracker>)o;
        }

        private void SetDistortion(object o)
        {
            _state.DistortionPlugin = (IPlugin<DistortionBase>)o;
        }

        private void ShowSettings(object o)
        {
            var window = Application.Current.Windows.Cast<Window>().SingleOrDefault(w => w.GetType() == typeof(SettingsWindow));
            if (window != null) 
            {
                window.Activate();
            }
            else 
            {
                window = new SettingsWindow();
                window.Show();
            }
        }

        private void ShowAbout(object o)
        {
            //Todo: Extract to UI layer
            MessageBox.Show(
                string.Format("VRPlayer ({0})", Assembly.GetExecutingAssembly().GetName().Version) +
                Environment.NewLine +
                Environment.NewLine +
                "(c)Stephane Levesque 2012-2013",    
                "About", 
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        #endregion
    }
}
