using System;
namespace SecretSanta.Models
{
    public class Invitation
    {
        public string Groupname
        {
            get;
            set;
        }

        public string InvitationStatus
        {
            get;
            set;
        }

        public string Username
        {
            get;
            set;
        }

        public DateTime DateCreated
        {
            get;
            set;
        }

        public string Id
        {
            get;
            set;
        }
    }
}
