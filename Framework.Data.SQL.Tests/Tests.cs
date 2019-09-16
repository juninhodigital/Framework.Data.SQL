using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using BLL;
using BES;
using Xunit.Abstractions;

namespace Framework.Data.SQL.Tests
{
    public class Tests
    {
        #region| Properties |

        private readonly ITestOutputHelper output;

        #endregion

        #region| Methods |

        private ContainerDI GetContainer(bool previewDetails=false)
        {
            var connection = "Password=p@ssw0rd;Persist Security Info=True;User ID=sa;Initial Catalog=DB_SYSADV;Data Source=LOCALHOST";
            var timeout    = 60;
            var path       = "";

            if(previewDetails)
            {
                path = @"D:\preview.txt";
            }

            var container = new ContainerDI(new SQLDatabaseContext(connection, timeout), new SQLDatabaseRepository(false, path));

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

        #region| Constructor |

        public Tests(ITestOutputHelper outputHelper)
        {
            this.output = outputHelper;
        }

        #endregion

        [Fact]
        public void Save()
        {
            output.WriteLine("Test: Save started");

            var container = GetContainer();
            var payload   = GetPayload();
                     
            using (var BLL = new UserBLL(container))
            {
                payload.ID  = BLL.Save(payload);
            }

            Assert.True(payload.ID > 0);

            output.WriteLine($"Test: Save finished. User ID: {payload.ID}");
        }

        [Fact]
        public void Update()
        {
            output.WriteLine("Test: Update started");

            // Use a valid user id
            var userId    = 1;
            var container = GetContainer();
            var payload   = GetPayload(userId);

            using (var BLL = new UserBLL(container))
            {
                BLL.Update(payload);
            }

            Assert.True(true);

            output.WriteLine("Test: Update finished");
        }

        [Fact]
        public void GetItems()
        {
            output.WriteLine("Test: Get items started");

            var users     = new List<UserBES>();
            var container = GetContainer();

            using (var BLL = new UserBLL(container))
            {
                users = BLL.Get().ToList();
            }

            Assert.Contains(users, user => user.Nickname == "Junior");

            output.WriteLine($"Test: Get items finished. Users count: {users.Count} ");
        }

        [Fact]
        public void GetItem()
        {
            output.WriteLine("Test: Get item started");

            // Use a valid user id
            var userId    = 1;
            var container = GetContainer();
            var payload   = GetPayload(userId);

            UserBES user = null;

            using (var BLL = new UserBLL(container))
            {
                user = BLL.GetByID(payload.ID);
            }

            Assert.NotNull(user);

            output.WriteLine($"Test: Get item finished. User ID: {user.ID}");
        }
    }
}
