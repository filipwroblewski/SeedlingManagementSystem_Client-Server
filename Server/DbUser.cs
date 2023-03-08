using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;

namespace DbUser
{
    public class Person
    {
        public int id = 0;
        public string firstName, lastName, email, address;

        public Person(string firstName, string lastName, string email, string address)
        {
            this.firstName = firstName;
            this.lastName = lastName;
            this.email = email;
            this.address = address;
        }

        public string FirstName { get => firstName; }
        public string LastName { get => lastName; }
        public string Email { get => email; }
        public string Address { get => address; }

        public override string ToString()
        {
            return $"{this.firstName} {this.lastName} ({this.email}), postal code: {this.address}";
        }
    }

    class User : Person
    {
        public int userId = 0;
        public int? personId;
        public string nickName, password, userType;

        public User(Person person, string nickName, string password, string userType) : base(person.FirstName, person.LastName, person.Email, person.Address)
        {
            this.nickName = nickName;
            this.password = password;
            this.userType = userType;
            this.personId = person.id;
        }

        public User(Person person, int userId, string nickName, string password, string userType) : base(person.FirstName, person.LastName, person.Email, person.Address)
        {
            this.userId = userId;
            this.nickName = nickName;
            this.password = password;
            this.userType = userType;
            this.personId = person.id;
        }

        public int UserId { get => userId; }
        public string NickName { get => nickName; }
        public string Password { get => password; }
        public string UserType { get => userType; }
        public int? ThreadId { get; set; }

        public override string ToString()
        {
            return $"Person info: {base.FirstName} {this.LastName} ({this.Email}), postal code: {this.Address}; <br>User info: {this.NickName} ({this.UserType}), password set: {this.password.Length > 0}";
        }

        public bool authorised(int threadId)
        {
            if (threadId == this.ThreadId)
                return true;
            else
                return false;
        }

        /**
         * SQL handlers
         */
        private static SqlCommand _insertStatement;
        private static SqlCommand _updateStatement;
        private static SqlCommand _deleteStatement;

        public static void InitADO(SqlDatabaseConnection conn)
        {
            string insertSql = @"INSERT INTO Users (firstName, lastName, email, address, nickName, password, userType) VALUES (@FirstName, @LastName, @Email, @Address, @NickName, @Password, @UserType); SELECT SCOPE_IDENTITY();";
            _insertStatement = new SqlCommand(insertSql, conn.sqlConn);
            _insertStatement.Prepare();
            string deleteSql = @"DELETE FROM Users WHERE userId = @UserId";
            _deleteStatement = new SqlCommand(deleteSql, conn.sqlConn);
            _deleteStatement.Prepare();
            string updateSql = @"UPDATE Users SET firstName = @FirstName, lastName = @LastName, email = @Email, address = @Address, nickName = @NickName, Password = @Password, userType = @UserType WHERE userId = @UserId";
            _updateStatement = new SqlCommand(updateSql, conn.sqlConn);
            _updateStatement.Prepare();
        }

        public static void InsertObject(SqlDatabaseConnection conn, User user)
        {
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(user.password);
            SqlTransaction t = conn.Begin();
            try
            {
                _insertStatement.Transaction = t;
                _insertStatement.Parameters.Clear();

                _insertStatement.Parameters.AddWithValue("@FirstName", user.firstName);
                _insertStatement.Parameters.AddWithValue("@LastName", user.lastName);
                _insertStatement.Parameters.AddWithValue("@Email", user.email);
                _insertStatement.Parameters.AddWithValue("@Address", user.address);

                _insertStatement.Parameters.AddWithValue("@NickName", user.nickName);
                _insertStatement.Parameters.AddWithValue("@Password", passwordHash);
                _insertStatement.Parameters.AddWithValue("@UserType", user.userType);
                SqlDataReader dr = _insertStatement.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();

                t.Commit();
                _insertStatement.Parameters.Clear();
                int v = Decimal.ToInt32((decimal)dt.Rows[0].ItemArray[0]);
                user.userId = v;
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void DeleteObject(SqlDatabaseConnection conn, User user)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _deleteStatement.Transaction = t;
                _deleteStatement.Parameters.Clear();
                _deleteStatement.Parameters.AddWithValue("@UserId", user.UserId);
                _deleteStatement.ExecuteNonQuery();
                t.Commit();
                _deleteStatement.Parameters.Clear();
            }
            catch (Exception e)
            {
                t.Rollback();
            }
        }

