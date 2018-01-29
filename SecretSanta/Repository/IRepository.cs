using System;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SecretSanta.Repository
{
    public interface IRepository<TEntityType, in TKeyType>
    {
    }
}
