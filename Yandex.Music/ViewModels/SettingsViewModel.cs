using Force.DeepCloner;
using Microsoft.Win32;
using Prism.Commands;
using System.Windows.Input;
using Yandex.Music.Infrastructure;
using Yandex.Music.Settings;

namespace Yandex.Music.ViewModels;
internal class SettingsViewModel : ViewModelBase
{
    public RootSettings RootSettings { get; set; }

    public string DownloadResultDirectoryPath { get; set; }

    public bool ApplyAndCancelButtonsVisibility { get; set; }

    private static RootSettings GetSettingsClone() => ConfigService.GetSettings().DeepClone();

    #region Command Initializing - Команда инициализация

    private ICommand _InitializingCommand;

    /// <summary>Команда - инициализация</summary>
    public ICommand InitializingCommand => _InitializingCommand
        ??= new DelegateCommand(OnInitializingCommandExecuted);

    private void OnInitializingCommandExecuted() {
        RootSettings = GetSettingsClone();
        DownloadResultDirectoryPath = RootSettings.CoreService.DownloadResultDirectoryPath;
    }

    #endregion

    #region Command Apply - Команда применить настройки

    private ICommand _ApplyCommand;

    /// <summary>Команда - применить настройки</summary>
    public ICommand ApplyCommand => _ApplyCommand
        ??= new DelegateCommand(OnApplyCommandExecuted);

    private void OnApplyCommandExecuted() {
        ConfigService.SetSettings(RootSettings);
        RootSettings = GetSettingsClone();
        ApplyAndCancelButtonsVisibility = false;
    }

    #endregion

    #region Command Cancel - Команда отменить

    private ICommand _CancelCommand;

    /// <summary>Команда - отменить</summary>
    public ICommand CancelCommand => _CancelCommand
        ??= new DelegateCommand(OnCancelCommandExecuted);

    private void OnCancelCommandExecuted() {
        RootSettings = GetSettingsClone();
        ApplyAndCancelButtonsVisibility = false;
    }

    #endregion

    #region Command Refresh - Команда обновление значений

    private ICommand _RefreshCommand;

    /// <summary>Команда - обновление значений</summary>
    public ICommand RefreshCommand => _RefreshCommand
        ??= new DelegateCommand(OnRefreshCommandExecuted);

    private void OnRefreshCommandExecuted() {
        ApplyAndCancelButtonsVisibility = true;
    }

    #endregion

    #region Command SelectPath - Команда выбрать путь

    private ICommand _SelectPathCommand;

    /// <summary>Команда - выбрать путь</summary>
    public ICommand SelectPathCommand => _SelectPathCommand
        ??= new DelegateCommand(OnSelectPathCommandExecuted);

    private void OnSelectPathCommandExecuted() {
        string path = new PathDialog().Open();
        if (string.IsNullOrWhiteSpace(path))
            return;
        DownloadResultDirectoryPath = path;
        RootSettings.CoreService.DownloadResultDirectoryPath = path;
    }

    #endregion
}
