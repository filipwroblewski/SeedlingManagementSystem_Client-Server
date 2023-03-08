using DbUser;
using System.Data;
using System.Data.SqlClient;

namespace System
{
    class BackupControl
    {
        private int daysToNextBackup;

        public BackupControl(int daysToNextBackup)
        {
            this.daysToNextBackup = daysToNextBackup;
        }

        public int DaysToNextBackup { get => daysToNextBackup; }

        public override string ToString()
        {
            return $"Days to next backup: {this.daysToNextBackup}";
        }
    }

    class Seedling
    {
        private int seedlingId, quantity;
        private string name;
        private float pricePerUnit;

        public Seedling(int seedlingId, string name, float pricePerUnit, int quantity)
        {
            this.seedlingId = seedlingId;
            this.name = name;
            this.pricePerUnit = pricePerUnit;
            this.quantity = quantity;
        }

        public int SeedlingId { get => seedlingId; }
        public string Name { get => name; }
        public float PricePerUnit { get => pricePerUnit; }
        public int Quantity { get => quantity; }

        public override string ToString()
        {
            return $"{this.SeedlingId} | {this.name}, price: {this.pricePerUnit}, quantity: {this.quantity}";
        }

        /*
         SQL
        */

        private static SqlCommand _insertStatement;
        private static SqlCommand _updateStatement;
        private static SqlCommand _deleteStatement;

        public static void InitADO(SqlDatabaseConnection conn)
        {
            string insertSql = @"INSERT INTO Seedlings (name, quantity, pricePerUnit) VALUES (@Name, @Quantity, @PricePerUnit); SELECT SCOPE_IDENTITY();";
            _insertStatement = new SqlCommand(insertSql, conn.sqlConn);
            _insertStatement.Prepare();
            string deleteSql = @"DELETE FROM Seedlings WHERE seedlingId = @SeedlingId";
            _deleteStatement = new SqlCommand(deleteSql, conn.sqlConn);
            _deleteStatement.Prepare();
            string updateSql = @"UPDATE Seedlings SET seedlingId = @SeedlingId, name = @Name, quantity = @Quantity, pricePerUnit = @PricePerUnit WHERE seedlingId = @SeedlingId";
            _updateStatement = new SqlCommand(updateSql, conn.sqlConn);
            _updateStatement.Prepare();
        }

        public static void InsertObject(SqlDatabaseConnection conn, System.Seedling seedling)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _insertStatement.Transaction = t;
                _insertStatement.Parameters.Clear();

                _insertStatement.Parameters.AddWithValue("@SeedlingId", seedling.seedlingId);
                _insertStatement.Parameters.AddWithValue("@Name", seedling.name);
                _insertStatement.Parameters.AddWithValue("@Quantity", seedling.quantity);
                _insertStatement.Parameters.AddWithValue("@PricePerUnit", seedling.pricePerUnit);
                
                SqlDataReader dr = _insertStatement.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();

                t.Commit();
                _insertStatement.Parameters.Clear();
                int v = Decimal.ToInt32((decimal)dt.Rows[0].ItemArray[0]);
                seedling.seedlingId = v;

                
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void DeleteObject(SqlDatabaseConnection conn, System.Seedling seedling)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _deleteStatement.Transaction = t;
                _deleteStatement.Parameters.Clear();
                _deleteStatement.Parameters.AddWithValue("@SeedlingId", seedling.seedlingId);
                _deleteStatement.ExecuteNonQuery();
                t.Commit();
                _deleteStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void UpdateObject(SqlDatabaseConnection conn, int seedlingId, string name, string price, int quantity)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _updateStatement.Transaction = t;
                _updateStatement.Parameters.Clear();

                _updateStatement.Parameters.AddWithValue("@SeedlingId", seedlingId);
                _updateStatement.Parameters.AddWithValue("@Name", name);
                _updateStatement.Parameters.AddWithValue("@Quantity", quantity);
                _updateStatement.Parameters.AddWithValue("@PricePerUnit", price);

