using Interfaces;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TelegramBotService.DBContext;

namespace MethodProcessor
{
    public class MethodProcessor : IMethodProcessor, IDisposable
    {
        readonly IDbProvider _dbProvider;
        User CurrentUser { get; set; }
        int CurrentMessageId { get; set; }
        ShoppingList CurrentShoppingList { get; set; }

        public MethodProcessor(IDbProvider dbProvider)
        {
            _dbProvider = dbProvider;
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

            if (CurrentShoppingList is null)
            {
                return "Can't find current shopping list use /create \"name\" to create new one";
            }

            if (!IsPayerInCurrentShoppingListAndNotDeleted(payer))
            {
                return "That payer is not in current shopping list or deleted";
            }

            _dbProvider.AddFundsToUser(count, payer);

            return $"Success added {countAsString} to payer";
        }

        public string AddPayer(string name)
        {

            if (CurrentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (CurrentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            _dbProvider.AddPayer(new Payer()
            {
                Name = name,
                Payed = 0,
                ShoppingList = CurrentShoppingList
            });

            return $"Success added {name} to {CurrentShoppingList}";
        }

        public string AddProduct(string name, string costAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            if (!decimal.TryParse(costAsString, out decimal cost))
            {
                return "Use \"/add (name of product) (cost of product)\"";
            }

            if (CurrentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (CurrentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            _dbProvider.AddProduct(new Product()
            {
                Name = name,
                Price = cost,
                MessageID = CurrentMessageId,
                ShoppingList = CurrentShoppingList,
                IsDeleted = false
            });

            return $"Success added {name} to {CurrentShoppingList}";
        }

        public string AddShoppingList(string name)
        {
            ShoppingList currentShoppingList = new ShoppingList()
            {
                Name = name,
                Owner = CurrentUser,
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
            var shoppingLists = _dbProvider.GetAllShoppingListNotDeletedByUser(CurrentUser);

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

            bool isInCurrentShoppingList = IsProductInCurrentShoppingList(currentProduct);

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
            bool isShoppingListOfCurrentUser = IsShoppingListOfCurrentUser(shoppingListId);

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
            var payers = _dbProvider.GetAllPayerNotDeletedByShoppingList(CurrentShoppingList);

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

            var sumOfProduct = _dbProvider.GetAllProductNotDeletedByShoppingList(CurrentShoppingList).Sum(x => x.Price);

            var separetedSum = sumOfProduct / payers.Count();

            foreach (var payerKey in payers)
            {
                payed[payerKey] = separetedSum - payed[payerKey];
            }

            StringBuilder result = new StringBuilder();

            result.Append($"Counting by {CurrentShoppingList}\n");

            foreach (var payer in payed)
            {
                result.Append($"{payer.Key.Name} {payer.Value}\n");
            }

            result.Append($"sum of products {sumOfProduct} - payed {sumPayed}");

            return result.ToString();
        }

        public string GetShoppingLists()
        {
            var allShoppingLists = _dbProvider.GetAllShoppingListNotDeletedByUser(CurrentUser);

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

            if (CurrentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (!IsPayerInCurrentShoppingListAndNotDeleted(payer))
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

            if (!IsShoppingListOfCurrentUser(idShoppingList))
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

            if (!IsShoppingListOfCurrentUser(idShoppingList))
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

            var currentProduct = _dbProvider.GetProductByMessageId(CurrentMessageId);
            _dbProvider.UpdateProduct(currentProduct, name, cost);

            return $"Success updated {name} with {cost}";
        }
        private bool IsPayerInCurrentShoppingListAndNotDeleted(Payer payer)
        {
            return payer.ShoppingList == CurrentShoppingList && !payer.IsDeleted;
        }

        private bool IsProductInCurrentShoppingList(Product currentProduct)
        {
            return _dbProvider.GetAllProductNotDeletedByShoppingList(CurrentShoppingList)
                            .Where(x => x == currentProduct && x.ShoppingList == CurrentShoppingList).Count() == 1;
        }

        private bool IsShoppingListOfCurrentUser(int shoppingListId)
        {
            return _dbProvider.GetAllShoppingListNotDeletedByUser(CurrentUser)
                    .Where(x => x.ID == shoppingListId && x.Owner == CurrentUser).Count() == 1;
        }
        public void Dispose()
        {
            _dbProvider.Dispose();
        }
    }
}