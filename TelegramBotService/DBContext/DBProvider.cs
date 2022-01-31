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

        public DBProvider()
        {
            _sqlLiteDBContext = new SqlLiteDBContext();
            _sqlLiteDBContext.Database.EnsureCreatedAsync();
        }

        public string AddFunds(string payerIdAsString, string countAsString, string messageIdAsString, string chatIdAsString)
        {
            countAsString = countAsString.Replace(".", ",");

            long chatId;
            decimal count;
            int payerId;
            if (!(int.TryParse(payerIdAsString, out payerId)
                && decimal.TryParse(countAsString, out count)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/addfunds (payer id) (count)\"";
            }

            

            
            var payers = _sqlLiteDBContext.Payers;

            Users currentUser = GetUserByChatID(chatId);
            ShoppingList currentShoppingList = GetCurrentShoppingList(currentUser);
            Payers payer = _sqlLiteDBContext.Payers.Find(payerId);
            
            if (currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            if(payer.ShoppingList != currentShoppingList && !payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }


            payers.Where(x => x == payer).AsQueryable().ForEachAsync(x => x.Payed += count);
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

        public string AddPayer(string name, string messageIdAsString, string chatIdAsString)
        {

            long chatId;
            if (!long.TryParse(chatIdAsString, out chatId))
            {
                return "Use \"/addpayer (payer name)\"";
            }

            var payers = _sqlLiteDBContext.Payers;
            
            Users currentUser = GetUserByChatID(chatId);
            ShoppingList currentShoppingList = GetCurrentShoppingList(currentUser);

            if (currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            payers.Add(new Payers()
            {
                Name = name,
                Payed = 0,
                ShoppingList=currentShoppingList
            });

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException )
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {name} to {currentShoppingList}";

        }

        public string AddProduct(string name, string costAsString, string messageIdAsString, string chatIdAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            long chatId;
            decimal cost;
            int messageId;
            if (!(int.TryParse(messageIdAsString, out messageId)
                && decimal.TryParse(costAsString, out cost)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/add (name of product) (cost of product)\"";
            }


            var product = _sqlLiteDBContext.Products;

            Users currentUser = GetUserByChatID(chatId);
            ShoppingList currentShoppingList = GetCurrentShoppingList(currentUser);

            if (currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            product.Add(new Product()
            {
                Name = name,
                Price = cost,
                MessageID = messageId,
                ShoppingList = currentShoppingList,
                IsDeleted = false
            });

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException )
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success added {name} to {currentShoppingList}";

        }

        public string AddShoppingList(string name, string messageIdAsString, string chatIdAsString)
        {

            long chatId;
            if(!long.TryParse(chatIdAsString, out chatId))
            {
                return "Use \"/add (name of shopping list)\"";
            }

            //var shoppingList = _sqlLiteDBContext.ShoppingList.ToList();

            Users currentUser = GetUserByChatID(chatId);
                     
            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            ShoppingList currentShoppingList = new ShoppingList()
            {
                Name = name,
                Owner = currentUser,
                Current = false
            };

            _sqlLiteDBContext.ShoppingList.Add(currentShoppingList);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException )
            {
                return "Something wen't wrong! Try another time later";
            }

            try
            {
                MakeShoppingListAsCurrent(currentUser, currentShoppingList);
            }
            catch (DbUpdateException )
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success {currentShoppingList}";

        }

        private void MakeShoppingListAsCurrent(Users currentUser, ShoppingList currentShoppingList)
        {
            var shoppingList = _sqlLiteDBContext.ShoppingList;

            shoppingList.Where(x => x.Owner == currentUser).AsQueryable().ForEachAsync(x => x.Current = false);
            shoppingList.Where(x => x == currentShoppingList).AsQueryable().ForEachAsync(x => x.Current = true);

            SaveChanges();

        }

        public string AddUser(string chatIdAsString, string name)
        {
            long chatId;
            if (!long.TryParse(chatIdAsString, out chatId))
            {
                return "SomeThink went wrong";
            }

            var users = _sqlLiteDBContext.Users;

            Users user = GetUserByChatID(chatId);

            if (user is null)
            {
                users.Add(new Users()
                {
                    Name = name,
                    ChatID = chatId
                });

                try
                {
                    SaveChanges();
                }
                catch (DbUpdateException )
                {
                    return "Something went wrong! Try another time later";
                }

                return "All good";
            }
            else
            {
                return "I know you already";
            }

        }

        public string DeletePayer(string idPayerAsString, string messageIdAsString, string chatIdAsString)
        {

            int payerId;
            long chatId;
            if (!(int.TryParse(idPayerAsString, out payerId)               
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/deletepayer (payer id)\"";
            }


            var payers = _sqlLiteDBContext.Payers;
            var shoppingList = _sqlLiteDBContext.ShoppingList.ToList().Where(x=>x.Owner.ID==chatId);

            Users currentUser = GetUserByChatID(chatId);

            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            var currentPayer = payers.Find(payerId);

            if(currentPayer is null)
            {
                return $"Can't find payer by that id{payerId}. Check ID and try one more time";
            }


            if (shoppingList.Any(x => x == currentPayer.ShoppingList))
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
            catch (DbUpdateException )
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Success deleted {currentPayer}";
        }

        public string DeleteProduct(string idProductAsString, string messageIdAsString, string chatIdAsString)
        {
            int productId;
            long chatId;
           
            if (!(int.TryParse(idProductAsString, out productId)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/delete (product id)\"";
            }

            var currentUser = GetUserByChatID(chatId);
            var currentShoppingList = GetCurrentShoppingList(currentUser);
            var currentProduct = GetProductByID(productId); 
            
            if (currentProduct is null)
            {
                return $"Can't find that product by id {productId}";
            }

            bool isInCurrentShoppingList =
                _sqlLiteDBContext.Products.Where(x => x == currentProduct && x.ShoppingList == currentShoppingList).Count() == 1;

            if(!isInCurrentShoppingList)
            {
                return $"Product {productId} isn't yours or it's in another shopping list";
            }

            _sqlLiteDBContext.Products.Where(x => x == currentProduct && x.ShoppingList == currentShoppingList).AsQueryable().ForEachAsync(x => x.IsDeleted = true);

            try
            {
                SaveChanges();
            }
            catch (DbUpdateException )
            {
                return "Something went wrong! Try another time later";
            }

            return $"Product deleted {currentProduct.Name}";

        }

        

        public string DeleteShoppingList(string idShoppingListAsString, string messageIdAsString, string chatIdAsString)
        {
            int shoppingListId;
            long chatId;

            if (!(int.TryParse(idShoppingListAsString, out shoppingListId)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/deleteshoppinglist (shopping list id)\"";
            }

            var currentUser = GetUserByChatID(chatId);

            bool isShoppingListOfCurrentUser 
                = _sqlLiteDBContext.ShoppingList.Where(x => x.ID == shoppingListId && x.Owner == currentUser).Count() == 1;
            
            if (!isShoppingListOfCurrentUser)
            {
                return $"Can't find shopping list with id {shoppingListId} or it's not yours.";
            }

            _sqlLiteDBContext.ShoppingList.Where(x => x.ID == shoppingListId && x.Owner == currentUser).AsQueryable().ForEachAsync(x => x.IsDeleted = false);

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

        public string GetCountingByPayers(string chatIdAsString, string notUsed)
        {
            long chatId;

            if (!long.TryParse(chatIdAsString, out chatId))
            {
                return "Somethimg went wrong";
            }

            var currentUser = GetUserByChatID(chatId);
            var currentShoppingList = GetCurrentShoppingList(currentUser);

            var payers = _sqlLiteDBContext.Payers.Where(x => x.ShoppingList == currentShoppingList && !x.IsDeleted).ToList();

            Dictionary<Payers, decimal> payed = new Dictionary<Payers, decimal>();

            decimal sumPayed = 0;
            
            if(payers.Count() == 0)
            {
                return "You haven't payers yet";
            }

            foreach(var payer in payers)
            {
                payed.Add(payer, payer.Payed);
                sumPayed += payer.Payed;
            }

            var sumOfProduct = _sqlLiteDBContext.Products.Where(x => x.ShoppingList == currentShoppingList && !x.IsDeleted).ToList().Sum(x=>x.Price);
            

            var separetedSum = sumOfProduct / payers.Count();

            foreach(var payerKey in payers)
            {
                payed[payerKey] = separetedSum - payed[payerKey];
            }

            StringBuilder result = new StringBuilder();

            result.Append($"Counting by {currentShoppingList}\n");

            foreach(var payer in payed)
            {
                result.Append($"{payer.Key.Name} {payer.Value}\n");
            }

            result.Append($"sum of products {sumOfProduct} - payed {sumPayed}");

            return result.ToString();

        }

        public string GetShoppingLists(string chatIdAsString, string notUsed)
        {
            long chatId;

            if (!long.TryParse(chatIdAsString, out chatId))
            {
                return "Somethimg went wrong";
            }

            var currentUsre = GetUserByChatID(chatId);

            var allShoppingLists = _sqlLiteDBContext.ShoppingList.Where(x => x.Owner == currentUsre && !x.IsDeleted).ToList();

            if (allShoppingLists.Count == 0)
            {
                return "You havent shopping list yet";
            }

            StringBuilder result = new StringBuilder();

            result.Append("Shopping list info:\n");

            foreach(var list in allShoppingLists)
            {
                result.Append($"{list.ID}. {list.Name} {(list.Current ? "(current)" : "")}\n");
            }

            return result.ToString();

        }

        public string RemoveFunds(string payerIdAsString, string countAsString, string messageIdAsString, string chatIdAsString)
        {
            countAsString = countAsString.Replace(".", ",");

            long chatId;
            decimal count;
            int payerId;
            if (!(int.TryParse(payerIdAsString, out payerId)
                && decimal.TryParse(countAsString, out count)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/removefunds (payer id) (count)\"";
            }
              
            var payers = _sqlLiteDBContext.Payers;

            Users currentUser = GetUserByChatID(chatId);
            ShoppingList currentShoppingList = GetCurrentShoppingList(currentUser);
            Payers payer = _sqlLiteDBContext.Payers.Find(payerId);

            if (currentShoppingList is null)
            {
                return "Can't find current shopping list";
            }

            if (currentUser is null)
            {
                return "Can't find user write \"/start\"";
            }

            if (payer.ShoppingList == currentShoppingList && payer.IsDeleted)
            {
                return "That payer is not in current shopping list or deleted";
            }


            payers.Where(x => x == payer).AsQueryable().ForEachAsync(x => x.Payed -= count);
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

        public string SelectShoppingList(string idShoppingListAsString, string messageIdAsString, string chatIdAsString)
        {
            long chatId;
            int idShoppingList;
            
            if (! (int.TryParse(idShoppingListAsString, out idShoppingList)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/select (shopping list id)\"";
            }

            var currentUser = GetUserByChatID(chatId);
            var currentShoppingList = GetShoppingListById(idShoppingList);

            if(currentShoppingList.Owner != currentUser)
            {
                return "That's shopping list is not yours";
            }

            try
            {
                MakeShoppingListAsCurrent(currentUser, currentShoppingList);
            }
            catch (DbUpdateException)
            {
                return "Something wen't wrong! Try another time later";
            }

            return $"Selected {currentShoppingList} as current";

        }

        public string Show(string idShoppingListAsString, string messageIdAsString, string chatIdAsString)
        {
            long chatId;
            int idShoppingList;

            if (!(int.TryParse(idShoppingListAsString, out idShoppingList)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/select (shopping list id)\"";
            }

            var currentUser = GetUserByChatID(chatId);
            var curentShoppingList = GetShoppingListById(idShoppingList);

            if(curentShoppingList is null)
            {
                return $"can't find shopping list with id {idShoppingList}";
            }

            if(curentShoppingList.Owner != currentUser)
            {
                return "That shopping list is not yours";
            }

            StringBuilder result = new StringBuilder();

            result.Append(curentShoppingList.ToString() + " " + (curentShoppingList.Current ? "(current)" : "")+"\n");

            var allProduct = _sqlLiteDBContext.Products.Where(x => x.ShoppingList == curentShoppingList && !x.IsDeleted).ToList();
            result.Append("Products:\n");

            foreach(var prod in allProduct)
            {
                result.Append($"{prod.ID}. {prod.Name} cost: {prod.Price}\n");
            }

            var allPayer = _sqlLiteDBContext.Payers.Where(x => x.ShoppingList == curentShoppingList && !x.IsDeleted).ToList();

            result.Append("Payers:\n");

            foreach (var payer in allPayer)
            {
                result.Append($"{payer.ID}. {payer.Name} payed: {payer.Payed}\n");
            }

            return result.ToString();

        }       

       

        public string AddUpdate(string name, string costAsString, string messageIdAsString, string chatIdAsString)
        {
            costAsString = costAsString.Replace(".", ",");

            long chatId;
            decimal cost;
            int messageId;
            if (!(int.TryParse(messageIdAsString, out messageId)
                && decimal.TryParse(costAsString, out cost)
                && long.TryParse(chatIdAsString, out chatId)))
            {
                return "Use \"/add (name of product) (cost of product)\"";
            }

            var currentProduct = _sqlLiteDBContext.Products.Where(x => x.MessageID == messageId).AsQueryable().ForEachAsync(
                x => { x.Name = name; x.Price = cost; });

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

        public string UpdateProduct(string name, string costAsString, string messageIdAsString, string chatIdAsString)
        {
            throw new NotImplementedException();
        }
        private void SaveChanges()
        {
            _sqlLiteDBContext.SaveChangesAsync();
        }

        private ShoppingList GetCurrentShoppingList(Users currentUser)
        {
            if (_sqlLiteDBContext.ShoppingList.ToList().Where(x => x.Owner == currentUser && x.Current).Count() != 0)
            {
                return _sqlLiteDBContext.ShoppingList.ToList().Where(x => x.Owner == currentUser && x.Current).First();
            }

            return null;
        }

        private Users GetUserByChatID(long chatId)
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

    }
}
