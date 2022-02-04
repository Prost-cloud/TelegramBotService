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
    public class MethodProcessor : IMethodProcessor, IDisposable
    {

        IDbProvider _dbProvider = new DbProvider();
        User _currentUser { get; set; }
        int _currentMessageId { get; set; }
        ShoppingList _currentShoppingList { get; set; }


        public MethodProcessor(long chatId, int messageIdAsString, string name)
        {            
            _currentMessageId = messageIdAsString;
            _currentUser = _dbProvider.GetUserByChatId(chatId);

            if (_currentUser == default(User))
            {
                _currentUser = _dbProvider.AddUser(name, chatId);
            }

            _currentShoppingList = _dbProvider.GetCurrentShoppingListByUser(_currentUser);

        }



        public string AddFunds(string payerIdAsString, string countAsString)
        {
            countAsString = countAsString.Replace(".", ",");

            if (!(int.TryParse(payerIdAsString, out int payerId)
                && decimal.TryParse(countAsString, out decimal count)))
            {
                return "Use \"/addfunds (payer id) (count)\"";
            }

            Payer payer = _dbProvider.GetPayerById(payerId);

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list use /create \"name\" to create new one";
            }


            if (payer.ShoppingList != _currentShoppingList && !payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }

            _dbProvider.AddFundsToUser(count, payer);           

            return $"Success added {countAsString} to payer";
        }

        public string AddPayer(string name)
        {
            
            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            _dbProvider.AddPayer(new Payer()
            {
                Name = name,
                Payed = 0,
                ShoppingList = _currentShoppingList
            });
                        
            return $"Success added {name} to {_currentShoppingList}";
        }

        public string AddProduct(string name, string costAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            if (!decimal.TryParse(costAsString, out decimal cost))
            {
                return "Use \"/add (name of product) (cost of product)\"";
            }

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (_currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            _dbProvider.AddProduct(new Product()
            {
                Name = name,
                Price = cost,
                MessageID = _currentMessageId,
                ShoppingList = _currentShoppingList,
                IsDeleted = false
            });            

            return $"Success added {name} to {_currentShoppingList}";
        }

        public string AddShoppingList(string name)
        {     
            ShoppingList currentShoppingList = new ShoppingList()
            {
                Name = name,
                Owner = _currentUser,
                Current = false
            };

            _dbProvider.AddShoppingList(currentShoppingList);           
            _dbProvider.MakeShoppingListAsCurrent(currentShoppingList);            

            return $"Success added {currentShoppingList}";
        }

        public string Start()
        {
            return "Hello!";
        }
          
        public string DeletePayer(string idPayerAsString)
        {

            if (!int.TryParse(idPayerAsString, out int payerId))
            {
                return "Use \"/deletepayer (payer id)\"";
            }

            var currentPayer = _dbProvider.GetPayerById(payerId);
            var shoppingLists = _dbProvider.GetAllShoppingListNotDeletedByUser(_currentUser);
                
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

            return $"Success deleted {currentPayer}";
        }

        public string DeleteProduct(string idProductAsString)
        {
            if (!int.TryParse(idProductAsString, out int productId))
            {
                return "Use \"/delete (product id)\"";
            }
                        
            var currentProduct = _dbProvider.GetProductById(productId);

            if (currentProduct is null)
            {
                return $"Can't find that product by id {productId}";
            }

            bool isInCurrentShoppingList =
                _dbProvider.GetAllProductNotDeletedByShoppingList(_currentShoppingList)
                .Where(x => x == currentProduct && x.ShoppingList == _currentShoppingList).Count() == 1;

            if (!isInCurrentShoppingList)
            {
                return $"Product {productId} isn't yours or it's in another shopping list";
            }

            _dbProvider.MarkAsDeleteProduct(currentProduct);
           
            return $"Product deleted {currentProduct.Name}";
        }



        public string DeleteShoppingList(string idShoppingListAsString)
        {

            if (!int.TryParse(idShoppingListAsString, out int shoppingListId))
            {
                return "Use \"/deleteshoppinglist (shopping list id)\"";
            }

            bool isShoppingListOfCurrentUser
                = _dbProvider.GetAllShoppingListNotDeletedByUser(_currentUser)
                    .Where(x => x.ID == shoppingListId && x.Owner == _currentUser).Count() == 1;

            if (!isShoppingListOfCurrentUser)
            {
                return $"Can't find shopping list with id {shoppingListId} or it's not yours.";
            }

            var shoppingList = _dbProvider.GetShoppingListById(shoppingListId);   
            _dbProvider.MarkAsDeleteShoppingList(shoppingList);

            return $"Shopping list deleted.";
        }

        public string GetCountingByPayers()
        {
            var payers = _dbProvider.GetAllPayerNotDeletedByShoppingList(_currentShoppingList);

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

            var sumOfProduct = _dbProvider.GetAllProductNotDeletedByShoppingList(_currentShoppingList).Sum(x => x.Price);

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
            var allShoppingLists = _dbProvider.GetAllShoppingListNotDeletedByUser(_currentUser);

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

            Payer payer = _dbProvider.GetPayerById(payerId);

            if (_currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }
               
            if (payer.ShoppingList == _currentShoppingList && payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }
                        
            _dbProvider.AddFundsToUser(-count, payer);
           
            return $"Success removed {countAsString} from payer";
        }

        public string SelectShoppingList(string idShoppingListAsString)
        {

            if (!int.TryParse(idShoppingListAsString, out int idShoppingList))
            {
                return "Use \"/select (shopping list id)\"";
            }

            var selectedShoppingList = _dbProvider.GetShoppingListById(idShoppingList);

            if (selectedShoppingList.Owner != _currentUser)
            {
                return "That's shopping list is not yours";
            }

            _dbProvider.MakeShoppingListAsCurrent(selectedShoppingList);

            return $"Selected {selectedShoppingList} as current";
        }

        public string Show(string idShoppingListAsString)
        {
            if (!int.TryParse(idShoppingListAsString, out int idShoppingList))
            {
                return "Use \"/show (shopping list id)\"";
            }
            
            var currentShoppingList = _dbProvider.GetShoppingListById(idShoppingList);

            if (currentShoppingList is null)
            {
                return $"can't find shopping list with id {idShoppingList}";
            }    
            
            if (currentShoppingList.Owner != _currentUser)
            {
                return $"That list is not yours";
            }

            StringBuilder result = new StringBuilder();

            result.Append(currentShoppingList.ToString() + " " + (currentShoppingList.Current ? "(current)" : "") + "\n");

            var allProduct = _dbProvider.GetAllProductNotDeletedByShoppingList(currentShoppingList);
           
            result.Append("Products:\n");

            foreach (var prod in allProduct)
            {
                result.Append($"{prod.ID}. {prod.Name} cost: {prod.Price}\n");
            }

            var allPayer = _dbProvider.GetAllPayerNotDeletedByShoppingList(currentShoppingList);

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

            var currentProduct = _dbProvider.GetProductByMessageId(_currentMessageId);
            _dbProvider.UpdateProduct(currentProduct, name, cost);

            return $"Success updated {name} with {cost}";
        }

        public void Dispose()
        {
            _dbProvider.Dispose();
        }
    }
}