/*
Amanda M Lincoln
CIDM 2315-70 Final Project

This is a hotel management application intended to manage the following basic check-in and check-out functions, as well as manage data using mySQL:

1. View a list of available and reserved rooms at any given time
2. Check In Customers
3. Check Out Customers
4. Pull Records from Archives

The program also provides a lot of leeway for user error, with loops to account for user errors, and extra escape keys if a function must be ended prematurely.

References:

1. Anton Miki, Selman Genc, 13 October 2014, "Checking Console.ReadLine()!=null", stackoverflow, accessed 18 June 2023, https://stackoverflow.com/questions/26338571/checking-console-readline-null
2. "Int32.TryParse Method", Microsoft, accessed 4 August 2023, https://learn.microsoft.com/en-us/dotnet/api/system.int32.tryparse?view=net-7.0
3. Kirk Lankin, LeonardoX, 12 January 2023, "Converting null literal or possible null value to non-nullable type", stackoverflow, accessed 5 August 2023  https://stackoverflow.com/questions/62899915/converting-null-literal-or-possible-null-value-to-non-nullable-type 
4. Luis Contreras Garcia, Steve, 24 April 2019, "How to insert date/time in Mysql through c#?", stackoverflow, accessed 7 August 2023, https://stackoverflow.com/questions/55839076/how-to-insert-date-time-in-mysql-through-c 
5. Srinivas, 9 May 2020, "Copy Data from one table to another table in MySQL", Medium, accessed 7 August 2023, https://medium.com/@shadow.technos/copy-data-from-one-table-to-another-table-in-mysql-f1d563211028
6. Suman Saman, 21 October 2015, "Exit Methods in C# Application", C# Corner, accessed 30 June 2023, https://www.delftstack.com/howto/csharp/exit-console-application-in-csharp/

mySQL Specfic References:

For auto-incrementing primary key in Archives database
7. Sanchit Agarwal, 27 January 2022, "Setting up MySQL Auto Increment Primary Key 101: Syntax & Usage Simplified", Hevo, accessed 3 August 2023, https://hevodata.com/learn/mysql-auto-increment-primary-key/#t3
*/

namespace FinalProject;

