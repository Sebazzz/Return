// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : PluralizationConventions.cs
//  Project         : Return.Persistence
// ******************************************************************************

namespace Return.Persistence.Conventions {
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;

    internal static class PluralizationConventions {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder) {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes()) {
                if (!entity.IsOwned()) {
                    entity.SetTableName(entity.DisplayName());
                }
            }
        }
    }
}
