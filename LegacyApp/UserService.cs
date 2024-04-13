using System;

namespace LegacyApp
{
    public interface ICreditLimitService
    {
        int GetCreditLimit(string lastName, DateTime birthdate);
    }
    public interface IClientRepository
    {
        Client GetById(int idClient);
    }
    
    public class UserService
    {
        private IClientRepository _clientRepository;
        private ICreditLimitService _creditService;

        public UserService()
        {
            _clientRepository = new ClientRepository();
            _creditService = new UserCreditService();
        }
        
        public UserService(IClientRepository clientRepository,
            ICreditLimitService creditService)
        {
            _clientRepository = clientRepository;
            _creditService = creditService;
        }
        
        public bool AddUser(string firstName, string lastName, string email, DateTime dateOfBirth, int clientId)
        {
            //Logika biznesowa - walidacja
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                return false;
            }

            //Logika biznesowa - walidacja
            if (!email.Contains("@") && !email.Contains("."))
            {
                return false;
            }

            //Logika biznesowa
            var now = DateTime.Now;
            int age = now.Year - dateOfBirth.Year;
            if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

            if (age < 21)
            {
                return false;
            }

            //Infrastruktura
            var client = _clientRepository.GetById(clientId);

            var user = new User
            {
                Client = client,
                DateOfBirth = dateOfBirth,
                EmailAddress = email,
                FirstName = firstName,
                LastName = lastName
            };
            
            //Logika biznesowa + Infrastruktura
            if (client.Type == "VeryImportantClient")
            {
                user.HasCreditLimit = false;
            }
            else if (client.Type == "ImportantClient")
            {
                int creditLimit = _creditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                creditLimit = creditLimit * 2; 
                user.CreditLimit = creditLimit;
                
            }
            else
            {
                user.HasCreditLimit = true;
                int creditLimit = _creditService.GetCreditLimit(user.LastName, user.DateOfBirth);
                user.CreditLimit = creditLimit;
            }
            
            
            //Logika biznesowa
            if (user.HasCreditLimit && user.CreditLimit < 500)
            {
                return false;
            }

            //Infrastruktura
            UserDataAccess.AddUser(user);
            return true;
        }
    }
}
