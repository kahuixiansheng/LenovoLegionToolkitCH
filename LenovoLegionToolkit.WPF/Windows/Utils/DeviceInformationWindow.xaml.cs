﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LenovoLegionToolkit.Lib;
using LenovoLegionToolkit.Lib.Utils;
using LenovoLegionToolkit.WPF.Extensions;
using Wpf.Ui.Controls;

namespace LenovoLegionToolkit.WPF.Windows.Utils
{
    public partial class DeviceInformationWindow
    {
        private readonly WarrantyChecker _warrantyChecker = IoCContainer.Resolve<WarrantyChecker>();

        public DeviceInformationWindow()
        {
            InitializeComponent();

            ResizeMode = ResizeMode.NoResize;

            _titleBar.UseSnapLayout = false;
            _titleBar.CanMaximize = false;

            Loaded += DeviceInformationWindow_Loaded;
        }

        private async void DeviceInformationWindow_Loaded(object sender, RoutedEventArgs e) => await RefreshAsync();

        private async Task RefreshAsync(bool forceRefresh = false)
        {
            var mi = await Compatibility.GetMachineInformation();

            _manufacturerLabel.Content = mi.Vendor;
            _modelLabel.Content = mi.Model;
            _mtmLabel.Content = mi.MachineType;
            _serialNumberLabel.Content = mi.SerialNumber;
            _biosLabel.Content = mi.BIOSVersion;

            try
            {
                _refreshWarrantyButton.IsEnabled = false;

                _warrantyStatusLabel.Content = "-";
                _warrantyStartLabel.Content = "-";
                _warrantyEndLabel.Content = "-";
                _warrantyLinkCardAction.Tag = null;
                _warrantyLinkCardAction.IsEnabled = false;

                var warrantyInfo = await _warrantyChecker.GetWarrantyInfo(mi, forceRefresh);
                _warrantyLinkCardAction.IsEnabled = false;

                if (!warrantyInfo.HasValue)
                    return;

                _warrantyStatusLabel.Content = warrantyInfo.Value.Status ?? "-";
                _warrantyStartLabel.Content = warrantyInfo.Value.Start is not null ? $"{warrantyInfo.Value.Start:d}" : "-";
                _warrantyEndLabel.Content = warrantyInfo.Value.End is not null ? $"{warrantyInfo.Value.End:d}" : "-";
                _warrantyLinkCardAction.Tag = warrantyInfo.Value.Link;

                _warrantyLinkCardAction.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"无法加载保修信息。", ex);
            }
            finally
            {
                _refreshWarrantyButton.IsEnabled = true;
            }
        }

        private async void RefreshWarrantyButton_OnClick(object sender, RoutedEventArgs e) => await RefreshAsync(true);

        private async void DeviceCardControl_Click(object sender, RoutedEventArgs e)
        {
            if (((sender as CardControl)?.Content as Label)?.Content is not string str)
                return;

            try
            {
                Clipboard.SetText(str);
                await _snackBar.ShowAsync("已复制！", $"\"{str}\" 已复制到剪贴板。");
            }
            catch (Exception ex)
            {
                if (Log.Instance.IsTraceEnabled)
                    Log.Instance.Trace($"无法复制到剪贴板", ex);
            }
        }

        private void WarrantyLinkCardAction_OnClick(object sender, RoutedEventArgs e)
        {
            var link = _warrantyLinkCardAction.Tag as Uri;
            link?.Open();
        }
    }
}