using System.Data;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("----- CIDM2315 Final Project: Amanda Lincoln -----");
        Console.WriteLine("----- Welcome to Buff Hotel -----");

        bool loginVerification = UserLogin();

        while (loginVerification)
        {
            Console.WriteLine("*******************");
            Console.WriteLine("--> Please select:");
            Console.WriteLine("1. Show Available Room");
            Console.WriteLine("2. Check-In");
            Console.WriteLine("3. Show Reserved Room");
            Console.WriteLine("4. Check-Out");
            // Only extra full function not listed on instructions. Added basic archive storing and viewing capabilities.
            Console.WriteLine("5. Pull Records From Archives"); 
            Console.WriteLine("6. Log Out");
            Console.WriteLine("*******************");

            string ? userChoice = Console.ReadLine();

            switch (userChoice)
            {
                case "1":
                    PrintRoomListings("Available");
                    break;
                case "2":
                    int validCapacity;
                    GetValidInteger(2, out validCapacity);
                    if (validCapacity == 9999)
                    {
                        Console.WriteLine("--> Return to main menu");
                        break;
                    }
                    
                    if (PrintRoomListings(validCapacity))
                    {
                        break;
                    }
                    else
                    {   
                        CheckIn();
                        break;
                    }
                case "3":
                    PrintRoomListings("Reserved");
                    break;
                case "4":
                    CheckOut();
                    break;
                case "5":
                    AccessArchives();
                    break;
                case "6":
                    Console.WriteLine("-->Log out System");
                    // Environment.Exit(0) taken from reference 6.
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("--> Invalid entry");
                    continue;
            }
        }    
    }
    public static bool UserLogin()
    {
        Dictionary <string, string> authorized_users = new Dictionary <string, string>();
        // Hard coded per instructions; chose list function over including in database due to database not being case-sensitive and thus less secure.
        authorized_users.Add("alice", "alice123");

        Console.WriteLine("--> Please input username: ");
        string ? inpUsername = Console.ReadLine();
        Console.WriteLine("--> Please input password: ");
        string ? inpPassword = Console.ReadLine();
        // All uses of ! prompted by null warning messages and taken from reference 3.
        if ((authorized_users.ContainsKey(inpUsername!)) && (authorized_users[inpUsername!] == inpPassword))
        {
            Console.WriteLine("--> Login Successfully.");
            Console.WriteLine($"** Hello User: {inpUsername} **");
            return true;
        }
        else 
        {
            Console.WriteLine("--> Wrong Username/Password");
            return false;
        }
    }
    public static void PrintRoomListings(string inpAvailability)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        int sum = 0;
        if (inpAvailability == "Available")
        {        
            string sqlPRLa = @"SELECT COUNT(*) AS countRooms FROM Rooms WHERE Rooms.roomAvailability = 'Available';";

            if (EmptyRecordsCheck(sqlPRLa))
            {
                Console.WriteLine("No rooms available at this time.");
            }
            else
            {
                try
                {
                    conn.Open();

                    string sqlPRLa2 = "SELECT roomNumber, roomCapacity FROM Rooms WHERE roomAvailability = 'Available';";
                    MySqlCommand cmd = new MySqlCommand (sqlPRLa2, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Console.WriteLine($"+ Room number: {rdr["roomNumber"]}; Capacity: {rdr["roomCapacity"]}");
                        sum += 1;
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine ("Unable to pull list of rooms at this time");
                    Console.WriteLine(ex.ToString());
                }
                conn.Close();
                Console.WriteLine($"------Number of Available Rooms: {sum}-------");
            }    
        }
        else
        {
            string sqlPRLa3 = @"SELECT COUNT(*) AS countRooms FROM Rooms WHERE Rooms.roomAvailability = 'Reserved';";

            if (EmptyRecordsCheck(sqlPRLa3))
            {
                Console.WriteLine("No rooms reserved at this time.");
            }
            else
            {
                try
                {
                    Console.WriteLine("-------Reserved Room----------");
                    conn.Open();

                    string sqlPRLa4 = "SELECT roomNumber, customerName FROM ReservedRoomList;";
                    MySqlCommand cmd = new MySqlCommand (sqlPRLa4, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        Console.WriteLine($"+ Room: {rdr["roomNumber"]}; Customer: {rdr["customerName"]};");
                    }
                    rdr.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine ("Unable to pull list of rooms at this time");
                    Console.WriteLine(ex.ToString());
                }  
                conn.Close();  
            }    
        }
    }
    public static bool PrintRoomListings(int inpCapacity)
    {
        int sum = 0;
        for (int capacity = inpCapacity; capacity <= 4; capacity++)
        {
            string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
            MySqlConnection conn = new MySqlConnection (connStr);
            try
            {
                conn.Open();

                string sqlPRLb = "SELECT roomNumber, roomCapacity FROM Rooms WHERE roomCapacity = @roomCapacity AND roomAvailability = 'Available';";
                MySqlCommand cmd = new MySqlCommand (sqlPRLb, conn);
                cmd.Parameters.AddWithValue("@roomCapacity", capacity);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Console.WriteLine($"+ Room number: {rdr["roomNumber"]}; Capacity: {rdr["roomCapacity"]}");
                    sum += 1;
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine ("Unable to pull list of rooms at this time");
                Console.WriteLine(ex.ToString());
            }
            conn.Close(); 
        }

        Console.WriteLine($"------Number of Available Rooms: {sum}-------");

        if (sum == 0)
        {
            Console.WriteLine("--> No suitable room at this time.");
            return true;
        }
        else 
        {
            return false;
        }      
    }
    public static void GetValidRoomNumber(string inpAvailability, out int validRoomNumber)
    {
        validRoomNumber = 0;
        int intRoomNumber = 0;
        while ((intRoomNumber <= 0) || (validRoomNumber == 0))
        {
            Console.WriteLine("--> Input Room Number or Input ~ to Return to Main Menu:");
            string ? inpRoomNumber = Console.ReadLine();
            // All uses of int.TryParse(parameter, out parameter) taken from reference 2.
            if (int.TryParse(inpRoomNumber, out intRoomNumber))
            {
                if (inpAvailability == "Available")
                {
                    string sqlGVRNa = @"SELECT COUNT(*) AS countRooms FROM Rooms WHERE Rooms.roomNumber = @roomNumber AND Rooms.roomAvailability = 'Available';";
                
                    if (EmptyRecordsCheck(sqlGVRNa, intRoomNumber))
                    {
                        Console.WriteLine("Room is currently unavailable or entry is invalid");
                        continue;
                    }
                    else 
                    {
                        validRoomNumber = intRoomNumber;
                    }
                }
                else
                {
                    string qlGVRNa2 = @"SELECT COUNT(*) AS countRooms FROM Rooms WHERE Rooms.roomNumber = @roomNumber AND Rooms.roomAvailability = 'Reserved';";

                    if (EmptyRecordsCheck(qlGVRNa2, intRoomNumber))
                    {
                        Console.WriteLine("--> Could not find a customer record of this room");
                        continue;
                    }
                    else 
                    {
                        validRoomNumber = intRoomNumber;
                    }
                }    
            }
            else if (inpRoomNumber == "~")
            {
                validRoomNumber = 9999;
                break;
            }
            else
            {
                Console.WriteLine("--> Invalid entry");
                continue;
            }
        }
    }
    public static void GetValidInteger(int inpIndicator, out int validInteger)
    {
        validInteger = 0;
        int intInteger = 0;
        while ((intInteger <= 0) || (validInteger == 0))
        {
            if (inpIndicator == 1)
            {
                Console.WriteLine("--> Input WTAMU-Issued ID Number or Input ~ to Return to Main Menu:");
            }
            else
            {
                Console.WriteLine("--> Input Number of People or Press ~ to Return to Main Menu:");
            }

            string ? inpInteger = Console.ReadLine();

            if (int.TryParse(inpInteger, out intInteger))
            {
                if (intInteger <= 0)
                {
                    Console.WriteLine("--> Invalid entry");
                    continue;
                }
                validInteger = intInteger;
            }
            else if (inpInteger == "~")
            {
                validInteger = 9999;
                break;
            }
            else
            {
                Console.WriteLine("--> Invalid entry");
                continue;
            }
        }
    }
    public static void GetValidCustomerName(out string validCustomerName)
    {
        validCustomerName = "";
        // All instances of string.IsNullOrEmpty(variable) taken from reference 1.
        while ((string.IsNullOrEmpty(validCustomerName)))
        {
            Console.WriteLine("--> Input Customer Name or Input ~ to Return to Main Menu:");
            string ? inpCustomerName = Console.ReadLine();

            if (string.IsNullOrEmpty(inpCustomerName))
            {
                Console.WriteLine("--> Invalid entry");
                continue;
            }
            else
            {
                validCustomerName = inpCustomerName;   
            }
        }
    }
    public static void GetValidCustomerEmail(out string validCustomerEmail)
    {
        validCustomerEmail = "";
        while ((string.IsNullOrEmpty(validCustomerEmail)))
        {
            Console.WriteLine("--> Input Customer Email or Input ~ to Return to Main Menu:");
            string ? inpCustomerEmail = Console.ReadLine();

            if (string.IsNullOrEmpty(inpCustomerEmail))
            {
                Console.WriteLine("--> Invalid entry");
                continue;
            }
            else
            {
                validCustomerEmail = inpCustomerEmail;
                break;
            }
        }
    }    
    public static void GetValidConfirmation(int inpIndicator, out string validConfirmation)
    {
        string [] acceptableConfirmationInput = {"y", "Y", "N", "n", "~"};
        validConfirmation = "";

        if (inpIndicator == 1)
        {
            while (string.IsNullOrEmpty(validConfirmation))
            {
                Console.WriteLine("Press y to confirm customer name, n to update, or ~ to exit to main menu:");
                string ? inpConfirmation = Console.ReadLine();
            
                if (acceptableConfirmationInput.Contains(inpConfirmation))
                {
                    validConfirmation = inpConfirmation!;
                }
                else
                {
                    Console.WriteLine("--> Invalid entry");
                    continue;
                }
            }
        }
        else
        {
            while (string.IsNullOrEmpty(validConfirmation))
            {
                Console.WriteLine("Press y to confirm customer email, n to update, or ~ to exit to main menu:");
                string ? inpConfirmation = Console.ReadLine();
            
                if (acceptableConfirmationInput.Contains(inpConfirmation))
                {
                    validConfirmation = inpConfirmation!;
                }
                else
                {
                    Console.WriteLine("--> Invalid entry");
                    continue;
                }
            }
        }    
    }
    public static bool EmptyRecordsCheck(string inpSql)
    {
       string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
       MySqlConnection conn = new MySqlConnection (connStr);
       string count = "";

       try
       {
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (inpSql, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = rdr["countRooms"].ToString()!;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to check room statuses at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close(); 

        if (count == "0")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool EmptyRecordsCheck(string inpSql, int inpRoomNumber)
    {
       string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
       MySqlConnection conn = new MySqlConnection (connStr);
       string count = "";

       try
       {
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (inpSql, conn);
            cmd.Parameters.AddWithValue("@roomNumber", inpRoomNumber);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = rdr["countRooms"].ToString()!;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to check room statuses at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close(); 

        if (count == "0")
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static bool EmptyRecordsCheck(int inpInteger)
    {
       string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
       MySqlConnection conn = new MySqlConnection (connStr);
       string count = "";
       
       try
       {
            string sqlERCc = @"SELECT COUNT(*) AS countCustomers FROM Customers WHERE Customers.buffID = @buffID;";
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (sqlERCc, conn);
            cmd.Parameters.AddWithValue("@buffID", inpInteger);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = rdr["countCustomers"].ToString()!;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to access customer records");
            Console.WriteLine(ex.ToString());
        }
        conn.Close(); 

        if (count == "0")
        {
            return true;
        }
        else
        {
            return false;
        }  
    }
    public static void CheckIn()
    {
        int validRoomNumber;
        GetValidRoomNumber("Available", out validRoomNumber);

        if (validRoomNumber == 9999)
        {
            Console.WriteLine("--> Return to main menu");
        }
        else
        {
            // Customer name was insufficient as a customer database key; decided to use wtamu-issued ID as unique identifier
            int validBuffID;
            GetValidInteger(1, out validBuffID);
            
            if (validBuffID == 9999)
            {
                Console.WriteLine("--> Return to main menu");
            }
            else if (EmptyRecordsCheck(validBuffID))
            {
                string validCustomerName;
                GetValidCustomerName(out validCustomerName);
        
                if (validCustomerName == "~")
                {
                    Console.WriteLine("--> Return to main menu");
                }
                else
                {
                    // Allowed null to be considered a valid entry for those that do not have an email, though rare, or those who do not wish to provide it
                    Console.WriteLine("--> Input Customer Email or Input ~ to Return to Main Menu:");
                    string ? inpCustomerEmail = Console.ReadLine();

                    if (inpCustomerEmail == "~")
                    {
                        Console.WriteLine("--> Return to main menu");
                    }
                    else
                    {
                        NewCustomerRecord(validBuffID, validCustomerName, inpCustomerEmail!);
                        CheckInUpdate(validBuffID, validRoomNumber);
                    }
                }    
            }
            else
            {
                ExistingCustomerCheckIn(validBuffID, validRoomNumber);
            }
        } 
    } 
    public static void NewCustomerRecord(int inpBuffID, string inpCustomerName, string inpCustomerEmail)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        try
        {
            conn.Open();
                            
            string sqlNCR = "INSERT INTO Customers VALUES (@buffID, @customerName, @customerEmail);";
            MySqlCommand cmd = new MySqlCommand (sqlNCR, conn);
            cmd.Parameters.AddWithValue("@customerName", inpCustomerName);
            cmd.Parameters.AddWithValue("@customerEmail", inpCustomerEmail);
            cmd.Parameters.AddWithValue("@buffID", inpBuffID);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to create customer record at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close();
    } 
    public static void CheckInUpdate(int inpBuffID, int inpRoomNumber)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        try
        {
            conn.Open();
            // All instances of DateTime dateNow = DateTime.Now;, dateNow taken from reference 4.
            DateTime dateNow = DateTime.Now;
            dateNow.ToString("MM/dd/yyyy HH:mm");

            string sqlCIUa = "INSERT INTO Occupancy (roomNumber, buffID, checkInDate) VALUES (@roomNumber, @buffID, @checkInDate);";
            MySqlCommand cmd = new MySqlCommand (sqlCIUa, conn);
            cmd.Parameters.AddWithValue("@roomNumber", inpRoomNumber);
            cmd.Parameters.AddWithValue("@buffID", inpBuffID);
            cmd.Parameters.AddWithValue("@checkInDate", dateNow);
            cmd.ExecuteNonQuery();

            string sqlCIUa2 = @"UPDATE Rooms SET RoomAvailability = 'Reserved' WHERE roomNumber = @roomNumber;";
            MySqlCommand cmd2 = new MySqlCommand (sqlCIUa2, conn);
            cmd2.Parameters.AddWithValue("@roomNumber", inpRoomNumber);
            cmd2.ExecuteNonQuery();

            Console.WriteLine($"--> Check-In successfully! Customer is assigned to room {inpRoomNumber}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to check-in at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close();
    }
    public static void CheckInUpdate (int inpBuffID, int inpRoomNumber, string inpCustomerUpdate, int inpIndicator)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        switch (inpIndicator)
        {
            case 1:
                try
                {
                    conn.Open();
    
                    string sqlCIOb = @"UPDATE Customers SET customerName = @customerName WHERE buffID = @buffID;";
                    MySqlCommand cmd = new MySqlCommand (sqlCIOb, conn);
                    cmd.Parameters.AddWithValue("@customerName", inpCustomerUpdate);
                    cmd.Parameters.AddWithValue("@buffID", inpBuffID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> Unable to check-in at this time");
                    Console.WriteLine(ex.ToString());
                }
                CheckInUpdate(inpBuffID, inpRoomNumber);
                break;
            case 2:
                try
                {
                    conn.Open();

                    string sqlCIOb2 = @"UPDATE Customers SET customerEmail = @customerEmail WHERE buffID = @buffID;";
                    MySqlCommand cmd = new MySqlCommand (sqlCIOb2, conn);
                    cmd.Parameters.AddWithValue("@customerEmail", inpCustomerUpdate);
                    cmd.Parameters.AddWithValue("@buffID", inpBuffID);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("--> Unable to check-in at this time");
                    Console.WriteLine(ex.ToString());
                }
                CheckInUpdate(inpBuffID, inpRoomNumber);
                break;  
        }
    }  
    public static void CheckInUpdate (int inpBuffID, int inpRoomNumber, string inpCustomerNameUpdate, string inpCustomerEmailUpdate)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        try
        {
            conn.Open();

            string sqlCIUc = @"UPDATE Customers SET customerName = @customerName WHERE buffID = @buffID;";
            MySqlCommand cmd = new MySqlCommand (sqlCIUc, conn);
            cmd.Parameters.AddWithValue("@customerName", inpCustomerNameUpdate);
            cmd.Parameters.AddWithValue("@buffID", inpBuffID);
            cmd.ExecuteNonQuery();

            string sqlCIUc2 = @"UPDATE Customers SET customerEmail = @customerEmail WHERE buffID = @buffID;";
            MySqlCommand cmd2 = new MySqlCommand (sqlCIUc2, conn);
            cmd2.Parameters.AddWithValue("@customerEmail", inpCustomerEmailUpdate);
            cmd2.Parameters.AddWithValue("@buffID", inpBuffID);
            cmd2.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to check-in at this time");
            Console.WriteLine(ex.ToString());
        }
        CheckInUpdate(inpBuffID, inpRoomNumber);
    }  
    public static void ExistingCustomerCheckIn(int inpBuffID, int inpRoomNumber)
    {
        string customerName;
        string customerEmail;
        PullCustomerInformation(inpBuffID, out customerName, out customerEmail);
        Console.WriteLine($"+ Customer Name: {customerName}; Customer Email: {customerEmail}");

        string validConfirmation;
        GetValidConfirmation(1, out validConfirmation);

        if ((validConfirmation == "y") || (validConfirmation == "Y"))
        {
                string validConfirmation2;
                GetValidConfirmation(2, out validConfirmation2);

                if ((validConfirmation2 == "y") || (validConfirmation2 == "Y"))
                {
                    CheckInUpdate(inpBuffID, inpRoomNumber);
                }
                else if ((validConfirmation2 == "n") || (validConfirmation2 == "N"))
                {
                    string validCustomerEmail;
                    GetValidCustomerEmail(out validCustomerEmail);

                    if (validCustomerEmail == "~")
                    {
                        Console.WriteLine("--> Return to main menu");
                    }
                    else
                    {
                        CheckInUpdate(inpBuffID, inpRoomNumber, validCustomerEmail, 2);
                    }
                }                        
                else
                {
                    Console.WriteLine("--> Return to main menu");
                } 
        } 
        else if ((validConfirmation == "N") || (validConfirmation == "n"))
        {
            string validCustomerName;
            GetValidCustomerName(out validCustomerName);
            
            if (validCustomerName == "~")
            {
                Console.WriteLine("--> Return to main menu.");
            }
            else
            {
                string validConfirmation2;
                GetValidConfirmation(2, out validConfirmation2);
                
                if ((validConfirmation2 == "y") || (validConfirmation2 == "Y"))
                {
                    CheckInUpdate(inpBuffID, inpRoomNumber, validCustomerName, 1);
                }
                else if ((validConfirmation2 == "n") || (validConfirmation2 == "N"))
                {
                    string validCustomerEmail;
                    GetValidCustomerEmail(out validCustomerEmail);
                    
                    if (validCustomerEmail == "~")
                    {
                        Console.WriteLine("--> Return to main menu.");
                    }
                    else
                    {
                        CheckInUpdate(inpBuffID, inpRoomNumber, validCustomerName, validCustomerEmail); 
                    }
                }
                else
                {
                    Console.WriteLine("--> Return to main menu");
                }
            } 
        } 
        else
        {
            Console.WriteLine("--> Return to main menu");
        }
    }
    public static void PullCustomerInformation(int inpBuffID, out string customerName, out string customerEmail)
    {
        customerName = "";
        customerEmail = "";
        
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        try
        {
            string sqlPCI = @"SELECT customerName, customerEmail FROM Customers WHERE Customers.buffID = @buffID;";
            
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (sqlPCI, conn);
            cmd.Parameters.AddWithValue("@buffID", inpBuffID);
            MySqlDataReader rdr = cmd.ExecuteReader();

            while (rdr.Read())
            {
                customerName = rdr["customerName"].ToString()!;
                customerEmail = rdr["customerEmail"].ToString()!;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to access customer records at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close();
    }
    public static void CheckOut()
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        int validRoomNumber; 
        GetValidRoomNumber("Reserved", out validRoomNumber);
        if (validRoomNumber == 9999)
        {
            Console.WriteLine("--> Return to main menu");
        }
        else
        {
            try
            {
                conn.Open();

                string sqlCO = "SELECT roomNumber, customerName FROM ReservedRoomList WHERE roomNumber = @roomNumber;";
                MySqlCommand cmd = new MySqlCommand (sqlCO, conn);
                cmd.Parameters.AddWithValue("@roomNumber", validRoomNumber);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    Console.WriteLine($"+ Room: {rdr["roomNumber"]}; Customer: {rdr["customerName"]};");
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }  
            conn.Close();  

            Console.WriteLine("--> Please confirm the customer name and input y to continue Check-Out or input any key to cancel.");
            string ? userConfirmation = Console.ReadLine();

            if ((userConfirmation == "y") || (userConfirmation == "Y"))
            {
                try
                {
                    conn.Open();

                    DateTime dateNow = DateTime.Now;
                    dateNow.ToString("MM/dd/yyyy HH:mm");
                    string sqlCO2 = @"UPDATE Occupancy SET checkOutDate = @checkOutDate WHERE roomNumber = @roomNumber;";
                    MySqlCommand cmd = new MySqlCommand(sqlCO2, conn);
                    cmd.Parameters.AddWithValue ("@checkOutDate", dateNow);
                    cmd.Parameters.AddWithValue ("@roomNumber", validRoomNumber);
                    cmd.ExecuteNonQuery();

                    // Syntax for copying data from one table to another taken from reference 5.
                    string sqlCO3 = @"INSERT INTO Archives (buffID, roomNumber, checkInDate, checkOutDate) SELECT buffID, roomNumber, checkInDate, checkOutDate FROM Occupancy WHERE roomNumber = @roomNumber;";
                    MySqlCommand cmd2 = new MySqlCommand (sqlCO3, conn);
                    cmd2.Parameters.AddWithValue("@roomNumber", validRoomNumber);
                    cmd2.ExecuteNonQuery();

                    string sqlCO4 = @"DELETE FROM Occupancy WHERE roomNumber = @roomNumber";
                    MySqlCommand cmd3 = new MySqlCommand (sqlCO4, conn);
                    cmd3.Parameters.AddWithValue("@roomNumber", validRoomNumber);
                    cmd3.ExecuteNonQuery();

                    string sqlCO5 = @"UPDATE Rooms SET RoomAvailability = 'Available' WHERE roomNumber = @roomNumber";
                    MySqlCommand cmd4 = new MySqlCommand (sqlCO5, conn);
                    cmd4.Parameters.AddWithValue("@roomNumber", validRoomNumber);
                    cmd4.ExecuteNonQuery();

                    Console.WriteLine("--> Check-Out Successfully!");

                    PrintRoomListings("Reserved");    
                }      
                catch (Exception ex)
                {
                    Console.WriteLine("--> Unable to check out at this time");
                    Console.WriteLine(ex.ToString());
                }
                conn.Close();
            }
            else 
            {
                Console.WriteLine("--> Cancel Check-Out");
            }
        }    
    }
    public static void AccessArchives()
    {
        int validityCheck = 0;
        while (validityCheck == 0)
        {
            Console.WriteLine("--> Please enter customer's buffID or name:");
            string ? inpArchiveQuery = Console.ReadLine();

            if (string.IsNullOrEmpty(inpArchiveQuery))
            {
                Console.WriteLine("--> Invalid entry");
                continue;
            }
            else
            {
                // Will run using the string ? inpArchiveQuery and be converted in method if needed
                if (EmptyArchiveRecordsCheck(inpArchiveQuery))
                {
                    Console.WriteLine("No matching records");
                    break;
                }
                else
                {
                    int intArchiveQuery;
                    if (int.TryParse(inpArchiveQuery, out intArchiveQuery))
                    {
                        PrintRecordsFromArchives(intArchiveQuery);
                        break;
                    }
                    else
                    {
                        int validBuffID;
                        PrintPartialCustomerInformation(inpArchiveQuery, out validBuffID);
                        if (validBuffID == 9999)
                        {
                            Console.WriteLine("--> Return to main menu");
                            break;
                        }

                        if (EmptyArchiveRecordsCheck(validBuffID))
                        {
                            Console.WriteLine("No matching records");
                            break;
                        }
                        else
                        {
                            PrintRecordsFromArchives(validBuffID);
                        }
                        break;
                    }
                }
            }
        }
    }   
    public static bool EmptyArchiveRecordsCheck (string inpArchiveQuery)
    {
        // parameter for this method marked as string due to ambiguity of incoming query; user input left as string instead and converted in this method
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        int validBuffID;
        // If user inputs buff ID when querying archive
        if (int.TryParse(inpArchiveQuery, out validBuffID))
        {
            string count = "";
            try
            {
                string sqlEARCa = @"SELECT COUNT(*) AS countArchiveRecords FROM Archives WHERE Archives.buffID = @buffID;";
                conn.Open();

                MySqlCommand cmd = new MySqlCommand (sqlEARCa, conn);
                cmd.Parameters.AddWithValue("@buffID",validBuffID);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    count = rdr["countArchiveRecords"].ToString()!;
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to access archives at this time");
                Console.WriteLine(ex.ToString());
            }
            conn.Close(); 

            if (count == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        // If user inputs customer name when querying archive
        else
        {
            string count = "";
            try
            {
                string sqlEARCa2 = @"SELECT COUNT(*) AS countCustomers FROM Customers WHERE Customers.customerName = @customerName;";
                conn.Open();

                MySqlCommand cmd = new MySqlCommand (sqlEARCa2, conn);
                cmd.Parameters.AddWithValue("@customerName", inpArchiveQuery);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    count = rdr["countCustomers"].ToString()!;
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to access customer records at this time");
                Console.WriteLine(ex.ToString());
            }
            conn.Close(); 

            if (count == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    public static bool EmptyArchiveRecordsCheck (int inpArchiveQuery)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);

        string count = "";
        try
        {
            string sqlEARCb = @"SELECT COUNT(*) AS countArchiveRecords FROM Archives WHERE Archives.buffID = @buffID;";
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (sqlEARCb, conn);
            cmd.Parameters.AddWithValue("@buffID", inpArchiveQuery);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                count = rdr["countArchiveRecords"].ToString()!;
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to access archives at this time");
            Console.WriteLine(ex.ToString());
        }
        conn.Close(); 

        if (count == "0")
        {
            return true;
        }
        else
        {
            return false;
        }
    }  
    public static void PrintRecordsFromArchives(int inpArchiveQuery)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);
        
        try
        {
            string sqlPRFA = @"SELECT buffID, roomNumber, checkInDate, checkOutDate FROM Archives WHERE Archives.buffID = @buffID;";
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (sqlPRFA, conn);
            cmd.Parameters.AddWithValue("@buffID", inpArchiveQuery);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Console.WriteLine($"+ BuffID: {rdr["buffID"]}; Room number: {rdr["roomNumber"]}; Check-In Date: {rdr["checkInDate"]}; Check-Out Date: {rdr["checkOutDate"]}");
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to access archives at this time");
            Console.WriteLine(ex.ToString());
        }
            conn.Close(); 
    }
    public static void PrintPartialCustomerInformation(string inpArchiveQuery, out int buffID)
    {
        string connStr = "server=20.172.0.16;user=amlincoln1;database=amlincoln1;port=8080;password=amlincoln1";
        MySqlConnection conn = new MySqlConnection (connStr);
        
        try
        {
            string sqlPPCI = @"SELECT buffID, customerName FROM Customers WHERE Customers.customerName = @customerName;";
            conn.Open();

            MySqlCommand cmd = new MySqlCommand (sqlPPCI, conn);
            cmd.Parameters.AddWithValue("@customerName", inpArchiveQuery);
            MySqlDataReader rdr = cmd.ExecuteReader();
            while (rdr.Read())
            {
                Console.WriteLine($"+ BuffID: {rdr["buffID"]}; Customer Name: {rdr["customerName"]}");
            }
            rdr.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to access archives at this time");
            Console.WriteLine(ex.ToString());
        }
            conn.Close(); 

        int validityCheck = 0;
        buffID = 0;
        while (validityCheck == 0)
        {
            // Input this section to offset the possibility of individuals with the same name being in the archive.
            Console.WriteLine("--> Please input applicable customer ID from printed list or press ~ to return to main menu:");
            string ? inpBuffID = Console.ReadLine();

            int validBuffID;
            if (int.TryParse(inpBuffID, out validBuffID))
            {
                buffID = validBuffID;
                break;
            }
            else if (inpBuffID == "~")
            {
                buffID = 9999;
                break;
            }
            else
            {
                continue;
            }
        }    
    }
}   

