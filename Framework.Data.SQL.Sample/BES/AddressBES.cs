using Framework.Entity;

namespace BES
{
    /// <summary>
    /// Address model domain class
    /// </summary>	
    public sealed class AddressBES : BusinessEntityStructure
    {
        #region| Properties |

        public int ID { get; set; }
        public string Address { get; set; }
        public bool Enabled { get; set; }

        #endregion

        #region| Constructor |

        public AddressBES()
        {
            this.Map(nameof(ID));
            this.Map(nameof(Address));
            this.Map(nameof(Enabled));
        }

        #endregion
    }
}
