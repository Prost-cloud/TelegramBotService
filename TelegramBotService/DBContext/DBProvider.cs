using Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotService.DBContext;

namespace DBContext
{
    public class DBProvider : IDbProvider
    {

        SqlLiteDBContext _sqlLiteDBContext = new SqlLiteDBContext();
        User _currentUser { get; set; }
        int _currentMessageId { get; set; }
        ShoppingList _currentShoppingList { get; set; }


        public DBProvider(long chatIdAsString, int messageIdAsString, string name, long chatId)
        {
            _sqlLiteDBContext = new SqlLiteDBContext();
            _sqlLiteDBContext.Database.EnsureCreatedAsync();
            _currentMessageId = messageIdAsString;
            _currentUser = GetUserByChatID(chatIdAsString);

            if (_currentUser == default(User))
            {
                _currentUser = AddUser(name, chatId);
            }

            _currentShoppingList = GetCurrentShoppingList(_currentUser);

        }



        public string AddFunds(string payerIdAsString, string countAsString)
        {
            countAsString = countAsString.Replace(".", ",");

            if (!(int.TryParse(payerIdAsString, out int payerId)
                && decimal.TryParse(countAsString, out decimal count)))
            {
                return "Use \"/addfunds (payer id) (count)\"";
            }

            Payer payer = _sqlLiteDBContext.Payers.Find(payerId);

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list use /create \"name\" to create new one";
            }


            if (payer.ShoppingList != _currentShoppingList && !payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }

            AddFundsToUser(count, payer);
            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {countAsString} to payer";

        }

        public string AddPayer(string name)
        {
            var payers = _sqlLiteDBContext.Payers;

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            payers.Add(new Payer()
            {
                Name = name,
                Payed = 0,
                ShoppingList = _currentShoppingList
            });

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {name} to {_currentShoppingList}";

        }

        public string AddProduct(string name, string costAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            if (!decimal.TryParse(costAsString, out decimal cost))
            {
                return "Use \"/add (name of product) (cost of product)\"";
            }


            var product = _sqlLiteDBContext.Products;

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            product.Add(new Product()
            {
                Name = name,
                Price = cost,
                MessageID = _currentMessageId,
                ShoppingList = _currentShoppingList,
                IsDeleted = false
            });

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {name} to {_currentShoppingList}";

        }

        public string AddShoppingList(string name)
        {

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            ShoppingList currentShoppingList = new ShoppingList()
            {
                Name = name,
                Owner = _currentUser,
                Current = false
            };

            _sqlLiteDBContext.ShoppingList.Add(currentShoppingList);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            try
            {
                MakeShoppingListAsCurrent(_currentUser, currentShoppingList);
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {currentShoppingList}";
        }

        public string Start()
        {
            return "Hello!";
        }

        private User AddUser(string name, long chatId)
        {
            var users = _sqlLiteDBContext.Users;

            var newUser = new User()
            {
                Name = name,
                ChatID = chatId
            };

            if (_currentUser is null)
            {
                users.Add(newUser);

                try
                {
                    SaveChanges();
                }
                catch (DbUpdateException)
                {

                }
            }
            return newUser;
        }

        public string DeletePayer(string idPayerAsString)
        {

            if (!int.TryParse(idPayerAsString, out int payerId))
            {
                return "Use \"/deletepayer (payer id)\"";
            }


            var payers = _sqlLiteDBContext.Payers;
            var shoppingLists = _sqlLiteDBContext.ShoppingList.ToList().Where(x => x.Owner == _currentUser);

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            var currentPayer = payers.Find(payerId);

            if (currentPayer is null)
            {
                return $"Can't find payer by that id{payerId}. Check ID and try one more time";
            }


            if (shoppingLists.Any(x => x == currentPayer.ShoppingList))
            {
                currentPayer.IsDeleted = false;
            }
            else
            {
                return "can't find that payer";
            }


            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success deleted {currentPayer}";
        }

        public string DeleteProduct(string idProductAsString)
        {
            if (!int.TryParse(idProductAsString, out int productId))
            {
                return "Use \"/delete (product id)\"";
            }
                        
            var currentProduct = GetProductByID(productId);

            if (currentProduct is null)
            {
                return $"Can't find that product by id {productId}";
            }

            bool isInCurrentShoppingList =
                _sqlLiteDBContext.Products.Where(x => x == currentProduct && x.ShoppingList == _currentShoppingList).Count() == 1;

            if (!isInCurrentShoppingList)
            {
                return $"Product {productId} isn't yours or it's in another shopping list";
            }

            _sqlLiteDBContext.Products.Where(x => x == currentProduct && x.ShoppingList == _currentShoppingList).AsQueryable().ForEachAsync(x => x.IsDeleted = true);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something went wrong! Try another time later";
            }
           
            return $"Product deleted {currentProduct.Name}";
        }



