using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace SocketsSSLServer
{
    public class SocketServer
    {
        int socketNo;
        TcpListener listener;
        bool endFlag = true;
        Task mainLoopTask;
        string host;
        X509Certificate2 serverCertificate = null;


        public SocketServer(string _host, int _socketNo)
        {
            socketNo = _socketNo;
            host = _host;
            string certFile = "C:\\SeedlingManagementSystem\\certificate\\localhost.pfx";
            string password = "mytopsecretpasswd";
            serverCertificate = new X509Certificate2(certFile, password, X509KeyStorageFlags.MachineKeySet);
        }

        public void Initialize()
        {
            listener = new TcpListener(IPAddress.IPv6Any, socketNo);
            listener.Server.DualMode = true;
            listener.Start();
        }


        public void AcceptsRequests()
        {
            Database.databaseInit();
            DbUser.Person person = new DbUser.Person("FirstName", "LastName", "example@email.com", "00-000");
            DbUser.User user = new DbUser.User(person, 1, "user", "", "none");

            mainLoopTask = Task.Factory.StartNew(() =>
            {
                while (endFlag)
                {
                    TcpClient client = listener.AcceptTcpClient();
                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            NetworkStream nstr = client.GetStream();
                            SslStream sslStream = new SslStream(nstr, false);

                            sslStream.AuthenticateAsServer(serverCertificate, false, true);

                            StreamReader sr = new StreamReader(new BufferedStream(sslStream), Encoding.UTF8);
                            StreamWriter sw = new StreamWriter(sslStream, Encoding.UTF8);

                            int threadId = Thread.CurrentThread.ManagedThreadId;
                            while (endFlag)
                            {
                                try
                                {
                                    string message = sr.ReadLine();
                                    string response = message.ToUpper().Trim();

                                    response = Server.handleReceivedData(user, message, threadId);

                                    sw.WriteLine(response);
                                    sw.Flush();
                                    Console.WriteLine("Thread: " + threadId + " receive: " + message + " send: " + response);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.ToString());
                                    break;
                                }
                            }
                            sslStream.Close();
                            nstr.Close();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                        client.Close();
                    });
                }
            });

            while (true)
            {
                try
                {
                    string data;
                    Console.Write("Enter text (q - exits): ");
                    data = Console.ReadLine();

                    if (data.CompareTo("q") == 0)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    break;
                }
            }
        }

        public void Close()
        {
            endFlag = false;
            listener.Stop();
            Task.WaitAll(mainLoopTask);
        }
    }

    class Database
    {
        public static void databaseInit() 
        {
            Console.WriteLine("Lets open database!");
            DbUser.SqlDatabaseConnection mfp = new DbUser.SqlDatabaseConnection();
            mfp.InitConnection();

            // Users start
            DbUser.User.DropTable(mfp);

            DbUser.User.CreateTable(mfp);

            DbUser.User.InitADO(mfp);

            DbUser.Person p0 = new DbUser.Person("Bob", "Bobson", "bob.bobson@seedlings.com", "12-123");
            DbUser.Person p1 = new DbUser.Person("Mickey", "Mouse", "mickey@a.com", "Abc");
            DbUser.Person p2 = new DbUser.Person("Donald", "Duck", "donald@a.com", "Def");
            DbUser.Person p3 = new DbUser.Person("Goofy", "Error", "goofy@a.com", "Ghi");

            DbUser.User u0 = new DbUser.User(p0, "bb", "123", "admin");
            DbUser.User u1 = new DbUser.User(p1, "mm", "123", "user");
            DbUser.User u2 = new DbUser.User(p2, "dd", "123", "employee");
            DbUser.User u3 = new DbUser.User(p3, "gc", "123", "user");

            DbUser.User.InsertObject(mfp, u0);
            DbUser.User.InsertObject(mfp, u1);
            DbUser.User.InsertObject(mfp, u2);
            DbUser.User.InsertObject(mfp, u3);
            // Users end

            // Seedlings start
            System.Seedling.DropTable(mfp);

            System.Seedling.CreateTable(mfp);

            System.Seedling.InitADO(mfp);

            System.Seedling s0 = new System.Seedling(1, "Ficus elastica Belize", 18.99f, 34);
            System.Seedling s1 = new System.Seedling(2, "Philodendron gloriosum", 34.89f, 8);
            System.Seedling s2 = new System.Seedling(3, "Philodendron White Knight", 78.97f, 13);
            
            System.Seedling.InsertObject(mfp, s0);
            System.Seedling.InsertObject(mfp, s1);
            System.Seedling.InsertObject(mfp, s2);
            // Seedlings end

            // Orders start
            System.Order.DropTable(mfp);

            System.Order.CreateTable(mfp);

            System.Order.InitADO(mfp);

            System.Order o0 = new System.Order(1, "abc@wp.pl;(id:1)7x12.22;(id:2)3x4.11", "placed");
            System.Order o1 = new System.Order(2, "aaa@wp.pl;(id:1)3x12.22", "placed");
            System.Order o2 = new System.Order(3, "abc@wp.pl;(id:1)2x12.22;(id:2)1x4.11", "sent");

            System.Order.InsertObject(mfp, o0);
            System.Order.InsertObject(mfp, o1);
            System.Order.InsertObject(mfp, o2);
            // Orders end

            mfp.TerminateConnection();
            Thread.Sleep(100);
        }

    }

    class Server
    {
        public static string handleReceivedData(DbUser.User user, string message, int threadId)
        {
            var logger = new Logger.Logger();

            string[] data = message.Split(null);
            string result = "";
            switch (data[0])
            {
                case "whatToDo":
                    if (data.Length == 1)
                        result = DbUser.User.whatToDo(user);
                    else
                        result += "Something went wrong.";
                    break;
                case "userInfo":
                    if (data.Length == 1 && user.authorised(threadId))
                        result += user.ToString();
                    else
                        result += "Your account have to be authenticated. Make sure that you type correct data!";
                    break;
                case "login":
                    if (user.authorised(threadId))
                        result += $"You are already logged in.";
                    else
                    {
                        if (data.Length == 3)
                        {
                            string userData = DbUser.User.loadUser(data[1], data[2]);
                            
                            if (userData != "")
                            {
                                string[] userDataSplit = userData.Split(";");

                                user.firstName = userDataSplit[1];
                                user.lastName = userDataSplit[2];
                                user.email = userDataSplit[3];
                                user.address = userDataSplit[4];
                                user.nickName = userDataSplit[5];
                                user.password = userDataSplit[6];
                                user.userType = userDataSplit[7];

                                user.ThreadId = threadId;
                                result += $"Hello, {user.FirstName}! You've logged on succesfully on {user.NickName} account.";
                            }
                            else
                                result += $"User not found.";
                        }
                        else
                            result = "Make sure that you type correct data!";
                    }
                    break;
                case "logout":
                    if (data.Length == 1 && user.authorised(threadId))
                    {
                        user.ThreadId = null;
                        result += $"Logged out successfully.";
                    }
                    else
                        result += $"To logged out you have to login first.";
                    break;
                case "showUsers":
                    if (data.Length == 1 && user.authorised(threadId) && user.userType == "admin")
                        result = $"<br>{DbUser.Admin.showUsers()}";
                    else
                        result = $"Db error.";
                    break;
                case "addUser":
                    if (data.Length == 8 && user.authorised(threadId) && user.userType == "admin")
                    {
                        string firstName = data[1];
                        string lastName = data[2];
                        string email = data[3];
                        string address = data[4];
                        string nickName = data[5];
                        string password = data[6];
                        string userType = data[7];

                        if (!Validation.UserData.firstNameValid(firstName))
                            result += Validation.UserData.firstNameValidationFeedback(firstName);
                        if (!Validation.UserData.lastNameValid(lastName))
                            result += Validation.UserData.lastNameValidationFeedback(lastName);
                        if (!Validation.UserData.emailValid(email))
                            result += Validation.UserData.emailValidationFeedback(email);
                        if (!Validation.UserData.addressValid(address))
                            result += Validation.UserData.addressValidationFeedback(address);

                        else
                        {
                            DbUser.Admin.addUser(firstName, lastName, email, address, nickName, password, userType);
                            result = $"{firstName} added to db.";
                        }
                    }
                    else
                        result = $"Db insert error.";
                    break;
                case "addSeedling":
                    if (data.Length == 4 && user.authorised(threadId) && user.userType == "employee")
                    {
                        string name = data[1];
                        int quantity = int.Parse(data[2]);
                        float pricePerUnit = float.Parse(data[3]);

                        DbUser.Employee.addSeedling(name, quantity, pricePerUnit);
                        result = $"{data[1]} added to db.";
                    }
                    else
                        result = $"Db insert error.";
                    break;
                case "showSeedlings":
                    if (data.Length == 1 && user.authorised(threadId) && (user.userType == "employee" || user.userType == "user"))
                    {
                        result = $"<br>{System.Seedling.showSeedlings()}";
                    }
                    else
                    {
                        result = $"Db error.";
                    }
                    break;
                case "add":
                    if (data.Length == 3 && user.authorised(threadId) && user.userType == "user")
                    {
                        try
                        {
                            int seedlingId = int.Parse(data[1]);
                            int seedlingsQuantity = int.Parse(data[2]);
                            

                            string dataFromDb = System.Seedling.seedlingDataById(seedlingId);
                            
                            string[] row = dataFromDb.Split(";");
                            string s = $";(id:{row[0]}){row[1]}x{row[2]}";

                            string sId = row[0];
                            string sName = row[1];
                            string sQuantity = row[2];
                            string sPrice = row[3];

                            float price = float.Parse(sPrice);

                            if (int.Parse(sId) == seedlingId && int.Parse(sQuantity) >= seedlingsQuantity)
                            {
                                Cart.AddSeedling(seedlingId, seedlingsQuantity, price);
                                result += $"Seedling added to cart. <br>";
                            }
                            else
                                result += $"Unable to find seedling with typed data. Check if seedling with typed id exists.";
                        }
                        catch (Exception e)
                        {
                            result = $"<br>{e}";
                        }
                    }
                    else
                    {
                        result = $"Adding to cart error.";
                    }
                    break;
                case "placeOrder":
                    if (data.Length == 1 && user.authorised(threadId) && user.userType == "user")
                    {
                        try
                        {
                            string description = $"{user.email}";
                            foreach (string elem in Cart.cart)
                            {
                                string[] row = elem.Split(";");
                                description += $";(id:{row[0]}){row[1]}x{row[2]}";
                            }

                            DateTime dt = DateTime.Now;
                            int ms = dt.Millisecond;
                            Order o = new Order(1, description, "placed");
                            System.Order.placeOrder(o);

                            o = null;
                            Cart.cart.Clear();
                            result = $"Your order has been placed and cart has been set to empty.";
                        }
                        catch (Exception e)
                        {
                            result = $"<br>{e}";
                        }
                    }
                    else
                    {
                        result = $"Db update error.";
                    }
                    break;
                case "cart":
                    if (data.Length == 1 && user.authorised(threadId) && user.userType == "user")
                    {
                        string s = "";
                        foreach (string elem in Cart.cart)
                        {
                            s += $"{elem}<br>";
                        }
                        result = $"Seedlings added to cart:<br>{s}";
                    }
                    else
                    {
                        result = $"Adding to cart error.";
                    }
                    break;
                case "showOrders":
                    if (data.Length == 1 && user.authorised(threadId) && (user.userType == "employee"))
                    {
                        result = $"<br>{DbUser.Employee.showOrders()}";
                    }
                    else
                    {
                        result = $"Db error.";
                    }
                    break;
                case "orders":
                    if (data.Length == 1 && user.authorised(threadId) && (user.userType == "user"))
                    {
                        result = $"<br>{DbUser.User.orders(user.email)}";
                    }
                    else
                    {
                        result = $"Db error.";
                    }
                    break;
            }

            logger.Log($"(User email: {user.email}) {result}");
            return result;

        }
        
        
    }
}
