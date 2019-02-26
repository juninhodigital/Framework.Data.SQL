using System;
using System.Data;

namespace Framework.Data.SQL
{
    public static partial class Mapper
    {
        /// <summary>
        /// Permits specifying certain Mapper values globally.
        /// </summary>
        public static class Settings
        {
            #region| Fields |

            // disable single result by default; prevents errors AFTER the select being detected properly
            private const CommandBehavior DefaultAllowedCommandBehaviors = ~CommandBehavior.SingleResult;
            internal static CommandBehavior AllowedCommandBehaviors { get; private set; } = DefaultAllowedCommandBehaviors; 

            #endregion

            #region| Properties |

            /// <summary>
            /// Specifies the default Command Timeout for all Queries
            /// </summary>
            public static int? CommandTimeout { get; set; }

            /// <summary>
            /// Indicates whether nulls in data are silently ignored (default) vs actively applied and assigned to members
            /// </summary>
            public static bool ApplyNullValues { get; set; }

            /// <summary>
            /// Should list expansions be padded with null-valued parameters, to prevent query-plan saturation? For example,
            /// an 'in @foo' expansion with 7, 8 or 9 values will be sent as a list of 10 values, with 3, 2 or 1 of them null.
            /// The padding size is relative to the size of the list; "next 10" under 150, "next 50" under 500,
            /// "next 100" under 1500, etc.
            /// </summary>
            /// <remarks>
            /// Caution: this should be treated with care if your DB provider (or the specific configuration) allows for null
            /// equality (aka "ansi nulls off"), as this may change the intent of your query; as such, this is disabled by 
            /// default and must be enabled.
            /// </remarks>
            public static bool PadListExpansions { get; set; }
            /// <summary>
            /// If set (non-negative), when performing in-list expansions of integer types ("where id in @ids", etc), switch to a string_split based
            /// operation if there are more than this many elements. Note that this feautre requires SQL Server 2016 / compatibility level 130 (or above).
            /// </summary>
            public static int InListStringSplitCount { get; set; } = -1; 

            #endregion

            #region| Constructor |

            static Settings()
            {
                
            }

            #endregion

        }
    }
}
