using Vargshala.Contracts.Branches;

namespace Vargshala.Web.Services;

public class BranchContextService : IBranchContextService
{
    private readonly IBranchService _branchService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<BranchContextService> _logger;

    private List<BranchDto> _availableBranches = new();
    private Guid? _currentBranchId;
    private BranchDto? _currentBranch;
    private bool _isInitialized;
    private bool _isLoading;
    private bool _hasMainBranch;

    public BranchContextService(
        IBranchService branchService,
        INotificationService notificationService,
        ILogger<BranchContextService> logger)
    {
        _branchService = branchService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public Guid? CurrentBranchId => _currentBranchId;
    public BranchDto? CurrentBranch => _currentBranch;
    public bool IsAllBranches => !_currentBranchId.HasValue;
    public bool HasMainBranch => _hasMainBranch;
    public bool IsInitialized => _isInitialized;
    public bool IsLoading => _isLoading;
    public IReadOnlyList<BranchDto> AvailableBranches => _availableBranches;

    public event Func<Task>? OnBranchChanged;

    public async Task InitializeAsync(bool forceReload = false)
    {
        if (_isInitialized && !forceReload) return;

        _isLoading = true;
        try
        {
            var res = await _branchService.GetAllActiveBranchesAsync();
            if (res.Success && res.Data != null)
            {
                _availableBranches = res.Data.Where(b => b.IsActive).ToList();

                var mainBranch = _availableBranches.FirstOrDefault(b => b.IsMainBranch);
                if (mainBranch != null)
                {
                    _hasMainBranch = true;
                    _currentBranchId = mainBranch.Id;
                    _currentBranch = mainBranch;
                }
                else
                {
                    // No Main Branch found!
                    _hasMainBranch = false;
                    _currentBranchId = null; // Do NOT silently pick another branch as main branch
                    _currentBranch = null;

                    if (_availableBranches.Any())
                    {
                        _notificationService.Warning(
                            "No Main Branch is configured for your organization. Please designate a Main Branch in Branch Management.",
                            "Main Branch Required");
                    }
                }
            }
            else
            {
                _availableBranches = new();
                _currentBranchId = null;
                _currentBranch = null;
                _hasMainBranch = false;
            }

            _isInitialized = true;
            await NotifyBranchChangedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing BranchContextService");
        }
        finally
        {
            _isLoading = false;
        }
    }

    public async Task SetBranchAsync(Guid? branchId)
    {
        if (_currentBranchId == branchId) return;

        if (branchId.HasValue)
        {
            var branch = _availableBranches.FirstOrDefault(b => b.Id == branchId.Value);
            if (branch != null)
            {
                _currentBranchId = branch.Id;
                _currentBranch = branch;
            }
            else
            {
                _logger.LogWarning("Branch with ID {BranchId} not found among active branches.", branchId.Value);
                return;
            }
        }
        else
        {
            _currentBranchId = null;
            _currentBranch = null;
        }

        await NotifyBranchChangedAsync();
    }

    public async Task SetAllBranchesAsync()
    {
        await SetBranchAsync(null);
    }

    public Task ResetAsync()
    {
        _isInitialized = false;
        _availableBranches.Clear();
        _currentBranchId = null;
        _currentBranch = null;
        _hasMainBranch = false;
        return Task.CompletedTask;
    }

    private async Task NotifyBranchChangedAsync()
    {
        if (OnBranchChanged != null)
        {
            try
            {
                await OnBranchChanged.Invoke();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing OnBranchChanged callback in BranchContextService");
            }
        }
    }
}
