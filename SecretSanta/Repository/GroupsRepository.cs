using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using SecretSanta.Models;

namespace SecretSanta.Repository
{
    public class GroupsRepository : BaseRepository, IGroupsRepository
    {
        public GroupsRepository(IConfiguration Configuration) : base(Configuration)
        {
        }

        public async Task CreateGroupAsync(string groupName, string creator)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Groups(name, creator)" +
                                       "VALUES (?name, ?creator)";
                command.Parameters.Add("?name", MySqlDbType.VarChar).Value = groupName;
                command.Parameters.Add("?creator", MySqlDbType.VarChar).Value = creator;
                await command.ExecuteNonQueryAsync();
                await AddGroupMemberAsync(groupName, creator);

            }
        }

        public async Task AddGroupMemberAsync(string groupname, string username)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                var insertIntoGroup = connection.CreateCommand();
                insertIntoGroup.CommandText = "INSERT INTO GroupMembers(groupname, username) VALUES(@groupname, @username)";
                insertIntoGroup.Parameters.AddWithValue("@groupname", groupname);
                insertIntoGroup.Parameters.AddWithValue("@username", username);
                await insertIntoGroup.ExecuteNonQueryAsync();
            }
        }

        public async Task<string> GetAdminOfGroupAsync(string groupname)
        {
            IEnumerable<Group> groups = await GetAllGroupsAsync();
            return groups.FirstOrDefault(x => x.GroupName.Equals(groupname, StringComparison.OrdinalIgnoreCase)).Creator;
        }

        public async Task<IEnumerable<Group>> GetAllGroupsAsync()
        {
            List<Group> groups = new List<Group>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Groups";
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        groups.Add( new Group
                        {
                            GroupName = reader.GetString(0),
                            Creator = reader.GetString(1)
                        });
                    }
                }
            }
            return groups;
        }

        public async Task<Invitation> GetInvitationByIdAsync(string id)
        {
            Invitation inv = null;
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Invitations WHERE Id=@id";
                command.Parameters.AddWithValue("@id", id);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        inv = new Invitation
                        {
                            Groupname = reader.GetString(0),
                            DateCreated = reader.GetDateTime(1),
                            Username = reader.GetString(2),
                            Id = reader.GetString(4)
                        };
                    }
                }
            }
                return inv;
        }

        public async Task<IEnumerable<InvitationVM>> GetPaginatedInvitationsAsync(string username, int skip, int take, string order)
        {
            bool isAscOrder = order.Equals("asc");
            List<InvitationVM> invitations = new List<InvitationVM>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT groupname, datecreated, id FROM Invitations WHERE username=@username";
                command.Parameters.AddWithValue("@username", username);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while(reader.Read())
                    {
                        invitations.Add(new InvitationVM{
                            Groupname = reader.GetString(0),
                            DateCreated = reader.GetDateTime(1),
                            Id = reader.GetString(2)
                        });
                    }
                }
            }

            return isAscOrder ? invitations.OrderBy(x => x.DateCreated) : invitations.OrderByDescending( x => x.DateCreated);
        }

        public async Task<bool> GroupExistsAsync(string groupName)
        {
            IEnumerable<Group> groups = await GetAllGroupsAsync();
            return groups.Any(x => x.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SendInvitationAsync(Invitation invitation)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "INSERT INTO Invitations(id, username, groupname, datecreated, invitationStatus)" +
                    " VALUES(@id, @username, @groupname, @datecreated, @invitationStatus)";
                command.Parameters.AddWithValue("@id", invitation.Id);
                command.Parameters.AddWithValue("@username", invitation.Username);
                command.Parameters.AddWithValue("@groupname", invitation.Groupname);
                command.Parameters.AddWithValue("@datecreated", invitation.DateCreated);
                command.Parameters.AddWithValue("@invitationStatus", invitation.InvitationStatus);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> UserHasInvitationForGroupAsync(string groupname, string username)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT groupname FROM Invitations" +
                    " WHERE groupname=@groupname AND username=@username;";
                command.Parameters.AddWithValue("@groupname", groupname);
                command.Parameters.AddWithValue("@username", username);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (reader.Read())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public async Task DeleteInvitationAsync(string invitationId)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Invitations WHERE id=@id";
                command.Parameters.AddWithValue("@id", invitationId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<IEnumerable<GroupMember>> GetGroupMembersAsync(string groupname)
        {
            List<GroupMember> members = new List<GroupMember>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM groupmembers WHERE groupname=@groupname";
                command.Parameters.AddWithValue("@groupname", groupname);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        members.Add(new GroupMember
                        {
                            Groupname = reader.GetString(0),
                            Username = reader.GetString(1),
                            GiftsTo = reader.IsDBNull(2) ? null : reader.GetString(2)
                        });
                    }
                }
            }

            return members;
        }

        public async Task StartGiftGivingAsync(string groupname, IEnumerable<GroupMember> members)
        {
            int[] shuffle = new int[members.Count()];
            for (int i = 0; i < shuffle.Length; i++)
            {
                shuffle[i] = i;
            }

            await Task.Run(() =>
            {
                while (ArrayIsNotShuffledValidly(shuffle))
                {
                    Shuffle(shuffle);
                }

                for (int i = 0; i < members.Count(); i++)
                {
                    GroupMember current = members.ElementAt(i);
                    GroupMember giftsTo = members.ElementAt(shuffle[i]);
                    current.GiftsTo = giftsTo.Username;
                }
            }); 

            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                foreach (var member in members)
                {
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE groupmembers SET giftsTo=@giftsTo WHERE username=@username AND groupname=@groupname";
                    command.Parameters.AddWithValue("@giftsTo", member.GiftsTo);
                    command.Parameters.AddWithValue("@username", member.Username);
                    command.Parameters.AddWithValue("@groupname", member.Groupname);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<IEnumerable<object>> GetUserGroups(string username, int skip, int take)
        {
            List<object> groups = new List<object>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT groupname FROM groupmembers WHERE username=@username";
                command.Parameters.AddWithValue("@username", username);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        groups.Add(new
                        {
                            Groupname = reader.GetString(0)
                        });
                    }
                }
            }
            return groups.Skip(skip).Take(take);
        }

        private bool ArrayIsNotShuffledValidly(int[] shuffle)
        {
            for (int i = 0; i < shuffle.Length; i++)
            {
                if (shuffle[i] == i)
                {
                    return true;
                }
            }
            return false;
        }

        private void Shuffle(int[] array)
        {
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                int temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public async Task DeleteMemberAsync(GroupMember member)
        {
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM groupmembers WHERE username=@username AND groupname=@groupname";
                command.Parameters.AddWithValue("@username", member.Username);
                command.Parameters.AddWithValue("@groupname", member.Groupname);
                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
