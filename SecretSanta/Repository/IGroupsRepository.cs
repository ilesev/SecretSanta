using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public interface IGroupsRepository : IRepository<Group, string>
    {
        Task CreateGroupAsync(string groupName, string creator);
        Task<IEnumerable<Group>> GetAllGroupsAsync();
        Task<bool> groupExistsAsync(string groupName);
        Task<string> getAdminOfGroup(string groupname);
        Task<bool> UserHasInvitationForGroup(string groupname, string username);
        Task sendInvitation(Invitation invitation);
        Task<IEnumerable<InvitationVM>> getPaginatedInvitationsAsync(string username, int skip, int take, string order);
        Task<Invitation> GetInvitationById(string id);
        Task AddGroupMember(string groupname, string username);
        Task DeleteInvitation(string id);
    }
}
