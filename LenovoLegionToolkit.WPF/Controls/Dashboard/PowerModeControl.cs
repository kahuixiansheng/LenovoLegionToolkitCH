﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Features;
using LenovoLegionToolkit.Lib.Listeners;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Utils;
using LenovoLegionToolkit.WPF.Windows.Dashboard;
using Wpf.Ui.Common;
using Button = Wpf.Ui.Controls.Button;

namespace LenovoLegionToolkit.WPF.Controls.Dashboard
{
    public class PowerModeControl : AbstractComboBoxFeatureCardControl<PowerModeState>
    {
        private readonly PowerModeListener _powerModeListener = IoCContainer.Resolve<PowerModeListener>();
        private readonly PowerPlanListener _powerPlanListener = IoCContainer.Resolve<PowerPlanListener>();

        private readonly Button _configButton = new()
        {
            Icon = SymbolRegular.Settings24,
            FontSize = 20,
            Margin = new(8, 0, 0, 0),
            Visibility = Visibility.Collapsed,
        };

        public PowerModeControl()
        {
            Icon = SymbolRegular.Gauge24;
            Title = "性能模式";
            Subtitle = "性能模式切换。\n可使用Fn+Q快速切换。";

            _powerModeListener.Changed += PowerModeListener_Changed;
            _powerPlanListener.Changed += PowerPlanListener_Changed;
        }

        private void PowerModeListener_Changed(object? sender, PowerModeState e) => Dispatcher.Invoke(async () =>
        {
            if (IsLoaded && IsVisible)
                await RefreshAsync();
        });

        private void PowerPlanListener_Changed(object? sender, EventArgs e) => Dispatcher.Invoke(async () =>
        {
            if (IsLoaded && IsVisible)
                await RefreshAsync();
        });

        protected override async Task OnStateChange(ComboBox comboBox, IFeature<PowerModeState> feature, PowerModeState? newValue, PowerModeState? oldValue)
        {
            try
            {
                await base.OnStateChange(comboBox, feature, newValue, oldValue);

                if (comboBox.TryGetSelectedItem(out PowerModeState state) && state == PowerModeState.GodMode)
                {
                    _configButton.ToolTip = "Settings";
                    _configButton.Visibility = Visibility.Visible;
                }
                else
                    _configButton.Visibility = Visibility.Collapsed;
            }
            catch (InvalidOperationException ex)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"切换性能模式失败。", ex);

                await SnackbarHelper.ShowAsync("无法切换性能模式", ex.Message, true);
            }
        }

        protected override FrameworkElement? GetAccessory(ComboBox comboBox)
        {
            _configButton.Click += ConfigButton_Click;

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            stackPanel.Children.Add(_configButton);
            stackPanel.Children.Add(comboBox);

            return stackPanel;
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (_comboBox.TryGetSelectedItem(out PowerModeState state) && state == PowerModeState.GodMode)
            {
                var window = new GodModeSettingsWindow
                {
                    Owner = Window.GetWindow(this),
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ShowInTaskbar = false,
                };
                window.ShowDialog();
            }
        }
    }
}