                _updateStatement.ExecuteNonQuery();
                t.Commit();
                _updateStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void CreateTable(SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                string commandSql = @"CREATE TABLE Seedlings (seedlingId INT NOT NULL IDENTITY, name varchar(255), quantity INT, pricePerUnit varchar(255), PRIMARY KEY(seedlingId))";
                SqlCommand cm1 = new SqlCommand(commandSql, conn.sqlConn);
                cm1.Transaction = t;
                cm1.ExecuteNonQuery();
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void DropTable(SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                string commandSql = @"DROP TABLE IF EXISTS Seedlings";
                SqlCommand cm1 = new SqlCommand(commandSql, conn.sqlConn);
                cm1.Transaction = t;
                cm1.ExecuteNonQuery();
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
            }

        }

        /*
         * Seedlings actions
         */

        public static string showSeedlings()
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.QuerySeedlings.initADO(mfp);

            DataTable dt = System.QuerySeedlings.Query(mfp);
            Console.WriteLine("Returned rows containing " + dt.Columns.Count + " columns");
            string tableData = "";
            string colNames = "";
            bool colNamesNoSet = true;
            foreach (DataRow r in dt.Rows)
            {
                int field = 0;
                foreach (var f in r.ItemArray)
                {
                    if (field == 0)
                    {
                        tableData += f.ToString();
                        if (colNamesNoSet)
                            colNames += dt.Columns[field].ColumnName;
                    }
                    else
                    {
                        tableData += $";{f.ToString()}";
                        if (colNamesNoSet)
                            colNames += $";{dt.Columns[field].ColumnName}";
                    }
                    field++;
                }
                tableData += "<br>";
                colNamesNoSet = false;
            }

            mfp.TerminateConnection();
            Thread.Sleep(100);

            return colNames + "<br>" + tableData;
        }

        public static string seedlingDataById(int seedlingId)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.QuerySeedlingsId.initADO(mfp);

            DataTable dt = System.QuerySeedlingsId.Query(mfp, seedlingId);
            Console.WriteLine("Returned rows containing " + dt.Columns.Count + " columns");
            string tableData = "";
            foreach (DataRow r in dt.Rows)
            {
                int field = 0;
                foreach (var f in r.ItemArray)
                {
                    if (field == 0)
                    {
                        tableData += f.ToString();
                    }
                    else
                    {
                        tableData += $";{f.ToString()}";
                    }
                    field++;
                }
            }

            mfp.TerminateConnection();
            Thread.Sleep(100);

