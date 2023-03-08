using System;
using System.IO;

namespace Validation
{
    public class UserData
    {
        public static bool firstNameValid(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return false;
            if (firstName.Length < 2 || firstName.Length > 50)
                return false;
            return true;
        }

        public static string firstNameValidationFeedback(string firstName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                return "First name is required.<br>";
            if (firstName.Length < 2)
                return "First name has to contain 2 or more characters.<br>";
            if (firstName.Length > 50)
                return "First name could not have more tahn 50 characters.<br>";
            return "";
        }

        public static bool lastNameValid(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return false;
            if (lastName.Length < 2 || lastName.Length > 50)
                return false;
            return true;
        }

        public static string lastNameValidationFeedback(string lastName)
        {
            if (string.IsNullOrWhiteSpace(lastName))
                return "Last name is required.<br>";
            if (lastName.Length < 2)
                return "Last name has to contain 2 or more characters.<br>";
            if (lastName.Length > 50)
                return "Last name could not have more tahn 50 characters.<br>";
            return "";
        }

        public static bool emailValid(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public static string emailValidationFeedback(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "Email is required.<br>";
            if (!emailValid(email))
                return "Invalid email format.<br>";
            return "";
        }

        public static bool addressValid(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;
            if (address.Length < 5 || address.Length > 100)
                return false;
            return true;
        }

        public static string addressValidationFeedback(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return "Postal code is required<br>";
            return "";
        }

        public static bool nickNameValid(string nickName)
        {
            /*np. same litery (brak znakow specjalnych), jedno slowo, zakres znakow min, max itd.*/
            return true;
        }

        public static string nickNameValidationFeedback(string nickName)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

        public static bool passwordValid(string password)
        {
            /*np. jedno slowo, zakres znakow min, max itd.*/
            return true;
        }

        public static string passwordValidationFeedback(string password)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

        public static bool userTypeValid(string userType)
        {
            /*np. mamy tylko 3 opcje (user, employee, admin)*/
            return true;
        }

        public static string userTypeValidationFeedback(string userType)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

    }

    public class SeedlingsData
    {
        public static bool idValid(string id)
        {
            /*np. int, dodatni*/
            return true;
        }

        public static string idValidationFeedback(string id)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

        public static bool nameValid(string name)
        {
            /*np. same litery (brak znakow specjalnych), jedno slowo, zakres znakow min, max itd.*/
            return true;
        }

        public static string nameValidationFeedback(string name)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

        public static bool quantityValid(string quantity)
        {
            /*np. int, dodatni*/
            return true;
        }

        public static string quantityValidationFeedback(string quantity)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }

        public static bool pricePerUnitValid(string pricePerUnit)
        {
            /*np. float, dodatni, 2 miejsca po przecinku*/
            return true;
        }

        public static string pricePerUnitValidationFeedback(string pricePerUnit)
        {
            /*Dzialanie podobnie jak funkcje ____ValidationFeedback() wyzej*/
            return "";
        }
    }
}