        public string DeleteShoppingList(string idShoppingListAsString)
        {

            if (!int.TryParse(idShoppingListAsString, out int shoppingListId))
            {
                return "Use \"/deleteshoppinglist (shopping list id)\"";
            }

            bool isShoppingListOfCurrentUser
                = _sqlLiteDBContext.ShoppingList.Where(x => x.ID == shoppingListId && x.Owner == _currentUser).Count() == 1;

            if (!isShoppingListOfCurrentUser)
            {
                return $"Can't find shopping list with id {shoppingListId} or it's not yours.";
            }

            _sqlLiteDBContext.ShoppingList.Where(x => x.ID == shoppingListId && x.Owner == _currentUser).AsQueryable().ForEachAsync(x => x.IsDeleted = false);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something went wrong! Try another time later";
            }

            return $"Shopping list deleted.";


        }

        public string GetCountingByPayers()
        {
            var payers = _sqlLiteDBContext.Payers.Where(x => x.ShoppingList == _currentShoppingList && !x.IsDeleted).ToList();

            Dictionary<Payer, decimal> payed = new Dictionary<Payer, decimal>();

            decimal sumPayed = 0;

            if (payers.Count() == 0)
            {
                return "You haven't payers yet";
            }

            foreach (var payer in payers)
            {
                payed.Add(payer, payer.Payed);
                sumPayed += payer.Payed;
            }

            var sumOfProduct = _sqlLiteDBContext.Products.Where(x => x.ShoppingList == _currentShoppingList && !x.IsDeleted).ToList().Sum(x => x.Price);


            var separetedSum = sumOfProduct / payers.Count();

            foreach (var payerKey in payers)
            {
                payed[payerKey] = separetedSum - payed[payerKey];
            }

            StringBuilder result = new StringBuilder();

            result.Append($"Counting by {_currentShoppingList}\n");

            foreach (var payer in payed)
            {
                result.Append($"{payer.Key.Name} {payer.Value}\n");
            }

            result.Append($"sum of products {sumOfProduct} - payed {sumPayed}");

            return result.ToString();

        }

        public string GetShoppingLists()
        {
            var allShoppingLists = _sqlLiteDBContext.ShoppingList.Where(x => x.Owner == _currentUser && !x.IsDeleted).ToList();

            if (allShoppingLists.Count == 0)
            {
                return "You havent shopping list yet";
            }

            StringBuilder result = new StringBuilder();

            result.Append("Shopping list info:\n");

            foreach (var list in allShoppingLists)
            {
                result.Append($"{list.ID}. {list.Name} {(list.Current ? "(current)" : "")}\n");
            }

            return result.ToString();

        }

        public string RemoveFunds(string payerIdAsString, string countAsString)
        {
            countAsString = countAsString.Replace(".", ",");

            if (!(int.TryParse(payerIdAsString, out int payerId)
                && decimal.TryParse(countAsString, out decimal count)))
            {
                return "Use \"/removefunds (payer id) (count)\"";
            }

            var payers = _sqlLiteDBContext.Payers;

            Payer payer = _sqlLiteDBContext.Payers.Find(payerId);

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            if (payer.ShoppingList == _currentShoppingList && payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }


            payers.Where(x => x == payer).AsQueryable().ForEachAsync(x => x.Payed -= count);
            AddFundsToUser(-count, payer);
            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success removed {countAsString} from payer";

        }

        public string SelectShoppingList(string idShoppingListAsString)
        {

            if (!int.TryParse(idShoppingListAsString, out int idShoppingList))
            {
                return "Use \"/select (shopping list id)\"";
            }

            var selectedShoppingList = GetShoppingListById(idShoppingList);

            if (selectedShoppingList.Owner != _currentUser)
            {
                return "That's shopping list is not yours";
            }

            try
            {
                MakeShoppingListAsCurrent(_currentUser, selectedShoppingList);
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Selected {selectedShoppingList} as current";

        }

        public string Show(string idShoppingListAsString)
        {

            if (!int.TryParse(idShoppingListAsString, out int idShoppingList))
            {
                return "Use \"/select (shopping list id)\"";
            }

            if (_currentShoppingList is null)
            {
                return $"can't find shopping list with id {idShoppingList}";
            }

            if (_currentShoppingList.Owner != _currentUser)
            {
                return "That shopping list is not yours";
            }

            StringBuilder result = new StringBuilder();

            result.Append(_currentShoppingList.ToString() + " " + (_currentShoppingList.Current ? "(current)" : "") + "\n");

            var allProduct = _sqlLiteDBContext.Products.Where(x => x.ShoppingList == _currentShoppingList && !x.IsDeleted).ToList();
            result.Append("Products:\n");

            foreach (var prod in allProduct)
            {
                result.Append($"{prod.ID}. {prod.Name} cost: {prod.Price}\n");
            }

            var allPayer = _sqlLiteDBContext.Payers.Where(x => x.ShoppingList == _currentShoppingList && !x.IsDeleted).ToList();

            result.Append("Payers:\n");

            foreach (var payer in allPayer)
            {
                result.Append($"{payer.ID}. {payer.Name} payed: {payer.Payed}\n");
            }

            return result.ToString();

        }



        public string UpdateProduct(string name, string costAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            if (!decimal.TryParse(costAsString, out decimal cost))

            {
                return "Use \"/add (name of product) (cost of product)\"";
            }

            var currentProduct = _sqlLiteDBContext.Products.Where(x => x.MessageID == _currentMessageId)
                .AsQueryable()
                .ForEachAsync(x => { x.Name = name; x.Price = cost; });

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success updated {name} with {cost}";

        }

        private ShoppingList GetCurrentShoppingList(User currentUser)
        {
            if (_sqlLiteDBContext.ShoppingList.ToList().Where(x => x.Owner == currentUser && x.Current).Count() != 0)
            {
                return _sqlLiteDBContext.ShoppingList.ToList().Where(x => x.Owner == currentUser && x.Current).FirstOrDefault();
            }

            return null;
        }

        private void SaveChanges()
        {
            _sqlLiteDBContext.SaveChangesAsync();
        }

        private User GetUserByChatID(long chatId)
        {
            return _sqlLiteDBContext.Users.ToList().Where(x => x.ChatID == chatId).FirstOrDefault();
        }
        private Product GetProductByID(int productId)
        {
            return _sqlLiteDBContext.Products.ToList().Where(x => x.ID == productId).FirstOrDefault();
        }

        private ShoppingList GetShoppingListById(int idShoppingList)
        {
            return _sqlLiteDBContext.ShoppingList.Where(x => x.ID == idShoppingList && !x.IsDeleted).FirstOrDefault();
        }

        private void MakeShoppingListAsCurrent(User currentUser, ShoppingList currentShoppingList)
        {
            var shoppingList = _sqlLiteDBContext.ShoppingList;

            shoppingList.Where(x => x.Owner == currentUser).AsQueryable().ForEachAsync(x => x.Current = false);
            shoppingList.Where(x => x == currentShoppingList).AsQueryable().ForEachAsync(x => x.Current = true);

            SaveChanges();

        }
     
        private void AddFundsToUser(decimal count, Payer payer)
        {
            var payers = _sqlLiteDBContext.Payers;
            payers.Where(x => x == payer).AsQueryable().ForEachAsync(x => x.Payed += count);
        }
    }
}