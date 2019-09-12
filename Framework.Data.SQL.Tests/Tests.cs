using System;
using Xunit;

using Framework.Data;
using Framework.Data.SQL;
using BLL;
using System.Linq;
using BES;
using System.Collections.Generic;

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

        private UserBES GetPayload()
        {
            return new UserBES
            {
                Name     = "User test",
                Nickname = "Nickname",
                CPF      = new Random().Next(999, 5000).ToString(),
                RG       = new Random().Next(999, 5000).ToString(),
                Enabled  = true,
                Addresses = new List<AddressBES>
                {
                    new AddressBES{ Address = "Rua teste 1", Enabled=true },
                    new AddressBES{ Address = "Rua teste 2", Enabled=true }
                }
            };
        }

        #endregion

        [Fact]
        public void Run()
        {
            var container = GetContainer();
            var payload   = GetPayload();
                     
            using (var BLL = new UserBLL(container))
            {
                payload.ID  = BLL.Save(payload);
                payload.Name += " Updated";

                BLL.Update(payload);

                var users = BLL.Get().ToList();
                var user  = BLL.GetByID(payload.ID);

            }

            Assert.True(true);
        }
    }
}
