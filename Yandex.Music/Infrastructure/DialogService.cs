using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Yandex.Music.Infrastructure.Constants;
using Yandex.Music.Views;

namespace Yandex.Music.Infrastructure;

/// <summary>
/// Служба работы со всплывающими окнами.
/// </summary>
internal class DialogService
{
    public LinkedList<ViewContext> CallContext { get; } = new();

    private Task<DialogResult> dialogClosedTask;
    private ViewContext lastLayer;
    private NavigationParameters parameters = new();
    private DialogWindow dialog;

    public Task<DialogResult> ShowDialog(string viewName) {

        if (string.IsNullOrEmpty(viewName))
            throw new ArgumentNullException(nameof(viewName), "Название переключаемого представления не может быть пустым.");
        if (CallContext.Count == 0) {
            dialogClosedTask = CreateAwaitableDialogWindow(viewName);
            return dialogClosedTask;
        }
        ViewContext newLayer = new() {
            RegionManager = lastLayer.RegionManager,
            ViewName = viewName,
        };
        CallContext.AddLast(newLayer);
        newLayer.RegionManager.RequestNavigate(RegionNames.Dialog, viewName, parameters);
        return dialogClosedTask;
    }

    public void GoBack() {
        lastLayer = GetLastLayer();
        CallContext.RemoveLast();
        if (CallContext.Count == 0) {
            CloseDialog();
            return;
        }
        string prevViewName = GetLastLayer()?.ViewName;
        lastLayer.RegionManager.RequestNavigate(RegionNames.Dialog, prevViewName, lastLayer.DialogResult.Parameters);
        lastLayer = GetLastLayer();
    }

    public void AddParameter(string name, object value) {
        parameters.Add(name, value);
    }

    public void SetResult(DialogResults result) {
        lastLayer.DialogResult.Result = result;
    }

    private ViewContext GetLastLayer() => CallContext?.Last?.Value;

    private void CloseDialog() {
        dialog?.Close();
    }

    private DialogWindow CreateDialogWindow(string viewName) {
        MainWindow mainWindow = GetWindow<MainWindow>();
        DialogWindow dialogWindow = new();
        dialogWindow.Closed += (s, e) => mainWindow.Show();
        dialogWindow.Closed += (s, e) => CallContext.Clear();
        dialogWindow.Closed += (s, e) => parameters = new();
        mainWindow.Hide();
        dialogWindow.Show();
        IRegionManager regionManager = ServiceLocator.GetService<IRegionManager>();
        IRegionManager scopedRegionManager = regionManager.CreateRegionManager();
        RegionManager.SetRegionManager(dialogWindow, scopedRegionManager);
        ViewContext newLayer = new() {
            RegionManager = scopedRegionManager,
            ViewName = viewName,
            DialogResult = new()
        };
        CallContext.AddLast(newLayer);
        newLayer.RegionManager.RequestNavigate(RegionNames.Dialog, viewName, parameters);
        lastLayer = newLayer;
        return dialogWindow;
    }

    private Task<DialogResult> CreateAwaitableDialogWindow(string viewName) {
        var tcs = new TaskCompletionSource<DialogResult>();
        dialog = CreateDialogWindow(viewName);
        dialog.Closed += (s, e) => tcs.SetResult(lastLayer.DialogResult);
        lastLayer.DialogResult.Parameters = parameters;
        return tcs.Task;
    }

    private static T GetWindow<T>() where T : Window =>
     Application.Current.Windows.OfType<T>().First();

    public class ViewContext
    {
        public IRegionManager RegionManager { get; set; } = null!;
        public string ViewName { get; set; } = null!;
        public DialogResult DialogResult { get; set; }
    }

    public class DialogResult
    {
        public DialogResults Result { get; set; } = DialogResults.Cancel;
        public NavigationParameters Parameters { get; set; }
    }

    public enum DialogResults { Ok, Error, Cancel }
}