        public static void UpdateObject(SqlDatabaseConnection conn, User user)
        {
            SqlTransaction t = conn.Begin();
            try
            {
                _updateStatement.Transaction = t;
                _updateStatement.Parameters.Clear();
                _updateStatement.Parameters.AddWithValue("@FirstName", user.firstName);
                _updateStatement.Parameters.AddWithValue("@LastName", user.lastName);
                _updateStatement.Parameters.AddWithValue("@Email", user.email);
                _updateStatement.Parameters.AddWithValue("@Address", user.address);

                _updateStatement.Parameters.AddWithValue("@NickName", user.nickName);
                _updateStatement.Parameters.AddWithValue("@Password", user.password);
                _updateStatement.Parameters.AddWithValue("@UserType", user.userType);
                _updateStatement.Parameters.AddWithValue("@UserId", user.userId);
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
                string commandSql = @"CREATE TABLE Users (userId INT NOT NULL IDENTITY, firstName varchar(255), lastName varchar(255), email varchar(255), address varchar(255), nickName varchar(255), password varchar(255), userType varchar(255), PRIMARY KEY(userId))";
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
                string commandSql = @"DROP TABLE IF EXISTS Users";
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
         * User actions
         */

        public static string whatToDo(DbUser.User user)
        {
            string iCanDo = "whatToDo<br>";
            if (user.userType != "none")
            {
                iCanDo += "userInfo<br>logout<br>";
            }

            if (user.userType == "admin")
            {
                iCanDo += "showUsers<br>addUser [firstName] [lastName] [email] [postalCode] [nickName] [password] [userType]<br>";
            }
            if (user.userType == "employee")
            {
                iCanDo += "addSeedling [name] [quantity] [pricePerUnit]<br>showSeedlings<br>show orders: showOrders<br>";
            }
            if (user.userType == "user")
            {
                iCanDo += "showSeedlings<br>add [seedlingId] [yourSeedlingsQuantity] | add seedling to cart<br>placeOrder<br>cart<br>orders<br>";
            }
            else
            {
                iCanDo += "login [username] [password]<br>";
            }

            return iCanDo;
        }

        public static string loadUser(string login, string password)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            DbUser.QueryUsers.initADO(mfp);

            DataTable dt = DbUser.QueryUsers.Query(mfp);
            Console.WriteLine("Returned rows " + dt.Rows.Count + ", containing " + dt.Columns.Count + " columns");

            string rowTableData = "";
            foreach (DataRow r in dt.Rows)
            {
                int field = 0;
                string rowData = "";
                bool loginVeryfied = false;
                bool passwordVerified = false;
                foreach (var f in r.ItemArray)
                {
                    if (field == 0)
                        rowData += f.ToString();
                    else
                        rowData += $";{f.ToString()}";

                    if (dt.Columns[field].ColumnName == "nickName" && f.ToString() == login)
                        loginVeryfied = true;

                    if (dt.Columns[field].ColumnName == "password")
                        passwordVerified = BCrypt.Net.BCrypt.Verify(password, f.ToString());
                    field++;
                }


                if (loginVeryfied && passwordVerified)
                    rowTableData += rowData;
            }

            mfp.TerminateConnection();
            Thread.Sleep(100);

            return rowTableData;
        }

        public static string orders(string email)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.QueryOrders.initADO(mfp);

            DataTable dt = System.QueryOrders.Query(mfp);
            Console.WriteLine("Returned rows " + dt.Rows.Count + ", containing " + dt.Columns.Count + " columns");
            string rowTableData = "";
            foreach (DataRow r in dt.Rows)
            {
                int field = 0;
                string rowData = "";
                bool userOrder = false;
                foreach (var f in r.ItemArray)
                {
                    if (field == 0)
                        rowData += f.ToString();
                    else
                        rowData += $";{f.ToString()}";

                    if (dt.Columns[field].ColumnName == "details")
                    {
                        string s = f.ToString();
                        string str = s.Substring(0, s.IndexOf(";"));
                        if (str == email)
                        {
                            userOrder = true;
                        }
                    }
                    field++;
                }

                if (userOrder)
                    rowTableData += $"{rowData}<br>";
            }

            mfp.TerminateConnection();
            Thread.Sleep(100);

            return rowTableData;
        }
    }

