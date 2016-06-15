using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace EntityFramework.Utilities.SqlServer
{
    public class SqlServerBulkSettings
    {
        public SqlServerBulkSettings()
        {
            Factory = new HelperFactory();
        }

        public int? BatchSize { get; set; }
        public Action<SqlBulkCopy> CustomSettingsApplier { get; set; }

        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; }
        /// <summary>
        /// Tis is auto populated in most cases but if you have profiled connection you might need to supply this yourself
        /// </summary>
        public SqlConnection Connection { get; set; }
        public SqlTransaction Transaction { get; set; }
        /// <summary>
        /// If set to true Database generated Id's will be returned and populate the entities. The insert takes about 2-3x longer time with this enabled.   
        /// </summary>
        public bool ReturnIdsOnInsert { get; set; }

        public HelperFactory Factory { get; set; }

        public TempTableSqlServerBulkSettings TempSettings { get; set; }

        public Func<SqlConnection, SqlTransaction, Task> PreInsert { get; set; }
        public Func<SqlConnection, SqlTransaction, Task> PostInsert { get; set; }

    }

    public class HelperFactory
    {
        public Func<BulkInserter> Inserter { get; set; } = () => new BulkInserter();
        public Func<TSQLGenerator> SqlGenerator { get; set; } = () => new TSQLGenerator();
        public Func<TableCreator> TableCreator { get; set; } = () => new TableCreator();


    }

    public class TempTableSqlServerBulkSettings : SqlServerBulkSettings
    {
        public TempTableSqlServerBulkSettings(SqlServerBulkSettings settings)
        {
            BatchSize = settings.BatchSize;
            CustomSettingsApplier = settings.CustomSettingsApplier;
            SqlBulkCopyOptions = settings.SqlBulkCopyOptions;
            Connection = settings.Connection;
            Transaction = settings.Transaction;
            ReturnIdsOnInsert = settings.ReturnIdsOnInsert;
            Factory = settings.Factory;
            SqlGenerator = new TSQLGenerator();
        }

        public TSQLGenerator SqlGenerator { get; set; }

        public string TableName { get; set; }

    }
}
