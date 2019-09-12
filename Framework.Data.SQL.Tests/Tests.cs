using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using BLL;
using BES;

namespace Framework.Data.SQL.Tests
{
    public class Tests
    {
        #region| Methods |

        private ContainerDI GetContainer()
        {
            var connection = "Password=p@ssw0rd;Persist Security Info=True;User ID=sa;Initial Catalog=DB_SYSADV;Data Source=LOCALHOST";
            var timeout = 60;

            var container = new ContainerDI(new SQLDatabaseContext(connection, timeout), new SQLDatabaseRepository());

            return container;
        } 

        private UserBES GetPayload(int ID = 0)
        {
            return new UserBES
            {
                ID        = ID,
                Name      = "User test",
                Nickname  = "Nickname",
                CPF       = new Random().Next(999, 5000).ToString(),
                RG        = new Random().Next(999, 5000).ToString(),
                Enabled   = true,
                Addresses = new List<AddressBES>
                {
                    new AddressBES{ Address = "Rua teste 1", Enabled=true },
                    new AddressBES{ Address = "Rua teste 2", Enabled=true }
                }
            };
        }

        #endregion

        [Fact]
        public void Save()
        {
            var container = GetContainer();
            var payload   = GetPayload();
                     
            using (var BLL = new UserBLL(container))
            {
                payload.ID  = BLL.Save(payload);
            }

            Assert.True(true);
        }

        [Fact]
        public void Update()
        {
            var container = GetContainer();
            var payload   = GetPayload(1);

            using (var BLL = new UserBLL(container))
            {
                BLL.Update(payload);

                var users = BLL.Get().ToList();
                var user = BLL.GetByID(payload.ID);

            }

            Assert.True(true);
        }

        [Fact]
        public void Get()
        {
            var container = GetContainer();
            var payload = GetPayload();

            using (var BLL = new UserBLL(container))
            {
                var users = BLL.Get().ToList();
                var user = BLL.GetByID(payload.ID);

            }

            Assert.True(true);
        }
    }
}
