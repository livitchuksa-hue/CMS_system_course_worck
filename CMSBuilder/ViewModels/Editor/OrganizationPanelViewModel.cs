using System.Collections.ObjectModel;
using CMSBuilder.Core;
using CMSBuilder.Models.Dto;
using CMSBuilder.Services;
using CMSBuilder.ViewModels.Base;

namespace CMSBuilder.ViewModels.Editor;

public class OrganizationPanelViewModel : BaseViewModel
{
    private readonly WebsiteMemberService _members;
    private readonly int _websiteId;
    private readonly Action<string> _setStatus;
    private readonly Action? _onLeft;

    private string _inviteLogin = string.Empty;
    private string _selectedRoleCode = "viewer";
    private string _statusMessage = string.Empty;
    private bool _isOwner;
    private bool _canLeave;

    public OrganizationPanelViewModel(int websiteId, Action<string> setStatus, Action? onLeft = null)
    {
        _websiteId = websiteId;
        _setStatus = setStatus;
        _onLeft = onLeft;
        _members = App.Members;

        Members = new ObservableCollection<WebsiteMemberDto>();
        InviteRoles = new ObservableCollection<RoleOption>
        {
            new("admin", "Администратор"),
            new("moderator", "Модератор"),
            new("viewer", "Просмотр")
        };

        InviteCommand = new RelayCommand(Invite, () => CanInvite);
        RemoveMemberCommand = new RelayCommand<WebsiteMemberDto>(Remove, m => CanRemoveMember(m));
        LeaveCommand = new RelayCommand(Leave, () => CanLeave);
        RefreshCommand = new RelayCommand(Load);

        Load();
    }

    public ObservableCollection<WebsiteMemberDto> Members { get; }
    public ObservableCollection<RoleOption> InviteRoles { get; }

    public string InviteLogin
    {
        get => _inviteLogin;
        set
        {
            if (SetProperty(ref _inviteLogin, value))
            {
                OnPropertyChanged(nameof(CanInvite));
                InviteCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string SelectedRoleCode
    {
        get => _selectedRoleCode;
        set => SetProperty(ref _selectedRoleCode, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public bool IsOwner
    {
        get => _isOwner;
        set => SetProperty(ref _isOwner, value);
    }

    public bool CanLeave
    {
        get => _canLeave;
        set => SetProperty(ref _canLeave, value);
    }

    public bool CanInvite => IsOwner && !string.IsNullOrWhiteSpace(InviteLogin);

    public RelayCommand InviteCommand { get; }
    public RelayCommand<WebsiteMemberDto> RemoveMemberCommand { get; }
    public RelayCommand LeaveCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public void Load()
    {
        var userId = SessionContext.CurrentUser?.Id ?? 0;
        IsOwner = _members.IsOwner(_websiteId, userId);
        CanLeave = userId > 0 && !IsOwner &&
                   _members.GetMembers(_websiteId).Any(m => m.UserId == userId && !m.IsOwner);

        Members.Clear();
        foreach (var m in _members.GetMembers(_websiteId))
            Members.Add(m);

        InviteCommand.RaiseCanExecuteChanged();
        RemoveMemberCommand.RaiseCanExecuteChanged();
        LeaveCommand.RaiseCanExecuteChanged();
    }

    private void Invite()
    {
        var userId = SessionContext.CurrentUser?.Id ?? 0;
        var (ok, err) = _members.InviteByLogin(_websiteId, userId, InviteLogin, SelectedRoleCode);
        if (!ok)
        {
            StatusMessage = err;
            _setStatus(err);
            return;
        }

        InviteLogin = string.Empty;
        StatusMessage = "Участник добавлен.";
        _setStatus(StatusMessage);
        Load();
    }

    private bool CanRemoveMember(WebsiteMemberDto? member) =>
        IsOwner && member != null && !member.IsOwner;

    private void Remove(WebsiteMemberDto? member)
    {
        if (member == null) return;
        var userId = SessionContext.CurrentUser?.Id ?? 0;
        var (ok, err) = _members.RemoveMember(_websiteId, userId, member.UserId);
        StatusMessage = ok ? "Участник удалён." : err;
        _setStatus(StatusMessage);
        if (ok) Load();
    }

    private void Leave()
    {
        var userId = SessionContext.CurrentUser?.Id ?? 0;
        var (ok, err) = _members.LeaveWebsite(_websiteId, userId);
        if (!ok)
        {
            StatusMessage = err;
            _setStatus(err);
            return;
        }

        StatusMessage = "Вы покинули проект.";
        _setStatus(StatusMessage);
        _onLeft?.Invoke();
    }

    public record RoleOption(string Code, string Name);
}