    public class SqlDatabaseConnection
    {
        public SqlConnection sqlConn;

        public SqlDatabaseConnection() { }

        public void InitConnection()
        {
            string connectionString = "Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=C:\\Codding\\CSharp\\SeedlingManagementSystem\\SeedlingManagementSystem\\SeedlingManagementSystemDatabase.mdf;Integrated Security=True";
            sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();
            Console.WriteLine("Database connection server version: " + sqlConn.ServerVersion.ToString());
        }


        public void TerminateConnection()
        {
            sqlConn.Close();
            Console.WriteLine("Database connection closed");
        }


        public SqlTransaction Begin()
        {
            return sqlConn.BeginTransaction();
        }

        public void Commit(SqlTransaction sqltransaction)
        {
            sqltransaction.Commit();
        }

        public void Rollback(SqlTransaction sqltransaction)
        {
            sqltransaction.Rollback();
        }
    }

    public class QueryUsers
    {
        private static SqlCommand _selectStatement;

        public static void initADO(DbUser.SqlDatabaseConnection conn)
        {
            string selectSql = @"SELECT * FROM Users;";
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

    public class QueryPersonCars
    {
        private static SqlCommand _selectStatement;

        public static void initADO(DbUser.SqlDatabaseConnection conn)
        {
            string selectSql = @"SELECT p.firstName, p.lastName, p.age, c.name, c.registered FROM Person p, Car c WHERE p.id = c.ownerId AND p.id = @Id";
            _selectStatement = new SqlCommand(selectSql, conn.sqlConn);
            _selectStatement.Prepare();
        }

        public static DataTable Query(DbUser.SqlDatabaseConnection conn, DbUser.Person person)
        {
            SqlTransaction t = conn.Begin();
            DataTable dt = new DataTable();
            try
            {
                _selectStatement.Transaction = t;
                _selectStatement.Parameters.Clear();
                _selectStatement.Parameters.AddWithValue("@Id", person.id);
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

    class Admin
    {
        /*
         * Admin actions
         */

        public static string showUsers()
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            DbUser.QueryUsers.initADO(mfp);

            DataTable dt = DbUser.QueryUsers.Query(mfp);
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
                        if (dt.Columns[field].ColumnName == "password")
                            tableData += $";###";
                        else
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

        public static void addUser(string firstName, string lastName, string email, string address, string nickName, string password, string userType)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            DbUser.User.InitADO(mfp);

            DbUser.Person p = new DbUser.Person(firstName, lastName, email, address);
            DbUser.User u = new DbUser.User(p, nickName, password, userType);
            DbUser.User.InsertObject(mfp, u);

            mfp.TerminateConnection();
            Thread.Sleep(100);
        }
    }

    class Employee
    {
        /*
         * Employee actions
         */
        public static string showOrders()
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.QueryOrders.initADO(mfp);

            DataTable dt = System.QueryOrders.Query(mfp);
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

        public static void addSeedling(string name, int quantity, float pricePerUnit)
        {
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            System.Seedling.InitADO(mfp);

            System.Seedling s = new System.Seedling(0, name, pricePerUnit, quantity);
            System.Seedling.InsertObject(mfp, s);

            mfp.TerminateConnection();
            Thread.Sleep(100);
        }

    }
}