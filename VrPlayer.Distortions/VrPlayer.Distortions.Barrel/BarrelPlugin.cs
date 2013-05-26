﻿using System.ComponentModel.Composition;
using System.Configuration;
using VrPlayer.Contracts;
using VrPlayer.Contracts.Distortions;
using VrPlayer.Helpers;

namespace VrPlayer.Distortions.Barrel
{
    [Export(typeof(IPlugin<DistortionBase>))]
    public class BarrelPlugin : PluginBase<DistortionBase>
    {
        private static readonly Configuration Config = ConfigHelper.LoadConfig();

        public BarrelPlugin()
        {
            Name = "Barrel";
            var effect = new BarrelEffect();
            Content = effect;
            Panel = null;
        }
    }
}