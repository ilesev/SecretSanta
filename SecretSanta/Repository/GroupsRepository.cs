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
            }
        }

        public async Task<string> getAdminOfGroup(string groupname)
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

        public async Task<IEnumerable<InvitationVM>> getPaginatedInvitationsAsync(string username, int skip, int take, string order)
        {
            bool isAscOrder = order.Equals("asc");
            List<InvitationVM> invitations = new List<InvitationVM>();
            using (var connection = getConnection())
            {
                await connection.OpenAsync();
                MySqlCommand command = connection.CreateCommand();
                command.CommandText = "SELECT groupname, datecreated, id FROM Invitations";
                command.Parameters.AddWithValue("@order", order.ToUpper());
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

        public async Task<bool> groupExistsAsync(string groupName)
        {
            IEnumerable<Group> groups = await GetAllGroupsAsync();
            return groups.Any(x => x.GroupName.Equals(groupName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task sendInvitation(Invitation invitation)
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

        public async Task<bool> UserHasInvitationForGroup(string groupname, string username)
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
    }
}