            return tableData;
        }

    }

    class Cart
    {
        public static List<string> cart = new List<string>();

        public static void AddSeedling(int seedlingId, int seedlingsQuantity, float price)
        {
            cart.Add($"{seedlingId.ToString()};{seedlingsQuantity.ToString()};{price.ToString()}");
        }
    }

    class Order
    {
        private int orderId;
        private string details;
        private string status;

        public Order(int orderId, string details, string status)
        {
            this.orderId = orderId;
            this.details = details;
            this.status = status;
        }

        public int OrderId { get => orderId; }
        public string Detail { get => details; }
        public string Status { get => status; }

        public override string ToString()
        {
            return $"Order #{this.OrderId}, info: {this.details} (status: {this.status})";
        }

        /*
         SQL
        */

        private static SqlCommand _insertStatement;
        private static SqlCommand _updateStatement;
        private static SqlCommand _deleteStatement;

        public static void InitADO(SqlDatabaseConnection conn)
        {
            string insertSql = @"INSERT INTO Orders (details, status) VALUES (@Details, @Status); SELECT SCOPE_IDENTITY();";
            _insertStatement = new SqlCommand(insertSql, conn.sqlConn);
            _insertStatement.Prepare();
            string deleteSql = @"DELETE FROM Orders WHERE orderId = @OrderId";
            _deleteStatement = new SqlCommand(deleteSql, conn.sqlConn);
            _deleteStatement.Prepare();
            string updateSql = @"UPDATE Orders SET orderId = @OrderId, detail = @Details, status= @Status WHERE orderId = @OrderId";
            _updateStatement = new SqlCommand(updateSql, conn.sqlConn);
            _updateStatement.Prepare();
        }

        public static void InsertObject(SqlDatabaseConnection conn, System.Order order)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _insertStatement.Transaction = t;
                _insertStatement.Parameters.Clear();

                _insertStatement.Parameters.AddWithValue("@OrderId", order.orderId);
                _insertStatement.Parameters.AddWithValue("@Details", order.details);
                _insertStatement.Parameters.AddWithValue("@Status", order.status);

                SqlDataReader dr = _insertStatement.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();

                t.Commit();
                _insertStatement.Parameters.Clear();
                int v = Decimal.ToInt32((decimal)dt.Rows[0].ItemArray[0]);
                order.orderId = v;


            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void DeleteObject(SqlDatabaseConnection conn, System.Order order)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _deleteStatement.Transaction = t;
                _deleteStatement.Parameters.Clear();
                _deleteStatement.Parameters.AddWithValue("@OrderId", order.orderId);
                _deleteStatement.ExecuteNonQuery();
                t.Commit();
                _deleteStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void UpdateObject(SqlDatabaseConnection conn, System.Order order)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _updateStatement.Transaction = t;
                _updateStatement.Parameters.Clear();

                _updateStatement.Parameters.AddWithValue("@OrderId", order.orderId);
                _updateStatement.Parameters.AddWithValue("@Details", order.details);
                _updateStatement.Parameters.AddWithValue("@Status", order.status);

                _updateStatement.ExecuteNonQuery();
                t.Commit();
                _updateStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void CreateTable(SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                string commandSql = @"CREATE TABLE Orders (orderId INT NOT NULL IDENTITY, details varchar(255), status varchar(255), PRIMARY KEY(orderId))";
                SqlCommand cm1 = new SqlCommand(commandSql, conn.sqlConn);
                cm1.Transaction = t;
                cm1.ExecuteNonQuery();
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void DropTable(SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                string commandSql = @"DROP TABLE IF EXISTS Orders";
                SqlCommand cm1 = new SqlCommand(commandSql, conn.sqlConn);
                cm1.Transaction = t;
                cm1.ExecuteNonQuery();
                t.Commit();
            }
            catch (Exception e)
            {
                t.Rollback();
            }

        }

        /*
         * Order actions
         */

        public static void placeOrder(System.Order order)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.Order.InitADO(mfp);

            System.Order.InsertObject(mfp, order);

            mfp.TerminateConnection();
            Thread.Sleep(100);
        }
    }

    public class QuerySeedlings
    {
        private static SqlCommand _selectStatement;

        public static void initADO(DbUser.SqlDatabaseConnection conn)
        {
            string selectSql = @"SELECT * FROM Seedlings;";
            _selectStatement = new SqlCommand(selectSql, conn.sqlConn);
            _selectStatement.Prepare();
        }

        public static DataTable Query(DbUser.SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            DataTable dt = new DataTable();
            try
            {
                _selectStatement.Transaction = t;
                _selectStatement.Parameters.Clear();
                SqlDataReader dr = _selectStatement.ExecuteReader();
                dt.Load(dr);
                t.Commit();
                _selectStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }

            return dt;
        }
    }

    public class QuerySeedlingsId
    {
        private static SqlCommand _selectStatement;

        public static void initADO(DbUser.SqlDatabaseConnection conn)
        {
            string selectSql = @"SELECT * FROM Seedlings s WHERE s.seedlingId = @SeedlingId";
            _selectStatement = new SqlCommand(selectSql, conn.sqlConn);
            _selectStatement.Prepare();
        }

        public static DataTable Query(DbUser.SqlDatabaseConnection conn, int seedlingId)
        {
            SqlTransaction t = conn.Begin();
            DataTable dt = new DataTable();
            try
            {
                _selectStatement.Transaction = t;
                _selectStatement.Parameters.Clear();
                _selectStatement.Parameters.AddWithValue("@SeedlingId", seedlingId);
                SqlDataReader dr = _selectStatement.ExecuteReader();
                dt.Load(dr);
                t.Commit();
                _selectStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }

            return dt;
        }
    }

    public class QueryOrders
    {
        private static SqlCommand _selectStatement;

        public static void initADO(DbUser.SqlDatabaseConnection conn)
        {
            string selectSql = @"SELECT * FROM Orders;";
            _selectStatement = new SqlCommand(selectSql, conn.sqlConn);
            _selectStatement.Prepare();
        }

        public static DataTable Query(DbUser.SqlDatabaseConnection conn)
        {
            SqlTransaction t = conn.Begin();
            DataTable dt = new DataTable();
            try
            {
                _selectStatement.Transaction = t;
                _selectStatement.Parameters.Clear();
                SqlDataReader dr = _selectStatement.ExecuteReader();
                dt.Load(dr);
                t.Commit();
                _selectStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }

            return dt;
        }
    }

}