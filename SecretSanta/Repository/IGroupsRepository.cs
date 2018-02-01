﻿using System;
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
        Task<string> getAdminOfGroupAsync(string groupname);
        Task<bool> UserHasInvitationForGroupAsync(string groupname, string username);
        Task sendInvitationAsync(Invitation invitation);
        Task<IEnumerable<InvitationVM>> getPaginatedInvitationsAsync(string username, int skip, int take, string order);
        Task<Invitation> GetInvitationByIdAsync(string id);
        Task AddGroupMemberAsync(string groupname, string username);
        Task DeleteInvitationAsync(string id);
        Task<IEnumerable<GroupMember>> getGroupMembers(string groupname);
        Task StartGiftGivingAsync(string groupname, IEnumerable<GroupMember> members);
        Task<IEnumerable<object>> GetUserGroups(string username, int skip, int take);
        Task DeleteMemberAsync(GroupMember member);
    }
}
