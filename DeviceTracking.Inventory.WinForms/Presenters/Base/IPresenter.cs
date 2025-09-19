using System;
using System.Threading.Tasks;

namespace DeviceTracking.Inventory.WinForms.Presenters.Base;

/// <summary>
/// Base interface for all presenters in the MVP pattern
/// </summary>
public interface IPresenter
{
    /// <summary>
    /// Initialize the presenter
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Cleanup resources
    /// </summary>
    void Cleanup();
}

/// <summary>
/// Base interface for presenters that manage a specific view
/// </summary>
public interface IPresenter<TView> : IPresenter where TView : IView
{
    /// <summary>
    /// The view managed by this presenter
    /// </summary>
    TView View { get; }

    /// <summary>
    /// Initialize the presenter with a view
    /// </summary>
    /// <param name="view">The view to manage</param>
    Task InitializeAsync(TView view);
}

/// <summary>
/// Base interface for all views in the MVP pattern
/// </summary>
public interface IView
{
    /// <summary>
    /// Show the view
    /// </summary>
    void Show();

    /// <summary>
    /// Hide the view
    /// </summary>
    void Hide();

    /// <summary>
    /// Close the view
    /// </summary>
    void Close();

    /// <summary>
    /// Event raised when the view is loaded
    /// </summary>
    event EventHandler? ViewLoaded;

    /// <summary>
    /// Event raised when the view is closing
    /// </summary>
    event EventHandler? ViewClosing;
}

/// <summary>
/// Base interface for views that display data
/// </summary>
public interface IDataView : IView
{
    /// <summary>
    /// Set the loading state
    /// </summary>
    /// <param name="isLoading">Whether the view is loading data</param>
    void SetLoadingState(bool isLoading);

    /// <summary>
    /// Show an error message
    /// </summary>
    /// <param name="message">The error message</param>
    void ShowError(string message);

    /// <summary>
    /// Show a success message
    /// </summary>
    /// <param name="message">The success message</param>
    void ShowSuccess(string message);

    /// <summary>
    /// Show an information message
    /// </summary>
    /// <param name="message">The information message</param>
    void ShowInformation(string message);

    /// <summary>
    /// Show a confirmation dialog
    /// </summary>
    /// <param name="message">The confirmation message</param>
    /// <param name="title">The dialog title</param>
    /// <returns>True if confirmed, false otherwise</returns>
    bool ShowConfirmation(string message, string title = "Confirm");
}

/// <summary>
/// Base interface for views that can be refreshed
/// </summary>
public interface IRefreshableView : IDataView
{
    /// <summary>
    /// Refresh the view data
    /// </summary>
    void RefreshData();

    /// <summary>
    /// Clear the view data
    /// </summary>
    void ClearData();
}

/// <summary>
/// Base interface for views that support CRUD operations
/// </summary>
public interface ICrudView : IRefreshableView
{
    /// <summary>
    /// Enable or disable new operation
    /// </summary>
    void SetNewEnabled(bool enabled);

    /// <summary>
    /// Enable or disable edit operation
    /// </summary>
    void SetEditEnabled(bool enabled);

    /// <summary>
    /// Enable or disable delete operation
    /// </summary>
    void SetDeleteEnabled(bool enabled);

    /// <summary>
    /// Enable or disable save operation
    /// </summary>
    void SetSaveEnabled(bool enabled);

    /// <summary>
    /// Enable or disable cancel operation
    /// </summary>
    void SetCancelEnabled(bool enabled);

    /// <summary>
    /// Set the current operation mode
    /// </summary>
    void SetOperationMode(OperationMode mode);
}

/// <summary>
/// Operation modes for CRUD views
/// </summary>
public enum OperationMode
{
    /// <summary>
    /// View mode - read-only
    /// </summary>
    View,

    /// <summary>
    /// New mode - creating a new item
    /// </summary>
    New,

    /// <summary>
    /// Edit mode - editing an existing item
    /// </summary>
    Edit
}

/// <summary>
/// Base presenter class
/// </summary>
public abstract class BasePresenter<TView> : IPresenter<TView> where TView : IView
{
    /// <summary>
    /// The view managed by this presenter
    /// </summary>
    public TView View { get; private set; } = default!;

    /// <summary>
    /// Whether the presenter is initialized
    /// </summary>
    protected bool IsInitialized { get; private set; }

    /// <summary>
    /// Initialize the presenter
    /// </summary>
    public virtual Task InitializeAsync()
    {
        IsInitialized = true;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Initialize the presenter with a view
    /// </summary>
    public virtual async Task InitializeAsync(TView view)
    {
        View = view ?? throw new ArgumentNullException(nameof(view));

        // Subscribe to view events
        View.ViewLoaded += OnViewLoaded;
        View.ViewClosing += OnViewClosing;

        await InitializeAsync();
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public virtual void Cleanup()
    {
        if (View != null)
        {
            View.ViewLoaded -= OnViewLoaded;
            View.ViewClosing -= OnViewClosing;
        }

        IsInitialized = false;
    }

    /// <summary>
    /// Handle view loaded event
    /// </summary>
    protected virtual void OnViewLoaded(object? sender, EventArgs e)
    {
        // Override in derived classes
    }

    /// <summary>
    /// Handle view closing event
    /// </summary>
    protected virtual void OnViewClosing(object? sender, EventArgs e)
    {
        Cleanup();
    }

    /// <summary>
    /// Execute an action safely with error handling
    /// </summary>
    protected async Task ExecuteSafelyAsync(Func<Task> action, string errorMessage = "An error occurred")
    {
        try
        {
            if (View is IDataView dataView)
            {
                dataView.SetLoadingState(true);
            }

            await action();
        }
        catch (Exception ex)
        {
            if (View is IDataView dataView)
            {
                dataView.ShowError($"{errorMessage}: {ex.Message}");
            }
        }
        finally
        {
            if (View is IDataView dataView)
            {
                dataView.SetLoadingState(false);
            }
        }
    }
}
