using System.Data.Common;
using System.IO;
using System.Text;

namespace Arrowgene.Ddon.Database.Sql.Core.Migration
{
    public class RewardMissionProgressMigration(DatabaseSetting databaseSetting) : IMigrationStrategy
    {
        public uint From => 68;
        public uint To => 69;

        public bool Migrate(IDatabase db, DbConnection conn)
        {
            string adaptedSchema = DdonDatabaseBuilder.GetAdaptedSchema(databaseSetting, "Script/migration_reward_mission_progress.sql");
            db.Execute(conn, adaptedSchema);
            return true;
        }
    }
}
